const ResponseManager = require("../middleware/ResponseManager");
const sequelize = require("../database");

/**
 * Size conversion dict — ported from rs232_connector.py lines 8-22.
 * Maps logical size (0-22 range) to device-specific encoding.
 * Served to C# app so it doesn't need to hardcode this.
 */
const SIZE_CONVERSION = {
  "1": "0",
  "2": "1",
  "3": "17",
  "4": "3",
  "5": "18",
  "6": "20",
  "7": "5",
  "8": "6",
  "9": "7",
  "10": "8",
  "11": "9",
  "12": "10",
  "13": "11",
};

/**
 * Bilingual translations — ported from miscellaneous.py LangDict (lines 24-58).
 */
const TRANSLATIONS = {
  ENG: "\u0E44\u0E17\u0E22",
  Auto: "\u0E2D\u0E31\u0E15\u0E42\u0E19\u0E21\u0E31\u0E15\u0E34",
  Manual: "\u0E04\u0E27\u0E1A\u0E04\u0E38\u0E21\u0E40\u0E2D\u0E07",
  "Pattern No: ":
    "\u0E25\u0E33\u0E14\u0E31\u0E1A\u0E42\u0E1B\u0E23\u0E41\u0E01\u0E23\u0E21:",
  Enter: "\u0E14\u0E36\u0E07\u0E02\u0E49\u0E2D\u0E21\u0E39\u0E25",
  SEND: "\u0E2A\u0E48\u0E07",
  Browse: "\u0E40\u0E25\u0E37\u0E2D\u0E01\u0E44\u0E1F\u0E25\u0E4C",
  TEXT: "\u0E02\u0E49\u0E2D\u0E04\u0E27\u0E32\u0E21",
  TYPE: "\u0E1B\u0E23\u0E30\u0E40\u0E20\u0E17\u0E40\u0E2B\u0E25\u0E47\u0E01",
  SERVO:
    "\u0E41\u0E01\u0E19\u0E2B\u0E31\u0E27\u0E2D\u0E34\u0E07\u0E04\u0E4C\u0E40\u0E08\u0E17",
  "Position:": "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07",
  "Post Act.:":
    "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07\u0E41\u0E01\u0E19",
  "Servo: Post Act.:":
    "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07\u0E2B\u0E31\u0E27\u0E1E\u0E34\u0E21\u0E1E\u0E4C:",
  "Delay:": "\u0E2B\u0E19\u0E48\u0E27\u0E07\u0E40\u0E27\u0E25\u0E32",
  "Trigger Delay:":
    "\u0E2B\u0E19\u0E48\u0E27\u0E07\u0E23\u0E30\u0E22\u0E30\u0E17\u0E32\u0E07",
  Conveyor: "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19",
  SPEED: "\u0E04\u0E27\u0E32\u0E21\u0E40\u0E23\u0E47\u0E27",
  "Conveyor 1:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 1:",
  "Conveyor 2:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 2:",
  "Conveyor 3:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 3:",
  Size: "\u0E02\u0E19\u0E32\u0E14",
  Scale: "\u0E2A\u0E40\u0E01\u0E25",
  "Block 1:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 1:",
  "Block 2:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 2:",
  "Block 3:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 3:",
  "Block 4:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 4:",
  "Block 5:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 5:",
  Width: "\u0E04\u0E27\u0E32\u0E21\u0E2A\u0E39\u0E07",
  Height: "\u0E04\u0E27\u0E32\u0E21\u0E01\u0E27\u0E49\u0E32\u0E07",
  "No File Selected.":
    "\u0E22\u0E31\u0E07\u0E44\u0E21\u0E48\u0E44\u0E14\u0E49\u0E40\u0E25\u0E37\u0E2D\u0E01\u0E44\u0E1F\u0E25\u0E4C",
  PROGRAM: "\u0E42\u0E1B\u0E23\u0E41\u0E01\u0E23\u0E21",
  SWAP: "\u0E2A\u0E25\u0E31\u0E1A",
};

class SystemController {
  /**
   * POST /system/resetRuntime
   *
   * ล้างร่องรอยการเดินงานทั้งหมด ให้ทุกใบกลับไปเป็นรอเริ่ม — เครื่องมือสำหรับทดสอบ
   *
   * ลบคิวเครื่องทุกแถว ลบประวัติคำสั่งที่ส่งเข้าเครื่องทุกแถว และตั้งสถานะทุกงาน
   * กลับเป็น Waiting พร้อมล้างธงคำขอที่ ST3 ฝากไว้
   *
   * ธงคำขอต้องล้างด้วย ไม่งั้นพอรีเซ็ตเสร็จ ST1 จะเห็นใบที่ค้างธงอยู่แล้วยิงเข้า
   * เครื่องทันทีโดยไม่มีใครกด ซึ่งไม่ใช่สภาพเริ่มต้น
   *
   * ทำในทรานแซกชันเดียว ล้มกลางทางแล้วต้องไม่เหลือสภาพล้างไปได้ครึ่งเดียว
   *
   * ไม่แตะข้อมูลของงาน — pattern, uv_job_data, plan_routing, iai อยู่ครบเหมือนเดิม
   * ลบเฉพาะสิ่งที่บอกว่า "เดินไปถึงไหนแล้ว"
   */
  static async resetRuntime(req, res) {
    const t = await sequelize.transaction();
    try {
      // sequelize.query คืน [ผลลัพธ์, ข้อมูลกำกับ] จำนวนแถวที่โดนอยู่ในตัวหลัง
      // ไม่ใช่ตัวแรก หยิบผิดตัวจะรายงานว่าลบ 0 แถวทั้งที่ลบไปจริง
      const [, queue] = await sequelize.query("DELETE FROM machine_queues", {
        transaction: t,
      });

      const [, commands] = await sequelize.query("DELETE FROM print_job_commands", {
        transaction: t,
      });

      const [, meta] = await sequelize.query(
        `UPDATE print_jobs
            SET status = 'Waiting',
                remote_start = '0',
                remote_program = NULL,
                remote_step = NULL,
                remote_error = NULL`,
        { transaction: t }
      );

      await t.commit();

      return ResponseManager.SuccessResponse(req, res, 200, {
        jobs: meta?.rowCount ?? 0,
        queue_removed: queue?.rowCount ?? 0,
        commands_removed: commands?.rowCount ?? 0,
      });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async ping(req, res) {
    return ResponseManager.SuccessResponse(req, res, 200, { status: "ok" });
  }

  static async getSizeMap(req, res) {
    return ResponseManager.SuccessResponse(req, res, 200, SIZE_CONVERSION);
  }

  static async getTranslations(req, res) {
    return ResponseManager.SuccessResponse(req, res, 200, TRANSLATIONS);
  }
}

module.exports = SystemController;
