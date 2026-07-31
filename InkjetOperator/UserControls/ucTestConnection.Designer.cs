using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    partial class ucTestConnection
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;

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
        private Label lblT1;
        private TextBox txtT1;
        private Label lblT2;
        private TextBox txtT2;
        private Label lblT3;
        private TextBox txtT3;
        private Label lblT4;
        private TextBox txtT4;
        private Label lblT5;
        private TextBox txtT5;
        private Button btnWrite;
        private Label lblDbResult;

        private GroupBox grpSocket;
        private Label lblIp;
        private TextBox txtIp;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblProg;
        private TextBox txtProgram;
        private Label lblDocFolder;
        private TextBox txtDocFolder;
        private Button btnBrowseDoc;
        private Button btnKey85;
        private Button btnKey84;
        private Button btnKey83;
        private Label lblLog;
        private TextBox txtSocketLog;

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
            lblPath = new Label();
            txtDbPath = new TextBox();
            btnBrowse = new Button();
            lblTable = new Label();
            cmbTable = new ComboBox();
            lblLot = new Label();
            txtLot = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblT1 = new Label();
            txtT1 = new TextBox();
            lblT2 = new Label();
            txtT2 = new TextBox();
            lblT3 = new Label();
            txtT3 = new TextBox();
            lblT4 = new Label();
            txtT4 = new TextBox();
            lblT5 = new Label();
            txtT5 = new TextBox();
            btnWrite = new Button();
            lblDbResult = new Label();
            grpSocket = new GroupBox();
            lblIp = new Label();
            txtIp = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            lblProg = new Label();
            txtProgram = new TextBox();
            lblDocFolder = new Label();
            txtDocFolder = new TextBox();
            btnBrowseDoc = new Button();
            btnKey85 = new Button();
            btnKey84 = new Button();
            btnKey83 = new Button();
            lblLog = new Label();
            txtSocketLog = new TextBox();
            grpSql = new GroupBox();
            lblSql = new Label();
            txtSql = new TextBox();
            btnRunSql = new Button();
            txtSqlResult = new TextBox();
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
            lblTitle.Size = new Size(490, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "UV Connection Test (หน้างาน)";
            // 
            // grpDb
            // 
            grpDb.Controls.Add(lblPath);
            grpDb.Controls.Add(txtDbPath);
            grpDb.Controls.Add(btnBrowse);
            grpDb.Controls.Add(lblTable);
            grpDb.Controls.Add(cmbTable);
            grpDb.Controls.Add(lblLot);
            grpDb.Controls.Add(txtLot);
            grpDb.Controls.Add(lblName);
            grpDb.Controls.Add(txtName);
            grpDb.Controls.Add(lblT1);
            grpDb.Controls.Add(txtT1);
            grpDb.Controls.Add(lblT2);
            grpDb.Controls.Add(txtT2);
            grpDb.Controls.Add(lblT3);
            grpDb.Controls.Add(txtT3);
            grpDb.Controls.Add(lblT4);
            grpDb.Controls.Add(txtT4);
            grpDb.Controls.Add(lblT5);
            grpDb.Controls.Add(txtT5);
            grpDb.Controls.Add(btnWrite);
            grpDb.Controls.Add(lblDbResult);
            grpDb.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpDb.Location = new Point(20, 60);
            grpDb.Name = "grpDb";
            grpDb.Size = new Size(560, 470);
            grpDb.TabIndex = 1;
            grpDb.TabStop = false;
            grpDb.Text = "1) DB3 Test — เขียน CPI.db3 (MK063/MK067)";
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.Font = new Font("Segoe UI", 10F);
            lblPath.Location = new Point(18, 43);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(88, 28);
            lblPath.TabIndex = 0;
            lblPath.Text = "CPI.db3 :";
            // 
            // txtDbPath
            // 
            txtDbPath.Font = new Font("Segoe UI", 10F);
            txtDbPath.Location = new Point(110, 37);
            txtDbPath.Name = "txtDbPath";
            txtDbPath.Size = new Size(330, 34);
            txtDbPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Font = new Font("Segoe UI", 10F);
            btnBrowse.Location = new Point(450, 36);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(90, 29);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse";
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblTable
            // 
            lblTable.AutoSize = true;
            lblTable.Font = new Font("Segoe UI", 10F);
            lblTable.Location = new Point(18, 83);
            lblTable.Name = "lblTable";
            lblTable.Size = new Size(66, 28);
            lblTable.TabIndex = 3;
            lblTable.Text = "Table :";
            // 
            // cmbTable
            // 
            cmbTable.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTable.Font = new Font("Segoe UI", 10F);
            cmbTable.Location = new Point(110, 77);
            cmbTable.Name = "cmbTable";
            cmbTable.Size = new Size(140, 36);
            cmbTable.TabIndex = 4;
            // 
            // lblLot
            // 
            lblLot.AutoSize = true;
            lblLot.Font = new Font("Segoe UI", 10F);
            lblLot.Location = new Point(18, 125);
            lblLot.Name = "lblLot";
            lblLot.Size = new Size(45, 28);
            lblLot.TabIndex = 5;
            lblLot.Text = "lot :";
            // 
            // txtLot
            // 
            txtLot.Font = new Font("Segoe UI", 10F);
            txtLot.Location = new Point(110, 119);
            txtLot.Name = "txtLot";
            txtLot.Size = new Size(430, 34);
            txtLot.TabIndex = 6;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 10F);
            lblName.Location = new Point(18, 162);
            lblName.Name = "lblName";
            lblName.Size = new Size(69, 28);
            lblName.TabIndex = 7;
            lblName.Text = "name :";
            // 
            // txtName
            // 
            txtName.Font = new Font("Segoe UI", 10F);
            txtName.Location = new Point(110, 156);
            txtName.Name = "txtName";
            txtName.Size = new Size(430, 34);
            txtName.TabIndex = 8;
            // 
            // lblT1
            // 
            lblT1.AutoSize = true;
            lblT1.Font = new Font("Segoe UI", 10F);
            lblT1.Location = new Point(18, 199);
            lblT1.Name = "lblT1";
            lblT1.Size = new Size(65, 28);
            lblT1.TabIndex = 9;
            lblT1.Text = "text1 :";
            // 
            // txtT1
            // 
            txtT1.Font = new Font("Segoe UI", 10F);
            txtT1.Location = new Point(110, 193);
            txtT1.Name = "txtT1";
            txtT1.Size = new Size(430, 34);
            txtT1.TabIndex = 10;
            // 
            // lblT2
            // 
            lblT2.AutoSize = true;
            lblT2.Font = new Font("Segoe UI", 10F);
            lblT2.Location = new Point(18, 236);
            lblT2.Name = "lblT2";
            lblT2.Size = new Size(65, 28);
            lblT2.TabIndex = 11;
            lblT2.Text = "text2 :";
            // 
            // txtT2
            // 
            txtT2.Font = new Font("Segoe UI", 10F);
            txtT2.Location = new Point(110, 230);
            txtT2.Name = "txtT2";
            txtT2.Size = new Size(430, 34);
            txtT2.TabIndex = 12;
            // 
            // lblT3
            // 
            lblT3.AutoSize = true;
            lblT3.Font = new Font("Segoe UI", 10F);
            lblT3.Location = new Point(18, 273);
            lblT3.Name = "lblT3";
            lblT3.Size = new Size(65, 28);
            lblT3.TabIndex = 13;
            lblT3.Text = "text3 :";
            // 
            // txtT3
            // 
            txtT3.Font = new Font("Segoe UI", 10F);
            txtT3.Location = new Point(110, 267);
            txtT3.Name = "txtT3";
            txtT3.Size = new Size(430, 34);
            txtT3.TabIndex = 14;
            // 
            // lblT4
            // 
            lblT4.AutoSize = true;
            lblT4.Font = new Font("Segoe UI", 10F);
            lblT4.Location = new Point(18, 310);
            lblT4.Name = "lblT4";
            lblT4.Size = new Size(65, 28);
            lblT4.TabIndex = 15;
            lblT4.Text = "text4 :";
            // 
            // txtT4
            // 
            txtT4.Font = new Font("Segoe UI", 10F);
            txtT4.Location = new Point(110, 304);
            txtT4.Name = "txtT4";
            txtT4.Size = new Size(430, 34);
            txtT4.TabIndex = 16;
            // 
            // lblT5
            // 
            lblT5.AutoSize = true;
            lblT5.Font = new Font("Segoe UI", 10F);
            lblT5.Location = new Point(18, 347);
            lblT5.Name = "lblT5";
            lblT5.Size = new Size(65, 28);
            lblT5.TabIndex = 17;
            lblT5.Text = "text5 :";
            // 
            // txtT5
            // 
            txtT5.Font = new Font("Segoe UI", 10F);
            txtT5.Location = new Point(110, 341);
            txtT5.Name = "txtT5";
            txtT5.Size = new Size(430, 34);
            txtT5.TabIndex = 18;
            // 
            // btnWrite
            // 
            btnWrite.BackColor = Color.FromArgb(150, 190, 120);
            btnWrite.FlatStyle = FlatStyle.Flat;
            btnWrite.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnWrite.ForeColor = Color.White;
            btnWrite.Location = new Point(110, 385);
            btnWrite.Name = "btnWrite";
            btnWrite.Size = new Size(430, 40);
            btnWrite.TabIndex = 19;
            btnWrite.Text = "เขียนลง CPI.db3";
            btnWrite.UseVisualStyleBackColor = false;
            btnWrite.Click += btnWrite_Click;
            // 
            // lblDbResult
            // 
            lblDbResult.Font = new Font("Segoe UI", 9.5F);
            lblDbResult.ForeColor = Color.DimGray;
            lblDbResult.Location = new Point(18, 435);
            lblDbResult.Name = "lblDbResult";
            lblDbResult.Size = new Size(524, 28);
            lblDbResult.TabIndex = 20;
            lblDbResult.Text = "—";
            // 
            // grpSocket
            // 
            grpSocket.Controls.Add(lblIp);
            grpSocket.Controls.Add(txtIp);
            grpSocket.Controls.Add(lblPort);
            grpSocket.Controls.Add(txtPort);
            grpSocket.Controls.Add(lblProg);
            grpSocket.Controls.Add(txtProgram);
            grpSocket.Controls.Add(lblDocFolder);
            grpSocket.Controls.Add(txtDocFolder);
            grpSocket.Controls.Add(btnBrowseDoc);
            grpSocket.Controls.Add(btnKey85);
            grpSocket.Controls.Add(btnKey84);
            grpSocket.Controls.Add(btnKey83);
            grpSocket.Controls.Add(lblLog);
            grpSocket.Controls.Add(txtSocketLog);
            grpSocket.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpSocket.Location = new Point(600, 60);
            grpSocket.Name = "grpSocket";
            grpSocket.Size = new Size(540, 470);
            grpSocket.TabIndex = 2;
            grpSocket.TabStop = false;
            grpSocket.Text = "2) Socket Test — TCP KEY :10086";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.Font = new Font("Segoe UI", 10F);
            lblIp.Location = new Point(18, 47);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(37, 28);
            lblIp.TabIndex = 0;
            lblIp.Text = "IP :";
            // 
            // txtIp
            // 
            txtIp.Font = new Font("Segoe UI", 10F);
            txtIp.Location = new Point(60, 41);
            txtIp.Name = "txtIp";
            txtIp.Size = new Size(150, 34);
            txtIp.TabIndex = 1;
            txtIp.Text = "127.0.0.1";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Segoe UI", 10F);
            lblPort.Location = new Point(225, 47);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(57, 28);
            lblPort.TabIndex = 2;
            lblPort.Text = "Port :";
            // 
            // txtPort
            // 
            txtPort.Font = new Font("Segoe UI", 10F);
            txtPort.Location = new Point(285, 41);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(90, 34);
            txtPort.TabIndex = 3;
            txtPort.Text = "10086";
            // 
            // lblProg
            // 
            lblProg.AutoSize = true;
            lblProg.Font = new Font("Segoe UI", 10F);
            lblProg.Location = new Point(18, 88);
            lblProg.Name = "lblProg";
            lblProg.Size = new Size(67, 28);
            lblProg.TabIndex = 4;
            lblProg.Text = ".uvdx :";
            // 
            // txtProgram
            // 
            txtProgram.Font = new Font("Segoe UI", 10F);
            txtProgram.Location = new Point(90, 82);
            txtProgram.Name = "txtProgram";
            txtProgram.Size = new Size(430, 34);
            txtProgram.TabIndex = 5;
            txtProgram.Text = "compact";
            //
            // lblDocFolder
            //
            lblDocFolder.AutoSize = true;
            lblDocFolder.Font = new Font("Segoe UI", 10F);
            lblDocFolder.Location = new Point(18, 128);
            lblDocFolder.Name = "lblDocFolder";
            lblDocFolder.Size = new Size(110, 28);
            lblDocFolder.TabIndex = 20;
            lblDocFolder.Text = "Document :";
            //
            // txtDocFolder
            //
            txtDocFolder.Font = new Font("Segoe UI", 10F);
            txtDocFolder.Location = new Point(130, 122);
            txtDocFolder.Name = "txtDocFolder";
            txtDocFolder.Size = new Size(310, 34);
            txtDocFolder.TabIndex = 21;
            //
            // btnBrowseDoc
            //
            btnBrowseDoc.Font = new Font("Segoe UI", 9F);
            btnBrowseDoc.Location = new Point(448, 122);
            btnBrowseDoc.Name = "btnBrowseDoc";
            btnBrowseDoc.Size = new Size(72, 34);
            btnBrowseDoc.TabIndex = 22;
            btnBrowseDoc.Text = "Browse";
            btnBrowseDoc.Click += btnBrowseDoc_Click;
            //
            // btnKey85
            //
            btnKey85.BackColor = Color.FromArgb(103, 78, 167);
            btnKey85.FlatStyle = FlatStyle.Flat;
            btnKey85.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnKey85.ForeColor = Color.White;
            btnKey85.Location = new Point(18, 167);
            btnKey85.Name = "btnKey85";
            btnKey85.Size = new Size(180, 40);
            btnKey85.TabIndex = 6;
            btnKey85.Text = "KEY:85 โหลดโปรแกรม";
            btnKey85.UseVisualStyleBackColor = false;
            btnKey85.Click += btnKey85_Click;
            //
            // btnKey84
            //
            btnKey84.BackColor = Color.FromArgb(103, 78, 167);
            btnKey84.FlatStyle = FlatStyle.Flat;
            btnKey84.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnKey84.ForeColor = Color.White;
            btnKey84.Location = new Point(208, 167);
            btnKey84.Name = "btnKey84";
            btnKey84.Size = new Size(150, 40);
            btnKey84.TabIndex = 7;
            btnKey84.Text = "KEY:84 start";
            btnKey84.UseVisualStyleBackColor = false;
            btnKey84.Click += btnKey84_Click;
            //
            // btnKey83
            //
            btnKey83.BackColor = Color.FromArgb(103, 78, 167);
            btnKey83.FlatStyle = FlatStyle.Flat;
            btnKey83.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnKey83.ForeColor = Color.White;
            btnKey83.Location = new Point(368, 167);
            btnKey83.Name = "btnKey83";
            btnKey83.Size = new Size(150, 40);
            btnKey83.TabIndex = 8;
            btnKey83.Text = "KEY:83 stop";
            btnKey83.UseVisualStyleBackColor = false;
            btnKey83.Click += btnKey83_Click;
            //
            // lblLog
            //
            lblLog.AutoSize = true;
            lblLog.Font = new Font("Segoe UI", 10F);
            lblLog.Location = new Point(18, 218);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(103, 28);
            lblLog.TabIndex = 9;
            lblLog.Text = "Response :";
            //
            // txtSocketLog
            //
            txtSocketLog.BackColor = Color.White;
            txtSocketLog.Font = new Font("Consolas", 9.5F);
            txtSocketLog.Location = new Point(18, 245);
            txtSocketLog.Multiline = true;
            txtSocketLog.Name = "txtSocketLog";
            txtSocketLog.ReadOnly = true;
            txtSocketLog.ScrollBars = ScrollBars.Vertical;
            txtSocketLog.Size = new Size(504, 200);
            txtSocketLog.TabIndex = 10;
            // 
            // grpSql
            // 
            grpSql.Controls.Add(lblSql);
            grpSql.Controls.Add(txtSql);
            grpSql.Controls.Add(btnRunSql);
            grpSql.Controls.Add(txtSqlResult);
            grpSql.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grpSql.Location = new Point(20, 536);
            grpSql.Name = "grpSql";
            grpSql.Size = new Size(1120, 413);
            grpSql.TabIndex = 3;
            grpSql.TabStop = false;
            grpSql.Text = "3) Manual SQL — รันคำสั่งเองกับ CPI.db3 (ใช้ไฟล์จากช่อง 1)";
            // 
            // lblSql
            // 
            lblSql.AutoSize = true;
            lblSql.Font = new Font("Segoe UI", 10F);
            lblSql.Location = new Point(18, 37);
            lblSql.Name = "lblSql";
            lblSql.Size = new Size(56, 28);
            lblSql.TabIndex = 0;
            lblSql.Text = "SQL :";
            // 
            // txtSql
            // 
            txtSql.Font = new Font("Consolas", 10F);
            txtSql.Location = new Point(18, 62);
            txtSql.Multiline = true;
            txtSql.Name = "txtSql";
            txtSql.ScrollBars = ScrollBars.Vertical;
            txtSql.Size = new Size(980, 65);
            txtSql.TabIndex = 1;
            txtSql.Text = "SELECT * FROM MK063;";
            // 
            // btnRunSql
            // 
            btnRunSql.BackColor = Color.FromArgb(70, 130, 180);
            btnRunSql.FlatStyle = FlatStyle.Flat;
            btnRunSql.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRunSql.ForeColor = Color.White;
            btnRunSql.Location = new Point(1008, 62);
            btnRunSql.Name = "btnRunSql";
            btnRunSql.Size = new Size(94, 65);
            btnRunSql.TabIndex = 2;
            btnRunSql.Text = "Run SQL";
            btnRunSql.UseVisualStyleBackColor = false;
            btnRunSql.Click += btnRunSql_Click;
            // 
            // txtSqlResult
            // 
            txtSqlResult.BackColor = Color.White;
            txtSqlResult.Font = new Font("Consolas", 9.5F);
            txtSqlResult.Location = new Point(18, 133);
            txtSqlResult.Multiline = true;
            txtSqlResult.Name = "txtSqlResult";
            txtSqlResult.ReadOnly = true;
            txtSqlResult.ScrollBars = ScrollBars.Both;
            txtSqlResult.Size = new Size(1084, 243);
            txtSqlResult.TabIndex = 3;
            txtSqlResult.WordWrap = false;
            // 
            // ucTestConnection
            // 
            Controls.Add(lblTitle);
            Controls.Add(grpDb);
            Controls.Add(grpSocket);
            Controls.Add(grpSql);
            Name = "ucTestConnection";
            Size = new Size(1414, 1091);
            grpDb.ResumeLayout(false);
            grpDb.PerformLayout();
            grpSocket.ResumeLayout(false);
            grpSocket.PerformLayout();
            grpSql.ResumeLayout(false);
            grpSql.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
