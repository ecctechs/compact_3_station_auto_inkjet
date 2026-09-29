using System.Xml.Linq;

namespace InkjetOperator.Services;

public static class AppSettingsFile
{
    private const string FolderName = "CompactInkjet";

    public static string Folder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        FolderName);

    public static string Resolve(string fileName) // หาไฟล์ค่าตั้งใน ProgramData หรือย้ายจากที่เดิม
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

    private static string? SeedFor(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var seed = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"{name}.default{Path.GetExtension(fileName)}");

        return File.Exists(seed) ? seed : null;
    }

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

    private const string BackupSuffix = ".bak";

    public static void SaveAtomic(string path, Action<string> writeTo) // เขียนไฟล์ใหม่ให้ครบก่อนสลับแทนไฟล์เดิม
    {
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
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            throw;
        }
    }

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

    public static string? CheckWritable()
    {
        Exception? last = null;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var probe = Path.Combine(Folder, $".write-test-{Guid.NewGuid():N}");
            try
            {
                Directory.CreateDirectory(Folder);

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
