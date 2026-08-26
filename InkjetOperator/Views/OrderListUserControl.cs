using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    // สถานะมี 3 แบบเท่านั้น: Waiting แดง · Working ส้ม · Finished เขียว
    // ค่าที่ backend เก็บยังเป็น Waiting / Process / Success เหมือนเดิม แปลงตอนแสดงผล
    private static readonly Color StatusRed = DesignTokens.Danger;
    private static readonly Color StatusOrange = DesignTokens.Warning;
    private static readonly Color StatusGreen = DesignTokens.SuccessText;

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
        tblOrders.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("Start", "Start", AntdUI.ColumnAlign.Center) { Width = "150", SortOrder = true },
            new AntdUI.Column("End", "End", AntdUI.ColumnAlign.Center) { Width = "150", SortOrder = true },
            new AntdUI.Column("OrderNo", "Order No.", AntdUI.ColumnAlign.Center) { SortOrder = true },
            new AntdUI.Column("Customer", "Customer", AntdUI.ColumnAlign.Center) { SortOrder = true },
            new AntdUI.Column("Type", "Type", AntdUI.ColumnAlign.Center) { SortOrder = true },
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center) { SortOrder = true },
            new AntdUI.Column("Status", "Status", AntdUI.ColumnAlign.Center) { SortOrder = true },
            new AntdUI.Column("Source", "", AntdUI.ColumnAlign.Center) { Width = "140" },
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "250" },
        };
    }

    private void SetupEvents()
    {
        btnTabList.Click += (_, _) => SwitchTab(false);
        btnTabHistory.Click += (_, _) => SwitchTab(true);
        tblOrders.CellButtonClick += TblOrders_CellButtonClick;

        // AntdUI เรียงด้วยการเทียบ "ข้อความในเซลล์" ซึ่งทำให้ 26/08 มาหลัง 02/09
        // จึงดักเทียบเองเฉพาะค่าที่เป็นวันเวลา ที่เหลือปล่อยเป็นการเทียบแบบรู้ตัวเลข
        tblOrders.CustomSort += CompareCellText;

        // เปลี่ยนช่วงวันที่ = ต้องดึงใหม่ ไม่ใช่กรองของที่โหลดไว้ — งานเก่ายังไม่ได้อยู่ในมือ
        dtpHistoryRange.ValueChanged += async (_, _) => await RefreshDataAsync(force: true);
        btnClearDate.Click += (_, _) => dtpHistoryRange.Value = null;

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

        RebindTable();
    }

    private void RebindTable()
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses;
        var filtered = _allJobs
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase))
            .ToList();

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _);

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : dateFiltered && rows.Count == 0
                ? "ไม่มีงานในช่วงวันที่ที่เลือก"
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(DisplayStatusText))})";
        tblOrders.DataSource = null;
        tblOrders.DataSource = rows;
        ReapplySort();
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
        else if (e.Btn?.Id == "complete")
        {
            await CompleteJobAsync(row.Id);
        }
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

        // MK วิ่ง 2 รอบ: "จบรอบแรก" เป็นขั้นตอนกลางทาง ไม่ใช่การจบงาน
        // ถ้าเคยจบรอบแรกไปแล้วไม่ต้องถามซ้ำ ให้ตกไปใช้ทางยืนยันด้วยมือแทน
        if (steps.IsTwoRoundMk && steps.MkCount == 1 && !steps.Round1Done)
        {
            if (!Confirm.Ask(this, "จบงานรอบ 1",
                    $"Job #{jobId} — MK วิ่ง 2 รอบ\n\nส่ง MK ไปแล้ว {steps.MkCount} รอบ\nจบรอบแรกเพื่อกดส่ง MK อีกรอบ?"))
                return;

            await _api.SaveSendStepAsync(jobId, "MK_ROUND1_DONE");
            Notify.Success(this, $"Job #{jobId} จบรอบแรก — กดส่ง MK ได้อีกรอบ");
            await RefreshDataAsync();
            return;
        }

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
    private readonly record struct StepStatus(
        bool Complete, List<string> Missing, bool IsTwoRoundMk, int MkCount, bool Round1Done);

    /// <summary>
    /// ใช้ร่วมกันทั้งตอนระบายสีปุ่มและตอนกดจบงาน เพื่อไม่ให้สองที่ตัดสินคนละแบบ
    /// marking_method "22" คือ MK วิ่ง 2 รอบ ครบต่อเมื่อส่ง MK สำเร็จครบ 2 ครั้ง
    /// </summary>
    private static StepStatus CheckSteps(string? markingMethod, List<CommandResult>? commands)
    {
        var method = markingMethod ?? "";
        var done = (commands ?? new List<CommandResult>()).Where(c => c.Success).ToList();

        if (method == "22")
        {
            int mkCount = done.Count(c => c.Command is "MK" or "MK1" or "MK2");
            bool round1 = done.Any(c =>
                string.Equals(c.Command, "MK_ROUND1_DONE", StringComparison.OrdinalIgnoreCase));

            var missing = mkCount >= 2
                ? new List<string>()
                : new List<string> { $"MK รอบ {mkCount + 1}" };

            return new StepStatus(mkCount >= 2, missing, true, mkCount, round1);
        }

        var sent = done.Select(c => c.Command).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var need = GetRequiredSteps(method).Where(x => !sent.Contains(x)).ToList();
        return new StepStatus(need.Count == 0, need, false, 0, false);
    }

    /// <summary>Waiting / Process / Success ของ backend → ชื่อกับสีที่หน้าจอใช้</summary>
    private static (string Text, Color Color) DisplayStatus(string? status)
    {
        if (string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase))
            return ("Working", StatusOrange);
        if (string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase))
            return ("Finished", StatusGreen);
        if (string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return ("Waiting", StatusRed);

        // สถานะนอกเหนือจาก 3 แบบถูกกรองออกไปแล้ว โชว์ค่าดิบไว้กันงงถ้าหลุดมา
        return (status ?? "", StatusRed);
    }

    private static string DisplayStatusText(string status) => DisplayStatus(status).Text;

    private static List<string> GetRequiredSteps(string markingMethod)
    {
        char a = '0', b = '0';
        if (markingMethod is { Length: >= 2 })
        {
            a = markingMethod[0];
            b = markingMethod[1];
        }
        if (a == '3') a = '1';
        if (b == '3') b = '1';

        var steps = new List<string>();
        if (a == '2' || b == '2') steps.Add("MK");
        if (b == '1') steps.Add("UV1");
        if (a == '1') steps.Add("UV2");
        return steps;
    }

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

    private static OrderRow ToRow(PrintJob job, bool isHistory)
    {
        var (statusLabel, statusColor) = DisplayStatus(job.Status);
        var statusText = new AntdUI.CellText(statusLabel) { Fore = statusColor };

        var sourceTag = job.StStatus == "1"
            ? new AntdUI.CellTag[] { new AntdUI.CellTag("จาก ST3", AntdUI.TTypeMini.Success) }
            : null;

        var buttons = new List<AntdUI.CellButton>();
        if (!isHistory)
        {
            // เขียว = ส่งครบแล้วจบได้เลย · ส้ม = ยังไม่ครบ กดได้แต่จะเตือนก่อน
            // commands / plan_routing มาจาก /job/getAll ที่ include ไว้ให้แล้ว
            var steps = CheckSteps(job.PlanRouting?.MarkingMethod, job.Commands);
            buttons.Add(new AntdUI.CellButton("complete", "จบงาน",
                steps.Complete ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Warn)
            { Radius = 6 });
        }
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Primary) { Radius = 6, IconSvg = "SearchOutlined" });

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
            Status = statusText,
            Source = sourceTag,
            Op = buttons.ToArray(),
        };
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
    public AntdUI.CellText? Status { get; set; }
    public AntdUI.CellTag[]? Source { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
