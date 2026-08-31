using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class BackendSettingUserControl : UserControl
{
    private string _savedPcIp = "";

    public BackendSettingUserControl()
    {
        InitializeComponent();
        LoadSettings();

        txtPcIp.TextChanged += (_, _) => MarkDirty(txtPcIp);
        btnPcName.Click += (_, _) => EditName();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => { LoadSettings(); ResetColors(); };
    }

    private void LoadSettings()
    {
        _savedPcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        txtPcIp.Text = _savedPcIp;

        var name = CustomSettingsManager.Read("PC2IP_NAME", "PC");
        lblPcBadge.Text = name;

        ResetColors();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var ip = txtPcIp.Text.Trim();
        CustomSettingsManager.Write("PC_IP", ip);
        _savedPcIp = ip;

        ResetColors();
        Notify.Success(this, "Saved.");
    }

    private void EditName()
    {
        var current = CustomSettingsManager.Read("PC2IP_NAME", "PC");
        using var dlg = new InputDialog("Rename", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write("PC2IP_NAME", dlg.Value);
        lblPcBadge.Text = dlg.Value;
    }

    public async Task CheckStatusAsync()
    {
        var ip = txtPcIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus(Color.Gray);
            return;
        }

        btnCheckStatus.Loading = true;
        btnCheckStatus.Enabled = false;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await http.GetAsync($"http://{ip}:3000/");
            SetStatus(DesignTokens.Success);
        }
        catch
        {
            SetStatus(DesignTokens.Danger);
        }
        finally
        {
            btnCheckStatus.Loading = false;
            btnCheckStatus.Enabled = true;
        }
    }

    private void SetStatus(Color color)
    {
        if (lblPcStatus.InvokeRequired)
            lblPcStatus.Invoke(() => lblPcStatus.ForeColor = color);
        else
            lblPcStatus.ForeColor = color;
    }

    // รับเป็น Control เพราะช่อง IP เปลี่ยนไปใช้ IpAddressInput ที่ไม่ใช่ AntdUI.Input
    private void MarkDirty(Control input) =>
        input.BackColor = Color.LightYellow;

    private void ResetColors() =>
        txtPcIp.BackColor = Color.White;
}
