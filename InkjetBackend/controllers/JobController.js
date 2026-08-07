const ResponseManager = require("../middleware/ResponseManager");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
} = require("../model/patternModel");
const sequelize = require("../database");
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
   * Scanner sends barcode → backend parses, creates job, clones matching
   * pattern (with all children) so each job owns its own snapshot.
   */
  static async create(req, res) {
    const t = await sequelize.transaction();
    try {
      const { barcode_raw, created_by, order_no, customer_name, type, qty } =
        req.body;
      const { lotNumber, patternCode } = parseBarcode(barcode_raw);

      const job = await PrintJob.create(
        {
          barcode_raw,
          lot_number: lotNumber,
          order_no,
          customer_name,
          type,
          qty,
          created_by,
        },
        { transaction: t }
      );

      const template = await Pattern.findOne({
        where: { barcode: patternCode, is_active: true },
        include: PATTERN_INCLUDE,
        order: [["id", "DESC"]],
      });

      if (template) {
        const cloned = await Pattern.create(
          {
            job_id: job.id,
            barcode: template.barcode,
            description: template.description,
          },
          { transaction: t }
        );

        for (const cfg of template.inkjet_configs || []) {
          const cfgJson = cfg.toJSON();
          const { id: _, text_blocks, ...cfgData } = cfgJson;
          const newCfg = await InkjetConfig.create(
            { ...cfgData, pattern_id: cloned.id },
            { transaction: t }
          );

          for (const block of text_blocks || []) {
            const { id: __, ...blockData } = block;
            await TextBlock.create(
              { ...blockData, inkjet_config_id: newCfg.id },
              { transaction: t }
            );
          }
        }

        if (template.conveyor_speeds) {
          const { id: _, ...speedData } = template.conveyor_speeds.toJSON();
          await ConveyorSpeed.create(
            { ...speedData, pattern_id: cloned.id },
            { transaction: t }
          );
        }

        for (const servo of template.servo_configs || []) {
          const { id: _, ...servoData } = servo.toJSON();
          await ServoConfig.create(
            { ...servoData, pattern_id: cloned.id },
            { transaction: t }
          );
        }
      }

      await t.commit();

      const data = job.toJSON();
      if (!template) {
        data.warning = `No pattern found for barcode "${patternCode}"`;
      }

      return ResponseManager.SuccessResponse(req, res, 201, data);
    } catch (err) {
      await t.rollback();
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
   * Reset a failed job back to Waiting and increment attempt counter.
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
}

module.exports = JobController;
