const { z } = require("zod");

// Minimal validation - allow flexible values

const textBlockSchema = z.object({
  block_number: z.number().optional(),
  text: z.any().optional(),
  x: z.any().optional(),
  y: z.any().optional(),
  size: z.any().optional(),
  scale: z.any().optional(),
});

const inkjetConfigSchema = z.object({
  ordinal: z.number().optional(),
  program_number: z.any().nullable().optional(),
  program_name: z.any().optional(),
  width: z.any().optional(),
  height: z.any().optional(),
  trigger_delay: z.any().optional(),
  direction: z.any().optional(),
  steel_type: z.any().nullable().optional(),
  suspended: z.boolean().optional(),
  text_blocks: z.array(textBlockSchema).optional(),
});

const conveyorSpeedSchema = z.object({
  speed1: z.any().optional(),
  speed2: z.any().optional(),
  speed3: z.any().optional(),
});

const servoConfigSchema = z.object({
  ordinal: z.number().optional(),
  position: z.any().optional(),
  post_act: z.any().optional(),
  delay: z.any().optional(),
  trigger: z.any().optional(),
});

const createPatternSchema = z.object({
  barcode: z.string().optional(),
  description: z.any().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.nullable().optional(),
  servo_configs: z.array(servoConfigSchema).optional(),
});

const updatePatternSchema = z.object({
  barcode: z.string().optional(),
  description: z.any().optional(),
  is_active: z.boolean().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.nullable().optional(),
  servo_configs: z.array(servoConfigSchema).optional(),
});

const patternFilterSchema = z.object({
  barcode: z.string().optional(),
  is_active: z.string().optional(),
  page: z.coerce.number().optional(),
  limit: z.coerce.number().optional(),
});

module.exports = {
  createPatternSchema,
  updatePatternSchema,
  patternFilterSchema,
};
