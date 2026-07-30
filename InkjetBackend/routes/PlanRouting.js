const express = require("express");
const Route = express.Router();
const RouteName = "/plan-routing";
const PlanRoutingController = require("../controllers/PlanRoutingController");

Route.post(RouteName + "/create", PlanRoutingController.create);

Route.get(RouteName + "/getByJob/:jobId", PlanRoutingController.getByJob);

Route.delete(RouteName + "/deleteByJob/:jobId", PlanRoutingController.deleteByJob);

module.exports = Route;
