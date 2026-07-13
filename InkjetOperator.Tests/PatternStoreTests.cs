using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Tests
{
    /// <summary>
    /// PatternStore.Patterns เป็น static ตัวเดียวทั้งแอป — ทุก test class ที่แตะมัน
    /// ต้องอยู่ collection เดียวกันเพื่อไม่ให้ xUnit รันขนานแล้ว state ชนกัน
    /// </summary>
    [CollectionDefinition("PatternStore serial")]
    public class PatternStoreSerialCollection { }

    [Collection("PatternStore serial")]
    public class PatternStoreTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();

        public PatternStoreTests()
        {
            PatternStore.Patterns.Clear();
        }

        public void Dispose()
        {
            PatternStore.Patterns.Clear();
            foreach (var f in _tempFiles)
            {
                try { File.Delete(f); } catch { }
            }
        }

        /// <summary>path ไฟล์ชั่วคราวไม่ซ้ำกันต่อ test — ลบทิ้งอัตโนมัติตอนจบ</summary>
        private string TempXmlPath()
        {
            string path = Path.Combine(Path.GetTempPath(), $"patterns_test_{Guid.NewGuid():N}.xml");
            _tempFiles.Add(path);
            return path;
        }

        private static Pattern MakeDdddPattern()
        {
            var dddd = new Pattern
            {
                Name = "DDDD",
                Description = "test desc",
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
            return dddd;
        }

        // ── SeedDefaults ────────────────────────────────────

        [Fact]
        public void SeedDefaults_NoFile_CreatesFileWithCcccAndDddd()
        {
            string path = TempXmlPath();

            PatternStore.SeedDefaults(path);

            Assert.True(File.Exists(path));
            Assert.Equal(2, PatternStore.Patterns.Count);
            Assert.Contains(PatternStore.Patterns, p => p.Name == "CCCC");
            Assert.Contains(PatternStore.Patterns, p => p.Name == "DDDD");
        }

        [Fact]
        public void SeedDefaults_DefaultPatterns_ApplyMatchesTestPreview()
        {
            string path = TempXmlPath();

            PatternStore.SeedDefaults(path);

            var cccc = PatternStore.Patterns.Single(p => p.Name == "CCCC");
            var dddd = PatternStore.Patterns.Single(p => p.Name == "DDDD");
            // CCCC = copy barcode ทั้งตัว, DDDD = C200521-001 → FE21-01
            Assert.Equal("C240801-027", cccc.Apply(cccc.TestBarcode));
            Assert.Equal("FE21-01", dddd.Apply(dddd.TestBarcode));
        }

        [Fact]
        public void SeedDefaults_FileAlreadyExists_DoesNothing()
        {
            string path = TempXmlPath();
            File.WriteAllText(path, "existing-content");

            PatternStore.SeedDefaults(path);

            Assert.Empty(PatternStore.Patterns);
            Assert.Equal("existing-content", File.ReadAllText(path));
        }

        // ── Save / Load round-trip ──────────────────────────

        [Fact]
        public void SaveThenLoad_RoundTrip_PreservesPatternAndRules()
        {
            string path = TempXmlPath();
            PatternStore.Patterns.Add(MakeDdddPattern());
            PatternStore.Save(path);
            PatternStore.Patterns.Clear();

            PatternStore.Load(path);

            var loaded = Assert.Single(PatternStore.Patterns);
            Assert.Equal("DDDD", loaded.Name);
            Assert.Equal("test desc", loaded.Description);
            Assert.Equal("C200521-001", loaded.TestBarcode);
            Assert.Equal("DDDD-01", loaded.TestBlockText);
            Assert.Equal("FE21-01-01", loaded.TestPreview);
            Assert.Equal(6, loaded.Rules.Count);
            Assert.Equal(TransformRuleType.AZ_UPPER, loaded.Rules[1].TransformRule);
            Assert.Equal("15", loaded.Rules[1].Parameter);
            // rule ที่โหลดกลับมาต้องยังแปลง barcode ได้ผลเดิม
            Assert.Equal("FE21-01", loaded.Apply("C200521-001"));
        }

        [Fact]
        public void Load_ReplacesExistingPatterns()
        {
            string path = TempXmlPath();
            PatternStore.Patterns.Add(new Pattern { Name = "SAVED" });
            PatternStore.Save(path);
            PatternStore.Patterns.Clear();
            PatternStore.Patterns.Add(new Pattern { Name = "OLD_IN_MEMORY" });

            PatternStore.Load(path);

            var loaded = Assert.Single(PatternStore.Patterns);
            Assert.Equal("SAVED", loaded.Name);
        }

        // ── Load edge cases ─────────────────────────────────

        [Fact]
        public void Load_MissingFile_LeavesPatternsUntouched()
        {
            PatternStore.Patterns.Add(new Pattern { Name = "KEEP_ME" });

            PatternStore.Load(Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.xml"));

            var kept = Assert.Single(PatternStore.Patterns);
            Assert.Equal("KEEP_ME", kept.Name);
        }

        [Fact]
        public void Load_CorruptXml_DeletesFileAndKeepsExistingPatterns()
        {
            string path = TempXmlPath();
            File.WriteAllText(path, "<<< not valid xml >>>");
            PatternStore.Patterns.Add(new Pattern { Name = "KEEP_ME" });

            PatternStore.Load(path);

            // ไฟล์เสียถูกลบทิ้งเพื่อให้ SeedDefaults สร้างใหม่ได้ (พฤติกรรมที่ Program.cs พึ่งพา)
            Assert.False(File.Exists(path));
            var kept = Assert.Single(PatternStore.Patterns);
            Assert.Equal("KEEP_ME", kept.Name);
        }

        [Fact]
        public void Load_AfterCorruptFileDeleted_SeedDefaultsRecreates()
        {
            // จำลอง flow เต็มใน Program.cs: Load เจอไฟล์เสีย → ลบ → seed ใหม่ → load สำเร็จ
            string path = TempXmlPath();
            File.WriteAllText(path, "corrupted");

            PatternStore.Load(path);
            Assert.Empty(PatternStore.Patterns);

            PatternStore.SeedDefaults(path);
            PatternStore.Patterns.Clear();
            PatternStore.Load(path);

            Assert.Equal(2, PatternStore.Patterns.Count);
        }

        [Fact]
        public void Save_OverwritesExistingFile()
        {
            string path = TempXmlPath();
            PatternStore.Patterns.Add(new Pattern { Name = "FIRST" });
            PatternStore.Save(path);

            PatternStore.Patterns.Clear();
            PatternStore.Patterns.Add(new Pattern { Name = "SECOND" });
            PatternStore.Save(path);
            PatternStore.Patterns.Clear();

            PatternStore.Load(path);
            var loaded = Assert.Single(PatternStore.Patterns);
            Assert.Equal("SECOND", loaded.Name);
        }
    }
}
