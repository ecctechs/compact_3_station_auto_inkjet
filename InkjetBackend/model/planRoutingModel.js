const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

// Plan routing captured at register time (queried from source DB plan_routing).
// 1 row per job — เก็บค่าดิบไว้ ไม่ตีความ (marking_method เป็น NULL ได้)
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
    },
    lot_no: {
      type: DataTypes.STRING,
    },
    erp_mfg: {
      type: DataTypes.STRING,
    },
    marking_method: {
      type: DataTypes.STRING,
    },
    process_sequence: {
      type: DataTypes.STRING,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

PrintJob.hasOne(PlanRouting, {
  foreignKey: "print_jobs_id",
  as: "plan_routing",
  onDelete: "CASCADE",
  hooks: true,
});
PlanRouting.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { PlanRouting };
