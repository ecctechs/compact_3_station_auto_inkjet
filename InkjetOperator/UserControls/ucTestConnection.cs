using System;
using System.Drawing;
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

        // ── Socket ──
        private async void btnKey85_Click(object sender, EventArgs e)
        {
            var (_, log) = await _tcp.SendLoadAsync(txtIp.Text.Trim(), Port(), txtProgram.Text.Trim());
            AppendLog(log);
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
