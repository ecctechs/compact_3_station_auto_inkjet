using System.Net.Sockets;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// หน้าทดสอบเครื่อง UV แบบเดี่ยว — ไม่ต้องมี job ไม่ต้องสแกนบาร์โค้ด
/// กรอกชื่อโปรแกรมเองแล้วสั่ง Load / Start / Stop และเขียนข้อความลง CPI.db3 ได้จากหน้าเดียว
/// ใช้ตอนติดตั้งหน้างานและตอนหาสาเหตุเมื่อเครื่องไม่พิมพ์
/// </summary>
public partial class UvTestUserControl : UserControl
{
    private static readonly Color Green = DesignTokens.SuccessText;
    private static readonly Color Red = DesignTokens.Danger;

    private readonly UvTcpService _uvTcp = new();
    private int _uvNumber = 1;

    public UvTestUserControl()
    {
        InitializeComponent();
        SetupEvents();
        SelectUv(1);
    }

    // ── Setup ──────────────────────────────────────────────

    private void SetupEvents()
    {
        btnUv1.Click += (_, _) => SelectUv(1);
        btnUv2.Click += (_, _) => SelectUv(2);

        btnCheck.Click += async (_, _) => await CheckConnectionAsync();
        btnBrowseFolder.Click += (_, _) => BrowseFolder();
        btnFindProgram.Click += (_, _) => FindProgram();

        btnLoad.Click += async (_, _) => await LoadAsync();
        btnStart.Click += async (_, _) => await StartAsync();
        btnStop.Click += async (_, _) => await StopAsync();

        btnCpiWrite.Click += async (_, _) => await CpiWriteAsync();
        btnCpiRead.Click += async (_, _) => await CpiReadAsync();

        txtFolder.TextChanged += (_, _) => UpdatePathStatus();
    }

