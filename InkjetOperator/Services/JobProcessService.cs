using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class JobProcessService
{
    public const string Online = "Online";
    public const string Offline = "Offline";

    public static string? Current(PrintJob job) => // หา Online หรือ Offline ของงานตอนนี้
        BetweenRounds(job.PlanRouting?.MarkingMethod, job.Commands) // ตรวจว่างานหลายรอบอยู่ระหว่างพ่น Plate กับ Shim หรือไม่
            ? Offline // ส่งรอบแรกแล้วแต่ยังไม่ครบรอบ ให้อยู่ Offline
            : FromPlan(job.PlanRouting?.ProcessSequence); // นอกช่วงคั่นรอบ ใช้ Online/Offline จาก Routing

    public static bool BetweenRounds(string? markingMethod, IEnumerable<CommandResult>? commands) // ตรวจว่างาน MK สองรอบอยู่ระหว่างรอบไหม
    {
        var steps = MarkingMethodService.Resolve(markingMethod).Steps; // ดูจำนวนรอบเครื่องจากแผนพิมพ์

        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase)) // ตรวจเครื่องแต่ละชนิดในแผนเพียงครั้งเดียว
        {
            int rounds = steps.Count(s => Same(s, machine)); // นับว่าต้องเข้าเครื่องนี้กี่รอบ
            if (rounds < 2) continue; // เครื่องที่ใช้รอบเดียวไม่ใช่งานระหว่างรอบ

            int sent = commands?.Count(c => c.Success && Same(c.Command, machine)) ?? 0; // นับรอบที่มีประวัติส่งสำเร็จแล้ว
            if (sent > 0 && sent < rounds) return true; // ทำไปแล้วบางรอบ จึงเป็นช่วงออกนอกไลน์
        }

        return false;
    }

    private static string? FromPlan(string? processSequence)
    {
        var value = processSequence?.Trim(); // ใช้ข้อความจาก Process seq โดยตัดช่องว่าง
        if (Same(value, Online)) return Online; // รับค่า Online โดยไม่สนตัวพิมพ์ใหญ่เล็ก
        if (Same(value, Offline)) return Offline; // รับค่า Offline โดยไม่เดาจากคำอื่น
        return null;
    }

    private static bool Same(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
