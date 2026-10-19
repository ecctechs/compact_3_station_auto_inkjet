using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// รุ่นย่อย .uvdx ที่ยังไม่ได้พิมพ์ของงานหนึ่ง
///
/// <para>
/// งานหนึ่งใบที่โปรแกรม UV มีรุ่นย่อยหลายไฟล์ (เช่น SPK-LOT, SPK-LOT-1, SPK-LOT-2)
/// พิมพ์ได้มากกว่าหนึ่งรุ่น โดยเลือกทีละรุ่น พิมพ์เสร็จแล้วกด "เปลี่ยนรุ่น" บนจอ
/// ระหว่างที่งานยังถือเครื่องอยู่ (ข้อ 2.1 รายงานหน้างาน 18/10/2026) — ปุ่มหน้าเครื่อง
/// มีความหมายเดียวคือจบงาน รุ่นที่พิมพ์ไปแล้วไม่โผล่ให้เลือกซ้ำ ไม่ต้องพิมพ์ครบทุกรุ่น
/// </para>
/// <para>
/// รุ่นที่พิมพ์ไปแล้วอ่านจาก payload ของ command ที่ส่งสำเร็จ ซึ่งเก็บชื่อไฟล์ไว้อยู่แล้ว
/// จึงไม่ต้องเพิ่มคอลัมน์หรือตารางใหม่ที่ backend
/// </para>
/// </summary>
public static class UvVariantService
{
    /// <summary>คีย์ใน Setting.config — "1" = เปิด</summary>
    public const string Key = "UV_VARIANT_REPEAT";

    /// <summary>
    /// เปิดใช้การพิมพ์รุ่นย่อยเพิ่มให้งานใบเดิมไหม
    ///
    /// <para>
    /// <b>ค่าเริ่มต้นปิด</b> — เลือกรุ่นย่อยครั้งเดียวตอนกดเริ่มงาน เหมือนที่หน้างาน
    /// ใช้อยู่เดิม ปิดแล้วไม่มีปุ่ม "เปลี่ยนรุ่น" (ปุ่ม "เลือกรุ่น UV2" ของงานที่ฝากให้ ST3
    /// เลือกยังอยู่ — เป็นของตัวเลือก <see cref="Uv2PickAtSt3"/>)
    /// </para>
    /// <para>
    /// เปิดได้ที่ Setting → ตัวเลือกหน้างาน (โหมด Dev) เมื่อพร้อมลองที่หน้างานจริง
    /// </para>
    /// </summary>
    public static bool Enabled => CustomSettingsManager.Read(Key, "0").Trim() == "1";

    /// <summary>คีย์ใน payload ของ command ที่เก็บชื่อรุ่นย่อยที่ส่งจริง</summary>
    private const string ProgramKey = "program";

    public static int UvNumber(string step) =>
        string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

    /// <summary>
    /// สถานีนี้เป็นคนเลือกรุ่นของเครื่องนี้ไหม
    ///
    /// <para>
    /// UV1 ตั้งอยู่ที่ ST1 · UV2 ตั้งอยู่ที่ ST3 — คนที่ยืนอยู่หน้าเครื่องเห็นทั้งชิ้นงาน
    /// และจอเครื่อง จึงเลือกรุ่นได้ถูกกว่าคนที่อยู่อีกสถานี โหมดทดสอบเห็นทั้งคู่
    /// </para>
    /// </summary>
    public static bool StationOwns(string step) =>
        StationService.IsDevMode
        || (UvNumber(step) == 1 ? StationService.IsSt1Menu : StationService.IsSt3);

