using System.Globalization;
using System.Text;

namespace InkjetOperator.Services;

/// <summary>
/// บันทึกเหตุการณ์ของโปรแกรมลงไฟล์ — วันละไฟล์ที่ C:\ProgramData\CompactInkjet\logs\
/// <para>
/// เดิมโปรแกรมไม่ได้เขียน log เลย พอหน้างานเจอ error ไม่มีอะไรให้ย้อนดู นอกจากถาม
/// พนักงานว่าเห็นอะไร ซึ่งส่วนใหญ่จำข้อความไม่ได้ ไฟล์นี้ไว้ให้คนดูแลเปิดอ่านทีหลัง
/// </para>
/// <para>
/// เขียนไม่ได้ (ดิสก์เต็ม ไม่มีสิทธิ์) ก็เงียบไป — การเขียน log ต้องไม่ทำให้โปรแกรมพังเสียเอง
/// </para>
/// </summary>
public static class AppLog
{
    /// <summary>เก็บย้อนหลังกี่วัน ไฟล์ที่เก่ากว่านี้ถูกลบตอนเปิดโปรแกรม</summary>
    private const int KeepDays = 30;

    private static readonly object Gate = new();

    public static string Folder { get; } = Path.Combine(AppSettingsFile.Folder, "logs");

    public static void Info(string where, string message) => Write("INFO", where, message);

    public static void Warn(string where, string message) => Write("WARN", where, message);

    /// <summary>ผิดพลาดที่ไม่มี exception — เช่นข้อความ error ที่ขึ้นบนจอให้คนหน้างานเห็น</summary>
    public static void Error(string where, string message) => Write("ERROR", where, message);

    /// <summary>บันทึก exception ทั้งก้อน รวม stack trace — ข้อความบนจอไม่มีส่วนนี้</summary>
    public static void Error(string where, Exception ex) => Write("ERROR", where, ex.ToString());

    /// <summary>ลบไฟล์ log ที่เก่าเกิน <see cref="KeepDays"/> วัน — เรียกครั้งเดียวตอนเปิดโปรแกรม</summary>
    public static void Cleanup()
    {
        try
        {
            if (!Directory.Exists(Folder)) return;
            var cutoff = DateTime.Now.AddDays(-KeepDays);
            foreach (var file in Directory.GetFiles(Folder, "app-*.log"))
            {
                if (File.GetLastWriteTime(file) < cutoff) File.Delete(file);
            }
        }
        catch
        {
            // ลบไม่ได้ก็ไม่เป็นไร รอบหน้าค่อยลอง
        }
    }

    private static void Write(string level, string where, string text)
    {
        try
        {
            // ปี ค.ศ. เสมอ — เครื่องหน้างานเป็น th-TH ซึ่งให้ปี พ.ศ. ถ้ามี error ก่อน Program
            // จะเปลี่ยนปฏิทิน ชื่อไฟล์จะกลายเป็น app-2569... ปนกับไฟล์ปกติ
            var now = DateTime.Now;
            var stamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var day = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var line = $"{stamp} [{level}] {where}: {text}{Environment.NewLine}";
            lock (Gate)
            {
                Directory.CreateDirectory(Folder);
                File.AppendAllText(Path.Combine(Folder, $"app-{day}.log"), line, Encoding.UTF8);
            }
        }
        catch
        {
            // ดู summary ของคลาส — เขียนไม่ได้ต้องไม่ลามไปพังส่วนอื่น
        }
    }
}
