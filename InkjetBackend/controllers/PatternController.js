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

// ไม่ระบุ order = Postgres คืนแถวตามลำดับที่มันสะดวก (เช่น 2,1)
// ordinal คือช่องเครื่อง (1 = MK-058, 2 = MK-059) จึงต้องเรียงให้แน่นอนเสมอ
const PATTERN_ORDER = [
  [{ model: InkjetConfig, as: "inkjet_configs" }, "ordinal", "ASC"],
  [
    { model: InkjetConfig, as: "inkjet_configs" },
    { model: TextBlock, as: "text_blocks" },
    "block_number",
    "ASC",
  ],
  [{ model: ServoConfig, as: "servo_configs" }, "ordinal", "ASC"],
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
        order: PATTERN_ORDER,
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

  /**
   * GET /pattern/lookup/:barcode
   * barcode ซ้ำได้ → คืนตัวล่าสุด (สำเนาของ job ที่ register ทีหลังสุด)
   */
  static async lookup(req, res) {
    try {
      const pattern = await Pattern.findOne({
        where: { barcode: req.params.barcode, is_active: true },
        include: PATTERN_INCLUDE,
        order: [["id", "DESC"], ...PATTERN_ORDER],
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

  /**
   * POST /pattern/create
   * สร้าง pattern เป็นสำเนาของค่าที่ใช้พิมพ์ ผูกกับ job หนึ่งใบ
   * barcode ซ้ำได้ — lot เดิม register ใหม่ต้องได้ข้อมูลล่าสุดเสมอ
   */
  static async create(req, res) {
    const t = await sequelize.transaction();
    try {
      const {
        job_id,
        barcode,
        description,
        inkjet_configs,
        conveyor_speeds,
        servo_configs,
      } = req.body;

      const pattern = await Pattern.create(
        { job_id, barcode, description },
        { transaction: t }
      );

      if (inkjet_configs) {
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
        await ConveyorSpeed.create(
          { ...conveyor_speeds, pattern_id: pattern.id },
          { transaction: t }
        );
      }

      if (servo_configs) {
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
        order: PATTERN_ORDER,
      });

      return ResponseManager.SuccessResponse(req, res, 201, result);
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
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
        order: PATTERN_ORDER,
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
