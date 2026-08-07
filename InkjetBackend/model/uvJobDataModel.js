const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

const UvJobData = sequelize.define(
  "uv_job_data",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    print_jobs_id: {
      type: DataTypes.INTEGER,
    },
    machine: {
      type: DataTypes.STRING,
    },
    table_name: {
      type: DataTypes.STRING,
    },
    program_name: {
      type: DataTypes.STRING,
    },
    lot: {
      type: DataTypes.STRING,
    },
    name: {
      type: DataTypes.STRING,
    },
    text1: {
      type: DataTypes.STRING,
    },
    text2: {
      type: DataTypes.STRING,
    },
    text3: {
      type: DataTypes.STRING,
    },
    text4: {
      type: DataTypes.STRING,
    },
    text5: {
      type: DataTypes.STRING,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

PrintJob.hasMany(UvJobData, {
  foreignKey: "print_jobs_id",
  as: "uv_job_data",
  onDelete: "CASCADE",
  hooks: true,
});
UvJobData.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { UvJobData };
