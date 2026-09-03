using System.Net.Sockets;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// หน้าควบคุมแคลมป์ผ่าน PLC (MC Protocol)
///
/// ตามแผนภาพหน้างาน: PLC ตัวเดียวคุม 6 แกน — Plate/Shim × X/Z1/Z2
/// ใช้ตารางแทนช่องกรอกแยก เพราะแต่ละแกนมี 4 address (target/run/reset/status)
/// รวม 24 ช่อง ซึ่งวางเป็นฟอร์มแล้วอ่านยากกว่ามาก
///
/// แกนที่ยังไม่ได้กำหนด address (แผนภาพเขียน DXXX/MXXX) จะถูกกันไม่ให้สั่ง
/// </summary>
public partial class ClampSettingUserControl : UserControl
{
    private static readonly Color Green = Color.FromArgb(21, 128, 61);
    private static readonly Color Red = Color.FromArgb(220, 38, 38);

    /// <summary>ช่องในตารางที่แก้แล้วมีผลกับเครื่อง — ล็อกไว้จนกว่าจะปลดล็อก</summary>
    private static readonly HashSet<string> LockedColumns =
        new(StringComparer.Ordinal) { "ValueMm", "AddrTarget", "AddrRun", "AddrReset", "AddrStatus" };

    private ClampSettings _settings = new();
    private List<AxisRow> _rows = [];
    private bool _unlocked;

    public ClampSettingUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        SetupEvents();
        LoadSettings();
        ApplyLockState();

