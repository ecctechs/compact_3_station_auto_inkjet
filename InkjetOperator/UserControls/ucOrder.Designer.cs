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
            blockNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            textDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            xDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            yDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            sizeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            scaleDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindingSourceTextBlockDto = new BindingSource(components);
            btnSend = new Button();
            btnRetry = new Button();
            tabControl.SuspendLayout();
            tabList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistory).BeginInit();
            pnlJobs.SuspendLayout();
            pnlDetail.SuspendLayout();
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
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabList);
            tabControl.Controls.Add(tabHistory);
            tabControl.Location = new Point(9, 33);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(385, 626);
            tabControl.TabIndex = 0;
            // 
            // tabList
            // 
            tabList.Controls.Add(dgvList);
            tabList.Location = new Point(4, 29);
            tabList.Name = "tabList";
            tabList.Size = new Size(377, 593);
            tabList.TabIndex = 0;
            tabList.Text = "List";
            // 
            // dgvList
            // 
            dgvList.AllowUserToAddRows = false;
            dgvList.AutoGenerateColumns = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvList.ColumnHeadersHeight = 29;
            dgvList.Columns.AddRange(new DataGridViewColumn[] { orderNoDataGridViewTextBoxColumn, customerNameDataGridViewTextBoxColumn, typeDataGridViewTextBoxColumn, qtyDataGridViewTextBoxColumn, statusDataGridViewTextBoxColumn });
            dgvList.DataSource = bindingSource1;
            dgvList.Dock = DockStyle.Fill;
            dgvList.Location = new Point(0, 0);
            dgvList.Name = "dgvList";
            dgvList.ReadOnly = true;
            dgvList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvList.Size = new Size(377, 593);
            dgvList.TabIndex = 0;
            dgvList.CellClick += dgvList_CellClick;
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
            // bindingSource1
            // 
            bindingSource1.DataSource = typeof(Models.PrintJob);
            // 
            // tabHistory
            // 
            tabHistory.Controls.Add(dgvHistory);
            tabHistory.Location = new Point(4, 29);
            tabHistory.Name = "tabHistory";
            tabHistory.Size = new Size(377, 593);
            tabHistory.TabIndex = 1;
            tabHistory.Text = "History";
            // 
            // dgvHistory
            // 
            dgvHistory.ColumnHeadersHeight = 29;
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.Location = new Point(0, 0);
            dgvHistory.Name = "dgvHistory";
            dgvHistory.RowHeadersWidth = 51;
            dgvHistory.Size = new Size(377, 593);
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
            pnlJobs.Controls.Add(btnRefresh);
            pnlJobs.Controls.Add(tabControl);
            pnlJobs.Controls.Add(lblJobsTitle);
            pnlJobs.Dock = DockStyle.Left;
            pnlJobs.Location = new Point(0, 0);
            pnlJobs.Margin = new Padding(3, 4, 3, 4);
            pnlJobs.Name = "pnlJobs";
            pnlJobs.Padding = new Padding(6, 7, 6, 7);
            pnlJobs.Size = new Size(400, 772);
            pnlJobs.TabIndex = 2;
            // 
            // btnRefresh
            // 
            btnRefresh.Dock = DockStyle.Bottom;
            btnRefresh.Location = new Point(6, 725);
            btnRefresh.Margin = new Padding(3, 4, 3, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(388, 40);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            // 
            // lblJobsTitle
            // 
            lblJobsTitle.Dock = DockStyle.Top;
            lblJobsTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblJobsTitle.Location = new Point(6, 7);
            lblJobsTitle.Name = "lblJobsTitle";
            lblJobsTitle.Size = new Size(388, 33);
            lblJobsTitle.TabIndex = 2;
            lblJobsTitle.Text = "Pending Jobs";
            // 
            // pnlDetail
            // 
            pnlDetail.Controls.Add(btnSendUV2);
            pnlDetail.Controls.Add(btnSendMk3);
            pnlDetail.Controls.Add(btnSendUV1);
            pnlDetail.Controls.Add(btnSendMk1Mk2);
            pnlDetail.Controls.Add(groupBox1);
            pnlDetail.Controls.Add(lblDetailTitle);
            pnlDetail.Controls.Add(lblBarcode);
            pnlDetail.Controls.Add(txtBarcode);
            pnlDetail.Controls.Add(lblLot);
            pnlDetail.Controls.Add(txtLot);
            pnlDetail.Controls.Add(lblStatus);
            pnlDetail.Controls.Add(txtStatus);
            pnlDetail.Controls.Add(lblPattern);
            pnlDetail.Controls.Add(txtPattern);
            pnlDetail.Controls.Add(grpInkjetConfigs);
            pnlDetail.Controls.Add(grpTextBlocks);
            pnlDetail.Controls.Add(btnSend);
            pnlDetail.Controls.Add(btnRetry);
            pnlDetail.Dock = DockStyle.Fill;
            pnlDetail.Location = new Point(400, 0);
            pnlDetail.Margin = new Padding(3, 4, 3, 4);
            pnlDetail.Name = "pnlDetail";
            pnlDetail.Padding = new Padding(6, 7, 6, 7);
            pnlDetail.Size = new Size(843, 772);
            pnlDetail.TabIndex = 3;
            // 
            // btnSendUV2
            // 
            btnSendUV2.Location = new Point(606, 725);
            btnSendUV2.Name = "btnSendUV2";
            btnSendUV2.Size = new Size(136, 29);
            btnSendUV2.TabIndex = 17;
            btnSendUV2.Text = "ส่งหา UV2";
            btnSendUV2.UseVisualStyleBackColor = true;
            btnSendUV2.Click += btnSendUV2_Click_1;
            // 
            // btnSendMk3
            // 
            btnSendMk3.Location = new Point(606, 675);
            btnSendMk3.Name = "btnSendMk3";
            btnSendMk3.Size = new Size(136, 29);
            btnSendMk3.TabIndex = 16;
            btnSendMk3.Text = "ส่งหา MK3";
            btnSendMk3.UseVisualStyleBackColor = true;
            btnSendMk3.Click += btnSendMk3_Click;
            // 
            // btnSendUV1
            // 
            btnSendUV1.Location = new Point(454, 725);
            btnSendUV1.Name = "btnSendUV1";
            btnSendUV1.Size = new Size(136, 29);
            btnSendUV1.TabIndex = 15;
            btnSendUV1.Text = "ส่งหา UV1";
            btnSendUV1.UseVisualStyleBackColor = true;
            btnSendUV1.Click += btnSendUV1_Click;
            // 
            // btnSendMk1Mk2
            // 
            btnSendMk1Mk2.Location = new Point(454, 675);
            btnSendMk1Mk2.Name = "btnSendMk1Mk2";
            btnSendMk1Mk2.Size = new Size(136, 29);
            btnSendMk1Mk2.TabIndex = 14;
            btnSendMk1Mk2.Text = "ส่งหา MK1,MK2";
            btnSendMk1Mk2.UseVisualStyleBackColor = true;
            btnSendMk1Mk2.Click += btnSendMk1Mk2_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dataGridView1);
            groupBox1.Location = new Point(9, 468);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(720, 199);
            groupBox1.TabIndex = 13;
            groupBox1.TabStop = false;
            groupBox1.Text = "Inkje UV ";
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
            dataGridView1.Margin = new Padding(3, 4, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(714, 171);
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
            // lblDetailTitle
            // 
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDetailTitle.Location = new Point(12, 14);
            lblDetailTitle.Name = "lblDetailTitle";
            lblDetailTitle.Size = new Size(92, 23);
            lblDetailTitle.TabIndex = 0;
            lblDetailTitle.Text = "Job Detail";
            // 
            // lblBarcode
            // 
            lblBarcode.AutoSize = true;
            lblBarcode.Location = new Point(12, 47);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(67, 20);
            lblBarcode.TabIndex = 1;
            lblBarcode.Text = "Barcode:";
            // 
            // txtBarcode
            // 
            txtBarcode.Location = new Point(91, 36);
            txtBarcode.Margin = new Padding(3, 4, 3, 4);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.ReadOnly = true;
            txtBarcode.Size = new Size(228, 27);
            txtBarcode.TabIndex = 2;
            // 
            // lblLot
            // 
            lblLot.AutoSize = true;
            lblLot.Location = new Point(337, 47);
            lblLot.Name = "lblLot";
            lblLot.Size = new Size(33, 20);
            lblLot.TabIndex = 3;
            lblLot.Text = "Lot:";
            // 
            // txtLot
            // 
            txtLot.Location = new Point(366, 36);
            txtLot.Margin = new Padding(3, 4, 3, 4);
            txtLot.Name = "txtLot";
            txtLot.ReadOnly = true;
            txtLot.Size = new Size(171, 27);
            txtLot.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(555, 47);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Status:";
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(606, 36);
            txtStatus.Margin = new Padding(3, 4, 3, 4);
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(114, 27);
            txtStatus.TabIndex = 6;
            // 
            // lblPattern
            // 
            lblPattern.AutoSize = true;
            lblPattern.Location = new Point(12, 80);
            lblPattern.Name = "lblPattern";
            lblPattern.Size = new Size(58, 20);
            lblPattern.TabIndex = 7;
            lblPattern.Text = "Pattern:";
            // 
            // txtPattern
            // 
            txtPattern.Location = new Point(91, 69);
            txtPattern.Margin = new Padding(3, 4, 3, 4);
            txtPattern.Name = "txtPattern";
            txtPattern.ReadOnly = true;
            txtPattern.Size = new Size(228, 27);
            txtPattern.TabIndex = 8;
            // 
            // grpInkjetConfigs
            // 
            grpInkjetConfigs.Controls.Add(dgvConfigs);
            grpInkjetConfigs.Location = new Point(6, 107);
            grpInkjetConfigs.Margin = new Padding(3, 4, 3, 4);
            grpInkjetConfigs.Name = "grpInkjetConfigs";
            grpInkjetConfigs.Padding = new Padding(3, 4, 3, 4);
            grpInkjetConfigs.Size = new Size(720, 173);
            grpInkjetConfigs.TabIndex = 9;
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
            dgvConfigs.Margin = new Padding(3, 4, 3, 4);
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dgvConfigs.RowHeadersWidth = 51;
            dgvConfigs.Size = new Size(714, 145);
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
            programNumberDataGridViewTextBoxColumn.HeaderText = "ProgramNumber";
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
            grpTextBlocks.Location = new Point(6, 287);
            grpTextBlocks.Margin = new Padding(3, 4, 3, 4);
            grpTextBlocks.Name = "grpTextBlocks";
            grpTextBlocks.Padding = new Padding(3, 4, 3, 4);
            grpTextBlocks.Size = new Size(720, 173);
            grpTextBlocks.TabIndex = 10;
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
            dgvTextBlocks.Columns.AddRange(new DataGridViewColumn[] { blockNumberDataGridViewTextBoxColumn, textDataGridViewTextBoxColumn, xDataGridViewTextBoxColumn, yDataGridViewTextBoxColumn, sizeDataGridViewTextBoxColumn, scaleDataGridViewTextBoxColumn });
            dgvTextBlocks.DataSource = bindingSourceTextBlockDto;
            dgvTextBlocks.Dock = DockStyle.Fill;
            dgvTextBlocks.Location = new Point(3, 24);
            dgvTextBlocks.Margin = new Padding(3, 4, 3, 4);
            dgvTextBlocks.Name = "dgvTextBlocks";
            dgvTextBlocks.ReadOnly = true;
            dgvTextBlocks.RowHeadersWidth = 51;
            dgvTextBlocks.Size = new Size(714, 145);
            dgvTextBlocks.TabIndex = 0;
            // 
            // blockNumberDataGridViewTextBoxColumn
            // 
            blockNumberDataGridViewTextBoxColumn.DataPropertyName = "BlockNumber";
            blockNumberDataGridViewTextBoxColumn.HeaderText = "BlockNumber";
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
            // bindingSourceTextBlockDto
            // 
            bindingSourceTextBlockDto.DataSource = typeof(Models.TextBlockDto);
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.FromArgb(0, 120, 215);
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(12, 675);
            btnSend.Margin = new Padding(3, 4, 3, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(229, 53);
            btnSend.TabIndex = 11;
            btnSend.Text = "SEND TO DEVICES";
            btnSend.UseVisualStyleBackColor = false;
            // 
            // btnRetry
            // 
            btnRetry.Location = new Point(247, 675);
            btnRetry.Margin = new Padding(3, 4, 3, 4);
            btnRetry.Name = "btnRetry";
            btnRetry.Size = new Size(137, 53);
            btnRetry.TabIndex = 12;
            btnRetry.Text = "Retry Failed";
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
            pnlDetail.ResumeLayout(false);
            pnlDetail.PerformLayout();
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
        private DataGridViewTextBoxColumn blockNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn textDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn xDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn yDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scaleDataGridViewTextBoxColumn;
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
    }
}