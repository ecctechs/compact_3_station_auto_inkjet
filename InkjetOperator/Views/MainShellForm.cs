using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// Application main window (shell): top menu bar + content host. The pages are
/// placed in pnlContent (Dock=Fill) at design time; menu buttons switch between
/// them by showing one page and hiding the rest. The Input Order tab shows the
/// Scan Barcode page. Only navigation/tab-state code lives here — no other
/// business logic, no runtime control creation, no custom paint.
/// <para>
/// เดิมสลับหน้าด้วย <c>BringToFront()</c> อย่างเดียว ซึ่งเปลี่ยนแค่ลำดับการวางทับ
/// ทุกหน้าจึงยัง Visible อยู่พร้อมกันทั้งหมด pnlContent เปิด AutoScroll ไว้และคิด
/// พื้นที่เลื่อนจากลูก <b>ทุกตัว</b> ไม่ใช่แค่ตัวที่อยู่หน้าสุด พอหน้า Setting ขยาย
/// ความสูงตอนรัน (SettingUserControl.EnsureSubPage ตั้ง MinimumSize ของหน้าย่อย)
/// พื้นที่เลื่อนของแผงที่ใช้ร่วมกันก็โตตาม แถบเลื่อนโผล่ แล้วหน้าอื่นที่ Dock=Fill
/// อยู่ในแผงเดียวกันก็ถือขนาดที่ผิดไปด้วย — กลับมาหน้า Order List แล้วกรอบ Preview
/// กับ Processing หลุดออกนอกพื้นที่ที่ถูก clip เหลือแต่พื้นสีฟ้าของ pnlContent
/// </para>
/// <para>
/// โชว์ทีละหน้าแล้วซ่อนที่เหลือ AutoScroll จึงคิดจากหน้าที่เห็นอยู่หน้าเดียว และ
/// หน้าที่ถูกโชว์ได้ layout รอบใหม่ทุกครั้งที่สลับ
/// </para>
/// </summary>
public partial class MainShellForm : AntdUI.Window
{
    private static readonly System.Drawing.Color ActiveTab = DesignTokens.PrimaryBlue;
    private static readonly System.Drawing.Color InactiveTab = DesignTokens.Inactive;

    private AntdUI.Button[] _visibleTabs = [];

    /// <summary>หน้าที่สถานีนี้เข้าได้ เรียงคู่กับ <see cref="_visibleTabs"/> ทีละตัว</summary>
    private Control[] _visiblePages = [];

    public MainShellForm()
    {
        InitializeComponent();
        ApplyMenuLevel();
        ApplyProgramTitle();
        Load += MainShellForm_Load;

        // จอหน้างานเป็น Full HD และใช้เต็มจอตลอด
        // พับหน้าจอลงแถบงานได้ตามปกติ แต่ย่อให้เล็กกว่าเต็มจอไม่ได้ เพราะเลย์เอาต์
        // ออกแบบไว้ที่ 1920x1080 — ถ้ามีอะไรคืนขนาด (เช่น Win+Down) จะดันกลับเต็มจอ
        titleBar.MinimizeRequested += (_, _) => Min();
        titleBar.CloseRequested += (_, _) => Close();
        Resize += (_, _) => StayMaximized();

        // แถบหัวเป็นของเราเอง ไม่ได้ใช้กรอบของ Windows ไอคอนตรงนี้จึงไม่ได้โผล่
        // ที่มุมหน้าต่าง แต่ไปโผล่ที่แถบงานกับหน้า Alt-Tab
        // ดึงจากไอคอนของ .exe เองแทนการฝังไฟล์ซ้ำอีกชุด
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        btnLang.Click += (_, _) => ToggleLanguage();
        ApplyLanguage();
    }

