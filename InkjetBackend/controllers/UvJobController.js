const ResponseManager = require("../middleware/ResponseManager");
const { UvJobData } = require("../model/uvJobDataModel");

class UvJobController {
  /**
   * POST /uv-job/create
   * Replaces all uv_job_data rows for the given print_jobs_id, then bulk-inserts new ones.
   */
  static async create(req, res) { // Flow 10: รับ /uv-job/create
    try { // ลองบันทึกหรืออ่านข้อมูลตามคำขอ
      const { print_jobs_id, items } = req.body; // รับ Job ID และรายการ UV

      await UvJobData.destroy({ where: { print_jobs_id } }); // ล้าง UV เดิมของ Job นี้ก่อนใส่ชุดใหม่

      const rows = items.map((item) => ({ ...item, print_jobs_id })); // ใส่ Job ID ให้ทุกรายการ UV
      const created = await UvJobData.bulkCreate(rows); // บันทึกรายการ UV ทั้งชุด

      return ResponseManager.SuccessResponse(req, res, 201, created); // ส่งรายการที่บันทึกแล้วกลับให้ C#
    } catch (err) { // ทำงานไม่สำเร็จให้ส่งสาเหตุคืน
      return ResponseManager.CatchResponse(req, res, err.message); // ส่งข้อความผิดพลาดกลับไป C#
    }
  }

  /**
   * PATCH /uv-job/:id
   * body: { text1..text5 }
   *
   * แก้ข้อความของแถวเดียว — หน้า Order Detail ให้คนหน้างานพิมพ์ทับได้ทั้งห้าช่อง
   *
   * แก้เฉพาะช่องที่ส่งมา ช่องที่ไม่ได้ส่งคงค่าเดิมไว้ และไม่แตะ machine กับ
   * program_name เพราะสองตัวนั้นเป็นตัวบอกว่าแถวนี้เป็นของเครื่องไหนและใช้โปรแกรมไหน
   * ไม่ใช่ข้อความที่พ่นลงชิ้นงาน
   */
  static async update(req, res) {
    try {
      const row = await UvJobData.findByPk(req.params.id);
      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, "UV job data not found");
      }

      const patch = {};
      for (const field of ["text1", "text2", "text3", "text4", "text5"]) {
        if (req.body[field] !== undefined) patch[field] = req.body[field];
      }

      // omitNull ถูกเปิดไว้ทั้งโปรเจค การล้างข้อความให้ว่างจึงต้องปิดตรงนี้
      await row.update(patch, { omitNull: false });
      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = UvJobController;
