const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { Pattern } = require("./patternModel");

const LastSentJob = sequelize.define(
  "last_sent_jobs",
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
      defaultValue: "Waiting",
    },
    error_message: {
      type: DataTypes.TEXT,
    },
    created_by: {
      type: DataTypes.STRING,
    },
    attempt: {
      type: DataTypes.INTEGER,
      defaultValue: 0,
    },
    order_no: {
      type: DataTypes.STRING,
    },
    customer_name: {
      type: DataTypes.STRING,
    },
    type: {
      type: DataTypes.STRING,
    },
    qty: {
      type: DataTypes.INTEGER,
    },
    sent_time: {
      type: DataTypes.DATE,
    },
    st_status: {
      type: DataTypes.STRING,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

LastSentJob.belongsTo(Pattern, {
  foreignKey: "pattern_id",
  as: "pattern",
  onDelete: "SET NULL",
});

module.exports = { LastSentJob };
