namespace InkjetOperator.Models;

/// <summary>
/// ข้อมูลหัวงานของ lot หนึ่ง ที่หน้า Scan Barcode ดึงมาโชว์ทันทีที่สแกนจบ
///
/// รวมมาจากสองตารางใน PrintData.db3 เพราะค่าอยู่คนละที่:
///   · <see cref="ErpMfg"/> กับ <see cref="Qty"/> มาจาก <c>print_data</c>
///   · <see cref="MarkingMethod"/> มาจาก <c>plan_routing</c>
///
/// ทุกช่องเป็นค่าว่างได้ — lot ที่ยังไม่มีแถวใน plan_routing ก็ยังลงทะเบียนได้
/// </summary>
public sealed class LotSummary
{
    public string LotNo { get; set; } = "";

    /// <summary>print_data.erp_mfg — โชว์เป็น Order No. ในหน้าจอ</summary>
    public string? ErpMfg { get; set; }

    /// <summary>plan_routing.marking_method — เลขดิบ ไม่ได้แปลเป็นชื่อเครื่อง</summary>
    public string? MarkingMethod { get; set; }

    /// <summary>print_data.qty — จำนวนตั้งต้น ผู้ใช้แก้ทับได้เฉพาะงานที่กำลังลงทะเบียน</summary>
    public int? Qty { get; set; }
}
