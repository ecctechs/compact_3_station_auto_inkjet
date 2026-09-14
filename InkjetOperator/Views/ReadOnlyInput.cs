namespace InkjetOperator.Views;

/// <summary>
/// ช่องกรอกที่แตะไม่ได้เลยตอนตั้งเป็นอ่านอย่างเดียว
///
/// <para>
/// <c>ReadOnly</c> ของ AntdUI กันแค่การพิมพ์ ช่องยังรับโฟกัสได้อยู่ แตะแล้วมีเคอร์เซอร์
/// กระพริบเหมือนกรอกได้ ทั้งที่พิมพ์อะไรไม่ได้ พนักงานจึงนั่งกดอยู่นานโดยไม่รู้ว่าเป็น
/// ช่องแสดงผลเฉย ๆ
/// </para>
/// <para>
/// บนจอสัมผัสยังแย่กว่านั้น — ตอนช่องได้โฟกัส AntdUI จะเรียกคีย์บอร์ดบนจอขึ้นมาให้
/// (<c>Config.TouchKeyboard</c>) แตะช่องที่กรอกไม่ได้แล้วคีย์บอร์ดเด้งขึ้นมาบังจอ
/// </para>
/// <para>
/// ตัวนี้ปิดการรับโฟกัสไปเลยตอนเป็นอ่านอย่างเดียว จึงไม่มีเคอร์เซอร์ ไม่มีคีย์บอร์ดเด้ง
/// และ Tab ก็ข้ามไปช่องที่กรอกได้จริง ต่างจากการตั้ง <c>Enabled = false</c> ตรงที่
/// ตัวอักษรยังเข้มชัดอ่านง่ายเหมือนเดิม ไม่ได้จางลงเป็นสีเทา
/// </para>
/// <para>
/// ผูกกับ <c>ReadOnly</c> ไม่ได้ตั้งตายตัว ช่องไหนสลับกลับมากรอกได้ก็แตะได้เองทันที
/// </para>
/// </summary>
public class ReadOnlyInput : AntdUI.Input
{
    public ReadOnlyInput() => ApplyLock(base.ReadOnly);

    public override bool ReadOnly
    {
        get => base.ReadOnly;
        set
        {
            base.ReadOnly = value;
            ApplyLock(value);
        }
    }

    /// <summary>
    /// กลืนการกดทิ้งตอนล็อกอยู่
    ///
    /// <para>
    /// ปิด <c>ControlStyles.Selectable</c> อย่างเดียวไม่พอ เพราะ AntdUI เรียก
    /// <c>Focus()</c> กับ <c>Select()</c> เองตรง ๆ ใน <c>OnMouseDown</c>
    /// ซึ่งข้ามการตรวจของ WinForms ว่าคอนโทรลนี้เลือกได้หรือไม่
    /// </para>
    /// <para>
    /// ไม่เรียก base เลยจึงตัดทั้งการรับโฟกัส การวางเคอร์เซอร์ และการลากเลือกข้อความ
    /// ช่องพวกนี้เป็นช่องแสดงผลล้วน ไม่มีปุ่มหน้า/ท้ายช่องให้กดอยู่แล้ว
    /// </para>
    /// </summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (ReadOnly) return;
        base.OnMouseDown(e);
    }

    private void ApplyLock(bool locked)
    {
        // กันเส้นทางของ WinForms เอง เช่นการเลือกด้วย Tab หรือการ SelectNextControl
        // ไม่ได้กันการแตะ เพราะ AntdUI เรียก Focus() เองซึ่งไม่สนค่านี้ — ตัวที่กัน
        // การแตะคือ OnMouseDown ข้างบน
        SetStyle(ControlStyles.Selectable, !locked);

        TabStop = !locked;
        CaretVisible = !locked;
    }
}
