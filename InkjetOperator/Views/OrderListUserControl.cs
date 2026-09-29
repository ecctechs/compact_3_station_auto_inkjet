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
        tblOrders.SetRowStyle += TblOrders_SetRowStyle;

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

        btnSimDelayMk.Visible = dev;
        btnSimDelayUv1.Visible = dev;
        btnSimDelayUv2.Visible = dev;

        btnSimDelayMk.Click += (_, _) => StartDelayedPushTest("MK");
        btnSimDelayUv1.Click += (_, _) => StartDelayedPushTest("UV1");
        btnSimDelayUv2.Click += (_, _) => StartDelayedPushTest("UV2");

        btnSimPushMk.Visible = dev;
        btnSimPushUv1.Visible = dev;
        btnSimPushUv2.Visible = dev;

        btnSimPushMk.Click += async (_, _) => await OnPushButtonPressedAsync("MK");
        btnSimPushUv1.Click += async (_, _) => await OnPushButtonPressedAsync("UV1");
        btnSimPushUv2.Click += async (_, _) => await OnPushButtonPressedAsync("UV2");

        _pushButton.Trouble += (_, error) =>
        {
            if (IsDisposed) return;

            if (error == null) Notify.Success(this, "ปุ่มกดหน้างาน — กลับมาอ่านค่าได้แล้ว");
            else Notify.Warn(this, $"ปุ่มกดหน้างาน — อ่านค่าจาก PLC ไม่ได้ ({error})");
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
        !_pushHandling.Contains(machine) && !_dispatchingMachines.ContainsKey(machine) && !MachineBusy.IsBusy(machine);

    private void ShowBlockedPress()
    {
        long now = Environment.TickCount64;
        if (_lastBlockedPressNotice != long.MinValue && now - _lastBlockedPressNotice < 5000) return;
        _lastBlockedPressNotice = now;
        Notify.Warn(this, "มีคนกดปุ่มหน้างาน — ระบบกำลังทำงานอื่นอยู่ กรุณากดอีกครั้ง");
    }

    private async Task OnPushButtonPressedAsync(string machine) // ปล่อยเครื่องแล้วให้ ST1 รับคิวถัดไป
    {
        if (_api == null || IsDisposed) return;
        if (!CanReleaseNow(machine)) return; // เครื่องนี้ยังทำรายการอยู่ ไม่รับการกดซ้ำ
        _pushHandling.Add(machine); // ล็อกการกดซ้ำเฉพาะเครื่องนี้
        try
        {

            var (queue, queueError) = await _api.GetMachineQueueAsync(); // อ่านคิวล่าสุดก่อนปล่อยเครื่อง
            if (queueError != null) // อ่านคิวไม่ได้ จึงไม่เดาว่าจะปล่อยงานไหน
            {
                Notify.Warn(this, $"อ่านคิว {machine} ไม่สำเร็จ — {queueError}");
                return;
            }
            var holder = queue.FirstOrDefault(r => r.Machine == machine && r.State == "active"); // อ่านว่าตอนนี้กำลังปล่อยคิวหมายเลขใด

            var (release, error) = await _api.ReleaseMachineAsync( // ขอปล่อยคิวปัจจุบันและรับคิวถัดไปจาก Backend
                machine, holder?.Id, StationService.HoldForNextRound); // ส่งเลขคิวเดิมไปกันกดซ้ำแล้วข้ามงานถัดไป
            if (IsDisposed) return;

            if (release == null) // Backend ไม่คืนผลการปล่อยเครื่อง
            {
                Notify.Warn(this, $"ปล่อยเครื่อง {machine} ไม่สำเร็จ — {error}");
                return;
            }

            if (release.Next != null) // มีงานถัดไปได้รับสิทธิ์ใช้เครื่องแล้ว
            {
                if (StationService.IsSt3) // ที่ ST3 ให้รอ ST1 เป็นผู้ส่ง
                    Notify.Success(this, $"{machine} เข้าคิวแล้ว · ST1 จะส่งงานถัดไปให้");
                else
                    await ProcessMachineQueueAsync(machineFilter: machine); // ส่งคิวถัดไปของเครื่องที่เพิ่งปล่อยทันที
            }
            else
            {
                Notify.Success(this, $"{machine} ว่างแล้ว · ไม่มีงานรอคิว");
                await ResetHeadPositionAsync(machine); // ขอให้ PLC พาหัวพิมพ์กลับตำแหน่งเริ่มต้น
            }

        }
        finally
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
        if (IsDisposed || results.Count == 0) return;

        var failed = results
            .Where(r => r.Error != null || r.ReadBack != r.Value)
            .Select(r => r.Error != null
                ? $"{r.Name} {r.Error}"
                : $"{r.Name} ส่ง {r.Value} อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}")
            .ToList();
        if (failed.Count == 0) return; // แจ้งเครื่องว่างไว้แล้ว ไม่ต้องซ้อนข้อความสำเร็จ

        Notify.Warn(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นไม่สำเร็จ — " + string.Join(" · ", failed));
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
        if (_api == null || IsDisposed) return;
        ApplyProcessTabs();
        _refreshRequested |= force;

        if (_sending) return;

        if (_refreshing) return;

        force |= _refreshRequested;
        _refreshRequested = false;
        _refreshing = true;
        try
        {
            DateTime? fromUtc = null, toUtc = null;
            if (_showHistory && TryGetDateRange(out var from, out var to))
            {
                fromUtc = ToUtcFromThai(from);
                toUtc = ToUtcFromThai(to);
            }

            var (jobs, error) = await _api.GetAllJobsAsync(100, fromUtc, toUtc);
            if (IsDisposed) return;
            NotePollResult(error == null);
            if (_sending) { _refreshRequested = true; return; }
            if (error != null)
            {
                tblOrders.EmptyText = $"Error: {error}";
                return;
            }
            _allJobs = jobs;

            await RecoverAbandonedRemoteStartsAsync();
            if (IsDisposed) return;
            if (_sending) { _refreshRequested = true; return; }

            await ProcessRemoteStartsAsync();
            if (IsDisposed) return;
            if (_sending) { _refreshRequested = true; return; }

            var (queue, queueError) = await _api.GetMachineQueueAsync();
            if (IsDisposed) return;
            if (_sending) { _refreshRequested = true; return; }
            if (queueError == null)
            {
                bool changed = await ProcessMachineQueueAsync(queue);
                if (IsDisposed) return;
                if (_sending) { _refreshRequested = true; return; }
                await RefreshStationBarAsync(changed ? null : queue);
            }
            if (IsDisposed) return;
            if (_sending) { _refreshRequested = true; return; }

            await ShowRemoteErrorsAsync();
            if (IsDisposed) return;
            if (_sending) { _refreshRequested = true; return; }

            var signature = BuildSignature(jobs) + QueueSignature(); // คิวเปลี่ยนอย่างเดียวก็ต้องอัปเดตสถานะบนจอ
            if (!force && signature == _lastSignature) return;

            _lastSignature = signature;
            RebindTable();
            await UpdateProcessingAsync();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                tblOrders.EmptyText = $"Error: {ex.Message}";
        }
        finally
        {
            _refreshing = false;
            SchedulePendingRefresh();
        }
    }

    private void SchedulePendingRefresh()
    {
        if (!_refreshRequested || _refreshScheduled || _refreshing || _sending || IsDisposed || !IsHandleCreated) return;
        _refreshScheduled = true;
        BeginInvoke(new Action(async () =>
        {
            _refreshScheduled = false;
            if (!IsDisposed && _refreshRequested) await RefreshDataAsync(force: true);
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
        _processTabsEnabled = enabled;

        ShowProcessTabButtons();
        if (!changed) return;

        if (!enabled && _processFilter != null) // ปิดตัวเลือกขณะอยู่แท็บย่อย ต้องกลับ List
        {
            SwitchTab(false);
            return;
        }

        if (_allJobs.Count > 0) RebindTable();
    }

    private bool _processTabsEnabled;

    private void ShowProcessTabButtons()
    {
        bool show = _processTabsEnabled && !_showHistory;
        btnTabOnline.Visible = show;
        btnTabOffline.Visible = show;
    }

    private void SwitchTab(bool showHistory, string? process = null)
    {
        _showHistory = showHistory;
        _processFilter = showHistory ? null : process;

        ButtonStyles.SetSelected(btnTabList, !showHistory && _processFilter == null);
        ButtonStyles.SetSelected(btnTabOnline, _processFilter == JobProcessService.Online);
        ButtonStyles.SetSelected(btnTabOffline, _processFilter == JobProcessService.Offline);
        ButtonStyles.SetSelected(btnTabHistory, showHistory);
        ShowProcessTabButtons();

        lblDateFilter.Visible = showHistory;
        dtpHistoryRange.Visible = showHistory;
        btnSearchDate.Visible = showHistory;
        btnClearDate.Visible = showHistory;
        if (!showHistory) dtpHistoryRange.Value = null;

        _selectedJobId = null;
        ShowPreviewSides(null, null);

        pnlProcessing.Visible = !showHistory;

        ApplyTabColumns(showHistory);

        RebindTable();
        _ = UpdateProcessingAsync();
    }

    private static int StatusRank(PrintJob job) =>
        string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase) ? 0
        : string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase) ? 1
        : 2;

    private void RebindTable() // กรองและเติมรายการตาม Station กับแท็บที่เลือก
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses;
        int station = StationService.Current;

        bool showEveryStation = _showHistory && !StationService.IsSt3;

        var filtered = _allJobs
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase))
            .Where(j => showEveryStation
                || MarkingMethodService.VisibleAt(station, j.PlanRouting?.MarkingMethod))
            .Where(j => _processFilter == null || JobProcessService.Current(j) == _processFilter)
            .OrderBy(StatusRank)
            .ThenByDescending(j => j.CreatedAt ?? DateTime.MinValue)
            .ToList();

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _);

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : _processFilter != null && rows.Count == 0
                ? $"ไม่มีงาน {_processFilter}"
            : dateFiltered && rows.Count == 0
                ? "ไม่มีงานในช่วงวันที่ที่เลือก"
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(JobStatusDisplay.Text))})";
        _displayRows = rows;
        tblOrders.DataSource = rows;
        ReapplySort();
        RestoreSelection();

        ApplyStartLoading();
    }

    private int? _startingJobId;

    private const string StartButtonText = "เริ่มงาน";

    private void ShowStartLoading(int? jobId) // แสดงการรอที่ปุ่มเริ่มของงานที่เลือก
    {
        _startingJobId = jobId; // จำแถวที่กำลังเริ่ม เพื่อไม่ให้ไปหมุนผิดงาน
        ApplyStartLoading();
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

    private async Task<ResolvedJobResponse?> LoadJobAsync(int jobId, string busyText)
    {
        var task = _api!.GetResolvedJobAsync(jobId); // ขอข้อมูล Job พร้อม Pattern, UV และประวัติส่งจาก Backend
        if (await Task.WhenAny(task, Task.Delay(SlowLoadMs)) == task) return await task; // ถ้าอ่านทันภายในเวลาที่กำหนด ใช้ข้อมูลได้เลยโดยไม่ขึ้นหน้ารอ

        ShowSending(busyText); // ขึ้นข้อความรอเมื่ออ่านรายละเอียดช้า
        try
        {
            return await task;
        }
        finally
        {
            if (!IsDisposed && !_sending) ShowSending(null);
        }
    }

    private async Task HandleRowButtonAsync(string? buttonId, OrderRow row) // แยกงานตามปุ่มที่กดในตาราง
    {
        if (buttonId == "detail") // ปุ่มรายละเอียดเปิดข้อมูลของแถวที่เลือก
        {
            var resolved = await LoadJobAsync(row.Id, $"กำลังโหลดข้อมูล · {JobName(row.Id)}"); // อ่านรายละเอียดล่าสุดก่อนเปิดหน้าต่าง
            if (IsDisposed) return;
            if (resolved == null)
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ {JobName(row.Id)} ได้");
                return;
            }
            await ShowDetailDialogAsync(resolved); // เปิด Order Detail แล้วรอให้ผู้ใช้ปิด
        }
        else if (buttonId == "start")
        {
            if (_startingJobId != null) return;

            ShowStartLoading(row.Id); // ให้ปุ่มเริ่มหมุนตั้งแต่โหลดงานจนจบการส่ง
            try
            {
                await StartJobAsync(row.Id); // เริ่ม Job ของแถวที่กด
            }
            finally
            {
                ShowStartLoading(null); // คืนปุ่มเริ่มให้กดได้ทั้งกรณีสำเร็จและยกเลิก
            }
        }
        else if (buttonId == "complete")
        {
            await CompleteJobAsync(row.Id); // ตรวจและบันทึกจบ Job ที่เลือก
        }
        else if (buttonId == "cancel")
        {
            await CancelJobAsync(row.Id); // ยกเลิก Job ของแถวที่กด
        }
        else if (buttonId == "restore")
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
        if (_api == null) return;

        if (!Confirm.Ask(this, "ยืนยันนำกลับมาพิมพ์ใหม่", // ให้ผู้ใช้ยืนยันก่อนเปลี่ยนงานใน History กลับมารอ
                $"{JobName(jobId)}\n\n"
                + "งานจะกลับไปอยู่ในรายการงาน รอกดเริ่มงานอีกครั้ง\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Waiting"); // คืนสถานะรอ โดยยังเก็บประวัติส่งเดิม
        if (IsDisposed) return;

        if (!ok)
        {
            Notify.ErrorModal(this, "นำกลับมาไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้");
            return;
        }

        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอส่งเก่าของ ST3
        if (IsDisposed) return;

        Notify.Success(this, $"{JobName(jobId)} กลับไปอยู่ในรายการงานแล้ว");
        await RefreshDataAsync(force: true); // อ่านรายการใหม่ให้เห็นงานที่นำกลับ
    }

    private async Task CancelJobAsync(int jobId) // ยืนยันยกเลิกงานแล้วให้ Backend ล้างคิว
    {
        if (_api == null) return;

        if (!Confirm.Ask(this, "ยืนยันยกเลิกงาน", // ให้ผู้ใช้ยืนยันก่อนย้ายงานออกจากรายการผลิต
                $"ยกเลิก {JobName(jobId)}\n\n"
                + "งานจะถูกย้ายออกจากรายการไปอยู่ในประวัติ\n"
                + "ถ้าต้องการทำต่อ กดพิมพ์ใหม่ได้ที่แท็บ History\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Cancel");
        if (IsDisposed) return;

        if (!ok)
        {
            Notify.ErrorModal(this, "ยกเลิกงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะยกเลิกได้");
            return;
        }

        await _api.SetRemoteStartAsync(jobId, requested: false);
        if (IsDisposed) return;

        Notify.Success(this, $"ยกเลิก {JobName(jobId)} แล้ว");
        await RefreshDataAsync(force: true);
    }

    private int _sendOperations;
    private bool _sending => _sendOperations > 0;

    private void EndSending()
    {
        _sendOperations--;
        if (_sending || IsDisposed) return;
        ShowSending(null);
        if (!_showingSendReport)
        {
            _showingSendReport = true;
            try
            {
                while (!_sending && _sendReports.Count > 0 && !IsDisposed)
                {
                    var lines = _sendReports.ToArray();
                    _sendReports.Clear();
                    Notify.Result(this, "ผลส่งงาน", lines);
                }
            }
            finally { _showingSendReport = false; }
        }
        SchedulePendingRefresh();
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
        if (_api == null || _sending) return;

        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่านรายละเอียด Job ล่าสุดก่อนเริ่ม
        if (IsDisposed) return;
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้");
            return;
        }

        var method = resolved.PlanRouting?.MarkingMethod; // อ่านรหัสวิธีพิมพ์ของงาน
        int station = StationService.Current; // อ่าน Station ของจอนี้

        if (!MarkingMethodService.CanStartAt(station, method)) // ตรวจว่างานรหัสนี้เริ่มที่ Station ปัจจุบันได้หรือไม่
        {
            Notify.WarnModal(this, "เริ่มงานที่สถานีนี้ไม่ได้",
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + ((method ?? "").Trim() == "10"
                    ? "งาน marking 10 เริ่มได้ที่ ST3 เท่านั้น"
                    : "งานนี้เริ่มได้ที่ ST1 เท่านั้น"));
            return;
        }

        var plan = MarkingMethodService.Resolve(method); // แปลงรหัสเป็นเครื่องและลำดับพิมพ์
        if (plan.NoCase) // รหัสนี้ยังไม่มีแผนที่รองรับ
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"{JobName(jobId)} ใช้รหัส marking ที่ไม่มีอยู่จริง ({Method(method)})");
            return;
        }

        if (plan.Steps.Count == 0) // ไม่มีขั้นส่งเครื่อง เช่น marking 00
        {
            await StartWithoutSendingAsync(jobId, method); // เปลี่ยนเป็นกำลังผลิตอย่างเดียวสำหรับงานไม่มีขั้นส่ง
            return;
        }

        var uvPicks = PickUvPrograms(plan, resolved); // เลือกโปรแกรม UV ที่จอคนกด ก่อนจองคิว
        if (uvPicks == null || IsDisposed) return; // ยกเลิกเลือกโปรแกรมแล้วไม่จองคิว

        if (!await ConfirmStartAsync(jobId, resolved, plan, uvPicks)) return; // ให้ตรวจชื่อโปรแกรมและคิวก่อนเริ่มจริง
        if (IsDisposed) return;

        if (!StationService.IsSt3 && await BlockedByUnreachableAsync(jobId, plan, resolved)) return; // ST1 ต้องต่อเครื่องที่ใช้ให้ครบ ส่วน ST3 ฝากให้ ST1 ส่ง
        if (IsDisposed) return;

        var (queued, queueError) = await _api.EnqueueMachinesAsync( // จองคิวของ Job พร้อมชื่อโปรแกรมที่เลือก
            jobId, QueueItemsFor(plan.Steps, uvPicks)); // แยกเลขรอบเมื่อแผนใช้เครื่องเดิมซ้ำ
        if (IsDisposed) return;

        if (!queued) // จองคิวไม่ผ่าน จึงยังไม่ส่งเครื่อง
        {
            Notify.ErrorModal(this, "จองเครื่องไม่สำเร็จ",
                $"{JobName(jobId)} ยังไม่ได้เข้าคิว" + Environment.NewLine + Environment.NewLine
                + (queueError ?? "ติดต่อ backend ไม่ได้"));
            return;
        }

        if (station == StationService.St3) // ST3 ขอสิทธิ์คิวแล้วให้ ST1 เป็นผู้ส่งเครื่อง
        {
            var errors = await ClaimRemoteQueueAsync(jobId, plan.Steps); // ST3 ขอให้หัวคิวเป็น active เพื่อให้ ST1 รับไปส่ง
            if (IsDisposed) return;
            if (errors.Count == 0)
                Notify.Success(this, $"{JobName(jobId)} เข้าคิวแล้ว · ST1 จะส่งตามลำดับคิว");
            else
                Notify.Warn(this, $"{JobName(jobId)} เข้าคิวแล้ว แต่ยืนยันการเริ่มคิวไม่ได้ · "
                    + string.Join(" · ", errors) + " · ตรวจสถานะคิวก่อนกดเริ่มอีกครั้ง");
            await RefreshDataAsync(force: true);
            return;
        }

        await SendQueuedForJobAsync(jobId, resolved, $"เริ่มงาน {JobName(jobId)}"); // ST1 ขอสิทธิ์เครื่องแล้วส่งข้อมูลของ Job ที่กด
    }

    private async Task<List<string>> ClaimRemoteQueueAsync(int jobId, List<string> steps) // ST3 ขอสิทธิ์คิวไว้ให้ ST1 รับไปส่ง
    {
        var errors = new List<string>();
        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase)) // ขอเครื่องละหนึ่งครั้ง แม้แผนมีหลายรอบ
        {
            var (claim, error) = await _api!.ClaimMachineAsync(machine, jobId); // ให้ Backend ตรวจผู้ถือเครื่องและลำดับ FIFO
            if (IsDisposed) break;
            if (error != null || claim == null)
                errors.Add($"{machine}: {error ?? "ไม่ได้รับผลการขอคิว"}");
            else if (claim.Claimed == null && claim.Reason is not ("busy" or "queued")) // เครื่องติดงานหรือมีคนจองก่อนถือว่ารอคิวตามปกติ
                errors.Add($"{machine}: ไม่พบคิวที่พร้อมเริ่ม");
        }
        return errors;
    }

    private async Task<bool> ConfirmStartAsync( // ให้ตรวจแผนและคิวก่อนยืนยันเริ่มงาน
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan,
        Dictionary<string, string> uvPicks)
    {
        var (rows, _) = await _api!.GetMachineQueueAsync(); // อ่านคิวไว้สรุปให้ผู้ใช้ดูก่อนเริ่มงาน
        if (IsDisposed) return false;

        return Confirm.Ask(this, "ยืนยันเริ่มงาน",
            BuildStartPreview(jobId, resolved, plan, rows, uvPicks));
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

        foreach (var step in plan.Steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (UvNumberOf(step) is not int uvNumber) continue;

            var uvRow = resolved.UvJobData?.FirstOrDefault(r => // หาแถว UV ของเครื่องที่จะส่ง
                string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(uvRow?.ProgramName)) continue; // ไม่มีชื่อโปรแกรมให้เลือก จะตรวจความพร้อมอีกทีตอนส่ง

            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber);
            if (docFolder == null) continue;

            var pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, this); // ค้นไฟล์หรือเปิดให้เลือกรุ่นย่อยที่จอนี้
            if (pick.Program == null) return null;

            var uvName = UvSettingsManager.Read(
                uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

            if (pick.IsDefault &&
                !UvProgramResolver.ConfirmDefault(uvRow.ProgramName, uvName, this))
                return null;

            picks[step] = pick.Program; // จำชื่อไฟล์ไว้ส่งไปกับคิวเครื่อง
        }

        return picks;
    }

    private static string OrDash(string? text) =>
        string.IsNullOrWhiteSpace(text) ? Dash : text.Trim();

    private static readonly (string NameKey, string Fallback, int Ordinal)[] MkHeads =
    [
        ("MK058_NAME", "MK-058", 1),
        ("MK059_NAME", "MK-059", 2),
    ];

    private static List<MachineQueueItem> QueueItemsFor( // สร้างคิวเครื่องและแยกรอบของงาน
        List<string> steps, Dictionary<string, string> uvPicks)
    {
        var rounds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // นับเลขรอบแยกตามเครื่อง
        var items = new List<MachineQueueItem>(); // เตรียมรายการคิวที่จะส่งไป Backend

        foreach (var step in steps) // สร้างคิวตามทุกขั้นในแผน
        {
            rounds[step] = rounds.TryGetValue(step, out int used) ? used + 1 : 1; // เครื่องเดิมปรากฏซ้ำให้เป็นรอบถัดไป เช่น MK รอบ 2
            items.Add(new MachineQueueItem
            {
                Machine = step,
                Round = rounds[step],

                ProgramName = uvPicks.GetValueOrDefault(step), // ตอนส่งจริงใช้ชื่อนี้โดยไม่ถามเลือกซ้ำ
            });
        }

        return items;
    }

    private async Task SendQueuedForJobAsync(int jobId, ResolvedJobResponse resolved, string title) // ขอเครื่องที่ว่างแล้วรวมเป็นชุดส่ง
    {
        if (_sending) return;
        _sendOperations++;
        var lines = new List<Notify.ResultLine>();
        try
        {
            var (rows, error) = await _api!.GetMachineQueueAsync();
            if (IsDisposed) return;
            if (error != null) { Notify.Error(this, error); return; }
            var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps; // ใช้ลำดับเครื่องของงานเพื่อจัดชุดที่จะส่ง
            var machines = rows.Where(r => r.PrintJobsId == jobId && r.State == "pending") // เอาเฉพาะคิวรอของ Job ที่กดเริ่ม
                .Select(r => r.Machine).Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => PlanOrderOf(plan, m)).ToList(); // จัดรายการให้ตรงกับแผนพิมพ์ของงาน
            var ready = new List<PreparedQueueSend>();
            bool anyQueued = rows.Any(r => r.PrintJobsId == jobId && r.State == "active"); // จำว่ามีคิวได้รับสิทธิ์แล้ว เพื่อไม่ล้างทิ้งผิดจังหวะ
            foreach (var machine in machines) // ขอสิทธิ์ทีละเครื่อง แล้วรวมเครื่องที่พร้อมส่งเป็นชุด
            {
                var (claim, claimError) = await _api.ClaimMachineAsync(machine, jobId); // ขอสิทธิ์เฉพาะคิวของ Job นี้ ไม่หยิบงานอื่น
                if (IsDisposed) return;
                if (claimError != null) // ขอสิทธิ์เครื่องแล้ว Backend แจ้งข้อผิดพลาด
                {
                    anyQueued = true;
                    lines.Add(Notify.Bad($"{machine}: {claimError}")); // เก็บเหตุที่ขอใช้เครื่องนี้ไม่ได้
                }
                else if (claim?.Claimed is { } row) ready.Add(new(row, resolved)); // รวมคิวที่ได้เครื่องพร้อมข้อมูลของ Job เดียวกัน
                else
                {
                    anyQueued = true;
                    lines.Add(Notify.Note(claim?.Reason == "queued"
                        ? $"{machine}: มีงานเข้าคิวก่อน รอตามลำดับคิว"
                        : $"{machine}: เครื่องไม่ว่าง เข้าคิวรอไว้แล้ว"));
                }
            }
            var results = await SendPreparedBatchAsync(ready); // ส่งเครื่องที่พร้อมพร้อมกัน โดยแยกผลแต่ละเครื่อง
            lines.AddRange(results.SelectMany(r => r.Lines));
            anyQueued |= results.Any(r => r.HeldForReview); // คิวที่ยังไม่รู้ผลต้องถือไว้ ไม่ล้างหรือส่งซ้ำ
            if (!results.Any(r => r.Sent) && !anyQueued && !PrintedBefore(resolved)) // ล้างได้เมื่อไม่มีผลส่งเดิม ไม่มีคิวรอ และไม่มีคิวต้องตรวจ
            {
                var (cleared, clearError) = await _api.ClearMachineQueueAsync(jobId, onlyUnsent: true); // ขอให้ Backend ล้างเฉพาะงานที่ทุกคิวยังไม่ถูกหยิบส่ง
                if (!cleared) lines.Add(Notify.Bad($"ล้างคิวไม่สำเร็จ: {clearError}"));
            }
            if (!IsDisposed) _sendReports.AddRange(lines.Select(l => l with { Text = $"{title} · {l.Text}" }));
        }
        finally
        {
            EndSending();
            SchedulePendingRefresh();
            if (!IsDisposed && !_sending) ShowSending(null);
        }
        if (!IsDisposed) await RefreshDataAsync(force: true);
    }

    private bool ReserveDispatch(string machine) => _dispatchingMachines.TryAdd(machine, Guid.NewGuid());

    private void ReleaseDispatch(string machine, Guid token)
    {
        if (_dispatchingMachines.TryGetValue(machine, out var current) && current == token) // รอบเก่าปลดสิทธิ์ได้เฉพาะ token ของตัวเอง
        {
            _dispatchingMachines.Remove(machine); // เปิดทางให้เครื่องนี้รับรอบส่งใหม่
            if (!IsDisposed && _dispatchingMachines.Count > 0)
                ShowSending($"กำลังส่งไปที่ {string.Join(" / ", _dispatchingMachines.Keys)}");
        }
    }

    private sealed record PreparedQueueSend(MachineQueueRow Row, ResolvedJobResponse Job);

    private async Task<StepSendResult[]> SendPreparedBatchAsync(List<PreparedQueueSend> items, bool includeJobNames = false, bool machinesReserved = false) // เตรียมโปรแกรมแล้วส่งเครื่องที่พร้อมพร้อมกัน
    {
        if (_preparingPrograms) return [new(false, [], HeldForReview: true)];
        var owned = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var selected = new List<PreparedQueueSend>(); // เก็บคิวที่ชุดนี้ได้รับสิทธิ์ส่งจริง
        foreach (var item in items.GroupBy(i => i.Row.Machine, StringComparer.OrdinalIgnoreCase).Select(g => g.First())) // เลือกเครื่องละรอบ ไม่ส่ง MK รอบสองตามไปเอง
            if (machinesReserved || ReserveDispatch(item.Row.Machine)) // ใช้สิทธิ์ที่จองไว้ หรือขอสิทธิ์ในโปรแกรมก่อนส่ง
            {
                selected.Add(item); // รวมคิวที่ส่งได้ในชุดนี้
                owned[item.Row.Machine] = _dispatchingMachines[item.Row.Machine]; // จำ token ไว้คืนสิทธิ์เมื่อเครื่องนี้ทำเสร็จ
            }
        try
        {
            var ready = new List<PreparedQueueSend>();
            var results = new List<StepSendResult>();
            if (selected.Count != items.Count) results.Add(new(false, [], HeldForReview: true)); // บางคิวไม่ได้สิทธิ์ จึงห้ามตีความว่างานไม่มีคิวค้าง
            foreach (var item in selected)
            {
                var row = item.Row;
                if (IsDisposed) break;
                if (UvNumberOf(row.Machine) is int uvNumber && string.IsNullOrWhiteSpace(row.ProgramName)) // คิว UV ที่ยังไม่เลือกโปรแกรมต้องเตรียมให้เสร็จก่อน
                {
                    if (MachineBusy.Active) { results.Add(new(false, [], HeldForReview: true)); continue; } // ยังมีเครื่องส่งอยู่ อย่าเปิดกล่องเลือกโปรแกรมแทรก
                    _preparingPrograms = true; // กันชุดอื่นเริ่มส่งระหว่างคนเลือกโปรแกรม
                    try
                    {
                        var requested = item.Job.UvJobData?.FirstOrDefault(u => u.Machine == row.Machine)?.ProgramName; // อ่านชื่อโปรแกรมต้นทางของเครื่องในคิว
                        var pick = UvProgramResolver.Resolve(requested, UvSettingsManager.GetDocumentFolder(uvNumber), this); // เลือกไฟล์ UV สำหรับคิวที่ยังไม่มีชื่อจริง
                        if (pick.Program == null || (pick.IsDefault &&
                            !UvProgramResolver.ConfirmDefault(requested ?? "", row.Machine, this)))
                        {
                            var reset = await _api!.UpdateMachineQueueAsync(row.Id, state: "pending"); // ยกเลิกเลือกโปรแกรม ให้คืนคิวไปรอก่อน
                            results.Add(new(false, [Notify.Careful($"{row.Machine}: ยังไม่ส่ง เพราะไม่ได้เลือกโปรแกรม")],
                                SafeToRetry: reset.ok, HeldForReview: !reset.ok));
                            if (!reset.ok) results[^1].Lines.Add(Notify.Bad($"คืนคิวไม่ได้: {reset.error}"));
                            continue;
                        }
                        var saved = await _api!.UpdateMachineQueueAsync(row.Id, programName: pick.Program); // เก็บโปรแกรมลงคิวก่อนแตะเครื่องจริง
                        if (!saved.ok)
                        {
                            results.Add(new(false, [Notify.Bad($"{row.Machine}: บันทึกโปรแกรมไม่ได้ — {saved.error}")], HeldForReview: true));
                            continue;
                        }
                        row.ProgramName = pick.Program; // ใช้โปรแกรมเดียวกับที่เพิ่งบันทึกลง Backend
                    }
                    finally { _preparingPrograms = false; } // ปลดช่วงเลือกโปรแกรมแม้ผู้ใช้ยกเลิก
                }
                ready.Add(item); // โปรแกรมพร้อมแล้ว จึงรวมคิวในชุดส่ง
            }
            if (IsDisposed) return results.ToArray();
            foreach (var (machine, token) in owned)
                if (!ready.Any(i => i.Row.Machine == machine)) ReleaseDispatch(machine, token); // คืนสิทธิ์เครื่องที่ยังไม่พร้อมส่งในรอบนี้
            if (ready.Count > 0) ShowSending($"กำลังส่งไปที่ {string.Join(" / ", _dispatchingMachines.Keys)}");
            var sent = await MachineSendBatch.RunAsync(ready, i => i.Row.Machine, // เริ่มส่งคนละเครื่องพร้อมกัน
                async i =>
                {
                    _activeStatusQueues.Add(i.Row.Id); // เก็บข้อความระหว่างส่งไว้ ไม่ให้รอบรีเฟรชลบทิ้ง
                    try
                    {
                        var result = await SendQueueStepAsync(i.Row, i.Job); // บันทึกสิทธิ์ส่งกับ Backend ก่อนส่งอุปกรณ์
                        return includeJobNames
                            ? result with { Lines = result.Lines.Select(l => l with { Text = $"{JobName(i.Row.PrintJobsId)} · {l.Text}" }).ToList() }
                            : result;
                    }
                    finally
                    {
                        _activeStatusQueues.Remove(i.Row.Id); // จบรอบส่งแล้ว ให้สถานะจาก Backend เป็นหลัก
                        ReleaseDispatch(i.Row.Machine, owned[i.Row.Machine]); // เครื่องนี้เสร็จแล้ว ไม่ต้องรอเครื่องอื่นเพื่อคืนสิทธิ์
                    }
                },
                (i, ex) => new StepSendResult(false,
                    [Notify.Bad($"{i.Row.Machine} คิว {i.Row.Id}: ตรวจสอบผลก่อนส่งซ้ำ — {ex.Message}")], HeldForReview: true));
            results.AddRange(sent);
            return results.ToArray();
        }
        finally
        {
            foreach (var (machine, token) in owned) ReleaseDispatch(machine, token);
        }
    }

    private sealed record StepSendResult(bool Sent, List<Notify.ResultLine> Lines,
        bool SafeToRetry = false, object? Detail = null, bool HeldForReview = false);

    private async Task<StepSendResult> SendQueueStepAsync(MachineQueueRow row, ResolvedJobResponse resolved) // จดรอบส่ง ส่งเครื่อง แล้วบันทึกผล
    {
        using var lease = MachineBusy.TryHoldExclusive(row.Machine); // กันทางส่งอื่นในโปรแกรมเข้าเครื่องเดียวกันซ้อน
        if (lease == null)
            return new(false, [Notify.Note($"{row.Machine}: รอการส่งรอบปัจจุบันจบก่อน")], HeldForReview: true);
        SetMachineStatus(row, "กำลังเตรียมส่ง", AntdUI.TTypeMini.Primary);
        var token = Guid.NewGuid().ToString(); // สร้างรหัสอ้างอิงเฉพาะรอบส่งนี้
        var (began, beginError) = await _api!.BeginQueueSendAsync(row.Id, token); // ขอให้ Backend จดหลักฐานก่อนแตะเครื่อง
        if (!began)
        {
            SetMachineStatus(row, "ตรวจสอบคิวก่อนส่งซ้ำ", AntdUI.TTypeMini.Warn);
            return new(false, [Notify.Bad($"{row.Machine}: ยังไม่ส่ง — {beginError}")], HeldForReview: true);
        }
        SetMachineStatus(row, "กำลังส่ง", AntdUI.TTypeMini.Primary);

        StepSendResult result;
        try
        {
            if (IsDisposed) result = new(false, [], SafeToRetry: true);
            else result = await SendStepAsync(row.PrintJobsId, row.Machine, resolved, row.ProgramName); // ส่งข้อมูลตาม Job และโปรแกรมที่ผูกกับคิว
        }
        catch (Exception ex)
        {
            result = new(false, [Notify.Bad($"{row.Machine}: {ex.Message}")]);
        }

        var outcome = result.Sent ? "sent" : result.SafeToRetry ? "not_sent" : "unknown"; // แยกส่งแล้ว ยังไม่ส่งแน่นอน และยังยืนยันผลไม่ได้
        var error = string.Join(" · ", result.Lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text));
        if (error.Length > 4000) error = error[..4000];
        SetMachineStatus(row, "กำลังบันทึกผล", AntdUI.TTypeMini.Primary);
        var recorded = await _api.FinishQueueSendAsync(row.Id, token, outcome, result.Detail, error); // บันทึกผลคิวกับประวัติส่งในคำขอเดียว
        if (!recorded.ok)
            recorded = await _api.FinishQueueSendAsync(row.Id, token, outcome, result.Detail, error); // ลองบันทึกซ้ำด้วย token เดิม ไม่ยิงเครื่องซ้ำ

        bool held = !recorded.ok || outcome == "unknown"; // ยังบันทึกไม่ได้หรือผลไม่แน่นอน ต้องถือคิวไว้ตรวจ
        if (held)
            result.Lines.Add(Notify.Bad($"{row.Machine}: ถือคิว {row.Id} ไว้ตรวจผล ห้ามส่งซ้ำหรือปล่อยเครื่อง"
                + (recorded.ok ? "" : $" · บันทึกผลไม่ได้: {recorded.error}")));
        SetMachineStatus(row, held ? "ต้องตรวจสอบก่อนส่งซ้ำ" : result.Sent ? SentStatus(row.Machine) : "ยังไม่ส่ง / ส่งไม่สำเร็จ",
            held ? AntdUI.TTypeMini.Warn : result.Sent ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Error);
        return result with { HeldForReview = held };
    }

    private async Task<StepSendResult> SendStepAsync( // เลือกส่ง MK หรือ UV ตามขั้นของงาน
        int jobId, string step, ResolvedJobResponse resolved,
        string? forcedProgram = null)
    {
        if (step == "MK")
        {
            var plcLines = new List<Notify.ResultLine>();
            var plcTask = SendJobPlcAsync(resolved, plcLines); // เริ่มเขียนตำแหน่งหัวและสายพานไปพร้อมการส่ง MK
            var mk = await JobSendService.SendMkAsync(resolved.Pattern); // ส่ง Pattern ไปยังหัว MK ที่ตั้งค่าไว้

            try { await plcTask; } // รอผล PLC มารวม โดยไม่ส่ง MK ซ้ำ
            catch (Exception ex) { plcLines.Add(Notify.Careful($"PLC — {ex.Message}")); }

            var mkLines = Notify.MkLines(mk.Machines);

            if (mkLines.Count == 0)
                mkLines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"));

            var lines = new List<Notify.ResultLine>(plcLines); // เรียงผล PLC ก่อนผลหัว MK ให้คนอ่านตามได้
            lines.AddRange(mkLines);

            bool ok = mk.Status == SendStatus.Ok; // ใช้ผลรวมการส่ง MK เป็นตัวตัดสินความสำเร็จ

            return new StepSendResult(ok, lines);
        }

        int uvNumber = step == "UV1" ? 1 : 2;
        var uv = await JobSendService.SendUvAsync(this, uvNumber, resolved.UvJobData, forcedProgram, allowPrompt: false); // ส่ง UV ด้วยโปรแกรมที่เตรียมไว้ ไม่เปิดกล่องเลือกกลางชุด

        if (uv.Status == SendStatus.Ok) // UV ส่งผ่านครบตามเงื่อนไขใน Service
        {
            var detail = new
            {
                requested = resolved.UvJobData.FirstOrDefault(r => r.Machine == step)?.ProgramName ?? "", // เก็บชื่อโปรแกรมต้นทางไว้เทียบกับที่เลือกใช้
                program = uv.ProgramFile, // เก็บโปรแกรมที่ส่งเข้าเครื่องจริง
                is_default = uv.UsedDefault, // เก็บว่าใช้โปรแกรมสำรองหรือไม่
                start_confirmed = uv.StartWarning == null, // แยกผลรับคำสั่ง Start ออกจากผลโหลดข้อมูล
                start_warning = uv.StartWarning, // เก็บเหตุที่ Start ไม่ยืนยันไว้ให้ตรวจที่เครื่อง
            };

            var lines = new List<Notify.ResultLine>
                { Notify.Ok($"{uv.MachineName} — ส่งข้อมูลแล้ว ({uv.ProgramFile}.uvdx)") };
            if (uv.StartWarning != null)
                lines.Add(Notify.Careful($"{uv.MachineName} — {uv.StartWarning} · ตรวจสถานะเริ่มพิมพ์ที่เครื่อง"));
            return new StepSendResult(true, lines, Detail: detail);
        }

        return new StepSendResult(false, uv.Status switch
        {
            SendStatus.Cancelled => [], // ผู้ใช้ยกเลิกเลือกโปรแกรม จึงไม่ต้องเด้งข้อความผิดพลาด
            SendStatus.Unreachable => // ต่อ UV ไม่ได้ ให้รายงานปลายทางที่ติดต่อ
                [Notify.Bad($"{uv.MachineName} — เชื่อมต่อไม่ได้ ({uv.Ip}:{uv.Port})")],
            _ => [Notify.Bad($"{uv.MachineName} — {uv.FailReason}")], // ปัญหาอื่นใช้เหตุที่ Service ส่งกลับมา
        }, SafeToRetry: uv.Status is SendStatus.Cancelled or SendStatus.Unreachable or SendStatus.NotConfigured);
    }

    private async Task StartWithoutSendingAsync(int jobId, string? markingMethod)
    {
        if (!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ยืนยันเริ่มงานที่ไม่มีขั้นส่งเครื่อง
                $"{JobName(jobId)} — marking {Method(markingMethod)}\n\n"
                + "งานนี้ไม่มีขั้นตอนต้องส่งเข้าเครื่อง จะเปลี่ยนสถานะเป็นกำลังผลิตอย่างเดียว\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api!.UpdateJobStatusAsync(jobId, "Process");
        if (IsDisposed) return;

        if (ok) Notify.Success(this, $"เริ่มงาน {JobName(jobId)} แล้ว");
        else Notify.ErrorModal(this, "เริ่มงานไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้");

        await RefreshDataAsync(force: true);
    }

    private async Task RequestRemoteStartAsync( // ฝากขั้นและโปรแกรมให้ ST1 ส่งผ่านคิว
        int jobId, string step, ResolvedJobResponse resolved, bool askFirst = true)
    {
        int machineStation = JobStationService.StationOf(step) ?? 0;
        if (StationOwner(machineStation, jobId) is { } busyJob) // ตรวจว่ามี Job อื่นครอง Station นั้นอยู่หรือไม่
        {
            Notify.WarnModal(this, "สถานีไม่ว่าง",
                $"ST{machineStation} มีงาน {JobLabel(busyJob)} อยู่\n\nต้องจบงานนั้นก่อนถึงจะเริ่มงานนี้ได้");
            return;
        }

        int uvNumber = step == "UV1" ? 1 : 2;
        var uvRow = resolved.UvJobData.FirstOrDefault(r => r.Machine == step); // อ่านข้อมูล UV ของขั้นที่ต้องส่ง

        var pick = UvProgramResolver.Resolve( // หาโปรแกรมจริงหรือให้ผู้ใช้เลือกรุ่นย่อยก่อนฝากส่ง
            uvRow?.ProgramName, UvSettingsManager.GetDocumentFolder(uvNumber), this); // ค้นจากโฟลเดอร์ UV ของจอที่กำลังส่งคำขอ

        if (pick.Program == null) return;   // ผู้ใช้ปิดกล่องเลือกรุ่นย่อย

        var uvName = UvSettingsManager.Read(
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

        if (pick.IsDefault &&
            !UvProgramResolver.ConfirmDefault(uvRow?.ProgramName ?? "", uvName, this)) // ยืนยันว่าใช้โปรแกรมสำรองแทนชื่อจากงานได้
            return;

        if (askFirst &&!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ถามยืนยัน Job และโปรแกรมที่จะให้ ST1 ส่ง
                $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}\n\n"
                + $"ส่งไป {step} ด้วยโปรแกรม {pick.Program}.uvdx\n"
                + "คำสั่งจะถูกส่งเข้าเครื่องโดยโปรแกรมที่ ST1\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api!.SetRemoteStartAsync(
            jobId, requested: true, pick.Program, step: step); // ฝากทั้ง Job, โปรแกรม และขั้น UV ที่ต้องการให้ ST1 ส่ง
        if (IsDisposed) return;

        if (!ok)
        {
            await _api.UpdateJobStatusAsync(jobId, "Waiting");
            Notify.ErrorModal(this, "ส่งคำขอไม่สำเร็จ", err ?? "ไม่สามารถฝากคำขอไว้ที่ ST1 ได้");
            await RefreshDataAsync(force: true);
            return;
        }

        _sendOperations++;
        ShowSending($"กำลังส่งไปที่ ST1 · {JobName(jobId)}"); // บอกผู้ใช้ว่ากำลังรอให้ ST1 ส่ง
        try
        {
            await ShowRemoteOutcomeAsync(jobId, step); // ติดตามประวัติส่งและข้อผิดพลาดที่ ST1 ฝากกลับมา
        }
        finally
        {
            EndSending();
            SchedulePendingRefresh();
            ShowSending(null);
        }

        if (!IsDisposed) await RefreshDataAsync(force: true);
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
            if (IsDisposed) return;

            var job = await _api!.GetJobByIdAsync(jobId); // อ่าน Job ล่าสุดรวมประวัติส่งและ remote_error
            if (IsDisposed) return;
            if (job == null) continue; // อ่าน Job ไม่ได้ในรอบนี้ ให้ลองใหม่ภายในเวลาที่เหลือ

            var failure = job.RemoteError?.Trim(); // อ่านเหตุที่ ST1 ส่งไม่สำเร็จ
            if (!string.IsNullOrEmpty(failure)) // มีข้อผิดพลาดที่ ST1 ฝากกลับมา
            {
                await _api.SetRemoteStartAsync(jobId, requested: false);
                if (IsDisposed) return;

                Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Bad(failure)]);
                return;
            }

            bool sent = job.Commands?.Any(c => c.Success && // ค้นประวัติส่งสำเร็จของขั้นที่ขอ
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;
            if (!sent) continue; // ยังไม่มีประวัติสำเร็จของขั้นนี้ ให้รอต่อ

            var uvName = UvSettingsManager.Read(step == "UV1" ? "UV1_NAME" : "UV2_NAME", step); // ใช้ชื่อ UV ที่ตั้งไว้แสดงผล
            Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Ok($"{uvName} — ส่งสำเร็จ")]);
            return;
        }

        var final = await _api!.GetJobByIdAsync(jobId); // ครบเวลารอแล้ว อ่านสถานะอีกครั้งก่อนตัดสิน
        if (IsDisposed) return;

        if (final?.RemoteStart == RemoteSending) // ST1 รับคำขอไปแล้วแต่ยังไม่จบการส่ง
        {
            Notify.WarnModal(this, "ST1 กำลังส่งอยู่",
                $"{JobName(jobId)}\n\n"
                + "ST1 รับคำขอไปแล้วและกำลังส่งเข้าเครื่อง แต่ใช้เวลานานกว่าปกติ\n\n"
                + "งานยังเดินอยู่ ไม่ต้องกดซ้ำ — รอผลอีกสักครู่");
            return;
        }

        await _api.SetRemoteStartAsync(jobId, requested: false);
        await _api.UpdateJobStatusAsync(jobId, "Waiting");
        if (IsDisposed) return;

        Notify.WarnModal(this, "ST1 ไม่รับคำขอ",
            $"{JobName(jobId)}\n\n"
            + $"รอมา {RemoteOutcomeWait.TotalSeconds:0} วินาทีแล้วยังไม่มีใครรับไปส่ง\n"
            + "งานถูกตีกลับเป็นรอเริ่มแล้ว ยังไม่มีอะไรถูกส่งเข้าเครื่อง\n\n"
            + "ตรวจว่าโปรแกรมที่เครื่อง ST1 เปิดอยู่และต่อ Backend ได้ แล้วกดเริ่มงานใหม่");
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
        if (_api == null || StationService.IsSt3 || IsDisposed || _preparingPrograms) return false;
        var owned = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        _sendOperations++;
        try
        {
            var (rows, error) = snapshot == null // ใช้คิวจากรอบอ่านเดิมได้ ถ้าไม่มีจึงขอ Backend ใหม่
                ? await _api.GetMachineQueueAsync()
                : (snapshot, (string?)null);
            if (error != null || IsDisposed) return false;
            var review = rows.Where(r => r.NeedsSendReview && !_dispatchingMachines.ContainsKey(r.Machine) && _reportedUncertainQueues.Add(r.Id)).ToList();
            if (review.Count > 0)
                Notify.Warn(this, string.Join(" / ", review.Select(r => $"{r.Machine} คิว {r.Id}"))
                    + ": กำลังส่งหรือรอตรวจสอบผล ระบบจะไม่ส่งซ้ำเอง");

            var ready = rows.Where(r => r.State == "active" && r.SentAt == null && !r.NeedsSendReview // ส่งเฉพาะ active ที่ยังไม่ส่งและไม่มีผลค้างตรวจ
                    && (machineFilter == null || string.Equals(r.Machine, machineFilter, StringComparison.OrdinalIgnoreCase))) // หลังปล่อยปุ่ม ให้รับต่อเฉพาะเครื่องที่เพิ่งว่าง
                .OrderBy(r => r.Id)
                .Where(r => !MachineBusy.IsBusy(r.Machine) && ReserveDispatch(r.Machine)).ToList(); // กันทางอื่นใช้เครื่องเดียวกันระหว่างเตรียมส่ง
            foreach (var row in ready) owned[row.Machine] = _dispatchingMachines[row.Machine]; // จำสิทธิ์ที่ชุดนี้ถือไว้ เพื่อปล่อยเฉพาะของตัวเอง
            var prepared = new List<PreparedQueueSend>();
            var lines = new List<Notify.ResultLine>();
            foreach (var group in ready.GroupBy(r => r.PrintJobsId)) // รวมคิวของ Job เดียวกันเพื่ออ่านรายละเอียดครั้งเดียว
            {
                var resolved = await _api.GetResolvedJobAsync(group.Key); // อ่าน Pattern และ UV ล่าสุดของ Job ในคิว
                if (IsDisposed) return false;
                foreach (var row in group)
                {
                    if (resolved != null) prepared.Add(new(row, resolved)); // จับคิวกับข้อมูล Job ก่อนส่ง
                    else
                    {
                        var reset = await _api.UpdateMachineQueueAsync(row.Id, state: "pending"); // อ่าน Job ไม่ได้ ให้ขอคืนคิวนี้ไปรอก่อน
                        lines.Add(Notify.Bad($"{row.Machine} คิว {row.Id}: โหลดข้อมูลงานไม่ได้"
                            + (reset.ok ? " คืนเข้าคิวรอแล้ว" : $" · คืนคิวไม่ได้: {reset.error}")));
                    }
                }
            }
            var results = await SendPreparedBatchAsync(prepared, includeJobNames: true, machinesReserved: true); // ใช้สิทธิ์ที่จองไว้ส่งเครื่องที่พร้อมพร้อมกัน
            lines.AddRange(results.SelectMany(r => r.Lines));
            if (!IsDisposed) _sendReports.AddRange(lines); // เก็บผลไว้รายงานรวมหลังเริ่มส่งเครื่องที่พร้อมครบแล้ว
            return ready.Count > 0;
        }
        finally
        {
            foreach (var (machine, token) in owned) ReleaseDispatch(machine, token); // คืนสิทธิ์ของชุดนี้เสมอ แม้เตรียมหรือส่งบางเครื่องไม่ผ่าน
            EndSending();
            SchedulePendingRefresh(); // ให้รายการที่รอรีเฟรชตามหลังการส่งทำงานต่อ
            if (!IsDisposed && !_sending) ShowSending(null);
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

        if (!live)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false);
            return;
        }

        var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod); // อ่านแผนเครื่องที่ Job นี้ใช้จริง

        var requested = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteStep; // อ่านชื่อขั้นที่ ST3 ฝากมา
        var step = string.IsNullOrWhiteSpace(requested) // ดูว่าคำขอระบุขั้นมาหรือไม่
            ? plan.Steps.FirstOrDefault()
            : plan.Steps.FirstOrDefault(x =>
                string.Equals(x, requested.Trim(), StringComparison.OrdinalIgnoreCase));

        if (step == null)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false,
                failure: string.IsNullOrWhiteSpace(requested)
                    ? null
                    : $"งานนี้ไม่มีขั้นตอน {requested.Trim()} ให้ส่ง");
            return;
        }

        if (resolved.Commands?.Any(c => c.Success &&
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false);
            return;
        }

        int machineStation = JobStationService.StationOf(step) ?? 0;
        if (StationOwner(machineStation, jobId) != null) return; // ยังมี Job อื่นครอง Station ให้คงคำขอไว้รอรอบหน้า

        var program = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteProgram; // ใช้ชื่อโปรแกรมที่ ST3 เลือกฝากไว้

        await _api.ClaimRemoteStartAsync(jobId, program, step); // เปลี่ยนธงเป็นกำลังส่ง เพื่อให้ ST3 รู้ว่ารับแล้ว
        if (IsDisposed) return;

        var (queued, queueError) = await _api.EnqueueMachinesAsync(jobId,
            [new MachineQueueItem { Machine = step, Round = 1, ProgramName = program }]);
        var (claim, claimError) = queued
            ? await _api.ClaimMachineAsync(step, jobId)
            : (null, queueError);
        List<Notify.ResultLine> lines;
        if (claim?.Claimed is { } row)
            lines = (await SendPreparedBatchAsync([new(row, resolved)])).SelectMany(r => r.Lines).ToList();
        else if (claimError == null && claim?.Reason is "busy" or "queued")
            lines = [Notify.Note($"{step}: เข้าคิวแล้ว รอตามลำดับคิว")];
        else
            lines = [Notify.Bad(claimError ?? "เครื่องยังไม่ว่างหรือคิวถูกส่งแล้ว กรุณาตรวจสถานะงาน")];

        bool failed = lines.Any(l => l.Kind == Notify.ResultKind.Error); // ดูว่าผลส่งมีข้อความผิดพลาดหรือไม่
        var failure = failed // เตรียมเหตุที่ต้องส่งกลับให้ ST3
            ? string.Join(" · ", lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text))
            : null;

        await _api.SetRemoteStartAsync(jobId, requested: false, failure: failure);

        if (IsDisposed || lines.Count == 0) return;

        var text = $"{JobName(jobId)} — {lines[0].Text} (คำขอจาก ST3)"; // ระบุ Job และผลส่งว่าเป็นคำขอจาก ST3

        if (failed) Notify.Warn(this, text); // แจ้งเตือนแบบไม่ค้างรอคนปิดที่ ST1
        else Notify.Success(this, text);
    }

    private bool _showingRemoteError;

    private async Task ShowRemoteErrorsAsync()
    {
        if (_api == null || !StationService.IsSt3 || _showingRemoteError) return;

        var failed = _allJobs.FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.RemoteError)); // หางานที่ ST1 ฝากเหตุส่งไม่สำเร็จไว้
        if (failed == null) return;

        var message = failed.RemoteError!; // เก็บข้อความไว้ก่อนล้างค่าที่ Backend

        await _api.SetRemoteStartAsync(failed.Id, requested: false);
        if (IsDisposed) return;

        _showingRemoteError = true;
        try
        {
            Notify.ErrorModal(this, "ST1 ส่งงานไม่สำเร็จ",
                $"{JobLabel(failed)}\n\n{message}\n\n"
                + "งานถูกตีกลับเป็นรอเริ่ม กดเริ่มงานใหม่ได้");
        }
        finally
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
        if (_api == null) return;

        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่านประวัติส่งล่าสุดของ Job
        if (IsDisposed) return;
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้");
            return;
        }

        var method = resolved.PlanRouting?.MarkingMethod; // ใช้รหัสพิมพ์ตรวจสิทธิ์จบงาน
        if (!MarkingMethodService.CanCompleteAt(StationService.Current, method)) // ตรวจสิทธิ์จบงานของ Station ปัจจุบัน
        {
            Notify.WarnModal(this, "จบงานที่สถานีนี้ไม่ได้",
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + "งาน marking 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น");
            return;
        }

        var steps = CheckSteps(method, resolved.Commands); // เทียบแผนกับประวัติส่ง รวมจำนวนรอบของเครื่องเดิม

        bool manual = !steps.Complete; // ถ้าประวัติยังไม่ครบ ต้องจบแบบยืนยันด้วยมือ
        if (manual)
        {
            var list = string.Join(", ", steps.Missing); // รวมชื่อขั้นที่ยังไม่มีประวัติครบ
            if (!Confirm.Ask(this, "งานยังส่งไม่ครบ", // ให้ยืนยันว่าจะจบทั้งที่ส่งยังไม่ครบ
                    $"{JobName(jobId)} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\n" +
                    "ยืนยันจบงานทั้งที่ยังส่งไม่ครบหรือไม่?"))
                return;
        }
        else if (!Confirm.Ask(this, "ยืนยันจบงาน",
                     $"จบงาน {JobName(jobId)}\n\nยืนยันหรือไม่?"))
        {
            return;
        }

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success"); // เปลี่ยนเป็น Success และล้างคิวที่ Backend
        if (ok)
        {
            if (manual && !await _api.SaveSendStepAsync(jobId, "MANUAL_COMPLETE")) // จบด้วยมือแล้วบันทึกประวัติการยืนยัน
                Notify.Warn(this, "จบงานแล้ว แต่บันทึกประวัติการยืนยันด้วยมือไม่สำเร็จ");
            Notify.Success(this, manual
                ? $"{JobName(jobId)} จบงานแล้ว (ยืนยันด้วยมือ)"
                : $"{JobName(jobId)} จบงานแล้ว");
            await RefreshDataAsync(); // อ่านรายการใหม่ให้งานที่จบออกจาก List
        }
        else
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้");
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
            dlg.Text = dlg.TitleText;
            dlg.LoadDetail(resolved, _api); // เติมรายละเอียดงานและตัวเชื่อม Backend ให้หน้ารายละเอียด
            dlg.ShowDialog(this); // รอให้ผู้ใช้ดูรายละเอียดหรือกดขอส่งแล้วปิดหน้าต่าง
            requestedStep = dlg.RemoteStartStep; // รับขั้นที่ผู้ใช้กดขอให้ ST1 ส่ง
        }

        if (requestedStep == null || _api == null || IsDisposed) return; // ปิดเฉย ๆ หรือหน้าหลักไม่พร้อม จึงไม่ฝากคำขอ

        await RequestRemoteStartFromDetailAsync(resolved.Job.Id); // อ่านข้อมูลสดและตรวจขั้นอีกครั้งก่อนฝากส่ง
    }

    private async Task RequestRemoteStartFromDetailAsync(int jobId)
    {
        var resolved = await LoadJobAsync(jobId, $"กำลังตรวจสอบงาน · {JobName(jobId)}"); // อ่านประวัติใหม่ เผื่อมีคนส่งไปแล้วระหว่างเปิด Detail
        if (resolved == null || IsDisposed) return;

        var steps = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps; // อ่านขั้นที่งานนี้ต้องส่งจากวิธีพิมพ์
        int next = steps.FindIndex(step => !SentAlready(resolved, step)); // หาขั้นแรกที่ยังไม่มีประวัติส่งสำเร็จ

        if (next <= 0)
        {
            Notify.WarnModal(this, "ไม่มีขั้นที่ต้องส่ง",
                $"{JobName(jobId)} ไม่มีขั้นถัดไปที่รอ ST1 ส่งแล้ว\n\n"
                + "อาจมีคนกดปุ่มหน้างานไปก่อนหน้านี้");
            return;
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
        int jobId, MarkingPlan plan, ResolvedJobResponse resolved)
    {
        ShowSending($"กำลังตรวจการเชื่อมต่อ · {JobName(jobId)}");
        List<JobSendService.UnreachableMachine> bad;
        try
        {
            bad = await JobSendService.UnreachableAsync(
                plan.Steps, resolved.Pattern, resolved.UvJobData);
        }
        finally
        {
            if (!IsDisposed && !_sending) ShowSending(null);
        }

        if (bad.Count == 0) return false;
        if (IsDisposed) return true;

        Notify.ErrorModal(this, "เริ่มงานไม่ได้ — ต่อเครื่องไม่ครบ",
            $"{JobName(jobId)} ต้องใช้ {plan.Steps.Count} ขั้นตอน และต้องต่อได้ครบทุกเครื่อง"
            + Environment.NewLine + Environment.NewLine
            + string.Join(Environment.NewLine, bad.Select(m => $"• {m.Name} — {m.Reason}"))
            + Environment.NewLine + Environment.NewLine
            + "ยังไม่มีอะไรถูกส่งเข้าเครื่อง และงานยังไม่เข้าคิว"
            + Environment.NewLine
            + "แก้แล้วกดเริ่มงานใหม่ได้เลย");

        return true;
    }

    private async Task SendJobPlcAsync(ResolvedJobResponse resolved, List<Notify.ResultLine> lines) // ส่งตำแหน่งหัวและสายพานไป PLC
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, resolved.Pattern, usedHeadsOnly: true); // เตรียมค่า PLC เฉพาะหัวที่ Job ใช้ พร้อมสายพาน
        if (IsDisposed || plan.Count == 0) return;

        var results = await PlcOrderService.SendAsync(plan); // เขียนแต่ละ register แล้วอ่านกลับมาเทียบ
        if (IsDisposed) return;

        foreach (var r in results)
        {
            if (r.Error != null)
            {
                lines.Add(Notify.Careful($"PLC {r.Name} — {r.Error}"));
                continue;
            }

            if (r.ReadBack != r.Value) // เครื่องตอบรับแต่ค่าไม่ตรง ก็ต้องเตือนหน้างาน
                lines.Add(Notify.Careful(
                    $"PLC {r.Name} = {r.Value} · อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}"));
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
