const { z } = require("zod");

const createJobSchema = z.object({
  barcode_raw: z.string().min(1),
  order_no: z.string().optional(),
  customer_name: z.string().optional(),
  type: z.string().optional(),
  qty: z.coerce.number().int().min(0).optional(),
  pattern_id: z.coerce.number().int().min(1).optional(),
  pattern_no_erp: z.string().optional(),
  lot_number: z.string().optional(),
  warning: z.string().optional(),
  created_by: z.string().optional(),
  st_status: z.string().optional(),
});

const jobFilterSchema = z.object({
  status: z.enum(["Waiting", "executing", "completed", "failed"]).optional(),
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
