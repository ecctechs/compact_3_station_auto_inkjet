const sequelize = require("../database");
const { PrintJobCommand } = require("../model/jobModel");

const DISPATCH = "QUEUE_DISPATCH";

// ล็อกเฉพาะช่วงแก้ฐานข้อมูล ไม่ถือไว้ตอนคุยกับเครื่องจริง
// ใช้ล็อกเดียวกันทุกทาง รวมตอนเครื่องว่างที่ยังไม่มีแถวให้ล็อก
async function withQueueLock(work) {
  return sequelize.transaction(async (transaction) => {
    await sequelize.query("SELECT pg_advisory_xact_lock(72419, 1)", { transaction });
    return work(transaction);
  });
}

function conflict(message) {
  const error = new Error(message);
  error.statusCode = 409;
  throw error;
}

async function latestDispatch(queueId, transaction) {
  return PrintJobCommand.findOne({
    where: { command: DISPATCH, payload: { queue_id: queueId } },
    order: [["id", "DESC"]], transaction,
  });
}

function unresolved(attempt) {
  return attempt && ["sending", "unknown"].includes(attempt.payload?.outcome);
}

async function assertNoUnresolved(rows, transaction) {
  for (const row of rows) {
    if (unresolved(await latestDispatch(row.id, transaction)))
      conflict(`${row.machine}: กำลังส่งหรือยังไม่ทราบผล ห้ามปล่อยคิวก่อนตรวจเครื่อง — ตรวจเครื่องแล้วกด ข้ามเครื่อง ในแถวงาน (คิว ${row.id})`);
  }
}

// ปลดผลส่งที่ยังไม่รู้ (sending / unknown) ให้เป็น "ไม่ได้ส่ง"
//
// ทางเดียวที่เรียกได้คือคนหน้างานยืนยัน "ข้ามเครื่อง" (POST /machine-queue/skip) หลังตรวจเครื่องแล้ว
// ทางอื่นทั้งหมด (ปุ่มหน้าเครื่อง ยกเลิก จบงาน ล้างคิว) ยังห้ามด้วย assertNoUnresolved
// เดิมโหมด "ส่งไม่ครบ ไม่ส่งเลย" ส่งธงมาปลดผ่านทางเหล่านั้นได้ — เลิกโหมดนั้นแล้ว 19/10/2026
//
// เก็บร่องรอยไว้ใน payload ว่าใครปลด ไม่ลบแถว QUEUE_DISPATCH ทิ้ง
async function resolveUnresolved(rows, transaction) {
  let resolved = 0;
  for (const row of rows) {
    const attempt = await latestDispatch(row.id, transaction);
    if (!unresolved(attempt)) continue;
    await attempt.update({
      payload: {
        ...attempt.payload,
        outcome: "not_sent",
        resolved_from: attempt.payload.outcome,
        resolved_by: "operator",
        resolved_at: new Date().toISOString(),
      },
    }, { transaction });
    resolved++;
  }
  return resolved;
}

module.exports = { DISPATCH, withQueueLock, conflict, latestDispatch, unresolved, assertNoUnresolved, resolveUnresolved };
