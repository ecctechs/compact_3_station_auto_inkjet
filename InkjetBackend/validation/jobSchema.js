const { z } = require("zod");

const createJobSchema = z.object({
  barcode_raw: z.string().min(1),
  created_by: z.enum(["scanner", "operator"]).default("scanner"),
  order_no: z.string().optional(),
  customer_name: z.string().optional(),
  type: z.string().optional(),
  qty: z.number().int().optional(),
});

const jobFilterSchema = z.object({
  status: z.enum(["pending", "executing", "completed", "failed"]).optional(),
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(20),
});

const commandResultSchema = z.object({
  ordinal: z.number().int().optional(),
  command: z.string().min(1),
  payload: z.record(z.unknown()).optional(),
  response: z.string().optional(),
  success: z.boolean(),
  sent_at: z.string().optional(),
});

const jobResultsSchema = z.object({
  success: z.boolean(),
  error_message: z.string().optional(),
  commands: z.array(commandResultSchema),
});

module.exports = {
  createJobSchema,
  jobFilterSchema,
  jobResultsSchema,
};
