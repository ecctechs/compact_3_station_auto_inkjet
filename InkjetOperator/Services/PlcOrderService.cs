using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// ส่งค่าของงานหนึ่งเข้า PLC — servo ของ MK ทั้งสองตัวและความเร็วสายพาน
/// <para>
/// ชุดค่าและลำดับยกมาจากโปรแกรมเดิม (PySocketClient/plc_interface.py) แต่ address
/// ไม่ได้ฝังไว้ในโค้ด — อ่านจากตาราง register map ในหน้า PLC Setting จับคู่ด้วย
/// <c>list_name</c> ที่นั่นจึงเป็นที่เดียวที่กำหนดว่าค่าไหนลง register ไหน
/// </para>
/// <para>
/// โปรแกรมเดิมส่ง <c>Position</c> กับ <c>Trigger</c> เป็น 0 ตายตัวทั้งคู่ ที่นี่ Position
/// ยังเป็น 0 เหมือนเดิมเพราะไม่มีค่านี้ในงาน ส่วน <c>Trigger</c> ส่ง Trigger Delay
/// ของเครื่องนั้นไปจริง
/// </para>
/// </summary>
public static class PlcOrderService
{
    /// <summary>ค่าหนึ่งตัวที่จะส่ง พร้อม address ที่หามาได้จากตาราง map</summary>
    /// <param name="Label">ชื่อที่แสดงให้ผู้ใช้เห็น เช่น "MK-058 Servo Post Act."</param>
    /// <param name="ListName">ชื่อแถวในตาราง register map ที่ใช้จับคู่</param>
    /// <param name="Address">null = ไม่พบแถวนี้ในตาราง</param>
    public sealed record PlcField(string Label, string ListName, int? Address, int Value);

    /// <summary>ผลการเขียนหนึ่งชุด — ชุดที่ address ติดกันถูกยิงไปในคำสั่งเดียว</summary>
    public readonly record struct BlockResult(string Name, string? Error);

    /// <summary>
    /// รายการค่าทั้งหมดที่จะส่งของงานนี้ เรียงตามลำดับที่ผู้ใช้เห็นบนหน้าจอ
    /// <para>
    /// ไม่ยิงอะไรออกไป ใช้ทั้งตอนโชว์ address บนหน้า Order Detail และตอนสรุปให้ยืนยัน
    /// ก่อนส่งจริง แถวไหนหาไม่เจอในตาราง map จะได้ <c>Address = null</c> กลับไป
    /// </para>
    /// </summary>
    public static async Task<List<PlcField>> BuildPlanAsync(ApiClient? api, PatternDetail? pattern)
    {
        var map = api == null
            ? new List<PlcRegisterMap>()
            : await api.GetAllPlcSettingsAsync();

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        var speeds = pattern?.ConveyorSpeeds;

        var fields = new List<PlcField>();
        AddServo(fields, map, mk1, Servo(pattern, 1), Inkjet(pattern, 1));
        AddServo(fields, map, mk2, Servo(pattern, 2), Inkjet(pattern, 2));

        Add(fields, map, "Conveyor Speed 1", "Conveyor 1 (Hz)", Whole(speeds?.Speed1));
        Add(fields, map, "Conveyor Speed 2", "Conveyor 2 (Hz)", Whole(speeds?.Speed2));
        Add(fields, map, "Conveyor Speed 3", "Conveyor 3 (Hz)", Whole(speeds?.Speed3));

        return fields;
    }

    /// <summary>
    /// เขียนค่าตามแผนที่สร้างไว้ ข้ามตัวที่ยังไม่มี address
    /// <para>
    /// address ที่เรียงติดกันถูกรวมเป็นคำสั่งเดียว (FC 16) เพื่อให้ PLC เห็นค่าเปลี่ยน
    /// พร้อมกันทั้งชุด ไม่ใช่ทยอยเปลี่ยนทีละตัวจนได้ค่าครึ่ง ๆ กลาง ๆ ระหว่างทาง
    /// </para>
    /// </summary>
    public static async Task<List<BlockResult>> SendAsync(List<PlcField> plan)
    {
        var results = new List<BlockResult>();

        var ip = CustomSettingsManager.Read("PLC_IP", "").Trim();
        if (ip.Length == 0)
        {
            results.Add(new BlockResult("PLC", "ยังไม่ได้ตั้งค่า IP ในหน้า PLC Setting"));
            return results;
        }

        int port = int.TryParse(CustomSettingsManager.Read("PLC_PORT", "502"), out var p) ? p : 502;

        var ready = plan.Where(f => f.Address != null).OrderBy(f => f.Address).ToList();
        if (ready.Count == 0)
        {
            results.Add(new BlockResult("PLC", "ไม่มีค่าไหนที่ map address ไว้"));
            return results;
        }

        foreach (var run in GroupConsecutive(ready))
        {
            var name = run.Count == 1
                ? $"D{run[0].Address}"
                : $"D{run[0].Address}-D{run[^1].Address}";

            var (ok, error) = await ModbusTcpService.WriteMultipleRegistersAsync(
                ip, port, run[0].Address!.Value, run.Select(f => f.Value).ToList());

            results.Add(new BlockResult(name, ok ? null : error));
        }

        return results;
    }

    /// <summary>รวมค่าที่ address ต่อกันเป็นชุดเดียว รายการต้องเรียง address มาแล้ว</summary>
    private static List<List<PlcField>> GroupConsecutive(List<PlcField> sorted)
    {
        var runs = new List<List<PlcField>>();
        foreach (var field in sorted)
        {
            var last = runs.Count > 0 ? runs[^1] : null;
            if (last != null && field.Address == last[^1].Address + 1) last.Add(field);
            else runs.Add([field]);
        }
        return runs;
    }

    private static void AddServo(
        List<PlcField> fields, List<PlcRegisterMap> map, string machine,
        ServoConfigDto? servo, InkjetConfigDto? inkjet)
    {
        Add(fields, map, $"{machine} Position", $"{machine} Position", 0);
        Add(fields, map, $"{machine} PostAct", $"{machine} Servo Post Act.", Whole(servo?.PostAct));
        Add(fields, map, $"{machine} Delay", $"{machine} Delay (mm.)", Whole(servo?.Delay));

        // Trigger Delay อยู่บน InkjetConfig ไม่ใช่ ServoConfig และส่งเป็นค่าดิบตามที่
        // เก็บไว้ ไม่ได้คูณ 10 แบบตอนประกอบคำสั่ง FM ซึ่งเป็นกติกาของเครื่อง MK เอง
        Add(fields, map, $"{machine} Trigger", $"{machine} Trigger Delay", Whole(inkjet?.TriggerDelay));
    }

    private static void Add(
        List<PlcField> fields, List<PlcRegisterMap> map, string listName, string label, int value)
    {
        var row = map.FirstOrDefault(r =>
            string.Equals(r.ListName?.Trim(), listName, StringComparison.OrdinalIgnoreCase));

        fields.Add(new PlcField(label, listName, row?.AddressStart, value));
    }

    private static ServoConfigDto? Servo(PatternDetail? pattern, int ordinal) =>
        pattern?.ServoConfigs.FirstOrDefault(s => s.Ordinal == ordinal);

    private static InkjetConfigDto? Inkjet(PatternDetail? pattern, int ordinal) =>
        pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);

    /// <summary>register ของ Modbus เก็บจำนวนเต็ม 16 บิต ค่าทศนิยมจึงต้องปัดก่อนส่ง</summary>
    private static int Whole(double? value) =>
        value == null ? 0 : (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);

    private static int Whole(int? value) => value ?? 0;
}
