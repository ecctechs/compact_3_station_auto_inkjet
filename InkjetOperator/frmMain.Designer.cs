namespace InkjetOperator;

partial class frmMain
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tmrPoll = new System.Windows.Forms.Timer(components);
        pnlConnection = new Panel();
        grpInkjet12 = new GroupBox();
        lblCom1 = new Label();
        cmbCom1 = new ComboBox();
        lblCom2 = new Label();
        cmbCom2 = new ComboBox();
        btnConnectRs232 = new Button();
        btnDisconnectRs232 = new Button();
        lblStatusIj1 = new Label();
        lblStatusIj2 = new Label();
        grpInkjet34 = new GroupBox();
        lblTcpHost = new Label();
        txtTcpHost = new TextBox();
        lblTcpPort = new Label();
        txtTcpPort = new TextBox();
        btnConnectTcp = new Button();
        btnDisconnectTcp = new Button();
        lblStatusIj3 = new Label();
        lblStatusIj4 = new Label();
        grpBackend = new GroupBox();
        lblApiUrl = new Label();
        txtApiUrl = new TextBox();
        btnApplyApi = new Button();
        lblApiStatus = new Label();
        pnlJobs = new Panel();
        dgvJobs = new DataGridView();
        btnRefresh = new Button();
        lblJobsTitle = new Label();
        pnlDetail = new Panel();
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
        grpTextBlocks = new GroupBox();
        dgvTextBlocks = new DataGridView();
        btnSend = new Button();
        btnRetry = new Button();
        pnlLog = new Panel();
        txtLog = new TextBox();
        lblLogTitle = new Label();
        button1 = new Button();
        pnlConnection.SuspendLayout();
        grpInkjet12.SuspendLayout();
        grpInkjet34.SuspendLayout();
        grpBackend.SuspendLayout();
        pnlJobs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvJobs).BeginInit();
        pnlDetail.SuspendLayout();
        grpInkjetConfigs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvConfigs).BeginInit();
        grpTextBlocks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).BeginInit();
        pnlLog.SuspendLayout();
        SuspendLayout();
        // 
        // tmrPoll
        // 
        tmrPoll.Interval = 5000;
        tmrPoll.Tick += tmrPoll_Tick;
        // 
        // pnlConnection
        // 
        pnlConnection.Controls.Add(button1);
        pnlConnection.Controls.Add(grpInkjet12);
        pnlConnection.Controls.Add(grpInkjet34);
        pnlConnection.Controls.Add(grpBackend);
        pnlConnection.Dock = DockStyle.Top;
        pnlConnection.Location = new Point(0, 0);
        pnlConnection.Margin = new Padding(3, 4, 3, 4);
        pnlConnection.Name = "pnlConnection";
        pnlConnection.Size = new Size(1143, 153);
        pnlConnection.TabIndex = 3;
        // 
        // grpInkjet12
        // 
        grpInkjet12.Controls.Add(lblCom1);
        grpInkjet12.Controls.Add(cmbCom1);
        grpInkjet12.Controls.Add(lblCom2);
        grpInkjet12.Controls.Add(cmbCom2);
        grpInkjet12.Controls.Add(btnConnectRs232);
        grpInkjet12.Controls.Add(btnDisconnectRs232);
        grpInkjet12.Controls.Add(lblStatusIj1);
        grpInkjet12.Controls.Add(lblStatusIj2);
        grpInkjet12.Location = new Point(6, 7);
        grpInkjet12.Margin = new Padding(3, 4, 3, 4);
        grpInkjet12.Name = "grpInkjet12";
        grpInkjet12.Padding = new Padding(3, 4, 3, 4);
        grpInkjet12.Size = new Size(714, 67);
        grpInkjet12.TabIndex = 0;
        grpInkjet12.TabStop = false;
        grpInkjet12.Text = "Inkjet 1 & 2 (RS232)";
        // 
        // lblCom1
        // 
        lblCom1.AutoSize = true;
        lblCom1.Location = new Point(11, 29);
        lblCom1.Name = "lblCom1";
        lblCom1.Size = new Size(84, 20);
        lblCom1.TabIndex = 0;
        lblCom1.Text = "COM1 (IJ1):";
        // 
        // cmbCom1
        // 
        cmbCom1.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCom1.Location = new Point(103, 25);
        cmbCom1.Margin = new Padding(3, 4, 3, 4);
        cmbCom1.Name = "cmbCom1";
        cmbCom1.Size = new Size(102, 28);
        cmbCom1.TabIndex = 1;
        // 
        // lblCom2
        // 
        lblCom2.AutoSize = true;
        lblCom2.Location = new Point(217, 29);
        lblCom2.Name = "lblCom2";
        lblCom2.Size = new Size(84, 20);
        lblCom2.TabIndex = 2;
        lblCom2.Text = "COM2 (IJ2):";
        // 
        // cmbCom2
        // 
        cmbCom2.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCom2.Location = new Point(309, 25);
        cmbCom2.Margin = new Padding(3, 4, 3, 4);
        cmbCom2.Name = "cmbCom2";
        cmbCom2.Size = new Size(102, 28);
        cmbCom2.TabIndex = 3;
        // 
        // btnConnectRs232
        // 
        btnConnectRs232.Location = new Point(423, 24);
        btnConnectRs232.Margin = new Padding(3, 4, 3, 4);
        btnConnectRs232.Name = "btnConnectRs232";
        btnConnectRs232.Size = new Size(86, 33);
        btnConnectRs232.TabIndex = 4;
        btnConnectRs232.Text = "Connect";
        btnConnectRs232.Click += btnConnectRs232_Click;
        // 
        // btnDisconnectRs232
        // 
        btnDisconnectRs232.Location = new Point(514, 24);
        btnDisconnectRs232.Margin = new Padding(3, 4, 3, 4);
        btnDisconnectRs232.Name = "btnDisconnectRs232";
        btnDisconnectRs232.Size = new Size(91, 33);
        btnDisconnectRs232.TabIndex = 5;
        btnDisconnectRs232.Text = "Disconnect";
        btnDisconnectRs232.Click += btnDisconnectRs232_Click;
        // 
        // lblStatusIj1
        // 
        lblStatusIj1.BackColor = Color.Gray;
        lblStatusIj1.ForeColor = Color.White;
        lblStatusIj1.Location = new Point(617, 29);
        lblStatusIj1.Name = "lblStatusIj1";
        lblStatusIj1.Size = new Size(40, 24);
        lblStatusIj1.TabIndex = 6;
        lblStatusIj1.Text = "IJ1";
        lblStatusIj1.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblStatusIj2
        // 
        lblStatusIj2.BackColor = Color.Gray;
        lblStatusIj2.ForeColor = Color.White;
        lblStatusIj2.Location = new Point(663, 29);
        lblStatusIj2.Name = "lblStatusIj2";
        lblStatusIj2.Size = new Size(40, 24);
        lblStatusIj2.TabIndex = 7;
        lblStatusIj2.Text = "IJ2";
        lblStatusIj2.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // grpInkjet34
        // 
        grpInkjet34.Controls.Add(lblTcpHost);
        grpInkjet34.Controls.Add(txtTcpHost);
        grpInkjet34.Controls.Add(lblTcpPort);
        grpInkjet34.Controls.Add(txtTcpPort);
        grpInkjet34.Controls.Add(btnConnectTcp);
        grpInkjet34.Controls.Add(btnDisconnectTcp);
        grpInkjet34.Controls.Add(lblStatusIj3);
        grpInkjet34.Controls.Add(lblStatusIj4);
        grpInkjet34.Location = new Point(6, 77);
        grpInkjet34.Margin = new Padding(3, 4, 3, 4);
        grpInkjet34.Name = "grpInkjet34";
        grpInkjet34.Padding = new Padding(3, 4, 3, 4);
        grpInkjet34.Size = new Size(617, 67);
        grpInkjet34.TabIndex = 1;
        grpInkjet34.TabStop = false;
        grpInkjet34.Text = "Inkjet 3 & 4 (TCP)";
        // 
        // lblTcpHost
        // 
        lblTcpHost.AutoSize = true;
        lblTcpHost.Location = new Point(11, 29);
        lblTcpHost.Name = "lblTcpHost";
        lblTcpHost.Size = new Size(43, 20);
        lblTcpHost.TabIndex = 0;
        lblTcpHost.Text = "Host:";
        // 
        // txtTcpHost
        // 
        txtTcpHost.Location = new Point(57, 25);
        txtTcpHost.Margin = new Padding(3, 4, 3, 4);
        txtTcpHost.Name = "txtTcpHost";
        txtTcpHost.Size = new Size(137, 27);
        txtTcpHost.TabIndex = 1;
        txtTcpHost.Text = "192.168.1.100";
        // 
        // lblTcpPort
        // 
        lblTcpPort.AutoSize = true;
        lblTcpPort.Location = new Point(206, 29);
        lblTcpPort.Name = "lblTcpPort";
        lblTcpPort.Size = new Size(38, 20);
        lblTcpPort.TabIndex = 2;
        lblTcpPort.Text = "Port:";
        // 
        // txtTcpPort
        // 
        txtTcpPort.Location = new Point(246, 25);
        txtTcpPort.Margin = new Padding(3, 4, 3, 4);
        txtTcpPort.Name = "txtTcpPort";
        txtTcpPort.Size = new Size(68, 27);
        txtTcpPort.TabIndex = 3;
        txtTcpPort.Text = "9100";
        // 
        // btnConnectTcp
        // 
        btnConnectTcp.Location = new Point(326, 24);
        btnConnectTcp.Margin = new Padding(3, 4, 3, 4);
        btnConnectTcp.Name = "btnConnectTcp";
        btnConnectTcp.Size = new Size(86, 33);
        btnConnectTcp.TabIndex = 4;
        btnConnectTcp.Text = "Connect";
        btnConnectTcp.Click += btnConnectTcp_Click;
        // 
        // btnDisconnectTcp
        // 
        btnDisconnectTcp.Location = new Point(417, 24);
        btnDisconnectTcp.Margin = new Padding(3, 4, 3, 4);
        btnDisconnectTcp.Name = "btnDisconnectTcp";
        btnDisconnectTcp.Size = new Size(91, 33);
        btnDisconnectTcp.TabIndex = 5;
        btnDisconnectTcp.Text = "Disconnect";
        btnDisconnectTcp.Click += btnDisconnectTcp_Click;
        // 
        // lblStatusIj3
        // 
        lblStatusIj3.BackColor = Color.Gray;
        lblStatusIj3.ForeColor = Color.White;
        lblStatusIj3.Location = new Point(520, 29);
        lblStatusIj3.Name = "lblStatusIj3";
        lblStatusIj3.Size = new Size(40, 24);
        lblStatusIj3.TabIndex = 6;
        lblStatusIj3.Text = "IJ3";
        lblStatusIj3.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblStatusIj4
        // 
        lblStatusIj4.BackColor = Color.Gray;
        lblStatusIj4.ForeColor = Color.White;
        lblStatusIj4.Location = new Point(566, 29);
        lblStatusIj4.Name = "lblStatusIj4";
        lblStatusIj4.Size = new Size(40, 24);
        lblStatusIj4.TabIndex = 7;
        lblStatusIj4.Text = "IJ4";
        lblStatusIj4.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // grpBackend
        // 
        grpBackend.Controls.Add(lblApiUrl);
        grpBackend.Controls.Add(txtApiUrl);
        grpBackend.Controls.Add(btnApplyApi);
        grpBackend.Controls.Add(lblApiStatus);
        grpBackend.Location = new Point(629, 77);
        grpBackend.Margin = new Padding(3, 4, 3, 4);
        grpBackend.Name = "grpBackend";
        grpBackend.Padding = new Padding(3, 4, 3, 4);
        grpBackend.Size = new Size(423, 67);
        grpBackend.TabIndex = 2;
        grpBackend.TabStop = false;
        grpBackend.Text = "Backend API";
        // 
        // lblApiUrl
        // 
        lblApiUrl.AutoSize = true;
        lblApiUrl.Location = new Point(11, 29);
        lblApiUrl.Name = "lblApiUrl";
        lblApiUrl.Size = new Size(34, 20);
        lblApiUrl.TabIndex = 0;
        lblApiUrl.Text = "API:";
        // 
        // txtApiUrl
        // 
        txtApiUrl.Location = new Point(46, 25);
        txtApiUrl.Margin = new Padding(3, 4, 3, 4);
        txtApiUrl.Name = "txtApiUrl";
        txtApiUrl.Size = new Size(228, 27);
        txtApiUrl.TabIndex = 1;
        txtApiUrl.Text = "http://localhost:3000";
        // 
        // btnApplyApi
        // 
        btnApplyApi.Location = new Point(280, 24);
        btnApplyApi.Margin = new Padding(3, 4, 3, 4);
        btnApplyApi.Name = "btnApplyApi";
        btnApplyApi.Size = new Size(63, 33);
        btnApplyApi.TabIndex = 2;
        btnApplyApi.Text = "Apply";
        btnApplyApi.Click += btnApplyApi_Click;
        // 
        // lblApiStatus
        // 
        lblApiStatus.AutoSize = true;
        lblApiStatus.Location = new Point(354, 29);
        lblApiStatus.Name = "lblApiStatus";
        lblApiStatus.Size = new Size(27, 20);
        lblApiStatus.TabIndex = 3;
        lblApiStatus.Text = "---";
        // 
        // pnlJobs
        // 
        pnlJobs.Controls.Add(dgvJobs);
        pnlJobs.Controls.Add(btnRefresh);
        pnlJobs.Controls.Add(lblJobsTitle);
        pnlJobs.Dock = DockStyle.Left;
        pnlJobs.Location = new Point(0, 153);
        pnlJobs.Margin = new Padding(3, 4, 3, 4);
        pnlJobs.Name = "pnlJobs";
        pnlJobs.Padding = new Padding(6, 7, 6, 7);
        pnlJobs.Size = new Size(400, 527);
        pnlJobs.TabIndex = 1;
        // 
        // dgvJobs
        // 
        dgvJobs.AllowUserToAddRows = false;
        dgvJobs.AllowUserToDeleteRows = false;
        dgvJobs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvJobs.ColumnHeadersHeight = 29;
        dgvJobs.Dock = DockStyle.Fill;
        dgvJobs.Location = new Point(6, 40);
        dgvJobs.Margin = new Padding(3, 4, 3, 4);
        dgvJobs.MultiSelect = false;
        dgvJobs.Name = "dgvJobs";
        dgvJobs.ReadOnly = true;
        dgvJobs.RowHeadersWidth = 51;
        dgvJobs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvJobs.Size = new Size(388, 440);
        dgvJobs.TabIndex = 0;
        dgvJobs.SelectionChanged += dgvJobs_SelectionChanged;
        // 
        // btnRefresh
        // 
        btnRefresh.Dock = DockStyle.Bottom;
        btnRefresh.Location = new Point(6, 480);
        btnRefresh.Margin = new Padding(3, 4, 3, 4);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(388, 40);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "Refresh";
        btnRefresh.Click += btnRefresh_Click;
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
        pnlDetail.Location = new Point(400, 153);
        pnlDetail.Margin = new Padding(3, 4, 3, 4);
        pnlDetail.Name = "pnlDetail";
        pnlDetail.Padding = new Padding(6, 7, 6, 7);
        pnlDetail.Size = new Size(743, 527);
        pnlDetail.TabIndex = 0;
        // 
        // lblDetailTitle
        // 
        lblDetailTitle.AutoSize = true;
        lblDetailTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDetailTitle.Location = new Point(6, 7);
        lblDetailTitle.Name = "lblDetailTitle";
        lblDetailTitle.Size = new Size(92, 23);
        lblDetailTitle.TabIndex = 0;
        lblDetailTitle.Text = "Job Detail";
        // 
        // lblBarcode
        // 
        lblBarcode.AutoSize = true;
        lblBarcode.Location = new Point(6, 40);
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
        lblLot.Location = new Point(331, 40);
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
        lblStatus.Location = new Point(549, 40);
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
        lblPattern.Location = new Point(6, 73);
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
        dgvConfigs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvConfigs.ColumnHeadersHeight = 29;
        dgvConfigs.Dock = DockStyle.Fill;
        dgvConfigs.Location = new Point(3, 24);
        dgvConfigs.Margin = new Padding(3, 4, 3, 4);
        dgvConfigs.Name = "dgvConfigs";
        dgvConfigs.ReadOnly = true;
        dgvConfigs.RowHeadersWidth = 51;
        dgvConfigs.Size = new Size(714, 145);
        dgvConfigs.TabIndex = 0;
        dgvConfigs.SelectionChanged += dgvConfigs_SelectionChanged;
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
        dgvTextBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvTextBlocks.ColumnHeadersHeight = 29;
        dgvTextBlocks.Dock = DockStyle.Fill;
        dgvTextBlocks.Location = new Point(3, 24);
        dgvTextBlocks.Margin = new Padding(3, 4, 3, 4);
        dgvTextBlocks.Name = "dgvTextBlocks";
        dgvTextBlocks.ReadOnly = true;
        dgvTextBlocks.RowHeadersWidth = 51;
        dgvTextBlocks.Size = new Size(714, 145);
        dgvTextBlocks.TabIndex = 0;
        // 
        // btnSend
        // 
        btnSend.BackColor = Color.FromArgb(0, 120, 215);
        btnSend.FlatStyle = FlatStyle.Flat;
        btnSend.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnSend.ForeColor = Color.White;
        btnSend.Location = new Point(6, 473);
        btnSend.Margin = new Padding(3, 4, 3, 4);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(229, 53);
        btnSend.TabIndex = 11;
        btnSend.Text = "SEND TO DEVICES";
        btnSend.UseVisualStyleBackColor = false;
        btnSend.Click += btnSend_Click;
        // 
        // btnRetry
        // 
        btnRetry.Location = new Point(246, 473);
        btnRetry.Margin = new Padding(3, 4, 3, 4);
        btnRetry.Name = "btnRetry";
        btnRetry.Size = new Size(137, 53);
        btnRetry.TabIndex = 12;
        btnRetry.Text = "Retry Failed";
        btnRetry.Click += btnRetry_Click;
        // 
        // pnlLog
        // 
        pnlLog.Controls.Add(txtLog);
        pnlLog.Controls.Add(lblLogTitle);
        pnlLog.Dock = DockStyle.Bottom;
        pnlLog.Location = new Point(0, 680);
        pnlLog.Margin = new Padding(3, 4, 3, 4);
        pnlLog.Name = "pnlLog";
        pnlLog.Padding = new Padding(6, 7, 6, 7);
        pnlLog.Size = new Size(1143, 187);
        pnlLog.TabIndex = 2;
        // 
        // txtLog
        // 
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 8.5F);
        txtLog.Location = new Point(6, 34);
        txtLog.Margin = new Padding(3, 4, 3, 4);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(1131, 146);
        txtLog.TabIndex = 0;
        // 
        // lblLogTitle
        // 
        lblLogTitle.Dock = DockStyle.Top;
        lblLogTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLogTitle.Location = new Point(6, 7);
        lblLogTitle.Name = "lblLogTitle";
        lblLogTitle.Size = new Size(1131, 27);
        lblLogTitle.TabIndex = 1;
        lblLogTitle.Text = "Log";
        // 
        // button1
        // 
        button1.Location = new Point(834, 28);
        button1.Name = "button1";
        button1.Size = new Size(94, 29);
        button1.TabIndex = 3;
        button1.Text = "button1";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // frmMain
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1143, 867);
        Controls.Add(pnlDetail);
        Controls.Add(pnlJobs);
        Controls.Add(pnlLog);
        Controls.Add(pnlConnection);
        Margin = new Padding(3, 4, 3, 4);
        Name = "frmMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Inkjet Operator";
        FormClosing += frmMain_FormClosing;
        Load += frmMain_Load;
        pnlConnection.ResumeLayout(false);
        grpInkjet12.ResumeLayout(false);
        grpInkjet12.PerformLayout();
        grpInkjet34.ResumeLayout(false);
        grpInkjet34.PerformLayout();
        grpBackend.ResumeLayout(false);
        grpBackend.PerformLayout();
        pnlJobs.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvJobs).EndInit();
        pnlDetail.ResumeLayout(false);
        pnlDetail.PerformLayout();
        grpInkjetConfigs.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvConfigs).EndInit();
        grpTextBlocks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvTextBlocks).EndInit();
        pnlLog.ResumeLayout(false);
        pnlLog.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    // Timer
    private System.Windows.Forms.Timer tmrPoll;

    // Connection panel
    private Panel pnlConnection;
    private GroupBox grpInkjet12;
    private Label lblCom1;
    private ComboBox cmbCom1;
    private Label lblCom2;
    private ComboBox cmbCom2;
    private Button btnConnectRs232;
    private Button btnDisconnectRs232;
    private Label lblStatusIj1;
    private Label lblStatusIj2;

    private GroupBox grpInkjet34;
    private Label lblTcpHost;
    private TextBox txtTcpHost;
    private Label lblTcpPort;
    private TextBox txtTcpPort;
    private Button btnConnectTcp;
    private Button btnDisconnectTcp;
    private Label lblStatusIj3;
    private Label lblStatusIj4;

    private GroupBox grpBackend;
    private Label lblApiUrl;
    private TextBox txtApiUrl;
    private Button btnApplyApi;
    private Label lblApiStatus;

    // Jobs panel
    private Panel pnlJobs;
    private Label lblJobsTitle;
    private DataGridView dgvJobs;
    private Button btnRefresh;

    // Detail panel
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

    // Log panel
    private Panel pnlLog;
    private Label lblLogTitle;
    private TextBox txtLog;
    private Button button1;
}
