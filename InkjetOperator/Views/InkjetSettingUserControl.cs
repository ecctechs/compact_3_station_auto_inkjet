using System.Net.Sockets;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class InkjetSettingUserControl : UserControl
{
    private const int PrinterPort = 9004;
    private string _savedMk058Ip = "";
    private string _savedMk059Ip = "";

    public InkjetSettingUserControl()
    {
        InitializeComponent();
        LoadSettings();

        txtMk058Ip.TextChanged += (_, _) => MarkDirty(txtMk058Ip);
        txtMk059Ip.TextChanged += (_, _) => MarkDirty(txtMk059Ip);
        btnMk058Name.Click += (_, _) => EditName("MK058_NAME", lblMk058Badge);
        btnMk059Name.Click += (_, _) => EditName("MK059_NAME", lblMk059Badge);
        btnCheckStatus.Click += async (_, _) => await CheckAllStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => { LoadSettings(); ResetColors(); };
    }

    private void LoadSettings()
    {
        _savedMk058Ip = CustomSettingsManager.Read("MK058_COM");
        _savedMk059Ip = CustomSettingsManager.Read("MK059_COM");
        txtMk058Ip.Text = _savedMk058Ip;
        txtMk059Ip.Text = _savedMk059Ip;

        var name058 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var name059 = CustomSettingsManager.Read("MK059_NAME", "MK-059");
        lblMk058Badge.Text = name058;
        lblMk059Badge.Text = name059;

        ResetColors();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var ip058 = txtMk058Ip.Text.Trim();
        var ip059 = txtMk059Ip.Text.Trim();

        if (!string.IsNullOrEmpty(ip058) && !string.IsNullOrEmpty(ip059) && ip058 == ip059)
        {
            MessageBox.Show("IP addresses must not be the same.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CustomSettingsManager.Write("MK058_COM", ip058);
        CustomSettingsManager.Write("MK059_COM", ip059);
        _savedMk058Ip = ip058;
        _savedMk059Ip = ip059;

        ResetColors();
        MessageBox.Show("Saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditName(string key, Label badge)
    {
        var current = CustomSettingsManager.Read(key, key.Replace("_NAME", ""));
        using var dlg = new InputDialog("Edit Name", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write(key, dlg.Value);
        badge.Text = dlg.Value;
    }

    private async Task CheckAllStatusAsync()
    {
        btnCheckStatus.Enabled = false;
        try
        {
            var t1 = CheckPrinterAsync(txtMk058Ip.Text.Trim(), lblMk058Status);
            var t2 = CheckPrinterAsync(txtMk059Ip.Text.Trim(), lblMk059Status);
            await Task.WhenAll(t1, t2);
        }
        finally { btnCheckStatus.Enabled = true; }
    }

    private static async Task CheckPrinterAsync(string ip, AntdUI.Label statusDot)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus(statusDot, Color.Gray);
            return;
        }

        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(ip, PrinterPort).WaitAsync(TimeSpan.FromSeconds(3));
            SetStatus(statusDot, Color.FromArgb(76, 175, 80));
        }
        catch
        {
            SetStatus(statusDot, Color.FromArgb(220, 38, 38));
        }
    }

    private static void SetStatus(AntdUI.Label dot, Color color)
    {
        if (dot.InvokeRequired)
            dot.Invoke(() => dot.ForeColor = color);
        else
            dot.ForeColor = color;
    }

    private void MarkDirty(AntdUI.Input input) =>
        input.BackColor = Color.LightYellow;

    private void ResetColors()
    {
        txtMk058Ip.BackColor = Color.White;
        txtMk059Ip.BackColor = Color.White;
    }
}
