using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InkjetOperator.Views;

/// <summary>
/// ลากนิ้วเลื่อนหน้าแบบมือถือบนจอสัมผัส (panel PC) — ทั้งโปรแกรม ติดตั้งครั้งเดียวที่ <c>Program.Main</c>
///
/// <para>
/// หน้าที่ยาวกว่าจอ (Setting ทุกหน้า, Order Detail, Station Options) เป็น Panel ของ WinForms
/// ที่เลื่อนได้ทาง scrollbar อย่างเดียว นิ้วลากบนปุ่ม/ช่องกรอกแล้วหน้าไม่ขยับ (ทดสอบด้วยนิ้วจำลอง
/// InjectTouchInput) ตัวนี้ทำให้ลากตรงไหนของหน้าก็เลื่อนได้ ปล่อยนิ้วแล้วไหลต่อแล้วค่อย ๆ หยุด
/// </para>
/// <para>
/// กติกา:
/// · แตะเฉย ๆ (ขยับไม่เกิน <see cref="SlopDip"/>) = กดตามปกติ ไม่แตะอะไรเลย
/// · ลากแนวตั้งเกินระยะนั้น = เลื่อนหน้า และปุ่มที่นิ้วเริ่มแตะจะไม่ถูกกด (กันกดพลาดตอนเลื่อนหาแถว)
/// · ลากแนวนอน = ปล่อยให้ control ข้างใต้จัดการเอง
/// · นิ้วลงระหว่างหน้ากำลังไหล = หยุดไหล ไม่กดอะไร · ลากต่อได้ทันที ปัดซ้ำได้ (แบบ iPhone)
/// · ตาราง AntdUI ที่เลื่อนได้ และ ListBox / TextBox / ComboBox เลื่อนด้วยนิ้วได้เองอยู่แล้ว ไม่ไปยุ่ง
///   (ตาราง AntdUI ไหลต่อหลังปล่อยนิ้วเองด้วย) — ตารางที่แสดงครบไม่มีให้เลื่อน ลากแล้วหน้าที่ครอบอยู่เลื่อนแทน
/// · ทำงานเฉพาะการแตะจากจอสัมผัส (Windows ติดป้ายไว้กับข้อความเมาส์) ใช้เมาส์เหมือนเดิมทุกอย่าง
/// </para>
/// <para>
/// ไม่วาดอะไรเอง ไม่สร้าง control — แค่อ่านข้อความเมาส์ก่อนถึง control แล้วเลื่อน AutoScrollPosition
/// </para>
/// </summary>
internal sealed class TouchScroll : IMessageFilter
{
    /// <summary>ระยะที่นิ้วขยับได้โดยยังนับเป็น "แตะ" — หน่วยออกแบบ (96 dpi) คูณ DPI จริงตอนใช้</summary>
    private const int SlopDip = 14;

    /// <summary>ปล่อยนิ้วเร็วกว่านี้ (จุด/มิลลิวินาที) หน้าจะไหลต่อ</summary>
    private const double FlingStartSpeed = 0.25;

    /// <summary>ความเร็วต่ำกว่านี้หยุดไหล</summary>
    private const double FlingStopSpeed = 0.02;

    /// <summary>
    /// แรงหน่วงต่อมิลลิวินาที — 0.997 ใกล้เคียง UIScrollView ของ iOS (0.998 ไหลนานกว่า)
    /// จอโรงงานไม่ต้องไหลไกล หยุดไวขึ้นนิดหนึ่งหาของง่ายกว่า
    /// </summary>
    private const double FlingDecayPerMs = 0.997;

    /// <summary>
    /// แตะภายในเวลานี้หลังหน้าหยุดไหลเอง ยังนับเป็น "แตะเพื่อหยุด" ไม่กดปุ่ม (มิลลิวินาที)
    /// <para>
    /// Windows ส่งการกดของการแตะสั้น ๆ มาตอนยกนิ้ว ไม่ใช่ตอนนิ้วลง — แตะเพื่อหยุดตอนหน้าใกล้หยุด
    /// การกดจะมาถึงหลังหน้าหยุดไปแล้วนิดเดียว กลายเป็นกดปุ่มที่บังเอิญอยู่ใต้นิ้ว (เช่น เริ่มงาน)
    /// </para>
    /// </summary>
    private const int StopTapGraceMs = 200;

