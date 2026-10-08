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

// ปล่อยเครื่อง — บอกได้ว่าให้ถือเครื่องไว้ให้รอบถัดไปของงานเดิมไหม
const releaseSchema = z.object({
  machine: machineName,
  expected_holder_id: z.number().int().positive().nullable(),
  hold_for_next_round: z.boolean().optional(),
});

// ระบุงานได้ แต่ต้องเป็นหัวคิวของเครื่องนั้นด้วย ห้ามข้ามใบที่รอก่อน
const claimSchema = z.object({
  machine: machineName,
  print_jobs_id: z.number().int().min(1).optional(),
});

const updateQueueSchema = z.object({
  state: z.enum(["pending", "active", "done"]).optional(),
  program_name: z.string().nullable().optional(),
  sent: z.boolean().optional(),
});

const beginSendSchema = z.object({ token: z.string().uuid() });
const finishSendSchema = z.object({
  token: z.string().uuid(),
  outcome: z.enum(["sent", "not_sent", "unknown"]),
  detail: z.record(z.unknown()).nullable().optional(),
  error: z.string().max(4000).optional(),
});

module.exports = {
  enqueueSchema,
  machineSchema,
  releaseSchema,
  claimSchema,
  updateQueueSchema,
  beginSendSchema,
  finishSendSchema,
};
