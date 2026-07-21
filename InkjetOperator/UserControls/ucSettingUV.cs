using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSettingUV : UserControl
    {
        // ── UV1 → ตาราง MK063 ──
        private const string KEY_UV1_IP = "UV001_IP";
        private const string KEY_UV1_PORT = "UV001_PORT";
        private const string KEY_UV1_DB = "UV1DB3_PATH";
        private const string TABLE_UV1 = "MK063";

        // ── UV2 → ตาราง MK067 ──
        private const string KEY_UV2_IP = "UV002_IP";
        private const string KEY_UV2_PORT = "UV002_PORT";
        private const string KEY_UV2_DB = "UV1DB3_PATH_2";
        private const string TABLE_UV2 = "MK067";

        public ucSettingUV()
        {
            InitializeComponent();
            LoadData();
        }

        // ================= LOAD =================
        private void LoadData()
        {
            // IP/Port เก็บใน CustomSettings (ชุดเดียวกับหน้า IP Address Setting เดิม)
            txtUv1Ip.Text = CustomSettingsManager.GetValue(KEY_UV1_IP) ?? "";
            txtUv1Port.Text = CustomSettingsManager.GetValue(KEY_UV1_PORT) ?? "";
            txtUv2Ip.Text = CustomSettingsManager.GetValue(KEY_UV2_IP) ?? "";
            txtUv2Port.Text = CustomSettingsManager.GetValue(KEY_UV2_PORT) ?? "";

            // path CPI.db3 เก็บใน UvSettings (ชุดเดียวกับที่ SqliteDataService อ่าน)
            txtUv1Db.Text = UvSettingsManager.GetValue(KEY_UV1_DB) ?? "";
            txtUv2Db.Text = UvSettingsManager.GetValue(KEY_UV2_DB) ?? "";
        }

        // ================= BROWSE =================
        private void btnBrowse1_Click(object sender, EventArgs e) => BrowseInto(txtUv1Db);

        private void btnBrowse2_Click(object sender, EventArgs e) => BrowseInto(txtUv2Db);

        /// <summary>เปิด dialog เลือกไฟล์ .db3 (พิมพ์ UNC \\ip\share ได้) แล้วใส่ผลลง TextBox ที่ระบุ</summary>
        private void BrowseInto(TextBox target)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*";
                dlg.Title = "เลือกไฟล์ CPI Database (.db3)";

                if (!string.IsNullOrEmpty(target.Text))
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(target.Text);
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                            dlg.InitialDirectory = dir;
                    }
                    catch { /* path แปลก ๆ ก็ปล่อยให้เปิด default */ }
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                    target.Text = dlg.FileName;
            }
        }

        // ================= SAVE =================
        private void btnSave_Click(object sender, EventArgs e)
        {
            string db1 = txtUv1Db.Text.Trim();
            string db2 = txtUv2Db.Text.Trim();

            // validate เฉพาะช่อง CPI.db3 ที่กรอก (เว้นว่างได้ถ้าเครื่องนั้นไม่ใช้)
            if (!ValidateDbPath(db1, TABLE_UV1, "UV1")) return;
            if (!ValidateDbPath(db2, TABLE_UV2, "UV2")) return;

            // IP/Port
            CustomSettingsManager.SetValue(KEY_UV1_IP, txtUv1Ip.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV1_PORT, txtUv1Port.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV2_IP, txtUv2Ip.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV2_PORT, txtUv2Port.Text.Trim());

            // CPI.db3 path
            UvSettingsManager.SetValue(KEY_UV1_DB, db1);
            UvSettingsManager.SetValue(KEY_UV2_DB, db2);

            MessageBox.Show("บันทึกเรียบร้อย", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>ตรวจไฟล์ CPI.db3 ที่กรอก: ต้องมีอยู่จริง + มีตารางที่ต้องการ (ช่องว่าง = ผ่าน)</summary>
        private bool ValidateDbPath(string path, string requiredTable, string label)
        {
            if (string.IsNullOrEmpty(path)) return true;

            if (!File.Exists(path))
            {
                MessageBox.Show($"[{label}] ไม่พบไฟล์:\n{path}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!HasTable(path, requiredTable))
            {
                MessageBox.Show(
                    $"[{label}] ไฟล์ Database ไม่มีตาราง '{requiredTable}'\n\n" +
                    $"ไฟล์: {path}\n\n" +
                    $"กรุณาเลือกไฟล์ CPI.db3 ที่มีตาราง {requiredTable}",
                    $"ตาราง {requiredTable} ไม่พบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool HasTable(string dbPath, string tableName)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                conn.Open();
                using var cmd = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name=@t;", conn);
                cmd.Parameters.AddWithValue("@t", tableName);
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
    }
}
