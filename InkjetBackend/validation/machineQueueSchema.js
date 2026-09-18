const { z } = require("zod");

// ชื่อเครื่องตรงกับชื่อขั้นตอนใน marking method — ไม่มีเครื่องอื่นนอกจากสามตัวนี้
const machineName = z.enum(["MK", "UV1", "UV2"]);

const enqueueItemSchema = z.object({
  machine: machineName,
  round: z.number().int().min(1).max(9).optional(),
  program_name: z.string().nullable().optional(),
});

const enqueueSchema = z.object({
  print_jobs_id: z.number().int().min(1),
  items: z.array(enqueueItemSchema).min(1),
});

const machineSchema = z.object({
  machine: machineName,
});

const updateQueueSchema = z.object({
  state: z.enum(["pending", "active", "done"]).optional(),
  program_name: z.string().nullable().optional(),
});

module.exports = { enqueueSchema, machineSchema, updateQueueSchema };
