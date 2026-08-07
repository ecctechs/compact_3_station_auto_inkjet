const express = require("express");
const Route = express.Router();
const RouteName = "/uv-job";
const UvJobController = require("../controllers/UvJobController");
const validate = require("../middleware/validate");
const { createUvJobSchema } = require("../validation/uvJobSchema");

Route.post(
  RouteName + "/create",
  validate(createUvJobSchema),
  UvJobController.create
);

module.exports = Route;
