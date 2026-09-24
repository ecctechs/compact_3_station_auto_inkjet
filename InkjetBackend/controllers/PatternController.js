const ResponseManager = require("../middleware/ResponseManager");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
} = require("../model/patternModel");
const sequelize = require("../database");
const { Op } = require("sequelize");

const PATTERN_INCLUDE = [
  {
    model: InkjetConfig,
    as: "inkjet_configs",
    include: [{ model: TextBlock, as: "text_blocks" }],
  },
  { model: ConveyorSpeed, as: "conveyor_speeds" },
  { model: ServoConfig, as: "servo_configs" },
];

class PatternController {
  static async getAll(req, res) {
    try {
      const { barcode, is_active, page, limit } = req.query;
      const where = {};

      if (barcode) {
        where.barcode = { [Op.iLike]: `%${barcode}%` };
      }
      if (is_active !== undefined) {
        where.is_active = is_active;
      }

      const offset = (page - 1) * limit;
      const { count, rows } = await Pattern.findAndCountAll({
        where,
        order: [["id", "DESC"]],
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
      const pattern = await Pattern.findByPk(req.params.id, {
        include: PATTERN_INCLUDE,
      });

      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern not found"
        );
      }

      return ResponseManager.SuccessResponse(req, res, 200, pattern);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async lookup(req, res) {
    try {
      const pattern = await Pattern.findOne({
        where: { barcode: req.params.barcode, is_active: true },
        include: PATTERN_INCLUDE,
      });

      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern not found for barcode"
        );
      }

      return ResponseManager.SuccessResponse(req, res, 200, pattern);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async create(req, res) { // Flow 9: รับ /pattern/create
    const t = await sequelize.transaction(); // เตรียมบันทึก Pattern และตารางลูกพร้อมกัน
    try { // ลองบันทึกหรืออ่านข้อมูลตามคำขอ
      const { barcode, description, job_id, inkjet_configs, conveyor_speeds, servo_configs } = req.body; // แยกข้อมูล Pattern ที่ C# ส่งมา

      const pattern = await Pattern.create( // สร้างหัว Pattern ก่อน
        { barcode, description, job_id }, // ผูกหัว Pattern กับ Job
        { transaction: t } // ใช้ธุรกรรมเดียวกันกับคำสั่งก่อนหน้า
      );

      if (inkjet_configs) { // มีข้อมูล MK ให้บันทึกด้วย
        for (const cfg of inkjet_configs) { // บันทึกค่าหัวพิมพ์ทีละตัว
          const { text_blocks, ...cfgData } = cfg; // แยกข้อความออกจากค่าหัวพิมพ์
          const config = await InkjetConfig.create( // สร้างค่าตั้งหัวพิมพ์
            { ...cfgData, pattern_id: pattern.id }, // ผูกหัวพิมพ์กับ Pattern ที่สร้าง
            { transaction: t } // ใช้ธุรกรรมเดียวกันกับคำสั่งก่อนหน้า
          );

          if (text_blocks) { // มีข้อความพิมพ์ให้บันทึกต่อ
            for (const block of text_blocks) { // บันทึกข้อความทีละบล็อก
              await TextBlock.create( // สร้างแถวข้อความพิมพ์
                { ...block, inkjet_config_id: config.id }, // ผูกข้อความกับค่าหัวพิมพ์ตัวนี้
                { transaction: t } // ใช้ธุรกรรมเดียวกันกับคำสั่งก่อนหน้า
              );
            }
          }
        }
      }

      if (conveyor_speeds) { // มีค่าความเร็วสายพานให้เก็บ
        await ConveyorSpeed.create( // บันทึกความเร็วสายพาน
          { ...conveyor_speeds, pattern_id: pattern.id }, // ผูกความเร็วกับ Pattern นี้
          { transaction: t } // ใช้ธุรกรรมเดียวกันกับคำสั่งก่อนหน้า
        );
      }

      if (servo_configs) { // มีค่า Servo ให้เก็บ
        for (const servo of servo_configs) { // อ่านค่า Servo ทีละตัว
          await ServoConfig.create( // บันทึกค่า Servo
            { ...servo, pattern_id: pattern.id }, // ผูก Servo กับ Pattern นี้
            { transaction: t } // ใช้ธุรกรรมเดียวกันกับคำสั่งก่อนหน้า
          );
        }
      }

      await t.commit(); // ยืนยันข้อมูลทั้งหมดในธุรกรรมนี้

      const result = await Pattern.findByPk(pattern.id, { // อ่าน Pattern ที่บันทึกกลับมา
        include: PATTERN_INCLUDE, // รวมค่าหัวพิมพ์ ข้อความ สายพาน และ Servo
      });

      return ResponseManager.SuccessResponse(req, res, 201, result); // ส่ง Pattern กลับให้หน้าสแกนทำขั้นถัดไป
    } catch (err) { // ทำงานไม่สำเร็จให้ส่งสาเหตุคืน
      await t.rollback(); // ยกเลิกข้อมูลในธุรกรรมนี้เมื่อพลาด
      return ResponseManager.CatchResponse(req, res, err.message); // ส่งข้อความผิดพลาดกลับไป C#
    }
  }

  static async update(req, res) {
    const t = await sequelize.transaction();
    try {
      const pattern = await Pattern.findByPk(req.params.id);
      if (!pattern) {
        await t.rollback();
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern not found"
        );
      }

      const { inkjet_configs, conveyor_speeds, servo_configs, ...patternData } = req.body;

      await pattern.update(patternData, { transaction: t });

      if (inkjet_configs) {
        // Delete old configs and their text blocks, then re-insert
        const oldConfigs = await InkjetConfig.findAll({
          where: { pattern_id: pattern.id },
        });
        const oldConfigIds = oldConfigs.map((c) => c.id);
        if (oldConfigIds.length > 0) {
          await TextBlock.destroy({
            where: { inkjet_config_id: oldConfigIds },
            transaction: t,
          });
        }
        await InkjetConfig.destroy({
          where: { pattern_id: pattern.id },
          transaction: t,
        });

        for (const cfg of inkjet_configs) {
          const { text_blocks, ...cfgData } = cfg;
          const config = await InkjetConfig.create(
            { ...cfgData, pattern_id: pattern.id },
            { transaction: t }
          );

          if (text_blocks) {
            for (const block of text_blocks) {
              await TextBlock.create(
                { ...block, inkjet_config_id: config.id },
                { transaction: t }
              );
            }
          }
        }
      }

      if (conveyor_speeds) {
        await ConveyorSpeed.destroy({
          where: { pattern_id: pattern.id },
          transaction: t,
        });
        await ConveyorSpeed.create(
          { ...conveyor_speeds, pattern_id: pattern.id },
          { transaction: t }
        );
      }

      if (servo_configs) {
        await ServoConfig.destroy({
          where: { pattern_id: pattern.id },
          transaction: t,
        });
        for (const servo of servo_configs) {
          await ServoConfig.create(
            { ...servo, pattern_id: pattern.id },
            { transaction: t }
          );
        }
      }

      await t.commit();

      const result = await Pattern.findByPk(pattern.id, {
        include: PATTERN_INCLUDE,
      });

      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async delete(req, res) {
    try {
      const pattern = await Pattern.findByPk(req.params.id);
      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern not found"
        );
      }

      await pattern.update({ is_active: false });

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "Pattern deactivated"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = PatternController;
