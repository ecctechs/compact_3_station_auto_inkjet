const ResponseManager = require("../middleware/ResponseManager");
const sequelize = require("../database");
const { Op } = require("sequelize");
const { IaiClampSetting } = require("../model/iaiSettingModel");
const {
  resolveProgramFields,
  oppositeFields,
  siblingProgramName,
  escapeLike,
  toNormalized,
} = require("../utils/iaiProgramFields");

const NOT_FOUND = "IAI clamp setting not found";

/**
 * เทียบเท่า INSERT OR IGNORE + UPDATE ... WHERE program = ? ของเดิม ในก้อนเดียว
 *
 * 1 แถว = 1 ชิ้นงาน ที่มีโปรแกรม 2 ตัว (Plate P-XXX และ Shim S-XXX) แต่ละตัวมีค่าแคลมป์ของตัวเอง
 * ไม่ใช่ 1 แถว = 1 โปรแกรม — จึงต้องหาแถวผ่านชื่อคู่ก่อนตัดสินใจสร้างแถวใหม่
 *
 * ไม่ใช้ bulkSave แบบ destroy-all เหมือน plc-setting เพราะ IAI เป็นข้อมูลสะสม
 * ถ้าสถานีหนึ่งส่งมาแค่โปรแกรมฝั่งตัวเอง แถวของอีกฝั่งจะหายทั้งหมดและกู้ไม่ได้
 */
async function upsertOne(payload, transaction) {
  const { program_name, iai, iai_z1, iai_z2, status } = payload;
  const f = resolveProgramFields(program_name);

  // 1. หาแถวที่มีชื่อโปรแกรมนี้อยู่แล้ว
  let row = await IaiClampSetting.findOne({
    where: { [f.programField]: program_name },
    transaction,
  });

  // 2. ไม่เจอ -> ลองหาแถวของชิ้นงานเดียวกันผ่านชื่อคู่ (S-XXX <-> P-XXX)
  //    รับเฉพาะแถวที่ช่องฝั่งเรายังว่าง ถ้าถูกโปรแกรมอื่นจองไว้แล้วให้ไปสร้างแถวใหม่แทน
  let linked = false;
  if (!row) {
    const sibling = siblingProgramName(program_name);
    if (sibling) {
      const o = oppositeFields(f);
      row = await IaiClampSetting.findOne({
        where: { [o.programField]: sibling, [f.programField]: null },
        transaction,
      });
      linked = !!row;
    }
  }

  // 3. ยังไม่เจอ -> ชิ้นงานใหม่ สร้างแถว
  if (!row) {
    const fresh = await IaiClampSetting.create(
      {
        [f.programField]: program_name,
        [f.valueField]: iai,
        [f.z1Field]: iai_z1,
        [f.z2Field]: iai_z2,
        status: status !== undefined ? status : true,
      },
      { transaction }
    );
    return { created: true, linked: false, ...toNormalized(fresh, f), row: fresh };
  }

  // 4. อัปเดตเฉพาะฝั่งของตัวเอง — ค่าอีกฝั่งต้องไม่ถูกแตะ
  const patch = {
    [f.valueField]: iai !== undefined ? iai : row[f.valueField],
    [f.z1Field]: iai_z1 !== undefined ? iai_z1 : row[f.z1Field],
    [f.z2Field]: iai_z2 !== undefined ? iai_z2 : row[f.z2Field],
    status: status !== undefined ? status : row.status,
  };

  // เจอผ่านชื่อคู่ = แถวนี้ยังไม่มีชื่อฝั่งเรา ต้องเติมเข้าไป
  if (linked) patch[f.programField] = program_name;

  await row.update(patch, { transaction });

  return { created: false, linked, ...toNormalized(row, f), row };
}

