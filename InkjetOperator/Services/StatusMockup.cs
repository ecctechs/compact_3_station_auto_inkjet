namespace InkjetOperator.Services;

/// <summary>
/// โหมดให้สถานะการเชื่อมต่อขึ้นสำเร็จทั้งหมด — สำหรับถ่ายรูปทำคู่มือบนเครื่องที่ไม่ได้ต่อเครื่องจริง
///
/// <para>
/// เปิด/ปิดที่หน้า ตัวเลือกหน้างาน ซึ่งเห็นเฉพาะโหมด Dev แต่เมื่อเปิดแล้วมีผลกับทุกหน้า
/// ทุกสถานีบนเครื่องนี้ (Scan Barcode · ST1 · ST3) จะได้สลับ MENU_LEVEL ไปถ่ายรูป
/// หน้าของแต่ละสถานีได้ ค่าเก็บใน Setting.config จึงค้างอยู่จนกว่าจะกลับมาปิด
/// </para>
/// <para>
/// แตะเฉพาะ "การแสดงผล" ของการเช็คการเชื่อมต่อ — ไฟสถานะ ป้ายผลตรวจ และบรรทัดในกล่อง
/// ผลการทำงาน ไม่แตะการส่งงาน การเขียนค่าเข้า PLC หรือการสั่งเครื่องจริงใด ๆ ทั้งสิ้น
/// ของพวกนั้นยังต่อจริงและรายงานผลจริงเสมอ
/// </para>
/// </summary>
public static class StatusMockup
{
    /// <summary>คีย์ใน Setting.config — "1" = เปิด</summary>
    public const string Key = "MOCKUP_STATUS";

    public static bool Enabled => CustomSettingsManager.Read(Key, "0").Trim() == "1";
}
