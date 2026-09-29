namespace InkjetOperator.Services;

public enum MarkingMachine
{
    None,

    Mk,

    Uv1,

    Uv2,
}

public sealed record MarkingPlan(
    bool NoCase,
    MarkingMachine Plate,
    MarkingMachine Shim,
    List<string> Steps);

public static class MarkingMethodService
{
    public static MarkingPlan Resolve(string? markingMethod) // แปลงรหัส Marking เป็นแผนเครื่อง
    {
        var code = Code(markingMethod);
        var shimDigit = code.Length >= 2 ? code[0] : '0'; // หลักแรกคือวิธีพิมพ์ฝั่ง Shim
        var plateDigit = code.Length >= 2 ? code[1] : '0'; // หลักที่สองคือวิธีพิมพ์ฝั่ง Plate

        if (shimDigit == '2' && plateDigit == '1') // Shim ใช้ MK แต่ Plate ใช้ UV เป็นคู่ที่ไม่รองรับ
            return new MarkingPlan(true, MarkingMachine.None, MarkingMachine.None, []);

        var steps = new List<string>(); // เก็บลำดับเครื่องที่ต้องส่งตามวิธีพิมพ์

        if (shimDigit == '2' && plateDigit == '2') steps.Add("MK"); // 22 เพิ่มรอบ MK อีกหนึ่งรอบ เพื่อแยก Plate กับ Shim

        if (shimDigit == '2' || plateDigit == '2') steps.Add("MK"); // มีฝั่งใดใช้ MK ให้เพิ่มขั้น MK
        if (plateDigit == '1') steps.Add("UV1"); // Plate ใช้ UV ให้ส่งผ่าน UV1
        if (shimDigit == '1') steps.Add("UV2"); // Shim ใช้ UV ให้ส่งผ่าน UV2

        var plate = plateDigit switch // ระบุเครื่องของฝั่ง Plate สำหรับแสดงแผน
        {
            '1' => MarkingMachine.Uv1, // Plate รหัส 1 ใช้ UV1
            '2' => MarkingMachine.Mk,
            _ => MarkingMachine.None,
        };
        var shim = shimDigit switch // ระบุเครื่องของฝั่ง Shim สำหรับแสดงแผน
        {
            '1' => MarkingMachine.Uv2, // Shim รหัส 1 ใช้ UV2
            '2' => MarkingMachine.Mk,
            _ => MarkingMachine.None,
        };

        return new MarkingPlan(false, plate, shim, steps);
    }

    private static readonly string[] St3Codes = ["10", "11", "12"];

    public static bool VisibleAt(int station, string? markingMethod) // ตรวจว่างานนี้ควรแสดงที่ Station ไหน
    {
        var code = Code(markingMethod);
        return station == StationService.St3
            ? St3Codes.Contains(code)
            : code != "10";
    }

    public static List<string> MissingSteps( // เทียบจำนวนครั้งที่ส่งกับขั้นในแผน
        string? markingMethod, IEnumerable<Models.CommandResult>? commands)
    {
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
                continue;
            }

            missing.Add(step); // ยังไม่มีประวัติรองรับขั้นนี้ ให้แสดงว่ายังขาด
        }

        return missing;
    }

    public static bool FinishedIncomplete(
        string? status, string? markingMethod, IEnumerable<Models.CommandResult>? commands) =>
        string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase)
        && MissingSteps(markingMethod, commands).Count > 0;

    public static bool CanStartAt(int station, string? markingMethod) => // ตรวจว่า Station นี้เริ่มรหัสพิมพ์นี้ได้ไหม
        Code(markingMethod) == "10" // 10 เป็นงานที่เริ่มจาก UV2 อย่างเดียว
            ? station == StationService.St3
            : station == StationService.St1;

    public static bool CanCompleteAt(int station, string? markingMethod) => // ตรวจว่า Station นี้จบงานได้ไหม
        !St3Codes.Contains(Code(markingMethod)) || station == StationService.St3; // งานที่ผ่าน ST3 ต้องจบที่ ST3 ส่วนรหัสอื่นไม่จำกัดด้วยกฎนี้

    private static string Code(string? markingMethod)
    {
        var code = (markingMethod ?? "").Trim(); // ตัดช่องว่างของรหัสจากฐานข้อมูล
        if (code.Length < 2) return code; // รหัสไม่ครบสองหลัก ยังไม่แปลงเลขแต่ละฝั่ง

        return $"{Same(code[0])}{Same(code[1])}";

        static char Same(char digit) => digit == '3' ? '1' : digit; // รหัส 3 ใช้เส้นทางเครื่องเดียวกับรหัส 1
    }

    public static string Label(MarkingMachine machine) => machine switch
    {
        MarkingMachine.Mk => "MK",
        MarkingMachine.Uv1 => "UV1",
        MarkingMachine.Uv2 => "UV2",
        _ => "None",
    };
}
