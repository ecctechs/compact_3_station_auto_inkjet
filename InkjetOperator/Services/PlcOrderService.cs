using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class PlcOrderService
{
    public sealed record PlcField(string Label, string ListName, int? Address, int Value);

    public readonly record struct BlockResult(string Name, int Value, int? ReadBack, string? Error);

    public static async Task<List<PlcField>> BuildPlanAsync( // จับค่าตำแหน่งและสายพานกับ register ของ PLC
        ApiClient? api, PatternDetail? pattern, bool usedHeadsOnly = false)
    {
        var map = api == null
            ? new List<PlcRegisterMap>()
            : await api.GetAllPlcSettingsAsync();

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        var speeds = pattern?.ConveyorSpeeds; // อ่านความเร็วสายพานของ Job ที่กำลังส่ง

        var fields = new List<PlcField>();
        if (!usedHeadsOnly || HasProgram(Inkjet(pattern, 1))) // โหมดส่งจริงเอาเฉพาะหัวแรกที่งานใช้
            AddServo(fields, map, mk1, Servo(pattern, 1)); // จับค่า PostAct และ Delay ของหัวแรกกับ register map
        if (!usedHeadsOnly || HasProgram(Inkjet(pattern, 2))) // หัวที่สองไม่มีงานให้ข้ามค่า servo ของหัวนั้น
            AddServo(fields, map, mk2, Servo(pattern, 2)); // จับค่า servo ของหัวที่สองกับ address ที่ตั้งไว้

        Add(fields, map, "Conveyor Speed 1", "Conveyor 1 (Hz)", Whole(speeds?.Speed1)); // ใช้ความเร็วสายพานตัวแรกเพียงตัวเดียว

        return fields;
    }

    public static async Task<List<BlockResult>> SendAsync(List<PlcField> plan) // เขียนค่า PLC แล้วอ่านกลับมาเทียบ
    {
        var results = new List<BlockResult>();

        var ip = CustomSettingsManager.Read("PLC_IP", "").Trim();
        if (ip.Length == 0)
        {
            results.Add(new BlockResult("PLC", 0, null, "ยังไม่ได้ตั้งค่า IP ในหน้า PLC Setting"));
            return results;
        }

        int port = int.TryParse(CustomSettingsManager.Read("PLC_PORT", "502"), out var p) ? p : 502;

        var ready = plan.Where(f => f.Address != null).OrderBy(f => f.Address).ToList(); // ส่งเฉพาะค่าที่มี register map แล้ว
        if (ready.Count == 0)
        {
            results.Add(new BlockResult("PLC", 0, null, "ไม่มีค่าไหนที่ map address ไว้"));
            return results;
        }

        var (session, connectError) = await ModbusTcpService.OpenAsync(ip, port); // เปิด TCP ครั้งเดียวใช้เขียนและอ่านทุกค่า
        if (session == null)
        {
            foreach (var field in ready)
                results.Add(new BlockResult(
                    $"D{field.Address!.Value}  {field.Label}", field.Value, null, connectError));

            return results;
        }

        using (session)
        {
            foreach (var field in ready)
            {
                int address = field.Address!.Value;
                var name = $"D{address}  {field.Label}";

                var (ok, error) = await session.WriteSingleRegisterAsync(address, field.Value); // เขียนทีละ register ด้วย FC6
                if (!ok)
                {
                    results.Add(new BlockResult(name, field.Value, null, error));
                    continue;
                }

                var (readOk, values, _) = await session.ReadHoldingRegistersAsync(address, 1); // อ่านกลับทันทีเพื่อตรวจว่าค่าเข้า PLC จริง

                results.Add(new BlockResult(
                    name, field.Value, readOk && values.Length > 0 ? values[0] : null, null));
            }
        }

        return results;
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
        var map = api == null
            ? new List<PlcRegisterMap>()
            : await api.GetAllPlcSettingsAsync();

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        int home = HomePosition; // ใช้ตำแหน่งเริ่มต้นจาก Setting ค่าเริ่มต้นคือ 1
        var fields = new List<PlcField>();
        Add(fields, map, $"{mk1} PostAct", $"{mk1} ตำแหน่งเริ่มต้น", home); // คืนหัวแรกผ่านช่อง PostAct เดียวกับที่ใช้ส่งงาน
        Add(fields, map, $"{mk2} PostAct", $"{mk2} ตำแหน่งเริ่มต้น", home); // คืนหัวที่สองตามค่าเริ่มต้นเดียวกัน

        if (fields.All(f => f.Address == null)) return [];

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
