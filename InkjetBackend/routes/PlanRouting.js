const express = require("express");
const Route = express.Router();
const RouteName = "/plan-routing";
const PlanRoutingController = require("../controllers/PlanRoutingController");
const validate = require("../middleware/validate");
const { createPlanRoutingSchema } = require("../validation/planRoutingSchema");

Route.post(
  RouteName + "/create",
  validate(createPlanRoutingSchema),
  PlanRoutingController.create
);

Route.get(RouteName + "/getByJob/:jobId", PlanRoutingController.getByJob);

module.exports = Route;
