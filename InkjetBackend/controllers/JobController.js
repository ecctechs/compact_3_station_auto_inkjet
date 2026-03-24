const ResponseManager = require("../middleware/ResponseManager");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
} = require("../model/patternModel");
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
   * Main entry point — scanner sends barcode, backend parses and creates job.
   * Ported from client.py press_enter() lines 211-221.
   */
  static async create(req, res) {
    try {
      const { barcode_raw, created_by, order_no, customer_name, type, qty } = req.body;
      const { lotNumber, patternCode } = parseBarcode(barcode_raw);

      const pattern = await Pattern.findOne({
        where: { barcode: patternCode, is_active: true },
      });

      const job = await PrintJob.create({
        barcode_raw,
        pattern_id: pattern ? pattern.id : null,
        lot_number: lotNumber,
        created_by,
        order_no,
        customer_name,
        type,
        qty,
      });

      const data = job.toJSON();
      if (!pattern) {
        data.warning = `No pattern found for barcode "${patternCode}"`;
      }

      return ResponseManager.SuccessResponse(req, res, 201, data);
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
      if (job.status !== "pending") {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          `Job status is "${job.status}", expected "pending"`
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
   * C# calls this right before sending to hardware.
   *
   * Templates resolved:
   *   {yyyy}, {mm}, {dd} — current date
   *   {s-N}              — sequential counter (N + attempt)
   *   CCCC               — lot number
   *   DDDD               — encoded date from lot number
   */
  static async getResolved(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }
      if (!job.pattern_id) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "Job has no associated pattern"
        );
      }

      const pattern = await Pattern.findByPk(job.pattern_id, {
        include: PATTERN_INCLUDE,
      });
      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Associated pattern not found"
        );
      }

      const ctx = {
        lotNumber: job.lot_number || "",
        attempt: job.attempt,
      };

      // Deep-clone pattern and resolve templates in all text block strings
      const resolved = JSON.parse(JSON.stringify(pattern));
      for (const config of resolved.inkjet_configs || []) {
        for (const block of config.text_blocks || []) {
          if (block.text) {
            block.text = resolveTemplates(block.text, ctx);
          }
        }
      }

      return ResponseManager.SuccessResponse(req, res, 200, {
        job,
        pattern: resolved,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /job/postResults/:id
   * C# posts execution results + command log after sending to hardware.
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
   * Reset a failed job back to pending and increment attempt counter.
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
        status: "pending",
        attempt: job.attempt + 1,
        error_message: null,
      });

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "Job reset to pending"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = JobController;
