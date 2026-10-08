const { z } = require("zod");

const createPlanRoutingSchema = z.object({
  print_jobs_id: z.number().int().min(1),
  lot_no: z.string().nullable().optional(),
  erp_mfg: z.string().nullable().optional(),
  marking_method: z.string().nullable().optional(),
  process_sequence: z.string().nullable().optional(),
});

module.exports = { createPlanRoutingSchema };
