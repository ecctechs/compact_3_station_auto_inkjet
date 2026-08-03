using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucTestConnection : UserControl
    {
        private readonly SqliteDataService _sqlite = new SqliteDataService();
        private readonly UvTcpService _tcp = new UvTcpService();

        public ucTestConnection()
        {
            InitializeComponent();
            cmbTable.Items.AddRange(new object[] { "MK063", "MK067" });
            cmbTable.SelectedIndex = 0;
        }

        // ── DB3 ──
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*",
                Title = "เลือกไฟล์ CPI.db3"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtDbPath.Text = dlg.FileName;
        }

        private async void btnWrite_Click(object sender, EventArgs e)
        {
            var data = new UvJobData
            {
                Lot = txtLot.Text.Trim(),
                Name = txtName.Text.Trim(),
                Text1 = txtT1.Text,
                Text2 = txtT2.Text,
                Text3 = txtT3.Text,
                Text4 = txtT4.Text,
                Text5 = txtT5.Text,
            };
            string table = cmbTable.SelectedItem?.ToString() ?? "MK063";

            btnWrite.Enabled = false;
            try
            {
                var (ok, msg) = await _sqlite.WriteUvToCpiAsync(txtDbPath.Text.Trim(), table, data);
                lblDbResult.Text = msg;
                lblDbResult.ForeColor = ok ? Color.Green : Color.Red;
            }
            finally
            {
                btnWrite.Enabled = true;
            }
        }

        private int GetUvNumber()
        {
            string table = cmbTable.SelectedItem?.ToString() ?? "MK063";
            return table == "MK067" ? 2 : 1;
        }

        // ── Socket ──
        private async void btnKey85_Click(object sender, EventArgs e)
        {
            string program = txtProgram.Text.Trim();
            if (string.IsNullOrEmpty(program))
            {
                AppendLog("กรุณาใส่ชื่อโปรแกรม");
                return;
            }

            int uvNum = GetUvNumber();
            string? docFolder = ucSettingUV.GetDocumentFolder(uvNum);

            if (string.IsNullOrEmpty(docFolder))
            {
                MessageBox.Show(
                    $"ยังไม่ได้ตั้งค่าโฟลเดอร์ซอฟต์แวร์ UV{uvNum}\nกรุณาไปตั้งค่าในหน้า UV Setting ก่อน",
                    "ไม่พบ Document folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppendLog($"ไม่พบ document folder สำหรับ UV{uvNum} — ตั้งค่าในหน้า UV Setting");
                return;
            }

            string? cpiPath = ucSettingUV.GetCpiPath(uvNum);
            if (cpiPath == null)
            {
                MessageBox.Show(
                    $"ไม่พบ CPI.db3 สำหรับ UV{uvNum}\nกรุณาตรวจสอบโฟลเดอร์ซอฟต์แวร์ในหน้า UV Setting",
                    "ไม่พบ CPI.db3", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppendLog($"ไม่พบ CPI.db3 สำหรับ UV{uvNum}");
                return;
            }

            string defaultUvdx = Path.Combine(docFolder, "default.uvdx");
            if (!File.Exists(defaultUvdx))
            {
                MessageBox.Show(
                    $"ไม่พบ default.uvdx ในโฟลเดอร์:\n{docFolder}\n\nกรุณาเพิ่มไฟล์ default.uvdx ก่อน",
                    "ไม่พบ default.uvdx", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppendLog("ไม่พบ default.uvdx");
                return;
            }

            string? resolved = ResolveProgram(program, docFolder);
            if (resolved == null) return;

            var (_, log) = await _tcp.SendLoadAsync(txtIp.Text.Trim(), Port(), resolved);
            AppendLog(log);
        }

        private string? ResolveProgram(string program, string docFolder)
        {
            string baseName = program.EndsWith(".uvdx", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileNameWithoutExtension(program)
                : program;

            var allUvdx = Directory.GetFiles(docFolder, "*.uvdx")
                .Select(f => Path.GetFileNameWithoutExtension(f)!)
                .Where(name => name.StartsWith(baseName, StringComparison.Ordinal))
                .OrderBy(n => n)
                .ToList();

            if (allUvdx.Count == 1)
            {
                AppendLog($"พบ 1 โปรแกรม: {allUvdx[0]}.uvdx");
                return allUvdx[0];
            }

            if (allUvdx.Count > 1)
            {
                string? chosen = PromptVariant(baseName, allUvdx);
                if (chosen == null)
                {
                    AppendLog("ยกเลิกการเลือก");
                    return null;
                }
                AppendLog($"เลือก {chosen}");
                return chosen;
            }

            if (File.Exists(Path.Combine(docFolder, "default.uvdx")))
            {
                MessageBox.Show(
                    $"ไม่พบโปรแกรม \"{baseName}\" ในโฟลเดอร์ document\nจะใช้ default.uvdx ไปก่อน\n\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                    "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppendLog($"ไม่พบ {baseName}.uvdx — ใช้ default.uvdx แทน");
                return "default";
            }

            MessageBox.Show(
                $"ไม่พบโปรแกรม \"{baseName}\" และไม่มี default.uvdx\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppendLog($"ไม่พบ {baseName}.uvdx และไม่มี default.uvdx");
            return null;
        }

        private string? PromptVariant(string baseName, List<string> variants)
        {
            using var frm = new Form
            {
                Text = "เลือกโปรแกรมย่อย",
                Size = new Size(360, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label
            {
                Text = $"โปรแกรม \"{baseName}\" มีหลายตัวเลือก :",
                Location = new Point(12, 12),
                AutoSize = true
            };

            var lst = new ListBox
            {
                Location = new Point(12, 40),
                Size = new Size(320, 120)
            };
            foreach (var v in variants)
                lst.Items.Add(v + ".uvdx");
            lst.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "ตกลง",
                DialogResult = DialogResult.OK,
                Location = new Point(170, 175),
                Size = new Size(80, 32)
            };
            var btnCancel = new Button
            {
                Text = "ยกเลิก",
                DialogResult = DialogResult.Cancel,
                Location = new Point(256, 175),
                Size = new Size(80, 32)
            };

            frm.Controls.AddRange(new Control[] { lbl, lst, btnOk, btnCancel });
            frm.AcceptButton = btnOk;
            frm.CancelButton = btnCancel;

            if (frm.ShowDialog(this.ParentForm) != DialogResult.OK || lst.SelectedIndex < 0)
                return null;

            return variants[lst.SelectedIndex];
        }

        private async void btnKey84_Click(object sender, EventArgs e)
        {
            var (_, log) = await _tcp.SendStartAsync(txtIp.Text.Trim(), Port());
            AppendLog(log);
        }

        private async void btnKey83_Click(object sender, EventArgs e)
        {
            var (_, log) = await _tcp.SendStopAsync(txtIp.Text.Trim(), Port());
            AppendLog(log);
        }

        // ── Manual SQL ──
        private async void btnRunSql_Click(object sender, EventArgs e)
        {
            btnRunSql.Enabled = false;
            try
            {
                txtSqlResult.Text = await _sqlite.RunSqlAsync(txtDbPath.Text.Trim(), txtSql.Text);
            }
            finally
            {
                btnRunSql.Enabled = true;
            }
        }

        private int Port() => int.TryParse(txtPort.Text.Trim(), out int p) && p > 0 ? p : 10086;

        private void AppendLog(string line)
        {
            txtSocketLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        }
    }
}
