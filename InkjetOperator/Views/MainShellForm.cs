using InkjetOperator.Theme;

namespace InkjetOperator.Views;
// หน้าหลักรวมเมนูและหน้าต่าง ๆ ที่สร้างไว้ใน Designer
// กดเมนูเพื่อดึงหน้านั้นมาไว้ด้านหน้า ไม่ได้สร้างหน้าใหม่
public partial class MainShellForm : AntdUI.Window
{
    // สีเมนูที่เลือกและเมนูอื่น
    private static readonly System.Drawing.Color ActiveTab = DesignTokens.PrimaryBlue;
    private static readonly System.Drawing.Color InactiveTab = DesignTokens.Inactive;

    // เก็บเฉพาะปุ่มเมนูที่โหมดนี้แสดง
    private AntdUI.Button[] _visibleTabs = [];

    // เตรียมหน้าจอ เมนู ชื่อสถานี และผูกปุ่มกับงานที่ต้องทำ
    public MainShellForm()
    {
        // สร้างปุ่มและจัดหน้าจอตามไฟล์ Designer
        InitializeComponent();
         //เลือกเมนูตาม MENU_LEVEL 
        ApplyMenuLevel();
        ApplyProgramTitle();
        // เมื่อหน้าหลักโหลด ให้เตรียมช่องสแกนและตรวจการเชื่อมต่อ
        Load += MainShellForm_Load;
        // พับลงแถบงานหรือปิดโปรแกรมได้; เปลี่ยนขนาดแล้วให้กลับเต็มจอ
        titleBar.MinimizeRequested += (_, _) => Min();
        titleBar.CloseRequested += (_, _) => Close();
        Resize += (_, _) => StayMaximized();
        // ใช้ไอคอนจาก EXE บนแถบงาน; ถ้าอ่านไม่ได้ก็เปิดโปรแกรมต่อ
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // กดปุ่มภาษาเพื่อสลับไทย/อังกฤษ
        btnLang.Click += (_, _) => ToggleLanguage();
        ApplyLanguage();
    }
    // แสดงชื่อสถานีทั้งแถบหัวและชื่อหน้าต่างของ Windows
    private void ApplyProgramTitle()
    {
        var title = Services.StationService.ProgramTitle;

        titleBar.TitleText = title;
        Text = title;
    }
    // เปลี่ยนภาษาที่จำไว้ แล้วอัปเดตข้อความบนหน้าจอ
    private void ToggleLanguage()
    {
        Services.LanguageService.Toggle();
        ApplyLanguage();
    }

    // แปลข้อความในหน้าหลักและหน้าที่อยู่ข้างใน
    private void ApplyLanguage()
    {
        Services.LanguageService.Apply(this);
        // ปุ่มแสดงภาษาที่กำลังใช้อยู่
        btnLang.Text = Services.LanguageService.IsThai ? "ไทย" : "EN";
    }
    // ให้ตัวช่วยปรับขนาดเต็มจอ ก่อนส่งข้อความให้หน้าต่างจัดการต่อ
    protected override void WndProc(ref Message m)
    {
        FullScreenMaximize.Handle(this, ref m);
        base.WndProc(ref m);
    }
    // กัน Resize เรียกซ้ำระหว่างที่เรากำลังปรับขนาด
    private bool _forcingFullScreen;

    // รักษาหน้าต่างให้เต็มจอ แต่ยังพับลงแถบงานได้
    private void StayMaximized()
    {
        // ถ้ากำลังปรับขนาดอยู่ ไม่ต้องเริ่มซ้ำ
        if (_forcingFullScreen) return;
        // ตอนพับหน้าต่าง ห้ามสั่งให้เด้งกลับขึ้นมา
        if (WindowState == FormWindowState.Minimized) return;
        // เทียบพื้นที่ทำงาน เพราะกรอบ AntdUI อาจมีขนาดไม่เท่าจอพอดี
        var work = Screen.FromHandle(Handle).WorkingArea;
        // เต็มพื้นที่อยู่แล้วก็ไม่ต้องปรับใหม่
        if (WindowState == FormWindowState.Maximized
            && Bounds.Width >= work.Width && Bounds.Height >= work.Height) return;
        // สลับผ่าน Normal เพื่อให้ Windows คำนวณขนาด Maximized ใหม่
        _forcingFullScreen = true;
        try
        {
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
        }
        finally
        {
            // จบการปรับขนาดแล้ว เปิดให้รับ Resize รอบถัดไป
            _forcingFullScreen = false;
        }
    }

