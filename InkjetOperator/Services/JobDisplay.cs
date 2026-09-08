using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// ชื่อเรียกงานในข้อความที่พนักงานอ่าน
///
/// <para>
/// พนักงานไม่รู้จักเลข id ของ backend เห็น "Job #63" แล้วไม่รู้ว่างานไหน
/// ต้องเรียกด้วย ERP MFG กับ Lot ซึ่งเป็นเลขที่อยู่บนใบงานและบนบาร์โค้ดจริง
/// </para>
/// <para>
/// อยู่ที่นี่เพราะทุกหน้าต้องเรียกงานเดียวกันด้วยชื่อเดียวกัน — ที่ผ่านมาต่างหน้า
/// ต่างประกอบข้อความเอง แล้วมีบางจุดตกหล่นเป็น id ค้างอยู่
/// </para>
/// <para>
/// คนละเรื่องกับ <c>OrderDetailUserControl.JobTitle</c> ซึ่งใช้ job_no ที่นับใหม่
/// ทุกวัน — อันนั้นเป็นหัวจอ ตั้งใจให้เป็นเลขลำดับของวัน ไม่ใช่ชื่อเรียกงาน
/// </para>
/// </summary>
public static class JobDisplay
{
    /// <summary>
    /// "ERP (LOT)" — ขาดตัวไหนใช้ตัวที่เหลือ ขาดทั้งคู่ค่อยตกไปใช้ id
    /// </summary>
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
