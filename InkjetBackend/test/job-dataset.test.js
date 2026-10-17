// ชุดข้อมูลงานทดสอบ — ล้าง / ส่งออก / นำเข้า (POST /system/clearJobData, GET exportJobData, POST importJobData)
//
// รันแบบเดียวกับ queue-reliability.test.js:
//   QUEUE_TEST_DATABASE_URL=postgres://.../queue_reliability_test node --test test/
const { test, before, after } = require("node:test");
const assert = require("node:assert/strict");

// ห้ามใช้ฐานหน้างาน: ต้องระบุฐานทดสอบบน loopback โดยตรงเท่านั้น
const url = new URL(process.env.QUEUE_TEST_DATABASE_URL || "http://missing");
if (!["127.0.0.1", "localhost"].includes(url.hostname) || url.pathname !== "/queue_reliability_test")
  throw new Error("Set QUEUE_TEST_DATABASE_URL to an isolated local queue_reliability_test database");
process.env.POSTGRESQL_HOST = url.href;
const sequelize = require("../database");
const express = require("express");
const app = express();
app.use("/system/importJobData", express.json({ limit: "50mb" }));
app.use(express.json());
app.use(require("../routes/System"));
let server, base;

before(async () => {
  // ตารางทั้งหมดต้องมีอยู่ (สร้างตอน backend เปิดครั้งแรก) — โหลดทุกโมเดลแล้ว sync
  for (const m of ["jobModel", "machineQueueModel", "patternModel", "planRoutingModel", "uvJobModel", "iaiModel"]) {
    try { require(`../model/${m}`); } catch { /* ชื่อไฟล์ต่างรุ่น ไม่เป็นไร */ }
  }
  await sequelize.sync();
  server = app.listen(0, "127.0.0.1");
  await new Promise(resolve => server.once("listening", resolve));
  base = `http://127.0.0.1:${server.address().port}`;
});
after(async () => {
  if (server) await new Promise(resolve => server.close(resolve));
  await sequelize.close();
});

