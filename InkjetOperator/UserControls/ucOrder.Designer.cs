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
            stationDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindingSource1 = new BindingSource(components);
            tabHistory = new TabPage();
            dgvHistory = new DataGridView();
            timerPoll = new System.Windows.Forms.Timer(components);
            pnlJobs = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            button1 = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            dataGridView2 = new DataGridView();
            bindingSourceJobSt3 = new BindingSource(components);
            label1 = new Label();
            btnRefresh = new Button();
            lblJobsTitle = new Label();
            pnlDetail = new Panel();
            tblDetailLayout = new TableLayoutPanel();
            lblDetailTitle = new Label();
            pnlJobInfo = new TableLayoutPanel();
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
            blockNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            textDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            xDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            yDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            sizeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            scaleDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            RuleResult = new DataGridViewTextBoxColumn();
            bindingSourceTextBlockDto = new BindingSource(components);
            groupBox1 = new GroupBox();
            dataGridView1 = new DataGridView();
            idDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            inkjetNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            Lot = new DataGridViewTextBoxColumn();
            Name = new DataGridViewTextBoxColumn();
            ProgramName = new DataGridViewTextBoxColumn();
            bindingSourceUVinkjet = new BindingSource(components);
            pnlButtons = new FlowLayoutPanel();
            btnSendMk1Mk2 = new Button();
            btnSendUV1 = new Button();
            btnSendMk3 = new Button();
            btnSendUV2 = new Button();
            textBlocksBindingSource = new BindingSource(components);
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            st1_confirmation = new DataGridViewTextBoxColumn();
            st1_send_time = new DataGridViewTextBoxColumn();
            tabControl.SuspendLayout();
            tabList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistory).BeginInit();
            pnlJobs.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceJobSt3).BeginInit();
            pnlDetail.SuspendLayout();
            tblDetailLayout.SuspendLayout();
            pnlJobInfo.SuspendLayout();
            grpInkjetConfigs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindSourceInkjetConfigDto).BeginInit();
            grpTextBlocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceTextBlockDto).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).BeginInit();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textBlocksBindingSource).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabList);
            tabControl.Controls.Add(tabHistory);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(3, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(453, 322);
            tabControl.TabIndex = 0;
            // 
            // tabList
            // 
            tabList.Controls.Add(dgvList);
            tabList.Location = new Point(4, 29);
            tabList.Name = "tabList";
            tabList.Padding = new Padding(3);
            tabList.Size = new Size(445, 289);
            tabList.TabIndex = 0;
            tabList.Text = "List";
            // 
            // dgvList
            // 
            dgvList.AllowUserToAddRows = false;
            dgvList.AutoGenerateColumns = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvList.ColumnHeadersHeight = 29;
            dgvList.Columns.AddRange(new DataGridViewColumn[] { orderNoDataGridViewTextBoxColumn, customerNameDataGridViewTextBoxColumn, typeDataGridViewTextBoxColumn, qtyDataGridViewTextBoxColumn, statusDataGridViewTextBoxColumn, stationDataGridViewTextBoxColumn });
            dgvList.DataSource = bindingSource1;
            dgvList.Dock = DockStyle.Fill;
            dgvList.Location = new Point(3, 3);
            dgvList.Name = "dgvList";
            dgvList.ReadOnly = true;
            dgvList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvList.Size = new Size(439, 283);
            dgvList.TabIndex = 0;
            dgvList.CellClick += dgvList_CellClick;
            dgvList.CellContentClick += dgvList_CellContentClick;
            // 
            // orderNoDataGridViewTextBoxColumn
            // 
            orderNoDataGridViewTextBoxColumn.DataPropertyName = "OrderNo";
            orderNoDataGridViewTextBoxColumn.HeaderText = "Order No.";
            orderNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            orderNoDataGridViewTextBoxColumn.Name = "orderNoDataGridViewTextBoxColumn";
            orderNoDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // customerNameDataGridViewTextBoxColumn
            // 
            customerNameDataGridViewTextBoxColumn.DataPropertyName = "CustomerName";
            customerNameDataGridViewTextBoxColumn.HeaderText = "Customer";
            customerNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            customerNameDataGridViewTextBoxColumn.Name = "customerNameDataGridViewTextBoxColumn";
            customerNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn.HeaderText = "Type";
            typeDataGridViewTextBoxColumn.MinimumWidth = 6;
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // qtyDataGridViewTextBoxColumn
            // 
            qtyDataGridViewTextBoxColumn.DataPropertyName = "Qty";
            qtyDataGridViewTextBoxColumn.HeaderText = "Qty";
            qtyDataGridViewTextBoxColumn.MinimumWidth = 6;
            qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            qtyDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // stationDataGridViewTextBoxColumn
            // 
            stationDataGridViewTextBoxColumn.DataPropertyName = "Station";
            stationDataGridViewTextBoxColumn.HeaderText = "Station";
            stationDataGridViewTextBoxColumn.MinimumWidth = 6;
            stationDataGridViewTextBoxColumn.Name = "stationDataGridViewTextBoxColumn";
            stationDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bindingSource1
            // 
            bindingSource1.DataSource = typeof(PrintJob);
            // 
            // tabHistory
            // 
            tabHistory.Controls.Add(dgvHistory);
            tabHistory.Location = new Point(4, 29);
            tabHistory.Name = "tabHistory";
            tabHistory.Padding = new Padding(3);
            tabHistory.Size = new Size(445, 289);
            tabHistory.TabIndex = 1;
            tabHistory.Text = "History";
            // 
            // dgvHistory
            // 
            dgvHistory.ColumnHeadersHeight = 29;
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.Location = new Point(3, 3);
            dgvHistory.Name = "dgvHistory";
            dgvHistory.RowHeadersWidth = 51;
            dgvHistory.Size = new Size(439, 283);
            dgvHistory.TabIndex = 0;
            // 
            // timerPoll
            // 
            timerPoll.Enabled = true;
            timerPoll.Interval = 5000;
            timerPoll.Tick += timerPoll_Tick;
            // 
            // pnlJobs
            // 
            pnlJobs.Controls.Add(tableLayoutPanel1);
            pnlJobs.Controls.Add(btnRefresh);
            pnlJobs.Controls.Add(lblJobsTitle);
            pnlJobs.Dock = DockStyle.Left;
            pnlJobs.Location = new Point(0, 0);
            pnlJobs.Name = "pnlJobs";
            pnlJobs.Padding = new Padding(6);
            pnlJobs.Size = new Size(471, 772);
            pnlJobs.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(button1, 0, 2);
            tableLayoutPanel1.Controls.Add(tabControl, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(6, 36);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanel1.Size = new Size(459, 694);
            tableLayoutPanel1.TabIndex = 3;
            tableLayoutPanel1.Paint += tableLayoutPanel1_Paint;
            // 
            // button1
            // 
            button1.Location = new Point(3, 659);
            button1.Name = "button1";
            button1.Size = new Size(130, 32);
            button1.TabIndex = 3;
            button1.Text = "Confirm Job ST3";
            button1.Click += button1_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(dataGridView2, 0, 1);
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 331);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 9.063444F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 90.936554F));
            tableLayoutPanel2.Size = new Size(453, 322);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ColumnHeadersHeight = 29;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, st1_confirmation, st1_send_time });
            dataGridView2.DataSource = bindingSourceJobSt3;
            dataGridView2.Dock = DockStyle.Fill;
            dataGridView2.Location = new Point(3, 32);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridView2.Size = new Size(447, 287);
            dataGridView2.TabIndex = 4;
            // 
            // bindingSourceJobSt3
            // 
            bindingSourceJobSt3.DataSource = typeof(PrintJob);
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(447, 29);
            label1.TabIndex = 3;
            label1.Text = "Job Form - ST3";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnRefresh
            // 
            btnRefresh.Dock = DockStyle.Bottom;
            btnRefresh.Location = new Point(6, 730);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(459, 36);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            // 
            // lblJobsTitle
            // 
            lblJobsTitle.Dock = DockStyle.Top;
            lblJobsTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblJobsTitle.Location = new Point(6, 6);
            lblJobsTitle.Name = "lblJobsTitle";
            lblJobsTitle.Size = new Size(459, 30);
            lblJobsTitle.TabIndex = 2;
            lblJobsTitle.Text = "Pending Jobs";
            lblJobsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlDetail
            // 
            pnlDetail.Controls.Add(tblDetailLayout);
            pnlDetail.Dock = DockStyle.Fill;
            pnlDetail.Location = new Point(471, 0);
            pnlDetail.Name = "pnlDetail";
            pnlDetail.Padding = new Padding(8);
            pnlDetail.Size = new Size(772, 772);
            pnlDetail.TabIndex = 0;
            // 
            // tblDetailLayout
            // 
            tblDetailLayout.ColumnCount = 1;
            tblDetailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDetailLayout.Controls.Add(lblDetailTitle, 0, 0);
            tblDetailLayout.Controls.Add(pnlJobInfo, 0, 1);
            tblDetailLayout.Controls.Add(grpInkjetConfigs, 0, 2);
            tblDetailLayout.Controls.Add(grpTextBlocks, 0, 3);
            tblDetailLayout.Controls.Add(groupBox1, 0, 4);
            tblDetailLayout.Controls.Add(pnlButtons, 0, 5);
            tblDetailLayout.Dock = DockStyle.Fill;
            tblDetailLayout.Location = new Point(8, 8);
            tblDetailLayout.Margin = new Padding(0);
            tblDetailLayout.Name = "tblDetailLayout";
            tblDetailLayout.RowCount = 6;
            tblDetailLayout.RowStyles.Add(new RowStyle());
            tblDetailLayout.RowStyles.Add(new RowStyle());
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            tblDetailLayout.RowStyles.Add(new RowStyle());
            tblDetailLayout.Size = new Size(756, 756);
            tblDetailLayout.TabIndex = 0;
            // 
            // lblDetailTitle
            // 
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDetailTitle.Location = new Point(3, 6);
            lblDetailTitle.Margin = new Padding(3, 6, 3, 6);
            lblDetailTitle.Name = "lblDetailTitle";
            lblDetailTitle.Size = new Size(100, 25);
            lblDetailTitle.TabIndex = 0;
            lblDetailTitle.Text = "Job Detail";
            // 
            // pnlJobInfo
            // 
            pnlJobInfo.AutoSize = true;
            pnlJobInfo.ColumnCount = 8;
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle());
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle());
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle());
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));
            pnlJobInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            pnlJobInfo.Controls.Add(lblBarcode, 0, 0);
            pnlJobInfo.Controls.Add(txtBarcode, 1, 0);
            pnlJobInfo.Controls.Add(lblLot, 2, 0);
            pnlJobInfo.Controls.Add(txtLot, 3, 0);
            pnlJobInfo.Controls.Add(lblStatus, 4, 0);
            pnlJobInfo.Controls.Add(txtStatus, 5, 0);
            pnlJobInfo.Controls.Add(lblPattern, 0, 1);
            pnlJobInfo.Controls.Add(txtPattern, 1, 1);
            pnlJobInfo.Dock = DockStyle.Fill;
            pnlJobInfo.Location = new Point(3, 40);
            pnlJobInfo.Name = "pnlJobInfo";
            pnlJobInfo.RowCount = 2;
            pnlJobInfo.RowStyles.Add(new RowStyle());
            pnlJobInfo.RowStyles.Add(new RowStyle());
            pnlJobInfo.Size = new Size(750, 66);
            pnlJobInfo.TabIndex = 1;
            // 
            // lblBarcode
            // 
            lblBarcode.Anchor = AnchorStyles.Left;
            lblBarcode.AutoSize = true;
            lblBarcode.Location = new Point(3, 6);
            lblBarcode.Margin = new Padding(3, 6, 3, 6);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(67, 20);
            lblBarcode.TabIndex = 0;
            lblBarcode.Text = "Barcode:";
            // 
            // txtBarcode
            // 
            txtBarcode.Dock = DockStyle.Fill;
            txtBarcode.Location = new Point(76, 3);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.ReadOnly = true;
            txtBarcode.Size = new Size(168, 27);
            txtBarcode.TabIndex = 1;
            // 
            // lblLot
            // 
            lblLot.Anchor = AnchorStyles.Left;
            lblLot.AutoSize = true;
            lblLot.Location = new Point(250, 6);
            lblLot.Margin = new Padding(3, 6, 3, 6);
            lblLot.Name = "lblLot";
            lblLot.Size = new Size(33, 20);
            lblLot.TabIndex = 2;
            lblLot.Text = "Lot:";
            // 
            // txtLot
            // 
            txtLot.Dock = DockStyle.Fill;
            txtLot.Location = new Point(289, 3);
            txtLot.Name = "txtLot";
            txtLot.ReadOnly = true;
            txtLot.Size = new Size(139, 27);
            txtLot.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(434, 6);
            lblStatus.Margin = new Padding(3, 6, 3, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Status:";
            // 
            // txtStatus
            // 
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.Location = new Point(492, 3);
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(110, 27);
            txtStatus.TabIndex = 5;
            // 
            // lblPattern
            // 
            lblPattern.Anchor = AnchorStyles.Left;
            lblPattern.AutoSize = true;
            lblPattern.Location = new Point(3, 39);
            lblPattern.Margin = new Padding(3, 6, 3, 6);
            lblPattern.Name = "lblPattern";
            lblPattern.Size = new Size(58, 20);
            lblPattern.TabIndex = 6;
            lblPattern.Text = "Pattern:";
            // 
            // txtPattern
            // 
            txtPattern.Dock = DockStyle.Fill;
            txtPattern.Location = new Point(76, 36);
            txtPattern.Name = "txtPattern";
            txtPattern.ReadOnly = true;
            txtPattern.Size = new Size(168, 27);
            txtPattern.TabIndex = 7;
            // 
            // grpInkjetConfigs
            // 
            grpInkjetConfigs.Controls.Add(dgvConfigs);
            grpInkjetConfigs.Dock = DockStyle.Fill;
            grpInkjetConfigs.Location = new Point(3, 112);
            grpInkjetConfigs.Name = "grpInkjetConfigs";
            grpInkjetConfigs.Padding = new Padding(3, 4, 3, 4);
            grpInkjetConfigs.Size = new Size(750, 187);
            grpInkjetConfigs.TabIndex = 2;
            grpInkjetConfigs.TabStop = false;
            grpInkjetConfigs.Text = "Inkjet Configs";
            // 
            // dgvConfigs
            // 
            dgvConfigs.AllowUserToAddRows = false;
            dgvConfigs.AllowUserToDeleteRows = false;
            dgvConfigs.AutoGenerateColumns = false;
            dgvConfigs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvConfigs.ColumnHeadersHeight = 29;
            dgvConfigs.Columns.AddRange(new DataGridViewColumn[] { ordinalDataGridViewTextBoxColumn, programNumberDataGridViewTextBoxColumn, programNameDataGridViewTextBoxColumn, widthDataGridViewTextBoxColumn, heightDataGridViewTextBoxColumn, triggerDelayDataGridViewTextBoxColumn, directionDataGridViewTextBoxColumn });
            dgvConfigs.DataSource = bindSourceInkjetConfigDto;
            dgvConfigs.Dock = DockStyle.Fill;
            dgvConfigs.Location = new Point(3, 24);
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dgvConfigs.RowHeadersWidth = 51;
            dgvConfigs.Size = new Size(744, 159);
            dgvConfigs.TabIndex = 0;
            dgvConfigs.CellClick += dgvConfigs_CellClick;
            // 
            // ordinalDataGridViewTextBoxColumn
            // 
            ordinalDataGridViewTextBoxColumn.DataPropertyName = "Ordinal";
            ordinalDataGridViewTextBoxColumn.HeaderText = "Ordinal";
            ordinalDataGridViewTextBoxColumn.MinimumWidth = 6;
            ordinalDataGridViewTextBoxColumn.Name = "ordinalDataGridViewTextBoxColumn";
            ordinalDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // programNumberDataGridViewTextBoxColumn
            // 
            programNumberDataGridViewTextBoxColumn.DataPropertyName = "ProgramNumber";
            programNumberDataGridViewTextBoxColumn.HeaderText = "Program#";
            programNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            programNumberDataGridViewTextBoxColumn.Name = "programNumberDataGridViewTextBoxColumn";
            programNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // programNameDataGridViewTextBoxColumn
            // 
            programNameDataGridViewTextBoxColumn.DataPropertyName = "ProgramName";
            programNameDataGridViewTextBoxColumn.HeaderText = "ProgramName";
            programNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            programNameDataGridViewTextBoxColumn.Name = "programNameDataGridViewTextBoxColumn";
            programNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // widthDataGridViewTextBoxColumn
            // 
            widthDataGridViewTextBoxColumn.DataPropertyName = "Width";
            widthDataGridViewTextBoxColumn.HeaderText = "Width";
            widthDataGridViewTextBoxColumn.MinimumWidth = 6;
            widthDataGridViewTextBoxColumn.Name = "widthDataGridViewTextBoxColumn";
            widthDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // heightDataGridViewTextBoxColumn
            // 
            heightDataGridViewTextBoxColumn.DataPropertyName = "Height";
            heightDataGridViewTextBoxColumn.HeaderText = "Height";
            heightDataGridViewTextBoxColumn.MinimumWidth = 6;
            heightDataGridViewTextBoxColumn.Name = "heightDataGridViewTextBoxColumn";
            heightDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // triggerDelayDataGridViewTextBoxColumn
            // 
            triggerDelayDataGridViewTextBoxColumn.DataPropertyName = "TriggerDelay";
            triggerDelayDataGridViewTextBoxColumn.HeaderText = "TriggerDelay";
            triggerDelayDataGridViewTextBoxColumn.MinimumWidth = 6;
            triggerDelayDataGridViewTextBoxColumn.Name = "triggerDelayDataGridViewTextBoxColumn";
            triggerDelayDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // directionDataGridViewTextBoxColumn
            // 
            directionDataGridViewTextBoxColumn.DataPropertyName = "Direction";
            directionDataGridViewTextBoxColumn.HeaderText = "Direction";
            directionDataGridViewTextBoxColumn.MinimumWidth = 6;
            directionDataGridViewTextBoxColumn.Name = "directionDataGridViewTextBoxColumn";
            directionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bindSourceInkjetConfigDto
            // 
            bindSourceInkjetConfigDto.DataSource = typeof(Models.InkjetConfigDto);
            // 
            // grpTextBlocks
            // 
            grpTextBlocks.Controls.Add(dgvTextBlocks);
            grpTextBlocks.Dock = DockStyle.Fill;
            grpTextBlocks.Location = new Point(3, 305);
            grpTextBlocks.Name = "grpTextBlocks";
            grpTextBlocks.Padding = new Padding(3, 4, 3, 4);
            grpTextBlocks.Size = new Size(750, 187);
            grpTextBlocks.TabIndex = 3;
            grpTextBlocks.TabStop = false;
            grpTextBlocks.Text = "Text Blocks";
            // 
            // dgvTextBlocks
            // 
            dgvTextBlocks.AllowUserToAddRows = false;
            dgvTextBlocks.AllowUserToDeleteRows = false;
            dgvTextBlocks.AutoGenerateColumns = false;
            dgvTextBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTextBlocks.ColumnHeadersHeight = 29;
            dgvTextBlocks.Columns.AddRange(new DataGridViewColumn[] { blockNumberDataGridViewTextBoxColumn, textDataGridViewTextBoxColumn, xDataGridViewTextBoxColumn, yDataGridViewTextBoxColumn, sizeDataGridViewTextBoxColumn, scaleDataGridViewTextBoxColumn, RuleResult });
            dgvTextBlocks.DataSource = bindingSourceTextBlockDto;
            dgvTextBlocks.Dock = DockStyle.Fill;
            dgvTextBlocks.Location = new Point(3, 24);
            dgvTextBlocks.Name = "dgvTextBlocks";
            dgvTextBlocks.ReadOnly = true;
            dgvTextBlocks.RowHeadersWidth = 51;
            dgvTextBlocks.Size = new Size(744, 159);
            dgvTextBlocks.TabIndex = 0;
            // 
            // blockNumberDataGridViewTextBoxColumn
            // 
            blockNumberDataGridViewTextBoxColumn.DataPropertyName = "BlockNumber";
            blockNumberDataGridViewTextBoxColumn.HeaderText = "Block#";
            blockNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            blockNumberDataGridViewTextBoxColumn.Name = "blockNumberDataGridViewTextBoxColumn";
            blockNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // textDataGridViewTextBoxColumn
            // 
            textDataGridViewTextBoxColumn.DataPropertyName = "Text";
            textDataGridViewTextBoxColumn.HeaderText = "Text";
            textDataGridViewTextBoxColumn.MinimumWidth = 6;
            textDataGridViewTextBoxColumn.Name = "textDataGridViewTextBoxColumn";
            textDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // xDataGridViewTextBoxColumn
            // 
            xDataGridViewTextBoxColumn.DataPropertyName = "X";
            xDataGridViewTextBoxColumn.HeaderText = "X";
            xDataGridViewTextBoxColumn.MinimumWidth = 6;
            xDataGridViewTextBoxColumn.Name = "xDataGridViewTextBoxColumn";
            xDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // yDataGridViewTextBoxColumn
            // 
            yDataGridViewTextBoxColumn.DataPropertyName = "Y";
            yDataGridViewTextBoxColumn.HeaderText = "Y";
            yDataGridViewTextBoxColumn.MinimumWidth = 6;
            yDataGridViewTextBoxColumn.Name = "yDataGridViewTextBoxColumn";
            yDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            sizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            sizeDataGridViewTextBoxColumn.MinimumWidth = 6;
            sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            sizeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scaleDataGridViewTextBoxColumn
            // 
            scaleDataGridViewTextBoxColumn.DataPropertyName = "Scale";
            scaleDataGridViewTextBoxColumn.HeaderText = "Scale";
            scaleDataGridViewTextBoxColumn.MinimumWidth = 6;
            scaleDataGridViewTextBoxColumn.Name = "scaleDataGridViewTextBoxColumn";
            scaleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // RuleResult
            // 
            RuleResult.DataPropertyName = "RuleResult";
            RuleResult.HeaderText = "RuleResult";
            RuleResult.MinimumWidth = 6;
            RuleResult.Name = "RuleResult";
            RuleResult.ReadOnly = true;
            // 
            // bindingSourceTextBlockDto
            // 
            bindingSourceTextBlockDto.DataSource = typeof(Models.TextBlockDto);
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dataGridView1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 498);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(750, 187);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Inkjet UV";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeight = 29;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { idDataGridViewTextBoxColumn, inkjetNameDataGridViewTextBoxColumn, Lot, Name, ProgramName });
            dataGridView1.DataSource = bindingSourceUVinkjet;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 24);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(744, 159);
            dataGridView1.TabIndex = 0;
            // 
            // idDataGridViewTextBoxColumn
            // 
            idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            idDataGridViewTextBoxColumn.HeaderText = "Id";
            idDataGridViewTextBoxColumn.MinimumWidth = 6;
            idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            // 
            // inkjetNameDataGridViewTextBoxColumn
            // 
            inkjetNameDataGridViewTextBoxColumn.DataPropertyName = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.HeaderText = "InkjetName";
            inkjetNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            inkjetNameDataGridViewTextBoxColumn.Name = "inkjetNameDataGridViewTextBoxColumn";
            // 
            // Lot
            // 
            Lot.DataPropertyName = "Lot";
            Lot.HeaderText = "Lot";
            Lot.MinimumWidth = 6;
            Lot.Name = "Lot";
            // 
            // Name
            // 
            Name.DataPropertyName = "Name";
            Name.HeaderText = "Name";
            Name.MinimumWidth = 6;
            Name.Name = "Name";
            // 
            // ProgramName
            // 
            ProgramName.DataPropertyName = "ProgramName";
            ProgramName.HeaderText = "ProgramName";
            ProgramName.MinimumWidth = 6;
            ProgramName.Name = "ProgramName";
            // 
            // bindingSourceUVinkjet
            // 
            bindingSourceUVinkjet.DataSource = typeof(Models.UVinkjet);
            // 
            // pnlButtons
            // 
            pnlButtons.AutoSize = true;
            pnlButtons.Controls.Add(btnSendMk1Mk2);
            pnlButtons.Controls.Add(btnSendUV1);
            pnlButtons.Controls.Add(btnSendMk3);
            pnlButtons.Controls.Add(btnSendUV2);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Location = new Point(3, 691);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(0, 6, 0, 6);
            pnlButtons.Size = new Size(750, 62);
            pnlButtons.TabIndex = 5;
            // 
            // btnSendMk1Mk2
            // 
            btnSendMk1Mk2.Location = new Point(3, 9);
            btnSendMk1Mk2.Name = "btnSendMk1Mk2";
            btnSendMk1Mk2.Size = new Size(130, 44);
            btnSendMk1Mk2.TabIndex = 2;
            btnSendMk1Mk2.Text = "ส่งหา MK1,MK2";
            btnSendMk1Mk2.Click += btnSendMk1Mk2_Click;
            // 
            // btnSendUV1
            // 
            btnSendUV1.Location = new Point(139, 9);
            btnSendUV1.Name = "btnSendUV1";
            btnSendUV1.Size = new Size(120, 44);
            btnSendUV1.TabIndex = 4;
            btnSendUV1.Text = "ส่งหา UV1";
            btnSendUV1.Click += btnSendUV1_Click;
            // 
            // btnSendMk3
            // 
            btnSendMk3.Location = new Point(265, 9);
            btnSendMk3.Name = "btnSendMk3";
            btnSendMk3.Size = new Size(120, 44);
            btnSendMk3.TabIndex = 3;
            btnSendMk3.Text = "ส่งหา MK3";
            btnSendMk3.Click += btnSendMk3_Click;
            // 
            // btnSendUV2
            // 
            btnSendUV2.Location = new Point(391, 9);
            btnSendUV2.Name = "btnSendUV2";
            btnSendUV2.Size = new Size(120, 44);
            btnSendUV2.TabIndex = 5;
            btnSendUV2.Text = "ส่งหา UV2";
            btnSendUV2.Click += btnSendUV2_Click_1;
            // 
            // textBlocksBindingSource
            // 
            textBlocksBindingSource.DataMember = "TextBlocks";
            textBlocksBindingSource.DataSource = bindSourceInkjetConfigDto;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.DataPropertyName = "OrderNo";
            dataGridViewTextBoxColumn1.HeaderText = "Order No.";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.DataPropertyName = "CustomerName";
            dataGridViewTextBoxColumn2.HeaderText = "Customer";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.DataPropertyName = "Type";
            dataGridViewTextBoxColumn3.HeaderText = "Type";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.DataPropertyName = "Qty";
            dataGridViewTextBoxColumn4.HeaderText = "Qty";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // st1_confirmation
            // 
            st1_confirmation.DataPropertyName = "st1_confirmation";
            st1_confirmation.HeaderText = "Status";
            st1_confirmation.MinimumWidth = 6;
            st1_confirmation.Name = "st1_confirmation";
            st1_confirmation.ReadOnly = true;
            // 
            // st1_send_time
            // 
            st1_send_time.DataPropertyName = "st1_send_time";
            st1_send_time.HeaderText = "Send time";
            st1_send_time.MinimumWidth = 6;
            st1_send_time.Name = "st1_send_time";
            st1_send_time.ReadOnly = true;
            // 
            // ucOrder
            // 
            Controls.Add(pnlDetail);
            Controls.Add(pnlJobs);
            //Name = "ucOrder";
            Size = new Size(1243, 772);
            tabControl.ResumeLayout(false);
            tabList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvList).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            tabHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvHistory).EndInit();
            pnlJobs.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceJobSt3).EndInit();
            pnlDetail.ResumeLayout(false);
            tblDetailLayout.ResumeLayout(false);
            tblDetailLayout.PerformLayout();
            pnlJobInfo.ResumeLayout(false);
            pnlJobInfo.PerformLayout();
            grpInkjetConfigs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConfigs).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindSourceInkjetConfigDto).EndInit();
            grpTextBlocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceTextBlockDto).EndInit();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).EndInit();
            pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)textBlocksBindingSource).EndInit();
            ResumeLayout(false);
        }

        // Fields
        private TableLayoutPanel tblDetailLayout;
        private TableLayoutPanel pnlJobInfo;
        private FlowLayoutPanel pnlButtons;

        private BindingSource bindingSource1;
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
        private BindingSource textBlocksBindingSource;
        private DataGridViewTextBoxColumn orderNoDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn customerNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn stationDataGridViewTextBoxColumn;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private DataGridView dataGridView2;
        private Label label1;
        private Button button1;
        private BindingSource bindingSourceJobSt3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn st1_confirmation;
        private DataGridViewTextBoxColumn st1_send_time;
    }
}