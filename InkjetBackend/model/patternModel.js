const { DataTypes } = require("sequelize");
const sequelize = require("../database");

// สำเนาค่าที่ใช้พิมพ์ ณ เวลาที่ register — 1 job มี 1 pattern เป็นของตัวเอง
// barcode ซ้ำได้ (lot เดิม register ใหม่ = pattern ใหม่ที่มีข้อมูลล่าสุด)
const Pattern = sequelize.define(
  "patterns",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    job_id: {
      type: DataTypes.INTEGER,
      allowNull: true,
      references: { model: "print_jobs", key: "id" },
      onDelete: "CASCADE",
    },
    barcode: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    description: {
      type: DataTypes.STRING,
    },
    is_active: {
      type: DataTypes.BOOLEAN,
      allowNull: false,
      defaultValue: true,
    },
  },
  {
    timestamps: false,
    indexes: [{ fields: ["barcode"] }, { fields: ["job_id"] }],
  }
);

const InkjetConfig = sequelize.define(
  "inkjet_configs",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    pattern_id: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    ordinal: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    program_number: {
      type: DataTypes.INTEGER,
    },
    program_name: {
      type: DataTypes.STRING,
    },
    width: {
      type: DataTypes.INTEGER,
    },
    height: {
      type: DataTypes.INTEGER,
    },
    trigger_delay: {
      type: DataTypes.INTEGER,
    },
    pos_act: {
      type: DataTypes.INTEGER,
    },
    delay: {
      type: DataTypes.INTEGER,
    },
    direction: {
      type: DataTypes.INTEGER,
    },
    steel_type: {
      type: DataTypes.STRING,
    },
    suspended: {
      type: DataTypes.BOOLEAN,
      allowNull: false,
      defaultValue: false,
    },
  },
  { timestamps: false }
);

const TextBlock = sequelize.define(
  "text_blocks",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    inkjet_config_id: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    block_number: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    text: {
      type: DataTypes.STRING,
    },
    x: {
      type: DataTypes.INTEGER,
    },
    y: {
      type: DataTypes.INTEGER,
    },
    size: {
      type: DataTypes.INTEGER,
    },
    scale: {
      type: DataTypes.INTEGER,
    },
  },
  { timestamps: false }
);

const ConveyorSpeed = sequelize.define(
  "conveyor_speeds",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    pattern_id: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    speed1: {
      type: DataTypes.INTEGER,
    },
    speed2: {
      type: DataTypes.INTEGER,
    },
    speed3: {
      type: DataTypes.INTEGER,
    },
  },
  { timestamps: false }
);

const ServoConfig = sequelize.define(
  "servo_configs",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    pattern_id: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    ordinal: {
      type: DataTypes.INTEGER,
      allowNull: false,
    },
    position: {
      type: DataTypes.INTEGER,
    },
    post_act: {
      type: DataTypes.REAL,
    },
    delay: {
      type: DataTypes.REAL,
    },
    trigger: {
      type: DataTypes.INTEGER,
    },
  },
  { timestamps: false }
);

// Associations — ลบ pattern แล้ว config/block/speed/servo ต้องหายตามทั้งหมด
Pattern.hasMany(InkjetConfig, {
  foreignKey: "pattern_id",
  as: "inkjet_configs",
  onDelete: "CASCADE",
  hooks: true,
});
InkjetConfig.belongsTo(Pattern, { foreignKey: "pattern_id" });

InkjetConfig.hasMany(TextBlock, {
  foreignKey: "inkjet_config_id",
  as: "text_blocks",
  onDelete: "CASCADE",
  hooks: true,
});
TextBlock.belongsTo(InkjetConfig, { foreignKey: "inkjet_config_id" });

Pattern.hasOne(ConveyorSpeed, {
  foreignKey: "pattern_id",
  as: "conveyor_speeds",
  onDelete: "CASCADE",
  hooks: true,
});
ConveyorSpeed.belongsTo(Pattern, { foreignKey: "pattern_id" });

Pattern.hasMany(ServoConfig, {
  foreignKey: "pattern_id",
  as: "servo_configs",
  onDelete: "CASCADE",
  hooks: true,
});
ServoConfig.belongsTo(Pattern, { foreignKey: "pattern_id" });

module.exports = { Pattern, InkjetConfig, TextBlock, ConveyorSpeed, ServoConfig };
