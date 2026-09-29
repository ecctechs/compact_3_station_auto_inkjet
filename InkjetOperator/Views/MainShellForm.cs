using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class MainShellForm : AntdUI.Window
{
    private static readonly System.Drawing.Color ActiveTab = DesignTokens.PrimaryBlue;
    private static readonly System.Drawing.Color InactiveTab = DesignTokens.Inactive;

    private AntdUI.Button[] _visibleTabs = [];

    public MainShellForm()
    {
        InitializeComponent();
        ApplyMenuLevel();
        ApplyProgramTitle();
        Load += MainShellForm_Load;

        titleBar.MinimizeRequested += (_, _) => Min();
        titleBar.CloseRequested += (_, _) => Close();
        Resize += (_, _) => StayMaximized();

        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        btnLang.Click += (_, _) => ToggleLanguage();
        ApplyLanguage();
    }

    private void ApplyProgramTitle()
    {
        var title = Services.StationService.ProgramTitle;

        titleBar.TitleText = title;
        Text = title;
    }

    private void ToggleLanguage()
    {
        Services.LanguageService.Toggle();
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Services.LanguageService.Apply(this);

        btnLang.Text = Services.LanguageService.IsThai ? "ไทย" : "EN";
    }

    protected override void WndProc(ref Message m)
    {
        FullScreenMaximize.Handle(this, ref m);
        base.WndProc(ref m);
    }

    private bool _forcingFullScreen;

    private void StayMaximized()
    {
        if (_forcingFullScreen) return;

        if (WindowState == FormWindowState.Minimized) return;

        var work = Screen.FromHandle(Handle).WorkingArea;
        if (WindowState == FormWindowState.Maximized
            && Bounds.Width >= work.Width && Bounds.Height >= work.Height) return;

        _forcingFullScreen = true;
        try
        {
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
        }
        finally
        {
            _forcingFullScreen = false;
        }
    }

    private async void MainShellForm_Load(object? sender, EventArgs e)
    {
        if (scanBarcodePage.Visible) scanBarcodePage.FocusBarcode();

        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);
        if (level <= 1 || level == 9)
            await settingPage.CheckAllStatusAsync();
    }

    private const int EditPatternTab = 2;

    private void ApplyMenuLevel() // เลือกเมนูและหน้าแรกตาม Mode
    {
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting };
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage };

        bool[] visible = level switch
        {
            0 => [true, false, false, true],
            1 => [false, true, true, true],
            3 => [false, true, false, true],    // ST3 — Order List + Setting
            9 => [false, false, false, true],   // โหมดทดสอบหน้างาน — เข้าได้เฉพาะ Setting
            _ => [true, true, true, true],
        };

        visible[EditPatternTab] = false;

        for (int i = 0; i < allTabs.Length; i++)
        {
            allTabs[i].Visible = visible[i];
            allPages[i].Visible = visible[i];
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i]
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute;
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 250F : 0F;
        }

        _visibleTabs = allTabs.Where((_, i) => visible[i]).ToArray();

        if (_visibleTabs.Length > 0)
        {
            var firstTab = _visibleTabs[0];
            var firstPage = allPages[Array.IndexOf(allTabs, firstTab)];
            firstPage.BringToFront();
            SetActiveTab(firstTab);
        }
    }

    private void SetActiveTab(AntdUI.Button active)
    {
        foreach (var b in _visibleTabs)
        {
            var color = b == active ? ActiveTab : InactiveTab;
            b.DefaultBack = color;
        }
    }

    private void btnInputOrder_Click(object sender, EventArgs e)
    {
        scanBarcodePage.BringToFront();
        SetActiveTab(btnInputOrder);
        scanBarcodePage.FocusBarcode();
    }

    private void btnOrderList_Click(object sender, EventArgs e)
    {
        orderListPage.BringToFront();
        SetActiveTab(btnOrderList);
    }

    private void btnEditPattern_Click(object sender, EventArgs e)
    {
        editPatternPage.BringToFront();
        SetActiveTab(btnEditPattern);
    }

    private void btnSetting_Click(object sender, EventArgs e)
    {
        settingPage.BringToFront();
        SetActiveTab(btnSetting);
    }
}
