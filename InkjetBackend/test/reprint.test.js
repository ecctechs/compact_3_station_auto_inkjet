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

// งานที่รอบก่อนส่ง MK สำเร็จ แล้วรอบนี้ส่งเครื่องแรกไม่ผ่านแน่นอน
async function failFirstSend(job) {
  await request("/machine-queue/enqueue", { print_jobs_id: job.id, items: [{ machine: "MK" }] });
  const row = (await request("/machine-queue/claim", { machine: "MK", print_jobs_id: job.id })).data.claimed;
  await job.update({ status: "Process" });
  const token = randomUUID();
  await request(`/machine-queue/${row.id}/begin-send`, { token });
  const r = await request(`/machine-queue/${row.id}/finish-send`, { token, outcome: "not_sent", error: "ต่อไม่ติด" });
  assert.equal(r.status, 200);
  return (await PrintJob.findByPk(job.id)).status;
}

test("after REPRINT, a failed first send returns the job to Waiting (old attempt is not counted)", async () => {
  const job = await PrintJob.create({ barcode_raw: randomUUID(), status: "Waiting" });
  await PrintJobCommand.create({ job_id: job.id, command: "MK", success: true, sent_at: new Date() });
  await request(`/job/addCommand/${job.id}`, { command: "REPRINT", success: true });
  assert.equal(await failFirstSend(job), "Waiting");
});

test("without REPRINT, an earlier success still keeps the job in Process (unchanged rule)", async () => {
  const job = await PrintJob.create({ barcode_raw: randomUUID(), status: "Waiting" });
  await PrintJobCommand.create({ job_id: job.id, command: "MK", success: true, sent_at: new Date() });
  assert.equal(await failFirstSend(job), "Process");
});

test("a success after REPRINT keeps the job in Process", async () => {
  const job = await PrintJob.create({ barcode_raw: randomUUID(), status: "Waiting" });
  await request(`/job/addCommand/${job.id}`, { command: "REPRINT", success: true });
  await PrintJobCommand.create({ job_id: job.id, command: "UV2", success: true, sent_at: new Date() });
  assert.equal(await failFirstSend(job), "Process");
});
