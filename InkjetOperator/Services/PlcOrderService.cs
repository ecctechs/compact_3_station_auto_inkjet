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
    public static async Task<List<PlcField>> BuildPlanAsync(ApiClient? api, PatternDetail? pattern)
    {
        var map = api == null
            ? new List<PlcRegisterMap>()
            : await api.GetAllPlcSettingsAsync();

        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        var speeds = pattern?.ConveyorSpeeds;

        var fields = new List<PlcField>();
        AddServo(fields, map, mk1, Servo(pattern, 1));
        AddServo(fields, map, mk2, Servo(pattern, 2));

        // สายพานตัวเดียว — ตาราง register map เหลือ Conveyor Speed 1 แถวเดียว
        // โปรแกรมเดิมส่งสามตัวรวดเดียว (D10-D12) แต่ของใหม่ตกลงกันว่าเหลือตัวแรก
        Add(fields, map, "Conveyor Speed 1", "Conveyor 1 (Hz)", Whole(speeds?.Speed1));

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

        var ready = plan.Where(f => f.Address != null).OrderBy(f => f.Address).ToList();
        if (ready.Count == 0)
        {
            results.Add(new BlockResult("PLC", 0, null, "ไม่มีค่าไหนที่ map address ไว้"));
            return results;
        }

        foreach (var field in ready)
        {
            int address = field.Address!.Value;
            var name = $"D{address}  {field.Label}";

            var (ok, error) = await ModbusTcpService.WriteSingleRegisterAsync(
                ip, port, address, field.Value);

            if (!ok)
            {
                results.Add(new BlockResult(name, field.Value, null, error));
                continue;
            }

            // อ่านกลับทันทีเหมือนที่หน้า PLC Setting ทำ — เขียนผ่านแต่ค่าไม่เข้า
            // จะได้เห็นตั้งแต่ตรงนี้ ไม่ใช่ไปรู้เอาตอนเครื่องเดินผิด
            var (readOk, values, _) = await ModbusTcpService.ReadHoldingRegistersAsync(
                ip, port, address, 1);

            results.Add(new BlockResult(
                name, field.Value, readOk && values.Length > 0 ? values[0] : null, null));
        }

        return results;
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

    private static ServoConfigDto? Servo(PatternDetail? pattern, int ordinal) =>
        pattern?.ServoConfigs.FirstOrDefault(s => s.Ordinal == ordinal);

    private static InkjetConfigDto? Inkjet(PatternDetail? pattern, int ordinal) =>
        pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);

    /// <summary>register ของ Modbus เก็บจำนวนเต็ม 16 บิต ค่าทศนิยมจึงต้องปัดก่อนส่ง</summary>
    private static int Whole(double? value) =>
        value == null ? 0 : (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);

    private static int Whole(int? value) => value ?? 0;
}