    /// <summary>สลับเครื่อง แล้วดึงค่าที่ตั้งไว้ของเครื่องนั้นมาเติมให้อัตโนมัติ</summary>
    private void SelectUv(int uvNumber)
    {
        _uvNumber = uvNumber;

        btnUv1.Type = uvNumber == 1 ? AntdUI.TTypeMini.Primary : AntdUI.TTypeMini.Default;
        btnUv2.Type = uvNumber == 2 ? AntdUI.TTypeMini.Primary : AntdUI.TTypeMini.Default;
        btnUv1.ForeColor = uvNumber == 1 ? DesignTokens.TextOnPrimary : DesignTokens.DarkNavy;
        btnUv2.ForeColor = uvNumber == 2 ? DesignTokens.TextOnPrimary : DesignTokens.DarkNavy;

        btnUv1.Text = UvSettingsManager.Read("UV1_NAME", "UV-001");
        btnUv2.Text = UvSettingsManager.Read("UV2_NAME", "UV-002");

        txtIp.Text = CustomSettingsManager.Read($"UV00{uvNumber}_IP");
        txtPort.Text = CustomSettingsManager.Read($"UV00{uvNumber}_PORT", "10086");
        txtFolder.Text = UvSettingsManager.Read(uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER");

        // ตารางใน CPI.db3 ผูกกับเครื่อง — UV1 = Plate, UV2 = Shim
        txtTable.Text = uvNumber == 1 ? "MK063" : "MK067";

        UpdatePathStatus();
        Log($"เลือกเครื่อง {CurrentName()}");
    }

    private string CurrentName() =>
        UvSettingsManager.Read(_uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{_uvNumber}");

    // ── โฟลเดอร์ ──────────────────────────────────────────

    /// <summary>
    /// เลือกโฟลเดอร์รากของซอฟต์แวร์ UV แล้วบันทึกทันที
    /// CPI.db3 กับ document เป็นโฟลเดอร์ย่อยข้างใน ไม่ต้องเลือกแยก
    /// </summary>
    private void BrowseFolder()
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "เลือกโฟลเดอร์ที่ติดตั้งซอฟต์แวร์ UV (ต้องมี database\\sys และ document)",
            UseDescriptionForTitle = true,
        };

        var current = txtFolder.Text.Trim();
        if (current.Length > 0 && Directory.Exists(current))
            dlg.SelectedPath = current;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtFolder.Text = dlg.SelectedPath;
        UvSettingsManager.Write(_uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER", dlg.SelectedPath);

        UpdatePathStatus();
        Log($"ตั้งโฟลเดอร์ {CurrentName()} = {dlg.SelectedPath}");
    }

    private void UpdatePathStatus()
    {
        var cpi = UvSettingsManager.GetCpiPath(_uvNumber);
        var doc = UvSettingsManager.GetDocumentFolder(_uvNumber);

        int fileCount = 0;
        if (doc != null)
        {
            try { fileCount = Directory.GetFiles(doc, "*.uvdx").Length; } catch { }
        }

        var parts = new List<string>
        {
            cpi != null ? "✓ CPI.db3" : "✗ ไม่พบ database\\sys\\CPI.db3",
            doc != null ? $"✓ document ({fileCount} ไฟล์)" : "✗ ไม่พบโฟลเดอร์ document",
        };

        lblPathStatus.Text = string.Join("      ", parts);
        lblPathStatus.ForeColor = (cpi != null && doc != null) ? Green : Red;
    }

    /// <summary>ดูว่ามีไฟล์ .uvdx ชื่อไหนบ้างที่ตรงกับที่พิมพ์ — ช่วยตอนจำชื่อเต็มไม่ได้</summary>
    private void FindProgram()
    {
        var doc = UvSettingsManager.GetDocumentFolder(_uvNumber);
        if (doc == null)
        {
            Warn("ยังไม่ได้ตั้งโฟลเดอร์ UV หรือไม่พบโฟลเดอร์ document");
            return;
        }

        string keyword = txtProgram.Text.Trim();

        List<string> names;
        try
        {
            names = Directory.GetFiles(doc, "*.uvdx")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .Where(n => keyword.Length == 0 ||
                            n!.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .ToList()!;
        }
        catch (Exception ex)
        {
            Warn($"อ่านโฟลเดอร์ไม่ได้: {ex.Message}");
            return;
        }

        if (names.Count == 0)
        {
            Warn(keyword.Length == 0
                ? "ไม่มีไฟล์ .uvdx ในโฟลเดอร์ document"
                : $"ไม่พบไฟล์ที่มีคำว่า \"{keyword}\"");
            return;
        }

        var picked = PickFromList(names, keyword);
        if (picked == null) return;

        txtProgram.Text = picked;
        Log($"เลือกโปรแกรม {picked}.uvdx");
    }

    /// <summary>กล่องเลือกรายการ — คลิกสองครั้งเลือกได้เลย</summary>
    private string? PickFromList(List<string> items, string keyword)
    {
        using var dlg = new Form
        {
            Text = keyword.Length == 0
                ? $"ไฟล์โปรแกรมทั้งหมด ({items.Count})"
                : $"ผลค้นหา \"{keyword}\" ({items.Count})",
            Size = new Size(460, 420),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowIcon = false,
        };

        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.Body(11f),
            IntegralHeight = false,
        };
        list.Items.AddRange(items.ToArray());
        list.SelectedIndex = 0;

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 56,
            Padding = new Padding(10),
        };
        var ok = new Button { Text = "เลือก", DialogResult = DialogResult.OK, Size = new Size(96, 34) };
        var cancel = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Size = new Size(96, 34) };
        bar.Controls.Add(ok);
        bar.Controls.Add(cancel);

        list.DoubleClick += (_, _) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

        dlg.Controls.Add(list);
        dlg.Controls.Add(bar);
        dlg.AcceptButton = ok;
        dlg.CancelButton = cancel;

