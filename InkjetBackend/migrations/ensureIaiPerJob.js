const sequelize = require("../database");

// iai_clamp_settings เดิมออกแบบเป็น master ต่อโปรแกรม (m1/m2_program_name เป็น UNIQUE)
// เปลี่ยนเป็น snapshot ต่อ job → 1 job = 1 แถว โปรแกรมเดียวกันซ้ำได้
//
// sequelize.sync() ไม่ถอด UNIQUE ที่มีอยู่ และไม่เพิ่มคอลัมน์ให้ จึงต้องทำเอง
// รันซ้ำได้ ไม่พัง
module.exports = async function ensureIaiPerJob() {
  try {
    const [exists] = await sequelize.query(`
      SELECT 1 FROM information_schema.tables
      WHERE table_name = 'iai_clamp_settings'`);
    if (exists.length === 0) return; // ตารางยังไม่ถูกสร้าง — sync() จะสร้างให้ถูกอยู่แล้ว

    // 1) เพิ่มคอลัมน์ print_jobs_id ถ้ายังไม่มี
    const [cols] = await sequelize.query(`
      SELECT column_name FROM information_schema.columns
      WHERE table_name = 'iai_clamp_settings' AND column_name = 'print_jobs_id'`);

    if (cols.length === 0) {
      await sequelize.query(`
        ALTER TABLE iai_clamp_settings
          ADD COLUMN print_jobs_id INTEGER
          REFERENCES print_jobs (id) ON DELETE CASCADE ON UPDATE CASCADE`);
      console.log("iai_clamp_settings: added print_jobs_id");
    }

    // 2) ถอด UNIQUE ของชื่อโปรแกรม — งานคนละใบใช้โปรแกรมเดียวกันได้
    const [uniques] = await sequelize.query(`
      SELECT c.conname
      FROM pg_constraint c
      JOIN pg_class t ON t.oid = c.conrelid
      JOIN unnest(c.conkey) AS k(attnum) ON true
      JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = k.attnum
      WHERE c.contype = 'u'
        AND t.relname = 'iai_clamp_settings'
        AND a.attname IN ('m1_program_name', 'm2_program_name')`);

    for (const row of uniques) {
      await sequelize.query(
        `ALTER TABLE iai_clamp_settings DROP CONSTRAINT "${row.conname}"`
      );
      console.log(`iai_clamp_settings: dropped unique ${row.conname}`);
    }

    // 3) index บน print_jobs_id ไว้ค้นเร็ว
    await sequelize.query(`
      CREATE INDEX IF NOT EXISTS iai_clamp_settings_print_jobs_id
        ON iai_clamp_settings (print_jobs_id)`);
  } catch (err) {
    console.error("ensureIaiPerJob failed:", err.message);
  }
};
