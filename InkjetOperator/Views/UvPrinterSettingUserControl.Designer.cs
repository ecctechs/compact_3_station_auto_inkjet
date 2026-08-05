namespace InkjetOperator.Views;

partial class UvPrinterSettingUserControl
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

        grpUv1 = new System.Windows.Forms.GroupBox();
        tlpUv1 = new System.Windows.Forms.TableLayoutPanel();
        lblUv1Dot = new System.Windows.Forms.Label();
        lblUv1Badge = new System.Windows.Forms.Label();
        btnUv1Edit = new AntdUI.Button();
        lblUv1IpLabel = new System.Windows.Forms.Label();
        txtUv1Ip = new AntdUI.Input();
        lblUv1Colon = new System.Windows.Forms.Label();
        txtUv1Port = new AntdUI.Input();
        lblUv1FolderLabel = new System.Windows.Forms.Label();
        txtUv1Folder = new AntdUI.Input();
        btnUv1Browse = new AntdUI.Button();
        lblUv1Status = new System.Windows.Forms.Label();

        grpUv2 = new System.Windows.Forms.GroupBox();
        tlpUv2 = new System.Windows.Forms.TableLayoutPanel();
        lblUv2Dot = new System.Windows.Forms.Label();
        lblUv2Badge = new System.Windows.Forms.Label();
        btnUv2Edit = new AntdUI.Button();
        lblUv2IpLabel = new System.Windows.Forms.Label();
        txtUv2Ip = new AntdUI.Input();
        lblUv2Colon = new System.Windows.Forms.Label();
        txtUv2Port = new AntdUI.Input();
        lblUv2FolderLabel = new System.Windows.Forms.Label();
        txtUv2Folder = new AntdUI.Input();
        btnUv2Browse = new AntdUI.Button();
        lblUv2Status = new System.Windows.Forms.Label();

        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpUv1.SuspendLayout();
        tlpUv1.SuspendLayout();
        grpUv2.SuspendLayout();
        tlpUv2.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpUv1, 0, 0);
        tlpRoot.Controls.Add(grpUv2, 0, 1);
        tlpRoot.Controls.Add(flpActions, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(780, 800);
        tlpRoot.TabIndex = 0;

        // ===================================================================
        // grpUv1 — UV1 MK063 (Plate)
        // ===================================================================
        grpUv1.Controls.Add(tlpUv1);
        grpUv1.Dock = System.Windows.Forms.DockStyle.Fill;
        grpUv1.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpUv1.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpUv1.Location = new System.Drawing.Point(19, 19);
        grpUv1.Name = "grpUv1";
        grpUv1.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpUv1.Size = new System.Drawing.Size(742, 344);
        grpUv1.TabIndex = 0;
        grpUv1.TabStop = false;
        grpUv1.Text = "UV1 — MK063 (Plate)";
        //
        // tlpUv1 — 6 cols: dot(36) | badge(140) | label(100) | IP(fill) | colon(20) | port/browse(90)
        //
        tlpUv1.BackColor = System.Drawing.Color.White;
        tlpUv1.ColumnCount = 6;
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tlpUv1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpUv1.Controls.Add(lblUv1Dot, 0, 0);
        tlpUv1.Controls.Add(lblUv1Badge, 1, 0);
        tlpUv1.Controls.Add(btnUv1Edit, 2, 0);
        tlpUv1.Controls.Add(lblUv1IpLabel, 1, 1);
        tlpUv1.Controls.Add(txtUv1Ip, 3, 1);
        tlpUv1.Controls.Add(lblUv1Colon, 4, 1);
        tlpUv1.Controls.Add(txtUv1Port, 5, 1);
        tlpUv1.Controls.Add(lblUv1FolderLabel, 1, 2);
        tlpUv1.Controls.Add(txtUv1Folder, 3, 2);
        tlpUv1.Controls.Add(btnUv1Browse, 5, 2);
        tlpUv1.Controls.Add(lblUv1Status, 1, 3);
        tlpUv1.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUv1.Location = new System.Drawing.Point(16, 36);
        tlpUv1.Name = "tlpUv1";
        tlpUv1.RowCount = 4;
        tlpUv1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv1.Size = new System.Drawing.Size(710, 300);
        tlpUv1.TabIndex = 0;
        //
        // lblUv1Dot
        //
        lblUv1Dot.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Dot.Font = new System.Drawing.Font("Segoe UI", 22F);
        lblUv1Dot.ForeColor = System.Drawing.Color.Gray;
        lblUv1Dot.Location = new System.Drawing.Point(3, 0);
        lblUv1Dot.Name = "lblUv1Dot";
        lblUv1Dot.Size = new System.Drawing.Size(30, 50);
        lblUv1Dot.TabIndex = 0;
        lblUv1Dot.Text = "●";
        lblUv1Dot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblUv1Badge
        //
        lblUv1Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblUv1Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblUv1Badge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblUv1Badge.ForeColor = System.Drawing.Color.White;
        lblUv1Badge.Location = new System.Drawing.Point(39, 7);
        lblUv1Badge.Name = "lblUv1Badge";
        lblUv1Badge.Size = new System.Drawing.Size(120, 36);
        lblUv1Badge.TabIndex = 1;
        lblUv1Badge.Text = "UV-001";
        lblUv1Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnUv1Edit
        //
        btnUv1Edit.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv1Edit.BorderWidth = 2F;
        btnUv1Edit.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Edit.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnUv1Edit.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Edit.Location = new System.Drawing.Point(179, 7);
        btnUv1Edit.Name = "btnUv1Edit";
        btnUv1Edit.Radius = 6;
        btnUv1Edit.Size = new System.Drawing.Size(56, 36);
        btnUv1Edit.TabIndex = 2;
        btnUv1Edit.Text = "Edit";
        btnUv1Edit.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv1IpLabel
        //
        lblUv1IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1IpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv1IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1IpLabel.Location = new System.Drawing.Point(39, 50);
        lblUv1IpLabel.Name = "lblUv1IpLabel";
        tlpUv1.SetColumnSpan(lblUv1IpLabel, 2);
        lblUv1IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv1IpLabel.Size = new System.Drawing.Size(236, 50);
        lblUv1IpLabel.TabIndex = 3;
        lblUv1IpLabel.Text = "IP Address :";
        lblUv1IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv1Ip
        //
        txtUv1Ip.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv1Ip.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv1Ip.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUv1Ip.Location = new System.Drawing.Point(279, 55);
        txtUv1Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Ip.Name = "txtUv1Ip";
        txtUv1Ip.PlaceholderText = "e.g. 192.168.3.100";
        txtUv1Ip.Radius = 4;
        txtUv1Ip.Size = new System.Drawing.Size(318, 40);
        txtUv1Ip.TabIndex = 4;
        //
        // lblUv1Colon
        //
        lblUv1Colon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Colon.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblUv1Colon.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1Colon.Location = new System.Drawing.Point(603, 50);
        lblUv1Colon.Name = "lblUv1Colon";
        lblUv1Colon.Size = new System.Drawing.Size(14, 50);
        lblUv1Colon.TabIndex = 5;
        lblUv1Colon.Text = ":";
        lblUv1Colon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtUv1Port
        //
        txtUv1Port.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv1Port.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv1Port.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUv1Port.Location = new System.Drawing.Point(623, 55);
        txtUv1Port.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Port.Name = "txtUv1Port";
        txtUv1Port.PlaceholderText = "Port";
        txtUv1Port.Radius = 4;
        txtUv1Port.Size = new System.Drawing.Size(84, 40);
        txtUv1Port.TabIndex = 6;
        //
        // lblUv1FolderLabel
        //
        lblUv1FolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1FolderLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv1FolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1FolderLabel.Location = new System.Drawing.Point(39, 100);
        lblUv1FolderLabel.Name = "lblUv1FolderLabel";
        tlpUv1.SetColumnSpan(lblUv1FolderLabel, 2);
        lblUv1FolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv1FolderLabel.Size = new System.Drawing.Size(236, 50);
        lblUv1FolderLabel.TabIndex = 7;
        lblUv1FolderLabel.Text = "UV Software Folder :";
        lblUv1FolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv1Folder
        //
        txtUv1Folder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv1Folder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv1Folder.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUv1Folder.Location = new System.Drawing.Point(279, 105);
        txtUv1Folder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Folder.Name = "txtUv1Folder";
        txtUv1Folder.PlaceholderText = "Select UV software folder...";
        txtUv1Folder.Radius = 4;
        txtUv1Folder.ReadOnly = true;
        tlpUv1.SetColumnSpan(txtUv1Folder, 2);
        txtUv1Folder.Size = new System.Drawing.Size(338, 40);
        txtUv1Folder.TabIndex = 8;
        //
        // btnUv1Browse
        //
        btnUv1Browse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv1Browse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnUv1Browse.BorderWidth = 2F;
        btnUv1Browse.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnUv1Browse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Browse.Location = new System.Drawing.Point(623, 105);
        btnUv1Browse.Name = "btnUv1Browse";
        btnUv1Browse.Radius = 6;
        btnUv1Browse.Size = new System.Drawing.Size(84, 40);
        btnUv1Browse.TabIndex = 9;
        btnUv1Browse.Text = "Browse...";
        btnUv1Browse.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv1Status
        //
        lblUv1Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Status.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv1Status.ForeColor = System.Drawing.Color.Gray;
        lblUv1Status.Location = new System.Drawing.Point(39, 150);
        lblUv1Status.Name = "lblUv1Status";
        tlpUv1.SetColumnSpan(lblUv1Status, 5);
        lblUv1Status.Padding = new System.Windows.Forms.Padding(60, 4, 0, 0);
        lblUv1Status.Size = new System.Drawing.Size(668, 150);
        lblUv1Status.TabIndex = 10;
        lblUv1Status.Text = "";

        // ===================================================================
        // grpUv2 — UV2 MK067 (Shim)
        // ===================================================================
        grpUv2.Controls.Add(tlpUv2);
        grpUv2.Dock = System.Windows.Forms.DockStyle.Fill;
        grpUv2.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpUv2.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpUv2.Location = new System.Drawing.Point(19, 369);
        grpUv2.Name = "grpUv2";
        grpUv2.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpUv2.Size = new System.Drawing.Size(742, 344);
        grpUv2.TabIndex = 1;
        grpUv2.TabStop = false;
        grpUv2.Text = "UV2 — MK067 (Shim)";
        //
        // tlpUv2
        //
        tlpUv2.BackColor = System.Drawing.Color.White;
        tlpUv2.ColumnCount = 6;
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tlpUv2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpUv2.Controls.Add(lblUv2Dot, 0, 0);
        tlpUv2.Controls.Add(lblUv2Badge, 1, 0);
        tlpUv2.Controls.Add(btnUv2Edit, 2, 0);
        tlpUv2.Controls.Add(lblUv2IpLabel, 1, 1);
        tlpUv2.Controls.Add(txtUv2Ip, 3, 1);
        tlpUv2.Controls.Add(lblUv2Colon, 4, 1);
        tlpUv2.Controls.Add(txtUv2Port, 5, 1);
        tlpUv2.Controls.Add(lblUv2FolderLabel, 1, 2);
        tlpUv2.Controls.Add(txtUv2Folder, 3, 2);
        tlpUv2.Controls.Add(btnUv2Browse, 5, 2);
        tlpUv2.Controls.Add(lblUv2Status, 1, 3);
        tlpUv2.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUv2.Location = new System.Drawing.Point(16, 36);
        tlpUv2.Name = "tlpUv2";
        tlpUv2.RowCount = 4;
        tlpUv2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpUv2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2.Size = new System.Drawing.Size(710, 300);
        tlpUv2.TabIndex = 0;
        //
        // lblUv2Dot
        //
        lblUv2Dot.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Dot.Font = new System.Drawing.Font("Segoe UI", 22F);
        lblUv2Dot.ForeColor = System.Drawing.Color.Gray;
        lblUv2Dot.Location = new System.Drawing.Point(3, 0);
        lblUv2Dot.Name = "lblUv2Dot";
        lblUv2Dot.Size = new System.Drawing.Size(30, 50);
        lblUv2Dot.TabIndex = 0;
        lblUv2Dot.Text = "●";
        lblUv2Dot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblUv2Badge
        //
        lblUv2Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblUv2Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblUv2Badge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblUv2Badge.ForeColor = System.Drawing.Color.White;
        lblUv2Badge.Location = new System.Drawing.Point(39, 7);
        lblUv2Badge.Name = "lblUv2Badge";
        lblUv2Badge.Size = new System.Drawing.Size(120, 36);
        lblUv2Badge.TabIndex = 1;
        lblUv2Badge.Text = "UV-002";
        lblUv2Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnUv2Edit
        //
        btnUv2Edit.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv2Edit.BorderWidth = 2F;
        btnUv2Edit.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Edit.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnUv2Edit.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Edit.Location = new System.Drawing.Point(179, 7);
        btnUv2Edit.Name = "btnUv2Edit";
        btnUv2Edit.Radius = 6;
        btnUv2Edit.Size = new System.Drawing.Size(56, 36);
        btnUv2Edit.TabIndex = 2;
        btnUv2Edit.Text = "Edit";
        btnUv2Edit.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv2IpLabel
        //
        lblUv2IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2IpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv2IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2IpLabel.Location = new System.Drawing.Point(39, 50);
        lblUv2IpLabel.Name = "lblUv2IpLabel";
        tlpUv2.SetColumnSpan(lblUv2IpLabel, 2);
        lblUv2IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv2IpLabel.Size = new System.Drawing.Size(236, 50);
        lblUv2IpLabel.TabIndex = 3;
        lblUv2IpLabel.Text = "IP Address :";
        lblUv2IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv2Ip
        //
        txtUv2Ip.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv2Ip.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Ip.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUv2Ip.Location = new System.Drawing.Point(279, 55);
        txtUv2Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Ip.Name = "txtUv2Ip";
        txtUv2Ip.PlaceholderText = "e.g. 192.168.3.101";
        txtUv2Ip.Radius = 4;
        txtUv2Ip.Size = new System.Drawing.Size(318, 40);
        txtUv2Ip.TabIndex = 4;
        //
        // lblUv2Colon
        //
        lblUv2Colon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Colon.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblUv2Colon.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2Colon.Location = new System.Drawing.Point(603, 50);
        lblUv2Colon.Name = "lblUv2Colon";
        lblUv2Colon.Size = new System.Drawing.Size(14, 50);
        lblUv2Colon.TabIndex = 5;
        lblUv2Colon.Text = ":";
        lblUv2Colon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtUv2Port
        //
        txtUv2Port.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv2Port.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Port.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtUv2Port.Location = new System.Drawing.Point(623, 55);
        txtUv2Port.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Port.Name = "txtUv2Port";
        txtUv2Port.PlaceholderText = "Port";
        txtUv2Port.Radius = 4;
        txtUv2Port.Size = new System.Drawing.Size(84, 40);
        txtUv2Port.TabIndex = 6;
        //
        // lblUv2FolderLabel
        //
        lblUv2FolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2FolderLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv2FolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2FolderLabel.Location = new System.Drawing.Point(39, 100);
        lblUv2FolderLabel.Name = "lblUv2FolderLabel";
        tlpUv2.SetColumnSpan(lblUv2FolderLabel, 2);
        lblUv2FolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv2FolderLabel.Size = new System.Drawing.Size(236, 50);
        lblUv2FolderLabel.TabIndex = 7;
        lblUv2FolderLabel.Text = "UV Software Folder :";
        lblUv2FolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv2Folder
        //
        txtUv2Folder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtUv2Folder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Folder.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUv2Folder.Location = new System.Drawing.Point(279, 105);
        txtUv2Folder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Folder.Name = "txtUv2Folder";
        txtUv2Folder.PlaceholderText = "Select UV software folder...";
        txtUv2Folder.Radius = 4;
        txtUv2Folder.ReadOnly = true;
        tlpUv2.SetColumnSpan(txtUv2Folder, 2);
        txtUv2Folder.Size = new System.Drawing.Size(338, 40);
        txtUv2Folder.TabIndex = 8;
        //
        // btnUv2Browse
        //
        btnUv2Browse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv2Browse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnUv2Browse.BorderWidth = 2F;
        btnUv2Browse.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnUv2Browse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Browse.Location = new System.Drawing.Point(623, 105);
        btnUv2Browse.Name = "btnUv2Browse";
        btnUv2Browse.Radius = 6;
        btnUv2Browse.Size = new System.Drawing.Size(84, 40);
        btnUv2Browse.TabIndex = 9;
        btnUv2Browse.Text = "Browse...";
        btnUv2Browse.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv2Status
        //
        lblUv2Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Status.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv2Status.ForeColor = System.Drawing.Color.Gray;
        lblUv2Status.Location = new System.Drawing.Point(39, 150);
        lblUv2Status.Name = "lblUv2Status";
        tlpUv2.SetColumnSpan(lblUv2Status, 5);
        lblUv2Status.Padding = new System.Windows.Forms.Padding(60, 4, 0, 0);
        lblUv2Status.Size = new System.Drawing.Size(668, 150);
        lblUv2Status.TabIndex = 10;
        lblUv2Status.Text = "";

        // ===================================================================
        // flpActions
        // ===================================================================
        flpActions.BackColor = System.Drawing.Color.White;
        flpActions.Controls.Add(btnSave);
        flpActions.Controls.Add(btnCancel);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Location = new System.Drawing.Point(16, 720);
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(748, 68);
        flpActions.TabIndex = 2;
        flpActions.WrapContents = false;
        //
        // btnSave
        //
        btnSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        btnSave.ForeColor = System.Drawing.Color.White;
        btnSave.Location = new System.Drawing.Point(591, 12);
        btnSave.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnSave.Name = "btnSave";
        btnSave.Radius = 8;
        btnSave.Size = new System.Drawing.Size(154, 44);
        btnSave.TabIndex = 0;
        btnSave.Text = "Save";
        btnSave.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.BorderWidth = 2F;
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Location = new System.Drawing.Point(431, 12);
        btnCancel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(154, 44);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // UvPrinterSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "UvPrinterSettingUserControl";
        Size = new System.Drawing.Size(780, 800);
        tlpRoot.ResumeLayout(false);
        grpUv1.ResumeLayout(false);
        tlpUv1.ResumeLayout(false);
        grpUv2.ResumeLayout(false);
        tlpUv2.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;

    private System.Windows.Forms.GroupBox grpUv1;
    private System.Windows.Forms.TableLayoutPanel tlpUv1;
    private System.Windows.Forms.Label lblUv1Dot;
    private System.Windows.Forms.Label lblUv1Badge;
    private AntdUI.Button btnUv1Edit;
    private System.Windows.Forms.Label lblUv1IpLabel;
    private AntdUI.Input txtUv1Ip;
    private System.Windows.Forms.Label lblUv1Colon;
    private AntdUI.Input txtUv1Port;
    private System.Windows.Forms.Label lblUv1FolderLabel;
    private AntdUI.Input txtUv1Folder;
    private AntdUI.Button btnUv1Browse;
    private System.Windows.Forms.Label lblUv1Status;

    private System.Windows.Forms.GroupBox grpUv2;
    private System.Windows.Forms.TableLayoutPanel tlpUv2;
    private System.Windows.Forms.Label lblUv2Dot;
    private System.Windows.Forms.Label lblUv2Badge;
    private AntdUI.Button btnUv2Edit;
    private System.Windows.Forms.Label lblUv2IpLabel;
    private AntdUI.Input txtUv2Ip;
    private System.Windows.Forms.Label lblUv2Colon;
    private AntdUI.Input txtUv2Port;
    private System.Windows.Forms.Label lblUv2FolderLabel;
    private AntdUI.Input txtUv2Folder;
    private AntdUI.Button btnUv2Browse;
    private System.Windows.Forms.Label lblUv2Status;

    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
