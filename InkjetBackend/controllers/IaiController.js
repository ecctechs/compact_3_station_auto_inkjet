const ResponseManager = require("../middleware/ResponseManager");
const { IaiClampSetting } = require("../model/iaiSettingModel");
const { PrintJob } = require("../model/jobModel");

// ค่าที่หาไม่เจอเก็บเป็น null ไม่ใช่ 0 — 0 mm เป็นระยะที่ใช้ได้จริง
// ถ้าเก็บ 0 แทน "ไม่มีค่า" จะแยกไม่ออกว่ายังไม่ได้ setup หรือ setup ไว้ที่ 0 พอดี
const toInt = (v) => { // แปลงระยะที่รับมาเป็นจำนวนเต็ม
  if (v === null || v === undefined || v === "") return null; // ไม่มีค่าให้คงเป็น null
  const n = Number(v); // แปลงค่าที่รับเป็นตัวเลข
  return Number.isFinite(n) ? Math.round(n) : null; // ปัดเป็นจำนวนเต็ม แปลงไม่ได้ให้ค่าว่าง
};

const pick = (body) => ({ // เลือกเฉพาะชื่อโปรแกรมและระยะแคลมป์
  m1_program_name: body.m1_program_name ?? null, // เก็บชื่อโปรแกรม Plate
  iaip: toInt(body.iaip), // ระยะ Plate แกน X
  iaip_z1: toInt(body.iaip_z1), // ระยะ Plate แกน Z1
  iaip_z2: toInt(body.iaip_z2), // ระยะ Plate แกน Z2
  m2_program_name: body.m2_program_name ?? null, // เก็บชื่อโปรแกรม Shim
  iai: toInt(body.iai), // ระยะ Shim แกน X
  iai_z1: toInt(body.iai_z1), // ระยะ Shim แกน Z1
  iai_z2: toInt(body.iai_z2), // ระยะ Shim แกน Z2
});

class IaiController {
  /**
   * POST /iai/create
   * body: { print_jobs_id, m1_program_name, iaip, m2_program_name, iai, ... }
   *
   * 1 job = 1 แถว — เรียกซ้ำด้วย job เดิมจะทับแถวเดิม ไม่สร้างซ้ำ
   * ค่าไหนหาไม่เจอส่ง null มาได้ ระบบจะเก็บ null ไว้เพื่อบอกว่า "หาแล้วไม่มี"
   */
  static async create(req, res) { // Flow 12: รับ /iai/create
    try { // ลองบันทึกหรืออ่านข้อมูลตามคำขอ
      const jobId = Number(req.body.print_jobs_id); // อ่าน Job ID ที่จะเก็บค่าแคลมป์
      if (!Number.isInteger(jobId) || jobId <= 0) { // Job ID ต้องเป็นจำนวนเต็มมากกว่า 0
        return ResponseManager.ErrorResponse( // ปฏิเสธคำขอที่ไม่มี Job ID ถูกต้อง
          req, // แนบคำขอเดิมให้ตัวตอบกลับ
          res, // ใช้ช่องตอบกลับของคำขอนี้
          400, // ตอบว่าเนื้อหาคำขอไม่ถูกต้อง
          "print_jobs_id is required" // ระบุว่าต้องส่ง Job ID
        );
      }

      const payload = { print_jobs_id: jobId, ...pick(req.body) }; // รวม Job ID กับค่า IAI ที่เลือกไว้

      const existing = await IaiClampSetting.findOne({ // ค้นว่า Job นี้มีค่า IAI แล้วหรือยัง
        where: { print_jobs_id: jobId }, // หาเฉพาะ Job ที่รับมา
      });

      let row; // เก็บแถวที่จะส่งกลับ
      let created = false; // เริ่มจากยังไม่ได้สร้างแถวใหม่

      if (existing) { // Job นี้เคยเก็บค่า IAI แล้ว
        await existing.update(payload); // อัปเดตแถวเดิม
        row = existing; // ใช้แถวเดิมเป็นผลลัพธ์
      } else { // ยังไม่มีค่า IAI ของ Job นี้
        row = await IaiClampSetting.create(payload); // สร้างแถว IAI ใหม่
        created = true; // จำว่าครั้งนี้สร้างแถวใหม่
      }

      return ResponseManager.SuccessResponse(req, res, created ? 201 : 200, row); // คืน 201 เมื่อสร้างใหม่ หรือ 200 เมื่อแก้แถวเดิม
    } catch (err) { // ทำงานไม่สำเร็จให้ส่งสาเหตุคืน
      return ResponseManager.CatchResponse(req, res, err.message); // ส่งข้อความผิดพลาดกลับไป C#
    }
  }

  // GET /iai/getByJob/:jobId — ค่าแคลมป์ของงานใบนั้น
  static async getByJob(req, res) {
    try {
      const jobId = Number(req.params.jobId);
      if (!Number.isInteger(jobId) || jobId <= 0) {
        return ResponseManager.ErrorResponse(req, res, 400, "invalid jobId");
      }

      const row = await IaiClampSetting.findOne({
        where: { print_jobs_id: jobId },
      });

      if (!row) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          `ไม่พบค่าแคลมป์ของ job ${jobId}`
        );
      }

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  // GET /iai/getAll?limit= — เรียงงานล่าสุดขึ้นก่อน
  static async getAll(req, res) {
    try {
      const { limit } = req.query;
      const rows = await IaiClampSetting.findAll({
        order: [["id", "DESC"]],
        limit: limit ? Number(limit) : 100,
      });
      return ResponseManager.SuccessResponse(req, res, 200, rows);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /iai/update/:jobId
   * body: { iai } หรือ { iaip } — ใช้ตอน operator ปรับระยะแล้วกด Upload
   * ส่งฟิลด์ไหนมาเขียนเฉพาะฟิลด์นั้น ไม่ทับค่าที่ไม่ได้ส่ง
   *
   * รับเฉพาะงานที่เริ่มไปแล้ว (status = Process) — ระยะแคลมป์เป็นค่าที่ผูกกับ
   * ชิ้นงานที่อยู่ที่เครื่องตอนนั้น งานที่ยังไม่เริ่มจึงไม่มีอะไรให้ปรับ
   * ด่านนี้อยู่ฝั่ง server เพราะหน้าจอกันได้แค่ปุ่ม แต่ API ยิงตรงได้เสมอ
   */
  static async update(req, res) {
    try {
      const jobId = Number(req.params.jobId);

      const job = await PrintJob.findByPk(jobId);
      if (!job) {
        return ResponseManager.ErrorResponse(req, res, 404, `ไม่พบ job ${jobId}`);
      }

      if (String(job.status).toLowerCase() !== "process") {
        return ResponseManager.ErrorResponse(
          req,
          res,
          409,
          `job ${jobId} ยังไม่ได้เริ่มงาน (สถานะ ${job.status}) — ส่งค่า IAI ไม่ได้`
        );
      }

      const row = await IaiClampSetting.findOne({
        where: { print_jobs_id: jobId },
      });

      if (!row) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          `ไม่พบค่าแคลมป์ของ job ${jobId}`
        );
      }

      const patch = {};
      for (const f of ["iai", "iai_z1", "iai_z2", "iaip", "iaip_z1", "iaip_z2"]) {
        if (f in req.body) patch[f] = toInt(req.body[f]);
      }
      for (const f of ["m1_program_name", "m2_program_name"]) {
        if (f in req.body) patch[f] = req.body[f] ?? null;
      }

      await row.update(patch);
      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = IaiController;