class IaiSettingController {
  /**
   * GET /iai-setting/getAll
   * คืนทุกแถวเมื่อไม่ส่ง page/limit, paginate เมื่อร้องขอ
   * ?q ค้นชื่อโปรแกรมทั้งสองฝั่ง
   */
  static async getAll(req, res) {
    try {
      const { page, limit, q } = req.query;

      const where = q
        ? {
            [Op.or]: [
              { m2_program_name: { [Op.iLike]: `%${escapeLike(q)}%` } },
              { m1_program_name: { [Op.iLike]: `%${escapeLike(q)}%` } },
            ],
          }
        : {};

      const order = [
        ["m2_program_name", "ASC"],
        ["m1_program_name", "ASC"],
      ];

      if (!page && !limit) {
        const rows = await IaiClampSetting.findAll({ where, order });
        return ResponseManager.SuccessResponse(req, res, 200, {
          data: rows,
          total: rows.length,
          page: 1,
          limit: rows.length,
        });
      }

      const pageNum = parseInt(page) || 1;
      const limitNum = parseInt(limit) || 10;
      const offset = (pageNum - 1) * limitNum;

      const { count, rows } = await IaiClampSetting.findAndCountAll({
        where,
        order,
        offset,
        limit: limitNum,
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: rows,
        total: count,
        page: pageNum,
        limit: limitNum,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /iai-setting/getById/:id
   */
  static async getById(req, res) {
    try {
      const row = await IaiClampSetting.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, NOT_FOUND);
      }

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /iai-setting/lookup/:programName
   * คำถามหลัก: โปรแกรมนี้ค่าแคลมป์เท่าไร
   * คืนรูปแบบกลาง ผู้เรียกไม่ต้องรู้ว่าค่าอยู่คอลัมน์ IAI หรือ IAIP
   */
  static async lookup(req, res) {
    try {
      const programName = req.params.programName;
      const f = resolveProgramFields(programName);

      const row = await IaiClampSetting.findOne({
        where: { [f.programField]: programName },
      });

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, NOT_FOUND);
      }

      return ResponseManager.SuccessResponse(req, res, 200, {
        ...toNormalized(row, f),
        row,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /iai-setting/variants/:baseName
   * แทน WHERE <programField> = 'base' OR <programField> LIKE 'base-%'
   */
  static async variants(req, res) {
    try {
      const base = req.params.baseName;
      const f = resolveProgramFields(base);

      const rows = await IaiClampSetting.findAll({
        where: {
          [Op.or]: [
            { [f.programField]: base },
            // iLike ไม่ใช่ like: SQLite LIKE เป็น case-insensitive แต่ Postgres LIKE เป็น case-sensitive
            { [f.programField]: { [Op.iLike]: `${escapeLike(base)}-%` } },
          ],
        },
        order: [[f.programField, "ASC"]],
      });

      const data = rows.map((r) => toNormalized(r, f));

      return ResponseManager.SuccessResponse(req, res, 200, {
        data,
        total: data.length,
        page: 1,
        limit: data.length,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /iai-setting/upsert
   * Body: { program_name, iai, iai_z1, iai_z2, status }
   */
  static async upsert(req, res) {
    try {
      const result = await upsertOne(req.body);
      return ResponseManager.SuccessResponse(
        req,
        res,
        result.created ? 201 : 200,
        result
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /iai-setting/bulkUpsert
   * Body: { rows: [{ program_name, iai, ... }, ...] } ทำใน transaction เดียว
   */
  static async bulkUpsert(req, res) {
    const t = await sequelize.transaction();
    try {
      const out = [];
      for (const r of req.body.rows) {
        out.push(await upsertOne(r, t));
      }

      await t.commit();

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: out,
        total: out.length,
      });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * PUT /iai-setting/update/:id
   * แก้ตรงคอลัมน์ ใช้ตอนแก้มือ
   */
  static async update(req, res) {
    try {
      const row = await IaiClampSetting.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, NOT_FOUND);
      }

      const b = req.body;
      const keep = (k) => (b[k] !== undefined ? b[k] : row[k]);

      await row.update({
        m2_program_name: keep("m2_program_name"),
        iai: keep("iai"),
        iai_z1: keep("iai_z1"),
        iai_z2: keep("iai_z2"),
        status: keep("status"),
        m1_program_name: keep("m1_program_name"),
        iaip: keep("iaip"),
        iaip_z1: keep("iaip_z1"),
        iaip_z2: keep("iaip_z2"),
      });

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /iai-setting/delete/:id
   */
  static async delete(req, res) {
    try {
      const row = await IaiClampSetting.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, NOT_FOUND);
      }

      await row.destroy();

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: "IAI clamp setting deleted successfully",
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = IaiSettingController;
