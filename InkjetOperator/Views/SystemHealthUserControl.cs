using InkjetOperator.Services;
using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class SystemHealthUserControl : UserControl
{
    public SystemHealthUserControl()
    {
        InitializeComponent();
        ConfigureColumns();

        Render(HealthMonitor.Latest);

        HealthMonitor.Updated += OnHealthUpdated;
        Disposed += (_, _) => HealthMonitor.Updated -= OnHealthUpdated;
    }

    private void ConfigureColumns()
    {
        tblHealth.Columns =
        [
            new AntdUI.Column("Group", "หมวด") { Width = "22%" },
            new AntdUI.Column("Name", "รายการ") { Width = "28%" },
            new AntdUI.Column("Status", "สถานะ") { Width = "14%" },
            new AntdUI.Column("Detail", "รายละเอียด") { Width = "36%" },
        ];
    }

    private void OnHealthUpdated(object? sender, IReadOnlyList<HealthItem> items)
    {
        if (IsDisposed || !IsHandleCreated) return;

        try
        {
            BeginInvoke(() => { if (!IsDisposed) Render(items); });
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void Render(IReadOnlyList<HealthItem> items)
    {
        if (items.Count == 0)
        {
            lblHealthSummary.Text = "กำลังตรวจสอบ...";
            return;
        }

        tblHealth.DataSource = items.Select(ToRow).ToList();

        int bad = items.Count(i => i.State == HealthState.Bad);
        int skipped = items.Count(i => i.State == HealthState.NotConfigured);
        int ok = items.Count - bad - skipped;

        lblHealthSummary.Text = bad == 0
            ? $"ใช้งานได้ {ok} รายการ · ยังไม่ได้ตั้งค่า {skipped} รายการ"
            : $"มีปัญหา {bad} รายการ · ใช้งานได้ {ok} · ยังไม่ได้ตั้งค่า {skipped}";

        lblHealthSummary.ForeColor = bad == 0 ? DesignTokens.SuccessText : DesignTokens.Danger;
    }

    private static HealthRow ToRow(HealthItem item)
    {
        var (text, colour) = item.State switch
        {
            HealthState.Ok => ("ปกติ", DesignTokens.SuccessText),
            HealthState.Bad => ("มีปัญหา", DesignTokens.Danger),
            _ => ("ยังไม่ตั้งค่า", Color.Gray),
        };

        return new HealthRow
        {
            Group = item.Group,
            Name = item.Name,
            Status = new AntdUI.CellText(text) { Fore = colour },
            Detail = item.Detail,
        };
    }
}

internal class HealthRow
{
    public string Group { get; set; } = "";
    public string Name { get; set; } = "";
    public AntdUI.CellText? Status { get; set; }
    public string Detail { get; set; } = "";
}
