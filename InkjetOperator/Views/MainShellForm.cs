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
        ApplyMenuLevel(); // ไปเลือกเมนูและหน้าเริ่มต้นตามโหมด
        ApplyProgramTitle(); // แสดงชื่อสถานีบนหน้าหลัก
        // เมื่อหน้าหลักโหลด ให้เตรียมช่องสแกนและตรวจการเชื่อมต่อ
        Load += MainShellForm_Load; // หน้าโหลดแล้วเตรียมช่อง Barcode
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
    private async void MainShellForm_Load(object? sender, EventArgs e) // เตรียมหน้าจอหลังเปิดหน้าหลัก
    {
        // ถ้าหน้าสแกนแสดงอยู่ ให้วางเคอร์เซอร์พร้อมรับ Barcode
        if (scanBarcodePage.Visible) scanBarcodePage.FocusBarcode(); // หน้าสแกนแสดงอยู่ให้พร้อมรับ Barcode

        // อ่าน MENU_LEVEL; ถ้าไม่มีใช้ 1 แต่ถ้าแปลงเลขไม่ได้ level จะเป็น 0
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1"); // อ่านโหมดเครื่อง ค่าเริ่มต้นคือ 1
        int.TryParse(raw, out var level); // แปลงเป็นเลข ถ้าแปลงไม่ได้จะได้ 0
        // ตรวจสถานะตอนเปิดเฉพาะโหมดตามเงื่อนไขนี้ โดยรอแบบ async
        if (level <= 1 || level == 9) // โหมดกลุ่มนี้ตรวจการเชื่อมต่อตอนเปิด
            await settingPage.CheckAllStatusAsync(); // รอหน้า Setting ตรวจสถานะ
    }
    // Edit Pattern อยู่ลำดับที่ 3 ในชุดเมนู (นับจาก 0 จึงเป็น 2)
    private const int EditPatternTab = 2;

    // อ่านโหมดเครื่อง แล้วกำหนดเมนูและหน้าเริ่มต้น
    private void ApplyMenuLevel() // Flow 1: เลือกหน้าแรกของเครื่องนี้
    {
        // อ่าน MENU_LEVEL; ถ้าไม่มีใช้ 1 แต่ถ้าแปลงเลขไม่ได้ level จะเป็น 0
        var raw = Services.CustomSettingsManager.Read("MENU_LEVEL", "1"); // อ่านโหมดเครื่อง ค่าเริ่มต้นคือ 1
        int.TryParse(raw, out var level); // แปลงเป็นเลข ถ้าแปลงไม่ได้จะได้ 0

        // ลำดับปุ่มต้องตรงกับลำดับหน้าและค่า visible ด้านล่าง
        var allTabs = new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting }; // เรียงปุ่มเมนูให้ตรงกับหน้า
        var allPages = new Control[] { scanBarcodePage, orderListPage, editPatternPage, settingPage }; // จับคู่หน้าจอกับปุ่มตามลำดับ

        // true = แสดง, false = ซ่อน; เรียง Scan / Order List / Edit Pattern / Setting
        bool[] visible = level switch // เลือกเมนูที่แสดงตามโหมด
        {
            // โหมด 0: รับ Barcode และตั้งค่า
            0 => [true, false, false, true], // Mode 0 แสดง Scan Barcode และ Setting
            // โหมด 1: รายการงานและตั้งค่า (Edit Pattern ถูกซ่อนอีกทีด้านล่าง)
            1 => [false, true, true, true], // Mode 1 เปิดชุดเมนูของ Station 1
            // ST3 ใช้หน้า Order List ร่วมกับ ST1; กรองงานในหน้ารายการ
            3 => [false, true, false, true], // ST3: รายการงานและตั้งค่า
            9 => [false, false, false, true], // โหมดทดสอบ: ตั้งค่าเท่านั้น
            // โหมดอื่นเปิดทุกเมนูก่อน แล้วใช้กฎซ่อนด้านล่าง
            _ => [true, true, true, true], // โหมดอื่นให้แสดงทุกเมนูก่อน
        };
        // ซ่อน Edit Pattern ทุกโหมด โดยยังเก็บหน้าและโค้ดไว้
        visible[EditPatternTab] = false; // ซ่อน Edit Pattern ทุกโหมด

        // แสดงหรือซ่อนปุ่มพร้อมหน้าที่คู่กัน
        for (int i = 0; i < allTabs.Length; i++) // กำหนดการแสดงปุ่มและหน้าทีละคู่
        {
            allTabs[i].Visible = visible[i]; // แสดงหรือซ่อนปุ่มเมนู
            allPages[i].Visible = visible[i]; // แสดงหรือซ่อนหน้าของปุ่มนั้น
            // ใช้ความกว้างตายตัวทั้งเมนูที่แสดงและเมนูที่ซ่อน
            tlpMenuBar.ColumnStyles[i].SizeType = visible[i] // ตั้งรูปแบบความกว้างช่องเมนู
                ? System.Windows.Forms.SizeType.Absolute : System.Windows.Forms.SizeType.Absolute; // ทั้งสองกรณีใช้ความกว้างตายตัว
            // เมนูที่แสดงกว้าง 250; เมนูที่ซ่อนยุบช่องเหลือ 0
            tlpMenuBar.ColumnStyles[i].Width = visible[i] ? 250F : 0F; // ปุ่มที่ซ่อนให้ยุบช่องเหลือ 0
        }

        // เก็บปุ่มที่แสดงไว้ใช้เปลี่ยนสีเวลาเลือกเมนู
        _visibleTabs = allTabs.Where((_, i) => visible[i]).ToArray(); // เก็บเฉพาะเมนูที่แสดงจริง

        // เปิดหน้าแรกที่โหมดนี้อนุญาตให้เห็น
        if (_visibleTabs.Length > 0) // มีเมนูให้เปิดอย่างน้อยหนึ่งหน้า
        {
            var firstTab = _visibleTabs[0]; // เลือกปุ่มแรกที่แสดง
            var firstPage = allPages[Array.IndexOf(allTabs, firstTab)]; // หาหน้าของปุ่มแรก
            firstPage.BringToFront(); // Mode 0 จะดึงหน้าสแกนมาไว้ด้านหน้า
            SetActiveTab(firstTab); // ทำสีปุ่มให้ตรงกับหน้าที่เปิด
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
