using System.Drawing;

namespace InkjetOperator.Services;

/// <summary>
/// Looks up marking reference images stored as loose files in a configurable
/// folder (Setting.config key MARKING_REF_FOLDER, typically a network share).
/// Files are named by program_name; one program may have several images named
/// "{program_name}", "{program_name}-1", "{program_name}-2", ...
/// </summary>
public static class MarkingRefImageService
{
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".bmp" };

    /// <summary>Configured folder (network share). Empty if not set.</summary>
    public static string FolderPath => CustomSettingsManager.Read("MARKING_REF_FOLDER", "");

    /// <summary>
    /// สภาพของโฟลเดอร์รูป — ใช้แยกให้ผู้ใช้เห็นว่า "งานนี้ไม่มีรูป" ต่างจาก
    /// "ยังไม่ได้ตั้งค่า" และ "ต่อ share ไม่ได้" ซึ่งเดิมเงียบเหมือนกันหมด
    /// </summary>
    public enum FolderState
    {
        /// <summary>ยังไม่ได้ตั้ง path ในหน้า Setting</summary>
        NotConfigured,

        /// <summary>ตั้งไว้แล้วแต่เข้าไม่ถึง — share ล่ม ไม่มีสิทธิ์ หรือ path ผิด</summary>
        Unreachable,

        /// <summary>เข้าถึงได้ปกติ</summary>
        Ok,
    }

    /// <summary>ตรวจโฟลเดอร์รูปหนึ่งครั้ง เพื่อเลือกข้อความที่จะบอกผู้ใช้</summary>
    public static FolderState CheckFolder()
    {
        var folder = FolderPath;
        if (string.IsNullOrWhiteSpace(folder)) return FolderState.NotConfigured;

        try
        {
            return Directory.Exists(folder) ? FolderState.Ok : FolderState.Unreachable;
        }
        catch
        {
            return FolderState.Unreachable;
        }
    }

    /// <summary>ข้อความอธิบายสาเหตุที่ไม่มีรูป ให้ทุกหน้าพูดเหมือนกัน</summary>
    public static string DescribeEmpty(FolderState state) => state switch
    {
        FolderState.NotConfigured => "ยังไม่ได้ตั้งโฟลเดอร์รูปอ้างอิง (Setting → Inkjet Setting)",
        FolderState.Unreachable => "เข้าโฟลเดอร์รูปอ้างอิงไม่ได้ — ตรวจการเชื่อมต่อ share",
        _ => "ไม่มีรูปอ้างอิง",
    };

    /// <summary>
    /// Files whose name (without extension) equals <paramref name="programName"/>
    /// or starts with "{programName}-". Extension-agnostic, case-insensitive,
    /// sorted by name. Returns empty on missing folder / unreachable share.
    /// </summary>
    public static List<string> FindImages(string? programName) => Search(programName, true);

    /// <summary>
    /// เอาเฉพาะไฟล์ที่ชื่อตรงเป๊ะ ไม่กินไฟล์ที่ขึ้นต้นเหมือนกัน
    ///
    /// ใช้ตอนที่ "-1" "-2" หมายถึงคนละรุ่นย่อย เช่นในหน้าจอเลือกโปรแกรม UV
    /// ถ้าใช้ <see cref="FindImages"/> ที่นั่น เลือก P-DPX-666 แล้วจะเห็นรูปของ
    /// P-DPX-666-1 ติดมาด้วย ทั้งที่เป็นคนละโปรแกรม
    /// </summary>
    public static List<string> FindImagesExact(string? programName) => Search(programName, false);

    private static List<string> Search(string? programName, bool includeSuffixed)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(programName)) return result;

        string folder = FolderPath;
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return result;

        string name = programName.Trim();
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                if (Array.IndexOf(Extensions, Path.GetExtension(file).ToLowerInvariant()) < 0)
                    continue;

                string stem = Path.GetFileNameWithoutExtension(file);
                bool match = stem.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    (includeSuffixed && stem.StartsWith(name + "-", StringComparison.OrdinalIgnoreCase));

                if (match) result.Add(file);
            }
        }
        catch
        {
            // share unavailable / permission — treat as no images
            return new List<string>();
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// รูปแทนสำหรับช่องที่ไม่มีรูป — ฝังมากับ .exe ไม่ได้อ่านจากดิสก์
    /// จึงใช้ได้แม้ share ล่มหรือยังไม่ได้ตั้งค่าโฟลเดอร์
    /// <para>
    /// คืน "สำเนาใหม่" ทุกครั้งโดยตั้งใจ เพราะ PictureBox ทุกช่องเรียก
    /// <c>Image.Dispose()</c> ตอนเปลี่ยนรูป ถ้าคืนตัวเดียวกันไปเรื่อย ๆ
    /// ครั้งแรกที่ถูก dispose รูปจะพังทั้งโปรแกรม
    /// </para>
    /// </summary>
    public static Image Placeholder() => new Bitmap(Properties.Resources.NoImageAvailable);

    /// <summary>
    /// Load an image as an independent copy so the source file is NOT locked
    /// (customer can overwrite the file anytime). Returns null on error.
    /// </summary>
    public static Image? LoadImageNoLock(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            using var ms = new MemoryStream(bytes);
            using var tmp = Image.FromStream(ms);
            return new Bitmap(tmp); // copy — no dependency on stream/file
        }
        catch
        {
            return null;
        }
    }
}
