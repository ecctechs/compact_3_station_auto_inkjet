const { DataTypes } = require("sequelize");
const sequelize = require("../database");

const Pattern = sequelize.define(
  "patterns",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    barcode: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: true,
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
  { timestamps: false }
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
      type: DataTypes.INTEGER,
    },
    delay: {
      type: DataTypes.INTEGER,
    },
    trigger: {
      type: DataTypes.INTEGER,
    },
  },
  { timestamps: false }
);

// Associations
Pattern.hasMany(InkjetConfig, { foreignKey: "pattern_id", as: "inkjet_configs" });
InkjetConfig.belongsTo(Pattern, { foreignKey: "pattern_id" });

InkjetConfig.hasMany(TextBlock, { foreignKey: "inkjet_config_id", as: "text_blocks" });
TextBlock.belongsTo(InkjetConfig, { foreignKey: "inkjet_config_id" });

Pattern.hasOne(ConveyorSpeed, { foreignKey: "pattern_id", as: "conveyor_speeds" });
ConveyorSpeed.belongsTo(Pattern, { foreignKey: "pattern_id" });

Pattern.hasMany(ServoConfig, { foreignKey: "pattern_id", as: "servo_configs" });
ServoConfig.belongsTo(Pattern, { foreignKey: "pattern_id" });

module.exports = { Pattern, InkjetConfig, TextBlock, ConveyorSpeed, ServoConfig };
