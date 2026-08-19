using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    private static readonly Color RowGreen = DesignTokens.RowSuccess;
    private static readonly Color StatusRed = DesignTokens.Danger;

    private static readonly string[] ActiveStatuses = ["Waiting", "executing"];
    private static readonly string[] HistoryStatuses = ["completed", "failed"];

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
            new AntdUI.Column("Op", "Detail", AntdUI.ColumnAlign.Center) { Width = "120" },
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

        var rows = filtered.Select(ToRow).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses)})";
        tblOrders.DataSource = null;
        tblOrders.DataSource = rows;
    }

    private async void TblOrders_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Btn?.Id != "detail" || e.Record is not OrderRow row) return;
        if (_api == null) return;

        var resolved = await _api.GetResolvedJobAsync(row.Id);
        if (resolved == null)
        {
            Notify.Warn(this, $"ไม่สามารถโหลด Detail ของ Job #{row.Id} ได้");
            return;
        }

        ShowDetailDialog(row.Id, resolved);
    }

    private void ShowDetailDialog(int jobId, ResolvedJobResponse resolved)
    {
        using var dlg = new Form
        {
            Text = $"Job #{jobId} — Order Detail",
            Size = new Size(1320, 940),
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = true,
            ShowIcon = false,
        };

        var detail = new OrderDetailUserControl { Dock = DockStyle.Fill };
        detail.LoadDetail(resolved, _api);
        detail.CloseRequested += (_, _) => dlg.Close();

        dlg.Controls.Add(detail);
        dlg.ShowDialog();
    }


    private static OrderRow ToRow(PrintJob job)
    {
        var isProcessing = string.Equals(job.Status, "executing", StringComparison.OrdinalIgnoreCase);
        var isWaiting = string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase);

        var statusText = new AntdUI.CellText(job.Status ?? "");
        if (isWaiting)
            statusText.Fore = StatusRed;

        var row = new OrderRow
        {
            Id = job.Id,
            OrderNo = job.OrderNo ?? "",
            Customer = job.CustomerName ?? "",
            Type = job.Type ?? "",
            Qty = job.Qty?.ToString() ?? "",
            Status = statusText,
            Op = [new AntdUI.CellButton("detail", "Detail", AntdUI.TTypeMini.Primary) { Radius = 6 }],
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
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
