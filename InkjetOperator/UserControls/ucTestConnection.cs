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
    /// <summary>
    /// หน้าเทสหน้างาน (MenuMode 99) — พิสูจน์การเชื่อมต่อเครื่อง UV 2 ทาง:
    /// (1) DB3 = เขียน CPI.db3 (MK063/MK067)  (2) Socket = TCP KEY :10086
    /// self-contained: browse ไฟล์ + ใส่ IP เองในหน้านี้ ไม่ต้องพึ่งหน้าอื่น
    /// </summary>
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

        private void btnBrowseDoc_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "เลือกโฟลเดอร์ document ที่เก็บไฟล์ .uvdx",
                ShowNewFolderButton = false
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtDocFolder.Text = dlg.SelectedPath;
        }

        // ── Socket ──
        private async void btnKey85_Click(object sender, EventArgs e)
        {
            string program = txtProgram.Text.Trim();
            if (string.IsNullOrEmpty(program))
            {
                AppendLog("⚠ กรุณาใส่ชื่อโปรแกรม");
                return;
            }

            string resolved = ResolveProgram(program);
            if (resolved == null) return;

            var (_, log) = await _tcp.SendLoadAsync(txtIp.Text.Trim(), Port(), resolved);
            AppendLog(log);
        }

        private string ResolveProgram(string program)
        {
            string docFolder = txtDocFolder.Text.Trim();
            if (string.IsNullOrEmpty(docFolder) || !Directory.Exists(docFolder))
            {
                AppendLog("⚠ ไม่ได้ระบุ Document folder หรือโฟลเดอร์ไม่มี — ส่งชื่อตรงๆ");
                return program;
            }

            string baseName = program.EndsWith(".uvdx", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileNameWithoutExtension(program)
                : program;

            string exactFile = Path.Combine(docFolder, baseName + ".uvdx");
            if (File.Exists(exactFile))
            {
                AppendLog($"✔ พบ {baseName}.uvdx");
                return baseName;
            }

            var variants = Directory.GetFiles(docFolder, baseName + "-*.uvdx")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .OrderBy(n => n)
                .ToList();

            if (variants.Count > 0)
            {
                string chosen = PromptVariant(baseName, variants);
                if (chosen == null)
                {
                    AppendLog("⚠ ยกเลิกการเลือก");
                    return null;
                }
                AppendLog($"✔ เลือก {chosen}.uvdx");
                return chosen;
            }

            string defaultFile = Path.Combine(docFolder, "default.uvdx");
            if (File.Exists(defaultFile))
            {
                MessageBox.Show(
                    $"ไม่พบโปรแกรม \"{baseName}\" ในโฟลเดอร์ document\nจะใช้ default.uvdx ไปก่อน\n\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                    "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppendLog($"⚠ ไม่พบ {baseName}.uvdx — ใช้ default.uvdx แทน");
                return "default";
            }

            MessageBox.Show(
                $"ไม่พบโปรแกรม \"{baseName}\" และไม่มี default.uvdx ในโฟลเดอร์ document\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppendLog($"✖ ไม่พบ {baseName}.uvdx และไม่มี default.uvdx");
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
