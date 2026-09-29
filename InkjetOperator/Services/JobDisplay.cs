using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class JobDisplay
{
    public static string Label(string? erpMfg, string? lot, int jobId)
    {
        var erp = (erpMfg ?? "").Trim();
        var lotNo = (lot ?? "").Trim();

        if (erp.Length > 0 && lotNo.Length > 0) return $"{erp} ({lotNo})";
        if (erp.Length > 0) return erp;
        if (lotNo.Length > 0) return lotNo;

        return $"#{jobId}";
    }

    public static string Label(PrintJob job) =>
        Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id);
}
