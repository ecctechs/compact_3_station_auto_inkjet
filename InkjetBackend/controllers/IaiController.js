const ResponseManager = require("../middleware/ResponseManager");
const { IaiClampSetting } = require("../model/iaiSettingModel");

// ค่าที่หาไม่เจอเก็บเป็น null ไม่ใช่ 0 — 0 mm เป็นระยะที่ใช้ได้จริง
// ถ้าเก็บ 0 แทน "ไม่มีค่า" จะแยกไม่ออกว่ายังไม่ได้ setup หรือ setup ไว้ที่ 0 พอดี
const toInt = (v) => {
  if (v === null || v === undefined || v === "") return null;
  const n = Number(v);
  return Number.isFinite(n) ? Math.round(n) : null;
};

const pick = (body) => ({
  m1_program_name: body.m1_program_name ?? null,
  iaip: toInt(body.iaip),
  iaip_z1: toInt(body.iaip_z1),
  iaip_z2: toInt(body.iaip_z2),
  m2_program_name: body.m2_program_name ?? null,
  iai: toInt(body.iai),
  iai_z1: toInt(body.iai_z1),
  iai_z2: toInt(body.iai_z2),
});

class IaiController {
  /**
   * POST /iai/create
   * body: { print_jobs_id, m1_program_name, iaip, m2_program_name, iai, ... }
   *
   * 1 job = 1 แถว — เรียกซ้ำด้วย job เดิมจะทับแถวเดิม ไม่สร้างซ้ำ
   * ค่าไหนหาไม่เจอส่ง null มาได้ ระบบจะเก็บ null ไว้เพื่อบอกว่า "หาแล้วไม่มี"
   */
  static async create(req, res) {
    try {
      const jobId = Number(req.body.print_jobs_id);
      if (!Number.isInteger(jobId) || jobId <= 0) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "print_jobs_id is required"
        );
      }

      const payload = { print_jobs_id: jobId, ...pick(req.body) };

      const existing = await IaiClampSetting.findOne({
        where: { print_jobs_id: jobId },
      });

      let row;
      let created = false;

      if (existing) {
        await existing.update(payload);
        row = existing;
      } else {
        row = await IaiClampSetting.create(payload);
        created = true;
      }

      return ResponseManager.SuccessResponse(req, res, created ? 201 : 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
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
   */
  static async update(req, res) {
    try {
      const jobId = Number(req.params.jobId);
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
