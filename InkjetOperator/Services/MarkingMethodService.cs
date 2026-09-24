namespace InkjetOperator.Services;

/// <summary>เครื่องที่รับผิดชอบด้านหนึ่งของงาน</summary>
public enum MarkingMachine
{
    /// <summary>ด้านนี้ไม่ต้องทำ</summary>
    None,

    /// <summary>MK Inkjet — marking_method ไม่ได้บอกว่าเป็นเครื่องไหนใน MK-058 / MK-059</summary>
    Mk,

    /// <summary>UV เครื่องที่ 1 — ฝั่ง Plate</summary>
    Uv1,

    /// <summary>UV เครื่องที่ 2 — ฝั่ง Shim</summary>
    Uv2,
}

/// <summary>ผลการแปล marking_method ของงานหนึ่ง</summary>
/// <param name="NoCase">true = เป็นรหัสที่ไม่มีจริง (21) ทำอะไรไม่ได้</param>
/// <param name="Plate">ด้าน Plate ทำโดยเครื่องไหน</param>
/// <param name="Shim">ด้าน Shim ทำโดยเครื่องไหน</param>
/// <param name="Steps">ขั้นตอนที่ต้องส่ง เรียงตามลำดับที่ต้องกด</param>
public sealed record MarkingPlan(
    bool NoCase,
    MarkingMachine Plate,
    MarkingMachine Shim,
    List<string> Steps);

/// <summary>
/// แปล marking_method ให้เป็นแผนการทำงานของ 1 งาน
///
/// รหัสเป็นเลข 2 หลัก: <b>หลักที่ 1 = Shim · หลักที่ 2 = Plate</b>
/// ค่าของแต่ละหลัก 0 = ไม่ทำ · 1 = UV · 2 = MK · 3 ทำงานเหมือน 1
/// ฝั่ง UV แยกเครื่องตามด้าน: Plate ไป UV1 · Shim ไป UV2
///
/// เดิมกฎนี้ถูกเขียนซ้ำไว้ 2 ที่ (หน้า Order List กับหน้า Order Detail) และตีความ
/// รหัส 21 ไม่ตรงกัน ทำให้งาน 21 กดส่งไม่ได้แต่ปุ่มจบงานกลับบอกว่ายังไม่ครบ
/// จึงย้ายมารวมไว้ที่เดียว ห้ามเขียนกฎนี้ซ้ำที่อื่นอีก
/// </summary>
public static class MarkingMethodService
{
    public static MarkingPlan Resolve(string? markingMethod)
    {
        var code = Code(markingMethod); // ทำรหัสให้เป็นรูปเดียวกันก่อนเลือกเครื่อง
        var shimDigit = code.Length >= 2 ? code[0] : '0'; // หลักแรกคือวิธีพิมพ์ฝั่ง Shim
        var plateDigit = code.Length >= 2 ? code[1] : '0'; // หลักที่สองคือวิธีพิมพ์ฝั่ง Plate

        // Shim=MK + Plate=UV ไม่มีอยู่จริงตามสเปกของสายการผลิต
        if (shimDigit == '2' && plateDigit == '1') // Shim ใช้ MK แต่ Plate ใช้ UV เป็นคู่ที่ไม่รองรับ
            return new MarkingPlan(true, MarkingMachine.None, MarkingMachine.None, []); // แจ้งตัวเรียกว่าคู่นี้ไม่มีแผนให้ส่งเครื่อง

        var steps = new List<string>(); // เก็บลำดับเครื่องที่ต้องส่งตามวิธีพิมพ์

        // 22 = เข้าเครื่อง MK สองรอบ ไม่ใช่รอบเดียว
        //
        // รอบแรกพ่นลงเหล็ก (plate) แล้วเอาชิ้นงานออกนอกไลน์ไปติด shim จากนั้นเอากลับมา
        // พ่นรอบสองลงบน shim เป็นงานพิเศษที่ทำนาน ๆ ที แต่ถ้านับเป็นรอบเดียวเหมือนเดิม
        // โปรแกรมจะบอกว่างานจบตั้งแต่พ่น plate เสร็จ ทั้งที่ยังไม่ได้พ่น shim
        if (shimDigit == '2' && plateDigit == '2') steps.Add("MK"); // 22 เพิ่มรอบ MK อีกหนึ่งรอบ เพื่อแยก Plate กับ Shim

        if (shimDigit == '2' || plateDigit == '2') steps.Add("MK"); // มีฝั่งใดใช้ MK ให้เพิ่มขั้น MK
        if (plateDigit == '1') steps.Add("UV1"); // Plate ใช้ UV ให้ส่งผ่าน UV1
        if (shimDigit == '1') steps.Add("UV2"); // Shim ใช้ UV ให้ส่งผ่าน UV2

        var plate = plateDigit switch // ระบุเครื่องของฝั่ง Plate สำหรับแสดงแผน
        {
            '1' => MarkingMachine.Uv1, // Plate รหัส 1 ใช้ UV1
            '2' => MarkingMachine.Mk, // Plate รหัส 2 ใช้ MK
            _ => MarkingMachine.None, // รหัสอื่นไม่ระบุเครื่องพิมพ์ฝั่ง Plate
        };
        var shim = shimDigit switch // ระบุเครื่องของฝั่ง Shim สำหรับแสดงแผน
        {
            '1' => MarkingMachine.Uv2, // Shim รหัส 1 ใช้ UV2
            '2' => MarkingMachine.Mk, // Shim รหัส 2 ใช้ MK
            _ => MarkingMachine.None, // รหัสอื่นไม่ระบุเครื่องพิมพ์ฝั่ง Shim
        };

        return new MarkingPlan(false, plate, shim, steps); // ส่งทั้งเครื่องแต่ละฝั่งและลำดับขั้นกลับให้หน้ารายการ
    }

