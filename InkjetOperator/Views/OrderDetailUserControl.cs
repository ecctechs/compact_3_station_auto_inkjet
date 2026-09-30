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

    private bool _savingPattern;
    private List<string> _sendSteps = [];
    private int _currentStep;
    private int _jobId;
    private ApiClient? _api;
    private ImageHoverPopup? _refPopup;
    private List<UvJobDataDto> _uvData = [];

    private string? _markingMethod;
    private string? _erpMfg;

    private readonly Dictionary<string, string> _chosenUvProgram = new(StringComparer.OrdinalIgnoreCase);

    private string _jobStatus = "";

    private string _jobLabel = "";

    private bool _connCheckBusy;

    private readonly bool _isDevMode;
    private IaiClampSettingDto? _origIai;

    public event EventHandler? CloseRequested;

    public event EventHandler<string>? RemoteStartRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        ConfigureNumericInputs();

        var rawLevel = CustomSettingsManager.Read("MENU_LEVEL", "1");
        _isDevMode = int.TryParse(rawLevel, out var lvl) && lvl == 99;

        ButtonStyles.Close(btnDetailClose);
        btnDetailClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        btnMkSwap.Click += async (_, _) => await SwapMkDataAsync();
        picMk1Abc.Click += async (_, _) => await ToggleAbcAsync(1, picMk1Abc);
        picMk2Abc.Click += async (_, _) => await ToggleAbcAsync(2, picMk2Abc);
        btnSendMk.Click += async (_, _) => await SendToMkAsync();
        btnSavePattern.Click += async (_, _) => await SaveEditedValuesAsync();
        btnRemoteSend.Click += (_, _) => RequestRemoteStart();
        tmrConnCheck.Tick += ConnCheck_Tick;
        btnSendUv1.Click += async (_, _) => await SendToUvAsync(1);
        btnSendUv2.Click += async (_, _) => await SendToUvAsync(2);
        btnTestPlc.Click += async (_, _) => await TestPlcAsync();

        btnFlowPlate.Click += (_, _) => OpenFlowRefImages(btnFlowPlate);
        btnFlowShim.Click += (_, _) => OpenFlowRefImages(btnFlowShim);

        WireIaiAdjustEvents();
        ShowClampAddresses();
        WireRefImageHover();
        Disposed += (_, _) => _refPopup?.Dispose();
    }

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

        string machine = ReferenceEquals(box, txtUv1Program) ? "UV1" : "UV2";
        var paths = _chosenUvProgram.ContainsKey(machine)
            ? MarkingRefImageService.FindImagesExact(name)
            : MarkingRefImageService.FindImages(name);

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

        tblMk1Blocks.EditMode = AntdUI.TEditMode.Click;
        tblMk2Blocks.EditMode = AntdUI.TEditMode.Click;
        tblUv1Texts.Columns = BuildUvColumns();
        tblUv2Texts.Columns = BuildUvColumns();

        tblUv1Texts.EditMode = AntdUI.TEditMode.Click;
        tblUv2Texts.EditMode = AntdUI.TEditMode.Click;

        NumericInput.DigitsOnlyColumns(tblMk1Blocks, "X", "Y", "Size", "Scale");
        NumericInput.DigitsOnlyColumns(tblMk2Blocks, "X", "Y", "Size", "Scale");
    }

    private void ConfigureNumericInputs()
    {
        NumericInput.DigitsOnly(
            txtMk1Width, txtMk1Height, txtMk1Trigger,
            txtMk2Width, txtMk2Height, txtMk2Trigger,
            txtConveyor1, txtConveyor2, txtConveyor3,
            txtIaiAdj1Value, txtIaiAdj1Z1Value, txtIaiAdj1Z2Value,
            txtIaiAdj2Value, txtIaiAdj2Z1Value, txtIaiAdj2Z2Value);

        NumericInput.DecimalOnly(
            txtMk1PosAct, txtMk1Delay,
            txtMk2PosAct, txtMk2Delay);
    }

    private static AntdUI.ColumnCollection BuildBlockColumns() =>
    [
        new AntdUI.Column("Block", "Block", AntdUI.ColumnAlign.Center) { Width = "16%", Editable = false },
        new AntdUI.Column("BlockText", "Text", AntdUI.ColumnAlign.Left) { Width = "36%" },
        new AntdUI.Column("X", "X", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Y", "Y", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Size", "Size", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Scale", "Scale", AntdUI.ColumnAlign.Center) { Width = "12%" },
    ];

    private static AntdUI.ColumnCollection BuildUvColumns() =>
    [
        new AntdUI.Column("Field", "Field", AntdUI.ColumnAlign.Center) { Width = "30%", Editable = false },
        new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Left) { Width = "70%", Editable = true },
    ];

    public static string JobTitle(PrintJob job)
    {
        var date = ThaiTime.Text(job.CreatedAt, ThaiTime.DateFormat, "");
        var no = job.JobNo?.ToString() ?? job.Id.ToString();

        return date.Length == 0 ? $"Job #{no}" : $"Job #{no}  ·  {date}";
    }

    public void LoadDetail(ResolvedJobResponse resolved, ApiClient? api = null)
    {
        _pattern = resolved.Pattern;
        _barcode = resolved.Job.BarcodeRaw ?? "";
        _isSwapped = false;
        _jobId = resolved.Job.Id;
        _api = api;
        _uvData = resolved.UvJobData;
        _jobStatus = resolved.Job.Status;
        _markingMethod = resolved.PlanRouting?.MarkingMethod;
        _erpMfg = resolved.PlanRouting?.ErpMfg;

        _chosenUvProgram.Clear();
        foreach (var machine in new[] { "UV1", "UV2" })
        {
            var chosen = SentProgram(resolved.Commands, machine) ?? PendingProgram(resolved, machine);
            if (chosen != null) _chosenUvProgram[machine] = chosen;
        }

        lblHeaderTitle.Text = $"Job Information — {JobTitle(resolved.Job)}";

        _ = ShowPlcAddressesAsync();

        SortPatternByOrdinal();
        FillJobInfo(resolved);
        FillMkChipLabels();
        FillUvChipLabels();
        ApplyMarkingMethodButtons();
        RestoreCompletedSteps(resolved.Commands);
        ApplyStepButtons();
        FillMkSection(_pattern);
        FillConveyor(_pattern);
        FillUvSection(resolved);
        ClearIaiFields();
        _ = LoadIaiAsync(resolved.Job.Id);
        _ = CheckConnectionsAsync(showChecking: true);
        tmrConnCheck.Start();
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

    private async void ConnCheck_Tick(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        await CheckConnectionsAsync(showChecking: false);
    }

    private async Task CheckConnectionsAsync(bool showChecking)
    {
        if (_connCheckBusy || IsDisposed) return;

        if (MachineBusy.Active) return;

        _connCheckBusy = true;
        try
        {
            await RunConnectionCheckAsync(showChecking);
        }
        catch
        {
        }
        finally
        {
            _connCheckBusy = false;
        }
    }

    private async Task RunConnectionCheckAsync(bool showChecking)
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

        if (showChecking)
        {
            SetConnLabel(lblConnMk1, mk1Name, mk1Ip, "", "กำลังตรวจสอบ...", Color.Gray);
            SetConnLabel(lblConnMk2, mk2Name, mk2Ip, "", "กำลังตรวจสอบ...", Color.Gray);
            SetConnLabel(lblConnUv1, uv1Name, uv1Ip, uv1Port, "กำลังตรวจสอบ...", Color.Gray);
            SetConnLabel(lblConnUv2, uv2Name, uv2Ip, uv2Port, "กำลังตรวจสอบ...", Color.Gray);
        }

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
            if (lbl.IsDisposed) return;
            lbl.Text = $"●  {name}  ({addr})  {status}";
            lbl.ForeColor = color;
        }

        try
        {
            if (lbl.InvokeRequired) lbl.Invoke(Apply); else Apply();
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }

    private static async Task<bool> TcpCheckAsync(string ip, int port)
    {
        if (string.IsNullOrWhiteSpace(ip) || port <= 0) return false;
        var tcp = new TcpManager();
        var connect = tcp.ConnectAsync(ip, port);

        _ = connect.ContinueWith(static t => _ = t.Exception,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

        try
        {
            await connect.WaitAsync(TimeSpan.FromSeconds(3));
            return tcp.IsConnected();
        }
        catch { return false; }
        finally { tcp.Disconnect(); }
    }

    private static int Flip(int o) => o == 1 ? 2 : o == 2 ? 1 : o;

    private async Task SwapMkDataAsync() // สลับชุดพิมพ์กับ servo ระหว่างหัว MK
    {
        if (_pattern == null || _savingPattern) return; // ยังไม่มี Pattern หรือกำลังบันทึก ให้รอก่อน

        FlipMkOrdinals(); // สลับปลายทางของ Pattern และ servo ในหน่วยความจำก่อน

        var error = await SavePatternAsync(); // บันทึก Pattern ของ Job ลง Backend
        if (error == null) return; // บันทึกผ่านแล้ว จบโดยไม่ย้อนค่าบนจอ

        FlipMkOrdinals(); // บันทึกไม่ผ่าน จึงสลับค่าบนจอกลับตามฐานเดิม
        Notify.ErrorModal(this, "สลับเครื่องไม่สำเร็จ", // แจ้งว่าสลับหัวไม่สำเร็จ
            $"ยังไม่ได้บันทึกลงฐานข้อมูล จอจึงถูกปรับกลับเป็นค่าเดิม\n\n{error}"); // บอกว่าคืนค่าบนจอตามฐานเดิมแล้ว
    }

    private void FlipMkOrdinals() // สลับลำดับหัวใน Pattern และค่าตำแหน่ง
    {
        if (_pattern == null) return; // ยังไม่มี Pattern จึงทำรายการต่อไม่ได้

        foreach (var cfg in _pattern.InkjetConfigs) // ไล่ค่าพิมพ์ทุกหัวใน Pattern
            cfg.Ordinal = Flip(cfg.Ordinal); // สลับหัว MK ที่จะรับชุดพิมพ์นี้

        foreach (var servo in _pattern.ServoConfigs) // ไล่ค่า servo ที่ผูกกับแต่ละหัว
            servo.Ordinal = Flip(servo.Ordinal); // ให้ค่าตำแหน่งและหน่วงตามไปกับหัว MK

        SortPatternByOrdinal(); // เรียงใหม่ตามหัวจริงก่อนเติมค่าบนจอ

        _isSwapped = !_isSwapped; // สลับสถานะว่าจออยู่ในโหมดสลับหัว
        lblMkSectionTitle.Text = _isSwapped // เลือกหัวข้อให้ตรงสถานะสลับหัว
            ? "MK Section (MK Inkjet) — SWAPPED" // เพิ่มคำว่า SWAPPED ให้เห็นชัด
            : "MK Section (MK Inkjet)"; // ใช้หัวข้อปกติเมื่อกลับค่าหัวเดิม
        lblMkSectionTitle.ForeColor = _isSwapped // เลือกสีหัวข้อตามสถานะสลับ
            ? DesignTokens.Warning // ใช้สีเตือนเมื่อสลับหัวอยู่
            : DesignTokens.DarkNavy; // ใช้สีปกติเมื่อไม่ได้สลับหัว

        FillMkSection(_pattern); // เติมค่าหัว MK บนจอจาก Pattern ล่าสุด
    }

    private async Task SaveEditedValuesAsync() // บันทึก Pattern แล้วบันทึกข้อความ UV
    {
        if (_savingPattern) return; // กำลังบันทึกรอบก่อน ไม่รับคำขอซ้ำ

        if (CollectEditedValues() is string problem) // ตรวจค่าที่กรอกครบก่อนย้ายกลับเข้า Pattern
        {
            Notify.WarnModal(this, "ค่าที่กรอกไม่ถูกต้อง", problem); // แจ้งช่องที่กรอกแล้วตรวจไม่ผ่าน
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        btnSavePattern.Enabled = false; // ปิดปุ่ม Save ระหว่างบันทึก
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var error = await SavePatternAsync(); // บันทึกค่าที่แก้ลง Job เพราะหน้า List จะอ่านจาก Backend ใหม่
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            if (error != null) // มีรายละเอียดข้อผิดพลาดส่งกลับมา
            {
                Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", // แจ้งว่า Pattern ยังบันทึกไม่ผ่าน
                    "ค่าที่แก้ยังไม่ได้ลงฐานข้อมูล" + Environment.NewLine + Environment.NewLine + error); // แนบเหตุที่ค่าบนจอยังไม่ลงฐาน
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            var uvError = await SaveUvTextsAsync(); // บันทึกข้อความ UV หลัง Pattern ผ่านแล้ว
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            FillMkSection(_pattern!); // เติมค่าหัว MK บนจอจาก Pattern ล่าสุด
            FillConveyor(_pattern!); // เติมค่าสายพานจากข้อมูลที่บันทึก

            if (uvError != null) // ฝั่ง MK ลงแล้ว แต่ UV มีปัญหา ต้องบอกแยกส่วน
            {
                Notify.ErrorModal(this, "บันทึกข้อความ UV ไม่สำเร็จ", // แจ้งปัญหาเฉพาะส่วนข้อความ UV
                    "ค่าฝั่ง MK บันทึกแล้ว แต่ข้อความ UV ยังไม่ได้ลงฐานข้อมูล" // บอกให้แยกผล MK ที่บันทึกแล้วออกจาก UV
                    + Environment.NewLine + Environment.NewLine + uvError); // แนบเหตุที่บันทึกข้อความ UV ไม่ได้
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            Notify.Success(this, "บันทึกค่าเรียบร้อย"); // แจ้งว่าบันทึกครบตามขั้นแล้ว
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            if (!IsDisposed) btnSavePattern.Enabled = true; // เปิดปุ่ม Save กลับเมื่อหน้ายังอยู่
        }
    }

    private async Task<string?> SaveUvTextsAsync() // บันทึกเฉพาะข้อความ UV ที่แก้
    {
        if (_api == null) return null; // ไม่มีตัวเรียก Backend ให้ข้ามการบันทึก UV

        var problems = new List<string>(); // เตรียมรวมปัญหาของ UV ทั้งสองชุด

        foreach (var (table, machine) in new[] // ตรวจตารางข้อความแยกตามเครื่อง
                 {
                     (tblUv1Texts, "UV1"), // จับตารางแรกกับข้อมูล UV1
                     (tblUv2Texts, "UV2"), // จับตารางที่สองกับข้อมูล UV2
                 })
        {
            var row = _uvData.FirstOrDefault(r => // หาข้อมูล UV ให้ตรงตารางที่กำลังแก้
                string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase)); // เลือกแถวของเครื่องเดียวกันโดยไม่สนตัวพิมพ์

            if (row == null) continue; // ไม่พบแถวของเครื่องนี้ ให้ข้าม
            if (table.DataSource is not List<UvTextRow> edited) continue; // ไม่มีตารางค่าที่แก้แล้ว ให้ข้าม
            if (table.Tag is not List<string> original) continue; // ไม่มีค่าต้นฉบับให้เทียบ ให้ข้าม

            var changed = new Dictionary<string, string?>(); // เก็บเฉพาะช่องที่ต่างจากตอนเปิดหน้า
            for (int i = 0; i < edited.Count && i < original.Count; i++) // เทียบเฉพาะช่องที่มีทั้งค่าเก่าและค่าใหม่
            {
                var now = (edited[i].Value ?? "").Trim(); // อ่านค่าข้อความปัจจุบันแล้วตัดช่องว่าง
                if (now == Dash) now = ""; // ขีดหมายถึงตั้งใจล้างข้อความช่องนี้

                var before = (original[i] ?? "").Trim(); // อ่านค่าที่แสดงไว้ตอนเปิดหน้า
                if (before == Dash) before = ""; // ขีดในค่าเดิมก็นับเป็นข้อความว่าง

                if (now == before) continue; // ช่องที่ไม่ได้แก้ไม่ต้องส่งทับ
                changed[$"text{i + 1}"] = now.Length == 0 ? null : now; // ผูกค่าที่แก้กับ text1 ถึง text5 รวมการล้างช่อง
            }

            if (changed.Count == 0) continue; // เครื่องนี้ไม่มีช่องเปลี่ยน ไม่ต้องบันทึก

            var (ok, error) = await _api.UpdateUvTextsAsync(row.Id, changed); // อัปเดตเฉพาะข้อมูล UV ของแถวนี้
            if (IsDisposed) return null; // ปิดหน้าแล้วไม่แก้ข้อมูลบนจอต่อ

            if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
            {
                problems.Add($"{machine}: {error}"); // เก็บเหตุบันทึกไม่ได้แยกชื่อ UV
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            foreach (var (field, value) in changed) // นำค่าที่บันทึกแล้วกลับเข้าข้อมูลในหน้านี้
            {
                switch (field) // เลือกช่องข้อความที่จะเปลี่ยน
                {
                    case "text1": row.Text1 = value; break; // อัปเดตข้อความช่อง 1 ในหน่วยความจำ
                    case "text2": row.Text2 = value; break; // อัปเดตข้อความช่อง 2 ในหน่วยความจำ
                    case "text3": row.Text3 = value; break; // อัปเดตข้อความช่อง 3 ในหน่วยความจำ
                    case "text4": row.Text4 = value; break; // อัปเดตข้อความช่อง 4 ในหน่วยความจำ
                    case "text5": row.Text5 = value; break; // อัปเดตข้อความช่อง 5 ในหน่วยความจำ
                }
            }

            table.Tag = edited.Select(r => r.Value).ToList(); // ใช้ค่าที่บันทึกแล้วเป็นฐานเทียบการแก้รอบหน้า
        }

        return problems.Count == 0 ? null : string.Join(Environment.NewLine, problems); // รวมเหตุผิดพลาด ถ้าไม่มีให้ถือว่าบันทึกครบ
    }

    private string? CollectEditedValues() // ตรวจและเก็บค่าบนจอกลับเข้า Pattern
    {
        if (_pattern == null) return "ยังไม่มีข้อมูล pattern ของงานนี้"; // ต้องโหลด Pattern ก่อนอ่านหรือบันทึกค่าที่แก้

        var errors = new List<string>(); // เตรียมเก็บปัญหาที่พบในรอบนี้

        int? Int(AntdUI.Input box, string label) // ตัวช่วยอ่านช่องที่ต้องเป็นจำนวนเต็ม
        {
            var text = box.Text.Trim(); // อ่านข้อความในช่องแล้วตัดช่องว่าง
            if (text.Length == 0 || text == Dash) return null; // ช่องว่างหรือขีดยังไม่มีค่าตัวเลข
            if (int.TryParse(text, out int v)) return v; // ใช้ค่าที่แปลงเป็นจำนวนเต็มได้
            errors.Add($"{label}: \"{text}\" ไม่ใช่จำนวนเต็ม"); // เก็บชื่อช่องที่กรอกไม่เป็นจำนวนเต็ม
            return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ
        }

        double? Dbl(AntdUI.Input box, string label) // ตัวช่วยอ่านช่องที่รับทศนิยม
        {
            var text = box.Text.Trim(); // อ่านข้อความในช่องแล้วตัดช่องว่าง
            if (text.Length == 0 || text == Dash) return null; // ช่องว่างหรือขีดยังไม่มีค่าตัวเลข
            if (double.TryParse(text, out double v)) return v; // ใช้ค่าที่แปลงเป็นทศนิยมได้
            errors.Add($"{label}: \"{text}\" ไม่ใช่ตัวเลข"); // เก็บชื่อช่องที่กรอกไม่เป็นตัวเลข
            return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ
        }

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058"); // อ่านชื่อหัว MK ตัวแรกจาก Setting
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059"); // อ่านชื่อหัว MK ตัวที่สองจาก Setting

        var v1 = (W: Int(txtMk1Width, $"{mk1} Width"), H: Int(txtMk1Height, $"{mk1} Height"), // อ่านความกว้างและความสูงของ MK หัวแรก
                  Trig: Int(txtMk1Trigger, $"{mk1} Trigger Delay"), // อ่าน Trigger Delay ของหัวแรก
                  Act: Dbl(txtMk1PosAct, $"{mk1} Pos Act"), Dly: Dbl(txtMk1Delay, $"{mk1} Delay")); // อ่านตำแหน่งและหน่วง servo หัวแรก

        var v2 = (W: Int(txtMk2Width, $"{mk2} Width"), H: Int(txtMk2Height, $"{mk2} Height"), // อ่านความกว้างและความสูงของ MK หัวที่สอง
                  Trig: Int(txtMk2Trigger, $"{mk2} Trigger Delay"), // อ่าน Trigger Delay ของหัวที่สอง
                  Act: Dbl(txtMk2PosAct, $"{mk2} Pos Act"), Dly: Dbl(txtMk2Delay, $"{mk2} Delay")); // อ่านตำแหน่งและหน่วง servo หัวที่สอง

        var s1 = Int(txtConveyor1, "Conveyor 1"); // อ่านความเร็วสายพานตัวที่ 1
        var s2 = Int(txtConveyor2, "Conveyor 2"); // อ่านความเร็วสายพานตัวที่ 2
        var s3 = Int(txtConveyor3, "Conveyor 3"); // อ่านความเร็วสายพานตัวที่ 3

        var blocks1 = ReadBlocks(tblMk1Blocks, mk1, errors); // อ่านข้อความและค่าบล็อกของ MK หัวแรก
        var blocks2 = ReadBlocks(tblMk2Blocks, mk2, errors); // อ่านหัวที่สองแยกกัน ไม่สลับกับหัวแรก

        if (errors.Count > 0) return string.Join(Environment.NewLine, errors); // มีช่องผิดให้หยุดก่อนแก้ Pattern เพื่อไม่ค้างค่าครึ่งชุด

        void ApplyMk(int ordinal, // ตัวช่วยใส่ค่ากลับให้หัวตามลำดับ
            (int? W, int? H, int? Trig, double? Act, double? Dly) v) // รับชุดค่าพิมพ์และค่า servo ของหัวนั้น
        {
            var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal); // หา InkjetConfig ของหัวที่กำหนด
            if (config != null) // มีชุดค่าพิมพ์ของหัวนี้แล้ว
            {
                config.Width = v.W; // เก็บความกว้างที่แก้บนจอ
                config.Height = v.H; // เก็บความสูงที่แก้บนจอ
                config.TriggerDelay = v.Trig; // เก็บ Trigger Delay ที่แก้บนจอ
            }

            var servo = _pattern.ServoConfigs.FirstOrDefault(s => s.Ordinal == ordinal); // หาค่า servo ให้ตรงหัวเดียวกัน
            if (servo != null) // มีค่า servo ของหัวนี้แล้ว
            {
                servo.PostAct = v.Act; // เก็บตำแหน่งหัวที่แก้
                servo.Delay = v.Dly; // เก็บค่าหน่วงของ servo ที่แก้
            }
        }

        ApplyMk(1, v1); // นำค่าชุดแรกใส่ MK หัวแรก
        ApplyMk(2, v2); // นำค่าชุดที่สองใส่ MK หัวที่สอง
        ApplyBlocks(1, blocks1); // เก็บบล็อกข้อความของหัวแรก
        ApplyBlocks(2, blocks2); // เก็บบล็อกข้อความของหัวที่สอง

        if (_pattern.ConveyorSpeeds is { } speeds) // ตรวจว่ามีชุดความเร็วสายพานใน Pattern
        {
            speeds.Speed1 = s1; // เก็บความเร็วสายพานตัวที่ 1
            speeds.Speed2 = s2; // เก็บความเร็วสายพานตัวที่ 2
            speeds.Speed3 = s3; // เก็บความเร็วสายพานตัวที่ 3
        }

        return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ
    }

    private readonly record struct BlockEdit(
        int Number, string? Text, int? X, int? Y, int? Size, int? Scale);

    private List<BlockEdit> ReadBlocks(AntdUI.Table table, string machine, List<string> errors) // อ่านค่าบล็อกในตารางพร้อมตรวจตัวเลข
    {
        var result = new List<BlockEdit>(); // เตรียมรายการบล็อกที่แก้แล้ว
        if (table.DataSource is not List<BlockRow> rows) return result; // ไม่มีข้อมูลตาราง ให้จบด้วยรายการว่าง

        int? Int(string? text, string label) // ตัวช่วยแปลงค่าบล็อกเป็นจำนวนเต็ม
        {
            var value = (text ?? "").Trim(); // แทนค่าว่างแล้วตัดช่องว่างหัวท้าย
            if (value.Length == 0 || value == Dash) return null; // ช่องว่างหรือขีดหมายถึงยังไม่มีค่า
            if (int.TryParse(value, out int v)) return v; // ใช้จำนวนเต็มที่แปลงได้
            errors.Add($"{label}: \"{value}\" ไม่ใช่จำนวนเต็ม"); // เก็บช่องบล็อกที่กรอกตัวเลขผิด
            return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ
        }

        foreach (var row in rows) // อ่านบล็อกทีละแถว
        {
            if (!int.TryParse(row.Block, out int number)) continue; // เลขบล็อกไม่ถูกต้อง ให้ข้ามแถวนี้

            var where = $"{machine} Block {number}"; // ระบุหัวและเลขบล็อกสำหรับรายงานปัญหา
            var text = (row.BlockText ?? "").Trim(); // อ่านข้อความพิมพ์ในแถวนี้

            result.Add(new BlockEdit( // รวมค่าที่อ่านได้เป็นบล็อกหนึ่งชุด
                number, // เก็บหมายเลขบล็อก
                text.Length == 0 || text == Dash ? null : text, // ช่องว่างหรือขีดใช้แทนการล้างข้อความ
                Int(row.X, $"{where} X"), // อ่านตำแหน่ง X ของข้อความ
                Int(row.Y, $"{where} Y"), // อ่านตำแหน่ง Y ของข้อความ
                Int(row.Size, $"{where} Size"), // อ่านขนาดตัวอักษร
                Int(row.Scale, $"{where} Scale"))); // อ่านอัตราขยายของข้อความ
        }

        return result; // ส่งบล็อกที่ตรวจแล้วกลับไปเก็บใน Pattern
    }

    private void ApplyBlocks(int ordinal, List<BlockEdit> edits)
    {
        var config = _pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
        if (config == null) return;

        foreach (var edit in edits)
        {
            var block = config.TextBlocks.FirstOrDefault(b => b.BlockNumber == edit.Number);
            if (block == null) continue;

            block.X = edit.X;
            block.Y = edit.Y;
            block.Size = edit.Size;
            block.Scale = edit.Scale;

            var shown = PatternEngine.Process(_barcode, block.Text ?? "");
            if (edit.Text != shown) block.Text = edit.Text;
        }
    }

    private async Task<string?> SavePatternAsync() // ส่ง Pattern ปัจจุบันไปอัปเดต Backend
    {
        if (_pattern == null) return "ยังไม่มีข้อมูล pattern ของงานนี้"; // ต้องโหลด Pattern ก่อนอ่านหรือบันทึกค่าที่แก้
        if (_api == null) return "ยังไม่ได้เชื่อมต่อ backend"; // ต้องมี Backend ก่อนบันทึก Pattern

        _savingPattern = true; // จำว่ากำลังบันทึก เพื่อกันแก้ซ้อน
        btnMkSwap.Enabled = false; // ปิดปุ่มสลับหัวระหว่างบันทึก
        picMk1Abc.Enabled = false; // ปิดปุ่มกลับทิศหัวแรกระหว่างบันทึก
        picMk2Abc.Enabled = false; // ปิดปุ่มกลับทิศหัวที่สองระหว่างบันทึก
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var (ok, error) = await _api.UpdatePatternAsync(_pattern.Id, _pattern); // อัปเดต Pattern ใบเดิมด้วยค่าปัจจุบัน
            return ok ? null : error ?? "บันทึกไม่สำเร็จ"; // ส่งเหตุที่บันทึกไม่ผ่านให้ผู้เรียก
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            _savingPattern = false; // จบรอบบันทึกแล้วรับการแก้ไขใหม่ได้
            btnMkSwap.Enabled = true; // เปิดปุ่มสลับหัวกลับ
            picMk1Abc.Enabled = true; // เปิดปุ่มกลับทิศหัวแรกกลับ
            picMk2Abc.Enabled = true; // เปิดปุ่มกลับทิศหัวที่สองกลับ
        }
    }

    private void SortPatternByOrdinal()
    {
        if (_pattern == null) return;
        _pattern.InkjetConfigs = _pattern.InkjetConfigs
            .OrderBy(c => c.Ordinal).ToList();
        _pattern.ServoConfigs = _pattern.ServoConfigs
            .OrderBy(s => s.Ordinal).ToList();
    }

    private async Task ToggleAbcAsync(int ordinal, PictureBox box) // เปลี่ยนทิศทางข้อความแล้วบันทึก Pattern
    {
        if (_pattern == null || _savingPattern) return; // ยังไม่มี Pattern หรือกำลังบันทึก ให้รอก่อน

        var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal); // หาค่าพิมพ์ของหัวที่กด ABC
        if (config == null) // ยังไม่มีค่าพิมพ์ของหัวนั้น
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบ InkjetConfig ordinal {ordinal}"); // บอกลำดับหัวที่หาไม่พบ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        FlipDirection(config, box); // กลับทิศข้อความและรูปบนจอก่อนบันทึก

        var error = await SavePatternAsync(); // บันทึก Pattern ของ Job ลง Backend
        if (error == null) return; // บันทึกผ่านแล้ว จบโดยไม่ย้อนค่าบนจอ

        FlipDirection(config, box); // บันทึกไม่ได้ ให้กลับทิศคืนค่าเดิม
        Notify.ErrorModal(this, "สลับทิศทางพิมพ์ไม่สำเร็จ", // แจ้งว่ากลับทิศพิมพ์ไม่สำเร็จ
            $"ยังไม่ได้บันทึกลงฐานข้อมูล จอจึงถูกปรับกลับเป็นค่าเดิม\n\n{error}"); // บอกว่าคืนค่าบนจอตามฐานเดิมแล้ว
    }

    private static void FlipDirection(InkjetConfigDto config, PictureBox box)
    {
        config.Direction = MkCompactAdapter.IsFlipped(config.Direction)
            ? MkCompactAdapter.DirectionNormal
            : MkCompactAdapter.DirectionFlipped;

        ApplyAbc(box, config.Direction);
    }

    private static void ApplyAbc(PictureBox box, int? direction)
    {
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

    private void ApplyMarkingMethodButtons()
    {
        var plan = MarkingMethodService.Resolve(_markingMethod);

        _sendSteps = plan.NoCase ? [] : new List<string>(plan.Steps);
        _currentStep = 0;

        RefreshFlowRows();
        ApplyStepButtons();
    }

    private void RefreshFlowRows()
    {
        var plan = MarkingMethodService.Resolve(_markingMethod);

        if (plan.NoCase)
        {
            ApplyFlowRow(btnFlowPlate, "Plate", MarkingMachine.None, null, "   ไม่มีเคสนี้");
            ApplyFlowRow(btnFlowShim, "Shim", MarkingMachine.None, null, "   ไม่มีเคสนี้");
            return;
        }

        var sides = MarkingRefResolver.Resolve(
            _markingMethod, _erpMfg, UvProgramOf("UV1"), UvProgramOf("UV2"));

        ApplyFlowRow(btnFlowPlate, "Plate", plan.Plate,
            sides.FirstOrDefault(s => s.Side == "Plate"), "");
        ApplyFlowRow(btnFlowShim, "Shim", plan.Shim,
            sides.FirstOrDefault(s => s.Side == "Shim"), "");
    }

    private UvProgramInfo UvProgramOf(string machine)
    {
        if (_chosenUvProgram.TryGetValue(machine, out var chosen))
            return new UvProgramInfo(chosen, Confirmed: true);

        var baseName = _uvData.FirstOrDefault(r => r.Machine == machine)?.ProgramName;
        return new UvProgramInfo(baseName, Confirmed: false);
    }

    private sealed record FlowRef(string Name, List<string> Images);

    private static void ApplyFlowRow(
        AntdUI.Button row, string side, MarkingMachine machine,
        MarkingRefSide? reference, string suffix)
    {
        var refName = reference?.LookupName;
        bool hasRef = !string.IsNullOrEmpty(refName) && refName != Dash;

        row.Text = $"{side} - {MachineLabel(machine)}"
            + (hasRef ? $" - ({refName})" : "")
            + suffix;

        row.Tag = hasRef ? new FlowRef(refName!, reference!.Images) : null;

        row.IconSvg = hasRef ? "PictureOutlined" : null;
        row.BorderWidth = hasRef ? 1F : 0F;
        row.DefaultBack = hasRef ? System.Drawing.Color.FromArgb(237, 243, 249) : Color.Transparent;
        row.Cursor = hasRef ? Cursors.Hand : Cursors.Default;
    }

    private void OpenFlowRefImages(AntdUI.Button row)
    {
        if (row.Tag is not FlowRef reference) return;

        if (reference.Images.Count == 0)
        {
            Notify.WarnModal(this, "รูปอ้างอิง",
                MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder()));
            return;
        }

        MarkingRefPickerDialog.View(
            this, $"รูปอ้างอิง — {reference.Name}", reference.Name, reference.Images);
    }

    private static string MachineLabel(MarkingMachine machine) =>
        MarkingMethodService.Label(machine);

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

    private bool HasStep(string step) =>
        _sendSteps.Any(s => string.Equals(s, step, StringComparison.OrdinalIgnoreCase));

    private string? NextRemoteStep() // หาขั้นถัดไปที่ฝากให้ ST1 ส่งได้
    {
        if (_currentStep <= 0 || _currentStep >= _sendSteps.Count) return null; // ต้องผ่านขั้นแรกแล้ว และยังมีขั้นถัดไปค้างอยู่
        if (!string.Equals(_jobStatus, "Process", StringComparison.OrdinalIgnoreCase)) return null; // ฝากส่งขั้นถัดไปได้เฉพาะงานที่กำลังผลิต

        return _sendSteps[_currentStep]; // ส่งชื่อเครื่องของขั้นปัจจุบันให้ปุ่มฝากส่ง
    }

    private void RequestRemoteStart() // ฝากขั้นถัดไปกลับไปที่หน้า Order List
    {
        if (NextRemoteStep() is not string step) return; // ไม่มีขั้นถัดไปที่ฝากส่งได้ จึงไม่ส่ง event

        RemoteStartRequested?.Invoke(this, step); // ฝากชื่อขั้นให้ OrderDetailDialog รับไว้
        CloseRequested?.Invoke(this, EventArgs.Empty); // ปิด Detail เพื่อให้ Order List เปิดกล่องยืนยันและรอผล
    }

    private void ApplyStepButtons() // แสดงปุ่มส่งตามขั้นและสิทธิ์ของ Station
    {
        var remoteStep = NextRemoteStep(); // ตรวจว่างานมีขั้นถัดไปให้ ST1 ส่งหรือไม่

        bool showRemote = remoteStep != null // มีขั้นถัดไปจึงพิจารณาแสดงปุ่มสำรอง
            && StationService.ManualRemoteSendEnabled // ต้องเปิดตัวเลือกฝากส่งด้วยมือไว้ก่อน
            && (StationService.IsSt3 || _isDevMode); // ให้เห็นเฉพาะ ST3 หรือโหมดทดสอบ

        btnRemoteSend.Visible = showRemote; // แสดงปุ่มฝากส่งตามสิทธิ์ที่ตรวจไว้
        btnRemoteSend.Enabled = showRemote; // เปิดให้กดด้วยเงื่อนไขเดียวกัน ไม่อ่าน Visible กลับมา
        if (remoteStep != null) btnRemoteSend.Text = $"ขอให้ ST1 ส่ง {remoteStep}"; // ใส่ชื่อเครื่องที่ขอให้ ST1 ส่งบนปุ่ม

        btnTestPlc.Visible = _isDevMode; // แสดงปุ่ม Test PLC เฉพาะโหมดทดสอบ
        btnSendMk.Visible = _isDevMode; // แสดงปุ่มส่ง MK ตรงเฉพาะโหมดทดสอบ
        btnSendUv1.Visible = _isDevMode; // แสดงปุ่มส่ง UV1 ตรงเฉพาะโหมดทดสอบ
        btnSendUv2.Visible = _isDevMode; // แสดงปุ่มส่ง UV2 ตรงเฉพาะโหมดทดสอบ

        if (_isDevMode) // โหมดทดสอบเปิดให้ส่งแต่ละเครื่องเอง
        {
            btnTestPlc.Enabled = true; // เปิดให้ทดสอบ PLC ได้
            btnSendMk.Enabled = true; // เปิดให้ทดสอบส่ง MK ได้
            btnSendUv1.Enabled = true; // เปิดให้ทดสอบส่ง UV1 ได้
            btnSendUv2.Enabled = true; // เปิดให้ทดสอบส่ง UV2 ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        btnTestPlc.Enabled = false; // ปิดการส่ง PLC ตรงในโหมดหน้างาน
        btnSendMk.Enabled = false; // ปิดปุ่มส่ง MK ไว้ก่อนตรวจขั้น
        btnSendUv1.Enabled = false; // ปิดปุ่มส่ง UV1 ไว้ก่อนตรวจขั้น
        btnSendUv2.Enabled = false; // ปิดปุ่มส่ง UV2 ไว้ก่อนตรวจขั้น

        if (_currentStep < _sendSteps.Count) // ยังมีขั้นที่ส่งไม่ครบในแผน
        {
            var step = _sendSteps[_currentStep]; // อ่านชื่อเครื่องของขั้นที่ค้าง
            GetSendButton(step).Enabled = true; // เปิดปุ่มของขั้นที่ถึงลำดับ
        }

        for (int i = 0; i < _currentStep && i < _sendSteps.Count; i++) // ไล่ขั้นที่มีประวัติส่งแล้ว
            MarkButtonSent(GetSendButton(_sendSteps[i])); // แสดงเครื่องหมายส่งแล้วบนปุ่มนั้น
    }

    private void CompleteSendStep(string stepName, object? detail = null)
    {
        if (_isDevMode) return;

        _ = _api?.SaveSendStepAsync(_jobId, stepName, detail);

        if (_currentStep >= _sendSteps.Count) return;
        if (_sendSteps[_currentStep] != stepName) return;

        bool isFirstStep = _currentStep == 0;

        var btn = GetSendButton(stepName);
        btn.Enabled = false;
        MarkButtonSent(btn);

        _currentStep++;
        if (_currentStep < _sendSteps.Count)
            GetSendButton(_sendSteps[_currentStep]).Enabled = true;

        if (isFirstStep)
        {
            _ = _api?.UpdateJobStatusAsync(_jobId, "Process");

            _jobStatus = "Process";
        }
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
        if (_pattern == null)
        {
            Notify.WarnModal(this, "ส่งหา MK", "ยังไม่มีข้อมูล pattern ของงานที่เลือก");
            return;
        }

        using var lease = MachineBusy.TryHoldExclusive("MK");
        if (lease == null) { Notify.Warn(this, "MK กำลังส่งงานอยู่ กรุณารอให้เสร็จก่อน"); return; }
        btnSendMk.Enabled = false;
        var originalText = btnSendMk.Text;
        btnSendMk.Text = "กำลังส่ง...";

        try
        {
            var mk = await JobSendService.SendMkAsync(_pattern);

            var lines = Notify.MkLines(mk.Machines);

            if (lines.Count == 0)
                lines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"));

            if (mk.Status == SendStatus.Ok) CompleteSendStep("MK");

            Notify.Result(this, "ผลการส่ง MK", lines);
        }
        catch (Exception ex)
        {
            Notify.ErrorModal(this, "Error", $"เกิดข้อผิดพลาด: {ex.Message}");
        }
        finally
        {
            if (!IsDisposed && btnSendMk.Text?.StartsWith('✓') != true)
            {
                btnSendMk.Text = originalText;
                btnSendMk.Enabled = true;
            }
        }
    }

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
            using var busy = MachineBusy.TryHoldExclusive($"UV{uvNumber}");
            if (busy == null) { Notify.Warn(this, $"UV{uvNumber} กำลังส่งงานอยู่ กรุณารอให้เสร็จก่อน"); return; }

            var uvTcp = new UvTcpService();

            var (stopOk, _) = await uvTcp.StopAsync(ip, port);
            done.Add(stopOk ? "สั่งหยุดเครื่อง" : "สั่งหยุดเครื่อง (ไม่ตอบรับ — ทำต่อ)");

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

            var (tcpOk, tcpLog, startWarning) = await uvTcp.LoadAndStartAsync(ip, port, programFile);
            if (!tcpOk)
            {
                ShowUvFailure(uvName, done, tcpLog.Trim());
                return;
            }
            done.Add($"โหลดโปรแกรม {programFile}.uvdx");
            if (startWarning == null) done.Add("เครื่องตอบรับคำสั่งเริ่มพิมพ์");

            CompleteSendStep(stepName, new
            {
                requested = uvRow.ProgramName ?? "",
                program = programFile,
                is_default = pick.IsDefault,
                start_confirmed = startWarning == null,
                start_warning = startWarning,
            });

            (uvNumber == 1 ? txtUv1Program : txtUv2Program).Text = programFile;

            _chosenUvProgram[stepName] = programFile;
            RefreshFlowRows();

            var summary = $"ส่งข้อมูล {uvName} สำเร็จ\n\n"
                + string.Join("\n", done.Select(s => "• " + s))
                + (startWarning == null ? "" : "\n\n⚠ " + startWarning + "\nตรวจสถานะเริ่มพิมพ์ที่เครื่อง")
                + (pick.IsDefault ? "\n\n⚠ ใช้ default.uvdx เพราะไม่พบโปรแกรมที่ต้องการ" : "");

            if (startWarning == null) Notify.SuccessDetail(this, $"{uvName} — ส่งข้อมูลแล้ว", summary);
            else Notify.WarnDetail(this, $"{uvName} — ส่งข้อมูลแล้ว / ตรวจ Start", summary);
        }
        catch (Exception ex)
        {
            ShowUvFailure(uvName, done, ex.Message);
        }
        finally
        {
            if (btn.Text?.StartsWith('✓') != true)
            {
                btn.Text = originalText;
                btn.Enabled = true;
            }
        }
    }

    private readonly Dictionary<AntdUI.Label, string> _plcLabelText = new();

    private async Task ShowPlcAddressesAsync()
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, _pattern);
        if (IsDisposed) return;

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        TagAddress(lblMk1PosAct, plan, $"{mk1} PostAct");
        TagAddress(lblMk1Delay, plan, $"{mk1} Delay");
        TagAddress(lblMk2PosAct, plan, $"{mk2} PostAct");
        TagAddress(lblMk2Delay, plan, $"{mk2} Delay");
        TagAddress(lblConveyor1, plan, "Conveyor Speed 1");
    }

    private void TagAddress(AntdUI.Label label, List<PlcOrderService.PlcField> plan, string listName)
    {
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

    private async Task TestPlcAsync() // ทดสอบ PLC ด้วยค่าที่เพิ่งแก้บนจอ
    {
        if (CollectEditedValues() is string invalid) // ใช้ค่าที่เพิ่งพิมพ์บนจอ แม้ยังไม่ได้กดบันทึก
        {
            Notify.WarnModal(this, "ค่าที่กรอกไม่ถูกต้อง", invalid); // แจ้งค่าบนจอที่แปลงไม่ผ่าน
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (PlcOrderService.UnsendableReason(_pattern) is string blank) // กันช่องจำเป็นว่างก่อนส่ง PLC ในทางทดสอบ
        {
            Notify.WarnModal(this, "ส่งเข้า PLC ไม่ได้", // แจ้งว่าค่าจำเป็นของ PLC ยังไม่ครบ
                blank + Environment.NewLine + Environment.NewLine // แนบชื่อช่องที่ยังว่าง
                + "กรอกค่าให้ครบแล้วลองใหม่"); // ให้กรอกค่าก่อนกดทดสอบใหม่
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var plan = await PlcOrderService.BuildPlanAsync(_api, _pattern, usedHeadsOnly: true); // จับค่ากับ register เฉพาะหัวที่งานใช้จริง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (plan.Count == 0) // ตรวจว่าไม่มีแผนค่า PLC ให้ส่ง
        {
            Notify.WarnModal(this, "ทดสอบส่ง PLC", "ยังไม่มีข้อมูลงานให้ส่ง"); // แจ้งว่ายังไม่มีข้อมูลงานสำหรับทดสอบ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var ready = plan.Where(f => f.Address != null).ToList(); // แยกรายการที่มี address พร้อมส่ง
        var missing = plan.Where(f => f.Address == null).ToList(); // รายการที่ยังไม่ map ต้องบอกผู้ทดสอบก่อนยืนยัน

        if (ready.Count == 0) // ตรวจว่าไม่มีค่าที่ map address แล้ว
        {
            Notify.WarnModal(this, "ทดสอบส่ง PLC", // แจ้งปัญหาการตั้ง register map
                "ไม่มีค่าไหน map address ไว้เลย — ตั้งค่าที่ตาราง register map ในหน้า PLC Setting ก่อน"); // ชี้ให้ตั้ง address ในหน้า PLC Setting
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var summary = string.Join(Environment.NewLine, // รวมค่าที่จะส่งเป็นข้อความยืนยัน
            ready.Select(f => $"D{f.Address}   {f.Label}   =  {f.Value}")); // แสดง address ชื่อค่า และค่าที่จะเขียน

        if (missing.Count > 0) // มีรายการที่ยังขาด address
        {
            summary += Environment.NewLine + Environment.NewLine // เพิ่มส่วนแจ้งรายการที่จะข้าม
                + "ข้ามเพราะยังไม่ได้ map: " // ระบุว่าข้ามเพราะยังไม่ map
                + string.Join(", ", missing.Select(f => f.ListName)); // แนบชื่อรายการที่ไม่มี address
        }

        var ip = CustomSettingsManager.Read("PLC_IP", "").Trim(); // อ่าน IP ของ PLC ที่จะทดสอบ
        var port = CustomSettingsManager.Read("PLC_PORT", "502"); // อ่านพอร์ต PLC หรือใช้ 502
        var target = ip.Length == 0 ? "(ยังไม่ได้ตั้ง IP)" : $"{ip}:{port}"; // เตรียมปลายทางให้ผู้ใช้ตรวจในกล่องยืนยัน

        if (!Confirm.Ask(this, "ยืนยันส่งค่าเข้า PLC", // ให้ยืนยันก่อนเขียนค่าเข้า PLC
                $"PLC {target}" + Environment.NewLine + Environment.NewLine + summary // แสดงปลายทางพร้อมค่าแต่ละ register
                + Environment.NewLine + Environment.NewLine + "ยืนยันส่งหรือไม่?")) // ถามยืนยันส่งชุดที่แสดง
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        btnTestPlc.Enabled = false; // กันกด Test PLC ซ้ำระหว่างส่ง
        var originalText = btnTestPlc.Text; // จำข้อความปุ่มเดิมไว้คืนตอนจบ
        btnTestPlc.Text = "กำลังส่ง..."; // เปลี่ยนปุ่มให้รู้ว่ากำลังส่ง PLC
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var lines = (await PlcOrderService.SendAsync(plan)) // ส่งชุดค่าแล้วเตรียมข้อความผลแต่ละ register
                .Select(b => // แปลงผลแต่ละค่าเป็นรายการแจ้งบนจอ
                {
                    if (b.Error != null) return Notify.Bad($"{b.Name} — {b.Error}"); // มีข้อผิดพลาด ให้แสดงชื่อค่าและสาเหตุ

                    if (b.ReadBack == null) // ตรวจว่ามีค่าที่อ่านกลับได้หรือไม่
                        return Notify.Careful($"{b.Name} = {b.Value} (อ่านกลับไม่ได้)"); // เตือนว่าส่งแล้วแต่ยังยืนยันค่ากลับไม่ได้

                    return b.ReadBack == b.Value // เทียบค่าที่อ่านกลับกับค่าที่ตั้งใจส่ง
                        ? Notify.Ok($"{b.Name} = {b.Value}") // ค่าตรงกันให้แสดงผลผ่าน
                        : Notify.Careful($"{b.Name} = {b.Value} · อ่านกลับได้ {b.ReadBack}"); // ค่าต่างกันให้แสดงทั้งสองค่าเพื่อเทียบ
                })
                .ToList(); // เก็บผลที่กรองแล้วเป็นรายการ

            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            Notify.Result(this, "ผลการส่ง PLC", lines); // แสดงผลส่ง PLC ทั้งชุด
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            if (!IsDisposed) // ทำต่อเมื่อหน้ายังเปิดอยู่
            {
                btnTestPlc.Text = originalText; // คืนข้อความปุ่มหลังจบทดสอบ
                btnTestPlc.Enabled = true; // เปิดปุ่มให้ทดสอบครั้งถัดไป
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

        _jobLabel = Services.JobDisplay.Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id);

        txtJobErpMfg.Text = OrDash(job.OrderNo);
        txtJobLotNo.Text = OrDash(job.BarcodeRaw);
        txtJobCustomer.Text = OrDash(job.CustomerName);
        txtJobQty.Text = job.Qty?.ToString() ?? Dash;
        var jobStatus = Theme.JobStatusDisplay.Resolve(
            job.Status,
            MarkingMethodService.FinishedIncomplete(
                job.Status, resolved.PlanRouting?.MarkingMethod, resolved.Commands));
        txtJobStatus.Text = OrDash(jobStatus.Text);
        txtJobStatus.ForeColor = jobStatus.Fore;
        _baseStatusText = jobStatus.Text;

        _ = ShowStageAsync(job.Id);

        var marking = resolved.PlanRouting?.MarkingMethod;
        txtMarkingMethod.Text = string.IsNullOrWhiteSpace(marking) ? "ไม่ระบุ" : marking;
    }

    private string _baseStatusText = "";

    private async Task ShowStageAsync(int jobId) // เติมเลข Q หรือด้านพิมพ์ท้ายสถานะ
    {
        if (_api == null) return; // ยังไม่มีตัวเรียก Backend ให้หยุดก่อน

        var (rows, error) = await _api.GetMachineQueueAsync(); // อ่านคิวกลางก่อนเติมรายละเอียดท้ายสถานะ
        if (error != null || IsDisposed) return; // อ่านคิวไม่ได้หรือปิดหน้าแล้ว ให้ใช้สถานะเดิม

        if (jobId != _jobId) return; // เปลี่ยน Job ระหว่างรอแล้ว ไม่ใช้คำตอบเก่าทับหน้าปัจจุบัน

        var stage = Services.JobStageService.Describe(jobId, _markingMethod, rows); // แปลงคิวเป็นเลข Q หรือด้านที่กำลังพิมพ์
        if (stage == null || _baseStatusText.Length == 0) return; // ไม่มีสถานะย่อยหรือสถานะหลัก ให้ข้าม

        txtJobStatus.Text = $"{_baseStatusText} ({stage})"; // เติมรายละเอียดหลังสถานะ เช่น Process (Mark Plate)
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

    private void FillUvSection(ResolvedJobResponse resolved)
    {
        var uvRows = resolved.UvJobData;
        var commands = resolved.Commands;

        var uv1 = uvRows.FirstOrDefault(r => r.Machine == "UV1");
        var uv2 = uvRows.FirstOrDefault(r => r.Machine == "UV2");

        txtUvQtyShared.Text = (uv1?.Qty ?? uv2?.Qty)?.ToString() ?? Dash;

        FillUv(uv1, txtUv1Program, txtUv1ErpMfg, tblUv1Texts,
            SentProgram(commands, "UV1") ?? PendingProgram(resolved, "UV1"));
        FillUv(uv2, txtUv2Program, txtUv2ErpMfg, tblUv2Texts,
            SentProgram(commands, "UV2") ?? PendingProgram(resolved, "UV2"));
    }

    private static string? PendingProgram(ResolvedJobResponse resolved, string machine)
    {
        var pending = resolved.Job.RemoteProgram?.Trim();
        if (string.IsNullOrEmpty(pending)) return null;

        var step = MarkingMethodService
            .Resolve(resolved.PlanRouting?.MarkingMethod)
            .Steps.FirstOrDefault();

        return string.Equals(step, machine, StringComparison.OrdinalIgnoreCase) ? pending : null;
    }

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

        btnIaiAdj1Z1Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z1Value, -1);
        btnIaiAdj1Z1Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z1Value, +1);
        btnIaiAdj1Z1Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj1Z1Value, isPlate: true, zone: "Z1");
        btnIaiAdj1Z1Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj1Z1Value, txtUv1Program, txtUv1IaiZ1, isPlate: true, zone: "Z1");
        btnIaiAdj1Z1Reset.Click += (_, _) => { txtIaiAdj1Z1Value.Text = _origIai?.IaipZ1?.ToString() ?? ""; };

        btnIaiAdj1Z2Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z2Value, -1);
        btnIaiAdj1Z2Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj1Z2Value, +1);
        btnIaiAdj1Z2Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj1Z2Value, isPlate: true, zone: "Z2");
        btnIaiAdj1Z2Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj1Z2Value, txtUv1Program, txtUv1IaiZ2, isPlate: true, zone: "Z2");
        btnIaiAdj1Z2Reset.Click += (_, _) => { txtIaiAdj1Z2Value.Text = _origIai?.IaipZ2?.ToString() ?? ""; };

        btnIaiAdj2Z1Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z1Value, -1);
        btnIaiAdj2Z1Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z1Value, +1);
        btnIaiAdj2Z1Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj2Z1Value, isPlate: false, zone: "Z1");
        btnIaiAdj2Z1Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj2Z1Value, txtUv2Program, txtUv2IaiZ1, isPlate: false, zone: "Z1");
        btnIaiAdj2Z1Reset.Click += (_, _) => { txtIaiAdj2Z1Value.Text = _origIai?.IaiZ1?.ToString() ?? ""; };

        btnIaiAdj2Z2Minus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z2Value, -1);
        btnIaiAdj2Z2Plus.Click += (_, _) => AdjustIaiValue(txtIaiAdj2Z2Value, +1);
        btnIaiAdj2Z2Send.Click += async (_, _) => await IaiSendAsync(txtIaiAdj2Z2Value, isPlate: false, zone: "Z2");
        btnIaiAdj2Z2Upload.Click += async (_, _) => await IaiUploadAsync(txtIaiAdj2Z2Value, txtUv2Program, txtUv2IaiZ2, isPlate: false, zone: "Z2");
        btnIaiAdj2Z2Reset.Click += (_, _) => { txtIaiAdj2Z2Value.Text = _origIai?.IaiZ2?.ToString() ?? ""; };
    }

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

    private static string IaiAxisKey(bool isPlate, string? zone) =>
        (isPlate ? "IAIP" : "IAI") + (zone ?? "");

    private string JobText() => _jobLabel.Length > 0 ? _jobLabel : $"#{_jobId}";

    private bool CanCommandIai() // เช็กสิทธิ์ก่อน Send หรือ Upload ค่าแคลมป์
    {
        if (_isDevMode || _jobId <= 0) return true; // โหมด Dev และหน้าที่ไม่ผูก Job สั่งแคลมป์ได้โดยตรง

        if (string.Equals(_jobStatus, "Process", StringComparison.OrdinalIgnoreCase)) // งานปกติต้องเริ่มเป็น Process ก่อนสั่งแคลมป์
            return true;

        Notify.WarnModal(this, "ยังสั่งแคลมป์ไม่ได้", // แจ้งว่างานยังไม่พร้อมสั่งแคลมป์
            $"{JobText()} ยังไม่ได้เริ่มงาน (สถานะ {JobStatusDisplay.Text(_jobStatus)})\n\n" // แสดง Job และสถานะปัจจุบันที่ถูกกันไว้
            + "กดเริ่มงานที่หน้ารายการงานก่อน จึงจะสั่งค่า IAI ได้"); // ให้เริ่มงานจาก List ก่อนสั่ง IAI
        return false;
    }

    private async Task IaiSendAsync(AntdUI.Input input, bool isPlate, string? zone) // ส่งระยะไปยังแกนแคลมป์ที่เลือก
    {
        if (!CanCommandIai()) return; // ตรวจสิทธิ์สั่งแคลมป์ก่อนทำต่อ

        if (!int.TryParse(input.Text.Trim(), out int mm)) // ระยะต้องกรอกเป็นจำนวนเต็ม
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณากรอกค่า IAI เป็นตัวเลข"); // เตือนให้กรอกระยะเป็นตัวเลข
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        mm = ClampService.ClampMm(mm); // จำกัดระยะให้อยู่ในช่วงของแคลมป์
        var s = ClampSettings.Load(); // อ่านค่าฐานข้อมูลและ PLC ของแคลมป์

        if (string.IsNullOrEmpty(s.Ip)) // ตรวจว่ายังไม่ได้ตั้ง IP ของแคลมป์หรือไม่
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ยังไม่ได้ตั้งค่า Clamp PLC IP ในหน้า Setting"); // ชี้ให้ตั้ง IP ของ Clamp PLC ก่อน
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var axis = s.Find(IaiAxisKey(isPlate, zone)); // เลือกแกนให้ตรง Plate หรือ Shim และโซน
        if (axis == null) return; // ไม่มีข้อมูลแกนนี้ จึงยังสั่งแคลมป์ไม่ได้

        if (!axis.IsConfigured) // ตรวจว่า Target และ Run ตั้งครบหรือยัง
        {
            var missing = axis.AddrTarget.Trim().Length == 0 // แยกว่าขาด address ของ Target หรือไม่
                ? (axis.AddrRun.Trim().Length == 0 ? "Target (D) และ Run (M)" : "Target (D)") // ระบุว่าขาดทั้งสองช่องหรือขาดแค่ Target
                : "Run (M)"; // มี Target แล้ว จึงเหลือช่อง Run ที่ขาด

            Notify.WarnModal(this, "แจ้งเตือน", // เตือนเรื่อง address ของแกนที่ยังไม่ครบ
                $"แกน {axis.Display} ยังขาด {missing}\nตั้งค่าได้ที่ Setting -> Clamp Setting"); // บอกชื่อแกนกับช่องที่ต้องตั้งเพิ่ม
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (!Confirm.Ask(this, "ยืนยันสั่งแคลมป์", // ให้ยืนยันแกนและปลายทางก่อนสั่งวิ่ง
                $"สั่ง {axis.Display} ไปที่ {mm} mm\n\n" // แสดงแกนและระยะที่ผู้ใช้เลือก
                + $"-> {axis.AddrTarget} = {ClampService.ToRaw(mm)}\n" // แสดง D และค่าที่แปลงให้ PLC แล้ว
                + $"-> {axis.AddrRun} (pulse)\n\nยืนยันหรือไม่?")) // แสดงบิต Run ที่จะพัลส์ก่อนยืนยัน
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        SetIaiAdjustBusy(true); // กันกดสั่งแคลมป์ซ้ำระหว่างทำรายการ
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var result = await ClampService.ApplyAsync(s, axis, mm); // เขียนระยะลง D แล้วพัลส์ M ให้แกนวิ่ง
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            if (result.Ok) // ตรวจผลเขียนระยะและพัลส์แกน
                Notify.Success(this, $"สั่ง {axis.Display} ไปที่ {mm} mm สำเร็จ"); // แจ้งว่าออกคำสั่งแกนตามระยะแล้ว
            else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                Notify.ErrorModal(this, "สั่งแคลมป์ไม่สำเร็จ", result.Log); // แสดง log เมื่อสั่งแคลมป์ไม่ผ่าน
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            if (!IsDisposed) SetIaiAdjustBusy(false); // จบรายการแล้วเปิดปุ่มแคลมป์ให้ใช้อีกครั้ง
        }
    }

    private async Task IaiUploadAsync(AntdUI.Input input, AntdUI.Input programInput, AntdUI.Input displayInput, bool isPlate, string? zone) // เก็บระยะที่ปรับไว้ใช้กับโปรแกรมและ Job นี้
    {
        if (!CanCommandIai()) return; // ตรวจสิทธิ์สั่งแคลมป์ก่อนทำต่อ

        if (!int.TryParse(input.Text.Trim(), out int mm)) // ระยะต้องกรอกเป็นจำนวนเต็ม
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณากรอกค่า IAI เป็นตัวเลข"); // เตือนให้กรอกระยะเป็นตัวเลข
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        string program = programInput.Text.Trim(); // ใช้ชื่อโปรแกรม UV หาระยะแคลมป์ในฐานต้นทาง
        if (string.IsNullOrEmpty(program) || program == Dash) // ต้องมีชื่อโปรแกรมจริงก่อน Upload
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ไม่มีชื่อโปรแกรม UV"); // แจ้งว่าช่องชื่อโปรแกรม UV ยังว่าง
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var s = ClampSettings.Load(); // อ่านค่าฐานข้อมูลและ PLC ของแคลมป์
        if (string.IsNullOrEmpty(s.DbPath)) // ตรวจว่าตั้งไฟล์ฐานแคลมป์แล้วไหม
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ยังไม่ได้ตั้ง path ของ mydatabase.db3 ในหน้า Setting"); // ชี้ให้ตั้ง mydatabase.db3 ก่อนบันทึก
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        mm = ClampService.ClampMm(mm); // จำกัดระยะให้อยู่ในช่วงของแคลมป์
        var axis = s.Find(IaiAxisKey(isPlate, zone)); // เลือกแกนให้ตรง Plate หรือ Shim และโซน
        if (axis == null) return; // ไม่มีข้อมูลแกนนี้ จึงยังสั่งแคลมป์ไม่ได้
        string col = axis.Column; // เลือกคอลัมน์ให้ตรงกับแกน Plate หรือ Shim

        if (!Confirm.Ask(this, "ยืนยัน Upload", // ให้ยืนยันก่อนแก้ระยะในสองฐาน
                $"บันทึก {col} = {mm} mm ให้ \"{program}\"\nไปยัง mydatabase และ Backend\n\nยืนยันหรือไม่?")) // แสดงแกน ระยะ โปรแกรม และฐานที่จะบันทึก
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        SetIaiAdjustBusy(true); // กันกดสั่งแคลมป์ซ้ำระหว่างทำรายการ
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var errors = new List<string>(); // เตรียมเก็บปัญหาที่พบในรอบนี้

            var (dbOk, dbMsg) = ClampService.Upload(s.DbPath, program, axis, mm); // อัปเดตระยะใน mydatabase ก่อนบันทึกฐานกลาง
            if (!dbOk) errors.Add($"mydatabase: {dbMsg}"); // เก็บข้อผิดพลาดไว้ โดยยังลองบันทึก Backend ต่อ

            if (_api != null && _jobId > 0) // มี Backend และ Job สำหรับเก็บค่า IAI
            {
                var request = new IaiCreateRequest { PrintJobsId = _jobId }; // ผูกค่าที่ปรับกับ Job ที่เปิดอยู่
                if (isPlate) // เลือกค่าของฝั่ง Plate
                {
                    request.M1ProgramName = program; // ใช้ชื่อโปรแกรมของ Plate
                    if (zone == null) request.Iaip = mm; // บันทึกแกนหลักของ Plate
                    else if (zone == "Z1") request.IaipZ1 = mm; // บันทึกแกน Z1 ของ Plate
                    else if (zone == "Z2") request.IaipZ2 = mm; // บันทึกแกน Z2 ของ Plate
                }
                else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                {
                    request.M2ProgramName = program; // ใช้ชื่อโปรแกรมของ Shim
                    if (zone == null) request.Iai = mm; // บันทึกแกนหลักของ Shim
                    else if (zone == "Z1") request.IaiZ1 = mm; // บันทึกแกน Z1 ของ Shim
                    else if (zone == "Z2") request.IaiZ2 = mm; // บันทึกแกน Z2 ของ Shim
                }

                var (apiOk, apiErr) = await _api.CreateIaiAsync(request); // บันทึก Backend แยกจาก mydatabase จึงอาจสำเร็จแค่ฝั่งเดียว
                if (!apiOk) errors.Add($"Backend: {apiErr}"); // เก็บเหตุที่ฝั่ง Backend บันทึกไม่ผ่าน
            }
            else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
            {
                errors.Add("Backend: ไม่มีการเชื่อมต่อ API หรือยังไม่ได้โหลดงาน"); // จดว่าฐานกลางยังไม่มี Job หรือการเชื่อมต่อ
            }

            if (errors.Count == 0) // ทั้งสองปลายทางไม่มีเหตุผิดพลาด
            {
                Notify.Success(this, $"บันทึก {col} = {mm} mm ให้ \"{program}\" แล้ว"); // แจ้งโปรแกรมและระยะที่บันทึกแล้ว
                displayInput.Text = mm.ToString(); // อัปเดตค่าระยะที่แสดงบนจอ
            }
            else if (dbOk || (_api != null && _jobId > 0)) // เข้าเงื่อนไขแจ้งผลบันทึกบางส่วนตามทางเดิม
                Notify.WarnDetail(this, "Upload บางส่วนไม่สำเร็จ", string.Join("\n", errors)); // แสดงส่วนที่ Upload ยังไม่ผ่าน
            else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                Notify.ErrorModal(this, "Upload ไม่สำเร็จ", string.Join("\n", errors)); // แสดงปัญหาเมื่อ Upload ทำต่อไม่ได้
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            SetIaiAdjustBusy(false); // เปิดให้ปรับแคลมป์ได้หลังจบรอบ
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
        program.Text = sentProgram ?? OrDash(uv?.ProgramName);
        erpMfg.Text = OrDash(uv?.ErpMfg);

        var values = new[] { uv?.Text1, uv?.Text2, uv?.Text3, uv?.Text4, uv?.Text5 };
        var rows = values
            .Select((v, i) => new UvTextRow { Field = $"Text{i + 1}", Value = OrDash(v) })
            .ToList();

        table.Tag = rows.Select(r => r.Value).ToList();

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
