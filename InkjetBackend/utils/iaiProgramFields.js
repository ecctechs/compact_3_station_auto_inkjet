/**
 * IAI = ตำแหน่งเป้าหมายของแคลมป์ (clamp) หน่วย mm ผูกกับชื่อโปรแกรม 1 ต่อ 1
 *
 * กฎ P-/S- อยู่ที่ไฟล์นี้ที่เดียว เพราะถูกใช้ทั้งใน upsert, lookup และ variants
 * ชื่อโปรแกรมขึ้นต้น "P-" -> ใช้ชุด m1_program_name / iaip
 * นอกนั้น (S-)          -> ใช้ชุด m2_program_name / iai
 */

const IAI_MIN_MM = 0;
const IAI_MAX_MM = 155;

const P_FIELDS = {
  kind: "P",
  programField: "m1_program_name",
  valueField: "iaip",
  z1Field: "iaip_z1",
  z2Field: "iaip_z2",
};

const S_FIELDS = {
  kind: "S",
  programField: "m2_program_name",
  valueField: "iai",
  z1Field: "iai_z1",
  z2Field: "iai_z2",
};

function resolveProgramFields(programName) {
  return String(programName || "").trim().toUpperCase().startsWith("P-")
    ? P_FIELDS
    : S_FIELDS;
}

function oppositeFields(f) {
  return f.kind === "P" ? S_FIELDS : P_FIELDS;
}

/**
 * ชื่อโปรแกรมอีกฝั่งของชิ้นงานเดียวกัน: S-XXX <-> P-XXX (ต่างกันแค่ตัวหน้า)
 * ยืนยันจากข้อมูลจริง 1462/1462 แถวที่มีทั้งสองฝั่ง ชื่อหลัง prefix ตรงกันหมด
 * คืน null ถ้าชื่อไม่ได้ขึ้นต้นด้วย P- หรือ S- (เดาคู่ไม่ได้)
 */
function siblingProgramName(programName) {
  const s = String(programName || "").trim();
  const head = s.slice(0, 2).toUpperCase();
  if (head === "P-") return "S-" + s.slice(2);
  if (head === "S-") return "P-" + s.slice(2);
  return null;
}

// %, _ และ \ เป็น wildcard ของ LIKE ต้อง escape ก่อนต่อท้ายด้วย "-%"
function escapeLike(value) {
  return String(value).replace(/[\\%_]/g, "\\$&");
}

// รูปแบบกลาง: ผู้เรียกได้ program_name + iai โดยไม่ต้องรู้ว่าค่าอยู่คอลัมน์ฝั่งไหน
function toNormalized(row, f) {
  return {
    id: row.id,
    program_name: row[f.programField],
    kind: f.kind,
    iai: row[f.valueField],
    iai_z1: row[f.z1Field],
    iai_z2: row[f.z2Field],
    status: row.status,
  };
}

module.exports = {
  IAI_MIN_MM,
  IAI_MAX_MM,
  resolveProgramFields,
  oppositeFields,
  siblingProgramName,
  escapeLike,
  toNormalized,
  P_FIELDS,
  S_FIELDS,
};
