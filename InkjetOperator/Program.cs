using System.Globalization;
using InkjetOperator.Services;

namespace InkjetOperator;

static class Program
{
    /// <summary>
    /// ชื่อล็อกที่บอกว่าโปรแกรมเปิดอยู่แล้ว — ตั้งชื่อเฉพาะเจาะจงกันไปชนกับโปรแกรมอื่น
    /// </summary>
    private const string SingleInstanceName = "CompactInkjet.Operator.SingleInstance";

    [STAThread]
    static void Main() // Flow 1: จุดเริ่มโปรแกรม
    {
        // เปิดได้ทีละตัวเท่านั้น
        //
        // ไอคอนบนเดสก์ท็อปกดรัว ๆ ได้ง่ายมากบนจอสัมผัส และการเปิดโปรแกรมใช้เวลา
        // หลายวินาที (รอ backend ตอบก่อนถึงจะขึ้นหน้าจอ) ระหว่างนั้นไม่มีอะไรบอกว่า
        // กำลังเปิดอยู่ พนักงานจึงกดซ้ำ แล้วได้โปรแกรมซ้อนกันหลายตัว
        //
        // ตัวที่สองเป็นปัญหาจริง ไม่ใช่แค่รก — ทั้งสองตัวเฝ้าบิตปุ่มกดหน้างานคนละ
        // ตัว กดปุ่มครั้งเดียวจึงถูกนับสองรอบ และตัวที่สองยังไปแย่งเปิด backend
        // ทับตัวแรกอีก
        //
        // ปล่อยล็อกตอน Main จบ ไม่ว่าจะปิดตามปกติหรือหลุดกลางคัน
        using var single = new Mutex(initiallyOwned: true, SingleInstanceName, out bool isFirst); // กันเปิดโปรแกรมซ้อนกัน
        if (!isFirst) // มีโปรแกรมตัวเดิมเปิดอยู่แล้ว
        {
            BringRunningInstanceToFront(); // แสดงตัวเดิมแทนการเปิดซ้ำ
            return; // ใช้โปรแกรมตัวเดิม จึงไม่เปิดหน้าหลักซ้ำ
        }

        // Must be the very first call. It applies <ApplicationHighDpiMode>,
        // <ApplicationDefaultFont>, EnableVisualStyles() and
        // SetCompatibleTextRenderingDefault(false) from InkjetOperator.csproj.
        // Process DPI awareness can only be set before the first window exists,
        // and AntdUI caches Config.Dpi the first time anything reads it - so this
        // has to happen before ConfigureAntdUi() touches the library.
        ApplicationConfiguration.Initialize(); // ตั้งค่าพื้นฐาน WinForms ก่อนสร้างหน้าจอ

        UseGregorianYears(); // ตั้งการแสดงปีเป็น ค.ศ.
        Services.LanguageService.Init(); // โหลดภาษาที่เลือกไว้
        ConfigureAntdUi(); // ตั้งค่าชุดคอนโทรล AntdUI

        // Load local transform patterns (patterns.xml next to the exe).
        string patternsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patterns.xml"); // หาไฟล์รูปแบบแปลงข้อความข้าง EXE
        PatternStore.Load(patternsPath); // โหลดรูปแบบแปลงข้อความ
        PatternStore.SeedDefaults(patternsPath); // เติมรูปแบบเริ่มต้นที่ยังไม่มี

        WarnIfSettingsReadOnly(); // เตือนถ้าไฟล์ตั้งค่าเขียนไม่ได้
        StartBackendIfNeeded(); // เปิด Backend ตามค่าที่ตั้งไว้

        StartHealthMonitorIfDevMode(); // เปิดตัวเฝ้าสถานะเฉพาะโหมดทดสอบ

        Application.Run(new Views.MainShellForm()); // เปิดหน้าหลัก ให้ ApplyMenuLevel() เลือกหน้าตามโหมด
        HealthMonitor.Stop(); // ปิดตัวเฝ้าสถานะเมื่อโปรแกรมจบ
    }

