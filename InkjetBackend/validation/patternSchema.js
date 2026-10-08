const { z } = require("zod");

const textBlockSchema = z.object({
  block_number: z.number().int().min(1).max(5),
  text: z.string().nullable().optional(),
  x: z.number().int().min(0).max(4095).nullable().optional(),
  y: z.number().int().min(0).max(31).nullable().optional(),
  size: z.number().int().min(0).max(22).nullable().optional(),
  scale: z.number().int().nullable().optional(),
});

const inkjetConfigSchema = z.object({
  ordinal: z.number().int().min(1),
  program_number: z.number().int().min(1).max(500).nullable().optional(),
  program_name: z.string().nullable().optional(),
  width: z.number().int().nullable().optional(),
  height: z.number().int().nullable().optional(),
  trigger_delay: z.number().int().nullable().optional(),
  pos_act: z.number().nullable().optional(),
  delay: z.number().nullable().optional(),
  direction: z.number().int().nullable().optional(),
  steel_type: z.string().nullable().optional(),
  suspended: z.boolean().default(false),
  text_blocks: z.array(textBlockSchema).max(5).optional(),
});

const conveyorSpeedSchema = z.object({
  speed1: z.number().int().nullable().optional(),
  speed2: z.number().int().nullable().optional(),
  speed3: z.number().int().nullable().optional(),
});

const servoConfigSchema = z.object({
  ordinal: z.number().int().min(1),
  position: z.number().nullable().optional(),
  post_act: z.number().nullable().optional(),
  delay: z.number().nullable().optional(),
  trigger: z.number().int().nullable().optional(),
});

const createPatternSchema = z.object({
  barcode: z.string().min(1),
  description: z.string().nullable().optional(),
  job_id: z.number().int().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.nullable().optional(),
  servo_configs: z.array(servoConfigSchema).optional(),
});

const updatePatternSchema = z.object({
  barcode: z.string().min(1).optional(),
  description: z.string().nullable().optional(),
  is_active: z.boolean().optional(),
  job_id: z.number().int().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.nullable().optional(),
  servo_configs: z.array(servoConfigSchema).optional(),
});

const patternFilterSchema = z.object({
  barcode: z.string().optional(),
  is_active: z
    .string()
    .transform((v) => v === "true")
    .optional(),
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(20),
});

module.exports = {
  createPatternSchema,
  updatePatternSchema,
  patternFilterSchema,
};
