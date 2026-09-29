using System.Xml.Linq;

namespace InkjetOperator.Services;

/// <summary>
/// ไฟล์ตั้งค่าหนึ่งไฟล์ อ่านผ่านสำเนาในหน่วยความจำ
///
/// <para>
/// เดิมการอ่านค่าหนึ่งค่าเปิดไฟล์แล้ว parse XML ใหม่ทั้งไฟล์ วัดได้ 0.077 ms ต่อครั้ง
/// ซึ่งดูน้อยแต่มีจุดเรียกกว่า 140 จุด และการสร้างแถวตารางหนึ่งแถวเรียกราวสามครั้ง
/// ตารางสองร้อยแถวจึงเสียไป 48 ms ทุกรอบรีเฟรช บนเธรดที่วาดจออยู่
/// </para>
/// <para>
/// เก็บค่าที่อ่านได้ไว้เป็น dictionary แล้วคืนจากตัวนั้น การอ่านจึงไม่แตะดิสก์อีก
/// ยังเห็นการแก้ไฟล์จากข้างนอก (แก้มือ หรือโปรแกรมอีกตัว) เพราะถาม timestamp กับ
/// ขนาดไฟล์เป็นระยะ ไม่ถามทุกครั้งที่อ่าน เพราะนั่นก็เป็นการแตะดิสก์เหมือนกัน
/// </para>
/// <para>
/// การบันทึกยังเขียนผ่าน <c>XDocument</c> ของไฟล์จริงเสมอ ไม่ได้เขียนจาก dictionary
/// เพื่อให้คอมเมนต์ ลำดับคีย์ และคีย์ที่โปรแกรมนี้ไม่รู้จักในไฟล์ยังอยู่ครบ
/// </para>
/// </summary>
internal sealed class SettingsStore
{
    /// <summary>
    /// ห่างจากการถามดิสก์ครั้งก่อนเกินเท่านี้ ค่อยถามใหม่ว่าไฟล์เปลี่ยนไหม
    ///
    /// การแก้ไฟล์จากข้างนอกจึงเห็นผลช้าสุดเท่านี้ ส่วนการบันทึกจากในโปรแกรมเห็นทันที
    /// เพราะอัปเดตสำเนาให้เลย ครึ่งวินาทีคือช่วงที่คนกดอะไรต่อไม่ทันอยู่แล้ว
    /// </summary>
    private const long RecheckMs = 500;

    private readonly object _gate = new();
    private readonly string _path;

    private Dictionary<string, string>? _values;
    private DateTime _writtenUtc;
    private long _length = -1;
    private long _checkedAt = long.MinValue;

    /// <param name="path">ที่อยู่เต็มของไฟล์ ผ่าน <see cref="AppSettingsFile.Resolve"/> มาแล้ว</param>
    public SettingsStore(string path) => _path = path;

    /// <summary>ที่อยู่ของไฟล์จริง</summary>
    public string Path => _path;

    public string Read(string key, string defaultValue = "")
    {
        try
        {
            lock (_gate)
                return Values().TryGetValue(key, out var value) ? value : defaultValue;
        }
        catch { return defaultValue; }
    }

    /// <summary>
    /// บันทึกค่าหนึ่งค่า — คืนข้อความปัญหา หรือ null เมื่อสำเร็จ
    /// </summary>
    public string? Write(string key, string value)
    {
        lock (_gate)
        {
            try
            {
                AppSettingsFile.EnsureAppSettingsFile(_path);

                var doc = XDocument.Load(_path);
                var settings = doc.Root?.Element("appSettings");
                if (settings == null) return "ไฟล์ตั้งค่าเสียหาย — ไม่พบส่วน appSettings";

                var el = settings.Elements("add")
                    .FirstOrDefault(e => e.Attribute("key")?.Value == key);

                if (el != null)
                    el.SetAttributeValue("value", value);
                else
                    settings.Add(new XElement("add",
                        new XAttribute("key", key),
                        new XAttribute("value", value)));

                // ไม่เขียนทับไฟล์เดิมตรง ๆ — เหตุผลที่ AppSettingsFile.SaveAtomic
                AppSettingsFile.SaveAtomic(_path, doc.Save);

                // ทิ้งสำเนาไป ไม่แก้ทีละคีย์ — รอบหน้าอ่านไฟล์ที่เพิ่งเขียนมาใหม่หมด
                // จึงไม่มีทางที่สำเนาจะไม่ตรงกับไฟล์ ไม่ว่าการเขียนจะไปโดนอะไรบ้าง
                Forget();
                return null;
            }
            catch (Exception ex)
            {
                Forget();
                return ex.Message;
            }
        }
    }

    /// <summary>ลืมสำเนาที่จำไว้ ให้การอ่านครั้งหน้าไปเอาจากไฟล์</summary>
    public void Forget()
    {
        lock (_gate)
        {
            _values = null;
            _length = -1;
            _checkedAt = long.MinValue;
        }
    }

    /// <summary>ค่าทั้งไฟล์ — เรียกใต้ <see cref="_gate"/> เท่านั้น</summary>
    private Dictionary<string, string> Values()
    {
        long now = Environment.TickCount64;

        if (_values != null && now - _checkedAt < RecheckMs) return _values;

        var info = new FileInfo(_path);
        if (!info.Exists)
        {
            AppSettingsFile.EnsureAppSettingsFile(_path);
            info = new FileInfo(_path);
        }
        _checkedAt = now;

        if (_values != null && info.LastWriteTimeUtc == _writtenUtc && info.Length == _length)
            return _values;

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var el in XDocument.Load(_path).Root?
                     .Element("appSettings")?.Elements("add") ?? [])
        {
            var key = el.Attribute("key")?.Value;

            // คีย์ซ้ำในไฟล์ให้ตัวแรกชนะ เหมือนตอนที่ยังไล่หาทีละ element
            if (key != null && !map.ContainsKey(key))
                map[key] = el.Attribute("value")?.Value ?? "";
        }

        _values = map;
        _writtenUtc = info.LastWriteTimeUtc;
        _length = info.Length;
        return map;
    }
}
