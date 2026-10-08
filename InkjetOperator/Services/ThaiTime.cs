namespace InkjetOperator.Services;

/// <summary>
/// เวลาไทยที่ทุกหน้าจอใช้ร่วมกัน — backend เก็บเป็น UTC หน้าจอแสดงเป็นเวลาไทย
///
/// อยู่ที่เดียวเพราะทั้งตารางออเดอร์และหัวหน้า Order Detail ต้องอ่านวันเดียวกัน
/// ของงานเดียวกัน ถ้าแยกกันแปลงแล้ววันหนึ่งใครแก้ไม่ครบ สองหน้าจะบอกคนละวัน
/// </summary>
public static class ThaiTime
{
    /// <summary>
    /// วันที่พร้อมเวลา — สั้นพอให้อยู่ในคอลัมน์เดียว และใช้แกะกลับตอนเรียงตาราง
    /// ปีเป็น ค.ศ. 2 หลัก ตรงกับปฏิทินของตัวกรองวันที่ ไม่ใช่ปี พ.ศ.
    /// </summary>
    public const string Format = "dd/MM/yy HH:mm";

    /// <summary>เฉพาะวันที่ ไม่มีเวลา — ใช้ตอนที่ต้องการแค่บอกว่างานของวันไหน</summary>
    public const string DateFormat = "dd/MM/yy";

    /// <summary>
    /// เครื่องหน้างานอาจตั้ง time zone ไว้ไม่ตรง จึงยึดเวลาไทยตายตัว ไม่ใช้เวลาเครื่อง
    /// ชื่อโซนบน Windows กับ Linux คนละแบบ ถ้าหาไม่เจอทั้งคู่ค่อยใช้ UTC+7 ตรง ๆ
    /// </summary>
    private static readonly TimeZoneInfo Zone = ResolveZone();

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Bangkok" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "ICT", "ICT");
    }

    /// <summary>เวลาไทย → UTC สำหรับส่งเป็นเงื่อนไขให้ backend</summary>
    public static DateTime ToUtc(DateTime thai) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(thai, DateTimeKind.Unspecified), Zone);

    /// <summary>UTC ที่ backend ส่งมา → เวลาไทย</summary>
    public static DateTime? ToThai(DateTime? utc)
    {
        if (utc == null) return null;

        var value = utc.Value;
        if (value.Kind == DateTimeKind.Unspecified)
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), Zone);
    }

    /// <summary>
    /// ข้อความที่เอาไปแสดงได้เลย — ไม่มีค่าคืน <paramref name="empty"/>
    ///
    /// ใช้ InvariantCulture เสมอ เครื่องหน้างานตั้งเป็น th-TH ซึ่งจะให้ปี พ.ศ.
    /// ทำให้คอลัมน์ยาวขึ้นและแกะกลับตอนเรียงไม่ได้
    /// </summary>
    public static string Text(DateTime? utc, string format = Format, string empty = "-") =>
        ToThai(utc) is { } t
            ? t.ToString(format, System.Globalization.CultureInfo.InvariantCulture)
            : empty;
}
