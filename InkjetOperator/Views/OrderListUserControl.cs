using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    private static readonly string[] ActiveStatuses = ["Waiting", "Process"];
    private static readonly string[] HistoryStatuses = ["Success"];

    private ApiClient? _api;
    private System.Windows.Forms.Timer? _pollTimer;
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
        // SortOrder = ให้ AntdUI จัดเรียงคอลัมน์นั้นเองเมื่อคลิกหัวตาราง
        // คอลัมน์ Source / Op ไม่ใส่ เพราะเป็นแท็กกับปุ่ม เรียงแล้วไม่มีความหมาย
        //
        // ความกว้างเป็น % ของตาราง รวมกันพอดี 100 — ทุกคอลัมน์แบ่งพื้นที่ตามสัดส่วน
        // ไม่มีช่องว่างเหลือ และไม่ขยับตอนสลับภาษาเพราะไม่ได้วัดจากข้อความหัวตาราง
        // (ถ้าไม่กำหนด Width เลย AntdUI จะวัดจากหัวตารางให้ ซึ่งเปลี่ยนตามภาษา)
        tblOrders.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("Start", "Start", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("End", "End", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("OrderNo", "Order No.", AntdUI.ColumnAlign.Center) { Width = "11%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Customer", "Customer", AntdUI.ColumnAlign.Center) { Width = "10%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Type", "Type", AntdUI.ColumnAlign.Center) { Width = "5%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center) { Width = "5%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Method", "Method", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Plate", "Plate", AntdUI.ColumnAlign.Center) { Width = "7%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Shim", "Shim", AntdUI.ColumnAlign.Center) { Width = "7%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Station", "Station", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Status", "Status", AntdUI.ColumnAlign.Center) { Width = "8%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Source", "", AntdUI.ColumnAlign.Center) { Width = "5%" },
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "12%" },
        };

        // ColBreak above is what centres the titles, and it is not obvious why.
        //
        // A sortable header normally reserves a strip on its right for the sort arrow
        // (SFWidth, derived from the text height - SortOrderSize does not change it),
        // and AntdUI centres the title inside what is left of the cell rather than
        // inside the whole cell. Every title therefore sat about half an arrow-width
        // left of centre.
        //
        // With ColBreak set AND a percentage Width, AntdUI measures the header against
        // the column width and returns early, leaving SFWidth at 0 - so the title is
        // centred across the full cell while the arrow is still drawn. The trade-off is
        // that a title too wide for its column now wraps (mid-word) instead of forcing
        // the column wider. "Method" is the first to go: measured on this table it
        // wraps once the table falls below roughly 1600px at 100% scaling (1.6x that
        // at 150%). Full screen on the panel PC leaves about 8% of headroom, so widen
        // Method/Station before shrinking any column.
        //
        // Both parts are required: dropping either ColBreak or the % Width brings the
        // off-centre title back.

        // Sort arrows default to 60% of the header text height, which at 14pt bold
        // crowds the title. Pin a smaller size that is still easy to read across the room.
        tblOrders.SortOrderSize = 12;
    }

    private void SetupEvents()
    {
        btnTabList.Click += (_, _) => SwitchTab(false);
        btnTabHistory.Click += (_, _) => SwitchTab(true);
        tblOrders.CellButtonClick += TblOrders_CellButtonClick;

        // AntdUI เรียงด้วยการเทียบ "ข้อความในเซลล์" ซึ่งทำให้ 26/08 มาหลัง 02/09
        // จึงดักเทียบเองเฉพาะค่าที่เป็นวันเวลา ที่เหลือปล่อยเป็นการเทียบแบบรู้ตัวเลข
        tblOrders.CustomSort += CompareCellText;
        tblOrders.SetRowStyle += TblOrders_SetRowStyle;

        // เปลี่ยนช่วงวันที่ = ต้องดึงใหม่ ไม่ใช่กรองของที่โหลดไว้ — งานเก่ายังไม่ได้อยู่ในมือ
        dtpHistoryRange.ValueChanged += async (_, _) => await RefreshDataAsync(force: true);
        btnClearDate.Click += (_, _) => dtpHistoryRange.Value = null;

        WirePanels();

        Load += OnLoad;
        Disposed += OnDisposed;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        _api = new ApiClient($"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000");
        _ = RefreshDataAsync();
        StartPolling();
    }

    private void OnDisposed(object? sender, EventArgs e)
    {
        _pollTimer?.Stop();
        _pollTimer?.Dispose();
        DisposePanelImages();
    }

    private void StartPolling()
    {
        _pollTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _pollTimer.Tick += async (_, _) => await RefreshDataAsync();
        _pollTimer.Start();
    }

    private async Task RefreshDataAsync(bool force = false)
    {
        if (_api == null) return;

        // ระหว่างส่งงานห้ามผูก DataSource ใหม่ ไม่งั้นแถวขยับใต้มือผู้ใช้
        // และกล่องเลือกรุ่นย่อยของ UV อาจถูกวาดทับ
        if (_sending) return;
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
            if (error != null)
            {
                tblOrders.EmptyText = $"Error: {error}";
                return;
            }
            _allJobs = jobs;

            // ผูก DataSource ใหม่ทีไร ตารางจะรีเซ็ตทั้งลำดับที่เรียงไว้และตำแหน่ง scroll
            // รอบ poll ที่ข้อมูลไม่เปลี่ยนจึงไม่ต้องผูกใหม่ ไม่งั้นทุก 5 วิจะกระตุกทีนึง
            var signature = BuildSignature(jobs);
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
    }

    /// <summary>ย่อทุกอย่างที่ตารางวาดให้เหลือข้อความเดียว ไว้เทียบว่ารอบนี้มีอะไรเปลี่ยนไหม</summary>
    private static string BuildSignature(List<PrintJob> jobs)
    {
        var sb = new System.Text.StringBuilder(jobs.Count * 48);
        foreach (var j in jobs)
        {
            sb.Append(j.Id).Append('|')
              .Append(j.Status).Append('|')
              .Append(j.OrderNo).Append('|')
              .Append(j.CustomerName).Append('|')
              .Append(j.Type).Append('|')
              .Append(j.Qty).Append('|')
              .Append(j.StStatus).Append('|')
              .Append(j.CreatedAt?.Ticks).Append('|')
              .Append(j.UpdatedAt?.Ticks).Append('|')
              .Append(j.PlanRouting?.MarkingMethod).Append('|')
              .Append(j.Commands?.Count(c => c.Success) ?? 0).Append(';');
        }
        return sb.ToString();
    }

    private void SwitchTab(bool showHistory)
    {
        _showHistory = showHistory;

        ButtonStyles.SetSelected(btnTabList, !showHistory);
        ButtonStyles.SetSelected(btnTabHistory, showHistory);

        // ตัวกรองวันที่มีเฉพาะแท็บ History — ออกจากแท็บแล้วล้างค่าทิ้ง
        // ไม่งั้นกลับเข้ามาใหม่จะเห็นรายการหายไปโดยไม่รู้ว่าโดนกรองอยู่
        lblDateFilter.Visible = showHistory;
        dtpHistoryRange.Visible = showHistory;
        btnClearDate.Visible = showHistory;
        if (!showHistory) dtpHistoryRange.Value = null;

        // ออกจากแท็บแล้วการเลือกเดิมไม่มีความหมาย ล้างทั้งไฮไลต์และรูป
        _selectedJobId = null;
        ShowPreviewSides(null, null);

        // แผงขวามีเฉพาะแท็บ List
        pnlProcessing.Visible = !showHistory;

        RebindTable();
        _ = UpdateProcessingAsync();
    }

    /// <summary>
    /// ลำดับตั้งต้นของตาราง 2 ชั้น — ชั้นแรกสถานะ ชั้นสองเวลารับงานใหม่สุดขึ้นก่อน
    ///
    /// งานที่เดินอยู่ต้องอยู่บนสุดเสมอ เพราะเป็นงานที่ผู้ใช้ต้องแตะ ส่วนงานที่รอ
    /// อยู่ล่าง — ตารางยาวแค่ไหนก็ไม่ต้องเลื่อนหา
    ///
    /// AntdUI เรียงได้ทีละคอลัมน์ ทำ 2 ชั้นในตัวมันไม่ได้ จึงเรียงลิสต์เองก่อน
    /// ส่งเข้าตาราง ถ้าผู้ใช้กดหัวคอลัมน์ การเรียงนั้นจะทับลำดับนี้ทั้งหมด
    /// (กดซ้ำจนลูกศรกลับเป็นเทาก็ได้ลำดับนี้คืน)
    /// </summary>
    private static int StatusRank(PrintJob job) =>
        string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase) ? 0
        : string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase) ? 1
        : 2;

    private void RebindTable()
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses;
        var filtered = _allJobs
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase))
            .OrderBy(StatusRank)
            .ThenByDescending(j => j.CreatedAt ?? DateTime.MinValue)
            .ToList();

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _);

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : dateFiltered && rows.Count == 0
                ? "ไม่มีงานในช่วงวันที่ที่เลือก"
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(JobStatusDisplay.Text))})";
        tblOrders.DataSource = null;
        tblOrders.DataSource = rows;
        ReapplySort();
        RestoreSelection();
    }

    /// <summary>
    /// ตั้ง DataSource ใหม่ทีไร AntdUI ล้างลำดับที่เรียงไว้ทุกที (แต่ลูกศรบนหัวตารางยังค้าง)
    /// เรียงกลับตามคอลัมน์เดิม ไม่งั้นหัวตารางกับข้อมูลจะไม่ตรงกัน
    /// </summary>
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

    /// <summary>ช่วงวันที่ที่เลือก ขยายเป็นทั้งวันตามเวลาไทย (00:00 ถึง 23:59:59)</summary>
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
        if (_api == null) return;

        if (e.Btn?.Id == "detail")
        {
            var resolved = await _api.GetResolvedJobAsync(row.Id);
            if (resolved == null)
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ Job #{row.Id} ได้");
                return;
            }
            ShowDetailDialog(row.Id, resolved);
        }
        else if (e.Btn?.Id == "start")
        {
            await StartJobAsync(row.Id);
        }
        else if (e.Btn?.Id == "complete")
        {
            await CompleteJobAsync(row.Id);
        }
    }

    // ── เริ่มงาน ────────────────────────────────────────────

    /// <summary>กำลังส่งงานอยู่ — กันทั้งการกดซ้ำและการรีเฟรชตารางทับ</summary>
    private bool _sending;

    /// <summary>
    /// งานที่กดเริ่มได้ = ยังไม่ได้เริ่ม และมีขั้นตอนให้ส่งจริง
    /// <para>
    /// รหัส "00" ไม่มีขั้นตอนเลย ส่วน "21" เป็นรหัสที่ไม่มีอยู่จริง สองอันนี้
    /// กดเริ่มไม่ได้ ให้ตกไปใช้ปุ่มจบงานแทน
    /// </para>
    /// </summary>
    private static bool CanStart(PrintJob job)
    {
        if (!string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return false;

        var plan = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod);
        return !plan.NoCase && plan.Steps.Count > 0;
    }

    /// <summary>
    /// เริ่มงาน: ส่งเข้าสถานีแรกตาม marking method แล้วเปลี่ยนสถานะเป็น Working
    /// <para>
    /// หนึ่งสถานีรับงานได้ทีละงาน ตรงกับความจริงหน้างานที่ชิ้นงานอยู่ที่เครื่อง
    /// ได้ทีละชิ้น ถ้าสถานีแรกไม่ว่างจะไม่ยอมให้เริ่ม
    /// </para>
    /// </summary>
    private async Task StartJobAsync(int jobId)
    {
        if (_api == null || _sending) return;

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await _api.GetResolvedJobAsync(jobId);
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล Job #{jobId} ได้");
            return;
        }

        var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod);
        if (plan.NoCase || plan.Steps.Count == 0)
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"Job #{jobId} ไม่มีขั้นตอนให้ส่ง (marking {Method(resolved.PlanRouting?.MarkingMethod)})");
            return;
        }

        var step = plan.Steps[0];
        int station = JobStationService.StationOf(step) ?? 0;

        if (StationOwner(station, jobId) is int busyJob)
        {
            Notify.WarnModal(this, "สถานีไม่ว่าง",
                $"ST{station} มีงาน #{busyJob} อยู่\n\nต้องจบงานนั้นก่อนถึงจะเริ่มงานนี้ได้");
            return;
        }

        var rest = string.Join(" -> ", plan.Steps.Skip(1));
        var next = plan.Steps.Count > 1 ? $"\n\nขั้นตอนถัดไป: {rest}" : "";

        if (!Confirm.Ask(this, "ยืนยันเริ่มงาน",
                $"Job #{jobId} — marking {Method(resolved.PlanRouting?.MarkingMethod)}\n\n"
                + $"ส่งไป {step} (ST{station}){next}\n\nยืนยันหรือไม่?"))
            return;

        _sending = true;
        try
        {
            var lines = await SendFirstStepAsync(jobId, step, resolved);
            if (IsDisposed) return;

            if (lines.Count > 0)
                Notify.Result(this, $"เริ่มงาน Job #{jobId}", lines);
        }
        finally
        {
            _sending = false;
        }

        if (!IsDisposed) await RefreshDataAsync(force: true);
    }

    /// <summary>
    /// ส่งขั้นตอนแรกเข้าเครื่อง แล้วบันทึกลงประวัติถ้าสำเร็จ
    /// <para>
    /// เปลี่ยนสถานะเป็น Process ก่อนส่ง เพื่อให้แถวขึ้นสีและกันคนอื่นเริ่มงานซ้ำ
    /// ระหว่างที่เครื่องกำลังรับข้อมูลอยู่
    /// </para>
    /// <para>
    /// คืนรายการว่างเมื่อผู้ใช้กดยกเลิกที่กล่องเลือกรุ่นย่อย — ไม่ต้องรายงานอะไร
    /// แต่สถานะที่ตั้งไปแล้วจะถูกคืนกลับเป็น Waiting
    /// </para>
    /// </summary>
    private async Task<List<Notify.ResultLine>> SendFirstStepAsync(
        int jobId, string step, ResolvedJobResponse resolved)
    {
        await _api!.UpdateJobStatusAsync(jobId, "Process");

        if (step == "MK")
        {
            var mk = await JobSendService.SendMkAsync(resolved.Pattern);
            var lines = mk.Machines
                .Select(m => m.Ok
                    ? Notify.Ok($"{m.Name} — ส่งสำเร็จ")
                    : Notify.Bad($"{m.Name} — {m.Error}"))
                .ToList();

            if (lines.Count == 0)
                lines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"));

            if (mk.Status == SendStatus.Ok)
                await _api.SaveSendStepAsync(jobId, "MK");
            else
                await _api.UpdateJobStatusAsync(jobId, "Waiting");

            return lines;
        }

        int uvNumber = step == "UV1" ? 1 : 2;
        var uv = await JobSendService.SendUvAsync(this, uvNumber, resolved.UvJobData);

        if (uv.Status == SendStatus.Ok)
        {
            await _api.SaveSendStepAsync(jobId, step, new
            {
                requested = resolved.UvJobData.FirstOrDefault(r => r.Machine == step)?.ProgramName ?? "",
                program = uv.ProgramFile,
                is_default = uv.UsedDefault,
            });

            return [Notify.Ok($"{uv.MachineName} — ส่งสำเร็จ ({uv.ProgramFile}.uvdx)")];
        }

        await _api.UpdateJobStatusAsync(jobId, "Waiting");

        return uv.Status switch
        {
            // ยกเลิกที่กล่องเลือกรุ่นย่อย ไม่ใช่ความผิดพลาด ไม่ต้องขึ้นกล่องสรุป
            SendStatus.Cancelled => [],
            SendStatus.Unreachable =>
                [Notify.Bad($"{uv.MachineName} — เชื่อมต่อไม่ได้ ({uv.Ip}:{uv.Port})")],
            _ => [Notify.Bad($"{uv.MachineName} — {uv.FailReason}")],
        };
    }

    /// <summary>งานที่จองสถานีนี้อยู่ — null = ว่าง</summary>
    private int? StationOwner(int station, int exceptJobId)
    {
        if (station == 0) return null;

        foreach (var job in _allJobs)
        {
            if (job.Id == exceptJobId) continue;
            if (!string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)) continue;
            if (JobStationService.Current(job.Commands) == station) return job.Id;
        }

        return null;
    }

    private async Task CompleteJobAsync(int jobId)
    {
        if (_api == null) return;

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await _api.GetResolvedJobAsync(jobId);
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล Job #{jobId} ได้");
            return;
        }

        var steps = CheckSteps(resolved.PlanRouting?.MarkingMethod, resolved.Commands);

        // งานยังไม่ครบก็จบได้ ถ้าผู้ใช้ยืนยันเอง — บันทึกไว้ว่าเป็นการจบด้วยมือ
        bool manual = !steps.Complete;
        if (manual)
        {
            var list = string.Join(", ", steps.Missing);
            if (!Confirm.Ask(this, "งานยังส่งไม่ครบ",
                    $"Job #{jobId} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\n" +
                    "ยืนยันจบงานทั้งที่ยังส่งไม่ครบหรือไม่?"))
                return;
        }
        else if (!Confirm.Ask(this, "ยืนยันจบงาน",
                     $"จบงาน Job #{jobId}\n\nยืนยันหรือไม่?"))
        {
            return;
        }

        if (manual)
            await _api.SaveSendStepAsync(jobId, "MANUAL_COMPLETE");

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success");
        if (ok)
        {
            Notify.Success(this, manual
                ? $"Job #{jobId} จบงานแล้ว (ยืนยันด้วยมือ)"
                : $"Job #{jobId} จบงานแล้ว");
            await RefreshDataAsync();
        }
        else
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้");
        }
    }

    // ── ตรวจความครบของงาน ──────────────────────────────────

    /// <summary>ผลตรวจว่างานส่งครบทุกขั้นตอนแล้วหรือยัง</summary>
    private readonly record struct StepStatus(bool Complete, List<string> Missing);

    /// <summary>
    /// ใช้ร่วมกันทั้งตอนระบายสีปุ่มและตอนกดจบงาน เพื่อไม่ให้สองที่ตัดสินคนละแบบ
    ///
    /// รหัส "22" เคยถูกดักเป็นกรณีพิเศษว่าต้องส่ง MK สำเร็จ 2 ครั้งถึงจะครบ
    /// ตอนนี้เลิกแล้ว ส่ง MK ครั้งเดียวก็จบงานได้ เหมือนรหัสอื่นที่ใช้ MK อย่างเดียว
    /// </summary>
    private static StepStatus CheckSteps(string? markingMethod, List<CommandResult>? commands)
    {
        var sent = (commands ?? new List<CommandResult>())
            .Where(c => c.Success)
            .Select(c => c.Command)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var need = GetRequiredSteps(markingMethod ?? "").Where(x => !sent.Contains(x)).ToList();
        return new StepStatus(need.Count == 0, need);
    }

    /// <summary>
    /// พื้นหลังของแถว — งานที่กำลังเดินอยู่ (Working) ระบายเขียวอ่อนให้สะดุดตา
    /// จากระยะไกล
    ///
    /// คืนเฉพาะ BackColor ไม่แตะ ForeColor เพราะถ้าใส่ ForeColor มา AntdUI จะเอา
    /// สีนั้นทาทับทุกเซลล์ในแถว แล้วสีของคอลัมน์ Status ที่บอกสถานะด้วยสีจะหายไป
    /// </summary>
    private static AntdUI.Table.CellStyleInfo? TblOrders_SetRowStyle(
        object sender, AntdUI.TableSetRowStyleEventArgs e)
    {
        if (e.Record is not OrderRow row || row.Back is not Color back) return null;
        return new AntdUI.Table.CellStyleInfo { BackColor = back };
    }

    /// <summary>
    /// กฎอยู่ที่ <see cref="MarkingMethodService"/> ที่เดียว หน้า Order Detail ใช้ตัวเดียวกัน
    /// เดิมที่นี่ไม่รู้จักรหัส 21 ทำให้งาน 21 ถูกมองว่ายังต้องส่ง MK ทั้งที่กดส่งไม่ได้
    /// </summary>
    private static List<string> GetRequiredSteps(string markingMethod) =>
        MarkingMethodService.Resolve(markingMethod).Steps;

    private void ShowDetailDialog(int jobId, ResolvedJobResponse resolved)
    {
        using var dlg = new OrderDetailDialog();
        dlg.TitleText = $"Job #{jobId} — Order Detail";
        dlg.Text = dlg.TitleText;
        dlg.LoadDetail(resolved, _api);
        dlg.ShowDialog(this);
    }


    // ── เวลา ───────────────────────────────────────────────

    /// <summary>
    /// รูปแบบเวลาในตาราง — สั้นพอให้อยู่ในคอลัมน์เดียว และใช้แกะกลับตอนเรียง
    /// ปีเป็น ค.ศ. 2 หลัก ตรงกับปฏิทินของตัวกรองวันที่ ไม่ใช่ปี พ.ศ.
    /// </summary>
    private const string TimeFormat = "dd/MM/yy HH:mm";

    private const string Dash = "-";

    /// <summary>
    /// เครื่องหน้างานอาจตั้ง time zone ไว้ไม่ตรง จึงยึดเวลาไทยตายตัว ไม่ใช้เวลาเครื่อง
    /// ชื่อโซนบน Windows กับ Linux คนละแบบ ถ้าหาไม่เจอทั้งคู่ค่อยใช้ UTC+7 ตรง ๆ
    /// </summary>
    private static readonly TimeZoneInfo ThaiTimeZone = ResolveThaiTimeZone();

    private static TimeZoneInfo ResolveThaiTimeZone()
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Bangkok" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "ICT", "ICT");
    }

    /// <summary>เวลาไทย → UTC สำหรับส่งเป็นเงื่อนไขให้ backend</summary>
    private static DateTime ToUtcFromThai(DateTime thai) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(thai, DateTimeKind.Unspecified), ThaiTimeZone);

    /// <summary>UTC ที่ backend ส่งมา → เวลาไทย</summary>
    private static DateTime? ToThaiTime(DateTime? utc)
    {
        if (utc == null) return null;

        var value = utc.Value;
        if (value.Kind == DateTimeKind.Unspecified)
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), ThaiTimeZone);
    }

    /// <summary>
    /// ใช้ InvariantCulture เสมอ — เครื่องหน้างานตั้งเป็น th-TH ซึ่งจะให้ปี พ.ศ.
    /// ทำให้คอลัมน์ยาวขึ้นและแกะกลับตอนเรียงไม่ได้
    /// </summary>
    private static string FormatThaiTime(DateTime? utc) =>
        ToThaiTime(utc) is { } t
            ? t.ToString(TimeFormat, System.Globalization.CultureInfo.InvariantCulture)
            : Dash;

    /// <summary>
    /// ตัวเทียบของ AntdUI ได้มาแค่ข้อความในเซลล์ ถ้าทั้งคู่เป็นเวลาก็เทียบเป็นเวลา
    /// นอกนั้นส่งต่อให้การเทียบแบบรู้ตัวเลข (เพื่อให้ Qty 9 มาก่อน 10)
    /// </summary>
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

    /// <summary>เทียบข้อความโดยอ่านกลุ่มตัวเลขเป็นจำนวน ไม่ใช่ทีละตัวอักษร</summary>
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

    /// <summary>
    /// เลข marking_method ดิบ เช่น "12" "02" "22" — ไม่แปลเป็นชื่อเครื่อง
    /// เพราะหน้านี้ใช้ไล่เทียบกับใบสั่งงาน ซึ่งเขียนเป็นตัวเลขเหมือนกัน
    /// ความหมายของแต่ละหลักดูได้ที่ Order Detail บรรทัด Plate / Shim
    /// </summary>
    private static string Method(string? markingMethod)
    {
        var value = (markingMethod ?? "").Trim();
        return value.Length == 0 ? Dash : value;
    }

    /// <summary>
    /// เครื่องที่มาร์กด้านนั้น ในตารางใช้ขีดแทน "None" ให้เข้าชุดกับคอลัมน์ End
    /// ที่ใช้ขีดแทนช่องว่างอยู่แล้ว
    /// </summary>
    private static string MachineCell(MarkingMachine machine) =>
        machine == MarkingMachine.None ? Dash : MarkingMethodService.Label(machine);

    /// <summary>
    /// สถานีล่าสุดที่กดส่งสำเร็จ — ยังไม่เคยส่งเป็นขีด เข้าชุดกับคอลัมน์ End
    /// งานที่จบแล้วยังค้างอยู่ที่สถานีสุดท้ายของมัน ไม่ได้ล้างทิ้ง
    /// </summary>
    private static string OrDashStation(int? station)
    {
        var label = JobStationService.Label(station);
        return label.Length == 0 ? Dash : label;
    }

    private static OrderRow ToRow(PrintJob job, bool isHistory)
    {
        var plan = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod);

        var (statusLabel, statusColor) = JobStatusDisplay.Resolve(job.Status);
        var statusText = new AntdUI.CellText(statusLabel) { Fore = statusColor };

        var sourceTag = job.StStatus == "1"
            ? new AntdUI.CellTag[] { new AntdUI.CellTag("จาก ST3", AntdUI.TTypeMini.Success) }
            : null;

        var buttons = new List<AntdUI.CellButton>();
        if (!isHistory)
        {
            // คอลัมน์ปุ่มกว้าง 12% ยัดสามปุ่มไม่ลง จึงสลับปุ่มตามสถานะแทน
            // งานที่ยังไม่เริ่ม = เริ่มงาน · งานที่เดินอยู่ = จบงาน
            if (CanStart(job))
            {
                buttons.Add(new AntdUI.CellButton("start", "เริ่มงาน", AntdUI.TTypeMini.Primary)
                { Radius = 6 });
            }
            else
            {
                // เขียว = ส่งครบแล้วจบได้เลย · ส้ม = ยังไม่ครบ กดได้แต่จะเตือนก่อน
                // commands / plan_routing มาจาก /job/getAll ที่ include ไว้ให้แล้ว
                var steps = CheckSteps(job.PlanRouting?.MarkingMethod, job.Commands);
                buttons.Add(new AntdUI.CellButton("complete", "จบงาน",
                    steps.Complete ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Warn)
                { Radius = 6 });
            }
        }
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Default) { Radius = 6, IconSvg = "SearchOutlined" });

        // End มีความหมายเฉพาะงานที่จบแล้ว — งานที่ยังวิ่งอยู่ updated_at คือเวลาแก้ล่าสุด ไม่ใช่เวลาจบ
        bool finished = string.Equals(job.Status, "Success", StringComparison.OrdinalIgnoreCase);

        return new OrderRow
        {
            Id = job.Id,
            Start = FormatThaiTime(job.CreatedAt),
            End = finished ? FormatThaiTime(job.UpdatedAt) : Dash,
            OrderNo = job.OrderNo ?? "",
            Customer = job.CustomerName ?? "",
            Type = job.Type ?? "",
            Qty = job.Qty?.ToString() ?? "",
            Method = Method(job.PlanRouting?.MarkingMethod),
            Plate = plan.NoCase ? Dash : MachineCell(plan.Plate),
            Shim = plan.NoCase ? Dash : MachineCell(plan.Shim),
            Station = OrDashStation(JobStationService.Current(job.Commands)),
            Status = statusText,
            Source = sourceTag,
            Op = buttons.ToArray(),
            Back = string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)
                ? DesignTokens.RowSuccess
                : null,
        };
    }

    // ── แผง Preview / Processing ───────────────────────────

    /// <summary>job ที่ผู้ใช้กดเลือกในตาราง — แผงซ้ายผูกกับตัวนี้</summary>
    private int? _selectedJobId;

    /// <summary>job ที่แผงขวากำลังแสดง กับเวลาแก้ล่าสุดของมัน ไว้กันโหลดซ้ำทุกรอบ poll</summary>
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

    /// <summary>
    /// คลิกแถวไหนก็แสดงรูปของงานนั้นในแผงซ้าย
    /// ข้ามคอลัมน์ Op เพราะเป็นปุ่ม — ปล่อยให้ CellButtonClick จัดการไปตามเดิม
    /// </summary>
    private async void TblOrders_CellClick(object? sender, AntdUI.TableClickEventArgs e)
    {
        if (e.RowType != AntdUI.RowType.None) return;
        if (e.Column?.Key == "Op") return;
        if (e.Record is not OrderRow row) return;
        if (_selectedJobId == row.Id) return;

        _selectedJobId = row.Id;
        await UpdatePreviewAsync();
    }

    /// <summary>แผงซ้าย — รูปของงานที่เลือกอยู่</summary>
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

    /// <summary>
    /// แผงขวา — งานที่กำลังผลิตอยู่จริง ไม่เกี่ยวกับแถวที่เลือก
    /// เกณฑ์คือ status = Process ถ้ามีหลายงานเอาอันที่แก้ล่าสุด
    /// โหลดใหม่เฉพาะตอนเปลี่ยนตัวหรือ updated_at ขยับ ไม่ใช่ทุกรอบ poll
    /// </summary>
    private async Task UpdateProcessingAsync()
    {
        var job = _showHistory
            ? null
            : _allJobs
                .Where(j => string.Equals(j.Status, "Process", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(j => j.UpdatedAt ?? DateTime.MinValue)
                .FirstOrDefault();

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

        // แผงนี้บอกว่า "ส่งอะไรเข้าเครื่องไปแล้ว" ไม่ใช่ "งานนี้ต้องทำอะไรบ้าง"
        // method 12 กดส่ง MK อย่างเดียวต้องเห็นแค่ด้านของ MK จนกว่าจะกดส่ง UV2
        sides = OnlySent(sides, job.Commands);

        ShowProcessingSides(Find(sides, "Plate"), Find(sides, "Shim"));
    }

    private static MarkingRefSide? Find(List<MarkingRefSide> sides, string side) =>
        sides.FirstOrDefault(s => s.Side == side);

    /// <summary>
    /// กรองเหลือเฉพาะด้านที่ step ของมันถูกกดส่งสำเร็จไปแล้ว
    ///
    /// ข้อจำกัด: marking 22 ที่ MK ทำทั้งสองด้านมี command "MK" อันเดียว
    /// ระบบแยกไม่ออกว่ารอบแรกทำด้านไหน จึงขึ้นทั้งสองด้านพร้อมกัน
    /// </summary>
    private static List<MarkingRefSide> OnlySent(
        List<MarkingRefSide> sides, List<CommandResult>? commands)
    {
        var sent = (commands ?? new List<CommandResult>())
            .Where(c => c.Success)
            .Select(c => c.Command)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ของเก่าบางงานบันทึกเป็น MK1 / MK2 แทน MK
        bool mkSent = sent.Contains("MK") || sent.Contains("MK1") || sent.Contains("MK2");

        return sides
            .Where(s => s.Step == "MK" ? mkSent : sent.Contains(s.Step))
            .ToList();
    }

    /// <summary>
    /// ฝั่ง MK ใช้ erp_mfg ที่ติดมากับ /job/getAll อยู่แล้ว
    /// ฝั่ง UV ต้องรู้ชื่อโปรแกรมซึ่งอยู่ใน uv_job_data จึงต้องยิง getResolved
    /// เรียกเฉพาะงานที่ใช้ UV จริง และเฉพาะตอนที่งานเปลี่ยนเท่านั้น
    /// </summary>
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

    /// <summary>
    /// ส่งไปแล้ว → ใช้รุ่นย่อยที่เลือกจริง (เก็บใน payload ของ command) ถือว่ายืนยันแล้ว
    /// ยังไม่ได้ส่ง → ได้แค่ชื่อฐานจากข้อมูลงาน ยังตอบไม่ได้ว่าจะพิมพ์รุ่นไหน
    /// </summary>
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

    // ── แผงรูปทั้งแผง ──────────────────────────────────────

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

    /// <summary>
    /// สองช่องแบ่งกรอบคนละครึ่งตายตัว พองานใช้ด้านเดียว (เช่น method 02 มีแต่ Plate)
    /// รูปเลยไปเกาะครึ่งซ้าย เหลือครึ่งขวาว่าง ดูเหมือนวางผิดที่มากกว่าจะดูตั้งใจ
    ///
    /// ยุบคอลัมน์ที่ไม่ได้ใช้เหลือ 0 ด้านที่เหลือจะกินเต็มกรอบ และ PictureBox โหมด
    /// Zoom จัดรูปไว้กลางกรอบให้เอง โดยไม่ยืดรูปผิดสัดส่วน
    /// </summary>
    private static void BalanceSlots(TableLayoutPanel slots, PictureBox left, PictureBox right)
    {
        bool hasLeft = left.Visible, hasRight = right.Visible;

        // ไม่เหลือด้านไหนเลยก็คืนเป็นครึ่งต่อครึ่ง กรอบว่างจะได้ไม่เพี้ยน
        if (hasLeft == hasRight)
        {
            slots.ColumnStyles[0].Width = 50F;
            slots.ColumnStyles[1].Width = 50F;
            return;
        }

        slots.ColumnStyles[0].Width = hasLeft ? 100F : 0F;
        slots.ColumnStyles[1].Width = hasRight ? 100F : 0F;
    }

    // ── ช่องรูปหนึ่งช่อง ───────────────────────────────────

    private static void ClearSlot(PictureBox box, Label caption)
    {
        box.Image?.Dispose();
        box.Image = null;
        box.Tag = null;
        box.Visible = false;
        caption.Text = "";
        caption.Visible = false;
    }

    /// <summary>
    /// ด้านที่ marking method ไม่ได้ใช้ → ซ่อนทั้งช่อง
    /// ด้านที่ใช้แต่ไม่มีรูป → ยังโชว์ป้ายไว้ พร้อมบอกสาเหตุ ไม่ปล่อยว่างเปล่า
    /// </summary>
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
            return;
        }

        // ยังไม่ได้เลือกรุ่นย่อย — หยิบใบไหนมาโชว์ก็ดูเหมือนระบบเลือกไว้ให้แล้ว
        // ปล่อยกรอบว่างไว้แล้วบอกตรง ๆ ว่ามีให้เลือกกี่แบบ (กดที่กรอบดูตัวอย่างได้)
        if (side.Pending)
        {
            caption.Text = $"{side.Side} · {side.Machine} · ยังไม่ได้เลือกรุ่น ({side.Images.Count} แบบ)";
            return;
        }

        box.Image = MarkingRefImageService.LoadImageNoLock(path);
        if (box.Image == null)
        {
            caption.Text = $"{side.Side} · {side.Machine} · เปิดไฟล์รูปไม่ได้";
            return;
        }

        var more = side.Images.Count > 1 ? $"  (+{side.Images.Count - 1})" : "";
        caption.Text = $"{side.Side} · {side.Machine} · {Path.GetFileName(path)}{more}";
    }

    /// <summary>
    /// คลิกรูปเพื่อดูใหญ่และเลือกใบอื่นของด้านเดียวกัน
    /// ฝั่งนี้เลือกแล้วเปลี่ยนแค่รูปที่ดู ไม่กระทบงานที่พิมพ์ ข้อความจึงต้องบอกให้ชัด
    /// </summary>
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

        // เปิดดูเองตอนที่ยังไม่ได้เลือกรุ่น ต้องไม่ให้เข้าใจว่านี่คือรุ่นที่จะพิมพ์
        if (side.Pending) caption.Text += "  (ตัวอย่าง)";
    }

    /// <summary>
    /// ผูก DataSource ใหม่ทีไร AntdUI ล้าง selection ทิ้งเสมอ ต้องเลือกกลับให้เอง
    /// ไม่งั้นไฮไลต์กับรูปจะหายทุกครั้งที่มีงานไหนก็ตามเปลี่ยนสถานะ
    /// งานที่หลุดจากลิสต์แล้วจริง ๆ ค่อยล้างทั้งไฮไลต์และรูปพร้อมกัน
    /// </summary>
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
    public string OrderNo { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Type { get; set; } = "";
    public string Qty { get; set; } = "";
    public string Method { get; set; } = "";
    public string Plate { get; set; } = "";
    public string Shim { get; set; } = "";
    public string Station { get; set; } = "";
    public AntdUI.CellText? Status { get; set; }
    public AntdUI.CellTag[]? Source { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
