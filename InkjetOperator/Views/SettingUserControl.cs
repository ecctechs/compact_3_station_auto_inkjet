using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class SettingUserControl : UserControl
{
    /// <summary>ระดับเมนูของโหมดทดสอบ — ตัวเดียวกับที่หน้า Order Detail ใช้</summary>
    private const int DevMenuLevel = 99;

    /// <summary>ตำแหน่งของปุ่มสถานะระบบในตารางระดับเมนู</summary>
    private const int SystemHealthTab = 6;

    /// <summary>ตำแหน่งของปุ่มตัวเลือกหน้างานในตารางระดับเมนู</summary>
    private const int StationOptionsTab = 7;

    private AntdUI.Button[] _menuButtons = [];
    private AntdUI.Button? _activeButton;
    private readonly Dictionary<string, UserControl> _subPages = new();

    public SettingUserControl()
    {
        InitializeComponent();
        ApplyMenuLevel();

        // ทุกระดับเมนูเห็นปุ่มนี้ — ตอนติดตั้งเครื่องอยู่ระดับสถานีแล้ว (เลือกตอนเปิดครั้งแรก)
        // ไม่มีใครอยู่โหมด Dev และ IP กระจายอยู่หลายหน้าที่แต่ละสถานีเห็นไม่ครบ
        btnSiteNetwork.Click += (_, _) => ApplySiteNetwork();
    }

    /// <summary>
    /// ใส่ IP หน้างานทุกช่องทีเดียว — ถามยืนยันพร้อมบอกว่าช่องไหนเปลี่ยนจากอะไรเป็นอะไร
    /// ค่าทั้งหมดอยู่ที่ <see cref="SiteNetwork"/>
    /// </summary>
    private void ApplySiteNetwork()
    {
        var changes = SiteNetwork.Pending();
        if (changes.Count == 0)
        {
            Notify.Success(this, "IP ตรงกับหน้างานครบทุกช่องอยู่แล้ว");
            return;
        }

        var lines = changes.Select(c =>
            $"{c.Entry.Label}:  {(c.Old.Length == 0 ? "(ว่าง)" : c.Old)}  →  {c.Entry.Value}");
        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "ใช้ IP หน้างาน",
                Headline = $"เปลี่ยน {changes.Count} ช่องเป็น IP หน้างาน?",
                Detail = string.Join(Environment.NewLine, lines),
                Message = "ค่าอื่น (path, สถานี, ชื่อเครื่อง) ไม่แตะ · เก็บสำเนาไฟล์ Setting เดิมไว้ให้",
                OkText = "ใช้ IP นี้",
                Icon = "GlobalOutlined",
            }))
            return;

        var (error, backup) = SiteNetwork.Apply(changes);
        if (error != null)
        {
            Notify.ErrorModal(this, "ใช้ IP หน้างานไม่สำเร็จ", error
                + (backup == null ? "" : Environment.NewLine + Environment.NewLine + "สำเนาไฟล์เดิม: " + backup));
            return;
        }

        // หน้าที่เปิดค้างไว้ยังแสดงค่าเก่าในช่อง กด Save ทีหลังจะเขียนค่าเก่าทับคืน — ทิ้งแล้วสร้างใหม่
        pnlContentArea.Controls.Clear();
        foreach (var page in _subPages.Values) page.Dispose();
        _subPages.Clear();
        if (_activeButton != null) ShowSubPage(_activeButton.Name);
        HealthMonitor.CheckNow();

        Notify.SuccessDetail(this, "ใช้ IP หน้างานแล้ว",
            "ปิดโปรแกรมแล้วเปิดใหม่ 1 ครั้ง" + Environment.NewLine + Environment.NewLine
            + $"แก้ {changes.Count} ช่อง · ทุกหน้าจะต่อ IP ใหม่หลังเปิดใหม่" + Environment.NewLine
            + "สำเนาไฟล์เดิม: " + backup);
    }

    private void ApplyMenuLevel()
    {
        var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var allButtons = new[] { btnDatabaseSetting, btnDbPathSetting, btnDB3Setting, btnPLCSetting, btnClampSetting, btnUvTest, btnSystemHealth, btnStationOptions };

        bool[] visible = level switch
        {
            0 => [false, true, true, false, false, false, false, false],
            1 => [true, false, false, true, true, false, false, false],
            // ST3 — Backend DB อย่างเดียว
            //
            // โฟลเดอร์ UV2 ที่ ST3 ต้องใช้ไล่ดูรุ่นย่อยของ .uvdx ตั้งที่ UV2_FOLDER
            // ใน uv.config ของเครื่องนั้น ตั้งครั้งเดียวตอนติดตั้ง ไม่มีหน้าจอให้แก้
            // เพราะหน้า Printer Setting ที่มีช่องนี้อยู่แล้วมี COM port ของ MK กับ
            // IP ของ UV ปนอยู่ด้วย ซึ่งไม่ควรเปิดให้ ST3 แตะ
            3 => [false, false, true, false, false, false, false, false],
            9 => [false, false, false, true, true, true, false, false],  // ทดสอบหน้างาน: PLC / Clamp / UV Test
            _ => [true, true, true, true, true, true, false, false],
        };

        // หน้าสถานะระบบเปิดเฉพาะโหมดทดสอบ
        //
        // เป็นหน้าไว้ไล่หาสาเหตุตอนมีอะไรใช้ไม่ได้ ไม่ใช่หน้าที่พนักงานต้องดูระหว่าง
        // ทำงาน และรายการที่ขึ้นแดงส่วนใหญ่เป็นของที่สถานีนั้นไม่ได้ใช้อยู่แล้ว
        // เปิดให้ทุกคนเห็นมีแต่จะสร้างคำถามโดยไม่จำเป็น
        visible[SystemHealthTab] = level == DevMenuLevel;

        // ตัวเลือกหน้างานก็เปิดเฉพาะโหมดทดสอบด้วยเหตุผลเดียวกัน — ในนั้นเป็นสวิตช์
        // ที่เปลี่ยนกฎการทำงานหน้างาน ไม่ใช่ค่าที่พนักงานควรพลิกเองระหว่างผลิต
        visible[StationOptionsTab] = level == DevMenuLevel;

        // ปุ่ม "ใช้ IP หน้างาน" อยู่ล่างสุดของแถบ — โหมด Dev มีเมนูครบแปดอันซึ่งสูงเกินที่ว่าง
        // เมนูอันล่างสุดจะโดนปุ่มทับ จึงลดความสูงแต่ละแถวลงให้พอดี (ไม่เกิน 96 เท่าเดิม)
        int count = visible.Count(v => v);
        int space = pnlSidebar.ClientSize.Height - tlpSiteNetwork.Height - pnlSidebar.Padding.Vertical - 4;
        float rowHeight = count == 0 ? 96F : Math.Min(96F, space / count);

        int row = 0;
        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].Visible = visible[i];
            if (visible[i])
            {
                tlpSidebar.SetRow(allButtons[i], row);
                tlpSidebar.RowStyles[row].SizeType = SizeType.Absolute;
                tlpSidebar.RowStyles[row].Height = rowHeight;
                row++;
            }
        }

        for (int r = row; r < tlpSidebar.RowStyles.Count; r++)
        {
            tlpSidebar.RowStyles[r].SizeType = SizeType.Absolute;
            tlpSidebar.RowStyles[r].Height = 0F;
        }

        tlpSidebar.Height = (int)(row * rowHeight);

        _menuButtons = allButtons.Where((_, i) => visible[i]).ToArray();
        foreach (var btn in _menuButtons)
            btn.Click += MenuButton_Click;

        if (_menuButtons.Length > 0)
            SelectMenu(_menuButtons[0]);
    }

    private void MenuButton_Click(object? sender, EventArgs e)
    {
        if (sender is AntdUI.Button btn)
            SelectMenu(btn);
    }

    private void SelectMenu(AntdUI.Button btn)
    {
        if (_activeButton == btn) return;

        foreach (var b in _menuButtons)
            ButtonStyles.SetSelected(b, false);

        ButtonStyles.SetSelected(btn, true);
        _activeButton = btn;

        ShowSubPage(btn.Name);
    }

    public async Task CheckAllStatusAsync()
    {
        var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var tasks = new List<Task>();

        if (level == 0)
        {
            EnsureSubPage(nameof(btnDbPathSetting));
            EnsureSubPage(nameof(btnDB3Setting));

            if (_subPages.TryGetValue(nameof(btnDbPathSetting), out var dbPage) && dbPage is DatabaseSettingUserControl db)
                tasks.Add(db.CheckStatusAsync());
            if (_subPages.TryGetValue(nameof(btnDB3Setting), out var bePage) && bePage is BackendSettingUserControl be)
                tasks.Add(be.CheckStatusAsync());
        }
        else if (level == 1)
        {
            EnsureSubPage(nameof(btnDatabaseSetting));
            EnsureSubPage(nameof(btnPLCSetting));

            if (_subPages.TryGetValue(nameof(btnDatabaseSetting), out var inkPage) && inkPage is InkjetSettingUserControl ink)
                tasks.Add(ink.CheckAllStatusAsync());
            if (_subPages.TryGetValue(nameof(btnPLCSetting), out var plcPage) && plcPage is PlcSettingUserControl plc)
                tasks.Add(plc.CheckStatusAsync());

            EnsureSubPage(nameof(btnClampSetting));
            if (_subPages.TryGetValue(nameof(btnClampSetting), out var clampPage) && clampPage is ClampSettingUserControl clamp)
                tasks.Add(clamp.CheckStatusAsync());
        }
        else if (level == 9)
        {
            // โหมดทดสอบหน้างาน — เช็คเฉพาะสามหน้าที่เปิดให้ใช้
            EnsureSubPage(nameof(btnPLCSetting));
            EnsureSubPage(nameof(btnClampSetting));

            if (_subPages.TryGetValue(nameof(btnPLCSetting), out var plcTest) && plcTest is PlcSettingUserControl plc9)
                tasks.Add(plc9.CheckStatusAsync());
            if (_subPages.TryGetValue(nameof(btnClampSetting), out var clampTest) && clampTest is ClampSettingUserControl clamp9)
                tasks.Add(clamp9.CheckStatusAsync());
        }

        await Task.WhenAll(tasks);
    }

    private void EnsureSubPage(string buttonName)
    {
        if (_subPages.ContainsKey(buttonName)) return;
        var page = CreateSubPage(buttonName);
        if (page != null)
        {
            // จอที่เตี้ยกว่าที่ออกแบบไว้ Dock.Fill จะบีบหน้าลงมา แถวล่างสุด
            // (ตาราง / ปุ่ม Save) หายไปเลยเพราะไม่มีที่ให้วาด
            // ล็อกความสูงขั้นต่ำเท่าที่ Designer ออกแบบ แล้วให้ pnlContentArea เลื่อนแทน
            // ล็อกเฉพาะความสูง — ความกว้างยังยืดหดตามจอได้เหมือนเดิม
            page.MinimumSize = new Size(0, page.Height);
            page.Dock = DockStyle.Fill;
            LanguageService.Apply(page);
            _subPages[buttonName] = page;
        }
    }

    private static UserControl? CreateSubPage(string buttonName) => buttonName switch
    {
        nameof(btnDatabaseSetting) => new InkjetSettingUserControl(),
        nameof(btnDbPathSetting) => new DatabaseSettingUserControl(),
        nameof(btnDB3Setting) => new BackendSettingUserControl(),
        nameof(btnPLCSetting) => new PlcSettingUserControl(),
        nameof(btnClampSetting) => new ClampSettingUserControl(),
        nameof(btnUvTest) => new UvTestUserControl(),
        nameof(btnSystemHealth) => new SystemHealthUserControl(),
        nameof(btnStationOptions) => new StationOptionsUserControl(),
        _ => null,
    };

    private void ShowSubPage(string buttonName)
    {
        pnlContentArea.Controls.Clear();

        EnsureSubPage(buttonName);
        _subPages.TryGetValue(buttonName, out var page);

        if (page == null)
        {
            pnlContentArea.AutoScrollMinSize = Size.Empty;
            return;
        }

        // แต่ละหน้าสูงไม่เท่ากัน ต้องบอกช่วงเลื่อนใหม่ทุกครั้งที่สลับหน้า
        // ไม่งั้นหน้าเตี้ยจะยังมี scrollbar ค้างจากหน้าก่อนหน้า
        pnlContentArea.AutoScrollPosition = Point.Empty;
        pnlContentArea.AutoScrollMinSize = new Size(0, page.MinimumSize.Height);
        // แปลทุกครั้งที่แสดง ไม่ใช่แค่ตอนสร้าง — หน้าที่ถูกสร้างไว้ก่อนสลับภาษา
        // จะถูกถอดออกจาก control tree ตอนสลับหน้า ทำให้ตอนสลับภาษาแปลไม่ถึง
        LanguageService.Apply(page);
        pnlContentArea.Controls.Add(page);
    }
}
