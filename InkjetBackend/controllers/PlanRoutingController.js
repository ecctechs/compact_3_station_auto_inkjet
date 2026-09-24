const ResponseManager = require("../middleware/ResponseManager");
const { PlanRouting } = require("../model/planRoutingModel");

class PlanRoutingController {
  /**
   * POST /plan-routing/create
   * Replaces the plan_routing row for the given print_jobs_id, then inserts the new one.
   */
  static async create(req, res) { // Flow 11: รับ /plan-routing/create
    try { // ลองบันทึกหรืออ่านข้อมูลตามคำขอ
      const { // แยกแผนงานจากคำขอ
        print_jobs_id, // รหัส Job ที่ใช้ผูกข้อมูล
        lot_no, // เลข Lot
        erp_mfg, // Order No จากต้นทาง
        marking_method, // รหัสวิธีพิมพ์
        process_sequence, // ลำดับกระบวนการ
      } = req.body; // ใช้ข้อมูล JSON ที่ C# ส่งมา

      await PlanRouting.destroy({ where: { print_jobs_id } }); // ล้างแผนงานเดิมของ Job นี้

      const created = await PlanRouting.create({ // สร้างแผนงานชุดใหม่
        print_jobs_id, // รหัส Job ที่ใช้ผูกข้อมูล
        lot_no: lot_no ?? null, // เก็บ Lot ไม่มีค่าให้ใช้ null
        erp_mfg: erp_mfg ?? null, // เก็บ Order No ไม่มีค่าให้ใช้ null
        marking_method: marking_method ?? null, // เก็บวิธีพิมพ์ ไม่มีค่าให้ใช้ null
        process_sequence: process_sequence ?? null, // เก็บลำดับกระบวนการ ไม่มีค่าให้ใช้ null
      });

      return ResponseManager.SuccessResponse(req, res, 201, created); // ส่งแผนงานที่บันทึกแล้วกลับไป
    } catch (err) { // ทำงานไม่สำเร็จให้ส่งสาเหตุคืน
      return ResponseManager.CatchResponse(req, res, err.message); // ส่งข้อความผิดพลาดกลับไป C#
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
