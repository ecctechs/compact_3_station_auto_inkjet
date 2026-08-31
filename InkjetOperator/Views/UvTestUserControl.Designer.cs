namespace InkjetOperator.Views;

partial class UvTestUserControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        tlpRoot = new System.Windows.Forms.TableLayoutPanel();

        grpMachine = new System.Windows.Forms.GroupBox();
        tlpMachine = new System.Windows.Forms.TableLayoutPanel();
        flpPick = new System.Windows.Forms.FlowLayoutPanel();
        btnUv1 = new AntdUI.Button();
        btnUv2 = new AntdUI.Button();
        lblIpLabel = new System.Windows.Forms.Label();
        txtIp = new IpAddressInput();
        lblPortLabel = new System.Windows.Forms.Label();
        txtPort = new AntdUI.Input();
        btnCheck = new AntdUI.Button();
        lblFolderLabel = new System.Windows.Forms.Label();
        txtFolder = new AntdUI.Input();
        btnBrowseFolder = new AntdUI.Button();
        lblPathStatus = new System.Windows.Forms.Label();

        grpCommand = new System.Windows.Forms.GroupBox();
        tlpCommand = new System.Windows.Forms.TableLayoutPanel();
        lblProgramLabel = new System.Windows.Forms.Label();
        txtProgram = new AntdUI.Input();
        btnFindProgram = new AntdUI.Button();
        flpCmdButtons = new System.Windows.Forms.FlowLayoutPanel();
        btnLoad = new AntdUI.Button();
        btnStart = new AntdUI.Button();
        btnStop = new AntdUI.Button();

        grpCpi = new System.Windows.Forms.GroupBox();
        tlpCpi = new System.Windows.Forms.TableLayoutPanel();
        lblTableLabel = new System.Windows.Forms.Label();
        txtTable = new AntdUI.Input();
        lblLotLabel = new System.Windows.Forms.Label();
        txtLot = new AntdUI.Input();
        lblNameLabel = new System.Windows.Forms.Label();
        txtName = new AntdUI.Input();
        lblText1Label = new System.Windows.Forms.Label();
        txtText1 = new AntdUI.Input();
        lblText2Label = new System.Windows.Forms.Label();
        txtText2 = new AntdUI.Input();
        lblText3Label = new System.Windows.Forms.Label();
        txtText3 = new AntdUI.Input();
        lblText4Label = new System.Windows.Forms.Label();
        txtText4 = new AntdUI.Input();
        lblText5Label = new System.Windows.Forms.Label();
        txtText5 = new AntdUI.Input();
        flpCpiButtons = new System.Windows.Forms.FlowLayoutPanel();
        btnCpiRead = new AntdUI.Button();
        btnCpiWrite = new AntdUI.Button();

        grpLog = new System.Windows.Forms.GroupBox();
        txtLog = new System.Windows.Forms.TextBox();

        tlpRoot.SuspendLayout();
        grpMachine.SuspendLayout();
        tlpMachine.SuspendLayout();
        flpPick.SuspendLayout();
        grpCommand.SuspendLayout();
        tlpCommand.SuspendLayout();
        flpCmdButtons.SuspendLayout();
        grpCpi.SuspendLayout();
        tlpCpi.SuspendLayout();
        flpCpiButtons.SuspendLayout();
        grpLog.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpMachine, 0, 0);
        tlpRoot.Controls.Add(grpCommand, 0, 1);
        tlpRoot.Controls.Add(grpCpi, 0, 2);
        tlpRoot.Controls.Add(grpLog, 0, 3);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 4;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 375F));
        tlpRoot.Size = new System.Drawing.Size(1225, 1462);
        tlpRoot.TabIndex = 0;
        //
        // grpMachine
        //
        grpMachine.AutoSize = true;
        grpMachine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpMachine.Controls.Add(tlpMachine);
        grpMachine.Dock = System.Windows.Forms.DockStyle.Fill;
        grpMachine.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpMachine.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpMachine.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpMachine.Name = "grpMachine";
        grpMachine.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpMachine.TabIndex = 0;
        grpMachine.TabStop = false;
        grpMachine.Text = "1. เลือกเครื่อง";
        //
        // tlpMachine — label(120) | input(fill) | label(70) | input(110) | button(120)
        //
        tlpMachine.AutoSize = true;
        tlpMachine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpMachine.BackColor = System.Drawing.Color.White;
        tlpMachine.ColumnCount = 5;
        tlpMachine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpMachine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMachine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
        tlpMachine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
        tlpMachine.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
        tlpMachine.Controls.Add(flpPick, 0, 0);
        tlpMachine.Controls.Add(lblIpLabel, 0, 1);
        tlpMachine.Controls.Add(txtIp, 1, 1);
        tlpMachine.Controls.Add(lblPortLabel, 2, 1);
        tlpMachine.Controls.Add(txtPort, 3, 1);
        tlpMachine.Controls.Add(btnCheck, 4, 1);
        tlpMachine.Controls.Add(lblFolderLabel, 0, 2);
        tlpMachine.Controls.Add(txtFolder, 1, 2);
        tlpMachine.Controls.Add(btnBrowseFolder, 4, 2);
        tlpMachine.Controls.Add(lblPathStatus, 1, 3);
        tlpMachine.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMachine.Name = "tlpMachine";
        tlpMachine.RowCount = 4;
        tlpMachine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
        tlpMachine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpMachine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpMachine.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
        tlpMachine.TabIndex = 0;
        //
        // flpPick
        //
        flpPick.BackColor = System.Drawing.Color.White;
        flpPick.Controls.Add(btnUv1);
        flpPick.Controls.Add(btnUv2);
        flpPick.Dock = System.Windows.Forms.DockStyle.Fill;
        flpPick.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpPick.Margin = new System.Windows.Forms.Padding(0);
        flpPick.Name = "flpPick";
        flpPick.TabIndex = 0;
        flpPick.WrapContents = false;
        tlpMachine.SetColumnSpan(flpPick, 5);
        //
        // btnUv1
        //
        btnUv1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnUv1.ForeColor = System.Drawing.Color.White;
        btnUv1.Margin = new System.Windows.Forms.Padding(0, 6, 10, 6);
        btnUv1.Name = "btnUv1";
        btnUv1.Radius = 8;
        btnUv1.Size = new System.Drawing.Size(238, 52);
        btnUv1.TabIndex = 0;
        btnUv1.Text = "UV-001";
        btnUv1.Type = AntdUI.TTypeMini.Primary;
        //
        // btnUv2
        //
        btnUv2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnUv2.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2.Margin = new System.Windows.Forms.Padding(0, 6, 10, 6);
        btnUv2.Name = "btnUv2";
        btnUv2.Radius = 8;
        btnUv2.Size = new System.Drawing.Size(238, 52);
        btnUv2.TabIndex = 1;
        btnUv2.Text = "UV-002";
        btnUv2.Type = AntdUI.TTypeMini.Default;
        //
        // lblIpLabel
        //
        lblIpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblIpLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblIpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblIpLabel.Name = "lblIpLabel";
        lblIpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblIpLabel.TabIndex = 1;
        lblIpLabel.Text = "IP:";
        lblIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtIp
        //
        txtIp.Dock = System.Windows.Forms.DockStyle.Fill;
        txtIp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtIp.Name = "txtIp";
        txtIp.TabIndex = 2;
        //
        // lblPortLabel
        //
        lblPortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPortLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblPortLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPortLabel.Name = "lblPortLabel";
        lblPortLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblPortLabel.TabIndex = 3;
        lblPortLabel.Text = "Port:";
        lblPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPort
        //
        txtPort.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPort.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPort.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtPort.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPort.Name = "txtPort";
        txtPort.PlaceholderText = "10086";
        txtPort.Radius = 4;
        txtPort.TabIndex = 4;
        //
        // btnCheck
        //
        btnCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnCheck.BorderWidth = 2F;
        btnCheck.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheck.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnCheck.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheck.Name = "btnCheck";
        btnCheck.Radius = 6;
        btnCheck.Size = new System.Drawing.Size(155, 45);
        btnCheck.TabIndex = 5;
        btnCheck.Text = "เช็คการเชื่อมต่อ";
        btnCheck.Type = AntdUI.TTypeMini.Default;
        //
        // lblFolderLabel
        //
        lblFolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblFolderLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblFolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblFolderLabel.Name = "lblFolderLabel";
        lblFolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblFolderLabel.TabIndex = 6;
        lblFolderLabel.Text = "โฟลเดอร์ UV:";
        lblFolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtFolder
        //
        txtFolder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtFolder.Dock = System.Windows.Forms.DockStyle.Fill;
        txtFolder.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtFolder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtFolder.Name = "txtFolder";
        txtFolder.PlaceholderText = "เลือกโฟลเดอร์ที่ติดตั้งซอฟต์แวร์ UV (มี database\\sys และ document)";
        txtFolder.Radius = 4;
        txtFolder.ReadOnly = true;
        txtFolder.TabIndex = 7;
        tlpMachine.SetColumnSpan(txtFolder, 3);
        //
        // btnBrowseFolder
        //
        btnBrowseFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowseFolder.BorderWidth = 2F;
        btnBrowseFolder.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowseFolder.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnBrowseFolder.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowseFolder.Name = "btnBrowseFolder";
        btnBrowseFolder.IconSvg = "FolderOpenFilled";
        btnBrowseFolder.IconRatio = 1.2F;
        btnBrowseFolder.Radius = 6;
        btnBrowseFolder.Size = new System.Drawing.Size(52, 42);
        btnBrowseFolder.TabIndex = 8;
        btnBrowseFolder.Type = AntdUI.TTypeMini.Default;
        //
        // lblPathStatus
        //
        lblPathStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPathStatus.Font = new System.Drawing.Font("Segoe UI", 12F);
        lblPathStatus.ForeColor = System.Drawing.Color.Gray;
        lblPathStatus.Name = "lblPathStatus";
        lblPathStatus.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
        lblPathStatus.TabIndex = 9;
        lblPathStatus.Text = "";
        tlpMachine.SetColumnSpan(lblPathStatus, 4);
        //
        // grpCommand
        //
        grpCommand.AutoSize = true;
        grpCommand.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpCommand.Controls.Add(tlpCommand);
        grpCommand.Dock = System.Windows.Forms.DockStyle.Fill;
        grpCommand.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpCommand.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpCommand.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpCommand.Name = "grpCommand";
        grpCommand.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpCommand.TabIndex = 1;
        grpCommand.TabStop = false;
        grpCommand.Text = "2. สั่งงานเครื่อง";
        //
        // tlpCommand
        //
        tlpCommand.AutoSize = true;
        tlpCommand.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpCommand.BackColor = System.Drawing.Color.White;
        tlpCommand.ColumnCount = 3;
        tlpCommand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpCommand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpCommand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 162F));
        tlpCommand.Controls.Add(lblProgramLabel, 0, 0);
        tlpCommand.Controls.Add(txtProgram, 1, 0);
        tlpCommand.Controls.Add(btnFindProgram, 2, 0);
        tlpCommand.Controls.Add(flpCmdButtons, 1, 1);
        tlpCommand.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpCommand.Name = "tlpCommand";
        tlpCommand.RowCount = 2;
        tlpCommand.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        tlpCommand.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
        tlpCommand.TabIndex = 0;
        //
        // lblProgramLabel
        //
        lblProgramLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProgramLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblProgramLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblProgramLabel.Name = "lblProgramLabel";
        lblProgramLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblProgramLabel.TabIndex = 0;
        lblProgramLabel.Text = "Program:";
        lblProgramLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtProgram
        //
        txtProgram.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        txtProgram.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtProgram.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtProgram.Name = "txtProgram";
        txtProgram.PlaceholderText = "พิมพ์ชื่อโปรแกรม เช่น S-DEX-1624-1 (ไม่ต้องใส่ .uvdx)";
        txtProgram.Radius = 4;
        txtProgram.TabIndex = 1;
        //
        // btnFindProgram
        //
        btnFindProgram.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnFindProgram.BorderWidth = 2F;
        btnFindProgram.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnFindProgram.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnFindProgram.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnFindProgram.Name = "btnFindProgram";
        btnFindProgram.Radius = 6;
        btnFindProgram.Size = new System.Drawing.Size(155, 45);
        btnFindProgram.TabIndex = 2;
        btnFindProgram.Text = "ค้นหาไฟล์";
        btnFindProgram.Type = AntdUI.TTypeMini.Default;
        //
        // flpCmdButtons
        //
        flpCmdButtons.BackColor = System.Drawing.Color.White;
        flpCmdButtons.Controls.Add(btnLoad);
        flpCmdButtons.Controls.Add(btnStart);
        flpCmdButtons.Controls.Add(btnStop);
        flpCmdButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        flpCmdButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpCmdButtons.Margin = new System.Windows.Forms.Padding(0);
        flpCmdButtons.Name = "flpCmdButtons";
        flpCmdButtons.TabIndex = 3;
        flpCmdButtons.WrapContents = false;
        tlpCommand.SetColumnSpan(flpCmdButtons, 2);
        //
        // btnLoad
        //
        btnLoad.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnLoad.ForeColor = System.Drawing.Color.White;
        btnLoad.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
        btnLoad.Name = "btnLoad";
        btnLoad.Radius = 8;
        btnLoad.Size = new System.Drawing.Size(262, 58);
        btnLoad.TabIndex = 0;
        btnLoad.Text = "Load โปรแกรม  (85)";
        btnLoad.Type = AntdUI.TTypeMini.Primary;
        //
        // btnStart
        //
        btnStart.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnStart.ForeColor = System.Drawing.Color.White;
        btnStart.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
        btnStart.Name = "btnStart";
        btnStart.Radius = 8;
        btnStart.Size = new System.Drawing.Size(212, 58);
        btnStart.TabIndex = 1;
        btnStart.Text = "Start  (83)";
        btnStart.Type = AntdUI.TTypeMini.Success;
        //
        // btnStop
        //
        btnStop.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnStop.ForeColor = System.Drawing.Color.White;
        btnStop.Margin = new System.Windows.Forms.Padding(0, 8, 3, 8);
        btnStop.Name = "btnStop";
        btnStop.Radius = 8;
        btnStop.Size = new System.Drawing.Size(212, 58);
        btnStop.TabIndex = 2;
        btnStop.Text = "Stop  (84)";
        btnStop.Type = AntdUI.TTypeMini.Error;
        //
        // grpCpi
        //
        grpCpi.AutoSize = true;
        grpCpi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpCpi.Controls.Add(tlpCpi);
        grpCpi.Dock = System.Windows.Forms.DockStyle.Fill;
        grpCpi.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpCpi.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpCpi.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpCpi.Name = "grpCpi";
        grpCpi.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpCpi.TabIndex = 2;
        grpCpi.TabStop = false;
        grpCpi.Text = "3. ข้อความใน CPI.db3";
        //
        // tlpCpi — label(120) | input(fill) | label(70) | input(fill)
        //
        tlpCpi.AutoSize = true;
        tlpCpi.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpCpi.BackColor = System.Drawing.Color.White;
        tlpCpi.ColumnCount = 4;
        tlpCpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpCpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpCpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
        tlpCpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpCpi.Controls.Add(lblTableLabel, 0, 0);
        tlpCpi.Controls.Add(txtTable, 1, 0);
        tlpCpi.Controls.Add(lblLotLabel, 0, 1);
        tlpCpi.Controls.Add(txtLot, 1, 1);
        tlpCpi.Controls.Add(lblNameLabel, 2, 1);
        tlpCpi.Controls.Add(txtName, 3, 1);
        tlpCpi.Controls.Add(lblText1Label, 0, 2);
        tlpCpi.Controls.Add(txtText1, 1, 2);
        tlpCpi.Controls.Add(lblText2Label, 0, 3);
        tlpCpi.Controls.Add(txtText2, 1, 3);
        tlpCpi.Controls.Add(lblText3Label, 0, 4);
        tlpCpi.Controls.Add(txtText3, 1, 4);
        tlpCpi.Controls.Add(lblText4Label, 0, 5);
        tlpCpi.Controls.Add(txtText4, 1, 5);
        tlpCpi.Controls.Add(lblText5Label, 0, 6);
        tlpCpi.Controls.Add(txtText5, 1, 6);
        tlpCpi.Controls.Add(flpCpiButtons, 1, 7);
        tlpCpi.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpCpi.Name = "tlpCpi";
        tlpCpi.RowCount = 8;
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpCpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
        tlpCpi.TabIndex = 0;
        //
        // lblTableLabel
        //
        lblTableLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTableLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblTableLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblTableLabel.Name = "lblTableLabel";
        lblTableLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblTableLabel.TabIndex = 0;
        lblTableLabel.Text = "ตาราง:";
        lblTableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtTable
        //
        txtTable.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtTable.Dock = System.Windows.Forms.DockStyle.Fill;
        txtTable.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtTable.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtTable.Name = "txtTable";
        txtTable.PlaceholderText = "MK063";
        txtTable.Radius = 4;
        txtTable.TabIndex = 1;
        //
        // lblLotLabel
        //
        lblLotLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblLotLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblLotLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblLotLabel.Name = "lblLotLabel";
        lblLotLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblLotLabel.TabIndex = 2;
        lblLotLabel.Text = "Lot:";
        lblLotLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtLot
        //
        txtLot.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtLot.Dock = System.Windows.Forms.DockStyle.Fill;
        txtLot.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtLot.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtLot.Name = "txtLot";
        txtLot.Radius = 4;
        txtLot.TabIndex = 3;
        //
        // lblNameLabel
        //
        lblNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblNameLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblNameLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblNameLabel.Name = "lblNameLabel";
        lblNameLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblNameLabel.TabIndex = 4;
        lblNameLabel.Text = "Name:";
        lblNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtName
        //
        txtName.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtName.Dock = System.Windows.Forms.DockStyle.Fill;
        txtName.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtName.Name = "txtName";
        txtName.Radius = 4;
        txtName.TabIndex = 5;
        //
        // lblText1Label
        //
        lblText1Label.Dock = System.Windows.Forms.DockStyle.Fill;
        lblText1Label.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblText1Label.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblText1Label.Name = "lblText1Label";
        lblText1Label.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblText1Label.TabIndex = 6;
        lblText1Label.Text = "Text 1:";
        lblText1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtText1
        //
        txtText1.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtText1.Dock = System.Windows.Forms.DockStyle.Fill;
        txtText1.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtText1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtText1.Name = "txtText1";
        txtText1.Radius = 4;
        txtText1.TabIndex = 7;
        tlpCpi.SetColumnSpan(txtText1, 3);
        //
        // lblText2Label
        //
        lblText2Label.Dock = System.Windows.Forms.DockStyle.Fill;
        lblText2Label.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblText2Label.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblText2Label.Name = "lblText2Label";
        lblText2Label.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblText2Label.TabIndex = 8;
        lblText2Label.Text = "Text 2:";
        lblText2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtText2
        //
        txtText2.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtText2.Dock = System.Windows.Forms.DockStyle.Fill;
        txtText2.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtText2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtText2.Name = "txtText2";
        txtText2.Radius = 4;
        txtText2.TabIndex = 9;
        tlpCpi.SetColumnSpan(txtText2, 3);
        //
        // lblText3Label
        //
        lblText3Label.Dock = System.Windows.Forms.DockStyle.Fill;
        lblText3Label.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblText3Label.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblText3Label.Name = "lblText3Label";
        lblText3Label.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblText3Label.TabIndex = 10;
        lblText3Label.Text = "Text 3:";
        lblText3Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtText3
        //
        txtText3.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtText3.Dock = System.Windows.Forms.DockStyle.Fill;
        txtText3.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtText3.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtText3.Name = "txtText3";
        txtText3.Radius = 4;
        txtText3.TabIndex = 11;
        tlpCpi.SetColumnSpan(txtText3, 3);
        //
        // lblText4Label
        //
        lblText4Label.Dock = System.Windows.Forms.DockStyle.Fill;
        lblText4Label.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblText4Label.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblText4Label.Name = "lblText4Label";
        lblText4Label.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblText4Label.TabIndex = 12;
        lblText4Label.Text = "Text 4:";
        lblText4Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtText4
        //
        txtText4.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtText4.Dock = System.Windows.Forms.DockStyle.Fill;
        txtText4.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtText4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtText4.Name = "txtText4";
        txtText4.Radius = 4;
        txtText4.TabIndex = 13;
        tlpCpi.SetColumnSpan(txtText4, 3);
        //
        // lblText5Label
        //
        lblText5Label.Dock = System.Windows.Forms.DockStyle.Fill;
        lblText5Label.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblText5Label.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblText5Label.Name = "lblText5Label";
        lblText5Label.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblText5Label.TabIndex = 14;
        lblText5Label.Text = "Text 5:";
        lblText5Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtText5
        //
        txtText5.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtText5.Dock = System.Windows.Forms.DockStyle.Fill;
        txtText5.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtText5.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtText5.Name = "txtText5";
        txtText5.Radius = 4;
        txtText5.TabIndex = 15;
        tlpCpi.SetColumnSpan(txtText5, 3);
        //
        // flpCpiButtons
        //
        flpCpiButtons.BackColor = System.Drawing.Color.White;
        flpCpiButtons.Controls.Add(btnCpiWrite);
        flpCpiButtons.Controls.Add(btnCpiRead);
        flpCpiButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        flpCpiButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpCpiButtons.Margin = new System.Windows.Forms.Padding(0);
        flpCpiButtons.Name = "flpCpiButtons";
        flpCpiButtons.TabIndex = 16;
        flpCpiButtons.WrapContents = false;
        tlpCpi.SetColumnSpan(flpCpiButtons, 3);
        //
        // btnCpiWrite
        //
        btnCpiWrite.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnCpiWrite.ForeColor = System.Drawing.Color.White;
        btnCpiWrite.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
        btnCpiWrite.Name = "btnCpiWrite";
        btnCpiWrite.Radius = 8;
        btnCpiWrite.Size = new System.Drawing.Size(262, 58);
        btnCpiWrite.TabIndex = 0;
        btnCpiWrite.Text = "เขียนลง CPI.db3";
        btnCpiWrite.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCpiRead
        //
        btnCpiRead.BorderWidth = 2F;
        btnCpiRead.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCpiRead.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnCpiRead.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCpiRead.Margin = new System.Windows.Forms.Padding(0, 8, 3, 8);
        btnCpiRead.Name = "btnCpiRead";
        btnCpiRead.Radius = 8;
        btnCpiRead.Size = new System.Drawing.Size(238, 58);
        btnCpiRead.TabIndex = 1;
        btnCpiRead.Text = "อ่านค่าปัจจุบัน";
        btnCpiRead.Type = AntdUI.TTypeMini.Default;
        //
        // grpLog
        //
        grpLog.Controls.Add(txtLog);
        grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
        grpLog.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpLog.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpLog.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        grpLog.Name = "grpLog";
        grpLog.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpLog.TabIndex = 3;
        grpLog.TabStop = false;
        grpLog.Text = "ผลการทำงาน";
        //
        // txtLog
        //
        txtLog.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        txtLog.Font = new System.Drawing.Font("Consolas", 13F);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        txtLog.WordWrap = false;
        txtLog.TabIndex = 0;
        //
        // UvTestUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "UvTestUserControl";
        Size = new System.Drawing.Size(1225, 1462);

        tlpRoot.ResumeLayout(false);
        grpMachine.ResumeLayout(false);
        tlpMachine.ResumeLayout(false);
        tlpMachine.PerformLayout();
        flpPick.ResumeLayout(false);
        grpCommand.ResumeLayout(false);
        tlpCommand.ResumeLayout(false);
        tlpCommand.PerformLayout();
        flpCmdButtons.ResumeLayout(false);
        grpCpi.ResumeLayout(false);
        tlpCpi.ResumeLayout(false);
        tlpCpi.PerformLayout();
        flpCpiButtons.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;

    private System.Windows.Forms.GroupBox grpMachine;
    private System.Windows.Forms.TableLayoutPanel tlpMachine;
    private System.Windows.Forms.FlowLayoutPanel flpPick;
    private AntdUI.Button btnUv1;
    private AntdUI.Button btnUv2;
    private System.Windows.Forms.Label lblIpLabel;
    private IpAddressInput txtIp;
    private System.Windows.Forms.Label lblPortLabel;
    private AntdUI.Input txtPort;
    private AntdUI.Button btnCheck;
    private System.Windows.Forms.Label lblFolderLabel;
    private AntdUI.Input txtFolder;
    private AntdUI.Button btnBrowseFolder;
    private System.Windows.Forms.Label lblPathStatus;

    private System.Windows.Forms.GroupBox grpCommand;
    private System.Windows.Forms.TableLayoutPanel tlpCommand;
    private System.Windows.Forms.Label lblProgramLabel;
    private AntdUI.Input txtProgram;
    private AntdUI.Button btnFindProgram;
    private System.Windows.Forms.FlowLayoutPanel flpCmdButtons;
    private AntdUI.Button btnLoad;
    private AntdUI.Button btnStart;
    private AntdUI.Button btnStop;

    private System.Windows.Forms.GroupBox grpCpi;
    private System.Windows.Forms.TableLayoutPanel tlpCpi;
    private System.Windows.Forms.Label lblTableLabel;
    private AntdUI.Input txtTable;
    private System.Windows.Forms.Label lblLotLabel;
    private AntdUI.Input txtLot;
    private System.Windows.Forms.Label lblNameLabel;
    private AntdUI.Input txtName;
    private System.Windows.Forms.Label lblText1Label;
    private AntdUI.Input txtText1;
    private System.Windows.Forms.Label lblText2Label;
    private AntdUI.Input txtText2;
    private System.Windows.Forms.Label lblText3Label;
    private AntdUI.Input txtText3;
    private System.Windows.Forms.Label lblText4Label;
    private AntdUI.Input txtText4;
    private System.Windows.Forms.Label lblText5Label;
    private AntdUI.Input txtText5;
    private System.Windows.Forms.FlowLayoutPanel flpCpiButtons;
    private AntdUI.Button btnCpiRead;
    private AntdUI.Button btnCpiWrite;

    private System.Windows.Forms.GroupBox grpLog;
    private System.Windows.Forms.TextBox txtLog;
}
