using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    partial class ucTestPLC
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            _plc?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlMain = new Panel();
            pnlConnection = new Panel();
            lblTitle = new Label();
            lblIp = new Label();
            txtIp = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            btnConnect = new Button();
            lblStatus = new Label();
            lblMapTitle = new Label();
            btnReadAll = new Button();
            btnWriteAll = new Button();
            btnAddRow = new Button();
            dgvRegisters = new DataGridView();
            colAddrStart = new DataGridViewTextBoxColumn();
            colAddrStop = new DataGridViewTextBoxColumn();
            colPlcStart = new DataGridViewTextBoxColumn();
            colPlcStop = new DataGridViewTextBoxColumn();
            colListName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewComboBoxColumn();
            colBit = new DataGridViewComboBoxColumn();
            colValue = new DataGridViewTextBoxColumn();
            colRead = new DataGridViewButtonColumn();
            colWrite = new DataGridViewButtonColumn();
            colDelete = new DataGridViewButtonColumn();
            lblLogTitle = new Label();
            txtLog = new TextBox();

            pnlMain.SuspendLayout();
            pnlConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRegisters).BeginInit();
            SuspendLayout();
            //
            // pnlMain
            //
            pnlMain.BackColor = Color.White;
            pnlMain.Controls.Add(pnlConnection);
            pnlMain.Controls.Add(lblMapTitle);
            pnlMain.Controls.Add(btnReadAll);
            pnlMain.Controls.Add(btnWriteAll);
            pnlMain.Controls.Add(btnAddRow);
            pnlMain.Controls.Add(dgvRegisters);
            pnlMain.Controls.Add(lblLogTitle);
            pnlMain.Controls.Add(txtLog);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(29, 20, 29, 20);
            pnlMain.Size = new Size(1200, 950);
            pnlMain.TabIndex = 0;
            //
            // pnlConnection
            //
            pnlConnection.BackColor = Color.White;
            pnlConnection.BorderStyle = BorderStyle.FixedSingle;
            pnlConnection.Controls.Add(lblTitle);
            pnlConnection.Controls.Add(lblIp);
            pnlConnection.Controls.Add(txtIp);
            pnlConnection.Controls.Add(lblPort);
            pnlConnection.Controls.Add(txtPort);
            pnlConnection.Controls.Add(btnConnect);
            pnlConnection.Controls.Add(lblStatus);
            pnlConnection.Location = new Point(85, 25);
            pnlConnection.Name = "pnlConnection";
            pnlConnection.Size = new Size(700, 100);
            pnlConnection.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.Location = new Point(14, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 38);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "PLC Test — Modbus TCP";
            //
            // lblIp
            //
            lblIp.AutoSize = true;
            lblIp.Font = new Font("Segoe UI", 10F);
            lblIp.Location = new Point(14, 60);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(30, 25);
            lblIp.TabIndex = 1;
            lblIp.Text = "IP :";
            //
            // txtIp
            //
            txtIp.Font = new Font("Segoe UI", 10F);
            txtIp.Location = new Point(50, 56);
            txtIp.Name = "txtIp";
            txtIp.Size = new Size(180, 31);
            txtIp.TabIndex = 2;
            txtIp.Text = "10.10.100.100";
            //
            // lblPort
            //
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Segoe UI", 10F);
            lblPort.Location = new Point(245, 60);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(50, 25);
            lblPort.TabIndex = 3;
            lblPort.Text = "Port :";
            //
            // txtPort
            //
            txtPort.Font = new Font("Segoe UI", 10F);
            txtPort.Location = new Point(300, 56);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(80, 31);
            txtPort.TabIndex = 4;
            txtPort.Text = "5012";
            //
            // btnConnect
            //
            btnConnect.BackColor = Color.FromArgb(70, 130, 180);
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(400, 52);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(110, 38);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = false;
            //
            // lblStatus
            //
            lblStatus.BackColor = Color.Gray;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.Location = new Point(525, 60);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(24, 24);
            lblStatus.TabIndex = 6;
            //
            // lblMapTitle
            //
            lblMapTitle.AutoSize = true;
            lblMapTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblMapTitle.Location = new Point(85, 140);
            lblMapTitle.Name = "lblMapTitle";
            lblMapTitle.Size = new Size(200, 30);
            lblMapTitle.TabIndex = 1;
            lblMapTitle.Text = "Register Map";
            //
            // btnReadAll
            //
            btnReadAll.BackColor = Color.FromArgb(46, 139, 87);
            btnReadAll.FlatAppearance.BorderSize = 0;
            btnReadAll.FlatStyle = FlatStyle.Flat;
            btnReadAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnReadAll.ForeColor = Color.White;
            btnReadAll.Location = new Point(650, 135);
            btnReadAll.Name = "btnReadAll";
            btnReadAll.Size = new Size(110, 38);
            btnReadAll.TabIndex = 2;
            btnReadAll.Text = "Read All";
            btnReadAll.UseVisualStyleBackColor = false;
            //
            // btnWriteAll
            //
            btnWriteAll.BackColor = Color.FromArgb(103, 78, 167);
            btnWriteAll.FlatAppearance.BorderSize = 0;
            btnWriteAll.FlatStyle = FlatStyle.Flat;
            btnWriteAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnWriteAll.ForeColor = Color.White;
            btnWriteAll.Location = new Point(770, 135);
            btnWriteAll.Name = "btnWriteAll";
            btnWriteAll.Size = new Size(110, 38);
            btnWriteAll.TabIndex = 3;
            btnWriteAll.Text = "Write All";
            btnWriteAll.UseVisualStyleBackColor = false;
            //
            // btnAddRow
            //
            btnAddRow.BackColor = Color.FromArgb(70, 130, 180);
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.FlatStyle = FlatStyle.Flat;
            btnAddRow.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddRow.ForeColor = Color.White;
            btnAddRow.Location = new Point(890, 135);
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new Size(110, 38);
            btnAddRow.TabIndex = 4;
            btnAddRow.Text = "+ Add Row";
            btnAddRow.UseVisualStyleBackColor = false;
            //
            // dgvRegisters
            //
            dgvRegisters.AllowUserToAddRows = false;
            dgvRegisters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRegisters.BackgroundColor = Color.White;
            dgvRegisters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRegisters.Columns.AddRange(new DataGridViewColumn[] {
                colAddrStart, colAddrStop, colPlcStart, colPlcStop,
                colListName, colType, colBit, colValue, colRead, colWrite, colDelete
            });
            dgvRegisters.Location = new Point(85, 185);
            dgvRegisters.Name = "dgvRegisters";
            dgvRegisters.RowHeadersWidth = 30;
            dgvRegisters.RowTemplate.Height = 32;
            dgvRegisters.Size = new Size(915, 480);
            dgvRegisters.TabIndex = 5;
            //
            // colAddrStart
            //
            colAddrStart.HeaderText = "Addr Start";
            colAddrStart.Name = "colAddrStart";
            colAddrStart.FillWeight = 70F;
            //
            // colAddrStop
            //
            colAddrStop.HeaderText = "Addr Stop";
            colAddrStop.Name = "colAddrStop";
            colAddrStop.FillWeight = 70F;
            //
            // colPlcStart
            //
            colPlcStart.HeaderText = "PLC Start";
            colPlcStart.Name = "colPlcStart";
            colPlcStart.FillWeight = 80F;
            //
            // colPlcStop
            //
            colPlcStop.HeaderText = "PLC Stop";
            colPlcStop.Name = "colPlcStop";
            colPlcStop.FillWeight = 80F;
            //
            // colListName
            //
            colListName.HeaderText = "List";
            colListName.Name = "colListName";
            colListName.FillWeight = 120F;
            //
            // colType
            //
            colType.HeaderText = "Type";
            colType.Name = "colType";
            colType.Items.AddRange(new object[] { "String", "Int" });
            colType.FillWeight = 60F;
            //
            // colBit
            //
            colBit.HeaderText = "Bit";
            colBit.Name = "colBit";
            colBit.Items.AddRange(new object[] { 32, 64 });
            colBit.FillWeight = 50F;
            //
            // colValue
            //
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            colValue.FillWeight = 100F;
            //
            // colRead
            //
            colRead.HeaderText = "Read";
            colRead.Name = "colRead";
            colRead.Text = "R";
            colRead.UseColumnTextForButtonValue = true;
            colRead.FillWeight = 35F;
            //
            // colWrite
            //
            colWrite.HeaderText = "Write";
            colWrite.Name = "colWrite";
            colWrite.Text = "W";
            colWrite.UseColumnTextForButtonValue = true;
            colWrite.FillWeight = 35F;
            //
            // colDelete
            //
            colDelete.HeaderText = "Del";
            colDelete.Name = "colDelete";
            colDelete.Text = "X";
            colDelete.UseColumnTextForButtonValue = true;
            colDelete.FillWeight = 35F;
            //
            // lblLogTitle
            //
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLogTitle.Location = new Point(85, 680);
            lblLogTitle.Name = "lblLogTitle";
            lblLogTitle.Size = new Size(50, 25);
            lblLogTitle.TabIndex = 6;
            lblLogTitle.Text = "Log";
            //
            // txtLog
            //
            txtLog.BackColor = Color.White;
            txtLog.Font = new Font("Consolas", 9.5F);
            txtLog.Location = new Point(85, 710);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(915, 180);
            txtLog.TabIndex = 7;
            //
            // ucTestPLC
            //
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(pnlMain);
            Name = "ucTestPLC";
            Size = new Size(1200, 950);
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlConnection.ResumeLayout(false);
            pnlConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRegisters).EndInit();
            ResumeLayout(false);
        }

        private Panel pnlMain;
        private Panel pnlConnection;
        private Label lblTitle;
        private Label lblIp;
        private TextBox txtIp;
        private Label lblPort;
        private TextBox txtPort;
        private Button btnConnect;
        private Label lblStatus;
        private Label lblMapTitle;
        private Button btnReadAll;
        private Button btnWriteAll;
        private Button btnAddRow;
        private DataGridView dgvRegisters;
        private DataGridViewTextBoxColumn colAddrStart;
        private DataGridViewTextBoxColumn colAddrStop;
        private DataGridViewTextBoxColumn colPlcStart;
        private DataGridViewTextBoxColumn colPlcStop;
        private DataGridViewTextBoxColumn colListName;
        private DataGridViewComboBoxColumn colType;
        private DataGridViewComboBoxColumn colBit;
        private DataGridViewTextBoxColumn colValue;
        private DataGridViewButtonColumn colRead;
        private DataGridViewButtonColumn colWrite;
        private DataGridViewButtonColumn colDelete;
        private Label lblLogTitle;
        private TextBox txtLog;
    }
}
