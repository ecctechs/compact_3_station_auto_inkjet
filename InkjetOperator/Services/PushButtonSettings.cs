namespace InkjetOperator.Services;

/// <summary>
/// ค่าตั้งของปุ่มกดหน้างาน — สัญญาณที่บอกว่าให้ส่งงานไปสถานีถัดไปได้แล้ว
///
/// <para>
/// ตั้งค่าที่หน้า PLC UV Setting หัวข้อ "ปุ่มกดหน้างาน" ใช้ PLC ตัวเดียวกับแคลมป์
/// จึงไม่มี IP กับ port ของตัวเอง ยืมของแคลมป์มาใช้ ถ้าแยกกันเมื่อไหร่ค่อยเพิ่ม
/// </para>
/// <para>
/// PLC เป็นฝ่ายตั้งบิตเป็น 1 ค้างไว้ 1-2 วินาทีแล้วปล่อยกลับเป็น 0 เอง
/// ฝั่งโปรแกรมอ่านอย่างเดียว ไม่เขียนกลับ เพื่อไม่ให้แย่งกันคุมบิตเดียวกัน
/// </para>
/// </summary>
public sealed class PushButtonSettings
{
    // หน้า Setting บันทึกบน UI thread; แจ้งเมื่อเขียนครบแล้วเท่านั้น
    public static event EventHandler? Saved;
    /// <summary>ช้ากว่านี้เสี่ยงพลาดสัญญาณที่ค้างแค่ 1 วินาที</summary>
    public const int MaxPollMs = 900;

    private const int MinPollMs = 100;
    private const int DefaultPollMs = 300;

    public bool Enabled { get; set; }

    /// <summary>ที่อยู่บิตของปุ่มกดที่ ST1 เช่น M800</summary>
    public string AddressSt1 { get; set; } = "";

    /// <summary>ที่อยู่บิตของปุ่มกดที่ ST2</summary>
    public string AddressSt2 { get; set; } = "";

    /// <summary>ที่อยู่บิตของปุ่มกดที่ ST3</summary>
    public string AddressSt3 { get; set; } = "";

    /// <summary>ที่อยู่ของสถานีที่ระบุ — 1, 2 หรือ 3</summary>
    public string AddressFor(int station) => station switch
    {
        1 => AddressSt1.Trim(),
        2 => AddressSt2.Trim(),
        _ => AddressSt3.Trim(),
    };

    /// <summary>เครื่องที่ปุ่มของสถานีนั้นปล่อย — ST1 ปล่อย MK · ST2 ปล่อย UV1 · ST3 ปล่อย UV2</summary>
    public static string MachineFor(int station) => station switch
    {
        1 => "MK",
        2 => "UV1",
        _ => "UV2",
    };

    /// <summary>
    /// ปุ่มที่<b>เครื่องนี้</b>ต้องเฝ้า พร้อมเครื่องที่จะถูกปล่อยเมื่อมีคนกด
    ///
    /// <para>
    /// PC ของ ST1 เฝ้าทั้งสามปุ่ม ส่วนเครื่องอื่นไม่เฝ้าเลย — ปุ่มหนึ่งปุ่มต้องมี
    /// คนเฝ้าคนเดียวเท่านั้น ถ้าสองเครื่องอ่านบิตเดียวกัน การกดครั้งเดียวจะกลายเป็น
    /// สั่งปล่อยเครื่องสองรอบ แล้วคิวจะเดินข้ามงานไปหนึ่งใบโดยไม่มีอะไรฟ้อง
    /// </para>
    /// <para>
    /// ที่เลือก ST1 เพราะสาย MK กับ UV ต่ออยู่กับ PC ของ ST1 ที่เดียว การปล่อยเครื่อง
    /// แล้วส่งงานใบถัดไปต่อทันทีจึงจบได้ในเครื่องเดียว และ ST2 ไม่มี PC ของตัวเอง
    /// ถ้าไม่ให้ ST1 เฝ้าให้ ปุ่มของ ST2 จะไม่มีใครอ่านเลย คิวของ UV1 จะค้างตลอดกาล
    /// </para>
    /// <para>
    /// ผลข้างเคียงที่ต้องรู้: ปิดโปรแกรมที่ ST1 เมื่อไหร่ ปุ่มหน้างานหยุดทำงานทั้งสามปุ่ม
    /// </para>
    /// </summary>
    public IEnumerable<(int Station, string Address, string Machine)> Watched()
    {
        // ST3 มีจอของตัวเองแต่ไม่ต้องเฝ้าปุ่มไหน — ST1 เฝ้าให้ครบแล้ว
        if (StationService.IsSt3) yield break;

        foreach (var (station, address) in Addresses())
        {
            if (address.Length == 0) continue;
            yield return (station, address, MachineFor(station));
        }
    }

    /// <summary>ทุกกี่มิลลิวินาทีจะอ่านบิตหนึ่งครั้ง</summary>
    public int PollMs { get; set; } = DefaultPollMs;

    /// <summary>ยืมจากค่าตั้งของแคลมป์ — PLC ตัวเดียวกัน</summary>
    public string Ip => CustomSettingsManager.Read("CLAMP_PLC_IP", "").Trim();

