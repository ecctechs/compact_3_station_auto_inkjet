using System;
using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator.UserControls
{
    public partial class ucBot : UserControl
    {
        public ucBot()
        {
            InitializeComponent();
            InitGrid();
            LoadConfig();
        }

        // ================= INIT GRID =================
        private void InitGrid()
        {
            dgvXY.Rows.Clear();

            dgvXY.Rows.Add("Document", "0", "0");
            dgvXY.Rows.Add("Open", "0", "0");
            dgvXY.Rows.Add("SelectFile", "0", "0");
            dgvXY.Rows.Add("OpenBtn", "0", "0");
        }

        // ================= LOAD =================
        private void LoadConfig()
        {
            txtMain.Text = UvSettingsManager.GetValue("MAIN_PATH") ?? "";
            txtBackup.Text = UvSettingsManager.GetValue("BACKUP_PATH") ?? "";

            dgvXY.Rows[0].Cells[1].Value = UvSettingsManager.GetValue("DOC_X") ?? "0";
            dgvXY.Rows[0].Cells[2].Value = UvSettingsManager.GetValue("DOC_Y") ?? "0";

            dgvXY.Rows[1].Cells[1].Value = UvSettingsManager.GetValue("OPEN_X") ?? "0";
            dgvXY.Rows[1].Cells[2].Value = UvSettingsManager.GetValue("OPEN_Y") ?? "0";

            dgvXY.Rows[2].Cells[1].Value = UvSettingsManager.GetValue("SELECT_X") ?? "0";
            dgvXY.Rows[2].Cells[2].Value = UvSettingsManager.GetValue("SELECT_Y") ?? "0";

            dgvXY.Rows[3].Cells[1].Value = UvSettingsManager.GetValue("OPENBTN_X") ?? "0";
            dgvXY.Rows[3].Cells[2].Value = UvSettingsManager.GetValue("OPENBTN_Y") ?? "0";
        }

        // ================= SAVE =================
        private void SaveConfig()
        {
            UvSettingsManager.SetValue("MAIN_PATH", txtMain.Text);
            UvSettingsManager.SetValue("BACKUP_PATH", txtBackup.Text);

            UvSettingsManager.SetValue("DOC_X", dgvXY.Rows[0].Cells[1].Value?.ToString() ?? "0");
            UvSettingsManager.SetValue("DOC_Y", dgvXY.Rows[0].Cells[2].Value?.ToString() ?? "0");

            UvSettingsManager.SetValue("OPEN_X", dgvXY.Rows[1].Cells[1].Value?.ToString() ?? "0");
            UvSettingsManager.SetValue("OPEN_Y", dgvXY.Rows[1].Cells[2].Value?.ToString() ?? "0");

            UvSettingsManager.SetValue("SELECT_X", dgvXY.Rows[2].Cells[1].Value?.ToString() ?? "0");
            UvSettingsManager.SetValue("SELECT_Y", dgvXY.Rows[2].Cells[2].Value?.ToString() ?? "0");

            UvSettingsManager.SetValue("OPENBTN_X", dgvXY.Rows[3].Cells[1].Value?.ToString() ?? "0");
            UvSettingsManager.SetValue("OPENBTN_Y", dgvXY.Rows[3].Cells[2].Value?.ToString() ?? "0");

            MessageBox.Show("Save เรียบร้อย");
        }


        // ================= VALIDATE =================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMain.Text))
            {
                MessageBox.Show("กรุณาใส่ Main Path");
                return false;
            }

            // validate X/Y ทุก row
            for (int i = 0; i < dgvXY.Rows.Count; i++)
            {
                var x = dgvXY.Rows[i].Cells[1].Value?.ToString();
                var y = dgvXY.Rows[i].Cells[2].Value?.ToString();

                if (!int.TryParse(x, out _) || !int.TryParse(y, out _))
                {
                    MessageBox.Show($"Step {dgvXY.Rows[i].Cells[0].Value} ค่า X/Y ไม่ถูกต้อง");
                    return false;
                }
            }

            return true;
        }

        // ================= GET STEP =================
        private BotStep GetStepFromGrid(int index)
        {
            var row = dgvXY.Rows[index];

            return new BotStep
            {
                Name = row.Cells[0].Value?.ToString() ?? "",
                X = int.Parse(row.Cells[1].Value?.ToString() ?? "0"),
                Y = int.Parse(row.Cells[2].Value?.ToString() ?? "0"),
                VerifyArea = GetVerifyArea(index)
            };
        }

        private Rectangle GetVerifyArea(int index)
        {
            return index switch
            {
                0 => new Rectangle(2100, 0, 400, 300),
                1 => new Rectangle(2100, 100, 400, 400),
                2 => new Rectangle(800, 400, 800, 600),
                3 => new Rectangle(1500, 800, 500, 300),
                _ => Rectangle.Empty
            };
        }

        private void btnBrowseMain_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "เลือกโฟลเดอร์ Main Program";

                if (!string.IsNullOrEmpty(txtMain.Text) && Directory.Exists(txtMain.Text))
                {
                    dialog.SelectedPath = txtMain.Text;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtMain.Text = dialog.SelectedPath;

                    // ✅ Save ลง config
                    UvSettingsManager.SetValue("MAIN_PATH", txtMain.Text);
                }
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "เลือกโฟลเดอร์ Backup Program";

                if (!string.IsNullOrEmpty(txtBackup.Text) && Directory.Exists(txtBackup.Text))
                {
                    dialog.SelectedPath = txtBackup.Text;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackup.Text = dialog.SelectedPath;

                    // ✅ Save ลง config
                    UvSettingsManager.SetValue("BACKUP_PATH", txtBackup.Text);
                }
            }
        }

        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            string logPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "capture_log"
            );

            // ถ้า folder ยังไม่มี → สร้างให้เลย
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // เปิด folder ด้วย Windows Explorer
            System.Diagnostics.Process.Start("explorer.exe", logPath);
        }


        private void btnRunBot_Click_1(object sender, EventArgs e)
        {
            SaveConfig();

            if (!ValidateInput()) return;

            string fileName = "SPK-LOT.uvdx";

            var steps = new[]
            {
                GetStepFromGrid(0),
                GetStepFromGrid(1),
                GetStepFromGrid(2),
                GetStepFromGrid(3)
            };

            // copy file
            BotClickHelper.CopyFile(fileName);

            // run bot
            BotClickHelper.RunAsync("uvinkjet", steps, result =>
            {
                BotClickHelper.ClearDocumentFolder();

                if (this.IsHandleCreated)
                    this.Invoke((MethodInvoker)(() =>
                        MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error)));
            });
        }
    }
}