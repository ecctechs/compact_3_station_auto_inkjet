const { z } = require("zod");
const { IAI_MIN_MM, IAI_MAX_MM } = require("../utils/iaiProgramFields");

/**
 * ค่า IAI เป็นตำแหน่ง actuator จริง เกินช่วงต้องได้ 400 พร้อมบอกฟิลด์ ไม่ใช่ 500 เปล่าๆ
 *
 * preprocess ต้องครอบ coerce: "" -> coerce.number() ได้ 0 ซึ่งแปลว่าแคลมป์ปิดสุด
 * ฟอร์มที่เว้นว่างไว้จึงต้องกลายเป็น undefined ก่อนถึง coerce
 */
const mmOptional = z.preprocess(
  (v) => (v === "" ? undefined : v),
  z.coerce.number().int().min(IAI_MIN_MM).max(IAI_MAX_MM).nullable().optional()
);

// รับได้ทั้ง 'True'/'False' แบบ SQLite เดิม และ boolean/0/1
const statusValue = z.preprocess((v) => {
  if (typeof v === "string") {
    const s = v.trim().toLowerCase();
    if (s === "true" || s === "1") return true;
    if (s === "false" || s === "0") return false;
  }
  if (v === 1) return true;
  if (v === 0) return false;
  return v;
}, z.boolean());

const programName = z.string().trim().min(1).max(255);

const upsertIaiSchema = z.object({
  program_name: programName,
  iai: mmOptional,
  iai_z1: mmOptional,
  iai_z2: mmOptional,
  status: statusValue.optional(),
});

const bulkUpsertIaiSchema = z.object({
  rows: z.array(upsertIaiSchema).min(1),
});

const updateIaiSchema = z.object({
  m2_program_name: programName.nullish(),
  iai: mmOptional,
  iai_z1: mmOptional,
  iai_z2: mmOptional,
  status: statusValue.optional(),
  m1_program_name: programName.nullish(),
  iaip: mmOptional,
  iaip_z1: mmOptional,
  iaip_z2: mmOptional,
});

const iaiFilterSchema = z.object({
  page: z.coerce.number().int().min(1).optional(),
  limit: z.coerce.number().int().min(1).max(500).optional(),
  q: z.string().trim().optional(),
});

module.exports = {
  upsertIaiSchema,
  bulkUpsertIaiSchema,
  updateIaiSchema,
  iaiFilterSchema,
};
