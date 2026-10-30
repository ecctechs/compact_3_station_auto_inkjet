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
// เรียกจากปุ่มข้ามเครื่อง (POST /machine-queue/skip) และจาก settleUnresolved (จบงาน / ยกเลิก /
// ปุ่มหน้าเครื่อง / ล้างคิว) · เก็บร่องรอยไว้ใน payload ว่าใครปลด ไม่ลบแถว QUEUE_DISPATCH ทิ้ง
async function resolveUnresolved(rows, transaction, by = "operator") {
  let resolved = 0;
  for (const row of rows) {
    const attempt = await latestDispatch(row.id, transaction);
    if (!unresolved(attempt)) continue;
    await attempt.update({
      payload: {
        ...attempt.payload,
        outcome: "not_sent",
        resolved_from: attempt.payload.outcome,
        resolved_by: by,
        resolved_at: new Date().toISOString(),
      },
    }, { transaction });
    resolved++;
  }
  return resolved;
}

// "กำลังส่ง" ที่ใหม่กว่านี้ถือว่ายังคุยกับเครื่องอยู่จริง — การส่งหนึ่งครั้งนานสุดราว 40 วินาที
// (MK หมดเวลาคำสั่งละ 3 วินาที · UV โหลดโปรแกรมรอได้ 15 วินาที) เผื่อไว้เป็น 2 นาที
const SENDING_GRACE_MS = 2 * 60 * 1000;

// ทางของคนหน้างาน (จบงาน / ยกเลิก / ปุ่มหน้าเครื่อง / ล้างคิว) ต้องไม่ติดค้างเพราะผลส่งที่ไม่รู้
//
// เดิม assertNoUnresolved ปฏิเสธทุกทางจนกว่าจะมีคนกด "ข้ามเครื่อง" ลูกค้ากดจบ / ยกเลิกงานไม่ได้
// ตกลงใหม่ 30/10/2026: ส่งไม่ผ่าน = ไม่ได้ส่ง (โปรแกรมสั่งหยุดเครื่องให้) ไม่มีการถือคิวไว้ตรวจ
// ทางเหล่านี้จึงปลดผลที่ไม่รู้ให้เองพร้อมร่องรอย — ยกเว้นแถวที่กำลังส่งอยู่จริง ยังรอให้ส่งจบก่อน
// ไม่งั้นงานถัดไปจะถูกส่งทับเครื่องกลางการส่ง (ST1 ปิดกลางคัน = ค้างเกินเวลานี้ ปลดได้)
//
// คืนจำนวนแถวที่ปลด
async function settleUnresolved(rows, transaction, by) {
  for (const row of rows) {
    const attempt = await latestDispatch(row.id, transaction);
    if (!unresolved(attempt) || attempt.payload.outcome !== "sending") continue;
    const age = Date.now() - new Date(attempt.sent_at || attempt.created_at || 0).getTime();
    if (age < SENDING_GRACE_MS)
      conflict(`${row.machine}: กำลังส่งงานเข้าเครื่องอยู่ — รอสักครู่แล้วกดใหม่ (คิว ${row.id})`);
  }
  return resolveUnresolved(rows, transaction, by);
}

module.exports = { DISPATCH, withQueueLock, conflict, latestDispatch, unresolved, assertNoUnresolved, resolveUnresolved, settleUnresolved, SENDING_GRACE_MS };
