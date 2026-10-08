const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

// ระยะแคลมป์ (IAI) ที่งานหนึ่งใช้ — 1 job = 1 แถว
//
// รูปแบบเดียวกับ MainTable ใน mydatabase.db3 คือเก็บทั้งฝั่ง Plate และ Shim ไว้แถวเดียว
//   m1_program_name / iaip   → งาน Plate (ชื่อขึ้นต้น "P-")
//   m2_program_name / iai    → งาน Shim
//
// เป็น snapshot ต่อ job ไม่ใช่ master ต่อโปรแกรม — lot ใหม่ที่ใช้โปรแกรมเดิม
// จะได้แถวใหม่ของตัวเอง ทำให้ตรวจย้อนหลังได้ว่างานนั้นใช้ระยะเท่าไร
//
// หาค่าไม่เจอก็ยังสร้างแถว เก็บเป็น null — จะได้รู้ว่า "เคยหาแล้วไม่มี"
// ต่างจาก "ยังไม่เคยหา" ซึ่งคือไม่มีแถวเลย
const IaiClampSetting = sequelize.define(
  "iai_clamp_setting",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    print_jobs_id: {
      type: DataTypes.INTEGER,
      references: { model: "print_jobs", key: "id" },
      onDelete: "CASCADE",
    },
    m2_program_name: {
      type: DataTypes.STRING,
    },
    iai: {
      type: DataTypes.INTEGER,
    },
    iai_z1: {
      type: DataTypes.INTEGER,
    },
    iai_z2: {
      type: DataTypes.INTEGER,
    },
    status: {
      type: DataTypes.BOOLEAN,
      allowNull: false,
      defaultValue: true,
    },
    m1_program_name: {
      type: DataTypes.STRING,
    },
    iaip: {
      type: DataTypes.INTEGER,
    },
    iaip_z1: {
      type: DataTypes.INTEGER,
    },
    iaip_z2: {
      type: DataTypes.INTEGER,
    },
  },
  {
    tableName: "iai_clamp_settings",
    timestamps: true,
    createdAt: "created_at",
    updatedAt: "updated_at",
    // index ของ print_jobs_id สร้างใน migrations/ensureIaiPerJob.js
    // ประกาศไว้ตรงนี้ไม่ได้ เพราะ sync() จะพยายามสร้าง index ก่อนที่ migration
    // จะเพิ่มคอลัมน์ให้ตารางเดิม แล้วล้มทั้ง sync
  }
);

// ลบ job แล้วค่าแคลมป์ของงานนั้นต้องหายตาม
PrintJob.hasOne(IaiClampSetting, {
  foreignKey: "print_jobs_id",
  as: "iai_clamp_setting",
  onDelete: "CASCADE",
  hooks: true,
});
IaiClampSetting.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { IaiClampSetting };
