namespace InkjetOperator.Services;

/// <summary>
/// ตัวเลือก "เปิดหน้า Scan Barcode ที่ ST1" — ตั้งที่ Setting → ตัวเลือกหน้างาน (โหมด Dev)
///
/// <para>
/// ปกติหน้า Scan Barcode เป็นของสถานีสแกน (MENU_LEVEL 0) เท่านั้น ที่ ST1 ไม่มีแท็บนี้
/// เปิดตัวเลือกนี้แล้ว ST1 จะมีแท็บ Scan Barcode เพิ่มมาให้ใช้ด้วย
/// </para>
/// <para>
/// ค่าเริ่มต้นปิด เพราะเพิ่มแท็บที่หน้าจอคนหน้างานเป็นการเปลี่ยนสิ่งที่เขาเห็นทุกวัน
/// ควรเป็นการตัดสินใจของคนตั้งค่า ไม่ใช่โผล่มาเอง
/// </para>
/// </summary>
public static class ScanTabAtSt1
{
    /// <summary>คีย์ใน Setting.config — "1" = เปิด</summary>
    public const string Key = "SCAN_TAB_AT_ST1";

    public static bool Enabled => CustomSettingsManager.Read(Key, "0").Trim() == "1";
}
