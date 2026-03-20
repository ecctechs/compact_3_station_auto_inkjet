using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Xml.Linq;
using System.Diagnostics;

namespace BotClickApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // new: hold values to update after bot run
        private string pendingLot = string.Empty;
        private string pendingName = string.Empty;

        // ==============================
        // 🔍 Window Control
        // ==============================
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        const int SW_MAXIMIZE = 3;

        public bool ActivateProgramByProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                MessageBox.Show("ไม่เจอโปรแกรม");
                return false;
            }

            var proc = processes[0];
            IntPtr hWnd = proc.MainWindowHandle;

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("หา window ไม่เจอ");
                return false;
            }

            ShowWindow(hWnd, SW_MAXIMIZE);
            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);

            return true;
        }

        // ==============================
        // 🖱 Mouse Click
        // ==============================
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;

        private void LeftClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            Thread.Sleep(200);

            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        // ==============================
        // 📸 Capture Screen
        // ==============================
        private Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        private void SaveCapture(string stepName)
        {
            Bitmap img = CaptureScreen();

            string path = Application.StartupPath + $"\\{stepName}_{DateTime.Now:HHmmss}.png";
            img.Save(path, ImageFormat.Png);

            pictureBox1.Image = img;
        }

        // ==============================
        // 📸 Capture เฉพาะพื้นที่
        // ==============================
        private Bitmap CaptureArea(Rectangle area)
        {
            Bitmap bmp = new Bitmap(area.Width, area.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(area.Location, Point.Empty, area.Size);
            }

            return bmp;
        }

        // ==============================
        // 🔍 Compare ภาพ
        // ==============================
        private bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size)
                return false;

            for (int x = 0; x < bmp1.Width; x += 5)
            {
                for (int y = 0; y < bmp1.Height; y += 5)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                        return false;
                }
            }
            return true;
        }

        // ==============================
        // 🔁 Click + Verify + Retry
        // ==============================
        private bool ClickAndVerify(int x, int y, Rectangle checkArea, string stepName, int maxRetry = 3)
        {
            for (int i = 1; i <= maxRetry; i++)
            {
                Console.WriteLine($"[{stepName}] Try {i}");

                Bitmap before = CaptureArea(checkArea);

                LeftClick(x, y);

                Thread.Sleep(1500);

                Bitmap after = CaptureArea(checkArea);

                bool same = CompareBitmapsFast(before, after);

                SaveCapture(stepName + "_Try" + i);

                if (!same)
                {
                    Console.WriteLine($"[{stepName}] ✅ Success");
                    return true;
                }

                Console.WriteLine($"[{stepName}] ❌ Retry...");
                Thread.Sleep(1000);
            }

            Console.WriteLine($"[{stepName}] ❌ Failed");
            MessageBox.Show(stepName + " Failed");
            return false;
        }

        // ==============================
        // 🤖 BOT FLOW
        // ==============================
        private void RunBot()
        {
            Thread.Sleep(3000);

            // 🔥 1. หา + ดึงโปรแกรมขึ้นมา
            bool ok = ActivateProgramByProcess("uvinkjet");

            if (!ok)
            {
                MessageBox.Show("ไม่เจอโปรแกรม UV");
                return;
            }

            Thread.Sleep(2000); // รอ UI พร้อม

            // Step 1: Document
            Rectangle docArea = new Rectangle(2100, 0, 400, 300);
            if (!ClickAndVerify(2325, 59, docArea, "Step1_Document"))
                return;

            Thread.Sleep(1000);

            // Step 2: Open
            Rectangle openArea = new Rectangle(2100, 100, 400, 400);
            if (!ClickAndVerify(2207, 133, openArea, "Step2_Open"))
                return;

            Thread.Sleep(1000);

            // Step 3: Adler
            Rectangle fileArea = new Rectangle(800, 400, 800, 600);
            if (!ClickAndVerify(1506, 654, fileArea, "Step3_Adler"))
                return;

            Thread.Sleep(1000);

            // Step 4: Open Button
            Rectangle btnOpenArea = new Rectangle(1500, 800, 500, 300);
            ClickAndVerify(1652, 946, btnOpenArea, "Step4_OpenButton");

            // After bot finished, perform DB update with pending values (if any)
            try
            {
                // small delay to ensure UI settled
                Thread.Sleep(500);

                // If we have pending values, call UpdateText on UI thread (UpdateText shows MessageBox)
                if (!string.IsNullOrEmpty(pendingLot) || !string.IsNullOrEmpty(pendingName))
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate {
                                UpdateText(pendingLot, pendingName);
                            });
                        }
                        catch
                        {
                            // If invoke fails for any reason, try calling directly (fallback)
                            UpdateText(pendingLot, pendingName);
                        }
                    }
                    else
                    {
                        // UI not available - call directly
                        UpdateText(pendingLot, pendingName);
                    }
                }
            }
            catch
            {
                // swallow any exceptions here to avoid crashing background thread
            }
        }

        // ==============================
        // 🎯 UI
        // ==============================
        private void btnStart_Click(object sender, EventArgs e)
        {
            // capture values to update after bot completes
            pendingLot = txtLot.Text;
            pendingName = txtName.Text;

            Thread t = new Thread(new ThreadStart(RunBot));
            t.IsBackground = true;
            t.Start();
        }

        private void btnEditPattern_Click(object sender, EventArgs e)
        {
            using (var dlg = new FormEditPattern())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = Cursor.Position.ToString();
        }

        private void UpdateText(string lot, string name)
        {
            string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";

            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@lot", lot);
                    cmd.Parameters.AddWithValue("@name", name);

                    int rows = cmd.ExecuteNonQuery();

                    MessageBox.Show("Update สำเร็จ: " + rows + " row");
                }
            }
        }
    }
}