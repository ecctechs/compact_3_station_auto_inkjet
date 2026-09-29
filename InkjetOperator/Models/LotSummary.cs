namespace InkjetOperator.Models;

public sealed class LotSummary
{
    public string LotNo { get; set; } = "";

    public string? ErpMfg { get; set; }

    public string? Customer { get; set; }

    public string? MarkingMethod { get; set; }

    public int? Qty { get; set; }
}
