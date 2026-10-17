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

    /// <summary>
    /// สถานีที่งานนี้ถือเครื่องอยู่ตอนนี้ — คิวของงานนี้ที่ยัง active (ได้เครื่องแล้ว ยังไม่กดปล่อย)
    /// เรียงตามลำดับสาย ไม่ซ้ำ (งาน 22 ถือ MK สองรอบก็ยังเป็น ST1 ที่เดียว)
    /// </summary>
    public static List<int> Holding(IEnumerable<MachineQueueRow>? queue, int jobId) =>
        (queue ?? [])
            .Where(r => r.PrintJobsId == jobId && r.State == "active")
            .Select(r => StationOf(r.Machine))
            .OfType<int>()
            .Distinct()
            .OrderBy(s => s)
            .ToList();

    /// <summary>
    /// ข้อความในคอลัมน์ Station ของหน้า Order List
    /// <para>
    /// งานที่ส่งเข้าหลายเครื่องพร้อมกัน (11 = UV1 + UV2, 12 = MK + UV2) อยู่หลายสถานีจริง
    /// ในเวลาเดียวกัน เดิมขึ้นแค่สถานีของเครื่องที่ส่งเสร็จ "ล่าสุด" ซึ่งต่างกันแค่เสี้ยววินาที
    /// บางครั้งได้ ST2 บางครั้งได้ ST3 คนดูไม่รู้ว่าอีกเครื่องหายไปไหน
    /// </para>
    /// <para>
    /// ตอนนี้ขึ้นทุกสถานีที่งานถือเครื่องอยู่ คั่นด้วย " + " (เช่น "ST2 + ST3")
    /// ปล่อยเครื่องไหนแล้วสถานีนั้นหายไป ไม่ถือเครื่องไหนแล้ว (ปล่อยครบ / จบงาน / งานเก่า
    /// ที่ไม่มีคิว) ค่อยกลับไปใช้สถานีที่ส่งสำเร็จล่าสุดแบบเดิม
    /// </para>
    /// </summary>
    public static string Display(IEnumerable<MachineQueueRow>? queue, int jobId, IEnumerable<CommandResult>? commands)
    {
        var holding = Holding(queue, jobId);
        return holding.Count > 0
            ? string.Join(" + ", holding.Select(s => $"ST{s}"))
            : Label(LastOnLine(commands));
    }

    /// <summary>ส่งห่างกันไม่เกินนี้นับว่ากดเริ่มรอบเดียวกัน (ส่งหลายเครื่องพร้อมกัน)</summary>
    private static readonly TimeSpan SameBatch = TimeSpan.FromMinutes(1);

    /// <summary>
    /// สถานีของงานที่ไม่ได้ถือเครื่องแล้ว — สถานีท้ายสุดตามลำดับสายของการส่งรอบล่าสุด
    /// <para>
    /// งานที่ส่งหลายเครื่องพร้อมกัน ใบไหนเสร็จทีหลังขึ้นกับจังหวะเครือข่าย ถ้าใช้ "ส่งล่าสุด"
    /// ตรง ๆ งาน 11 ที่ปล่อยครบแล้วจะขึ้น ST2 บ้าง ST3 บ้าง จึงเลือกสถานีท้ายสุดในชุดที่ส่ง
    /// พร้อมกันแทน (11 → ST3) งานที่ส่งทีละเครื่องได้ผลเท่าเดิม เพราะชุดล่าสุดมีเครื่องเดียว
    /// </para>
    /// <para>
    /// ไม่ใช้สถานีสูงสุดของทั้งประวัติ เพราะงานที่นำกลับมาพิมพ์ใหม่ยังมีประวัติส่งของรอบก่อน
    /// อยู่ — ส่งรอบใหม่ไปแค่ MK ต้องขึ้น ST1 ไม่ใช่ ST3 จากรอบเก่า
    /// </para>
    /// </summary>
    public static int? LastOnLine(IEnumerable<CommandResult>? commands)
    {
        var sent = (commands ?? [])
            .Where(c => c.Success && StationOf(c.Command) != null)
            .Select(c => (Station: StationOf(c.Command)!.Value, At: ParseSentAt(c.SentAt)))
            .ToList();
        if (sent.Count == 0) return null;

        // อ่านเวลาไม่ออกสักแถว ตัดสินชุดไม่ได้ — ใช้กติกาเดิม
        if (sent.Any(s => s.At == DateTime.MinValue)) return Current(commands);

        var latest = sent.Max(s => s.At);
        return sent.Where(s => latest - s.At <= SameBatch).Max(s => s.Station);
    }

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
