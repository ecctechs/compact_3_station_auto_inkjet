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
    /// <summary>ช้ากว่านี้เสี่ยงพลาดสัญญาณที่ค้างแค่ 1 วินาที</summary>
    public const int MaxPollMs = 900;

    private const int MinPollMs = 100;
    private const int DefaultPollMs = 300;

    public bool Enabled { get; set; }

    /// <summary>ที่อยู่บิตที่ PLC ใช้บอกว่ามีการกดปุ่ม เช่น M800</summary>
    public string Address { get; set; } = "";

    /// <summary>ทุกกี่มิลลิวินาทีจะอ่านบิตหนึ่งครั้ง</summary>
    public int PollMs { get; set; } = DefaultPollMs;

    /// <summary>ยืมจากค่าตั้งของแคลมป์ — PLC ตัวเดียวกัน</summary>
    public string Ip => CustomSettingsManager.Read("CLAMP_PLC_IP", "").Trim();

    public int Port =>
        int.TryParse(CustomSettingsManager.Read("CLAMP_PLC_PORT", "5012"), out int p) ? p : 5012;

    /// <summary>พร้อมใช้จริงไหม — เปิดไว้ กรอกที่อยู่แล้ว และรู้ว่าจะไปคุยกับ PLC ตัวไหน</summary>
    public bool IsReady => Enabled && Address.Trim().Length > 0 && Ip.Length > 0;

    public static PushButtonSettings Load() => new()
    {
        Enabled = CustomSettingsManager.Read("PUSHBTN_ENABLED", "0").Trim() == "1",
        Address = CustomSettingsManager.Read("PUSHBTN_ADDRESS", "").Trim(),
        PollMs = Clamp(CustomSettingsManager.Read("PUSHBTN_POLL_MS", "")),
    };

    public void Save()
    {
        CustomSettingsManager.Write("PUSHBTN_ENABLED", Enabled ? "1" : "0");
        CustomSettingsManager.Write("PUSHBTN_ADDRESS", Address.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_POLL_MS", Clamp(PollMs.ToString()).ToString());
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

        var address = Address.Trim();
        if (address.Length == 0)
            return "เปิดใช้งานปุ่มกดหน้างานแล้ว แต่ยังไม่ได้กรอก address";

        // ต้องเป็นอุปกรณ์ชนิดบิต — D กับ W เป็น word อ่านเป็นบิตไม่ได้
        if (!address.StartsWith("M", StringComparison.OrdinalIgnoreCase))
            return $"address ของปุ่มกดต้องเป็น M เท่านั้น เช่น M800 (กรอกมาว่า \"{address}\")";

        if (!McProtocolService.TryParseAddress(address, out _, out _, out string error))
            return error;

        return null;
    }

    private static int Clamp(string text)
    {
        if (!int.TryParse(text.Trim(), out int ms)) return DefaultPollMs;
        return Math.Clamp(ms, MinPollMs, MaxPollMs);
    }
}