        lblStatus.ForeColor = Color.Gray;
        _ = CheckStatusAsync();
    }

    // ── Setup ──────────────────────────────────────────────

    private void ConfigureColumns()
    {
        tblAxes.Columns =
        [
            new AntdUI.Column("Axis", "Axis", AntdUI.ColumnAlign.Center) { Width = "11%" },
            new AntdUI.Column("Column", "DB Column", AntdUI.ColumnAlign.Center) { Width = "10%" },
            new AntdUI.Column("ValueMm", "Value (mm)", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("AddrTarget", "Target (D)", AntdUI.ColumnAlign.Center) { Editable = true, Width = "10%" },
            new AntdUI.Column("AddrRun", "Run (M)", AntdUI.ColumnAlign.Center) { Editable = true, Width = "10%" },
            new AntdUI.Column("AddrReset", "Reset (M)", AntdUI.ColumnAlign.Center) { Editable = true, Width = "10%" },
            new AntdUI.Column("AddrStatus", "Status", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("Raw", "Raw", AntdUI.ColumnAlign.Center) { Width = "9%" },
            new AntdUI.Column("Op", "Action", AntdUI.ColumnAlign.Center) { Width = "22%" },
        ];
    }

    private void SetupEvents()
    {
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnBrowse.Click += (_, _) => BrowseDatabase();

        btnLoadAll.Click += (_, _) => LoadAllFromDatabase();
        btnApplyAll.Click += async (_, _) => await ApplyAllAsync();
        btnUploadAll.Click += (_, _) => UploadAll();

        btnSave.Click += (_, _) => SaveSettings();
        btnCancel.Click += (_, _) => LoadSettings();
        btnUnlock.Click += (_, _) => ToggleLock();

        tblAxes.CellButtonClick += TblAxes_CellButtonClick;
        tblAxes.CellBeginEdit += TblAxes_CellBeginEdit;
        tblAxes.CellEndEdit += TblAxes_CellEndEdit;

        txtIp.TextChanged += (_, _) => txtIp.BackColor = Color.LightYellow;
        txtPort.TextChanged += (_, _) => txtPort.BackColor = Color.LightYellow;
    }

    private void LoadSettings()
    {
        _settings = ClampSettings.Load();

        txtIp.Text = _settings.Ip;
        txtPort.Text = _settings.Port.ToString();
        txtDbPath.Text = _settings.DbPath;

        _rows = _settings.Axes.Select(a => new AxisRow
        {
            Key = a.Key,
            Axis = a.Display,
            Column = a.Column,
            ValueMm = "",
            AddrTarget = a.AddrTarget,
            AddrRun = a.AddrRun,
            AddrReset = a.AddrReset,
            AddrStatus = a.AddrStatus,
            Raw = "-",
            Op = NewButtons(),
        }).ToList();

        RebindTable();
        ResetColors();
        UpdateDbStatus();
    }

    /// <summary>
    /// ชุดสีเดียวกับตาราง Register Map ในหน้า PLC Setting —
    /// อ่านอย่างเดียว = Default (ขาว) · สั่งงานเครื่อง = Primary (น้ำเงิน)
    /// เรียงให้ปุ่มที่ปลอดภัยอยู่ซ้าย เหมือน [Read] [Write] ของหน้านั้น
    /// </summary>
    private static AntdUI.CellButton[] NewButtons() =>
    [
        new AntdUI.CellButton("read", "Read", AntdUI.TTypeMini.Default) { Radius = 6 },
        new AntdUI.CellButton("apply", "Write", AntdUI.TTypeMini.Primary) { Radius = 6 },
        new AntdUI.CellButton("reset", "Reset", AntdUI.TTypeMini.Warn) { Radius = 6 },
    ];

    private void RebindTable()
    {
        tblAxes.DataSource = null;
        tblAxes.DataSource = _rows;
    }

    // ── Lock / Unlock ──────────────────────────────────────

    /// <summary>
    /// หน้านี้เป็นหน้าตั้งค่า/ทดสอบ ไม่ใช่หน้าใช้งานประจำวัน
    /// การปรับแคลมป์ตอนผลิตจริงทำที่หน้า Order Detail (ส่วน IAI)
    /// จึงล็อกทุกอย่างที่เปลี่ยนค่าได้ เหลือไว้เฉพาะคำสั่งที่อ่านอย่างเดียว
    /// (เช็คการเชื่อมต่อ, ปุ่ม "อ่าน" รายแถว) ซึ่งไม่ทำให้เครื่องขยับ
    ///
    /// ใช้รหัสเดียวกับหน้า PLC Setting (คีย์ PLC_PASSWORD) จะได้ไม่ต้องจำสองชุด
    /// </summary>
    private void ToggleLock()
    {
        if (_unlocked)
        {
            _unlocked = false;
            ApplyLockState();
            Log("ล็อกแล้ว — ดูได้อย่างเดียว");
            return;
        }

        var password = CustomSettingsManager.Read("PLC_PASSWORD", "1234");
        using var dlg = new InputDialog("Unlock", "กรุณาใส่รหัสผ่าน:", "");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (dlg.Value != password)
        {
            Warn("รหัสผ่านไม่ถูกต้อง");
            return;
        }

        _unlocked = true;
        ApplyLockState();
        Log("ปลดล็อกแล้ว — แก้ค่าและสั่งงานได้");
    }

    private void ApplyLockState()
    {
        btnUnlock.Text = _unlocked ? "🔓 Lock" : "🔒 Unlock";

        // 1. การเชื่อมต่อ — btnCheckStatus ไม่ล็อก เพราะแค่ ping ไม่เปลี่ยนอะไร
        txtIp.Enabled = _unlocked;
        txtPort.Enabled = _unlocked;
        txtDbPath.Enabled = _unlocked;
        btnBrowse.Enabled = _unlocked;

        // 2. ชื่อโปรแกรม
        txtPlateProgram.Enabled = _unlocked;
        txtShimProgram.Enabled = _unlocked;
        btnLoadAll.Enabled = _unlocked;
        btnApplyAll.Enabled = _unlocked;
        btnUploadAll.Enabled = _unlocked;

        btnSave.Enabled = _unlocked;
        btnCancel.Enabled = _unlocked;

        lblAxesHint.Text = _unlocked
            ? "ปลดล็อกอยู่ — แก้ค่าและสั่งงานได้ อย่าลืมกด Save"
            : "ล็อกอยู่ — ดูได้อย่างเดียว กด Unlock เพื่อแก้ค่าหรือสั่งงาน";
    }

    /// <summary>ดึงค่าจากตารางกลับเข้า settings — ตารางคือแหล่งความจริงระหว่างที่หน้าเปิดอยู่</summary>
    private void SyncRowsToSettings()
    {
        _settings.Ip = txtIp.Text.Trim();
        _settings.Port = int.TryParse(txtPort.Text.Trim(), out int p) ? p : 5012;
        _settings.DbPath = txtDbPath.Text.Trim();

        foreach (var row in _rows)
        {
            var axis = _settings.Find(row.Key);
            if (axis == null) continue;

            axis.AddrTarget = (row.AddrTarget ?? "").Trim();
            axis.AddrRun = (row.AddrRun ?? "").Trim();
            axis.AddrReset = (row.AddrReset ?? "").Trim();
            axis.AddrStatus = (row.AddrStatus ?? "").Trim();
        }
    }

    private void SaveSettings()
    {
        SyncRowsToSettings();

        if (!ValidateAddresses(out string error))
        {
            Warn(error);
            return;
        }

        _settings.Save();
        ResetColors();
        _ = CheckStatusAsync();

        MessageBox.Show("บันทึกเรียบร้อย", "Clamp", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>ตรวจเฉพาะช่องที่กรอกมา — ปล่อยว่างได้ แปลว่าแกนนั้นยังไม่พร้อมใช้</summary>
    private bool ValidateAddresses(out string error)
    {
        foreach (var axis in _settings.Axes)
        {
            foreach (var (label, address) in new[]
                     {
                         ("Target", axis.AddrTarget),
                         ("Run", axis.AddrRun),
                         ("Reset", axis.AddrReset),
                         ("Status", axis.AddrStatus),
                     })
            {
                if (address.Trim().Length == 0) continue;
                if (!McProtocolService.TryParseAddress(address, out _, out _, out string e))
                {
                    error = $"{axis.Display} — {label}: {e}";
                    return false;
                }
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

        var current = txtDbPath.Text.Trim();
        if (current.Length > 0)
        {
            var dir = Path.GetDirectoryName(current);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) dlg.InitialDirectory = dir;
        }

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtDbPath.Text = dlg.FileName;
        txtDbPath.BackColor = Color.LightYellow;
        UpdateDbStatus();
    }

    /// <summary>บอกว่าไฟล์นี้มีคอลัมน์ครบ 6 แกนไหม — ฐานข้อมูลรุ่นเก่าไม่มีคอลัมน์ Z</summary>
    private void UpdateDbStatus()
    {
        var path = txtDbPath.Text.Trim();
        if (path.Length == 0)
        {
            lblStatus.Text = "ยังไม่ได้เลือก mydatabase.db3";
            lblStatus.ForeColor = Color.Gray;
            return;
        }

        var columns = ClampService.ReadColumns(path);
        if (columns.Count == 0)
        {
            lblStatus.Text = "✗ เปิดไฟล์ไม่ได้ หรือไม่มีตาราง MainTable";
            lblStatus.ForeColor = Red;
            return;
        }

        var missing = _settings.Axes
            .Where(a => !columns.Contains(a.Column))
            .Select(a => a.Column)
            .ToList();

        if (missing.Count == 0)
        {
            lblStatus.Text = "✓ MainTable มีคอลัมน์ครบทั้ง 6 แกน";
            lblStatus.ForeColor = Green;
        }
        else
        {
            lblStatus.Text = $"⚠ ไม่มีคอลัมน์: {string.Join(", ", missing)} — แกนนั้นจะโหลด/บันทึกไม่ได้";
            lblStatus.ForeColor = Red;
        }
    }

    /// <summary>โหลดค่าทั้ง 6 แกนจาก MainTable ตามชื่อโปรแกรมของแต่ละฝั่ง</summary>
    private void LoadAllFromDatabase()
    {
        SyncRowsToSettings();

        string plate = txtPlateProgram.Text.Trim();
        string shim = txtShimProgram.Text.Trim();

        if (plate.Length == 0 && shim.Length == 0)
        {
            Warn("กรุณากรอกชื่อโปรแกรมอย่างน้อยหนึ่งฝั่ง");
            return;
        }

        int loaded = 0;

        foreach (var row in _rows)
        {
            var axis = _settings.Find(row.Key);
            if (axis == null) continue;

            string program = axis.Side == ClampSide.Plate ? plate : shim;
            if (program.Length == 0) continue;

            var result = ClampService.Lookup(_settings.DbPath, program, axis);
            if (result.Found)
            {
                row.ValueMm = result.ValueMm.ToString();
                row.Raw = ClampService.ToRaw(result.ValueMm).ToString();
                loaded++;
                Log($"{axis.Display} ({axis.Column}) = {result.ValueMm} mm");
            }
            else
            {
                row.ValueMm = "";
                row.Raw = "-";
                Log($"{axis.Display} ({axis.Column}) → {result.Error}");
            }
        }

        RebindTable();
        Log($"โหลดสำเร็จ {loaded} จาก {_rows.Count} แกน");
    }

    private void UploadAll()
    {
        SyncRowsToSettings();

        var targets = _rows.Where(r => ParseMm(r.ValueMm).HasValue).ToList();
        if (targets.Count == 0)
        {
            Warn("ยังไม่มีแกนไหนที่กรอกค่าไว้");
            return;
        }

        var confirm = MessageBox.Show(
            $"บันทึกค่า {targets.Count} แกนทับของเดิมในฐานข้อมูล\n\nยืนยันหรือไม่?",
            "ยืนยัน Upload", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        foreach (var row in targets) UploadRow(row);
    }

    private void UploadRow(AxisRow row)
    {
        var axis = _settings.Find(row.Key);
        if (axis == null) return;

        int? mm = ParseMm(row.ValueMm);
        if (mm == null)
        {
            Log($"{axis.Display} → ค่ายังว่าง ข้ามไป");
            return;
        }

        string program = axis.Side == ClampSide.Plate
            ? txtPlateProgram.Text.Trim()
            : txtShimProgram.Text.Trim();

        var (ok, message) = ClampService.Upload(_settings.DbPath, program, axis, mm.Value);
        Log($"{axis.Display} → {(ok ? "" : "❌ ")}{message}");
    }

    // ── PLC ────────────────────────────────────────────────

    private async Task ApplyAllAsync()
    {
        SyncRowsToSettings();

        var ready = _rows
            .Where(r => ParseMm(r.ValueMm).HasValue)
            .Where(r => _settings.Find(r.Key)?.IsConfigured == true)
            .ToList();

        if (ready.Count == 0)
        {
            Warn("ยังไม่มีแกนไหนพร้อมสั่ง\n(ต้องมีทั้งค่า mm และ address Target/Run)");
            return;
        }

        var names = string.Join("\n", ready.Select(r => $"  • {r.Axis} = {r.ValueMm} mm"));
        var confirm = MessageBox.Show(
            $"สั่งแคลมป์ {ready.Count} แกน\n\n{names}\n\nยืนยันหรือไม่?",
            "ยืนยันสั่งแคลมป์", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        SetBusy(true);
        try
        {
            foreach (var row in ready)
            {
                await ApplyRowAsync(row, confirmFirst: false);
                if (IsDisposed) return;
            }
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    private async Task ApplyRowAsync(AxisRow row, bool confirmFirst)
    {
        var axis = _settings.Find(row.Key);
        if (axis == null) return;

        int? mm = ParseMm(row.ValueMm);
        if (mm == null)
        {
            Warn($"{axis.Display}: กรุณากรอกค่า (mm) ก่อน");
            return;
        }

        if (!axis.IsConfigured)
        {
            Warn($"{axis.Display}: ยังไม่ได้กำหนด address Target/Run");
            return;
        }

        if (confirmFirst)
        {
            var confirm = MessageBox.Show(
                $"สั่ง {axis.Display} ไปที่ {mm} mm\n" +
                $"เขียน {axis.AddrTarget} = {ClampService.ToRaw(mm.Value)}\n" +
                $"แล้วพัลส์ {axis.AddrRun}\n\nยืนยันหรือไม่?",
                "ยืนยันสั่งแคลมป์", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
        }

        var result = await ClampService.ApplyAsync(_settings, axis, mm.Value);
        if (IsDisposed) return;

        Log(result.Log);
        row.Raw = result.RawWritten.ToString();
        RebindTable();
    }

    private async Task ResetRowAsync(AxisRow row)
    {
        var axis = _settings.Find(row.Key);
        if (axis == null) return;

        var confirm = MessageBox.Show(
            $"ส่งพัลส์รีเซ็ต {axis.Display} ที่ {axis.AddrReset}\n\nยืนยันหรือไม่?",
            "ยืนยันรีเซ็ต", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        var (_, log) = await ClampService.ResetAsync(_settings, axis);
        if (!IsDisposed) Log(log);
    }

    private async Task ReadRowAsync(AxisRow row)
    {
        var axis = _settings.Find(row.Key);
        if (axis == null) return;

        var (ok, value, error) = await ClampService.ReadStatusAsync(_settings, axis);
        if (IsDisposed) return;

        Log(ok
            ? $"{axis.Display} — อ่าน {axis.AddrStatus} → {value}"
            : $"{axis.Display} — ❌ {error}");
    }

    public async Task CheckStatusAsync()
    {
        string ip = txtIp.Text.Trim();

        if (ip.Length == 0 || !int.TryParse(txtPort.Text.Trim(), out int port))
        {
            Log(ip.Length == 0 ? "ยังไม่ได้ตั้ง IP ของ PLC แคลมป์" : "Port ไม่ถูกต้อง");
            return;
        }

        try
        {
            using var tcp = new TcpClient();
            var connect = tcp.ConnectAsync(ip, port);
            var completed = await Task.WhenAny(connect, Task.Delay(3000));
            if (IsDisposed) return;

            bool ok = completed == connect && !connect.IsFaulted && tcp.Connected;
            Log(ok ? $"เชื่อมต่อ {ip}:{port} ได้" : $"เชื่อมต่อ {ip}:{port} ไม่ได้");
        }
        catch (Exception ex)
        {
            if (!IsDisposed) Log($"เชื่อมต่อ {ip}:{port} ไม่ได้ — {ex.Message}");
        }
    }

    // ── Table events ───────────────────────────────────────

    private async void TblAxes_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        // ล็อกอยู่ให้อ่านได้อย่างเดียว สั่งแกนวิ่งหรือรีเซ็ตต้องปลดล็อกก่อน
        if (!_unlocked && e.Btn?.Id != "read")
        {
            Notify.WarnModal(this, "ล็อกอยู่",
                "กด Unlock ก่อนจึงจะสั่งงานแคลมป์ได้");
            return;
        }

        if (e.Record is not AxisRow row) return;

        // "อ่าน" ไม่ทำให้เครื่องขยับ จึงใช้ได้ตอนล็อก ส่วน "สั่ง"/"Reset" ต้องปลดล็อกก่อน
        if (!_unlocked && e.Btn?.Id != "read")
        {
            Warn("ล็อกอยู่ — กด Unlock ก่อนสั่งงาน");
            return;
        }

        SyncRowsToSettings();
        SetBusy(true);
        try
        {
            switch (e.Btn?.Id)
            {
                case "apply": await ApplyRowAsync(row, confirmFirst: true); break;
                case "reset": await ResetRowAsync(row); break;
                case "read": await ReadRowAsync(row); break;
            }
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    /// <summary>คืน false = ไม่ให้เข้าโหมดแก้ไขช่องนั้น</summary>
    private bool TblAxes_CellBeginEdit(object? sender, AntdUI.TableEventArgs e)
    {
        if (_unlocked || !LockedColumns.Contains(e.Column.Key)) return true;

        Log("ตารางถูกล็อกอยู่ — กด Unlock ก่อน");
        return false;
    }

    private bool TblAxes_CellEndEdit(object? sender, AntdUI.TableEndEditEventArgs e)
    {
        if (e.Record is not AxisRow row) return true;

        var value = e.Value ?? "";

        // กันไว้อีกชั้น เผื่อมีทางเข้าโหมดแก้ไขที่ไม่ผ่าน CellBeginEdit
        if (!_unlocked && e.Column != null && LockedColumns.Contains(e.Column.Key)) return false;

        switch (e.Column?.Key)
        {
            case "ValueMm":
                row.ValueMm = value;
                // อัปเดตค่าที่จะเขียนให้เห็นทันที จะได้ตรวจก่อนกดสั่ง
                var mm = ParseMm(value);
                row.Raw = mm.HasValue ? ClampService.ToRaw(mm.Value).ToString() : "-";
                BeginInvoke(RebindTable);
                break;

            case "AddrTarget": row.AddrTarget = value; break;
            case "AddrRun": row.AddrRun = value; break;
            case "AddrReset": row.AddrReset = value; break;
            case "AddrStatus": row.AddrStatus = value; break;
        }

        return true;
    }

    // ── Helpers ────────────────────────────────────────────

    /// <summary>คืน null เมื่อช่องว่าง = แกนนี้ไม่ถูกใช้ในรอบนี้ (ต่างจาก 0 ที่เป็นระยะจริง)</summary>
    private static int? ParseMm(string? text)
    {
        var s = (text ?? "").Trim();
        if (s.Length == 0) return null;
        return double.TryParse(s, out double v) ? ClampService.ClampMm(v) : null;
    }

    private void SetBusy(bool busy)
    {
        btnApplyAll.Enabled = !busy;
        btnLoadAll.Enabled = !busy;
        btnUploadAll.Enabled = !busy;
        btnCheckStatus.Enabled = !busy;
    }

    private void Log(string message)
    {
        if (IsDisposed || string.IsNullOrWhiteSpace(message)) return;

        foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line.TrimEnd()}{Environment.NewLine}");
    }

    private void ResetColors()
    {
        txtIp.BackColor = Color.White;
        txtPort.BackColor = Color.White;
        txtDbPath.BackColor = Color.White;
    }

    private static void Warn(string message) =>
        MessageBox.Show(message, "Clamp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

/// <summary>1 แถวในตารางแกน — ผูกกับ ClampAxis ผ่าน Key</summary>
internal class AxisRow
{
    public string Key { get; set; } = "";
    public string Axis { get; set; } = "";
    public string Column { get; set; } = "";
    public string ValueMm { get; set; } = "";
    public string AddrTarget { get; set; } = "";
    public string AddrRun { get; set; } = "";
    public string AddrReset { get; set; } = "";
    public string AddrStatus { get; set; } = "";
    public string Raw { get; set; } = "-";
    public AntdUI.CellButton[] Op { get; set; } = [];
}
