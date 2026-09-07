const sequelize = require("../database");

// print_jobs ต้องมี job_no / job_date — เลขงานประจำวันที่เริ่มใหม่ที่ 1 ทุกเที่ยงคืนไทย
// sequelize.sync() ไม่เพิ่มคอลัมน์ให้ตารางที่มีอยู่แล้ว จึงต้องเติมเอง — รันซ้ำได้ ไม่พัง
module.exports = async function ensureJobNumberColumns() {
  try {
    const [cols] = await sequelize.query(`
      SELECT column_name FROM information_schema.columns
      WHERE table_schema = 'public' AND table_name = 'print_jobs'`);

    if (cols.length === 0) return; // ยังไม่มีตาราง — sync() จะสร้างให้ครบเอง

    const has = (c) => cols.some((x) => x.column_name === c);

    if (!has("job_no")) {
      await sequelize.query(`ALTER TABLE print_jobs ADD COLUMN job_no INTEGER`);
      console.log("print_jobs.job_no added");
    }

    if (!has("job_date")) {
      await sequelize.query(`ALTER TABLE print_jobs ADD COLUMN job_date DATE`);
      console.log("print_jobs.job_date added");
    }

    // ไล่เลขย้อนหลังให้งานเก่าที่ยังไม่มีเลข เพื่อให้ประวัติอ่านได้เหมือนกันทั้งตาราง
    // แบ่งวันตามเวลาไทยและเรียงตามเวลารับงาน ผลจึงตรงกับที่ระบบจะให้ถ้าเคยมีคอลัมน์นี้มาแต่แรก
    //
    // แตะเฉพาะแถวที่ job_no ยังว่าง รันซ้ำอีกกี่รอบก็ไม่เปลี่ยนเลขที่ลงไปแล้ว
    const [, meta] = await sequelize.query(`
      WITH numbered AS (
        SELECT id,
               (created_at AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Bangkok')::date AS d,
               ROW_NUMBER() OVER (
                 PARTITION BY (created_at AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Bangkok')::date
                 ORDER BY created_at, id
               ) AS n
          FROM print_jobs
         WHERE job_no IS NULL
      )
      UPDATE print_jobs p
         SET job_no = numbered.n,
             job_date = numbered.d
        FROM numbered
       WHERE p.id = numbered.id`);

    if (meta && meta.rowCount > 0) {
      console.log(`print_jobs.job_no backfilled (${meta.rowCount} rows)`);
    }
  } catch (err) {
    console.error("ensureJobNumberColumns failed:", err.message);
  }
};
