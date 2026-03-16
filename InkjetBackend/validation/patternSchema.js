const { z } = require("zod");

// Matches validation from csv_extractor.py lines 200-253, 297-346
const textBlockSchema = z.object({
  block_number: z.number().int().min(1).max(5),
  text: z.string().optional(),
  x: z.number().int().min(0).max(4095).optional(),
  y: z.number().int().min(0).max(31).optional(),
  size: z.number().int().min(0).max(22).optional(),
  scale: z.number().int().min(1).max(10).optional(),
});

const inkjetConfigSchema = z.object({
  ordinal: z.number().int().min(1),
  program_number: z.number().int().min(1).max(500).nullable().optional(),
  program_name: z.string().optional(),
  width: z.number().int().min(10).max(500).optional(),
  height: z.number().int().min(50).max(200).optional(),
  trigger_delay: z.number().int().min(10).max(99999).optional(), // stored as x10 int
  direction: z
    .number()
    .int()
    .refine((d) => d === 0 || d === 3, { message: "Direction must be 0 or 3" })
    .optional(),
  steel_type: z.string().optional(),
  suspended: z.boolean().default(false),
  text_blocks: z.array(textBlockSchema).max(5).optional(),
});

const conveyorSpeedSchema = z.object({
  speed1: z.number().int().optional(),
  speed2: z.number().int().optional(),
  speed3: z.number().int().optional(),
});

const servoConfigSchema = z.object({
  ordinal: z.number().int().min(1),
  position: z.number().int().optional(),
  post_act: z.number().int().optional(),
  delay: z.number().int().optional(),
  trigger: z.number().int().optional(),
});

const createPatternSchema = z.object({
  barcode: z.string().min(1),
  description: z.string().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.optional(),
  servo_configs: z.array(servoConfigSchema).optional(),
});

const updatePatternSchema = z.object({
  barcode: z.string().min(1).optional(),
  description: z.string().optional(),
  is_active: z.boolean().optional(),
  inkjet_configs: z.array(inkjetConfigSchema).optional(),
  conveyor_speeds: conveyorSpeedSchema.optional(),
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
