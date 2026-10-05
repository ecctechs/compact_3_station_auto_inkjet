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

    /// <summary>ชื่อผลิตภัณฑ์ — ไม่แปลตามภาษา ดูที่ <c>LanguageService</c></summary>
    public const string ProductName = "Compact Inkjet";

    /// <summary>สถานีของเครื่องนี้ — ค่าที่ไม่ใช่ 3 ถือเป็น ST1 ทั้งหมด</summary>
    public static int Current => Level == St3 ? St3 : St1;

    public static bool IsSt3 => Current == St3;

    /// <summary>
    /// ชื่อที่ขึ้นบนแถบหัวและแถบงาน — บอกว่าเครื่องตรงหน้าเป็นเครื่องไหน
    ///
    /// <para>
    /// เลขที่ขึ้นตรงกับ <c>MENU_LEVEL</c> ของเครื่อง — ระดับ 3 คือ Station 3
    /// ตรงกับที่ <see cref="St3"/> กับกฎการผลิตทั้งหมดใช้อยู่ ไม่มีเครื่องไหน
    /// ตั้งเป็น 2 เลย เลข 2 จึงไม่โผล่ที่ไหน
    /// </para>
    /// <para>
    /// ระดับอื่นเช่นโหมดทดสอบขึ้นชื่อผลิตภัณฑ์เปล่า ๆ เพราะไม่ได้ผูกกับเครื่องไหน
    /// </para>
    /// </summary>
    public static string ProgramTitle => Level switch
    {
        0 => $"{ProductName} - Scan barcode",
        1 => $"{ProductName} - Station 1",
        3 => $"{ProductName} - Station 3",
        _ => ProductName,
    };

    /// <summary>
    /// คีย์ใน Setting.config ที่บอกว่าให้โชว์ปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ไหม
    ///
    /// <para>
    /// ปุ่มนั้นเป็นทางสำรองของปุ่มกดหน้างานตอนปุ่มกดใช้ไม่ได้ ตามปกติจึงปิดไว้
    /// เปิดได้ที่หน้า Setting → ตัวเลือกหน้างาน ซึ่งเห็นเฉพาะโหมดทดสอบ
    /// คนคุมเครื่องจึงเปิดเองไม่ได้ ต้องมีคนที่รู้เรื่องเข้าไปเปิดให้
    /// </para>
    /// </summary>
    public const string ManualRemoteSendKey = "ST3_MANUAL_SEND";

    /// <summary>ปุ่มสำรองเปิดอยู่ไหม — ค่าเริ่มต้นคือปิด</summary>
    public static bool ManualRemoteSendEnabled =>
        CustomSettingsManager.Read(ManualRemoteSendKey, "0") == "1";

    /// <summary>คีย์ใน Setting.config ที่คุมตัวกรอง In-line / Off-line ในหน้า Order List</summary>
    public const string ProcessTabsKey = "ORDER_PROCESS_TABS";

    /// <summary>ใครเห็นตัวกรอง In-line / Off-line — ตั้งได้ที่ Setting → ตัวเลือกหน้างาน (โหมดทดสอบ)</summary>
    public enum ProcessTabsMode
    {
        /// <summary>ปิดทั้งหมด ไม่มีเครื่องไหนเห็น</summary>
        Off,

        /// <summary>เห็นเฉพาะโหมดทดสอบ — ค่าเริ่มต้น ไว้ลองก่อนเปิดให้หน้างานใช้</summary>
        DevOnly,

        /// <summary>เห็นที่ ST1 กับ ST3 (และโหมดทดสอบ)</summary>
        Stations,
    }

    /// <summary>ค่าที่ตั้งไว้ — ค่าที่อ่านไม่ออกถือเป็นเฉพาะโหมดทดสอบ ไม่เปิดให้หน้างานเอง</summary>
    public static ProcessTabsMode ProcessTabs =>
        CustomSettingsManager.Read(ProcessTabsKey, "").Trim().ToLowerInvariant() switch
        {
            "off" => ProcessTabsMode.Off,
            "stations" => ProcessTabsMode.Stations,
            _ => ProcessTabsMode.DevOnly,
        };

    /// <summary>ค่าที่เขียนลงไฟล์ของแต่ละตัวเลือก</summary>
    public static string ProcessTabsValue(ProcessTabsMode mode) => mode switch
    {
        ProcessTabsMode.Off => "off",
        ProcessTabsMode.Stations => "stations",
        _ => "dev",
    };

    /// <summary>
    /// เครื่องนี้ควรเห็นตัวกรอง In-line / Off-line ไหม
    ///
    /// <para>
    /// เปิดให้สถานีแล้ว โหมดทดสอบก็ยังเห็นด้วย เพราะโหมดทดสอบเห็นทุกอย่างเสมอ
    /// ส่วนหน้า Scan barcode (ระดับ 0) กับโหมดทดสอบหน้างาน (ระดับ 9) ไม่มีหน้า
    /// Order List ให้ดูอยู่แล้ว
    /// </para>
    /// </summary>
    public static bool ShowProcessTabs => ProcessTabs switch
    {
        ProcessTabsMode.Stations => Level is St1 or St3 || IsDevMode,
        ProcessTabsMode.DevOnly => IsDevMode,
        _ => false,
    };

    /// <summary>
    /// โหมดทดสอบไหม — <c>MENU_LEVEL</c> 99
    ///
    /// ใช้เปิดเครื่องมือที่มีไว้ลองของเท่านั้น เช่นปุ่มส่งตรงเข้าเครื่องกับปุ่มจำลอง
    /// ปุ่มกดหน้างาน ซึ่งเปิดไว้ที่หน้างานจริงแล้วเสี่ยงพิมพ์ซ้ำหรือพิมพ์ข้ามขั้น
    /// </summary>
    public static bool IsDevMode => Level == 99;

    private static int Level
    {
        get
        {
            var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
            return int.TryParse(raw, out var level) ? level : St1;
        }
    }
}