async function call(path, method = "GET", body) {
  const r = await fetch(base + path, {
    method, headers: { "content-type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  return { status: r.status, ...(await r.json()) };
}
const q = async (sql) => (await sequelize.query(sql))[0];

async function seedTwoJobs() {
  await call("/system/clearJobData", "POST");
  for (const lot of ["DS-1", "DS-2"]) {
    const [{ id: job }] = await q(`INSERT INTO print_jobs (barcode_raw, lot_number, order_no, status, attempt, stations_required, created_at, updated_at)
      VALUES ('${lot}', '${lot}', 'ERP-${lot}', 'Process', 0, '[1,2,3,4]', now(), now()) RETURNING id`);
    const [{ id: pat }] = await q(`INSERT INTO patterns (job_id, barcode, is_active) VALUES (${job}, '${lot}', true) RETURNING id`);
    const [{ id: cfg }] = await q(`INSERT INTO inkjet_configs (pattern_id, ordinal, program_number, program_name, suspended) VALUES (${pat}, 1, 295, 'DND', false) RETURNING id`);
    await q(`INSERT INTO text_blocks (inkjet_config_id, block_number, text, x, y, size, scale) VALUES (${cfg}, 1, 'TXT-${lot}', 8, 16, 2, 1)`);
    await q(`INSERT INTO conveyor_speeds (pattern_id, speed1, speed2, speed3) VALUES (${pat}, 25, 45, 50)`);
    await q(`INSERT INTO servo_configs (pattern_id, ordinal, position, post_act, delay, trigger) VALUES (${pat}, 1, 10, 0.5, 0.25, 1)`);
    await q(`INSERT INTO plan_routings (print_jobs_id, lot_no, erp_mfg, marking_method, process_sequence, created_at, updated_at) VALUES (${job}, '${lot}', 'ERP-${lot}', '22', 'In-line', now(), now())`);
    await q(`INSERT INTO uv_job_data (print_jobs_id, machine, table_name, program_name, lot, created_at, updated_at) VALUES (${job}, 'UV2', 'MK067', 'S-${lot}', '${lot}', now(), now())`);
    await q(`INSERT INTO machine_queues (print_jobs_id, machine, round, state, created_at, updated_at) VALUES (${job}, 'MK', 1, 'active', now(), now())`);
    await q(`INSERT INTO print_job_commands (job_id, command, success, sent_at) VALUES (${job}, 'MK', true, now())`);
  }
}

test("ล้างข้อมูลงาน: ทุกตารางของงานว่าง id เริ่มที่ 1 และค่าตั้ง register map ไม่ถูกแตะ", async () => {
  await seedTwoJobs();
  const before = Number((await q("SELECT COUNT(*) n FROM plc_register_maps"))[0].n);

  const r = await call("/system/clearJobData", "POST");
  assert.equal(r.status, 200);
  assert.equal(r.data.removed.print_jobs, 2);
  for (const t of ["print_jobs", "patterns", "inkjet_configs", "text_blocks", "plan_routings", "uv_job_data", "machine_queues", "print_job_commands"])
    assert.equal(Number((await q(`SELECT COUNT(*) n FROM ${t}`))[0].n), 0, t);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM plc_register_maps"))[0].n), before);

  const [{ id }] = await q(`INSERT INTO print_jobs (barcode_raw, status, attempt, stations_required, created_at, updated_at) VALUES ('X', 'Waiting', 0, '[1,2,3,4]', now(), now()) RETURNING id`);
  assert.equal(id, 1);
});

test("ส่งออกแล้วนำเข้า: ได้ข้อมูลงานครบเหมือนเดิม id เดิม ทุกใบเป็นรอเริ่ม ไม่มีคิวค้าง", async () => {
  await seedTwoJobs();
  const exported = await call("/system/exportJobData");
  assert.equal(exported.status, 200);
  const file = JSON.parse(JSON.stringify(exported.data)); // เหมือนบันทึกเป็นไฟล์แล้วเปิดใหม่
  assert.equal(file.format, "compact-inkjet-job-dataset");
  assert.equal(file.tables.print_jobs.length, 2);
  assert.equal(file.tables.machine_queues, undefined); // ไม่ส่งออกร่องรอยการเดินงาน

  // เครื่องปลายทาง: ข้อมูลอื่นอยู่ก่อน → นำเข้าแล้วถูกแทนที่ทั้งหมด
  await call("/system/clearJobData", "POST");
  await q(`INSERT INTO print_jobs (barcode_raw, status, attempt, stations_required, created_at, updated_at) VALUES ('OTHER', 'Waiting', 0, '[1,2,3,4]', now(), now())`);

  const r = await call("/system/importJobData", "POST", file);
  assert.equal(r.status, 200);
  assert.equal(r.data.imported.print_jobs, 2);

  const jobs = await q("SELECT id, lot_number, status, stations_required FROM print_jobs ORDER BY id");
  assert.deepEqual(jobs.map(j => j.lot_number), ["DS-1", "DS-2"]);
  assert.deepEqual(jobs.map(j => j.id), file.tables.print_jobs.map(j => j.id));
  assert.ok(jobs.every(j => j.status === "Waiting"));
  assert.deepEqual(jobs[0].stations_required, [1, 2, 3, 4]);
  assert.equal((await q("SELECT text FROM text_blocks ORDER BY id"))[0].text, "TXT-DS-1");
  assert.equal(Number((await q("SELECT post_act FROM servo_configs ORDER BY id"))[0].post_act), 0.5);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM machine_queues"))[0].n), 0);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM print_job_commands"))[0].n), 0);

  // ลงทะเบียนงานใหม่ต่อได้ id ไม่ชนของที่นำเข้า
  const [{ id }] = await q(`INSERT INTO print_jobs (barcode_raw, status, attempt, stations_required, created_at, updated_at) VALUES ('NEW', 'Waiting', 0, '[1,2,3,4]', now(), now()) RETURNING id`);
  assert.equal(id, Math.max(...jobs.map(j => j.id)) + 1);
});

test("นำเข้าไฟล์ผิดรูปแบบ: ปฏิเสธ และข้อมูลเดิมไม่หาย", async () => {
  await seedTwoJobs();
  const r = await call("/system/importJobData", "POST", { hello: "world" });
  assert.equal(r.status, 400);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM print_jobs"))[0].n), 2);
});

test("นำเข้าที่พังกลางทาง: ย้อนทั้งหมด ข้อมูลเดิมยังอยู่ครบ", async () => {
  await seedTwoJobs();
  const exported = (await call("/system/exportJobData")).data;
  exported.tables.patterns[0].job_id = 999999; // ชี้ไปงานที่ไม่มี → foreign key ล้ม
  const r = await call("/system/importJobData", "POST", exported);
  assert.notEqual(r.status, 200);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM print_jobs"))[0].n), 2);
  assert.equal(Number((await q("SELECT COUNT(*) n FROM machine_queues"))[0].n), 2);
});
