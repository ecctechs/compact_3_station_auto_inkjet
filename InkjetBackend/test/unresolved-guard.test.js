// คิวที่ส่งแล้วไม่รู้ผล ปล่อย / ล้าง / ยกเลิก / จบ ไม่ได้ — ทางออกเดียวคือคนยืนยันข้ามเครื่อง
// (POST /machine-queue/skip ดู machine-skip.test.js)
//
// เดิมโหมด "ส่งไม่ครบ ไม่ส่งเลย" ส่งธง resolve_unresolved / ?resolve=true มาปลดได้
// เลิกโหมดนั้นแล้ว 19/10/2026 — โปรแกรมรุ่นเก่าที่ยังส่งธงมา ต้องไม่มีผลอะไร
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
async function job() { return PrintJob.create({ barcode_raw: randomUUID(), status: "Waiting" }); }
const enqueue = (id, items) => request("/machine-queue/enqueue", { print_jobs_id: id, items });
const claim = (id, machine) => request("/machine-queue/claim", { machine, print_jobs_id: id });
const begin = (id, token) => request(`/machine-queue/${id}/begin-send`, { token });
const finish = (id, token, outcome) => request(`/machine-queue/${id}/finish-send`, { token, outcome });

/** งานหนึ่งใบที่ถือ UV1 ไว้ และผลส่งค้างเป็น unknown (แบบคิว 441) */
async function stuckUv1() {
  const j = await job();
  assert.ok([200, 201].includes((await enqueue(j.id, [{ machine: "UV1" }])).status));
  const row = (await claim(j.id, "UV1")).data.claimed;
  const token = randomUUID();
  assert.equal((await begin(row.id, token)).status, 200);
  assert.equal((await finish(row.id, token, "unknown")).status, 200);
  return { job: j, row };
}

async function lastDispatch(rowId) {
  return PrintJobCommand.findOne({
    where: { command: "QUEUE_DISPATCH", payload: { queue_id: rowId } }, order: [["id", "DESC"]],
  });
}

test("ไม่ส่งธง: คิวที่ไม่รู้ผลยังปล่อย / ล้าง / ยกเลิกไม่ได้ เหมือนเดิม", async () => {
  const { job: j, row } = await stuckUv1();
  assert.equal((await request("/machine-queue/release", { machine: "UV1", expected_holder_id: row.id })).status, 409);
  assert.equal((await request(`/machine-queue/job/${j.id}`, undefined, "DELETE")).status, 409);
  assert.equal((await request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH")).status, 409);
  assert.equal((await lastDispatch(row.id)).payload.outcome, "unknown");
});

test("ธงเก่าของโหมดส่งไม่ครบไม่มีผลแล้ว: ปล่อย / ล้าง / ยกเลิก / จบ ยังไม่ได้ ผลยังเป็น unknown", async () => {
  const { job: j, row } = await stuckUv1();
  assert.equal((await request("/machine-queue/release",
    { machine: "UV1", expected_holder_id: row.id, resolve_unresolved: true })).status, 409);
  assert.equal((await request(`/machine-queue/job/${j.id}?resolve=true`, undefined, "DELETE")).status, 409);
  assert.equal((await request(`/job/${j.id}/status`, { status: "Cancel", resolve_unresolved: true }, "PATCH")).status, 409);
  assert.equal((await request(`/job/${j.id}/status`, { status: "Success", resolve_unresolved: true }, "PATCH")).status, 409);

  assert.equal((await MachineQueue.findByPk(row.id)).state, "active");
  assert.equal((await lastDispatch(row.id)).payload.outcome, "unknown");
  assert.ok(!["Cancel", "Success"].includes((await PrintJob.findByPk(j.id)).status));
});

test("ข้ามเครื่องแล้ว ยกเลิกงานได้ตามปกติ", async () => {
  const { job: j, row } = await stuckUv1();
  assert.equal((await request("/machine-queue/skip", { print_jobs_id: j.id, machine: "UV1" })).status, 200);
  assert.equal((await lastDispatch(row.id)).payload.outcome, "not_sent");
  assert.equal((await request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH")).status, 200);
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: j.id } }), 0);
});

test("งาน 11 ส่ง UV1 ผ่าน UV2 ไม่ผ่าน (not_sent) → ล้างคิวและตีกลับได้ (ส่งไม่ผ่านแน่นอนไม่ใช่ผลค้าง)", async () => {
  const j = await job();
  await enqueue(j.id, [{ machine: "UV1" }, { machine: "UV2" }]);
  const uv1 = (await claim(j.id, "UV1")).data.claimed;
  const uv2 = (await claim(j.id, "UV2")).data.claimed;
  const t1 = randomUUID(), t2 = randomUUID();
  assert.equal((await begin(uv1.id, t1)).status, 200);
  assert.equal((await begin(uv2.id, t2)).status, 200);
  assert.equal((await finish(uv1.id, t1, "sent")).status, 200);
  assert.equal((await finish(uv2.id, t2, "not_sent")).status, 200);

  assert.equal((await request(`/machine-queue/job/${j.id}`, undefined, "DELETE")).status, 200);
  assert.equal((await request(`/job/${j.id}/status`, { status: "Waiting" }, "PATCH")).status, 200);
  assert.equal((await PrintJob.findByPk(j.id)).status, "Waiting");
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: j.id } }), 0);

  // กดเริ่มงานใหม่ได้ทันที — จองเครื่องทั้งคู่ได้อีกครั้ง
  assert.ok([200, 201].includes((await enqueue(j.id, [{ machine: "UV1" }, { machine: "UV2" }])).status));
  assert.ok((await claim(j.id, "UV1")).data.claimed);
  assert.ok((await claim(j.id, "UV2")).data.claimed);
});
