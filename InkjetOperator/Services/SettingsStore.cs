using System.Xml.Linq;

namespace InkjetOperator.Services;

internal sealed class SettingsStore
{
    private const long RecheckMs = 500;

    private readonly object _gate = new();
    private readonly string _path;

    private Dictionary<string, string>? _values;
    private DateTime _writtenUtc;
    private long _length = -1;
    private long _checkedAt = long.MinValue;

    public SettingsStore(string path) => _path = path;

    public string Path => _path;

    public string Read(string key, string defaultValue = "") // อ่านค่าตั้งจากแคชและเช็กไฟล์ที่เปลี่ยน
    {
        try
        {
            lock (_gate)
                return Values().TryGetValue(key, out var value) ? value : defaultValue;
        }
        catch { return defaultValue; }
    }

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

                AppSettingsFile.SaveAtomic(_path, doc.Save); // เขียนให้ครบแล้วสลับไฟล์ ป้องกันค่าว่างกลางบันทึก

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

    public void Forget()
    {
        lock (_gate)
        {
            _values = null; // บังคับให้การอ่านครั้งหน้าโหลดค่าชุดใหม่
            _length = -1;
            _checkedAt = long.MinValue;
        }
    }

    private Dictionary<string, string> Values()
    {
        long now = Environment.TickCount64;

        if (_values != null && now - _checkedAt < RecheckMs) return _values;

        var info = new FileInfo(_path); // ใช้เวลาแก้ไขและขนาดตรวจว่าไฟล์ถูกแก้จากภายนอกหรือไม่
        if (!info.Exists)
        {
            AppSettingsFile.EnsureAppSettingsFile(_path);
            info = new FileInfo(_path);
        }
        _checkedAt = now; // เว้นช่วงตรวจดิสก์ ไม่เปิดไฟล์ซ้ำทุกครั้งที่อ่านค่า

        if (_values != null && info.LastWriteTimeUtc == _writtenUtc && info.Length == _length)
            return _values;

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var el in XDocument.Load(_path).Root?
                     .Element("appSettings")?.Elements("add") ?? [])
        {
            var key = el.Attribute("key")?.Value;

            if (key != null && !map.ContainsKey(key))
                map[key] = el.Attribute("value")?.Value ?? "";
        }

        _values = map; // เก็บค่าล่าสุดให้ส่วนอื่นอ่านจากหน่วยความจำ
        _writtenUtc = info.LastWriteTimeUtc; // จำเวลาไฟล์ไว้ตรวจการแก้จากโปรแกรมอื่น
        _length = info.Length; // ใช้ขนาดไฟล์ช่วยตรวจว่าชุดตั้งค่าเปลี่ยนแล้ว
        return map;
    }
}
