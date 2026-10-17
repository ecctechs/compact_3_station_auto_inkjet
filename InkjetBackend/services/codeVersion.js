// backend ที่รันอยู่ตรงกับไฟล์ในเครื่องไหม — ใช้ในหน้า System Health
//
// Node อ่านโค้ดครั้งเดียวตอนเปิด อัปเดตไฟล์ backend แล้วถ้าไม่รีสตาร์ท ตัวที่รันอยู่ยังเป็นรุ่นเก่า
// ต่อได้ปกติทุกอย่าง แต่คำสั่งใหม่ตอบ 404 (เคยเจอ: ปุ่มบันทึกชุดข้อมูลงานขึ้น Cannot GET)
// จึงจำลายนิ้วมือของไฟล์โค้ดไว้ตอนเปิด แล้วเทียบกับไฟล์ตอนนี้ทุกครั้งที่ถูกถาม
const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

// ไม่ใช่โค้ดที่ backend โหลด — เปลี่ยนแล้วไม่ต้องรีสตาร์ท
const SKIP = new Set(["node_modules", "logs", "test", ".git"]);

function sourceFiles(root) {
  const out = [];
  const walk = (dir) => {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      if (SKIP.has(entry.name) || entry.name.startsWith(".")) continue;
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) walk(full);
      else if (entry.name.endsWith(".js") || entry.name === "package.json") out.push(full);
    }
  };
  walk(root);
  return out.map(f => path.relative(root, f).split(path.sep).join("/")).sort();
}

function fingerprint(root) {
  const hash = crypto.createHash("sha1");
  for (const file of sourceFiles(root)) {
    hash.update(file).update("\0");
    hash.update(fs.readFileSync(path.join(root, file))).update("\0");
  }
  return hash.digest("hex").slice(0, 12);
}

// อ่านไม่ได้ = null — โมดูลนี้ถูกโหลดตอน backend เปิด ห้ามทำให้ backend เปิดไม่ขึ้นเพราะแค่เช็ครุ่น
function tryFingerprint(root) {
  try { return fingerprint(root); } catch { return null; }
}

function createTracker(root) {
  const loaded = tryFingerprint(root);
  const startedAt = new Date().toISOString();
  return {
    status() {
      // อ่านไม่ได้ชั่วคราว (กำลังคัดลอกทับ) ไม่ฟ้องว่าเก่า
      const onDisk = tryFingerprint(root);
      return {
        started_at: startedAt, loaded, on_disk: onDisk,
        stale: loaded !== null && onDisk !== null && onDisk !== loaded,
      };
    },
  };
}

// จำตอนโมดูลนี้ถูกโหลด = ตอน backend เปิด (routes โหลด controller ตั้งแต่เริ่ม)
const backend = createTracker(path.join(__dirname, ".."));

module.exports = { createTracker, status: () => backend.status() };
