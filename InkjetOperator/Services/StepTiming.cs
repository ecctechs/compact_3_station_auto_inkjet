using System.Diagnostics;

namespace InkjetOperator.Services;

/// <summary>
/// จับเวลาทีละขั้นของงานหนึ่งก้อน แล้วเขียนลง log เป็นบรรทัดเดียว
/// <para>
/// หน้างานแจ้งว่า "กดเริ่มงานแล้วหน่วง" ซึ่งบอกไม่ได้ว่าหน่วงตรงไหน — โหลดข้อมูลงาน
/// แตะแชร์ของเครื่อง UV ไล่ต่อเครื่อง หรือรอเครื่องตอบตอนส่ง บรรทัดนี้แยกให้เห็น
/// ว่าแต่ละขั้นกินเวลาเท่าไหร่ โดยไม่ต้องไปนั่งจับเวลาที่หน้าเครื่อง
/// </para>
/// </summary>
public sealed class StepTiming
{
    private readonly string _where;
    private readonly string _what;
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    private readonly List<string> _steps = new();
    private long _last;

    public StepTiming(string where, string what)
    {
        _where = where;
        _what = what;
    }

    /// <summary>ปิดขั้นที่เพิ่งจบ — เวลาที่จดคือนับจากขั้นก่อนหน้าถึงตอนนี้</summary>
    public void Mark(string step)
    {
        long now = _watch.ElapsedMilliseconds;
        _steps.Add($"{step} {now - _last} ms");
        _last = now;
    }

    /// <summary>เขียนลง log — เรียกครั้งเดียวตอนจบ ไม่ว่าจะจบแบบไหน</summary>
    public void Write(string? outcome = null)
    {
        var text = $"{_what} · รวม {_watch.ElapsedMilliseconds} ms";
        if (_steps.Count > 0) text += " · " + string.Join(" · ", _steps);
        if (!string.IsNullOrEmpty(outcome)) text += " · " + outcome;
        AppLog.Info(_where, text);
    }
}