    /// <summary>
    /// เฝ้าสถานะไฟล์ โฟลเดอร์ และเครื่องปลายทาง เฉพาะโหมดทดสอบ
    ///
    /// <para>
    /// หน้าที่แสดงผลเปิดเฉพาะโหมดทดสอบ เครื่องที่ใช้งานจริงจึงไม่มีใครดูผลเลย
    /// การเช็คทุก 30 วินาทีบนเครื่องพวกนั้นคือการเปิดสายเข้าเครื่องพิมพ์กับ PLC
    /// ทิ้งเปล่า ๆ ตลอดกะ ไม่มีประโยชน์และเป็นภาระของปลายทางโดยไม่จำเป็น
    /// </para>
    /// </summary>
    private static void StartHealthMonitorIfDevMode()
    {
        var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
        if (int.TryParse(raw, out var level) && level == DevMenuLevel)
            HealthMonitor.Start();
    }

    /// <summary>ระดับเมนูของโหมดทดสอบ — ตัวเดียวกับที่หน้า Setting และ Order Detail ใช้</summary>
    private const int DevMenuLevel = 99;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>คืนหน้าต่างจากที่พับไว้ที่แถบงาน ถ้าไม่ได้พับอยู่ก็ไม่มีผลอะไร</summary>
    private const int SW_RESTORE = 9;

    /// <summary>
    /// ดึงตัวที่เปิดอยู่แล้วขึ้นมาแทนการเปิดตัวใหม่
    ///
    /// <para>
    /// ต้องทำให้เห็นอะไรสักอย่าง ไม่งั้นพนักงานกดไอคอนแล้วเงียบสนิทจะนึกว่าไม่ติด
    /// แล้วกดซ้ำอีก — ปลุกตัวเดิมขึ้นมาคือคำตอบที่ตรงกับสิ่งที่เขาต้องการอยู่แล้ว
    /// </para>
    /// <para>
    /// หาไม่เจอก็เงียบไป ดีกว่าเด้งกล่องบอกว่าเปิดอยู่แล้วซึ่งต้องกดปิดอีกที
    /// </para>
    /// </summary>
    private static void BringRunningInstanceToFront()
    {
        try
        {
            using var me = System.Diagnostics.Process.GetCurrentProcess();
            foreach (var other in System.Diagnostics.Process.GetProcessesByName(me.ProcessName))
            {
                using (other)
                {
                    if (other.Id == me.Id) continue;

                    var window = other.MainWindowHandle;
                    if (window == IntPtr.Zero) continue;

                    ShowWindow(window, SW_RESTORE);
                    SetForegroundWindow(window);
                    return;
                }
            }
        }
        catch
        {
            // อ่านรายการโปรเซสไม่ได้ก็ไม่ต้องทำอะไร ตัวที่สองปิดตัวเองอยู่ดี
        }
    }

    /// <summary>
    /// เปิด backend ให้เองก่อนเปิดหน้าจอ ถ้ามันอยู่เครื่องเดียวกันและยังไม่ได้เปิด
    /// <para>
    /// รอจนกว่าจะตอบก่อนค่อยเปิดหน้าจอ เพราะทุกหน้าเรียก API ตั้งแต่โหลด
    /// ถ้าปล่อยให้หน้าจอขึ้นก่อนจะเห็นตารางว่างแล้วค่อย ๆ มีข้อมูลโผล่ ดูเหมือนพัง
    /// ปกติ node ขึ้นภายในไม่กี่วินาที
    /// </para>
    /// <para>
    /// พลาดแล้วยังเปิดโปรแกรมต่อ ไม่ปิดตัวเอง — อาจแค่ตั้งค่ายังไม่ครบ และหน้า
    /// Setting ยังต้องเข้าได้เพื่อไปแก้
    /// </para>
    /// </summary>
    private static void StartBackendIfNeeded()
    {
        var problem = BackendLauncher.EnsureRunningAsync().GetAwaiter().GetResult();
        if (problem == null) return;

        MessageBox.Show(
            $"{problem}\n\n"
            + "โปรแกรมยังเปิดใช้งานได้ แต่จะดึงข้อมูลไม่ได้จนกว่า backend จะทำงาน",
            "เปิด backend ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        // Draw glyphs as antialiased outlines (GraphicsPath) rather than through
        // GDI+ DrawString - this is what keeps AntdUI text even at fractional
        // display scaling instead of showing uneven stems.
        AntdUI.Config.TextRenderingHighQuality = true;

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
