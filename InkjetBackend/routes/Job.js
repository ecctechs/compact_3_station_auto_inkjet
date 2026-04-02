const express = require("express");
const Route = express.Router();
const RouteName = "/job";
const JobController = require("../controllers/JobController");
const validate = require("../middleware/validate");
const {
  createJobSchema,
  jobFilterSchema,
  jobResultsSchema,
} = require("../validation/jobSchema");

Route.post(
  RouteName + "/create",
  validate(createJobSchema),
  JobController.create
);

Route.get(
  RouteName + "/getAll",
  validate(jobFilterSchema, "query"),
  JobController.getAll
);

Route.get(RouteName + "/getById/:id", JobController.getById);

Route.post(RouteName + "/execute/:id", JobController.execute);

Route.get(RouteName + "/getResolved/:id", JobController.getResolved);

Route.post(
  RouteName + "/postResults/:id",
  validate(jobResultsSchema),
  JobController.postResults
);

Route.post(RouteName + "/retry/:id", JobController.retry);

Route.post(RouteName + "/update/:id", JobController.update);

// ==================== LastSentJob Routes (Station 3) ====================

Route.post(RouteName + "/lastSent/create", JobController.createLastSent);

Route.get(RouteName + "/lastSent/getAll", JobController.getAllLastSent);

Route.get(RouteName + "/lastSent/getById/:id", JobController.getLastSentById);

Route.post(RouteName + "/lastSent/update/:id", JobController.updateLastSent);

Route.delete(RouteName + "/lastSent/remove/:id", JobController.removeLastSent);


module.exports = Route;
