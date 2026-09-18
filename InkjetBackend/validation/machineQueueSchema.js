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

// ระบุงานได้ = หยิบเฉพาะแถวของงานใบนั้น ไม่ระบุ = หยิบใบที่รอมาก่อนสุด
const claimSchema = z.object({
  machine: machineName,
  print_jobs_id: z.number().int().min(1).optional(),
});

const updateQueueSchema = z.object({
  state: z.enum(["pending", "active", "done"]).optional(),
  program_name: z.string().nullable().optional(),
  sent: z.boolean().optional(),
});

module.exports = { enqueueSchema, machineSchema, claimSchema, updateQueueSchema };
