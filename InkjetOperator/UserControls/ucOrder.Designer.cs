namespace InkjetOperator
{
    partial class ucOrder
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControl;
        private TabPage tabList;
        private TabPage tabHistory;
        private DataGridView dgvList;
        private DataGridView dgvHistory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl = new TabControl();
            tabList = new TabPage();
            dgvList = new DataGridView();
            orderNoDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            customerNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            typeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            qtyDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            statusDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindingSource1 = new BindingSource(components);
            tabHistory = new TabPage();
            dgvHistory = new DataGridView();
            timerPoll = new System.Windows.Forms.Timer(components);
            pnlJobs = new Panel();
            btnRefresh = new Button();
            lblJobsTitle = new Label();
            pnlDetail = new Panel();
            btnSendUV2 = new Button();
            btnSendMk3 = new Button();
            btnSendUV1 = new Button();
            btnSendMk1Mk2 = new Button();
            groupBox1 = new GroupBox();
            dataGridView1 = new DataGridView();
            idDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            inkjetNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            Lot = new DataGridViewTextBoxColumn();
            Name = new DataGridViewTextBoxColumn();
            ProgramName = new DataGridViewTextBoxColumn();
            bindingSourceUVinkjet = new BindingSource(components);
            lblDetailTitle = new Label();
            lblBarcode = new Label();
            txtBarcode = new TextBox();
            lblLot = new Label();
            txtLot = new TextBox();
            lblStatus = new Label();
            txtStatus = new TextBox();
            lblPattern = new Label();
            txtPattern = new TextBox();
            grpInkjetConfigs = new GroupBox();
            dgvConfigs = new DataGridView();
            ordinalDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            programNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            programNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            widthDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            heightDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            triggerDelayDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            directionDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindSourceInkjetConfigDto = new BindingSource(components);
            grpTextBlocks = new GroupBox();
            dgvTextBlocks = new DataGridView();
            bindingSourceTextBlockDto = new BindingSource(components);
            btnSend = new Button();
            btnRetry = new Button();
            blockNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            textDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            xDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            yDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            sizeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            scaleDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            RuleResult = new DataGridViewTextBoxColumn();

            // ═══ NEW: Layout panels ═══
            tblDetailLayout = new TableLayoutPanel();
            pnlJobInfo = new TableLayoutPanel();
            pnlButtons = new FlowLayoutPanel();

            tabControl.SuspendLayout();
            tabList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistory).BeginInit();
            pnlJobs.SuspendLayout();
            pnlDetail.SuspendLayout();
            tblDetailLayout.SuspendLayout();
            pnlJobInfo.SuspendLayout();
            pnlButtons.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).BeginInit();
            grpInkjetConfigs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindSourceInkjetConfigDto).BeginInit();
            grpTextBlocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceTextBlockDto).BeginInit();
            SuspendLayout();

            // ═══════════════════════════════════════════
            //  LEFT PANEL — pnlJobs (Dock Left, fixed width)
            // ═══════════════════════════════════════════

            // tabControl
            tabControl.Controls.Add(tabList);
            tabControl.Controls.Add(tabHistory);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 0;

            // tabList
            tabList.Controls.Add(dgvList);
            tabList.Name = "tabList";
            tabList.Padding = new Padding(3);
            tabList.Text = "List";

            // dgvList
            dgvList.AllowUserToAddRows = false;
            dgvList.AutoGenerateColumns = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvList.ColumnHeadersHeight = 29;
            dgvList.Columns.AddRange(new DataGridViewColumn[] { orderNoDataGridViewTextBoxColumn, customerNameDataGridViewTextBoxColumn, typeDataGridViewTextBoxColumn, qtyDataGridViewTextBoxColumn, statusDataGridViewTextBoxColumn });
            dgvList.DataSource = bindingSource1;
            dgvList.Dock = DockStyle.Fill;
            dgvList.Name = "dgvList";
            dgvList.ReadOnly = true;
            dgvList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvList.TabIndex = 0;
            dgvList.CellClick += dgvList_CellClick;

            orderNoDataGridViewTextBoxColumn.DataPropertyName = "OrderNo";
            orderNoDataGridViewTextBoxColumn.HeaderText = "Order No.";
            orderNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            orderNoDataGridViewTextBoxColumn.Name = "orderNoDataGridViewTextBoxColumn";
            orderNoDataGridViewTextBoxColumn.ReadOnly = true;

            customerNameDataGridViewTextBoxColumn.DataPropertyName = "CustomerName";
            customerNameDataGridViewTextBoxColumn.HeaderText = "Customer";
            customerNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            customerNameDataGridViewTextBoxColumn.Name = "customerNameDataGridViewTextBoxColumn";
            customerNameDataGridViewTextBoxColumn.ReadOnly = true;

            typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn.HeaderText = "Type";
            typeDataGridViewTextBoxColumn.MinimumWidth = 6;
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;

            qtyDataGridViewTextBoxColumn.DataPropertyName = "Qty";
            qtyDataGridViewTextBoxColumn.HeaderText = "Qty";
            qtyDataGridViewTextBoxColumn.MinimumWidth = 6;
            qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            qtyDataGridViewTextBoxColumn.ReadOnly = true;

            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.ReadOnly = true;

            bindingSource1.DataSource = typeof(InkjetOperator.Models.PrintJob);

            // tabHistory
            tabHistory.Controls.Add(dgvHistory);
            tabHistory.Name = "tabHistory";
            tabHistory.Padding = new Padding(3);
            tabHistory.Text = "History";

            dgvHistory.ColumnHeadersHeight = 29;
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.Name = "dgvHistory";
            dgvHistory.RowHeadersWidth = 51;

            // timerPoll
            timerPoll.Enabled = true;
            timerPoll.Interval = 5000;
            timerPoll.Tick += timerPoll_Tick;

            // lblJobsTitle
            lblJobsTitle.Dock = DockStyle.Top;
            lblJobsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblJobsTitle.Name = "lblJobsTitle";
            lblJobsTitle.Size = new System.Drawing.Size(0, 30);
            lblJobsTitle.Text = "Pending Jobs";
            lblJobsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // btnRefresh
            btnRefresh.Dock = DockStyle.Bottom;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(0, 36);
            btnRefresh.Text = "Refresh";

            // pnlJobs
            pnlJobs.Controls.Add(tabControl);
            pnlJobs.Controls.Add(btnRefresh);
            pnlJobs.Controls.Add(lblJobsTitle);
            pnlJobs.Dock = DockStyle.Left;
            pnlJobs.Name = "pnlJobs";
            pnlJobs.Padding = new Padding(6);
            pnlJobs.Size = new System.Drawing.Size(360, 0);
            pnlJobs.TabIndex = 0;

            // ═══════════════════════════════════════════
            //  RIGHT PANEL — pnlDetail (Dock Fill)
            // ═══════════════════════════════════════════

            // pnlJobInfo
            pnlJobInfo.ColumnCount = 8;
            pnlJobInfo.RowCount = 2;
            pnlJobInfo.Dock = DockStyle.Fill;
            pnlJobInfo.AutoSize = true;
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pnlJobInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlJobInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblBarcode.Text = "Barcode:";
            lblBarcode.AutoSize = true;
            lblBarcode.Anchor = AnchorStyles.Left;
            lblBarcode.Margin = new Padding(3, 6, 3, 6);

            txtBarcode.Dock = DockStyle.Fill;
            txtBarcode.ReadOnly = true;

            lblLot.Text = "Lot:";
            lblLot.AutoSize = true;
            lblLot.Anchor = AnchorStyles.Left;
            lblLot.Margin = new Padding(3, 6, 3, 6);

            txtLot.Dock = DockStyle.Fill;
            txtLot.ReadOnly = true;

            lblStatus.Text = "Status:";
            lblStatus.AutoSize = true;
            lblStatus.Anchor = AnchorStyles.Left;
            lblStatus.Margin = new Padding(3, 6, 3, 6);

            txtStatus.Dock = DockStyle.Fill;
            txtStatus.ReadOnly = true;

            lblPattern.Text = "Pattern:";
            lblPattern.AutoSize = true;
            lblPattern.Anchor = AnchorStyles.Left;
            lblPattern.Margin = new Padding(3, 6, 3, 6);

            txtPattern.Dock = DockStyle.Fill;
            txtPattern.ReadOnly = true;

            pnlJobInfo.Controls.Add(lblBarcode, 0, 0);
            pnlJobInfo.Controls.Add(txtBarcode, 1, 0);
            pnlJobInfo.Controls.Add(lblLot, 2, 0);
            pnlJobInfo.Controls.Add(txtLot, 3, 0);
            pnlJobInfo.Controls.Add(lblStatus, 4, 0);
            pnlJobInfo.Controls.Add(txtStatus, 5, 0);
            pnlJobInfo.Controls.Add(lblPattern, 0, 1);
            pnlJobInfo.Controls.Add(txtPattern, 1, 1);

            // tblDetailLayout
            tblDetailLayout.Dock = DockStyle.Fill;
            tblDetailLayout.ColumnCount = 1;
            tblDetailLayout.RowCount = 6;
            tblDetailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblDetailLayout.Padding = new Padding(0);
            tblDetailLayout.Margin = new Padding(0);

            // Title
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblDetailTitle.Text = "Job Detail";
            lblDetailTitle.Margin = new Padding(3, 6, 3, 6);

            // Inkjet Configs
            grpInkjetConfigs.Controls.Add(dgvConfigs);
            grpInkjetConfigs.Dock = DockStyle.Fill;
            grpInkjetConfigs.Text = "Inkjet Configs";
            grpInkjetConfigs.Padding = new Padding(3, 4, 3, 4);

            dgvConfigs.AllowUserToAddRows = false;
            dgvConfigs.AllowUserToDeleteRows = false;
            dgvConfigs.AutoGenerateColumns = false;
            dgvConfigs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvConfigs.ColumnHeadersHeight = 29;
            dgvConfigs.Columns.AddRange(new DataGridViewColumn[] { ordinalDataGridViewTextBoxColumn, programNumberDataGridViewTextBoxColumn, programNameDataGridViewTextBoxColumn, widthDataGridViewTextBoxColumn, heightDataGridViewTextBoxColumn, triggerDelayDataGridViewTextBoxColumn, directionDataGridViewTextBoxColumn });
            dgvConfigs.DataSource = bindSourceInkjetConfigDto;
            dgvConfigs.Dock = DockStyle.Fill;
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dgvConfigs.RowHeadersWidth = 51;
            dgvConfigs.CellClick += dgvConfigs_CellClick;

            ordinalDataGridViewTextBoxColumn.DataPropertyName = "Ordinal";
            ordinalDataGridViewTextBoxColumn.HeaderText = "Ordinal";
            ordinalDataGridViewTextBoxColumn.MinimumWidth = 6;
            ordinalDataGridViewTextBoxColumn.ReadOnly = true;

            programNumberDataGridViewTextBoxColumn.DataPropertyName = "ProgramNumber";
            programNumberDataGridViewTextBoxColumn.HeaderText = "Program#";
            programNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            programNumberDataGridViewTextBoxColumn.ReadOnly = true;

            programNameDataGridViewTextBoxColumn.DataPropertyName = "ProgramName";
            programNameDataGridViewTextBoxColumn.HeaderText = "ProgramName";
            programNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            programNameDataGridViewTextBoxColumn.ReadOnly = true;

            widthDataGridViewTextBoxColumn.DataPropertyName = "Width";
            widthDataGridViewTextBoxColumn.HeaderText = "Width";
            widthDataGridViewTextBoxColumn.MinimumWidth = 6;
            widthDataGridViewTextBoxColumn.ReadOnly = true;

            heightDataGridViewTextBoxColumn.DataPropertyName = "Height";
            heightDataGridViewTextBoxColumn.HeaderText = "Height";
            heightDataGridViewTextBoxColumn.MinimumWidth = 6;
            heightDataGridViewTextBoxColumn.ReadOnly = true;

            triggerDelayDataGridViewTextBoxColumn.DataPropertyName = "TriggerDelay";
            triggerDelayDataGridViewTextBoxColumn.HeaderText = "TriggerDelay";
            triggerDelayDataGridViewTextBoxColumn.MinimumWidth = 6;
            triggerDelayDataGridViewTextBoxColumn.ReadOnly = true;

            directionDataGridViewTextBoxColumn.DataPropertyName = "Direction";
            directionDataGridViewTextBoxColumn.HeaderText = "Direction";
            directionDataGridViewTextBoxColumn.MinimumWidth = 6;
            directionDataGridViewTextBoxColumn.ReadOnly = true;

            bindSourceInkjetConfigDto.DataSource = typeof(InkjetOperator.Models.InkjetConfigDto);

            // Text Blocks
            grpTextBlocks.Controls.Add(dgvTextBlocks);
            grpTextBlocks.Dock = DockStyle.Fill;
            grpTextBlocks.Text = "Text Blocks";
            grpTextBlocks.Padding = new Padding(3, 4, 3, 4);

            dgvTextBlocks.AllowUserToAddRows = false;
            dgvTextBlocks.AllowUserToDeleteRows = false;
            dgvTextBlocks.AutoGenerateColumns = false;
            dgvTextBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTextBlocks.ColumnHeadersHeight = 29;
            dgvTextBlocks.Columns.AddRange(new DataGridViewColumn[] { blockNumberDataGridViewTextBoxColumn, textDataGridViewTextBoxColumn, xDataGridViewTextBoxColumn, yDataGridViewTextBoxColumn, sizeDataGridViewTextBoxColumn, scaleDataGridViewTextBoxColumn, RuleResult });
            dgvTextBlocks.DataSource = bindingSourceTextBlockDto;
            dgvTextBlocks.Dock = DockStyle.Fill;
            dgvTextBlocks.Name = "dgvTextBlocks";
            dgvTextBlocks.ReadOnly = true;
            dgvTextBlocks.RowHeadersWidth = 51;

            blockNumberDataGridViewTextBoxColumn.DataPropertyName = "BlockNumber";
            blockNumberDataGridViewTextBoxColumn.HeaderText = "Block#";
            blockNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            blockNumberDataGridViewTextBoxColumn.ReadOnly = true;

            textDataGridViewTextBoxColumn.DataPropertyName = "Text";
            textDataGridViewTextBoxColumn.HeaderText = "Text";
            textDataGridViewTextBoxColumn.MinimumWidth = 6;
            textDataGridViewTextBoxColumn.ReadOnly = true;

            xDataGridViewTextBoxColumn.DataPropertyName = "X";
            xDataGridViewTextBoxColumn.HeaderText = "X";
            xDataGridViewTextBoxColumn.MinimumWidth = 6;
            xDataGridViewTextBoxColumn.ReadOnly = true;

            yDataGridViewTextBoxColumn.DataPropertyName = "Y";
            yDataGridViewTextBoxColumn.HeaderText = "Y";
            yDataGridViewTextBoxColumn.MinimumWidth = 6;
            yDataGridViewTextBoxColumn.ReadOnly = true;

            sizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            sizeDataGridViewTextBoxColumn.MinimumWidth = 6;
            sizeDataGridViewTextBoxColumn.ReadOnly = true;

            scaleDataGridViewTextBoxColumn.DataPropertyName = "Scale";
            scaleDataGridViewTextBoxColumn.HeaderText = "Scale";
            scaleDataGridViewTextBoxColumn.MinimumWidth = 6;
            scaleDataGridViewTextBoxColumn.ReadOnly = true;

            RuleResult.DataPropertyName = "RuleResult";
            RuleResult.HeaderText = "RuleResult";
            RuleResult.MinimumWidth = 6;
            RuleResult.ReadOnly = true;

            bindingSourceTextBlockDto.DataSource = typeof(InkjetOperator.Models.TextBlockDto);

            // UV Inkjet
            groupBox1.Controls.Add(dataGridView1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Text = "Inkjet UV";
            groupBox1.Padding = new Padding(3, 4, 3, 4);

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeight = 29;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { idDataGridViewTextBoxColumn, inkjetNameDataGridViewTextBoxColumn, Lot, Name, ProgramName });
            dataGridView1.DataSource = bindingSourceUVinkjet;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;

            idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            idDataGridViewTextBoxColumn.HeaderText = "Id";
            idDataGridViewTextBoxColumn.MinimumWidth = 6;

            inkjetNameDataGridViewTextBoxColumn.DataPropertyName = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.HeaderText = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.MinimumWidth = 6;

            Lot.DataPropertyName = "Lot";
            Lot.HeaderText = "Lot";
            Lot.MinimumWidth = 6;

            Name.DataPropertyName = "Name";
            Name.HeaderText = "Name";
            Name.MinimumWidth = 6;

            ProgramName.DataPropertyName = "ProgramName";
            ProgramName.HeaderText = "ProgramName";
            ProgramName.MinimumWidth = 6;

            bindingSourceUVinkjet.DataSource = typeof(InkjetOperator.Models.UVinkjet);

            // Buttons
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.AutoSize = true;
            pnlButtons.Padding = new Padding(0, 6, 0, 6);
            pnlButtons.WrapContents = true;

            btnSend.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnSend.ForeColor = System.Drawing.Color.White;
            btnSend.Size = new System.Drawing.Size(200, 44);
            btnSend.Text = "SEND TO DEVICES";
            btnSend.UseVisualStyleBackColor = false;
            btnSend.Margin = new Padding(3);

            btnRetry.Size = new System.Drawing.Size(120, 44);
            btnRetry.Text = "Retry Failed";
            btnRetry.Margin = new Padding(3);
            btnRetry.Click += btnRetry_Click;

            btnSendMk1Mk2.Size = new System.Drawing.Size(130, 44);
            btnSendMk1Mk2.Text = "ส่งหา MK1,MK2";
            btnSendMk1Mk2.Margin = new Padding(3);
            btnSendMk1Mk2.Click += btnSendMk1Mk2_Click;

            btnSendMk3.Size = new System.Drawing.Size(120, 44);
            btnSendMk3.Text = "ส่งหา MK3";
            btnSendMk3.Margin = new Padding(3);
            btnSendMk3.Click += btnSendMk3_Click;

            btnSendUV1.Size = new System.Drawing.Size(120, 44);
            btnSendUV1.Text = "ส่งหา UV1";
            btnSendUV1.Margin = new Padding(3);
            btnSendUV1.Click += btnSendUV1_Click;

            btnSendUV2.Size = new System.Drawing.Size(120, 44);
            btnSendUV2.Text = "ส่งหา UV2";
            btnSendUV2.Margin = new Padding(3);
            btnSendUV2.Click += btnSendUV2_Click_1;

            pnlButtons.Controls.Add(btnSend);
            pnlButtons.Controls.Add(btnRetry);
            pnlButtons.Controls.Add(btnSendMk1Mk2);
            pnlButtons.Controls.Add(btnSendMk3);
            pnlButtons.Controls.Add(btnSendUV1);
            pnlButtons.Controls.Add(btnSendUV2);

            // Assemble tblDetailLayout
            tblDetailLayout.Controls.Add(lblDetailTitle, 0, 0);
            tblDetailLayout.Controls.Add(pnlJobInfo, 0, 1);
            tblDetailLayout.Controls.Add(grpInkjetConfigs, 0, 2);
            tblDetailLayout.Controls.Add(grpTextBlocks, 0, 3);
            tblDetailLayout.Controls.Add(groupBox1, 0, 4);
            tblDetailLayout.Controls.Add(pnlButtons, 0, 5);

            // pnlDetail
            pnlDetail.Controls.Add(tblDetailLayout);
            pnlDetail.Dock = DockStyle.Fill;
            pnlDetail.Padding = new Padding(8);
            pnlDetail.Name = "pnlDetail";

            // ucOrder
            Controls.Add(pnlDetail);
            Controls.Add(pnlJobs);
            Size = new System.Drawing.Size(1243, 772);

            // Resume Layout
            tabControl.ResumeLayout(false);
            tabList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvList).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            tabHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvHistory).EndInit();
            pnlJobs.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            pnlJobInfo.ResumeLayout(false);
            pnlJobInfo.PerformLayout();
            tblDetailLayout.ResumeLayout(false);
            tblDetailLayout.PerformLayout();
            pnlDetail.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).EndInit();
            grpInkjetConfigs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindSourceInkjetConfigDto).EndInit();
            grpTextBlocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceTextBlockDto).EndInit();
            ResumeLayout(false);
        }

        // Fields
        private TableLayoutPanel tblDetailLayout;
        private TableLayoutPanel pnlJobInfo;
        private FlowLayoutPanel pnlButtons;

        private BindingSource bindingSource1;
        private DataGridViewTextBoxColumn orderNoDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn customerNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private System.Windows.Forms.Timer timerPoll;
        private Panel pnlJobs;
        private Button btnRefresh;
        private Label lblJobsTitle;
        private Panel pnlDetail;
        private Label lblDetailTitle;
        private Label lblBarcode;
        private TextBox txtBarcode;
        private Label lblLot;
        private TextBox txtLot;
        private Label lblStatus;
        private TextBox txtStatus;
        private Label lblPattern;
        private TextBox txtPattern;
        private GroupBox grpInkjetConfigs;
        private DataGridView dgvConfigs;
        private GroupBox grpTextBlocks;
        private DataGridView dgvTextBlocks;
        private Button btnSend;
        private Button btnRetry;
        private BindingSource bindSourceInkjetConfigDto;
        private DataGridViewTextBoxColumn ordinalDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn programNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn programNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn widthDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn heightDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn triggerDelayDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn directionDataGridViewTextBoxColumn;
        private BindingSource bindingSourceTextBlockDto;
        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private BindingSource bindingSourceUVinkjet;
        private DataGridViewTextBoxColumn lotDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn programnameDataGridViewTextBoxColumn1;
        private Button btnSendMk1Mk2;
        private Button btnSendUV2;
        private Button btnSendMk3;
        private Button btnSendUV1;
        private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn inkjetNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn Lot;
        private DataGridViewTextBoxColumn Name;
        private DataGridViewTextBoxColumn ProgramName;
        private DataGridViewTextBoxColumn blockNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn textDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn xDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn yDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scaleDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn RuleResult;
    }
}