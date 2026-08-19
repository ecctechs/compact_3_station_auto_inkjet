using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class OrderDetailUserControl : UserControl
{
    private const string Dash = "-";

    private PatternDetail? _pattern;
    private string _barcode = "";
    private bool _isSwapped;
    private List<string> _sendSteps = [];
    private int _currentStep;
    private int _jobId;
    private ApiClient? _api;
    private ImageHoverPopup? _refPopup;
    private List<UvJobDataDto> _uvData = [];
    private readonly bool _isDevMode;

    public event EventHandler? CloseRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();

        var rawLevel = CustomSettingsManager.Read("MENU_LEVEL", "1");
        _isDevMode = int.TryParse(rawLevel, out var lvl) && lvl == 99;

        btnDetailClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        btnMkSwap.Click += (_, _) => SwapMkData();
        btnMk1Abc.Click += (_, _) => ShowAbcDialog(1);
        btnMk2Abc.Click += (_, _) => ShowAbcDialog(2);
        btnSendMk.Click += async (_, _) => await SendToMkAsync();
        btnSendUv1.Click += async (_, _) => await SendToUvAsync(1);
        btnSendUv2.Click += async (_, _) => await SendToUvAsync(2);

        WireRefImageHover();
        Disposed += (_, _) => _refPopup?.Dispose();
    }

    // ── Marking reference image (hover preview) ──────────────

    private void WireRefImageHover()
    {
        foreach (var box in new[] { txtMk1Program, txtMk2Program, txtUv1Program, txtUv2Program })
        {
            box.MouseEnter += ProgramField_MouseEnter;
            box.MouseLeave += ProgramField_MouseLeave;
        }
    }

    private void ProgramField_MouseEnter(object? sender, EventArgs e)
    {
        if (sender is not AntdUI.Input box) return;

        var paths = MarkingRefImageService.FindImages(box.Text);
        if (paths.Count == 0) return;

        _refPopup ??= new ImageHoverPopup();
        var anchor = box.PointToScreen(new Point(0, box.Height + 4));
        _refPopup.ShowImages(paths, anchor);
    }

    private void ProgramField_MouseLeave(object? sender, EventArgs e)
    {
        _refPopup?.HidePopup();
    }

    private void ConfigureColumns()
    {
        tblMk1Blocks.Columns = BuildBlockColumns();
        tblMk2Blocks.Columns = BuildBlockColumns();
        tblUv1Texts.Columns = BuildUvColumns();
        tblUv2Texts.Columns = BuildUvColumns();
    }

    private static AntdUI.ColumnCollection BuildBlockColumns() =>
    [
        new AntdUI.Column("Block", "Block", AntdUI.ColumnAlign.Center) { Width = "70" },
        new AntdUI.Column("BlockText", "Text", AntdUI.ColumnAlign.Left),
        new AntdUI.Column("X", "X", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Y", "Y", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Size", "Size", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Scale", "Scale", AntdUI.ColumnAlign.Center) { Width = "60" },
    ];

    private static AntdUI.ColumnCollection BuildUvColumns() =>
    [
        new AntdUI.Column("Field", "Field", AntdUI.ColumnAlign.Center) { Width = "90" },
        new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Left),
    ];

    public void LoadDetail(ResolvedJobResponse resolved, ApiClient? api = null)
    {
        _pattern = resolved.Pattern;
        _barcode = resolved.Job.BarcodeRaw ?? "";
        _isSwapped = false;
        _jobId = resolved.Job.Id;
        _api = api;
        _uvData = resolved.UvJobData;

        lblHeaderTitle.Text = $"Job Information — Job #{resolved.Job.Id}";

        SortPatternByOrdinal();
        FillJobInfo(resolved);
        FillMkChipLabels();
        FillUvChipLabels();
        ApplyMarkingMethodButtons(resolved.PlanRouting?.MarkingMethod);
        RestoreCompletedSteps(resolved.Commands);
        ApplyStepButtons();
        FillMkSection(_pattern);
        FillConveyor(_pattern);
        FillUvSection(resolved.UvJobData);
        ClearIaiFields();
        _ = LoadIaiAsync(resolved.Job.Id);
        _ = CheckConnectionsAsync();
    }

    private void FillMkChipLabels()
    {
        lblMk1Chip.Text = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        lblMk2Chip.Text = CustomSettingsManager.Read("MK059_NAME", "MK-059");
    }

    private void FillUvChipLabels()
    {
        lblUv1Chip.Text = UvSettingsManager.Read("UV1_NAME", "UV-001");
        lblUv2Chip.Text = UvSettingsManager.Read("UV2_NAME", "UV-002");
    }

    private async Task CheckConnectionsAsync()
    {
        var mk1Ip = CustomSettingsManager.Read("MK058_COM");
        var mk2Ip = CustomSettingsManager.Read("MK059_COM");
        var uv1Ip = CustomSettingsManager.Read("UV001_IP");
        var uv1Port = CustomSettingsManager.Read("UV001_PORT");
        var uv2Ip = CustomSettingsManager.Read("UV002_IP");
        var uv2Port = CustomSettingsManager.Read("UV002_PORT");

        var mk1Name = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2Name = CustomSettingsManager.Read("MK059_NAME", "MK-059");
        var uv1Name = UvSettingsManager.Read("UV1_NAME", "UV-001");
        var uv2Name = UvSettingsManager.Read("UV2_NAME", "UV-002");

        SetConnLabel(lblConnMk1, mk1Name, mk1Ip, "", "กำลังตรวจสอบ...", Color.Gray);
        SetConnLabel(lblConnMk2, mk2Name, mk2Ip, "", "กำลังตรวจสอบ...", Color.Gray);
        SetConnLabel(lblConnUv1, uv1Name, uv1Ip, uv1Port, "กำลังตรวจสอบ...", Color.Gray);
        SetConnLabel(lblConnUv2, uv2Name, uv2Ip, uv2Port, "กำลังตรวจสอบ...", Color.Gray);

        var results = await Task.WhenAll(
            TcpCheckAsync(mk1Ip, 9004),
            TcpCheckAsync(mk2Ip, 9004),
            TcpCheckAsync(uv1Ip, int.TryParse(uv1Port, out var p1) ? p1 : 0),
            TcpCheckAsync(uv2Ip, int.TryParse(uv2Port, out var p2) ? p2 : 0));

        if (IsDisposed) return;

        SetConnResult(lblConnMk1, mk1Name, mk1Ip, "", results[0]);
        SetConnResult(lblConnMk2, mk2Name, mk2Ip, "", results[1]);
        SetConnResult(lblConnUv1, uv1Name, uv1Ip, uv1Port, results[2]);
        SetConnResult(lblConnUv2, uv2Name, uv2Ip, uv2Port, results[3]);
    }

    private static void SetConnResult(AntdUI.Label lbl, string name, string ip, string port, bool ok)
    {
        var status = ok ? "เชื่อมต่อสำเร็จ" : "ไม่สามารถเชื่อมต่อ";
        var color = ok ? Color.FromArgb(21, 128, 61) : Color.FromArgb(220, 38, 38);
        if (string.IsNullOrWhiteSpace(ip)) { color = Color.Gray; status = "ไม่ได้ตั้งค่า"; }
        SetConnLabel(lbl, name, ip, port, status, color);
    }

    private static void SetConnLabel(AntdUI.Label lbl, string name, string ip, string port, string status, Color color)
    {
        var addr = string.IsNullOrWhiteSpace(ip) ? "—" :
            string.IsNullOrWhiteSpace(port) ? ip : $"{ip}:{port}";
        void Apply()
        {
            lbl.Text = $"●  {name}  ({addr})  {status}";
            lbl.ForeColor = color;
        }
        if (lbl.InvokeRequired) lbl.Invoke(Apply); else Apply();
    }

    private static async Task<bool> TcpCheckAsync(string ip, int port)
    {
        if (string.IsNullOrWhiteSpace(ip) || port <= 0) return false;
        var tcp = new TcpManager();
        try
        {
            await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromSeconds(3));
            return tcp.IsConnected();
        }
        catch { return false; }
        finally { tcp.Disconnect(); }
    }

    private static int Flip(int o) => o == 1 ? 2 : o == 2 ? 1 : o;

    private void SwapMkData()
    {
        if (_pattern == null) return;

        foreach (var cfg in _pattern.InkjetConfigs)
            cfg.Ordinal = Flip(cfg.Ordinal);

        foreach (var servo in _pattern.ServoConfigs)
            servo.Ordinal = Flip(servo.Ordinal);

        SortPatternByOrdinal();

        _isSwapped = !_isSwapped;
        lblMkSectionTitle.Text = _isSwapped
            ? "MK Section (MK Inkjet) — SWAPPED"
            : "MK Section (MK Inkjet)";
        lblMkSectionTitle.ForeColor = _isSwapped
            ? Color.FromArgb(212, 136, 6)
            : Color.FromArgb(36, 71, 101);

        var mk1Name = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2Name = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        lblMk1Chip.Text = _isSwapped ? mk2Name : mk1Name;
        lblMk2Chip.Text = _isSwapped ? mk1Name : mk2Name;

        FillMkSection(_pattern);
    }

    private void SortPatternByOrdinal()
    {
        if (_pattern == null) return;
        _pattern.InkjetConfigs = _pattern.InkjetConfigs
            .OrderBy(c => c.Ordinal).ToList();
        _pattern.ServoConfigs = _pattern.ServoConfigs
            .OrderBy(s => s.Ordinal).ToList();
    }

    private void ShowAbcDialog(int ordinal)
    {
        if (_pattern == null) return;

        var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
        if (config == null)
        {
            MessageBox.Show($"ไม่พบ InkjetConfig ordinal {ordinal}",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var lines = config.TextBlocks
            .OrderBy(b => b.BlockNumber)
            .Select(b => $"[Block {b.BlockNumber}]  {b.Text ?? Dash}")
            .ToList();

        var preview = lines.Count == 0
            ? "(No text blocks)"
            : string.Join(Environment.NewLine, lines);

        using var dlg = new Form
        {
            Text = $"MK-{ordinal} — Text Preview",
            Size = new Size(480, 320),
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowIcon = false,
        };

        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 13F),
            BackColor = Color.FromArgb(237, 243, 249),
            ForeColor = Color.FromArgb(17, 17, 17),
            Text = preview,
        };

        dlg.Controls.Add(txt);
        dlg.ShowDialog();
    }

    private void ApplyMarkingMethodButtons(string? markingMethod)
    {
        char a = '0', b = '0';
        if (markingMethod is { Length: >= 2 })
        {
            a = markingMethod[0]; // Shim digit
            b = markingMethod[1]; // Plate digit
        }

        // '3' behaves the same as '1' (UV).
        if (a == '3') a = '1';
        if (b == '3') b = '1';

        // "No case": Shim=Inkjet(2) + Plate=UV(1) — e.g. 21 — does not exist per spec.
        if (a == '2' && b == '1')
        {
            _sendSteps = [];
            _currentStep = 0;
            lblMarkingFlow.Text = "ไม่มีเคสนี้";
            ApplyStepButtons();
            return;
        }

        bool needMk = a == '2' || b == '2';
        bool needUv1 = b == '1';
        bool needUv2 = a == '1';

        _sendSteps = [];
        if (needMk) _sendSteps.Add("MK");
        if (needUv1) _sendSteps.Add("UV1");
        if (needUv2) _sendSteps.Add("UV2");
        _currentStep = 0;

        // Show which machine marks the Plate / Shim.
        // marking_method = [Shim][Plate]: 0=None, 1=UV (Plate→UV1, Shim→UV2), 2=Inkjet(MK).
        string plate = b switch { '1' => "UV1", '2' => "MK", _ => "None" };
        string shim = a switch { '1' => "UV2", '2' => "MK", _ => "None" };
        string twoRound = (a == '2' && b == '2') ? "   (MK วิ่ง 2 รอบ)" : "";
        lblMarkingFlow.Text = $"Plate - {plate}{Environment.NewLine}Shim - {shim}{twoRound}";

        ApplyStepButtons();
    }

    private void RestoreCompletedSteps(List<CommandResult> commands)
    {
        var completed = new HashSet<string>(
            commands
                .Where(c => c.Success)
                .Select(c => c.Command),
            StringComparer.OrdinalIgnoreCase);

        while (_currentStep < _sendSteps.Count
               && completed.Contains(_sendSteps[_currentStep]))
        {
            _currentStep++;
        }
    }

    private void ApplyStepButtons()
    {
        if (_isDevMode)
        {
            btnSendMk.Enabled = true;
            btnSendUv1.Enabled = true;
            btnSendUv2.Enabled = true;
            return;
        }

        btnSendMk.Enabled = false;
        btnSendUv1.Enabled = false;
        btnSendUv2.Enabled = false;

        if (_currentStep < _sendSteps.Count)
        {
            var step = _sendSteps[_currentStep];
            GetSendButton(step).Enabled = true;
        }

        for (int i = 0; i < _currentStep && i < _sendSteps.Count; i++)
            MarkButtonSent(GetSendButton(_sendSteps[i]));
    }

    private void CompleteSendStep(string stepName)
    {
        if (_isDevMode)
        {
            _ = _api?.SaveSendStepAsync(_jobId, stepName);
            return;
        }

        if (_currentStep >= _sendSteps.Count) return;
        if (_sendSteps[_currentStep] != stepName) return;

        var btn = GetSendButton(stepName);
        btn.Enabled = false;
        MarkButtonSent(btn);

        _currentStep++;
        if (_currentStep < _sendSteps.Count)
            GetSendButton(_sendSteps[_currentStep]).Enabled = true;

        _ = _api?.SaveSendStepAsync(_jobId, stepName);
    }

    private AntdUI.Button GetSendButton(string step) => step switch
    {
        "MK" => btnSendMk,
        "UV1" => btnSendUv1,
        _ => btnSendUv2,
    };

    private static void MarkButtonSent(AntdUI.Button btn)
    {
        btn.Enabled = false;
        btn.DefaultBack = Color.FromArgb(200, 220, 200);
        btn.ForeColor = Color.FromArgb(21, 128, 61);
        btn.Type = AntdUI.TTypeMini.Default;
        if (btn.Text?.StartsWith('✓') != true)
            btn.Text = "✓ " + btn.Text;
    }

    private async Task SendToMkAsync()
    {
        if (_pattern == null) return;

        btnSendMk.Enabled = false;
        var originalText = btnSendMk.Text;
        btnSendMk.Text = "กำลังส่ง...";

        try
        {
            var mk1Ip = CustomSettingsManager.Read("MK058_COM");
            var mk2Ip = CustomSettingsManager.Read("MK059_COM");
            var config1 = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == 1);
            var config2 = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == 2);

            var errors = new List<string>();
            int sent = 0;

            if (config1 != null && !string.IsNullOrWhiteSpace(mk1Ip))
            {
                var err = await SendToOneMkAsync(mk1Ip, config1, "MK1");
                if (err != null) errors.Add(err);
                else sent++;
            }

            if (config2 != null && !string.IsNullOrWhiteSpace(mk2Ip))
            {
                var err = await SendToOneMkAsync(mk2Ip, config2, "MK2");
                if (err != null) errors.Add(err);
                else sent++;
            }

            if (errors.Count > 0 || sent == 0)
            {
                btnSendMk.Text = originalText;
                btnSendMk.Enabled = true;
                var msg = sent == 0 && errors.Count == 0
                    ? "ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"
                    : string.Join("\n", errors);
                MessageBox.Show(msg,
                    "ส่งไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CompleteSendStep("MK");
        }
        catch (Exception ex)
        {
            btnSendMk.Text = originalText;
            btnSendMk.Enabled = true;
            MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static async Task<string?> SendToOneMkAsync(string ip, InkjetConfigDto config, string label)
    {
        var tcp = new TcpManager();
        try
        {
            await tcp.ConnectAsync(ip, 9004).WaitAsync(TimeSpan.FromSeconds(3));
            var adapter = new MkCompactAdapter(tcp);

            var sr = await adapter.SuspendAsync();
            if (!sr.Success) return $"{label}: Suspend ไม่สำเร็จ";

            var fw = await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1);
            if (!fw.Success) return $"{label}: เปลี่ยนโปรแกรมไม่สำเร็จ";

            var fm = await adapter.SendConfigAsync(config);
            if (!fm.Success) return $"{label}: ส่ง Config ไม่สำเร็จ";

            foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber))
            {
                var fb = await adapter.SendTextBlockAsync(block, block.BlockNumber);
                if (!fb.Success) return $"{label}: ส่ง Block {block.BlockNumber} ไม่สำเร็จ";
            }

            var sq = await adapter.ResumeAsync();
            if (!sq.Success) return $"{label}: Resume ไม่สำเร็จ";

            return null;
        }
        catch (Exception ex)
        {
            return $"{label}: {ex.Message}";
        }
        finally
        {
            tcp.Disconnect();
        }
    }

    /// <summary>
    /// ปุ่มเดียวจบงาน: หยุดเครื่อง → เขียน CPI.db3 → โหลดโปรแกรม → เริ่มพิมพ์
    ///
    /// dialog ที่ถามผู้ใช้ทั้งหมดต้องจบก่อนคำสั่งแรกที่ส่งถึงเครื่อง
    /// ถ้าถามทีหลังแล้วผู้ใช้กดยกเลิก จะทิ้งเครื่องค้างอยู่ในสถานะหยุดโดยไม่ตั้งใจ
    /// </summary>
    private async Task SendToUvAsync(int uvNumber)
    {
        string stepName = uvNumber == 1 ? "UV1" : "UV2";
        string table = uvNumber == 1 ? "MK063" : "MK067";
        var btn = GetSendButton(stepName);

        var uvRow = _uvData.FirstOrDefault(r => r.Machine == stepName);
        if (uvRow == null)
        {
            MessageBox.Show($"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก",
                "ไม่มีข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cpiPath = UvSettingsManager.GetCpiPath(uvNumber);
        if (cpiPath == null)
        {
            MessageBox.Show($"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3",
                "ตั้งค่าไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var ip = CustomSettingsManager.Read($"UV00{uvNumber}_IP");
        if (string.IsNullOrWhiteSpace(ip))
        {
            MessageBox.Show($"ยังไม่ได้ตั้งค่า IP ของ UV{uvNumber}",
                "ตั้งค่าไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        int port = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p)
            ? p
            : 10086;

        var uvName = uvNumber == 1
            ? UvSettingsManager.Read("UV1_NAME", "UV-001")
            : UvSettingsManager.Read("UV2_NAME", "UV-002");

        if (!await TestUvConnectionAsync(ip, port, uvName))
            return;

        var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber);
        var pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, this);

        var programFile = pick.Program;
        if (programFile == null) return;

        if (pick.IsDefault &&
            !UvProgramResolver.ConfirmDefault(uvRow.ProgramName ?? "", uvName, this))
            return;

        var originalText = btn.Text;
        btn.Enabled = false;
        btn.Text = "กำลังส่ง...";

        var done = new List<string>();
        try
        {
            var uvTcp = new UvTcpService();

            // 1. หยุดเครื่องก่อนเสมอ — ไม่ตอบรับก็ไปต่อ เพราะเครื่องอาจหยุดอยู่แล้ว
            var (stopOk, _) = await uvTcp.StopAsync(ip, port);
            done.Add(stopOk ? "สั่งหยุดเครื่อง" : "สั่งหยุดเครื่อง (ไม่ตอบรับ — ทำต่อ)");

            // 2. เขียนข้อความลง CPI.db3
            var (writeOk, writeMsg) = await CpiWriteService.WriteAsync(
                cpiPath, table,
                uvRow.Lot, uvRow.ErpMfg,
                uvRow.Text1, uvRow.Text2, uvRow.Text3, uvRow.Text4, uvRow.Text5);

            if (!writeOk)
            {
                ShowUvFailure(uvName, done, $"เขียน CPI.db3 ({table}) — {writeMsg}");
                return;
            }
            done.Add($"เขียน CPI.db3 ({table})\n    Lot: {OrDash(uvRow.Lot)}\n    Name: {OrDash(uvRow.ErpMfg)}");

            // 3. โหลดโปรแกรม แล้วสั่งเริ่มพิมพ์
            var (tcpOk, tcpLog) = await uvTcp.LoadAndStartAsync(ip, port, programFile);
            if (!tcpOk)
            {
                ShowUvFailure(uvName, done, tcpLog.Trim());
                return;
            }
            done.Add($"โหลดโปรแกรม {programFile}.uvdx");
            done.Add("สั่งเริ่มพิมพ์");

            CompleteSendStep(stepName);

            var summary = $"ส่ง {uvName} สำเร็จ\n\n"
                + string.Join("\n", done.Select(s => "• " + s))
                + (pick.IsDefault ? "\n\n⚠ ใช้ default.uvdx เพราะไม่พบโปรแกรมที่ต้องการ" : "");

            MessageBox.Show(summary, $"{uvName} — สำเร็จ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowUvFailure(uvName, done, ex.Message);
        }
        finally
        {
            // สำเร็จแล้ว CompleteSendStep จะ MarkButtonSent ให้เอง นอกนั้นคืนปุ่มกลับสภาพเดิม
            if (btn.Text?.StartsWith('✓') != true)
            {
                btn.Text = originalText;
                btn.Enabled = true;
            }
        }
    }

    private static async Task<bool> TestUvConnectionAsync(string ip, int port, string uvName)
    {
        try
        {
            using var tcp = new System.Net.Sockets.TcpClient();
            await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromSeconds(3));
            return true;
        }
        catch
        {
            MessageBox.Show(
                $"ไม่สามารถเชื่อมต่อ {uvName} ({ip}:{port}) ได้\n\n"
                + "กรุณาตรวจสอบ:\n"
                + "• เครื่อง UV เปิดอยู่หรือไม่\n"
                + "• ซอฟต์แวร์ UV เปิดอยู่หรือไม่\n"
                + "• IP และ Port ถูกต้องหรือไม่",
                $"{uvName} — เชื่อมต่อไม่ได้",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    /// <summary>บอกว่าทำอะไรสำเร็จไปแล้วบ้างและหยุดที่ขั้นไหน — เครื่องยังค้างอยู่ในสถานะหยุด</summary>
    private static void ShowUvFailure(string uvName, List<string> done, string failReason)
    {
        var msg = $"ส่ง {uvName} ไม่สำเร็จ\n\n"
            + string.Join("\n", done.Select(s => "✔ " + s))
            + (done.Count > 0 ? "\n" : "")
            + $"✖ {failReason}\n\n"
            + "ยังไม่ได้สั่งเริ่มพิมพ์ — เครื่องอยู่ในสถานะหยุด";

        MessageBox.Show(msg, $"{uvName} — ไม่สำเร็จ",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void FillJobInfo(ResolvedJobResponse resolved)
    {
        var job = resolved.Job;

        txtJobOrderNo.Text = OrDash(job.OrderNo);
        txtJobLotNo.Text = OrDash(job.BarcodeRaw);
        txtJobCustomer.Text = OrDash(job.CustomerName);
        txtJobType.Text = OrDash(job.Type);
        txtJobQty.Text = job.Qty?.ToString() ?? Dash;
        txtJobStatus.Text = OrDash(job.Status);

        var marking = resolved.PlanRouting?.MarkingMethod;
        txtMarkingMethod.Text = string.IsNullOrWhiteSpace(marking) ? "ไม่ระบุ" : marking;
    }

    private void FillMkSection(PatternDetail pattern)
    {
        var configs = pattern.InkjetConfigs;
        var servos = pattern.ServoConfigs;

        FillMk(
            configs.FirstOrDefault(c => c.Ordinal == 1),
            servos.FirstOrDefault(s => s.Ordinal == 1),
            txtMk1Program, txtMk1ProgramNo, txtMk1Width, txtMk1Height,
            txtMk1Trigger, txtMk1PosAct, txtMk1Delay, tblMk1Blocks);

        FillMk(
            configs.FirstOrDefault(c => c.Ordinal == 2),
            servos.FirstOrDefault(s => s.Ordinal == 2),
            txtMk2Program, txtMk2ProgramNo, txtMk2Width, txtMk2Height,
            txtMk2Trigger, txtMk2PosAct, txtMk2Delay, tblMk2Blocks);
    }

    private void FillMk(
        InkjetConfigDto? config, ServoConfigDto? servo,
        AntdUI.Input program, AntdUI.Input programNo,
        AntdUI.Input width, AntdUI.Input height,
        AntdUI.Input trigger, AntdUI.Input posAct, AntdUI.Input delay,
        AntdUI.Table table)
    {
        program.Text = OrDash(config?.ProgramName);
        programNo.Text = Number(config?.ProgramNumber);
        width.Text = Number(config?.Width);
        height.Text = Number(config?.Height);
        trigger.Text = Number(config?.TriggerDelay);
        posAct.Text = Number(servo?.PostAct);
        delay.Text = Number(servo?.Delay);

        var rows = (config?.TextBlocks ?? [])
            .OrderBy(b => b.BlockNumber)
            .Select(b =>
            {
                var original = b.Text ?? "";
                var result = PatternEngine.Process(_barcode, original);
                bool matched = !string.IsNullOrEmpty(_barcode)
                    && !string.IsNullOrEmpty(original)
                    && result != original;

                return new BlockRow
                {
                    Block = b.BlockNumber.ToString(),
                    BlockText = OrDash(matched ? result : original),
                    X = Number(b.X),
                    Y = Number(b.Y),
                    Size = Number(b.Size),
                    Scale = Number(b.Scale),
                };
            })
            .ToList();

        table.DataSource = null;
        table.DataSource = rows;
    }

    private void FillConveyor(PatternDetail pattern)
    {
        var speeds = pattern.ConveyorSpeeds;
        txtConveyor1.Text = Number(speeds?.Speed1);
        txtConveyor2.Text = Number(speeds?.Speed2);
        txtConveyor3.Text = Number(speeds?.Speed3);
    }

    private void FillUvSection(List<UvJobDataDto> uvRows)
    {
        var uv1 = uvRows.FirstOrDefault(r => r.Machine == "UV1");
        var uv2 = uvRows.FirstOrDefault(r => r.Machine == "UV2");

        txtUvQtyShared.Text = (uv1?.Qty ?? uv2?.Qty)?.ToString() ?? Dash;

        FillUv(uv1, txtUv1Program, txtUv1ErpMfg, tblUv1Texts);
        FillUv(uv2, txtUv2Program, txtUv2ErpMfg, tblUv2Texts);
    }

    /// <summary>
    /// โหลดระยะแคลมป์ของงานจาก backend มาโชว์ใต้ Program Name
    /// UV1 = Plate → iaip/z1/z2 · UV2 = Shim → iai/z1/z2
    /// ค่า null โชว์ "-" เพื่อให้เห็นว่ามีช่องนี้อยู่แต่ยังไม่ได้ setup
    /// </summary>
    private async Task LoadIaiAsync(int jobId)
    {
        if (_api == null || jobId <= 0) return;

        var (iai, _) = await _api.GetIaiByJobAsync(jobId);
        if (IsDisposed) return;

        // ไม่มีข้อมูล (404) → คงค่า "-" ที่ ClearIaiFields ตั้งไว้
        if (iai == null) return;

        txtUv1Iai.Text = Number(iai.Iaip);
        txtUv1IaiZ1.Text = Number(iai.IaipZ1);
        txtUv1IaiZ2.Text = Number(iai.IaipZ2);

        txtUv2Iai.Text = Number(iai.Iai);
        txtUv2IaiZ1.Text = Number(iai.IaiZ1);
        txtUv2IaiZ2.Text = Number(iai.IaiZ2);
    }

    private void ClearIaiFields()
    {
        foreach (var box in new[]
                 {
                     txtUv1Iai, txtUv1IaiZ1, txtUv1IaiZ2,
                     txtUv2Iai, txtUv2IaiZ1, txtUv2IaiZ2,
                 })
            box.Text = Dash;
    }

    private static void FillUv(
        UvJobDataDto? uv,
        AntdUI.Input program, AntdUI.Input erpMfg,
        AntdUI.Table table)
    {
        program.Text = OrDash(uv?.ProgramName);
        erpMfg.Text = OrDash(uv?.ErpMfg);

        var values = new[] { uv?.Text1, uv?.Text2, uv?.Text3, uv?.Text4, uv?.Text5 };
        var rows = values
            .Select((v, i) => new UvTextRow { Field = $"Text{i + 1}", Value = OrDash(v) })
            .ToList();

        table.DataSource = null;
        table.DataSource = uv == null ? null : rows;
    }

    private static string OrDash(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Dash : value;

    private static string Number(int? value) => value?.ToString() ?? Dash;

    private static string Number(double? value) => value?.ToString() ?? Dash;
}

internal class BlockRow : AntdUI.NotifyProperty
{
    public string Block { get; set; } = "";
    public string BlockText { get; set; } = "";
    public string X { get; set; } = "";
    public string Y { get; set; } = "";
    public string Size { get; set; } = "";
    public string Scale { get; set; } = "";
}

internal class UvTextRow : AntdUI.NotifyProperty
{
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
}
