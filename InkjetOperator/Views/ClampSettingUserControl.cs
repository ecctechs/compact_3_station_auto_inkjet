using System.Net.Sockets;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// หน้าควบคุมแคลมป์ผ่าน PLC (MC Protocol) — แยกจากหน้า Order ไว้ก่อน
/// ใช้ตั้งค่า commissioning และแก้ปัญหาหน้างาน
/// เมื่อพร้อมรวมเข้า flow หลัก ให้เรียก <see cref="ClampService"/> จากหน้า Order ได้เลย
/// </summary>
public partial class ClampSettingUserControl : UserControl
{
    private static readonly Color StatusGray = Color.Gray;
    private static readonly Color StatusGreen = Color.FromArgb(76, 175, 80);
    private static readonly Color StatusRed = Color.FromArgb(220, 38, 38);

    public ClampSettingUserControl()
    {
        InitializeComponent();
        SetupEvents();
        LoadSettings();

        lblStatus.ForeColor = StatusGray;
        _ = CheckStatusAsync();
    }

    // ── Setup ──────────────────────────────────────────────

    private void SetupEvents()
    {
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnBrowse.Click += (_, _) => BrowseDatabase();
        btnLoad.Click += (_, _) => LoadFromDatabase();
        btnUpload.Click += (_, _) => UploadToDatabase();
        btnApply.Click += async (_, _) => await ApplyAsync();
        btnReset.Click += async (_, _) => await ResetAsync();
        btnReadStatus.Click += async (_, _) => await ReadStatusAsync();
        btnSave.Click += (_, _) => SaveSettings();
        btnCancel.Click += (_, _) => LoadSettings();

        txtValueMm.TextChanged += (_, _) => UpdateRawPreview();

        foreach (var input in new[] { txtIp, txtPort, txtAddrTarget, txtAddrRun, txtAddrReset, txtAddrStatus })
        {
            var captured = input;
            captured.TextChanged += (_, _) => captured.BackColor = Color.LightYellow;
        }
    }

    private void LoadSettings()
    {
        var s = ClampSettings.Load();

        txtIp.Text = s.Ip;
        txtPort.Text = s.Port.ToString();
        txtDbPath.Text = s.DbPath;
        txtAddrTarget.Text = s.AddrTarget;
        txtAddrRun.Text = s.AddrRun;
        txtAddrReset.Text = s.AddrReset;
        txtAddrStatus.Text = s.AddrStatus;

        ResetColors();
        UpdateRawPreview();
    }

    private void SaveSettings()
    {
        if (!TryBuildSettings(out var s, out string error))
        {
            Warn(error);
            return;
        }

        s.Save();
        ResetColors();
        _ = CheckStatusAsync();

        MessageBox.Show("บันทึกเรียบร้อย", "Clamp", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>อ่านค่าจากหน้าจอ พร้อมตรวจว่าที่อยู่ทุกตัวใช้ได้จริงก่อนเอาไปสั่ง PLC</summary>
    private bool TryBuildSettings(out ClampSettings settings, out string error)
    {
        settings = new ClampSettings
        {
            Ip = txtIp.Text.Trim(),
            DbPath = txtDbPath.Text.Trim(),
            AddrTarget = txtAddrTarget.Text.Trim(),
            AddrRun = txtAddrRun.Text.Trim(),
            AddrReset = txtAddrReset.Text.Trim(),
            AddrStatus = txtAddrStatus.Text.Trim(),
        };

        if (!int.TryParse(txtPort.Text.Trim(), out int port) || port <= 0 || port > 65535)
        {
            error = "Port ไม่ถูกต้อง";
            return false;
        }
        settings.Port = port;

        foreach (var (label, address) in new[]
                 {
                     ("ค่าเป้าหมาย", settings.AddrTarget),
                     ("พัลส์สั่งวิ่ง", settings.AddrRun),
                     ("พัลส์รีเซ็ต", settings.AddrReset),
                     ("อ่านสถานะ", settings.AddrStatus),
                 })
        {
            if (!McProtocolService.TryParseAddress(address, out _, out _, out string addrError))
            {
                error = $"{label}: {addrError}";
                return false;
            }
        }

        error = "";
        return true;
    }

    // ── Database ───────────────────────────────────────────

    private void BrowseDatabase()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "เลือกไฟล์ mydatabase.db3",
            Filter = "SQLite database (*.db3;*.db)|*.db3;*.db|All files (*.*)|*.*",
        };

        if (!string.IsNullOrWhiteSpace(txtDbPath.Text))
        {
            var dir = Path.GetDirectoryName(txtDbPath.Text.Trim());
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                dlg.InitialDirectory = dir;
        }

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtDbPath.Text = dlg.FileName;
        txtDbPath.BackColor = Color.LightYellow;
    }

    /// <summary>หาค่า IAI ของโปรแกรมแล้วเติมลงช่องระยะ</summary>
    private void LoadFromDatabase()
    {
        string program = txtProgram.Text.Trim();
        var result = ClampService.Lookup(txtDbPath.Text.Trim(), program);

        if (!result.Found)
        {
            Log($"Load \"{program}\" → ❌ {result.Error}");
            Warn(result.Error);
            return;
        }

        txtValueMm.Text = result.ValueMm.ToString();
        UpdateRawPreview();
        Log($"Load \"{program}\" → {result.Column} = {result.ValueMm} mm");
    }

