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
      type: DataTypes.TEXT,
      allowNull: false,
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
    lot_number: {
      type: DataTypes.STRING,
    },
    pattern_no_erp: {
      type: DataTypes.STRING,
    },
    status: {
      type: DataTypes.STRING,
      allowNull: false,
      defaultValue: "Waiting",
    },
    error_message: {
      type: DataTypes.TEXT,
    },
    warning: {
      type: DataTypes.TEXT,
    },
    attempt: {
      type: DataTypes.INTEGER,
      allowNull: false,
      defaultValue: 0,
    },
    created_by: {
      type: DataTypes.STRING,
    },
    st_status: {
      type: DataTypes.STRING(255),
    },
    stations_required: {
      type: DataTypes.JSONB,
      allowNull: false,
      defaultValue: [1, 2, 3, 4],
    },
    st1_confirmation: {
      type: DataTypes.STRING,
    },
    st1_send_time: {
      type: DataTypes.DATE,
    },
    // ST3 กดเริ่มงานแล้วฝากให้ ST1 เป็นคนส่งคำสั่งเข้าเครื่องแทน
    // (เครื่อง MK/UV ต่ออยู่กับ PC ของ ST1 ที่เดียว)
    // "0" = ไม่มีคำขอค้าง · "1" = รอ ST1 หยิบไปส่ง
    remote_start: {
      type: DataTypes.STRING,
      defaultValue: "0",
    },
    // โปรแกรม UV ที่ ST3 เลือกไว้ให้เสร็จแล้ว — ST1 จะได้ส่งโดยไม่ต้องถามใครที่จอตัวเอง
    remote_program: {
      type: DataTypes.STRING,
    },
    // สาเหตุที่ ST1 ส่งให้ไม่สำเร็จ — ST3 อ่านไปแสดงที่จอตัวเองแล้วล้างทิ้ง
    // แยกจาก error_message ที่เป็นของ flow postResults/retry คนละเรื่องกัน
    remote_error: {
      type: DataTypes.TEXT,
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
PrintJob.hasMany(PrintJobCommand, {
  foreignKey: "job_id",
  as: "commands",
  onDelete: "CASCADE",
  hooks: true,
});
PrintJobCommand.belongsTo(PrintJob, { foreignKey: "job_id" });

PrintJob.hasOne(Pattern, {
  foreignKey: "job_id",
  as: "pattern",
  onDelete: "CASCADE",
  hooks: true,
});
Pattern.belongsTo(PrintJob, { foreignKey: "job_id" });

module.exports = { PrintJob, PrintJobCommand };
