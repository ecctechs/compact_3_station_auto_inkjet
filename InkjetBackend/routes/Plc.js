const express = require("express");
const Route = express.Router();
const RouteName = "/plc-setting";
const PlcController = require("../controllers/PlcController");

Route.get(RouteName + "/getAll", PlcController.getAll);
Route.post(RouteName + "/bulkSave", PlcController.bulkSave);

module.exports = Route;
