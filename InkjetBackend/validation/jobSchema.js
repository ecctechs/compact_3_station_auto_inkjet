const { z } = require("zod");

// ทุกช่องที่ไม่บังคับต้องรับ null ด้วย ไม่ใช่แค่ optional
//
// .optional() เฉย ๆ แปลว่า "ไม่ส่ง key มาก็ได้" แต่ถ้าส่งมาต้องเป็น string —
// ส่ง null มาจะโดนตีกลับ 400 ทันที ซึ่งเป็นสิ่งที่เกิดขึ้นตอนหน้า Scan Barcode
// เปลี่ยนไปดึง customer_name จาก inkjet_data แล้ว lot นั้นไม่มีชื่อลูกค้า
//
// คอลัมน์พวกนี้ใน print_jobs เป็น NULL ได้อยู่แล้ว และ schema อื่นในโปรเจกต์
// (plan_routing, uv_job_data) ก็ใช้ .nullable().optional() มาตั้งแต่แรก
const createJobSchema = z.object({
  barcode_raw: z.string().min(1),
  created_by: z.enum(["scanner", "operator"]).default("scanner"),
  order_no: z.string().nullable().optional(),
  customer_name: z.string().nullable().optional(),
  type: z.string().nullable().optional(),
  qty: z.number().int().nullable().optional(),
  st_status: z.string().nullable().optional(),
});

const jobFilterSchema = z.object({
  status: z
    .enum(["Waiting", "executing", "completed", "failed"])
    .optional(),
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
