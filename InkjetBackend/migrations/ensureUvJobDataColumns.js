const sequelize = require("../database");

// uv_job_data เดิมมีคอลัมน์ name และไม่มี qty
// sequelize.sync() ไม่เปลี่ยน schema ของตารางที่มีอยู่ จึงต้องปรับเอง — รันซ้ำได้ ไม่พัง
module.exports = async function ensureUvJobDataColumns() {
  try {
    const [cols] = await sequelize.query(`
      SELECT column_name FROM information_schema.columns
      WHERE table_schema = 'public' AND table_name = 'uv_job_data'`);

    if (cols.length === 0) return; // ยังไม่มีตาราง — sync() จะสร้างให้ครบเอง

    const has = (c) => cols.some((x) => x.column_name === c);

    // name -> erp_mfg (ถ้ามี erp_mfg อยู่แล้วแปลว่าเคยรันไปแล้ว)
    if (has("name") && !has("erp_mfg")) {
      await sequelize.query(`ALTER TABLE uv_job_data RENAME COLUMN name TO erp_mfg`);
      console.log("uv_job_data.name -> erp_mfg (renamed)");
    }

    if (!has("qty")) {
      await sequelize.query(`ALTER TABLE uv_job_data ADD COLUMN qty INTEGER`);
      console.log("uv_job_data.qty added");
    }
  } catch (err) {
    console.error("ensureUvJobDataColumns failed:", err.message);
  }
};
