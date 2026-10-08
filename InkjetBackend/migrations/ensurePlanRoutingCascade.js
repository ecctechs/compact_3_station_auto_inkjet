const sequelize = require("../database");

const CONSTRAINT = "plan_routings_print_jobs_id_fkey";

// plan_routings ถูกสร้างครั้งแรกด้วย FK แบบ NO ACTION → ลบแถวใน print_jobs ไม่ได้
// ตารางลูกตัวอื่น (patterns, uv_job_data, print_job_commands) เป็น CASCADE อยู่แล้ว
// sequelize.sync() ไม่แก้ constraint ที่มีอยู่ จึงต้องปรับเอง — รันซ้ำได้ ไม่พัง
module.exports = async function ensurePlanRoutingCascade() {
  try {
    const [rows] = await sequelize.query(`
      SELECT c.confdeltype
      FROM pg_constraint c
      JOIN pg_class t ON t.oid = c.conrelid
      WHERE c.contype = 'f'
        AND t.relname = 'plan_routings'
        AND c.conname = '${CONSTRAINT}'`);

    // ไม่มี constraint (ตารางเพิ่งสร้าง) หรือเป็น CASCADE อยู่แล้ว → ไม่ต้องทำอะไร
    if (rows.length === 0 || rows[0].confdeltype === "c") return;

    await sequelize.query(`
      ALTER TABLE plan_routings
        DROP CONSTRAINT ${CONSTRAINT},
        ADD CONSTRAINT ${CONSTRAINT}
          FOREIGN KEY (print_jobs_id) REFERENCES print_jobs (id)
          ON DELETE CASCADE ON UPDATE CASCADE`);

    console.log("plan_routings FK -> ON DELETE CASCADE (updated)");
  } catch (err) {
    console.error("ensurePlanRoutingCascade failed:", err.message);
  }
};
