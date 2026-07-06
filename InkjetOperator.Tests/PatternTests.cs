using InkjetOperator.Models;

namespace InkjetOperator.Tests
{
    public class PatternTests
    {
        // ── Empty / null cases ──────────────────────────────

        [Fact]
        public void Apply_NoRules_ReturnsEmpty()
        {
            var pattern = new Pattern();
            Assert.Equal(string.Empty, pattern.Apply("anything"));
        }

        [Fact]
        public void Apply_NullInput_DoesNotThrow()
        {
            var pattern = new Pattern();
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 999,
                TransformRule = TransformRuleType.COPY
            });
            Assert.Equal(string.Empty, pattern.Apply(null!));
        }

        // ── CCCC pattern (copy entire barcode) ──────────────

        [Theory]
        [InlineData("C240801-027")]
        [InlineData("XYZ-123-456")]
        [InlineData("SIMPLE")]
        public void Apply_CcccPattern_CopiesFullBarcode(string barcode)
        {
            var pattern = new Pattern { Name = "CCCC" };
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 999,
                TransformRule = TransformRuleType.COPY
            });

            Assert.Equal(barcode, pattern.Apply(barcode));
        }

        // ── DDDD pattern (production example) ───────────────

        [Fact]
        public void Apply_DdddPattern_C200521_001_ReturnsFE21Dash01()
        {
            var pattern = BuildDdddPattern();
            Assert.Equal("FE21-01", pattern.Apply("C200521-001"));
        }

        [Theory]
        [InlineData("C200101-001", "FA01-01")]
        [InlineData("C211231-099", "GL31-99")]
        public void Apply_DdddPattern_VariousInputs(string barcode, string expected)
        {
            var pattern = BuildDdddPattern();
            Assert.Equal(expected, pattern.Apply(barcode));
        }

        // Note: Pattern.Apply has a defensive try/catch around Rule.Apply,
        // but Rule.Apply is not virtual so we can't inject a throwing subclass.
        // The catch block is defensive — tested implicitly via integration.

        // ── Inactive rule in chain ──────────────────────────

        [Fact]
        public void Apply_InactiveRuleInChain_SkippedAsEmpty()
        {
            var pattern = new Pattern();
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 3,
                TransformRule = TransformRuleType.COPY
            });
            pattern.Rules.Add(new Rule
            {
                SourceStart = 4, SourceEnd = 6,
                TransformRule = TransformRuleType.COPY,
                IsActive = false
            });

            Assert.Equal("ABC", pattern.Apply("ABCDEF"));
        }

        // ── Multiple rules concat ───────────────────────────

        [Fact]
        public void Apply_MultipleRules_ConcatsResults()
        {
            var pattern = new Pattern();
            pattern.Rules.Add(new Rule
            {
                SourceStart = 1, SourceEnd = 3,
                TransformRule = TransformRuleType.COPY
            });
            pattern.Rules.Add(new Rule
            {
                TransformRule = TransformRuleType.FIX_TEXT,
                Parameter = "-"
            });
            pattern.Rules.Add(new Rule
            {
                SourceStart = 4, SourceEnd = 6,
                TransformRule = TransformRuleType.COPY
            });

            Assert.Equal("ABC-DEF", pattern.Apply("ABCDEF"));
        }

        // ── Helpers ─────────────────────────────────────────

        private static Pattern BuildDdddPattern()
        {
            var pattern = new Pattern { Name = "DDDD" };
            pattern.Rules.AddRange(new[]
            {
                new Rule { SourceStart = 1,  SourceEnd = 1,  TransformRule = TransformRuleType.DELETE },
                new Rule { SourceStart = 2,  SourceEnd = 3,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "15" },
                new Rule { SourceStart = 4,  SourceEnd = 5,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "1" },
                new Rule { SourceStart = 6,  SourceEnd = 7,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 8,  SourceEnd = 8,  TransformRule = TransformRuleType.COPY },
                new Rule { SourceStart = 9,  SourceEnd = 11, TransformRule = TransformRuleType.TAKE_RIGHT, Parameter = "2" },
            });
            return pattern;
        }

    }
}
