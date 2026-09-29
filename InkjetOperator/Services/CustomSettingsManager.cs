namespace InkjetOperator.Services;

public static class CustomSettingsManager
{
    // อยู่ที่ ProgramData ไม่ใช่ข้าง ๆ ตัวโปรแกรม เพราะ Program Files เขียนไม่ได้
    // ถ้าไม่ได้รันแบบ admin — รายละเอียดที่ AppSettingsFile
    //
    // การอ่านผ่านสำเนาในหน่วยความจำ ไม่แตะดิสก์ทุกครั้ง — เหตุผลที่ SettingsStore
    private static readonly SettingsStore _store = new(AppSettingsFile.Resolve("Setting.config"));

    /// <summary>สาเหตุที่บันทึกครั้งล่าสุดไม่สำเร็จ — null = สำเร็จ</summary>
    public static string? LastError { get; private set; }

    /// <summary>ให้ตัวจัดการไฟล์ตั้งค่าตัวอื่นรายงานปัญหามาที่เดียวกัน</summary>
    internal static void ReportWriteError(string message) => LastError = message;

    public static string Read(string key, string defaultValue = "") => _store.Read(key, defaultValue);

    /// <summary>คืน false เมื่อบันทึกไม่สำเร็จ ดูสาเหตุได้ที่ <see cref="LastError"/></summary>
    public static bool Write(string key, string value)
    {
        // เดิมกลืน error ทิ้งเงียบ ๆ ผู้ใช้เลยไม่รู้ว่ากด Save แล้วไม่ได้บันทึกจริง
        LastError = _store.Write(key, value);
        return LastError == null;
    }
}
