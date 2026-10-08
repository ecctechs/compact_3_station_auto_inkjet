const express = require("express");
const Route = express.Router();
const RouteName = "/uv-job";
const UvJobController = require("../controllers/UvJobController");
const validate = require("../middleware/validate");
const {
  createUvJobSchema,
  updateUvTextSchema,
} = require("../validation/uvJobSchema");

Route.post(
  RouteName + "/create",
  validate(createUvJobSchema),
  UvJobController.create
);

Route.patch(
  RouteName + "/:id",
  validate(updateUvTextSchema),
  UvJobController.update
);

module.exports = Route;
