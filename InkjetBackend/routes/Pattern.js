const express = require("express");
const Route = express.Router();
const RouteName = "/pattern";
const PatternController = require("../controllers/PatternController");
const validate = require("../middleware/validate");
const {
  createPatternSchema,
  updatePatternSchema,
  patternFilterSchema,
  createPatternRuleSchema,
  updatePatternRuleSchema,
} = require("../validation/patternSchema");

Route.get(
  RouteName + "/getAll",
  validate(patternFilterSchema, "query"),
  PatternController.getAll
);

Route.get(RouteName + "/getById/:id", PatternController.getById);

Route.get(RouteName + "/lookup/:barcode", PatternController.lookup);

Route.post(
  RouteName + "/create",
  validate(createPatternSchema),
  PatternController.create
);

Route.put(
  RouteName + "/update/:id",
  validate(updatePatternSchema),
  PatternController.update
);

Route.delete(RouteName + "/delete/:id", PatternController.delete);

// ========== PatternRule Standalone Routes ==========
Route.get(RouteName + "/rule/getAll", PatternController.getAllRules);

Route.get(RouteName + "/rule/getById/:id", PatternController.getRuleById);

Route.post(
  RouteName + "/rule/create",
  validate(createPatternRuleSchema),
  PatternController.createRule
);

Route.put(
  RouteName + "/rule/update/:id",
  validate(updatePatternRuleSchema),
  PatternController.updateRule
);

Route.delete(RouteName + "/rule/delete/:id", PatternController.deleteRule);

module.exports = Route;
