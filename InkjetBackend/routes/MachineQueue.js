const express = require("express");
const Route = express.Router();
const RouteName = "/machine-queue";
const MachineQueueController = require("../controllers/MachineQueueController");
const validate = require("../middleware/validate");
const {
  enqueueSchema,
  machineSchema,
  releaseSchema,
  claimSchema,
  updateQueueSchema,
  beginSendSchema,
  finishSendSchema,
  recoverSchema,
} = require("../validation/machineQueueSchema");

Route.get(RouteName + "/getAll", MachineQueueController.getAll);

Route.post(
  RouteName + "/enqueue",
  validate(enqueueSchema),
  MachineQueueController.enqueue
);

Route.post(
  RouteName + "/claim",
  validate(claimSchema),
  MachineQueueController.claim
);

Route.post(
  RouteName + "/release",
  validate(releaseSchema),
  MachineQueueController.release
);

Route.patch(
  RouteName + "/:id",
  validate(updateQueueSchema),
  MachineQueueController.update
);

Route.post(RouteName + "/:id/begin-send", validate(beginSendSchema), MachineQueueController.beginSend);
Route.post(RouteName + "/:id/finish-send", validate(finishSendSchema), MachineQueueController.finishSend);
Route.post(RouteName + "/:id/recover", validate(recoverSchema), MachineQueueController.recover);

Route.delete(RouteName + "/job/:jobId", MachineQueueController.clearJob);

module.exports = Route;
