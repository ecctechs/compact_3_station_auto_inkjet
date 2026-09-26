const { test, before, after, beforeEach } = require("node:test");
const assert = require("node:assert/strict");
const { randomUUID } = require("node:crypto");
const { spawnSync } = require("node:child_process");

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
const enqueue = (id, items = [{ machine: "MK" }]) => request("/machine-queue/enqueue", { print_jobs_id: id, items });
const claim = (id, machine = "MK") => request("/machine-queue/claim", { machine, print_jobs_id: id });
const begin = (id, token = randomUUID()) => request(`/machine-queue/${id}/begin-send`, { token });
const finish = (id, token, outcome = "sent") => request(`/machine-queue/${id}/finish-send`, { token, outcome });
const release = (id, extra = {}) => request("/machine-queue/release", { machine: "MK", expected_holder_id: id, ...extra });
async function active() {
  const j = await job();
  await enqueue(j.id);
  const c = await claim(j.id);
  assert.equal(c.status, 200);
  return { job: j, row: c.data.claimed };
}
async function sent(row) {
  const token = randomUUID();
  assert.equal((await begin(row.id, token)).status, 200);
  assert.equal((await finish(row.id, token)).status, 200);
  return token;
}

test("concurrent enqueue of the same job creates one row per machine/round", async () => {
  const j = await job();
  const items = [{ machine: "MK", round: 1 }, { machine: "MK", round: 2 }, { machine: "UV2" }];
  const replies = await Promise.all(Array.from({ length: 12 }, () => enqueue(j.id, items)));
  assert.ok(replies.every(r => r.status === 201));
  assert.equal(await MachineQueue.count(), 3);
});

test("concurrent claims on an empty machine grant only one holder", async () => {
  const jobs = await Promise.all(Array.from({ length: 8 }, job));
  await Promise.all(jobs.map(j => enqueue(j.id)));
  const claims = await Promise.all(jobs.map(j => claim(j.id)));
  assert.equal(claims.filter(c => c.data.claimed).length, 1);
  assert.equal(await MachineQueue.count({ where: { state: "active" } }), 1);
});

test("duplicate release cannot skip the newly promoted job", async () => {
  const a = await active();
  await sent(a.row);
  const b = await job(); await enqueue(b.id);
  const c = await job(); await enqueue(c.id);
  const results = await Promise.all([release(a.row.id), release(a.row.id)]);
  assert.deepEqual(results.map(r => r.status).sort(), [200, 409]);
  const holder = await MachineQueue.findOne({ where: { state: "active" } });
  assert.equal(holder.print_jobs_id, b.id);
  assert.equal((await release(holder.id)).status, 409); // ยังไม่ได้ส่ง ห้ามข้าม
  assert.equal((await request("/machine-queue/release", { machine: "MK" })).status, 400);
});

test("two senders get only one permission before any hardware IO", async () => {
  const { row } = await active();
  const results = await Promise.all([begin(row.id), begin(row.id)]);
  assert.deepEqual(results.map(r => r.status).sort(), [200, 409]);
  assert.equal(await PrintJobCommand.count({ where: { command: "QUEUE_DISPATCH" } }), 1);
});

test("lost begin response / fresh process cannot send again", async () => {
  const { row } = await active();
  const token = randomUUID();
  assert.equal((await begin(row.id, token)).status, 200);
  assert.equal((await begin(row.id, token)).status, 409);
  const probe = spawnSync(process.execPath, ["-e", `
    const { latestDispatch, unresolved } = require('./services/queueGuard');
    const db = require('./database');
    latestDispatch(${row.id}).then(async a => {
      if (!unresolved(a)) process.exitCode = 1;
      await db.close();
    }).catch(() => process.exit(2));
  `], { cwd: require("node:path").resolve(__dirname, ".."), env: process.env, encoding: "utf8" });
  assert.equal(probe.status, 0, probe.stderr);
  assert.equal((await begin(row.id)).status, 409);
  const rows = await request("/machine-queue/getAll", undefined, "GET");
  assert.equal(rows.status, 200);
  assert.equal(rows.data[0].dispatch_state, "sending");
});

test("successful completion is atomic and duplicate response retry creates one command", async () => {
  const { row } = await active();
  const token = await sent(row);
  const replies = await Promise.all([finish(row.id, token), finish(row.id, token)]);
  assert.ok(replies.every(r => r.status === 200));
  assert.equal(await PrintJobCommand.count({ where: { success: true, command: "MK" } }), 1);
  assert.ok((await MachineQueue.findByPk(row.id)).sent_at);
  assert.equal((await begin(row.id)).status, 409);
});

