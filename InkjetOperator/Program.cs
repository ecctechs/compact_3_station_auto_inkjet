using System.Diagnostics;
using InkjetOperator.Services;

namespace InkjetOperator;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
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

        // 🔥 init API ครั้งเดียว
        ApiProvider.Init();

        Application.Run(new Form1());
    }
}