    public int Port =>
        int.TryParse(CustomSettingsManager.Read("CLAMP_PLC_PORT", "5012"), out int p) ? p : 5012;

    /// <summary>พร้อมใช้จริงไหม — เปิดไว้ มีปุ่มให้เฝ้า และรู้ว่าจะไปคุยกับ PLC ตัวไหน</summary>
    public bool IsReady => Enabled && Ip.Length > 0 && Watched().Any();

    public static PushButtonSettings Load() => new()
    {
        Enabled = CustomSettingsManager.Read("PUSHBTN_ENABLED", "0").Trim() == "1",
        AddressSt1 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST1", "").Trim(),
        AddressSt2 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST2", "").Trim(),

        // ค่าที่ตั้งไว้ก่อนแยกเป็นสามช่องคือปุ่มของ ST3 เพราะตอนนั้นมีสถานีเดียว
        // ที่อ่านปุ่มกด รับช่วงมาให้เอง คนที่ตั้งค่าไว้แล้วจะได้ไม่ต้องกรอกใหม่
        AddressSt3 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST3", "").Trim() is { Length: > 0 } st3
            ? st3
            : CustomSettingsManager.Read("PUSHBTN_ADDRESS", "").Trim(),

        PollMs = Clamp(CustomSettingsManager.Read("PUSHBTN_POLL_MS", "")),
    };

    public void Save()
    {
        CustomSettingsManager.Write("PUSHBTN_ENABLED", Enabled ? "1" : "0");
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST1", AddressSt1.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST2", AddressSt2.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST3", AddressSt3.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_POLL_MS", Clamp(PollMs.ToString()).ToString());
        Saved?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ตรวจที่อยู่ที่กรอกมา — คืนข้อความปัญหา หรือ null เมื่อใช้ได้
    /// <para>
    /// ปิดใช้งานอยู่ก็ปล่อยผ่าน จะได้บันทึกค่าอื่นในหน้าเดียวกันได้โดยไม่ติดขัด
    /// </para>
    /// </summary>
    public string? Validate()
    {
        if (!Enabled) return null;

        // ตรวจทุกช่องที่กรอกมา ไม่ใช่เฉพาะของสถานีตัวเอง — คนตั้งค่าอาจกรอกครบ
        // สามช่องที่เครื่องเดียวแล้วก๊อปไฟล์ตั้งค่าไปใช้ต่อ กรอกผิดต้องรู้ตั้งแต่ตรงนี้
        foreach (var (station, address) in Addresses())
        {
            if (address.Length == 0) continue;
            if (CheckOne(station, address) is string problem) return problem;
        }

        // เครื่องที่ไม่ได้เฝ้าปุ่มไหน (ST3) บันทึกได้โดยไม่ต้องกรอกอะไร — ช่องทั้งสาม
        // มีไว้ให้ตั้งทีเดียวแล้วก๊อป Setting.config ไปใช้ทุกเครื่อง
        if (StationService.IsSt3) return null;

        if (!Watched().Any())
            return "เปิดใช้งานปุ่มกดหน้างานแล้ว แต่ยังไม่ได้กรอก address ของปุ่มไหนเลย";

        // ขาด IP แล้วตัวเฝ้าจะไม่เริ่มอ่านเลย (IsReady เป็น false) โดยไม่มีอะไรฟ้อง
        //
        // เคยเจอจริง: ติ๊กเปิดใช้งาน กรอก address ครบ กด Save ผ่านฉลุย แล้วงงว่า
        // ทำไมกดปุ่มหน้างานไม่มีอะไรเกิดขึ้น เพราะ IP ของ PLC แคลมป์ยังว่างอยู่
        if (Ip.Length == 0)
            return "เปิดใช้งานปุ่มกดหน้างานแล้ว แต่ยังไม่ได้ตั้ง IP ของ PLC แคลมป์ "
                 + "— กรอกที่หัวข้อ \"การเชื่อมต่อ\" ด้านบนของหน้านี้ก่อน";

        return null;
    }

    /// <summary>ที่อยู่ทั้งสามช่องพร้อมเลขสถานี</summary>
    public IEnumerable<(int Station, string Address)> Addresses()
    {
        yield return (1, AddressSt1.Trim());
        yield return (2, AddressSt2.Trim());
        yield return (3, AddressSt3.Trim());
    }

    private static string? CheckOne(int station, string address)
    {
        // ต้องเป็นอุปกรณ์ชนิดบิต — D กับ W เป็น word อ่านเป็นบิตไม่ได้
        if (!address.StartsWith("M", StringComparison.OrdinalIgnoreCase))
            return $"address ปุ่มกดของ ST{station} ต้องเป็น M เท่านั้น เช่น M800 (กรอกมาว่า \"{address}\")";

        return McProtocolService.TryParseAddress(address, out _, out _, out string error)
            ? null
            : $"ST{station}: {error}";
    }

    private static int Clamp(string text)
    {
        if (!int.TryParse(text.Trim(), out int ms)) return DefaultPollMs;
        return Math.Clamp(ms, MinPollMs, MaxPollMs);
    }
}