    /// <summary>ความเร็วตอนปล่อยนิ้ว คิดจากการเคลื่อนที่ช่วงท้ายเท่านี้ (มิลลิวินาที)</summary>
    private const int VelocityWindowMs = 90;

    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_LBUTTONDBLCLK = 0x0203;

    // ป้ายที่ Windows ติดกับข้อความเมาส์ที่แปลงมาจากนิ้ว/ปากกา — บิต 0x80 = นิ้ว
    private const long SignatureMask = 0xFFFFFF00;
    private const long PenOrTouchSignature = 0xFF515700;
    private const long TouchBit = 0x80;

    [DllImport("user32.dll")] private static extern IntPtr GetMessageExtraInfo();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

    /// <summary>ตำแหน่งนิ้วบนจอ จากข้อความเอง — ไม่ใช้ตำแหน่งเคอร์เซอร์ซึ่งอาจตามไม่ทันนิ้ว</summary>
    private static Point ScreenPoint(Message m)
    {
        long lp = m.LParam.ToInt64();
        var point = new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF));
        ClientToScreen(m.HWnd, ref point);
        return point;
    }

    private static TouchScroll? _installed;

    /// <summary>เปิดใช้ทั้งโปรแกรม — เรียกซ้ำไม่มีผล</summary>
    public static void Install()
    {
        if (_installed != null) return;
        _installed = new TouchScroll();
        Application.AddMessageFilter(_installed);
    }

    // ── สถานะของนิ้วที่กำลังแตะอยู่ ─────────────────────────

    private ScrollableControl? _host;
    private IntPtr _pressedWindow;
    private Point _down;
    private int _downOffset;
    private bool _tracking;
    private bool _panning;

    /// <summary>การกดของนิ้วนี้ไม่ได้ส่งไปถึง control (นิ้วลงตอนหน้ากำลังไหล) — ต้องกลืนจนยกนิ้ว</summary>
    private bool _downSwallowed;
    private readonly Queue<(long Ms, int Y)> _samples = new();

    // ── ไหลต่อหลังปล่อยนิ้ว ─────────────────────────────────

    private readonly System.Windows.Forms.Timer _fling = new() { Interval = 15 };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private ScrollableControl? _flingHost;
    private double _flingVelocity;
    private double _flingOffset;
    private long _flingLastMs;
    private long _flingEndedMs = long.MinValue / 2;

    private TouchScroll()
    {
        _fling.Tick += (_, _) => FlingStep();
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg is not (WM_LBUTTONDOWN or WM_LBUTTONDBLCLK or WM_MOUSEMOVE or WM_LBUTTONUP)) return false;
        if (!_tracking && m.Msg != WM_LBUTTONDOWN && m.Msg != WM_LBUTTONDBLCLK) return false;
        if (!FromTouch()) return false;

        try
        {
            return m.Msg switch
            {
                WM_LBUTTONDOWN or WM_LBUTTONDBLCLK => OnDown(m),
                WM_MOUSEMOVE => OnMove(m),
                _ => OnUp(),
            };
        }
        catch
        {
            // การเลื่อนเป็นของแถม ห้ามพาโปรแกรมล้ม — ปล่อยข้อความไปตามปกติ
            Reset();
            return false;
        }
    }

    private static bool FromTouch()
    {
        long info = GetMessageExtraInfo().ToInt64();
        return (info & SignatureMask) == PenOrTouchSignature && (info & TouchBit) != 0;
    }

    private bool OnDown(Message m)
    {
        Reset();

        // นิ้วลงตอนหน้ากำลังไหล (หรือเพิ่งหยุด) = หยุดไหล และไม่ส่งการกดลงไปถึงปุ่มใต้นิ้ว
        // แต่ยังตามนิ้วต่อ — ลากต่อได้ทันที ปัดซ้ำเพื่อเลื่อนไกลขึ้นได้ (แบบ iPhone)
        // Windows อาจส่งการแตะนี้มาเป็นดับเบิลคลิก (แตะซ้ำที่เดิมเร็ว ๆ) จึงดักทั้งสองแบบ
        bool stopTap = _fling.Enabled || _clock.ElapsedMilliseconds - _flingEndedMs < StopTapGraceMs;
        var flingHost = _flingHost;
        if (stopTap) StopFling();

        var control = Control.FromChildHandle(m.HWnd);
        _host = stopTap && flingHost is { IsDisposed: false } ? flingHost
            : control == null ? null : FindHost(control);
        if (_host == null) return false;   // ไม่มีหน้าให้เลื่อนแล้ว (ถูกปิดไป) — ส่งการกดไปตามปกติ

        _downSwallowed = stopTap;
        _pressedWindow = stopTap ? IntPtr.Zero : m.HWnd;
        _down = ScreenPoint(m);
        _downOffset = -_host.AutoScrollPosition.Y;
        _tracking = true;
        AddSample(_down.Y);

        // ปกติ: การกดลงไปถึง control ตามปกติ ยังไม่รู้ว่าแตะหรือลาก
        return stopTap;
    }

    private bool OnMove(Message m)
    {
        if (!_tracking || _host == null || _host.IsDisposed) { Reset(); return false; }

        var now = ScreenPoint(m);
        AddSample(now.Y);

        if (!_panning)
        {
            int slop = (int)Math.Round(SlopDip * _host.DeviceDpi / 96.0);
            int dx = now.X - _down.X, dy = now.Y - _down.Y;
            if (Math.Abs(dy) <= slop && Math.Abs(dx) <= slop) return _downSwallowed;

            // ลากแนวนอน — ไม่ใช่การเลื่อนหน้า ปล่อยให้ control ข้างใต้จัดการ
            // (นิ้วที่กลืนการกดไปแล้ว ต้องกลืนต่อจนยกนิ้ว ไม่งั้น control ได้แต่การปล่อยนิ้ว)
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (_downSwallowed) return true;
                Reset();
                return false;
            }

            _panning = true;
            CancelPress();

            // เริ่มเลื่อนจากจุดนี้ ไม่กระโดดตามระยะที่ขยับไปแล้วก่อนรู้ว่าเป็นการลาก
            _down = now;
            _downOffset = -_host.AutoScrollPosition.Y;
        }

        ScrollTo(_host, _downOffset - (now.Y - _down.Y));
        return true;
    }

    private bool OnUp()
    {
        bool wasPanning = _panning;
        bool swallowed = _downSwallowed;
        var host = _host;
        double velocity = Velocity();
        Reset();

        if (!wasPanning || host == null) return swallowed;

        // นิ้วเลื่อนขึ้น = เนื้อหาเลื่อนขึ้น = ตำแหน่งเลื่อนเพิ่ม · ความเร็วนิ้วจึงกลับเครื่องหมาย
        if (Math.Abs(velocity) >= FlingStartSpeed) StartFling(host, -velocity);
        return true;   // ปุ่มที่นิ้วเริ่มแตะถูกยกเลิกไปแล้ว ไม่ต้องส่งการปล่อยนิ้วต่อ
    }

    /// <summary>
    /// ยกเลิกการกดของ control ใต้นิ้ว — ส่ง "ปล่อยนิ้ว" ที่ตำแหน่งนอกตัวมัน ปุ่มจะคืนสภาพโดยไม่นับเป็นคลิก
    /// (ปุ่มของ WinForms และ AntdUI นับคลิกเฉพาะปล่อยนิ้วในกรอบของตัวเอง)
    /// </summary>
    private void CancelPress()
    {
        if (_pressedWindow == IntPtr.Zero) return;
        const short outside = -30000;
        int lParam = (outside & 0xFFFF) | (outside << 16);
        SendMessage(_pressedWindow, WM_LBUTTONUP, IntPtr.Zero, (IntPtr)lParam);
        _pressedWindow = IntPtr.Zero;
    }

    /// <summary>
    /// หน้าที่จะเลื่อน — แผงที่เลื่อนแนวตั้งได้อยู่ใกล้นิ้วที่สุด
    /// null = ไม่ยุ่ง (control ข้างใต้เลื่อนด้วยนิ้วได้เอง หรือไม่มีอะไรให้เลื่อน)
    /// </summary>
    private static ScrollableControl? FindHost(Control control)
    {
        for (var c = control; c != null; c = c.Parent)
        {
            switch (c)
            {
                // ตาราง AntdUI ลากเลื่อนและไหลต่อเองอยู่แล้ว — ยกเว้นตารางที่แสดงครบ ไม่มีอะไรให้เลื่อน
                case AntdUI.Table table when table.ScrollBar.ShowY:
                    return null;
                // control ของ Windows ที่เลื่อนด้วยนิ้วได้เอง
                case ListBox or TextBoxBase or ComboBox or DataGridView or TreeView or ListView:
                    return null;
                case ScrollableControl panel when panel.AutoScroll && panel.VerticalScroll.Visible:
                    return panel;
            }
        }
        return null;
    }

    private static void ScrollTo(ScrollableControl host, double offset)
    {
        int max = Math.Max(0, host.DisplayRectangle.Height - host.ClientSize.Height);
        int y = (int)Math.Round(Math.Clamp(offset, 0, max));
        if (y == -host.AutoScrollPosition.Y) return;
        host.AutoScrollPosition = new Point(-host.AutoScrollPosition.X, y);
    }

    // ── ความเร็วตอนปล่อยนิ้ว ───────────────────────────────

    private void AddSample(int y)
    {
        long ms = _clock.ElapsedMilliseconds;
        _samples.Enqueue((ms, y));
        while (_samples.Count > 2 && ms - _samples.Peek().Ms > VelocityWindowMs) _samples.Dequeue();
    }

    /// <summary>จุดต่อมิลลิวินาที — บวก = นิ้วเลื่อนลง</summary>
    private double Velocity()
    {
        if (_samples.Count < 2) return 0;
        var first = _samples.Peek();
        var last = _samples.Last();
        long dt = last.Ms - first.Ms;

        // นิ้วหยุดนิ่งก่อนปล่อย = ไม่ได้ปัด ไม่ต้องไหลต่อ
        if (dt <= 0 || _clock.ElapsedMilliseconds - last.Ms > VelocityWindowMs) return 0;
        return (last.Y - first.Y) / (double)dt;
    }

    // ── ไหลต่อ ─────────────────────────────────────────────

    private void StartFling(ScrollableControl host, double velocity)
    {
        _flingHost = host;
        _flingVelocity = velocity;
        _flingOffset = -host.AutoScrollPosition.Y;
        _flingLastMs = _clock.ElapsedMilliseconds;
        _fling.Start();
    }

    private void FlingStep()
    {
        var host = _flingHost;
        if (host == null || host.IsDisposed || !host.Visible) { StopFling(); return; }

        long now = _clock.ElapsedMilliseconds;
        long dt = Math.Max(1, now - _flingLastMs);
        _flingLastMs = now;

        _flingOffset += _flingVelocity * dt;
        _flingVelocity *= Math.Pow(FlingDecayPerMs, dt);

        int max = Math.Max(0, host.DisplayRectangle.Height - host.ClientSize.Height);
        bool atEdge = _flingOffset <= 0 || _flingOffset >= max;
        ScrollTo(host, _flingOffset);

        if (atEdge || Math.Abs(_flingVelocity) < FlingStopSpeed) StopFling();
    }

    private void StopFling()
    {
        if (_fling.Enabled) _flingEndedMs = _clock.ElapsedMilliseconds;
        _fling.Stop();
        _flingHost = null;
        _flingVelocity = 0;
    }

    private void Reset()
    {
        _tracking = false;
        _panning = false;
        _downSwallowed = false;
        _host = null;
        _pressedWindow = IntPtr.Zero;
        _samples.Clear();
    }
}
