using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// งานอยู่ที่สถานีไหน — คิดจากคำสั่งที่กดส่งสำเร็จล่าสุด ไม่ได้เก็บเป็นคอลัมน์แยก
/// <para>
/// ทุกครั้งที่กดส่ง backend เขียนแถวลง <c>print_job_commands</c> อยู่แล้ว
/// (<c>command</c> = MK / UV1 / UV2 พร้อม <c>success</c> และ <c>sent_at</c>)
/// ข้อมูลที่ต้องใช้จึงมีครบอยู่แล้ว การอนุมานจากตรงนั้นทำให้ไม่มีทางขัดกับ
/// ประวัติการส่งจริง และใช้ได้กับงานเก่าที่มีอยู่ในดาต้าเบสทันทีโดยไม่ต้อง migrate
/// </para>
/// <para>
/// "อยู่ที่สถานี N" หมายถึงกดส่งเข้าเครื่องนั้นสำเร็จล่าสุด ไม่ใช่กำลังผลิตอยู่จริง
/// งานที่จบแล้วจึงค้างอยู่ที่สถานีสุดท้ายของมันตาม marking method
/// </para>
/// </summary>
public static class JobStationService
{
    /// <summary>ผังสายการผลิต: MK อยู่สถานี 1 · UV1 สถานี 2 · UV2 สถานี 3</summary>
    public static int? StationOf(string? command)
    {
        if (string.Equals(command, "MK", StringComparison.OrdinalIgnoreCase)) return 1;
        if (string.Equals(command, "UV1", StringComparison.OrdinalIgnoreCase)) return 2;
        if (string.Equals(command, "UV2", StringComparison.OrdinalIgnoreCase)) return 3;

        // คำสั่งอื่นเช่น MK_ROUND1_DONE เป็นหมุดบอกสถานะ ไม่ใช่การส่งเข้าเครื่อง
        return null;
    }

    /// <summary>สถานีล่าสุดของงาน — null เมื่อยังไม่เคยกดส่งสำเร็จเลย</summary>
    public static int? Current(IEnumerable<CommandResult>? commands)
    {
        if (commands == null) return null;

        int? station = null;
        var latest = DateTime.MinValue;
        int index = 0, latestIndex = -1;

        foreach (var command in commands)
        {
            int position = index++;
            if (!command.Success) continue;
            if (StationOf(command.Command) is not int candidate) continue;

            // sent_at เป็นสตริง ISO จาก backend แถวที่อ่านเวลาไม่ออกจะได้ MinValue
            // แล้วตกไปเรียงตามลำดับที่ API ส่งมาแทน ซึ่งเป็นลำดับที่บันทึกอยู่แล้ว
            var when = ParseSentAt(command.SentAt);
            if (when < latest) continue;
            if (when == latest && position < latestIndex) continue;

            latest = when;
            latestIndex = position;
            station = candidate;
        }

        return station;
    }

    /// <summary>ชื่อที่แสดงบนจอ เช่น ST2 · ยังไม่เคยส่งคืนค่าว่าง ให้ผู้เรียกตัดสินใจเอง</summary>
    public static string Label(int? station) => station == null ? "" : $"ST{station}";

    private static DateTime ParseSentAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DateTime.MinValue;

        return DateTime.TryParse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : DateTime.MinValue;
    }
}
