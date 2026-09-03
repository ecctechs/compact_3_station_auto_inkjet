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

        // จอหน้างานเป็น Full HD และใช้เต็มจอตลอด
        // พับหน้าจอลงแถบงานได้ตามปกติ แต่ย่อให้เล็กกว่าเต็มจอไม่ได้ เพราะเลย์เอาต์
        // ออกแบบไว้ที่ 1920x1080 — ถ้ามีอะไรคืนขนาด (เช่น Win+Down) จะดันกลับเต็มจอ
        titleBar.MinimizeRequested += (_, _) => Min();
        titleBar.CloseRequested += (_, _) => Close();
        Resize += (_, _) => StayMaximized();

        btnLang.Click += (_, _) => ToggleLanguage();
        ApplyLanguage();
    }

    /// <summary>
    /// สลับภาษาทั้งหน้าต่าง — หน้าที่สร้างทีหลัง (หน้าย่อยของ Setting, dialog)
    /// จะแปลตัวเองตอนถูกสร้าง จึงไม่ต้องตามไปไล่ที่นี่
    /// </summary>
    private void ToggleLanguage()
    {
        Services.LanguageService.Toggle();
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Services.LanguageService.Apply(this);

        // ปุ่มบอก "ภาษาที่ใช้อยู่ตอนนี้" กดเพื่อสลับไปอีกภาษา
        btnLang.Text = Services.LanguageService.IsThai ? "ไทย" : "EN";
    }

    /// <summary>
    /// หน้าต่างต้องเต็มจอเสมอตอนแสดงผล ถ้าถูกคืนขนาดก็ดันกลับไปเต็มจอทันที
    /// แต่ตอนพับลงแถบงานต้องปล่อยไว้ ไม่งั้นผู้ใช้พับหน้าจอไม่ได้
    /// ใช้ <c>MaxRestore</c> ของ AntdUI เพราะการเซ็ต <see cref="Form.WindowState"/>
    /// ตรง ๆ ไม่ได้อัปเดตธงภายในของไลบรารีและกรอบหน้าต่างจะเพี้ยน
    /// </summary>
    /// <summary>
    /// ขยายเต็มจอต้องได้ขนาดจอเต็ม ไม่ใช่แค่พื้นที่ทำงาน
    /// เหตุผลอยู่ที่ <see cref="FullScreenMaximize"/>
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        FullScreenMaximize.Handle(this, ref m);
        base.WndProc(ref m);
    }

    private void StayMaximized()
    {
        // เช็ค Normal อย่างเดียว — ถ้าเช็ค "ไม่ใช่ Maximized" ตอนผู้ใช้กดพับหน้าจอ
        // หน้าต่างจะเด้งกลับขึ้นมาทันทีจนพับไม่ได้
        if (WindowState == FormWindowState.Normal) MaxRestore();
    }

    private async void MainShellForm_Load(object? sender, EventArgs e)
    {
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);
        if (level <= 1 || level == 9)
            await settingPage.CheckAllStatusAsync();
    }

    /// <summary>Position of Edit Pattern in the tab / page / visibility arrays.</summary>
    private const int EditPatternTab = 2;

    private void ApplyMenuLevel()
    {
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnTransfer, btnSetting };
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, transferListPage, settingPage };

        bool[] visible = level switch
        {
            0 => [true, false, false, false, true],
            1 => [false, true, true, false, true],
            3 => [false, false, false, true, true],    // ST3 — Transfer + Setting
            9 => [false, false, false, false, true],   // โหมดทดสอบหน้างาน — เข้าได้เฉพาะ Setting
            _ => [true, true, true, true, true],
        };

        // Edit Pattern is hidden at every menu level. Kept as one override rather
        // than edited into each arm above, so the page comes back by deleting this
        // line - the tab, the page and the per-level table are all still intact.
        visible[EditPatternTab] = false;

        for (int i = 0; i < allTabs.Length; i++)
        {
            allTabs[i].Visible = visible[i];
            allPages[i].Visible = visible[i];
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i]
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute;
            // แท็บชิดกันเป็นแถบเดียว กว้างพอดีข้อความที่ 17pt ไม่เว้นร่อง
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

    private void btnTransfer_Click(object sender, EventArgs e)
    {
        transferListPage.BringToFront();
        SetActiveTab(btnTransfer);
    }
}
