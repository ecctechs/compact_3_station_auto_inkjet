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
        lblPortLabel = new System.Windows.Forms.Label();
        txtPort = new AntdUI.Input();
        btnCheckStatus = new AntdUI.Button();
        lblDbLabel = new System.Windows.Forms.Label();
        txtDbPath = new AntdUI.Input();
        btnBrowse = new AntdUI.Button();
        lblStatus = new System.Windows.Forms.Label();

        grpProgram = new System.Windows.Forms.GroupBox();
        tlpProgram = new System.Windows.Forms.TableLayoutPanel();
        lblPlateLabel = new System.Windows.Forms.Label();
        txtPlateProgram = new AntdUI.Input();
        lblShimLabel = new System.Windows.Forms.Label();
        txtShimProgram = new AntdUI.Input();
        flpProgramActions = new System.Windows.Forms.FlowLayoutPanel();
        btnLoadAll = new AntdUI.Button();
        btnApplyAll = new AntdUI.Button();
        btnUploadAll = new AntdUI.Button();

        grpAxes = new System.Windows.Forms.GroupBox();
        tlpAxes = new System.Windows.Forms.TableLayoutPanel();
        tlpAxesHeader = new System.Windows.Forms.TableLayoutPanel();
        lblAxesHint = new System.Windows.Forms.Label();
        btnUnlock = new AntdUI.Button();
        tblAxes = new AntdUI.Table();

        grpLog = new System.Windows.Forms.GroupBox();
        txtLog = new System.Windows.Forms.TextBox();

        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpConnection.SuspendLayout();
        tlpConn.SuspendLayout();
        grpProgram.SuspendLayout();
        tlpProgram.SuspendLayout();
        flpProgramActions.SuspendLayout();
        grpAxes.SuspendLayout();
        tlpAxes.SuspendLayout();
        tlpAxesHeader.SuspendLayout();
        grpLog.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpConnection, 0, 0);
        tlpRoot.Controls.Add(grpProgram, 0, 1);
        tlpRoot.Controls.Add(grpAxes, 0, 2);
        tlpRoot.Controls.Add(grpLog, 0, 3);
        tlpRoot.Controls.Add(flpActions, 0, 4);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 5;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 382F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 260F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(1080, 1112);
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
        grpConnection.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "1. PLC แคลมป์ (MC Protocol) — คุมทั้ง 6 แกน";
        //
        // tlpConn — label(150) | input(fill) | label(60) | input(110) | button(130)
        //
        tlpConn.AutoSize = true;
        tlpConn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpConn.BackColor = System.Drawing.Color.White;
        tlpConn.ColumnCount = 5;
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        tlpConn.Controls.Add(lblIpLabel, 0, 0);
        tlpConn.Controls.Add(txtIp, 1, 0);
        tlpConn.Controls.Add(lblPortLabel, 2, 0);
        tlpConn.Controls.Add(txtPort, 3, 0);
        tlpConn.Controls.Add(btnCheckStatus, 4, 0);
        tlpConn.Controls.Add(lblDbLabel, 0, 1);
        tlpConn.Controls.Add(txtDbPath, 1, 1);
        tlpConn.Controls.Add(btnBrowse, 4, 1);
        tlpConn.Controls.Add(lblStatus, 1, 2);
        tlpConn.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpConn.Name = "tlpConn";
        tlpConn.RowCount = 3;
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
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
        // lblPortLabel
        //
        lblPortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPortLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPortLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPortLabel.Name = "lblPortLabel";
        lblPortLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblPortLabel.TabIndex = 2;
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
        txtPort.TabIndex = 3;
        //
        // btnCheckStatus
        //
        btnCheckStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnCheckStatus.BorderWidth = 2F;
        btnCheckStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheckStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnCheckStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheckStatus.Name = "btnCheckStatus";
        btnCheckStatus.Radius = 6;
        btnCheckStatus.Size = new System.Drawing.Size(124, 36);
        btnCheckStatus.TabIndex = 4;
        btnCheckStatus.Text = "เช็คการเชื่อมต่อ";
        btnCheckStatus.Type = AntdUI.TTypeMini.Default;
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
        tlpConn.SetColumnSpan(txtDbPath, 3);
        //
        // btnBrowse
        //
        btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowse.BorderWidth = 2F;
        btnBrowse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowse.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnBrowse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Radius = 6;
        btnBrowse.Size = new System.Drawing.Size(124, 36);
        btnBrowse.TabIndex = 7;
        btnBrowse.Text = "Browse...";
        btnBrowse.Type = AntdUI.TTypeMini.Default;
        //
        // lblStatus
        //
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        lblStatus.ForeColor = System.Drawing.Color.Gray;
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
        lblStatus.TabIndex = 8;
        lblStatus.Text = "";
        tlpConn.SetColumnSpan(lblStatus, 4);
        //
        // grpProgram
        //
        grpProgram.AutoSize = true;
        grpProgram.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpProgram.Controls.Add(tlpProgram);
        grpProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        grpProgram.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpProgram.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpProgram.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpProgram.Name = "grpProgram";
        grpProgram.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpProgram.TabIndex = 1;
        grpProgram.TabStop = false;
        grpProgram.Text = "2. ชื่อโปรแกรมของแต่ละฝั่ง";
        //
        // tlpProgram — label(150) | input(fill) | label(150) | input(fill)
        //
        tlpProgram.AutoSize = true;
        tlpProgram.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpProgram.BackColor = System.Drawing.Color.White;
        tlpProgram.ColumnCount = 4;
        tlpProgram.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpProgram.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpProgram.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpProgram.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpProgram.Controls.Add(lblPlateLabel, 0, 0);
        tlpProgram.Controls.Add(txtPlateProgram, 1, 0);
        tlpProgram.Controls.Add(lblShimLabel, 2, 0);
        tlpProgram.Controls.Add(txtShimProgram, 3, 0);
        tlpProgram.Controls.Add(flpProgramActions, 0, 1);
        tlpProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpProgram.Name = "tlpProgram";
        tlpProgram.RowCount = 2;
        tlpProgram.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpProgram.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpProgram.TabIndex = 0;
        //
        // lblPlateLabel
        //
        lblPlateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPlateLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPlateLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPlateLabel.Name = "lblPlateLabel";
        lblPlateLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblPlateLabel.TabIndex = 0;
        lblPlateLabel.Text = "Plate (m1) :";
        lblPlateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPlateProgram
        //
        txtPlateProgram.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPlateProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPlateProgram.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtPlateProgram.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPlateProgram.Name = "txtPlateProgram";
        txtPlateProgram.PlaceholderText = "เช่น P-DEX-681";
        txtPlateProgram.Radius = 4;
        txtPlateProgram.TabIndex = 1;
        //
        // lblShimLabel
        //
        lblShimLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblShimLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblShimLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblShimLabel.Name = "lblShimLabel";
        lblShimLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblShimLabel.TabIndex = 2;
        lblShimLabel.Text = "Shim (m2) :";
        lblShimLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtShimProgram
        //
        txtShimProgram.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtShimProgram.Dock = System.Windows.Forms.DockStyle.Fill;
        txtShimProgram.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtShimProgram.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtShimProgram.Name = "txtShimProgram";
        txtShimProgram.PlaceholderText = "เช่น S-DEX-681";
        txtShimProgram.Radius = 4;
        txtShimProgram.TabIndex = 3;
        //
        // flpProgramActions
        //
        flpProgramActions.BackColor = System.Drawing.Color.White;
        flpProgramActions.Controls.Add(btnLoadAll);
        flpProgramActions.Controls.Add(btnApplyAll);
        flpProgramActions.Controls.Add(btnUploadAll);
        flpProgramActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpProgramActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpProgramActions.Margin = new System.Windows.Forms.Padding(0);
        flpProgramActions.Name = "flpProgramActions";
        flpProgramActions.TabIndex = 4;
        flpProgramActions.WrapContents = false;
        tlpProgram.SetColumnSpan(flpProgramActions, 4);
        //
        // btnLoadAll
        //
        btnLoadAll.BorderWidth = 2F;
        btnLoadAll.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnLoadAll.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnLoadAll.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnLoadAll.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
        btnLoadAll.Name = "btnLoadAll";
        btnLoadAll.Radius = 6;
        btnLoadAll.Size = new System.Drawing.Size(190, 42);
        btnLoadAll.TabIndex = 0;
        btnLoadAll.Text = "Load ทั้ง 6 แกน";
        btnLoadAll.Type = AntdUI.TTypeMini.Default;
        //
        // btnApplyAll
        //
        btnApplyAll.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnApplyAll.ForeColor = System.Drawing.Color.White;
        btnApplyAll.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
        btnApplyAll.Name = "btnApplyAll";
        btnApplyAll.Radius = 6;
        btnApplyAll.Size = new System.Drawing.Size(210, 42);
        btnApplyAll.TabIndex = 1;
        btnApplyAll.Text = "สั่งทุกแกนที่พร้อม";
        btnApplyAll.Type = AntdUI.TTypeMini.Primary;
        //
        // btnUploadAll
        //
        btnUploadAll.BorderWidth = 2F;
        btnUploadAll.DefaultBorderColor = System.Drawing.Color.FromArgb(76, 175, 80);
        btnUploadAll.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnUploadAll.ForeColor = System.Drawing.Color.FromArgb(56, 130, 60);
        btnUploadAll.Margin = new System.Windows.Forms.Padding(0, 8, 3, 8);
        btnUploadAll.Name = "btnUploadAll";
        btnUploadAll.Radius = 6;
        btnUploadAll.Size = new System.Drawing.Size(190, 42);
        btnUploadAll.TabIndex = 2;
        btnUploadAll.Text = "Upload ทั้งหมด";
        btnUploadAll.Type = AntdUI.TTypeMini.Default;
        //
        // grpAxes
        //
        grpAxes.Controls.Add(tlpAxes);
        grpAxes.Dock = System.Windows.Forms.DockStyle.Fill;
        grpAxes.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpAxes.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpAxes.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        grpAxes.Name = "grpAxes";
        grpAxes.Padding = new System.Windows.Forms.Padding(16, 24, 16, 10);
        grpAxes.TabIndex = 2;
        grpAxes.TabStop = false;
        grpAxes.Text = "3. แกนแคลมป์ (Plate / Shim × X, Z1, Z2)";
        //
        // tlpAxes
        //
        tlpAxes.BackColor = System.Drawing.Color.White;
        tlpAxes.ColumnCount = 1;
        tlpAxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAxes.Controls.Add(tlpAxesHeader, 0, 0);
        tlpAxes.Controls.Add(tblAxes, 0, 1);
        tlpAxes.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpAxes.Margin = new System.Windows.Forms.Padding(0);
        tlpAxes.Name = "tlpAxes";
        tlpAxes.RowCount = 2;
        tlpAxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        tlpAxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAxes.TabIndex = 0;
        //
        // tlpAxesHeader
        //
        tlpAxesHeader.BackColor = System.Drawing.Color.White;
        tlpAxesHeader.ColumnCount = 2;
        tlpAxesHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAxesHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpAxesHeader.Controls.Add(lblAxesHint, 0, 0);
        tlpAxesHeader.Controls.Add(btnUnlock, 1, 0);
        tlpAxesHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpAxesHeader.Margin = new System.Windows.Forms.Padding(0);
        tlpAxesHeader.Name = "tlpAxesHeader";
        tlpAxesHeader.RowCount = 1;
        tlpAxesHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAxesHeader.TabIndex = 0;
        //
        // lblAxesHint
        //
        lblAxesHint.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAxesHint.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblAxesHint.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblAxesHint.Name = "lblAxesHint";
        lblAxesHint.TabIndex = 0;
        lblAxesHint.Text = "ค่า (mm) แก้ได้ตลอด · ช่อง address ต้องปลดล็อกก่อน";
        lblAxesHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnUnlock
        //
        btnUnlock.Anchor = System.Windows.Forms.AnchorStyles.Right;
        btnUnlock.BorderWidth = 2F;
        btnUnlock.DefaultBorderColor = System.Drawing.Color.FromArgb(220, 160, 40);
        btnUnlock.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnUnlock.ForeColor = System.Drawing.Color.FromArgb(140, 90, 10);
        btnUnlock.Name = "btnUnlock";
        btnUnlock.Radius = 8;
        btnUnlock.Size = new System.Drawing.Size(134, 42);
        btnUnlock.TabIndex = 1;
        btnUnlock.Text = "🔒 Unlock";
        btnUnlock.Type = AntdUI.TTypeMini.Default;
        //
        // tblAxes
        //
        tblAxes.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        tblAxes.Bordered = true;
        tblAxes.ColumnBack = System.Drawing.Color.FromArgb(30, 30, 30);
        tblAxes.ColumnFont = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        tblAxes.ColumnFore = System.Drawing.Color.White;
        tblAxes.Dock = System.Windows.Forms.DockStyle.Fill;
        tblAxes.EditMode = AntdUI.TEditMode.Click;
        tblAxes.EmptyText = "ไม่มีข้อมูลแกน";
        tblAxes.Font = new System.Drawing.Font("Segoe UI", 11F);
        tblAxes.Name = "tblAxes";
        tblAxes.Radius = 8;
        tblAxes.RowHeight = 46;
        tblAxes.TabIndex = 1;
        //
        // grpLog
        //
        grpLog.Controls.Add(txtLog);
        grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
        grpLog.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpLog.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpLog.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
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
        txtLog.Font = new System.Drawing.Font("Consolas", 10.5F);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        txtLog.TabIndex = 0;
        txtLog.WordWrap = false;
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
        flpActions.TabIndex = 4;
        flpActions.WrapContents = false;
        //
        // btnSave
        //
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
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "ClampSettingUserControl";
        Size = new System.Drawing.Size(1080, 1112);

        tlpRoot.ResumeLayout(false);
        grpConnection.ResumeLayout(false);
        tlpConn.ResumeLayout(false);
        tlpConn.PerformLayout();
        grpProgram.ResumeLayout(false);
        tlpProgram.ResumeLayout(false);
        tlpProgram.PerformLayout();
        flpProgramActions.ResumeLayout(false);
        grpAxes.ResumeLayout(false);
        tlpAxes.ResumeLayout(false);
        tlpAxesHeader.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;

    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.TableLayoutPanel tlpConn;
    private System.Windows.Forms.Label lblIpLabel;
    private AntdUI.Input txtIp;
    private System.Windows.Forms.Label lblPortLabel;
    private AntdUI.Input txtPort;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.Label lblDbLabel;
    private AntdUI.Input txtDbPath;
    private AntdUI.Button btnBrowse;
    private System.Windows.Forms.Label lblStatus;

    private System.Windows.Forms.GroupBox grpProgram;
    private System.Windows.Forms.TableLayoutPanel tlpProgram;
    private System.Windows.Forms.Label lblPlateLabel;
    private AntdUI.Input txtPlateProgram;
    private System.Windows.Forms.Label lblShimLabel;
    private AntdUI.Input txtShimProgram;
    private System.Windows.Forms.FlowLayoutPanel flpProgramActions;
    private AntdUI.Button btnLoadAll;
    private AntdUI.Button btnApplyAll;
    private AntdUI.Button btnUploadAll;

    private System.Windows.Forms.GroupBox grpAxes;
    private System.Windows.Forms.TableLayoutPanel tlpAxes;
    private System.Windows.Forms.TableLayoutPanel tlpAxesHeader;
    private System.Windows.Forms.Label lblAxesHint;
    private AntdUI.Button btnUnlock;
    private AntdUI.Table tblAxes;

    private System.Windows.Forms.GroupBox grpLog;
    private System.Windows.Forms.TextBox txtLog;

    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
