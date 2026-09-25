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

    /// <summary>กำลังบันทึก pattern อยู่ — กันกดปุ่มที่แก้ pattern ซ้อนกันระหว่างนั้น</summary>
    private bool _savingPattern;
    private List<string> _sendSteps = [];
    private int _currentStep;
    private int _jobId;
    private ApiClient? _api;
    private ImageHoverPopup? _refPopup;
    private List<UvJobDataDto> _uvData = [];

    // เก็บไว้เพื่อคำนวณบรรทัด Plate / Shim ใหม่ได้ทุกเมื่อ ไม่ใช่แค่ตอนเปิดหน้า
    private string? _markingMethod;
    private string? _erpMfg;

    /// <summary>
    /// โปรแกรม UV ที่ "เลือกแล้ว" ของแต่ละเครื่อง — คีย์เป็น "UV1" / "UV2"
    ///
    /// มาจากสามทาง: รุ่นที่ส่งเข้าเครื่องไปแล้ว · รุ่นที่ ST3 เลือกไว้รอ ST1 ส่ง ·
    /// และรุ่นที่ผู้ใช้เพิ่งเลือกในหน้านี้ ไม่มีในนี้ = ยังใช้ชื่อฐานจากข้อมูลงาน
    ///
    /// รูปอ้างอิงของ Plate / Shim อ่านจากตรงนี้ที่เดียว เปลี่ยนค่าแล้วเรียก
    /// <see cref="RefreshFlowRows"/> รูปจะตามทันทีโดยไม่มีทางค้างรุ่นเก่า
    /// </summary>
    private readonly Dictionary<string, string> _chosenUvProgram = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>สถานะงานตอนเปิดหน้า — ใช้กันไม่ให้สั่งแคลมป์ก่อนเริ่มงาน</summary>
    private string _jobStatus = "";

    /// <summary>ชื่อเรียกงานในข้อความที่พนักงานอ่าน — "ERP (LOT)" ไม่ใช่เลข id</summary>
    private string _jobLabel = "";

    /// <summary>รอบก่อนยังตรวจไม่เสร็จ — กันไม่ให้รอบใหม่ทับ</summary>
    private bool _connCheckBusy;

    private readonly bool _isDevMode;
    private IaiClampSettingDto? _origIai;

    public event EventHandler? CloseRequested;

    /// <summary>
    /// ขอให้หน้า Order List สั่ง ST1 ส่งขั้นถัดไปให้ — ค่าที่แนบมาคือชื่อขั้น เช่น "UV2"
    ///
    /// <para>
    /// หน้านี้ไม่ยิงคำขอเอง เพราะการขอให้ ST1 ส่งมีด่านตรวจอยู่ที่หน้า Order List ครบแล้ว
    /// (สถานีปลายทางว่างไหม · เลือกรุ่นย่อยของโปรแกรม UV · รอผลจริงจาก ST1 ไม่เกิน 40 วิ)
    /// ทำอีกชุดที่นี่คือเปิดทางให้สองที่ตรวจไม่เหมือนกันในวันข้างหน้า
    /// </para>
    /// </summary>
    public event EventHandler<string>? RemoteStartRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        ConfigureNumericInputs();

        var rawLevel = CustomSettingsManager.Read("MENU_LEVEL", "1");
        _isDevMode = int.TryParse(rawLevel, out var lvl) && lvl == 99;

        // หน้าตาปุ่มปิดมาจากที่เดียวกับทุกหน้า — designer คุมแค่ตำแหน่งกับขนาด
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

        // เลือกรุ่นย่อยแล้วต้องเห็นเฉพาะรุ่นนั้น ค้นแบบตรงเป๊ะจึงไม่ลากรุ่นพี่น้อง
        // ที่ชื่อขึ้นต้นเหมือนกันติดมาด้วย — ยังไม่ได้เลือกค่อยดูรวมทุกรุ่นไปก่อน
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

        // ข้อความกับตำแหน่งของแต่ละบล็อกแก้ได้เหมือนโปรแกรมเดิม แตะช่องแล้วพิมพ์ทับ
        // แตะครั้งเดียวพอ ไม่ใช่ดับเบิลคลิก เพราะบนจอสัมผัสดับเบิลคลิกทำยาก
        tblMk1Blocks.EditMode = AntdUI.TEditMode.Click;
        tblMk2Blocks.EditMode = AntdUI.TEditMode.Click;
        tblUv1Texts.Columns = BuildUvColumns();
        tblUv2Texts.Columns = BuildUvColumns();

        // ข้อความของ UV แก้ได้ทุกช่อง — คลิกที่ช่องแล้วพิมพ์ทับได้เลย
        // ส่วนอื่นของฝั่ง UV ยังล็อกไว้เหมือนเดิม
        tblUv1Texts.EditMode = AntdUI.TEditMode.Click;
        tblUv2Texts.EditMode = AntdUI.TEditMode.Click;

        // X Y Size Scale เป็นตัวเลขล้วน ส่วนช่อง Text ของบล็อกและของ UV พิมพ์อะไรก็ได้
        NumericInput.DigitsOnlyColumns(tblMk1Blocks, "X", "Y", "Size", "Scale");
        NumericInput.DigitsOnlyColumns(tblMk2Blocks, "X", "Y", "Size", "Scale");
    }

    /// <summary>
    /// ช่องที่รับได้แต่ตัวเลข — ผูกครั้งเดียวตอนสร้างหน้า
    ///
    /// <para>
    /// Width Height Trigger Delay และความเร็วสายพาน เก็บเป็นจำนวนเต็ม ส่วน
    /// Servo Post Act. กับ Delay เก็บเป็นทศนิยม จึงยอมให้ใส่จุดได้จุดเดียว
    /// </para>
    /// <para>
    /// ค่า IAI ทั้งหกช่องเป็นจำนวนเต็มมิลลิเมตร ปุ่ม Send กับ Upload ตรวจซ้ำอยู่แล้ว
    /// ตรงนี้กันไม่ให้พิมพ์ผิดตั้งแต่แรกเฉย ๆ
    /// </para>
    /// </summary>
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
        // เลขบล็อกเป็นตัวชี้ว่าแถวนี้คือช่องไหนของเครื่อง แก้ไม่ได้
        new AntdUI.Column("Block", "Block", AntdUI.ColumnAlign.Center) { Width = "16%", Editable = false },
        new AntdUI.Column("BlockText", "Text", AntdUI.ColumnAlign.Left) { Width = "36%" },
        new AntdUI.Column("X", "X", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Y", "Y", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Size", "Size", AntdUI.ColumnAlign.Center) { Width = "12%" },
        new AntdUI.Column("Scale", "Scale", AntdUI.ColumnAlign.Center) { Width = "12%" },
    ];

    private static AntdUI.ColumnCollection BuildUvColumns() =>
    [
        // ชื่อช่องเป็นตัวชี้ว่าแถวนี้คือข้อความที่เท่าไร แก้ไม่ได้
        new AntdUI.Column("Field", "Field", AntdUI.ColumnAlign.Center) { Width = "30%", Editable = false },
        new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Left) { Width = "70%", Editable = true },
    ];

    /// <summary>
    /// ชื่อเรียกงานบนหัวจอ — "Job #1 · 07/09/26"
    ///
    /// เลขงานเริ่มที่ 1 ใหม่ทุกวัน ลำพังเลขจึงระบุงานไม่ได้ ต้องมีวันที่กำกับเสมอ
    /// งานเก่าที่รับก่อนมีเลขประจำวันตกไปใช้ id ของตารางแทน
    ///
    /// เป็น public เพราะแถบหัวหน้าต่างที่ครอบหน้านี้อยู่ต้องเรียกชื่อเดียวกัน —
    /// เดิมสองที่ประกอบชื่อกันเอง หัวหน้าต่างเลยขึ้น id ส่วนหัวในหน้าขึ้นเลขประจำวัน
    /// กลายเป็นงานเดียวกันแต่เห็นสองเลขบนจอเดียว
    /// </summary>
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

        // ต้องรู้ว่าแต่ละเครื่องเลือกรุ่นไหนไว้แล้ว ก่อนจะไปคำนวณบรรทัด Plate / Shim
        _chosenUvProgram.Clear();
        foreach (var machine in new[] { "UV1", "UV2" })
        {
            var chosen = SentProgram(resolved.Commands, machine) ?? PendingProgram(resolved, machine);
            if (chosen != null) _chosenUvProgram[machine] = chosen;
        }

        lblHeaderTitle.Text = $"Job Information — {JobTitle(resolved.Job)}";

        // โชว์ address ที่ค่าแต่ละช่องจะถูกส่งไป ดึงจากตาราง register map ของ
        // หน้า PLC Setting ไม่ได้ให้รอ เพราะแค่ป้ายกำกับ ไม่ควรหน่วงการเปิดหน้า
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
        // รอบแรกโชว์ "กำลังตรวจสอบ..." ได้ เพราะยังไม่มีอะไรให้ดูอยู่ก่อน
        // รอบถัด ๆ ไปห้ามล้างเป็นสีเทา ไม่งั้นไฟจะกะพริบเทา-เขียวทุก 15 วิ
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

    /// <summary>
    /// จังหวะรีเฟรชไฟสถานะ — <c>async void</c> ตัวเดียวที่ยอมให้มีในหน้านี้
    /// ข้างในจึงห้ามโยน exception ออกมาเด็ดขาด ไม่งั้นโปรแกรมหลุดทั้งตัว
    /// (<see cref="CheckConnectionsAsync"/> กลืน exception ไว้หมดแล้ว)
    /// </summary>
    private async void ConnCheck_Tick(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        await CheckConnectionsAsync(showChecking: false);
    }

    /// <summary>
    /// ไฟสี่ดวงบอกว่าต่อเครื่องไหนติดบ้าง — รอบแรกตอนเปิดหน้า แล้ววนเองทุก 15 วินาที
    ///
    /// <para>
    /// ไม่บล็อกอะไรเลย งานทั้งหมดเป็น I/O แบบ async และ timeout อยู่ที่ 3 วินาที
    /// ต่อปลายทาง ยิงพร้อมกันทั้งสี่ รอบหนึ่งจึงนานเท่ารายที่ช้าที่สุดรายเดียว
    /// </para>
    /// <para>
    /// เงียบเสมอ ไม่มีกล่องเด้ง ต่อไม่ติดก็แค่เปลี่ยนสีกับข้อความบนป้าย และถ้ารอบไหน
    /// พลาด ไฟจะค้างค่าเดิมไว้เฉย ๆ รอรอบหน้า ดีกว่าล้างเป็นเทาให้คนเข้าใจผิด
    /// </para>
    /// </summary>
    private async Task CheckConnectionsAsync(bool showChecking)
    {
        // รอบก่อนยังไม่จบก็ข้ามรอบนี้ ไม่ต่อคิวซ้อนกันตอนปลายทางอืด
        if (_connCheckBusy || IsDisposed) return;

        // กำลังส่งงานเข้าเครื่องอยู่ก็ข้ามเหมือนกัน เหตุผลอยู่ที่ MachineBusy
        // ไฟสถานะยอมช้าไปหนึ่งรอบได้ การส่งงานยอมพลาดไม่ได้
        if (MachineBusy.Active) return;

        _connCheckBusy = true;
        try
        {
            await RunConnectionCheckAsync(showChecking);
        }
        catch
        {
            // ไฟสถานะพังไม่ควรลากทั้งหน้าไปด้วย ปล่อยค้างค่าเดิม รอบหน้ามาใหม่
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

        // ผลอาจกลับมาตอนคนปิดหน้าไปแล้วพอดี — ปล่อยผ่านเงียบ ๆ ไม่ใช่ปล่อยให้หลุด
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

        // ตอน timeout เราเดินต่อโดยทิ้ง task ไว้ ต้องมีคนรับ exception ของมัน
        // ไม่งั้นกลายเป็น unobserved exception ลอยอยู่ — เดิมเช็คครั้งเดียวตอนเปิดหน้า
        // เลยไม่เห็นผล ตอนนี้มันวนทุก 15 วินาทีตราบเท่าที่หน้ายังเปิดอยู่
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

    /// <summary>
    /// สลับว่าโปรแกรมไหนไปเข้าเครื่องไหน แล้วบันทึกลงฐานข้อมูล
    ///
    /// <para>
    /// ต้องบันทึกจริง ไม่ใช่เก็บไว้ในจอ เพราะปุ่มเริ่มงานย้ายไปอยู่หน้า Order List
    /// แล้ว และหน้านั้นอ่าน pattern ใหม่จาก backend ทุกครั้งที่สั่งส่ง ถ้าเก็บไว้
    /// แค่ในหน้านี้ กดสลับไปก็ไม่มีผลอะไรเลย
    /// </para>
    /// <para>
    /// บันทึกไม่ผ่านก็สลับกลับทันที จอจะได้ตรงกับของที่อยู่ในฐานข้อมูลจริงเสมอ
    /// ไม่ใช่ค้างโชว์ค่าที่ไม่ได้ถูกบันทึก แล้วพนักงานเข้าใจว่าสลับไปแล้ว
    /// </para>
    /// </summary>
    private async Task SwapMkDataAsync()
    {
        if (_pattern == null || _savingPattern) return;

        FlipMkOrdinals();

        var error = await SavePatternAsync();
        if (error == null) return;

        FlipMkOrdinals();
        Notify.ErrorModal(this, "สลับเครื่องไม่สำเร็จ",
            $"ยังไม่ได้บันทึกลงฐานข้อมูล จอจึงถูกปรับกลับเป็นค่าเดิม\n\n{error}");
    }

    /// <summary>
    /// สลับ ordinal ของทั้ง inkjet และ servo แล้ววาดจอใหม่
    ///
    /// ordinal คือตัวชี้ว่าไปเครื่องไหน — <c>JobSendService.SendMkAsync</c> หยิบ
    /// config ตาม ordinal (1 = MK-058, 2 = MK-059) การสลับ ordinal จึงเท่ากับ
    /// สลับปลายทางจริง ไม่ใช่แค่สลับที่โชว์บนจอ
    ///
    /// <para>
    /// ป้ายชื่อเครื่องบนหัวคอลัมน์ไม่ต้องสลับตาม เพราะคอลัมน์ซ้ายผูกกับ ordinal 1
    /// ซึ่งคือ MK-058 เสมอ เดิมโค้ดสลับป้ายด้วย ป้ายเลยบอกเครื่องผิดตัวหลังกดสลับ
    /// </para>
    /// </summary>
    private void FlipMkOrdinals()
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

        FillMkSection(_pattern);
    }

    /// <summary>
    /// บันทึก pattern ที่ถืออยู่ในหน้านี้ลง backend — คืนข้อความปัญหา หรือ null เมื่อสำเร็จ
    ///
    /// ล็อกปุ่มที่แก้ pattern ไว้ระหว่างบันทึก กันกดรัวจนคำสั่งสองชุดไปถึง backend
    /// สลับกันแล้วได้ผลลัพธ์ที่ไม่ตรงกับที่เห็นบนจอ
    /// </summary>
    /// <summary>
    /// เก็บค่าที่แก้ในหน้านี้กลับเข้า pattern แล้วบันทึกลงฐานข้อมูล
    ///
    /// <para>
    /// ต้องบันทึกลงฐานข้อมูลจริง ไม่ใช่เก็บไว้ในจอ เพราะการส่งงานจริงเกิดที่หน้า
    /// Order List ซึ่งอ่าน pattern ใหม่จาก backend ทุกครั้ง แก้ไว้ในจออย่างเดียว
    /// จึงไม่มีผลอะไรเลย — เหตุผลเดียวกับปุ่ม Swap กับ ABC
    /// </para>
    /// <para>
    /// pattern ผูกกับงานแบบหนึ่งต่อหนึ่ง (backend หาด้วย <c>job_id</c>) การแก้ที่นี่
    /// จึงกระทบงานนี้งานเดียว ไม่ลามไปงานอื่นที่ใช้แบบเดียวกัน
    /// </para>
    /// </summary>
    private async Task SaveEditedValuesAsync()
    {
        if (_savingPattern) return;

        if (CollectEditedValues() is string problem)
        {
            Notify.WarnModal(this, "ค่าที่กรอกไม่ถูกต้อง", problem);
            return;
        }

        btnSavePattern.Enabled = false;
        try
        {
            var error = await SavePatternAsync();
            if (IsDisposed) return;

            if (error != null)
            {
                Notify.ErrorModal(this, "บันทึกไม่สำเร็จ",
                    "ค่าที่แก้ยังไม่ได้ลงฐานข้อมูล" + Environment.NewLine + Environment.NewLine + error);
                return;
            }

            // ข้อความ UV อยู่คนละตารางกับ pattern จึงต้องบันทึกแยก
            var uvError = await SaveUvTextsAsync();
            if (IsDisposed) return;

            // วาดใหม่จากค่าที่บันทึกแล้ว ช่องที่เว้นว่างไว้จะได้กลับมาเป็นขีด
            FillMkSection(_pattern!);
            FillConveyor(_pattern!);

            if (uvError != null)
            {
                Notify.ErrorModal(this, "บันทึกข้อความ UV ไม่สำเร็จ",
                    "ค่าฝั่ง MK บันทึกแล้ว แต่ข้อความ UV ยังไม่ได้ลงฐานข้อมูล"
                    + Environment.NewLine + Environment.NewLine + uvError);
                return;
            }

            Notify.Success(this, "บันทึกค่าเรียบร้อย");
        }
        finally
        {
            if (!IsDisposed) btnSavePattern.Enabled = true;
        }
    }

    /// <summary>
    /// บันทึกข้อความ UV ที่ถูกแก้ — คืนข้อความปัญหา หรือ null เมื่อไม่มีอะไรผิด
    ///
    /// <para>
    /// ส่งเฉพาะช่องที่ค่าต่างไปจากที่วาดไว้ตอนเปิดหน้า และส่งเฉพาะแถวที่มีการแก้จริง
    /// แถวที่ไม่ได้แตะจะไม่ถูกเขียนทับ กันการเผลอลบข้อความของเครื่องที่ไม่ได้ยุ่งด้วย
    /// </para>
    /// <para>
    /// ช่องที่ลบจนว่างหรือใส่ขีดไว้ ถือว่าตั้งใจล้างข้อความนั้น ส่งเป็นค่าว่างไป
    /// ไม่ใช่ข้ามไปเฉย ๆ ไม่งั้นลบข้อความทิ้งไม่ได้เลย
    /// </para>
    /// </summary>
    private async Task<string?> SaveUvTextsAsync()
    {
        if (_api == null) return null;

        var problems = new List<string>();

        foreach (var (table, machine) in new[]
                 {
                     (tblUv1Texts, "UV1"),
                     (tblUv2Texts, "UV2"),
                 })
        {
            var row = _uvData.FirstOrDefault(r =>
                string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase));

            if (row == null) continue;
            if (table.DataSource is not List<UvTextRow> edited) continue;
            if (table.Tag is not List<string> original) continue;

            var changed = new Dictionary<string, string?>();
            for (int i = 0; i < edited.Count && i < original.Count; i++)
            {
                var now = (edited[i].Value ?? "").Trim();
                if (now == Dash) now = "";

                var before = (original[i] ?? "").Trim();
                if (before == Dash) before = "";

                if (now == before) continue;
                changed[$"text{i + 1}"] = now.Length == 0 ? null : now;
            }

            if (changed.Count == 0) continue;

            var (ok, error) = await _api.UpdateUvTextsAsync(row.Id, changed);
            if (IsDisposed) return null;

            if (!ok)
            {
                problems.Add($"{machine}: {error}");
                continue;
            }

            // เขียนกลับลงข้อมูลในมือด้วย ไม่งั้นกดส่งต่อทันทีจะส่งข้อความเก่าเข้าเครื่อง
            foreach (var (field, value) in changed)
            {
                switch (field)
                {
                    case "text1": row.Text1 = value; break;
                    case "text2": row.Text2 = value; break;
                    case "text3": row.Text3 = value; break;
                    case "text4": row.Text4 = value; break;
                    case "text5": row.Text5 = value; break;
                }
            }

            table.Tag = edited.Select(r => r.Value).ToList();
        }

        return problems.Count == 0 ? null : string.Join(Environment.NewLine, problems);
    }

    /// <summary>
    /// อ่านค่าจากช่องกรอกทั้งหมดกลับเข้า <see cref="_pattern"/>
    /// คืนข้อความปัญหาเมื่อมีช่องที่กรอกมาไม่ถูก หรือ null เมื่อเก็บครบ
    ///
    /// <para>
    /// ตรวจให้ครบก่อนค่อยเขียนลง pattern จะได้ไม่เหลือสภาพเก็บไปได้ครึ่งเดียว
    /// แล้วเด้ง error ทิ้งไว้
    /// </para>
    /// </summary>
    private string? CollectEditedValues()
    {
        if (_pattern == null) return "ยังไม่มีข้อมูล pattern ของงานนี้";

        var errors = new List<string>();

        int? Int(AntdUI.Input box, string label)
        {
            var text = box.Text.Trim();
            if (text.Length == 0 || text == Dash) return null;
            if (int.TryParse(text, out int v)) return v;
            errors.Add($"{label}: \"{text}\" ไม่ใช่จำนวนเต็ม");
            return null;
        }

        double? Dbl(AntdUI.Input box, string label)
        {
            var text = box.Text.Trim();
            if (text.Length == 0 || text == Dash) return null;
            if (double.TryParse(text, out double v)) return v;
            errors.Add($"{label}: \"{text}\" ไม่ใช่ตัวเลข");
            return null;
        }

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        // ชื่อโปรแกรมกับหมายเลขโปรแกรมไม่ได้อ่านกลับ เพราะล็อกไม่ให้แก้
        // เป็นตัวชี้ว่าจะใช้โปรแกรมไหนในเครื่อง เปลี่ยนคือพิมพ์คนละแบบทั้งใบ
        //
        // อ่านช่องที่เหลือให้ครบก่อน แม้เจอที่ผิดแล้ว จะได้บอกทีเดียวว่าผิดตรงไหนบ้าง
        var v1 = (W: Int(txtMk1Width, $"{mk1} Width"), H: Int(txtMk1Height, $"{mk1} Height"),
                  Trig: Int(txtMk1Trigger, $"{mk1} Trigger Delay"),
                  Act: Dbl(txtMk1PosAct, $"{mk1} Pos Act"), Dly: Dbl(txtMk1Delay, $"{mk1} Delay"));

        var v2 = (W: Int(txtMk2Width, $"{mk2} Width"), H: Int(txtMk2Height, $"{mk2} Height"),
                  Trig: Int(txtMk2Trigger, $"{mk2} Trigger Delay"),
                  Act: Dbl(txtMk2PosAct, $"{mk2} Pos Act"), Dly: Dbl(txtMk2Delay, $"{mk2} Delay"));

        var s1 = Int(txtConveyor1, "Conveyor 1");
        var s2 = Int(txtConveyor2, "Conveyor 2");
        var s3 = Int(txtConveyor3, "Conveyor 3");

        var blocks1 = ReadBlocks(tblMk1Blocks, mk1, errors);
        var blocks2 = ReadBlocks(tblMk2Blocks, mk2, errors);

        if (errors.Count > 0) return string.Join(Environment.NewLine, errors);

        void ApplyMk(int ordinal,
            (int? W, int? H, int? Trig, double? Act, double? Dly) v)
        {
            var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
            if (config != null)
            {
                config.Width = v.W;
                config.Height = v.H;
                config.TriggerDelay = v.Trig;
            }

            // PosAct กับ Delay อยู่บน ServoConfig ไม่ใช่ InkjetConfig — เป็นค่าที่ส่งเข้า PLC
            var servo = _pattern.ServoConfigs.FirstOrDefault(s => s.Ordinal == ordinal);
            if (servo != null)
            {
                servo.PostAct = v.Act;
                servo.Delay = v.Dly;
            }
        }

        ApplyMk(1, v1);
        ApplyMk(2, v2);
        ApplyBlocks(1, blocks1);
        ApplyBlocks(2, blocks2);

        if (_pattern.ConveyorSpeeds is { } speeds)
        {
            speeds.Speed1 = s1;
            speeds.Speed2 = s2;
            speeds.Speed3 = s3;
        }

        return null;
    }

    /// <summary>ค่าของบล็อกหนึ่งแถวที่อ่านกลับมาจากตาราง</summary>
    private readonly record struct BlockEdit(
        int Number, string? Text, int? X, int? Y, int? Size, int? Scale);

    /// <summary>
    /// อ่านค่าที่แก้ในตารางบล็อกกลับมา — ข้อความ ตำแหน่ง ขนาด และสเกล
    ///
    /// <para>
    /// ช่องข้อความเก็บผลที่ผ่าน <see cref="PatternEngine"/> แล้ว ไม่ใช่สูตรดิบ
    /// ถ้าผู้ใช้ไม่ได้แตะแถวนั้น ค่าที่อ่านกลับมาจึงเท่ากับผลลัพธ์เดิม ไม่ใช่สูตร
    /// การเขียนทับจึงทำเฉพาะแถวที่ค่าต่างไปจากที่วาดไว้ตอนเปิดหน้า
    /// </para>
    /// </summary>
    private List<BlockEdit> ReadBlocks(AntdUI.Table table, string machine, List<string> errors)
    {
        var result = new List<BlockEdit>();
        if (table.DataSource is not List<BlockRow> rows) return result;

        int? Int(string? text, string label)
        {
            var value = (text ?? "").Trim();
            if (value.Length == 0 || value == Dash) return null;
            if (int.TryParse(value, out int v)) return v;
            errors.Add($"{label}: \"{value}\" ไม่ใช่จำนวนเต็ม");
            return null;
        }

        foreach (var row in rows)
        {
            if (!int.TryParse(row.Block, out int number)) continue;

            var where = $"{machine} Block {number}";
            var text = (row.BlockText ?? "").Trim();

            result.Add(new BlockEdit(
                number,
                text.Length == 0 || text == Dash ? null : text,
                Int(row.X, $"{where} X"),
                Int(row.Y, $"{where} Y"),
                Int(row.Size, $"{where} Size"),
                Int(row.Scale, $"{where} Scale")));
        }

        return result;
    }

    /// <summary>
    /// เขียนค่าบล็อกที่แก้แล้วกลับลง pattern
    ///
    /// <para>
    /// ข้ามช่องข้อความของแถวที่ผู้ใช้ไม่ได้แตะ — บล็อกที่มีสูตรอยู่ (เช่นสูตรตัด
    /// บาร์โค้ด) ถูกวาดบนจอเป็นผลลัพธ์ที่แปลแล้ว ถ้าเขียนค่าที่เห็นกลับลงไปทุกแถว
    /// สูตรจะถูกแทนที่ด้วยข้อความตายตัวของงานนี้ แล้วงานถัดไปที่ใช้สูตรเดียวกัน
    /// จะพิมพ์ข้อความของงานเก่า
    /// </para>
    /// </summary>
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

            // ข้อความเขียนทับเฉพาะตอนที่ต่างไปจากผลลัพธ์ที่วาดไว้ตอนเปิดหน้า
            var shown = PatternEngine.Process(_barcode, block.Text ?? "");
            if (edit.Text != shown) block.Text = edit.Text;
        }
    }

    private async Task<string?> SavePatternAsync()
    {
        if (_pattern == null) return "ยังไม่มีข้อมูล pattern ของงานนี้";
        if (_api == null) return "ยังไม่ได้เชื่อมต่อ backend";

        _savingPattern = true;
        btnMkSwap.Enabled = false;
        picMk1Abc.Enabled = false;
        picMk2Abc.Enabled = false;
        try
        {
            var (ok, error) = await _api.UpdatePatternAsync(_pattern.Id, _pattern);
            return ok ? null : error ?? "บันทึกไม่สำเร็จ";
        }
        finally
        {
            _savingPattern = false;
            btnMkSwap.Enabled = true;
            picMk1Abc.Enabled = true;
            picMk2Abc.Enabled = true;
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

    // ── ABC = พิมพ์กลับหัว ─────────────────────────────────

    /// <summary>
    /// สลับทิศทางการพิมพ์ของเครื่อง MK ตัวนั้น ระหว่างปกติกับกลับหัว 180 องศา
    /// แล้วบันทึกลงฐานข้อมูล
    ///
    /// <para>
    /// ค่านี้ถูกใส่ไปในคำสั่ง FM ตอนส่งเข้าเครื่อง และคนกดส่งคือหน้า Order List
    /// ซึ่งอ่าน pattern ใหม่จาก backend จึงต้องบันทึกจริง เดิมเก็บไว้แค่ในหน้านี้
    /// ตอนที่ปุ่มเริ่มงานยังอยู่ที่นี่ พอย้ายปุ่มไปหน้า Order List แล้วการกดปุ่มนี้
    /// ก็ไม่มีผลกับงานที่ส่งออกไปอีกเลย
    /// </para>
    /// <para>
    /// บันทึกไม่ผ่านก็พลิกกลับทันที รูป ABC บนจอจะได้ตรงกับทิศทางที่จะถูกส่งจริง
    /// — พิมพ์กลับหัวผิดคืองานเสียทั้งล็อต
    /// </para>
    /// </summary>
    private async Task ToggleAbcAsync(int ordinal, PictureBox box)
    {
        if (_pattern == null || _savingPattern) return;

        var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
        if (config == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบ InkjetConfig ordinal {ordinal}");
            return;
        }

        FlipDirection(config, box);

        var error = await SavePatternAsync();
        if (error == null) return;

        FlipDirection(config, box);
        Notify.ErrorModal(this, "สลับทิศทางพิมพ์ไม่สำเร็จ",
            $"ยังไม่ได้บันทึกลงฐานข้อมูล จอจึงถูกปรับกลับเป็นค่าเดิม\n\n{error}");
    }

    private static void FlipDirection(InkjetConfigDto config, PictureBox box)
    {
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
    private void ApplyMarkingMethodButtons()
    {
        var plan = MarkingMethodService.Resolve(_markingMethod);

        _sendSteps = plan.NoCase ? [] : new List<string>(plan.Steps);
        _currentStep = 0;

        RefreshFlowRows();
        ApplyStepButtons();
    }

    /// <summary>
    /// คำนวณบรรทัด Plate / Shim ใหม่ทั้งสองบรรทัด รวมถึงรูปอ้างอิงของแต่ละด้าน
    ///
    /// แยกออกมาจาก <see cref="ApplyMarkingMethodButtons"/> เพราะต้องเรียกซ้ำได้
    /// ทุกครั้งที่ผู้ใช้เลือกรุ่นย่อยใหม่ โดยไม่ไปรีเซ็ตลำดับขั้นตอนที่ส่งไปแล้ว
    ///
    /// ชื่อรูปฝั่ง UV มาจากรุ่นที่เลือกไว้ ไม่ใช่ชื่อฐาน — เปลี่ยนรุ่นเมื่อไหร่รูปจึงตามทันที
    /// ส่วนฝั่ง MK ประกอบจาก ERP ซึ่งไม่เปลี่ยนตามการเลือก
    /// </summary>
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

    /// <summary>โปรแกรมของเครื่อง UV เครื่องหนึ่ง — เลือกแล้วหรือยังเป็นชื่อฐาน</summary>
    private UvProgramInfo UvProgramOf(string machine)
    {
        if (_chosenUvProgram.TryGetValue(machine, out var chosen))
            return new UvProgramInfo(chosen, Confirmed: true);

        var baseName = _uvData.FirstOrDefault(r => r.Machine == machine)?.ProgramName;
        return new UvProgramInfo(baseName, Confirmed: false);
    }

    /// <summary>
    /// ชื่อรูปกับ path ของรูปที่บรรทัดหนึ่งถืออยู่
    ///
    /// เก็บ path ไปด้วยแทนที่จะเก็บแค่ชื่อแล้วค่อยไปค้นตอนกด เพราะการค้นใหม่จากชื่อ
    /// คือทางที่ทำให้รูปไม่ตรงกับที่บรรทัดบอก ถ้าชื่อถูกเปลี่ยนระหว่างนั้น
    /// </summary>
    private sealed record FlowRef(string Name, List<string> Images);

    /// <summary>
    /// หนึ่งบรรทัดของ marking method — "Plate - MK - (P-ABC123)"
    ///
    /// ชื่อรูปอ้างอิงฝั่ง MK เคยเป็นช่องกรอกแยกอยู่ใน MK Section แล้วต้อง
    /// เอาเมาส์ไปจ่อถึงจะเห็นรูป ตอนนี้ต่อท้ายบรรทัดที่บอกอยู่แล้วว่าด้านนี้ใครมาร์ก
    /// และมีไอคอนรูปกำกับว่ากดได้ — บรรทัดที่ไม่มีรูปจะไม่มีไอคอนและกดไม่ได้
    /// </summary>
    private static void ApplyFlowRow(
        AntdUI.Button row, string side, MarkingMachine machine,
        MarkingRefSide? reference, string suffix)
    {
        var refName = reference?.LookupName;
        bool hasRef = !string.IsNullOrEmpty(refName) && refName != Dash;

        row.Text = $"{side} - {MachineLabel(machine)}"
            + (hasRef ? $" - ({refName})" : "")
            + suffix;

        // Tag พาทั้งชื่อและรูปของรอบนี้ไปให้ตัวจัดการคลิก บรรทัดไหนไม่มีรูปก็ไม่มี Tag
        row.Tag = hasRef ? new FlowRef(refName!, reference!.Images) : null;

        // บรรทัดที่กดไม่ได้ถอดกรอบกับพื้นออกให้เหลือเป็นข้อความเปล่า ๆ ไม่ใช้ Enabled
        // เพราะปุ่มที่ถูก disable จะจางลงทั้งบรรทัด ทั้งที่ "Plate - UV1" เป็นข้อมูล
        // ที่ต้องอ่านออกเท่า ๆ กับบรรทัดที่กดได้
        row.IconSvg = hasRef ? "PictureOutlined" : null;
        row.BorderWidth = hasRef ? 1F : 0F;
        row.DefaultBack = hasRef ? System.Drawing.Color.FromArgb(237, 243, 249) : Color.Transparent;
        row.Cursor = hasRef ? Cursors.Hand : Cursors.Default;
    }

    /// <summary>
    /// เปิดรูปชุดที่บรรทัดนี้ถืออยู่ — เป็นชุดเดียวกับที่ <see cref="RefreshFlowRows"/>
    /// คำนวณไว้ล่าสุด จึงตรงกับชื่อที่บรรทัดแสดงเสมอ ไม่ว่าผู้ใช้จะเปลี่ยนรุ่นย่อยกี่รอบ
    /// </summary>
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

    /// <summary>งานนี้มีขั้นตอนนี้อยู่ในแผนไหม — ชื่อขั้นมาจาก MarkingMethodService</summary>
    private bool HasStep(string step) =>
        _sendSteps.Any(s => string.Equals(s, step, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// ขั้นถัดไปที่ต้องให้ ST1 เป็นคนส่งเข้าเครื่อง — null เมื่องานนี้ไม่เข้าเงื่อนไข
    ///
    /// เงื่อนไขเดียวกับที่ปุ่มกดหน้างานใช้เลือกงาน: งานเดินอยู่ · ขั้นแรกส่งไปแล้ว ·
    /// ยังมีขั้นเหลือ ขั้นแรกของงานเป็นหน้าที่ของปุ่มเริ่มงานหน้า Order List ไม่ใช่ปุ่มนี้
    /// </summary>
    private string? NextRemoteStep()
    {
        if (_currentStep <= 0 || _currentStep >= _sendSteps.Count) return null;
        if (!string.Equals(_jobStatus, "Process", StringComparison.OrdinalIgnoreCase)) return null;

        return _sendSteps[_currentStep];
    }

    /// <summary>
    /// ทางสำรองของปุ่มกดหน้างาน สำหรับตอนปุ่มกดหรือ PLC ใช้ไม่ได้
    ///
    /// ปิดหน้านี้ก่อนแล้วให้หน้า Order List เป็นคนยิงคำขอ วงกลมหมุนกับกล่องยืนยัน
    /// จะได้อยู่บนหน้าที่ไม่มีอะไรบัง ไม่ใช่โผล่อยู่หลังกล่อง Order Detail
    /// </summary>
    private void RequestRemoteStart()
    {
        if (NextRemoteStep() is not string step) return;

        RemoteStartRequested?.Invoke(this, step);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyStepButtons()
    {
        // ปุ่มสำรองของปุ่มกดหน้างาน — ปิดไว้เป็นค่าเริ่มต้น เปิดที่ Setting → ตัวเลือกหน้างาน
        // ซึ่งเห็นเฉพาะโหมดทดสอบ คนคุมเครื่องจึงเปิดเองไม่ได้
        //
        // โชว์เฉพาะเครื่องของ ST3 เพราะขั้นที่สองของงานสองสถานีเป็นของ ST3 ที่เดียว
        // (เปิดให้โหมดทดสอบเห็นด้วย ไว้ลองก่อนเอาไปเปิดใช้จริงที่หน้างาน)
        var remoteStep = NextRemoteStep();

        // เก็บเป็นตัวแปรแล้วใช้ค่านั้นทั้งสองที่ ห้ามอ่าน .Visible กลับมาใช้ต่อ
        //
        // ตัวอ่านของ Control.Visible คืน false ถ้าพ่อแม่ชั้นไหนยังไม่ได้ถูกแสดง
        // ไม่ใช่ค่าที่เพิ่งเซ็ตลงไป และหน้านี้ถูกเติมข้อมูลตั้งแต่ก่อนกล่องจะ ShowDialog
        // (OrderDetailDialog.LoadDetail มาก่อน dlg.ShowDialog เสมอ) ผลคือ Enabled
        // ถูกตั้งเป็น false ค้างไว้ พอกล่องเปิดขึ้นมาปุ่มจึงโผล่มาแบบกดไม่ได้
        bool showRemote = remoteStep != null
            && StationService.ManualRemoteSendEnabled
            && (StationService.IsSt3 || _isDevMode);

        btnRemoteSend.Visible = showRemote;
        btnRemoteSend.Enabled = showRemote;
        if (remoteStep != null) btnRemoteSend.Text = $"ขอให้ ST1 ส่ง {remoteStep}";

        // ปุ่มส่งมือเหลือไว้เฉพาะโหมดทดสอบ
        //
        // การส่งงานจริงเป็นหน้าที่ของปุ่มเริ่มงานหน้า Order List กับปุ่มกดหน้างาน
        // ซึ่งเดินตามลำดับขั้นของ marking method ให้เอง ปุ่มพวกนี้กดข้ามลำดับได้
        // เปิดไว้ในโหมดใช้งานปกติจึงเสี่ยงที่จะส่งซ้ำหรือส่งข้ามขั้น แล้วพ่นซ้ำ
        // ลงชิ้นงานจริง
        //
        // โหมดทดสอบโชว์ครบทุกเครื่อง ไม่ดูว่างานนี้มีขั้นนั้นอยู่ในแผนไหม
        //
        // เพราะจุดประสงค์ของปุ่มชุดนี้คือยิงข้อความไปหาเครื่องเพื่อดูว่าเครื่องรับไหม
        // ไม่ใช่การเดินงานตามแผน คนทดสอบจึงต้องเลือกเครื่องไหนก็ได้จากงานใบเดียว
        // ปุ่มทดสอบส่ง PLC ก็อยู่ในชุดเดียวกัน — designer ซ่อนไว้เป็นค่าตั้งต้น
        // ที่นี่คือที่เดียวที่เปิดให้เห็น
        btnTestPlc.Visible = _isDevMode;
        btnSendMk.Visible = _isDevMode;
        btnSendUv1.Visible = _isDevMode;
        btnSendUv2.Visible = _isDevMode;

        if (_isDevMode)
        {
            btnTestPlc.Enabled = true;
            btnSendMk.Enabled = true;
            btnSendUv1.Enabled = true;
            btnSendUv2.Enabled = true;
            return;
        }

        btnTestPlc.Enabled = false;
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

    /// <summary>
    /// <paramref name="detail"/> เก็บลง payload ของ command — ฝั่ง UV ใช้บันทึกว่า
    /// พิมพ์ด้วยรุ่นย่อยไหนจริง ไม่งั้นย้อนดูทีหลังไม่รู้ว่าเป็น ABC-1 หรือ ABC-2
    /// </summary>
    private void CompleteSendStep(string stepName, object? detail = null)
    {
        // โหมดทดสอบไม่แตะประวัติและไม่แตะสถานะงาน ออกตรงนี้ก่อนทุกอย่าง
        //
        // ปุ่มชุดนั้นมีไว้ยิงข้อความหาเครื่องอย่างเดียว การบันทึกว่า "ขั้นนี้ส่งแล้ว"
        // จะทำให้ปุ่มเริ่มงานกับปุ่มกดหน้างานข้ามขั้นนั้นไป ทั้งที่ยังไม่ได้พิมพ์จริง
        // และงานที่เอามาลองก็จะเปลี่ยนสถานะไปเองโดยไม่มีใครสั่ง
        if (_isDevMode) return;

        // บันทึกก่อนเสมอ ก่อนเช็คลำดับขั้นตอนใด ๆ — มาถึงบรรทัดนี้คือส่งเข้าเครื่อง
        // สำเร็จไปแล้วจริง ต้องมีร่องรอยไว้เสมอ
        //
        // เดิมสองบรรทัดเช็คลำดับข้างล่างคืนค่าออกไปก่อนถึงการบันทึก ทำให้การส่งซ้ำ
        // (ส่ง UV2 ไปแล้ว แล้วเลือกรุ่นย่อยใหม่ส่งอีกรอบ) ไม่ถูกบันทึกเลย
        // เปิด Order Detail ใหม่จึงเห็นรุ่นเก่า ไม่ใช่รุ่นที่เพิ่งเลือกและพิมพ์จริง
        _ = _api?.SaveSendStepAsync(_jobId, stepName, detail);

        // ที่เหลือคือการเดินสถานะปุ่มตามลำดับขั้นตอน — ส่งซ้ำหรือส่งข้ามลำดับ
        // ไม่ควรเลื่อนลำดับ จึงยังคงเงื่อนไขเดิมไว้ตรงนี้
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

            // ปลดล็อกปุ่มสั่งแคลมป์ทันทีโดยไม่ต้องปิดเปิดหน้าใหม่ — งานเริ่มไปแล้วจริง
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

        btnSendMk.Enabled = false;
        var originalText = btnSendMk.Text;
        btnSendMk.Text = "กำลังส่ง...";

        try
        {
            // ใช้ตัวส่งชุดเดียวกับปุ่มเริ่มงานที่หน้า Order List — เดิมหน้านี้มีโค้ดส่ง
            // ของตัวเองอีกชุด แก้กฎการส่งทีหนึ่งต้องไล่แก้สองที่ และพลาดไปแล้วหนึ่งรอบ
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
            // สำเร็จในโหมดใช้งานจริง CompleteSendStep จะ MarkButtonSent ให้เอง
            // นอกนั้นคืนปุ่มกลับสภาพเดิมเสมอ — โหมดทดสอบไม่มีใครมาปลดปุ่มให้
            // ถ้าไม่คืนตรงนี้ ปุ่มจะค้างอยู่ที่ "กำลังส่ง..." แบบกดไม่ได้ตลอด
            if (!IsDisposed && btnSendMk.Text?.StartsWith('✓') != true)
            {
                btnSendMk.Text = originalText;
                btnSendMk.Enabled = true;
            }
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
            // ปุ่มนี้มีเฉพาะโหมดทดสอบ แต่ก็ต้องกันไฟสถานะไม่ให้แย่งซ็อกเก็ตเหมือนกัน
            // จองตรงนี้ ไม่ใช่ตั้งแต่ต้นฟังก์ชัน เพราะข้างบนมีกล่องเลือกรุ่นย่อยที่ค้างรอคนได้นาน
            using var busy = MachineBusy.Hold();

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

            // จำรุ่นที่เพิ่งเลือก แล้ววาดบรรทัด Plate / Shim ใหม่ทั้งสองบรรทัด —
            // รูปอ้างอิงของด้านนี้จะเปลี่ยนตามรุ่นใหม่ทันที ไม่ค้างรุ่นเดิม
            _chosenUvProgram[stepName] = programFile;
            RefreshFlowRows();

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

        // Trigger Delay กับสายพาน 2/3 ไม่ได้ส่งเข้า PLC แล้ว จึงไม่ติดป้าย address ให้
        // ไม่งั้นจะขึ้นว่า "ยังไม่ได้ map" ค้างอยู่ ทั้งที่ตั้งใจให้ไม่มี map
        TagAddress(lblMk1PosAct, plan, $"{mk1} PostAct");
        TagAddress(lblMk1Delay, plan, $"{mk1} Delay");
        TagAddress(lblMk2PosAct, plan, $"{mk2} PostAct");
        TagAddress(lblMk2Delay, plan, $"{mk2} Delay");
        TagAddress(lblConveyor1, plan, "Conveyor Speed 1");
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
                .Select(b =>
                {
                    if (b.Error != null) return Notify.Bad($"{b.Name} — {b.Error}");

                    // อ่านกลับไม่ตรงกับที่เขียนคือค่าไม่เข้า ต้องขึ้นเป็นคำเตือน
                    // ไม่ใช่รายงานว่าสำเร็จ เพราะคำสั่งผ่านแต่ผลไม่ได้ตามนั้น
                    if (b.ReadBack == null)
                        return Notify.Careful($"{b.Name} = {b.Value} (อ่านกลับไม่ได้)");

                    return b.ReadBack == b.Value
                        ? Notify.Ok($"{b.Name} = {b.Value}")
                        : Notify.Careful($"{b.Name} = {b.Value} · อ่านกลับได้ {b.ReadBack}");
                })
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

        _jobLabel = Services.JobDisplay.Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id);

        txtJobErpMfg.Text = OrDash(job.OrderNo);
        txtJobLotNo.Text = OrDash(job.BarcodeRaw);
        txtJobCustomer.Text = OrDash(job.CustomerName);
        txtJobQty.Text = job.Qty?.ToString() ?? Dash;
        // ทั้งคำและสีมาจาก JobStatusDisplay ที่เดียวกับคอลัมน์ Status ในตาราง
        // Order List — backend เก็บเป็น Process / Success แต่บนจอเรียก Working /
        // Finished ทั้งสองหน้า ไม่งั้นงานเดียวกันดูสองหน้าแล้วเหมือนคนละสถานะ
        var jobStatus = Theme.JobStatusDisplay.Resolve(
            job.Status,
            MarkingMethodService.FinishedIncomplete(
                job.Status, resolved.PlanRouting?.MarkingMethod, resolved.Commands));
        txtJobStatus.Text = OrDash(jobStatus.Text);
        txtJobStatus.ForeColor = jobStatus.Fore;
        _baseStatusText = jobStatus.Text;

        // ต่อท้ายว่างานรออะไรอยู่ ต้องถามคิวจาก backend จึงทำแยกไม่ให้หน่วงการเปิดหน้า
        _ = ShowStageAsync(job.Id);

        var marking = resolved.PlanRouting?.MarkingMethod;
        txtMarkingMethod.Text = string.IsNullOrWhiteSpace(marking) ? "ไม่ระบุ" : marking;
    }

    /// <summary>คำสถานะล้วน ๆ ก่อนต่อวงเล็บ — เก็บไว้เพื่อไม่ต่อซ้ำทับของเดิม</summary>
    private string _baseStatusText = "";

    /// <summary>
    /// ต่อท้ายสถานะว่างานนี้กำลังทำด้านไหน หรือรออยู่คิวที่เท่าไร
    ///
    /// <para>
    /// เช่น <c>Working (Mark Plate)</c> · <c>Working (Mark Shim)</c> · <c>Waiting (Q2)</c>
    /// กติกาอยู่ที่ <see cref="Services.JobStageService"/> ที่เดียว หน้า Order List
    /// โชว์คำสถานะคำเดียวล้วน ๆ ไม่มีวงเล็บ
    /// </para>
    /// <para>
    /// ถามคิวไม่ได้ก็ปล่อยคำเดิมไว้ ไม่ต้องฟ้อง — วงเล็บเป็นข้อมูลเสริม ไม่ใช่สิ่งที่
    /// ขาดแล้วอ่านหน้านี้ไม่รู้เรื่อง
    /// </para>
    /// </summary>
    private async Task ShowStageAsync(int jobId)
    {
        if (_api == null) return;

        var (rows, error) = await _api.GetMachineQueueAsync();
        if (error != null || IsDisposed) return;

        // งานเปลี่ยนไปแล้วระหว่างรอคำตอบ อย่าเขียนทับของงานใบใหม่
        if (jobId != _jobId) return;

        var stage = Services.JobStageService.Describe(jobId, _markingMethod, rows);
        if (stage == null || _baseStatusText.Length == 0) return;

        txtJobStatus.Text = $"{_baseStatusText} ({stage})";
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

    /// <summary>
    /// รุ่นย่อยที่ผู้ใช้เลือกไว้แล้วแต่ยังไม่ได้ส่งเข้าเครื่อง
    ///
    /// เกิดตอน ST3 กดเริ่มงาน: ผู้ใช้เลือกรุ่นย่อยที่จอ ST3 แล้วฝากคำขอไว้ให้ ST1 ส่งแทน
    /// ค่าที่เลือกถูกเก็บไว้ที่ <c>print_jobs.remote_program</c> ระหว่างรอ ถ้าไม่เอามาโชว์
    /// หน้าจอจะยังบอกชื่อฐาน ทั้งที่ผู้ใช้เพิ่งเลือกรุ่นย่อยไปกับมือ
    ///
    /// พอ ST1 ส่งสำเร็จ payload ของ command จะมีค่านี้แล้ว และ backend ล้าง
    /// remote_program ทิ้ง — ลำดับการหาค่าจึงยังถูกต้องทุกช่วงเวลา
    /// </summary>
    private static string? PendingProgram(ResolvedJobResponse resolved, string machine)
    {
        var pending = resolved.Job.RemoteProgram?.Trim();
        if (string.IsNullOrEmpty(pending)) return null;

        // remote_program มีค่าเดียวต่องาน จึงต้องรู้ว่าเป็นของเครื่องไหน —
        // ขั้นตอนแรกตาม marking method คือขั้นที่ ST3 ฝากให้ ST1 ส่ง
        var step = MarkingMethodService
            .Resolve(resolved.PlanRouting?.MarkingMethod)
            .Steps.FirstOrDefault();

        return string.Equals(step, machine, StringComparison.OrdinalIgnoreCase) ? pending : null;
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

    /// <summary>
    /// สั่งแคลมป์ได้เฉพาะงานที่เริ่มไปแล้ว — แกนจะวิ่งจริงตอนกด ถ้างานยังไม่เริ่ม
    /// แปลว่าชิ้นงานยังไม่อยู่ที่เครื่อง การสั่งแกนวิ่งตอนนั้นคือสั่งลงบนงานของคนอื่น
    /// ที่ค้างอยู่ หรือสั่งลงบนที่ว่าง
    /// <para>
    /// โหมดทดสอบหน้างาน (MENU_LEVEL 99) และหน้าที่เปิดโดยไม่มี job จริงไม่โดนกัน
    /// เพราะสองกรณีนั้นตั้งใจใช้สั่งแกนโดยไม่มีงานอยู่แล้ว
    /// </para>
    /// </summary>
    /// <summary>ชื่องานสำหรับข้อความ — เผื่อเรียกก่อนที่ข้อมูลงานจะโหลดเสร็จ</summary>
    private string JobText() => _jobLabel.Length > 0 ? _jobLabel : $"#{_jobId}";

    private bool CanCommandIai()
    {
        if (_isDevMode || _jobId <= 0) return true;

        if (string.Equals(_jobStatus, "Process", StringComparison.OrdinalIgnoreCase))
            return true;

        Notify.WarnModal(this, "ยังสั่งแคลมป์ไม่ได้",
            $"{JobText()} ยังไม่ได้เริ่มงาน (สถานะ {JobStatusDisplay.Text(_jobStatus)})\n\n"
            + "กดเริ่มงานที่หน้ารายการงานก่อน จึงจะสั่งค่า IAI ได้");
        return false;
    }

    private async Task IaiSendAsync(AntdUI.Input input, bool isPlate, string? zone)
    {
        if (!CanCommandIai()) return;

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
        // กันด้วยกฎเดียวกับปุ่ม Send — ค่านี้เขียนทับระยะแคลมป์ที่ผูกกับงาน
        // และ backend ก็ปฏิเสธงานที่ยังไม่เริ่มอยู่แล้ว ดักที่นี่เพื่อบอกสาเหตุให้ตรง
        if (!CanCommandIai()) return;

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

        // จำค่าที่วาดไว้ เพื่อให้ตอนบันทึกรู้ว่าแถวไหนถูกแก้จริง ไม่ใช่เขียนทับทั้งห้าช่อง
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
