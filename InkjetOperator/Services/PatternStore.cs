using System.Xml.Serialization;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class PatternStore
{
    public static string FilePath { get; } = AppSettingsFile.Resolve("patterns.xml");

    public static List<Pattern> Patterns { get; private set; } = new();

    public static void Save()
    {
        var serializer = new XmlSerializer(typeof(List<Pattern>));

        AppSettingsFile.SaveAtomic(FilePath, temp =>
        {
            using var stream = new FileStream(temp, FileMode.Create, FileAccess.Write);
            serializer.Serialize(stream, Patterns);
        });
    }

    public static void Load()
    {
        if (!File.Exists(FilePath)) return;

        try
        {
            var serializer = new XmlSerializer(typeof(List<Pattern>));
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            Patterns = (List<Pattern>?)serializer.Deserialize(stream) ?? new List<Pattern>();
        }
        catch
        {
            Patterns = new List<Pattern>();

            try { File.Move(FilePath, FilePath + ".bad", overwrite: true); } catch { /* ignore */ }
        }
    }

    public static void SeedDefaults()
    {
        if (File.Exists(FilePath)) return;

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

        try { Save(); }
        catch (Exception ex) { CustomSettingsManager.ReportWriteError(ex.Message); }
    }
}
