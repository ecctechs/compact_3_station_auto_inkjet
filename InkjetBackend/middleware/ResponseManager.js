// คำขอที่ตอบ error ลง log ทุกครั้ง (utils/logger.js เขียน console ลง logs/backend-YYYYMMDD.log)
// เดิมไม่มีบันทึก — ลูกค้าบอกว่า "ส่งไม่ได้" แล้ว backend ปฏิเสธ (409 คิวชน / 500 ฐานข้อมูล)
// ย้อนดูไม่ได้ว่าคำขอไหน เพราะอะไร · ไม่บันทึก body ของคำขอ เก็บแค่เส้นทางกับข้อความตอบ
function logError(req, statusCode, data) {
  try {
    const text = typeof data === "string" ? data : JSON.stringify(data);
    const message = (text ?? "").length > 500 ? `${text.slice(0, 500)}…` : text;
    const log = statusCode >= 500 ? console.error : console.warn;
    log(`${req.method} ${req.originalUrl} → ${statusCode} ${message}`);
  } catch {
    // บันทึกไม่ได้ต้องไม่ทำให้ตอบคำขอไม่ได้
  }
}

class ResponseManager {
  static SuccessResponse(req, res, statusCode, data) {
    res.status(statusCode).json({
      statusCode: statusCode,
      data: data,
    });
  }

  static ErrorResponse(req, res, statusCode, data) {
    logError(req, statusCode, data);
    res.status(statusCode).json({
      statusCode: statusCode,
      data: data,
    });
  }

  static CatchResponse(req, res, errorMessage) {
    logError(req, 500, errorMessage);
    res.status(500).json({
      statusCode: 500,
      data: errorMessage,
    });
  }
}

module.exports = ResponseManager;
