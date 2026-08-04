const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

// UV job detail captured at register time (queried from print_data).
// 2 rows per job: UV1 (Plate/MK063) + UV2 (Shim/MK067).
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
      allowNull: true,
      references: {
        model: "print_jobs",
        key: "id",
      },
    },
    machine: {
      type: DataTypes.STRING, // "UV1" / "UV2"
      allowNull: true,
    },
    table_name: {
      type: DataTypes.STRING, // "MK063" / "MK067"
      allowNull: true,
    },
    program_name: {
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
    text1: { type: DataTypes.STRING, allowNull: true },
    text2: { type: DataTypes.STRING, allowNull: true },
    text3: { type: DataTypes.STRING, allowNull: true },
    text4: { type: DataTypes.STRING, allowNull: true },
    text5: { type: DataTypes.STRING, allowNull: true },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

// ลบ job แล้ว UV detail ต้องหายตาม
PrintJob.hasMany(UvJobData, {
  foreignKey: "print_jobs_id",
  as: "uv_job_data",
  onDelete: "CASCADE",
  hooks: true,
});
UvJobData.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { UvJobData };
