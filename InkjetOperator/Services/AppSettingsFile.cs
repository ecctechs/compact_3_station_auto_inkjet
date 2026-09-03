namespace InkjetOperator.Services;

/// <summary>
/// หาที่อยู่ของไฟล์ตั้งค่า และย้ายของเดิมมาให้ครั้งแรกที่เปิด
/// <para>
/// เดิมไฟล์ตั้งค่าวางไว้ข้าง ๆ ตัวโปรแกรม ซึ่งใช้ได้ตอนรันจากโฟลเดอร์ build แต่พอ
/// ติดตั้งด้วย MSI ตัวโปรแกรมไปอยู่ใน Program Files ที่ Windows ไม่ให้เขียนถ้าไม่ได้
/// รันแบบ admin — กด Save แล้วดูเหมือนสำเร็จ แต่ค่าหายหมดตอนเปิดใหม่
/// </para>
/// <para>
/// ย้ายมาไว้ที่ <c>C:\ProgramData\CompactInkjet\</c> ซึ่งเป็นที่มาตรฐานของ Windows
/// สำหรับค่าที่ใช้ร่วมกันทั้งเครื่องและเขียนได้โดยไม่ต้องเป็น admin
/// </para>
/// </summary>
public static class AppSettingsFile
{
    private const string FolderName = "CompactInkjet";

    /// <summary>โฟลเดอร์เก็บไฟล์ตั้งค่าทั้งหมด</summary>
    public static string Folder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        FolderName);

    /// <summary>
    /// ที่อยู่เต็มของไฟล์ตั้งค่าชื่อนั้น
    /// <para>
    /// ถ้ายังไม่มีในที่ใหม่แต่มีไฟล์เดิมอยู่ข้าง ๆ ตัวโปรแกรม จะคัดลอกมาให้ก่อน
    /// เครื่องที่ตั้งค่าไว้แล้วจึงไม่เหมือนโดนล้างค่าตอนอัปเดตโปรแกรม
    /// </para>
    /// <para>
    /// ถ้าสร้างโฟลเดอร์ไม่ได้จริง ๆ จะคืนที่อยู่เดิมข้าง ๆ ตัวโปรแกรม เพื่อให้โปรแกรม
    /// ยังอ่านค่าเดิมได้ ดีกว่าเปิดไม่ขึ้นเลย
    /// </para>
    /// </summary>
    public static string Resolve(string fileName)
    {
        var legacy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

        try
        {
            Directory.CreateDirectory(Folder);
            var target = Path.Combine(Folder, fileName);

            if (!File.Exists(target) && File.Exists(legacy))
                File.Copy(legacy, target);

            return target;
        }
        catch
        {
            return legacy;
        }
    }

    /// <summary>
    /// ลองเขียนไฟล์จริงเพื่อดูว่าบันทึกค่าได้ไหม — คืนข้อความปัญหา หรือ null เมื่อเขียนได้
    /// <para>
    /// เช็คด้วยการเขียนจริง ไม่ใช่ดูสิทธิ์จาก ACL เพราะผลจริงขึ้นกับหลายอย่าง
    /// ทั้งสิทธิ์ นโยบายขององค์กร โปรแกรมป้องกันไวรัส และพื้นที่ดิสก์
    /// </para>
    /// </summary>
    public static string? CheckWritable()
    {
        var probe = Path.Combine(Folder, ".write-test");
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(probe, "");
            File.Delete(probe);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
