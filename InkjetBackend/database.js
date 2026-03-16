require("dotenv").config();
const { Sequelize } = require("sequelize");

const sequelize = new Sequelize(process.env.POSTGRESQL_HOST, {
  dialect: "postgres",
  omitNull: true,
  logging: false,
});

module.exports = sequelize;
