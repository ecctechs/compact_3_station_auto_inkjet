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

        // Timer
        tmrPoll = new System.Windows.Forms.Timer(components);
        tmrPoll.Interval = 5000;
        tmrPoll.Tick += tmrPoll_Tick;

        // ── Top panel: Connection settings ──
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

        // ── Left panel: Job queue ──
        pnlJobs = new Panel();
        lblJobsTitle = new Label();
        dgvJobs = new DataGridView();
        btnRefresh = new Button();

        // ── Right panel: Pattern detail + actions ──
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

        // ── Bottom: Log ──
        pnlLog = new Panel();
        lblLogTitle = new Label();
        txtLog = new TextBox();

        SuspendLayout();

        // ════════════════════════════════════════
        //  Connection panel (top)
        // ════════════════════════════════════════

        // -- grpInkjet12 --
        lblCom1.Text = "COM1 (IJ1):";
        lblCom1.Location = new Point(10, 22);
        lblCom1.AutoSize = true;

        cmbCom1.Location = new Point(90, 19);
        cmbCom1.Size = new Size(90, 23);
        cmbCom1.DropDownStyle = ComboBoxStyle.DropDownList;

        lblCom2.Text = "COM2 (IJ2):";
        lblCom2.Location = new Point(190, 22);
        lblCom2.AutoSize = true;

        cmbCom2.Location = new Point(270, 19);
        cmbCom2.Size = new Size(90, 23);
        cmbCom2.DropDownStyle = ComboBoxStyle.DropDownList;

        btnConnectRs232.Text = "Connect";
        btnConnectRs232.Location = new Point(370, 18);
        btnConnectRs232.Size = new Size(75, 25);
        btnConnectRs232.Click += btnConnectRs232_Click;

        btnDisconnectRs232.Text = "Disconnect";
        btnDisconnectRs232.Location = new Point(450, 18);
        btnDisconnectRs232.Size = new Size(80, 25);
        btnDisconnectRs232.Click += btnDisconnectRs232_Click;

        lblStatusIj1.Text = "IJ1";
        lblStatusIj1.Location = new Point(540, 22);
        lblStatusIj1.Size = new Size(35, 18);
        lblStatusIj1.BackColor = Color.Gray;
        lblStatusIj1.ForeColor = Color.White;
        lblStatusIj1.TextAlign = ContentAlignment.MiddleCenter;

        lblStatusIj2.Text = "IJ2";
        lblStatusIj2.Location = new Point(580, 22);
        lblStatusIj2.Size = new Size(35, 18);
        lblStatusIj2.BackColor = Color.Gray;
        lblStatusIj2.ForeColor = Color.White;
        lblStatusIj2.TextAlign = ContentAlignment.MiddleCenter;

        grpInkjet12.Text = "Inkjet 1 & 2 (RS232)";
        grpInkjet12.Location = new Point(5, 5);
        grpInkjet12.Size = new Size(625, 50);
        grpInkjet12.Controls.AddRange(new Control[] {
            lblCom1, cmbCom1, lblCom2, cmbCom2,
            btnConnectRs232, btnDisconnectRs232, lblStatusIj1, lblStatusIj2
        });

        // -- grpInkjet34 --
        lblTcpHost.Text = "Host:";
        lblTcpHost.Location = new Point(10, 22);
        lblTcpHost.AutoSize = true;

        txtTcpHost.Text = "192.168.1.100";
        txtTcpHost.Location = new Point(50, 19);
        txtTcpHost.Size = new Size(120, 23);

        lblTcpPort.Text = "Port:";
        lblTcpPort.Location = new Point(180, 22);
        lblTcpPort.AutoSize = true;

        txtTcpPort.Text = "9100";
        txtTcpPort.Location = new Point(215, 19);
        txtTcpPort.Size = new Size(60, 23);

        btnConnectTcp.Text = "Connect";
        btnConnectTcp.Location = new Point(285, 18);
        btnConnectTcp.Size = new Size(75, 25);
        btnConnectTcp.Click += btnConnectTcp_Click;

        btnDisconnectTcp.Text = "Disconnect";
        btnDisconnectTcp.Location = new Point(365, 18);
        btnDisconnectTcp.Size = new Size(80, 25);
        btnDisconnectTcp.Click += btnDisconnectTcp_Click;

        lblStatusIj3.Text = "IJ3";
        lblStatusIj3.Location = new Point(455, 22);
        lblStatusIj3.Size = new Size(35, 18);
        lblStatusIj3.BackColor = Color.Gray;
        lblStatusIj3.ForeColor = Color.White;
        lblStatusIj3.TextAlign = ContentAlignment.MiddleCenter;

        lblStatusIj4.Text = "IJ4";
        lblStatusIj4.Location = new Point(495, 22);
        lblStatusIj4.Size = new Size(35, 18);
        lblStatusIj4.BackColor = Color.Gray;
        lblStatusIj4.ForeColor = Color.White;
        lblStatusIj4.TextAlign = ContentAlignment.MiddleCenter;

        grpInkjet34.Text = "Inkjet 3 & 4 (TCP)";
        grpInkjet34.Location = new Point(5, 58);
        grpInkjet34.Size = new Size(540, 50);
        grpInkjet34.Controls.AddRange(new Control[] {
            lblTcpHost, txtTcpHost, lblTcpPort, txtTcpPort,
            btnConnectTcp, btnDisconnectTcp, lblStatusIj3, lblStatusIj4
        });

        // -- grpBackend --
        lblApiUrl.Text = "API:";
        lblApiUrl.Location = new Point(10, 22);
        lblApiUrl.AutoSize = true;

        txtApiUrl.Text = "http://localhost:3000";
        txtApiUrl.Location = new Point(40, 19);
        txtApiUrl.Size = new Size(200, 23);

        btnApplyApi.Text = "Apply";
        btnApplyApi.Location = new Point(245, 18);
        btnApplyApi.Size = new Size(55, 25);
        btnApplyApi.Click += btnApplyApi_Click;

        lblApiStatus.Text = "---";
        lblApiStatus.Location = new Point(310, 22);
        lblApiStatus.AutoSize = true;

        grpBackend.Text = "Backend API";
        grpBackend.Location = new Point(550, 58);
        grpBackend.Size = new Size(370, 50);
        grpBackend.Controls.AddRange(new Control[] {
            lblApiUrl, txtApiUrl, btnApplyApi, lblApiStatus
        });

        pnlConnection.Dock = DockStyle.Top;
        pnlConnection.Height = 115;
        pnlConnection.Controls.AddRange(new Control[] { grpInkjet12, grpInkjet34, grpBackend });

        // ════════════════════════════════════════
        //  Jobs panel (left)
        // ════════════════════════════════════════

        lblJobsTitle.Text = "Pending Jobs";
        lblJobsTitle.Font = new Font(Font.FontFamily, 10, FontStyle.Bold);
        lblJobsTitle.Dock = DockStyle.Top;
        lblJobsTitle.Height = 25;

        dgvJobs.Dock = DockStyle.Fill;
        dgvJobs.ReadOnly = true;
        dgvJobs.AllowUserToAddRows = false;
        dgvJobs.AllowUserToDeleteRows = false;
        dgvJobs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvJobs.MultiSelect = false;
        dgvJobs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvJobs.SelectionChanged += dgvJobs_SelectionChanged;

        btnRefresh.Text = "Refresh";
        btnRefresh.Dock = DockStyle.Bottom;
        btnRefresh.Height = 30;
        btnRefresh.Click += btnRefresh_Click;

        pnlJobs.Dock = DockStyle.Left;
        pnlJobs.Width = 350;
        pnlJobs.Padding = new Padding(5);
        pnlJobs.Controls.Add(dgvJobs);
        pnlJobs.Controls.Add(btnRefresh);
        pnlJobs.Controls.Add(lblJobsTitle);

        // ════════════════════════════════════════
        //  Log panel (bottom)
        // ════════════════════════════════════════

        lblLogTitle.Text = "Log";
        lblLogTitle.Font = new Font(Font.FontFamily, 9, FontStyle.Bold);
        lblLogTitle.Dock = DockStyle.Top;
        lblLogTitle.Height = 20;

        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Font = new Font("Consolas", 8.5f);

        pnlLog.Dock = DockStyle.Bottom;
        pnlLog.Height = 140;
        pnlLog.Padding = new Padding(5);
        pnlLog.Controls.Add(txtLog);
        pnlLog.Controls.Add(lblLogTitle);

        // ════════════════════════════════════════
        //  Detail panel (fill, right of jobs)
        // ════════════════════════════════════════

        lblDetailTitle.Text = "Job Detail";
        lblDetailTitle.Font = new Font(Font.FontFamily, 10, FontStyle.Bold);
        lblDetailTitle.Location = new Point(5, 5);
        lblDetailTitle.AutoSize = true;

        lblBarcode.Text = "Barcode:";
        lblBarcode.Location = new Point(5, 30);
        lblBarcode.AutoSize = true;
        txtBarcode.Location = new Point(80, 27);
        txtBarcode.Size = new Size(200, 23);
        txtBarcode.ReadOnly = true;

        lblLot.Text = "Lot:";
        lblLot.Location = new Point(290, 30);
        lblLot.AutoSize = true;
        txtLot.Location = new Point(320, 27);
        txtLot.Size = new Size(150, 23);
        txtLot.ReadOnly = true;

        lblStatus.Text = "Status:";
        lblStatus.Location = new Point(480, 30);
        lblStatus.AutoSize = true;
        txtStatus.Location = new Point(530, 27);
        txtStatus.Size = new Size(100, 23);
        txtStatus.ReadOnly = true;

        lblPattern.Text = "Pattern:";
        lblPattern.Location = new Point(5, 55);
        lblPattern.AutoSize = true;
        txtPattern.Location = new Point(80, 52);
        txtPattern.Size = new Size(200, 23);
        txtPattern.ReadOnly = true;

        // Inkjet configs grid
        grpInkjetConfigs.Text = "Inkjet Configs";
        grpInkjetConfigs.Location = new Point(5, 80);
        grpInkjetConfigs.Size = new Size(630, 130);

        dgvConfigs.Dock = DockStyle.Fill;
        dgvConfigs.ReadOnly = true;
        dgvConfigs.AllowUserToAddRows = false;
        dgvConfigs.AllowUserToDeleteRows = false;
        dgvConfigs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvConfigs.SelectionChanged += dgvConfigs_SelectionChanged;
        grpInkjetConfigs.Controls.Add(dgvConfigs);

        // Text blocks grid
        grpTextBlocks.Text = "Text Blocks";
        grpTextBlocks.Location = new Point(5, 215);
        grpTextBlocks.Size = new Size(630, 130);

        dgvTextBlocks.Dock = DockStyle.Fill;
        dgvTextBlocks.ReadOnly = true;
        dgvTextBlocks.AllowUserToAddRows = false;
        dgvTextBlocks.AllowUserToDeleteRows = false;
        dgvTextBlocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grpTextBlocks.Controls.Add(dgvTextBlocks);

        // Action buttons
        btnSend.Text = "SEND TO DEVICES";
        btnSend.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
        btnSend.Location = new Point(5, 355);
        btnSend.Size = new Size(200, 40);
        btnSend.BackColor = Color.FromArgb(0, 120, 215);
        btnSend.ForeColor = Color.White;
        btnSend.FlatStyle = FlatStyle.Flat;
        btnSend.Click += btnSend_Click;

        btnRetry.Text = "Retry Failed";
        btnRetry.Location = new Point(215, 355);
        btnRetry.Size = new Size(120, 40);
        btnRetry.Click += btnRetry_Click;

        pnlDetail.Dock = DockStyle.Fill;
        pnlDetail.Padding = new Padding(5);
        pnlDetail.Controls.AddRange(new Control[] {
            lblDetailTitle,
            lblBarcode, txtBarcode,
            lblLot, txtLot,
            lblStatus, txtStatus,
            lblPattern, txtPattern,
            grpInkjetConfigs, grpTextBlocks,
            btnSend, btnRetry
        });

        // ════════════════════════════════════════
        //  Form
        // ════════════════════════════════════════

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 650);
        Controls.Add(pnlDetail);
        Controls.Add(pnlJobs);
        Controls.Add(pnlLog);
        Controls.Add(pnlConnection);
        Text = "Inkjet Operator";
        StartPosition = FormStartPosition.CenterScreen;
        Load += frmMain_Load;
        FormClosing += frmMain_FormClosing;

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
}
