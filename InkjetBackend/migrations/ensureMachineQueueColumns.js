const sequelize = require("../database");

// machine_queue ต้องมี queued_at ไว้เรียงลำดับคิว แยกจาก created_at
//
// created_at คือเวลาที่แถวถูกสร้าง ซึ่งของทุกรอบในงานเดียวกันเท่ากันหมด เพราะถูก
// สร้างพร้อมกันตอนกดเริ่มงาน ใช้เรียงคิวได้ตราบใดที่ลำดับไม่เคยเปลี่ยน แต่ตัวเลือก
// "ปล่อยเครื่อง" ของงานที่เข้าเครื่องเดิมหลายรอบ ต้องดันรอบถัดไปไปต่อท้ายคิว
// ซึ่ง Sequelize ไม่ยอมให้แก้ created_at เลยต้องมีคอลัมน์ของตัวเองไว้ขยับ
//
// sequelize.sync() ไม่เพิ่มคอลัมน์ให้ตารางที่มีอยู่แล้ว จึงต้องเติมเอง — รันซ้ำได้ ไม่พัง
module.exports = async function ensureMachineQueueColumns() {
  try {
    const [cols] = await sequelize.query(`
      SELECT column_name FROM information_schema.columns
      WHERE table_schema = 'public' AND table_name = 'machine_queues'`);

    if (cols.length === 0) return; // ยังไม่มีตาราง — sync() จะสร้างให้ครบเอง
    const has = (name) => cols.some((x) => x.column_name === name);

    if (!has("queued_at")) {
      await sequelize.query(`ALTER TABLE machine_queues ADD COLUMN queued_at TIMESTAMP WITH TIME ZONE`);

      // แถวที่มีอยู่แล้วให้ยึดเวลาเดิมไว้ ลำดับคิวที่ค้างอยู่จะได้ไม่สลับ
      await sequelize.query(`UPDATE machine_queues SET queued_at = created_at WHERE queued_at IS NULL`);

      console.log("machine_queues.queued_at added");
    }

    // UV2 ที่จองคิวไว้รอ ST3 เลือกรุ่น — แถวเดิมทั้งหมดไม่ได้รอ (false)
    if (!has("awaiting_pick")) {
      await sequelize.query(`ALTER TABLE machine_queues ADD COLUMN awaiting_pick BOOLEAN NOT NULL DEFAULT false`);
      console.log("machine_queues.awaiting_pick added");
    }
  } catch (err) {
    console.error("ensureMachineQueueColumns failed:", err.message);
  }
};
