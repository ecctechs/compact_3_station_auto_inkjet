const { Op } = require("sequelize");
const sequelize = require("../database");
const ResponseManager = require("../middleware/ResponseManager");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
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

// วันที่ "วันนี้" ตามเวลาไทย — เครื่อง server ตั้ง time zone ไว้ยังไงก็ได้ค่าเดียวกัน
// ใช้ตัวเดียวกันทั้งตอนอ่านเลขล่าสุดและตอนเขียน job_date จะได้ไม่มีทางคนละวันกัน
const THAI_TODAY = "(now() AT TIME ZONE 'Asia/Bangkok')::date";

class JobController {
  /**
   * POST /job/create
   * Creates a print_jobs record. Pattern + UV data are created by separate calls.
   */
  static async create(req, res) {
    try {
      const { barcode_raw, created_by, order_no, customer_name, type, qty, st_status } =
        req.body;

      // เลขงานประจำวันต้องไม่ซ้ำกัน สองคนสแกนพร้อมกันแล้วอ่าน MAX ได้เลขเดียวกันไม่ได้
      // advisory lock กันไว้ทั้งการอ่านเลขและการ insert ให้เป็นคิวเดียว
      // ปลดเองตอน transaction จบ ไม่ว่าจะ commit หรือ rollback
      const job = await sequelize.transaction(async (t) => {
        await sequelize.query(
          "SELECT pg_advisory_xact_lock(hashtext('print_jobs_job_no'))",
          { transaction: t }
        );

        // คืนวันที่เป็นข้อความ 'YYYY-MM-DD' ไม่ใช่ชนิด date — ถ้าปล่อยเป็น date
        // ไดรเวอร์จะแปลงเป็น JS Date ตาม time zone ของ node แล้ววันอาจเลื่อนไปหนึ่งวัน
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

      // ตัวกรองวันที่ของหน้า History — ส่งมาเป็น ISO (UTC) ที่ฝั่ง client
      // ขยายเป็นทั้งวันตามเวลาไทยไว้แล้ว ที่นี่จึงกรองตรง ๆ ไม่ตีความเพิ่ม
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

      // commands + plan_routing มาด้วยเลย เพราะหน้า Order List ต้องรู้ว่างานส่งครบยัง
      // จึงจะระบายสีปุ่มจบงานได้ — ถ้าไม่ include ต้องยิง getResolved ทีละแถวทุกรอบ poll
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

  /**
   * POST /job/execute/:id
   * Marks job as executing (C# is about to send commands to hardware).
   */
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

  /**
   * GET /job/getResolved/:id
   * Returns job data with all template placeholders resolved.
   */
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

      // plan_routing ของ job นี้ — ไม่มีก็ส่ง null ไม่ถือว่า error
      const planRouting = await PlanRouting.findOne({
        where: { print_jobs_id: job.id },
      });

      // uv_job_data (UV1/UV2) — ไม่มีก็ส่ง array ว่าง
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

  /**
   * POST /job/postResults/:id
   */
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

  /**
   * POST /job/addCommand/:id
   */
  static async addCommand(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { command, ordinal, success, sent_at, payload } = req.body;

      // payload เก็บรายละเอียดของ step นั้น เช่นรุ่นย่อย .uvdx ที่เลือกจริง
      // คอลัมน์เป็น JSONB อยู่แล้ว เดิมรับมาแล้วทิ้ง ทำให้ย้อนดูไม่ได้ว่าพิมพ์ด้วยรุ่นไหน
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

  /**
   * POST /job/retry/:id
   */
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

  /**
   * PATCH /job/:id/status
   */
  static async updateStatus(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { status } = req.body;
      if (!status) {
        return ResponseManager.ErrorResponse(req, res, 400, "status is required");
      }

      await job.update({ status });

      return ResponseManager.SuccessResponse(req, res, 200, "Status updated");
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /job/getByMarkingMethod/:method
   */
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

  /**
   * PATCH /job/:id/send-to-st1
   */
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

  /**
   * PATCH /job/:id/remote-start
   *
   * ST3 ฝากงานให้ ST1 ส่งคำสั่งเข้าเครื่องแทน (เครื่องต่ออยู่กับ PC ของ ST1 ที่เดียว)
   * body: { remote_start: "0" | "1", remote_program?: string, remote_error?: string }
   *
   * ST3 ตั้ง "1" พร้อมชื่อโปรแกรมที่เลือกไว้แล้ว · ST1 ตั้งกลับเป็น "0" เมื่อส่งเสร็จ
   * หรือส่งไม่สำเร็จ — เก็บเป็นธงใบเดียว งานหนึ่งจึงมีคำขอค้างได้ไม่เกินหนึ่งใบ
   *
   * remote_error คือสาเหตุที่ ST1 ส่งไม่สำเร็จ ฝากไว้ให้ ST3 อ่านไปแสดงที่จอตัวเอง
   * แล้วเรียกมาล้างทิ้ง (ส่งค่าว่างมา) — ทุกครั้งที่เขียนใหม่คือทับของเดิมเสมอ
   */
  static async setRemoteStart(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const { remote_start, remote_program, remote_step, remote_error } = req.body;

      await job.update(
        {
          // "1" = ST3 ฝากไว้ ยังไม่มีใครหยิบ · "2" = ST1 หยิบไปแล้วกำลังส่งเข้าเครื่อง
          // แยกสองสถานะเพราะ ST3 ต้องรู้ว่า "ไม่มีใครทำ" ต่างจาก "กำลังทำอยู่"
          // ใบที่ไม่มีใครหยิบเลยถึงจะตีกลับเป็น Waiting ได้ ใบที่กำลังส่งห้ามแตะ
          remote_start: ["1", "2"].includes(String(remote_start)) ? String(remote_start) : "0",
          remote_program: remote_program ?? null,
          remote_step: remote_step ?? null,
          remote_error: remote_error || null,
        },
        // ทั้งสี่ช่องนี้เป็นชุดเดียวกัน ส่งอะไรมาก็ต้องได้อย่างนั้น รวมถึงการล้างเป็นค่าว่าง
        //
        // ตัว sequelize ตั้ง omitNull ไว้ทั้งโปรเจค (database.js) ซึ่งแปลว่าช่องที่เป็น
        // null จะถูกข้ามตอน UPDATE ค่าเก่าจึงค้างอยู่ ผลคือ
        //   คำขอที่ไม่ได้ระบุขั้นตอน ได้ขั้นของคำขอก่อนหน้าติดมา แล้วส่งผิดขั้น
        //   remote_error ล้างไม่ออก ST3 เลยเด้งกล่องเดิมซ้ำทุกรอบ poll
        // ปิดเฉพาะตรงนี้ ไม่ไปแตะค่าเริ่มต้นของทั้งโปรเจค
        { omitNull: false }
      );

      return ResponseManager.SuccessResponse(req, res, 200, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /job/remove/:id — deletes job and all children (CASCADE).
   */
  static async remove(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      await job.destroy();

      return ResponseManager.SuccessResponse(req, res, 200, "Job deleted");
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = JobController;
