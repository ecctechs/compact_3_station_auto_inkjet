// ส่งซ้ำเข้าเครื่องที่งานถืออยู่ (POST /machine-queue/:id/begin-resend) — หลัง SWAP / แก้ข้อมูลในหน้า Order Detail
//
// รันแบบเดียวกับ queue-reliability.test.js:
//   QUEUE_TEST_DATABASE_URL=postgres://.../queue_reliability_test node --test test/
const { test, before, after, beforeEach } = require("node:test");
const assert = require("node:assert/strict");
const { randomUUID } = require("node:crypto");

// ห้ามใช้ฐานหน้างาน: ต้องระบุฐานทดสอบบน loopback โดยตรงเท่านั้น
const url = new URL(process.env.QUEUE_TEST_DATABASE_URL || "http://missing");
if (!["127.0.0.1", "localhost"].includes(url.hostname) || url.pathname !== "/queue_reliability_test")
  throw new Error("Set QUEUE_TEST_DATABASE_URL to an isolated local queue_reliability_test database");
process.env.POSTGRESQL_HOST = url.href;
const sequelize = require("../database");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const { MachineQueue } = require("../model/machineQueueModel");
const express = require("express");
const app = express();
app.use(express.json());
app.use(require("../routes/MachineQueue"));
app.use(require("../routes/Job"));
let server, base;

before(async () => {
  await sequelize.sync();
  server = app.listen(0, "127.0.0.1");
  await new Promise(resolve => server.once("listening", resolve));
  base = `http://127.0.0.1:${server.address().port}`;
});
after(async () => {
  if (server) await new Promise(resolve => server.close(resolve));
  await sequelize.close();
});
beforeEach(async () => {
  await MachineQueue.destroy({ where: {} });
  await PrintJobCommand.destroy({ where: {} });
  await PrintJob.destroy({ where: {} });
});

