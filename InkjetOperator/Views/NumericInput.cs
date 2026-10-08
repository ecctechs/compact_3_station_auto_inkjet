namespace InkjetOperator.Views;

/// <summary>
/// กันไม่ให้พิมพ์อย่างอื่นนอกจากตัวเลขลงช่องที่เป็นค่าตัวเลข
///
/// <para>
/// ช่องพวกนี้ถูกอ่านกลับด้วย <c>int.TryParse</c> ตอนกดบันทึกอยู่แล้ว ตัวอักษรที่หลุด
/// เข้าไปจึงกลายเป็นข้อความผิดพลาดตอนกดเซฟ ซึ่งคนหน้างานต้องไล่แก้ทีละช่อง
/// ปิดตั้งแต่ตอนพิมพ์ทำให้ค่าผิดไม่มีทางเกิดตั้งแต่แรก
/// </para>
/// <para>
/// ไม่ได้แทนการตรวจตอนบันทึก ยังต้องตรวจซ้ำที่นั่นเสมอ เพราะค่าที่มาจากฐานข้อมูล
/// หรือจากงานเก่าอาจมีตัวอักษรติดมาอยู่ก่อนแล้ว และช่องที่ล็อกไว้ก็ปลดล็อกได้ทีหลัง
/// </para>
/// <para>
/// ใช้ <c>VerifyChar</c> ของ AntdUI ซึ่งครอบทั้งการพิมพ์และการวางจากคลิปบอร์ด
/// (ตัววางไล่ตรวจทีละตัวอักษรด้วยทางเดียวกัน)
/// </para>
/// </summary>
internal static class NumericInput
{
    /// <summary>รับเฉพาะเลข 0-9 — ใช้กับช่องที่ฐานข้อมูลเก็บเป็นจำนวนเต็ม</summary>
    public static void DigitsOnly(params AntdUI.Input[] boxes)
    {
        foreach (var box in boxes)
            box.VerifyChar += (_, e) => e.Result = char.IsDigit(e.Char);
    }

    /// <summary>
    /// รับเลข 0-9 และจุดทศนิยมได้จุดเดียว — ใช้กับช่องที่ฐานข้อมูลเก็บเป็นทศนิยม
    ///
    /// <para>
    /// Servo Post Act. กับ Delay เก็บเป็น <c>real</c> และในข้อมูลจริงมีงานที่ใช้
    /// ทศนิยมอยู่ ถ้าห้ามจุดไปเลย ค่าเดิมของงานพวกนั้นจะพิมพ์กลับเข้าไปไม่ได้
    /// </para>
    /// </summary>
    public static void DecimalOnly(params AntdUI.Input[] boxes)
    {
        foreach (var box in boxes)
        {
            box.VerifyChar += (sender, e) =>
            {
                if (char.IsDigit(e.Char)) return;

                // จุดที่สองไม่ใช่ตัวเลขแล้ว ปล่อยผ่านไปก็ parse ไม่ได้อยู่ดี
                e.Result = e.Char == '.'
                    && sender is AntdUI.Input input
                    && !input.Text.Contains('.');
            };
        }
    }

    /// <summary>
    /// คอลัมน์ในตารางที่รับเฉพาะตัวเลข
    ///
    /// <para>
    /// ตารางของ AntdUI สร้างช่องกรอกขึ้นมาใหม่ทุกครั้งที่เริ่มแก้เซลล์ จึงต้องผูก
    /// ตอนที่มันสร้าง ไม่ใช่ผูกไว้ล่วงหน้าเหมือนช่องธรรมดา
    /// </para>
    /// </summary>
    public static void DigitsOnlyColumns(AntdUI.Table table, params string[] columnKeys)
    {
        table.CellBeginEditInputStyle += (_, e) =>
        {
            if (!columnKeys.Contains(e.Column.Key, StringComparer.Ordinal)) return;
            e.Input.VerifyChar += (_, key) => key.Result = char.IsDigit(key.Char);
        };
    }
}
