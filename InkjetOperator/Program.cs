using System.Globalization;
using InkjetOperator.Services;

namespace InkjetOperator;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Must be the very first call. It applies <ApplicationHighDpiMode>,
        // <ApplicationDefaultFont>, EnableVisualStyles() and
        // SetCompatibleTextRenderingDefault(false) from InkjetOperator.csproj.
        // Process DPI awareness can only be set before the first window exists,
        // and AntdUI caches Config.Dpi the first time anything reads it - so this
        // has to happen before ConfigureAntdUi() touches the library.
        ApplicationConfiguration.Initialize();

        UseGregorianYears();
        Services.LanguageService.Init();
        ConfigureAntdUi();

        // Load local transform patterns (patterns.xml next to the exe).
        string patternsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patterns.xml");
        PatternStore.Load(patternsPath);
        PatternStore.SeedDefaults(patternsPath);

        Application.Run(new Views.MainShellForm());
    }

    /// <summary>
    /// Keep Thai names but count years the western way.
    /// <para>
    /// .NET pairs th-TH with <see cref="ThaiBuddhistCalendar"/>, so a plain
    /// <c>DateTime.ToString("dd/MM/yyyy")</c> on these machines prints 2569 rather
    /// than 2026. The backend, the barcodes and the Order List columns all carry
    /// AD years, so one BE year showing up inside a date picker reads as a
    /// different date entirely. Swapping just the calendar keeps Thai month and day
    /// names everywhere they are used.
    /// </para>
    /// </summary>
    private static void UseGregorianYears()
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        if (culture.DateTimeFormat.Calendar is GregorianCalendar) return;

        var gregorian = culture.OptionalCalendars.OfType<GregorianCalendar>().FirstOrDefault();
        if (gregorian == null) return;

        culture.DateTimeFormat.Calendar = gregorian;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
    }

    /// <summary>
    /// Global AntdUI settings. Everything here has to be applied before the first
    /// AntdUI control is constructed, so it runs straight after
    /// <see cref="ApplicationConfiguration.Initialize"/> in <c>Main</c>.
    /// <para>
    /// Mirrors the configuration used by the NastoKeyence reference project. The
    /// application font is deliberately not set here: <c>AntdUI.Config.Font</c> is
    /// marked <c>[Obsolete]</c> in 2.4.3, so the font comes from
    /// <c>&lt;ApplicationDefaultFont&gt;</c> in the .csproj instead.
    /// </para>
    /// </summary>
    private static void ConfigureAntdUi()
    {
        // Build the icon table first - see InitAntdUiIconDb() for why.
        InitAntdUiIconDb();

        // AntdUI ships Chinese strings by default - the calendar popup and the
        // table filter would otherwise render as Chinese. Has to be set before the
        // first control is built, because the pickers read it when they are created.
        AntdUI.Localization.Provider = new Theme.ThaiLocalization();

        AntdUI.Config.Animation = true;

        // Show Message/Notification inside the application window instead of as
        // separate desktop-level windows floating over whatever else is on screen.
        AntdUI.Config.ShowInWindow = true;
        AntdUI.Config.ShowInWindowByMessage = true;
        AntdUI.Config.ShowInWindowByNotification = true;

        // AntdUI splits a string into runs so it can draw emoji in a separate font.
        // The splitter drops combining characters, and this UI has no emoji in it,
        // so drawing each string in one pass is both safer and cheaper.
        AntdUI.Config.EmojiEnabled = false;

        // Draw glyphs as antialiased outlines (GraphicsPath) rather than through
        // GDI+ DrawString - this is what keeps AntdUI text even at fractional
        // display scaling instead of showing uneven stems.
        AntdUI.Config.TextRenderingHighQuality = true;

        AntdUI.Config.Mode = AntdUI.TMode.Light;
    }

    /// <summary>
    /// AntdUI 2.4.3's SvgDb (built-in SVG icon table) builds a culture-sensitive,
    /// case-insensitive dictionary in its static constructor. Under some cultures
    /// (e.g. th-TH) two icon-name keys collide and the type initializer throws
    /// "An item with the same key has already been added". Force that one-time
    /// initialization to run under the invariant culture, then restore the
    /// original culture so the rest of the app is unaffected.
    /// </summary>
    private static void InitAntdUiIconDb()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var svgDb = typeof(AntdUI.Button).Assembly.GetType("AntdUI.SvgDb");
            if (svgDb != null)
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(svgDb.TypeHandle);
        }
        catch
        {
            // Best-effort: if AntdUI internals change, fall through — controls will
            // still attempt their own initialization.
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
