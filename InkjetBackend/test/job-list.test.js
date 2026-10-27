// รายการงาน (GET /job/getAll) — แท็บรายการงานต้องได้งานที่ยังไม่จบครบ ไม่หลุดเพราะงานจบใหม่กว่า
//
// รันแบบเดียวกับ queue-reliability.test.js:
//   QUEUE_TEST_DATABASE_URL=postgres://.../queue_reliability_test node --test test/
const { test, before, after } = require("node:test");
const assert = require("node:assert/strict");

const url = new URL(process.env.QUEUE_TEST_DATABASE_URL || "http://missing");
if (!["127.0.0.1", "localhost"].includes(url.hostname) || url.pathname !== "/queue_reliability_test")
  throw new Error("Set QUEUE_TEST_DATABASE_URL to an isolated local queue_reliability_test database");
process.env.POSTGRESQL_HOST = url.href;
const sequelize = require("../database");
const express = require("express");
const app = express();
app.use(express.json());
app.use(require("../routes/Job"));
let server, base;

before(async () => {
  await sequelize.sync();
  server = app.listen(0, "127.0.0.1");
  await new Promise((resolve) => server.once("listening", resolve));
  base = `http://127.0.0.1:${server.address().port}`;
});
after(async () => {
  if (server) await new Promise((resolve) => server.close(resolve));
  await sequelize.close();
});

const q = async (sql) => (await sequelize.query(sql))[0];
async function list(query) {
  const r = await fetch(`${base}/job/getAll?${query}`);
  return { status: r.status, body: await r.json() };
}

async function seed() {
  await q("TRUNCATE print_jobs RESTART IDENTITY CASCADE");
  await q(`INSERT INTO print_jobs (barcode_raw, lot_number, status, attempt, stations_required, created_at, updated_at)
    VALUES ('OLD-OPEN', 'OLD-OPEN', 'Process', 0, '[1,2,3,4]', now() - interval '5 days', now())`);
  await q(`INSERT INTO print_jobs (barcode_raw, lot_number, status, attempt, stations_required, created_at, updated_at)
    SELECT 'DONE-'||g, 'DONE-'||g, 'Success', 0, '[1,2,3,4]', now() - (g || ' minutes')::interval, now() FROM generate_series(1,120) g`);
  await q(`INSERT INTO print_jobs (barcode_raw, lot_number, status, attempt, stations_required, created_at, updated_at)
    VALUES ('NEW-WAIT', 'NEW-WAIT', 'Waiting', 0, '[1,2,3,4]', now(), now())`);
}

test("งานค้างเก่ากว่างานจบ 120 ใบ: ขอ Waiting,Process ยังได้ครบ เรียงใหม่ก่อน ไม่มีงานจบปน", async () => {
  await seed();
  const r = await list("page=1&limit=1000&status=Waiting,Process");
  assert.equal(r.status, 200);
  const lots = r.body.data.data.map((j) => j.lot_number);
  assert.deepEqual(lots, ["NEW-WAIT", "OLD-OPEN"]);
  assert.ok(Array.isArray(r.body.data.data[0].commands)); // commands มาด้วย ใช้ระบายสีปุ่มจบงาน
});

test("ขอแบบเดิม (100 ล่าสุด ไม่กรองสถานะ) ยังทำงานเหมือนเดิม", async () => {
  await seed();
  const r = await list("page=1&limit=100");
  assert.equal(r.status, 200);
  assert.equal(r.body.data.data.length, 100);
  assert.equal(r.body.data.total, 122);
});

test("สถานะเดียว และสถานะที่ไม่มีจริงถูกปฏิเสธ", async () => {
  await seed();
  const one = await list("page=1&limit=10&status=Waiting");
  assert.deepEqual(one.body.data.data.map((j) => j.lot_number), ["NEW-WAIT"]);
  assert.equal((await list("page=1&limit=10&status=Waiting,Nope")).status, 400);
  assert.equal((await list("page=1&limit=5000")).status, 400);
});

// History กรองตามวันจบ (date_by=finished) — งานรับนานแล้วแต่จบวันนี้ต้องขึ้น (ตกลง 27/10/2026)
test("date_by=finished กรองและเรียงตาม updated_at · ค่าเดิมยังกรองตาม created_at", async () => {
  await q("TRUNCATE print_jobs RESTART IDENTITY CASCADE");
  const ins = (lot, status, created, updated) => q(`INSERT INTO print_jobs
    (barcode_raw, lot_number, status, attempt, stations_required, created_at, updated_at)
    VALUES ('${lot}', '${lot}', '${status}', 0, '[1,2,3,4]', ${created}, ${updated})`);
  await ins("OLD-DONE-TODAY", "Success", "now() - interval '30 days'", "now() - interval '1 hour'");
  await ins("NEW-DONE-TODAY", "Cancel", "now() - interval '3 hours'", "now() - interval '2 hours'");
  await ins("TODAY-DONE-LAST-WEEK", "Success", "now() - interval '8 days'", "now() - interval '7 days'");
  await ins("TODAY-OPEN", "Process", "now() - interval '10 minutes'", "now()");

  const from = new Date(Date.now() - 6 * 3600e3).toISOString();
  const to = new Date(Date.now() + 60e3).toISOString();
  const range = `from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`;

  const fin = await list(`page=1&limit=100&status=Success,Cancel&date_by=finished&${range}`);
  assert.equal(fin.status, 200);
  assert.deepEqual(fin.body.data.data.map((j) => j.lot_number), ["OLD-DONE-TODAY", "NEW-DONE-TODAY"]);

  const created = await list(`page=1&limit=100&status=Success,Cancel&${range}`);
  assert.deepEqual(created.body.data.data.map((j) => j.lot_number), ["NEW-DONE-TODAY"]);

  assert.equal((await list("page=1&limit=10&date_by=nope")).status, 400);
});
