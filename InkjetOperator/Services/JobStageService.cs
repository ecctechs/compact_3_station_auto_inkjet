using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class JobStageService
{
    private const string Pending = "pending";
    private const string Active = "active";

    public static string? Describe(int jobId, string? markingMethod, IEnumerable<MachineQueueRow> rows) // แปลงสถานะคิวเป็นข้อความท้ายสถานะ Job
    {
        var all = rows as IList<MachineQueueRow> ?? rows.ToList(); // เก็บลำดับคิวตามที่ Backend ส่งมา
        var mine = all.Where(r => r.PrintJobsId == jobId).ToList(); // แยกแถวคิวของ Job ที่กำลังดู
        if (mine.Count == 0) return null;

        if (mine.Any(r => r.NeedsSendReview)) return "กำลังส่ง / รอตรวจสอบผล"; // ถ้ายังไม่รู้ผลส่ง ให้บอกจุดนี้ก่อนแสดงด้านหรือเลขคิว

        var plan = MarkingMethodService.Resolve(markingMethod); // ใช้แผน Marking แยกว่าเครื่องกำลังทำ Plate หรือ Shim

        if (FirstInPlanOrder(plan, mine.Where(IsActive)) is { } holding) // งานที่ถือเครื่องอยู่แสดงด้านที่มาก่อนในแผน
            return SideOf(plan, holding);

        if (FirstInPlanOrder(plan, mine.Where(IsPending)) is not { } waiting) return null;

        int position = all
            .Where(r => IsPending(r) && SameMachine(r.Machine, waiting.Machine)) // นับลำดับเฉพาะคิวรอของเครื่องเดียวกัน
            .ToList()
            .FindIndex(r => r.Id == waiting.Id); // หาตำแหน่งของงานนี้ก่อนแสดง Q1, Q2 ต่อกัน

        return position < 0 ? null : $"Q{position + 1}";
    }

    private static MachineQueueRow? FirstInPlanOrder(
        MarkingPlan plan, IEnumerable<MachineQueueRow> rows) =>
        rows.OrderBy(r => PlanIndex(plan, r.Machine)).ThenBy(r => r.Round).FirstOrDefault();

    private static int PlanIndex(MarkingPlan plan, string machine)
    {
        int index = plan.Steps.FindIndex(step => SameMachine(step, machine));
        return index < 0 ? int.MaxValue : index;
    }

    private static string? SideOf(MarkingPlan plan, MachineQueueRow row)
    {
        if (plan.Plate == plan.Shim) // กรณีใช้เครื่องเดิมสองด้าน ต้องดูเลขรอบประกอบ
            return plan.Plate == MarkingMachine.None
                ? null
                : row.Round >= 2 ? "Mark Shim" : "Mark Plate";

        if (SameMachine(MarkingMethodService.Label(plan.Plate), row.Machine)) return "Mark Plate";
        if (SameMachine(MarkingMethodService.Label(plan.Shim), row.Machine)) return "Mark Shim";

        return null;
    }

    private static bool IsActive(MachineQueueRow row) =>
        string.Equals(row.State, Active, StringComparison.OrdinalIgnoreCase);

    private static bool IsPending(MachineQueueRow row) =>
        string.Equals(row.State, Pending, StringComparison.OrdinalIgnoreCase);

    private static bool SameMachine(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
