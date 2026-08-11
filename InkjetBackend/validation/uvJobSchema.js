const { z } = require("zod");

const uvJobItemSchema = z.object({
  machine: z.string().nullable().optional(),
  table_name: z.string().nullable().optional(),
  program_name: z.string().nullable().optional(),
  lot: z.string().nullable().optional(),
  erp_mfg: z.string().nullable().optional(),
  qty: z.number().int().nullable().optional(),
  text1: z.string().nullable().optional(),
  text2: z.string().nullable().optional(),
  text3: z.string().nullable().optional(),
  text4: z.string().nullable().optional(),
  text5: z.string().nullable().optional(),
});

const createUvJobSchema = z.object({
  print_jobs_id: z.number().int().min(1),
  items: z.array(uvJobItemSchema).min(1),
});

module.exports = { createUvJobSchema };
