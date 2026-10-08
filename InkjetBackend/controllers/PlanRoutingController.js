const ResponseManager = require("../middleware/ResponseManager");
const { PlanRouting } = require("../model/planRoutingModel");

class PlanRoutingController {
  /**
   * POST /plan-routing/create
   * Replaces the plan_routing row for the given print_jobs_id, then inserts the new one.
   */
  static async create(req, res) {
    try {
      const {
        print_jobs_id,
        lot_no,
        erp_mfg,
        marking_method,
        process_sequence,
      } = req.body;

      await PlanRouting.destroy({ where: { print_jobs_id } });

      const created = await PlanRouting.create({
        print_jobs_id,
        lot_no: lot_no ?? null,
        erp_mfg: erp_mfg ?? null,
        marking_method: marking_method ?? null,
        process_sequence: process_sequence ?? null,
      });

      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /plan-routing/getByJob/:jobId
   */
  static async getByJob(req, res) {
    try {
      const row = await PlanRouting.findOne({
        where: { print_jobs_id: req.params.jobId },
      });

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = PlanRoutingController;
