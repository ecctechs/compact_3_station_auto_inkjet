const ensurePlanRoutingCascade = require("./ensurePlanRoutingCascade");
const ensureUvJobDataColumns = require("./ensureUvJobDataColumns");
const ensureIaiPerJob = require("./ensureIaiPerJob");

// รันหลัง sequelize.sync() — ปรับ schema ที่ sync() แก้ให้ไม่ได้ (constraint / rename / add column)
// ทุกตัวต้อง idempotent
module.exports = async function runMigrations() {
  await ensurePlanRoutingCascade();
  await ensureUvJobDataColumns();
  await ensureIaiPerJob();
};
