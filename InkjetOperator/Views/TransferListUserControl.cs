using InkjetOperator.Models;
using InkjetOperator.Services;
using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class TransferListUserControl : UserControl
{
    private static readonly Color RowGreen = DesignTokens.RowSuccess;

    private ApiClient? _api;
    private System.Windows.Forms.Timer? _pollTimer;
    private bool _showSent;
    private List<PrintJob> _allJobs = new();

    public TransferListUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        SetupEvents();
    }

    private void ConfigureColumns()
    {
        tblTransfer.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("OrderNo", "Order No.", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Customer", "Customer", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Type", "Type", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center),
            new AntdUI.Column("StStatus", "สถานะ", AntdUI.ColumnAlign.Center) { Width = "140" },
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "180" },
        };
    }

    private void SetupEvents()
    {
        btnTabPending.Click += (_, _) => SwitchTab(false);
        btnTabSent.Click += (_, _) => SwitchTab(true);
        tblTransfer.CellButtonClick += TblTransfer_CellButtonClick;

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
            var (jobs, error) = await _api.GetJobsByMarkingMethodAsync("12");
            if (IsDisposed) return;
            if (error != null)
            {
                tblTransfer.EmptyText = $"Error: {error}";
                return;
            }
            _allJobs = jobs;
            RebindTable();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                tblTransfer.EmptyText = $"Error: {ex.Message}";
        }
    }

    private void SwitchTab(bool showSent)
    {
        _showSent = showSent;
        ButtonStyles.SetSelected(btnTabPending, !showSent);
        ButtonStyles.SetSelected(btnTabSent, showSent);
        RebindTable();
    }

    private void RebindTable()
    {
        var filtered = _allJobs
            .Where(j => _showSent
                ? j.StStatus == "1"
                : j.StStatus is null or "0" or "")
            .ToList();

        var rows = filtered.Select(j => ToRow(j, _showSent)).ToList();
        tblTransfer.EmptyText = filtered.Count == 0
            ? (_showSent ? "ยังไม่มี job ที่ส่งแล้ว" : "ไม่มี job ที่รอส่ง")
            : "";
        tblTransfer.DataSource = null;
        tblTransfer.DataSource = rows;
    }

    private async void TblTransfer_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Record is not TransferRow row) return;
        if (_api == null) return;

        if (e.Btn?.Id == "send")
        {
            if (!Confirm.Ask(this, "ยืนยันส่ง ST1",
                    $"ส่ง Job #{row.Id} ({row.OrderNo}) ไป Station 1\n\nยืนยันหรือไม่?"))
                return;

            var (ok, err) = await _api.SendToSt1Async(row.Id);
            if (ok)
            {
                Notify.Success(this, $"ส่ง Job #{row.Id} ไป ST1 แล้ว");
                await RefreshDataAsync();
            }
            else
            {
                Notify.ErrorModal(this, "ส่งไม่สำเร็จ", err ?? "Unknown error");
            }
        }
        else if (e.Btn?.Id == "detail")
        {
            var resolved = await _api.GetResolvedJobAsync(row.Id);
            if (resolved == null)
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ Job #{row.Id} ได้");
                return;
            }

            ShowDetailDialog(row.Id, resolved);
        }
    }

    private void ShowDetailDialog(int jobId, ResolvedJobResponse resolved)
    {
        using var dlg = new OrderDetailDialog();
        dlg.TitleText = $"Job #{jobId} — Order Detail";
        dlg.Text = dlg.TitleText;
        dlg.LoadDetail(resolved, _api);
        dlg.ShowDialog(this);
    }

    private static TransferRow ToRow(PrintJob job, bool isSent)
    {
        var statusCell = isSent
            ? new AntdUI.CellText("ส่งแล้ว") { Fore = DesignTokens.SuccessText }
            : new AntdUI.CellText("รอส่ง") { Fore = DesignTokens.Danger };

        var buttons = new List<AntdUI.CellButton>();
        if (!isSent)
            buttons.Add(new AntdUI.CellButton("send", "ส่ง ST1", AntdUI.TTypeMini.Success) { Radius = 6 });
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Primary) { Radius = 6, IconSvg = "SearchOutlined" });

        return new TransferRow
        {
            Id = job.Id,
            OrderNo = job.OrderNo ?? "",
            Customer = job.CustomerName ?? "",
            Type = job.Type ?? "",
            Qty = job.Qty?.ToString() ?? "",
            StStatus = statusCell,
            Op = buttons.ToArray(),
        };
    }
}

internal class TransferRow : AntdUI.NotifyProperty
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Type { get; set; } = "";
    public string Qty { get; set; } = "";
    public AntdUI.CellText? StStatus { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = [];
}
