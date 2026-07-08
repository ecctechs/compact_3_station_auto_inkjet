const express = require("express");
const Route = express.Router();
const RouteName = "/plc-setting";
const PlcSettingController = require("../controllers/PlcSettingController");

Route.post(RouteName + "/create", PlcSettingController.create);

Route.get(RouteName + "/getAll", PlcSettingController.getAll);

Route.get(RouteName + "/getById/:id", PlcSettingController.getById);

Route.put(RouteName + "/update/:id", PlcSettingController.update);

Route.delete(RouteName + "/delete/:id", PlcSettingController.delete);

Route.post(RouteName + "/bulkSave", PlcSettingController.bulkSave);

module.exports = Route;
