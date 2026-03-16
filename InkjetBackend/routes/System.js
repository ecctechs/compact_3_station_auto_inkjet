const express = require("express");
const Route = express.Router();
const RouteName = "/system";
const SystemController = require("../controllers/SystemController");

Route.get(RouteName + "/getSizeMap", SystemController.getSizeMap);

Route.get(RouteName + "/getTranslations", SystemController.getTranslations);

module.exports = Route;
