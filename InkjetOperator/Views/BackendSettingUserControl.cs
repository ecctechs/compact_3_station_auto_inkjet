using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class BackendSettingUserControl : UserControl
{
    private string _savedPcIp = "";

    public BackendSettingUserControl()
    {
        InitializeComponent();
        LoadSettings();

        txtPcIp.TextChanged += (_, _) =>
        {
            MarkDirty(txtPcIp);
            lblApiUrlValue.Text = BuildApiUrl(txtPcIp.Text.Trim());
        };
        btnPcName.Click += (_, _) => EditName();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => { LoadSettings(); ResetColors(); };
    }

    private void LoadSettings()
    {
        _savedPcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        txtPcIp.Text = _savedPcIp;
        lblApiUrlValue.Text = BuildApiUrl(_savedPcIp);

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
        MessageBox.Show("Saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditName()
    {
        var current = CustomSettingsManager.Read("PC2IP_NAME", "PC");
        using var dlg = new InputDialog("Edit Name", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write("PC2IP_NAME", dlg.Value);
        lblPcBadge.Text = dlg.Value;
    }

    private async Task CheckStatusAsync()
    {
        var ip = txtPcIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus(Color.Gray);
            return;
        }

        btnCheckStatus.Enabled = false;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await http.GetAsync($"http://{ip}:3000/");
            SetStatus(Color.FromArgb(76, 175, 80));
        }
        catch
        {
            SetStatus(Color.FromArgb(220, 38, 38));
        }
        finally { btnCheckStatus.Enabled = true; }
    }

    private void SetStatus(Color color)
    {
        if (lblPcStatus.InvokeRequired)
            lblPcStatus.Invoke(() => lblPcStatus.ForeColor = color);
        else
            lblPcStatus.ForeColor = color;
    }

    private static string BuildApiUrl(string ip) =>
        string.IsNullOrWhiteSpace(ip) ? "" : $"http://{ip}:3000";

    private void MarkDirty(AntdUI.Input input) =>
        input.BackColor = Color.LightYellow;

    private void ResetColors() =>
        txtPcIp.BackColor = Color.White;
}
