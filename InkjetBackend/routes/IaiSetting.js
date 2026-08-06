const express = require("express");
const Route = express.Router();
const RouteName = "/iai-setting";
const IaiSettingController = require("../controllers/IaiSettingController");
const validate = require("../middleware/validate");
const {
  upsertIaiSchema,
  bulkUpsertIaiSchema,
  updateIaiSchema,
  iaiFilterSchema,
} = require("../validation/iaiSettingSchema");

Route.get(
  RouteName + "/getAll",
  validate(iaiFilterSchema, "query"),
  IaiSettingController.getAll
);

Route.get(RouteName + "/getById/:id", IaiSettingController.getById);

Route.get(RouteName + "/lookup/:programName", IaiSettingController.lookup);

Route.get(RouteName + "/variants/:baseName", IaiSettingController.variants);

Route.post(
  RouteName + "/upsert",
  validate(upsertIaiSchema),
  IaiSettingController.upsert
);

Route.post(
  RouteName + "/bulkUpsert",
  validate(bulkUpsertIaiSchema),
  IaiSettingController.bulkUpsert
);

Route.put(
  RouteName + "/update/:id",
  validate(updateIaiSchema),
  IaiSettingController.update
);

Route.delete(RouteName + "/delete/:id", IaiSettingController.delete);

module.exports = Route;
