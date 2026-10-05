using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// คำในวงเล็บท้ายสถานะของหน้า Order Detail — งานนี้กำลังทำด้านไหน หรือรออยู่คิวที่เท่าไร
///
/// <para>
/// หน้า Order List โชว์คำสถานะคำเดียวล้วน ๆ ไม่มีวงเล็บ เพราะตารางต้องอ่านจากที่ไกล ๆ
/// รายละเอียดว่ารออะไรอยู่ไปดูที่หน้า Order Detail ซึ่งเปิดดูทีละงาน
/// </para>
/// <para>
/// กฎมีข้อเดียว เจออันไหนก่อนใช้อันนั้น — ถือเครื่องอยู่แปลว่าไม่มีใครขวาง บอกว่าทำ
/// ด้านไหนอยู่ · ยังไม่ถึงคิวแปลว่ามีคนอยู่ข้างหน้า บอกว่าอีกกี่คิวถึงตัวเอง
/// ไม่รวมสองอย่างเข้าด้วยกัน เพราะงานหนึ่งเป็นได้อย่างเดียวในเวลาหนึ่ง
/// </para>
/// </summary>
public static class JobStageService
{
    private const string Pending = "pending";
    private const string Active = "active";

    /// <summary>คำในวงเล็บของงานที่จบรอบหนึ่งแล้ว รอคนกดเริ่มงานรอบสอง</summary>
    public const string AwaitingRoundTwo = "รอรอบสอง";

    /// <summary>
    /// คำในวงเล็บของงานนี้ — null เมื่อไม่มีอะไรต้องบอก
    /// </summary>
    /// <param name="rows">
    /// คิวทั้งหมดที่ยังไม่ปล่อยเครื่อง ตามลำดับที่ backend คืนมา ซึ่งเรียงตามลำดับ
    /// ที่แต่ละเครื่องจะยกให้อยู่แล้ว — ลำดับนี้คือที่มาของเลข Q
    /// </param>
    /// <param name="commands">
    /// ประวัติการส่งของงานนี้ — ใช้บอกว่างานที่ไม่มีแถวในคิวแล้วกำลังรอรอบถัดไปอยู่
    /// ไม่ส่งมาก็ไม่บอกเรื่องนี้
    /// </param>
    public static string? Describe(
        int jobId, string? markingMethod, IEnumerable<MachineQueueRow> rows,
        IEnumerable<CommandResult>? commands = null)
    {
        var all = rows as IList<MachineQueueRow> ?? rows.ToList();
        var mine = all.Where(r => r.PrintJobsId == jobId).ToList();

        // ไม่มีแถวในคิว แต่ยังส่งไม่ครบทุกรอบ = กดปุ่มหน้างานรอบแรกแล้ว รอคนกดรอบสอง
        if (mine.Count == 0)
            return NextRound(markingMethod, commands) is not null ? AwaitingRoundTwo : null;

        if (mine.Any(r => r.NeedsSendReview)) return "กำลังส่ง / รอตรวจสอบผล";

        var plan = MarkingMethodService.Resolve(markingMethod);

        // ถือเครื่องอยู่ = ไม่มีใครขวาง บอกด้านที่กำลังทำ
        if (FirstInPlanOrder(plan, mine.Where(IsActive)) is { } holding)
            return SideOf(plan, holding);

        // ยังไม่ถึงคิว บอกว่าอีกกี่คิวถึงตัวเอง
        if (FirstInPlanOrder(plan, mine.Where(IsPending)) is not { } waiting) return null;

        int position = all
            .Where(r => IsPending(r) && SameMachine(r.Machine, waiting.Machine))
            .ToList()
            .FindIndex(r => r.Id == waiting.Id);

        return position < 0 ? null : $"Q{position + 1}";
    }

