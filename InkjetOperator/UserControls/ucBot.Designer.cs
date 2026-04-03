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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            pnlTop = new Panel();
            lblTitle = new Label();
            pnlButtons = new Panel();
            btnRunBot = new Button();
            btnOpenLog = new Button();
            grpProgram = new GroupBox();
            label1 = new Label();
            btnDBUV1 = new Button();
            txtDBUV1 = new TextBox();
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
            bindingSource2 = new BindingSource(components);
            groupBox1 = new GroupBox();
            dataGridView1 = new DataGridView();
            inkjetNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            lotDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            nameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            programNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindingSource1 = new BindingSource(components);
            timer1 = new System.Windows.Forms.Timer(components);
            InkjetName = new DataGridViewTextBoxColumn();
            Lot = new DataGridViewTextBoxColumn();
            Name = new DataGridViewTextBoxColumn();
            ProgramName = new DataGridViewTextBoxColumn();
            pnlTop.SuspendLayout();
            pnlButtons.SuspendLayout();
            grpProgram.SuspendLayout();
            grpXY.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvXY).BeginInit();
            grpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource2).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(90, 110, 140);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1151, 60);
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
            pnlButtons.Size = new Size(1151, 90);
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
            grpProgram.Controls.Add(label1);
            grpProgram.Controls.Add(btnDBUV1);
            grpProgram.Controls.Add(txtDBUV1);
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
            grpProgram.Size = new Size(1151, 164);
            grpProgram.TabIndex = 2;
            grpProgram.TabStop = false;
            grpProgram.Text = "Program Paths";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 114);
            label1.Name = "label1";
            label1.Size = new Size(103, 20);
            label1.TabIndex = 8;
            label1.Text = "Path UV1 DB3:";
            // 
            // btnDBUV1
            // 
            btnDBUV1.Location = new Point(590, 116);
            btnDBUV1.Name = "btnDBUV1";
            btnDBUV1.Size = new Size(100, 25);
            btnDBUV1.TabIndex = 7;
            btnDBUV1.Text = "Browse...";
            btnDBUV1.Click += btnDBUV1_Click;
            // 
            // txtDBUV1
            // 
            txtDBUV1.Location = new Point(180, 114);
            txtDBUV1.Name = "txtDBUV1";
            txtDBUV1.Size = new Size(400, 27);
            txtDBUV1.TabIndex = 6;
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
            grpXY.Location = new Point(0, 314);
            grpXY.Name = "grpXY";
            grpXY.Padding = new Padding(10);
            grpXY.Size = new Size(1151, 220);
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
            dgvXY.Size = new Size(1131, 180);
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
            grpLog.Location = new Point(596, 534);
            grpLog.Name = "grpLog";
            grpLog.Padding = new Padding(10);
            grpLog.Size = new Size(545, 222);
            grpLog.TabIndex = 0;
            grpLog.TabStop = false;
            grpLog.Text = "Success Log";
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AutoGenerateColumns = false;
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
            dgvLog.Columns.AddRange(new DataGridViewColumn[] { InkjetName, Lot, Name, ProgramName });
            dgvLog.DataSource = bindingSource2;
            dgvLog.Dock = DockStyle.Fill;
            dgvLog.EnableHeadersVisualStyles = false;
            dgvLog.Location = new Point(10, 30);
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersWidth = 51;
            dgvLog.Size = new Size(525, 182);
            dgvLog.TabIndex = 0;
            // 
            // bindingSource2
            // 
            bindingSource2.DataSource = typeof(Models.UVinkjet);
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dataGridView1);
            groupBox1.Location = new Point(3, 534);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(10);
            groupBox1.Size = new Size(587, 222);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Current job";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.LightGray;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.ColumnHeadersHeight = 29;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { inkjetNameDataGridViewTextBoxColumn, lotDataGridViewTextBoxColumn, nameDataGridViewTextBoxColumn, programNameDataGridViewTextBoxColumn });
            dataGridView1.DataSource = bindingSource1;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Location = new Point(10, 30);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(567, 182);
            dataGridView1.TabIndex = 0;
            // 
            // inkjetNameDataGridViewTextBoxColumn
            // 
            inkjetNameDataGridViewTextBoxColumn.DataPropertyName = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.HeaderText = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            inkjetNameDataGridViewTextBoxColumn.Name = "inkjetNameDataGridViewTextBoxColumn";
            inkjetNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // lotDataGridViewTextBoxColumn
            // 
            lotDataGridViewTextBoxColumn.DataPropertyName = "Lot";
            lotDataGridViewTextBoxColumn.HeaderText = "Lot";
            lotDataGridViewTextBoxColumn.MinimumWidth = 6;
            lotDataGridViewTextBoxColumn.Name = "lotDataGridViewTextBoxColumn";
            lotDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            nameDataGridViewTextBoxColumn.HeaderText = "Name";
            nameDataGridViewTextBoxColumn.MinimumWidth = 6;
            nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // programNameDataGridViewTextBoxColumn
            // 
            programNameDataGridViewTextBoxColumn.DataPropertyName = "ProgramName";
            programNameDataGridViewTextBoxColumn.HeaderText = "ProgramName";
            programNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            programNameDataGridViewTextBoxColumn.Name = "programNameDataGridViewTextBoxColumn";
            programNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bindingSource1
            // 
            bindingSource1.DataSource = typeof(Models.UVinkjet);
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // InkjetName
            // 
            InkjetName.DataPropertyName = "InkjetName";
            InkjetName.HeaderText = "InkjetName";
            InkjetName.MinimumWidth = 6;
            InkjetName.Name = "InkjetName";
            InkjetName.ReadOnly = true;
            // 
            // Lot
            // 
            Lot.DataPropertyName = "Lot";
            Lot.HeaderText = "Lot";
            Lot.MinimumWidth = 6;
            Lot.Name = "Lot";
            Lot.ReadOnly = true;
            // 
            // Name
            // 
            Name.DataPropertyName = "Name";
            Name.HeaderText = "Name";
            Name.MinimumWidth = 6;
            Name.Name = "Name";
            Name.ReadOnly = true;
            // 
            // ProgramName
            // 
            ProgramName.DataPropertyName = "ProgramName";
            ProgramName.HeaderText = "ProgramName";
            ProgramName.MinimumWidth = 6;
            ProgramName.Name = "ProgramName";
            ProgramName.ReadOnly = true;
            // 
            // ucBot
            // 
            Controls.Add(groupBox1);
            Controls.Add(grpLog);
            Controls.Add(grpXY);
            Controls.Add(grpProgram);
            Controls.Add(pnlButtons);
            Controls.Add(pnlTop);
            Name = "ucBot";
            Size = new Size(1151, 759);
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlButtons.ResumeLayout(false);
            grpProgram.ResumeLayout(false);
            grpProgram.PerformLayout();
            grpXY.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvXY).EndInit();
            grpLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource2).EndInit();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
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
        private Label label1;
        private Button btnDBUV1;
        private TextBox txtDBUV1;
        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private BindingSource bindingSource1;
        private System.Windows.Forms.Timer timer1;
        private BindingSource bindingSource2;
        private DataGridViewTextBoxColumn inkjetNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn lotDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn programNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn InkjetName;
        private DataGridViewTextBoxColumn Lot;
        private DataGridViewTextBoxColumn Name;
        private DataGridViewTextBoxColumn ProgramName;
    }
}