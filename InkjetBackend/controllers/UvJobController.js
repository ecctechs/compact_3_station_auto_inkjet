const ResponseManager = require("../middleware/ResponseManager");
const { UvJobData } = require("../model/uvJobDataModel");

class UvJobController {
  /**
   * POST /uv-job/create
   * Replaces all uv_job_data rows for the given print_jobs_id, then bulk-inserts new ones.
   */
  static async create(req, res) {
    try {
      const { print_jobs_id, items } = req.body;

      await UvJobData.destroy({ where: { print_jobs_id } });

      const rows = items.map((item) => ({ ...item, print_jobs_id }));
      const created = await UvJobData.bulkCreate(rows);

      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = UvJobController;
