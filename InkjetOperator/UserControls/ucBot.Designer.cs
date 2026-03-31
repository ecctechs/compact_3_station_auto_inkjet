namespace InkjetOperator.UserControls
{
    partial class ucBot
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Designer

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            pnlTop = new Panel();
            lblTitle = new Label();
            pnlButtons = new Panel();
            btnRunBot = new Button();
            btnOpenLog = new Button();
            grpProgram = new GroupBox();
            lblMain = new Label();
            txtMain = new TextBox();
            btnBrowseMain = new Button();
            lblBackup = new Label();
            txtBackup = new TextBox();
            btnBrowseBackup = new Button();
            grpXY = new GroupBox();
            dgvXY = new DataGridView();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            grpLog = new GroupBox();
            dgvLog = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            pnlTop.SuspendLayout();
            pnlButtons.SuspendLayout();
            grpProgram.SuspendLayout();
            grpXY.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvXY).BeginInit();
            grpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(90, 110, 140);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(900, 60);
            pnlTop.TabIndex = 4;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(168, 37);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "UV BotClick";
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnRunBot);
            pnlButtons.Controls.Add(btnOpenLog);
            pnlButtons.Dock = DockStyle.Top;
            pnlButtons.Location = new Point(0, 60);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(20);
            pnlButtons.Size = new Size(900, 90);
            pnlButtons.TabIndex = 3;
            // 
            // btnRunBot
            // 
            btnRunBot.BackColor = Color.FromArgb(40, 160, 80);
            btnRunBot.FlatAppearance.BorderSize = 0;
            btnRunBot.FlatStyle = FlatStyle.Flat;
            btnRunBot.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnRunBot.ForeColor = Color.White;
            btnRunBot.Location = new Point(20, 15);
            btnRunBot.Name = "btnRunBot";
            btnRunBot.Size = new Size(300, 55);
            btnRunBot.TabIndex = 0;
            btnRunBot.Text = "RUN BOT";
            btnRunBot.UseVisualStyleBackColor = false;
            btnRunBot.Click += btnRunBot_Click_1;
            // 
            // btnOpenLog
            // 
            btnOpenLog.BackColor = Color.FromArgb(200, 200, 200);
            btnOpenLog.FlatStyle = FlatStyle.Flat;
            btnOpenLog.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnOpenLog.Location = new Point(340, 15);
            btnOpenLog.Name = "btnOpenLog";
            btnOpenLog.Size = new Size(250, 55);
            btnOpenLog.TabIndex = 1;
            btnOpenLog.Text = "📁 Open Log Folder";
            btnOpenLog.UseVisualStyleBackColor = false;
            btnOpenLog.Click += btnOpenLog_Click;
            // 
            // grpProgram
            // 
            grpProgram.Controls.Add(lblMain);
            grpProgram.Controls.Add(txtMain);
            grpProgram.Controls.Add(btnBrowseMain);
            grpProgram.Controls.Add(lblBackup);
            grpProgram.Controls.Add(txtBackup);
            grpProgram.Controls.Add(btnBrowseBackup);
            grpProgram.Dock = DockStyle.Top;
            grpProgram.Location = new Point(0, 150);
            grpProgram.Name = "grpProgram";
            grpProgram.Padding = new Padding(15);
            grpProgram.Size = new Size(900, 130);
            grpProgram.TabIndex = 2;
            grpProgram.TabStop = false;
            grpProgram.Text = "Program Paths";
            // 
            // lblMain
            // 
            lblMain.AutoSize = true;
            lblMain.Location = new Point(20, 30);
            lblMain.Name = "lblMain";
            lblMain.Size = new Size(138, 20);
            lblMain.TabIndex = 0;
            lblMain.Text = "Main Program Path:";
            // 
            // txtMain
            // 
            txtMain.Location = new Point(180, 27);
            txtMain.Name = "txtMain";
            txtMain.Size = new Size(400, 27);
            txtMain.TabIndex = 1;
            // 
            // btnBrowseMain
            // 
            btnBrowseMain.Location = new Point(590, 26);
            btnBrowseMain.Name = "btnBrowseMain";
            btnBrowseMain.Size = new Size(100, 25);
            btnBrowseMain.TabIndex = 2;
            btnBrowseMain.Text = "Browse...";
            btnBrowseMain.Click += btnBrowseMain_Click;
            // 
            // lblBackup
            // 
            lblBackup.AutoSize = true;
            lblBackup.Location = new Point(20, 70);
            lblBackup.Name = "lblBackup";
            lblBackup.Size = new Size(153, 20);
            lblBackup.TabIndex = 3;
            lblBackup.Text = "Backup Program Path:";
            // 
            // txtBackup
            // 
            txtBackup.Location = new Point(180, 67);
            txtBackup.Name = "txtBackup";
            txtBackup.Size = new Size(400, 27);
            txtBackup.TabIndex = 4;
            // 
            // btnBrowseBackup
            // 
            btnBrowseBackup.Location = new Point(590, 66);
            btnBrowseBackup.Name = "btnBrowseBackup";
            btnBrowseBackup.Size = new Size(100, 25);
            btnBrowseBackup.TabIndex = 5;
            btnBrowseBackup.Text = "Browse...";
            btnBrowseBackup.Click += btnBrowseBackup_Click;
            // 
            // grpXY
            // 
            grpXY.Controls.Add(dgvXY);
            grpXY.Dock = DockStyle.Top;
            grpXY.Location = new Point(0, 280);
            grpXY.Name = "grpXY";
            grpXY.Padding = new Padding(10);
            grpXY.Size = new Size(900, 220);
            grpXY.TabIndex = 1;
            grpXY.TabStop = false;
            grpXY.Text = "Bot Click Coordinates";
            // 
            // dgvXY
            // 
            dgvXY.AllowUserToAddRows = false;
            dgvXY.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.LightGray;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvXY.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvXY.ColumnHeadersHeight = 29;
            dgvXY.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6 });
            dgvXY.Dock = DockStyle.Fill;
            dgvXY.EnableHeadersVisualStyles = false;
            dgvXY.Location = new Point(10, 30);
            dgvXY.Name = "dgvXY";
            dgvXY.RowHeadersVisible = false;
            dgvXY.RowHeadersWidth = 51;
            dgvXY.Size = new Size(880, 180);
            dgvXY.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Step";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.HeaderText = "X";
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.HeaderText = "Y";
            dataGridViewTextBoxColumn6.MinimumWidth = 6;
            dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // grpLog
            // 
            grpLog.Controls.Add(dgvLog);
            grpLog.Dock = DockStyle.Fill;
            grpLog.Location = new Point(0, 500);
            grpLog.Name = "grpLog";
            grpLog.Padding = new Padding(10);
            grpLog.Size = new Size(900, 150);
            grpLog.TabIndex = 0;
            grpLog.TabStop = false;
            grpLog.Text = "Success Log";
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.LightGray;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvLog.ColumnHeadersHeight = 29;
            dgvLog.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3 });
            dgvLog.Dock = DockStyle.Fill;
            dgvLog.EnableHeadersVisualStyles = false;
            dgvLog.Location = new Point(10, 30);
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersWidth = 51;
            dgvLog.Size = new Size(880, 110);
            dgvLog.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Time";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "File";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Status";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // ucBot
            // 
            Controls.Add(grpLog);
            Controls.Add(grpXY);
            Controls.Add(grpProgram);
            Controls.Add(pnlButtons);
            Controls.Add(pnlTop);
            Name = "ucBot";
            Size = new Size(900, 650);
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlButtons.ResumeLayout(false);
            grpProgram.ResumeLayout(false);
            grpProgram.PerformLayout();
            grpXY.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvXY).EndInit();
            grpLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // controls
        private Panel pnlTop;
        private Label lblTitle;

        private Panel pnlButtons;
        private Button btnRunBot;
        private Button btnOpenLog;

        private GroupBox grpProgram;
        private Label lblMain;
        private TextBox txtMain;
        private Button btnBrowseMain;
        private Label lblBackup;
        private TextBox txtBackup;
        private Button btnBrowseBackup;

        private GroupBox grpXY;
        private DataGridView dgvXY;

        private GroupBox grpLog;
        private DataGridView dgvLog;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    }
}