test("database failure during completion rolls back history and keeps the send guard", async () => {
  const { row } = await active(); const token = randomUUID();
  await begin(row.id, token);
  MachineQueue.addHook("beforeUpdate", "fail-finish", instance => {
    if (instance.changed("sent_at")) throw new Error("injected completion failure");
  });
  try { assert.equal((await finish(row.id, token)).status, 500); }
  finally { MachineQueue.removeHook("beforeUpdate", "fail-finish"); }
  assert.equal(await PrintJobCommand.count({ where: { success: true } }), 0);
  assert.equal((await MachineQueue.findByPk(row.id)).sent_at, null);
  assert.equal((await begin(row.id)).status, 409);
  assert.equal((await finish(row.id, token)).status, 200); // retry DB only
});

test("uncertain send cannot be released, cleared, cancelled or deleted", async () => {
  const { job: j, row } = await active(); const token = randomUUID();
  await begin(row.id, token); await finish(row.id, token, "unknown");
  assert.equal((await release(row.id)).status, 409);
  assert.equal((await request(`/machine-queue/job/${j.id}`, undefined, "DELETE")).status, 409);
  assert.equal((await request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH")).status, 409);
  assert.equal((await request(`/job/remove/${j.id}`, undefined, "DELETE")).status, 409);
  assert.equal((await request(`/machine-queue/${row.id}`, { state: "pending" }, "PATCH")).status, 409);
  assert.equal((await finish(row.id, token, "not_sent")).status, 409);
});

test("definite no-send returns to pending but does not automatically dispatch again", async () => {
  const { row } = await active(); const token = randomUUID();
  await begin(row.id, token);
  assert.equal((await finish(row.id, token, "not_sent")).status, 200);
  assert.equal((await MachineQueue.findByPk(row.id)).state, "pending");
  assert.equal((await begin(row.id)).status, 409);
});

test("marking 22 holds round 2 or moves it behind waiting jobs as configured", async () => {
  for (const hold of [true, false]) {
    await MachineQueue.destroy({ where: {} });
    const a = await job();
    await enqueue(a.id, [{ machine: "MK", round: 1 }, { machine: "MK", round: 2 }]);
    const first = (await claim(a.id)).data.claimed; await sent(first);
    const b = await job(); await enqueue(b.id);
    const result = await release(first.id, { hold_for_next_round: hold });
    assert.equal(result.status, 200);
    assert.equal(result.data.next.print_jobs_id, hold ? a.id : b.id);
    if (hold) assert.equal(result.data.next.round, 2);
  }
});

test("cancel and queue cleanup commit together and block later enqueue", async () => {
  const { job: j, row } = await active(); await sent(row);
  const cancelled = await request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH");
  assert.equal(cancelled.status, 200);
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: j.id } }), 0);
  assert.equal((await enqueue(j.id)).status, 409);
});

test("pending cleanup cannot erase a queue claimed by another caller", async () => {
  const { job: j } = await active();
  assert.equal((await request(`/machine-queue/job/${j.id}?only_unsent=true`, undefined, "DELETE")).status, 409);
  assert.equal(await MachineQueue.count(), 1);
});

