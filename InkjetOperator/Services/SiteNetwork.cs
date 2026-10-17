namespace InkjetOperator.Services;

/// <summary>
/// IP ของเครื่องที่หน้างานจริง — ปุ่ม "ใช้ IP หน้างาน" ในหน้า Setting ใส่ให้ทีเดียวครบทุกช่อง
///
/// <para>
/// มีไว้ตอนติดตั้ง: Setting.default.config ใช้ได้เฉพาะเครื่องที่ไม่เคยมีไฟล์ Setting
/// (ถอนแล้วลงใหม่ไฟล์เดิมยังอยู่ ค่าตั้งต้นไม่ไปทับ) และ IP กระจายอยู่สี่หน้า
/// ปุ่มนี้แก้เฉพาะช่อง IP ค่าอื่น (path, สถานี, ชื่อเครื่อง) ไม่แตะ
/// </para>
/// <para>
/// เครือข่ายหน้างานเปลี่ยน แก้ที่ <see cref="Entries"/> ที่เดียว
/// </para>
/// </summary>
public static class SiteNetwork
{
    /// <param name="Key">ชื่อค่าใน Setting.config</param>
    /// <param name="Label">ชื่อที่หน้างานเรียก</param>
    /// <param name="OnlyIfEmpty">
    /// พอร์ต — ใส่เฉพาะตอนยังว่าง ถ้าตั้งไว้แล้วถือว่าคนตั้งรู้ดีกว่า ไม่ทับ
    /// </param>
    public sealed record Entry(string Key, string Label, string Value, bool OnlyIfEmpty = false);

    public static readonly IReadOnlyList<Entry> Entries =
    [
        new("PC_IP", "Backend", "10.10.100.6"),
        new("MK058_COM", "MK58", "10.10.100.8"),
        new("MK059_COM", "MK59", "10.10.100.9"),
        new("UV001_IP", "UV63 (UV1 Plate)", "10.10.100.4"),
        new("UV002_IP", "UV67 (UV2 Shim)", "10.10.100.3"),
        new("PLC_IP", "PLC MK", "192.168.1.1"),
        new("CLAMP_PLC_IP", "PLC UV (แคลมป์)", "10.10.100.100"),
        new("UV001_PORT", "พอร์ต UV63", "10086", OnlyIfEmpty: true),
        new("UV002_PORT", "พอร์ต UV67", "10086", OnlyIfEmpty: true),
        new("PLC_PORT", "พอร์ต PLC MK", "502", OnlyIfEmpty: true),
        new("CLAMP_PLC_PORT", "พอร์ต PLC UV", "5012", OnlyIfEmpty: true),
    ];

    /// <param name="Old">ค่าตอนนี้ — ว่าง = ยังไม่ได้ตั้ง</param>
    public sealed record Change(Entry Entry, string Old);

    /// <summary>ช่องที่ยังไม่ตรงกับหน้างาน — ว่าง = ตั้งไว้ครบแล้ว ไม่ต้องทำอะไร</summary>
    public static List<Change> Pending() =>
        Entries
            .Select(e => new Change(e, CustomSettingsManager.Read(e.Key, "").Trim()))
            .Where(c => c.Entry.OnlyIfEmpty ? c.Old.Length == 0 : c.Old != c.Entry.Value)
            .ToList();

    /// <summary>
    /// เขียนค่าที่ต่าง — เก็บสำเนาไฟล์ทั้งไฟล์ไว้ก่อน (.bak ปกติเก็บได้แค่ก่อนเขียนครั้งล่าสุด
    /// เขียนเจ็ดช่องแล้วย้อนไม่ได้) คืนสาเหตุที่เขียนไม่ผ่าน หรือ null เมื่อครบ
    /// </summary>
    public static (string? Error, string? Backup) Apply(IReadOnlyList<Change> changes)
    {
        string? backup = null;
        try
        {
            var file = CustomSettingsManager.FilePath;
            if (File.Exists(file))
            {
                backup = file + $".before-site-ip-{DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture)}.bak";
                File.Copy(file, backup, overwrite: false);
            }
        }
        catch (Exception ex)
        {
            // สำรองไม่ได้ก็ยังไม่เขียน — ให้คนตัดสินใจแก้ทีละหน้าเองดีกว่าเขียนแล้วย้อนไม่ได้
            return ($"เก็บสำเนาไฟล์ Setting ก่อนแก้ไม่ได้ จึงยังไม่ได้แก้อะไร: {ex.Message}", null);
        }

        foreach (var c in changes)
        {
            if (!CustomSettingsManager.Write(c.Entry.Key, c.Entry.Value))
                return ($"บันทึก {c.Entry.Label} ไม่สำเร็จ: {CustomSettingsManager.LastError}", backup);
        }

        AppLog.Info("SiteNetwork", "ใช้ IP หน้างาน: "
            + string.Join(", ", changes.Select(c => $"{c.Entry.Key} {(c.Old.Length == 0 ? "(ว่าง)" : c.Old)} → {c.Entry.Value}")));
        return (null, backup);
    }
}
