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
            new AntdUI.Column("OrderNo", "Order No.", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Customer", "Customer", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Type", "Type", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Status", "Status", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Source", "", AntdUI.ColumnAlign.Center) { Width = "120" },
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "220" },
        };
    }

    private void SetupEvents()
    {
        btnTabList.Click += (_, _) => SwitchTab(false);
        btnTabHistory.Click += (_, _) => SwitchTab(true);
        tblOrders.CellButtonClick += TblOrders_CellButtonClick;

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

    private async Task RefreshDataAsync()
    {
        if (_api == null) return;
        try
        {
            var (jobs, error) = await _api.GetAllJobsAsync(100);
            if (IsDisposed) return;
            if (error != null)
            {
                tblOrders.EmptyText = $"Error: {error}";
                return;
            }
            _allJobs = jobs;
            RebindTable();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                tblOrders.EmptyText = $"Error: {ex.Message}";
        }
    }

    private void SwitchTab(bool showHistory)
    {
        _showHistory = showHistory;

        ButtonStyles.SetSelected(btnTabList, !showHistory);
        ButtonStyles.SetSelected(btnTabHistory, showHistory);

        RebindTable();
    }

    private void RebindTable()
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses;
        var filtered = _allJobs
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(DisplayStatusText))})";
        tblOrders.DataSource = null;
        tblOrders.DataSource = rows;
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

        return new OrderRow
        {
            Id = job.Id,
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
    public string OrderNo { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Type { get; set; } = "";
    public string Qty { get; set; } = "";
    public AntdUI.CellText? Status { get; set; }
    public AntdUI.CellTag[]? Source { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
