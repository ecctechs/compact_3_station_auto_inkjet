using InkjetOperator.Services;

namespace InkjetOperator.Tests
{
    public class LangTests : IDisposable
    {
        public LangTests()
        {
            // Reset to EN before each test via toggle if needed
            while (Lang.Current != "EN") Lang.Toggle();
        }

        public void Dispose()
        {
            // Reset to EN after each test
            while (Lang.Current != "EN") Lang.Toggle();
        }

        [Fact]
        public void Get_EnglishKey_ReturnsEnglishText()
        {
            Assert.Equal("Input Order", Lang.Get("menu.input_order"));
        }

        [Fact]
        public void Get_AfterToggle_ReturnsThaiText()
        {
            Lang.Toggle();
            Assert.Equal("สร้างงาน", Lang.Get("menu.input_order"));
        }

        [Fact]
        public void Get_DoubleToggle_ReturnsEnglishAgain()
        {
            Lang.Toggle();
            Lang.Toggle();
            Assert.Equal("Input Order", Lang.Get("menu.input_order"));
        }

        [Fact]
        public void Get_UnknownKey_ReturnsFallbackKey()
        {
            Assert.Equal("nonexistent.key", Lang.Get("nonexistent.key"));
        }

        [Fact]
        public void Format_WithArgs_ReplacesPlaceholders()
        {
            Assert.Equal("Error", Lang.Format("msg.error"));
        }

        [Fact]
        public void Format_SendError_WithArg()
        {
            string result = Lang.Format("msg.send_error", "timeout");
            Assert.Equal("Error: timeout", result);
        }

        [Fact]
        public void Format_PatternNotFound_WithArg()
        {
            string result = Lang.Format("msg.pattern_not_found", "XXXX");
            Assert.Equal("Pattern 'XXXX' not registered", result);
        }

        [Fact]
        public void Toggle_FiresLanguageChangedEvent()
        {
            bool fired = false;
            void handler() => fired = true;

            Lang.LanguageChanged += handler;
            try
            {
                Lang.Toggle();
                Assert.True(fired);
            }
            finally
            {
                Lang.LanguageChanged -= handler;
            }
        }

        [Fact]
        public void Current_DefaultIsEN()
        {
            Assert.Equal("EN", Lang.Current);
        }

        [Fact]
        public void Current_AfterToggle_IsTH()
        {
            Lang.Toggle();
            Assert.Equal("TH", Lang.Current);
        }
    }
}
