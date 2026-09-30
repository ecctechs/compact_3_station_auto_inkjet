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
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1"); // อ่าน Mode ที่ตั้งไว้ของ PC นี้
        int.TryParse(raw, out var level); // แปลง Mode จากข้อความเป็นตัวเลข

        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting }; // เรียงปุ่มเมนูให้ตรงกับหน้าที่เปิด
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage }; // รวมหน้าจอตามลำดับปุ่มเมนู

        bool[] visible = level switch // กำหนดเมนูที่แต่ละ Mode ใช้ได้
        {
            0 => [true, false, false, true], // Mode 0 แสดงหน้าสแกนกับ Setting
            1 => [false, true, true, true], // Mode 1 แสดงงาน Pattern และ Setting
            3 => [false, true, false, true],    // ST3 — Order List + Setting
            9 => [false, false, false, true],   // โหมดทดสอบหน้างาน — เข้าได้เฉพาะ Setting
            _ => [true, true, true, true], // Mode อื่นเริ่มจากเปิดทุกเมนู
        };

        visible[EditPatternTab] = false; // ซ่อนเมนู Edit Pattern เพิ่มตามเงื่อนไขด้านบน

        for (int i = 0; i < allTabs.Length; i++) // ปรับปุ่มและหน้าจอทีละเมนู
        {
            allTabs[i].Visible = visible[i]; // แสดงหรือซ่อนปุ่มเมนูนี้
            allPages[i].Visible = visible[i]; // แสดงหรือซ่อนหน้าที่คู่กับเมนู
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i] // กำหนดชนิดความกว้างของช่องเมนู
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute; // ใช้ความกว้างแบบระบุค่าคงที่
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 250F : 0F; // เมนูที่ซ่อนลดความกว้างเหลือศูนย์
        }

        _visibleTabs = allTabs.Where((_, i) => visible[i]).ToArray(); // เก็บเฉพาะปุ่มเมนูที่มองเห็น

        if (_visibleTabs.Length > 0) // มีเมนูให้เลือกอย่างน้อยหนึ่งหน้า
        {
            var firstTab = _visibleTabs[0]; // เลือกปุ่มแรกเป็นเมนูเริ่มต้น
            var firstPage = allPages[Array.IndexOf(allTabs, firstTab)]; // หาหน้าที่ตรงกับเมนูแรก
            firstPage.BringToFront(); // นำหน้าแรกขึ้นมาแสดง
            SetActiveTab(firstTab); // ทำเครื่องหมายเมนูที่เปิดอยู่
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
