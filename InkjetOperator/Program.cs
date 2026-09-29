using System.Globalization;
using InkjetOperator.Services;

namespace InkjetOperator;

static class Program
{
    private const string SingleInstanceName = "CompactInkjet.Operator.SingleInstance";

    [STAThread] // เตรียมโปรแกรมแล้วเปิดหน้าหลัก
    static void Main()
    {
        using var single = new Mutex(initiallyOwned: true, SingleInstanceName, out bool isFirst);
        if (!isFirst)
        {
            BringRunningInstanceToFront();
            return;
        }

        ApplicationConfiguration.Initialize();

        UseGregorianYears();
        Services.LanguageService.Init();
        ConfigureAntdUi();

        PatternStore.Load();
        PatternStore.SeedDefaults();

        WarnIfSettingsReadOnly();
        StartBackendIfNeeded();

        StartHealthMonitorIfDevMode();

        Application.Run(new Views.MainShellForm());
        HealthMonitor.Stop();
    }

    private static void StartHealthMonitorIfDevMode()
    {
        var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
        if (int.TryParse(raw, out var level) && level == DevMenuLevel)
            HealthMonitor.Start();
    }

    private const int DevMenuLevel = 99;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

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
        }
    }

    private static void StartBackendIfNeeded()
    {
        var problem = BackendLauncher.EnsureRunningAsync().GetAwaiter().GetResult();
        if (problem == null) return;

        MessageBox.Show(
            $"{problem}\n\n"
            + "โปรแกรมยังเปิดใช้งานได้ แต่จะดึงข้อมูลไม่ได้จนกว่า backend จะทำงาน",
            "เปิด backend ไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

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

    private static void ConfigureAntdUi()
    {
        InitAntdUiIconDb();

        AntdUI.Localization.Provider = new Theme.ThaiLocalization();

        AntdUI.Config.Animation = true;

        AntdUI.Config.ShowInWindow = true;
        AntdUI.Config.ShowInWindowByMessage = true;
        AntdUI.Config.ShowInWindowByNotification = true;

        AntdUI.Config.EmojiEnabled = false;

        AntdUI.Config.TextRenderingHighQuality = true;

        AntdUI.Style.Set(AntdUI.Colour.TextQuaternary, System.Drawing.Color.FromArgb(150, 150, 150), nameof(AntdUI.Table));
        AntdUI.Style.Set(AntdUI.Colour.Primary, System.Drawing.Color.White, nameof(AntdUI.Table));

        AntdUI.Config.Mode = AntdUI.TMode.Light;
    }

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
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
