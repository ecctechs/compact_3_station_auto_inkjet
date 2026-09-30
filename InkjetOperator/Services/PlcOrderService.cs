using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class PlcOrderService
{
    public sealed record PlcField(string Label, string ListName, int? Address, int Value);

    public readonly record struct BlockResult(string Name, int Value, int? ReadBack, string? Error);

    public static async Task<List<PlcField>> BuildPlanAsync( // จับค่าตำแหน่งและสายพานกับ register ของ PLC
        ApiClient? api, PatternDetail? pattern, bool usedHeadsOnly = false) // รับตัวอ่านฐาน Pattern และตัวเลือกกรองหัว
    {
        var map = api == null // ตรวจว่ามี Backend ให้อ่าน register map ไหม
            ? new List<PlcRegisterMap>() // ไม่มี Backend ให้เริ่มด้วย map ว่าง
            : await api.GetAllPlcSettingsAsync(); // อ่าน address ที่ตั้งไว้ในฐานกลาง

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058"); // อ่านชื่อหัว MK ตัวแรกจาก Setting
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059"); // อ่านชื่อหัว MK ตัวที่สองจาก Setting

        var speeds = pattern?.ConveyorSpeeds; // อ่านความเร็วสายพานของ Job ที่กำลังส่ง

        var fields = new List<PlcField>(); // เตรียมรายการค่าที่ต้องเขียน PLC
        if (!usedHeadsOnly || HasProgram(Inkjet(pattern, 1))) // โหมดส่งจริงเอาเฉพาะหัวแรกที่งานใช้
            AddServo(fields, map, mk1, Servo(pattern, 1)); // จับค่า PostAct และ Delay ของหัวแรกกับ register map
        if (!usedHeadsOnly || HasProgram(Inkjet(pattern, 2))) // หัวที่สองไม่มีงานให้ข้ามค่า servo ของหัวนั้น
            AddServo(fields, map, mk2, Servo(pattern, 2)); // จับค่า servo ของหัวที่สองกับ address ที่ตั้งไว้

        Add(fields, map, "Conveyor Speed 1", "Conveyor 1 (Hz)", Whole(speeds?.Speed1)); // ใช้ความเร็วสายพานตัวแรกเพียงตัวเดียว

        return fields; // ส่งค่าและ address ที่จับคู่แล้ว
    }

    public static async Task<List<BlockResult>> SendAsync(List<PlcField> plan) // เขียนค่า PLC แล้วอ่านกลับมาเทียบ
    {
        var results = new List<BlockResult>(); // เตรียมผลเขียนและอ่านกลับของแต่ละค่า

        var ip = CustomSettingsManager.Read("PLC_IP", "").Trim(); // อ่าน IP ของ PLC จาก Setting
        if (ip.Length == 0) // ตรวจว่ายังไม่มี IP ของ PLC
        {
            results.Add(new BlockResult("PLC", 0, null, "ยังไม่ได้ตั้งค่า IP ในหน้า PLC Setting")); // ระบุว่าต้องตั้งปลายทาง PLC ก่อน
            return results; // จบพร้อมเหตุที่ยังส่ง PLC ไม่ได้
        }

        int port = int.TryParse(CustomSettingsManager.Read("PLC_PORT", "502"), out var p) ? p : 502; // อ่านพอร์ต PLC ถ้าไม่ถูกต้องให้ใช้ 502

        var ready = plan.Where(f => f.Address != null).OrderBy(f => f.Address).ToList(); // ส่งเฉพาะค่าที่มี register map แล้ว
        if (ready.Count == 0) // ตรวจว่ามีรายการที่ map address แล้วหรือไม่
        {
            results.Add(new BlockResult("PLC", 0, null, "ไม่มีค่าไหนที่ map address ไว้")); // ระบุว่ายังไม่มี address สำหรับค่าที่จะส่ง
            return results; // จบพร้อมเหตุที่ยังส่ง PLC ไม่ได้
        }

        var (session, connectError) = await ModbusTcpService.OpenAsync(ip, port); // เปิด TCP ครั้งเดียวใช้เขียนและอ่านทุกค่า
        if (session == null) // ตรวจว่าการเปิด Modbus สำเร็จไหม
        {
            foreach (var field in ready) // แจกแจงผลต่อไม่ติดให้ทุกค่าที่รอส่ง
                results.Add(new BlockResult( // เพิ่มผลการเชื่อมต่อที่ไม่ผ่าน
                    $"D{field.Address!.Value}  {field.Label}", field.Value, null, connectError)); // แนบ register ค่าที่ตั้งใจส่ง และเหตุที่ต่อไม่ได้

            return results; // จบพร้อมเหตุที่ยังส่ง PLC ไม่ได้
        }

        using (session) // ใช้การเชื่อมต่อเดียวแล้วปิดเมื่อส่งครบ
        {
            foreach (var field in ready) // เขียนค่าที่พร้อมทีละ register
            {
                int address = field.Address!.Value; // ใช้ address ที่จับคู่ไว้ของค่านี้
                var name = $"D{address}  {field.Label}"; // ตั้งชื่อผลให้รู้ว่าเป็นค่าใดใน PLC

                var (ok, error) = await session.WriteSingleRegisterAsync(address, field.Value); // เขียนทีละ register ด้วย FC6
                if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
                {
                    results.Add(new BlockResult(name, field.Value, null, error)); // จดเหตุที่เขียน register นี้ไม่ได้
                    continue; // ข้ามรายการนี้ไปตัวถัดไป
                }

                var (readOk, values, _) = await session.ReadHoldingRegistersAsync(address, 1); // อ่านกลับทันทีเพื่อตรวจว่าค่าเข้า PLC จริง

                results.Add(new BlockResult( // เพิ่มผลอ่านกลับของค่าที่เพิ่งส่ง
                    name, field.Value, readOk && values.Length > 0 ? values[0] : null, null)); // แนบค่าที่อ่านได้ ถ้าอ่านไม่ได้ให้ว่างไว้
            }
        }

        return results; // ส่งผลตรวจ PLC ทั้งชุดกลับหน้าจอ
    }

    public const string HomePositionKey = "HEAD_HOME_POSITION";

    public const int DefaultHomePosition = 1;

    public const int MaxHomePosition = short.MaxValue;

    public static int HomePosition =>
        int.TryParse(CustomSettingsManager.Read(HomePositionKey, ""), out var value)
        && value is >= 1 and <= MaxHomePosition
            ? value
            : DefaultHomePosition;

    public static async Task<List<BlockResult>> ResetPositionAsync(ApiClient? api) // ส่งค่า Home ไปตำแหน่งหัว MK ทั้งสอง
    {
        var map = api == null // ตรวจว่ามี Backend ให้อ่าน register map ไหม
            ? new List<PlcRegisterMap>() // ไม่มี Backend ให้เริ่มด้วย map ว่าง
            : await api.GetAllPlcSettingsAsync(); // อ่าน address ที่ตั้งไว้ในฐานกลาง

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058"); // อ่านชื่อหัว MK ตัวแรกจาก Setting
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059"); // อ่านชื่อหัว MK ตัวที่สองจาก Setting

        int home = HomePosition; // ใช้ตำแหน่งเริ่มต้นจาก Setting ค่าเริ่มต้นคือ 1
        var fields = new List<PlcField>(); // เตรียมค่าตำแหน่ง Home ที่จะส่ง
        Add(fields, map, $"{mk1} PostAct", $"{mk1} ตำแหน่งเริ่มต้น", home); // คืนหัวแรกผ่านช่อง PostAct เดียวกับที่ใช้ส่งงาน
        Add(fields, map, $"{mk2} PostAct", $"{mk2} ตำแหน่งเริ่มต้น", home); // คืนหัวที่สองตามค่าเริ่มต้นเดียวกัน

        if (fields.All(f => f.Address == null)) return []; // ไม่มี address ของหัวเลย ให้ข้ามการคืน Home

        return await SendAsync(fields); // เขียนตำแหน่งเริ่มต้นและอ่านค่ากลับด้วยทางส่ง PLC เดิม
    }

    private static void AddServo(
        List<PlcField> fields, List<PlcRegisterMap> map, string machine, ServoConfigDto? servo)
    {
        Add(fields, map, $"{machine} PostAct", $"{machine} Servo Post Act.", Whole(servo?.PostAct));
        Add(fields, map, $"{machine} Delay", $"{machine} Delay (mm.)", Whole(servo?.Delay));
    }

    private static void Add(
        List<PlcField> fields, List<PlcRegisterMap> map, string listName, string label, int value)
    {
        var row = map.FirstOrDefault(r =>
            string.Equals(r.ListName?.Trim(), listName, StringComparison.OrdinalIgnoreCase));

        fields.Add(new PlcField(label, listName, row?.AddressStart, value));
    }

    private static bool HasProgram(InkjetConfigDto? config) =>
        config != null
        && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

    public static string? UnsendableReason(PatternDetail? pattern)
    {
        var names = new[]
        {
            (Ordinal: 1, Name: CustomSettingsManager.Read("MK058_NAME", "MK-058")),
            (Ordinal: 2, Name: CustomSettingsManager.Read("MK059_NAME", "MK-059")),
        };

        foreach (var (ordinal, name) in names)
        {
            if (!HasProgram(Inkjet(pattern, ordinal))) continue;

            var servo = Servo(pattern, ordinal);
            if (servo?.PostAct == null)
                return $"{name}: ยังไม่ได้กรอก Servo Post Act.";
            if (servo.Delay == null)
                return $"{name}: ยังไม่ได้กรอก Delay (mm.)";
        }

        if (pattern?.ConveyorSpeeds?.Speed1 == null)
            return "Conveyor 1 (Hz): ยังไม่ได้กรอก";

        return null;
    }

    private static ServoConfigDto? Servo(PatternDetail? pattern, int ordinal) =>
        pattern?.ServoConfigs.FirstOrDefault(s => s.Ordinal == ordinal);

    private static InkjetConfigDto? Inkjet(PatternDetail? pattern, int ordinal) =>
        pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);

    private static int Whole(double? value) =>
        value == null ? 0 : (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);

    private static int Whole(int? value) => value ?? 0;
}
