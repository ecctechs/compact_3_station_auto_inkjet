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

        grpDev.Visible = StationService.IsDevMode;

        txtPcIp.TextChanged += (_, _) => MarkDirty(txtPcIp);
        btnPcName.Click += (_, _) => EditName();
        btnBrowseBackend.Click += (_, _) => BrowseBackendFolder();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => { LoadSettings(); ResetColors(); };

        Services.StatusRecheck.Wire(this, tmrAutoCheck, () => CheckStatusAsync(quiet: true));
    }

    private void LoadSettings()
    {
        _savedPcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        txtPcIp.Text = _savedPcIp;

        var name = CustomSettingsManager.Read("PC2IP_NAME", "PC");
        lblPcBadge.Text = name;

        txtBackendPath.Text = CustomSettingsManager.Read("BACKEND_PATH");
        UpdateBackendPathStatus(txtBackendPath.Text);

        ResetColors();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var ip = txtPcIp.Text.Trim();

        var backend = txtBackendPath.Text.Trim();
        if (grpDev.Visible && backend.Length > 0 && !File.Exists(Path.Combine(backend, BackendEntryFile)))
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"ไม่พบไฟล์ {BackendEntryFile} ในโฟลเดอร์นี้\n\n{backend}");
            return;
        }

        CustomSettingsManager.Write("PC_IP", ip);
        _savedPcIp = ip;

        if (grpDev.Visible)
        {
            CustomSettingsManager.Write("BACKEND_PATH", backend);
            UpdateBackendPathStatus(backend);
        }

        ResetColors();
        Notify.Success(this, "Saved.");
    }

    private const string BackendEntryFile = "index.js";

    private void BrowseBackendFolder()
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = $"เลือกโฟลเดอร์ backend (ต้องมีไฟล์ {BackendEntryFile})",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
        };

        var current = txtBackendPath.Text.Trim();
        if (current.Length > 0 && Directory.Exists(current)) dlg.SelectedPath = current;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtBackendPath.Text = dlg.SelectedPath;
        MarkDirty(txtBackendPath);
        UpdateBackendPathStatus(dlg.SelectedPath);
    }

    private void UpdateBackendPathStatus(string path)
    {
        path = path.Trim();

        if (path.Length == 0)
        {
            lblBackendPathStatus.Text = "ยังไม่ได้ตั้งค่า — โปรแกรมจะไม่เปิด backend ให้เอง";
            lblBackendPathStatus.ForeColor = Color.Gray;
        }
        else if (!Directory.Exists(path))
        {
            lblBackendPathStatus.Text = "✗  ไม่พบโฟลเดอร์";
            lblBackendPathStatus.ForeColor = DesignTokens.Danger;
        }
        else if (!File.Exists(Path.Combine(path, BackendEntryFile)))
        {
            lblBackendPathStatus.Text = $"✗  ไม่มีไฟล์ {BackendEntryFile} ในโฟลเดอร์นี้";
            lblBackendPathStatus.ForeColor = DesignTokens.Danger;
        }
        else
        {
            lblBackendPathStatus.Text = "✓  พร้อมใช้งาน";
            lblBackendPathStatus.ForeColor = DesignTokens.SuccessText;
        }
    }

    private void EditName()
    {
        var current = CustomSettingsManager.Read("PC2IP_NAME", "PC");
        using var dlg = new InputDialog("Rename", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write("PC2IP_NAME", dlg.Value);
        lblPcBadge.Text = dlg.Value;
    }

    public async Task CheckStatusAsync(bool quiet = false)
    {
        var ip = txtPcIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus(Color.Gray);
            return;
        }

        if (!quiet)
        {
            btnCheckStatus.Loading = true;
            btnCheckStatus.Enabled = false;
        }
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
            if (!quiet)
            {
                btnCheckStatus.Loading = false;
                btnCheckStatus.Enabled = true;
            }
        }
    }

    private void SetStatus(Color color)
    {
        if (lblPcStatus.InvokeRequired)
            lblPcStatus.Invoke(() => lblPcStatus.ForeColor = color);
        else
            lblPcStatus.ForeColor = color;
    }

    private void MarkDirty(Control input) =>
        input.BackColor = Color.LightYellow;

    private void ResetColors()
    {
        txtPcIp.BackColor = Color.White;
        txtBackendPath.BackColor = Color.White;
    }
}
