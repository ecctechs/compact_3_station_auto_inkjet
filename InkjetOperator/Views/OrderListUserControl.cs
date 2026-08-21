using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    private static readonly Color RowGreen = DesignTokens.RowSuccess;
    private static readonly Color StatusRed = DesignTokens.Danger;

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
            : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses)})";
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

        var resolved = await _api.GetResolvedJobAsync(jobId);
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล Job #{jobId} ได้");
            return;
        }

        var markingMethod = resolved.PlanRouting?.MarkingMethod ?? "";
        var steps = GetRequiredSteps(markingMethod);
        var completedCmds = resolved.Commands
            .Where(c => c.Success)
            .Select(c => c.Command)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool isTwoRoundMk = markingMethod == "22";

        if (isTwoRoundMk)
        {
            int mkCount = resolved.Commands.Count(c => c.Success &&
                c.Command is "MK" or "MK1" or "MK2");

            if (mkCount < 1)
            {
                Notify.WarnModal(this, "ยังกดไม่ครบ", "ยังไม่ได้ส่ง MK — กรุณาส่ง MK ก่อนจบงาน");
                return;
            }

            if (mkCount < 2)
            {
                if (!Confirm.Ask(this, "จบงานรอบ 1",
                        $"Job #{jobId} — MK วิ่ง 2 รอบ\n\nส่ง MK ไปแล้ว {mkCount} รอบ\nจบรอบแรกเพื่อกดส่ง MK อีกรอบ?"))
                    return;

                await _api.SaveSendStepAsync(jobId, "MK_ROUND1_DONE");
                Notify.Success(this, $"Job #{jobId} จบรอบแรก — กดส่ง MK ได้อีกรอบ");
                await RefreshDataAsync();
                return;
            }
        }
        else
        {
            var missing = steps.Where(s => !completedCmds.Contains(s)).ToList();
            if (missing.Count > 0)
            {
                var list = string.Join(", ", missing);
                Notify.WarnModal(this, "ยังกดไม่ครบ",
                    $"Job #{jobId} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\nกรุณาส่งให้ครบก่อนจบงาน");
                return;
            }
        }

        if (!Confirm.Ask(this, "ยืนยันจบงาน",
                $"จบงาน Job #{jobId}\n\nยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success");
        if (ok)
        {
            Notify.Success(this, $"Job #{jobId} จบงานแล้ว");
            await RefreshDataAsync();
        }
        else
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้");
        }
    }

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
        var isProcessing = string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase);
        var isWaiting = string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase);

        var statusText = new AntdUI.CellText(job.Status ?? "");
        if (isWaiting)
            statusText.Fore = StatusRed;

        var sourceTag = job.StStatus == "1"
            ? new AntdUI.CellTag[] { new AntdUI.CellTag("จาก ST3", AntdUI.TTypeMini.Success) }
            : null;

        var buttons = new List<AntdUI.CellButton>();
        if (!isHistory)
            buttons.Add(new AntdUI.CellButton("complete", "จบงาน", AntdUI.TTypeMini.Success) { Radius = 6 });
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Primary) { Radius = 6, IconSvg = "SearchOutlined" });

        var row = new OrderRow
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

        if (isProcessing)
            row.Back = RowGreen;

        return row;
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
