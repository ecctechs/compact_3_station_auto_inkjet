using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class OrderDetailUserControl : UserControl
{
    private const string Dash = "-";

    private PatternDetail? _pattern;
    private bool _isSwapped;

    public event EventHandler? CloseRequested;

    public OrderDetailUserControl()
    {
        InitializeComponent();
        ConfigureColumns();
        btnDetailClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        btnMkSwap.Click += (_, _) => SwapMkData();
        btnMk1Abc.Click += (_, _) => ShowAbcDialog(1);
        btnMk2Abc.Click += (_, _) => ShowAbcDialog(2);
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

    public void LoadDetail(ResolvedJobResponse resolved)
    {
        _pattern = resolved.Pattern;
        _isSwapped = false;

        lblHeaderTitle.Text = $"Job Information — Job #{resolved.Job.Id}";

        SortPatternByOrdinal();
        FillJobInfo(resolved);
        FillMkChipLabels();
        FillUvChipLabels();
        ApplyMarkingMethodButtons(resolved.PlanRouting?.MarkingMethod);
        FillMkSection(_pattern);
        FillConveyor(_pattern);
        FillUvSection(resolved.UvJobData);
    }

    private void FillMkChipLabels()
    {
        lblMk1Chip.Text = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        lblMk2Chip.Text = CustomSettingsManager.Read("MK059_NAME", "MK-059");
    }

    private void FillUvChipLabels()
    {
        lblUv1Chip.Text = UvSettingsManager.Read("UV1_NAME", "UV-001");
        lblUv2Chip.Text = UvSettingsManager.Read("UV2_NAME", "UV-002");
    }

    private static int Flip(int o) => o == 1 ? 2 : o == 2 ? 1 : o;

    private void SwapMkData()
    {
        if (_pattern == null) return;

        foreach (var cfg in _pattern.InkjetConfigs)
            cfg.Ordinal = Flip(cfg.Ordinal);

        foreach (var servo in _pattern.ServoConfigs)
            servo.Ordinal = Flip(servo.Ordinal);

        SortPatternByOrdinal();

        _isSwapped = !_isSwapped;
        lblMkSectionTitle.Text = _isSwapped
            ? "MK Section (MK Inkjet) — SWAPPED"
            : "MK Section (MK Inkjet)";
        lblMkSectionTitle.ForeColor = _isSwapped
            ? Color.FromArgb(212, 136, 6)
            : Color.FromArgb(36, 71, 101);

        var mk1Name = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2Name = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        lblMk1Chip.Text = _isSwapped ? mk2Name : mk1Name;
        lblMk2Chip.Text = _isSwapped ? mk1Name : mk2Name;

        FillMkSection(_pattern);
    }

    private void SortPatternByOrdinal()
    {
        if (_pattern == null) return;
        _pattern.InkjetConfigs = _pattern.InkjetConfigs
            .OrderBy(c => c.Ordinal).ToList();
        _pattern.ServoConfigs = _pattern.ServoConfigs
            .OrderBy(s => s.Ordinal).ToList();
    }

    private void ShowAbcDialog(int ordinal)
    {
        if (_pattern == null) return;

        var config = _pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
        if (config == null)
        {
            MessageBox.Show($"ไม่พบ InkjetConfig ordinal {ordinal}",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var lines = config.TextBlocks
            .OrderBy(b => b.BlockNumber)
            .Select(b => $"[Block {b.BlockNumber}]  {b.Text ?? Dash}")
            .ToList();

        var preview = lines.Count == 0
            ? "(No text blocks)"
            : string.Join(Environment.NewLine, lines);

        using var dlg = new Form
        {
            Text = $"MK-{ordinal} — Text Preview",
            Size = new Size(480, 320),
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowIcon = false,
        };

        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 13F),
            BackColor = Color.FromArgb(237, 243, 249),
            ForeColor = Color.FromArgb(17, 17, 17),
            Text = preview,
        };

        dlg.Controls.Add(txt);
        dlg.ShowDialog();
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
