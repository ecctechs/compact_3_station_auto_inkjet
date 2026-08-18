namespace InkjetOperator.Views;

partial class ClampSettingUserControl
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
        grpConnection = new System.Windows.Forms.GroupBox();
        tlpConn = new System.Windows.Forms.TableLayoutPanel();
        lblIpLabel = new System.Windows.Forms.Label();
        txtIp = new AntdUI.Input();
        btnCheckStatus = new AntdUI.Button();
        lblPortLabel = new System.Windows.Forms.Label();
        txtPort = new AntdUI.Input();
        lblDbLabel = new System.Windows.Forms.Label();
        txtDbPath = new AntdUI.Input();
        btnBrowse = new AntdUI.Button();
        lblStatus = new System.Windows.Forms.Label();
        grpAddress = new System.Windows.Forms.GroupBox();
        tlpAddr = new System.Windows.Forms.TableLayoutPanel();
        lblTargetLabel = new System.Windows.Forms.Label();
        txtAddrTarget = new AntdUI.Input();
        lblRunLabel = new System.Windows.Forms.Label();
        txtAddrRun = new AntdUI.Input();
        lblResetLabel = new System.Windows.Forms.Label();
        txtAddrReset = new AntdUI.Input();
        lblStatusAddrLabel = new System.Windows.Forms.Label();
        txtAddrStatus = new AntdUI.Input();
        grpClamp = new System.Windows.Forms.GroupBox();
        tlpClamp = new System.Windows.Forms.TableLayoutPanel();
        lblProgramLabel = new System.Windows.Forms.Label();
        txtProgram = new AntdUI.Input();
        btnLoad = new AntdUI.Button();
        lblMmLabel = new System.Windows.Forms.Label();
        txtValueMm = new AntdUI.Input();
        lblRawLabel = new System.Windows.Forms.Label();
        lblRawValue = new System.Windows.Forms.Label();
        flpClampActions = new System.Windows.Forms.FlowLayoutPanel();
        btnApply = new AntdUI.Button();
        btnReset = new AntdUI.Button();
        btnReadStatus = new AntdUI.Button();
        btnUpload = new AntdUI.Button();
        txtLog = new System.Windows.Forms.TextBox();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpConnection.SuspendLayout();
        tlpConn.SuspendLayout();
        grpAddress.SuspendLayout();
        tlpAddr.SuspendLayout();
        grpClamp.SuspendLayout();
        tlpClamp.SuspendLayout();
        flpClampActions.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpConnection, 0, 0);
        tlpRoot.Controls.Add(grpAddress, 0, 1);
        tlpRoot.Controls.Add(grpClamp, 0, 2);
        tlpRoot.Controls.Add(flpActions, 0, 3);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 4;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(880, 900);
        tlpRoot.TabIndex = 0;
        //
        // grpConnection
        //
        grpConnection.AutoSize = true;
        grpConnection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpConnection.Controls.Add(tlpConn);
        grpConnection.Dock = System.Windows.Forms.DockStyle.Fill;
        grpConnection.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpConnection.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpConnection.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpConnection.Name = "grpConnection";
        grpConnection.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "PLC แคลมป์ (MC Protocol)";
        //
        // tlpConn — 4 cols: label(160) | input(fill) | gap(8) | button(110)
        //
        tlpConn.BackColor = System.Drawing.Color.White;
        tlpConn.ColumnCount = 4;
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
        tlpConn.Controls.Add(lblIpLabel, 0, 0);
        tlpConn.Controls.Add(txtIp, 1, 0);
        tlpConn.Controls.Add(btnCheckStatus, 3, 0);
        tlpConn.Controls.Add(lblPortLabel, 0, 1);
        tlpConn.Controls.Add(txtPort, 1, 1);
        tlpConn.Controls.Add(lblDbLabel, 0, 2);
        tlpConn.Controls.Add(txtDbPath, 1, 2);
        tlpConn.Controls.Add(btnBrowse, 3, 2);
        tlpConn.Controls.Add(lblStatus, 1, 3);
        tlpConn.AutoSize = true;
        tlpConn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpConn.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpConn.Name = "tlpConn";
        tlpConn.RowCount = 4;
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
        tlpConn.TabIndex = 0;
        //
        // lblIpLabel
        //
        lblIpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblIpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblIpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblIpLabel.Name = "lblIpLabel";
        lblIpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblIpLabel.TabIndex = 0;
        lblIpLabel.Text = "PLC IP :";
        lblIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtIp
        //
        txtIp.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtIp.Dock = System.Windows.Forms.DockStyle.Fill;
        txtIp.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtIp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtIp.Name = "txtIp";
        txtIp.PlaceholderText = "10.10.100.100";
        txtIp.Radius = 4;
        txtIp.TabIndex = 1;
        //
        // btnCheckStatus
        //
        btnCheckStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnCheckStatus.BorderWidth = 2F;
        btnCheckStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheckStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnCheckStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheckStatus.Name = "btnCheckStatus";
        btnCheckStatus.Radius = 6;
        btnCheckStatus.Size = new System.Drawing.Size(104, 34);
        btnCheckStatus.TabIndex = 2;
        btnCheckStatus.Text = "Check";
        btnCheckStatus.Type = AntdUI.TTypeMini.Default;
        //
        // lblPortLabel
        //
        lblPortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPortLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPortLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPortLabel.Name = "lblPortLabel";
        lblPortLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblPortLabel.TabIndex = 3;
        lblPortLabel.Text = "Port :";
        lblPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPort
        //
        txtPort.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPort.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPort.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtPort.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPort.Name = "txtPort";
        txtPort.PlaceholderText = "5012";
        txtPort.Radius = 4;
        txtPort.TabIndex = 4;
        //
        // lblDbLabel
        //
        lblDbLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDbLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblDbLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDbLabel.Name = "lblDbLabel";
        lblDbLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblDbLabel.TabIndex = 5;
        lblDbLabel.Text = "mydatabase.db3 :";
        lblDbLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtDbPath
        //
        txtDbPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtDbPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtDbPath.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtDbPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtDbPath.Name = "txtDbPath";
        txtDbPath.PlaceholderText = "เลือกไฟล์ mydatabase.db3 (ตาราง MainTable)...";
        txtDbPath.Radius = 4;
        txtDbPath.ReadOnly = true;
        txtDbPath.TabIndex = 6;
        //
        // btnBrowse
        //
        btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowse.BorderWidth = 2F;
        btnBrowse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnBrowse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Radius = 6;
        btnBrowse.Size = new System.Drawing.Size(104, 34);
        btnBrowse.TabIndex = 7;
        btnBrowse.Text = "Browse...";
        btnBrowse.Type = AntdUI.TTypeMini.Default;
        //
        // lblStatus
        //
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblStatus.ForeColor = System.Drawing.Color.Gray;
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblStatus.TabIndex = 8;
        lblStatus.Text = "";
        tlpConn.SetColumnSpan(lblStatus, 3);
        //
        // grpAddress
        //
        grpAddress.AutoSize = true;
        grpAddress.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpAddress.Controls.Add(tlpAddr);
        grpAddress.Dock = System.Windows.Forms.DockStyle.Fill;
        grpAddress.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpAddress.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpAddress.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpAddress.Name = "grpAddress";
        grpAddress.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpAddress.TabIndex = 1;
        grpAddress.TabStop = false;
        grpAddress.Text = "Register Address";
        //
        // tlpAddr — 4 cols: label | input | label | input
        //
        tlpAddr.BackColor = System.Drawing.Color.White;
        tlpAddr.ColumnCount = 4;
        tlpAddr.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        tlpAddr.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpAddr.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        tlpAddr.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpAddr.Controls.Add(lblTargetLabel, 0, 0);
        tlpAddr.Controls.Add(txtAddrTarget, 1, 0);
        tlpAddr.Controls.Add(lblRunLabel, 2, 0);
        tlpAddr.Controls.Add(txtAddrRun, 3, 0);
        tlpAddr.Controls.Add(lblResetLabel, 0, 1);
        tlpAddr.Controls.Add(txtAddrReset, 1, 1);
        tlpAddr.Controls.Add(lblStatusAddrLabel, 2, 1);
        tlpAddr.Controls.Add(txtAddrStatus, 3, 1);
        tlpAddr.AutoSize = true;
        tlpAddr.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpAddr.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpAddr.Name = "tlpAddr";
        tlpAddr.RowCount = 2;
        tlpAddr.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpAddr.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpAddr.TabIndex = 0;
        //
        // lblTargetLabel
        //
        lblTargetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTargetLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblTargetLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblTargetLabel.Name = "lblTargetLabel";
        lblTargetLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblTargetLabel.TabIndex = 0;
        lblTargetLabel.Text = "ค่าเป้าหมาย :";
        lblTargetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtAddrTarget
        //
        txtAddrTarget.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtAddrTarget.Dock = System.Windows.Forms.DockStyle.Fill;
        txtAddrTarget.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtAddrTarget.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtAddrTarget.Name = "txtAddrTarget";
        txtAddrTarget.PlaceholderText = "D216";
        txtAddrTarget.Radius = 4;
        txtAddrTarget.TabIndex = 1;
        //
        // lblRunLabel
        //
        lblRunLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRunLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblRunLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblRunLabel.Name = "lblRunLabel";
        lblRunLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblRunLabel.TabIndex = 2;
        lblRunLabel.Text = "พัลส์สั่งวิ่ง :";
        lblRunLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtAddrRun
        //
        txtAddrRun.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtAddrRun.Dock = System.Windows.Forms.DockStyle.Fill;
        txtAddrRun.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtAddrRun.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtAddrRun.Name = "txtAddrRun";
        txtAddrRun.PlaceholderText = "M700";
        txtAddrRun.Radius = 4;
        txtAddrRun.TabIndex = 3;
        //
        // lblResetLabel
        //
        lblResetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblResetLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblResetLabel.Name = "lblResetLabel";
        lblResetLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblResetLabel.TabIndex = 4;
        lblResetLabel.Text = "พัลส์รีเซ็ต :";
        lblResetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtAddrReset
        //
        txtAddrReset.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtAddrReset.Dock = System.Windows.Forms.DockStyle.Fill;
        txtAddrReset.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtAddrReset.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtAddrReset.Name = "txtAddrReset";
        txtAddrReset.PlaceholderText = "M701";
        txtAddrReset.Radius = 4;
        txtAddrReset.TabIndex = 5;
        //
        // lblStatusAddrLabel
        //
        lblStatusAddrLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatusAddrLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblStatusAddrLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblStatusAddrLabel.Name = "lblStatusAddrLabel";
        lblStatusAddrLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblStatusAddrLabel.TabIndex = 6;
        lblStatusAddrLabel.Text = "อ่านสถานะ :";
        lblStatusAddrLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtAddrStatus
        //
        txtAddrStatus.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtAddrStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        txtAddrStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtAddrStatus.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtAddrStatus.Name = "txtAddrStatus";
        txtAddrStatus.PlaceholderText = "W38 (ฐานสิบหก)";
        txtAddrStatus.Radius = 4;
        txtAddrStatus.TabIndex = 7;
        //
        // grpClamp
        //
        grpClamp.Controls.Add(tlpClamp);
        grpClamp.Dock = System.Windows.Forms.DockStyle.Fill;
        grpClamp.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpClamp.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpClamp.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpClamp.MinimumSize = new System.Drawing.Size(0, 340);
        grpClamp.Name = "grpClamp";
        grpClamp.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpClamp.TabIndex = 2;
        grpClamp.TabStop = false;
        grpClamp.Text = "ควบคุมแคลมป์";
        //
        // tlpClamp — 4 cols: label(160) | input(fill) | gap(8) | button(110)
        //
        tlpClamp.BackColor = System.Drawing.Color.White;
        tlpClamp.ColumnCount = 4;
        tlpClamp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        tlpClamp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpClamp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        tlpClamp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
        tlpClamp.Controls.Add(lblProgramLabel, 0, 0);
        tlpClamp.Controls.Add(txtProgram, 1, 0);
        tlpClamp.Controls.Add(btnLoad, 3, 0);
        tlpClamp.Controls.Add(lblMmLabel, 0, 1);
        tlpClamp.Controls.Add(txtValueMm, 1, 1);
        tlpClamp.Controls.Add(lblRawLabel, 0, 2);
        tlpClamp.Controls.Add(lblRawValue, 1, 2);
        tlpClamp.Controls.Add(flpClampActions, 1, 3);
        tlpClamp.Controls.Add(txtLog, 0, 4);
        tlpClamp.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpClamp.Name = "tlpClamp";
        tlpClamp.RowCount = 5;
        tlpClamp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpClamp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpClamp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
        tlpClamp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        tlpClamp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpClamp.TabIndex = 0;
        //
        // lblProgramLabel
        //
        lblProgramLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProgramLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblProgramLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblProgramLabel.Name = "lblProgramLabel";
        lblProgramLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblProgramLabel.TabIndex = 0;
        lblProgramLabel.Text = "Program Name :";
        lblProgramLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtProgram
        //
        txtProgram.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        txtProgram.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtProgram.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtProgram.Name = "txtProgram";
        txtProgram.PlaceholderText = "เช่น S-DEX-1624-1  (ขึ้นต้น P- จะใช้ IAIP)";
        txtProgram.Radius = 4;
        txtProgram.TabIndex = 1;
        //
        // btnLoad
        //
        btnLoad.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnLoad.BorderWidth = 2F;
        btnLoad.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnLoad.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnLoad.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnLoad.Name = "btnLoad";
        btnLoad.Radius = 6;
        btnLoad.Size = new System.Drawing.Size(104, 34);
        btnLoad.TabIndex = 2;
        btnLoad.Text = "Load SQL";
        btnLoad.Type = AntdUI.TTypeMini.Default;
        //
        // lblMmLabel
        //
        lblMmLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMmLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMmLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMmLabel.Name = "lblMmLabel";
        lblMmLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblMmLabel.TabIndex = 3;
        lblMmLabel.Text = "ระยะแคลมป์ (0-155) :";
        lblMmLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtValueMm
        //
        txtValueMm.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtValueMm.Dock = System.Windows.Forms.DockStyle.Fill;
        txtValueMm.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtValueMm.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtValueMm.Name = "txtValueMm";
        txtValueMm.PlaceholderText = "mm";
        txtValueMm.Radius = 4;
        txtValueMm.TabIndex = 4;
        //
        // lblRawLabel
        //
        lblRawLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRawLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblRawLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblRawLabel.Name = "lblRawLabel";
        lblRawLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblRawLabel.TabIndex = 5;
        lblRawLabel.Text = "ค่าที่จะเขียน :";
        lblRawLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // lblRawValue
        //
        lblRawValue.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRawValue.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblRawValue.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblRawValue.Name = "lblRawValue";
        lblRawValue.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
        lblRawValue.TabIndex = 6;
        lblRawValue.Text = "-";
        lblRawValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        tlpClamp.SetColumnSpan(lblRawValue, 3);
        //
        // flpClampActions
        //
        flpClampActions.BackColor = System.Drawing.Color.White;
        flpClampActions.Controls.Add(btnApply);
        flpClampActions.Controls.Add(btnUpload);
        flpClampActions.Controls.Add(btnReset);
        flpClampActions.Controls.Add(btnReadStatus);
        flpClampActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpClampActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpClampActions.Margin = new System.Windows.Forms.Padding(0);
        flpClampActions.Name = "flpClampActions";
        flpClampActions.TabIndex = 7;
        flpClampActions.WrapContents = false;
        tlpClamp.SetColumnSpan(flpClampActions, 3);
        //
        // btnApply
        //
        btnApply.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnApply.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnApply.ForeColor = System.Drawing.Color.White;
        btnApply.Margin = new System.Windows.Forms.Padding(3, 8, 12, 8);
        btnApply.Name = "btnApply";
        btnApply.Radius = 6;
        btnApply.Size = new System.Drawing.Size(150, 36);
        btnApply.TabIndex = 0;
        btnApply.Text = "สั่งแคลมป์";
        btnApply.Type = AntdUI.TTypeMini.Primary;
        //
        // btnReset
        //
        btnReset.BorderWidth = 2F;
        btnReset.DefaultBorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReset.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnReset.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReset.Margin = new System.Windows.Forms.Padding(3, 8, 12, 8);
        btnReset.Name = "btnReset";
        btnReset.Radius = 6;
        btnReset.Size = new System.Drawing.Size(120, 36);
        btnReset.TabIndex = 1;
        btnReset.Text = "Reset";
        btnReset.Type = AntdUI.TTypeMini.Default;
        //
        // btnReadStatus
        //
        btnReadStatus.BorderWidth = 2F;
        btnReadStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnReadStatus.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnReadStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnReadStatus.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        btnReadStatus.Name = "btnReadStatus";
        btnReadStatus.Radius = 6;
        btnReadStatus.Size = new System.Drawing.Size(140, 36);
        btnReadStatus.TabIndex = 2;
        btnReadStatus.Text = "อ่านสถานะ";
        btnReadStatus.Type = AntdUI.TTypeMini.Default;
        //
        // btnUpload
        //
        btnUpload.BorderWidth = 2F;
        btnUpload.DefaultBorderColor = System.Drawing.Color.FromArgb(76, 175, 80);
        btnUpload.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnUpload.ForeColor = System.Drawing.Color.FromArgb(56, 130, 60);
        btnUpload.Margin = new System.Windows.Forms.Padding(3, 8, 12, 8);
        btnUpload.Name = "btnUpload";
        btnUpload.Radius = 6;
        btnUpload.Size = new System.Drawing.Size(150, 36);
        btnUpload.TabIndex = 1;
        btnUpload.Text = "Upload ลง SQL";
        btnUpload.Type = AntdUI.TTypeMini.Default;
        //
        // txtLog
        //
        txtLog.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        txtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
        txtLog.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.TabIndex = 8;
        tlpClamp.SetColumnSpan(txtLog, 4);
        //
        // flpActions
        //
        flpActions.BackColor = System.Drawing.Color.White;
        flpActions.Controls.Add(btnSave);
        flpActions.Controls.Add(btnCancel);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.TabIndex = 3;
        flpActions.WrapContents = false;
        //
        // btnSave
        //
        btnSave.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        btnSave.ForeColor = System.Drawing.Color.White;
        btnSave.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnSave.Name = "btnSave";
        btnSave.Radius = 6;
        btnSave.Size = new System.Drawing.Size(140, 40);
        btnSave.TabIndex = 0;
        btnSave.Text = "Save";
        btnSave.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.BorderWidth = 2F;
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Margin = new System.Windows.Forms.Padding(3, 12, 12, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 6;
        btnCancel.Size = new System.Drawing.Size(140, 40);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // ClampSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "ClampSettingUserControl";
        Size = new System.Drawing.Size(880, 900);

        tlpRoot.ResumeLayout(false);
        grpConnection.ResumeLayout(false);
        tlpConn.ResumeLayout(false);
        tlpConn.PerformLayout();
        grpAddress.ResumeLayout(false);
        tlpAddr.ResumeLayout(false);
        tlpAddr.PerformLayout();
        grpClamp.ResumeLayout(false);
        tlpClamp.ResumeLayout(false);
        tlpClamp.PerformLayout();
        flpClampActions.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.TableLayoutPanel tlpConn;
    private System.Windows.Forms.Label lblIpLabel;
    private AntdUI.Input txtIp;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.Label lblPortLabel;
    private AntdUI.Input txtPort;
    private System.Windows.Forms.Label lblDbLabel;
    private AntdUI.Input txtDbPath;
    private AntdUI.Button btnBrowse;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.GroupBox grpAddress;
    private System.Windows.Forms.TableLayoutPanel tlpAddr;
    private System.Windows.Forms.Label lblTargetLabel;
    private AntdUI.Input txtAddrTarget;
    private System.Windows.Forms.Label lblRunLabel;
    private AntdUI.Input txtAddrRun;
    private System.Windows.Forms.Label lblResetLabel;
    private AntdUI.Input txtAddrReset;
    private System.Windows.Forms.Label lblStatusAddrLabel;
    private AntdUI.Input txtAddrStatus;
    private System.Windows.Forms.GroupBox grpClamp;
    private System.Windows.Forms.TableLayoutPanel tlpClamp;
    private System.Windows.Forms.Label lblProgramLabel;
    private AntdUI.Input txtProgram;
    private AntdUI.Button btnLoad;
    private System.Windows.Forms.Label lblMmLabel;
    private AntdUI.Input txtValueMm;
    private System.Windows.Forms.Label lblRawLabel;
    private System.Windows.Forms.Label lblRawValue;
    private System.Windows.Forms.FlowLayoutPanel flpClampActions;
    private AntdUI.Button btnApply;
    private AntdUI.Button btnReset;
    private AntdUI.Button btnReadStatus;
    private AntdUI.Button btnUpload;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
