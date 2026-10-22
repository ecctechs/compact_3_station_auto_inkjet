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
  switchProgramSchema,
  skipSchema,
  beginSendSchema,
  finishSendSchema,
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

Route.post(
  RouteName + "/skip",
  validate(skipSchema),
  MachineQueueController.skip
);

Route.post(
  RouteName + "/switch-program",
  validate(switchProgramSchema),
  MachineQueueController.switchProgram
);

Route.patch(
  RouteName + "/:id",
  validate(updateQueueSchema),
  MachineQueueController.update
);

Route.post(RouteName + "/:id/begin-send", validate(beginSendSchema), MachineQueueController.beginSend);
// ส่งซ้ำเข้าเครื่องที่งานถืออยู่ (หลัง SWAP / แก้ข้อมูล) — ใช้ finish-send ตัวเดิมปิดผล
Route.post(RouteName + "/:id/begin-resend", validate(beginSendSchema), MachineQueueController.beginResend);
Route.post(RouteName + "/:id/finish-send", validate(finishSendSchema), MachineQueueController.finishSend);

Route.delete(RouteName + "/job/:jobId", MachineQueueController.clearJob);

module.exports = Route;
