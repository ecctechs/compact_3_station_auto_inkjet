using System.Xml.Linq;

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
    /// ถ้ายังไม่มีในที่ใหม่ จะหาของมาตั้งต้นให้สองทาง ไล่ตามลำดับ — ไฟล์เดิมที่อยู่
    /// ข้าง ๆ ตัวโปรแกรม (เครื่องที่ตั้งค่าไว้แล้วจึงไม่เหมือนโดนล้างค่าตอนอัปเดต)
    /// แล้วจึงเป็นไฟล์ค่าตั้งต้นที่ไปกับตัวโปรแกรม เช่น <c>Setting.default.config</c>
    /// (เครื่องที่ติดตั้งใหม่จะได้ค่าตั้งต้นชุดเดียวกันทุกเครื่อง)
    /// </para>
    /// <para>
    /// ถ้าสร้างโฟลเดอร์ไม่ได้จริง ๆ จะคืนที่อยู่เดิมข้าง ๆ ตัวโปรแกรม เพื่อให้โปรแกรม
    /// ยังอ่านค่าเดิมได้ ดีกว่าเปิดไม่ขึ้นเลย
    /// </para>
    /// </summary>
    public static string Resolve(string fileName)
    {
        var legacy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName); // เก็บทางไปไฟล์ตั้งค่ารุ่นเดิมข้างโปรแกรม

        try
        {
            Directory.CreateDirectory(Folder);
            var target = Path.Combine(Folder, fileName); // ใช้ไฟล์นี้ร่วมกันหลัง Build หรืออัปเดตโปรแกรม

            RestoreIfEmpty(target); // ถ้าไฟล์ว่าง ให้ลองกู้จาก .bak ก่อนอ่าน

            if (!File.Exists(target))
            {
                if (File.Exists(legacy)) File.Copy(legacy, target); // เครื่องที่ยังไม่ย้ายค่า ให้ใช้ไฟล์เดิมเป็นจุดตั้งต้น
                else if (SeedFor(fileName) is string seed) File.Copy(seed, target); // เครื่องติดตั้งใหม่ใช้ไฟล์ default ที่มากับโปรแกรม
            }

            return target;
        }
        catch
        {
            return legacy;
        }
    }

    /// <summary>
    /// ไฟล์ค่าตั้งต้นที่ไปกับตัวโปรแกรม — <c>Setting.config</c> คู่กับ
    /// <c>Setting.default.config</c> คืน null เมื่อไฟล์นั้นไม่มีค่าตั้งต้นมาให้
    /// </summary>
    private static string? SeedFor(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var seed = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"{name}.default{Path.GetExtension(fileName)}");

        return File.Exists(seed) ? seed : null;
    }

    /// <summary>
    /// สร้างไฟล์ตั้งค่าเปล่าถ้ายังไม่มี เพื่อให้บันทึกค่าลงไปได้ตั้งแต่ครั้งแรก
    ///
    /// <para>
    /// ตัวอ่านและตัวเขียนเปิดไฟล์ด้วย <c>XDocument.Load</c> ซึ่งไฟล์ที่ไม่มีอยู่จะ
    /// โยน error ออกมา ผลคือทุกค่าเป็นค่าเริ่มต้นและกด Save ก็ไม่ผ่าน — เครื่องที่
    /// ติดตั้งใหม่แล้วไม่มีทั้งไฟล์เดิมและไฟล์ค่าตั้งต้นจะเจอกรณีนี้
    /// </para>
    /// </summary>
    public static void EnsureAppSettingsFile(string path)
    {
        if (File.Exists(path)) return;

        try
        {
            SaveAtomic(path, temp => new XDocument(
                new XElement("configuration", new XElement("appSettings"))).Save(temp));
        }
        catch { /* เขียนไม่ได้ก็ให้ตัวเรียกรายงานปัญหาไปตามทางเดิม */ }
    }

    /// <summary>ท้ายชื่อไฟล์สำรองที่ <see cref="SaveAtomic"/> ทิ้งไว้ให้ทุกครั้งที่บันทึก</summary>
    private const string BackupSuffix = ".bak";

    /// <summary>
    /// บันทึกไฟล์โดยไม่มีจังหวะที่ไฟล์เหลือครึ่ง ๆ
    ///
    /// <para>
    /// เดิมเขียนทับไฟล์เดิมตรง ๆ ซึ่งล้างของเก่าทิ้งก่อนแล้วค่อยเขียนของใหม่ลงไป
    /// ถ้าโปรแกรมถูกปิดหรือไฟดับในช่วงนั้น ไฟล์ตั้งค่าจะเหลือครึ่งเดียวจนอ่านไม่ออก
    /// ผลคือทุกหน้ากลับไปโชว์ค่าเริ่มต้น และกด Save รอบต่อไปก็ไม่ผ่านเพราะอ่านไฟล์
    /// เดิมไม่ได้ จังหวะที่เจอได้บ่อยสุดคือกด Rebuild ใน Visual Studio ซึ่งฆ่า
    /// โปรแกรมที่รันอยู่ทันที
    /// </para>
    /// <para>
    /// เขียนลงไฟล์ข้าง ๆ ให้เสร็จก่อน แล้วสลับเข้าที่ด้วย <c>File.Replace</c> ซึ่ง
    /// NTFS ทำให้เป็นจังหวะเดียว คนอ่านจึงเห็นไฟล์เก่าครบ หรือไฟล์ใหม่ครบ
    /// ไม่มีทางเห็นไฟล์ครึ่ง ๆ และของเดิมถูกเก็บไว้เป็น <c>.bak</c> ให้กู้ได้อีกชั้น
    /// </para>
    /// </summary>
    /// <param name="path">ไฟล์ปลายทางจริง</param>
    /// <param name="writeTo">เขียนเนื้อหาลงที่อยู่ที่ส่งให้ — ไฟล์ชั่วคราว ไม่ใช่ปลายทาง</param>
    public static void SaveAtomic(string path, Action<string> writeTo)
    {
        // ชื่อไม่ซ้ำกันทุกครั้ง จึงไม่ชนกับโปรแกรมตัวอื่นที่บันทึกพร้อมกัน
        var temp = $"{path}.{Guid.NewGuid():N}.tmp"; // เขียนไฟล์ใหม่ให้ครบก่อนแทนของเดิม

        try
        {
            writeTo(temp); // บันทึกเนื้อหาลงไฟล์ชั่วคราวของรอบนี้

            if (File.Exists(path))
                File.Replace(temp, path, path + BackupSuffix, ignoreMetadataErrors: true); // สลับไฟล์พร้อมเก็บชุดก่อนหน้าเป็น .bak
            else
                File.Move(temp, path); // ครั้งแรกยังไม่มีไฟล์เดิมให้แทน
        }
        catch
        {
            // ไฟล์ชั่วคราวที่ค้างอยู่ไม่มีใครใช้ต่อ เก็บกวาดก่อนโยนต่อให้ผู้เรียก
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            throw;
        }
    }

    /// <summary>
    /// ไฟล์ว่างเปล่าแต่มีไฟล์สำรองที่มีเนื้อหา — เอาไฟล์สำรองกลับมาใช้
    ///
    /// <para>
    /// กู้ความเสียหายที่เกิดขึ้นไปแล้วก่อนจะมี <see cref="SaveAtomic"/> ไฟล์ขนาด
    /// 0 ไบต์อ่านเป็น XML ไม่ได้ ซึ่งทำให้ทุกค่ากลายเป็นค่าเริ่มต้นทั้งที่ของจริง
    /// ยังอยู่ในไฟล์สำรอง
    /// </para>
    /// </summary>
    private static void RestoreIfEmpty(string target)
    {
        var backup = target + BackupSuffix;

        try
        {
            if (!File.Exists(target) || new FileInfo(target).Length > 0) return;
            if (!File.Exists(backup) || new FileInfo(backup).Length == 0) return;

            File.Copy(backup, target, overwrite: true); // กู้ไฟล์ตั้งค่าที่เป็นศูนย์ไบต์จากชุดสำรอง
        }
        catch { /* กู้ไม่ได้ก็ปล่อยไปตามเดิม ไม่ให้เปิดโปรแกรมไม่ขึ้น */ }
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
