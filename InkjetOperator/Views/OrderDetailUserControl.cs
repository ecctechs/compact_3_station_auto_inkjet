using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

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
    private bool _transferMode;
    private IaiClampSettingDto? _origIai;

    public event EventHandler? CloseRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();

        var rawLevel = CustomSettingsManager.Read("MENU_LEVEL", "1");
        _isDevMode = int.TryParse(rawLevel, out var lvl) && lvl == 99;

        btnDetailClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        btnMkSwap.Click += (_, _) => SwapMkData();
        picMk1Abc.Click += (_, _) => ToggleAbc(1, picMk1Abc);
        picMk2Abc.Click += (_, _) => ToggleAbc(2, picMk2Abc);
        btnSendMk.Click += async (_, _) => await SendToMkAsync();
        btnSendUv1.Click += async (_, _) => await SendToUvAsync(1);
        btnSendUv2.Click += async (_, _) => await SendToUvAsync(2);
        btnSendToSt1.Click += async (_, _) => await SendToSt1Async();
        btnTestPlc.Click += async (_, _) => await TestPlcAsync();

        btnFlowPlate.Click += (_, _) => OpenFlowRefImages(btnFlowPlate);
        btnFlowShim.Click += (_, _) => OpenFlowRefImages(btnFlowShim);

        WireIaiAdjustEvents();
        ShowClampAddresses();
        WireRefImageHover();
        Disposed += (_, _) => _refPopup?.Dispose();
    }

    // ── Marking reference image (hover preview) ──────────────

    /// <summary>
    /// เหลือเฉพาะช่องชื่อโปรแกรมของ UV
    ///
    /// ฝั่ง MK ย้ายไปอยู่บนบรรทัด marking method แล้ว และเปลี่ยนเป็นกดคลิก
    /// ไม่ใช่ hover — hover ไม่มีอะไรบอกว่ามีรูปให้ดู
    /// </summary>
    private void WireRefImageHover()
    {
        foreach (var box in new[] { txtUv1Program, txtUv2Program })
        {
            box.MouseEnter += ProgramField_MouseEnter;
            box.MouseLeave += ProgramField_MouseLeave;
        }
    }

    private void ProgramField_MouseEnter(object? sender, EventArgs e)
    {
        if (sender is not AntdUI.Input box) return;

        var name = box.Text.Trim();
        if (name.Length == 0 || name == Dash) return;

        // ยังไม่รู้ว่าจะเลือกรุ่นย่อยไหน ให้ดูรวมทุกรุ่นไปก่อน
        var paths = MarkingRefImageService.FindImages(name);
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
        new AntdUI.Column("Block", "Block", AntdUI.ColumnAlign.Center) { Width = "16%" },
        new AntdUI.Column("BlockText", "Text", AntdUI.ColumnAlign.Left) { Width = "36%" },
        new AntdUI.Column("X", "X", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Y", "Y", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Size", "Size", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Scale", "Scale", AntdUI.ColumnAlign.Center) { Width = "12%" },
    ];

    private static AntdUI.ColumnCollection BuildUvColumns() =>
    [
        new AntdUI.Column("Field", "Field", AntdUI.ColumnAlign.Center) { Width = "30%" },
        new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Left) { Width = "70%" },
    ];

    public void SetTransferMode() => _transferMode = true;

    public void LoadDetail(ResolvedJobResponse resolved, ApiClient? api = null)
    {
        _pattern = resolved.Pattern;
        _barcode = resolved.Job.BarcodeRaw ?? "";
        _isSwapped = false;
        _jobId = resolved.Job.Id;
        _api = api;
        _uvData = resolved.UvJobData;

        lblHeaderTitle.Text = $"Job Information — Job #{resolved.Job.Id}";

        // โชว์ address ที่ค่าแต่ละช่องจะถูกส่งไป ดึงจากตาราง register map ของ
        // หน้า PLC Setting ไม่ได้ให้รอ เพราะแค่ป้ายกำกับ ไม่ควรหน่วงการเปิดหน้า
        _ = ShowPlcAddressesAsync();

        SortPatternByOrdinal();
        FillJobInfo(resolved);
        FillMkChipLabels();
        FillUvChipLabels();
        ApplyMarkingMethodButtons(resolved.PlanRouting?.MarkingMethod, resolved.PlanRouting?.ErpMfg);
        RestoreCompletedSteps(resolved.Commands);
        ApplyStepButtons();
        FillMkSection(_pattern);
        FillConveyor(_pattern);
        FillUvSection(resolved.UvJobData, resolved.Commands);
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
        var color = ok ? DesignTokens.SuccessText : DesignTokens.Danger;
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
            ? DesignTokens.Warning
            : DesignTokens.DarkNavy;

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

    // ── ABC = พิมพ์กลับหัว ─────────────────────────────────

    /// <summary>
    /// สลับทิศทางการพิมพ์ของเครื่อง MK ตัวนั้น ระหว่างปกติกับกลับหัว 180 องศา
    ///
    /// เขียนลง InkjetConfig ที่ถืออยู่ในหน้านี้ ตอนกดส่ง MK ค่าจะถูกใส่ไปในคำสั่ง
    /// FM เอง ไม่ได้บันทึกกลับ backend — กดแล้วมีผลกับการส่งรอบนี้เท่านั้น เปิด
    /// Order Detail ใหม่จะกลับไปใช้ค่าที่เก็บไว้ในดาต้าเบส เหมือนโปรแกรมเดิมที่
    /// อ่านมุมจากบนจอตอนกดส่ง ไม่ได้เขียนกลับลงไฟล์ตั้งต้น
    /// </summary>
    private void ToggleAbc(int ordinal, PictureBox box)
    {
        if (_pattern == null) return;

        var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
        if (config == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบ InkjetConfig ordinal {ordinal}");
            return;
        }

        config.Direction = MkCompactAdapter.IsFlipped(config.Direction)
            ? MkCompactAdapter.DirectionNormal
            : MkCompactAdapter.DirectionFlipped;

        ApplyAbc(box, config.Direction);
    }

    /// <summary>
    /// วาดคำว่า ABC หัวตั้งหรือหัวกลับลงในกรอบ เหมือน canvas ของโปรแกรมเดิม
    ///
    /// ตัวอักษรที่พลิกจริงอ่านออกทันทีจากอีกฝั่งของเครื่อง ต่างจากการเปลี่ยนแค่สี
    /// ปุ่ม ซึ่งสำคัญเพราะพิมพ์กลับหัวผิดคืองานเสียทั้งล็อต
    ///
    /// วาดเป็น SVG แล้วให้ AntdUI แปลงเป็นบิตแมป WinForms หมุนข้อความบนคอนโทรล
    /// เองไม่ได้ถ้าไม่เขียนโค้ดวาดทับ ซึ่งกฎของโปรเจคนี้ห้ามไว้
    /// </summary>
    private static void ApplyAbc(PictureBox box, int? direction)
    {
        // หมุนรอบจุดกึ่งกลางของตัวอักษร (y=20) ไม่ใช่รอบเส้นฐาน (y=29)
        // ไม่งั้นหัวตั้งกับหัวกลับจะลอยอยู่คนละระดับในกรอบ
        string rotate = MkCompactAdapter.IsFlipped(direction)
            ? " transform='rotate(180 50 20)'"
            : "";

        string svg = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 40'>"
            + $"<text x='50' y='29' font-family='Segoe UI, Arial' font-size='26' "
            + $"font-weight='bold' text-anchor='middle' fill='#000000'{rotate}>ABC</text></svg>";

        var previous = box.Image;
        box.Image = AntdUI.SvgExtend.SvgToBmp(svg, 240, 96, DesignTokens.DarkNavy);
        previous?.Dispose();
    }

    /// <summary>
    /// กฎการแปล marking_method อยู่ที่ <see cref="MarkingMethodService"/> ที่เดียว
    /// หน้า Order List ใช้ตัวเดียวกัน ห้ามตีความซ้ำที่นี่
    /// </summary>
    private void ApplyMarkingMethodButtons(string? markingMethod, string? erpMfg)
    {
        var plan = MarkingMethodService.Resolve(markingMethod);

        if (plan.NoCase)
        {
            _sendSteps = [];
            _currentStep = 0;
            ApplyFlowRow(btnFlowPlate, "Plate", MarkingMachine.None, null, "   ไม่มีเคสนี้");
            ApplyFlowRow(btnFlowShim, "Shim", MarkingMachine.None, null, "   ไม่มีเคสนี้");
            ApplyStepButtons();
            return;
        }

        _sendSteps = new List<string>(plan.Steps);
        _currentStep = 0;

        ApplyFlowRow(btnFlowPlate, "Plate", plan.Plate, ErpRefName(plan.Plate, erpMfg, "P-"), "");
        ApplyFlowRow(btnFlowShim, "Shim", plan.Shim, ErpRefName(plan.Shim, erpMfg, "S-"), "");

        ApplyStepButtons();
    }

    /// <summary>
    /// หนึ่งบรรทัดของ marking method — "Plate - MK - (P-ABC123)"
    ///
    /// ชื่อรูปอ้างอิงฝั่ง MK เคยเป็นช่องกรอกแยกอยู่ใน MK Section แล้วต้อง
    /// เอาเมาส์ไปจ่อถึงจะเห็นรูป ตอนนี้ต่อท้ายบรรทัดที่บอกอยู่แล้วว่าด้านนี้ใครมาร์ก
    /// และมีไอคอนรูปกำกับว่ากดได้ — บรรทัดที่ไม่มีรูปจะไม่มีไอคอนและกดไม่ได้
    /// </summary>
    private static void ApplyFlowRow(
        AntdUI.Button row, string side, MarkingMachine machine, string? refName, string suffix)
    {
        bool hasRef = refName != null && refName != Dash;

        row.Text = $"{side} - {MachineLabel(machine)}"
            + (hasRef ? $" - ({refName})" : "")
            + suffix;

        // Tag พาชื่อรูปไปให้ตัวจัดการคลิก บรรทัดไหนไม่มีรูปก็ไม่มี Tag
        row.Tag = hasRef ? refName : null;

        // บรรทัดที่กดไม่ได้ถอดกรอบกับพื้นออกให้เหลือเป็นข้อความเปล่า ๆ ไม่ใช้ Enabled
        // เพราะปุ่มที่ถูก disable จะจางลงทั้งบรรทัด ทั้งที่ "Plate - UV1" เป็นข้อมูล
        // ที่ต้องอ่านออกเท่า ๆ กับบรรทัดที่กดได้
        row.IconSvg = hasRef ? "PictureOutlined" : null;
        row.BorderWidth = hasRef ? 1F : 0F;
        row.DefaultBack = hasRef ? System.Drawing.Color.FromArgb(237, 243, 249) : Color.Transparent;
        row.Cursor = hasRef ? Cursors.Hand : Cursors.Default;
    }

    /// <summary>
    /// ชื่อรูปฝั่ง MK ลงท้าย "-1" "-2" คือคนละงาน ไม่ใช่รุ่นย่อยของงานเดียวกัน
    /// จึงค้นแบบตรงเป๊ะ ต่างจากฝั่ง UV ที่ยังไม่รู้ว่าจะเลือกรุ่นไหน
    /// </summary>
    private void OpenFlowRefImages(AntdUI.Button row)
    {
        if (row.Tag is not string name || name.Length == 0) return;

        var paths = MarkingRefImageService.FindImagesExact(name);
        if (paths.Count == 0)
        {
            Notify.WarnModal(this, "รูปอ้างอิง",
                MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder()));
            return;
        }

        MarkingRefPickerDialog.View(this, $"รูปอ้างอิง — {name}", name, paths);
    }

    private static string MachineLabel(MarkingMachine machine) =>
        MarkingMethodService.Label(machine);

    private static string ErpRefName(MarkingMachine machine, string? erpMfg, string prefix)
    {
        if (machine != MarkingMachine.Mk) return Dash;

        var erp = (erpMfg ?? "").Trim();
        return erp.Length == 0 ? Dash : prefix + erp;
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

        if (_transferMode)
        {
            btnSendMk.Visible = false;
            btnSendUv1.Visible = false;
            btnSendUv2.Visible = false;
            btnSendToSt1.Visible = true;
            btnSendToSt1.Enabled = true;
            return;
        }

        if (_currentStep < _sendSteps.Count)
        {
            var step = _sendSteps[_currentStep];
            GetSendButton(step).Enabled = true;
        }

        for (int i = 0; i < _currentStep && i < _sendSteps.Count; i++)
            MarkButtonSent(GetSendButton(_sendSteps[i]));
    }

    /// <summary>
    /// <paramref name="detail"/> เก็บลง payload ของ command — ฝั่ง UV ใช้บันทึกว่า
    /// พิมพ์ด้วยรุ่นย่อยไหนจริง ไม่งั้นย้อนดูทีหลังไม่รู้ว่าเป็น ABC-1 หรือ ABC-2
    /// </summary>
    private void CompleteSendStep(string stepName, object? detail = null)
    {
        if (_isDevMode)
        {
            _ = _api?.SaveSendStepAsync(_jobId, stepName, detail);
            return;
        }

        if (_currentStep >= _sendSteps.Count) return;
        if (_sendSteps[_currentStep] != stepName) return;

        bool isFirstStep = _currentStep == 0;

        var btn = GetSendButton(stepName);
        btn.Enabled = false;
        MarkButtonSent(btn);

        _currentStep++;
        if (_currentStep < _sendSteps.Count)
            GetSendButton(_sendSteps[_currentStep]).Enabled = true;

        _ = _api?.SaveSendStepAsync(_jobId, stepName, detail);

        if (isFirstStep)
            _ = _api?.UpdateJobStatusAsync(_jobId, "Process");
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
        btn.DefaultBack = DesignTokens.SuccessSoft;
        btn.ForeColor = DesignTokens.SuccessText;
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
            var mk1Name = CustomSettingsManager.Read("MK058_NAME", "MK-058");
            var mk2Name = CustomSettingsManager.Read("MK059_NAME", "MK-059");
            var config1 = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == 1);
            var config2 = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == 2);

            // เก็บผลแยกทีละเครื่อง — เครื่องหนึ่งสำเร็จอีกเครื่องพลาดเป็นเรื่องปกติ
            // สรุปรวมเป็นบรรทัดเดียวจะไม่รู้ว่าเครื่องไหนไม่ผ่าน
            var lines = new List<Notify.ResultLine>();
            int sent = 0;

            if (config1 != null && !string.IsNullOrWhiteSpace(mk1Ip))
            {
                var err = await SendToOneMkAsync(mk1Ip, config1, "MK1");
                if (err == null) { sent++; lines.Add(Notify.Ok($"{mk1Name} — ส่งสำเร็จ")); }
                else lines.Add(Notify.Bad($"{mk1Name} — {err}"));
            }

            if (config2 != null && !string.IsNullOrWhiteSpace(mk2Ip))
            {
                var err = await SendToOneMkAsync(mk2Ip, config2, "MK2");
                if (err == null) { sent++; lines.Add(Notify.Ok($"{mk2Name} — ส่งสำเร็จ")); }
                else lines.Add(Notify.Bad($"{mk2Name} — {err}"));
            }

            if (lines.Count == 0)
                lines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"));

            bool allOk = sent > 0 && sent == lines.Count;


            if (!allOk)
            {
                btnSendMk.Text = originalText;
                btnSendMk.Enabled = true;
                Notify.Result(this, "ผลการส่ง MK", lines);
                return;
            }

            CompleteSendStep("MK");
            Notify.Result(this, "ผลการส่ง MK", lines);
        }
        catch (Exception ex)
        {
            btnSendMk.Text = originalText;
            btnSendMk.Enabled = true;
            Notify.ErrorModal(this, "Error", $"เกิดข้อผิดพลาด: {ex.Message}");
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

            foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber))
            {
                var fb = await adapter.SendTextBlockAsync(block, block.BlockNumber);
                if (!fb.Success) return $"{label}: ส่ง Block {block.BlockNumber} ไม่สำเร็จ";
            }

            // FM ต้องมาหลัง FS/F1 ตามสเปกของเครื่อง (FW -> FS/F1 -> FM)
            // เดิมส่ง FM ก่อน Block ทิศทางที่ตั้งไว้จึงถูก Block ที่ตามมาเขียนทับ
            // ปุ่ม ABC เลยกดแล้วเครื่องพิมพ์หัวตั้งเหมือนเดิม
            var fm = await adapter.SendConfigAsync(config);
            if (!fm.Success) return $"{label}: ส่ง Config ไม่สำเร็จ";

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
            Notify.WarnModal(this, "แจ้งเตือน", $"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก");
            return;
        }

        var cpiPath = UvSettingsManager.GetCpiPath(uvNumber);
        if (cpiPath == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3");
            return;
        }

        var ip = CustomSettingsManager.Read($"UV00{uvNumber}_IP");
        if (string.IsNullOrWhiteSpace(ip))
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ยังไม่ได้ตั้งค่า IP ของ UV{uvNumber}");
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

            CompleteSendStep(stepName, new
            {
                requested = uvRow.ProgramName ?? "",
                program = programFile,
                is_default = pick.IsDefault,
            });

            // ช่อง Program ยังเป็นชื่อฐานอยู่ ถ้าไม่อัปเดตหน้าจอจะบอกคนละตัวกับที่เครื่องพิมพ์
            // และ hover ดูรูปจะได้รูปของรุ่นที่พิมพ์จริงด้วย
            (uvNumber == 1 ? txtUv1Program : txtUv2Program).Text = programFile;

            var summary = $"ส่ง {uvName} สำเร็จ\n\n"
                + string.Join("\n", done.Select(s => "• " + s))
                + (pick.IsDefault ? "\n\n⚠ ใช้ default.uvdx เพราะไม่พบโปรแกรมที่ต้องการ" : "");

            Notify.SuccessDetail(this, $"{uvName} — สำเร็จ", summary);
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

    // ── PLC ────────────────────────────────────────────────

    /// <summary>ข้อความเดิมของป้ายกำกับ ก่อนต่อท้ายด้วย address</summary>
    private readonly Dictionary<AntdUI.Label, string> _plcLabelText = new();

    /// <summary>
    /// ต่อท้ายป้ายกำกับด้วย address ที่ค่านั้นจะถูกส่งไป เช่น "Delay (mm.)  →  D2"
    ///
    /// address มาจากตาราง register map ในหน้า PLC Setting ที่เดียว ช่องไหนยังไม่ได้
    /// map จะขึ้นว่า ยังไม่ได้ map เพื่อให้เห็นตั้งแต่เปิดหน้า ไม่ต้องรอกดส่งแล้วค่อยรู้
    /// </summary>
    private async Task ShowPlcAddressesAsync()
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, _pattern);
        if (IsDisposed) return;

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        TagAddress(lblMk1Trigger, plan, $"{mk1} Trigger");
        TagAddress(lblMk1PosAct, plan, $"{mk1} PostAct");
        TagAddress(lblMk1Delay, plan, $"{mk1} Delay");
        TagAddress(lblMk2Trigger, plan, $"{mk2} Trigger");
        TagAddress(lblMk2PosAct, plan, $"{mk2} PostAct");
        TagAddress(lblMk2Delay, plan, $"{mk2} Delay");
        TagAddress(lblConveyor1, plan, "Conveyor Speed 1");
        TagAddress(lblConveyor2, plan, "Conveyor Speed 2");
        TagAddress(lblConveyor3, plan, "Conveyor Speed 3");
    }

    private void TagAddress(AntdUI.Label label, List<PlcOrderService.PlcField> plan, string listName)
    {
        // เก็บข้อความเดิมไว้ครั้งแรก ไม่งั้นเปิดหน้าซ้ำ address จะต่อพอกกันไปเรื่อย ๆ
        if (!_plcLabelText.TryGetValue(label, out var baseText))
        {
            baseText = label.Text ?? "";
            _plcLabelText[label] = baseText;
        }

        var field = plan.FirstOrDefault(f => f.ListName == listName);
        var address = field?.Address;

        label.Text = address == null
            ? $"{baseText}  ·  ยังไม่ได้ map"
            : $"{baseText}  ·  D{address}";
    }

    /// <summary>
    /// ทดสอบส่งค่าเข้า PLC — แยกจากปุ่มส่ง MK เพื่อให้ลองค่าได้โดยไม่แตะเครื่องพิมพ์
    ///
    /// สรุปให้ดูก่อนทุกครั้งว่าจะเขียนอะไรลง register ไหน เพราะเขียนผิดตำแหน่ง
    /// หมายถึงไปทับค่าอื่นใน PLC ซึ่งย้อนกลับเองไม่ได้
    /// </summary>
    private async Task TestPlcAsync()
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, _pattern);
        if (IsDisposed) return;

        if (plan.Count == 0)
        {
            Notify.WarnModal(this, "ทดสอบส่ง PLC", "ยังไม่มีข้อมูลงานให้ส่ง");
            return;
        }

        var ready = plan.Where(f => f.Address != null).ToList();
        var missing = plan.Where(f => f.Address == null).ToList();

        if (ready.Count == 0)
        {
            Notify.WarnModal(this, "ทดสอบส่ง PLC",
                "ไม่มีค่าไหน map address ไว้เลย — ตั้งค่าที่ตาราง register map ในหน้า PLC Setting ก่อน");
            return;
        }

        var summary = string.Join(Environment.NewLine,
            ready.Select(f => $"D{f.Address}   {f.Label}   =  {f.Value}"));

        if (missing.Count > 0)
        {
            summary += Environment.NewLine + Environment.NewLine
                + "ข้ามเพราะยังไม่ได้ map: "
                + string.Join(", ", missing.Select(f => f.ListName));
        }

        var ip = CustomSettingsManager.Read("PLC_IP", "").Trim();
        var port = CustomSettingsManager.Read("PLC_PORT", "502");
        var target = ip.Length == 0 ? "(ยังไม่ได้ตั้ง IP)" : $"{ip}:{port}";

        if (!Confirm.Ask(this, "ยืนยันส่งค่าเข้า PLC",
                $"PLC {target}" + Environment.NewLine + Environment.NewLine + summary
                + Environment.NewLine + Environment.NewLine + "ยืนยันส่งหรือไม่?"))
            return;

        btnTestPlc.Enabled = false;
        var originalText = btnTestPlc.Text;
        btnTestPlc.Text = "กำลังส่ง...";
        try
        {
            var lines = (await PlcOrderService.SendAsync(plan))
                .Select(b => b.Error == null
                    ? Notify.Ok($"{b.Name} — ส่งสำเร็จ")
                    : Notify.Bad($"{b.Name} — {b.Error}"))
                .ToList();

            if (IsDisposed) return;
            Notify.Result(this, "ผลการส่ง PLC", lines);
        }
        finally
        {
            if (!IsDisposed)
            {
                btnTestPlc.Text = originalText;
                btnTestPlc.Enabled = true;
            }
        }
    }

    private async Task SendToSt1Async()
    {
        if (_api == null) return;

        if (!Confirm.Ask(this, "ยืนยันส่ง ST1",
                $"ส่ง Job #{_jobId} ไป Station 1\n\nยืนยันหรือไม่?"))
            return;

        btnSendToSt1.Enabled = false;
        btnSendToSt1.Text = "กำลังส่ง...";
        try
        {
            var (ok, err) = await _api.SendToSt1Async(_jobId);
            if (ok)
            {
                btnSendToSt1.Text = "✓ ส่งแล้ว";
                Notify.Success(this, $"ส่ง Job #{_jobId} ไป ST1 แล้ว");
            }
            else
            {
                btnSendToSt1.Text = "ส่งไป ST1";
                btnSendToSt1.Enabled = true;
                Notify.ErrorModal(this, "ส่งไม่สำเร็จ", err ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            btnSendToSt1.Text = "ส่งไป ST1";
            btnSendToSt1.Enabled = true;
            Notify.ErrorModal(this, "ส่งไม่สำเร็จ", ex.Message);
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
            Notify.ErrorDetail(
                null,
                $"{uvName} — เชื่อมต่อไม่ได้",
                $"ไม่สามารถเชื่อมต่อ {uvName} ({ip}:{port}) ได้\n\n"
                + "กรุณาตรวจสอบ:\n"
                + "• เครื่อง UV เปิดอยู่หรือไม่\n"
                + "• ซอฟต์แวร์ UV เปิดอยู่หรือไม่\n"
                + "• IP และ Port ถูกต้องหรือไม่");
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

        Notify.WarnDetail(null, $"{uvName} — ไม่สำเร็จ", msg);
    }

    private void FillJobInfo(ResolvedJobResponse resolved)
    {
        var job = resolved.Job;

        txtJobOrderNo.Text = OrDash(job.OrderNo);
        txtJobLotNo.Text = OrDash(job.BarcodeRaw);
        txtJobCustomer.Text = OrDash(job.CustomerName);
        txtJobType.Text = OrDash(job.Type);
        txtJobQty.Text = job.Qty?.ToString() ?? Dash;
        // ทั้งคำและสีมาจาก JobStatusDisplay ที่เดียวกับคอลัมน์ Status ในตาราง
        // Order List — backend เก็บเป็น Process / Success แต่บนจอเรียก Working /
        // Finished ทั้งสองหน้า ไม่งั้นงานเดียวกันดูสองหน้าแล้วเหมือนคนละสถานะ
        var jobStatus = Theme.JobStatusDisplay.Resolve(job.Status);
        txtJobStatus.Text = OrDash(jobStatus.Text);
        txtJobStatus.ForeColor = jobStatus.Fore;

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

        ApplyAbc(picMk1Abc, configs.FirstOrDefault(c => c.Ordinal == 1)?.Direction);
        ApplyAbc(picMk2Abc, configs.FirstOrDefault(c => c.Ordinal == 2)?.Direction);
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

    private void FillUvSection(List<UvJobDataDto> uvRows, List<CommandResult> commands)
    {
        var uv1 = uvRows.FirstOrDefault(r => r.Machine == "UV1");
        var uv2 = uvRows.FirstOrDefault(r => r.Machine == "UV2");

        txtUvQtyShared.Text = (uv1?.Qty ?? uv2?.Qty)?.ToString() ?? Dash;

        FillUv(uv1, txtUv1Program, txtUv1ErpMfg, tblUv1Texts, SentProgram(commands, "UV1"));
        FillUv(uv2, txtUv2Program, txtUv2ErpMfg, tblUv2Texts, SentProgram(commands, "UV2"));
    }

    /// <summary>
    /// รุ่นย่อยที่ส่งเข้าเครื่องไปแล้วจริง อ่านจาก payload ของ command
    ///
    /// ค่าใน uv_job_data เป็นชื่อฐานที่ระบบต้นทางสั่งมา (เช่น P-DPX-666)
    /// แต่ที่พิมพ์จริงอาจเป็นรุ่นย่อย (P-DPX-666-1) หน้าจอต้องบอกตัวที่พิมพ์จริง
    /// ไม่งั้นหน้าจอกับเครื่องพูดไม่ตรงกัน — ส่วนค่าที่สั่งมายังอยู่ครบใน payload
    /// </summary>
    private static string? SentProgram(List<CommandResult> commands, string machine)
    {
        var sent = commands
            .LastOrDefault(c => c.Success &&
                string.Equals(c.Command, machine, StringComparison.OrdinalIgnoreCase));

        if (sent?.Payload == null) return null;
        if (!sent.Payload.TryGetValue("program", out var value)) return null;

        var program = value?.ToString()?.Trim();
        return string.IsNullOrEmpty(program) ? null : program;
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

        _origIai = iai;

        if (iai == null) return;

        txtUv1Iai.Text = Number(iai.Iaip);
        txtUv1IaiZ1.Text = Number(iai.IaipZ1);
        txtUv1IaiZ2.Text = Number(iai.IaipZ2);

        txtUv2Iai.Text = Number(iai.Iai);
        txtUv2IaiZ1.Text = Number(iai.IaiZ1);
        txtUv2IaiZ2.Text = Number(iai.IaiZ2);

        txtIaiAdj1Value.Text = iai.Iaip?.ToString() ?? "";
        txtIaiAdj2Value.Text = iai.Iai?.ToString() ?? "";

        txtIaiAdj1Z1Value.Text = iai.IaipZ1?.ToString() ?? "";
        txtIaiAdj1Z2Value.Text = iai.IaipZ2?.ToString() ?? "";
        txtIaiAdj2Z1Value.Text = iai.IaiZ1?.ToString() ?? "";
        txtIaiAdj2Z2Value.Text = iai.IaiZ2?.ToString() ?? "";
    }

    private void ClearIaiFields()
    {
        foreach (var box in new[]
                 {
                     txtUv1Iai, txtUv1IaiZ1, txtUv1IaiZ2,
                     txtUv2Iai, txtUv2IaiZ1, txtUv2IaiZ2,
                 })
            box.Text = Dash;

        txtIaiAdj1Value.Text = "";
        txtIaiAdj2Value.Text = "";
        txtIaiAdj1Z1Value.Text = "";
        txtIaiAdj1Z2Value.Text = "";
        txtIaiAdj2Z1Value.Text = "";
        txtIaiAdj2Z2Value.Text = "";
        _origIai = null;
    }

    // ── IAI Adjust ─────────────────────────────────────────

    private void WireIaiAdjustEvents()
    {
        btnIaiAdj1Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Value, -1);
        btnIaiAdj1Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Value, +1);
        btnIaiAdj2Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Value, -1);
        btnIaiAdj2Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Value, +1);

        btnIaiAdj1Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj1Value, isPlate: true, zone: null);
        btnIaiAdj2Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj2Value, isPlate: false, zone: null);

        btnIaiAdj1Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj1Value, txtUv1Program, txtUv1Iai, isPlate: true, zone: null);
        btnIaiAdj2Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj2Value, txtUv2Program, txtUv2Iai, isPlate: false, zone: null);

        btnIaiAdj1Reset.Click += (_, _) =>
        {
            txtIaiAdj1Value.Text = _origIai?.Iaip?.ToString() ?? "";
        };
        btnIaiAdj2Reset.Click += (_, _) =>
        {
            txtIaiAdj2Value.Text = _origIai?.Iai?.ToString() ?? "";
        };

        // UV1 Z1
        btnIaiAdj1Z1Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z1Value, -1);
        btnIaiAdj1Z1Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z1Value, +1);
        btnIaiAdj1Z1Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj1Z1Value, isPlate: true, zone: "Z1");
        btnIaiAdj1Z1Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj1Z1Value, txtUv1Program, txtUv1IaiZ1, isPlate: true, zone: "Z1");
        btnIaiAdj1Z1Reset.Click += (_, _) => { txtIaiAdj1Z1Value.Text = _origIai?.IaipZ1?.ToString() ?? ""; };

        // UV1 Z2
        btnIaiAdj1Z2Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z2Value, -1);
        btnIaiAdj1Z2Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z2Value, +1);
        btnIaiAdj1Z2Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj1Z2Value, isPlate: true, zone: "Z2");
        btnIaiAdj1Z2Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj1Z2Value, txtUv1Program, txtUv1IaiZ2, isPlate: true, zone: "Z2");
        btnIaiAdj1Z2Reset.Click += (_, _) => { txtIaiAdj1Z2Value.Text = _origIai?.IaipZ2?.ToString() ?? ""; };

        // UV2 Z1
        btnIaiAdj2Z1Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z1Value, -1);
        btnIaiAdj2Z1Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z1Value, +1);
        btnIaiAdj2Z1Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj2Z1Value, isPlate: false, zone: "Z1");
        btnIaiAdj2Z1Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj2Z1Value, txtUv2Program, txtUv2IaiZ1, isPlate: false, zone: "Z1");
        btnIaiAdj2Z1Reset.Click += (_, _) => { txtIaiAdj2Z1Value.Text = _origIai?.IaiZ1?.ToString() ?? ""; };

        // UV2 Z2
        btnIaiAdj2Z2Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z2Value, -1);
        btnIaiAdj2Z2Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z2Value, +1);
        btnIaiAdj2Z2Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj2Z2Value, isPlate: false, zone: "Z2");
        btnIaiAdj2Z2Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj2Z2Value, txtUv2Program, txtUv2IaiZ2, isPlate: false, zone: "Z2");
        btnIaiAdj2Z2Reset.Click += (_, _) => { txtIaiAdj2Z2Value.Text = _origIai?.IaiZ2?.ToString() ?? ""; };
    }

    /// <summary>
    /// ต่อท้ายป้ายของแต่ละแกนด้วย address ที่คำสั่งจะเขียนลงไป เช่น "IAIP · D100"
    ///
    /// address มาจากหน้า Clamp Setting ที่เดียว ตัวเดียวกับที่ ClampService ใช้ยิงจริง
    /// แกนไหนยังไม่ได้ตั้งจะขึ้นว่ายังไม่ได้ตั้ง — กดปุ่ม Send ของแกนนั้นก็จะโดนกันไว้
    /// ให้เห็นตั้งแต่เปิดหน้า ไม่ต้องรอกดแล้วค่อยรู้
    ///
    /// โชว์เฉพาะ address ปลายทาง (TARGET) ที่ค่าจะไปอยู่ ส่วน RUN เป็นบิตสั่งให้แกน
    /// วิ่ง ไม่ใช่ที่เก็บค่า จึงไม่เอามารก
    /// </summary>
    private void ShowClampAddresses()
    {
        var settings = ClampSettings.Load();

        TagClamp(lblIaiAdj1, settings, "IAIP");
        TagClamp(lblIaiAdj1Z1, settings, "IAIPZ1");
        TagClamp(lblIaiAdj1Z2, settings, "IAIPZ2");
        TagClamp(lblIaiAdj2, settings, "IAI");
        TagClamp(lblIaiAdj2Z1, settings, "IAIZ1");
        TagClamp(lblIaiAdj2Z2, settings, "IAIZ2");
    }

    private void TagClamp(AntdUI.Label label, ClampSettings settings, string axisKey)
    {
        if (!_plcLabelText.TryGetValue(label, out var baseText))
        {
            baseText = label.Text ?? "";
            _plcLabelText[label] = baseText;
        }

        var axis = settings.Find(axisKey);
        var target = axis?.AddrTarget.Trim() ?? "";
        var run = axis?.AddrRun.Trim() ?? "";

        // แยกให้ชัดว่า 'ยังไม่ได้ตั้งเลย' กับ 'ตั้ง Target แล้วแต่ยังไม่มี Run'
        // คนละเรื่องกัน — อย่างหลังเห็น address บนจอแล้วแต่ยังสั่งไม่ได้
        // เพราะไม่มีบิตสั่งวิ่ง ถ้าเขียนรวมเป็น 'ยังไม่ได้ตั้ง' จะงงว่าใส่ไปแล้วทำไมไม่ขึ้น
        label.Text = target.Length == 0
            ? $"{baseText}  ·  ยังไม่ได้ตั้ง"
            : run.Length == 0
                ? $"{baseText}  ·  {target} (ยังไม่มี Run)"
                : $"{baseText}  ·  {target}";
    }

    private static void AdjustIaiValue(AntdUI.Input input, int delta)
    {
        if (!int.TryParse(input.Text.Trim(), out int current)) current = 0;
        int next = ClampService.ClampMm(current + delta);
        input.Text = next.ToString();
    }

    /// <summary>
    /// คีย์ของแกนใน ClampSettings — ตรงกับชื่อคอลัมน์ใน MainTable
    /// Plate = IAIP / IAIPZ1 / IAIPZ2 · Shim = IAI / IAIZ1 / IAIZ2
    /// </summary>
    private static string IaiAxisKey(bool isPlate, string? zone) =>
        (isPlate ? "IAIP" : "IAI") + (zone ?? "");

    private async Task IaiSendAsync(AntdUI.Input input, bool isPlate, string? zone)
    {
        if (!int.TryParse(input.Text.Trim(), out int mm))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณากรอกค่า IAI เป็นตัวเลข");
            return;
        }

        mm = ClampService.ClampMm(mm);
        var s = ClampSettings.Load();

        if (string.IsNullOrEmpty(s.Ip))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ยังไม่ได้ตั้งค่า Clamp PLC IP ในหน้า Setting");
            return;
        }

        var axis = s.Find(IaiAxisKey(isPlate, zone));
        if (axis == null) return;

        // ปุ่ม Send ต้องมีทั้ง Target (ที่เก็บค่า) และ Run (บิตสั่งวิ่ง) ขาดอย่างใด
        // อย่างหนึ่งก็สั่งไม่ได้ — บอกให้ตรงว่าขาดอันไหน จะได้ไม่ต้องเดาว่าใส่ตรงไหนแล้ว
        if (!axis.IsConfigured)
        {
            var missing = axis.AddrTarget.Trim().Length == 0
                ? (axis.AddrRun.Trim().Length == 0 ? "Target (D) และ Run (M)" : "Target (D)")
                : "Run (M)";

            Notify.WarnModal(this, "แจ้งเตือน",
                $"แกน {axis.Display} ยังขาด {missing}\nตั้งค่าได้ที่ Setting -> Clamp Setting");
            return;
        }

        // บอกให้ครบว่าค่าจะไปลงที่ address ไหน สั่งผิดแกนคือชิ้นงานเสีย
        if (!Confirm.Ask(this, "ยืนยันสั่งแคลมป์",
                $"สั่ง {axis.Display} ไปที่ {mm} mm\n\n"
                + $"-> {axis.AddrTarget} = {ClampService.ToRaw(mm)}\n"
                + $"-> {axis.AddrRun} (pulse)\n\nยืนยันหรือไม่?"))
            return;

        SetIaiAdjustBusy(true);
        try
        {
            var result = await ClampService.ApplyAsync(s, axis, mm);
            if (IsDisposed) return;

            if (result.Ok)
                Notify.Success(this, $"สั่ง {axis.Display} ไปที่ {mm} mm สำเร็จ");
            else
                Notify.ErrorModal(this, "สั่งแคลมป์ไม่สำเร็จ", result.Log);
        }
        finally
        {
            if (!IsDisposed) SetIaiAdjustBusy(false);
        }
    }

    private async Task IaiUploadAsync(AntdUI.Input input, AntdUI.Input programInput, AntdUI.Input displayInput, bool isPlate, string? zone)
    {
        if (!int.TryParse(input.Text.Trim(), out int mm))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณากรอกค่า IAI เป็นตัวเลข");
            return;
        }

        string program = programInput.Text.Trim();
        if (string.IsNullOrEmpty(program) || program == Dash)
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ไม่มีชื่อโปรแกรม UV");
            return;
        }

        var s = ClampSettings.Load();
        if (string.IsNullOrEmpty(s.DbPath))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ยังไม่ได้ตั้ง path ของ mydatabase.db3 ในหน้า Setting");
            return;
        }

        mm = ClampService.ClampMm(mm);
        var axis = s.Find(IaiAxisKey(isPlate, zone));
        if (axis == null) return;
        string col = axis.Column;

        if (!Confirm.Ask(this, "ยืนยัน Upload",
                $"บันทึก {col} = {mm} mm ให้ \"{program}\"\nไปยัง mydatabase และ Backend\n\nยืนยันหรือไม่?"))
            return;

        SetIaiAdjustBusy(true);
        try
        {
            var errors = new List<string>();

            var (dbOk, dbMsg) = ClampService.Upload(s.DbPath, program, axis, mm);
            if (!dbOk) errors.Add($"mydatabase: {dbMsg}");

            if (_api != null && _jobId > 0)
            {
                var request = new IaiCreateRequest { PrintJobsId = _jobId };
                if (isPlate)
                {
                    request.M1ProgramName = program;
                    if (zone == null) request.Iaip = mm;
                    else if (zone == "Z1") request.IaipZ1 = mm;
                    else if (zone == "Z2") request.IaipZ2 = mm;
                }
                else
                {
                    request.M2ProgramName = program;
                    if (zone == null) request.Iai = mm;
                    else if (zone == "Z1") request.IaiZ1 = mm;
                    else if (zone == "Z2") request.IaiZ2 = mm;
                }

                var (apiOk, apiErr) = await _api.CreateIaiAsync(request);
                if (!apiOk) errors.Add($"Backend: {apiErr}");
            }
            else
            {
                errors.Add("Backend: ไม่มีการเชื่อมต่อ API หรือยังไม่ได้โหลดงาน");
            }

            if (errors.Count == 0)
            {
                Notify.Success(this, $"บันทึก {col} = {mm} mm ให้ \"{program}\" แล้ว");
                displayInput.Text = mm.ToString();
            }
            else if (dbOk || (_api != null && _jobId > 0))
                Notify.WarnDetail(this, "Upload บางส่วนไม่สำเร็จ", string.Join("\n", errors));
            else
                Notify.ErrorModal(this, "Upload ไม่สำเร็จ", string.Join("\n", errors));
        }
        finally
        {
            SetIaiAdjustBusy(false);
        }
    }

    private void SetIaiAdjustBusy(bool busy)
    {
        foreach (var b in new[]
                 {
                     btnIaiAdj1Send, btnIaiAdj1Upload, btnIaiAdj1Reset,
                     btnIaiAdj2Send, btnIaiAdj2Upload, btnIaiAdj2Reset,
                     btnIaiAdj1Z1Send, btnIaiAdj1Z1Upload, btnIaiAdj1Z1Reset,
                     btnIaiAdj1Z2Send, btnIaiAdj1Z2Upload, btnIaiAdj1Z2Reset,
                     btnIaiAdj2Z1Send, btnIaiAdj2Z1Upload, btnIaiAdj2Z1Reset,
                     btnIaiAdj2Z2Send, btnIaiAdj2Z2Upload, btnIaiAdj2Z2Reset,
                 })
        {
            b.Loading = busy;
            b.Enabled = !busy;
        }
    }

    private static void FillUv(
        UvJobDataDto? uv,
        AntdUI.Input program, AntdUI.Input erpMfg,
        AntdUI.Table table, string? sentProgram = null)
    {
        // ส่งไปแล้วให้โชว์รุ่นที่พิมพ์จริง ยังไม่ส่งก็โชว์ชื่อฐานตามข้อมูลงาน
        program.Text = sentProgram ?? OrDash(uv?.ProgramName);
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
