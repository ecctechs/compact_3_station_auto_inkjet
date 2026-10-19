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
const enqueue = (id, machine = "UV2", program_name = "SPK-LOT") =>
  request("/machine-queue/enqueue", { print_jobs_id: id, items: [{ machine, program_name }] });
const claim = (id, machine = "UV2") => request("/machine-queue/claim", { machine, print_jobs_id: id });
const begin = (id, token) => request(`/machine-queue/${id}/begin-send`, { token });
const finish = (id, token) => request(`/machine-queue/${id}/finish-send`, { token, outcome: "sent", detail: { program: "x" } });
const release = (id, machine = "UV2") => request("/machine-queue/release", { machine, expected_holder_id: id });
const switchTo = (id, program_name, machine = "UV2") =>
  request("/machine-queue/switch-program", { machine, expected_holder_id: id, program_name });

async function sent(row) {
  const token = randomUUID();
  assert.equal((await begin(row.id, token)).status, 200);
  assert.equal((await finish(row.id, token)).status, 200);
}
// งานหนึ่งที่ถือ UV2 อยู่และส่งรุ่นแรกเข้าเครื่องแล้ว
async function holding(machine = "UV2") {
  const j = await job();
  await enqueue(j.id, machine);
  const row = (await claim(j.id, machine)).data.claimed;
  await sent(row);
  return { job: j, row };
}

test("switch keeps the machine: holder done, next round of the same job active and unsent", async () => {
  const a = await holding();
  const b = await job();
  await enqueue(b.id);                       // B รอคิว UV2 อยู่หลัง A

  const r = await switchTo(a.row.id, "SPK-LOT-1");
  assert.equal(r.status, 200);
  assert.equal(r.data.released.id, a.row.id);
  assert.equal(r.data.next.print_jobs_id, a.job.id);
  assert.equal(r.data.next.round, 2);
  assert.equal(r.data.next.program_name, "SPK-LOT-1");
  assert.equal(r.data.next.state, "active");
  assert.equal(r.data.next.sent_at, null);

  const rows = await MachineQueue.findAll({ where: { machine: "UV2" }, order: [["id", "ASC"]] });
  assert.deepEqual(rows.map(x => [x.print_jobs_id, x.state]),
    [[a.job.id, "done"], [b.id, "pending"], [a.job.id, "active"]]);
});

test("repeated switches keep counting rounds and B still gets the machine after the push button", async () => {
  const a = await holding();
  const b = await job();
  await enqueue(b.id);

  let holder = a.row;
  for (const [program, round] of [["SPK-LOT-1", 2], ["SPK-LOT-2", 3]]) {
    const r = await switchTo(holder.id, program);
    assert.equal(r.status, 200);
    assert.equal(r.data.next.round, round);
    holder = r.data.next;
    await sent(holder);                      // ST1 ส่งรุ่นใหม่เข้าเครื่อง
  }

  const pushed = await release(holder.id);   // ปุ่มหน้าเครื่อง = จบงาน
  assert.equal(pushed.status, 200);
  assert.equal(pushed.data.next.print_jobs_id, b.id);
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: a.job.id, state: ["pending", "active"] } }), 0);
});

test("stale holder id changes nothing", async () => {
  const a = await holding();
  const r = await switchTo(a.row.id + 999, "SPK-LOT-1");
  assert.equal(r.status, 409);
  assert.equal(await MachineQueue.count(), 1);
  assert.equal((await MachineQueue.findByPk(a.row.id)).state, "active");
});

test("cannot switch before the first program reached the machine", async () => {
  const j = await job();
  await enqueue(j.id);
  const row = (await claim(j.id)).data.claimed;
  const r = await switchTo(row.id, "SPK-LOT-1");
  assert.equal(r.status, 409);
  assert.equal(await MachineQueue.count(), 1);
});

test("cannot switch while the current send is unresolved", async () => {
  const j = await job();
  await enqueue(j.id);
  const row = (await claim(j.id)).data.claimed;
  assert.equal((await begin(row.id, randomUUID())).status, 200);   // เริ่มส่งแล้ว ยังไม่รู้ผล
  await MachineQueue.update({ sent_at: new Date() }, { where: { id: row.id } });
  const r = await switchTo(row.id, "SPK-LOT-1");
  assert.equal(r.status, 409);
  assert.equal(await MachineQueue.count(), 1);
});

test("cannot switch a job that is no longer in production", async () => {
  const a = await holding();
  await a.job.update({ status: "Success" });
  const r = await switchTo(a.row.id, "SPK-LOT-1");
  assert.equal(r.status, 409);
  assert.equal(await MachineQueue.count(), 1);
});

test("MK has no variants and an empty program is rejected", async () => {
  const a = await holding("MK");
  assert.equal((await switchTo(a.row.id, "SPK-LOT-1", "MK")).status, 400);
  const u = await holding("UV1");
  assert.equal((await switchTo(u.row.id, "  ", "UV1")).status, 400);
});

test("concurrent switches from two screens produce one new round", async () => {
  const a = await holding();
  const replies = await Promise.all([switchTo(a.row.id, "SPK-LOT-1"), switchTo(a.row.id, "SPK-LOT-2")]);
  assert.deepEqual(replies.map(r => r.status).sort(), [200, 409]);
  assert.equal(await MachineQueue.count({ where: { state: "active" } }), 1);
});
