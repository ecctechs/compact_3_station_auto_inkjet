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
const ensureMachineQueueColumns = require("../migrations/ensureMachineQueueColumns");
const express = require("express");
const app = express();
app.use(express.json());
app.use(require("../routes/MachineQueue"));
let server, base;

before(async () => {
  await sequelize.sync();
  await ensureMachineQueueColumns();          // ตารางเดิมของฐานทดสอบได้คอลัมน์ awaiting_pick
  await ensureMachineQueueColumns();          // รันซ้ำต้องไม่พัง
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
const job = () => PrintJob.create({ barcode_raw: randomUUID(), status: "Process" });
const enqueue = (id, item) => request("/machine-queue/enqueue", { print_jobs_id: id, items: [{ machine: "UV2", ...item }] });
const release = (id) => request("/machine-queue/release", { machine: "UV2", expected_holder_id: id });
const rowOf = (jobId) => MachineQueue.findOne({ where: { print_jobs_id: jobId, machine: "UV2" } });

// X ถือ UV2 อยู่ · A (มีรุ่นย่อย รอ ST3 เลือก) กดเริ่มก่อน · B (ไฟล์เดียว) กดเริ่มทีหลัง
async function setup() {
  const x = await job(); await enqueue(x.id, { program_name: "X" });
  const holder = (await request("/machine-queue/claim", { machine: "UV2", print_jobs_id: x.id })).data.claimed;
  await MachineQueue.update({ sent_at: new Date() }, { where: { id: holder.id } });
  const a = await job(); await enqueue(a.id, { awaiting_pick: true });
  const b = await job(); await enqueue(b.id, { program_name: "B" });
  return { holder, a, b };
}

test("A that waits for ST3 keeps its place: B cannot overtake on UV2", async () => {
  const { holder, a, b } = await setup();
  assert.equal((await rowOf(a.id)).awaiting_pick, true);
  const r = await release(holder.id);
  assert.equal(r.status, 200);
  assert.equal(r.data.next.print_jobs_id, a.id);
  assert.equal((await rowOf(b.id)).state, "pending");
});

test("a row waiting for ST3 cannot be sent until a variant is picked", async () => {
  const { holder, a } = await setup();
  await release(holder.id);
  const row = await rowOf(a.id);
  const blocked = await request(`/machine-queue/${row.id}/begin-send`, { token: randomUUID() });
  assert.equal(blocked.status, 409);
  assert.match(JSON.stringify(blocked), /ST3/);

  const picked = await request(`/machine-queue/${row.id}`, { program_name: "SPK-LOT-2" }, "PATCH");
  assert.equal(picked.status, 200);
  const after = await rowOf(a.id);
  assert.equal(after.awaiting_pick, false);
  assert.equal(after.program_name, "SPK-LOT-2");
  assert.equal((await request(`/machine-queue/${row.id}/begin-send`, { token: randomUUID() })).status, 200);
});

test("ST3 can pick before A reaches the head — order does not change", async () => {
  const { holder, a, b } = await setup();
  const row = await rowOf(a.id);
  await request(`/machine-queue/${row.id}`, { program_name: "SPK-LOT-1" }, "PATCH");
  assert.equal((await rowOf(a.id)).state, "pending");
  const r = await release(holder.id);
  assert.equal(r.data.next.print_jobs_id, a.id);
  assert.equal(r.data.next.awaiting_pick, false);
  assert.equal((await rowOf(b.id)).state, "pending");
});

test("a row booked with a program is never marked as waiting", async () => {
  const j = await job();
  await enqueue(j.id, { program_name: "ONE", awaiting_pick: true });
  assert.equal((await rowOf(j.id)).awaiting_pick, false);
});
