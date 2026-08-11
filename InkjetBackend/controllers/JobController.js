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

class JobController {
  /**
   * POST /job/create
   * Creates a print_jobs record. Pattern + UV data are created by separate calls.
   */
  static async create(req, res) {
    try {
      const { barcode_raw, created_by, order_no, customer_name, type, qty, st_status } =
        req.body;

      const job = await PrintJob.create({
        barcode_raw,
        lot_number: barcode_raw,
        pattern_no_erp: barcode_raw,
        order_no,
        customer_name,
        type,
        qty,
        created_by,
        st_status: st_status || "0",
      });

      return ResponseManager.SuccessResponse(req, res, 201, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async getAll(req, res) {
    try {
      const { status, page, limit } = req.query;
      const where = {};

      if (status) {
        where.status = status;
      }

      const offset = (page - 1) * limit;
      const { count, rows } = await PrintJob.findAndCountAll({
        where,
        order: [["created_at", "DESC"]],
        offset,
        limit,
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

      return ResponseManager.SuccessResponse(req, res, 200, {
        job,
        pattern: resolved,
        plan_routing: planRouting,
        uv_job_data: uvJobData,
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
