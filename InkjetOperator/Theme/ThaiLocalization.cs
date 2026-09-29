namespace InkjetOperator.Theme;

public sealed class ThaiLocalization : AntdUI.ILocalization
{
    private static readonly Dictionary<string, string> Strings = new()
    {
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

        ["OK"] = "ตกลง",
        ["Cancel"] = "ยกเลิก",
        ["NoData"] = "ไม่มีข้อมูล",

        ["Filter"] = "กรอง",
        ["Filter.Search"] = "ค้นหา",
        ["Filter.SelectAll"] = "(ทั้งหมด)",

        ["ItemsPerPage"] = "รายการ/หน้า",
    };

    public string? GetLocalizedString(string key) =>
        Strings.TryGetValue(key, out var value) ? value : null;
}
