const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { IAI_MIN_MM, IAI_MAX_MM } = require("../utils/iaiProgramFields");

/**
 * รูปคอลัมน์ยึดตาม MainTable ในไดอะแกรม (9 คอลัมน์ เรียงลำดับเดิม)
 *
 * ชื่อคอลัมน์ใช้ snake_case เพราะ Sequelize quote identifier เสมอ
 * ถ้าตั้งเป็น "IAI Z1" ตรงๆ คอลัมน์จะต้อง quote ตลอดชีวิต ป้ายเดิมเก็บไว้ที่ IAI_COLUMN_LABELS
 *
 * allowNull: true จำเป็น ไม่ใช่ทางเลือก — INSERT OR IGNORE ของเดิมสร้างแถวโดยยังไม่มีค่า IAI
 */
const mm = (comment) => ({
  type: DataTypes.INTEGER,
  allowNull: true,
  validate: { min: IAI_MIN_MM, max: IAI_MAX_MM },
  comment,
});

const IaiClampSetting = sequelize.define(
  "iai_clamp_setting",
  {
    id: { type: DataTypes.INTEGER, autoIncrement: true, primaryKey: true },

    // ---- ชุด S- ----
    m2_program_name: { type: DataTypes.STRING, allowNull: true, unique: true },
    iai: mm("IAI"),
    iai_z1: mm("IAI Z1"),
    iai_z2: mm("IAI Z2"),

    status: { type: DataTypes.BOOLEAN, allowNull: false, defaultValue: true },

    // ---- ชุด P- ----
    m1_program_name: { type: DataTypes.STRING, allowNull: true, unique: true },
    iaip: mm("IAIP"),
    iaip_z1: mm("IAIP Z1"),
    iaip_z2: mm("IAIP Z2"),
  },
  {
    timestamps: true,
    createdAt: "created_at",
    updatedAt: "updated_at",
    validate: {
      // ต้องผูกกับโปรแกรมอย่างน้อยหนึ่งฝั่ง (ไม่บังคับ XOR เผื่อ import ข้อมูลที่มีทั้งสองฝั่ง)
      atLeastOneProgramName() {
        if (!this.m2_program_name && !this.m1_program_name) {
          throw new Error("Either m2_program_name or m1_program_name is required");
        }
      },
    },
  }
);

// ป้ายหัวคอลัมน์ตามไดอะแกรม -> ชื่อคอลัมน์จริง (ที่เดียวสำหรับ sync SQLite / UI header ในอนาคต)
const IAI_COLUMN_LABELS = {
  m2_program_name: "m2_program_name",
  iai: "IAI",
  iai_z1: "IAI Z1",
  iai_z2: "IAI Z2",
  status: "status",
  m1_program_name: "m1_program_name",
  iaip: "IAIP",
  iaip_z1: "IAIP Z1",
  iaip_z2: "IAIP Z2",
};

module.exports = { IaiClampSetting, IAI_COLUMN_LABELS };
