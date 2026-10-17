// ชุดข้อมูลงานสำหรับทดสอบ — ล้าง / ส่งออก / นำเข้า ข้อมูลงานทั้งหมด (เครื่องมือโหมด Dev)
//
// "ข้อมูลงาน" คือทุกตารางที่ผูกกับใบงาน ไม่รวมค่าตั้ง (plc_register_maps) ที่เป็นของเครื่อง
// ไม่ใช่ของงาน — ล้างแล้วตาราง register map ยังอยู่ครบ ไม่ต้องตั้งใหม่
//
// ส่งออกเฉพาะ "ตัวงาน" ไม่เอาร่องรอยการเดินงาน (คิว ประวัติคำสั่ง สถานะเครื่อง) — นำเข้าแล้ว
// ทุกใบเป็นรอเริ่มเหมือนเพิ่งลงทะเบียน ถ้าเอาคิวที่กำลังเดินติดไปด้วย เครื่องปลายทางจะได้คิว
// ค้างที่ไม่มีเครื่องจริงรองรับ
const sequelize = require("../database");
const { QueryTypes } = require("sequelize");

// SELECT ทุกตัวระบุ type ไว้ — ไม่งั้นรูปแบบผลที่ได้ใน transaction ไม่เหมือนนอก transaction
const select = (sql, options) => sequelize.query(sql, { ...options, type: QueryTypes.SELECT });

const FORMAT = "compact-inkjet-job-dataset";
const VERSION = 1;

// ตัวงาน — เรียงตามลำดับที่ต้องใส่กลับ (แม่ก่อนลูก) ให้ foreign key ผ่าน
const JOB_TABLES = [
  "print_jobs",
  "patterns",
  "inkjet_configs",
  "text_blocks",
  "conveyor_speeds",
  "servo_configs",
  "plan_routings",
  "uv_job_data",
  "iai_clamp_settings",
];

// ร่องรอยการเดินงาน — ล้างทิ้งทุกครั้ง ไม่ส่งออก
const RUNTIME_TABLES = ["machine_queues", "print_job_commands", "last_sent_jobs", "machine_status"];

async function existingTables(names, transaction) {
  // อ่านจาก pg_tables ไม่ใช่ information_schema.tables — Sequelize จับคำสั่งที่อ่าน
  // information_schema.tables แล้วแปลงผลเป็นรูปอื่นเอง (ถือว่าเป็นคำสั่ง "show tables")
  const rows = await select(
    `SELECT tablename AS name FROM pg_tables
      WHERE schemaname = 'public' AND tablename IN (:names)`,
    { replacements: { names }, transaction });
  const found = new Set(rows.map(r => r.name));
  return names.filter(n => found.has(n));
}

async function countRows(tables, transaction) {
  const counts = {};
  for (const t of tables) {
    const [row] = await select(`SELECT COUNT(*)::int AS n FROM "${t}"`, { transaction });
    counts[t] = row.n;
  }
  return counts;
}

// ล้างทุกตารางของงาน และเริ่มนับ id ใหม่ที่ 1
async function truncateAll(transaction) {
  const tables = await existingTables([...JOB_TABLES, ...RUNTIME_TABLES], transaction);
  if (tables.length === 0) return;
  await sequelize.query(
    `TRUNCATE ${tables.map(t => `"${t}"`).join(", ")} RESTART IDENTITY CASCADE`, { transaction });
}

async function clearAll() {
  return sequelize.transaction(async (t) => {
    const tables = await existingTables([...JOB_TABLES, ...RUNTIME_TABLES], t);
    const removed = await countRows(tables, t);
    await truncateAll(t);
    return { removed };
  });
}

async function exportAll() {
  return sequelize.transaction(async (t) => {
    const tables = {};
    for (const name of await existingTables(JOB_TABLES, t)) {
      const rows = await select(`SELECT * FROM "${name}" ORDER BY id`, { transaction: t });
      tables[name] = rows;
    }
    return { format: FORMAT, version: VERSION, exported_at: new Date().toISOString(), tables };
  });
}

function badRequest(message) {
  const error = new Error(message);
  error.statusCode = 400;
  return error;
}

// แทนที่ข้อมูลงานทั้งหมดด้วยชุดในไฟล์ — ทำในทรานแซกชันเดียว พังกลางทางข้อมูลเดิมยังอยู่ครบ
async function importAll(dataset) {
  if (!dataset || dataset.format !== FORMAT || typeof dataset.tables !== "object")
    throw badRequest("ไฟล์นี้ไม่ใช่ชุดข้อมูลงานของ Compact Inkjet");
  if (dataset.version > VERSION)
    throw badRequest(`ไฟล์ชุดข้อมูลรุ่นใหม่กว่าโปรแกรมนี้ (รุ่น ${dataset.version}) — อัปเดต backend ก่อน`);

  return sequelize.transaction(async (t) => {
    await truncateAll(t);

    const imported = {};
    for (const name of await existingTables(JOB_TABLES, t)) {
      const rows = Array.isArray(dataset.tables[name]) ? dataset.tables[name] : [];
      imported[name] = rows.length;
      if (rows.length === 0) continue;

      // ใส่เฉพาะคอลัมน์ที่มีทั้งในไฟล์และในตารางตอนนี้ — ไฟล์จาก backend รุ่นก่อน / รุ่นหลัง
      // ที่คอลัมน์ไม่ตรงกันทุกตัวยังนำเข้าได้ คอลัมน์ที่ไฟล์ไม่มีใช้ค่าตั้งต้นของตาราง
      const cols = await select(
        `SELECT attname AS col FROM pg_attribute
          WHERE attrelid = to_regclass(:table) AND attnum > 0 AND NOT attisdropped
          ORDER BY attnum`,
        { replacements: { table: `public.${name}` }, transaction: t });
      const keys = new Set(rows.flatMap(r => Object.keys(r)));
      const use = cols.map(c => c.col).filter(c => keys.has(c));
      const list = use.map(c => `"${c}"`).join(", ");

      // json_populate_recordset แปลงชนิดตามตารางให้เอง (วันที่ jsonb ตัวเลข) ไม่ต้องเดาเอง
      await sequelize.query(
        `INSERT INTO "${name}" (${list})
         SELECT ${list} FROM json_populate_recordset(NULL::"${name}", :data::json)`,
        { replacements: { data: JSON.stringify(rows) }, transaction: t });

      // id ถัดไปต่อจากเลขสูงสุดในไฟล์ ไม่งั้นงานที่ลงทะเบียนต่อจะชนเลขเดิม
      await sequelize.query(
        `SELECT setval(pg_get_serial_sequence('"${name}"', 'id'), COALESCE(MAX(id), 0) + 1, false) FROM "${name}"`,
        { transaction: t });
    }

    // นำเข้าแล้วทุกใบเป็นรอเริ่ม — ไม่มีคิวหรือคำขอค้างจากเครื่องที่ส่งออกมา
    await sequelize.query(
      `UPDATE print_jobs SET status = 'Waiting', remote_start = '0', remote_program = NULL,
         remote_step = NULL, remote_error = NULL`, { transaction: t });

    return { imported, exported_at: dataset.exported_at ?? null };
  });
}

module.exports = { FORMAT, VERSION, JOB_TABLES, RUNTIME_TABLES, clearAll, exportAll, importAll };
