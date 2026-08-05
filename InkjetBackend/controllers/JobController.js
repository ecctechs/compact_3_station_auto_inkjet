const ResponseManager = require("../middleware/ResponseManager");
const { PrintJob, PrintJobCommand , LastSentJob } = require("../model/jobModel");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
} = require("../model/patternModel");
const { UvJobData } = require("../model/uvJobDataModel");
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

// ไม่ระบุ order = Postgres คืนแถวตามลำดับที่มันสะดวก (เช่น 2,1)
// ordinal คือช่องเครื่อง (1 = MK-058, 2 = MK-059) จึงต้องเรียงให้แน่นอนเสมอ
const PATTERN_ORDER = [
  [{ model: InkjetConfig, as: "inkjet_configs" }, "ordinal", "ASC"],
  [
    { model: InkjetConfig, as: "inkjet_configs" },
    { model: TextBlock, as: "text_blocks" },
    "block_number",
    "ASC",
  ],
  [{ model: ServoConfig, as: "servo_configs" }, "ordinal", "ASC"],
];

class JobController {
  /**
   * POST /job/create
   * Main entry point — scanner sends barcode, backend parses and creates job.
   * Ported from client.py press_enter() lines 211-221.
   */
  static async create(req, res) {
    try {
      const {
        barcode_raw,
        order_no,
        customer_name,
        type,
        qty,
        lot_number,
        created_by,
        st_status
      } = req.body;

      // job ถูกสร้างก่อน แล้ว client ค่อยสร้าง pattern ผูกกลับมาด้วย job_id
      const job = await PrintJob.create({
        barcode_raw,
        order_no,
        customer_name,
        type,
        qty,
        pattern_no_erp: barcode_raw,
        lot_number: lot_number || barcode_raw,
        created_by,
        st_status,
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
   * C# calls this right before sending to hardware.
   *
   * Templates resolved:
   *   {yyyy}, {mm}, {dd} — current date
   *   {s-N}              — sequential counter (N + attempt)
   *   CCCC               — lot number
   *   DDDD               — encoded date from lot number
   */
  // static async getResolved(req, res) {
  //   try {
  //     const job = await PrintJob.findByPk(req.params.id);
  //     if (!job) {
  //       return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
  //     }
  //     if (!job.pattern_id) {
  //       return ResponseManager.ErrorResponse(
  //         req,
  //         res,
  //         400,
  //         "Job has no associated pattern"
  //       );
  //     }

  //     const pattern = await Pattern.findByPk(job.pattern_id, {
  //       include: PATTERN_INCLUDE,
  //     });
  //     if (!pattern) {
  //       return ResponseManager.ErrorResponse(
  //         req,
  //         res,
  //         404,
  //         "Associated pattern not found"
  //       );
  //     }

  //     const ctx = {
  //       lotNumber: job.lot_number || "",
  //       attempt: job.attempt,
  //     };

  //     // Deep-clone pattern and resolve templates in all text block strings
  //     const resolved = JSON.parse(JSON.stringify(pattern));
  //     for (const config of resolved.inkjet_configs || []) {
  //       for (const block of config.text_blocks || []) {
  //         if (block.text) {
  //           block.text = resolveTemplates(block.text, ctx);
  //         }
  //       }
  //     }

  //     return ResponseManager.SuccessResponse(req, res, 200, {
  //       job,
  //       pattern: resolved,
  //     });
  //   } catch (err) {
  //     return ResponseManager.CatchResponse(req, res, err.message);
  //   }
  // }
static async getResolved(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const pattern = await Pattern.findOne({
        where: { job_id: job.id },
        include: PATTERN_INCLUDE,
        order: PATTERN_ORDER,
      });
      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Job has no associated pattern"
        );
      }

      return ResponseManager.SuccessResponse(req, res, 200, {
        job,
        pattern,
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

  /**
   * DELETE /job/remove/:id
   * ลบ job ใบเดียว — pattern, configs, text_blocks, servo, conveyor,
   * command log, UV detail หายตามด้วย FK CASCADE ทั้งหมด
   */
  static async remove(req, res) {
    const t = await sequelize.transaction();
    try {
      const job = await PrintJob.findByPk(req.params.id, { transaction: t });
      if (!job) {
        await t.rollback();
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const jobId = job.id;

      // นับไว้ก่อนลบ เพื่อรายงานให้ operator เห็นว่าอะไรหายไปบ้าง
      const [commands, uvJobData, patterns] = await Promise.all([
        PrintJobCommand.count({ where: { job_id: jobId }, transaction: t }),
        UvJobData.count({ where: { print_jobs_id: jobId }, transaction: t }),
        Pattern.count({ where: { job_id: jobId }, transaction: t }),
      ]);

      await job.destroy({ transaction: t });
      await t.commit();

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: `Job ${jobId} deleted`,
        deleted: {
          job: 1,
          pattern: patterns,
          commands,
          uv_job_data: uvJobData,
        },
      });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  // ==================== LastSentJob (Station 3) ====================

  /**
   * POST /job/lastSent/create
   * Create a new LastSentJob record (Station 3)
   */
  static async createLastSent(req, res) {
    try {
      const {
        barcode_raw,
        pattern_id,
        lot_number,
        status,
        error_message,
        created_by,
        attempt,
        order_no,
        customer_name,
        type,
        qty,
        sent_time,
        st_status,
      } = req.body;

      const lastSentJob = await LastSentJob.create({
        barcode_raw,
        pattern_id,
        lot_number,
        status: status || "Waiting",
        error_message,
        created_by,
        attempt: attempt || 0,
        order_no,
        customer_name,
        type,
        qty,
        sent_time,
        st_status,
      });

      return ResponseManager.SuccessResponse(req, res, 201, lastSentJob);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /job/lastSent/getAll
   * Get all LastSentJob records
   */
  static async getAllLastSent(req, res) {
    try {
      const { st_status, page, limit } = req.query;
      const where = {};

      if (st_status) {
        where.st_status = st_status;
      }

      const offset = (page - 1) * limit;
      const { count, rows } = await LastSentJob.findAndCountAll({
        where,
        include: [{ model: Pattern, as: "pattern" }],
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

  /**
   * GET /job/lastSent/getById/:id
   * Get LastSentJob by ID
   */
  static async getLastSentById(req, res) {
    try {
      const lastSentJob = await LastSentJob.findByPk(req.params.id, {
        include: [{ model: Pattern, as: "pattern" }],
      });

      if (!lastSentJob) {
        return ResponseManager.ErrorResponse(req, res, 404, "LastSentJob not found");
      }

      return ResponseManager.SuccessResponse(req, res, 200, lastSentJob);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /job/lastSent/update/:id
   * Update LastSentJob
   */
  static async updateLastSent(req, res) {
    try {
      const lastSentJob = await LastSentJob.findByPk(req.params.id);

      if (!lastSentJob) {
        return ResponseManager.ErrorResponse(req, res, 404, "LastSentJob not found");
      }

      const {
        barcode_raw,
        pattern_id,
        lot_number,
        status,
        error_message,
        created_by,
        attempt,
        order_no,
        customer_name,
        type,
        qty,
        sent_time,
        st_status

      } = req.body;

      await lastSentJob.update({
        barcode_raw,
        pattern_id,
        lot_number,
        status,
        error_message,
        created_by,
        attempt,
        order_no,
        customer_name,
        type,
        qty,
        sent_time,
        st_status
      });

      return ResponseManager.SuccessResponse(req, res, 200, lastSentJob);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /job/lastSent/remove/:id
   * Delete LastSentJob
   */
  static async removeLastSent(req, res) {
    try {
      const lastSentJob = await LastSentJob.findByPk(req.params.id);

      if (!lastSentJob) {
        return ResponseManager.ErrorResponse(req, res, 404, "LastSentJob not found");
      }

      await lastSentJob.destroy();

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "LastSentJob deleted successfully"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

   static async update(req, res) {
    try {
      const job = await PrintJob.findByPk(req.params.id);

      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, "Job not found");
      }

      const {
        barcode_raw,
        order_no,
        customer_name,
        type,
        qty,
        pattern_id,
        lot_number,
        pattern_no_erp,
        status,
        error_message,
        warning,
        attempt,
        created_by,
        st_status,
        st1_confirmation,
        st1_send_time,
      } = req.body;

      await job.update({
        barcode_raw,
        order_no,
        customer_name,
        type,
        qty,
        pattern_id,
        lot_number,
        pattern_no_erp,
        status,
        error_message,
        warning,
        attempt,
        created_by,
        st_status,
        st1_confirmation,
        st1_send_time,
      });

      return ResponseManager.SuccessResponse(req, res, 200, job);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}



module.exports = JobController;
