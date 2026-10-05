using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// งานนี้เดินในไลน์ (In-line) หรือออกนอกไลน์ (Off-line) — ใช้กับตัวกรองในหน้า Order List
///
/// <para>
/// อ่านจากช่อง Process seq ของแผนการผลิตอย่างเดียว ตามที่หน้างานตกลงไว้ว่าการออกนอกไลน์
/// ไม่ได้ขึ้นกับ marking method งานไหนก็อยู่ในไลน์หรือนอกไลน์ได้ และโปรแกรมไม่ได้
/// อัปเดตหรือเปลี่ยนค่านี้เอง แค่อ่านมาแสดง
/// </para>
/// </summary>
public static class JobProcessService
{
    public const string InLine = "In-line";
    public const string OffLine = "Off-line";

    public static string? Current(PrintJob job) => // หา In-line หรือ Off-line ของงาน
        FromPlan(job.PlanRouting?.ProcessSequence); // ใช้ค่าจาก Process seq ตรง ๆ ไม่มีกติกาอื่นมาทับ

    /// <summary>
    /// แปลงคำในช่องเป็นค่ามาตรฐาน — null เมื่อไม่ใช่สองคำนี้
    ///
    /// <para>
    /// หน้างานเขียนว่า In-line / Off-line ยอมให้ต่างกันแค่ตัวพิมพ์ ขีด และช่องว่าง
    /// (เช่น Inline, OFF-LINE) เพราะเป็นข้อมูลที่คนพิมพ์เข้าระบบ ค่าอื่นไม่เดา
    /// งานพวกนั้นยังเห็นครบเมื่อเลือก "ทั้งหมด"
    /// </para>
    /// </summary>
    private static string? FromPlan(string? processSequence)
    {
        var value = Normalize(processSequence); // ตัดขีด ช่องว่าง และตัวพิมพ์ใหญ่เล็กออกก่อนเทียบ
        if (value == Normalize(InLine)) return InLine;
        if (value == Normalize(OffLine)) return OffLine;
        return null;
    }

    private static string Normalize(string? value) =>
        new string((value ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
