const express = require("express");
const Route = express.Router();
const RouteName = "/system";
const SystemController = require("../controllers/SystemController");

Route.get(RouteName + "/ping", SystemController.ping);

Route.get(RouteName + "/getSizeMap", SystemController.getSizeMap);

Route.get(RouteName + "/getTranslations", SystemController.getTranslations);

Route.post(RouteName + "/resetRuntime", SystemController.resetRuntime);

// ชุดข้อมูลงานทดสอบ (หน้า Station Options โหมด Dev)
Route.post(RouteName + "/clearJobData", SystemController.clearJobData);
Route.get(RouteName + "/exportJobData", SystemController.exportJobData);
Route.post(RouteName + "/importJobData", SystemController.importJobData);

module.exports = Route;