test("cancel rollback preserves both the job status and its queue", async () => {
  const { job: j, row } = await active(); await sent(row);
  PrintJob.addHook("beforeUpdate", "fail-cancel", instance => {
    if (instance.status === "Cancel") throw new Error("injected status failure");
  });
  try {
    assert.equal((await request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH")).status, 500);
  } finally { PrintJob.removeHook("beforeUpdate", "fail-cancel"); }
  assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
  assert.ok(await MachineQueue.findByPk(row.id));
});

test("cancel racing begin either prevents IO permission or preserves the uncertain holder", async () => {
  const { job: j, row } = await active();
  const [started, cancelled] = await Promise.all([
    begin(row.id), request(`/job/${j.id}/status`, { status: "Cancel" }, "PATCH"),
  ]);
  assert.equal([started, cancelled].filter(r => r.status === 200).length, 1);
  if (started.status === 200) {
    assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
    assert.ok(await MachineQueue.findByPk(row.id));
  } else {
    assert.equal((await PrintJob.findByPk(j.id)).status, "Cancel");
    assert.equal(await MachineQueue.findByPk(row.id), null);
  }
});

test("two-machine jobs keep successful machine while a definite unsent machine waits", async () => {
  for (const machines of [["UV1", "UV2"], ["MK", "UV2"]]) {
    await MachineQueue.destroy({ where: {} });
    const j = await job(); await enqueue(j.id, machines.map(machine => ({ machine })));
    const first = (await claim(j.id, machines[0])).data.claimed;
    await sent(first);
    const second = (await claim(j.id, machines[1])).data.claimed;
    const token = randomUUID(); await begin(second.id, token);
    assert.equal((await finish(second.id, token, "not_sent")).status, 200);
    assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
    assert.ok((await MachineQueue.findByPk(first.id)).sent_at);
    assert.equal((await MachineQueue.findByPk(second.id)).state, "pending");
  }
});

test("real C# ApiClient agrees with the backend contract and marking rules", async () => {
  const j = await job();
  const execFile = require("node:util").promisify(require("node:child_process").execFile);
  const project = require("node:path").resolve(__dirname, "../../tests/QueueClientRegression/QueueClientRegression.csproj");
  const { stdout } = await execFile("dotnet", ["run", "--project", project, "--no-build"], {
    env: { ...process.env, QUEUE_TEST_API_URL: base, QUEUE_TEST_JOB_ID: String(j.id) },
    timeout: 60000,
  });
  assert.match(stdout, /PASS: real C# API client/);
});

test("a definite failure cannot reset a job while another machine has an unresolved send", async () => {
  const j = await job(); await enqueue(j.id, [{ machine: "UV1" }, { machine: "UV2" }]);
  const first = (await claim(j.id, "UV1")).data.claimed;
  const second = (await claim(j.id, "UV2")).data.claimed;
  const a = randomUUID(), b = randomUUID();
  await begin(first.id, a); await begin(second.id, b);
  assert.equal((await finish(first.id, a, "not_sent")).status, 200);
  assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
  assert.equal((await finish(second.id, b, "not_sent")).status, 200);
  assert.equal((await PrintJob.findByPk(j.id)).status, "Waiting");
});

test("parallel machine finishes preserve independent success and failure results", async () => {
  for (const machines of [["UV1", "UV2"], ["MK", "UV2"]]) {
    for (const outcomes of [["not_sent", "sent"], ["sent", "not_sent"], ["sent", "sent"], ["unknown", "sent"]]) {
      await MachineQueue.destroy({ where: {} });
      const j = await job();
      await enqueue(j.id, machines.map(machine => ({ machine })));
      const rows = await Promise.all(machines.map(async machine => (await claim(j.id, machine)).data.claimed));
      const tokens = rows.map(() => randomUUID());
      const started = await Promise.all(rows.map((r, i) => begin(r.id, tokens[i])));
      assert.ok(started.every(r => r.status === 200));
      const finished = await Promise.all(rows.map((r, i) => finish(r.id, tokens[i], outcomes[i])));
      assert.ok(finished.every(r => r.status === 200));
      assert.equal((await PrintJob.findByPk(j.id)).status, "Process");
      for (let i = 0; i < rows.length; i++) {
        const actual = await MachineQueue.findByPk(rows[i].id);
        assert.equal(Boolean(actual.sent_at), outcomes[i] === "sent");
        assert.equal(actual.state, outcomes[i] === "not_sent" ? "pending" : "active");
        assert.equal(await PrintJobCommand.count({ where: { job_id: j.id, command: machines[i], success: true } }),
          outcomes[i] === "sent" ? 1 : 0);
      }
    }
  }
});

test("simultaneous MK and UV2 release promotes B on both machines without duplicate MK skipping B", async () => {
  const a = await job(), b = await job(), c = await job();
  const items = [{ machine: "MK" }, { machine: "UV2" }];
  await enqueue(a.id, items);
  const mk = (await claim(a.id, "MK")).data.claimed;
  const uv = (await claim(a.id, "UV2")).data.claimed;
  await Promise.all([sent(mk), sent(uv)]);
  await enqueue(b.id, items);
  await enqueue(c.id, items);
  const replies = await Promise.all([
    release(mk.id), release(uv.id, { machine: "UV2" }), release(mk.id),
  ]);
  assert.equal(replies[1].status, 200);
  assert.deepEqual([replies[0].status, replies[2].status].sort(), [200, 409]);
  const activeRows = await MachineQueue.findAll({ where: { state: "active" } });
  assert.equal(activeRows.length, 2);
  assert.ok(activeRows.every(r => r.print_jobs_id === b.id && r.sent_at === null));
  assert.deepEqual(activeRows.map(r => r.machine).sort(), ["MK", "UV2"]);
  assert.equal(await MachineQueue.count({ where: { print_jobs_id: c.id, state: "pending" } }), 2);
});
