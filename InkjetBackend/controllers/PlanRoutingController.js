const ResponseManager = require("../middleware/ResponseManager");
const { PlanRouting } = require("../model/planRoutingModel");

class PlanRoutingController {
  /**
   * POST /plan-routing/create
   * Store plan routing for a job (captured at register from source DB plan_routing).
   * Body: { print_jobs_id, lot_no, erp_mfg, marking_method, process_sequence }
   * Replaces any existing row for the same print_jobs_id (re-register safe).
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

      // upsert: ลบของเดิมของ job นี้ก่อน แล้วค่อยใส่ใหม่
      if (print_jobs_id !== undefined && print_jobs_id !== null) {
        await PlanRouting.destroy({ where: { print_jobs_id } });
      }

      const created = await PlanRouting.create({
        print_jobs_id: print_jobs_id ?? null,
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
   * Poll plan routing for a job → operator ucOrder ใช้เปิด/ปิดปุ่มส่งเครื่อง.
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

  /**
   * DELETE /plan-routing/deleteByJob/:jobId
   */
  static async deleteByJob(req, res) {
    try {
      const count = await PlanRouting.destroy({
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

module.exports = PlanRoutingController;
