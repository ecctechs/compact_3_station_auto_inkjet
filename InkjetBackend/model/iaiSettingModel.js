const { DataTypes } = require("sequelize");
const sequelize = require("../database");

const IaiClampSetting = sequelize.define(
  "iai_clamp_setting",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    m2_program_name: {
      type: DataTypes.STRING,
      unique: true,
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
      unique: true,
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
  }
);

module.exports = { IaiClampSetting };
