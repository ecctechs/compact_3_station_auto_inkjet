const sequelize = require("../database");
const { PrintJobCommand } = require("../model/jobModel");

const DISPATCH = "QUEUE_DISPATCH";

// ล็อกเฉพาะช่วงแก้ฐานข้อมูล ไม่ถือไว้ตอนคุยกับเครื่องจริง
// ใช้ล็อกเดียวกันทุกทาง รวมตอนเครื่องว่างที่ยังไม่มีแถวให้ล็อก
async function withQueueLock(work) {
  return sequelize.transaction(async (transaction) => {
    await sequelize.query("SELECT pg_advisory_xact_lock(72419, 1)", { transaction }); // ล็อกการแก้คิวร่วมกันทุก Station แม้เครื่องยังไม่มีแถว active
    return work(transaction); // ทำการแก้คิวทั้งหมดใน transaction เดียวกัน
  });
}

function conflict(message) {
  const error = new Error(message);
  error.statusCode = 409; // แจ้งว่าคำขอชนกับสถานะคิวปัจจุบัน
  throw error;
}

async function latestDispatch(queueId, transaction) {
  return PrintJobCommand.findOne({
    where: { command: DISPATCH, payload: { queue_id: queueId } }, // อ่านหลักฐานรอบส่งของคิวนี้โดยตรง
    order: [["id", "DESC"]], transaction,
  });
}

function unresolved(attempt) {
  return attempt && ["sending", "unknown"].includes(attempt.payload?.outcome); // ยังส่งไม่จบหรือไม่รู้ผล ต้องถือคิวไว้ตรวจ
}

async function assertNoUnresolved(rows, transaction) {
  for (const row of rows) {
    if (unresolved(await latestDispatch(row.id, transaction))) // กันการปล่อยหรือล้างคิวที่เครื่องอาจรับงานไปแล้ว
      conflict(`${row.machine}: กำลังส่งหรือยังไม่ทราบผล ห้ามปล่อยคิวก่อนตรวจเครื่อง (คิว ${row.id})`);
  }
}

module.exports = { DISPATCH, withQueueLock, conflict, latestDispatch, unresolved, assertNoUnresolved };
