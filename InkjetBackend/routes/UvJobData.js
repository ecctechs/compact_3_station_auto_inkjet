const express = require("express");
const Route = express.Router();
const RouteName = "/uv-job";
const UvJobDataController = require("../controllers/UvJobDataController");

Route.post(RouteName + "/create", UvJobDataController.create);

Route.get(RouteName + "/getByJob/:jobId", UvJobDataController.getByJob);

Route.delete(RouteName + "/deleteByJob/:jobId", UvJobDataController.deleteByJob);

module.exports = Route;