    // ── งานไหนเป็นของสถานีไหน ──────────────────────────────
    //
    // กฎสามข้อล่างนี้เป็นกฎการผลิต ไม่ใช่กฎการแสดงผล จึงอยู่รวมที่นี่กับการแปลรหัส
    // ห้ามเขียนซ้ำในหน้าจอ — ที่ผ่านมาการแยกกฎไปไว้หลายที่ทำให้สองหน้าตีความไม่ตรงกัน

    /// <summary>
    /// งานที่วิ่งผ่าน ST3 — UV2 เป็นขั้นตอนสุดท้ายของทั้งสามรหัสนี้
    ///
    /// เทียบกับรหัสที่ผ่าน <see cref="Code"/> มาแล้ว จึงครอบคลุมรหัสที่ใช้เลข 3
    /// ด้วยโดยอัตโนมัติ — 30 เท่ากับ 10 · 31 กับ 33 เท่ากับ 11 · 32 เท่ากับ 12
    /// </summary>
    private static readonly string[] St3Codes = ["10", "11", "12"];

    /// <summary>
    /// สถานีนี้ควรเห็นงาน marking นี้ในตารางไหม
    ///
    /// ST3 เห็นเฉพาะงานที่ตัวเองต้องแตะ (10 / 11 / 12) · ST1 เห็นทุกอย่างยกเว้น 10
    /// ซึ่งเป็นงานที่ทำที่ ST3 ตั้งแต่ต้นจนจบ
    /// </summary>
    public static bool VisibleAt(int station, string? markingMethod)
    {
        var code = Code(markingMethod); // เทียบรหัสหลังแปลง 3 เป็น 1 แล้ว
        return station == StationService.St3 // แยกกฎแสดงงานของ ST3 ออกจาก ST1
            ? St3Codes.Contains(code) // ST3 เห็นเฉพาะ 10 / 11 / 12
            : code != "10"; // ST1 เห็นทุกงานยกเว้น 10
    }

    /// <summary>
    /// ขั้นที่แผนกำหนดไว้แต่ยังไม่มีคำสั่งส่งสำเร็จ — รายการว่างคือส่งครบแล้ว
    ///
    /// <para>
    /// อยู่ที่นี่เพราะ Order List กับ Order Detail ต้องตอบเหมือนกันเสมอว่างานหนึ่ง
    /// ส่งครบหรือยัง ถ้าแยกกันคิด วันหนึ่งสองหน้าจะบอกคนละเรื่องกันเรื่องงานเดียวกัน
    /// </para>
    /// <para>
    /// คิดจากคำสั่งที่ส่งสำเร็จจริง ไม่ได้ดูหมุด MANUAL_COMPLETE เพราะงานเก่าที่จบ
    /// ไปก่อนจะมีหมุดนั้นก็ต้องนับว่าไม่ครบเหมือนกัน
    /// </para>
    /// </summary>
    public static List<string> MissingSteps(
        string? markingMethod, IEnumerable<Models.CommandResult>? commands)
    {
        // นับจำนวนครั้ง ไม่ใช่ดูว่าเคยส่งไหม
        //
        // งาน 22 ต้องเข้าเครื่อง MK สองรอบ ถ้าดูแค่ "เคยส่ง MK ไหม" พอจบรอบแรก
        // ก็จะถือว่าครบแล้ว ทั้งที่ยังเหลืออีกรอบ
        var sent = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // นับประวัติสำเร็จแยกตามเครื่อง เพื่อรองรับ MK หลายรอบ
        foreach (var command in commands ?? []) // อ่านประวัติส่งทั้งหมดของ Job
        {
            if (!command.Success || command.Command == null) continue; // ไม่นับคำสั่งที่ไม่สำเร็จหรือไม่มีชื่อขั้น
            sent[command.Command] = sent.TryGetValue(command.Command, out int n) ? n + 1 : 1; // เพิ่มจำนวนครั้งที่ส่งเครื่องนี้สำเร็จ
        }

        var missing = new List<string>(); // เก็บขั้นที่ยังขาดประวัติสำเร็จ
        foreach (var step in Resolve(markingMethod).Steps) // เทียบประวัติกับทุกขั้นในแผน รวมชื่อเครื่องที่ซ้ำ
        {
            if (sent.TryGetValue(step, out int left) && left > 0) // ยังมีประวัติของเครื่องนี้เหลือให้จับคู่กับขั้นนี้
            {
                sent[step] = left - 1; // ใช้ประวัติไปหนึ่งครั้ง ไม่เอาไปนับซ้ำกับรอบถัดไป
                continue; // ขั้นนี้มีประวัติรองรับแล้ว ไปตรวจขั้นถัดไป
            }

            missing.Add(step); // ยังไม่มีประวัติรองรับขั้นนี้ ให้แสดงว่ายังขาด
        }

        return missing; // ส่งขั้นที่ยังขาดกลับไปเตือนก่อนจบงาน
    }

