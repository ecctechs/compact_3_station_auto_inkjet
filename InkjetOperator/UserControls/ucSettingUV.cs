using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSettingUV : UserControl
    {
        private const string KEY_UV1_IP = "UV001_IP";
        private const string KEY_UV1_PORT = "UV001_PORT";
        private const string KEY_UV1_FOLDER = "UV1_FOLDER";
        private const string TABLE_UV1 = "MK063";

        private const string KEY_UV2_IP = "UV002_IP";
        private const string KEY_UV2_PORT = "UV002_PORT";
        private const string KEY_UV2_FOLDER = "UV2_FOLDER";
        private const string TABLE_UV2 = "MK067";

        private const string REL_CPI = @"database\sys\CPI.db3";
        private const string REL_DEFAULT_UVDX = @"document\default.uvdx";
        private const string REL_DOCUMENT = "document";

        public ucSettingUV()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            txtUv1Ip.Text = CustomSettingsManager.GetValue(KEY_UV1_IP) ?? "";
            txtUv1Port.Text = CustomSettingsManager.GetValue(KEY_UV1_PORT) ?? "";
            txtUv2Ip.Text = CustomSettingsManager.GetValue(KEY_UV2_IP) ?? "";
            txtUv2Port.Text = CustomSettingsManager.GetValue(KEY_UV2_PORT) ?? "";

            txtUv1Folder.Text = UvSettingsManager.GetValue(KEY_UV1_FOLDER) ?? "";
            txtUv2Folder.Text = UvSettingsManager.GetValue(KEY_UV2_FOLDER) ?? "";

            ValidateAndShowStatus(txtUv1Folder.Text, lblUv1Status, TABLE_UV1);
            ValidateAndShowStatus(txtUv2Folder.Text, lblUv2Status, TABLE_UV2);
        }

        private void btnBrowse1_Click(object sender, EventArgs e) => BrowseFolder(txtUv1Folder, lblUv1Status, TABLE_UV1);
        private void btnBrowse2_Click(object sender, EventArgs e) => BrowseFolder(txtUv2Folder, lblUv2Status, TABLE_UV2);

        private void BrowseFolder(TextBox target, Label statusLabel, string requiredTable)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "เลือกโฟลเดอร์ซอฟต์แวร์ UV Inkjet (เช่น uvinkjet-250702-new)",
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
                dlg.SelectedPath = target.Text;

            if (dlg.ShowDialog() != DialogResult.OK) return;

            target.Text = dlg.SelectedPath;
            var warnings = ValidateFolder(dlg.SelectedPath, requiredTable);
            ValidateAndShowStatus(dlg.SelectedPath, statusLabel, requiredTable);

            if (warnings.Count > 0)
            {
                MessageBox.Show(
                    string.Join("\n\n", warnings),
                    "ตรวจพบปัญหา", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private System.Collections.Generic.List<string> ValidateFolder(string folder, string requiredTable)
        {
            var warnings = new System.Collections.Generic.List<string>();
            if (string.IsNullOrEmpty(folder)) return warnings;

            string cpiPath = Path.Combine(folder, REL_CPI);
            string defaultUvdx = Path.Combine(folder, REL_DEFAULT_UVDX);

            if (!File.Exists(cpiPath))
                warnings.Add($"ไม่พบ CPI.db3:\n{cpiPath}");
            else if (!HasTable(cpiPath, requiredTable))
                warnings.Add($"CPI.db3 ไม่มีตาราง '{requiredTable}':\n{cpiPath}");

            if (!File.Exists(defaultUvdx))
                warnings.Add($"ไม่พบ default.uvdx:\n{defaultUvdx}\n\nกรุณาเพิ่มไฟล์ default.uvdx ในโฟลเดอร์ document");

            return warnings;
        }

        private void ValidateAndShowStatus(string folder, Label statusLabel, string requiredTable)
        {
            if (string.IsNullOrEmpty(folder))
            {
                statusLabel.Text = "";
                return;
            }

            var warnings = ValidateFolder(folder, requiredTable);
            if (warnings.Count == 0)
            {
                statusLabel.Text = "✓ CPI.db3 + default.uvdx พร้อมใช้งาน";
                statusLabel.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                statusLabel.Text = $"⚠ พบ {warnings.Count} ปัญหา";
                statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string f1 = txtUv1Folder.Text.Trim();
            string f2 = txtUv2Folder.Text.Trim();

            if (!string.IsNullOrEmpty(f1) && !Directory.Exists(f1))
            {
                MessageBox.Show($"[UV1] ไม่พบโฟลเดอร์:\n{f1}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(f2) && !Directory.Exists(f2))
            {
                MessageBox.Show($"[UV2] ไม่พบโฟลเดอร์:\n{f2}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CustomSettingsManager.SetValue(KEY_UV1_IP, txtUv1Ip.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV1_PORT, txtUv1Port.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV2_IP, txtUv2Ip.Text.Trim());
            CustomSettingsManager.SetValue(KEY_UV2_PORT, txtUv2Port.Text.Trim());

            UvSettingsManager.SetValue(KEY_UV1_FOLDER, f1);
            UvSettingsManager.SetValue(KEY_UV2_FOLDER, f2);

            // backward compat: เก็บ CPI path แบบเดิมด้วยเพื่อให้ส่วนอื่นยังอ่านได้
            if (!string.IsNullOrEmpty(f1))
                UvSettingsManager.SetValue("UV1DB3_PATH", Path.Combine(f1, REL_CPI));
            if (!string.IsNullOrEmpty(f2))
                UvSettingsManager.SetValue("UV1DB3_PATH_2", Path.Combine(f2, REL_CPI));

            MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            catch { return false; }
        }

        private void btnCancel_Click(object sender, EventArgs e) => LoadData();

        // ── static helpers สำหรับหน้าอื่นเรียกใช้ ──

        public static string? GetCpiPath(int uvNumber)
        {
            string key = uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER";
            string? folder = UvSettingsManager.GetValue(key);
            if (string.IsNullOrEmpty(folder)) return null;
            string path = Path.Combine(folder, REL_CPI);
            return File.Exists(path) ? path : null;
        }

        public static string? GetDocumentFolder(int uvNumber)
        {
            string key = uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER";
            string? folder = UvSettingsManager.GetValue(key);
            if (string.IsNullOrEmpty(folder)) return null;
            string docPath = Path.Combine(folder, REL_DOCUMENT);
            return Directory.Exists(docPath) ? docPath : null;
        }
    }
}
