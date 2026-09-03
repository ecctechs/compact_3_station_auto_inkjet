using System.Xml.Linq;

namespace InkjetOperator.Services;

public static class CustomSettingsManager
{
    // อยู่ที่ ProgramData ไม่ใช่ข้าง ๆ ตัวโปรแกรม เพราะ Program Files เขียนไม่ได้
    // ถ้าไม่ได้รันแบบ admin — รายละเอียดที่ AppSettingsFile
    private static readonly string _path = AppSettingsFile.Resolve("Setting.config");

    /// <summary>สาเหตุที่บันทึกครั้งล่าสุดไม่สำเร็จ — null = สำเร็จ</summary>
    public static string? LastError { get; private set; }

    /// <summary>ให้ตัวจัดการไฟล์ตั้งค่าตัวอื่นรายงานปัญหามาที่เดียวกัน</summary>
    internal static void ReportWriteError(string message) => LastError = message;

    public static string Read(string key, string defaultValue = "")
    {
        try
        {
            var doc = XDocument.Load(_path);
            var el = doc.Root?.Element("appSettings")?
                .Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);
            return el?.Attribute("value")?.Value ?? defaultValue;
        }
        catch { return defaultValue; }
    }

    /// <summary>คืน false เมื่อบันทึกไม่สำเร็จ ดูสาเหตุได้ที่ <see cref="LastError"/></summary>
    public static bool Write(string key, string value)
    {
        LastError = null;
        try
        {
            var doc = XDocument.Load(_path);
            var settings = doc.Root?.Element("appSettings");
            if (settings == null)
            {
                LastError = "ไฟล์ตั้งค่าเสียหาย — ไม่พบส่วน appSettings";
                return false;
            }

            var el = settings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);

            if (el != null)
                el.SetAttributeValue("value", value);
            else
                settings.Add(new XElement("add",
                    new XAttribute("key", key),
                    new XAttribute("value", value)));

            doc.Save(_path);
            return true;
        }
        catch (Exception ex)
        {
            // เดิมกลืน error ทิ้งเงียบ ๆ ผู้ใช้เลยไม่รู้ว่ากด Save แล้วไม่ได้บันทึกจริง
            LastError = ex.Message;
            return false;
        }
    }
}
