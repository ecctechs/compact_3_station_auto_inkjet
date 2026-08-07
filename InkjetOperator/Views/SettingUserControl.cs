using InkjetOperator.Services;

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

        var allButtons = new[] { btnDatabaseSetting, btnDbPathSetting, btnDB3Setting, btnPLCSetting };

        bool[] visible = level switch
        {
            0 => [false, true, true, false],
            _ => [true, true, true, true],
        };

        int row = 0;
        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].Visible = visible[i];
            if (visible[i])
            {
                tlpSidebar.SetRow(allButtons[i], row);
                tlpSidebar.RowStyles[row].SizeType = SizeType.Absolute;
                tlpSidebar.RowStyles[row].Height = 84F;
                row++;
            }
        }

        for (int r = row; r < tlpSidebar.RowStyles.Count; r++)
        {
            tlpSidebar.RowStyles[r].SizeType = SizeType.Absolute;
            tlpSidebar.RowStyles[r].Height = 0F;
        }

        tlpSidebar.Height = row * 84;

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
        {
            b.Type = AntdUI.TTypeMini.Default;
            b.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        }

        btn.Type = AntdUI.TTypeMini.Primary;
        btn.ForeColor = System.Drawing.Color.White;
        _activeButton = btn;

        ShowSubPage(btn.Name);
    }

    public async Task CheckAllStatusAsync()
    {
        EnsureSubPage(nameof(btnDbPathSetting));
        EnsureSubPage(nameof(btnDB3Setting));

        var tasks = new List<Task>();
        if (_subPages.TryGetValue(nameof(btnDbPathSetting), out var dbPage) && dbPage is DatabaseSettingUserControl db)
            tasks.Add(db.CheckStatusAsync());
        if (_subPages.TryGetValue(nameof(btnDB3Setting), out var bePage) && bePage is BackendSettingUserControl be)
            tasks.Add(be.CheckStatusAsync());

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