    /// <summary>บันทึกระยะที่ปรับแล้วกลับลง MainTable</summary>
    private void UploadToDatabase()
    {
        string program = txtProgram.Text.Trim();
        if (!TryReadValueMm(out int mm)) return;

        var confirm = MessageBox.Show(
            $"บันทึก {mm} mm ทับค่าเดิมของ \"{program}\" ในฐานข้อมูล\n\nยืนยันหรือไม่?",
            "ยืนยัน Upload", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        var (ok, message) = ClampService.Upload(txtDbPath.Text.Trim(), program, mm);

        Log($"Upload → {(ok ? "" : "❌ ")}{message}");
        if (!ok) Warn(message);
    }

    // ── PLC ────────────────────────────────────────────────

    private async Task ApplyAsync()
    {
        if (!TryBuildSettings(out var s, out string error)) { Warn(error); return; }
        if (!TryReadValueMm(out int mm)) return;

        var confirm = MessageBox.Show(
            $"สั่งแคลมป์ไปที่ {mm} mm\n" +
            $"เขียน {s.AddrTarget} = {ClampService.ToRaw(mm)}\n" +
            $"แล้วพัลส์ {s.AddrRun}\n\nยืนยันหรือไม่?",
            "ยืนยันสั่งแคลมป์", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true);
        try
        {
            var result = await ClampService.ApplyAsync(s, mm);
            if (IsDisposed) return;

            Log(result.Log);
            Log(result.Ok
                ? $"✅ สั่งแคลมป์ {result.ValueMm} mm สำเร็จ" +
                  (result.Status.HasValue ? $" (สถานะ {s.AddrStatus} = {result.Status})" : "")
                : "❌ สั่งแคลมป์ไม่สำเร็จ");
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    private async Task ResetAsync()
    {
        if (!TryBuildSettings(out var s, out string error)) { Warn(error); return; }

        var confirm = MessageBox.Show(
            $"ส่งพัลส์รีเซ็ตที่ {s.AddrReset}\n\nยืนยันหรือไม่?",
            "ยืนยันรีเซ็ต", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true);
        try
        {
            var (ok, log) = await ClampService.ResetAsync(s);
            if (IsDisposed) return;

            Log(log);
            Log(ok ? "✅ รีเซ็ตสำเร็จ" : "❌ รีเซ็ตไม่สำเร็จ");
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    private async Task ReadStatusAsync()
    {
        if (!TryBuildSettings(out var s, out string error)) { Warn(error); return; }

        SetBusy(true);
        try
        {
            var (ok, value, readError) = await ClampService.ReadStatusAsync(s);
            if (IsDisposed) return;

            Log(ok
                ? $"อ่าน {s.AddrStatus} → {value}"
                : $"อ่าน {s.AddrStatus} → ❌ {readError}");
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    public async Task CheckStatusAsync()
    {
        string ip = txtIp.Text.Trim();

        if (string.IsNullOrEmpty(ip) || !int.TryParse(txtPort.Text.Trim(), out int port))
        {
            SetStatus(string.IsNullOrEmpty(ip) ? StatusGray : StatusRed,
                string.IsNullOrEmpty(ip) ? "ยังไม่ได้ตั้ง IP" : "Port ไม่ถูกต้อง");
            return;
        }

        try
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(ip, port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(3000));

            if (IsDisposed) return;
            bool ok = completed == connectTask && !connectTask.IsFaulted && tcp.Connected;
            SetStatus(ok ? StatusGreen : StatusRed,
                ok ? $"เชื่อมต่อ {ip}:{port} ได้" : $"เชื่อมต่อ {ip}:{port} ไม่ได้");
        }
        catch
        {
            if (IsDisposed) return;
            SetStatus(StatusRed, $"เชื่อมต่อ {ip}:{port} ไม่ได้");
        }
    }

    // ── Helpers ────────────────────────────────────────────

    private bool TryReadValueMm(out int mm)
    {
        mm = 0;
        string text = txtValueMm.Text.Trim();

        if (!double.TryParse(text, out double value))
        {
            Warn("กรุณากรอกระยะแคลมป์เป็นตัวเลข");
            return false;
        }

        int clamped = ClampService.ClampMm(value);
        if (Math.Abs(clamped - value) > 0.001)
        {
            Log($"ระยะ {value} อยู่นอกช่วง {ClampService.MinMm}-{ClampService.MaxMm} → ใช้ {clamped} mm");
            txtValueMm.Text = clamped.ToString();
        }

        mm = clamped;
        return true;
    }

    private void UpdateRawPreview()
    {
        if (double.TryParse(txtValueMm.Text.Trim(), out double value))
        {
            int mm = ClampService.ClampMm(value);
            lblRawValue.Text = $"{ClampService.ToRaw(mm)}   ({mm} mm)";
        }
        else
        {
            lblRawValue.Text = "-";
        }
    }

    private void SetBusy(bool busy)
    {
        btnApply.Enabled = !busy;
        btnReset.Enabled = !busy;
        btnReadStatus.Enabled = !busy;
        btnLoad.Enabled = !busy;
        btnUpload.Enabled = !busy;
    }

    private void SetStatus(Color color, string text)
    {
        if (IsDisposed) return;

        void Apply()
        {
            if (IsDisposed) return;
            lblStatus.ForeColor = color;
            lblStatus.Text = text;
        }

        if (lblStatus.InvokeRequired) lblStatus.Invoke(Apply);
        else Apply();
    }

    private void Log(string message)
    {
        if (IsDisposed || string.IsNullOrEmpty(message)) return;
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ResetColors()
    {
        foreach (var input in new[]
                 { txtIp, txtPort, txtDbPath, txtAddrTarget, txtAddrRun, txtAddrReset, txtAddrStatus })
            input.BackColor = Color.White;
    }

    private static void Warn(string message) =>
        MessageBox.Show(message, "Clamp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
