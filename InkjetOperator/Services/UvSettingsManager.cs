namespace InkjetOperator.Services;

public static class UvSettingsManager
{
    private static readonly SettingsStore _store = new(AppSettingsFile.Resolve("uv.config"));

    public static string Read(string key, string defaultValue = "") => _store.Read(key, defaultValue);

    public static bool Write(string key, string value)
    {
        var error = _store.Write(key, value);
        if (error == null) return true;

        CustomSettingsManager.ReportWriteError(error);
        return false;
    }

    private const string REL_CPI = @"database\sys\CPI.db3";
    private const string REL_DOCUMENT = "document";

    public static string? GetCpiPath(int uvNumber)
    {
        var folder = Read(uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER");
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_CPI);
        return Exists(path, folder: false) ? path : null;
    }

    public static string? GetDocumentFolder(int uvNumber)
    {
        var folder = Read(uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER");
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_DOCUMENT);
        return Exists(path, folder: true) ? path : null;
    }

    /// <summary>
    /// เหมือน <see cref="GetCpiPath"/> แต่ทำบนเธรดพื้นหลัง
    /// <para>
    /// โฟลเดอร์ UV อยู่บนเครื่อง UV ผ่านเครือข่าย <c>File.Exists</c> บนแชร์ที่ช้าหรือหลุด
    /// ค้างได้หลายสิบวินาที ถ้าเรียกจากหน้าจอ หน้าจอจะนิ่งทั้งหน้าระหว่างนั้น
    /// ทางที่กดเริ่มงานจึงต้องเรียกตัวนี้ ผลที่ได้เหมือนตัวเดิมทุกประการ
    /// </para>
    /// </summary>
    public static Task<string?> GetCpiPathAsync(int uvNumber) => Task.Run(() => GetCpiPath(uvNumber));

    /// <summary>เหมือน <see cref="GetDocumentFolder"/> แต่ทำบนเธรดพื้นหลัง — เหตุผลเดียวกับ <see cref="GetCpiPathAsync"/></summary>
    public static Task<string?> GetDocumentFolderAsync(int uvNumber) => Task.Run(() => GetDocumentFolder(uvNumber));

    /// <summary>จำผลว่า "มีอยู่" ไว้นานเท่านี้</summary>
    private static readonly TimeSpan FoundCacheFor = TimeSpan.FromSeconds(10);

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> Found =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// ไฟล์ / โฟลเดอร์นี้มีอยู่ไหม — จำเฉพาะผลที่ "มี" ไว้ 10 วินาที
    /// <para>
    /// กดเริ่มงานหนึ่งครั้งเช็ค path เดิมซ้ำหลายรอบ (ตรวจก่อนส่ง แล้วตรวจอีกตอนส่งจริง)
    /// แต่ละรอบคือการวิ่งข้ามเครือข่ายไปถามเครื่อง UV การจำผลที่เพิ่งเช็คไว้ตัดรอบที่ซ้ำออก
    /// </para>
    /// <para>
    /// ผลที่ "ไม่มี" ไม่จำ เช็คใหม่ทุกครั้ง — ถ้าจำ คนที่เพิ่งแก้ปัญหาเสร็จ (เปิดเครื่อง UV
    /// เสียบสาย) แล้วกดเริ่มใหม่ทันทีจะยังเจอข้อความเดิม ผลลัพธ์จึงตรงกับของเดิมเสมอ
    /// </para>
    /// <para>
    /// ก่อนแตะไฟล์ถามตัวเครื่องก่อนด้วย <see cref="PathProbe"/> (TCP 445 รอไม่เกิน 1 วินาที)
    /// เครื่อง UV ดับ <c>File.Exists</c> ก็คืน false อยู่ดี แต่ต้องรอ SMB ยอมแพ้ 20–60 วินาที
    /// ถามเครื่องก่อนจึงได้คำตอบเดียวกันในไม่ถึงวินาที
    /// </para>
    /// </summary>
    private static bool Exists(string path, bool folder)
    {
        var key = (folder ? "D|" : "F|") + path;
        if (Found.TryGetValue(key, out var at) && DateTime.UtcNow - at < FoundCacheFor) return true;

        // Task.Run กัน deadlock ถ้าถูกเรียกจากหน้าจอ (await ข้างในจะไม่พยายามกลับมาเธรดหน้าจอ)
        if (PathProbe.HostOf(path) is string host &&
            !Task.Run(() => PathProbe.HostUpAsync(host)).GetAwaiter().GetResult())
        {
            Found.TryRemove(key, out _);
            return false;
        }

        bool exists = folder ? Directory.Exists(path) : File.Exists(path);
        if (exists) Found[key] = DateTime.UtcNow;
        else Found.TryRemove(key, out _);
        return exists;
    }
}
