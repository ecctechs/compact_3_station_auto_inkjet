using System.Xml.Serialization;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// Loads/saves the local transform patterns (patterns.xml next to the exe).
/// Holds one process-wide list, mirroring the old Linx PatternStore behaviour.
/// </summary>
public static class PatternStore
{
    /// <summary>The single in-memory list shared across the app.</summary>
    public static List<Pattern> Patterns { get; private set; } = new();

    /// <summary>Overwrite the file with the current list via XmlSerializer.</summary>
    public static void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(List<Pattern>));
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        serializer.Serialize(stream, Patterns);
    }

    /// <summary>
    /// Load from file. Missing file = silently skip. Unreadable/corrupt file =
    /// delete it (intentional) so SeedDefaults can rebuild a working set.
    /// </summary>
    public static void Load(string path)
    {
        if (!File.Exists(path)) return;

        try
        {
            var serializer = new XmlSerializer(typeof(List<Pattern>));
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Patterns = (List<Pattern>?)serializer.Deserialize(stream) ?? new List<Pattern>();
        }
        catch
        {
            Patterns = new List<Pattern>();
            try { Save(path); } catch { /* ignore */ }
        }
    }

    /// <summary>If the file already exists, do nothing. Otherwise create the CCCC
    /// and DDDD defaults and persist them.</summary>
    public static void SeedDefaults(string path)
    {
        if (File.Exists(path)) return;

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

        Save(path);
    }
}
