const express = require("express");
const Route = express.Router();
const RouteName = "/pattern";
const PatternController = require("../controllers/PatternController");
const validate = require("../middleware/validate");
const {
  createPatternSchema,
  updatePatternSchema,
  patternFilterSchema,
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

module.exports = Route;
