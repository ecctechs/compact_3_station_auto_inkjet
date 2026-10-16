// ต้องมาก่อนทุกอย่าง จะได้เก็บข้อความตั้งแต่บรรทัดแรก — ดูเหตุผลใน utils/logger.js
require("./utils/logger");

// error ที่ไม่มีใครจับ
//
// Node 15 ขึ้นไปปิด process ทันทีเมื่อ promise ล้มโดยไม่มีใครรอ เดิม backend จึงดับเงียบ ๆ
// แล้วทั้งสามสถานีต่อไม่ได้ ตอนนี้แค่บันทึกไว้แล้วทำงานต่อ เพราะ error แบบนี้ส่วนใหญ่
// จบอยู่ในคำขอเดียว ไม่ได้ทำให้ทั้งระบบเสีย
process.on("unhandledRejection", (reason) => {
  console.error("unhandledRejection:", reason);
});

// ส่วน exception ที่หลุดออกมาถึงตรงนี้ สถานะภายในอาจพังไปแล้ว ทำงานต่อไม่ปลอดภัย
// บันทึกแล้วปิดตัวเอง — โปรแกรม ST1 จะเห็นว่าพอร์ตหายแล้วเปิด backend ให้ใหม่
process.on("uncaughtException", (err) => {
  console.error("uncaughtException — ปิด backend ให้เปิดใหม่:", err);
  process.exit(1);
});

const express = require("express");
const sequelize = require("./database");
const cors = require("cors");
const bodyParser = require("body-parser");

// Load all models (registers associations before sync)
require("./model/patternModel");
require("./model/jobModel");
require("./model/plcModel");
require("./model/uvJobDataModel");
require("./model/planRoutingModel");
require("./model/lastSentJobModel");
require("./model/iaiSettingModel");
require("./model/machineQueueModel");

const runMigrations = require("./migrations");

// เตรียมฐานข้อมูล — ลองใหม่จนกว่าจะต่อได้ ไม่ล้ม
//
// เปิดเครื่องตอนเช้า PostgreSQL อาจยังขึ้นไม่เสร็จตอนที่โปรแกรม ST1 เปิด backend
// เดิม sync() ล้มแล้วไม่มีใครจับ backend จึงดับไปตั้งแต่ต้นวัน
// ระหว่างที่ยังต่อไม่ได้ API ยังเปิดรับอยู่ คำขอจะได้ error กลับไปแทนการรอค้าง
const DB_RETRY_MS = 5000;

async function initDatabase() {
  for (let attempt = 1; ; attempt++) {
    try {
      await sequelize.authenticate();
      await sequelize.sync();
      await runMigrations();
      console.log(`database ready (ครั้งที่ ${attempt})`);
      return;
    } catch (err) {
      // บันทึก 3 ครั้งแรก แล้วนาทีละครั้ง — ฐานข้อมูลหายนานจะได้ไม่ท่วมไฟล์ log
      if (attempt <= 3 || attempt % 12 === 0)
        console.error(`ต่อฐานข้อมูลไม่ได้ (ครั้งที่ ${attempt}) จะลองใหม่ทุก ${DB_RETRY_MS / 1000} วินาที: ${err.message}`);
      await new Promise((resolve) => setTimeout(resolve, DB_RETRY_MS));
    }
  }
}

initDatabase();

const patternRoute = require("./routes/Pattern");
const jobRoute = require("./routes/Job");
const uvJobRoute = require("./routes/UvJob");
const planRoutingRoute = require("./routes/PlanRouting");
const systemRoute = require("./routes/System");
const plcRoute = require("./routes/Plc");
const iaiRoute = require("./routes/Iai");
const machineQueueRoute = require("./routes/MachineQueue");

const app = express();
app.use(bodyParser.json());
app.use(express.json());
app.use(cors());

app.use(patternRoute);
app.use(jobRoute);
app.use(uvJobRoute);
app.use(planRoutingRoute);
app.use(systemRoute);
app.use(plcRoute);
app.use(iaiRoute);
app.use(machineQueueRoute);

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`server is running on port ${PORT}`));
