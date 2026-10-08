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
/// กันไว้ 2 ชั้น
/// <list type="number">
/// <item><c>WM_GETMINMAXINFO</c> — ตอบขนาดจอเต็มตอน Windows ถามว่าขยายได้เท่าไร</item>
/// <item><c>WM_WINDOWPOSCHANGING</c> — ดักทุกครั้งที่มีคนสั่งย่อ แล้วยัดขนาดเต็มจอ
/// กลับไป ชั้นนี้จำเป็นเพราะคีย์บอร์ดของบางเครื่องสั่งย่อหน้าต่างตรง ๆ
/// ไม่ได้ผ่านการเปลี่ยนพื้นที่ทำงาน</item>
/// </list>
/// </para>
/// <para>
/// ชั้นที่สองปล่อยผ่านตอนพับลงแถบงานเสมอ ไม่งั้นจะพับหน้าจอไม่ได้
/// </para>
/// <para>
/// ผลข้างเคียงที่ตั้งใจ: ตอนขยายเต็มจอจะทับแถบงานของ Windows ด้วย ซึ่งเป็นสิ่งที่
/// ต้องการอยู่แล้วสำหรับเครื่องหน้างานที่ให้เห็นเฉพาะโปรแกรมนี้
/// </para>
/// </summary>
internal static class FullScreenMaximize
{
    private const int WM_GETMINMAXINFO = 0x0024;
    private const int WM_WINDOWPOSCHANGING = 0x0046;

    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;

    /// <summary>หน้าต่างที่ถูกพับจะถูกย้ายไปไว้นอกจอที่พิกัดราว -32000</summary>
    private const int MinimizedEdge = -30000;

    /// <summary>เรียกจาก <c>WndProc</c> ก่อนส่งต่อให้ base</summary>
    public static void Handle(Form form, ref Message m)
    {
        switch (m.Msg)
        {
            case WM_GETMINMAXINFO:
                ApplyMaxInfo(ref m);
                break;

            case WM_WINDOWPOSCHANGING:
                KeepFullScreen(form, ref m);
                break;
        }
    }

    /// <summary>ชั้นที่ 1 — บอก Windows ว่าขยายเต็มได้เท่าขนาดจอ ไม่ใช่พื้นที่ทำงาน</summary>
    private static void ApplyMaxInfo(ref Message m)
    {
        var screen = Screen.FromHandle(m.HWnd);
        var info = Marshal.PtrToStructure<MinMaxInfo>(m.LParam);

        // ตำแหน่งอ้างอิงจุดกำเนิดของจอที่หน้าต่างอยู่ ไม่ใช่จุดกำเนิดเดสก์ท็อป
        // จึงต้องหักส่วนต่างออก ไม่งั้นจอที่สองจะวางผิดตำแหน่ง
        info.MaxPosition.X = screen.Bounds.X - screen.WorkingArea.X;
        info.MaxPosition.Y = screen.Bounds.Y - screen.WorkingArea.Y;
        info.MaxSize.X = screen.Bounds.Width;
        info.MaxSize.Y = screen.Bounds.Height;

        // ต้องขยายเพดานด้วย ไม่งั้น Windows ตัดขนาดกลับมาเท่าพื้นที่ทำงานอยู่ดี
        info.MaxTrackSize.X = screen.Bounds.Width;
        info.MaxTrackSize.Y = screen.Bounds.Height;

        Marshal.StructureToPtr(info, m.LParam, true);
    }

    /// <summary>ชั้นที่ 2 — ใครสั่งย่อระหว่างที่ขยายเต็มจออยู่ ให้ยัดขนาดเต็มกลับไป</summary>
    private static void KeepFullScreen(Form form, ref Message m)
    {
        if (form.WindowState != FormWindowState.Maximized) return;

        var pos = Marshal.PtrToStructure<WindowPos>(m.LParam);

        // กำลังพับลงแถบงาน — Windows ย้ายหน้าต่างออกไปนอกจอ ห้ามขวาง
        // ไม่งั้นผู้ใช้จะพับหน้าจอไม่ได้เลย
        if (pos.X <= MinimizedEdge || pos.Y <= MinimizedEdge) return;

        // ไม่ได้สั่งเปลี่ยนขนาด ไม่ต้องยุ่ง
        if ((pos.Flags & SWP_NOSIZE) != 0) return;

        var bounds = Screen.FromHandle(m.HWnd).Bounds;
        if (pos.Width >= bounds.Width && pos.Height >= bounds.Height) return;

        pos.X = bounds.X;
        pos.Y = bounds.Y;
        pos.Width = bounds.Width;
        pos.Height = bounds.Height;

        // ล้างธงที่บอกให้ข้ามการย้าย/ปรับขนาด ไม่งั้นค่าที่เพิ่งใส่จะถูกเมิน
        pos.Flags &= ~(SWP_NOSIZE | SWP_NOMOVE);

        Marshal.StructureToPtr(pos, m.LParam, true);
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

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPos
    {
        public IntPtr Handle;
        public IntPtr InsertAfter;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Flags;
    }
}
