namespace InkjetOperator.Services;

/// <summary>
/// ค่า IP และ address ตั้งต้นของเครื่อง ST1 (MENU_LEVEL 1)
///
/// <para>
/// ค่าพวกนี้เหมือนกันทุกไลน์ จึงตั้งให้เองตอนเปิดโปรแกรม <b>ครั้งเดียว</b> แล้วจด
/// <see cref="AppliedKey"/> ไว้ว่าลงชุดไหนไปแล้ว ครั้งต่อไปไม่แตะอีก ค่าที่หน้างาน
/// แก้ทีหลังที่หน้า Setting จึงไม่ถูกทับ
/// </para>
/// <para>
/// ลงทับค่าเดิมด้วย ไม่ใช่เติมเฉพาะช่องว่าง — เครื่องที่ใช้งานอยู่แล้วมี IP ชุดเก่า
/// ค้างอยู่ (ตอนทดสอบ) ถ้าเติมเฉพาะช่องว่างค่าชุดใหม่จะไม่ลงเลย
/// </para>
/// <para>
/// แก้ค่าในชุดนี้เมื่อไหร่ ให้บวก <see cref="Revision"/> ด้วย ไม่งั้นเครื่องที่ลง
/// ชุดเก่าไปแล้วจะไม่ได้ค่าใหม่
/// </para>
/// <para>
/// ทำเฉพาะ MENU_LEVEL 1 ตรง ๆ — เครื่องที่เพิ่งติดตั้งได้ MENU_LEVEL 99 มาจาก
/// Setting.default.config พอเปลี่ยนเป็น 1 แล้วเปิดโปรแกรมใหม่ ค่าชุดนี้จึงลงให้
/// </para>
/// </summary>
public static class St1DefaultSettings
{
    /// <summary>คีย์ที่จดว่าลงค่าชุดไหนไปแล้ว</summary>
    internal const string AppliedKey = "ST1_DEFAULTS_APPLIED";

    /// <summary>เลขของค่าชุดปัจจุบัน — บวกเมื่อแก้ <see cref="Values"/></summary>
    internal const int Revision = 1;

    /// <summary>คีย์ใน Setting.config กับค่าตั้งต้น เรียงตามหน้า Setting</summary>
    internal static readonly (string Key, string Value)[] Values =
    [
        // Printer Setting
        ("MK058_COM", "10.10.100.103"),
        ("MK059_COM", "10.10.100.104"),
        ("UV001_IP", "10.10.100.4"),
        ("UV001_PORT", "10086"),
        ("UV002_IP", "10.10.100.3"),
        ("UV002_PORT", "10086"),

        // PLC MK Setting
        ("PLC_IP", "192.168.1.1"),
        ("PLC_PORT", "502"),

        // PLC UV Setting — ปุ่มกดหน้างานต้องเป็นบิต M (ดู PushButtonSettings)
        // port กับการเปิดใช้งานปุ่มกดไม่ได้กำหนดมา ใช้ค่าที่เครื่องมีอยู่
        ("CLAMP_PLC_IP", "10.10.100.100"),
        ("PUSHBTN_ADDRESS_ST1", "M4000"),
        ("PUSHBTN_ADDRESS_ST2", "M4001"),
        ("PUSHBTN_ADDRESS_ST3", "M4003"),
    ];

    /// <summary>เรียกครั้งเดียวตอนเปิดโปรแกรม</summary>
    public static void ApplyIfSt1()
    {
        if (!StationService.IsSt1Menu) return;

        ApplyOnce(k => CustomSettingsManager.Read(k), CustomSettingsManager.Write);
    }

    /// <summary>
    /// ลงค่าชุดนี้ทับถ้ายังไม่เคยลงชุดนี้ — คืน true เมื่อลงในรอบนี้
    /// <para>รับตัวอ่านกับตัวเขียนเข้ามา เพื่อให้ทดสอบได้โดยไม่แตะไฟล์ตั้งค่าจริง</para>
    /// </summary>
    internal static bool ApplyOnce(Func<string, string> read, Func<string, string, bool> write)
    {
        if (int.TryParse(read(AppliedKey).Trim(), out int applied) && applied >= Revision) return false;

        bool ok = true;
        foreach (var (key, value) in Values)
            ok &= write(key, value);

        // จดเฉพาะตอนเขียนครบ ถ้าบางช่องไม่ผ่าน (เช่นไฟล์เป็นของบัญชีอื่น) เปิดครั้งหน้าลองใหม่
        if (ok) write(AppliedKey, Revision.ToString());
        return ok;
    }
}
