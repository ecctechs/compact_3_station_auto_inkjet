namespace InkjetOperator.Theme;

/// <summary>
/// ข้อความในตัว AntdUI ที่ default เป็นภาษาจีน — ปฏิทิน ตัวกรองตาราง ปุ่มยืนยัน
/// ตัวไหนไม่ได้ใส่ไว้ AntdUI จะใช้ค่า default ของมันเอง (คืน null = ใช้ค่าเดิม)
/// <para>
/// เรื่องปี: <c>ID</c> คือ culture ที่ AntdUI เอาไปสร้าง <c>CultureInfo</c> สำหรับปฏิทิน
/// ตั้งเป็น "th-TH" จะได้ชื่อเดือนไทย (ส.ค.) แต่ .NET ผูก th-TH ไว้กับปฏิทินพุทธ
/// ปีในปฏิทินจะกลายเป็น 2569 ไม่ตรงกับคอลัมน์ Start/End ที่เป็น ค.ศ.
/// จึงใช้ culture แบบ Gregorian แล้วแสดงเดือนเป็นตัวเลขแทน ให้ทั้งหน้าจอเป็นปีเดียวกัน
/// </para>
/// </summary>
public sealed class ThaiLocalization : AntdUI.ILocalization
{
    private static readonly Dictionary<string, string> Strings = new()
    {
        // ปฏิทิน — หัวตารางเป็น "ปี เดือน" เสมอ ไม่ว่าจะ culture ไหน
        ["ID"] = "en-US",
        ["YearFormat"] = "yyyy",
        ["MonthFormat"] = "MM",

        ["Mon"] = "จ",
        ["Tue"] = "อ",
        ["Wed"] = "พ",
        ["Thu"] = "พฤ",
        ["Fri"] = "ศ",
        ["Sat"] = "ส",
        ["Sun"] = "อา",

        ["ToDay"] = "วันนี้",
        ["Now"] = "ตอนนี้",

        // ปุ่มและข้อความรวมของ AntdUI
        ["OK"] = "ตกลง",
        ["Cancel"] = "ยกเลิก",
        ["NoData"] = "ไม่มีข้อมูล",

        // ตัวกรองบนหัวตาราง
        ["Filter"] = "กรอง",
        ["Filter.Search"] = "ค้นหา",
        ["Filter.SelectAll"] = "(ทั้งหมด)",

        ["ItemsPerPage"] = "รายการ/หน้า",
    };

    public string? GetLocalizedString(string key) =>
        Strings.TryGetValue(key, out var value) ? value : null;
}
