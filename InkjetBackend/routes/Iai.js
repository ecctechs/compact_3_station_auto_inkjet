const express = require("express");
const Route = express.Router();
const RouteName = "/iai";
const IaiController = require("../controllers/IaiController");

Route.post(RouteName + "/create", IaiController.create);
Route.get(RouteName + "/getByJob/:jobId", IaiController.getByJob);
Route.get(RouteName + "/getAll", IaiController.getAll);
Route.post(RouteName + "/update/:jobId", IaiController.update);

module.exports = Route;