    /// <summary>
    /// งานนี้จบไปแล้วทั้งที่ยังส่งไม่ครบทุกขั้นไหม — งานที่ถูกยกเลิกไม่นับ
    /// เพราะ "ไม่ได้ทำ" ไม่ใช่ "ทำไม่ครบ"
    /// </summary>
    public static bool FinishedIncomplete(
        string? status, string? markingMethod, IEnumerable<Models.CommandResult>? commands) =>
        string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase)
        && MissingSteps(markingMethod, commands).Count > 0;

    /// <summary>
    /// สถานีนี้กดเริ่มงาน marking นี้ได้ไหม
    ///
    /// 10 เริ่มได้ที่ ST3 เท่านั้น · ที่เหลือเริ่มได้ที่ ST1 เท่านั้น
    /// (11 กับ 12 นั้น ST3 เห็นงานได้แต่กดเริ่มไม่ได้ ต้องให้ ST1 เริ่ม)
    /// </summary>
    public static bool CanStartAt(int station, string? markingMethod) =>
        Code(markingMethod) == "10" // 10 เป็นงานที่เริ่มจาก UV2 อย่างเดียว
            ? station == StationService.St3 // จึงให้เริ่มจาก ST3
            : station == StationService.St1; // วิธีพิมพ์อื่นให้เริ่มจาก ST1

    /// <summary>
    /// สถานีนี้กดจบงาน marking นี้ได้ไหม
    ///
    /// ทั้ง 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น เพราะ UV2 เป็นขั้นตอนสุดท้ายของทั้งสามรหัส
    /// ST1 เห็น 11 กับ 12 ในตารางและกดเริ่มได้ แต่คนที่รู้ว่างานจบจริงคือคนที่ ST3
    /// </summary>
    public static bool CanCompleteAt(int station, string? markingMethod) =>
        !St3Codes.Contains(Code(markingMethod)) || station == StationService.St3; // งานที่ผ่าน ST3 ต้องจบที่ ST3 ส่วนรหัสอื่นไม่จำกัดด้วยกฎนี้

    /// <summary>
    /// รหัสในรูปมาตรฐานที่ใช้เทียบทุกกฎในไฟล์นี้ — ตัดช่องว่าง และแปลง 3 เป็น 1
    ///
    /// เดิมการแปลง 3 เป็น 1 ทำอยู่ใน <see cref="Resolve"/> ที่เดียว ส่วนกฎแบ่งสถานี
    /// ข้างบนเทียบกับข้อความดิบ ผลคือรหัส 32 ซึ่งแปลได้เท่ากับ 12 ทุกประการ
    /// (MK แล้วต่อ UV2) ไม่ถูกนับเป็นงานของ ST3 — ST3 ไม่เห็นงาน และ ST1
    /// กดจบงานได้ทั้งที่ UV2 ซึ่งเป็นเครื่องของ ST3 ยังไม่ได้ทำ
    /// ในฐานข้อมูลจริงรหัส 32 มีอยู่ 689 งาน มากเป็นอันดับสอง
    ///
    /// ย้ายมาไว้ที่นี่ที่เดียว ทั้ง Resolve และกฎแบ่งสถานีจึงมองรหัสเหมือนกันเสมอ
    /// 30 · 31 · 33 · 13 ที่ผิดด้วยเหตุเดียวกันก็ถูกแก้ไปพร้อมกัน
    /// </summary>
    private static string Code(string? markingMethod)
    {
        var code = (markingMethod ?? "").Trim(); // ตัดช่องว่างของรหัสจากฐานข้อมูล
        if (code.Length < 2) return code; // รหัสไม่ครบสองหลัก ยังไม่แปลงเลขแต่ละฝั่ง

        return $"{Same(code[0])}{Same(code[1])}"; // แปลงเลขของ Shim และ Plate ให้ใช้กฎเดียวกัน

        static char Same(char digit) => digit == '3' ? '1' : digit; // รหัส 3 ใช้เส้นทางเครื่องเดียวกับรหัส 1
    }

    /// <summary>
    /// ชื่อเครื่องที่แสดงบนจอ — อยู่ที่นี่เพราะทั้ง Order List และ Order Detail
    /// ต้องเรียกด้านเดียวกันว่าชื่อเดียวกัน หน้าไหนอยากให้ None เป็นขีดก็แปลงเอง
    /// </summary>
    public static string Label(MarkingMachine machine) => machine switch
    {
        MarkingMachine.Mk => "MK",
        MarkingMachine.Uv1 => "UV1",
        MarkingMachine.Uv2 => "UV2",
        _ => "None",
    };
}
