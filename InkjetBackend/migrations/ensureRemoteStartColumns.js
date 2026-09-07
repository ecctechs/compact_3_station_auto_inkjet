const sequelize = require("../database");

// print_jobs ต้องมี remote_start / remote_program ไว้ให้ ST3 ฝากงานให้ ST1 ส่งแทน
// sequelize.sync() ไม่เพิ่มคอลัมน์ให้ตารางที่มีอยู่แล้ว จึงต้องเติมเอง — รันซ้ำได้ ไม่พัง
module.exports = async function ensureRemoteStartColumns() {
  try {
    const [cols] = await sequelize.query(`
      SELECT column_name FROM information_schema.columns
      WHERE table_schema = 'public' AND table_name = 'print_jobs'`);

    if (cols.length === 0) return; // ยังไม่มีตาราง — sync() จะสร้างให้ครบเอง

    const has = (c) => cols.some((x) => x.column_name === c);

    if (!has("remote_start")) {
      await sequelize.query(
        `ALTER TABLE print_jobs ADD COLUMN remote_start VARCHAR(255) DEFAULT '0'`
      );
      console.log("print_jobs.remote_start added");
    }

    if (!has("remote_program")) {
      await sequelize.query(
        `ALTER TABLE print_jobs ADD COLUMN remote_program VARCHAR(255)`
      );
      console.log("print_jobs.remote_program added");
    }

    if (!has("remote_error")) {
      await sequelize.query(`ALTER TABLE print_jobs ADD COLUMN remote_error TEXT`);
      console.log("print_jobs.remote_error added");
    }
  } catch (err) {
    console.error("ensureRemoteStartColumns failed:", err.message);
  }
};