    /// <summary>
    /// ตั้งชื่อโปรแกรมตามสถานีของเครื่อง ทั้งแถบหัวบนจอและชื่อที่แถบงานกับ Alt-Tab
    ///
    /// <para>
    /// ตั้งทั้งสองที่เพราะคนละหน้าที่ — แถบหัวบอกคนที่ยืนอยู่หน้าเครื่อง ส่วนชื่อที่
    /// แถบงานใช้ตอนเปิดหลายเครื่องหรือรีโมตเข้ามาดู จะได้รู้ว่าหน้าต่างไหนของสถานีไหน
    /// </para>
    /// </summary>
    private void ApplyProgramTitle()
    {
        var title = Services.StationService.ProgramTitle;

        titleBar.TitleText = title;
        Text = title;
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

    /// <summary>กำลังบังคับขนาดอยู่ — กันไม่ให้ Resize ที่เกิดจากการบังคับวนกลับมาซ้ำ</summary>
    private bool _forcingFullScreen;

    private void StayMaximized()
    {
        if (_forcingFullScreen) return;

        // พับลงแถบงานอยู่ ต้องปล่อยไว้เฉย ๆ ไม่งั้นหน้าต่างจะเด้งกลับขึ้นมาทันที
        // จนผู้ใช้พับหน้าจอไม่ได้เลย
        if (WindowState == FormWindowState.Minimized) return;

        // ขยายอยู่และไม่เล็กกว่าพื้นที่ทำงาน = ปกติดี ไม่ต้องแตะ
        //
        // เทียบกับพื้นที่ทำงาน ไม่ใช่ขนาดจอ เพราะกรอบหน้าต่างของ AntdUI ปรับ
        // non-client area เอง ขนาดจริงตอนขยายจึงไม่เท่าขนาดจอพอดี ถ้าไปเทียบกับ
        // ขนาดจอตรง ๆ เงื่อนไขจะไม่มีวันเป็นจริง แล้วสั่งขยายซ้ำทุกครั้งที่หน้าต่างขยับ
        var work = Screen.FromHandle(Handle).WorkingArea;
        if (WindowState == FormWindowState.Maximized
            && Bounds.Width >= work.Width && Bounds.Height >= work.Height) return;

        // ต้องแวะ Normal ก่อนเสมอ สั่ง Maximized ทับตอนที่สถานะเป็น Maximized อยู่แล้ว
        // Windows มองว่าไม่มีอะไรเปลี่ยน จึงไม่คำนวณขนาดใหม่ให้
        //
        // เจอจริงตอนคืนจากการพับ — สถานะกลับมาเป็น Maximized แต่ขนาดเป็นขนาดที่
        // จำไว้ก่อนขยาย (1920x1080) การตั้ง Bounds ตรง ๆ ก็ไม่ช่วย เพราะ WinForms
        // เก็บค่าไว้เป็นขนาดตอนคืนสถานะแทนที่จะเอาไปใช้จริง
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
        // หน้า Scan Barcode เป็นหน้าแรกของสถานีบาร์โค้ด วางเคอร์เซอร์ให้ตั้งแต่เปิด
        // โปรแกรม พนักงานจะได้ยิงสแกนเนอร์ได้เลยโดยไม่ต้องแตะจอก่อน
        if (scanBarcodePage.Visible) scanBarcodePage.FocusBarcode();

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

        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting };
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage };

        bool[] visible = level switch
        {
            0 => [true, false, false, true],
            1 => [false, true, true, true],
            // ST3 ใช้หน้า Order List หน้าเดียวกับ ST1 แต่กรองงานคนละชุด
            // (กฎอยู่ที่ MarkingMethodService — ST3 เห็นเฉพาะ marking 10 / 11 / 12
            //  รวมถึงรหัสที่ลงท้ายด้วย 3 ซึ่งเดินเส้นทางเดียวกับ 1 เช่น 32 เท่ากับ 12)
            3 => [false, true, false, true],    // ST3 — Order List + Setting
            9 => [false, false, false, true],   // โหมดทดสอบหน้างาน — เข้าได้เฉพาะ Setting
            _ => [true, true, true, true],
        };

        // Edit Pattern is hidden at every menu level. Kept as one override rather
        // than edited into each arm above, so the page comes back by deleting this
        // line - the tab, the page and the per-level table are all still intact.
        visible[EditPatternTab] = false;

