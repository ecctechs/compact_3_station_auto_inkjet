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
const job = (status = "Process") => PrintJob.create({ barcode_raw: randomUUID(), status });
const enqueue = (id, machine = "UV2") => request("/machine-queue/enqueue", { print_jobs_id: id, items: [{ machine }] });
const claim = (id, machine = "UV2") => request("/machine-queue/claim", { machine, print_jobs_id: id });
const begin = (id, token) => request(`/machine-queue/${id}/begin-send`, { token });
const finish = (id, token, outcome = "sent", error) =>
  request(`/machine-queue/${id}/finish-send`, { token, outcome, ...(error ? { error } : {}) });
const skip = (jobId, machine = "UV2", reason = "ตรวจแล้ว เครื่องเสีย") =>
  request("/machine-queue/skip", { print_jobs_id: jobId, machine, reason });
const getAll = () => request("/machine-queue/getAll", undefined, "GET");

async function holder(machine = "UV2") {
  const j = await job();
  await enqueue(j.id, machine);
  const row = (await claim(j.id, machine)).data.claimed;
  return { job: j, row };
}

test("skipping the holder frees the machine for the head of the queue and records SKIP", async () => {
  const a = await holder();
  const token = randomUUID();
  await begin(a.row.id, token);
  await finish(a.row.id, token);
  const b = await job("Waiting"); await enqueue(b.id);
  const c = await job("Waiting"); await enqueue(c.id);

  const r = await skip(a.job.id);
  assert.equal(r.status, 200);
  assert.equal(r.data.freed, true);
  assert.equal(r.data.next.print_jobs_id, b.id);
  assert.equal((await MachineQueue.findByPk(a.row.id)).state, "done");
  assert.equal((await MachineQueue.findOne({ where: { print_jobs_id: c.id } })).state, "pending");

  const cmd = await PrintJobCommand.findOne({ where: { job_id: a.job.id, command: "SKIP_UV2" } });
  assert.ok(cmd && cmd.success);
  assert.equal(cmd.payload.reason, "ตรวจแล้ว เครื่องเสีย");
  assert.deepEqual(cmd.payload.queue_ids, [a.row.id]);
});

test("an unresolved send can be skipped: the dispatch is resolved with a trace, not deleted", async () => {
  const a = await holder();
  await begin(a.row.id, randomUUID());                 // ส่งออกไปแล้ว ไม่ได้คำตอบ
  const b = await job("Waiting"); await enqueue(b.id);

  const r = await skip(a.job.id);
  assert.equal(r.status, 200);
  assert.equal(r.data.next.print_jobs_id, b.id);
  const dispatch = await PrintJobCommand.findOne({ where: { job_id: a.job.id, command: "QUEUE_DISPATCH" } });
  assert.equal(dispatch.payload.outcome, "not_sent");
  assert.equal(dispatch.payload.resolved_from, "sending");
  assert.equal(dispatch.payload.resolved_by, "operator");
});

test("skipping a waiting row leaves the current holder alone", async () => {
  const a = await holder();
  const b = await job("Waiting"); await enqueue(b.id);

  const r = await skip(b.id);
  assert.equal(r.status, 200);
  assert.equal(r.data.freed, false);
  assert.equal(r.data.next, null);
  assert.equal((await MachineQueue.findByPk(a.row.id)).state, "active");
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: b.id, state: ["pending", "active"] } }), 0);
});

test("skip before any queue row blocks booking that machine later", async () => {
  const j = await job("Waiting");
  const r = await skip(j.id, "MK", "ต่อไม่ติด");
  assert.equal(r.status, 200);
  assert.equal(r.data.skipped[0].state, "done");
  assert.equal(r.data.skipped[0].round, 1);

  await enqueue(j.id, "MK");
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: j.id } }), 1);
});

test("finished or cancelled jobs cannot be skipped and bad machines are rejected", async () => {
  const done = await job("Success");
  assert.equal((await skip(done.id)).status, 409);
  const j = await job();
  assert.equal((await request("/machine-queue/skip", { print_jobs_id: j.id, machine: "PLC" })).status, 400);
  assert.equal(await PrintJobCommand.count(), 0);
});

test("getAll tells why the last send failed", async () => {
  const a = await holder();
  const token = randomUUID();
  await begin(a.row.id, token);
  await finish(a.row.id, token, "not_sent", "UV2: ต่อไม่ติด (10.0.0.9:10086)");
  const rows = (await getAll()).data;
  const row = rows.find(r => r.id === a.row.id);
  assert.equal(row.dispatch_state, "not_sent");
  assert.equal(row.dispatch_error, "UV2: ต่อไม่ติด (10.0.0.9:10086)");
});
