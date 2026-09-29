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
/// <b>Trigger Delay ไม่ได้อยู่ในชุดนี้</b> — เป็นค่าของหัวพ่น ส่งเข้าเครื่อง MK
/// ผ่านคำสั่ง FM โดยตรง (ดู <c>MkCompactAdapter.SendConfigAsync</c>) ไม่ใช่ค่าที่
/// PLC ต้องรู้ เดิมส่งซ้ำไปทั้งสองทางซึ่งทำให้เข้าใจผิดว่า PLC เป็นคนคุมค่านี้
/// </para>
/// </summary>
public static class PlcOrderService
{
    /// <summary>ค่าหนึ่งตัวที่จะส่ง พร้อม address ที่หามาได้จากตาราง map</summary>
    /// <param name="Label">ชื่อที่แสดงให้ผู้ใช้เห็น เช่น "MK-058 Servo Post Act."</param>
    /// <param name="ListName">ชื่อแถวในตาราง register map ที่ใช้จับคู่</param>
    /// <param name="Address">null = ไม่พบแถวนี้ในตาราง</param>
    public sealed record PlcField(string Label, string ListName, int? Address, int Value);

    /// <summary>ผลการเขียนหนึ่ง register</summary>
    /// <param name="Name">ชื่อที่รายงานให้ผู้ใช้ เช่น "D5 MK-058 Servo Post Act."</param>
    /// <param name="Value">ค่าที่เขียนลงไป</param>
    /// <param name="ReadBack">ค่าที่อ่านกลับมาได้หลังเขียน — null เมื่ออ่านไม่สำเร็จ</param>
    public readonly record struct BlockResult(string Name, int Value, int? ReadBack, string? Error);

    /// <summary>
    /// รายการค่าทั้งหมดที่จะส่งของงานนี้ เรียงตามลำดับที่ผู้ใช้เห็นบนหน้าจอ
    /// <para>
    /// ไม่ยิงอะไรออกไป ใช้ทั้งตอนโชว์ address บนหน้า Order Detail และตอนสรุปให้ยืนยัน
    /// ก่อนส่งจริง แถวไหนหาไม่เจอในตาราง map จะได้ <c>Address = null</c> กลับไป
    /// </para>
    /// </summary>
    /// <param name="usedHeadsOnly">
    /// true = ข้ามหัวพ่นที่งานนี้ไม่ได้ใช้ ใช้ตอนจะส่งจริง · false = เอาครบทุกช่อง
    /// ใช้ตอนติดป้าย address บนหน้าจอ ซึ่งต้องเห็นทุกช่องแม้ช่องที่งานนี้ไม่ได้ใช้
    /// </param>
    public static async Task<List<PlcField>> BuildPlanAsync(
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

        // สายพานตัวเดียว — ตาราง register map เหลือ Conveyor Speed 1 แถวเดียว
        // โปรแกรมเดิมส่งสามตัวรวดเดียว (D10-D12) แต่ของใหม่ตกลงกันว่าเหลือตัวแรก
        Add(fields, map, "Conveyor Speed 1", "Conveyor 1 (Hz)", Whole(speeds?.Speed1)); // ใช้ความเร็วสายพานตัวแรกเพียงตัวเดียว

        return fields;
    }

    /// <summary>
    /// เขียนค่าตามแผนที่สร้างไว้ ข้ามตัวที่ยังไม่มี address
    /// <para>
    /// เขียนทีละ register ด้วย FC 6 แล้วอ่านกลับมายืนยัน — วิธีเดียวกับปุ่ม Write
    /// ในตาราง register map หน้า PLC Setting ทุกประการ ค่าที่ส่งคือตัวเลขที่เห็น
    /// บนหน้าจอตรง ๆ ไม่มีการคูณหรือแปลงหน่วยใด ๆ ระหว่างทาง
    /// </para>
    /// <para>
    /// เดิมรวม address ที่ติดกันแล้วยิงเป็นชุดเดียวด้วย FC 16 ซึ่งต่างจากที่หน้า
    /// PLC Setting ใช้ ตอนนี้ยึดวิธีของหน้านั้นเป็นหลัก เพื่อให้ผลที่ได้จากปุ่มนี้
    /// กับที่ได้จากการกด Write ทีละแถวเป็นอย่างเดียวกัน
    /// </para>
    /// </summary>
    public static async Task<List<BlockResult>> SendAsync(List<PlcField> plan)
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

