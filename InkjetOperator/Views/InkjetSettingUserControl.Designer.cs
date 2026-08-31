namespace InkjetOperator.Views;

partial class InkjetSettingUserControl
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

        grpInkjet = new System.Windows.Forms.GroupBox();
        tlpMk = new System.Windows.Forms.TableLayoutPanel();
        lblMk058Status = new System.Windows.Forms.Label();
        lblMk058Badge = new System.Windows.Forms.Label();
        btnMk058Name = new AntdUI.Button();
        lblMk058IpLabel = new System.Windows.Forms.Label();
        txtMk058Ip = new IpAddressInput();
        lblMk059Status = new System.Windows.Forms.Label();
        lblMk059Badge = new System.Windows.Forms.Label();
        btnMk059Name = new AntdUI.Button();
        lblMk059IpLabel = new System.Windows.Forms.Label();
        txtMk059Ip = new IpAddressInput();

        grpUv = new System.Windows.Forms.GroupBox();
        tlpUv = new System.Windows.Forms.TableLayoutPanel();
        lblUv1Header = new System.Windows.Forms.Label();
        lblUv1Dot = new System.Windows.Forms.Label();
        lblUv1Badge = new System.Windows.Forms.Label();
        btnUv1Edit = new AntdUI.Button();
        lblUv1IpLabel = new System.Windows.Forms.Label();
        txtUv1Ip = new IpAddressInput();
        lblUv1Colon = new System.Windows.Forms.Label();
        txtUv1Port = new AntdUI.Input();
        lblUv1FolderLabel = new System.Windows.Forms.Label();
        txtUv1Folder = new AntdUI.Input();
        btnUv1Browse = new AntdUI.Button();
        lblUv1Status = new System.Windows.Forms.Label();
        lblUv2Header = new System.Windows.Forms.Label();
        lblUv2Dot = new System.Windows.Forms.Label();
        lblUv2Badge = new System.Windows.Forms.Label();
        btnUv2Edit = new AntdUI.Button();
        lblUv2IpLabel = new System.Windows.Forms.Label();
        txtUv2Ip = new IpAddressInput();
        lblUv2Colon = new System.Windows.Forms.Label();
        txtUv2Port = new AntdUI.Input();
        lblUv2FolderLabel = new System.Windows.Forms.Label();
        txtUv2Folder = new AntdUI.Input();
        btnUv2Browse = new AntdUI.Button();
        lblUv2Status = new System.Windows.Forms.Label();

        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnCheckStatus = new AntdUI.Button();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        grpMarkingRef = new System.Windows.Forms.GroupBox();
        tlpMarkingRef = new System.Windows.Forms.TableLayoutPanel();
        lblMarkingRefLabel = new System.Windows.Forms.Label();
        txtMarkingRefFolder = new AntdUI.Input();
        btnMarkingRefBrowse = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpInkjet.SuspendLayout();
        tlpMk.SuspendLayout();
        grpUv.SuspendLayout();
        tlpUv.SuspendLayout();
        flpActions.SuspendLayout();
        grpMarkingRef.SuspendLayout();
        tlpMarkingRef.SuspendLayout();
        SuspendLayout();

        // =================================================================
        // tlpRoot
        // =================================================================
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpInkjet, 0, 0);
        tlpRoot.Controls.Add(grpUv, 0, 1);
        tlpRoot.Controls.Add(grpMarkingRef, 0, 2);
        tlpRoot.Controls.Add(flpActions, 0, 3);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 4;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 118F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(780, 900);
        tlpRoot.TabIndex = 0;

        // =================================================================
        // grpInkjet — MK Inkjet Printer
        //   same 6-col grid as UV so controls align visually
        //   dot(36) | badge(140) | label/edit(100) | input(fill) | :(20) | extra(90)
        // =================================================================
        grpInkjet.Controls.Add(tlpMk);
        grpInkjet.Dock = System.Windows.Forms.DockStyle.Fill;
        grpInkjet.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpInkjet.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpInkjet.Name = "grpInkjet";
        grpInkjet.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpInkjet.TabIndex = 0;
        grpInkjet.TabStop = false;
        grpInkjet.Text = "MK Inkjet Printer";
        //
        // tlpMk — 6 cols (matches UV grid)
        //
        tlpMk.BackColor = System.Drawing.Color.White;
        tlpMk.ColumnCount = 6;
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tlpMk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpMk.Controls.Add(lblMk058Status, 0, 0);
        tlpMk.Controls.Add(lblMk058Badge, 1, 0);
        tlpMk.Controls.Add(btnMk058Name, 2, 0);
        tlpMk.Controls.Add(lblMk058IpLabel, 1, 1);
        tlpMk.Controls.Add(txtMk058Ip, 3, 1);
        tlpMk.Controls.Add(lblMk059Status, 0, 2);
        tlpMk.Controls.Add(lblMk059Badge, 1, 2);
        tlpMk.Controls.Add(btnMk059Name, 2, 2);
        tlpMk.Controls.Add(lblMk059IpLabel, 1, 3);
        tlpMk.Controls.Add(txtMk059Ip, 3, 3);
        tlpMk.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMk.Name = "tlpMk";
        tlpMk.RowCount = 4;
        tlpMk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpMk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpMk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpMk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpMk.TabIndex = 0;
        //
        // lblMk058Status
        //
        lblMk058Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk058Status.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblMk058Status.ForeColor = System.Drawing.Color.Gray;
        lblMk058Status.Name = "lblMk058Status";
        lblMk058Status.TabIndex = 0;
        lblMk058Status.Text = "●";
        lblMk058Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblMk058Badge
        //
        lblMk058Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblMk058Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblMk058Badge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblMk058Badge.ForeColor = System.Drawing.Color.White;
        lblMk058Badge.Name = "lblMk058Badge";
        lblMk058Badge.Size = new System.Drawing.Size(120, 34);
        lblMk058Badge.TabIndex = 1;
        lblMk058Badge.Text = "MK-058";
        lblMk058Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnMk058Name
        //
        btnMk058Name.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnMk058Name.BorderWidth = 2F;
        btnMk058Name.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk058Name.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnMk058Name.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk058Name.Name = "btnMk058Name";
        btnMk058Name.Radius = 6;
        btnMk058Name.Size = new System.Drawing.Size(80, 34);
        btnMk058Name.TabIndex = 2;
        btnMk058Name.Text = "Rename";
        btnMk058Name.Type = AntdUI.TTypeMini.Default;
        //
        // lblMk058IpLabel
        //
        lblMk058IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk058IpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblMk058IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMk058IpLabel.Name = "lblMk058IpLabel";
        tlpMk.SetColumnSpan(lblMk058IpLabel, 2);
        lblMk058IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblMk058IpLabel.TabIndex = 3;
        lblMk058IpLabel.Text = "IP Address :";
        lblMk058IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtMk058Ip — spans cols 3-5 (no port for MK)
        //
        txtMk058Ip.Dock = System.Windows.Forms.DockStyle.Fill;
        txtMk058Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtMk058Ip.Name = "txtMk058Ip";
        tlpMk.SetColumnSpan(txtMk058Ip, 3);
        txtMk058Ip.TabIndex = 4;
        //
        // lblMk059Status
        //
        lblMk059Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk059Status.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblMk059Status.ForeColor = System.Drawing.Color.Gray;
        lblMk059Status.Name = "lblMk059Status";
        lblMk059Status.TabIndex = 5;
        lblMk059Status.Text = "●";
        lblMk059Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblMk059Badge
        //
        lblMk059Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblMk059Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblMk059Badge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblMk059Badge.ForeColor = System.Drawing.Color.White;
        lblMk059Badge.Name = "lblMk059Badge";
        lblMk059Badge.Size = new System.Drawing.Size(120, 34);
        lblMk059Badge.TabIndex = 6;
        lblMk059Badge.Text = "MK-059";
        lblMk059Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnMk059Name
        //
        btnMk059Name.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnMk059Name.BorderWidth = 2F;
        btnMk059Name.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk059Name.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnMk059Name.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk059Name.Name = "btnMk059Name";
        btnMk059Name.Radius = 6;
        btnMk059Name.Size = new System.Drawing.Size(80, 34);
        btnMk059Name.TabIndex = 7;
        btnMk059Name.Text = "Rename";
        btnMk059Name.Type = AntdUI.TTypeMini.Default;
        //
        // lblMk059IpLabel
        //
        lblMk059IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk059IpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblMk059IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMk059IpLabel.Name = "lblMk059IpLabel";
        tlpMk.SetColumnSpan(lblMk059IpLabel, 2);
        lblMk059IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblMk059IpLabel.TabIndex = 8;
        lblMk059IpLabel.Text = "IP Address :";
        lblMk059IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtMk059Ip — spans cols 3-5
        //
        txtMk059Ip.Dock = System.Windows.Forms.DockStyle.Fill;
        txtMk059Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtMk059Ip.Name = "txtMk059Ip";
        tlpMk.SetColumnSpan(txtMk059Ip, 3);
        txtMk059Ip.TabIndex = 9;

        // =================================================================
        // grpUv — UV Printer (UV1 + UV2 in one box)
        //   same 6-col grid
        // =================================================================
        grpUv.Controls.Add(tlpUv);
        grpUv.Dock = System.Windows.Forms.DockStyle.Fill;
        grpUv.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpUv.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpUv.Name = "grpUv";
        grpUv.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpUv.TabIndex = 1;
        grpUv.TabStop = false;
        grpUv.Text = "UV Printer";
        //
        // tlpUv — 6 cols × 10 rows
        //
        tlpUv.BackColor = System.Drawing.Color.White;
        tlpUv.ColumnCount = 6;
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tlpUv.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpUv.Controls.Add(lblUv1Header, 0, 0);
        tlpUv.Controls.Add(lblUv1Dot, 0, 1);
        tlpUv.Controls.Add(lblUv1Badge, 1, 1);
        tlpUv.Controls.Add(btnUv1Edit, 2, 1);
        tlpUv.Controls.Add(lblUv1IpLabel, 1, 2);
        tlpUv.Controls.Add(txtUv1Ip, 3, 2);
        tlpUv.Controls.Add(lblUv1Colon, 4, 2);
        tlpUv.Controls.Add(txtUv1Port, 5, 2);
        tlpUv.Controls.Add(lblUv1FolderLabel, 1, 3);
        tlpUv.Controls.Add(txtUv1Folder, 3, 3);
        tlpUv.Controls.Add(btnUv1Browse, 5, 3);
        tlpUv.Controls.Add(lblUv1Status, 1, 4);
        tlpUv.Controls.Add(lblUv2Header, 0, 5);
        tlpUv.Controls.Add(lblUv2Dot, 0, 6);
        tlpUv.Controls.Add(lblUv2Badge, 1, 6);
        tlpUv.Controls.Add(btnUv2Edit, 2, 6);
        tlpUv.Controls.Add(lblUv2IpLabel, 1, 7);
        tlpUv.Controls.Add(txtUv2Ip, 3, 7);
        tlpUv.Controls.Add(lblUv2Colon, 4, 7);
        tlpUv.Controls.Add(txtUv2Port, 5, 7);
        tlpUv.Controls.Add(lblUv2FolderLabel, 1, 8);
        tlpUv.Controls.Add(txtUv2Folder, 3, 8);
        tlpUv.Controls.Add(btnUv2Browse, 5, 8);
        tlpUv.Controls.Add(lblUv2Status, 1, 9);
        tlpUv.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUv.Name = "tlpUv";
        tlpUv.RowCount = 10;
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv.TabIndex = 0;

        // ── UV1 sub-header ──
        lblUv1Header.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Header.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblUv1Header.ForeColor = System.Drawing.Color.FromArgb(91, 155, 213);
        lblUv1Header.Name = "lblUv1Header";
        tlpUv.SetColumnSpan(lblUv1Header, 6);
        lblUv1Header.TabIndex = 0;
        lblUv1Header.Text = "UV1 — MK063";
        lblUv1Header.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        lblUv1Header.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
        //
        // lblUv1Dot
        //
        lblUv1Dot.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Dot.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblUv1Dot.ForeColor = System.Drawing.Color.Gray;
        lblUv1Dot.Name = "lblUv1Dot";
        lblUv1Dot.TabIndex = 1;
        lblUv1Dot.Text = "●";
        lblUv1Dot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblUv1Badge
        //
        lblUv1Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblUv1Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblUv1Badge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblUv1Badge.ForeColor = System.Drawing.Color.White;
        lblUv1Badge.Name = "lblUv1Badge";
        lblUv1Badge.Size = new System.Drawing.Size(120, 34);
        lblUv1Badge.TabIndex = 2;
        lblUv1Badge.Text = "UV-001";
        lblUv1Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnUv1Edit
        //
        btnUv1Edit.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv1Edit.BorderWidth = 2F;
        btnUv1Edit.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Edit.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnUv1Edit.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Edit.Name = "btnUv1Edit";
        btnUv1Edit.Radius = 6;
        btnUv1Edit.Size = new System.Drawing.Size(80, 34);
        btnUv1Edit.TabIndex = 3;
        btnUv1Edit.Text = "Rename";
        btnUv1Edit.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv1IpLabel
        //
        lblUv1IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1IpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv1IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1IpLabel.Name = "lblUv1IpLabel";
        tlpUv.SetColumnSpan(lblUv1IpLabel, 2);
        lblUv1IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv1IpLabel.TabIndex = 4;
        lblUv1IpLabel.Text = "IP Address :";
        lblUv1IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv1Ip
        //
        txtUv1Ip.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv1Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Ip.Name = "txtUv1Ip";
        txtUv1Ip.TabIndex = 5;
        //
        // lblUv1Colon
        //
        lblUv1Colon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Colon.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblUv1Colon.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1Colon.Name = "lblUv1Colon";
        lblUv1Colon.TabIndex = 6;
        lblUv1Colon.Text = ":";
        lblUv1Colon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtUv1Port
        //
        txtUv1Port.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv1Port.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv1Port.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUv1Port.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Port.Name = "txtUv1Port";
        txtUv1Port.PlaceholderText = "Port";
        txtUv1Port.Radius = 4;
        txtUv1Port.TabIndex = 7;
        //
        // lblUv1FolderLabel
        //
        lblUv1FolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1FolderLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv1FolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv1FolderLabel.Name = "lblUv1FolderLabel";
        tlpUv.SetColumnSpan(lblUv1FolderLabel, 2);
        lblUv1FolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv1FolderLabel.TabIndex = 8;
        lblUv1FolderLabel.Text = "UV Software Folder :";
        lblUv1FolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv1Folder
        //
        txtUv1Folder.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv1Folder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv1Folder.Font = new System.Drawing.Font("Segoe UI", 9F);
        txtUv1Folder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv1Folder.Name = "txtUv1Folder";
        txtUv1Folder.PlaceholderText = "Select UV software folder...";
        txtUv1Folder.Radius = 4;
        txtUv1Folder.ReadOnly = true;
        tlpUv.SetColumnSpan(txtUv1Folder, 2);
        txtUv1Folder.TabIndex = 9;
        //
        // btnUv1Browse
        //
        btnUv1Browse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv1Browse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnUv1Browse.BorderWidth = 2F;
        btnUv1Browse.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnUv1Browse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv1Browse.Name = "btnUv1Browse";
        btnUv1Browse.Radius = 6;
        btnUv1Browse.Size = new System.Drawing.Size(84, 34);
        btnUv1Browse.TabIndex = 10;
        btnUv1Browse.Text = "Browse...";
        btnUv1Browse.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv1Status
        //
        lblUv1Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv1Status.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblUv1Status.ForeColor = System.Drawing.Color.Gray;
        lblUv1Status.Name = "lblUv1Status";
        tlpUv.SetColumnSpan(lblUv1Status, 5);
        lblUv1Status.Padding = new System.Windows.Forms.Padding(60, 2, 0, 0);
        lblUv1Status.TabIndex = 11;
        lblUv1Status.Text = "";

        // ── UV2 sub-header ──
        lblUv2Header.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Header.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblUv2Header.ForeColor = System.Drawing.Color.FromArgb(91, 155, 213);
        lblUv2Header.Name = "lblUv2Header";
        tlpUv.SetColumnSpan(lblUv2Header, 6);
        lblUv2Header.TabIndex = 12;
        lblUv2Header.Text = "UV2 — MK067";
        lblUv2Header.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        lblUv2Header.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
        //
        // lblUv2Dot
        //
        lblUv2Dot.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Dot.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblUv2Dot.ForeColor = System.Drawing.Color.Gray;
        lblUv2Dot.Name = "lblUv2Dot";
        lblUv2Dot.TabIndex = 13;
        lblUv2Dot.Text = "●";
        lblUv2Dot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblUv2Badge
        //
        lblUv2Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblUv2Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblUv2Badge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblUv2Badge.ForeColor = System.Drawing.Color.White;
        lblUv2Badge.Name = "lblUv2Badge";
        lblUv2Badge.Size = new System.Drawing.Size(120, 34);
        lblUv2Badge.TabIndex = 14;
        lblUv2Badge.Text = "UV-002";
        lblUv2Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnUv2Edit
        //
        btnUv2Edit.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv2Edit.BorderWidth = 2F;
        btnUv2Edit.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Edit.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnUv2Edit.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Edit.Name = "btnUv2Edit";
        btnUv2Edit.Radius = 6;
        btnUv2Edit.Size = new System.Drawing.Size(80, 34);
        btnUv2Edit.TabIndex = 15;
        btnUv2Edit.Text = "Rename";
        btnUv2Edit.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv2IpLabel
        //
        lblUv2IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2IpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv2IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2IpLabel.Name = "lblUv2IpLabel";
        tlpUv.SetColumnSpan(lblUv2IpLabel, 2);
        lblUv2IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv2IpLabel.TabIndex = 16;
        lblUv2IpLabel.Text = "IP Address :";
        lblUv2IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv2Ip
        //
        txtUv2Ip.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv2Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Ip.Name = "txtUv2Ip";
        txtUv2Ip.TabIndex = 17;
        //
        // lblUv2Colon
        //
        lblUv2Colon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Colon.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblUv2Colon.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2Colon.Name = "lblUv2Colon";
        lblUv2Colon.TabIndex = 18;
        lblUv2Colon.Text = ":";
        lblUv2Colon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtUv2Port
        //
        txtUv2Port.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv2Port.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Port.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtUv2Port.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Port.Name = "txtUv2Port";
        txtUv2Port.PlaceholderText = "Port";
        txtUv2Port.Radius = 4;
        txtUv2Port.TabIndex = 19;
        //
        // lblUv2FolderLabel
        //
        lblUv2FolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2FolderLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblUv2FolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2FolderLabel.Name = "lblUv2FolderLabel";
        tlpUv.SetColumnSpan(lblUv2FolderLabel, 2);
        lblUv2FolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblUv2FolderLabel.TabIndex = 20;
        lblUv2FolderLabel.Text = "UV Software Folder :";
        lblUv2FolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv2Folder
        //
        txtUv2Folder.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv2Folder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Folder.Font = new System.Drawing.Font("Segoe UI", 9F);
        txtUv2Folder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Folder.Name = "txtUv2Folder";
        txtUv2Folder.PlaceholderText = "Select UV software folder...";
        txtUv2Folder.Radius = 4;
        txtUv2Folder.ReadOnly = true;
        tlpUv.SetColumnSpan(txtUv2Folder, 2);
        txtUv2Folder.TabIndex = 21;
        //
        // btnUv2Browse
        //
        btnUv2Browse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnUv2Browse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnUv2Browse.BorderWidth = 2F;
        btnUv2Browse.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnUv2Browse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnUv2Browse.Name = "btnUv2Browse";
        btnUv2Browse.Radius = 6;
        btnUv2Browse.Size = new System.Drawing.Size(84, 34);
        btnUv2Browse.TabIndex = 22;
        btnUv2Browse.Text = "Browse...";
        btnUv2Browse.Type = AntdUI.TTypeMini.Default;
        //
        // lblUv2Status
        //
        lblUv2Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2Status.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblUv2Status.ForeColor = System.Drawing.Color.Gray;
        lblUv2Status.Name = "lblUv2Status";
        tlpUv.SetColumnSpan(lblUv2Status, 5);
        lblUv2Status.Padding = new System.Windows.Forms.Padding(60, 2, 0, 0);
        lblUv2Status.TabIndex = 23;
        lblUv2Status.Text = "";

        // =================================================================
        // grpMarkingRef
        // =================================================================
        grpMarkingRef.Controls.Add(tlpMarkingRef);
        grpMarkingRef.Dock = System.Windows.Forms.DockStyle.Fill;
        grpMarkingRef.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpMarkingRef.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpMarkingRef.Name = "grpMarkingRef";
        grpMarkingRef.Padding = new System.Windows.Forms.Padding(16, 20, 16, 12);
        grpMarkingRef.TabIndex = 2;
        grpMarkingRef.TabStop = false;
        grpMarkingRef.Text = "Marking Reference Image";
        //
        // tlpMarkingRef
        //
        tlpMarkingRef.BackColor = System.Drawing.Color.White;
        tlpMarkingRef.ColumnCount = 3;
        tlpMarkingRef.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpMarkingRef.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMarkingRef.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
        tlpMarkingRef.Controls.Add(lblMarkingRefLabel, 0, 0);
        tlpMarkingRef.Controls.Add(txtMarkingRefFolder, 1, 0);
        tlpMarkingRef.Controls.Add(btnMarkingRefBrowse, 2, 0);
        tlpMarkingRef.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMarkingRef.Name = "tlpMarkingRef";
        tlpMarkingRef.RowCount = 1;
        tlpMarkingRef.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMarkingRef.TabIndex = 0;
        //
        // lblMarkingRefLabel
        //
        lblMarkingRefLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMarkingRefLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMarkingRefLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMarkingRefLabel.Name = "lblMarkingRefLabel";
        lblMarkingRefLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblMarkingRefLabel.TabIndex = 0;
        lblMarkingRefLabel.Text = "Folder :";
        lblMarkingRefLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtMarkingRefFolder
        //
        txtMarkingRefFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtMarkingRefFolder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtMarkingRefFolder.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtMarkingRefFolder.Margin = new System.Windows.Forms.Padding(3, 10, 3, 10);
        txtMarkingRefFolder.Name = "txtMarkingRefFolder";
        txtMarkingRefFolder.PlaceholderText = @"\\server\share\Marking reference image";
        txtMarkingRefFolder.Radius = 4;
        txtMarkingRefFolder.Size = new System.Drawing.Size(484, 40);
        txtMarkingRefFolder.TabIndex = 1;
        //
        // btnMarkingRefBrowse
        //
        btnMarkingRefBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnMarkingRefBrowse.BorderWidth = 2F;
        btnMarkingRefBrowse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnMarkingRefBrowse.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnMarkingRefBrowse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMarkingRefBrowse.Name = "btnMarkingRefBrowse";
        btnMarkingRefBrowse.Radius = 6;
        btnMarkingRefBrowse.Size = new System.Drawing.Size(110, 44);
        btnMarkingRefBrowse.TabIndex = 2;
        btnMarkingRefBrowse.Text = "Browse";
        btnMarkingRefBrowse.Type = AntdUI.TTypeMini.Default;
        // =================================================================
        // flpActions
        // =================================================================
        flpActions.BackColor = System.Drawing.Color.White;
        flpActions.Controls.Add(btnSave);
        flpActions.Controls.Add(btnCancel);
        flpActions.Controls.Add(btnCheckStatus);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.TabIndex = 2;
        flpActions.WrapContents = false;
        //
        // btnSave
        //
        btnSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        btnSave.ForeColor = System.Drawing.Color.White;
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
        btnCancel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(154, 44);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // btnCheckStatus
        //
        btnCheckStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheckStatus.BorderWidth = 2F;
        btnCheckStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        btnCheckStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheckStatus.Margin = new System.Windows.Forms.Padding(16, 12, 3, 12);
        btnCheckStatus.Name = "btnCheckStatus";
        btnCheckStatus.Radius = 8;
        btnCheckStatus.Size = new System.Drawing.Size(170, 44);
        btnCheckStatus.TabIndex = 2;
        btnCheckStatus.Text = "Check Status";
        btnCheckStatus.Type = AntdUI.TTypeMini.Default;
        //
        // InkjetSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "InkjetSettingUserControl";
        Size = new System.Drawing.Size(780, 900);
        tlpRoot.ResumeLayout(false);
        grpInkjet.ResumeLayout(false);
        tlpMk.ResumeLayout(false);
        grpUv.ResumeLayout(false);
        tlpUv.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        grpMarkingRef.ResumeLayout(false);
        tlpMarkingRef.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;

    private System.Windows.Forms.GroupBox grpInkjet;
    private System.Windows.Forms.TableLayoutPanel tlpMk;
    private System.Windows.Forms.Label lblMk058Status;
    private System.Windows.Forms.Label lblMk058Badge;
    private AntdUI.Button btnMk058Name;
    private System.Windows.Forms.Label lblMk058IpLabel;
    private IpAddressInput txtMk058Ip;
    private System.Windows.Forms.Label lblMk059Status;
    private System.Windows.Forms.Label lblMk059Badge;
    private AntdUI.Button btnMk059Name;
    private System.Windows.Forms.Label lblMk059IpLabel;
    private IpAddressInput txtMk059Ip;

    private System.Windows.Forms.GroupBox grpUv;
    private System.Windows.Forms.TableLayoutPanel tlpUv;
    private System.Windows.Forms.Label lblUv1Header;
    private System.Windows.Forms.Label lblUv1Dot;
    private System.Windows.Forms.Label lblUv1Badge;
    private AntdUI.Button btnUv1Edit;
    private System.Windows.Forms.Label lblUv1IpLabel;
    private IpAddressInput txtUv1Ip;
    private System.Windows.Forms.Label lblUv1Colon;
    private AntdUI.Input txtUv1Port;
    private System.Windows.Forms.Label lblUv1FolderLabel;
    private AntdUI.Input txtUv1Folder;
    private AntdUI.Button btnUv1Browse;
    private System.Windows.Forms.Label lblUv1Status;
    private System.Windows.Forms.Label lblUv2Header;
    private System.Windows.Forms.Label lblUv2Dot;
    private System.Windows.Forms.Label lblUv2Badge;
    private AntdUI.Button btnUv2Edit;
    private System.Windows.Forms.Label lblUv2IpLabel;
    private IpAddressInput txtUv2Ip;
    private System.Windows.Forms.Label lblUv2Colon;
    private AntdUI.Input txtUv2Port;
    private System.Windows.Forms.Label lblUv2FolderLabel;
    private AntdUI.Input txtUv2Folder;
    private AntdUI.Button btnUv2Browse;
    private System.Windows.Forms.Label lblUv2Status;

    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnCheckStatus;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
    private System.Windows.Forms.GroupBox grpMarkingRef;
    private System.Windows.Forms.TableLayoutPanel tlpMarkingRef;
    private System.Windows.Forms.Label lblMarkingRefLabel;
    private AntdUI.Input txtMarkingRefFolder;
    private AntdUI.Button btnMarkingRefBrowse;
}
