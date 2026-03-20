using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BotClickApp
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware(); // 🔥 เพิ่ม

        [STAThread]
        static void Main()
        {
            SetProcessDPIAware(); // 🔥 เรียกตรงนี้

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormPatternLookup());
        }
    }
}
