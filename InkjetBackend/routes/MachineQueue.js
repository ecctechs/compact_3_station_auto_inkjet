const express = require("express");
const Route = express.Router();
const RouteName = "/machine-queue";
const MachineQueueController = require("../controllers/MachineQueueController");
const validate = require("../middleware/validate");
const {
  enqueueSchema,
  machineSchema,
  updateQueueSchema,
} = require("../validation/machineQueueSchema");

Route.get(RouteName + "/getAll", MachineQueueController.getAll);

Route.post(
  RouteName + "/enqueue",
  validate(enqueueSchema),
  MachineQueueController.enqueue
);

Route.post(
  RouteName + "/claim",
  validate(machineSchema),
  MachineQueueController.claim
);

Route.post(
  RouteName + "/release",
  validate(machineSchema),
  MachineQueueController.release
);

Route.patch(
  RouteName + "/:id",
  validate(updateQueueSchema),
  MachineQueueController.update
);

Route.delete(RouteName + "/job/:jobId", MachineQueueController.clearJob);

module.exports = Route;
