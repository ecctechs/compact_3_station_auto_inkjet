using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Tests
{
    public class PatternEngineTests : IDisposable
    {
        public PatternEngineTests()
        {
            PatternStore.Patterns.Clear();
        }

        public void Dispose()
        {
            PatternStore.Patterns.Clear();
        }

        [Theory]
        [InlineData(null, "block text", "block text")]
        [InlineData("", "block text", "block text")]
        public void Process_NullOrEmptyBarcode_ReturnsBlockText(string? barcode, string blockText, string expected)
        {
            Assert.Equal(expected, PatternEngine.Process(barcode!, blockText));
        }

        [Theory]
        [InlineData("barcode", null, "")]
        [InlineData("barcode", "", "")]
        public void Process_NullOrEmptyBlockText_ReturnsEmpty(string barcode, string? blockText, string expected)
        {
            Assert.Equal(expected, PatternEngine.Process(barcode, blockText!));
        }

        [Fact]
        public void Process_NoMatchingPattern_ReturnsBlockTextUnchanged()
        {
            var pattern = new Pattern { Name = "CCCC" };
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 999,
                TransformRule = TransformRuleType.COPY
            });
            PatternStore.Patterns.Add(pattern);

            Assert.Equal("no match here", PatternEngine.Process("barcode", "no match here"));
        }

        [Fact]
        public void Process_MatchingPattern_ReplacesPatternNameWithApplyResult()
        {
            var pattern = new Pattern { Name = "CCCC" };
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 999,
                TransformRule = TransformRuleType.COPY
            });
            PatternStore.Patterns.Add(pattern);

            string result = PatternEngine.Process("C240801-027", "CCCC-01 CPI291");
            Assert.Equal("C240801-027-01 CPI291", result);
        }

        [Fact]
        public void Process_DdddPattern_ReplacesCorrectly()
        {
            SeedDdddPattern();

            string result = PatternEngine.Process("C200521-001", "DDDD-01");
            Assert.Equal("FE21-01-01", result);
        }

        [Fact]
        public void Process_PatternWithNullName_Skipped()
        {
            PatternStore.Patterns.Add(new Pattern { Name = null! });

            Assert.Equal("some text", PatternEngine.Process("barcode", "some text"));
        }

        [Fact]
        public void Process_PatternWithEmptyName_Skipped()
        {
            PatternStore.Patterns.Add(new Pattern { Name = "" });

            Assert.Equal("some text", PatternEngine.Process("barcode", "some text"));
        }

        // ── ProcessBlocks ───────────────────────────────────

        [Fact]
        public void ProcessBlocks_NullArray_ReturnsEmptyArray()
        {
            var result = PatternEngine.ProcessBlocks("barcode", null!);
            Assert.Empty(result);
        }

        [Fact]
        public void ProcessBlocks_EmptyArray_ReturnsEmptyArray()
        {
            var result = PatternEngine.ProcessBlocks("barcode", Array.Empty<string>());
            Assert.Empty(result);
        }

        [Fact]
        public void ProcessBlocks_ProcessesEachElement()
        {
            var pattern = new Pattern { Name = "CCCC" };
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 999,
                TransformRule = TransformRuleType.COPY
            });
            PatternStore.Patterns.Add(pattern);

            var blocks = new[] { "CCCC line", "no pattern", "CCCC end" };
            var result = PatternEngine.ProcessBlocks("XYZ-001", blocks);

            Assert.Equal(3, result.Length);
            Assert.Equal("XYZ-001 line", result[0]);
            Assert.Equal("no pattern", result[1]);
            Assert.Equal("XYZ-001 end", result[2]);
        }

        private void SeedDdddPattern()
        {
            var dddd = new Pattern { Name = "DDDD" };
            dddd.Rules.AddRange(new[]
            {
                new Rule { SourceStart = 1,  SourceEnd = 1,  TransformRule = TransformRuleType.DELETE },
                new Rule { SourceStart = 2,  SourceEnd = 3,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "15" },
                new Rule { SourceStart = 4,  SourceEnd = 5,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "1" },
                new Rule { SourceStart = 6,  SourceEnd = 7,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 8,  SourceEnd = 8,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 9,  SourceEnd = 11, TransformRule = TransformRuleType.TAKE_RIGHT, Parameter = "2" },
            });
            PatternStore.Patterns.Add(dddd);
        }
    }
}