        for (int i = 0; i < allTabs.Length; i++)
        {
            allTabs[i].Visible = visible[i];

            // ซ่อนทุกหน้าไว้ก่อน แล้วให้ ShowPage เปิดหน้าแรกให้ทีเดียว — หน้าที่
            // สถานีนี้เข้าไม่ได้จะไม่ถูกเปิดเลย เพราะ ShowPage มองแค่ _visiblePages
            allPages[i].Visible = false;

            tlpMenuBar.ColumnStyles[i].SizeType = visible[i]
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute;
            // แท็บชิดกันเป็นแถบเดียว กว้างพอดีข้อความที่ 17pt ไม่เว้นร่อง
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 250F : 0F;
        }

        _visibleTabs = allTabs.Where((_, i) => visible[i]).ToArray();
        _visiblePages = allPages.Where((_, i) => visible[i]).ToArray();

        if (_visibleTabs.Length > 0)
            ShowPage(_visiblePages[0], _visibleTabs[0]);
    }

    /// <summary>
    /// โชว์หน้าเดียว ซ่อนที่เหลือ แล้วค่อยตั้งสีแท็บ
    ///
    /// <para>
    /// ต้องซ่อนของจริง ไม่ใช่แค่ BringToFront เพราะหน้าทั้งหมดอยู่ใน pnlContent
    /// แผงเดียวกันและแผงนั้นเปิด AutoScroll ไว้ เหตุผลเต็ม ๆ อยู่ที่หัวคลาส
    /// </para>
    /// </summary>
    private void ShowPage(Control page, AntdUI.Button tab)
    {
        foreach (var p in _visiblePages)
        {
            if (!ReferenceEquals(p, page)) p.Visible = false;
        }

        page.Visible = true;
        page.BringToFront();

        // หน้าก่อนหน้าอาจเลื่อนค้างไว้ หน้าใหม่ต้องเริ่มที่หัวเสมอ ไม่งั้นเปิดมา
        // เจอกลางหน้าโดยไม่มีใครสั่งเลื่อน
        //
        // ข้ามตอนยังไม่มี handle — ApplyMenuLevel เรียกฟังก์ชันนี้ตั้งแต่ใน constructor
        // ซึ่งตอนนั้นยังไม่มีอะไรให้เลื่อนอยู่แล้ว
        if (pnlContent.IsHandleCreated)
        {
            pnlContent.AutoScrollPosition = new Point(0, 0);

            // สั่งวาดใหม่ทั้งแผงรวมลูกทุกชั้น
            //
            // ลูกหลายตัวในหน้า Order List ตั้งพื้นหลังเป็น Transparent ซึ่ง WinForms
            // วาดโดยไปขอให้พ่อแม่วาดพื้นให้ก่อน การโชว์หน้าเฉย ๆ ไม่ได้สั่งวาดพื้น
            // ของแผงใหม่ พื้นที่นั้นจึงค้างสีฟ้าที่ pnlContent ลบทิ้งไว้ แล้วตัวหนังสือ
            // กับรูปไม่ถูกวาดทับ กลายเป็นกรอบเปล่าสีฟ้า
            //
            // เจอจริงที่ ST3 — กด Setting แล้วกดกลับ Order List กรอบ Preview กับ
            // Processing หายทั้งพื้นและตัวหนังสือ เหลือแต่เส้นขอบ ย่อหน้าต่างแล้วคืน
            // ขึ้นมาก็หายเอง ซึ่งยืนยันว่าเป็นเรื่องการวาด ไม่ใช่ layout หรือข้อมูล
            //
            // Refresh() = Invalidate(true) + Update() คือวาดทันที ไม่รอรอบ idle
            pnlContent.Refresh();
        }

        SetActiveTab(tab);
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
        ShowPage(scanBarcodePage, btnInputOrder);
        scanBarcodePage.FocusBarcode();
    }

    private void btnOrderList_Click(object sender, EventArgs e) =>
        ShowPage(orderListPage, btnOrderList);

    private void btnEditPattern_Click(object sender, EventArgs e) =>
        ShowPage(editPatternPage, btnEditPattern);

    private void btnSetting_Click(object sender, EventArgs e) =>
        ShowPage(settingPage, btnSetting);
}