    // ทำตอนหน้าหลักโหลด: เตรียมสแกนและตรวจสถานะตามโหมด
    private async void MainShellForm_Load(object? sender, EventArgs e)
    {
        // ถ้าหน้าสแกนแสดงอยู่ ให้วางเคอร์เซอร์พร้อมรับ Barcode
        if (scanBarcodePage.Visible) scanBarcodePage.FocusBarcode();

        // อ่าน MENU_LEVEL; ถ้าไม่มีใช้ 1 แต่ถ้าแปลงเลขไม่ได้ level จะเป็น 0
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);
        // ตรวจสถานะตอนเปิดเฉพาะโหมดตามเงื่อนไขนี้ โดยรอแบบ async
        if (level <= 1 || level == 9)
            await settingPage.CheckAllStatusAsync();
    }
    // Edit Pattern อยู่ลำดับที่ 3 ในชุดเมนู (นับจาก 0 จึงเป็น 2)
    private const int EditPatternTab = 2;

    // อ่านโหมดเครื่อง แล้วกำหนดเมนูและหน้าเริ่มต้น
    private void ApplyMenuLevel()
    {
        // อ่าน MENU_LEVEL; ถ้าไม่มีใช้ 1 แต่ถ้าแปลงเลขไม่ได้ level จะเป็น 0
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        // ลำดับปุ่มต้องตรงกับลำดับหน้าและค่า visible ด้านล่าง
        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting };
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage };

        // true = แสดง, false = ซ่อน; เรียง Scan / Order List / Edit Pattern / Setting
        bool[] visible = level switch
        {
            // โหมด 0: รับ Barcode และตั้งค่า
            0 => [true, false, false, true],
            // โหมด 1: รายการงานและตั้งค่า (Edit Pattern ถูกซ่อนอีกทีด้านล่าง)
            1 => [false, true, true, true],
            // ST3 ใช้หน้า Order List ร่วมกับ ST1; กรองงานในหน้ารายการ
            3 => [false, true, false, true], // ST3: รายการงานและตั้งค่า
            9 => [false, false, false, true], // โหมดทดสอบ: ตั้งค่าเท่านั้น
            // โหมดอื่นเปิดทุกเมนูก่อน แล้วใช้กฎซ่อนด้านล่าง
            _ => [true, true, true, true],
        };
        // ซ่อน Edit Pattern ทุกโหมด โดยยังเก็บหน้าและโค้ดไว้
        visible[EditPatternTab] = false;

        // แสดงหรือซ่อนปุ่มพร้อมหน้าที่คู่กัน
        for (int i = 0; i < allTabs.Length; i++)
        {
            allTabs[i].Visible = visible[i];
            allPages[i].Visible = visible[i];
            // ใช้ความกว้างตายตัวทั้งเมนูที่แสดงและเมนูที่ซ่อน
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i]
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute;
            // เมนูที่แสดงกว้าง 250; เมนูที่ซ่อนยุบช่องเหลือ 0
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 250F : 0F;
        }

        // เก็บปุ่มที่แสดงไว้ใช้เปลี่ยนสีเวลาเลือกเมนู
        _visibleTabs = allTabs.Where((_, i) => visible[i]).ToArray();

        // เปิดหน้าแรกที่โหมดนี้อนุญาตให้เห็น
        if (_visibleTabs.Length > 0)
        {
            var firstTab = _visibleTabs[0];
            var firstPage = allPages[Array.IndexOf(allTabs, firstTab)];
            firstPage.BringToFront();
            SetActiveTab(firstTab);
        }
    }

    // เปลี่ยนสีให้เห็นว่าตอนนี้เลือกเมนูไหน
    private void SetActiveTab(AntdUI.Button active)
    {
        foreach (var b in _visibleTabs)
        {
            var color = b == active ? ActiveTab : InactiveTab;
            b.DefaultBack = color;
        }
    }

    // เปิดหน้ารับ Barcode แล้ววางเคอร์เซอร์พร้อมสแกน
    private void btnInputOrder_Click(object sender, EventArgs e)
    {
        scanBarcodePage.BringToFront();
        SetActiveTab(btnInputOrder);
        scanBarcodePage.FocusBarcode();
    }

    // เปิดหน้ารายการงานและเปลี่ยนสีเมนู
    private void btnOrderList_Click(object sender, EventArgs e)
    {
        orderListPage.BringToFront();
        SetActiveTab(btnOrderList);
    }

    // เปิดหน้าแก้ Pattern; ปัจจุบันปุ่มนี้ถูกซ่อนทุกโหมด
    private void btnEditPattern_Click(object sender, EventArgs e)
    {
        editPatternPage.BringToFront();
        SetActiveTab(btnEditPattern);
    }

    // เปิดหน้าตั้งค่าและเปลี่ยนสีเมนู
    private void btnSetting_Click(object sender, EventArgs e)
    {
        settingPage.BringToFront();
        SetActiveTab(btnSetting);
    }
}