async function request(path, body, method = "POST") {
  const response = await fetch(base + path, {
    method, headers: { "content-type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  return { status: response.status, ...(await response.json()) };
}
const job = (status = "Waiting") => PrintJob.create({ barcode_raw: randomUUID(), status });
const enqueue = (id, machine = "MK", round = 1) =>
  request("/machine-queue/enqueue", { print_jobs_id: id, items: [{ machine, round }] });
const claim = (id, machine = "MK") => request("/machine-queue/claim", { machine, print_jobs_id: id });
const begin = (id, token) => request(`/machine-queue/${id}/begin-send`, { token });
const resend = (id, token) => request(`/machine-queue/${id}/begin-resend`, { token });
const finish = (id, token, outcome = "sent", detail) =>
  request(`/machine-queue/${id}/finish-send`, { token, outcome, ...(detail ? { detail } : {}) });
const release = (machine, holderId) =>
  request("/machine-queue/release", { machine, expected_holder_id: holderId });
const commands = (jobId, command) => PrintJobCommand.findAll({ where: { job_id: jobId, command }, order: [["id", "ASC"]] });
const latest = rowId => PrintJobCommand.findOne({
  where: { command: "QUEUE_DISPATCH", payload: { queue_id: rowId } }, order: [["id", "DESC"]],
});

/** งานที่ถือ MK และส่งรอบแรกผ่านแล้ว */
async function sentHolder(machine = "MK") {
  const j = await job();
  await enqueue(j.id, machine);
  const row = (await claim(j.id, machine)).data.claimed;
  const token = randomUUID();
  assert.equal((await begin(row.id, token)).status, 200);
  assert.equal((await finish(row.id, token)).status, 200);
  return { job: j, row: await MachineQueue.findByPk(row.id) };
}

test("ส่งซ้ำผ่าน: บันทึก RESEND_MK ไม่ใช่ MK · แถวยังถือเครื่อง sent_at เดิม · ปล่อยเครื่องได้ตามปกติ", async () => {
  const { job: j, row } = await sentHolder();
  const sentAt = row.sent_at.getTime();
  const t = randomUUID();
  assert.equal((await resend(row.id, t)).status, 200);
  assert.equal((await latest(row.id)).payload.resend, true);
  assert.equal((await finish(row.id, t, "sent", { swapped: true })).status, 200);

  assert.equal((await commands(j.id, "MK")).length, 1, "ส่งซ้ำต้องไม่นับเป็น MK อีกครั้ง (งาน 22 จะนับเป็นรอบสอง)");
  const again = await commands(j.id, "RESEND_MK");
  assert.equal(again.length, 1);
  assert.equal(again[0].success, true);
  assert.deepEqual(again[0].payload, { swapped: true });

  const after = await MachineQueue.findByPk(row.id);
  assert.equal(after.state, "active");
  assert.equal(after.sent_at.getTime(), sentAt);
  assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
  assert.equal((await release("MK", row.id)).status, 200);
});

test("ส่งซ้ำไม่ถึงเครื่อง (not_sent): แถวยังถือเครื่อง สถานะงานไม่ถอย ปล่อยเครื่องได้", async () => {
  const { job: j, row } = await sentHolder();
  const t = randomUUID();
  await resend(row.id, t);
  assert.equal((await finish(row.id, t, "not_sent")).status, 200);
  const after = await MachineQueue.findByPk(row.id);
  assert.equal(after.state, "active");
  assert.ok(after.sent_at);
  assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
  assert.equal((await commands(j.id, "RESEND_MK")).length, 0);
  assert.equal((await release("MK", row.id)).status, 200);
});

// ตกลง 30/10/2026: ผลไม่รู้ไม่ขวางปุ่มหน้าเครื่อง / จบงานแล้ว — ปลดให้เองพร้อมร่องรอย
test("ส่งซ้ำไม่รู้ผล (โปรแกรมรุ่นก่อน): ส่งซ้ำอีกครั้งได้ · ปล่อยเครื่อง / จบงานได้ ปลดผลพร้อมร่องรอย", async () => {
  const { row } = await sentHolder();
  const t1 = randomUUID();
  await resend(row.id, t1);
  assert.equal((await finish(row.id, t1, "unknown")).status, 200);
  assert.equal((await finish(row.id, t1, "not_sent")).status, 409, "ไม่รู้ผลแล้วย้อนเป็นไม่ได้ส่งเองไม่ได้");

  const t2 = randomUUID();
  assert.equal((await resend(row.id, t2)).status, 200);
  assert.equal((await finish(row.id, t2, "unknown")).status, 200);
  assert.equal((await release("MK", row.id)).status, 200);
  assert.equal((await latest(row.id)).payload.resolved_by, "release");

  // จบงานตอนส่งซ้ำไม่รู้ผล
  const { job: k, row: rowK } = await sentHolder();
  const t3 = randomUUID();
  await resend(rowK.id, t3);
  await finish(rowK.id, t3, "unknown");
  assert.equal((await request(`/job/${k.id}/status`, { status: "Success" }, "PATCH")).status, 200);
  assert.equal((await latest(rowK.id)).payload.resolved_by, "complete");
});

test("ส่งซ้ำไม่ได้: ยังไม่ส่งรอบแรก · กำลังส่งอยู่ · ไม่ได้ถือเครื่อง · token ซ้ำ · งานจบแล้ว", async () => {
  // ถึงคิวแต่ยังไม่ส่งรอบแรก
  const a = await job();
  await enqueue(a.id);
  const rowA = (await claim(a.id)).data.claimed;
  assert.equal((await resend(rowA.id, randomUUID())).status, 409);

  // กำลังส่งรอบแรก (sending) — ยังไม่มี sent_at
  const t = randomUUID();
  await begin(rowA.id, t);
  assert.equal((await resend(rowA.id, randomUUID())).status, 409);
  await finish(rowA.id, t);

  // กำลังส่งซ้ำอยู่ — ห้ามซ้อน
  const t1 = randomUUID();
  assert.equal((await resend(rowA.id, t1)).status, 200);
  assert.equal((await resend(rowA.id, randomUUID())).status, 409);
  await finish(rowA.id, t1);

  // token ที่ใช้ไปแล้ว
  assert.equal((await resend(rowA.id, t1)).status, 409);

  // งานที่รอคิว (ไม่ได้ถือเครื่อง)
  const b = await job();
  await enqueue(b.id);
  const rowB = await MachineQueue.findOne({ where: { print_jobs_id: b.id } });
  assert.equal(rowB.state, "pending");
  assert.equal((await resend(rowB.id, randomUUID())).status, 409);

  // ปล่อยเครื่องแล้ว (done)
  assert.equal((await release("MK", rowA.id)).status, 200);
  assert.equal((await resend(rowA.id, randomUUID())).status, 409);

  // งานถูกยกเลิกระหว่างถือเครื่อง
  const c = await sentHolder("UV2");
  await PrintJob.update({ status: "Cancel" }, { where: { id: c.job.id } });
  assert.equal((await resend(c.row.id, randomUUID())).status, 409);
});

test("ส่งซ้ำไม่เปลี่ยนลำดับคิว: งานที่รอได้เครื่องต่อเมื่อปล่อย", async () => {
  const { row } = await sentHolder();
  const waiting = await job();
  await enqueue(waiting.id);

  const t = randomUUID();
  await resend(row.id, t);
  await finish(row.id, t);
  assert.equal((await MachineQueue.findOne({ where: { print_jobs_id: waiting.id } })).state, "pending");

  const r = await release("MK", row.id);
  assert.equal(r.status, 200);
  assert.equal(r.data.next.print_jobs_id, waiting.id);
});

test("ส่งซ้ำแล้วไม่รู้ผล ข้ามเครื่องได้ (ปลดแบบมีร่องรอย)", async () => {
  const { job: j, row } = await sentHolder("UV2");
  const t = randomUUID();
  await resend(row.id, t);
  await finish(row.id, t, "unknown");
  const r = await request("/machine-queue/skip", { print_jobs_id: j.id, machine: "UV2" });
  assert.equal(r.status, 200);
  assert.equal((await MachineQueue.findByPk(row.id)).state, "done");
  const attempt = await latest(row.id);
  assert.equal(attempt.payload.outcome, "not_sent");
  assert.equal(attempt.payload.resolved_from, "unknown");
});
