const express = require("express");
const Route = express.Router();
const RouteName = "/uv-inkjet";
const UVInkjetController = require("../controllers/UVInkjetController");

Route.post(RouteName + "/create", UVInkjetController.create);

Route.get(RouteName + "/getAll", UVInkjetController.getAll);

Route.get(RouteName + "/getById/:id", UVInkjetController.getById);

Route.put(RouteName + "/update/:id", UVInkjetController.update);

Route.delete(RouteName + "/delete/:id", UVInkjetController.delete);

module.exports = Route;
