using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using InkjetOperator.Models;

namespace InkjetOperator.Services
{
    public static class PatternStore
    {
        public static List<Pattern> Patterns { get; } = new List<Pattern>();

        public static void Save(string path)
        {
            var ser = new XmlSerializer(typeof(List<Pattern>));
            using (var fs = new FileStream(path, FileMode.Create))
                ser.Serialize(fs, Patterns);
        }

        public static void Load(string path)
        {
            if (!File.Exists(path)) return;
            try
            {
                var ser = new XmlSerializer(typeof(List<Pattern>));
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var list = (List<Pattern>)ser.Deserialize(fs);
                    Patterns.Clear();
                    Patterns.AddRange(list);
                }
            }
            catch
            {
                // XML เสีย — ลบแล้วให้ SeedDefaults สร้างใหม่
                try { File.Delete(path); } catch { }
            }
        }

        /// <summary>สร้าง default CCCC + DDDD ถ้ายังไม่มีไฟล์ (ตรงกับ XML จริง)</summary>
        public static void SeedDefaults(string path)
        {
            if (File.Exists(path)) return;

            // CCCC: copy barcode ทั้งตัว
            var cccc = new Pattern
            {
                Name = "CCCC",
                TestBarcode = "C240801-027",
                TestBlockText = "CCCC-01 CPI291",
                TestPreview = "C240801-027-01 CPI291",
            };
            cccc.Rules.Add(new Rule { SourceStart = 1, SourceEnd = 999, TransformRule = TransformRuleType.COPY });
            Patterns.Add(cccc);

            // DDDD: C200521-001 → FE21-01
            var dddd = new Pattern
            {
                Name = "DDDD",
                TestBarcode = "C200521-001",
                TestBlockText = "DDDD-01",
                TestPreview = "FE21-01-01",
            };
            dddd.Rules.AddRange(new[]
            {
                new Rule { SourceStart = 1,  SourceEnd = 1,  TransformRule = TransformRuleType.DELETE },
                new Rule { SourceStart = 2,  SourceEnd = 3,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "15" },
                new Rule { SourceStart = 4,  SourceEnd = 5,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "1" },
                new Rule { SourceStart = 6,  SourceEnd = 7,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 8,  SourceEnd = 8,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 9,  SourceEnd = 11, TransformRule = TransformRuleType.TAKE_RIGHT, Parameter = "2" },
            });
            Patterns.Add(dddd);

            Save(path);
        }
    }
}