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
            orderNoDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            customerNameDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            typeDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            qtyDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            statusDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            bindingSourceJobCompleted = new BindingSource(components);
            timerPoll = new System.Windows.Forms.Timer(components);
            pnlJobs = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            button1 = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            dataGridView2 = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            st1_confirmation = new DataGridViewTextBoxColumn();
            st1_send_time = new DataGridViewTextBoxColumn();
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
            tabUv = new TabControl();
            tabUv1 = new TabPage();
            dgvUv1Blocks = new DataGridView();
            colUv1BlockNo = new DataGridViewTextBoxColumn();
            colUv1BlockText = new DataGridViewTextBoxColumn();
            colUv1BlockStatus = new DataGridViewTextBoxColumn();
            dgvUv1Fields = new DataGridView();
            colUv1Field = new DataGridViewTextBoxColumn();
            colUv1Value = new DataGridViewTextBoxColumn();
            pnlUv1Header = new Panel();
            lblUv1Dot = new Label();
            lblUv1Title = new Label();
            lblUv1Ip = new Label();
            lblUv1Status = new Label();
            tabUv2 = new TabPage();
            dgvUv2Blocks = new DataGridView();
            colUv2BlockNo = new DataGridViewTextBoxColumn();
            colUv2BlockText = new DataGridViewTextBoxColumn();
            colUv2BlockStatus = new DataGridViewTextBoxColumn();
            dgvUv2Fields = new DataGridView();
            colUv2Field = new DataGridViewTextBoxColumn();
            colUv2Value = new DataGridViewTextBoxColumn();
            pnlUv2Header = new Panel();
            lblUv2Dot = new Label();
            lblUv2Title = new Label();
            lblUv2Ip = new Label();
            lblUv2Status = new Label();
            dgvUvSummary = new DataGridView();
            colSumId = new DataGridViewTextBoxColumn();
            colSumMachine = new DataGridViewTextBoxColumn();
            colSumProgram = new DataGridViewTextBoxColumn();
            colSumLot = new DataGridViewTextBoxColumn();
            colSumName = new DataGridViewTextBoxColumn();
            bindingSourceUVinkjet = new BindingSource(components);
            pnlButtons = new FlowLayoutPanel();
            btnSendMk1Mk2 = new Button();
            btnSendUV1 = new Button();
            btnSendMk3 = new Button();
            btnSendUV2 = new Button();
            textBlocksBindingSource = new BindingSource(components);
            printJobBindingSource = new BindingSource(components);
            tabControl.SuspendLayout();
            tabList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceJobCompleted).BeginInit();
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
            tabUv.SuspendLayout();
            tabUv1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUv1Blocks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvUv1Fields).BeginInit();
            pnlUv1Header.SuspendLayout();
            tabUv2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUv2Blocks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvUv2Fields).BeginInit();
            pnlUv2Header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUvSummary).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).BeginInit();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textBlocksBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)printJobBindingSource).BeginInit();
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
            tabList.Location = new Point(4, 34);
            tabList.Name = "tabList";
            tabList.Padding = new Padding(3);
            tabList.Size = new Size(445, 284);
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
            dgvList.Size = new Size(439, 278);
            dgvList.TabIndex = 0;
            dgvList.CellClick += dgvList_CellClick;
            dgvList.CellContentClick += dgvList_CellContentClick;
            // 
            // orderNoDataGridViewTextBoxColumn
            // 
            orderNoDataGridViewTextBoxColumn.DataPropertyName = "OrderNo";
            orderNoDataGridViewTextBoxColumn.HeaderText = "Order No.";
            orderNoDataGridViewTextBoxColumn.FillWeight = 130F;
            orderNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            orderNoDataGridViewTextBoxColumn.Name = "orderNoDataGridViewTextBoxColumn";
            orderNoDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // customerNameDataGridViewTextBoxColumn
            // 
            customerNameDataGridViewTextBoxColumn.DataPropertyName = "CustomerName";
            customerNameDataGridViewTextBoxColumn.HeaderText = "Customer";
            customerNameDataGridViewTextBoxColumn.FillWeight = 140F;
            customerNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            customerNameDataGridViewTextBoxColumn.Name = "customerNameDataGridViewTextBoxColumn";
            customerNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn.HeaderText = "Type";
            typeDataGridViewTextBoxColumn.FillWeight = 60F;
            typeDataGridViewTextBoxColumn.MinimumWidth = 6;
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // qtyDataGridViewTextBoxColumn
            // 
            qtyDataGridViewTextBoxColumn.DataPropertyName = "Qty";
            qtyDataGridViewTextBoxColumn.HeaderText = "Qty";
            qtyDataGridViewTextBoxColumn.FillWeight = 50F;
            qtyDataGridViewTextBoxColumn.MinimumWidth = 6;
            qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            qtyDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.FillWeight = 90F;
            statusDataGridViewTextBoxColumn.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // stationDataGridViewTextBoxColumn
            // 
            stationDataGridViewTextBoxColumn.DataPropertyName = "Station";
            stationDataGridViewTextBoxColumn.HeaderText = "Station";
            stationDataGridViewTextBoxColumn.FillWeight = 70F;
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
            tabHistory.Location = new Point(4, 34);
            tabHistory.Name = "tabHistory";
            tabHistory.Padding = new Padding(3);
            tabHistory.Size = new Size(445, 284);
            tabHistory.TabIndex = 1;
            tabHistory.Text = "History";
            // 
            // dgvHistory
            // 
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.AutoGenerateColumns = false;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistory.ColumnHeadersHeight = 29;
            dgvHistory.Columns.AddRange(new DataGridViewColumn[] { orderNoDataGridViewTextBoxColumn1, customerNameDataGridViewTextBoxColumn1, typeDataGridViewTextBoxColumn1, qtyDataGridViewTextBoxColumn1, statusDataGridViewTextBoxColumn1 });
            dgvHistory.DataSource = bindingSourceJobCompleted;
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.Location = new Point(3, 3);
            dgvHistory.Name = "dgvHistory";
            dgvHistory.ReadOnly = true;
            dgvHistory.RowHeadersWidth = 51;
            dgvHistory.Size = new Size(439, 278);
            dgvHistory.TabIndex = 0;
            dgvHistory.CellClick += dgvHistory_CellClick;
            // 
            // orderNoDataGridViewTextBoxColumn1
            // 
            orderNoDataGridViewTextBoxColumn1.DataPropertyName = "OrderNo";
            orderNoDataGridViewTextBoxColumn1.HeaderText = "Order";
            orderNoDataGridViewTextBoxColumn1.FillWeight = 130F;
            orderNoDataGridViewTextBoxColumn1.MinimumWidth = 6;
            orderNoDataGridViewTextBoxColumn1.Name = "orderNoDataGridViewTextBoxColumn1";
            orderNoDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // customerNameDataGridViewTextBoxColumn1
            // 
            customerNameDataGridViewTextBoxColumn1.DataPropertyName = "CustomerName";
            customerNameDataGridViewTextBoxColumn1.HeaderText = "CustomerName";
            customerNameDataGridViewTextBoxColumn1.FillWeight = 140F;
            customerNameDataGridViewTextBoxColumn1.MinimumWidth = 6;
            customerNameDataGridViewTextBoxColumn1.Name = "customerNameDataGridViewTextBoxColumn1";
            customerNameDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn1
            // 
            typeDataGridViewTextBoxColumn1.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn1.HeaderText = "Type";
            typeDataGridViewTextBoxColumn1.FillWeight = 60F;
            typeDataGridViewTextBoxColumn1.MinimumWidth = 6;
            typeDataGridViewTextBoxColumn1.Name = "typeDataGridViewTextBoxColumn1";
            typeDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // qtyDataGridViewTextBoxColumn1
            // 
            qtyDataGridViewTextBoxColumn1.DataPropertyName = "Qty";
            qtyDataGridViewTextBoxColumn1.HeaderText = "Qty";
            qtyDataGridViewTextBoxColumn1.FillWeight = 50F;
            qtyDataGridViewTextBoxColumn1.MinimumWidth = 6;
            qtyDataGridViewTextBoxColumn1.Name = "qtyDataGridViewTextBoxColumn1";
            qtyDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // statusDataGridViewTextBoxColumn1
            // 
            statusDataGridViewTextBoxColumn1.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn1.HeaderText = "Status";
            statusDataGridViewTextBoxColumn1.FillWeight = 90F;
            statusDataGridViewTextBoxColumn1.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn1.Name = "statusDataGridViewTextBoxColumn1";
            statusDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // bindingSourceJobCompleted
            // 
            bindingSourceJobCompleted.DataSource = typeof(PrintJob);
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
            dataGridView2.CellClick += dataGridView2_CellClick;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.DataPropertyName = "OrderNo";
            dataGridViewTextBoxColumn1.HeaderText = "Order No.";
            dataGridViewTextBoxColumn1.FillWeight = 120F;
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.DataPropertyName = "CustomerName";
            dataGridViewTextBoxColumn2.HeaderText = "Customer";
            dataGridViewTextBoxColumn2.FillWeight = 120F;
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.DataPropertyName = "Type";
            dataGridViewTextBoxColumn3.HeaderText = "Type";
            dataGridViewTextBoxColumn3.FillWeight = 55F;
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.DataPropertyName = "Qty";
            dataGridViewTextBoxColumn4.HeaderText = "Qty";
            dataGridViewTextBoxColumn4.FillWeight = 45F;
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // st1_confirmation
            // 
            st1_confirmation.DataPropertyName = "st1_confirmation";
            st1_confirmation.HeaderText = "Status";
            st1_confirmation.FillWeight = 80F;
            st1_confirmation.MinimumWidth = 6;
            st1_confirmation.Name = "st1_confirmation";
            st1_confirmation.ReadOnly = true;
            // 
            // st1_send_time
            // 
            st1_send_time.DataPropertyName = "st1_send_time";
            st1_send_time.HeaderText = "Send time";
            st1_send_time.FillWeight = 130F;
            st1_send_time.MinimumWidth = 6;
            st1_send_time.Name = "st1_send_time";
            st1_send_time.ReadOnly = true;
            // 
            // bindingSourceJobSt3
            // 
            bindingSourceJobSt3.DataSource = typeof(PrintJob);
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
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
            btnRefresh.Font = new Font("Segoe UI", 10F);
            btnRefresh.Location = new Point(6, 730);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(459, 36);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            // 
            // lblJobsTitle
            // 
            lblJobsTitle.Dock = DockStyle.Top;
            lblJobsTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
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
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tblDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tblDetailLayout.RowStyles.Add(new RowStyle());
            tblDetailLayout.Size = new Size(756, 756);
            tblDetailLayout.TabIndex = 0;
            // 
            // lblDetailTitle
            // 
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblDetailTitle.Location = new Point(3, 6);
            lblDetailTitle.Margin = new Padding(3, 6, 3, 6);
            lblDetailTitle.Name = "lblDetailTitle";
            lblDetailTitle.Size = new Size(117, 30);
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
            pnlJobInfo.Location = new Point(3, 45);
            pnlJobInfo.Name = "pnlJobInfo";
            pnlJobInfo.RowCount = 2;
            pnlJobInfo.RowStyles.Add(new RowStyle());
            pnlJobInfo.RowStyles.Add(new RowStyle());
            pnlJobInfo.Size = new Size(750, 74);
            pnlJobInfo.TabIndex = 1;
            // 
            // lblBarcode
            // 
            lblBarcode.Anchor = AnchorStyles.Left;
            lblBarcode.AutoSize = true;
            lblBarcode.Font = new Font("Segoe UI", 10F);
            lblBarcode.Location = new Point(3, 6);
            lblBarcode.Margin = new Padding(3, 6, 3, 6);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(80, 25);
            lblBarcode.TabIndex = 0;
            lblBarcode.Text = "Barcode:";
            // 
            // txtBarcode
            // 
            txtBarcode.Dock = DockStyle.Fill;
            txtBarcode.Font = new Font("Segoe UI", 10F);
            txtBarcode.Location = new Point(89, 3);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.ReadOnly = true;
            txtBarcode.Size = new Size(158, 31);
            txtBarcode.TabIndex = 1;
            // 
            // lblLot
            // 
            lblLot.Anchor = AnchorStyles.Left;
            lblLot.AutoSize = true;
            lblLot.Font = new Font("Segoe UI", 10F);
            lblLot.Location = new Point(253, 6);
            lblLot.Margin = new Padding(3, 6, 3, 6);
            lblLot.Name = "lblLot";
            lblLot.Size = new Size(41, 25);
            lblLot.TabIndex = 2;
            lblLot.Text = "Lot:";
            // 
            // txtLot
            // 
            txtLot.Dock = DockStyle.Fill;
            txtLot.Font = new Font("Segoe UI", 10F);
            txtLot.Location = new Point(300, 3);
            txtLot.Name = "txtLot";
            txtLot.ReadOnly = true;
            txtLot.Size = new Size(130, 31);
            txtLot.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F);
            lblStatus.Location = new Point(436, 6);
            lblStatus.Margin = new Padding(3, 6, 3, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(64, 25);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Status:";
            // 
            // txtStatus
            // 
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.Font = new Font("Segoe UI", 10F);
            txtStatus.Location = new Point(506, 3);
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(103, 31);
            txtStatus.TabIndex = 5;
            // 
            // lblPattern
            // 
            lblPattern.Anchor = AnchorStyles.Left;
            lblPattern.AutoSize = true;
            lblPattern.Font = new Font("Segoe UI", 10F);
            lblPattern.Location = new Point(3, 43);
            lblPattern.Margin = new Padding(3, 6, 3, 6);
            lblPattern.Name = "lblPattern";
            lblPattern.Size = new Size(71, 25);
            lblPattern.TabIndex = 6;
            lblPattern.Text = "Pattern:";
            // 
            // txtPattern
            // 
            txtPattern.Dock = DockStyle.Fill;
            txtPattern.Font = new Font("Segoe UI", 10F);
            txtPattern.Location = new Point(89, 40);
            txtPattern.Name = "txtPattern";
            txtPattern.ReadOnly = true;
            txtPattern.Size = new Size(158, 31);
            txtPattern.TabIndex = 7;
            // 
            // grpInkjetConfigs
            // 
            grpInkjetConfigs.Controls.Add(dgvConfigs);
            grpInkjetConfigs.Dock = DockStyle.Fill;
            grpInkjetConfigs.Location = new Point(3, 125);
            grpInkjetConfigs.Name = "grpInkjetConfigs";
            grpInkjetConfigs.Padding = new Padding(3, 4, 3, 4);
            grpInkjetConfigs.Size = new Size(750, 182);
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
            dgvConfigs.Location = new Point(3, 28);
            dgvConfigs.Name = "dgvConfigs";
            dgvConfigs.ReadOnly = true;
            dgvConfigs.RowHeadersWidth = 51;
            dgvConfigs.Size = new Size(744, 150);
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
            programNameDataGridViewTextBoxColumn.FillWeight = 180F;
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
            grpTextBlocks.Location = new Point(3, 313);
            grpTextBlocks.Name = "grpTextBlocks";
            grpTextBlocks.Padding = new Padding(3, 4, 3, 4);
            grpTextBlocks.Size = new Size(750, 182);
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
            dgvTextBlocks.Location = new Point(3, 28);
            dgvTextBlocks.Name = "dgvTextBlocks";
            dgvTextBlocks.ReadOnly = true;
            dgvTextBlocks.RowHeadersWidth = 51;
            dgvTextBlocks.Size = new Size(744, 150);
            dgvTextBlocks.TabIndex = 0;
            // 
            // blockNumberDataGridViewTextBoxColumn
            // 
            blockNumberDataGridViewTextBoxColumn.DataPropertyName = "BlockNumber";
            blockNumberDataGridViewTextBoxColumn.HeaderText = "Block#";
            blockNumberDataGridViewTextBoxColumn.FillWeight = 60F;
            blockNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            blockNumberDataGridViewTextBoxColumn.Name = "blockNumberDataGridViewTextBoxColumn";
            blockNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // textDataGridViewTextBoxColumn
            // 
            textDataGridViewTextBoxColumn.DataPropertyName = "Text";
            textDataGridViewTextBoxColumn.HeaderText = "Text";
            textDataGridViewTextBoxColumn.FillWeight = 200F;
            textDataGridViewTextBoxColumn.MinimumWidth = 6;
            textDataGridViewTextBoxColumn.Name = "textDataGridViewTextBoxColumn";
            textDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // xDataGridViewTextBoxColumn
            // 
            xDataGridViewTextBoxColumn.DataPropertyName = "X";
            xDataGridViewTextBoxColumn.HeaderText = "X";
            xDataGridViewTextBoxColumn.FillWeight = 60F;
            xDataGridViewTextBoxColumn.MinimumWidth = 6;
            xDataGridViewTextBoxColumn.Name = "xDataGridViewTextBoxColumn";
            xDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // yDataGridViewTextBoxColumn
            // 
            yDataGridViewTextBoxColumn.DataPropertyName = "Y";
            yDataGridViewTextBoxColumn.HeaderText = "Y";
            yDataGridViewTextBoxColumn.FillWeight = 60F;
            yDataGridViewTextBoxColumn.MinimumWidth = 6;
            yDataGridViewTextBoxColumn.Name = "yDataGridViewTextBoxColumn";
            yDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            sizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            sizeDataGridViewTextBoxColumn.FillWeight = 60F;
            sizeDataGridViewTextBoxColumn.MinimumWidth = 6;
            sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            sizeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scaleDataGridViewTextBoxColumn
            // 
            scaleDataGridViewTextBoxColumn.DataPropertyName = "Scale";
            scaleDataGridViewTextBoxColumn.HeaderText = "Scale";
            scaleDataGridViewTextBoxColumn.FillWeight = 60F;
            scaleDataGridViewTextBoxColumn.MinimumWidth = 6;
            scaleDataGridViewTextBoxColumn.Name = "scaleDataGridViewTextBoxColumn";
            scaleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // RuleResult
            // 
            RuleResult.DataPropertyName = "RuleResult";
            RuleResult.HeaderText = "RuleResult";
            RuleResult.FillWeight = 150F;
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
            groupBox1.Controls.Add(tabUv);
            groupBox1.Controls.Add(dgvUvSummary);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 501);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(750, 182);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Inkjet UV";
            //
            // tabUv
            //
            tabUv.Controls.Add(tabUv1);
            tabUv.Controls.Add(tabUv2);
            tabUv.Dock = DockStyle.Fill;
            tabUv.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            tabUv.Location = new Point(3, 20);
            tabUv.Name = "tabUv";
            tabUv.SelectedIndex = 0;
            tabUv.Size = new Size(744, 66);
            tabUv.TabIndex = 2;
            //
            // tabUv1
            //
            tabUv1.Controls.Add(dgvUv1Blocks);
            tabUv1.Controls.Add(dgvUv1Fields);
            tabUv1.Controls.Add(pnlUv1Header);
            tabUv1.Controls.Add(lblUv1Status);
            tabUv1.Location = new Point(4, 27);
            tabUv1.Name = "tabUv1";
            tabUv1.Padding = new Padding(3);
            tabUv1.Size = new Size(736, 1);
            tabUv1.TabIndex = 0;
            tabUv1.Text = "UV1 — MK-063";
            tabUv1.UseVisualStyleBackColor = true;
            //
            // dgvUv1Blocks
            //
            dgvUv1Blocks.AllowUserToAddRows = false;
            dgvUv1Blocks.AllowUserToDeleteRows = false;
            dgvUv1Blocks.BackgroundColor = Color.White;
            dgvUv1Blocks.ColumnHeadersHeight = 26;
            dgvUv1Blocks.Columns.AddRange(new DataGridViewColumn[] { colUv1BlockNo, colUv1BlockText, colUv1BlockStatus });
            dgvUv1Blocks.Dock = DockStyle.Fill;
            dgvUv1Blocks.Location = new Point(3, 51);
            dgvUv1Blocks.Name = "dgvUv1Blocks";
            dgvUv1Blocks.ReadOnly = true;
            dgvUv1Blocks.RowHeadersVisible = false;
            dgvUv1Blocks.RowTemplate.Height = 24;
            dgvUv1Blocks.Size = new Size(730, 1);
            dgvUv1Blocks.TabIndex = 2;
            //
            // colUv1BlockNo
            //
            colUv1BlockNo.HeaderText = "Block#";
            colUv1BlockNo.FillWeight = 50F;
            colUv1BlockNo.Name = "colUv1BlockNo";
            colUv1BlockNo.ReadOnly = true;
            //
            // colUv1BlockText
            //
            colUv1BlockText.HeaderText = "Text";
            colUv1BlockText.FillWeight = 200F;
            colUv1BlockText.Name = "colUv1BlockText";
            colUv1BlockText.ReadOnly = true;
            //
            // colUv1BlockStatus
            //
            colUv1BlockStatus.HeaderText = "Status";
            colUv1BlockStatus.FillWeight = 60F;
            colUv1BlockStatus.Name = "colUv1BlockStatus";
            colUv1BlockStatus.ReadOnly = true;
            //
            // dgvUv1Fields
            //
            dgvUv1Fields.AllowUserToAddRows = false;
            dgvUv1Fields.AllowUserToDeleteRows = false;
            dgvUv1Fields.BackgroundColor = Color.White;
            dgvUv1Fields.ColumnHeadersHeight = 26;
            dgvUv1Fields.Columns.AddRange(new DataGridViewColumn[] { colUv1Field, colUv1Value });
            dgvUv1Fields.Dock = DockStyle.Top;
            dgvUv1Fields.Location = new Point(3, 31);
            dgvUv1Fields.Name = "dgvUv1Fields";
            dgvUv1Fields.ReadOnly = true;
            dgvUv1Fields.RowHeadersVisible = false;
            dgvUv1Fields.RowTemplate.Height = 24;
            dgvUv1Fields.ScrollBars = ScrollBars.None;
            dgvUv1Fields.Size = new Size(730, 76);
            dgvUv1Fields.TabIndex = 1;
            //
            // colUv1Field
            //
            colUv1Field.HeaderText = "Field";
            colUv1Field.FillWeight = 80F;
            colUv1Field.Name = "colUv1Field";
            colUv1Field.ReadOnly = true;
            //
            // colUv1Value
            //
            colUv1Value.HeaderText = "Value";
            colUv1Value.FillWeight = 200F;
            colUv1Value.Name = "colUv1Value";
            colUv1Value.ReadOnly = true;
            //
            // pnlUv1Header
            //
            pnlUv1Header.Controls.Add(lblUv1Dot);
            pnlUv1Header.Controls.Add(lblUv1Title);
            pnlUv1Header.Controls.Add(lblUv1Ip);
            pnlUv1Header.Dock = DockStyle.Top;
            pnlUv1Header.Location = new Point(3, 3);
            pnlUv1Header.Name = "pnlUv1Header";
            pnlUv1Header.Size = new Size(730, 28);
            pnlUv1Header.TabIndex = 0;
            //
            // lblUv1Dot
            //
            lblUv1Dot.BackColor = Color.Gray;
            lblUv1Dot.Location = new Point(6, 6);
            lblUv1Dot.Name = "lblUv1Dot";
            lblUv1Dot.Size = new Size(16, 16);
            lblUv1Dot.TabIndex = 0;
            //
            // lblUv1Title
            //
            lblUv1Title.AutoSize = true;
            lblUv1Title.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUv1Title.Location = new Point(28, 7);
            lblUv1Title.Name = "lblUv1Title";
            lblUv1Title.Size = new Size(140, 15);
            lblUv1Title.TabIndex = 1;
            lblUv1Title.Text = "UV1 — MK-063 (Plate)";
            //
            // lblUv1Ip
            //
            lblUv1Ip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUv1Ip.AutoSize = true;
            lblUv1Ip.ForeColor = Color.Gray;
            lblUv1Ip.Location = new Point(580, 7);
            lblUv1Ip.Name = "lblUv1Ip";
            lblUv1Ip.Size = new Size(120, 15);
            lblUv1Ip.TabIndex = 2;
            lblUv1Ip.Text = "192.168.0.3 : 10086";
            lblUv1Ip.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblUv1Status
            //
            lblUv1Status.BackColor = Color.FromArgb(240, 240, 240);
            lblUv1Status.Dock = DockStyle.Bottom;
            lblUv1Status.Font = new Font("Segoe UI", 8F);
            lblUv1Status.ForeColor = Color.DimGray;
            lblUv1Status.Location = new Point(3, 0);
            lblUv1Status.Name = "lblUv1Status";
            lblUv1Status.Size = new Size(730, 20);
            lblUv1Status.TabIndex = 3;
            lblUv1Status.Text = "TCP: — | KEY: —";
            lblUv1Status.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tabUv2
            //
            tabUv2.Controls.Add(dgvUv2Blocks);
            tabUv2.Controls.Add(dgvUv2Fields);
            tabUv2.Controls.Add(pnlUv2Header);
            tabUv2.Controls.Add(lblUv2Status);
            tabUv2.Location = new Point(4, 27);
            tabUv2.Name = "tabUv2";
            tabUv2.Padding = new Padding(3);
            tabUv2.Size = new Size(736, 1);
            tabUv2.TabIndex = 1;
            tabUv2.Text = "UV2 — MK-067";
            tabUv2.UseVisualStyleBackColor = true;
            //
            // dgvUv2Blocks
            //
            dgvUv2Blocks.AllowUserToAddRows = false;
            dgvUv2Blocks.AllowUserToDeleteRows = false;
            dgvUv2Blocks.BackgroundColor = Color.White;
            dgvUv2Blocks.ColumnHeadersHeight = 26;
            dgvUv2Blocks.Columns.AddRange(new DataGridViewColumn[] { colUv2BlockNo, colUv2BlockText, colUv2BlockStatus });
            dgvUv2Blocks.Dock = DockStyle.Fill;
            dgvUv2Blocks.Location = new Point(3, 51);
            dgvUv2Blocks.Name = "dgvUv2Blocks";
            dgvUv2Blocks.ReadOnly = true;
            dgvUv2Blocks.RowHeadersVisible = false;
            dgvUv2Blocks.RowTemplate.Height = 24;
            dgvUv2Blocks.Size = new Size(730, 1);
            dgvUv2Blocks.TabIndex = 2;
            //
            // colUv2BlockNo
            //
            colUv2BlockNo.HeaderText = "Block#";
            colUv2BlockNo.FillWeight = 50F;
            colUv2BlockNo.Name = "colUv2BlockNo";
            colUv2BlockNo.ReadOnly = true;
            //
            // colUv2BlockText
            //
            colUv2BlockText.HeaderText = "Text";
            colUv2BlockText.FillWeight = 200F;
            colUv2BlockText.Name = "colUv2BlockText";
            colUv2BlockText.ReadOnly = true;
            //
            // colUv2BlockStatus
            //
            colUv2BlockStatus.HeaderText = "Status";
            colUv2BlockStatus.FillWeight = 60F;
            colUv2BlockStatus.Name = "colUv2BlockStatus";
            colUv2BlockStatus.ReadOnly = true;
            //
            // dgvUv2Fields
            //
            dgvUv2Fields.AllowUserToAddRows = false;
            dgvUv2Fields.AllowUserToDeleteRows = false;
            dgvUv2Fields.BackgroundColor = Color.White;
            dgvUv2Fields.ColumnHeadersHeight = 26;
            dgvUv2Fields.Columns.AddRange(new DataGridViewColumn[] { colUv2Field, colUv2Value });
            dgvUv2Fields.Dock = DockStyle.Top;
            dgvUv2Fields.Location = new Point(3, 31);
            dgvUv2Fields.Name = "dgvUv2Fields";
            dgvUv2Fields.ReadOnly = true;
            dgvUv2Fields.RowHeadersVisible = false;
            dgvUv2Fields.RowTemplate.Height = 24;
            dgvUv2Fields.ScrollBars = ScrollBars.None;
            dgvUv2Fields.Size = new Size(730, 76);
            dgvUv2Fields.TabIndex = 1;
            //
            // colUv2Field
            //
            colUv2Field.HeaderText = "Field";
            colUv2Field.FillWeight = 80F;
            colUv2Field.Name = "colUv2Field";
            colUv2Field.ReadOnly = true;
            //
            // colUv2Value
            //
            colUv2Value.HeaderText = "Value";
            colUv2Value.FillWeight = 200F;
            colUv2Value.Name = "colUv2Value";
            colUv2Value.ReadOnly = true;
            //
            // pnlUv2Header
            //
            pnlUv2Header.Controls.Add(lblUv2Dot);
            pnlUv2Header.Controls.Add(lblUv2Title);
            pnlUv2Header.Controls.Add(lblUv2Ip);
            pnlUv2Header.Dock = DockStyle.Top;
            pnlUv2Header.Location = new Point(3, 3);
            pnlUv2Header.Name = "pnlUv2Header";
            pnlUv2Header.Size = new Size(730, 28);
            pnlUv2Header.TabIndex = 0;
            //
            // lblUv2Dot
            //
            lblUv2Dot.BackColor = Color.Gray;
            lblUv2Dot.Location = new Point(6, 6);
            lblUv2Dot.Name = "lblUv2Dot";
            lblUv2Dot.Size = new Size(16, 16);
            lblUv2Dot.TabIndex = 0;
            //
            // lblUv2Title
            //
            lblUv2Title.AutoSize = true;
            lblUv2Title.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUv2Title.Location = new Point(28, 7);
            lblUv2Title.Name = "lblUv2Title";
            lblUv2Title.Size = new Size(140, 15);
            lblUv2Title.TabIndex = 1;
            lblUv2Title.Text = "UV2 — MK-067 (Shim)";
            //
            // lblUv2Ip
            //
            lblUv2Ip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUv2Ip.AutoSize = true;
            lblUv2Ip.ForeColor = Color.Gray;
            lblUv2Ip.Location = new Point(580, 7);
            lblUv2Ip.Name = "lblUv2Ip";
            lblUv2Ip.Size = new Size(120, 15);
            lblUv2Ip.TabIndex = 2;
            lblUv2Ip.Text = "— : —";
            lblUv2Ip.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblUv2Status
            //
            lblUv2Status.BackColor = Color.FromArgb(240, 240, 240);
            lblUv2Status.Dock = DockStyle.Bottom;
            lblUv2Status.Font = new Font("Segoe UI", 8F);
            lblUv2Status.ForeColor = Color.DimGray;
            lblUv2Status.Location = new Point(3, 0);
            lblUv2Status.Name = "lblUv2Status";
            lblUv2Status.Size = new Size(730, 20);
            lblUv2Status.TabIndex = 3;
            lblUv2Status.Text = "TCP: — | KEY: —";
            lblUv2Status.TextAlign = ContentAlignment.MiddleLeft;
            //
            // dgvUvSummary
            //
            dgvUvSummary.AllowUserToAddRows = false;
            dgvUvSummary.AllowUserToDeleteRows = false;
            dgvUvSummary.AutoGenerateColumns = false;
            dgvUvSummary.BackgroundColor = Color.White;
            dgvUvSummary.ColumnHeadersHeight = 26;
            dgvUvSummary.Columns.AddRange(new DataGridViewColumn[] { colSumId, colSumMachine, colSumProgram, colSumLot, colSumName });
            dgvUvSummary.DataSource = bindingSourceUVinkjet;
            dgvUvSummary.Dock = DockStyle.Bottom;
            dgvUvSummary.Location = new Point(3, 86);
            dgvUvSummary.Name = "dgvUvSummary";
            dgvUvSummary.ReadOnly = true;
            dgvUvSummary.RowHeadersVisible = false;
            dgvUvSummary.RowTemplate.Height = 24;
            dgvUvSummary.Size = new Size(744, 76);
            dgvUvSummary.TabIndex = 1;
            //
            // colSumId
            //
            colSumId.DataPropertyName = "Id";
            colSumId.HeaderText = "Id";
            colSumId.FillWeight = 30F;
            colSumId.Name = "colSumId";
            colSumId.ReadOnly = true;
            //
            // colSumMachine
            //
            colSumMachine.DataPropertyName = "Machine";
            colSumMachine.HeaderText = "Machine";
            colSumMachine.FillWeight = 70F;
            colSumMachine.Name = "colSumMachine";
            colSumMachine.ReadOnly = true;
            //
            // colSumProgram
            //
            colSumProgram.DataPropertyName = "ProgramName";
            colSumProgram.HeaderText = "ProgramName";
            colSumProgram.FillWeight = 150F;
            colSumProgram.Name = "colSumProgram";
            colSumProgram.ReadOnly = true;
            //
            // colSumLot
            //
            colSumLot.DataPropertyName = "Lot";
            colSumLot.HeaderText = "Lot";
            colSumLot.FillWeight = 80F;
            colSumLot.Name = "colSumLot";
            colSumLot.ReadOnly = true;
            //
            // colSumName
            //
            colSumName.DataPropertyName = "Name";
            colSumName.HeaderText = "Name";
            colSumName.FillWeight = 150F;
            colSumName.Name = "colSumName";
            colSumName.ReadOnly = true;
            //
            // bindingSourceUVinkjet
            //
            bindingSourceUVinkjet.DataSource = typeof(Models.UVinkjet);
            // 
            // pnlButtons
            // 
            pnlButtons.AutoSize = true;
            pnlButtons.Controls.Add(btnSendUV2);
            pnlButtons.Controls.Add(btnSendMk3);
            pnlButtons.Controls.Add(btnSendUV1);
            pnlButtons.Controls.Add(btnSendMk1Mk2);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            pnlButtons.Location = new Point(3, 689);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(0, 4, 0, 4);
            pnlButtons.Size = new Size(750, 44);
            pnlButtons.TabIndex = 5;
            //
            // btnSendMk1Mk2
            //
            btnSendMk1Mk2.Font = new Font("Segoe UI", 9F);
            btnSendMk1Mk2.Location = new Point(637, 7);
            btnSendMk1Mk2.Name = "btnSendMk1Mk2";
            btnSendMk1Mk2.Size = new Size(110, 30);
            btnSendMk1Mk2.TabIndex = 2;
            btnSendMk1Mk2.Text = "ส่งหา MK1,MK2";
            btnSendMk1Mk2.Click += btnSendMk1Mk2_Click;
            //
            // btnSendUV1
            //
            btnSendUV1.Font = new Font("Segoe UI", 9F);
            btnSendUV1.Location = new Point(531, 7);
            btnSendUV1.Name = "btnSendUV1";
            btnSendUV1.Size = new Size(100, 30);
            btnSendUV1.TabIndex = 4;
            btnSendUV1.Text = "ส่งหา UV1";
            btnSendUV1.Click += btnSendUV1_Click;
            //
            // btnSendMk3
            //
            btnSendMk3.Font = new Font("Segoe UI", 9F);
            btnSendMk3.Location = new Point(425, 7);
            btnSendMk3.Name = "btnSendMk3";
            btnSendMk3.Size = new Size(100, 30);
            btnSendMk3.TabIndex = 3;
            btnSendMk3.Text = "ส่งหา MK3,MK4";
            btnSendMk3.Click += btnSendMk3_Click;
            //
            // btnSendUV2
            //
            btnSendUV2.Font = new Font("Segoe UI", 9F);
            btnSendUV2.Location = new Point(319, 7);
            btnSendUV2.Name = "btnSendUV2";
            btnSendUV2.Size = new Size(100, 30);
            btnSendUV2.TabIndex = 5;
            btnSendUV2.Text = "ส่งหา UV2";
            btnSendUV2.Click += btnSendUV2_Click_1;
            // 
            // textBlocksBindingSource
            // 
            textBlocksBindingSource.DataMember = "TextBlocks";
            textBlocksBindingSource.DataSource = bindSourceInkjetConfigDto;
            // 
            // printJobBindingSource
            // 
            printJobBindingSource.DataSource = typeof(PrintJob);
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
            ((System.ComponentModel.ISupportInitialize)bindingSourceJobCompleted).EndInit();
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
            tabUv.ResumeLayout(false);
            tabUv1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUv1Blocks).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvUv1Fields).EndInit();
            pnlUv1Header.ResumeLayout(false);
            pnlUv1Header.PerformLayout();
            tabUv2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUv2Blocks).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvUv2Fields).EndInit();
            pnlUv2Header.ResumeLayout(false);
            pnlUv2Header.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUvSummary).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSourceUVinkjet).EndInit();
            pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)textBlocksBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)printJobBindingSource).EndInit();
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
        private BindingSource bindingSourceUVinkjet;
        private Button btnSendMk1Mk2;
        private Button btnSendUV2;
        private Button btnSendMk3;
        private Button btnSendUV1;

        // Inkjet UV — Tabs
        private TabControl tabUv;
        private TabPage tabUv1;
        private Panel pnlUv1Header;
        private Label lblUv1Dot;
        private Label lblUv1Title;
        private Label lblUv1Ip;
        private Label lblUv1Status;
        private DataGridView dgvUv1Fields;
        private DataGridViewTextBoxColumn colUv1Field;
        private DataGridViewTextBoxColumn colUv1Value;
        private DataGridView dgvUv1Blocks;
        private DataGridViewTextBoxColumn colUv1BlockNo;
        private DataGridViewTextBoxColumn colUv1BlockText;
        private DataGridViewTextBoxColumn colUv1BlockStatus;
        private TabPage tabUv2;
        private Panel pnlUv2Header;
        private Label lblUv2Dot;
        private Label lblUv2Title;
        private Label lblUv2Ip;
        private Label lblUv2Status;
        private DataGridView dgvUv2Fields;
        private DataGridViewTextBoxColumn colUv2Field;
        private DataGridViewTextBoxColumn colUv2Value;
        private DataGridView dgvUv2Blocks;
        private DataGridViewTextBoxColumn colUv2BlockNo;
        private DataGridViewTextBoxColumn colUv2BlockText;
        private DataGridViewTextBoxColumn colUv2BlockStatus;

        // Inkjet UV — Summary
        private DataGridView dgvUvSummary;
        private DataGridViewTextBoxColumn colSumId;
        private DataGridViewTextBoxColumn colSumMachine;
        private DataGridViewTextBoxColumn colSumProgram;
        private DataGridViewTextBoxColumn colSumLot;
        private DataGridViewTextBoxColumn colSumName;
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
        private BindingSource bindingSourceJobCompleted;
        private BindingSource printJobBindingSource;
        private DataGridViewTextBoxColumn orderNoDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn customerNameDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn1;
    }
}