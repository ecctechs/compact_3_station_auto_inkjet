namespace InkjetOperator.Services;

/// <summary>
/// ทำให้ไฟสถานะของหน้า Setting ตรวจซ้ำเองระหว่างที่คนเปิดหน้านั้นค้างอยู่
///
/// <para>
/// เดิมทุกหน้าตรวจครั้งเดียวตอนเปิดโปรแกรม แล้วไม่ตรวจอีกเลยจนกว่าจะมีคนกดปุ่ม
/// ไฟที่เห็นจึงเป็นภาพนิ่งของเมื่อหลายชั่วโมงก่อน ถอดสายไปแล้วก็ยังเขียวอยู่
/// ซึ่งอันตรายกว่าการไม่มีไฟเลย เพราะคนเชื่อว่าเครื่องยังต่อติด
/// </para>
/// <para>
/// กติกาที่ทุกหน้าต้องเดินตามเหมือนกัน รวมไว้ที่นี่ที่เดียว:
/// ตรวจเฉพาะตอนหน้านั้นโชว์อยู่จริง · รอบก่อนยังไม่จบไม่เริ่มรอบใหม่ ·
/// หยุดให้ตอนกำลังส่งงานเข้าเครื่อง · และห้ามโยน exception ออกมาเด็ดขาด
/// </para>
/// </summary>
public static class StatusRecheck
{
    /// <summary>รอบตรวจซ้ำ — เท่ากับไฟสี่ดวงหน้า Order Detail ที่ใช้งานจริงอยู่แล้ว</summary>
    public const int IntervalMs = 15000;

    /// <summary>
    /// ผูก timer ของหน้าเข้ากับงานตรวจของหน้านั้น
    /// </summary>
    /// <param name="page">หน้าที่เป็นเจ้าของ timer</param>
    /// <param name="timer">timer ที่ประกาศไว้ใน Designer ของหน้านั้น</param>
    /// <param name="check">งานตรวจของหน้า ต้องเงียบ ไม่เด้งกล่อง ไม่เขียน log</param>
    public static void Wire(Control page, System.Windows.Forms.Timer timer, Func<Task> check)
    {
        bool busy = false;

        timer.Interval = IntervalMs;

        // async void ตัวเดียวที่ยอมให้มี — ข้างในจึงกลืน exception ไว้ทั้งหมด
        // หลุดออกมาเมื่อไรคือโปรแกรมปิดทั้งตัว
        timer.Tick += async (_, _) =>
        {
            // Visible ของ WinForms เป็น false เมื่อพ่อแม่ตัวใดตัวหนึ่งถูกซ่อน
            // เช็คตรงนี้ด้วยไม่ใช่พึ่งแต่ event เผื่อกรณีที่หน้าถูกถอดออกจาก
            // แผงเนื้อหาโดยไม่ได้ยิง VisibleChanged มาให้
            if (busy || page.IsDisposed || !page.Visible) return;

            // กำลังคุยกับเครื่องพิมพ์อยู่ก็ข้ามรอบ เหตุผลอยู่ที่ MachineBusy
            if (MachineBusy.Active) return;

            busy = true;
            try
            {
                await check();
            }
            catch
            {
                // ไฟดวงเดียวตรวจไม่ผ่านไม่ควรลากทั้งหน้าไปด้วย รอบหน้ามาใหม่
            }
            finally
            {
                busy = false;
            }
        };

        // เปิดหน้าอยู่ถึงเดิน ออกจากหน้าแล้วหยุด — ไม่งั้นเครื่องที่เคยเปิดหน้านี้
        // ครั้งเดียวจะยิงหาเครื่องปลายทางทุก 15 วินาทีไปทั้งกะทั้งที่ไม่มีใครดู
        page.VisibleChanged += (_, _) =>
        {
            if (page.Visible) timer.Start();
            else timer.Stop();
        };

        if (page.Visible) timer.Start();
    }
}
