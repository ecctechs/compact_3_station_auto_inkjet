using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

/// <summary>
/// สร้าง connection string ของ SQLite ให้เปิดไฟล์บนแชร์โฟลเดอร์ได้
/// <para>
/// ไฟล์ฐานข้อมูลของระบบนี้ (PrintData.db3 · mydatabase.db3 · CPI.db3) อยู่บน
/// เครื่องอื่นแล้วเข้าถึงผ่าน UNC path เช่น <c>\\server\share\PrintData.db3</c>
/// ซึ่ง Microsoft.Data.Sqlite เปิดตรง ๆ ไม่ได้ ต้องเปลี่ยน backslash เป็น
/// forward slash ก่อน
/// </para>
/// <para>
/// เดิมมีการแปลงนี้อยู่ที่ <see cref="CpiWriteService"/> ที่เดียว อีก 6 จุดที่เปิด
/// ฐานข้อมูลเดียวกันยังใช้ path ดิบ ทำให้ตอนตั้งค่าหน้างานแล้วชี้ไปที่แชร์โฟลเดอร์
/// โปรแกรมเปิดไฟล์ไม่ได้ แล้วรายงานว่าไม่พบไฟล์หรือไม่พบตาราง
/// </para>
/// <para>
/// ใช้ <see cref="SqliteConnectionStringBuilder"/> ประกอบสตริง ไม่ต่อเอง เพราะ
/// path จริงมีทั้งเว้นวรรค วงเล็บ และภาษาไทย ซึ่งถ้าต่อเองมีโอกาสหลุด
/// </para>
/// </summary>
public static class SqlitePath
{
    /// <summary>เปิดอ่านอย่างเดียว — ใช้กับทุกที่ที่แค่อ่านข้อมูลออกมา</summary>
    public static string ReadOnly(string path) => Build(path, SqliteOpenMode.ReadOnly);

    /// <summary>เปิดอ่านเขียน ไฟล์ต้องมีอยู่แล้ว</summary>
    public static string ReadWrite(string path) => Build(path, SqliteOpenMode.ReadWrite);

    /// <summary>เปิดอ่านเขียน สร้างไฟล์ใหม่ถ้ายังไม่มี</summary>
    public static string ReadWriteCreate(string path) => Build(path, SqliteOpenMode.ReadWriteCreate);

    private static string Build(string path, SqliteOpenMode mode) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = Normalize(path),
            Mode = mode,
        }.ToString();

    /// <summary>UNC path ต้องใช้ forward slash ส่วน path บนเครื่องตัวเองปล่อยไว้ตามเดิม</summary>
    private static string Normalize(string path)
    {
        var trimmed = (path ?? "").Trim();
        return trimmed.StartsWith(@"\\", StringComparison.Ordinal)
            ? trimmed.Replace('\\', '/')
            : trimmed;
    }
}
