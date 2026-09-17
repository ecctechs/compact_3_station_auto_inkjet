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

    /// <summary>มีใครกำลังส่งงานอยู่ไหม — true คือให้ตัวเช็คสถานะข้ามรอบนี้ไป</summary>
    public static bool Active => Volatile.Read(ref _holders) > 0;

    /// <summary>จองไว้ตลอดช่วงที่คุยกับเครื่อง — ใช้กับ <c>using</c> เสมอ</summary>
    public static IDisposable Hold() => new Holder();

    private sealed class Holder : IDisposable
    {
        private int _released;

        public Holder() => Interlocked.Increment(ref _holders);

        public void Dispose()
        {
            // กัน Dispose ซ้ำ ไม่งั้นตัวนับจะติดลบแล้วธงจะค้างว่า "ว่าง" ตลอดกาล
            if (Interlocked.Exchange(ref _released, 1) == 0)
                Interlocked.Decrement(ref _holders);
        }
    }
}
