const { DataTypes } = require("sequelize");
const sequelize = require("../database");
const { PrintJob } = require("./jobModel");

// คิวของแต่ละเครื่อง — งานไหนจองเครื่องไหนไว้ และกำลังถือเครื่องอยู่หรือรอคิว
//
// กดเริ่มงานหนึ่งครั้งจะจองทุกเครื่องที่ marking ของงานนั้นต้องใช้ เครื่องที่ว่าง
// จะถูกส่งงานเข้าไปเลย ส่วนเครื่องที่ไม่ว่างก็รออยู่ในคิวจนกว่าจะมีคนกดปุ่มหน้างาน
// ที่เครื่องนั้น ซึ่งแปลว่าพิมพ์ชิ้นเดิมเสร็จแล้ว พร้อมรับโปรแกรมใหม่
//
// ต้องเก็บที่ backend ไม่ใช่ในเครื่องใดเครื่องหนึ่ง เพราะ ST1 กับ ST3 ต้องเห็น
// คิวชุดเดียวกัน (สาย MK/UV ต่ออยู่กับ PC ของ ST1 ที่เดียว แต่คนกดปุ่มอยู่หน้าเครื่อง)
const MachineQueue = sequelize.define(
  "machine_queue",
  {
    id: {
      type: DataTypes.INTEGER,
      autoIncrement: true,
      primaryKey: true,
    },
    print_jobs_id: {
      type: DataTypes.INTEGER,
    },
    // เครื่องที่จองไว้ — "MK" / "UV1" / "UV2" ตรงกับชื่อขั้นตอนใน marking method
    machine: {
      type: DataTypes.STRING,
    },
    // รอบที่เท่าไรของเครื่องนั้นในงานเดียวกัน เริ่มที่ 1
    // มีไว้สำหรับ marking 22 ที่ชิ้นงานเข้าเครื่อง MK สองรอบ
    round: {
      type: DataTypes.INTEGER,
      defaultValue: 1,
    },
    // โปรแกรม UV รุ่นย่อยที่เลือกไว้แล้ว — ว่างแปลว่ายังไม่ได้เลือก หรือไม่ต้องเลือก
    program_name: {
      type: DataTypes.STRING,
    },
    // pending = รอคิว · active = ถือเครื่องอยู่ตอนนี้ · done = ปล่อยเครื่องแล้ว
    //
    // "เครื่องว่าง" คือเครื่องที่ไม่มีแถว active อยู่ ไม่ได้ดูจากสถานะของงาน
    // เพราะงานที่ลืมกดจบจะค้างสถานะไว้แล้วทำให้เครื่องไม่ว่างทั้งกะ
    state: {
      type: DataTypes.STRING,
      defaultValue: "pending",
    },
    // เวลาที่ใช้เรียงลำดับคิว — แยกจาก created_at เพราะต้องขยับได้
    //
    // ทุกรอบของงานเดียวกันถูกสร้างพร้อมกันตอนกดเริ่มงาน created_at จึงเท่ากันหมด
    // ตัวเลือก "ปล่อยเครื่อง" ต้องดันรอบถัดไปไปต่อท้ายคิว ซึ่งขยับ created_at ไม่ได้
    queued_at: {
      type: DataTypes.DATE,
      defaultValue: DataTypes.NOW,
    },
    // เวลาที่ส่งเข้าเครื่องจริง และเวลาที่ปล่อยเครื่อง
    sent_at: {
      type: DataTypes.DATE,
    },
    released_at: {
      type: DataTypes.DATE,
    },
  },
  { timestamps: true, createdAt: "created_at", updatedAt: "updated_at" }
);

PrintJob.hasMany(MachineQueue, {
  foreignKey: "print_jobs_id",
  as: "machine_queue",
  onDelete: "CASCADE",
  hooks: true,
});
MachineQueue.belongsTo(PrintJob, { foreignKey: "print_jobs_id" });

module.exports = { MachineQueue };
