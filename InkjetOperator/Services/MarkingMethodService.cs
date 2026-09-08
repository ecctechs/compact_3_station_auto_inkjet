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
        var code = Code(markingMethod);
        var shimDigit = code.Length >= 2 ? code[0] : '0';
        var plateDigit = code.Length >= 2 ? code[1] : '0';

        // Shim=MK + Plate=UV ไม่มีอยู่จริงตามสเปกของสายการผลิต
        if (shimDigit == '2' && plateDigit == '1')
            return new MarkingPlan(true, MarkingMachine.None, MarkingMachine.None, []);

        var steps = new List<string>();
        if (shimDigit == '2' || plateDigit == '2') steps.Add("MK");
        if (plateDigit == '1') steps.Add("UV1");
        if (shimDigit == '1') steps.Add("UV2");

        var plate = plateDigit switch
        {
            '1' => MarkingMachine.Uv1,
            '2' => MarkingMachine.Mk,
            _ => MarkingMachine.None,
        };
        var shim = shimDigit switch
        {
            '1' => MarkingMachine.Uv2,
            '2' => MarkingMachine.Mk,
            _ => MarkingMachine.None,
        };

        return new MarkingPlan(false, plate, shim, steps);
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
        var code = Code(markingMethod);
        return station == StationService.St3
            ? St3Codes.Contains(code)
            : code != "10";
    }

    /// <summary>
    /// สถานีนี้กดเริ่มงาน marking นี้ได้ไหม
    ///
    /// 10 เริ่มได้ที่ ST3 เท่านั้น · ที่เหลือเริ่มได้ที่ ST1 เท่านั้น
    /// (11 กับ 12 นั้น ST3 เห็นงานได้แต่กดเริ่มไม่ได้ ต้องให้ ST1 เริ่ม)
    /// </summary>
    public static bool CanStartAt(int station, string? markingMethod) =>
        Code(markingMethod) == "10"
            ? station == StationService.St3
            : station == StationService.St1;

    /// <summary>
    /// สถานีนี้กดจบงาน marking นี้ได้ไหม
    ///
    /// ทั้ง 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น เพราะ UV2 เป็นขั้นตอนสุดท้ายของทั้งสามรหัส
    /// ST1 เห็น 11 กับ 12 ในตารางและกดเริ่มได้ แต่คนที่รู้ว่างานจบจริงคือคนที่ ST3
    /// </summary>
    public static bool CanCompleteAt(int station, string? markingMethod) =>
        !St3Codes.Contains(Code(markingMethod)) || station == StationService.St3;

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
        var code = (markingMethod ?? "").Trim();
        if (code.Length < 2) return code;

        return $"{Same(code[0])}{Same(code[1])}";

        static char Same(char digit) => digit == '3' ? '1' : digit;
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
