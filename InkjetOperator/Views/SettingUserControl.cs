namespace InkjetOperator.Views;

public partial class SettingUserControl : UserControl
{
    private readonly AntdUI.Button[] _menuButtons;
    private AntdUI.Button? _activeButton;

    public SettingUserControl()
    {
        InitializeComponent();
        _menuButtons = new[] { btnDatabaseSetting, btnDB3Setting, btnPLCSetting, btnUVPrinterSetting };
        foreach (var btn in _menuButtons)
            btn.Click += MenuButton_Click;
        SelectMenu(btnDatabaseSetting);
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

    private void ShowSubPage(string buttonName)
    {
        pnlContentArea.Controls.Clear();
        // Sub-page UserControls will be added here when implemented.
    }
}
