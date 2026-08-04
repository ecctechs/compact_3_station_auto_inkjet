const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

const UVInkjet = sequelize.define(
  "uv_inkjet",
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
    inkjet_name: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    lot: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    name: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    program_name: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    status: {
      type: DataTypes.STRING,
      allowNull: true,
      defaultValue: "idle",
    },
    station: {
      type: DataTypes.STRING,
      allowNull: true,
    },
    update_at: {
      type: DataTypes.DATE,
      allowNull: true,
      defaultValue: DataTypes.NOW,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

// ลบ job แล้ว log การส่ง UV ต้องหายตาม
PrintJob.hasMany(UVInkjet, {
  foreignKey: "print_jobs_id",
  as: "uv_inkjets",
  onDelete: "CASCADE",
  hooks: true,
});
UVInkjet.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { UVInkjet };
