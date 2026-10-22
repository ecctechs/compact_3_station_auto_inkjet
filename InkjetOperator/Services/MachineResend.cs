using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// ส่งข้อมูลซ้ำเข้าเครื่องที่งานถืออยู่และส่งไปแล้ว — ปุ่ม "ส่ง MK / UV1 / UV2 ใหม่" ในหน้า Order Detail
///
/// <para>
/// หน้างานแจ้ง 22/10/2026: กดเริ่มงานแล้วกด SWAP หัว MK (หรือแก้ข้อความ / ทิศทาง) ค่าใหม่ถูกบันทึก
/// แต่ไม่มีทางเข้าเครื่อง เพราะคิวหนึ่งเริ่มส่งได้ครั้งเดียว — ทางนี้ส่งซ้ำผ่านคิวเดิม
/// (backend <c>begin-resend</c>) ได้กฎการส่งเดิมครบ และไม่นับเป็นรอบ / รุ่นใหม่
/// </para>
/// <para>
/// กฎว่าเครื่องไหนส่งซ้ำได้อยู่ที่นี่ที่เดียว หน้า Order Detail ใช้เปิดปิดปุ่ม · หน้า Order List
/// ใช้ตรวจซ้ำกับข้อมูลสดก่อนส่งจริง · ST1 ใช้ตรวจคำขอจาก ST3
/// </para>
/// </summary>
public static class MachineResend
{
    /// <summary>คำขอส่งซ้ำที่ ST3 ฝากให้ ST1 — ใส่ในช่อง remote_step เช่น "RESEND_UV2"</summary>
    private const string RemotePrefix = "RESEND_";

    public static string RemoteStep(string machine) => RemotePrefix + machine.Trim().ToUpperInvariant();

    /// <summary>เครื่องของคำขอส่งซ้ำ — null = ไม่ใช่คำขอส่งซ้ำ (เป็นคำขอเริ่มงานปกติ)</summary>
    public static string? MachineOfRemote(string? remoteStep)
    {
        var step = (remoteStep ?? "").Trim();
        return step.StartsWith(RemotePrefix, StringComparison.OrdinalIgnoreCase) && step.Length > RemotePrefix.Length
            ? step[RemotePrefix.Length..].ToUpperInvariant()
            : null;
    }

    /// <summary>
    /// แถวที่ส่งซ้ำได้ของงาน + เครื่องนี้ หรือเหตุผลที่ส่งไม่ได้ (ข้อความสั้นสำหรับคนหน้างาน)
    ///
    /// <para>
    /// ส่งซ้ำได้เฉพาะตอนงานถือเครื่องอยู่ และส่งรอบแรกเข้าไปแล้ว — ยังไม่ส่ง ระบบจะส่งค่าล่าสุดให้เอง
    /// ตามคิวอยู่แล้ว · ปล่อยเครื่องแล้ว ชิ้นงานไปต่อแล้ว · กำลังส่ง ห้ามซ้อน
    /// ส่งซ้ำรอบก่อนไม่รู้ผล ส่งใหม่ได้ (คนกดเองหลังตรวจเครื่อง)
    /// </para>
    /// </summary>
    /// <param name="jobStatus">null = ไม่ตรวจสถานะงาน (backend ตรวจซ้ำอีกชั้นตอนขอสิทธิ์ส่ง)</param>
    public static (MachineQueueRow? Row, string? Reason) Check(
        int jobId, string? jobStatus, string machine, IEnumerable<MachineQueueRow> queue)
    {
        if (jobStatus != null && !string.Equals(jobStatus, "Process", StringComparison.OrdinalIgnoreCase))
            return (null, string.Equals(jobStatus, "Waiting", StringComparison.OrdinalIgnoreCase)
                ? "งานยังไม่เริ่ม"
                : "งานจบหรือยกเลิกแล้ว");

        var mine = queue.Where(r => r.PrintJobsId == jobId
            && string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase)).ToList();

        var held = mine.FirstOrDefault(r => r.State == "active");
        if (held == null)
            return (null, mine.Any(r => r.State == "pending")
                ? $"ยังรอคิว {machine} — ถึงคิวแล้วระบบส่งค่าล่าสุดให้เอง"
                : $"งานนี้ไม่ได้ถือ {machine} แล้ว (ปล่อยเครื่องไปแล้ว หรือยังไม่ได้จอง)");

        if (held.AwaitingPick) return (null, $"{machine} รอ ST3 เลือกรุ่นก่อน");
        if (held.DispatchState == "sending") return (null, $"{machine} กำลังส่งอยู่ — รอให้เสร็จก่อน");
        if (held.SentAt == null)
            return (null, $"{machine} ยังไม่ได้ส่งรอบแรก — ระบบส่งค่าล่าสุดให้เองตามคิว");

        return (held, null);
    }

    /// <summary>
    /// รุ่นโปรแกรม UV ที่ต้องส่งซ้ำ — รุ่นที่อยู่ในเครื่องตอนนี้ ไม่ใช่ชื่อฐานหรือรุ่นใหม่
    /// แถวคิวเก็บไว้ (เลือกตอนเริ่มงาน / ST3 เลือก / เปลี่ยนรุ่น) · แถวรุ่นเก่าไม่มี ใช้รุ่นที่ส่งสำเร็จล่าสุด
    /// MK ไม่มีรุ่น คืน null
    /// </summary>
    public static string? UvProgram(MachineQueueRow row, IEnumerable<CommandResult>? commands)
    {
        if (string.Equals(row.Machine, "MK", StringComparison.OrdinalIgnoreCase)) return null;
        if (!string.IsNullOrWhiteSpace(row.ProgramName)) return row.ProgramName.Trim();

        var last = commands?.LastOrDefault(c => c.Success
            && string.Equals(c.Command, row.Machine, StringComparison.OrdinalIgnoreCase)
            && c.Payload != null && c.Payload.ContainsKey("program"));
        var name = last?.Payload?["program"]?.ToString()?.Trim();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    /// <summary>
    /// สถานีนี้มีปุ่มส่งซ้ำของเครื่องนี้ไหม — ST3 ไม่คุยกับเครื่องเอง มีแค่ UV2 (เครื่องอยู่ที่ ST3)
    /// ซึ่งเป็นการฝากให้ ST1 ส่ง · สถานีอื่น (ST1) ส่งเองได้ทุกเครื่อง
    /// </summary>
    public static bool StationCanRequest(string machine) =>
        !StationService.IsSt3 || string.Equals(machine, "UV2", StringComparison.OrdinalIgnoreCase);
}
