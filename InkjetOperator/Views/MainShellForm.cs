using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// Application main window (shell): top menu bar + content host. The pages are
/// placed in pnlContent (Dock=Fill) at design time; menu buttons switch between
/// them by z-order (BringToFront) and update the active-tab colour. The Input
/// Order tab shows the Scan Barcode page. Only navigation/tab-state code lives
/// here — no other business logic, no runtime control creation, no custom paint.
/// </summary>
public partial class MainShellForm : AntdUI.Window
{
    private static readonly System.Drawing.Color ActiveTab = DesignTokens.PrimaryBlue;
    private static readonly System.Drawing.Color InactiveTab = DesignTokens.Inactive;

    private AntdUI.Button[] _visibleTabs = [];

    public MainShellForm()
    {
        InitializeComponent();
        ApplyMenuLevel();
        Load += MainShellForm_Load;

        // AntdUI.Window draws no system chrome, so titleBar is what closes,
        // minimises, maximises and moves the window now.
        titleBar.MinimizeRequested += (_, _) => Min();
        titleBar.MaximizeRequested += (_, _) => ToggleMaximize();
        titleBar.CloseRequested += (_, _) => Close();
        titleBar.DragRequested += (_, _) => DraggableMouseDown();

        Resize += (_, _) => titleBar.SetMaximized(WindowState == FormWindowState.Maximized);
        titleBar.SetMaximized(WindowState == FormWindowState.Maximized);
    }

    private void ToggleMaximize()
    {
        if (WindowState == FormWindowState.Maximized)
        {
            WindowState = FormWindowState.Normal;
        }
        else
        {
            Max();
        }

        titleBar.SetMaximized(WindowState == FormWindowState.Maximized);
    }

    private async void MainShellForm_Load(object? sender, EventArgs e)
    {
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);
        if (level <= 1 || level == 9)
            await settingPage.CheckAllStatusAsync();
    }

    private void ApplyMenuLevel()
    {
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting };
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage };

        bool[] visible = level switch
        {
            0 => [true, false, false, true],
            1 => [false, true, true, true],
            9 => [false, false, false, true],   // โหมดทดสอบหน้างาน — เข้าได้เฉพาะ Setting
            _ => [true, true, true, true],
        };

        for (int i = 0; i < allTabs.Length; i++)
        {
            allTabs[i].Visible = visible[i];
            allPages[i].Visible = visible[i];
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i]
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute;
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 300F : 0F;
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
