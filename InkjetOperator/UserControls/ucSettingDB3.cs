using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSettingDB3 : UserControl
    {
        public ucSettingDB3()
        {
            InitializeComponent();
            LoadData();
        }

        // ================= LOAD =================
        private void LoadData()
        {
            txtDbPath.Text = CustomSettingsManager.GetValue("DB_PATH") ?? "";
        }

        // ================= BROWSE =================
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*";
                dlg.Title = "Select Database File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtDbPath.Text = dlg.FileName;
                }
            }
        }

        // ================= SAVE =================
        private void btnSave_Click(object sender, EventArgs e)
        {
            string dbPath = txtDbPath.Text.Trim();

            if (string.IsNullOrEmpty(dbPath))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ Database", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(dbPath))
            {
                MessageBox.Show($"ไม่พบไฟล์:\n{dbPath}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!HasInkjetDataTable(dbPath))
            {
                MessageBox.Show(
                    $"ไฟล์ Database ไม่มีตาราง 'inkjet_data'\n\n" +
                    $"ไฟล์: {dbPath}\n\n" +
                    "กรุณาเลือกไฟล์ที่มีตาราง inkjet_data",
                    "ตาราง inkjet_data ไม่พบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CustomSettingsManager.SetValue("DB_PATH", dbPath);
            MessageBox.Show("บันทึกเรียบร้อย", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool HasInkjetDataTable(string dbPath)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                conn.Open();
                using var cmd = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='inkjet_data';", conn);
                return cmd.ExecuteScalar() != null;
            }
            catch
            {
                return false;
            }
        }

        // ================= CANCEL =================
        private void btnCancel_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {

        }
    }
}