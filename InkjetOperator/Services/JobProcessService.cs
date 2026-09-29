using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// งานนี้ตอนนี้อยู่ในไลน์ (Online) หรือนอกไลน์ (Offline) — ใช้แยกแท็บในหน้า Order List
///
/// <para>
/// ปกติอ่านตรง ๆ จากช่อง Process seq ของแผนการผลิต ซึ่งหน้างานกรอกเป็นคำว่า
/// Online หรือ Offline อยู่แล้ว
/// </para>
/// <para>
/// ยกเว้นงานที่เข้าเครื่องเดิมหลายรอบ (marking 22) — รอบแรกพ่นในไลน์ แล้วเอาชิ้นงาน
/// ออกไปติด shim นอกไลน์ ก่อนกลับมาพ่นรอบสอง ช่วงที่ส่งรอบแรกไปแล้วแต่ยังไม่ได้ส่ง
/// รอบสองจึงนับเป็น Offline ส่งรอบสองแล้วกลับเป็นค่าตามแผนเหมือนเดิม
/// </para>
/// </summary>
public static class JobProcessService
{
    public const string Online = "Online";
    public const string Offline = "Offline";

    /// <summary>
    /// ตอนนี้งานอยู่ Online หรือ Offline — null เมื่อช่อง Process seq ไม่ใช่สองคำนี้
    ///
    /// <para>
    /// ค่าอื่นไม่เดา ไม่เอาไปใส่แท็บไหน งานพวกนั้นยังเห็นครบในแท็บ List
    /// ข้อมูลทดสอบบางชุดเก็บช่องนี้เป็นรหัสตัวเลข (02, 05, 10 …) ซึ่งบอกไม่ได้ว่า
    /// ในไลน์หรือนอกไลน์
    /// </para>
    /// </summary>
    public static string? Current(PrintJob job) =>
        BetweenRounds(job.PlanRouting?.MarkingMethod, job.Commands)
            ? Offline
            : FromPlan(job.PlanRouting?.ProcessSequence);

    /// <summary>
    /// ส่งรอบแรกของเครื่องที่ต้องเข้าซ้ำไปแล้ว แต่ยังส่งไม่ครบทุกรอบ
    ///
    /// <para>
    /// ดูจากแผนว่ามีเครื่องไหนต้องเข้ามากกว่าหนึ่งครั้ง (วันนี้มีแค่ marking 22 ที่เป็น
    /// MK สองรอบ) แล้วนับจากประวัติว่าส่งเข้าเครื่องนั้นสำเร็จไปแล้วกี่ครั้ง ใช้วิธีนับ
    /// เดียวกับที่ปุ่มในตารางดูว่าขั้นไหนส่งไปแล้ว
    /// </para>
    /// </summary>
    public static bool BetweenRounds(string? markingMethod, IEnumerable<CommandResult>? commands)
    {
        var steps = MarkingMethodService.Resolve(markingMethod).Steps;

        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            int rounds = steps.Count(s => Same(s, machine));
            if (rounds < 2) continue;

            int sent = commands?.Count(c => c.Success && Same(c.Command, machine)) ?? 0;
            if (sent > 0 && sent < rounds) return true;
        }

        return false;
    }

    private static string? FromPlan(string? processSequence)
    {
        var value = processSequence?.Trim();
        if (Same(value, Online)) return Online;
        if (Same(value, Offline)) return Offline;
        return null;
    }

    private static bool Same(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
