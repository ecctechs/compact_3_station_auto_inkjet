// เขียนทุกอย่างที่ออก console ลงไฟล์ด้วย — วันละไฟล์ที่ InkjetBackend/logs/backend-YYYYMMDD.log
//
// backend ถูกโปรแกรม ST1 เปิดแบบไม่มีหน้าต่าง (CreateNoWindow) ข้อความใน console จึงหายไปหมด
// เดิมพอ backend ล้มไม่มีอะไรบอกสาเหตุเลย ไฟล์นี้ต้อง require ก่อนทุกอย่างใน index.js
//
// เขียนแบบ sync โดยตั้งใจ — ตอน uncaughtException ต้องลงไฟล์ให้เสร็จก่อน process.exit
// ปริมาณ log ของ backend นี้น้อย จึงไม่กระทบความเร็ว

const fs = require("fs");
const path = require("path");
const util = require("util");

const LOG_DIR = path.join(__dirname, "..", "logs");
const KEEP_DAYS = 30;

const pad = (n, w = 2) => String(n).padStart(w, "0");

function stamp(d) {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ` +
    `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}.${pad(d.getMilliseconds(), 3)}`;
}

function fileFor(d) {
  return path.join(LOG_DIR, `backend-${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}.log`);
}

function write(level, args) {
  try {
    const now = new Date();
    fs.mkdirSync(LOG_DIR, { recursive: true });
    fs.appendFileSync(fileFor(now), `${stamp(now)} [${level}] ${util.format(...args)}\n`);
  } catch {
    // เขียนไม่ได้ (ดิสก์เต็ม ไม่มีสิทธิ์) ก็ปล่อยไป — log ต้องไม่ทำให้ backend ล้มเสียเอง
  }
}

function cleanup() {
  try {
    const cutoff = Date.now() - KEEP_DAYS * 24 * 60 * 60 * 1000;
    for (const name of fs.readdirSync(LOG_DIR)) {
      if (!/^backend-\d{8}\.log$/.test(name)) continue;
      const file = path.join(LOG_DIR, name);
      if (fs.statSync(file).mtimeMs < cutoff) fs.unlinkSync(file);
    }
  } catch {
    // ยังไม่มีโฟลเดอร์ หรือลบไม่ได้ — รอบหน้าค่อยลอง
  }
}

for (const [method, level] of [["log", "INFO"], ["info", "INFO"], ["warn", "WARN"], ["error", "ERROR"]]) {
  const original = console[method].bind(console);
  console[method] = (...args) => {
    original(...args);
    write(level, args);
  };
}

cleanup();

module.exports = { LOG_DIR };
