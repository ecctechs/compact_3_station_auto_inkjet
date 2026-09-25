namespace InkjetOperator.Services;

/// <summary>
/// "ตอนนี้กำลังคุยกับเครื่องพิมพ์อยู่" — ธงกลางที่ตัวเช็คสถานะต้องดูก่อนยิง
///
/// <para>
/// MK Compact กับ UV รับการเชื่อมต่อได้จำกัด การเปิดซ็อกเก็ตไปถามว่า "ยังอยู่ไหม"
/// ตอนที่อีกฝั่งกำลังส่งงานจริงอยู่ จึงมีโอกาสไปแย่งคิวจนงานส่งไม่ผ่าน
/// การเช็คสถานะเป็นแค่ไฟบอกสถานะ ยอมข้ามรอบได้เสมอ การส่งงานยอมไม่ได้
/// </para>
/// <para>
/// นับเป็นตัวเลขไม่ใช่ bool เพราะการส่งงานหนึ่งครั้งซ้อนกันได้หลายชั้น
/// (ส่ง MK สองเครื่องในรอบเดียว) ปล่อยชั้นในแล้วต้องไม่ถือว่าว่างทั้งหมด
/// </para>
/// </summary>
public static class MachineBusy
{
    private static int _holders;

    /// <summary>
    /// นับแยกรายเครื่องด้วย — ใช้ตอบว่า "เครื่องนี้กำลังถูกคุยอยู่ไหม" ไม่ใช่ "มีใคร
    /// คุยกับเครื่องไหนอยู่บ้างไหม"
    ///
    /// <para>
    /// ปุ่มกดหน้างานของแต่ละสถานีปล่อยคนละเครื่องกัน การส่งงานเข้า MK จึงไม่มีเหตุ
    /// ให้ขวางการปล่อย UV2 คนละสายคนละพอร์ต ทำพร้อมกันได้
    /// </para>
    /// </summary>
    private static readonly Dictionary<string, int> _byMachine = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>มีใครกำลังส่งงานอยู่ไหม — true คือให้ตัวเช็คสถานะข้ามรอบนี้ไป</summary>
    public static bool Active => Volatile.Read(ref _holders) > 0;

    /// <summary>เครื่องนี้กำลังถูกคุยอยู่ไหม — ชื่อว่างถือว่าไม่ใช่เครื่องไหนเลย</summary>
    public static bool IsBusy(string? machine)
    {
        if (string.IsNullOrWhiteSpace(machine)) return false;

        lock (_byMachine)
            return _byMachine.TryGetValue(machine, out int n) && n > 0;
    }

    /// <summary>
    /// จองไว้ตลอดช่วงที่คุยกับเครื่อง — ใช้กับ <c>using</c> เสมอ
    /// </summary>
    /// <param name="machine">
    /// ชื่อเครื่องที่กำลังคุยด้วย (MK / UV1 / UV2) — ไม่ส่งมาก็ยังนับรวมในธงกลาง
    /// แต่จะตอบ <see cref="IsBusy"/> ของเครื่องไหนไม่ได้
    /// </param>
    public static IDisposable Hold(string? machine = null) => new Holder(machine);

    private sealed class Holder : IDisposable
    {
        private readonly string? _machine;
        private int _released;

        public Holder(string? machine)
        {
            _machine = string.IsNullOrWhiteSpace(machine) ? null : machine;
            Interlocked.Increment(ref _holders);

            if (_machine == null) return;
            lock (_byMachine)
                _byMachine[_machine] = _byMachine.TryGetValue(_machine, out int n) ? n + 1 : 1;
        }

        public void Dispose()
        {
            // กัน Dispose ซ้ำ ไม่งั้นตัวนับจะติดลบแล้วธงจะค้างว่า "ว่าง" ตลอดกาล
            if (Interlocked.Exchange(ref _released, 1) != 0) return;

            Interlocked.Decrement(ref _holders);

            if (_machine == null) return;
            lock (_byMachine)
                if (_byMachine.TryGetValue(_machine, out int n))
                    _byMachine[_machine] = n - 1;
        }
    }
}
