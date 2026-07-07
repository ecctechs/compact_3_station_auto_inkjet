namespace InkjetOperator.Tests
{
    public class AppConfigTests
    {
        // ── Mode 0: input + setting only ────────────────────

        [Theory]
        [InlineData("input", true)]
        [InlineData("setting", true)]
        [InlineData("order", false)]
        [InlineData("edit", false)]
        [InlineData("bot", false)]
        [InlineData("st3", false)]
        public void ShouldShowMenu_Mode0(string menu, bool expected)
        {
            var config = new AppConfig { MenuMode = 0 };
            Assert.Equal(expected, config.ShouldShowMenu(menu));
        }

        // ── Mode 1: order + edit + setting ──────────────────

        [Theory]
        [InlineData("order", true)]
        [InlineData("edit", true)]
        [InlineData("setting", true)]
        [InlineData("input", false)]
        [InlineData("bot", false)]
        [InlineData("st3", false)]
        public void ShouldShowMenu_Mode1(string menu, bool expected)
        {
            var config = new AppConfig { MenuMode = 1 };
            Assert.Equal(expected, config.ShouldShowMenu(menu));
        }

        // ── Mode 2: bot + setting ───────────────────────────

        [Theory]
        [InlineData("bot", true)]
        [InlineData("setting", true)]
        [InlineData("input", false)]
        [InlineData("order", false)]
        [InlineData("edit", false)]
        [InlineData("st3", false)]
        public void ShouldShowMenu_Mode2(string menu, bool expected)
        {
            var config = new AppConfig { MenuMode = 2 };
            Assert.Equal(expected, config.ShouldShowMenu(menu));
        }

        // ── Mode 3: st3 + setting ───────────────────────────

        [Theory]
        [InlineData("st3", true)]
        [InlineData("setting", true)]
        [InlineData("bot", false)]
        [InlineData("input", false)]
        [InlineData("order", false)]
        [InlineData("edit", false)]
        public void ShouldShowMenu_Mode3(string menu, bool expected)
        {
            var config = new AppConfig { MenuMode = 3 };
            Assert.Equal(expected, config.ShouldShowMenu(menu));
        }

        // ── Mode 4: bot + setting ───────────────────────────

        [Theory]
        [InlineData("bot", true)]
        [InlineData("setting", true)]
        [InlineData("st3", false)]
        [InlineData("input", false)]
        [InlineData("order", false)]
        public void ShouldShowMenu_Mode4(string menu, bool expected)
        {
            var config = new AppConfig { MenuMode = 4 };
            Assert.Equal(expected, config.ShouldShowMenu(menu));
        }

        // ── Mode 5: everything true ─────────────────────────

        [Theory]
        [InlineData("input")]
        [InlineData("order")]
        [InlineData("edit")]
        [InlineData("bot")]
        [InlineData("st3")]
        [InlineData("setting")]
        [InlineData("anything")]
        public void ShouldShowMenu_Mode5_AllTrue(string menu)
        {
            var config = new AppConfig { MenuMode = 5 };
            Assert.True(config.ShouldShowMenu(menu));
        }

        // ── Invalid mode: everything false ──────────────────

        [Theory]
        [InlineData(99)]
        [InlineData(-1)]
        [InlineData(6)]
        public void ShouldShowMenu_InvalidMode_AllFalse(int mode)
        {
            var config = new AppConfig { MenuMode = mode };
            Assert.False(config.ShouldShowMenu("input"));
            Assert.False(config.ShouldShowMenu("order"));
            Assert.False(config.ShouldShowMenu("setting"));
            Assert.False(config.ShouldShowMenu("bot"));
            Assert.False(config.ShouldShowMenu("st3"));
        }

        // ── Case insensitive ────────────────────────────────

        [Theory]
        [InlineData("INPUT")]
        [InlineData("Input")]
        [InlineData("input")]
        [InlineData("iNpUt")]
        public void ShouldShowMenu_CaseInsensitive(string menu)
        {
            var config = new AppConfig { MenuMode = 0 };
            Assert.True(config.ShouldShowMenu(menu));
        }
    }
}