    /// <summary>
    /// แถวที่ชิ้นงานจะไปถึงก่อน — เรียงตามลำดับของแผน แล้วค่อยเรียงตามรอบ
    ///
    /// งานใบเดียวถือได้หลายเครื่องพร้อมกันเมื่อทุกเครื่องว่าง (marking 11 จอง UV1
    /// กับ UV2 พร้อมกัน) ต้องบอกด้านที่มาก่อนตามสายการผลิต ไม่ใช่ตัวที่บังเอิญมาก่อน
    /// ในรายการที่ backend คืนมา
    /// </summary>
    private static MachineQueueRow? FirstInPlanOrder(
        MarkingPlan plan, IEnumerable<MachineQueueRow> rows) =>
        rows.OrderBy(r => PlanIndex(plan, r.Machine)).ThenBy(r => r.Round).FirstOrDefault();

    private static int PlanIndex(MarkingPlan plan, string machine)
    {
        int index = plan.Steps.FindIndex(step => SameMachine(step, machine));
        return index < 0 ? int.MaxValue : index;
    }

    /// <summary>
    /// แถวนี้เป็นด้าน plate หรือ shim — null เมื่อแผนไม่ได้บอกไว้
    ///
    /// <para>
    /// งานที่เข้าเครื่องเดิมสองรอบ (marking 22) ทั้งสองด้านเป็นเครื่องเดียวกัน แยกกัน
    /// ด้วยรอบเท่านั้น — รอบแรกพ่นลงเหล็กคือ plate รอบสองพ่นลงบน shim ที่ติดมา
    /// </para>
    /// </summary>
    private static string? SideOf(MarkingPlan plan, MachineQueueRow row)
    {
        if (plan.Plate == plan.Shim)
            return plan.Plate == MarkingMachine.None
                ? null
                : row.Round >= 2 ? "Mark Shim" : "Mark Plate";

        if (SameMachine(MarkingMethodService.Label(plan.Plate), row.Machine)) return "Mark Plate";
        if (SameMachine(MarkingMethodService.Label(plan.Shim), row.Machine)) return "Mark Shim";

        return null;
    }

    /// <summary>
    /// รอบถัดไปของเครื่องที่งานนี้ต้องเข้าซ้ำ — null เมื่อไม่มีรอบค้าง
    ///
    /// <para>
    /// ใช้กับงานที่แผนให้เข้าเครื่องเดิมหลายรอบ วันนี้มีแค่ marking 22 ที่เป็น MK สองรอบ
    /// รอบแรกพ่นในไลน์ แล้วเอาชิ้นงานออกไปติด shim ก่อนกลับมาพ่นรอบสอง รอบสองไม่ได้
    /// จองคิวไว้ล่วงหน้า คนต้องกดเริ่มงานรอบสองเองเมื่อชิ้นงานพร้อม
    /// </para>
    /// <para>
    /// นับจากประวัติว่าส่งเข้าเครื่องนั้นสำเร็จไปแล้วกี่ครั้ง ด้วยวิธีเดียวกับที่ปุ่ม
    /// ในตารางใช้ดูว่าขั้นไหนส่งไปแล้ว ยังไม่เคยส่งเลยไม่นับ เพราะนั่นคืองานที่ยังไม่เริ่ม
    /// </para>
    /// </summary>
    public static (string Machine, int Round)? NextRound(
        string? markingMethod, IEnumerable<CommandResult>? commands)
    {
        var steps = MarkingMethodService.Resolve(markingMethod).Steps;

        foreach (var machine in steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            int rounds = steps.Count(s => SameMachine(s, machine));
            if (rounds < 2) continue;

            int sent = commands?.Count(c => c.Success && SameMachine(c.Command, machine)) ?? 0;
            if (sent > 0 && sent < rounds) return (machine, sent + 1);
        }

        return null;
    }

    private static bool IsActive(MachineQueueRow row) =>
        string.Equals(row.State, Active, StringComparison.OrdinalIgnoreCase);

    private static bool IsPending(MachineQueueRow row) =>
        string.Equals(row.State, Pending, StringComparison.OrdinalIgnoreCase);

    private static bool SameMachine(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
