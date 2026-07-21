const ResponseManager = require("../middleware/ResponseManager");
const { UvJobData } = require("../model/uvJobDataModel");

class UvJobDataController {
  /**
   * POST /uv-job/create
   * Store UV detail for a job (captured at register from print_data).
   * Body: { print_jobs_id, items: [ { machine, table_name, program_name, lot, name, text1..text5 }, ... ] }
   * Replaces any existing rows for the same print_jobs_id (re-register safe).
   */
  static async create(req, res) {
    try {
      const { print_jobs_id, items } = req.body;

      if (!Array.isArray(items) || items.length === 0) {
        return ResponseManager.ErrorResponse(req, res, 400, "items is required (array)");
      }

      // upsert: ลบของเดิมของ job นี้ก่อน แล้วค่อยใส่ใหม่
      if (print_jobs_id !== undefined && print_jobs_id !== null) {
        await UvJobData.destroy({ where: { print_jobs_id } });
      }

      const rows = items.map((it) => ({
        print_jobs_id: print_jobs_id ?? null,
        machine: it.machine ?? null,
        table_name: it.table_name ?? null,
        program_name: it.program_name ?? null,
        lot: it.lot ?? null,
        name: it.name ?? null,
        text1: it.text1 ?? null,
        text2: it.text2 ?? null,
        text3: it.text3 ?? null,
        text4: it.text4 ?? null,
        text5: it.text5 ?? null,
      }));

      const created = await UvJobData.bulkCreate(rows);

      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /uv-job/getByJob/:jobId
   * Poll UV detail for a job → operator preview (UV1/UV2 tabs).
   */
  static async getByJob(req, res) {
    try {
      const rows = await UvJobData.findAll({
        where: { print_jobs_id: req.params.jobId },
        order: [["machine", "ASC"]],
      });

      return ResponseManager.SuccessResponse(req, res, 200, rows);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /uv-job/deleteByJob/:jobId
   */
  static async deleteByJob(req, res) {
    try {
      const count = await UvJobData.destroy({
        where: { print_jobs_id: req.params.jobId },
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: "Deleted",
        count,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = UvJobDataController;