    /// <summary>ชื่อรุ่นย่อยที่ส่งสำเร็จไปแล้วของขั้นนี้</summary>
    public static HashSet<string> SentVariants(IEnumerable<CommandResult>? commands, string step)
    {
        var sent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in commands ?? [])
        {
            if (!c.Success) continue;
            if (!string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) continue;
            if (c.Payload == null) continue;
            if (!c.Payload.TryGetValue(ProgramKey, out var value)) continue;

            var name = value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(name)) sent.Add(name);
        }
        return sent;
    }

    /// <summary>
    /// แถวที่งานนี้ถือเครื่องของขั้นนี้อยู่ และส่งรุ่นเดิมเข้าเครื่องเรียบร้อยแล้ว — null = เปลี่ยนรุ่นไม่ได้
    ///
    /// <para>
    /// เปลี่ยนรุ่นได้เฉพาะตอนนี้ ยังไม่ถึงคิว / ยังไม่ได้ส่ง / ส่งไปแล้วยังไม่รู้ผล
    /// ห้ามเปลี่ยน — ไม่งั้นรุ่นใหม่จะไปทับรุ่นที่เครื่องยังไม่ได้รับ หรือส่งซ้อนกัน
    /// </para>
    /// </summary>
    public static MachineQueueRow? HeldRow(int jobId, string step, IEnumerable<MachineQueueRow> queue) =>
        queue.FirstOrDefault(r => r.PrintJobsId == jobId
            && string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase)
            && r.State == "active" && r.SentAt != null && !r.NeedsSendReview);

    /// <summary>
    /// ขั้นนี้อยู่ในแผนของงานนี้ และงานยังอยู่ระหว่างผลิตไหม
    /// </summary>
    public static bool InPlan(string? status, string? markingMethod, string step) =>
        string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase)
        && MarkingMethodService.Resolve(markingMethod).Steps
            .Contains(step, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// รุ่นย่อยทั้งหมดของโปรแกรมนี้ที่มีไฟล์อยู่จริง — อ่านโฟลเดอร์บนเธรดพื้นหลัง
    ///
    /// <para>
    /// โฟลเดอร์ .uvdx อยู่บนเครื่อง UV ผ่านเครือข่าย อ่านช้าหรือค้างได้ ห้ามเรียกจาก
    /// เส้นทางที่วาดตาราง ผู้เรียกต้องจำผลไว้เอง ไม่อ่านซ้ำทุกรอบ poll
    /// </para>
    /// </summary>
    public static async Task<List<string>> AllVariantsAsync(string? programName, string step)
    {
        if (string.IsNullOrWhiteSpace(programName)) return [];

        var docFolder = await UvSettingsManager.GetDocumentFolderAsync(UvNumber(step));
        if (docFolder == null) return [];

        return await UvProgramResolver.VariantsAsync(programName, docFolder);
    }

    /// <summary>
    /// ขั้น UV นี้เริ่มพิมพ์ไปแล้วอย่างน้อยหนึ่งรุ่นไหม — ยังไม่เริ่มไม่นับว่า "เหลือรุ่น"
    /// (รุ่นแรกเลือกตอนกดเริ่มงาน / ฝากให้ ST3 เลือก ซึ่งนับเป็นขั้นที่ยังไม่ส่งอยู่แล้ว)
    /// </summary>
    public static bool Started(IEnumerable<CommandResult>? commands, string step) =>
        commands?.Any(c => c.Success && string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    /// <summary>
    /// ต้องนับรุ่นที่ยังไม่ได้พิมพ์ของขั้นนี้ไหม — เริ่มพิมพ์แล้ว และคนไม่ได้ยืนยันข้ามเครื่องนี้
    ///
    /// <para>
    /// ข้ามเครื่องกลางคัน (เครื่องเสียหลังพิมพ์ไปบางรุ่น) แปลว่าตั้งใจไม่พิมพ์ที่เครื่องนี้อีก
    /// รุ่นที่เหลือจึงไม่ใช่ "ยังไม่ครบ" — ไม่งั้นปุ่มจบงานค้างสีส้ม เตือนตอนจบ และงานไม่จบเอง
    /// </para>
    /// </summary>
    public static bool CountsRemaining(IEnumerable<CommandResult>? commands, string step) =>
        Started(commands, step)
        && commands?.Any(c => c.Success
            && string.Equals(MarkingMethodService.SkippedStep(c.Command), step, StringComparison.OrdinalIgnoreCase)) != true;

    /// <summary>รุ่นที่ยังไม่ได้พิมพ์ — ทั้งหมดลบที่ส่งไปแล้ว</summary>
    public static List<string> Remaining(List<string> all, IEnumerable<CommandResult>? commands, string step)
    {
        var sent = SentVariants(commands, step);
        return all.Where(v => !sent.Contains(v)).ToList();
    }
}
