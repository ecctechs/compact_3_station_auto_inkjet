namespace InkjetOperator.Theme;

/// <summary>
/// แถบเลื่อนของตารางให้อ้วนพอที่จะจิ้มด้วยนิ้วบนจอสัมผัส
///
/// <para>
/// ค่าเริ่มต้นของ AntdUI คือพื้นที่รับการกด 16 px และแถบที่มองเห็น 6 px ซึ่งเป็น
/// ขนาดสำหรับเมาส์ พอมาอยู่บนจอสัมผัสขนาด Full HD ปลายนิ้วกว้างกว่าแถบหลายเท่า
/// พนักงานจึงจิ้มไม่โดน ต้องลากหลายรอบกว่าจะจับติด
/// </para>
/// <para>
/// ตั้งหลัง AntdUI สร้าง <c>ScrollBar</c> เสร็จแล้วเท่านั้น เพราะตัวมันคำนวณค่านี้
/// ใน constructor ถ้าตั้งก่อนจะถูกทับ และ AntdUI ไม่ได้คำนวณซ้ำอีกหลังจากนั้น
/// </para>
/// </summary>
public static class ScrollStyles
{
    /// <summary>พื้นที่ที่นิ้วจิ้มแล้วจับแถบได้ — หน่วยออกแบบ คูณ DPI ตอนใช้งาน</summary>
    private const int TouchTrack = 28;

    /// <summary>ความหนาของแถบที่มองเห็น — ต้องเห็นชัดว่ามีอะไรให้ลากจากระยะยืนทำงาน</summary>
    private const int TouchThumb = 14;

    /// <summary>
    /// ไล่ทั้งหน้าแล้วขยายแถบเลื่อนของทุกตารางที่เจอ
    ///
    /// เรียกครั้งเดียวจาก constructor ของหน้า ไม่ต้องไล่ตั้งทีละตาราง — หน้าไหน
    /// เพิ่มตารางใหม่ก็ได้ขนาดเดียวกันเองโดยไม่ต้องแก้อะไรเพิ่ม
    /// </summary>
    public static void Touch(Control root)
    {
        foreach (var table in Tables(root)) Apply(table);
    }

    private static void Apply(AntdUI.Table table)
    {
        // ScrollBar เป็นฟิลด์ที่ AntdUI สร้างมาให้พร้อมตัวตาราง ไม่เคยเป็น null
        // แต่เช็คไว้กันเวอร์ชันหน้าเปลี่ยนใจ
        if (table.ScrollBar == null) return;

        float dpi = AntdUI.Config.Dpi;
        table.ScrollBar.SIZE = (int)(TouchTrack * dpi);
        table.ScrollBar.SIZE_BAR = (int)(TouchThumb * dpi);
    }

    private static IEnumerable<AntdUI.Table> Tables(Control control)
    {
        if (control is AntdUI.Table table) yield return table;

        foreach (Control child in control.Controls)
            foreach (var found in Tables(child))
                yield return found;
    }
}
