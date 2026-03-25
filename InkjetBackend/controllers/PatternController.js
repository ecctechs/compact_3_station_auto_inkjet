const ResponseManager = require("../middleware/ResponseManager");
const {
  Pattern,
  InkjetConfig,
  TextBlock,
  ConveyorSpeed,
  ServoConfig,
  PatternRule,
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
  { model: PatternRule, as: "rules" },
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

  static async create(req, res) {
    const t = await sequelize.transaction();
    try {
      const { name, barcode, description, inkjet_configs, conveyor_speeds, servo_configs, rules } = req.body;

      const existing = await Pattern.findOne({ where: { barcode } });
      if (existing) {
        await t.rollback();
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "Barcode already exists"
        );
      }

      const pattern = await Pattern.create({ name, barcode, description }, { transaction: t });

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

      if (rules) {
        let ruleOrdinal = 1;
        for (const rule of rules) {
          // Generate name if not provided
          const ruleName = rule.name || `RULE_${pattern.id}_${ruleOrdinal}`;
          await PatternRule.create(
            { ...rule, name: ruleName, pattern_id: pattern.id },
            { transaction: t }
          );
          ruleOrdinal++;
        }
      }

      await t.commit();

      const result = await Pattern.findByPk(pattern.id, {
        include: PATTERN_INCLUDE,
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

      const { name, inkjet_configs, conveyor_speeds, servo_configs, rules, ...patternData } = req.body;

      await pattern.update({ name, ...patternData }, { transaction: t });

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

      if (rules) {
        await PatternRule.destroy({
          where: { pattern_id: pattern.id },
          transaction: t,
        });
        let ruleOrdinal = 1;
        for (const rule of rules) {
          const ruleName = rule.name || `RULE_${pattern.id}_${ruleOrdinal}`;
          await PatternRule.create(
            { ...rule, name: ruleName, pattern_id: pattern.id },
            { transaction: t }
          );
          ruleOrdinal++;
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

  // ========== PatternRule Standalone CRUD ==========
  static async getAllRules(req, res) {
    try {
      const { pattern_id, is_active } = req.query;
      const where = {};

      if (pattern_id) {
        where.pattern_id = pattern_id;
      }
      if (is_active !== undefined) {
        where.is_active = is_active === "true";
      }

      const rules = await PatternRule.findAll({
        where,
        order: [["pattern_id", "ASC"], ["ordinal", "ASC"]],
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: rules,
        total: rules.length,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async getRuleById(req, res) {
    try {
      const rule = await PatternRule.findByPk(req.params.id);

      if (!rule) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern rule not found"
        );
      }

      return ResponseManager.SuccessResponse(req, res, 200, rule);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async createRule(req, res) {
    try {
      const { name, pattern_id, ordinal, source_start, source_end, transform_rule, parameter, is_active } = req.body;

      // Check unique name
      const existingByName = await PatternRule.findOne({ where: { name } });
      if (existingByName) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "Pattern rule name already exists"
        );
      }

      // Check pattern exists
      const pattern = await Pattern.findByPk(pattern_id);
      if (!pattern) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          400,
          "Pattern not found"
        );
      }

      const rule = await PatternRule.create({
        name,
        pattern_id,
        ordinal,
        source_start,
        source_end,
        transform_rule,
        parameter,
        is_active,
      });

      return ResponseManager.SuccessResponse(req, res, 201, rule);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async updateRule(req, res) {
    try {
      const rule = await PatternRule.findByPk(req.params.id);
      if (!rule) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern rule not found"
        );
      }

      const { name, pattern_id, ordinal, source_start, source_end, transform_rule, parameter, is_active } = req.body;

      // Check unique name (exclude current record)
      if (name && name !== rule.name) {
        const existingByName = await PatternRule.findOne({ where: { name } });
        if (existingByName) {
          return ResponseManager.ErrorResponse(
            req,
            res,
            400,
            "Pattern rule name already exists"
          );
        }
      }

      // Check pattern exists if pattern_id is being changed
      if (pattern_id && pattern_id !== rule.pattern_id) {
        const pattern = await Pattern.findByPk(pattern_id);
        if (!pattern) {
          return ResponseManager.ErrorResponse(
            req,
            res,
            400,
            "Pattern not found"
          );
        }
      }

      await rule.update({
        name,
        pattern_id,
        ordinal,
        source_start,
        source_end,
        transform_rule,
        parameter,
        is_active,
      });

      return ResponseManager.SuccessResponse(req, res, 200, rule);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async deleteRule(req, res) {
    try {
      const rule = await PatternRule.findByPk(req.params.id);
      if (!rule) {
        return ResponseManager.ErrorResponse(
          req,
          res,
          404,
          "Pattern rule not found"
        );
      }

      await rule.update({ is_active: false });

      return ResponseManager.SuccessResponse(
        req,
        res,
        200,
        "Pattern rule deactivated"
      );
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = PatternController;
