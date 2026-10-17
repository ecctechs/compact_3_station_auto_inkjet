// backend ที่รันอยู่ตรงกับไฟล์ไหม (GET /system/version) — ไม่ต้องใช้ฐานข้อมูล ทดสอบกับโฟลเดอร์ชั่วคราว
const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("fs");
const os = require("os");
const path = require("path");
const { createTracker } = require("../services/codeVersion");

function tempBackend() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "codeversion-"));
  fs.mkdirSync(path.join(root, "routes"));
  fs.mkdirSync(path.join(root, "logs"));
  fs.mkdirSync(path.join(root, "node_modules"));
  fs.writeFileSync(path.join(root, "index.js"), "console.log(1)");
  fs.writeFileSync(path.join(root, "package.json"), "{}");
  fs.writeFileSync(path.join(root, "routes", "a.js"), "module.exports = 1");
  return root;
}

test("ไฟล์ไม่เปลี่ยน: ไม่เก่า", () => {
  const root = tempBackend();
  try {
    const s = createTracker(root).status();
    assert.equal(s.stale, false);
    assert.equal(s.loaded, s.on_disk);
    assert.ok(Date.parse(s.started_at));
  } finally { fs.rmSync(root, { recursive: true, force: true }); }
});

test("แก้ไฟล์โค้ด / เพิ่มไฟล์ใหม่ / package.json เปลี่ยน: ขึ้นว่าเก่า ต้องรีสตาร์ท", () => {
  for (const change of [
    r => fs.writeFileSync(path.join(r, "routes", "a.js"), "module.exports = 2"),
    r => fs.writeFileSync(path.join(r, "routes", "b.js"), "module.exports = 3"),
    r => fs.writeFileSync(path.join(r, "package.json"), "{\"version\":\"2\"}"),
  ]) {
    const root = tempBackend();
    try {
      const tracker = createTracker(root);
      change(root);
      assert.equal(tracker.status().stale, true);
    } finally { fs.rmSync(root, { recursive: true, force: true }); }
  }
});

test("log / node_modules / ไฟล์ที่ไม่ใช่โค้ดเปลี่ยน: ไม่นับ", () => {
  const root = tempBackend();
  try {
    const tracker = createTracker(root);
    fs.writeFileSync(path.join(root, "logs", "backend-1.log"), "x");
    fs.writeFileSync(path.join(root, "node_modules", "x.js"), "x");
    fs.writeFileSync(path.join(root, "notes.txt"), "x");
    assert.equal(tracker.status().stale, false);
  } finally { fs.rmSync(root, { recursive: true, force: true }); }
});
