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

        WarnIfSettingsReadOnly();

        Application.Run(new Views.MainShellForm());
    }

    /// <summary>
    /// เตือนตั้งแต่เปิดโปรแกรมถ้าบันทึกค่าไม่ได้
    /// <para>
    /// เดิมปัญหานี้เงียบสนิท — ผู้ใช้กด Save ในหน้า Setting แล้วเห็นว่าสำเร็จ
    /// แต่ค่าหายหมดตอนเปิดใหม่ กว่าจะรู้ว่าเป็นเรื่องสิทธิ์เขียนไฟล์ก็เสียเวลาไปมาก
    /// </para>
    /// </summary>
    private static void WarnIfSettingsReadOnly()
    {
        var problem = Services.AppSettingsFile.CheckWritable();
        if (problem == null) return;

        MessageBox.Show(
            $"บันทึกการตั้งค่าไม่ได้ที่\n{Services.AppSettingsFile.Folder}\n\n"
            + $"สาเหตุ: {problem}\n\n"
            + "ใช้งานโปรแกรมต่อได้ แต่ค่าที่ตั้งในหน้า Setting จะไม่ถูกบันทึก\n"
            + "แจ้งผู้ดูแลให้เปิดสิทธิ์เขียนโฟลเดอร์นี้",
            "ตั้งค่าไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        ConfigureTextRendering();

        // AntdUI draws the header sort arrows with TextQuaternary while a column is
        // unsorted and with Primary while it is sorted. Both defaults are dark, and
        // every table in this app uses a near-black header, so the arrows were only
        // legible under the pale hover background - which read as "the marker
        // disappears when the mouse leaves". Scope brighter colours to Table so the
        // state is readable at rest: light grey = not sorted, white = sorting by this
        // column. TextQuaternary also tints text in disabled table rows, which this
        // app does not use.
        AntdUI.Style.Set(AntdUI.Colour.TextQuaternary, System.Drawing.Color.FromArgb(150, 150, 150), nameof(AntdUI.Table));
        AntdUI.Style.Set(AntdUI.Colour.Primary, System.Drawing.Color.White, nameof(AntdUI.Table));

        AntdUI.Config.Mode = AntdUI.TMode.Light;
    }

    /// <summary>
    /// เลือกวิธีวาดตัวหนังสือตามจอที่กำลังใช้จริง
    ///
    /// <para>
    /// จอที่สเกลลงตัว (100% อย่าง panel PC 1920x1080 ที่หน้างานใช้ หรือ 200%) ได้ตัวหนังสือคม
    /// ที่สุดจาก ClearType ของ Windows เพราะมันจัดเส้นให้ลงกริดพิกเซลพอดีและใช้
    /// subpixel ช่วย ตัวอักษรจึงคมเหมือนโปรแกรมอื่นในเครื่อง
    /// </para>
    /// <para>
    /// ส่วนจอที่ตั้งสเกลไม่ลงตัว (125% / 150% อย่างจอ 4K ที่ใช้พัฒนา) การจัดลงกริด
    /// แบบนั้นกลับทำให้เส้นหนาบางไม่เท่ากัน จึงสลับไปวาดเป็นเส้นขอบแบบ antialias แทน
    /// ซึ่งเสียความคมไปบ้างแต่ได้ความสม่ำเสมอ
    /// </para>
    /// <para>
    /// เดิมบังคับใช้แบบหลังตลอด ซึ่งเลือกไว้ตอนพัฒนาบนจอ 4K ที่ย่อขยาย พอเอาไป
    /// รันบน panel PC ที่ 100% ตัวหนังสือจึงดูฟุ้งกว่าที่ควรทั้งโปรแกรม
    /// </para>
    /// </summary>
    private static void ConfigureTextRendering()
    {
        // เกณฑ์คือ "สเกลลงตัวไหม" ไม่ใช่ "100% หรือเปล่า" — ที่ 200% ตัวอักษรก็ยัง
        // ลงกริดพิกเซลพอดี ClearType จึงยังคมที่สุด มีแต่สเกลเศษอย่าง 125% / 150%
        // ที่ทำให้เส้นหนาบางไม่เท่ากันจนต้องยอมแลกไปใช้เส้นขอบ antialias แทน
        float dpi = ScreenDpi();
        bool wholeScale = Math.Abs(dpi % 96f) < 0.5f;

        AntdUI.Config.TextRenderingHighQuality = !wholeScale;

        if (wholeScale)
            AntdUI.Config.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    }

    /// <summary>
    /// DPI ของจอหลัก — 96 คือ 100% · ต้องเรียกหลัง ApplicationConfiguration.Initialize()
    /// ไม่งั้นได้ค่าที่ Windows หลอกให้โปรแกรมที่ยังไม่ประกาศ DPI awareness เห็น
    /// </summary>
    private static float ScreenDpi()
    {
        try
        {
            using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            return g.DpiX;
        }
        catch
        {
            // อ่านไม่ได้ก็ถือว่าเป็นจอปกติ — ตรงกับเครื่องที่หน้างานใช้
            return 96f;
        }
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
