using InkjetOperator.Models;

namespace InkjetOperator.Tests
{
    public class RuleTests
    {
        private static Rule MakeRule(
            TransformRuleType type,
            int start = 1, int end = 999,
            string parameter = "",
            bool active = true)
        {
            return new Rule
            {
                TransformRule = type,
                SourceStart = start,
                SourceEnd = end,
                Parameter = parameter,
                IsActive = active
            };
        }

        // ── IsActive ────────────────────────────────────────

        [Fact]
        public void Apply_InactiveRule_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.COPY, active: false);
            Assert.Equal(string.Empty, rule.Apply("anything"));
        }

        // ── DELETE ──────────────────────────────────────────

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData(null)]
        public void Apply_Delete_ReturnsEmpty(string? input)
        {
            var rule = MakeRule(TransformRuleType.DELETE);
            Assert.Equal(string.Empty, rule.Apply(input!));
        }

        // ── COPY ────────────────────────────────────────────

        [Theory]
        [InlineData("ABCDEF", 1, 999, "ABCDEF")]
        [InlineData("ABCDEF", 2, 4, "BCD")]
        [InlineData("ABCDEF", 1, 1, "A")]
        [InlineData("ABCDEF", 6, 6, "F")]
        public void Apply_Copy_ExtractsSubstring(string input, int start, int end, string expected)
        {
            var rule = MakeRule(TransformRuleType.COPY, start, end);
            Assert.Equal(expected, rule.Apply(input));
        }

        [Fact]
        public void Apply_Copy_NullInput_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.COPY, 1, 3);
            Assert.Equal(string.Empty, rule.Apply(null!));
        }

        [Fact]
        public void Apply_Copy_EmptyInput_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.COPY, 1, 3);
            Assert.Equal(string.Empty, rule.Apply(""));
        }

        [Fact]
        public void Apply_Copy_StartBeyondLength_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.COPY, 10, 15);
            Assert.Equal(string.Empty, rule.Apply("ABC"));
        }

        [Fact]
        public void Apply_Copy_EndBeyondLength_ClampedToLength()
        {
            var rule = MakeRule(TransformRuleType.COPY, 2, 100);
            Assert.Equal("BCDEF", rule.Apply("ABCDEF"));
        }

        [Fact]
        public void Apply_Copy_EndLessThanStart_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.COPY, 5, 2);
            Assert.Equal(string.Empty, rule.Apply("ABCDEF"));
        }

        // ── FIX_TEXT ────────────────────────────────────────

        [Fact]
        public void Apply_FixText_ReturnsParameter()
        {
            var rule = MakeRule(TransformRuleType.FIX_TEXT, parameter: "HELLO");
            Assert.Equal("HELLO", rule.Apply("ignored"));
        }

        [Fact]
        public void Apply_FixText_NullParameter_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.FIX_TEXT);
            rule.Parameter = null!;
            Assert.Equal(string.Empty, rule.Apply("ignored"));
        }

        // ── PAD_LEFT ────────────────────────────────────────

        [Fact]
        public void Apply_PadLeft_PrependsParameter()
        {
            var rule = MakeRule(TransformRuleType.PAD_LEFT, 1, 3, parameter: ">>>");
            Assert.Equal(">>>ABC", rule.Apply("ABCDEF"));
        }

        [Fact]
        public void Apply_PadLeft_NullParameter_OnlyExtract()
        {
            var rule = MakeRule(TransformRuleType.PAD_LEFT, 1, 3);
            rule.Parameter = null!;
            Assert.Equal("ABC", rule.Apply("ABCDEF"));
        }

        // ── PAD_RIGHT ───────────────────────────────────────

        [Fact]
        public void Apply_PadRight_AppendsParameter()
        {
            var rule = MakeRule(TransformRuleType.PAD_RIGHT, 1, 3, parameter: "<<<");
            Assert.Equal("ABC<<<", rule.Apply("ABCDEF"));
        }

        // ── AZ_UPPER ───────────────────────────────────────

        [Theory]
        [InlineData("20", "15", "F")]   // offset 5 → F
        [InlineData("05", "1", "E")]    // offset 4 → E
        [InlineData("01", "1", "A")]    // offset 0 → A
        [InlineData("26", "0", "AA")]   // offset 26 → AA
        [InlineData("01", "0", "B")]    // offset 1 → B
        [InlineData("00", "0", "A")]    // offset 0 → A
        public void Apply_AzUpper_ConvertsToUpperLetter(string input, string parameter, string expected)
        {
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 1, 999, parameter: parameter);
            Assert.Equal(expected, rule.Apply(input));
        }

        [Theory]
        [InlineData("05", "1", "e")]    // offset 4 → e (lowercase)
        [InlineData("01", "1", "a")]    // offset 0 → a
        public void Apply_AzLower_ConvertsToLowerLetter(string input, string parameter, string expected)
        {
            var rule = MakeRule(TransformRuleType.AZ_LOWER, 1, 999, parameter: parameter);
            Assert.Equal(expected, rule.Apply(input));
        }

        [Fact]
        public void Apply_AzUpper_NonNumericInput_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 1, 999, parameter: "0");
            Assert.Equal(string.Empty, rule.Apply("XY"));
        }

        [Fact]
        public void Apply_AzUpper_NegativeOffset_ReturnsEmpty()
        {
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 1, 999, parameter: "99");
            Assert.Equal(string.Empty, rule.Apply("05"));
        }

        [Fact]
        public void Apply_AzUpper_NullParameter_UsesBaseZero()
        {
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 1, 999);
            rule.Parameter = null!;
            Assert.Equal("D", rule.Apply("03"));
        }

        [Fact]
        public void Apply_AzUpper_EmptyParameter_UsesBaseZero()
        {
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 1, 999, parameter: "");
            Assert.Equal("D", rule.Apply("03"));
        }

        // ── TAKE_RIGHT ─────────────────────────────────────

        [Theory]
        [InlineData("ABCDEF", "3", "DEF")]
        [InlineData("ABCDEF", "1", "F")]
        [InlineData("ABCDEF", "6", "ABCDEF")]
        [InlineData("ABCDEF", "99", "ABCDEF")]  // n > length → full string
        [InlineData("001", "2", "01")]
        public void Apply_TakeRight_TakesLastNChars(string input, string param, string expected)
        {
            var rule = MakeRule(TransformRuleType.TAKE_RIGHT, 1, 999, parameter: param);
            Assert.Equal(expected, rule.Apply(input));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("abc")]
        [InlineData("")]
        public void Apply_TakeRight_InvalidParameter_ReturnsFullExtract(string param)
        {
            var rule = MakeRule(TransformRuleType.TAKE_RIGHT, 1, 999, parameter: param);
            Assert.Equal("ABCDEF", rule.Apply("ABCDEF"));
        }

        // ── TAKE_LEFT ───────────────────────────────────────

        [Theory]
        [InlineData("ABCDEF", "3", "ABC")]
        [InlineData("ABCDEF", "1", "A")]
        [InlineData("ABCDEF", "99", "ABCDEF")]
        public void Apply_TakeLeft_TakesFirstNChars(string input, string param, string expected)
        {
            var rule = MakeRule(TransformRuleType.TAKE_LEFT, 1, 999, parameter: param);
            Assert.Equal(expected, rule.Apply(input));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        public void Apply_TakeLeft_InvalidParameter_ReturnsFullExtract(string param)
        {
            var rule = MakeRule(TransformRuleType.TAKE_LEFT, 1, 999, parameter: param);
            Assert.Equal("ABCDEF", rule.Apply("ABCDEF"));
        }

        // ── Production scenario: DDDD rules individually ────

        [Fact]
        public void Apply_DdddRule2_Barcode_C200521_ExtractsF()
        {
            // SourceStart=2, SourceEnd=3 from "C200521-001" → "20", offset=20-15=5 → "F"
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 2, 3, parameter: "15");
            Assert.Equal("F", rule.Apply("C200521-001"));
        }

        [Fact]
        public void Apply_DdddRule3_Barcode_C200521_ExtractsE()
        {
            // SourceStart=4, SourceEnd=5 → "05", offset=5-1=4 → "E"
            var rule = MakeRule(TransformRuleType.AZ_UPPER, 4, 5, parameter: "1");
            Assert.Equal("E", rule.Apply("C200521-001"));
        }
    }
}
