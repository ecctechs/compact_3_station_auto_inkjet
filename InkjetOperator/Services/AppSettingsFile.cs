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
    /// <para>
    /// เดิมใช้ชื่อไฟล์ตายตัวว่า <c>.write-test</c> แล้วเขียนกับลบเป็นสองจังหวะ
    /// ซึ่งพลาดได้ทั้งที่สิทธิ์ปกติดี เพราะไปชนกับคนอื่นที่ถือไฟล์ชื่อเดียวกันอยู่
    /// ("used by another process") — ชนได้จากสองทาง คือเปิดโปรแกรมพร้อมกันสองตัว
    /// บนเครื่องเดียว หรือโปรแกรมสแกนไวรัสเปิดไฟล์ที่เพิ่งถูกสร้างขึ้นมาอ่านพอดี
    /// ทำให้เตือนผิดว่าบันทึกค่าไม่ได้ ทั้งที่บันทึกได้
    /// </para>
    /// </summary>
    public static string? CheckWritable()
    {
        Exception? last = null;

        // ลองสามครั้ง เผื่อโดนถือค้างชั่วคราวจากตัวสแกนไวรัส
        for (var attempt = 0; attempt < 3; attempt++)
        {
            // ชื่อไม่ซ้ำกันทุกครั้ง จึงไม่มีทางไปชนไฟล์ของโปรเซสอื่น
            var probe = Path.Combine(Folder, $".write-test-{Guid.NewGuid():N}");
            try
            {
                Directory.CreateDirectory(Folder);

                // DeleteOnClose ให้ Windows ลบให้เองตอนปิด handle จึงไม่มีจังหวะ
                // ลบแยกที่จะพลาดได้ ส่วน FileShare.Delete คือยอมให้คนอื่นเปิดค้าง
                // ไว้ระหว่างที่ไฟล์รอถูกลบ ตัวสแกนไวรัสจึงไม่ทำให้ล้ม
                using var fs = new FileStream(
                    probe, FileMode.CreateNew, FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete,
                    bufferSize: 1, FileOptions.DeleteOnClose);
                fs.WriteByte(0);
                fs.Flush();
                return null;
            }
            catch (Exception ex)
            {
                last = ex;
                Thread.Sleep(150);
            }
        }

        return last?.Message;
    }
}