        // เปิดสายเดียวแล้วยิงทุกคำสั่งบนสายนั้น ไม่เปิดปิดใหม่ทีละ register
        //
        // ห้าค่าต่องานหมายถึงเขียน 5 ครั้งและอ่านกลับ 5 ครั้ง ถ้าเปิดปิดสายทุกครั้งคือ
        // 10 รอบ ตอน PLC ต่อไม่ติดจะรอ timeout 3 วินาทีครบทั้ง 10 รอบ = 30 วินาที
        // ต่อการกดส่งหนึ่งครั้ง เปิดครั้งเดียวจึงรอแค่ครั้งเดียว
        var (session, connectError) = await ModbusTcpService.OpenAsync(ip, port); // เปิด TCP ครั้งเดียวใช้เขียนและอ่านทุกค่า
        if (session == null)
        {
            // ต่อไม่ติดคือทุกค่าไม่ได้ส่ง ไม่ใช่ปัญหาของ register ตัวใดตัวเดียว
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

                // อ่านกลับทันทีเหมือนที่หน้า PLC Setting ทำ — เขียนผ่านแต่ค่าไม่เข้า
                // จะได้เห็นตั้งแต่ตรงนี้ ไม่ใช่ไปรู้เอาตอนเครื่องเดินผิด
                var (readOk, values, _) = await session.ReadHoldingRegistersAsync(address, 1); // อ่านกลับทันทีเพื่อตรวจว่าค่าเข้า PLC จริง

                results.Add(new BlockResult(
                    name, field.Value, readOk && values.Length > 0 ? values[0] : null, null));
            }
        }

        return results;
    }

    /// <summary>คีย์ใน Setting.config ของตำแหน่งเริ่มต้นหัวพิมพ์ — ตั้งที่หน้า PLC Setting</summary>
    public const string HomePositionKey = "HEAD_HOME_POSITION";

    /// <summary>
    /// ค่าที่ใช้เมื่อยังไม่ได้ตั้ง
    ///
    /// <para>
    /// ไม่ใช่ 0 เพราะ ladder ของ PLC หน้างานไม่ขยับหัวพิมพ์เมื่อ PostAct เป็น 0
    /// (ลองแล้ว 0 ไม่ไป 1 ไป) และแก้ ladder ไม่ได้ จึงใช้ค่าต่ำสุดที่ PLC ยอมขยับแทน
    /// เดิมฝังไว้เป็น 0 ในโค้ด ผลคือปุ่มกลับตำแหน่งเริ่มต้นไม่เคยทำงานจริงเลย
    /// </para>
    /// </summary>
    public const int DefaultHomePosition = 1;

    /// <summary>ค่าสูงสุดที่ใส่ได้ — register ของ Modbus เก็บเลขมีเครื่องหมาย 16 บิต</summary>
    public const int MaxHomePosition = short.MaxValue;

    /// <summary>
    /// ตำแหน่งเริ่มต้นของหัวพิมพ์ — งานจบแล้วให้เลื่อนกลับมาที่นี่
    ///
    /// <para>
    /// ค่าที่ตั้งไว้เสียหรืออยู่นอกช่วงให้ใช้ค่าเริ่มต้น ไม่ใช่ 0 — ถ้าถอยไปเป็น 0
    /// หัวพิมพ์จะไม่ขยับโดยไม่มีใครรู้ตัว
    /// </para>
    /// </summary>
    public static int HomePosition =>
        int.TryParse(CustomSettingsManager.Read(HomePositionKey, ""), out var value)
        && value is >= 1 and <= MaxHomePosition
            ? value
            : DefaultHomePosition;

    /// <summary>
    /// เลื่อนหัวพิมพ์ทั้งสองตัวกลับตำแหน่งเริ่มต้น — ใช้ตอนเครื่องว่างและไม่มีงานรอคิว
    ///
    /// <para>
    /// เขียน <see cref="HomePosition"/> ลงช่องตำแหน่งของแต่ละหัว ซึ่งเป็น address
    /// เดิมที่ใช้ส่งค่าของงานอยู่แล้ว ไม่ใช่คำสั่งใหม่ของ PLC
    /// </para>
    /// <para>
    /// หา address จากตาราง register map ด้วยชื่อแถวที่ลงท้ายว่า Position เหมือนกับ
    /// ค่าอื่น ๆ ไม่ได้ฝัง address ไว้ในโค้ด แถวไหนไม่มีในตารางก็ข้ามไป — รายการที่
    /// คืนกลับมาว่างแปลว่ายังไม่ได้ตั้ง address ของช่องตำแหน่งไว้เลย
    /// </para>
    /// </summary>
    public static async Task<List<BlockResult>> ResetPositionAsync(ApiClient? api)
    {
        var map = api == null
            ? new List<PlcRegisterMap>()
            : await api.GetAllPlcSettingsAsync();

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        // เขียนทับช่องเดียวกับที่ส่งตำแหน่งของงานเข้าไป ไม่ใช่ช่องใหม่
        //
        // ช่องที่คนหน้างานกรอกและโปรแกรมส่งจริงคือ Servo Post Act. ส่วนช่องที่ชื่อ
        // Position ตรง ๆ ไม่เคยถูกใช้เลย — ในฐานข้อมูลเป็นค่าว่างทุกแถว และหน้าจอ
        // ของโปรแกรมเดิมก็ปิดช่องนั้นทิ้งไว้ เหลือให้กรอกแต่ Post Act.
        int home = HomePosition; // ใช้ตำแหน่งเริ่มต้นจาก Setting ค่าเริ่มต้นคือ 1
        var fields = new List<PlcField>();
        Add(fields, map, $"{mk1} PostAct", $"{mk1} ตำแหน่งเริ่มต้น", home); // คืนหัวแรกผ่านช่อง PostAct เดียวกับที่ใช้ส่งงาน
        Add(fields, map, $"{mk2} PostAct", $"{mk2} ตำแหน่งเริ่มต้น", home); // คืนหัวที่สองตามค่าเริ่มต้นเดียวกัน

        // ไม่มีแถวไหนตั้ง address ไว้ = ตารางยังไม่ครบ ไม่ต้องยิงอะไรออกไป
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

    /// <summary>งานนี้ใช้หัวพ่นตัวนี้จริงไหม — กฎเดียวกับฝั่งที่ส่งเข้าเครื่อง MK</summary>
    private static bool HasProgram(InkjetConfigDto? config) =>
        config != null
        && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

    /// <summary>
    /// เหตุผลที่ยังส่งเข้า PLC ไม่ได้ — null เมื่อส่งได้
    ///
    /// <para>
    /// หัวที่งานนี้ใช้จริงต้องมีทั้ง Servo Post Act. และ Delay ช่องว่างจะถูกแปลงเป็น 0
    /// ก่อนส่ง ซึ่งที่ช่อง PostAct เลข 0 ไม่ใช่ค่ากลาง ๆ — ladder ของ PLC หน้างาน
    /// ไม่ขยับหัวพิมพ์เมื่อได้ 0 หัวจะค้างอยู่ที่ตำแหน่งของงานก่อนหน้าแล้วพิมพ์ผิดที่
    /// </para>
    /// <para>
    /// หัวที่งานไม่ได้ใช้ไม่ต้องมีค่าพวกนี้ เพราะไม่ถูกส่งอยู่แล้ว
    /// </para>
    /// </summary>
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

        // สายพานส่งทุกงานไม่ว่าจะใช้หัวไหน ช่องว่างจึงกลายเป็น 0 ได้เสมอ
        // และ 0 Hz คือสั่งสายพานหยุด ไม่ใช่ค่ากลาง ๆ ถ้าตั้งใจให้หยุดจริงก็พิมพ์ 0 เอง
        // ระบบจะได้แยกออกว่าเป็นความตั้งใจ ไม่ใช่ลืมกรอก
        if (pattern?.ConveyorSpeeds?.Speed1 == null)
            return "Conveyor 1 (Hz): ยังไม่ได้กรอก";

        return null;
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
