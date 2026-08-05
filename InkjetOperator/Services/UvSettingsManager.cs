using System.Xml.Linq;

namespace InkjetOperator.Services;

public static class UvSettingsManager
{
    private static readonly string _path =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uv.config");

    public static string Read(string key, string defaultValue = "")
    {
        try
        {
            EnsureFile();
            var doc = XDocument.Load(_path);
            var el = doc.Root?.Element("appSettings")?
                .Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);
            return el?.Attribute("value")?.Value ?? defaultValue;
        }
        catch { return defaultValue; }
    }

    public static void Write(string key, string value)
    {
        try
        {
            EnsureFile();
            var doc = XDocument.Load(_path);
            var settings = doc.Root?.Element("appSettings");
            if (settings == null) return;

            var el = settings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);

            if (el != null)
                el.SetAttributeValue("value", value);
            else
                settings.Add(new XElement("add",
                    new XAttribute("key", key),
                    new XAttribute("value", value)));

            doc.Save(_path);
        }
        catch { /* ignore */ }
    }

    private static void EnsureFile()
    {
        if (File.Exists(_path)) return;
        var doc = new XDocument(
            new XElement("configuration",
                new XElement("appSettings")));
        doc.Save(_path);
    }
}
