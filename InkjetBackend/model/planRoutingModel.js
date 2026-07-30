const { DataTypes } = require("sequelize");
const sequelize = require("../database");

// Plan routing captured at register time (queried from source DB plan_routing).
// 1 row per job — เก็บ marking_method ไว้ตัดสินใจส่งงานให้ถูกเครื่อง (UV / Inkjet).
const PlanRouting = sequelize.define(
  "plan_routing",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    print_jobs_id: {
      type: DataTypes.INTEGER,
      allowNull: true,
      references: {
        model: "print_jobs",
        key: "id",
      },
    },
    lot_no: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    erp_mfg: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    marking_method: {
      type: DataTypes.STRING, // เลข 2 หลัก [Shim][Plate] เช่น "12"
      allowNull: true,
    },
    process_sequence: {
      type: DataTypes.STRING,
      allowNull: true,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

module.exports = { PlanRouting };
