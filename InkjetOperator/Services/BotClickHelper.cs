using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// BotClick — คลิกอัตโนมัติ + verify ด้วยการเทียบภาพ
/// ใช้สำหรับควบคุมโปรแกรม uvinkjet ผ่าน UI automation
/// Ported จาก BotClickApp.Form1 (โค้ดเก่า)
///
/// ระบบ Screen Capture:
///   • CaptureScreen() — แคปทั้งจอ primary monitor
///   • CaptureArea()   — แคปเฉพาะ area ที่สนใจ
///   • CaptureTargetWindow() — แคปเฉพาะหน้าต่างเป้าหมาย
///   • เปรียบเทียบภาพ before/after เพื่อ retry หาก UI ไม่เปลี่ยน
///   • บันทึก log เป็นรูปภาพลง capture_log/
/// </summary>
public static class BotClickHelper
{
    // ─── Win32 API ───

    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] static extern void mouse_event(int f, int x, int y, int d, int e);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] static extern bool GetCursorInfo(ref CURSORINFO pci);
    [DllImport("user32.dll")]
    static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop,
        IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);

    private static int stepCount = 0;

    private static string sourcePath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\document_backup";
    private static string targetPath = @"\\DESKTOP-KGODCT5\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\document";

    const int SW_MAXIMIZE = 3;
    const int MOUSE_DOWN = 0x02, MOUSE_UP = 0x04;
    const int CURSOR_SHOWING = 0x00000001;
    const int DI_NORMAL = 0x0003;

    [StructLayout(LayoutKind.Sequential)]
    private struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
        public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
    }

    private static IntPtr _targetHWnd = IntPtr.Zero;

    /// <summary>
    /// ภาพล่าสุดที่แคปได้ — ให้ form ดึงไปแสดงใน PictureBox ได้
    /// (เหมือน pictureBox1.Image = img ในโค้ดเก่า)
    /// </summary>
    public static Bitmap LastCapture { get; private set; }

    // ─── Public API ───

    /// <summary>
    /// รันทั้ง bot flow: sleep 3s → activate → click steps ตามลำดับ
    /// เรียกจาก background thread เท่านั้น
    /// (ตรงตามโค้ดเก่า RunBot())
    /// </summary>
    public static BotResult Run(string processName, BotStep[] steps)
    {
        Thread.Sleep(3000); // รอ UI พร้อม (เหมือนโค้ดเก่า)

        if (!ActivateWindow(processName, out string error))
            return BotResult.Fail(error);

        Thread.Sleep(2000); // รอ UI พร้อม

        foreach (var step in steps)
        {
            if (!ClickAndVerify(step))
                return BotResult.Fail($"{step.Name} failed after {step.MaxRetry} retries");
            Thread.Sleep(step.DelayAfter);
        }

        return BotResult.Ok();
    }

    /// <summary>
    /// รัน bot flow แบบ async — ไม่บล็อก UI thread
    /// </summary>
    public static void RunAsync(string processName, BotStep[] steps, Action<BotResult> onComplete = null)
    {
        var t = new Thread(() =>
        {
            var result = Run(processName, steps);
            onComplete?.Invoke(result);
        })
        { IsBackground = true };
        t.Start();
    }

    // ─── Screen Capture ───

    /// <summary>
    /// แคปทั้งจอ primary monitor พร้อมวาด cursor ลงภาพ
    /// </summary>
    public static Bitmap CaptureScreen()
    {
        Rectangle bounds = Screen.PrimaryScreen.Bounds;
        var bmp = new Bitmap(bounds.Width, bounds.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            DrawCursor(g, bounds.Location);
        }
        return bmp;
    }

    /// <summary>
    /// แคปเฉพาะ area ที่กำหนด (screen coordinates) พร้อมวาด cursor ถ้าอยู่ใน area
    /// </summary>
    public static Bitmap CaptureArea(Rectangle area)
    {
        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
        Rectangle validArea = Rectangle.Intersect(area, screenBounds);

        if (validArea.Width <= 0 || validArea.Height <= 0)
            throw new ArgumentException("Invalid capture area");

        var bmp = new Bitmap(validArea.Width, validArea.Height);

        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(validArea.Location, Point.Empty, validArea.Size);
            DrawCursor(g, validArea.Location);
        }

        return bmp;
    }

    /// <summary>
    /// แคปเฉพาะหน้าต่างโปรแกรมเป้าหมาย (ไม่รวม desktop/taskbar) พร้อมวาด cursor
    /// </summary>
    public static Bitmap CaptureTargetWindow()
    {
        if (_targetHWnd == IntPtr.Zero)
            return null;

        if (!GetWindowRect(_targetHWnd, out RECT rect))
            return null;

        var windowRect = rect.ToRectangle();
        if (windowRect.Width <= 0 || windowRect.Height <= 0)
            return null;

        var bmp = new Bitmap(windowRect.Width, windowRect.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(windowRect.Location, Point.Empty, windowRect.Size);
            DrawCursor(g, windowRect.Location);
        }
        return bmp;
    }

    // ─── Core ───

    private static bool ActivateWindow(string processName, out string error)
    {
        error = null;
        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0) { error = $"Process '{processName}' not found"; return false; }

        IntPtr hWnd = procs[0].MainWindowHandle;
        if (hWnd == IntPtr.Zero) { error = "Window handle not found"; return false; }

        _targetHWnd = hWnd;

        ShowWindow(hWnd, SW_MAXIMIZE);
        BringWindowToTop(hWnd);
        SetForegroundWindow(hWnd);
        return true;
    }

    /// <summary>
    /// Click → compare before/after area → SaveCapture ทั้งจอ → retry ถ้าภาพไม่เปลี่ยน
    /// (ตรงตามโค้ดเก่า ClickAndVerify())
    /// </summary>
    private static bool ClickAndVerify(BotStep step)
    {
        for (int i = 1; i <= step.MaxRetry; i++)
        {
            Console.WriteLine($"[{step.Name}] Try {i}");

            // แคป area ก่อน click
            Bitmap before = CaptureArea(step.VerifyArea);

            LeftClick(step.X, step.Y);

            Thread.Sleep(step.VerifyDelay);

            // แคป area หลัง click
            Bitmap after = CaptureArea(step.VerifyArea);

            // เปรียบเทียบแบบ exact (pixel ต่างเมื่อไหร่ = เปลี่ยน)
            bool same = CompareBitmapsFast(before, after);

            // save ทั้งจอลง capture_log (เหมือนโค้ดเก่า)
            SaveCapture($"{step.Name}_Try{i}");

            before.Dispose();
            after.Dispose();

            if (!same)
            {
                Console.WriteLine($"[{step.Name}] ✅ Success");
                return true;
            }

            Console.WriteLine($"[{step.Name}] ❌ Retry...");
            Thread.Sleep(1000);
        }

        Console.WriteLine($"[{step.Name}] ❌ Failed");
        return false;
    }

    /// <summary>
    /// เปรียบเทียบ pixel แบบ fast — เหมือนโค้ดเก่า CompareBitmapsFast()
    /// สุ่มทุก 5 pixel, pixel ต่างตัวเดียว = return false (ภาพเปลี่ยน)
    /// </summary>
    public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
    {
        if (bmp1.Size != bmp2.Size)
            return false;

        for (int x = 0; x < bmp1.Width; x += 5)
            for (int y = 0; y < bmp1.Height; y += 5)
                if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    return false;

        return true;
    }

    /// <summary>
    /// แคปทั้งจอ → บันทึกลง capture_log/ → เก็บ LastCapture
    /// (เหมือนโค้ดเก่า SaveCapture() ที่เรียก CaptureScreen + pictureBox1.Image = img)
    /// </summary>
    private static void SaveCapture(string stepName)
    {
        Bitmap img = CaptureScreen();

        var folder = System.IO.Path.Combine(Application.StartupPath, "capture_log");
        System.IO.Directory.CreateDirectory(folder);

        var path = System.IO.Path.Combine(
            folder, $"{stepName}_{DateTime.Now:HHmmss}.png");
        img.Save(path, ImageFormat.Png);

        // เก็บภาพล่าสุดให้ form ดึงไปแสดง
        LastCapture?.Dispose();
        LastCapture = img;
    }

    private static void LeftClick(int x, int y)
    {
        Cursor.Position = new Point(x, y);
        Thread.Sleep(200);
        mouse_event(MOUSE_DOWN, x, y, 0, 0);
        Thread.Sleep(100);
        mouse_event(MOUSE_UP, x, y, 0, 0);
    }

    /// <summary>
    /// วาด cursor ปัจจุบันลงบน Graphics (ตำแหน่ง offset จาก captureOrigin)
    /// </summary>
    private static void DrawCursor(Graphics g, Point captureOrigin)
    {
        var ci = new CURSORINFO();
        ci.cbSize = Marshal.SizeOf(ci);

        if (!GetCursorInfo(ref ci))
            return;

        if ((ci.flags & CURSOR_SHOWING) == 0)
            return;

        IntPtr hdc = g.GetHdc();
        try
        {
            int x = ci.ptScreenPos.X - captureOrigin.X;
            int y = ci.ptScreenPos.Y - captureOrigin.Y;
            DrawIconEx(hdc, x, y, ci.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
        }
        finally
        {
            g.ReleaseHdc(hdc);
        }
    }

    public static void CopyFile(string fileName)
    {
        try
        {
            string sourceFile = Path.Combine(sourcePath, fileName);
            string targetFile = Path.Combine(targetPath, fileName);

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, targetFile, true);
                Console.WriteLine($"✅ Copy สำเร็จ: {fileName}");
            }
            else
            {
                Console.WriteLine($"❌ ไม่พบไฟล์: {fileName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        // เพิ่ม step ทุกครั้งที่เรียก
        stepCount++;

        // ถ้าครบ 4 step → ลบไฟล์
        if (stepCount >= 4)
        {
            ClearDocumentFolder();
            stepCount = 0; // reset ใหม่
        }
    }

    public static void ClearDocumentFolder()
    {
        try
        {
            var files = Directory.GetFiles(targetPath);

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Console.WriteLine("🧹 ลบไฟล์ใน document เรียบร้อยแล้ว");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ลบไฟล์ไม่สำเร็จ: {ex.Message}");
        }
    }

    public static void WaitForFileReady(string filePath, int timeoutMs = 5000)
    {
        var start = DateTime.Now;

        while (true)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    break;
                }
            }
            catch
            {
                if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                    throw new Exception("File not ready (timeout): " + filePath);

                Thread.Sleep(200);
            }
        }
    }
}

// ─── Models ───

public class BotStep
{
    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Rectangle VerifyArea { get; set; }
    public int MaxRetry { get; set; } = 3;
    public int VerifyDelay { get; set; } = 1500;
    public int DelayAfter { get; set; } = 1000;
}

public class BotResult
{
    public bool Success { get; set; }
    public string Error { get; set; }

    public static BotResult Ok() => new BotResult { Success = true };
    public static BotResult Fail(string err) => new BotResult { Success = false, Error = err };
}