        return dlg.ShowDialog(this) == DialogResult.OK
            ? list.SelectedItem?.ToString()
            : null;
    }

    // ── สั่งงานเครื่อง ─────────────────────────────────────

    private async Task CheckConnectionAsync()
    {
        if (!TryGetEndpoint(out var ip, out int port)) return;

        SetBusy(true);
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
            Log($"เชื่อมต่อ {ip}:{port} ไม่ได้ — {ex.Message}");
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    private async Task LoadAsync()
    {
        if (!TryGetEndpoint(out var ip, out int port)) return;

        var requested = txtProgram.Text.Trim();
        if (requested.Length == 0)
        {
            Warn("กรุณากรอกชื่อโปรแกรม");
            return;
        }

        // ใช้ตรรกะเดียวกับปุ่มส่งจริงในหน้า Order — เลือกรุ่นย่อย / ตกไป default
        var doc = UvSettingsManager.GetDocumentFolder(_uvNumber);
        var pick = UvProgramResolver.Resolve(requested, doc, this);

        if (pick.Program == null)
        {
            Log("ยกเลิกการเลือกโปรแกรม");
            return;
        }

        if (pick.IsDefault && !UvProgramResolver.ConfirmDefault(requested, CurrentName(), this))
        {
            Log($"ไม่พบ {requested}.uvdx — ยกเลิก");
            return;
        }

        if (pick.Program != requested)
            Log($"เลือกใช้ {pick.Program}.uvdx (จากที่พิมพ์ \"{requested}\")");

        await RunAsync(() => _uvTcp.LoadAsync(ip, port, pick.Program));
    }

    private async Task StartAsync()
    {
        if (!TryGetEndpoint(out var ip, out int port)) return;
        await RunAsync(() => _uvTcp.StartAsync(ip, port));
    }

    private async Task StopAsync()
    {
        if (!TryGetEndpoint(out var ip, out int port)) return;
        await RunAsync(() => _uvTcp.StopAsync(ip, port));
    }

    private async Task RunAsync(Func<Task<(bool ok, string log)>> action)
    {
        SetBusy(true);
        try
        {
            var (_, log) = await action();
            if (IsDisposed) return;
            Log(log.TrimEnd());
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    // ── CPI.db3 ────────────────────────────────────────────

    private async Task CpiWriteAsync()
    {
        if (!TryGetCpi(out var cpiPath, out var table)) return;

        if (!Confirm.Ask(this, "ยืนยันเขียน CPI.db3",
                $"เขียนทับแถว id=1 ของตาราง {table}\n{cpiPath}\n\nยืนยันหรือไม่?"))
            return;

        SetBusy(true);
        try
        {
            var (ok, msg) = await CpiWriteService.WriteAsync(
                cpiPath, table,
                txtLot.Text.Trim(), txtName.Text.Trim(),
                txtText1.Text, txtText2.Text, txtText3.Text, txtText4.Text, txtText5.Text);

            if (IsDisposed) return;
            Log(msg);
            if (!ok) Warn(msg);
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    /// <summary>ดึงค่าที่อยู่ใน CPI.db3 ตอนนี้มาเติมในช่อง — ดูก่อนว่าเครื่องถืออะไรอยู่</summary>
    private async Task CpiReadAsync()
    {
        if (!TryGetCpi(out var cpiPath, out var table)) return;

        SetBusy(true);
        try
        {
            var (ok, row, msg) = await CpiWriteService.ReadAsync(cpiPath, table);
            if (IsDisposed) return;

            Log(msg);
            if (!ok || row == null)
            {
                Warn(msg);
                return;
            }

            txtLot.Text = row.Lot ?? "";
            txtName.Text = row.Name ?? "";
            txtText1.Text = row.Text1 ?? "";
            txtText2.Text = row.Text2 ?? "";
            txtText3.Text = row.Text3 ?? "";
            txtText4.Text = row.Text4 ?? "";
            txtText5.Text = row.Text5 ?? "";
        }
        finally
        {
            if (!IsDisposed) SetBusy(false);
        }
    }

    // ── Helpers ────────────────────────────────────────────

    private bool TryGetEndpoint(out string ip, out int port)
    {
        ip = txtIp.Text.Trim();
        port = int.TryParse(txtPort.Text.Trim(), out int p) ? p : 10086;

        if (ip.Length == 0)
        {
            Warn("กรุณากรอก IP ของเครื่อง UV");
            return false;
        }
        return true;
    }

    private bool TryGetCpi(out string cpiPath, out string table)
    {
        cpiPath = "";
        table = txtTable.Text.Trim();

        var path = UvSettingsManager.GetCpiPath(_uvNumber);
        if (path == null)
        {
            Warn("ไม่พบ CPI.db3 — กรุณาเลือกโฟลเดอร์ UV ให้ถูกต้อง");
            return false;
        }

        if (table.Length == 0)
        {
            Warn("กรุณากรอกชื่อตาราง เช่น MK063");
            return false;
        }

        cpiPath = path;
        return true;
    }

    private void SetBusy(bool busy)
    {
        foreach (var b in new[]
                 {
                     btnCheck, btnLoad, btnStart, btnStop,
                     btnCpiRead, btnCpiWrite, btnFindProgram,
                 })
            b.Enabled = !busy;
    }

    private void Log(string message)
    {
        if (IsDisposed || string.IsNullOrWhiteSpace(message)) return;

        foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line.TrimEnd()}{Environment.NewLine}");
    }

    private static void Warn(string message) =>
        Notify.Warn(null, message);
}
