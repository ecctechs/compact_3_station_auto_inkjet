using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// ตัวเลือก "รุ่นย่อย UV2 ให้ ST3 เลือก" — เปิด/ปิดที่ Setting → Station Options (โหมด Dev) ค่าเริ่มต้นปิด
/// <para>
/// เครื่อง UV2 ตั้งอยู่ที่ ST3 คนที่ ST3 เห็นทั้งชิ้นงานและจอเครื่องตรงหน้า จึงเลือกรุ่นย่อยได้ถูกกว่า
/// คนที่ ST1 ซึ่งอยู่ไกล — เปิดแล้ว ST1 กดเริ่มงานที่มี UV2 (11 / 12) จะส่งเครื่องอื่นตามปกติ
/// แต่ไม่ถามรุ่น UV2 และยังไม่จอง UV2 ถ้าโปรแกรม UV2 มีให้เลือกมากกว่าหนึ่งไฟล์
/// แถวงานขึ้น "Pick UV2 (ST3)" และ ST3 มีปุ่ม "เลือกรุ่น UV2" เลือกแล้วจองคิวพร้อมชื่อไฟล์ให้ ST1 ส่ง
/// </para>
/// <para>
/// ค่าอยู่ใน Setting.config — ตัวที่มีผลคือเครื่อง ST1 (คนกดเริ่มงาน) ST3 ไม่ต้องตั้งอะไร
/// เพราะ "งานไหนรอ ST3 เลือก" อ่านจากข้อมูลงานกับคิวที่ทุกสถานีเห็นเหมือนกัน (<see cref="Awaiting"/>)
/// </para>
/// </summary>
public static class Uv2PickAtSt3
{
    /// <summary>คีย์ใน Setting.config — "1" = เปิด</summary>
    public const string Key = "UV2_PICK_AT_ST3";

    /// <summary>ขั้นที่ฝากให้ ST3 เลือก</summary>
    public const string Step = "UV2";

    public static bool Enabled => CustomSettingsManager.Read(Key, "0").Trim() == "1";

    /// <summary>
    /// งานนี้รอ ST3 เลือกรุ่น UV2 อยู่ไหม — เริ่มงานแล้ว แผนมี UV2 แต่ UV2 ยังไม่เคยส่งสำเร็จ
    /// และไม่มีแถวคิว UV2 ค้างอยู่ (ไม่รอคิว ไม่ถือเครื่อง)
    /// <para>
    /// ตอนปิดตัวเลือก ST1 จองคิว UV2 ตั้งแต่กดเริ่มเสมอ งานที่เริ่มแล้วจึงมีแถว UV2 หรือส่ง UV2
    /// ไปแล้ว ไม่เข้าเงื่อนไขนี้ — ปุ่มเลือกรุ่นจะไม่โผล่มาเอง
    /// </para>
    /// </summary>
    public static bool Awaiting(PrintJob job, IEnumerable<MachineQueueRow> queue) =>
        Awaiting(job.Id, job.Status, job.PlanRouting?.MarkingMethod, job.Commands, queue);

    public static bool Awaiting(
        int jobId, string? status, string? markingMethod,
        IEnumerable<CommandResult>? commands, IEnumerable<MachineQueueRow> queue)
    {
        var steps = MarkingMethodService.Resolve(markingMethod).Steps;
        if (!steps.Contains(Step, StringComparer.OrdinalIgnoreCase)) return false;

        if (commands?.Any(c => c.Success && string.Equals(c.Command, Step, StringComparison.OrdinalIgnoreCase)) == true)
            return false;

        var rows = queue.Where(r => r.PrintJobsId == jobId
            && string.Equals(r.Machine, Step, StringComparison.OrdinalIgnoreCase)
            && r.State is "pending" or "active").ToList();
        bool booked = rows.Count > 0 && rows.All(r => r.AwaitingPick);

        // กำลังผลิต: ไม่มีแถว UV2 (งานที่เริ่มจากโปรแกรมรุ่นก่อน) หรือแถวที่จองไว้ยังรอเลือกรุ่น
        if (string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase))
            return rows.Count == 0 || booked;

        // Waiting แต่จองคิว UV2 ไว้รอเลือก — เครื่องแรกส่งไม่ผ่านแน่นอน backend ตีงานกลับเป็น Waiting
        // แต่แถว UV2 ยังถือคิว / เครื่องอยู่ ต้องให้ ST3 เลือกได้ ไม่งั้นเครื่อง UV2 ถูกถือค้าง
        // (UV2 ส่งแยกจากเครื่องอื่นได้ เหมือนงานที่ไม่ได้ฝาก ST3 ซึ่งส่งพร้อมกันตอนกดเริ่ม)
        return string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase) && booked;
    }
}
