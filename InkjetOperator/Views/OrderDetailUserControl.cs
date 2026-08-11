using InkjetOperator.Models;

namespace InkjetOperator.Views;

public partial class OrderDetailUserControl : UserControl
{
    private const string Dash = "-";

    /// <summary>เกิดเมื่อกดปุ่มปิด — ให้ host (Form/หน้าหลัก) เป็นคนตัดสินใจว่าจะปิดยังไง</summary>
    public event EventHandler? CloseRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        btnDetailClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureColumns()
    {
        tblMk1Blocks.Columns = BuildBlockColumns();
        tblMk2Blocks.Columns = BuildBlockColumns();
        tblUv1Texts.Columns = BuildUvColumns();
        tblUv2Texts.Columns = BuildUvColumns();
    }

    private static AntdUI.ColumnCollection BuildBlockColumns() =>
    [
        new AntdUI.Column("Block", "Block", AntdUI.ColumnAlign.Center) { Width = "70" },
        new AntdUI.Column("Text", "Text", AntdUI.ColumnAlign.Left),
        new AntdUI.Column("X", "X", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Y", "Y", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Size", "Size", AntdUI.ColumnAlign.Center) { Width = "60" },
        new AntdUI.Column("Scale", "Scale", AntdUI.ColumnAlign.Center) { Width = "60" },
    ];

    private static AntdUI.ColumnCollection BuildUvColumns() =>
    [
        new AntdUI.Column("Field", "Field", AntdUI.ColumnAlign.Center) { Width = "90" },
        new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Left),
    ];

    /// <summary>
    /// เติมข้อมูลจาก GET /job/getResolved (job + pattern + plan_routing + uv_job_data)
    /// </summary>
    public void LoadDetail(ResolvedJobResponse resolved)
    {
        lblHeaderTitle.Text = $"Job Information — Job #{resolved.Job.Id}";

        FillJobInfo(resolved);
        FillMkSection(resolved.Pattern);
        FillConveyor(resolved.Pattern);
        FillUvSection(resolved.UvJobData);
    }

    private void FillJobInfo(ResolvedJobResponse resolved)
    {
        var job = resolved.Job;

        txtJobOrderNo.Text = OrDash(job.OrderNo);
        txtJobLotNo.Text = OrDash(job.BarcodeRaw);
        txtJobCustomer.Text = OrDash(job.CustomerName);
        txtJobType.Text = OrDash(job.Type);
        txtJobQty.Text = job.Qty?.ToString() ?? Dash;
        txtJobStatus.Text = OrDash(job.Status);

        // ยังไม่ตีความ marking_method — แสดงค่าดิบ ค่าว่าง/NULL แสดงเป็นข้อความบอกสถานะ
        var marking = resolved.PlanRouting?.MarkingMethod;
        txtMarkingMethod.Text = string.IsNullOrWhiteSpace(marking) ? "ไม่ระบุ" : marking;
    }

    private void FillMkSection(PatternDetail pattern)
    {
        var configs = pattern.InkjetConfigs;
        var servos = pattern.ServoConfigs;

        FillMk(
            configs.FirstOrDefault(c => c.Ordinal == 1),
            servos.FirstOrDefault(s => s.Ordinal == 1),
            txtMk1Program, txtMk1ProgramNo, txtMk1Width, txtMk1Height,
            txtMk1Trigger, txtMk1PosAct, txtMk1Delay, tblMk1Blocks);

        FillMk(
            configs.FirstOrDefault(c => c.Ordinal == 2),
            servos.FirstOrDefault(s => s.Ordinal == 2),
            txtMk2Program, txtMk2ProgramNo, txtMk2Width, txtMk2Height,
            txtMk2Trigger, txtMk2PosAct, txtMk2Delay, tblMk2Blocks);
    }

    private static void FillMk(
        InkjetConfigDto? config, ServoConfigDto? servo,
        AntdUI.Input program, AntdUI.Input programNo,
        AntdUI.Input width, AntdUI.Input height,
        AntdUI.Input trigger, AntdUI.Input posAct, AntdUI.Input delay,
        AntdUI.Table table)
    {
        program.Text = OrDash(config?.ProgramName);
        programNo.Text = Number(config?.ProgramNumber);
        width.Text = Number(config?.Width);
        height.Text = Number(config?.Height);
        trigger.Text = Number(config?.TriggerDelay);
        posAct.Text = Number(servo?.PostAct);
        delay.Text = Number(servo?.Delay);

        var rows = (config?.TextBlocks ?? [])
            .OrderBy(b => b.BlockNumber)
            .Select(b => new BlockRow
            {
                Block = b.BlockNumber.ToString(),
                Text = OrDash(b.Text),
                X = Number(b.X),
                Y = Number(b.Y),
                Size = Number(b.Size),
                Scale = Number(b.Scale),
            })
            .ToList();

        table.DataSource = null;
        table.DataSource = rows;
    }

    private void FillConveyor(PatternDetail pattern)
    {
        var speeds = pattern.ConveyorSpeeds;
        txtConveyor1.Text = Number(speeds?.Speed1);
        txtConveyor2.Text = Number(speeds?.Speed2);
        txtConveyor3.Text = Number(speeds?.Speed3);
    }

    private void FillUvSection(List<UvJobDataDto> uvRows)
    {
        var uv1 = uvRows.FirstOrDefault(r => r.Machine == "UV1");
        var uv2 = uvRows.FirstOrDefault(r => r.Machine == "UV2");

        // qty มาจาก print_data แถวเดียวกัน UV1/UV2 จึงใช้ค่าร่วมกันเสมอ
        txtUvQtyShared.Text = (uv1?.Qty ?? uv2?.Qty)?.ToString() ?? Dash;

        FillUv(uv1, txtUv1Program, txtUv1ErpMfg, tblUv1Texts);
        FillUv(uv2, txtUv2Program, txtUv2ErpMfg, tblUv2Texts);
    }

    private static void FillUv(
        UvJobDataDto? uv,
        AntdUI.Input program, AntdUI.Input erpMfg,
        AntdUI.Table table)
    {
        program.Text = OrDash(uv?.ProgramName);
        erpMfg.Text = OrDash(uv?.ErpMfg);

        var values = new[] { uv?.Text1, uv?.Text2, uv?.Text3, uv?.Text4, uv?.Text5 };
        var rows = values
            .Select((v, i) => new UvTextRow { Field = $"Text{i + 1}", Value = OrDash(v) })
            .ToList();

        table.DataSource = null;
        table.DataSource = uv == null ? null : rows;
    }

    private static string OrDash(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Dash : value;

    private static string Number(int? value) => value?.ToString() ?? Dash;

    private static string Number(double? value) => value?.ToString() ?? Dash;
}

internal class BlockRow : AntdUI.NotifyProperty
{
    public string Block { get; set; } = "";
    public string Text { get; set; } = "";
    public string X { get; set; } = "";
    public string Y { get; set; } = "";
    public string Size { get; set; } = "";
    public string Scale { get; set; } = "";
}

internal class UvTextRow : AntdUI.NotifyProperty
{
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
}
