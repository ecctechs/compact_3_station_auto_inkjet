using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    private static readonly string[] ActiveStatuses = ["Waiting", "Process"];

    private static readonly string[] HistoryStatuses = ["Success", "Cancel"];

    private ApiClient? _api;
    private System.Windows.Forms.Timer? _pollTimer;

    private readonly PushButtonWatcher _pushButton = new();

    private readonly HashSet<string> _pushHandling = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Guid> _dispatchingMachines = new(StringComparer.OrdinalIgnoreCase);
    private bool _preparingPrograms;
    private readonly List<Notify.ResultLine> _sendReports = [];
    private bool _showingSendReport;

    private bool _refreshing;
    private bool _refreshRequested;
    private bool _refreshScheduled;
    private long _lastBlockedPressNotice = long.MinValue;
    private readonly HashSet<int> _reportedUncertainQueues = [];

    private bool _rowBusy;

    private const int SlowLoadMs = 250;
    private bool _showHistory;
    private List<PrintJob> _allJobs = new();
    private string _lastSignature = "";

    public OrderListUserControl()
    {
        InitializeComponent();

        ConfigureColumns();
        SetupEvents();
    }

    private void ConfigureColumns()
    {
        tblOrders.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("Start", "Start", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("End", "End", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("ErpMfg", "ERP MFG", AntdUI.ColumnAlign.Center) { Width = "12%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("LotNo", "Lot Number", AntdUI.ColumnAlign.Center) { Width = "12%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("ProcessSequence", "Process Sequence", AntdUI.ColumnAlign.Center) { Width = "13%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Plate", "Plate", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Shim", "Shim", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Station", "Station", AntdUI.ColumnAlign.Center) { Width = "7%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Status", "Status", AntdUI.ColumnAlign.Center) { Width = "8%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "12%" },
            machineStatusColumn,
        };

        tblOrders.SortOrderSize = 12;

        ApplyTabColumns(showHistory: false);
    }

    private static readonly (string Key, string ListWidth, string HistoryWidth)[] TabColumnWidths =
    [
        ("ErpMfg", "14%", "12%"),
        ("LotNo", "14%", "12%"),
        ("Qty", "7%", "6%"),
        ("ProcessSequence", "15%", "13%"),
        ("Plate", "7%", "6%"),
        ("Shim", "7%", "6%"),
    ];

    private void ApplyTabColumns(bool showHistory)
    {
        if (tblOrders.Columns == null) return;

        foreach (var column in tblOrders.Columns)
        {
            if (column.Key == "End") column.Visible = showHistory;

            foreach (var (key, list, history) in TabColumnWidths)
            {
                if (column.Key != key) continue;
                column.Width = showHistory ? history : list;
                break;
            }
        }
    }

    private void SetupEvents() // ผูกปุ่มและเหตุการณ์ของหน้า Order List
    {
        btnTabList.Click += (_, _) => SwitchTab(false); // เปิดรายการงานที่ยังไม่จบ
        btnTabOnline.Click += (_, _) => SwitchTab(false, JobProcessService.Online); // กรองเฉพาะงาน Online
        btnTabOffline.Click += (_, _) => SwitchTab(false, JobProcessService.Offline); // กรองเฉพาะงาน Offline
        btnTabHistory.Click += (_, _) => SwitchTab(true); // เปิดประวัติงานที่จบหรือยกเลิก
        ApplyProcessTabs(); // แสดงแท็บตามค่าของ Station

        btnTabHistory.Visible = !StationService.IsSt3; // ซ่อน History ที่ ST3
        tblOrders.CellButtonClick += TblOrders_CellButtonClick; // รับปุ่มเริ่ม จบ และรายละเอียดในแต่ละแถว

        tblOrders.CustomSort += CompareCellText; // เรียงวันที่ตามเวลา ไม่เทียบเป็นข้อความ
        tblOrders.SetRowStyle += TblOrders_SetRowStyle; // ผูกการกำหนดรูปแบบของแต่ละแถว

        dtpHistoryRange.ValueChanged += async (_, _) => await RefreshDataAsync(force: true); // เปลี่ยนช่วงวันแล้วอ่าน History ใหม่
        btnSearchDate.Click += async (_, _) => await RefreshDataAsync(force: true); // ค้นประวัติตามช่วงวันที่เลือก
        btnClearDate.Click += (_, _) => dtpHistoryRange.Value = null; // ล้างตัวกรองวันที่

        WirePanels(); // ผูกส่วนแสดงเครื่องและคิว

        WirePushButton(); // ผูกปุ่มหน้างานกับการปล่อยคิว

        Load += OnLoad; // เปิดหน้าแล้วเริ่มอ่านข้อมูล
        Disposed += OnDisposed; // ปิดหน้าแล้วหยุดงานเบื้องหลัง
    }

    private void OnLoad(object? sender, EventArgs e) // เริ่มอ่านงานและฟังปุ่มเมื่อเปิดหน้า
    {
        _api = new ApiClient($"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000"); // ชี้ไป Backend กลางตาม IP ที่ตั้งไว้
        _ = RefreshDataAsync(); // อ่านรายการงานทันทีที่เปิดหน้า
        StartPolling(); // ตั้งรอบอ่านงานจาก Backend
        _pushButton.Start(); // เริ่มฟังปุ่มหน้างานจาก PLC
        WarmUpDetailDialog(); // เตรียม Detail ไว้ก่อนผู้ใช้กดเปิด
    }

    private static bool _detailWarmedUp;

    private void WarmUpDetailDialog() // เตรียมหน้า Detail ให้เปิดครั้งแรกเร็วขึ้น
    {
        if (_detailWarmedUp) return; // เตรียมไว้แล้ว ไม่ต้องทำซ้ำ
        _detailWarmedUp = true; // จำว่าเตรียมหน้า Detail แล้ว

        var warmUp = new System.Windows.Forms.Timer { Interval = 1000 }; // ตั้งเวลาให้รอ 1 วินาที
        warmUp.Tick += (_, _) => // ครบเวลาแล้วเตรียมหน้าต่าง
        {
            warmUp.Stop(); // หยุดเวลา ให้ทำแค่ครั้งเดียว
            warmUp.Dispose(); // เลิกใช้ตัวจับเวลา

            try { using var throwaway = new OrderDetailDialog(); } // สร้างแล้วทิ้ง โดยไม่แสดงหน้าต่าง
            catch { /* ignore */ } // เตรียมไม่ผ่าน ยังเปิด Detail ทีหลังได้
        };
        warmUp.Start(); // เริ่มนับเวลา
    }

    private void OnDisposed(object? sender, EventArgs e) // หยุดตัวอ่านงานและคืนทรัพยากรเมื่อปิดหน้า
    {
        _pollTimer?.Stop(); // หยุดรอบอ่าน Backend
        _pollTimer?.Dispose(); // เลิกใช้ตัวจับเวลารายการงาน
        _pushButton.Dispose(); // หยุดฟังปุ่ม PLC
        DisposePanelImages(); // คืนรูปที่ใช้ในส่วนแสดงเครื่อง
    }

    private void WirePushButton() // ผูกปุ่มจริงและปุ่มจำลองเข้ากับเครื่อง
    {
        _pushButton.ShouldWatch = () => Visible; // อ่านปุ่มเมื่อหน้า Order List แสดงอยู่

        _pushButton.CanAct = CanReleaseNow; // ตรวจว่าเครื่องนี้ปล่อยคิวได้หรือยัง

        _pushButton.Pressed += async (_, machine) => await OnPushButtonPressedAsync(machine); // กดปุ่มแล้วปล่อยคิวของเครื่องนั้น
        _pushButton.BlockedPress += (_, _) => ShowBlockedPress(); // ถ้ายังปล่อยไม่ได้ ให้แจ้งกดใหม่

        bool dev = StationService.IsDevMode; // ปุ่มจำลองใช้เฉพาะโหมดทดสอบ

        btnSimDelayMk.Visible = dev; // แสดงปุ่มจำลอง MK เฉพาะโหมดทดสอบ
        btnSimDelayUv1.Visible = dev; // แสดงปุ่มจำลอง UV1 เฉพาะโหมดทดสอบ
        btnSimDelayUv2.Visible = dev; // แสดงปุ่มจำลอง UV2 เฉพาะโหมดทดสอบ

        btnSimDelayMk.Click += (_, _) => StartDelayedPushTest("MK"); // จำลองปุ่ม MK หลังครบเวลาหน่วง
        btnSimDelayUv1.Click += (_, _) => StartDelayedPushTest("UV1"); // จำลองปุ่ม UV1 หลังครบเวลาหน่วง
        btnSimDelayUv2.Click += (_, _) => StartDelayedPushTest("UV2"); // จำลองปุ่ม UV2 หลังครบเวลาหน่วง

        btnSimPushMk.Visible = dev; // แสดงปุ่มจำลอง MK เฉพาะโหมดทดสอบ
        btnSimPushUv1.Visible = dev; // แสดงปุ่มจำลอง UV1 เฉพาะโหมดทดสอบ
        btnSimPushUv2.Visible = dev; // แสดงปุ่มจำลอง UV2 เฉพาะโหมดทดสอบ

        btnSimPushMk.Click += async (_, _) => await OnPushButtonPressedAsync("MK"); // จำลองกดปุ่ม MK ทันที
        btnSimPushUv1.Click += async (_, _) => await OnPushButtonPressedAsync("UV1"); // จำลองกดปุ่ม UV1 ทันที
        btnSimPushUv2.Click += async (_, _) => await OnPushButtonPressedAsync("UV2"); // จำลองกดปุ่ม UV2 ทันที

        _pushButton.Trouble += (_, error) => // รับเหตุอ่านปุ่มมีปัญหาหรือกลับมาใช้ได้
        {
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            if (error == null) Notify.Success(this, "ปุ่มกดหน้างาน — กลับมาอ่านค่าได้แล้ว"); // แจ้งว่า PLC กลับมาอ่านปุ่มได้แล้ว
            else Notify.Warn(this, $"ปุ่มกดหน้างาน — อ่านค่าจาก PLC ไม่ได้ ({error})"); // แสดงเหตุที่อ่านปุ่มจาก PLC ไม่ได้
        };
    }

    private static bool AlreadySent(PrintJob job, string step) =>
        job.Commands?.Any(c => c.Success &&
            string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    private const int DelayedPushSeconds = 5;

    private void StartDelayedPushTest(string machine)
    {
        Notify.Success(this, $"จะจำลองการกดปุ่ม {machine} ในอีก {DelayedPushSeconds} วินาที");

        var timer = new System.Windows.Forms.Timer { Interval = DelayedPushSeconds * 1000 };
        timer.Tick += async (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            if (IsDisposed) return;

            if (!CanReleaseNow(machine))
            {
                ShowBlockedPress();
                return;
            }

            await OnPushButtonPressedAsync(machine);
        };
        timer.Start();
    }

    private bool CanReleaseNow(string machine) => // ตรวจว่าเครื่องนี้ไม่มีงานส่งหรือปล่อยคิวค้างอยู่
        !_pushHandling.Contains(machine) && !_dispatchingMachines.ContainsKey(machine) && !MachineBusy.IsBusy(machine); // ต้องไม่มีทั้งการปล่อยคิวและการส่งของเครื่องนี้

    private void ShowBlockedPress()
    {
        long now = Environment.TickCount64;
        if (_lastBlockedPressNotice != long.MinValue && now - _lastBlockedPressNotice < 5000) return;
        _lastBlockedPressNotice = now;
        Notify.Warn(this, "มีคนกดปุ่มหน้างาน — ระบบกำลังทำงานอื่นอยู่ กรุณากดอีกครั้ง");
    }

    private async Task OnPushButtonPressedAsync(string machine) // ปล่อยเครื่องแล้วให้ ST1 รับคิวถัดไป
    {
        if (_api == null || IsDisposed) return; // หยุดเมื่อไม่มี Backend หรือหน้าถูกปิด
        if (!CanReleaseNow(machine)) return; // เครื่องนี้ยังทำรายการอยู่ ไม่รับการกดซ้ำ
        _pushHandling.Add(machine); // ล็อกการกดซ้ำเฉพาะเครื่องนี้
        try // ดักข้อผิดพลาดของขั้นนี้
        {

            var (queue, queueError) = await _api.GetMachineQueueAsync(); // อ่านคิวล่าสุดก่อนปล่อยเครื่อง
            if (queueError != null) // อ่านคิวไม่ได้ จึงไม่เดาว่าจะปล่อยงานไหน
            {
                Notify.Warn(this, $"อ่านคิว {machine} ไม่สำเร็จ — {queueError}"); // แจ้งเหตุอ่านคิวไม่ได้ก่อนหยุดปล่อย
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }
            var holder = queue.FirstOrDefault(r => r.Machine == machine && r.State == "active"); // อ่านว่าตอนนี้กำลังปล่อยคิวหมายเลขใด

            var (release, error) = await _api.ReleaseMachineAsync( // ขอปล่อยคิวปัจจุบันและรับคิวถัดไปจาก Backend
                machine, holder?.Id, StationService.HoldForNextRound); // ส่งเลขคิวเดิมไปกันกดซ้ำแล้วข้ามงานถัดไป
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            if (release == null) // Backend ไม่คืนผลการปล่อยเครื่อง
            {
                Notify.Warn(this, $"ปล่อยเครื่อง {machine} ไม่สำเร็จ — {error}"); // แจ้งเหตุที่ Backend ไม่ยอมปล่อยเครื่อง
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            if (release.Next != null) // มีงานถัดไปได้รับสิทธิ์ใช้เครื่องแล้ว
            {
                if (StationService.IsSt3) // ที่ ST3 ให้รอ ST1 เป็นผู้ส่ง
                    Notify.Success(this, $"{machine} เข้าคิวแล้ว · ST1 จะส่งงานถัดไปให้"); // บอกว่าต้องรอ ST1 ส่งคิวใหม่
                else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                    await ProcessMachineQueueAsync(machineFilter: machine); // ส่งคิวถัดไปของเครื่องที่เพิ่งปล่อยทันที
            }
            else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
            {
                Notify.Success(this, $"{machine} ว่างแล้ว · ไม่มีงานรอคิว"); // แจ้งว่าเครื่องไม่มีงานรอแล้ว
                await ResetHeadPositionAsync(machine); // ขอให้ PLC พาหัวพิมพ์กลับตำแหน่งเริ่มต้น
            }

        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            _pushHandling.Remove(machine); // เปิดรับปุ่มเครื่องนี้อีกครั้ง โดยไม่รอเครื่องอื่น
        }
        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านสถานะคิวใหม่หลังรับปุ่ม
    }

    private async Task RefreshStationBarAsync(List<MachineQueueRow>? snapshot = null)
    {
        if (_api == null || IsDisposed) return;

        var (rows, error) = snapshot == null
            ? await _api.GetMachineQueueAsync()
            : (snapshot, (string?)null);
        if (error != null || IsDisposed) return;

        _queueRows = rows;
        ReconcileMachineStatus(); // ใช้ผล Backend แทนข้อความชั่วคราวของคิวที่ส่งจบแล้ว
        UpdateMachineStatusCells();

        foreach (var (machine, label, queueLabel, button) in StationSlots())
        {
            var holder = rows.FirstOrDefault(r => r.Machine == machine && r.State == "active");

            var waiting = rows
                .Where(r => r.Machine == machine && r.State == "pending")
                .OrderBy(r => r.Id)
                .ToList();

            button.Enabled = holder != null || waiting.Count > 0;

            if (holder == null)
            {
                label.Text = $"● {machine} — ว่าง";
                label.ForeColor = waiting.Count > 0 ? WaitingColor : DesignTokens.SuccessText;
            }
            else
            {
                var what = holder.NeedsSendReview ? "กำลังส่ง / รอตรวจสอบผล"
                    : holder.SentAt == null ? "รอ ST1 ส่ง" : machine == "MK" ? "กำลังพิมพ์" : "ส่งข้อมูลแล้ว";

                label.Text = $"● {machine} — {JobName(holder.PrintJobsId)} · {what}";
                label.ForeColor = WaitingColor;
            }

            queueLabel.Text = QueueLine(waiting);
        }
    }

    private string QueueLine(List<MachineQueueRow> waiting)
    {
        if (waiting.Count == 0) return "ไม่มีคิวรอ";

        var next = $"คิวถัดไป: {JobName(waiting[0].PrintJobsId)}";
        return waiting.Count == 1 ? next : $"{next}   (รอทั้งหมด {waiting.Count} ใบ)";
    }

    private List<MachineQueueRow> _queueRows = [];

    private static readonly Color WaitingColor = Color.FromArgb(214, 108, 0);

    private IEnumerable<(string Machine, AntdUI.Label Label, AntdUI.Label Queue, AntdUI.Button Button)> StationSlots()
    {
        yield return ("MK", lblStationMk, lblQueueMk, btnSimPushMk);
        yield return ("UV1", lblStationUv1, lblQueueUv1, btnSimPushUv1);
        yield return ("UV2", lblStationUv2, lblQueueUv2, btnSimPushUv2);
    }

    private async Task ResetHeadPositionAsync(string machine) // คืนหัว MK กลับ Home เมื่อไม่มีคิวต่อ
    {
        if (!string.Equals(machine, "MK", StringComparison.OrdinalIgnoreCase)) return; // รีเซ็ตตำแหน่งเฉพาะ MK; UV1 / UV2 ข้ามขั้นนี้

        var results = await PlcOrderService.ResetPositionAsync(_api); // สั่ง PLC คืนตำแหน่งหัว MK ผ่านค่าในระบบ
        if (IsDisposed || results.Count == 0) return; // ไม่มีจอหรือไม่มีผล Home ให้รายงาน

        var failed = results // รวบรวมรายการคืน Home ที่มีปัญหา
            .Where(r => r.Error != null || r.ReadBack != r.Value) // เลือกทั้งข้อผิดพลาดและค่าที่อ่านกลับไม่ตรง
            .Select(r => r.Error != null // แยกเหตุคำสั่งผิดพลาดจากเหตุค่ากลับไม่ตรง
                ? $"{r.Name} {r.Error}" // ใช้เหตุผิดพลาดของ register นั้น
                : $"{r.Name} ส่ง {r.Value} อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}") // แสดงค่าที่ส่งเทียบกับค่าที่อ่านได้
            .ToList(); // เก็บผลที่กรองแล้วเป็นรายการ
        if (failed.Count == 0) return; // แจ้งเครื่องว่างไว้แล้ว ไม่ต้องซ้อนข้อความสำเร็จ

        Notify.Warn(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นไม่สำเร็จ — " + string.Join(" · ", failed)); // รวมเหตุที่หัว MK กลับ Home ไม่สำเร็จ
    }

    private static bool SentAlready(ResolvedJobResponse resolved, string step) =>
        resolved.Commands?.Any(c => c.Success &&
            string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    private const int PollNormalMs = 5000;

    private const int PollWhenDownMs = 30000;

    private int _pollFailures;

    private void StartPolling() // ตั้งรอบอ่านงานและคิวจาก Backend
    {
        _pollTimer = new System.Windows.Forms.Timer { Interval = PollNormalMs }; // สร้างตัวจับเวลาตามรอบอ่านปกติ
        _pollTimer.Tick += async (_, _) => // ครบเวลาแล้วตรวจงานรอบใหม่
        {
            if (_sending) await ProcessMachineQueueAsync(); // ยังส่งอยู่ ให้ตรวจคิวเครื่องอื่นที่พร้อม
            else await RefreshDataAsync(); // ถ้าไม่ได้ส่ง ให้อ่านรายการงานใหม่
        };
        _pollTimer.Start(); // เริ่มอ่านงานเป็นรอบ
    }

    private void NotePollResult(bool reached)
    {
        if (_pollTimer == null) return;

        if (reached)
        {
            _pollFailures = 0;
            if (_pollTimer.Interval != PollNormalMs) _pollTimer.Interval = PollNormalMs;
            return;
        }

        _pollFailures++;
        int wanted = Math.Min(PollNormalMs * 2 * _pollFailures, PollWhenDownMs);
        if (_pollTimer.Interval != wanted) _pollTimer.Interval = wanted;
    }

    private async Task RefreshDataAsync(bool force = false) // อ่าน Job และคิวล่าสุดมาแสดงบนจอ
    {
        if (_api == null || IsDisposed) return; // หยุดเมื่อไม่มี Backend หรือหน้าถูกปิด
        ApplyProcessTabs(); // ปรับแท็บ Online และ Offline ตามค่าล่าสุด
        _refreshRequested |= force; // จำคำขอรีเฟรชบังคับไว้ระหว่างรอ

        if (_sending) return; // ยังมีงานส่งอยู่ ให้รอรอบถัดไป

        if (_refreshing) return; // กำลังอ่านรอบก่อนอยู่ ไม่อ่านซ้อน

        force |= _refreshRequested; // รวมคำขอที่ค้างไว้เข้ากับรอบนี้
        _refreshRequested = false; // รับคำขอรีเฟรชที่ค้างมาทำแล้ว
        _refreshing = true; // ล็อกไม่ให้ตัวจับเวลาเริ่มอ่านซ้อน
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            DateTime? fromUtc = null, toUtc = null; // เริ่มจากไม่จำกัดช่วงวัน
            if (_showHistory && TryGetDateRange(out var from, out var to)) // History มีช่วงวันที่เลือกไว้
            {
                fromUtc = ToUtcFromThai(from); // แปลงวันเริ่มจากเวลาไทยเป็น UTC
                toUtc = ToUtcFromThai(to); // แปลงวันสิ้นสุดจากเวลาไทยเป็น UTC
            }

            var (jobs, error) = await _api.GetAllJobsAsync(100, fromUtc, toUtc); // ขอรายการ Job พร้อมตัวกรองวัน
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            NotePollResult(error == null); // บันทึกว่ารอบอ่าน Backend ผ่านหรือไม่
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง
            if (error != null) // มีรายละเอียดข้อผิดพลาดส่งกลับมา
            {
                tblOrders.EmptyText = $"Error: {error}"; // แสดงเหตุอ่านข้อมูลไม่ได้ในพื้นที่ตาราง
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }
            _allJobs = jobs; // เก็บ Job ชุดใหม่ไว้ใช้กรองบนจอ

            await RecoverAbandonedRemoteStartsAsync(); // ตรวจคำขอฝากส่งเก่าที่ค้างอยู่
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง

            await ProcessRemoteStartsAsync(); // ให้ ST1 จัดการคำขอส่งจาก ST3
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง

            var (queue, queueError) = await _api.GetMachineQueueAsync(); // อ่านคิวเครื่องล่าสุดประกอบรายการงาน
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง
            if (queueError == null) // อ่านคิวผ่านจึงนำไปใช้ส่งต่อ
            {
                bool changed = await ProcessMachineQueueAsync(queue); // ตรวจคิวที่ได้สิทธิ์แล้วและยังรอส่ง
                if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
                if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง
                await RefreshStationBarAsync(changed ? null : queue); // อัปเดตแถบเครื่องด้วยคิวหลังประมวลผล
            }
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง

            await ShowRemoteErrorsAsync(); // แสดงเหตุผิดพลาดที่ฝากกลับจาก ST1
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (_sending) { _refreshRequested = true; return; } // มีงานส่งแทรกเข้ามา ให้เก็บรอรีเฟรชทีหลัง

            var signature = BuildSignature(jobs) + QueueSignature(); // คิวเปลี่ยนอย่างเดียวก็ต้องอัปเดตสถานะบนจอ
            if (!force && signature == _lastSignature) return; // ข้อมูลไม่เปลี่ยนและไม่ได้บังคับ ไม่วาดซ้ำ

            _lastSignature = signature; // จำลายเซ็นข้อมูลที่แสดงรอบนี้
            RebindTable(); // กรองและผูกข้อมูลใหม่เข้าตาราง
            await UpdateProcessingAsync(); // อัปเดตส่วนงานที่กำลังผลิต
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            if (!IsDisposed) // ทำต่อเมื่อหน้ายังเปิดอยู่
                tblOrders.EmptyText = $"Error: {ex.Message}"; // แสดงเหตุอ่านข้อมูลไม่ได้ในพื้นที่ตาราง
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            _refreshing = false; // จบการอ่านรอบนี้ เปิดให้รอบใหม่เริ่มได้
            SchedulePendingRefresh(); // นัดอ่านรายการที่ค้างรอรีเฟรช
        }
    }

    private void SchedulePendingRefresh() // นัดรีเฟรชที่ถูกเลื่อนระหว่างส่งงาน
    {
        if (!_refreshRequested || _refreshScheduled || _refreshing || _sending || IsDisposed || !IsHandleCreated) return; // รอจนมีคำขอและหน้าพร้อม โดยไม่ซ้อนงานเดิม
        _refreshScheduled = true; // จำว่านัดรีเฟรชไว้แล้ว
        BeginInvoke(new Action(async () => // ให้เธรดหน้าจอทำรีเฟรชในจังหวะถัดไป
        {
            _refreshScheduled = false; // รับงานที่นัดไว้แล้ว ปลดสถานะรอนัด
            if (!IsDisposed && _refreshRequested) await RefreshDataAsync(force: true); // ยังมีคำขอและหน้ายังอยู่จึงอ่านใหม่
        }));
    }

    private string QueueSignature()
    {
        var sb = new System.Text.StringBuilder(_queueRows.Count * 16);
        foreach (var r in _queueRows.OrderBy(r => r.Id))
            sb.Append(r.Id).Append(r.State).Append(r.DispatchState).Append(r.SentAt?.Ticks).Append('|');
        return sb.ToString();
    }

    private static string BuildSignature(List<PrintJob> jobs)
    {
        var sb = new System.Text.StringBuilder(jobs.Count * 48);
        foreach (var j in jobs)
        {
            sb.Append(j.Id).Append('|')
              .Append(j.Status).Append('|')
              .Append(j.OrderNo).Append('|')
              .Append(j.Qty).Append('|')
              .Append(j.StStatus).Append('|')
              .Append(j.CreatedAt?.Ticks).Append('|')
              .Append(j.UpdatedAt?.Ticks).Append('|')
              .Append(j.PlanRouting?.MarkingMethod).Append('|')
              .Append(j.PlanRouting?.ProcessSequence).Append('|')
              .Append(j.Commands?.Count(c => c.Success) ?? 0).Append(';');
        }
        return sb.ToString();
    }

    private string? _processFilter;

    private void ApplyProcessTabs() // เปิดแท็บ Online และ Offline ตามค่าตั้ง
    {
        bool enabled = StationService.ShowProcessTabs; // ใช้ตัวเลือกหน้างานกำหนดว่าจอนี้เห็น Online/Offline ไหม
        bool changed = enabled != _processTabsEnabled; // ตรวจว่าต้องเปลี่ยนแท็บหรือคอลัมน์จากค่าครั้งก่อนหรือไม่
        _processTabsEnabled = enabled; // จำสถานะเปิดแท็บย่อยรอบล่าสุด

        ShowProcessTabButtons(); // ปรับการแสดงปุ่มแท็บตามค่าตั้ง
        if (!changed) return; // สถานะเหมือนเดิม ไม่ต้องปรับรายการซ้ำ

        if (!enabled && _processFilter != null) // ปิดตัวเลือกขณะอยู่แท็บย่อย ต้องกลับ List
        {
            SwitchTab(false); // กลับแท็บ List เมื่อปิดตัวเลือกแท็บย่อย
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (_allJobs.Count > 0) RebindTable(); // มีข้อมูลอยู่แล้ว ให้กรองตารางใหม่
    }

    private bool _processTabsEnabled;

    private void ShowProcessTabButtons()
    {
        bool show = _processTabsEnabled && !_showHistory;
        btnTabOnline.Visible = show;
        btnTabOffline.Visible = show;
    }

    private void SwitchTab(bool showHistory, string? process = null) // สลับหน้า List, History หรือกรอง Process
    {
        _showHistory = showHistory; // จำว่ากำลังแสดง History หรือไม่
        _processFilter = showHistory ? null : process; // History ไม่ใช้ตัวกรอง Online หรือ Offline

        ButtonStyles.SetSelected(btnTabList, !showHistory && _processFilter == null); // ทำเครื่องหมาย List เมื่อไม่เลือกตัวกรองย่อย
        ButtonStyles.SetSelected(btnTabOnline, _processFilter == JobProcessService.Online); // ทำเครื่องหมายแท็บ Online ที่เลือก
        ButtonStyles.SetSelected(btnTabOffline, _processFilter == JobProcessService.Offline); // ทำเครื่องหมายแท็บ Offline ที่เลือก
        ButtonStyles.SetSelected(btnTabHistory, showHistory); // ทำเครื่องหมายแท็บ History ที่เลือก
        ShowProcessTabButtons(); // ปรับการแสดงปุ่มแท็บตามค่าตั้ง

        lblDateFilter.Visible = showHistory; // แสดงหัวข้อตัวกรองวันเฉพาะ History
        dtpHistoryRange.Visible = showHistory; // แสดงช่องเลือกช่วงวันเฉพาะ History
        btnSearchDate.Visible = showHistory; // แสดงปุ่มค้นหาวันเฉพาะ History
        btnClearDate.Visible = showHistory; // แสดงปุ่มล้างวันเฉพาะ History
        if (!showHistory) dtpHistoryRange.Value = null; // กลับ List แล้วล้างช่วงวันที่ค้างไว้

        _selectedJobId = null; // ล้าง Job ที่เคยเลือกในแท็บก่อน
        ShowPreviewSides(null, null); // ล้างรูปตัวอย่างงานก่อนเปลี่ยนรายการ

        pnlProcessing.Visible = !showHistory; // ส่วนงานกำลังผลิตแสดงเฉพาะ List

        ApplyTabColumns(showHistory); // ปรับคอลัมน์ให้เหมาะกับแท็บที่เลือก

        RebindTable(); // กรองและผูกข้อมูลใหม่เข้าตาราง
        _ = UpdateProcessingAsync(); // อัปเดตส่วนงานที่กำลังผลิต
    }

    private static int StatusRank(PrintJob job) =>
        string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase) ? 0
        : string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase) ? 1
        : 2;

    private void RebindTable() // กรองและเติมรายการตาม Station กับแท็บที่เลือก
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses; // เลือกสถานะงานตาม List หรือ History
        int station = StationService.Current; // อ่าน Station ของจอนี้

        bool showEveryStation = _showHistory && !StationService.IsSt3; // History ที่ไม่ใช่ ST3 ดูงานทุก Station ได้

        var filtered = _allJobs // เริ่มกรองจาก Job ที่อ่านมา
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase)) // เก็บเฉพาะสถานะของแท็บปัจจุบัน
            .Where(j => showEveryStation // History ที่ดูทุก Station ไม่ต้องกรองรหัสเครื่อง
                || MarkingMethodService.VisibleAt(station, j.PlanRouting?.MarkingMethod)) // รายการปกติใช้กฎการเห็นงานของ Station
            .Where(j => _processFilter == null || JobProcessService.Current(j) == _processFilter) // กรอง Online หรือ Offline เมื่อเลือกแท็บย่อย
            .OrderBy(StatusRank) // เรียงกลุ่มตามลำดับความสำคัญของสถานะ
            .ThenByDescending(j => j.CreatedAt ?? DateTime.MinValue) // ในกลุ่มเดียวกันให้งานใหม่ขึ้นก่อน
            .ToList(); // เก็บผลที่กรองแล้วเป็นรายการ

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _); // จำว่า History กำลังใช้ตัวกรองวัน

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList(); // แปลง Job เป็นแถวข้อมูลบนตาราง
        tblOrders.EmptyText = _allJobs.Count == 0 // เลือกข้อความตอนตารางไม่มีงาน
            ? "No orders" // ไม่มี Job เลยให้แสดงข้อความว่างทั่วไป
            : _processFilter != null && rows.Count == 0 // ตรวจว่ากรอง Process แล้วไม่เหลืองาน
                ? $"ไม่มีงาน {_processFilter}" // ระบุว่าไม่พบงานของ Process ที่เลือก
            : dateFiltered && rows.Count == 0 // ตรวจว่ากรองช่วงวันแล้วไม่เหลืองาน
                ? "ไม่มีงานในช่วงวันที่ที่เลือก" // แจ้งว่าไม่มีงานในช่วงวันที่เลือก
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(JobStatusDisplay.Text))})"; // ระบุจำนวนงานเดิมและสถานะที่กำลังกรอง
        _displayRows = rows; // เก็บแถวที่แสดงไว้ใช้กับการเลือกงาน
        tblOrders.DataSource = rows; // นำข้อมูลแถวไปแสดงในตาราง
        ReapplySort(); // เรียงตามคอลัมน์ที่ผู้ใช้เลือกไว้
        RestoreSelection(); // คืนการเลือกงานเดิมถ้ายังอยู่ในรายการ

        ApplyStartLoading(); // อัปเดตปุ่มหมุนให้ตรงกับ Job ที่เริ่ม
    }

    private int? _startingJobId;

    private const string StartButtonText = "เริ่มงาน";

    private void ShowStartLoading(int? jobId) // แสดงการรอที่ปุ่มเริ่มของงานที่เลือก
    {
        _startingJobId = jobId; // จำแถวที่กำลังเริ่ม เพื่อไม่ให้ไปหมุนผิดงาน
        ApplyStartLoading(); // อัปเดตปุ่มหมุนให้ตรงกับ Job ที่เริ่ม
    }

    private void ApplyStartLoading()
    {
        if (IsDisposed) return;

        foreach (var row in _displayRows)
        {
            foreach (var button in row.Op)
            {
                if (button.Id != "start") continue;

                bool spin = row.Id == _startingJobId;
                button.Loading = spin;

                button.Text = spin ? "" : StartButtonText;
            }
        }
    }

    private void ReapplySort()
    {
        if (tblOrders.Columns == null) return;

        foreach (var column in tblOrders.Columns)
        {
            if (column.SortMode == AntdUI.SortMode.NONE) continue;
            tblOrders.Sort(column);
            return;
        }
    }

    private bool TryGetDateRange(out DateTime from, out DateTime to)
    {
        from = default;
        to = default;

        var value = dtpHistoryRange.Value;
        if (value == null || value.Length < 2) return false;

        var a = value[0].Date;
        var b = value[1].Date;
        if (b < a) (a, b) = (b, a);

        from = a;
        to = b.AddDays(1).AddTicks(-1);
        return true;
    }

    private async void TblOrders_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Record is not OrderRow row) return;
        if (_api == null || _rowBusy) return; // Backend ยังไม่พร้อมหรือปุ่มก่อนหน้ายังทำไม่จบ ให้ข้าม

        _rowBusy = true;
        try
        {
            await HandleRowButtonAsync(e.Btn?.Id, row); // แยกไปทำตามปุ่มที่กดและ Job ของแถวนั้น
        }
        finally
        {
            _rowBusy = false;
        }
    }

    private async Task<ResolvedJobResponse?> LoadJobAsync(int jobId, string busyText) // โหลดรายละเอียด Job พร้อมข้อความรอเมื่อช้า
    {
        var task = _api!.GetResolvedJobAsync(jobId); // ขอข้อมูล Job พร้อม Pattern, UV และประวัติส่งจาก Backend
        if (await Task.WhenAny(task, Task.Delay(SlowLoadMs)) == task) return await task; // ถ้าอ่านทันภายในเวลาที่กำหนด ใช้ข้อมูลได้เลยโดยไม่ขึ้นหน้ารอ

        ShowSending(busyText); // ขึ้นข้อความรอเมื่ออ่านรายละเอียดช้า
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            return await task; // รอรายละเอียด Job ให้ครบก่อนส่งต่อ
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            if (!IsDisposed && !_sending) ShowSending(null); // ปิดข้อความรอเมื่อไม่มีงานส่งค้าง
        }
    }

    private async Task HandleRowButtonAsync(string? buttonId, OrderRow row) // แยกงานตามปุ่มที่กดในตาราง
    {
        if (buttonId == "detail") // ปุ่มรายละเอียดเปิดข้อมูลของแถวที่เลือก
        {
            var resolved = await LoadJobAsync(row.Id, $"กำลังโหลดข้อมูล · {JobName(row.Id)}"); // อ่านรายละเอียดล่าสุดก่อนเปิดหน้าต่าง
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (resolved == null) // อ่านรายละเอียด Job ไม่ได้
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ {JobName(row.Id)} ได้"); // แจ้งว่าอ่านรายละเอียดของงานนี้ไม่ได้
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }
            await ShowDetailDialogAsync(resolved); // เปิด Order Detail แล้วรอให้ผู้ใช้ปิด
        }
        else if (buttonId == "start") // ผู้ใช้กดปุ่มเริ่มงาน
        {
            if (_startingJobId != null) return; // ยังมี Job ที่กำลังเริ่มอยู่ ไม่รับกดซ้อน

            ShowStartLoading(row.Id); // ให้ปุ่มเริ่มหมุนตั้งแต่โหลดงานจนจบการส่ง
            try // ดักข้อผิดพลาดของขั้นนี้
            {
                await StartJobAsync(row.Id); // เริ่ม Job ของแถวที่กด
            }
            finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
            {
                ShowStartLoading(null); // คืนปุ่มเริ่มให้กดได้ทั้งกรณีสำเร็จและยกเลิก
            }
        }
        else if (buttonId == "complete") // ผู้ใช้กดปุ่มจบงาน
        {
            await CompleteJobAsync(row.Id); // ตรวจและบันทึกจบ Job ที่เลือก
        }
        else if (buttonId == "cancel") // ผู้ใช้กดปุ่มยกเลิกงาน
        {
            await CancelJobAsync(row.Id); // ยกเลิก Job ของแถวที่กด
        }
        else if (buttonId == "restore") // ผู้ใช้กดนำงานจาก History กลับมา
        {
            await RestoreJobAsync(row.Id); // คืน Job เดิมเป็น Waiting
        }
    }

    private string JobName(int jobId)
    {
        var job = _allJobs.FirstOrDefault(j => j.Id == jobId);
        return job == null ? $"#{jobId}" : JobLabel(job);
    }

    private static string JobLabel(PrintJob job) => JobDisplay.Label(job);

    private static string FirstFilled(params string?[] values)
    {
        foreach (var value in values)
        {
            var text = (value ?? "").Trim();
            if (text.Length > 0) return text;
        }
        return Dash;
    }

    private static bool IsCancelled(PrintJob job) =>
        string.Equals(job.Status, "Cancel", StringComparison.OrdinalIgnoreCase);

    private async Task RestoreJobAsync(int jobId) // นำ Job เดิมจาก History กลับมารอเริ่ม
    {
        if (_api == null) return; // ยังไม่มีตัวเรียก Backend ให้หยุดก่อน

        if (!Confirm.Ask(this, "ยืนยันนำกลับมาพิมพ์ใหม่", // ให้ผู้ใช้ยืนยันก่อนเปลี่ยนงานใน History กลับมารอ
                $"{JobName(jobId)}\n\n" // ระบุ Job ที่จะนำกลับในกล่องยืนยัน
                + "งานจะกลับไปอยู่ในรายการงาน รอกดเริ่มงานอีกครั้ง\n\n" // บอกว่านำกลับแล้วต้องกดเริ่มใหม่
                + "ยืนยันหรือไม่?")) // ถามยืนยันการนำ Job กลับ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Waiting"); // คืนสถานะรอ โดยยังเก็บประวัติส่งเดิม
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
        {
            Notify.ErrorModal(this, "นำกลับมาไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้"); // แจ้งเหตุที่คืนสถานะ Waiting ไม่ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอส่งเก่าของ ST3
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        Notify.Success(this, $"{JobName(jobId)} กลับไปอยู่ในรายการงานแล้ว"); // แจ้งว่า Job กลับมารอใน List แล้ว
        await RefreshDataAsync(force: true); // อ่านรายการใหม่ให้เห็นงานที่นำกลับ
    }

    private async Task CancelJobAsync(int jobId) // ยืนยันยกเลิกงานแล้วให้ Backend ล้างคิว
    {
        if (_api == null) return; // ยังไม่มีตัวเรียก Backend ให้หยุดก่อน

        if (!Confirm.Ask(this, "ยืนยันยกเลิกงาน", // ให้ผู้ใช้ยืนยันก่อนย้ายงานออกจากรายการผลิต
                $"ยกเลิก {JobName(jobId)}\n\n" // ระบุงานที่จะยกเลิกให้ตรวจอีกครั้ง
                + "งานจะถูกย้ายออกจากรายการไปอยู่ในประวัติ\n" // บอกว่างานย้ายไปอยู่ History
                + "ถ้าต้องการทำต่อ กดพิมพ์ใหม่ได้ที่แท็บ History\n\n" // บอกทางนำงานกลับมาทำต่อ
                + "ยืนยันหรือไม่?")) // ถามยืนยันก่อนเปลี่ยนเป็น Cancel
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Cancel"); // ขอเปลี่ยนสถานะและล้างคิวที่ Backend
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
        {
            Notify.ErrorModal(this, "ยกเลิกงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะยกเลิกได้"); // แจ้งเหตุที่ยกเลิก Job ไม่ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอฝากส่งของ Job นี้
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        Notify.Success(this, $"ยกเลิก {JobName(jobId)} แล้ว"); // แจ้งยืนยันผลยกเลิกงาน
        await RefreshDataAsync(force: true); // อ่าน Job และคิวใหม่โดยไม่ใช้ข้อมูลเดิม
    }

    private int _sendOperations;
    private bool _sending => _sendOperations > 0;

    private void EndSending() // ปิดรอบส่งและรายงานผลที่สะสมไว้
    {
        _sendOperations--; // ลดจำนวนชุดที่กำลังส่ง
        if (_sending || IsDisposed) return; // ยังมีชุดอื่นส่งอยู่หรือปิดหน้าแล้ว ให้หยุดก่อน
        ShowSending(null); // ปิดข้อความกำลังส่ง
        if (!_showingSendReport) // ยังไม่มีชุดรายงานผลเปิดอยู่
        {
            _showingSendReport = true; // ล็อกไม่ให้เปิดรายงานผลซ้อน
            try // ดักข้อผิดพลาดของขั้นนี้
            {
                while (!_sending && _sendReports.Count > 0 && !IsDisposed) // ทยอยรายงานเมื่อไม่มีงานส่งค้าง
                {
                    var lines = _sendReports.ToArray(); // แยกผลรอบนี้ออกจากรายการสะสม
                    _sendReports.Clear(); // ล้างที่เก็บเพื่อรับผลชุดถัดไป
                    Notify.Result(this, "ผลส่งงาน", lines); // แสดงผลรวมการส่งรอบนี้
                }
            }
            finally { _showingSendReport = false; } // ปลดสถานะรายงานผลเสมอเมื่อจบ
        }
        SchedulePendingRefresh(); // นัดอ่านรายการที่ค้างรอรีเฟรช
    }

    private static bool NotMyTurnYet(PrintJob job)
    {
        bool started = job.Commands?.Any(c => c.Success) == true;
        if (started) return false;

        var method = job.PlanRouting?.MarkingMethod;
        if (MarkingMethodService.Resolve(method).NoCase) return false;

        return !MarkingMethodService.CanStartAt(StationService.Current, method);
    }

    private static bool CanStart(PrintJob job)
    {
        if (!string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return false;

        if (job.RemoteStart is RemotePending or RemoteSending)
            return false;

        var method = job.PlanRouting?.MarkingMethod;
        if (!MarkingMethodService.CanStartAt(StationService.Current, method))
            return false;

        return !MarkingMethodService.Resolve(method).NoCase;
    }

    private async Task StartJobAsync(int jobId) // ตรวจงาน จองคิว แล้วเริ่มส่งตาม Station
    {
        if (_api == null || _sending) return; // Backend ยังไม่พร้อมหรือส่งอยู่ ให้รอก่อน

        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่านรายละเอียด Job ล่าสุดก่อนเริ่ม
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
        if (resolved == null) // อ่านรายละเอียด Job ไม่ได้
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้"); // แจ้งว่าอ่านรายละเอียด Job ไม่ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var method = resolved.PlanRouting?.MarkingMethod; // อ่านรหัสวิธีพิมพ์ของงาน
        int station = StationService.Current; // อ่าน Station ของจอนี้

        if (!MarkingMethodService.CanStartAt(station, method)) // ตรวจว่างานรหัสนี้เริ่มที่ Station ปัจจุบันได้หรือไม่
        {
            Notify.WarnModal(this, "เริ่มงานที่สถานีนี้ไม่ได้", // เตือนว่า Station นี้ไม่มีสิทธิ์เริ่มงาน
                $"{JobName(jobId)} — marking {Method(method)}\n\n" // ระบุ Job และรหัสพิมพ์ที่ถูกกันไว้
                + ((method ?? "").Trim() == "10" // แยกข้อความสำหรับรหัส 10
                    ? "งาน marking 10 เริ่มได้ที่ ST3 เท่านั้น" // ชี้ให้เริ่มงาน 10 จาก ST3
                    : "งานนี้เริ่มได้ที่ ST1 เท่านั้น")); // งานรหัสอื่นให้เริ่มจาก ST1
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var plan = MarkingMethodService.Resolve(method); // แปลงรหัสเป็นเครื่องและลำดับพิมพ์
        if (plan.NoCase) // รหัสนี้ยังไม่มีแผนที่รองรับ
        {
            Notify.WarnModal(this, "แจ้งเตือน", // แจ้งปัญหารหัส Marking
                $"{JobName(jobId)} ใช้รหัส marking ที่ไม่มีอยู่จริง ({Method(method)})"); // บอกงานและรหัสที่ไม่มีแผนรองรับ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (plan.Steps.Count == 0) // ไม่มีขั้นส่งเครื่อง เช่น marking 00
        {
            await StartWithoutSendingAsync(jobId, method); // เปลี่ยนเป็นกำลังผลิตอย่างเดียวสำหรับงานไม่มีขั้นส่ง
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var uvPicks = PickUvPrograms(plan, resolved); // เลือกโปรแกรม UV ที่จอคนกด ก่อนจองคิว
        if (uvPicks == null || IsDisposed) return; // ยกเลิกเลือกโปรแกรมแล้วไม่จองคิว

        if (!await ConfirmStartAsync(jobId, resolved, plan, uvPicks)) return; // ให้ตรวจชื่อโปรแกรมและคิวก่อนเริ่มจริง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (!StationService.IsSt3 && await BlockedByUnreachableAsync(jobId, plan, resolved)) return; // ST1 ต้องต่อเครื่องที่ใช้ให้ครบ ส่วน ST3 ฝากให้ ST1 ส่ง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        var (queued, queueError) = await _api.EnqueueMachinesAsync( // จองคิวของ Job พร้อมชื่อโปรแกรมที่เลือก
            jobId, QueueItemsFor(plan.Steps, uvPicks)); // แยกเลขรอบเมื่อแผนใช้เครื่องเดิมซ้ำ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (!queued) // จองคิวไม่ผ่าน จึงยังไม่ส่งเครื่อง
        {
            Notify.ErrorModal(this, "จองเครื่องไม่สำเร็จ", // แจ้งว่าจองคิวเครื่องไม่ผ่าน
                $"{JobName(jobId)} ยังไม่ได้เข้าคิว" + Environment.NewLine + Environment.NewLine // ระบุว่า Job ยังไม่ได้เข้าคิว
                + (queueError ?? "ติดต่อ backend ไม่ได้")); // แนบเหตุจาก Backend หรือเหตุที่ติดต่อไม่ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (station == StationService.St3) // ST3 ขอสิทธิ์คิวแล้วให้ ST1 เป็นผู้ส่งเครื่อง
        {
            var errors = await ClaimRemoteQueueAsync(jobId, plan.Steps); // ST3 ขอให้หัวคิวเป็น active เพื่อให้ ST1 รับไปส่ง
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (errors.Count == 0) // ขอสิทธิ์คิวครบโดยไม่พบปัญหา
                Notify.Success(this, $"{JobName(jobId)} เข้าคิวแล้ว · ST1 จะส่งตามลำดับคิว"); // แจ้งว่า ST1 จะรับส่งตามคิว
            else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                Notify.Warn(this, $"{JobName(jobId)} เข้าคิวแล้ว แต่ยืนยันการเริ่มคิวไม่ได้ · " // แจ้งว่าจองแล้วแต่ยังยืนยันสิทธิ์คิวไม่ได้
                    + string.Join(" · ", errors) + " · ตรวจสถานะคิวก่อนกดเริ่มอีกครั้ง"); // รวมปัญหาและให้ตรวจคิวก่อนเริ่มซ้ำ
            await RefreshDataAsync(force: true); // อ่าน Job และคิวใหม่โดยไม่ใช้ข้อมูลเดิม
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        await SendQueuedForJobAsync(jobId, resolved, $"เริ่มงาน {JobName(jobId)}"); // ST1 ขอสิทธิ์เครื่องแล้วส่งข้อมูลของ Job ที่กด
    }

    private async Task<List<string>> ClaimRemoteQueueAsync(int jobId, List<string> steps) // ST3 ขอสิทธิ์คิวไว้ให้ ST1 รับไปส่ง
    {
        var errors = new List<string>(); // เตรียมเก็บปัญหาที่พบในรอบนี้
        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase)) // ขอเครื่องละหนึ่งครั้ง แม้แผนมีหลายรอบ
        {
            var (claim, error) = await _api!.ClaimMachineAsync(machine, jobId); // ให้ Backend ตรวจผู้ถือเครื่องและลำดับ FIFO
            if (IsDisposed) break; // ปิดหน้าแล้วให้หยุดวนรายการ
            if (error != null || claim == null) // คำขอสิทธิ์ผิดพลาดหรือไม่มีคำตอบ
                errors.Add($"{machine}: {error ?? "ไม่ได้รับผลการขอคิว"}"); // เก็บปัญหาการขอสิทธิ์แยกเครื่อง
            else if (claim.Claimed == null && claim.Reason is not ("busy" or "queued")) // เครื่องติดงานหรือมีคนจองก่อนถือว่ารอคิวตามปกติ
                errors.Add($"{machine}: ไม่พบคิวที่พร้อมเริ่ม"); // เก็บเหตุว่าเครื่องยังไม่มีคิวพร้อมเริ่ม
        }
        return errors; // ส่งปัญหาทั้งชุดให้ผู้เริ่มงานดู
    }

    private async Task<bool> ConfirmStartAsync( // ให้ตรวจแผนและคิวก่อนยืนยันเริ่มงาน
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan, // รับ Job รายละเอียด และแผนส่งเครื่อง
        Dictionary<string, string> uvPicks) // รับโปรแกรม UV ที่ผู้ใช้เลือกไว้
    {
        var (rows, _) = await _api!.GetMachineQueueAsync(); // อ่านคิวไว้สรุปให้ผู้ใช้ดูก่อนเริ่มงาน
        if (IsDisposed) return false; // ปิดหน้าแล้วไม่เปิดกล่องยืนยันต่อ

        return Confirm.Ask(this, "ยืนยันเริ่มงาน", // ใช้คำตอบผู้ใช้ตัดสินว่าจะเริ่มหรือยกเลิก
            BuildStartPreview(jobId, resolved, plan, rows, uvPicks)); // สร้างสรุป Job เครื่อง คิว และโปรแกรมก่อนส่ง
    }

    private string BuildStartPreview(
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan, List<MachineQueueRow> rows,
        Dictionary<string, string> uvPicks)
    {
        var body = new List<string>
        {
            $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}",
            "",
        };

        foreach (var step in plan.Steps)
        {
            int station = JobStationService.StationOf(step) ?? 0;
            var holder = rows.FirstOrDefault(r => r.Machine == step && r.State == "active");

            var state = holder == null
                ? rows.Any(r => r.Machine == step && r.State == "pending")
                    ? "มีงานรอ · ส่งตามลำดับคิว"
                    : "ว่าง · ส่งเดี๋ยวนี้"
                : $"ไม่ว่าง ({JobName(holder.PrintJobsId)} ค้างอยู่) · เข้าคิวรอปุ่มกดหน้างาน";

            body.Add($"[ {step} · ST{station} ]  {state}");
            body.AddRange(StepPreviewLines(step, resolved, uvPicks).Select(line => "      " + line));
            body.Add("");
        }

        return string.Join(Environment.NewLine, body).TrimEnd();
    }

    private static List<string> StepPreviewLines(
        string step, ResolvedJobResponse resolved, Dictionary<string, string> uvPicks)
    {
        var lines = new List<string>();

        if (string.Equals(step, "MK", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var (nameKey, fallback, ordinal) in MkHeads)
            {
                var head = CustomSettingsManager.Read(nameKey, fallback);
                var config = resolved.Pattern?.InkjetConfigs
                    .FirstOrDefault(c => c.Ordinal == ordinal);

                bool used = config != null
                    && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

                lines.Add(used
                    ? $"{head}: {OrDash(config!.ProgramName)}  (No. {OrDash(config.ProgramNumber?.ToString())})"
                    : $"{head}: ไม่ได้ใช้ — จะสั่งหยุดเครื่อง");
            }

            return lines;
        }

        int uvNumber = string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
        var uvName = UvSettingsManager.Read(
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

        var uv = resolved.UvJobData?.FirstOrDefault(r =>
            string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase));

        if (uv == null)
        {
            lines.Add($"{uvName}: ยังไม่มีข้อมูลของงานนี้");
            return lines;
        }

        lines.Add($"{uvName}: โปรแกรม {OrDash(uv.ProgramName)}");
        lines.Add($"Lot: {OrDash(uv.Lot)}      Name: {OrDash(uv.ErpMfg)}");

        if (uvPicks.TryGetValue(step, out var chosen))
            lines.Add($"จะโหลดไฟล์ {chosen}.uvdx เข้าเครื่อง");
        else if (UvSettingsManager.GetDocumentFolder(uvNumber) == null)
            lines.Add($"(เครื่องนี้ยังไม่ได้ตั้งโฟลเดอร์ UV{uvNumber} — ถ้ามีรุ่นย่อย กล่องเลือกจะไปเด้งที่ ST1 ตอนส่ง)");
        else
            lines.Add("(ยังไม่รู้รุ่นย่อย — เครื่องที่ต่อสายจะถามตอนส่ง)");

        return lines;
    }

    private static int? UvNumberOf(string step) =>
        string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1
        : string.Equals(step, "UV2", StringComparison.OrdinalIgnoreCase) ? 2
        : null;

    private Dictionary<string, string>? PickUvPrograms(MarkingPlan plan, ResolvedJobResponse resolved) // เลือกโปรแกรม UV ก่อนส่งชื่อไปกับคิว
    {
        var picks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // เก็บโปรแกรมที่เลือกแยก UV1 กับ UV2

        foreach (var step in plan.Steps.Distinct(StringComparer.OrdinalIgnoreCase)) // เลือกโปรแกรมให้แต่ละเครื่องเพียงครั้งเดียว
        {
            if (UvNumberOf(step) is not int uvNumber) continue; // ขั้นที่ไม่ใช่ UV ไม่ต้องเลือกโปรแกรม

            var uvRow = resolved.UvJobData?.FirstOrDefault(r => // หาแถว UV ของเครื่องที่จะส่ง
                string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase)); // เลือกข้อมูลให้ตรงชื่อเครื่องในแผน

            if (string.IsNullOrWhiteSpace(uvRow?.ProgramName)) continue; // ไม่มีชื่อโปรแกรมให้เลือก จะตรวจความพร้อมอีกทีตอนส่ง

            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber); // อ่านโฟลเดอร์โปรแกรมของ UV นี้
            if (docFolder == null) continue; // ไม่มีโฟลเดอร์ให้ข้ามการเลือกในรอบนี้

            var pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, this); // ค้นไฟล์หรือเปิดให้เลือกรุ่นย่อยที่จอนี้
            if (pick.Program == null) return null; // ผู้ใช้ไม่เลือกโปรแกรม ให้ยกเลิกชุดเลือก

            var uvName = UvSettingsManager.Read( // อ่านชื่อเครื่อง UV ตามค่าตั้ง
                uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}"); // เลือกชุดชื่อให้ตรงกับ UV1 หรือ UV2

            if (pick.IsDefault && // ตรวจว่าโปรแกรมที่ได้เป็นตัวสำรอง
                !UvProgramResolver.ConfirmDefault(uvRow.ProgramName, uvName, this)) // ให้ยืนยันว่าจะใช้สำรองแทนชื่อในงาน
                return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ

            picks[step] = pick.Program; // จำชื่อไฟล์ไว้ส่งไปกับคิวเครื่อง
        }

        return picks; // ส่งโปรแกรมที่เลือกแยกตาม UV กลับไป
    }

    private static string OrDash(string? text) =>
        string.IsNullOrWhiteSpace(text) ? Dash : text.Trim();

    private static readonly (string NameKey, string Fallback, int Ordinal)[] MkHeads =
    [
        ("MK058_NAME", "MK-058", 1),
        ("MK059_NAME", "MK-059", 2),
    ];

    private static List<MachineQueueItem> QueueItemsFor( // สร้างคิวเครื่องและแยกรอบของงาน
        List<string> steps, Dictionary<string, string> uvPicks) // รับแผนเครื่องและชื่อโปรแกรมที่เลือก
    {
        var rounds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // นับเลขรอบแยกตามเครื่อง
        var items = new List<MachineQueueItem>(); // เตรียมรายการคิวที่จะส่งไป Backend

        foreach (var step in steps) // สร้างคิวตามทุกขั้นในแผน
        {
            rounds[step] = rounds.TryGetValue(step, out int used) ? used + 1 : 1; // เครื่องเดิมปรากฏซ้ำให้เป็นรอบถัดไป เช่น MK รอบ 2
            items.Add(new MachineQueueItem // เพิ่มคิวหนึ่งขั้นตามแผน
            {
                Machine = step, // ระบุเครื่องที่จะใช้ในแถวคิว
                Round = rounds[step], // ระบุรอบของเครื่องนี้

                ProgramName = uvPicks.GetValueOrDefault(step), // ตอนส่งจริงใช้ชื่อนี้โดยไม่ถามเลือกซ้ำ
            });
        }

        return items; // ส่งรายการคิวให้เรียก Backend
    }

    private async Task SendQueuedForJobAsync(int jobId, ResolvedJobResponse resolved, string title) // ขอเครื่องที่ว่างแล้วรวมเป็นชุดส่ง
    {
        if (_sending) return; // ยังมีงานส่งอยู่ ให้รอรอบถัดไป
        _sendOperations++; // นับชุดส่งที่กำลังทำงานเพิ่ม
        var lines = new List<Notify.ResultLine>(); // เตรียมเก็บผลแต่ละขั้นไว้รายงานรวม
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var (rows, error) = await _api!.GetMachineQueueAsync(); // อ่านคิวเครื่องล่าสุดจาก Backend
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (error != null) { Notify.Error(this, error); return; } // อ่านคิวไม่ได้ ให้แจ้งแล้วหยุดรอบส่ง
            var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps; // ใช้ลำดับเครื่องของงานเพื่อจัดชุดที่จะส่ง
            var machines = rows.Where(r => r.PrintJobsId == jobId && r.State == "pending") // เอาเฉพาะคิวรอของ Job ที่กดเริ่ม
                .Select(r => r.Machine).Distinct(StringComparer.OrdinalIgnoreCase) // เอารายชื่อเครื่องโดยตัดชื่อที่ซ้ำ
                .OrderBy(m => PlanOrderOf(plan, m)).ToList(); // จัดรายการให้ตรงกับแผนพิมพ์ของงาน
            var ready = new List<PreparedQueueSend>(); // เตรียมรวมคิวที่พร้อมส่ง
            bool anyQueued = rows.Any(r => r.PrintJobsId == jobId && r.State == "active"); // จำว่ามีคิวได้รับสิทธิ์แล้ว เพื่อไม่ล้างทิ้งผิดจังหวะ
            foreach (var machine in machines) // ขอสิทธิ์ทีละเครื่อง แล้วรวมเครื่องที่พร้อมส่งเป็นชุด
            {
                var (claim, claimError) = await _api.ClaimMachineAsync(machine, jobId); // ขอสิทธิ์เฉพาะคิวของ Job นี้ ไม่หยิบงานอื่น
                if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
                if (claimError != null) // ขอสิทธิ์เครื่องแล้ว Backend แจ้งข้อผิดพลาด
                {
                    anyQueued = true; // ขอสิทธิ์ไม่ชัดเจน ให้ถือว่าคิวอาจยังอยู่
                    lines.Add(Notify.Bad($"{machine}: {claimError}")); // เก็บเหตุที่ขอใช้เครื่องนี้ไม่ได้
                }
                else if (claim?.Claimed is { } row) ready.Add(new(row, resolved)); // รวมคิวที่ได้เครื่องพร้อมข้อมูลของ Job เดียวกัน
                else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                {
                    anyQueued = true; // เครื่องไม่ว่างหรือยังไม่ถึงคิว ให้คงคิวไว้
                    lines.Add(Notify.Note(claim?.Reason == "queued" // แยกข้อความเหตุที่ต้องรอเครื่อง
                        ? $"{machine}: มีงานเข้าคิวก่อน รอตามลำดับคิว" // บอกว่ามีงานมาก่อน ห้ามแซงลำดับ
                        : $"{machine}: เครื่องไม่ว่าง เข้าคิวรอไว้แล้ว")); // บอกว่าเครื่องติดงานและเก็บคิวรอแล้ว
                }
            }
            var results = await SendPreparedBatchAsync(ready); // ส่งเครื่องที่พร้อมพร้อมกัน โดยแยกผลแต่ละเครื่อง
            lines.AddRange(results.SelectMany(r => r.Lines)); // รวมผลของเครื่องที่ส่งพร้อมกัน
            anyQueued |= results.Any(r => r.HeldForReview); // คิวที่ยังไม่รู้ผลต้องถือไว้ ไม่ล้างหรือส่งซ้ำ
            if (!results.Any(r => r.Sent) && !anyQueued && !PrintedBefore(resolved)) // ล้างได้เมื่อไม่มีผลส่งเดิม ไม่มีคิวรอ และไม่มีคิวต้องตรวจ
            {
                var (cleared, clearError) = await _api.ClearMachineQueueAsync(jobId, onlyUnsent: true); // ขอให้ Backend ล้างเฉพาะงานที่ทุกคิวยังไม่ถูกหยิบส่ง
                if (!cleared) lines.Add(Notify.Bad($"ล้างคิวไม่สำเร็จ: {clearError}")); // เก็บเหตุที่ล้างคิวปลอดภัยไม่สำเร็จ
            }
            if (!IsDisposed) _sendReports.AddRange(lines.Select(l => l with { Text = $"{title} · {l.Text}" })); // แนบชื่อชุดงานไว้ในผลที่รอรายงาน
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            EndSending(); // จบชุดส่งแล้วตรวจงานที่ค้างอยู่
            SchedulePendingRefresh(); // นัดอ่านรายการที่ค้างรอรีเฟรช
            if (!IsDisposed && !_sending) ShowSending(null); // ปิดข้อความรอเมื่อไม่มีงานส่งค้าง
        }
        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านสถานะใหม่ถ้าหน้ายังเปิดอยู่
    }

    private bool ReserveDispatch(string machine) => _dispatchingMachines.TryAdd(machine, Guid.NewGuid()); // จองสิทธิ์ในโปรแกรมให้เครื่องละรอบส่งเดียว

    private void ReleaseDispatch(string machine, Guid token) // คืนสิทธิ์เฉพาะรอบส่งที่ถือ token นี้
    {
        if (_dispatchingMachines.TryGetValue(machine, out var current) && current == token) // รอบเก่าปลดสิทธิ์ได้เฉพาะ token ของตัวเอง
        {
            _dispatchingMachines.Remove(machine); // เปิดทางให้เครื่องนี้รับรอบส่งใหม่
            if (!IsDisposed && _dispatchingMachines.Count > 0) // ยังมีเครื่องอื่นในชุดที่กำลังส่ง
                ShowSending($"กำลังส่งไปที่ {string.Join(" / ", _dispatchingMachines.Keys)}"); // แสดงชื่อเครื่องที่ยังทำงานอยู่
        }
    }

    private sealed record PreparedQueueSend(MachineQueueRow Row, ResolvedJobResponse Job);

    private async Task<StepSendResult[]> SendPreparedBatchAsync(List<PreparedQueueSend> items, bool includeJobNames = false, bool machinesReserved = false) // เตรียมโปรแกรมแล้วส่งเครื่องที่พร้อมพร้อมกัน
    {
        if (_preparingPrograms) return [new(false, [], HeldForReview: true)]; // กำลังเลือกโปรแกรมอยู่ ให้ถือคิวชุดใหม่ไว้ก่อน
        var owned = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase); // จำสิทธิ์ส่งแยกเครื่องไว้คืนตอนจบ
        var selected = new List<PreparedQueueSend>(); // เก็บคิวที่ชุดนี้ได้รับสิทธิ์ส่งจริง
        foreach (var item in items.GroupBy(i => i.Row.Machine, StringComparer.OrdinalIgnoreCase).Select(g => g.First())) // เลือกเครื่องละรอบ ไม่ส่ง MK รอบสองตามไปเอง
            if (machinesReserved || ReserveDispatch(item.Row.Machine)) // ใช้สิทธิ์ที่จองไว้ หรือขอสิทธิ์ในโปรแกรมก่อนส่ง
            {
                selected.Add(item); // รวมคิวที่ส่งได้ในชุดนี้
                owned[item.Row.Machine] = _dispatchingMachines[item.Row.Machine]; // จำ token ไว้คืนสิทธิ์เมื่อเครื่องนี้ทำเสร็จ
            }
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var ready = new List<PreparedQueueSend>(); // เตรียมรวมคิวที่พร้อมส่ง
            var results = new List<StepSendResult>(); // เตรียมผลของทุกคิวในชุดนี้
            if (selected.Count != items.Count) results.Add(new(false, [], HeldForReview: true)); // บางคิวไม่ได้สิทธิ์ จึงห้ามตีความว่างานไม่มีคิวค้าง
            foreach (var item in selected) // ตรวจความพร้อมของคิวที่ได้สิทธิ์ทีละรายการ
            {
                var row = item.Row; // อ่านแถวคิวของรายการปัจจุบัน
                if (IsDisposed) break; // ปิดหน้าแล้วให้หยุดวนรายการ
                if (UvNumberOf(row.Machine) is int uvNumber && string.IsNullOrWhiteSpace(row.ProgramName)) // คิว UV ที่ยังไม่เลือกโปรแกรมต้องเตรียมให้เสร็จก่อน
                {
                    if (MachineBusy.Active) { results.Add(new(false, [], HeldForReview: true)); continue; } // ยังมีเครื่องส่งอยู่ อย่าเปิดกล่องเลือกโปรแกรมแทรก
                    _preparingPrograms = true; // กันชุดอื่นเริ่มส่งระหว่างคนเลือกโปรแกรม
                    try // ดักข้อผิดพลาดของขั้นนี้
                    {
                        var requested = item.Job.UvJobData?.FirstOrDefault(u => u.Machine == row.Machine)?.ProgramName; // อ่านชื่อโปรแกรมต้นทางของเครื่องในคิว
                        var pick = UvProgramResolver.Resolve(requested, UvSettingsManager.GetDocumentFolder(uvNumber), this); // เลือกไฟล์ UV สำหรับคิวที่ยังไม่มีชื่อจริง
                        if (pick.Program == null || (pick.IsDefault && // ตรวจว่ายังไม่เลือกหรือได้โปรแกรมสำรอง
                            !UvProgramResolver.ConfirmDefault(requested ?? "", row.Machine, this))) // ผู้ใช้ไม่ยืนยันสำรอง ให้เลื่อนคิวกลับไปรอ
                        {
                            var reset = await _api!.UpdateMachineQueueAsync(row.Id, state: "pending"); // ยกเลิกเลือกโปรแกรม ให้คืนคิวไปรอก่อน
                            results.Add(new(false, [Notify.Careful($"{row.Machine}: ยังไม่ส่ง เพราะไม่ได้เลือกโปรแกรม")], // บอกว่ายังไม่ส่งเพราะยังไม่เลือกโปรแกรม
                                SafeToRetry: reset.ok, HeldForReview: !reset.ok)); // ลองใหม่ได้ต่อเมื่อยืนยันคืนคิวแล้ว
                            if (!reset.ok) results[^1].Lines.Add(Notify.Bad($"คืนคิวไม่ได้: {reset.error}")); // แนบเหตุที่คืนคิวไป pending ไม่ได้
                            continue; // ข้ามรายการนี้ไปตัวถัดไป
                        }
                        var saved = await _api!.UpdateMachineQueueAsync(row.Id, programName: pick.Program); // เก็บโปรแกรมลงคิวก่อนแตะเครื่องจริง
                        if (!saved.ok) // เก็บชื่อโปรแกรมลงคิวไม่สำเร็จ
                        {
                            results.Add(new(false, [Notify.Bad($"{row.Machine}: บันทึกโปรแกรมไม่ได้ — {saved.error}")], HeldForReview: true)); // ถือคิวไว้ตรวจ เพราะชื่อโปรแกรมยังไม่แน่นอน
                            continue; // ข้ามรายการนี้ไปตัวถัดไป
                        }
                        row.ProgramName = pick.Program; // ใช้โปรแกรมเดียวกับที่เพิ่งบันทึกลง Backend
                    }
                    finally { _preparingPrograms = false; } // ปลดช่วงเลือกโปรแกรมแม้ผู้ใช้ยกเลิก
                }
                ready.Add(item); // โปรแกรมพร้อมแล้ว จึงรวมคิวในชุดส่ง
            }
            if (IsDisposed) return results.ToArray(); // ปิดหน้าแล้วให้จบด้วยผลที่มีอยู่
            foreach (var (machine, token) in owned) // ตรวจทุกสิทธิ์ที่ชุดนี้จองไว้
                if (!ready.Any(i => i.Row.Machine == machine)) ReleaseDispatch(machine, token); // คืนสิทธิ์เครื่องที่ยังไม่พร้อมส่งในรอบนี้
            if (ready.Count > 0) ShowSending($"กำลังส่งไปที่ {string.Join(" / ", _dispatchingMachines.Keys)}"); // มีเครื่องพร้อมส่งจึงแสดงสถานะกำลังส่ง
            var sent = await MachineSendBatch.RunAsync(ready, i => i.Row.Machine, // เริ่มส่งคนละเครื่องพร้อมกัน
                async i => // กำหนดวิธีส่งสำหรับคิวแต่ละเครื่อง
                {
                    _activeStatusQueues.Add(i.Row.Id); // เก็บข้อความระหว่างส่งไว้ ไม่ให้รอบรีเฟรชลบทิ้ง
                    try // ดักข้อผิดพลาดของขั้นนี้
                    {
                        var result = await SendQueueStepAsync(i.Row, i.Job); // บันทึกสิทธิ์ส่งกับ Backend ก่อนส่งอุปกรณ์
                        return includeJobNames // เลือกว่าต้องแนบชื่อ Job ในผลไหม
                            ? result with { Lines = result.Lines.Select(l => l with { Text = $"{JobName(i.Row.PrintJobsId)} · {l.Text}" }).ToList() } // เติมชื่อ Job หน้าทุกบรรทัดผลส่ง
                            : result; // ใช้ผลเดิมเมื่อไม่ต้องแนบชื่อ Job
                    }
                    finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
                    {
                        _activeStatusQueues.Remove(i.Row.Id); // จบรอบส่งแล้ว ให้สถานะจาก Backend เป็นหลัก
                        ReleaseDispatch(i.Row.Machine, owned[i.Row.Machine]); // เครื่องนี้เสร็จแล้ว ไม่ต้องรอเครื่องอื่นเพื่อคืนสิทธิ์
                    }
                },
                (i, ex) => new StepSendResult(false, // จัดผลเมื่อการส่งเครื่องนี้มีข้อผิดพลาด
                    [Notify.Bad($"{i.Row.Machine} คิว {i.Row.Id}: ตรวจสอบผลก่อนส่งซ้ำ — {ex.Message}")], HeldForReview: true)); // ระบุคิวที่ต้องตรวจ ห้ามเดาผลแล้วส่งซ้ำ
            results.AddRange(sent); // รวมผลเครื่องที่ส่งจริงกับผลเตรียมก่อนหน้า
            return results.ToArray(); // ส่งผลของทุกคิวในชุดนี้กลับผู้เรียก
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            foreach (var (machine, token) in owned) ReleaseDispatch(machine, token); // คืนสิทธิ์ทุกเครื่องของชุดนี้เมื่อจบเสมอ
        }
    }

    private sealed record StepSendResult(bool Sent, List<Notify.ResultLine> Lines,
        bool SafeToRetry = false, object? Detail = null, bool HeldForReview = false);

    private async Task<StepSendResult> SendQueueStepAsync(MachineQueueRow row, ResolvedJobResponse resolved) // จดรอบส่ง ส่งเครื่อง แล้วบันทึกผล
    {
        using var lease = MachineBusy.TryHoldExclusive(row.Machine); // กันทางส่งอื่นในโปรแกรมเข้าเครื่องเดียวกันซ้อน
        if (lease == null) // มีทางอื่นกำลังใช้เครื่องเดียวกัน
            return new(false, [Notify.Note($"{row.Machine}: รอการส่งรอบปัจจุบันจบก่อน")], HeldForReview: true); // ถือคิวไว้รอให้รอบปัจจุบันจบก่อน
        SetMachineStatus(row, "กำลังเตรียมส่ง", AntdUI.TTypeMini.Primary); // แสดงว่าคิวกำลังเตรียมก่อนแตะเครื่อง
        var token = Guid.NewGuid().ToString(); // สร้างรหัสอ้างอิงเฉพาะรอบส่งนี้
        var (began, beginError) = await _api!.BeginQueueSendAsync(row.Id, token); // ขอให้ Backend จดหลักฐานก่อนแตะเครื่อง
        if (!began) // Backend ยังไม่ยืนยันว่าเริ่มรอบส่งได้
        {
            SetMachineStatus(row, "ตรวจสอบคิวก่อนส่งซ้ำ", AntdUI.TTypeMini.Warn); // แสดงว่าต้องตรวจคิวก่อนลองใหม่
            return new(false, [Notify.Bad($"{row.Machine}: ยังไม่ส่ง — {beginError}")], HeldForReview: true); // จบโดยไม่ยิงเครื่องและถือคิวไว้ตรวจ
        }
        SetMachineStatus(row, "กำลังส่ง", AntdUI.TTypeMini.Primary); // แสดงว่ารอบนี้เริ่มส่งข้อมูลเข้าเครื่องแล้ว

        StepSendResult result; // เตรียมเก็บผลส่งของเครื่องนี้
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            if (IsDisposed) result = new(false, [], SafeToRetry: true); // ปิดหน้าก่อนแตะเครื่อง จึงระบุว่ายังไม่ส่ง
            else result = await SendStepAsync(row.PrintJobsId, row.Machine, resolved, row.ProgramName); // ส่งข้อมูลตาม Job และโปรแกรมที่ผูกกับคิว
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            result = new(false, [Notify.Bad($"{row.Machine}: {ex.Message}")]); // เก็บปัญหาที่เกิดระหว่างส่งเครื่อง
        }

        var outcome = result.Sent ? "sent" : result.SafeToRetry ? "not_sent" : "unknown"; // แยกส่งแล้ว ยังไม่ส่งแน่นอน และยังยืนยันผลไม่ได้
        var error = string.Join(" · ", result.Lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text)); // รวมเฉพาะข้อความผิดพลาดไว้บันทึก Backend
        if (error.Length > 4000) error = error[..4000]; // จำกัดความยาวเหตุผิดพลาดไม่เกิน 4000 ตัว
        SetMachineStatus(row, "กำลังบันทึกผล", AntdUI.TTypeMini.Primary); // แสดงว่ารอ Backend บันทึกผลรอบส่ง
        var recorded = await _api.FinishQueueSendAsync(row.Id, token, outcome, result.Detail, error); // บันทึกผลคิวกับประวัติส่งในคำขอเดียว
        if (!recorded.ok) // บันทึกไม่ผ่าน จึงลองจดผลเดิมอีกครั้ง
            recorded = await _api.FinishQueueSendAsync(row.Id, token, outcome, result.Detail, error); // ลองบันทึกซ้ำด้วย token เดิม ไม่ยิงเครื่องซ้ำ

        bool held = !recorded.ok || outcome == "unknown"; // ยังบันทึกไม่ได้หรือผลไม่แน่นอน ต้องถือคิวไว้ตรวจ
        if (held) // คิวนี้ยังต้องค้างไว้ตรวจผล
            result.Lines.Add(Notify.Bad($"{row.Machine}: ถือคิว {row.Id} ไว้ตรวจผล ห้ามส่งซ้ำหรือปล่อยเครื่อง" // เตือนว่าห้ามส่งซ้ำหรือปล่อยคิวที่ไม่รู้ผล
                + (recorded.ok ? "" : $" · บันทึกผลไม่ได้: {recorded.error}"))); // แนบเหตุที่บันทึกผลไม่ผ่านถ้ามี
        SetMachineStatus(row, held ? "ต้องตรวจสอบก่อนส่งซ้ำ" : result.Sent ? SentStatus(row.Machine) : "ยังไม่ส่ง / ส่งไม่สำเร็จ", // ตั้งข้อความสถานะตามผลส่งและการถือคิว
            held ? AntdUI.TTypeMini.Warn : result.Sent ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Error); // เลือกสีเตือน ผ่าน หรือผิดพลาดให้ตรงผล
        return result with { HeldForReview = held }; // แนบสถานะรอตรวจกลับไปพร้อมผลเครื่อง
    }

    private async Task<StepSendResult> SendStepAsync( // เลือกส่ง MK หรือ UV ตามขั้นของงาน
        int jobId, string step, ResolvedJobResponse resolved, // รับ Job ขั้นที่ส่ง และข้อมูลล่าสุด
        string? forcedProgram = null) // รับโปรแกรมที่กำหนดไว้ล่วงหน้าถ้ามี
    {
        if (step == "MK") // เข้าทางส่งหัว MK และ PLC
        {
            var plcLines = new List<Notify.ResultLine>(); // เก็บผล PLC แยกจากผลพิมพ์ MK
            var plcTask = SendJobPlcAsync(resolved, plcLines); // เริ่มเขียนตำแหน่งหัวและสายพานไปพร้อมการส่ง MK
            var mk = await JobSendService.SendMkAsync(resolved.Pattern); // ส่ง Pattern ไปยังหัว MK ที่ตั้งค่าไว้

            try { await plcTask; } // รอผล PLC มารวม โดยไม่ส่ง MK ซ้ำ
            catch (Exception ex) { plcLines.Add(Notify.Careful($"PLC — {ex.Message}")); } // PLC มีปัญหาให้เก็บเป็นคำเตือนประกอบ

            var mkLines = Notify.MkLines(mk.Machines); // แปลงผล MK แต่ละหัวเป็นข้อความ

            if (mkLines.Count == 0) // ไม่มีหัว MK ที่ได้ผลส่งมาเลย
                mkLines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้")); // เตือนว่าหัว MK ยังไม่มี IP ใช้งาน

            var lines = new List<Notify.ResultLine>(plcLines); // เรียงผล PLC ก่อนผลหัว MK ให้คนอ่านตามได้
            lines.AddRange(mkLines); // รวมผล MK ต่อจากผล PLC

            bool ok = mk.Status == SendStatus.Ok; // ใช้ผลรวมการส่ง MK เป็นตัวตัดสินความสำเร็จ

            return new StepSendResult(ok, lines); // ส่งผลรวมโดยยึดความสำเร็จของ MK
        }

        int uvNumber = step == "UV1" ? 1 : 2; // เลือกหมายเลข UV ตามขั้นในแผน
        var uv = await JobSendService.SendUvAsync(this, uvNumber, resolved.UvJobData, forcedProgram, allowPrompt: false); // ส่ง UV ด้วยโปรแกรมที่เตรียมไว้ ไม่เปิดกล่องเลือกกลางชุด

        if (uv.Status == SendStatus.Ok) // UV ส่งผ่านครบตามเงื่อนไขใน Service
        {
            var detail = new // เตรียมข้อมูลโปรแกรมไว้บันทึกประวัติส่ง
            {
                requested = resolved.UvJobData.FirstOrDefault(r => r.Machine == step)?.ProgramName ?? "", // เก็บชื่อโปรแกรมต้นทางไว้เทียบกับที่เลือกใช้
                program = uv.ProgramFile, // เก็บโปรแกรมที่ส่งเข้าเครื่องจริง
                is_default = uv.UsedDefault, // เก็บว่าใช้โปรแกรมสำรองหรือไม่
                start_confirmed = uv.StartWarning == null, // แยกผลรับคำสั่ง Start ออกจากผลโหลดข้อมูล
                start_warning = uv.StartWarning, // เก็บเหตุที่ Start ไม่ยืนยันไว้ให้ตรวจที่เครื่อง
            };

            var lines = new List<Notify.ResultLine> // เตรียมข้อความผล UV สำหรับผู้ใช้
                { Notify.Ok($"{uv.MachineName} — ส่งข้อมูลแล้ว ({uv.ProgramFile}.uvdx)") }; // ระบุว่าโหลดข้อมูลโปรแกรมนี้เข้า UV แล้ว
            if (uv.StartWarning != null) // เครื่องยังไม่ยืนยันคำสั่ง Start
                lines.Add(Notify.Careful($"{uv.MachineName} — {uv.StartWarning} · ตรวจสถานะเริ่มพิมพ์ที่เครื่อง")); // ให้ตรวจการเริ่มพิมพ์จริงที่เครื่อง
            return new StepSendResult(true, lines, Detail: detail); // ยืนยันผลส่งข้อมูลพร้อมรายละเอียด UV
        }

        return new StepSendResult(false, uv.Status switch // แยกเหตุที่ส่ง UV ไม่สำเร็จ
        {
            SendStatus.Cancelled => [], // ผู้ใช้ยกเลิกเลือกโปรแกรม จึงไม่ต้องเด้งข้อความผิดพลาด
            SendStatus.Unreachable => // ต่อ UV ไม่ได้ ให้รายงานปลายทางที่ติดต่อ
                [Notify.Bad($"{uv.MachineName} — เชื่อมต่อไม่ได้ ({uv.Ip}:{uv.Port})")], // แสดง IP และพอร์ต UV ที่เชื่อมต่อไม่ได้
            _ => [Notify.Bad($"{uv.MachineName} — {uv.FailReason}")], // ปัญหาอื่นใช้เหตุที่ Service ส่งกลับมา
        }, SafeToRetry: uv.Status is SendStatus.Cancelled or SendStatus.Unreachable or SendStatus.NotConfigured); // ลองใหม่ได้เฉพาะกรณีที่ยังไม่ได้ส่งแน่นอน
    }

    private async Task StartWithoutSendingAsync(int jobId, string? markingMethod) // เริ่มงานที่ไม่มีขั้นส่งเข้าเครื่อง
    {
        if (!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ยืนยันเริ่มงานที่ไม่มีขั้นส่งเครื่อง
                $"{JobName(jobId)} — marking {Method(markingMethod)}\n\n" // ระบุงานและรหัสพิมพ์ที่จะเริ่ม
                + "งานนี้ไม่มีขั้นตอนต้องส่งเข้าเครื่อง จะเปลี่ยนสถานะเป็นกำลังผลิตอย่างเดียว\n\n" // บอกว่าจะเปลี่ยนสถานะอย่างเดียว
                + "ยืนยันหรือไม่?")) // ถามยืนยันก่อนเริ่มงานไม่มีขั้นส่ง
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        var (ok, err) = await _api!.UpdateJobStatusAsync(jobId, "Process"); // เปลี่ยน Job เป็น Process ที่ Backend
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (ok) Notify.Success(this, $"เริ่มงาน {JobName(jobId)} แล้ว"); // แจ้งว่าเปลี่ยนเป็นงานกำลังผลิตแล้ว
        else Notify.ErrorModal(this, "เริ่มงานไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้"); // แสดงเหตุที่เริ่มงานไม่ได้

        await RefreshDataAsync(force: true); // อ่าน Job และคิวใหม่โดยไม่ใช้ข้อมูลเดิม
    }

    private async Task RequestRemoteStartAsync( // ฝากขั้นและโปรแกรมให้ ST1 ส่งผ่านคิว
        int jobId, string step, ResolvedJobResponse resolved, bool askFirst = true) // รับงาน ขั้น ข้อมูล และตัวเลือกถามยืนยัน
    {
        int machineStation = JobStationService.StationOf(step) ?? 0; // หา Station ที่ดูแลขั้นเครื่องนี้
        if (StationOwner(machineStation, jobId) is { } busyJob) // ตรวจว่ามี Job อื่นครอง Station นั้นอยู่หรือไม่
        {
            Notify.WarnModal(this, "สถานีไม่ว่าง", // เตือนว่าสถานีปลายทางยังมีงาน
                $"ST{machineStation} มีงาน {JobLabel(busyJob)} อยู่\n\nต้องจบงานนั้นก่อนถึงจะเริ่มงานนี้ได้"); // ระบุงานที่ครองสถานีอยู่ก่อน
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        int uvNumber = step == "UV1" ? 1 : 2; // เลือกหมายเลข UV ตามขั้นในแผน
        var uvRow = resolved.UvJobData.FirstOrDefault(r => r.Machine == step); // อ่านข้อมูล UV ของขั้นที่ต้องส่ง

        var pick = UvProgramResolver.Resolve( // หาโปรแกรมจริงหรือให้ผู้ใช้เลือกรุ่นย่อยก่อนฝากส่ง
            uvRow?.ProgramName, UvSettingsManager.GetDocumentFolder(uvNumber), this); // ค้นจากโฟลเดอร์ UV ของจอที่กำลังส่งคำขอ

        if (pick.Program == null) return;   // ผู้ใช้ปิดกล่องเลือกรุ่นย่อย

        var uvName = UvSettingsManager.Read( // อ่านชื่อเครื่อง UV ตามค่าตั้ง
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}"); // เลือกชุดชื่อให้ตรงกับ UV1 หรือ UV2

        if (pick.IsDefault && // ตรวจว่าต้องยืนยันโปรแกรมสำรองไหม
            !UvProgramResolver.ConfirmDefault(uvRow?.ProgramName ?? "", uvName, this)) // ยืนยันว่าใช้โปรแกรมสำรองแทนชื่อจากงานได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        if (askFirst &&!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ถามยืนยัน Job และโปรแกรมที่จะให้ ST1 ส่ง
                $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}\n\n" // แสดงชื่อ Job และรหัสพิมพ์ก่อนฝากส่ง
                + $"ส่งไป {step} ด้วยโปรแกรม {pick.Program}.uvdx\n" // แสดงเครื่องและไฟล์โปรแกรมที่เลือก
                + "คำสั่งจะถูกส่งเข้าเครื่องโดยโปรแกรมที่ ST1\n\n" // บอกว่า ST1 เป็นผู้ส่งคำสั่งจริง
                + "ยืนยันหรือไม่?")) // ถามยืนยันการฝากส่ง
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป

        var (ok, err) = await _api!.SetRemoteStartAsync( // บันทึกคำขอให้ ST1 มารับงานนี้
            jobId, requested: true, pick.Program, step: step); // ฝากทั้ง Job, โปรแกรม และขั้น UV ที่ต้องการให้ ST1 ส่ง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
        {
            await _api.UpdateJobStatusAsync(jobId, "Waiting"); // ฝากคำขอไม่ผ่าน ให้ขอคืนสถานะ Waiting
            Notify.ErrorModal(this, "ส่งคำขอไม่สำเร็จ", err ?? "ไม่สามารถฝากคำขอไว้ที่ ST1 ได้"); // แจ้งเหตุที่ฝากคำขอไป ST1 ไม่ได้
            await RefreshDataAsync(force: true); // อ่าน Job และคิวใหม่โดยไม่ใช้ข้อมูลเดิม
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        _sendOperations++; // นับชุดส่งที่กำลังทำงานเพิ่ม
        ShowSending($"กำลังส่งไปที่ ST1 · {JobName(jobId)}"); // บอกผู้ใช้ว่ากำลังรอให้ ST1 ส่ง
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            await ShowRemoteOutcomeAsync(jobId, step); // ติดตามประวัติส่งและข้อผิดพลาดที่ ST1 ฝากกลับมา
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            EndSending(); // จบชุดส่งแล้วตรวจงานที่ค้างอยู่
            SchedulePendingRefresh(); // นัดอ่านรายการที่ค้างรอรีเฟรช
            ShowSending(null); // ปิดข้อความรอผลฝากส่ง
        }

        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านสถานะใหม่ถ้าหน้ายังเปิดอยู่
    }

    private void ShowSending(string? text)
    {
        UpdateMachineStatusCells();
    }

    private static readonly TimeSpan RemoteOutcomeWait = TimeSpan.FromSeconds(40);

    private async Task ShowRemoteOutcomeAsync(int jobId, string step) // รอผลส่งที่ ST1 บันทึกกลับมา
    {
        var deadline = DateTime.UtcNow + RemoteOutcomeWait; // กำหนดเวลาสูงสุดที่หน้าจอจะรอผลรอบนี้

        while (DateTime.UtcNow < deadline) // ติดตามผลจนสำเร็จ มีปัญหา หรือครบเวลารอ
        {
            await Task.Delay(700); // เว้น 700 ms ก่อนอ่านผลครั้งถัดไป
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

            var job = await _api!.GetJobByIdAsync(jobId); // อ่าน Job ล่าสุดรวมประวัติส่งและ remote_error
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
            if (job == null) continue; // อ่าน Job ไม่ได้ในรอบนี้ ให้ลองใหม่ภายในเวลาที่เหลือ

            var failure = job.RemoteError?.Trim(); // อ่านเหตุที่ ST1 ส่งไม่สำเร็จ
            if (!string.IsNullOrEmpty(failure)) // มีข้อผิดพลาดที่ ST1 ฝากกลับมา
            {
                await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอฝากส่งของ Job นี้
                if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

                Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Bad(failure)]); // แสดงผลผิดพลาดที่ ST1 ฝากกลับมา
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            bool sent = job.Commands?.Any(c => c.Success && // ค้นประวัติส่งสำเร็จของขั้นที่ขอ
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true; // นับเฉพาะประวัติของขั้นที่กำลังรอ
            if (!sent) continue; // ยังไม่มีประวัติสำเร็จของขั้นนี้ ให้รอต่อ

            var uvName = UvSettingsManager.Read(step == "UV1" ? "UV1_NAME" : "UV2_NAME", step); // ใช้ชื่อ UV ที่ตั้งไว้แสดงผล
            Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Ok($"{uvName} — ส่งสำเร็จ")]); // แสดงว่า ST1 มีประวัติส่งขั้นนี้ผ่านแล้ว
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var final = await _api!.GetJobByIdAsync(jobId); // ครบเวลารอแล้ว อ่านสถานะอีกครั้งก่อนตัดสิน
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        if (final?.RemoteStart == RemoteSending) // ST1 รับคำขอไปแล้วแต่ยังไม่จบการส่ง
        {
            Notify.WarnModal(this, "ST1 กำลังส่งอยู่", // แจ้งว่าคำขอยังอยู่ระหว่างส่งที่ ST1
                $"{JobName(jobId)}\n\n" // ระบุ Job ที่กำลังรอผล
                + "ST1 รับคำขอไปแล้วและกำลังส่งเข้าเครื่อง แต่ใช้เวลานานกว่าปกติ\n\n" // บอกว่า ST1 รับแล้วแต่ยังส่งไม่จบ
                + "งานยังเดินอยู่ ไม่ต้องกดซ้ำ — รอผลอีกสักครู่"); // ให้รอผลแทนกดคำขอซ้ำ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอฝากส่งของ Job นี้
        await _api.UpdateJobStatusAsync(jobId, "Waiting"); // ขอคืนสถานะ Waiting เมื่อยังไม่มีผู้รับคำขอ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        Notify.WarnModal(this, "ST1 ไม่รับคำขอ", // แจ้งว่าครบเวลารอแล้วยังไม่มีผู้รับ
            $"{JobName(jobId)}\n\n" // ระบุ Job ที่กำลังรอผล
            + $"รอมา {RemoteOutcomeWait.TotalSeconds:0} วินาทีแล้วยังไม่มีใครรับไปส่ง\n" // แสดงเวลาที่รอคำขอไปแล้ว
            + "งานถูกตีกลับเป็นรอเริ่มแล้ว ยังไม่มีอะไรถูกส่งเข้าเครื่อง\n\n" // แสดงข้อความคืนรอของทางหมดเวลาเดิม
            + "ตรวจว่าโปรแกรมที่เครื่อง ST1 เปิดอยู่และต่อ Backend ได้ แล้วกดเริ่มงานใหม่"); // ให้ตรวจโปรแกรม ST1 และการต่อ Backend
    }

    private const string RemotePending = "1";

    private const string RemoteSending = "2";

    private readonly HashSet<int> _remoteInFlight = new();

    private async Task RecoverAbandonedRemoteStartsAsync()
    {
        if (_api == null || StationService.IsSt3) return;

        var abandoned = _allJobs
            .Where(j => j.RemoteStart == RemoteSending && !_remoteInFlight.Contains(j.Id))
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in abandoned)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false,
                failure: "ST1 ปิดกลางคันระหว่างส่ง — งานถูกคืนเป็นรอเริ่ม");
            await _api.UpdateJobStatusAsync(jobId, "Waiting");

            if (IsDisposed) return;
        }
    }

    private async Task<bool> ProcessMachineQueueAsync(List<MachineQueueRow>? snapshot = null, string? machineFilter = null) // ST1 รับคิวที่ได้สิทธิ์แล้ว ทั้งงานจาก ST1 และ ST3
    {
        if (_api == null || StationService.IsSt3 || IsDisposed || _preparingPrograms) return false; // เฉพาะ ST1 ที่พร้อมและไม่ได้เลือกโปรแกรมจึงส่งคิวได้
        var owned = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase); // จำสิทธิ์ส่งแยกเครื่องไว้คืนตอนจบ
        _sendOperations++; // นับชุดส่งที่กำลังทำงานเพิ่ม
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var (rows, error) = snapshot == null // ใช้คิวจากรอบอ่านเดิมได้ ถ้าไม่มีจึงขอ Backend ใหม่
                ? await _api.GetMachineQueueAsync() // ไม่มี snapshot จึงอ่านคิวจาก Backend
                : (snapshot, (string?)null); // มีคิวจากรอบอ่านอยู่แล้ว ใช้ชุดนั้นต่อ
            if (error != null || IsDisposed) return false; // อ่านคิวไม่ได้หรือปิดหน้าแล้ว ไม่ส่งต่อ
            var review = rows.Where(r => r.NeedsSendReview && !_dispatchingMachines.ContainsKey(r.Machine) && _reportedUncertainQueues.Add(r.Id)).ToList(); // หาคิวที่ไม่รู้ผลและยังไม่ได้แจ้งเตือนในจอนี้
            if (review.Count > 0) // มีคิวค้างตรวจผลอย่างน้อยหนึ่งรายการ
                Notify.Warn(this, string.Join(" / ", review.Select(r => $"{r.Machine} คิว {r.Id}")) // แสดงเครื่องและเลขคิวที่ต้องตรวจ
                    + ": กำลังส่งหรือรอตรวจสอบผล ระบบจะไม่ส่งซ้ำเอง"); // บอกว่าระบบจะไม่ส่งคิวไม่รู้ผลซ้ำเอง

            var ready = rows.Where(r => r.State == "active" && r.SentAt == null && !r.NeedsSendReview // ส่งเฉพาะ active ที่ยังไม่ส่งและไม่มีผลค้างตรวจ
                    && (machineFilter == null || string.Equals(r.Machine, machineFilter, StringComparison.OrdinalIgnoreCase))) // หลังปล่อยปุ่ม ให้รับต่อเฉพาะเครื่องที่เพิ่งว่าง
                .OrderBy(r => r.Id) // เรียงคิวที่ได้สิทธิ์ตามเลขคิว
                .Where(r => !MachineBusy.IsBusy(r.Machine) && ReserveDispatch(r.Machine)).ToList(); // กันทางอื่นใช้เครื่องเดียวกันระหว่างเตรียมส่ง
            foreach (var row in ready) owned[row.Machine] = _dispatchingMachines[row.Machine]; // จำสิทธิ์ที่ชุดนี้ถือไว้ เพื่อปล่อยเฉพาะของตัวเอง
            var prepared = new List<PreparedQueueSend>(); // เตรียมคิวพร้อมรายละเอียด Job สำหรับส่ง
            var lines = new List<Notify.ResultLine>(); // เตรียมเก็บผลแต่ละขั้นไว้รายงานรวม
            foreach (var group in ready.GroupBy(r => r.PrintJobsId)) // รวมคิวของ Job เดียวกันเพื่ออ่านรายละเอียดครั้งเดียว
            {
                var resolved = await _api.GetResolvedJobAsync(group.Key); // อ่าน Pattern และ UV ล่าสุดของ Job ในคิว
                if (IsDisposed) return false; // ปิดหน้าแล้ว ไม่เริ่มส่งเครื่องต่อ
                foreach (var row in group) // จับข้อมูล Job ให้แต่ละคิวในกลุ่ม
                {
                    if (resolved != null) prepared.Add(new(row, resolved)); // จับคิวกับข้อมูล Job ก่อนส่ง
                    else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
                    {
                        var reset = await _api.UpdateMachineQueueAsync(row.Id, state: "pending"); // อ่าน Job ไม่ได้ ให้ขอคืนคิวนี้ไปรอก่อน
                        lines.Add(Notify.Bad($"{row.Machine} คิว {row.Id}: โหลดข้อมูลงานไม่ได้" // จดว่าโหลดข้อมูลของ Job ในคิวไม่ได้
                            + (reset.ok ? " คืนเข้าคิวรอแล้ว" : $" · คืนคิวไม่ได้: {reset.error}"))); // แสดงผลว่าคืนคิวไปรอได้หรือมีปัญหา
                    }
                }
            }
            var results = await SendPreparedBatchAsync(prepared, includeJobNames: true, machinesReserved: true); // ใช้สิทธิ์ที่จองไว้ส่งเครื่องที่พร้อมพร้อมกัน
            lines.AddRange(results.SelectMany(r => r.Lines)); // รวมข้อความผลส่งของทุกเครื่อง
            if (!IsDisposed) _sendReports.AddRange(lines); // เก็บผลไว้รายงานรวมหลังเริ่มส่งเครื่องที่พร้อมครบแล้ว
            return ready.Count > 0; // บอกว่ารอบนี้มีคิวที่พร้อมประมวลผลไหม
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            foreach (var (machine, token) in owned) ReleaseDispatch(machine, token); // คืนสิทธิ์ของชุดนี้เสมอ แม้เตรียมหรือส่งบางเครื่องไม่ผ่าน
            EndSending(); // จบชุดส่งแล้วตรวจงานที่ค้างอยู่
            SchedulePendingRefresh(); // ให้รายการที่รอรีเฟรชตามหลังการส่งทำงานต่อ
            if (!IsDisposed && !_sending) ShowSending(null); // ปิดข้อความรอเมื่อไม่มีงานส่งค้าง
        }
    }

    private async Task ProcessRemoteStartsAsync()
    {
        if (_api == null || StationService.IsSt3 || _sending) return;

        var pending = _allJobs // เตรียม Job ที่มีคำขอจาก ST3
            .Where(j => j.RemoteStart == RemotePending && !_remoteInFlight.Contains(j.Id)) // เลือกธงรอส่งที่โปรแกรมนี้ยังไม่ได้กำลังจัดการ
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in pending) // จัดการคำขอทีละ Job
        {
            _remoteInFlight.Add(jobId); // จำว่าโปรแกรมนี้กำลังจัดการ Job นี้อยู่
            _sendOperations++;
            try
            {
                await RunRemoteStartAsync(jobId); // ตรวจคำขอแล้วส่งขั้นที่ ST3 ระบุ
            }
            finally
            {
                EndSending();
                SchedulePendingRefresh();
                _remoteInFlight.Remove(jobId); // เอา Job ออกจากชุดที่โปรแกรมนี้กำลังจัดการ
                if (!IsDisposed && !_sending) ShowSending(null);
            }

            if (IsDisposed) return;
        }
    }

    private async Task RunRemoteStartAsync(int jobId) // ST1 ตรวจคำขอจาก ST3 แล้วเข้าทางคิวปกติ
    {
        var resolved = await _api!.GetResolvedJobAsync(jobId); // อ่าน Job ล่าสุดก่อนรับคำขอจาก ST3
        if (resolved == null) return;   // อ่านไม่ได้ = คงธงไว้ให้รอบหน้าลองใหม่

        var status = resolved.Job.Status; // ตรวจสถานะจริงของ Job จาก Backend
        bool live = string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase) // รับคำขอของงานที่ยังรอเริ่ม
                 || string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase); // หรือของงานที่กำลังผลิตและมีขั้นถัดไป

        if (!live) // Job ไม่ได้อยู่ Waiting หรือ Process แล้ว
        {
            await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอฝากส่งของ Job นี้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod); // อ่านแผนเครื่องที่ Job นี้ใช้จริง

        var requested = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteStep; // อ่านชื่อขั้นที่ ST3 ฝากมา
        var step = string.IsNullOrWhiteSpace(requested) // ดูว่าคำขอระบุขั้นมาหรือไม่
            ? plan.Steps.FirstOrDefault() // ไม่ระบุขั้นมา ให้ดูขั้นแรกของแผน
            : plan.Steps.FirstOrDefault(x => // ระบุขั้นมา ให้ค้นเฉพาะขั้นนั้น
                string.Equals(x, requested.Trim(), StringComparison.OrdinalIgnoreCase)); // เทียบชื่อขั้นโดยไม่สนตัวพิมพ์

        if (step == null) // แผนไม่มีขั้นที่ขอให้ส่ง
        {
            await _api.SetRemoteStartAsync(jobId, requested: false, // ล้างคำขอที่ทำตามแผนไม่ได้
                failure: string.IsNullOrWhiteSpace(requested) // แยกคำขอว่างกับชื่อขั้นที่ไม่ถูกต้อง
                    ? null // คำขอว่างไม่แนบเหตุชื่อขั้นผิด
                    : $"งานนี้ไม่มีขั้นตอน {requested.Trim()} ให้ส่ง"); // แจ้งว่าขั้นที่ระบุไม่อยู่ในงานนี้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (resolved.Commands?.Any(c => c.Success && // ตรวจว่ามีประวัติส่งขั้นนี้ผ่านแล้วไหม
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true) // เทียบชื่อคำสั่งกับขั้นที่ขอ
        {
            await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอฝากส่งของ Job นี้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        int machineStation = JobStationService.StationOf(step) ?? 0; // หา Station ที่ดูแลขั้นเครื่องนี้
        if (StationOwner(machineStation, jobId) != null) return; // ยังมี Job อื่นครอง Station ให้คงคำขอไว้รอรอบหน้า

        var program = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteProgram; // ใช้ชื่อโปรแกรมที่ ST3 เลือกฝากไว้

        await _api.ClaimRemoteStartAsync(jobId, program, step); // เปลี่ยนธงเป็นกำลังส่ง เพื่อให้ ST3 รู้ว่ารับแล้ว
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        var (queued, queueError) = await _api.EnqueueMachinesAsync(jobId, // นำคำขอฝากส่งเข้าระบบคิวก่อน
            [new MachineQueueItem { Machine = step, Round = 1, ProgramName = program }]); // สร้างคิวขั้นที่ขอพร้อมโปรแกรมที่ ST3 เลือก
        var (claim, claimError) = queued // ตรวจผลจองก่อนขอสิทธิ์เครื่อง
            ? await _api.ClaimMachineAsync(step, jobId) // ขอสิทธิ์เฉพาะ Job และเครื่องนี้
            : (null, queueError); // จองไม่ผ่านให้ใช้เหตุจองคิวแทน
        List<Notify.ResultLine> lines; // เตรียมผลของคำขอฝากส่ง
        if (claim?.Claimed is { } row) // Backend ยกสิทธิ์คิวให้คำขอนี้แล้ว
            lines = (await SendPreparedBatchAsync([new(row, resolved)])).SelectMany(r => r.Lines).ToList(); // ส่งผ่านชุดปกติที่จด token ก่อนแตะเครื่อง
        else if (claimError == null && claim?.Reason is "busy" or "queued") // เครื่องติดงานหรือยังไม่ถึงคิว ไม่ถือว่าผิดพลาด
            lines = [Notify.Note($"{step}: เข้าคิวแล้ว รอตามลำดับคิว")]; // แสดงว่าเก็บคิวรอไว้แล้ว
        else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
            lines = [Notify.Bad(claimError ?? "เครื่องยังไม่ว่างหรือคิวถูกส่งแล้ว กรุณาตรวจสถานะงาน")]; // แสดงเหตุที่ยังรับเครื่องไปส่งไม่ได้

        bool failed = lines.Any(l => l.Kind == Notify.ResultKind.Error); // ดูว่าผลส่งมีข้อความผิดพลาดหรือไม่
        var failure = failed // เตรียมเหตุที่ต้องส่งกลับให้ ST3
            ? string.Join(" · ", lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text)) // รวมเหตุผิดพลาดที่จะฝากกลับ ST3
            : null; // ไม่มีปัญหาให้ล้างข้อความผิดพลาดเดิม

        await _api.SetRemoteStartAsync(jobId, requested: false, failure: failure); // จบคำขอและบันทึกผลไว้ให้ ST3 อ่าน

        if (IsDisposed || lines.Count == 0) return; // ถ้าปิดหน้าแล้วหรือไม่มีผลให้แจ้งก็หยุด

        var text = $"{JobName(jobId)} — {lines[0].Text} (คำขอจาก ST3)"; // ระบุ Job และผลส่งว่าเป็นคำขอจาก ST3

        if (failed) Notify.Warn(this, text); // แจ้งเตือนแบบไม่ค้างรอคนปิดที่ ST1
        else Notify.Success(this, text); // แจ้งผลเมื่อส่งงานสำเร็จ
    }

    private bool _showingRemoteError;

    private async Task ShowRemoteErrorsAsync() // ให้ ST3 แสดงปัญหาที่ ST1 ส่งกลับมา
    {
        if (_api == null || !StationService.IsSt3 || _showingRemoteError) return; // ข้ามเมื่อ API ยังไม่พร้อม ไม่ใช่ ST3 หรือกำลังแจ้งปัญหาอยู่

        var failed = _allJobs.FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.RemoteError)); // หางานที่ ST1 ฝากเหตุส่งไม่สำเร็จไว้
        if (failed == null) return; // หยุดเมื่อไม่มีงานส่งพลาด

        var message = failed.RemoteError!; // เก็บข้อความไว้ก่อนล้างค่าที่ Backend

        await _api.SetRemoteStartAsync(failed.Id, requested: false); // เคลียร์คำขอเริ่มงานที่ส่งพลาด
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        _showingRemoteError = true; // กันเปิดกล่องแจ้งปัญหาซ้อนกัน
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            Notify.ErrorModal(this, "ST1 ส่งงานไม่สำเร็จ", // เปิดกล่องแจ้งว่า ST1 ส่งงานไม่สำเร็จ
                $"{JobLabel(failed)}\n\n{message}\n\n" // ใส่ชื่องานกับสาเหตุที่ส่งพลาด
                + "งานถูกตีกลับเป็นรอเริ่ม กดเริ่มงานใหม่ได้"); // บอกให้กดเริ่มงานใหม่หลังงานกลับไปรอ
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            _showingRemoteError = false; // เปิดให้แจ้งปัญหางานถัดไปได้เมื่อปิดกล่องแล้ว
        }
    }

    private PrintJob? StationOwner(int station, int exceptJobId)
    {
        if (station == 0) return null;

        foreach (var job in _allJobs)
        {
            if (job.Id == exceptJobId) continue;
            if (!string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)) continue;
            if (JobStationService.Current(job.Commands) == station) return job;
        }

        return null;
    }

    private async Task CompleteJobAsync(int jobId) // ตรวจขั้นที่ส่งแล้วก่อนบันทึกจบงาน
    {
        if (_api == null) return; // ยังไม่มีตัวเรียก Backend ให้หยุดก่อน

        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่านประวัติส่งล่าสุดของ Job
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ
        if (resolved == null) // อ่านรายละเอียด Job ไม่ได้
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้"); // แจ้งว่าโหลดงานที่จะจบไม่ได้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var method = resolved.PlanRouting?.MarkingMethod; // ใช้รหัสพิมพ์ตรวจสิทธิ์จบงาน
        if (!MarkingMethodService.CanCompleteAt(StationService.Current, method)) // ตรวจสิทธิ์จบงานของ Station ปัจจุบัน
        {
            Notify.WarnModal(this, "จบงานที่สถานีนี้ไม่ได้", // แจ้งว่าสถานีนี้จบงานประเภทนี้ไม่ได้
                $"{JobName(jobId)} — marking {Method(method)}\n\n" // แสดงชื่องานและวิธีพิมพ์ที่เลือก
                + "งาน marking 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น"); // บอกว่างานกลุ่มนี้ต้องจบที่ ST3
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var steps = CheckSteps(method, resolved.Commands); // เทียบแผนกับประวัติส่ง รวมจำนวนรอบของเครื่องเดิม

        bool manual = !steps.Complete; // ถ้าประวัติยังไม่ครบ ต้องจบแบบยืนยันด้วยมือ
        if (manual) // แยกกรณียืนยันจบทั้งที่ส่งไม่ครบ
        {
            var list = string.Join(", ", steps.Missing); // รวมชื่อขั้นที่ยังไม่มีประวัติครบ
            if (!Confirm.Ask(this, "งานยังส่งไม่ครบ", // ให้ยืนยันว่าจะจบทั้งที่ส่งยังไม่ครบ
                    $"{JobName(jobId)} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\n" + // แสดงขั้นตอนที่ยังส่งไม่ครบ
                    "ยืนยันจบงานทั้งที่ยังส่งไม่ครบหรือไม่?")) // ให้คนกดยืนยันว่าจะจบงานนี้
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }
        else if (!Confirm.Ask(this, "ยืนยันจบงาน", // ถามยืนยันสำหรับงานที่ส่งครบแล้ว
                     $"จบงาน {JobName(jobId)}\n\nยืนยันหรือไม่?")) // แสดงชื่องานที่จะจบให้ตรวจอีกครั้ง
        {
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success"); // เปลี่ยนเป็น Success และล้างคิวที่ Backend
        if (ok) // ทำต่อเมื่อบันทึกจบงานสำเร็จ
        {
            if (manual && !await _api.SaveSendStepAsync(jobId, "MANUAL_COMPLETE")) // จบด้วยมือแล้วบันทึกประวัติการยืนยัน
                Notify.Warn(this, "จบงานแล้ว แต่บันทึกประวัติการยืนยันด้วยมือไม่สำเร็จ"); // แจ้งว่าจบงานแล้วแต่เก็บประวัติยืนยันไม่ได้
            Notify.Success(this, manual // เลือกข้อความแจ้งตามวิธีจบงาน
                ? $"{JobName(jobId)} จบงานแล้ว (ยืนยันด้วยมือ)" // ระบุว่างานนี้ยืนยันจบด้วยมือ
                : $"{JobName(jobId)} จบงานแล้ว"); // แจ้งจบงานตามปกติ
            await RefreshDataAsync(); // อ่านรายการใหม่ให้งานที่จบออกจาก List
        }
        else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้"); // แสดงสาเหตุที่บันทึกจบงานไม่ได้
        }
    }

    private readonly record struct StepStatus(bool Complete, List<string> Missing);

    private static StepStatus CheckSteps(string? markingMethod, List<CommandResult>? commands)
    {
        var need = MarkingMethodService.MissingSteps(markingMethod, commands); // ตรวจขั้นที่ยังขาด โดยนับประวัติให้ครบจำนวนรอบ
        return new StepStatus(need.Count == 0, need);
    }

    private static AntdUI.Table.CellStyleInfo? TblOrders_SetRowStyle(
        object sender, AntdUI.TableSetRowStyleEventArgs e)
    {
        if (e.Record is not OrderRow row || row.Back is not Color back) return null;
        return new AntdUI.Table.CellStyleInfo { BackColor = back };
    }

    private static List<string> GetRequiredSteps(string markingMethod) =>
        MarkingMethodService.Resolve(markingMethod).Steps;

    private async Task ShowDetailDialogAsync(ResolvedJobResponse resolved) // เปิดรายละเอียดและรับคำขอส่งจากหน้าต่าง
    {
        string? requestedStep; // เก็บขั้นที่ผู้ใช้ขอให้ ST1 ส่งจากหน้า Detail

        using (var dlg = new OrderDetailDialog()) // เปิดหน้าต่างรายละเอียดเฉพาะ Job นี้
        {
            dlg.TitleText = $"{OrderDetailUserControl.JobTitle(resolved.Job)} — Order Detail"; // ตั้งหัวหน้าต่างให้บอกงานที่กำลังดู
            dlg.Text = dlg.TitleText; // ตั้งชื่อหน้าต่างให้ตรงกับหัวข้อรายละเอียด
            dlg.LoadDetail(resolved, _api); // เติมรายละเอียดงานและตัวเชื่อม Backend ให้หน้ารายละเอียด
            dlg.ShowDialog(this); // รอให้ผู้ใช้ดูรายละเอียดหรือกดขอส่งแล้วปิดหน้าต่าง
            requestedStep = dlg.RemoteStartStep; // รับขั้นที่ผู้ใช้กดขอให้ ST1 ส่ง
        }

        if (requestedStep == null || _api == null || IsDisposed) return; // ปิดเฉย ๆ หรือหน้าหลักไม่พร้อม จึงไม่ฝากคำขอ

        await RequestRemoteStartFromDetailAsync(resolved.Job.Id); // อ่านข้อมูลสดและตรวจขั้นอีกครั้งก่อนฝากส่ง
    }

    private async Task RequestRemoteStartFromDetailAsync(int jobId) // ส่งคำขอจากหน้ารายละเอียดให้ ST1 เริ่มงาน
    {
        var resolved = await LoadJobAsync(jobId, $"กำลังตรวจสอบงาน · {JobName(jobId)}"); // อ่านประวัติใหม่ เผื่อมีคนส่งไปแล้วระหว่างเปิด Detail
        if (resolved == null || IsDisposed) return; // หยุดเมื่อโหลดงานไม่ได้หรือปิดหน้าแล้ว

        var steps = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps; // อ่านขั้นที่งานนี้ต้องส่งจากวิธีพิมพ์
        int next = steps.FindIndex(step => !SentAlready(resolved, step)); // หาขั้นแรกที่ยังไม่มีประวัติส่งสำเร็จ

        if (next <= 0) // เช็คว่ายังมีขั้นถัดไปให้ส่งหรือไม่
        {
            Notify.WarnModal(this, "ไม่มีขั้นที่ต้องส่ง", // แจ้งว่าไม่มีขั้นตอนค้างส่ง
                $"{JobName(jobId)} ไม่มีขั้นถัดไปที่รอ ST1 ส่งแล้ว\n\n" // ระบุงานที่ไม่มีขั้นถัดไปรอ ST1
                + "อาจมีคนกดปุ่มหน้างานไปก่อนหน้านี้"); // บอกว่าปุ่มหน้างานอาจทำขั้นนี้ไปแล้ว
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        await RequestRemoteStartAsync(jobId, steps[next], resolved, askFirst: true); // ให้เลือกโปรแกรมและยืนยันก่อนฝากขั้นถัดไปให้ ST1
    }

    private const string TimeFormat = ThaiTime.Format;

    private const string Dash = "-";

    private static DateTime ToUtcFromThai(DateTime thai) => ThaiTime.ToUtc(thai);

    private static string FormatThaiTime(DateTime? utc) => ThaiTime.Text(utc, TimeFormat, Dash);

    private static int CompareCellText(string x, string y)
    {
        if (TryParseCellTime(x, out var tx) && TryParseCellTime(y, out var ty))
            return tx.CompareTo(ty);

        return CompareNatural(x, y);
    }

    private static bool TryParseCellTime(string? text, out DateTime value) =>
        DateTime.TryParseExact(
            (text ?? "").Trim(), TimeFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out value);

    private static int CompareNatural(string? a, string? b)
    {
        string x = a ?? "", y = b ?? "";
        int i = 0, j = 0;

        while (i < x.Length && j < y.Length)
        {
            if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
            {
                int si = i, sj = j;
                while (i < x.Length && char.IsDigit(x[i])) i++;
                while (j < y.Length && char.IsDigit(y[j])) j++;

                var nx = x.Substring(si, i - si).TrimStart('0');
                var ny = y.Substring(sj, j - sj).TrimStart('0');

                if (nx.Length != ny.Length) return nx.Length - ny.Length;
                int digits = string.CompareOrdinal(nx, ny);
                if (digits != 0) return digits;
            }
            else
            {
                int ch = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (ch != 0) return ch;
                i++;
                j++;
            }
        }

        return (x.Length - i) - (y.Length - j);
    }

    private static string Method(string? markingMethod)
    {
        var value = (markingMethod ?? "").Trim();
        return value.Length == 0 ? Dash : value;
    }

    private static string MachineCell(MarkingMachine machine) =>
        machine == MarkingMachine.None ? Dash : MarkingMethodService.Label(machine);

    private static string OrDashStation(int? station)
    {
        var label = JobStationService.Label(station);
        return label.Length == 0 ? Dash : label;
    }

    private OrderRow ToRow(PrintJob job, bool isHistory)
    {
        var plan = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod);

        var (statusLabel, statusColor) = JobStatusDisplay.Resolve(
            job.Status,
            MarkingMethodService.FinishedIncomplete(
                job.Status, job.PlanRouting?.MarkingMethod, job.Commands));

        var statusText = new AntdUI.CellText(statusLabel) { Fore = statusColor };

        var buttons = new List<AntdUI.CellButton>();
        if (!isHistory)
        {
            if (CanStart(job))
            {
                buttons.Add(new AntdUI.CellButton("start", StartButtonText, AntdUI.TTypeMini.Primary)
                { Radius = 6 });
            }
            else if (NotMyTurnYet(job))
            {
            }
            else if (MarkingMethodService.CanCompleteAt(
                         StationService.Current, job.PlanRouting?.MarkingMethod))
            {
                var steps = CheckSteps(job.PlanRouting?.MarkingMethod, job.Commands);
                buttons.Add(new AntdUI.CellButton("complete", "จบงาน",
                    steps.Complete ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Warn)
                { Radius = 6 });
            }

            buttons.Add(new AntdUI.CellButton("cancel", "", AntdUI.TTypeMini.Error)
            { Radius = 6, IconSvg = "CloseOutlined" });
        }
        else if (IsCancelled(job))
        {
            buttons.Add(new AntdUI.CellButton("restore", "พิมพ์ใหม่", AntdUI.TTypeMini.Primary)
            { Radius = 6 });
        }
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Default) { Radius = 6, IconSvg = "SearchOutlined" });

        bool finished =
            string.Equals(job.Status, "Success", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(job.Status, "Cancel", StringComparison.OrdinalIgnoreCase);

        return new OrderRow
        {
            Id = job.Id,
            Start = FormatThaiTime(job.CreatedAt),
            End = finished ? FormatThaiTime(job.UpdatedAt) : Dash,
            ErpMfg = job.OrderNo ?? "",
            LotNo = FirstFilled(job.LotNumber, job.BarcodeRaw),
            Qty = job.Qty?.ToString() ?? "",
            ProcessSequence = StationService.ShowProcessTabs
                && JobProcessService.BetweenRounds(job.PlanRouting?.MarkingMethod, job.Commands)
                    ? JobProcessService.Offline
                    : job.PlanRouting?.ProcessSequence ?? "",
            Plate = plan.NoCase ? Dash : MachineCell(plan.Plate),
            Shim = plan.NoCase ? Dash : MachineCell(plan.Shim),
            Station = OrDashStation(JobStationService.Current(job.Commands)),
            Status = statusText,
            MachineStatus = BuildMachineStatus(job),
            Op = buttons.ToArray(),
            Back = string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)
                ? DesignTokens.RowSuccess
                : null,
        };
    }

    private int? _selectedJobId;

    private int? _processingJobId;
    private long _processingStamp;

    private void WirePanels()
    {
        tblOrders.CellClick += TblOrders_CellClick;

        picPrevPlate.Click += (_, _) => OpenSidePicker(picPrevPlate, lblPrevPlateCaption);
        picPrevShim.Click += (_, _) => OpenSidePicker(picPrevShim, lblPrevShimCaption);
        picProcPlate.Click += (_, _) => OpenSidePicker(picProcPlate, lblProcPlateCaption);
        picProcShim.Click += (_, _) => OpenSidePicker(picProcShim, lblProcShimCaption);

        ShowPreviewSides(null, null);
        ShowProcessingSides(null, null);
    }

    private async void TblOrders_CellClick(object? sender, AntdUI.TableClickEventArgs e)
    {
        if (e.RowType != AntdUI.RowType.None) return;
        if (e.Column?.Key == "Op") return;
        if (e.Record is not OrderRow row) return;
        if (_selectedJobId == row.Id) return;

        _selectedJobId = row.Id;
        await UpdatePreviewAsync();
    }

    private async Task UpdatePreviewAsync()
    {
        var job = _selectedJobId is int id
            ? _allJobs.FirstOrDefault(j => j.Id == id)
            : null;

        if (job == null)
        {
            ShowPreviewSides(null, null);
            return;
        }

        var sides = await BuildSidesAsync(job);
        if (IsDisposed) return;

        ShowPreviewSides(Find(sides, "Plate"), Find(sides, "Shim"));
    }

    private async Task UpdateProcessingAsync()
    {
        var job = _showHistory ? null : JobInMyMachine() ?? NewestProcessJob();

        if (job == null)
        {
            _processingJobId = null;
            _processingStamp = 0;
            ShowProcessingSides(null, null);
            return;
        }

        long stamp = job.UpdatedAt?.Ticks ?? 0;
        if (_processingJobId == job.Id && _processingStamp == stamp) return;

        _processingJobId = job.Id;
        _processingStamp = stamp;

        var sides = await BuildSidesAsync(job);
        if (IsDisposed) return;

        sides = OnlySent(sides, job.Commands);

        ShowProcessingSides(Find(sides, "Plate"), Find(sides, "Shim"));
    }

    private async Task<bool> BlockedByUnreachableAsync( // ตรวจปลายทางที่ต้องใช้ก่อนจองคิว
        int jobId, MarkingPlan plan, ResolvedJobResponse resolved) // รับรหัสงาน แผนส่ง และข้อมูลที่โหลดไว้
    {
        ShowSending($"กำลังตรวจการเชื่อมต่อ · {JobName(jobId)}"); // แสดงว่ากำลังตรวจเครื่องของงานนี้
        List<JobSendService.UnreachableMachine> bad; // เตรียมเก็บเครื่องที่เชื่อมต่อไม่ได้
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            bad = await JobSendService.UnreachableAsync( // ตรวจการเชื่อมต่อเครื่องที่งานนี้ต้องใช้
                plan.Steps, resolved.Pattern, resolved.UvJobData); // ส่งแผนงานพร้อมข้อมูล MK และ UV ไปตรวจ
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            if (!IsDisposed && !_sending) ShowSending(null); // ปิดข้อความรอเมื่อไม่มีงานส่งค้าง
        }

        if (bad.Count == 0) return false; // ผ่านการตรวจเมื่อไม่มีเครื่องที่ต่อไม่ได้
        if (IsDisposed) return true; // หยุดขั้นเริ่มงานเมื่อหน้าถูกปิดแล้ว

        Notify.ErrorModal(this, "เริ่มงานไม่ได้ — ต่อเครื่องไม่ครบ", // แจ้งว่าเริ่มไม่ได้เพราะต่อเครื่องไม่ครบ
            $"{JobName(jobId)} ต้องใช้ {plan.Steps.Count} ขั้นตอน และต้องต่อได้ครบทุกเครื่อง" // บอกจำนวนขั้นที่ต้องพร้อมก่อนเริ่มงาน
            + Environment.NewLine + Environment.NewLine // เว้นบรรทัดก่อนรายการเครื่องที่มีปัญหา
            + string.Join(Environment.NewLine, bad.Select(m => $"• {m.Name} — {m.Reason}")) // แสดงชื่อเครื่องและสาเหตุที่ต่อไม่ได้ทีละตัว
            + Environment.NewLine + Environment.NewLine // เว้นบรรทัดก่อนแจ้งสถานะงาน
            + "ยังไม่มีอะไรถูกส่งเข้าเครื่อง และงานยังไม่เข้าคิว" // แจ้งว่ายังไม่ได้ส่งเครื่องและยังไม่เข้าคิว
            + Environment.NewLine // ขึ้นบรรทัดใหม่สำหรับวิธีทำต่อ
            + "แก้แล้วกดเริ่มงานใหม่ได้เลย"); // บอกให้แก้การเชื่อมต่อแล้วเริ่มใหม่

        return true;
    }

    private async Task SendJobPlcAsync(ResolvedJobResponse resolved, List<Notify.ResultLine> lines) // ส่งตำแหน่งหัวและสายพานไป PLC
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, resolved.Pattern, usedHeadsOnly: true); // เตรียมค่า PLC เฉพาะหัวที่ Job ใช้ พร้อมสายพาน
        if (IsDisposed || plan.Count == 0) return; // ข้ามเมื่อปิดหน้าแล้วหรือไม่มีค่า PLC ให้ส่ง

        var results = await PlcOrderService.SendAsync(plan); // เขียนแต่ละ register แล้วอ่านกลับมาเทียบ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตต่อ

        foreach (var r in results) // ไล่ดูผลส่ง PLC แต่ละรายการ
        {
            if (r.Error != null) // เช็คว่ารายการนี้ส่งแล้วมีปัญหาหรือไม่
            {
                lines.Add(Notify.Careful($"PLC {r.Name} — {r.Error}")); // เก็บชื่อ PLC และสาเหตุไว้แจ้งคนใช้
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            if (r.ReadBack != r.Value) // เครื่องตอบรับแต่ค่าไม่ตรง ก็ต้องเตือนหน้างาน
                lines.Add(Notify.Careful( // เพิ่มผลอ่านกลับที่ต้องให้คนใช้ตรวจ
                    $"PLC {r.Name} = {r.Value} · อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}")); // แสดงค่าที่ส่งเทียบกับค่าที่อ่านกลับ
        }
    }

    private static int PlanOrderOf(List<string> planSteps, string machine)
    {
        int index = planSteps.FindIndex(step =>
            string.Equals(step, machine, StringComparison.OrdinalIgnoreCase));

        return index < 0 ? int.MaxValue : index;
    }

    private static bool PrintedBefore(ResolvedJobResponse resolved) =>
        resolved.Commands?.Any(c => c.Success) == true;

    private PrintJob? JobInMyMachine()
    {
        var machine = PushButtonSettings.MachineFor(StationService.Current);

        var holder = _queueRows.FirstOrDefault(r =>
            r.State == "active"
            && string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase));

        return holder == null
            ? null
            : _allJobs.FirstOrDefault(j => j.Id == holder.PrintJobsId);
    }

    private PrintJob? NewestProcessJob() =>
        _allJobs
            .Where(j => string.Equals(j.Status, "Process", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(j => j.UpdatedAt ?? DateTime.MinValue)
            .FirstOrDefault();

    private static MarkingRefSide? Find(List<MarkingRefSide> sides, string side) =>
        sides.FirstOrDefault(s => s.Side == side);

    private static List<MarkingRefSide> OnlySent(
        List<MarkingRefSide> sides, List<CommandResult>? commands)
    {
        var sent = (commands ?? new List<CommandResult>())
            .Where(c => c.Success)
            .Select(c => c.Command)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool mkSent = sent.Contains("MK") || sent.Contains("MK1") || sent.Contains("MK2");

        return sides
            .Where(s => s.Step == "MK" ? mkSent : sent.Contains(s.Step))
            .ToList();
    }

    private async Task<List<MarkingRefSide>> BuildSidesAsync(PrintJob job)
    {
        var method = job.PlanRouting?.MarkingMethod;
        var plan = MarkingMethodService.Resolve(method);

        UvProgramInfo? uv1 = null, uv2 = null;
        bool needsUv = plan.Plate == MarkingMachine.Uv1 || plan.Shim == MarkingMachine.Uv2;

        if (needsUv && _api != null)
        {
            var resolved = await _api.GetResolvedJobAsync(job.Id);
            if (resolved != null)
            {
                uv1 = PickUvProgram(resolved, "UV1");
                uv2 = PickUvProgram(resolved, "UV2");
            }
        }

        return MarkingRefResolver.Resolve(method, job.PlanRouting?.ErpMfg, uv1, uv2);
    }

    private static UvProgramInfo PickUvProgram(ResolvedJobResponse resolved, string machine)
    {
        var sent = resolved.Commands
            .LastOrDefault(c => c.Success &&
                string.Equals(c.Command, machine, StringComparison.OrdinalIgnoreCase));

        if (sent?.Payload != null && sent.Payload.TryGetValue("program", out var value))
        {
            var chosen = value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(chosen)) return new UvProgramInfo(chosen, true);
        }

        var baseName = resolved.UvJobData
            .FirstOrDefault(u => string.Equals(u.Machine, machine, StringComparison.OrdinalIgnoreCase))
            ?.ProgramName;

        return new UvProgramInfo(baseName, false);
    }

    private void ShowPreviewSides(MarkingRefSide? plate, MarkingRefSide? shim)
    {
        ApplySide(plate, picPrevPlate, lblPrevPlateCaption);
        ApplySide(shim, picPrevShim, lblPrevShimCaption);
        BalanceSlots(tlpPreviewSlots, picPrevPlate, picPrevShim);
    }

    private void ShowProcessingSides(MarkingRefSide? plate, MarkingRefSide? shim)
    {
        ApplySide(plate, picProcPlate, lblProcPlateCaption);
        ApplySide(shim, picProcShim, lblProcShimCaption);
        BalanceSlots(tlpProcessingSlots, picProcPlate, picProcShim);
    }

    private static void BalanceSlots(TableLayoutPanel slots, PictureBox left, PictureBox right)
    {
        bool hasLeft = left.Visible, hasRight = right.Visible;

        if (hasLeft == hasRight)
        {
            slots.ColumnStyles[0].Width = 50F;
            slots.ColumnStyles[1].Width = 50F;
            return;
        }

        slots.ColumnStyles[0].Width = hasLeft ? 100F : 0F;
        slots.ColumnStyles[1].Width = hasRight ? 100F : 0F;
    }

    private static void ClearSlot(PictureBox box, Label caption)
    {
        box.Image?.Dispose();
        box.Image = null;
        box.Tag = null;
        box.Visible = false;
        caption.Text = "";
        caption.Visible = false;
    }

    private static void ApplySide(MarkingRefSide? side, PictureBox box, Label caption)
    {
        box.Image?.Dispose();
        box.Image = null;

        if (side == null)
        {
            ClearSlot(box, caption);
            return;
        }

        box.Tag = side;
        box.Visible = true;
        caption.Visible = true;

        var path = side.Images.FirstOrDefault();
        if (path == null)
        {
            var reason = side.LookupName == null
                ? "ไม่มี ERP MFG"
                : side.NearMiss > 0
                    ? $"ไม่พบรูปชื่อ {side.LookupName} (มี {side.NearMiss} ไฟล์ชื่อใกล้เคียง)"
                    : MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder());
            caption.Text = $"{side.Side} · {side.Machine} · {reason}";
            box.Image = MarkingRefImageService.Placeholder();
            return;
        }

        if (side.Pending)
        {
            caption.Text = $"{side.Side} · {side.Machine} · ยังไม่ได้เลือกรุ่น ({side.Images.Count} แบบ)";
            return;
        }

        box.Image = MarkingRefImageService.LoadImageNoLock(path);
        if (box.Image == null)
        {
            caption.Text = $"{side.Side} · {side.Machine} · เปิดไฟล์รูปไม่ได้";
            box.Image = MarkingRefImageService.Placeholder();
            return;
        }

        var more = side.Images.Count > 1 ? $"  (+{side.Images.Count - 1})" : "";
        caption.Text = $"{side.Side} · {side.Machine} · {Path.GetFileName(path)}{more}";
    }

    private void OpenSidePicker(PictureBox box, Label caption)
    {
        if (box.Tag is not MarkingRefSide side || side.Images.Count == 0) return;

        var options = side.Images
            .Select(path => new MarkingRefOption(path, Path.GetFileName(path), new List<string> { path }))
            .ToList();

        var chosen = MarkingRefPickerDialog.Pick(
            this,
            "เลือกรูปอ้างอิง",
            $"{side.Side} · {side.Machine} — เลือกรูปที่จะดู (ไม่มีผลกับงานที่พิมพ์)",
            options);

        if (chosen == null) return;

        var reordered = new List<string> { chosen };
        reordered.AddRange(side.Images.Where(path => path != chosen));
        ApplySide(side with { Images = reordered, Pending = false }, box, caption);

        if (side.Pending) caption.Text += "  (ตัวอย่าง)";
    }

    private void RestoreSelection()
    {
        if (_selectedJobId is not int id) return;

        var display = tblOrders.SortList();
        int index = Array.FindIndex(display, r => r is OrderRow row && row.Id == id);

        if (index < 0)
        {
            _selectedJobId = null;
            ShowPreviewSides(null, null);
            return;
        }

        if (tblOrders.SelectedIndex != index) tblOrders.SelectedIndex = index;
    }

    private void DisposePanelImages()
    {
        foreach (var box in new[] { picPrevPlate, picPrevShim, picProcPlate, picProcShim })
        {
            box.Image?.Dispose();
            box.Image = null;
        }
    }
}

internal class OrderRow : AntdUI.NotifyProperty
{
    public int Id { get; set; }
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public string ErpMfg { get; set; } = "";
    public string LotNo { get; set; } = "";
    public string Qty { get; set; } = "";
    public string ProcessSequence { get; set; } = "";
    public string Plate { get; set; } = "";
    public string Shim { get; set; } = "";
    public string Station { get; set; } = "";
    public AntdUI.CellText? Status { get; set; }
    private AntdUI.CellTag[] _machineStatus = [];
    public AntdUI.CellTag[] MachineStatus
    {
        get => _machineStatus;
        set { _machineStatus = value; OnPropertyChanged(); }
    }
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
