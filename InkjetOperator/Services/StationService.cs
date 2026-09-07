namespace InkjetOperator.Services;

/// <summary>
/// เครื่องนี้ทำหน้าที่เป็นสถานีไหนในสายการผลิต
///
/// อ่านจาก <c>MENU_LEVEL</c> ใน Setting.config ตัวเดียวกับที่คุมว่าเห็นเมนูอะไร
/// เพราะหน้างานตั้งค่าไว้แล้วเครื่องละค่า: ระดับ 3 คือเครื่องของ ST3 นอกนั้นคือ ST1
/// <para>
/// ข้อควรระวัง: ค่านี้จึงคุมสองเรื่องพร้อมกัน ทั้งสิทธิ์เมนูและกฎการผลิต
/// เปลี่ยน MENU_LEVEL ของเครื่องไหนต้องนึกถึงทั้งสองด้าน
/// </para>
/// </summary>
public static class StationService
{
    public const int St1 = 1;
    public const int St3 = 3;

    /// <summary>สถานีของเครื่องนี้ — ค่าที่ไม่ใช่ 3 ถือเป็น ST1 ทั้งหมด</summary>
    public static int Current
    {
        get
        {
            var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
            return int.TryParse(raw, out var level) && level == St3 ? St3 : St1;
        }
    }

    public static bool IsSt3 => Current == St3;
}
