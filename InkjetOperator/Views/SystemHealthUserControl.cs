using InkjetOperator.Services;
using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// รายการสถานะของไฟล์ โฟลเดอร์ และเครื่องปลายทางทั้งหมด อัปเดตเองทุก 30 วินาที
///
/// <para>
/// ไม่มีปุ่มให้กดตรวจ เพราะ <see cref="HealthMonitor"/> เฝ้าอยู่แล้วตลอดเวลา
/// หน้านี้ทำหน้าที่แค่วาดผลรอบล่าสุดที่มันส่งมาให้
/// </para>
/// <para>
/// <b>ไม่เด้งกล่องอะไรทั้งนั้น</b> ต่อให้ทุกอย่างพังพร้อมกัน — เปลี่ยนแค่สีกับ
/// ข้อความในตาราง คนที่เปิดหน้านี้คือคนที่ตั้งใจมาดูสถานะอยู่แล้ว
/// </para>
/// </summary>
public partial class SystemHealthUserControl : UserControl
{
    public SystemHealthUserControl()
    {
        InitializeComponent();
        ConfigureColumns();

        // ผลรอบก่อนมีอยู่แล้วก็วาดเลย ไม่ต้องให้คนเปิดหน้ามานั่งรอรอบถัดไป
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

    /// <summary>
    /// ตัวเฝ้ายิงมาจากเธรดพูล ต้องข้ามกลับมาเธรดของหน้าจอก่อนแตะคอนโทรล
    ///
    /// ใช้ BeginInvoke ไม่ใช่ Invoke — ตัวเฝ้าไม่ควรต้องรอให้จอวาดเสร็จ
    /// และถ้าจอกำลังติดกล่อง modal อยู่ Invoke จะค้างยาว
    /// </summary>
    private void OnHealthUpdated(object? sender, IReadOnlyList<HealthItem> items)
    {
        if (IsDisposed || !IsHandleCreated) return;

        try
        {
            BeginInvoke(() => { if (!IsDisposed) Render(items); });
        }
        catch (ObjectDisposedException)
        {
            // หน้าถูกปิดระหว่างทางพอดี ไม่มีอะไรต้องทำต่อ
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
