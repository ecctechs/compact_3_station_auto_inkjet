using System.Runtime.InteropServices;

namespace InkjetOperator.Views;

internal static class FullScreenMaximize
{
    private const int WM_GETMINMAXINFO = 0x0024;
    private const int WM_WINDOWPOSCHANGING = 0x0046;

    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;

    private const int MinimizedEdge = -30000;

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

    private static void ApplyMaxInfo(ref Message m)
    {
        var screen = Screen.FromHandle(m.HWnd);
        var info = Marshal.PtrToStructure<MinMaxInfo>(m.LParam);

        info.MaxPosition.X = screen.Bounds.X - screen.WorkingArea.X;
        info.MaxPosition.Y = screen.Bounds.Y - screen.WorkingArea.Y;
        info.MaxSize.X = screen.Bounds.Width;
        info.MaxSize.Y = screen.Bounds.Height;

        info.MaxTrackSize.X = screen.Bounds.Width;
        info.MaxTrackSize.Y = screen.Bounds.Height;

        Marshal.StructureToPtr(info, m.LParam, true);
    }

    private static void KeepFullScreen(Form form, ref Message m)
    {
        if (form.WindowState != FormWindowState.Maximized) return;

        var pos = Marshal.PtrToStructure<WindowPos>(m.LParam);

        if (pos.X <= MinimizedEdge || pos.Y <= MinimizedEdge) return;

        if ((pos.Flags & SWP_NOSIZE) != 0) return;

        var bounds = Screen.FromHandle(m.HWnd).Bounds;
        if (pos.Width >= bounds.Width && pos.Height >= bounds.Height) return;

        pos.X = bounds.X;
        pos.Y = bounds.Y;
        pos.Width = bounds.Width;
        pos.Height = bounds.Height;

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
