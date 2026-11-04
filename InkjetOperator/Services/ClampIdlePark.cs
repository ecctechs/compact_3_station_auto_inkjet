using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// ตัวเลือก "คิว UV ว่าง → แคลมป์กลับตำแหน่ง 0" — ตั้งที่ Setting → ตัวเลือกหน้างาน (โหมด Dev)
///
/// <para>
/// ทำงานเหมือน PLC ตัวเก่าที่ถอยแคลมป์ไปตำแหน่ง 1 เองเมื่อไม่มีงาน ตัวใหม่ใช้ตำแหน่ง 0 บน HMI
/// (ตกลง 2/11/2026) แยกตามเครื่อง: คิว UV1 ว่าง → แกน Plate · คิว UV2 ว่าง → แกน Shim
/// </para>
/// <para>
/// "ว่าง" = ไม่มีงานถือเครื่องและไม่มีงานรอ — มีงานรอต่อไม่ถอย เดี๋ยวงานถัดไปก็ใช้แคลมป์
/// สั่งครั้งเดียวตอนเปลี่ยนจากมีงานเป็นว่าง ไม่สั่งทุกรอบ poll และไม่สั่งตอนเปิดโปรแกรม
/// (เปิดโปรแกรมมาแล้วแกนวิ่งเองโดยไม่มีใครกดอะไรเป็นเรื่องน่าตกใจ)
/// </para>
/// <para>
/// <b>ค่าเริ่มต้นเปิด</b> (ตกลง 2/11/2026 — ทำงานเหมือนตัวเก่าตั้งแต่ลงโปรแกรม) ปิดได้ที่หน้าเดียวกัน
/// ไม่เคยตั้งคีย์ = เปิด · ตั้ง "0" = ปิด
/// </para>
/// </summary>
public sealed class ClampIdlePark
{
    /// <summary>คีย์ใน Setting.config — "0" = ปิด · ไม่มีคีย์ / ค่าอื่น = เปิด</summary>
    public const string Key = "CLAMP_PARK_WHEN_IDLE";

    public static bool Enabled => CustomSettingsManager.Read(Key, "1").Trim() != "0";

    /// <summary>เครื่อง UV กับฝั่งแคลมป์ที่ใช้คู่กัน — ตรงกับ <see cref="ClampSide"/></summary>
    private static readonly (string Machine, ClampSide Side)[] Machines =
    [
        ("UV1", ClampSide.Plate),
        ("UV2", ClampSide.Shim),
    ];

    /// <summary>รอบก่อนเครื่องนี้มีงานไหม — ยังไม่มีคีย์ = ยังไม่เคยเห็นคิว (เพิ่งเปิดโปรแกรม)</summary>
    private readonly Dictionary<string, bool> _busy = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// ดูคิวชุดล่าสุด คืนฝั่งที่เพิ่งว่าง — เรียกทุกรอบแม้ปิดตัวเลือกไว้ จะได้รู้สถานะก่อนหน้าเสมอ
    /// (เปิดตัวเลือกกลางคันแล้วไม่สั่งผิดจังหวะ)
    /// </summary>
    public List<ClampSide> Observe(IEnumerable<MachineQueueRow> rows)
    {
        var list = rows as IList<MachineQueueRow> ?? rows.ToList();
        var idle = new List<ClampSide>();

        foreach (var (machine, side) in Machines)
        {
            bool busy = IsBusy(list, machine);

            if (_busy.TryGetValue(machine, out bool wasBusy) && wasBusy && !busy) idle.Add(side);
            _busy[machine] = busy;
        }

        return idle;
    }

    /// <summary>
    /// เครื่อง UV ของฝั่งนี้ยังมีงานไหม — ใช้ถามซ้ำจากคิวสด ๆ ก่อนถอยจริง
    /// (ระหว่างรอ งานใหม่อาจเข้าคิวแล้วส่งค่าแคลมป์ของมันไปแล้ว ถอยตามคิวรอบก่อนจะทับ)
    /// </summary>
    public static bool Busy(IEnumerable<MachineQueueRow> rows, ClampSide side) =>
        Machines.Where(m => m.Side == side).Any(m => IsBusy(rows, m.Machine));

    private static bool IsBusy(IEnumerable<MachineQueueRow> rows, string machine) =>
        rows.Any(r => string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase)
            && r.State is "active" or "pending");
}
