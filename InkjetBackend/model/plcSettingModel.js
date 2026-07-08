const { DataTypes } = require("sequelize");
const sequelize = require("../database");

const PlcRegisterMap = sequelize.define(
  "plc_register_map",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    address_start: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    address_stop: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    plc_start: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    plc_stop: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    list_name: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    data_type: {
      type: DataTypes.STRING,
      allowNull: false,
      defaultValue: "Int",
    },
    bit: {
      type: DataTypes.INTEGER,
      allowNull: false,
      defaultValue: 32,
    },
    sort_order: {
      type: DataTypes.INTEGER,
      allowNull: false,
      defaultValue: 0,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

module.exports = { PlcRegisterMap };
