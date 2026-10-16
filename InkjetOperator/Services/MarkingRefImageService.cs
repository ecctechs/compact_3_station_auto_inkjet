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
    public static FolderState CheckFolder() => GetListing().State;

    /// <summary>
    /// อ่านรายชื่อไฟล์ในโฟลเดอร์รูปเตรียมไว้บนเธรดพื้นหลัง
    /// เรียกก่อนเปิดหน้าจอที่จะค้นรูปหลายครั้ง การค้นหลังจากนั้นใช้รายชื่อที่จำไว้ ไม่แตะแชร์อีก
    /// </summary>
    public static Task WarmAsync() => Task.Run(() => GetListing());

    // ── รายชื่อไฟล์ที่จำไว้ ─────────────────────────────────
    //
    // เดิมค้นรูปหนึ่งครั้ง = ไล่รายชื่อไฟล์ทั้งโฟลเดอร์บนแชร์หนึ่งรอบ หน้า Preview หนึ่งงาน
    // ค้น 2–4 ครั้ง หน้าเลือกรุ่นย่อยค้นรุ่นละครั้ง ทุกครั้งวิ่งข้ามเครือข่ายใหม่หมด
    // และแชร์ที่ดับทำให้ค้างรอบละ 20–60 วินาที
    //
    // ตอนนี้อ่านรายชื่อครั้งเดียวแล้วจำไว้สั้น ๆ การจับคู่ชื่อ / เรียงลำดับยังทำแบบเดิมทุกอย่าง
    // ผลที่ได้จึงเหมือนเดิม ต่างแค่ไม่วิ่งไปถามแชร์ซ้ำ ๆ
    //
    // ไฟล์รูปที่เพิ่งวางลงโฟลเดอร์จะเห็นภายในไม่เกิน ListingCacheFor

    private sealed record Listing(FolderState State, string[] Files, DateTime At);

    private static readonly TimeSpan ListingCacheFor = TimeSpan.FromSeconds(10);

    /// <summary>ผลที่เข้าไม่ถึง จำไว้สั้นกว่า — แก้สายแล้วจะเห็นรูปเร็ว</summary>
    private static readonly TimeSpan FailedCacheFor = TimeSpan.FromSeconds(3);

    /// <summary>
    /// เพดานเวลารอรายชื่อ — เครื่องตอบแต่แชร์อ่านไม่ได้ก็มี ผู้เรียกจะไม่ค้างเกินนี้
    /// (การอ่านยังวิ่งต่อข้างหลัง เสร็จเมื่อไหร่ก็เก็บไว้ใช้รอบหน้า)
    /// </summary>
    private static readonly TimeSpan ListTimeout = TimeSpan.FromSeconds(6);

    private static readonly object Gate = new();
    private static Listing? _cached;
    private static string? _cachedFolder;
    private static Task<Listing>? _loading;
    private static string? _loadingFolder;

    private static Listing GetListing()
    {
        var folder = FolderPath;
        if (string.IsNullOrWhiteSpace(folder))
            return new Listing(FolderState.NotConfigured, [], DateTime.UtcNow);

        Task<Listing> task;
        lock (Gate)
        {
            if (_cached != null &&
                string.Equals(_cachedFolder, folder, StringComparison.OrdinalIgnoreCase) &&
                DateTime.UtcNow - _cached.At < (_cached.State == FolderState.Ok ? ListingCacheFor : FailedCacheFor))
                return _cached;

            // มีคนกำลังอ่านโฟลเดอร์เดียวกันอยู่ก็รอตัวนั้น ไม่เปิดเธรดใหม่ไปค้างซ้อน
            if (_loading != null && string.Equals(_loadingFolder, folder, StringComparison.OrdinalIgnoreCase))
            {
                task = _loading;
            }
            else
            {
                task = Task.Run(() => Load(folder));
                _loading = task;
                _loadingFolder = folder;
            }
        }

        try
        {
            return task.WaitAsync(ListTimeout).GetAwaiter().GetResult();
        }
        catch
        {
            return new Listing(FolderState.Unreachable, [], DateTime.UtcNow);
        }
    }

    private static Listing Load(string folder)
    {
        Listing listing;
        try
        {
            if (PathProbe.HostOf(folder) is string host && !PathProbe.HostUpAsync(host).GetAwaiter().GetResult())
            {
                listing = new Listing(FolderState.Unreachable, [], DateTime.UtcNow);
            }
            else if (!Directory.Exists(folder))
            {
                listing = new Listing(FolderState.Unreachable, [], DateTime.UtcNow);
            }
            else
            {
                var files = Directory.EnumerateFiles(folder)
                    .Where(f => Array.IndexOf(Extensions, Path.GetExtension(f).ToLowerInvariant()) >= 0)
                    .ToArray();
                listing = new Listing(FolderState.Ok, files, DateTime.UtcNow);
            }
        }
        catch
        {
            // share unavailable / permission — treat as no images
            listing = new Listing(FolderState.Unreachable, [], DateTime.UtcNow);
        }

        lock (Gate)
        {
            _cached = listing;
            _cachedFolder = folder;
            if (string.Equals(_loadingFolder, folder, StringComparison.OrdinalIgnoreCase))
            {
                _loading = null;
                _loadingFolder = null;
            }
        }
        return listing;
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

        var listing = GetListing();
        if (listing.State != FolderState.Ok) return result;

        string name = programName.Trim();
        foreach (var file in listing.Files)
        {
            string stem = Path.GetFileNameWithoutExtension(file);
            bool match = stem.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                (includeSuffixed && stem.StartsWith(name + "-", StringComparison.OrdinalIgnoreCase));

            if (match) result.Add(file);
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
