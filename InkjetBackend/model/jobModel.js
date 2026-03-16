const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { Pattern } = require("./patternModel");

const PrintJob = sequelize.define(
  "print_jobs",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    barcode_raw: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    pattern_id: {
      type: DataTypes.INTEGER,
    },
    lot_number: {
      type: DataTypes.STRING,
    },
    status: {
      type: DataTypes.STRING,
      allowNull: false,
      defaultValue: "pending",
    },
    error_message: {
      type: DataTypes.TEXT,
    },
    created_by: {
      type: DataTypes.STRING,
    },
    attempt: {
      type: DataTypes.INTEGER,
      allowNull: false,
      defaultValue: 0,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

const PrintJobCommand = sequelize.define(
  "print_job_commands",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    job_id: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    ordinal: {
      type: DataTypes.INTEGER,
    },
    command: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    payload: {
      type: DataTypes.JSONB,
    },
    response: {
      type: DataTypes.TEXT,
    },
    success: {
      type: DataTypes.BOOLEAN,
    },
    sent_at: {
      type: DataTypes.DATE,
    },
  },
  { timestamps: false }
);

// Associations
PrintJob.hasMany(PrintJobCommand, { foreignKey: "job_id", as: "commands" });
PrintJobCommand.belongsTo(PrintJob, { foreignKey: "job_id" });

PrintJob.belongsTo(Pattern, { foreignKey: "pattern_id", as: "pattern" });

module.exports = { PrintJob, PrintJobCommand };
