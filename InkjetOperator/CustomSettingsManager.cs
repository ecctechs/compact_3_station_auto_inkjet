using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

public static class CustomSettingsManager
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Setting.config");

    public static string? GetValue(string key)
    {
        if (!File.Exists(ConfigPath)) return null;
        var doc = new XmlDocument();
        doc.Load(ConfigPath);
        var node = doc.SelectSingleNode($"/configuration/appSettings/add[@key='{key}']");
        return node?.Attributes?["value"]?.Value;
    }

    public static void SetValue(string key, string value)
    {
        //Debug.WriteLine("WRITE PATH: " + ConfigPath);
        XmlDocument doc = new XmlDocument();
        if (File.Exists(ConfigPath))
        {
            doc.Load(ConfigPath);
        }
        else
        {
            doc.LoadXml("<configuration><appSettings></appSettings></configuration>");
        }

        var appSettings = doc.SelectSingleNode("/configuration/appSettings");
        var node = appSettings?.SelectSingleNode($"add[@key='{key}']") as XmlElement;

        if (node == null)
        {
            node = doc.CreateElement("add");
            node.SetAttribute("key", key);
            node.SetAttribute("value", value);
            appSettings?.AppendChild(node);
        }
        else
        {
            node.SetAttribute("value", value);
        }

        doc.Save(ConfigPath);
    }
}