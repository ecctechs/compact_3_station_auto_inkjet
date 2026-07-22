using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    partial class ucTestConnection
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;

        // DB3
        private GroupBox grpDb;
        private Label lblPath;
        private TextBox txtDbPath;
        private Button btnBrowse;
        private Label lblTable;
        private ComboBox cmbTable;
        private Label lblLot;
        private TextBox txtLot;
        private Label lblName;
        private TextBox txtName;
        private Label lblT1; private TextBox txtT1;
        private Label lblT2; private TextBox txtT2;
        private Label lblT3; private TextBox txtT3;
        private Label lblT4; private TextBox txtT4;
        private Label lblT5; private TextBox txtT5;
        private Button btnWrite;
        private Label lblDbResult;

        // Socket
        private GroupBox grpSocket;
        private Label lblIp;
        private TextBox txtIp;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblProg;
        private TextBox txtProgram;
        private Button btnKey85;
        private Button btnKey84;
        private Button btnKey83;
        private Label lblLog;
        private TextBox txtSocketLog;

        // Manual SQL
        private GroupBox grpSql;
        private Label lblSql;
        private TextBox txtSql;
        private Button btnRunSql;
        private TextBox txtSqlResult;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            grpDb = new GroupBox();
            lblPath = new Label(); txtDbPath = new TextBox(); btnBrowse = new Button();
            lblTable = new Label(); cmbTable = new ComboBox();
            lblLot = new Label(); txtLot = new TextBox();
            lblName = new Label(); txtName = new TextBox();
            lblT1 = new Label(); txtT1 = new TextBox();
            lblT2 = new Label(); txtT2 = new TextBox();
            lblT3 = new Label(); txtT3 = new TextBox();
            lblT4 = new Label(); txtT4 = new TextBox();
            lblT5 = new Label(); txtT5 = new TextBox();
            btnWrite = new Button(); lblDbResult = new Label();
            grpSocket = new GroupBox();
            lblIp = new Label(); txtIp = new TextBox();
            lblPort = new Label(); txtPort = new TextBox();
            lblProg = new Label(); txtProgram = new TextBox();
            btnKey85 = new Button(); btnKey84 = new Button(); btnKey83 = new Button();
            lblLog = new Label(); txtSocketLog = new TextBox();
            grpSql = new GroupBox(); lblSql = new Label(); txtSql = new TextBox();
            btnRunSql = new Button(); txtSqlResult = new TextBox();
            grpDb.SuspendLayout();
            grpSocket.SuspendLayout();
            grpSql.SuspendLayout();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Text = "UV Connection Test (หน้างาน)";
            //
            // grpDb
            //
            grpDb.Controls.Add(lblPath); grpDb.Controls.Add(txtDbPath); grpDb.Controls.Add(btnBrowse);
            grpDb.Controls.Add(lblTable); grpDb.Controls.Add(cmbTable);
            grpDb.Controls.Add(lblLot); grpDb.Controls.Add(txtLot);
            grpDb.Controls.Add(lblName); grpDb.Controls.Add(txtName);
            grpDb.Controls.Add(lblT1); grpDb.Controls.Add(txtT1);
            grpDb.Controls.Add(lblT2); grpDb.Controls.Add(txtT2);
            grpDb.Controls.Add(lblT3); grpDb.Controls.Add(txtT3);
            grpDb.Controls.Add(lblT4); grpDb.Controls.Add(txtT4);
            grpDb.Controls.Add(lblT5); grpDb.Controls.Add(txtT5);
            grpDb.Controls.Add(btnWrite); grpDb.Controls.Add(lblDbResult);
            grpDb.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpDb.Location = new Point(20, 60);
            grpDb.Name = "grpDb";
            grpDb.Size = new Size(560, 470);
            grpDb.TabStop = false;
            grpDb.Text = "1) DB3 Test — เขียน CPI.db3 (MK063/MK067)";
            //
            NormalField(lblPath, "CPI.db3 :", 18, 40);
            txtDbPath.Location = new Point(110, 37); txtDbPath.Size = new Size(330, 27); txtDbPath.Font = FieldFont();
            btnBrowse.Location = new Point(450, 36); btnBrowse.Size = new Size(90, 29); btnBrowse.Text = "Browse"; btnBrowse.Font = FieldFont(); btnBrowse.Click += btnBrowse_Click;
            NormalField(lblTable, "Table :", 18, 80);
            cmbTable.Location = new Point(110, 77); cmbTable.Size = new Size(140, 28); cmbTable.Font = FieldFont(); cmbTable.DropDownStyle = ComboBoxStyle.DropDownList;
            NormalField(lblLot, "lot :", 18, 122); Field(txtLot, 110, 119);
            NormalField(lblName, "name :", 18, 159); Field(txtName, 110, 156);
            NormalField(lblT1, "text1 :", 18, 196); Field(txtT1, 110, 193);
            NormalField(lblT2, "text2 :", 18, 233); Field(txtT2, 110, 230);
            NormalField(lblT3, "text3 :", 18, 270); Field(txtT3, 110, 267);
            NormalField(lblT4, "text4 :", 18, 307); Field(txtT4, 110, 304);
            NormalField(lblT5, "text5 :", 18, 344); Field(txtT5, 110, 341);
            btnWrite.Location = new Point(110, 385); btnWrite.Size = new Size(430, 40);
            btnWrite.BackColor = Color.FromArgb(150, 190, 120); btnWrite.ForeColor = Color.White;
            btnWrite.FlatStyle = FlatStyle.Flat; btnWrite.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnWrite.Text = "เขียนลง CPI.db3"; btnWrite.UseVisualStyleBackColor = false; btnWrite.Click += btnWrite_Click;
            lblDbResult.Location = new Point(18, 435); lblDbResult.Size = new Size(524, 28);
            lblDbResult.Font = new Font("Segoe UI", 9.5F); lblDbResult.ForeColor = Color.DimGray; lblDbResult.Text = "—";
            //
            // grpSocket
            //
            grpSocket.Controls.Add(lblIp); grpSocket.Controls.Add(txtIp);
            grpSocket.Controls.Add(lblPort); grpSocket.Controls.Add(txtPort);
            grpSocket.Controls.Add(lblProg); grpSocket.Controls.Add(txtProgram);
            grpSocket.Controls.Add(btnKey85); grpSocket.Controls.Add(btnKey84); grpSocket.Controls.Add(btnKey83);
            grpSocket.Controls.Add(lblLog); grpSocket.Controls.Add(txtSocketLog);
            grpSocket.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpSocket.Location = new Point(600, 60);
            grpSocket.Name = "grpSocket";
            grpSocket.Size = new Size(540, 470);
            grpSocket.TabStop = false;
            grpSocket.Text = "2) Socket Test — TCP KEY :10086";
            //
            NormalField(lblIp, "IP :", 18, 44); txtIp.Location = new Point(60, 41); txtIp.Size = new Size(150, 27); txtIp.Font = FieldFont(); txtIp.Text = "127.0.0.1";
            NormalField(lblPort, "Port :", 225, 44); txtPort.Location = new Point(285, 41); txtPort.Size = new Size(90, 27); txtPort.Font = FieldFont(); txtPort.Text = "10086";
            NormalField(lblProg, ".uvdx :", 18, 85); txtProgram.Location = new Point(90, 82); txtProgram.Size = new Size(430, 27); txtProgram.Font = FieldFont(); txtProgram.Text = "compact";
            btnKey85.Location = new Point(18, 122); btnKey85.Size = new Size(180, 40); btnKey85.Text = "KEY:85 โหลดโปรแกรม";
            btnKey84.Location = new Point(208, 122); btnKey84.Size = new Size(150, 40); btnKey84.Text = "KEY:84 start";
            btnKey83.Location = new Point(368, 122); btnKey83.Size = new Size(150, 40); btnKey83.Text = "KEY:83 stop";
            foreach (var b in new[] { btnKey85, btnKey84, btnKey83 })
            {
                b.BackColor = Color.FromArgb(103, 78, 167); b.ForeColor = Color.White;
                b.FlatStyle = FlatStyle.Flat; b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold); b.UseVisualStyleBackColor = false;
            }
            btnKey85.Click += btnKey85_Click; btnKey84.Click += btnKey84_Click; btnKey83.Click += btnKey83_Click;
            NormalField(lblLog, "Response :", 18, 178);
            txtSocketLog.Location = new Point(18, 205); txtSocketLog.Size = new Size(504, 240);
            txtSocketLog.Font = new Font("Consolas", 9.5F); txtSocketLog.Multiline = true; txtSocketLog.ReadOnly = true;
            txtSocketLog.ScrollBars = ScrollBars.Vertical; txtSocketLog.BackColor = Color.White;
            //
            // grpSql
            //
            grpSql.Controls.Add(lblSql);
            grpSql.Controls.Add(txtSql);
            grpSql.Controls.Add(btnRunSql);
            grpSql.Controls.Add(txtSqlResult);
            grpSql.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpSql.Location = new Point(20, 545);
            grpSql.Name = "grpSql";
            grpSql.Size = new Size(1120, 235);
            grpSql.TabStop = false;
            grpSql.Text = "3) Manual SQL — รันคำสั่งเองกับ CPI.db3 (ใช้ไฟล์จากช่อง 1)";
            //
            NormalField(lblSql, "SQL :", 18, 34);
            txtSql.Location = new Point(18, 62); txtSql.Size = new Size(980, 65);
            txtSql.Font = new Font("Consolas", 10F); txtSql.Multiline = true; txtSql.ScrollBars = ScrollBars.Vertical;
            txtSql.Text = "SELECT * FROM MK063;";
            btnRunSql.Location = new Point(1008, 62); btnRunSql.Size = new Size(94, 65);
            btnRunSql.BackColor = Color.FromArgb(70, 130, 180); btnRunSql.ForeColor = Color.White;
            btnRunSql.FlatStyle = FlatStyle.Flat; btnRunSql.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRunSql.Text = "Run SQL"; btnRunSql.UseVisualStyleBackColor = false; btnRunSql.Click += btnRunSql_Click;
            txtSqlResult.Location = new Point(18, 140); txtSqlResult.Size = new Size(1084, 80);
            txtSqlResult.Font = new Font("Consolas", 9.5F); txtSqlResult.Multiline = true; txtSqlResult.ReadOnly = true;
            txtSqlResult.ScrollBars = ScrollBars.Both; txtSqlResult.WordWrap = false; txtSqlResult.BackColor = Color.White;
            //
            // ucTestConnection
            //
            Controls.Add(lblTitle);
            Controls.Add(grpDb);
            Controls.Add(grpSocket);
            Controls.Add(grpSql);
            Name = "ucTestConnection";
            Size = new Size(1152, 800);
            grpDb.ResumeLayout(false); grpDb.PerformLayout();
            grpSocket.ResumeLayout(false); grpSocket.PerformLayout();
            grpSql.ResumeLayout(false); grpSql.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // helper จัด label/textbox ให้สั้นลง
        private static Font FieldFont() => new Font("Segoe UI", 10F);
        private void NormalField(Label l, string text, int x, int y)
        {
            l.AutoSize = true; l.Font = FieldFont(); l.Location = new Point(x, y + 3); l.Text = text;
        }
        private void Field(TextBox t, int x, int y)
        {
            t.Location = new Point(x, y); t.Size = new Size(430, 27); t.Font = FieldFont();
        }
    }
}
