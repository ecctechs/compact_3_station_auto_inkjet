using System.Xml.Serialization;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// Loads/saves the local transform patterns (patterns.xml).
/// Holds one process-wide list, mirroring the old Linx PatternStore behaviour.
///
/// <para>
/// ไฟล์อยู่ที่ <c>C:\ProgramData\CompactInkjet\</c> ที่เดียวกับไฟล์ตั้งค่าอื่น ไม่ใช่
/// ข้าง ๆ ตัวโปรแกรม — เดิมเก็บข้าง ๆ <c>.exe</c> ซึ่งเป็นโฟลเดอร์ที่ตัวติดตั้งเป็น
/// เจ้าของ ตัวติดตั้งตั้ง <c>RemovePreviousVersions</c> ไว้ การติดตั้งทับจึงถอนของเก่า
/// ออกก่อน แล้วลง <c>patterns.xml</c> ชุด default กลับมา pattern ที่หน้างานแก้ไว้
/// จึงหายทุกครั้งที่อัปเดตโปรแกรม
/// </para>
/// <para>
/// ครั้งแรกที่เปิดหลังอัปเดต ไฟล์เดิมที่อยู่ข้าง ๆ <c>.exe</c> จะถูกย้ายมาให้เอง
/// (<see cref="AppSettingsFile.Resolve"/>) เครื่องที่แก้ pattern ไว้แล้วจึงไม่เหมือน
/// ถูกล้างค่า
/// </para>
/// </summary>
public static class PatternStore
{
    /// <summary>ที่อยู่ของไฟล์ — ที่เดียวทั้งโปรแกรม ไม่ให้แต่ละหน้าคิดเองแล้วไม่ตรงกัน</summary>
    public static string FilePath { get; } = AppSettingsFile.Resolve("patterns.xml");

    /// <summary>The single in-memory list shared across the app.</summary>
    public static List<Pattern> Patterns { get; private set; } = new();

    /// <summary>Overwrite the file with the current list via XmlSerializer.</summary>
    public static void Save()
    {
        var serializer = new XmlSerializer(typeof(List<Pattern>));

        // เขียนแบบไม่ทิ้งไฟล์ครึ่ง ๆ — เหตุผลที่ AppSettingsFile.SaveAtomic
        AppSettingsFile.SaveAtomic(FilePath, temp =>
        {
            using var stream = new FileStream(temp, FileMode.Create, FileAccess.Write);
            serializer.Serialize(stream, Patterns);
        });
    }

    /// <summary>
    /// Load from file. Missing file = silently skip. Unreadable/corrupt file =
    /// set it aside as <c>.bad</c> so SeedDefaults can rebuild a working set.
    /// </summary>
    public static void Load()
    {
        if (!File.Exists(FilePath)) return;

        try
        {
            var serializer = new XmlSerializer(typeof(List<Pattern>));
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            Patterns = (List<Pattern>?)serializer.Deserialize(stream) ?? new List<Pattern>();
        }
        catch
        {
            Patterns = new List<Pattern>();

            // ย้ายไปเก็บไว้ ไม่เขียนทับ — ไฟล์ที่อ่านไม่ออกวันนี้อาจกู้ด้วยมือได้
            // ส่วน SeedDefaults จะเห็นว่าไม่มีไฟล์แล้วสร้างชุดเริ่มต้นให้ใช้งานต่อได้
            try { File.Move(FilePath, FilePath + ".bad", overwrite: true); } catch { /* ignore */ }
        }
    }

    /// <summary>If the file already exists, do nothing. Otherwise create the CCCC
    /// and DDDD defaults and persist them.</summary>
    public static void SeedDefaults()
    {
        if (File.Exists(FilePath)) return;

        Patterns = new List<Pattern>
        {
            new Pattern
            {
                Name = "CCCC",
                Description = "Copy whole barcode",
                TestBarcode = "C240801-027",
                TestBlockText = "CCCC-01 CPI291",
                Rules =
                {
                    new Rule { SourceStart = 1, SourceEnd = 999, TransformRule = TransformRuleType.COPY },
                },
            },
            new Pattern
            {
                Name = "DDDD",
                Description = "Date-encoded barcode",
                TestBarcode = "C200521-001",
                TestBlockText = "DDDD-01",
                Rules =
                {
                    new Rule { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.DELETE },
                    new Rule { SourceStart = 2, SourceEnd = 3, TransformRule = TransformRuleType.AZ_UPPER, Parameter = "15" },
                    new Rule { SourceStart = 4, SourceEnd = 5, TransformRule = TransformRuleType.AZ_UPPER, Parameter = "1" },
                    new Rule { SourceStart = 6, SourceEnd = 7, TransformRule = TransformRuleType.COPY },
                    new Rule { SourceStart = 8, SourceEnd = 8, TransformRule = TransformRuleType.COPY },
                    new Rule { SourceStart = 9, SourceEnd = 11, TransformRule = TransformRuleType.TAKE_RIGHT, Parameter = "2" },
                },
            },
        };

        // เขียนชุดเริ่มต้นไม่ลงก็ยังเปิดโปรแกรมได้ ใช้ชุดในหน่วยความจำไปก่อน
        // ถ้าปล่อยให้โยนออกไปจากตรงนี้ โปรแกรมจะเปิดไม่ขึ้นเลยตอนโฟลเดอร์เขียนไม่ได้
        // ซึ่งหน้าจอเตือนเรื่องบันทึกค่าไม่ได้ก็บอกเรื่องเดียวกันอยู่แล้ว
        try { Save(); }
        catch (Exception ex) { CustomSettingsManager.ReportWriteError(ex.Message); }
    }
}
