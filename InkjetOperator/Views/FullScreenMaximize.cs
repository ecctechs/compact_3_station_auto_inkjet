using System.Runtime.InteropServices;

namespace InkjetOperator.Views;

/// <summary>
/// บังคับให้หน้าต่างที่ขยายเต็มจอ กินพื้นที่ "ทั้งจอ" ไม่ใช่แค่ "พื้นที่ทำงาน"
/// <para>
/// ปกติ Windows ขยายหน้าต่างเต็มแค่พื้นที่ทำงาน ซึ่งหดลงเมื่อมีอะไรมาจอง เช่น
/// <b>คีย์บอร์ดสัมผัสบน Panel PC</b> พอพื้นที่ทำงานหด หน้าต่างที่ขยายอยู่ก็หดตาม
/// แล้วทุกอย่างในโปรแกรมถูกบีบ ช่องกรอกแบนติดกันจนใช้งานไม่ได้
/// </para>
/// <para>
/// ดักข้อความ WM_GETMINMAXINFO แล้วตอบขนาดของจอเต็มแทน คีย์บอร์ดจะลอยทับ
/// ด้านล่างของโปรแกรมแทนที่จะไปบีบมัน — ขนาดหน้าต่างคงเดิมตลอด
/// </para>
/// <para>
/// ผลข้างเคียงที่ตั้งใจ: ตอนขยายเต็มจอจะทับแถบงานของ Windows ด้วย ซึ่งเป็นสิ่งที่
/// ต้องการอยู่แล้วสำหรับเครื่องหน้างานที่ให้เห็นเฉพาะโปรแกรมนี้
/// </para>
/// </summary>
internal static class FullScreenMaximize
{
    private const int WM_GETMINMAXINFO = 0x0024;

    /// <summary>
    /// เรียกจาก <c>WndProc</c> ก่อนส่งต่อให้ base — คืน true เมื่อจัดการข้อความนี้แล้ว
    /// </summary>
    public static bool Handle(ref Message m)
    {
        if (m.Msg != WM_GETMINMAXINFO) return false;

        var screen = Screen.FromHandle(m.HWnd);
        var info = Marshal.PtrToStructure<MinMaxInfo>(m.LParam);

        // ตำแหน่งและขนาดอ้างอิงจุดกำเนิดของจอที่หน้าต่างอยู่ ไม่ใช่จุดกำเนิดเดสก์ท็อป
        // จึงต้องหักส่วนต่างออก ไม่งั้นจอที่สองจะวางผิดตำแหน่ง
        info.MaxPosition.X = screen.Bounds.X - screen.WorkingArea.X;
        info.MaxPosition.Y = screen.Bounds.Y - screen.WorkingArea.Y;
        info.MaxSize.X = screen.Bounds.Width;
        info.MaxSize.Y = screen.Bounds.Height;

        // ต้องขยายเพดานด้วย ไม่งั้น Windows ตัดขนาดกลับมาเท่าพื้นที่ทำงานอยู่ดี
        info.MaxTrackSize.X = screen.Bounds.Width;
        info.MaxTrackSize.Y = screen.Bounds.Height;

        Marshal.StructureToPtr(info, m.LParam, true);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point2
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point2 Reserved;
        public Point2 MaxSize;
        public Point2 MaxPosition;
        public Point2 MinTrackSize;
        public Point2 MaxTrackSize;
    }
}
