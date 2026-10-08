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

        // ช่องโฟลเดอร์ backend เห็นเฉพาะโหมดทดสอบ
        //
        // เป็นที่อยู่ของโค้ดที่โปรแกรมสั่งรันเอง (ดู BackendLauncher) ไม่ใช่ค่าที่
        // พนักงานหน้างานต้องแตะ ตั้งครั้งเดียวตอนติดตั้งแล้วไม่ต้องยุ่งอีก
        grpDev.Visible = StationService.IsDevMode;

        txtPcIp.TextChanged += (_, _) => MarkDirty(txtPcIp);
        btnPcName.Click += (_, _) => EditName();
        btnBrowseBackend.Click += (_, _) => BrowseBackendFolder();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => { LoadSettings(); ResetColors(); };

        // ไฟสถานะต้องตรงกับของจริง ไม่ใช่ภาพนิ่งตั้งแต่ตอนเปิดโปรแกรม
        // กติกาทั้งหมดอยู่ที่ StatusRecheck
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

        // โฟลเดอร์ที่ชี้ผิดแย่กว่าไม่ได้ตั้งไว้เลย — ตอนเปิดโปรแกรมจะพยายามรัน
        // แล้วค้างรอจนหมดเวลาโดยไม่มีอะไรบอกว่าผิดตรงไหน
        //
        // ว่างไว้ได้ แปลว่าไม่ให้โปรแกรมเปิด backend ให้
        var backend = txtBackendPath.Text.Trim();
        if (grpDev.Visible && backend.Length > 0 && !File.Exists(Path.Combine(backend, BackendEntryFile)))
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"ไม่พบไฟล์ {BackendEntryFile} ในโฟลเดอร์นี้\n\n{backend}");
            return;
        }

        CustomSettingsManager.Write("PC_IP", ip);
        _savedPcIp = ip;

        // เขียนเฉพาะตอนที่ช่องนี้โผล่ให้เห็น ไม่งั้นการกด Save ที่เครื่องหน้างาน
        // (ซึ่งไม่เห็นช่องนี้) จะล้างค่าที่ตั้งไว้ทิ้งโดยไม่มีใครตั้งใจ
        if (grpDev.Visible)
        {
            CustomSettingsManager.Write("BACKEND_PATH", backend);
            UpdateBackendPathStatus(backend);
        }

        ResetColors();
        Notify.Success(this, "Saved.");
    }

    /// <summary>ไฟล์ที่ <see cref="BackendLauncher"/> สั่งรัน — ใช้ตรวจว่าเลือกโฟลเดอร์ถูกไหม</summary>
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

    /// <summary>
    /// บอกว่าโฟลเดอร์ที่เลือกใช้ได้จริงไหม — ตรวจจากไฟล์ที่ต้องมี ไม่ใช่แค่ชื่อโฟลเดอร์
    /// ไม่ได้ตั้งไว้ก็ไม่ถือว่าผิด แค่แปลว่าต้องเปิด backend เอง
    /// </summary>
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

    /// <param name="quiet">true = รอบตรวจซ้ำอัตโนมัติ ไม่ต้องหมุนปุ่ม</param>
    public async Task CheckStatusAsync(bool quiet = false)
    {
        var ip = txtPcIp.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus(Color.Gray);
            return;
        }

        // โหมด Mockup สถานะ (ตัวเลือกหน้างาน) — ขึ้นว่าต่อได้โดยไม่ต่อจริง ใช้ถ่ายรูปคู่มือ
        if (StatusMockup.Enabled)
        {
            SetStatus(DesignTokens.Success);
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

    // รับเป็น Control เพราะช่อง IP เปลี่ยนไปใช้ IpAddressInput ที่ไม่ใช่ AntdUI.Input
    private void MarkDirty(Control input) =>
        input.BackColor = Color.LightYellow;

    private void ResetColors()
    {
        txtPcIp.BackColor = Color.White;
        txtBackendPath.BackColor = Color.White;
    }
}
