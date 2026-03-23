using System.Runtime.InteropServices;

namespace InkjetOperator;

static class Program
{
    [STAThread]
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();
    static void Main()
    {
        SetProcessDPIAware();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new frmMain());
    }
}
