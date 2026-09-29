const { Op } = require("sequelize");
const sequelize = require("../database");
const ResponseManager = require("../middleware/ResponseManager");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const { MachineQueue } = require("../model/machineQueueModel");
const { withQueueLock, assertNoUnresolved, conflict } = require("../services/queueGuard");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
} = require("../model/patternModel");
const { PlanRouting } = require("../model/planRoutingModel");
const { UvJobData } = require("../model/uvJobDataModel");
const { parseBarcode, resolveTemplates } = require("../utils/templateResolver");

const PATTERN_INCLUDE = [
  {
    model: InkjetConfig,
    as: "inkjet_configs",
    include: [{ model: TextBlock, as: "text_blocks" }],
  },
  { model: ConveyorSpeed, as: "conveyor_speeds" },
  { model: ServoConfig, as: "servo_configs" },
];

const THAI_TODAY = "(now() AT TIME ZONE 'Asia/Bangkok')::date";

class JobController {
  static async create(req, res) { // สร้าง Job พร้อมเลขงานประจำวัน
    try {
      const { barcode_raw, created_by, order_no, customer_name, type, qty, st_status } =
        req.body;

      const job = await sequelize.transaction(async (t) => {
        await sequelize.query(
          "SELECT pg_advisory_xact_lock(hashtext('print_jobs_job_no'))",
          { transaction: t }
        );

        const [[{ job_date, next_no }]] = await sequelize.query(
          `SELECT to_char(${THAI_TODAY}, 'YYYY-MM-DD') AS job_date,
                  COALESCE(MAX(job_no), 0) + 1 AS next_no
             FROM print_jobs
            WHERE job_date = ${THAI_TODAY}`,
          { transaction: t }
        );

        return PrintJob.create(
          {
            barcode_raw,
            lot_number: barcode_raw,
            pattern_no_erp: barcode_raw,
            job_no: Number(next_no),
            job_date,
            order_no,
            customer_name,
            type,
            qty,
            created_by,
            st_status: st_status || "0",
          },
          { transaction: t }
        );
      });

      return ResponseManager.SuccessResponse(req, res, 201, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async getAll(req, res) {
    try {
      const { status, page, limit, from, to } = req.query;
      const where = {};

      if (status) {
        where.status = status;
      }

      const fromAt = from ? new Date(from) : null;
      const toAt = to ? new Date(to) : null;
      if (fromAt && !isNaN(fromAt) && toAt && !isNaN(toAt)) {
        where.created_at = { [Op.between]: [fromAt, toAt] };
      } else if (fromAt && !isNaN(fromAt)) {
        where.created_at = { [Op.gte]: fromAt };
      } else if (toAt && !isNaN(toAt)) {
        where.created_at = { [Op.lte]: toAt };
      }

      const offset = (page - 1) * limit;

      const { count, rows } = await PrintJob.findAndCountAll({
        where,
        include: [
          { model: PrintJobCommand, as: "commands" },
          { model: PlanRouting, as: "plan_routing" },
        ],
        order: [["created_at", "DESC"]],
        offset,
        limit,
        distinct: true,
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: rows,
        total: count,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async getById(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id, {
        include: [{ model: PrintJobCommand, as: "commands" }],
      });

      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      return ResponseManager.SuccessResponse(req, res, 200, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async execute(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }
      if (job.status !== "Waiting") {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          `Job status is "${job.status}", expected "Waiting"`
        );
      }

      await job.update({ status: "executing" });

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "Job marked as executing"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async getResolved(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const pattern = await Pattern.findOne({
        where: { job_id: job.id, is_active: true },
        include: PATTERN_INCLUDE,
      });
      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "Job has no associated pattern"
        );
      }

      const ctx = {
        lotNumber: job.lot_number || "",
        attempt: job.attempt,
      };

      const resolved = JSON.parse(JSON.stringify(pattern));
      for (const config of resolved.inkjet_configs || []) {
        for (const block of config.text_blocks || []) {
          if (block.text) {
            block.text = resolveTemplates(block.text, ctx);
          }
        }
      }

      const planRouting = await PlanRouting.findOne({
        where: { print_jobs_id: job.id },
      });

      const uvJobData = await UvJobData.findAll({
        where: { print_jobs_id: job.id },
        order: [["id", "ASC"]],
      });

      const commands = await PrintJobCommand.findAll({
        where: { job_id: job.id },
        order: [["id", "ASC"]],
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        job,
        pattern: resolved,
        plan_routing: planRouting,
        uv_job_data: uvJobData,
        commands,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async postResults(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { success, error_message, commands } = req.body;

      for (const cmd of commands) {
        await PrintJobCommand.create({
          job_id: job.id,
          ordinal: cmd.ordinal || null,
          command: cmd.command,
          payload: cmd.payload || null,
          response: cmd.response || null,
          success: cmd.success,
          sent_at: cmd.sent_at || null,
        });
      }

      const newStatus = success ? "completed" : "failed";
      await job.update({
        status: newStatus,
        error_message: error_message || null,
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: success ? "Job completed" : "Job failed",
        status: newStatus,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async addCommand(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { command, ordinal, success, sent_at, payload } = req.body;

      const created = await PrintJobCommand.create({
        job_id: job.id,
        command,
        ordinal: ordinal || null,
        payload: payload ?? null,
        success: success ?? true,
        sent_at: sent_at || new Date(),
      });

      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async retry(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }
      if (job.status !== "failed") {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          `Job status is "${job.status}", expected "failed"`
        );
      }

      await job.update({
        status: "Waiting",
        attempt: job.attempt + 1,
        error_message: null,
      });

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "Job reset to Waiting"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async updateStatus(req, res) { // เปลี่ยนสถานะและล้างคิวใน transaction เดียวกัน
    try {
      const { status } = req.body;
      if (!status) {
        return ResponseManager.ErrorResponse(req, res, 400, "status is required");
      }

      await withQueueLock(async (transaction) => {
        const job = await PrintJob.findByPk(req.params.id, { transaction });
        if (!job) conflict("Job not found");
        const rows = await MachineQueue.findAll({ where: { print_jobs_id: job.id }, transaction });
        if (status !== "Process") await assertNoUnresolved(rows, transaction); // ห้ามจบ ยกเลิก หรือคืนรอ ขณะที่ยังไม่รู้ผลส่ง
        if (["Success", "Cancel"].includes(status)) // จบหรือยกเลิกต้องล้างคิวพร้อมเปลี่ยนสถานะ
          await MachineQueue.destroy({ where: { print_jobs_id: job.id }, transaction }); // ล้างคิวใน transaction เดียวกับสถานะ Job
        await job.update({ status }, { transaction }); // หากบันทึกไม่ผ่าน ให้คิวและสถานะย้อนกลับพร้อมกัน
      });

      return ResponseManager.SuccessResponse(req, res, 200, "Status updated");
    } catch (err) {
      return ResponseManager.ErrorResponse(req, res, err.statusCode || 500, err.message);
    }
  }

  static async getByMarkingMethod(req, res) {
    try {
      const { method } = req.params;
      const limit = Math.min(parseInt(req.query.limit) || 100, 100);

      const jobs = await PrintJob.findAll({
        include: [
          {
            model: PlanRouting,
            as: "plan_routing",
            where: { marking_method: method },
            attributes: [],
          },
        ],
        order: [["created_at", "DESC"]],
        limit,
      });

      return ResponseManager.SuccessResponse(req, res, 200, jobs);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async sendToSt1(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      await job.update({
        st_status: "1",
        st1_send_time: new Date(),
      });

      return ResponseManager.SuccessResponse(req, res, 200, "Sent to ST1");
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async setRemoteStart(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { remote_start, remote_program, remote_step, remote_error } = req.body;

      await job.update(
        {
          remote_start: ["1", "2"].includes(String(remote_start)) ? String(remote_start) : "0",
          remote_program: remote_program ?? null,
          remote_step: remote_step ?? null,
          remote_error: remote_error || null,
        },
        { omitNull: false }
      );

      return ResponseManager.SuccessResponse(req, res, 200, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async remove(req, res) {
    try {
      await withQueueLock(async (transaction) => {
        const job = await PrintJob.findByPk(req.params.id, { transaction });
        if (!job) conflict("Job not found");
        const rows = await MachineQueue.findAll({ where: { print_jobs_id: job.id }, transaction });
        await assertNoUnresolved(rows, transaction);
        await job.destroy({ transaction });
      });

      return ResponseManager.SuccessResponse(req, res, 200, "Job deleted");
    } catch (err) {
      return ResponseManager.ErrorResponse(req, res, err.statusCode || 500, err.message);
    }
  }
}

module.exports = JobController;
