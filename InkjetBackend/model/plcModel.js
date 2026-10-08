const { DataTypes } = require("sequelize");
const sequelize = require("../database");

// Register-map rows for the PLC settings page. The whole table is replaced on
// each bulkSave, so rows carry an explicit sort_order for display ordering.
const PlcRegisterMap = sequelize.define(
  "PlcRegisterMap",
  {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    address_start: { type: DataTypes.INTEGER, allowNull: false, defaultValue: 0 },
    address_stop: { type: DataTypes.INTEGER, allowNull: false, defaultValue: 0 },
    plc_start: { type: DataTypes.STRING, allowNull: false, defaultValue: "" },
    plc_stop: { type: DataTypes.STRING, allowNull: false, defaultValue: "" },
    list_name: { type: DataTypes.STRING, allowNull: false },
    data_type: { type: DataTypes.STRING, allowNull: false, defaultValue: "Int" },
    bit: { type: DataTypes.INTEGER, allowNull: false, defaultValue: 32 },
    sort_order: { type: DataTypes.INTEGER, allowNull: false, defaultValue: 0 },
  },
  {
    tableName: "plc_register_maps",
    timestamps: true,
    createdAt: "created_at",
    updatedAt: "updated_at",
  }
);

module.exports = { PlcRegisterMap };
