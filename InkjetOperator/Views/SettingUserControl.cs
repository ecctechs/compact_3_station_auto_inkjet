using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class SettingUserControl : UserControl
{
    private AntdUI.Button[] _menuButtons = [];
    private AntdUI.Button? _activeButton;
    private readonly Dictionary<string, UserControl> _subPages = new();

    public SettingUserControl()
    {
        InitializeComponent();
        ApplyMenuLevel();
    }

    private void ApplyMenuLevel()
    {
        var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
        int.TryParse(raw, out var level);

        var allButtons = new[] { btnDatabaseSetting, btnDbPathSetting, btnDB3Setting, btnPLCSetting, btnClampSetting, btnUvTest };

        bool[] visible = level switch
        {
            0 => [false, true, true, false, false, false],
            1 => [true, false, false, true, true, false],
            3 => [false, false, true, false, false, false],  // ST3 — Backend DB only
            9 => [false, false, false, true, true, true],    // ทดสอบหน้างาน: PLC / Clamp / UV Test
            _ => [true, true, true, true, true, true],
        };

        int row = 0;
        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].Visible = visible[i];
            if (visible[i])
            {
                tlpSidebar.SetRow(allButtons[i], row);
                tlpSidebar.RowStyles[row].SizeType = SizeType.Absolute;
                tlpSidebar.RowStyles[row].Height = 96F;
                row++;
            }
        }

        for (int r = row; r < tlpSidebar.RowStyles.Count; r++)
        {
            tlpSidebar.RowStyles[r].SizeType = SizeType.Absolute;
            tlpSidebar.RowStyles[r].Height = 0F;
        }

        tlpSidebar.Height = row * 96;

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
            page.Dock = DockStyle.Fill;
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
        _ => null,
    };

    private void ShowSubPage(string buttonName)
    {
        pnlContentArea.Controls.Clear();

        EnsureSubPage(buttonName);
        _subPages.TryGetValue(buttonName, out var page);

        if (page != null)
            pnlContentArea.Controls.Add(page);
    }
}
