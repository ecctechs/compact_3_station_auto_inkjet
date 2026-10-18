using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// รุ่นย่อย .uvdx ที่ยังไม่ได้พิมพ์ของงานหนึ่ง
///
/// <para>
/// งานหนึ่งใบที่โปรแกรม UV มีรุ่นย่อยหลายไฟล์ (เช่น SPK-LOT, SPK-LOT-1, SPK-LOT-2)
/// พิมพ์ได้มากกว่าหนึ่งรุ่น โดยเลือกทีละรุ่น พิมพ์เสร็จแล้วค่อยเลือกรุ่นถัดไป
/// รุ่นที่พิมพ์ไปแล้วต้องไม่โผล่ให้เลือกซ้ำ และจบงานตอนไหนก็ได้ ไม่ต้องครบทุกรุ่น
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
    /// ใช้อยู่เดิม ปิดแล้วปุ่มเลือกรุ่นในแถวงานหายทั้งหมด และปุ่มจบงานกลับไปดูแค่
    /// ขั้นในแผนเหมือนเดิม
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
    /// เครื่องของขั้นนี้ว่างจากงานใบนี้แล้วไหม — ไม่ถือเครื่องอยู่และไม่มีแถวรอคิว
    ///
    /// <para>
    /// ปุ่มเลือกรุ่นถัดไปโผล่หลังปล่อยเครื่องแล้วเท่านั้น กติกาเดียวกับปุ่ม "เริ่มรอบสอง"
    /// ของงาน 22 — ไม่ให้สั่งรุ่นใหม่ทับระหว่างที่เครื่องยังพิมพ์ใบเดิมค้างอยู่
    /// </para>
    /// </summary>
    public static bool MachineFree(int jobId, string step, IEnumerable<MachineQueueRow> queue) =>
        !queue.Any(r => r.PrintJobsId == jobId
            && string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase)
            && r.State is "pending" or "active");

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

    /// <summary>รุ่นที่ยังไม่ได้พิมพ์ — ทั้งหมดลบที่ส่งไปแล้ว</summary>
    public static List<string> Remaining(List<string> all, IEnumerable<CommandResult>? commands, string step)
    {
        var sent = SentVariants(commands, step);
        return all.Where(v => !sent.Contains(v)).ToList();
    }
}
