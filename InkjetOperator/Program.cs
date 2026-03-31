using System.Diagnostics;
using System.Runtime.InteropServices;
using InkjetOperator.Services;

namespace InkjetOperator;

static class Program
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();
    [STAThread]
    static void Main()
    {
        SetProcessDPIAware();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // โหลด patterns (สร้าง default ถ้ายังไม่มี)
        string xmlPath = Path.Combine(Application.StartupPath, "patterns.xml");
        PatternStore.SeedDefaults(xmlPath); // สร้างไฟล์ + default patterns ถ้ายังไม่มี
        PatternStore.Load(xmlPath);         // โหลด patterns จาก XML เข้า memory

        // ถ้า Load ล้มเหลว (XML เสีย → ถูกลบ) สร้าง + โหลดใหม่
        if (PatternStore.Patterns.Count == 0)
        {
            PatternStore.SeedDefaults(xmlPath);
            PatternStore.Load(xmlPath);
        }

        Application.Run(new Form1());
    }
}
