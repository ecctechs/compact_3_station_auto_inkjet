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
        tlpDevices = new System.Windows.Forms.TableLayoutPanel();

        lblMk058Status = new AntdUI.Label();
        lblMk058Badge = new System.Windows.Forms.Label();
        btnMk058Name = new AntdUI.Button();
        lblMk058IpLabel = new AntdUI.Label();
        txtMk058Ip = new AntdUI.Input();

        lblMk059Status = new AntdUI.Label();
        lblMk059Badge = new System.Windows.Forms.Label();
        btnMk059Name = new AntdUI.Button();
        lblMk059IpLabel = new AntdUI.Label();
        txtMk059Ip = new AntdUI.Input();

        btnCheckStatus = new AntdUI.Button();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpInkjet.SuspendLayout();
        tlpDevices.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpInkjet, 0, 0);
        tlpRoot.Controls.Add(flpActions, 0, 1);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 2;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(780, 800);
        tlpRoot.TabIndex = 0;
        //
        // grpInkjet
        //
        grpInkjet.Controls.Add(tlpDevices);
        grpInkjet.Dock = System.Windows.Forms.DockStyle.Fill;
        grpInkjet.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        grpInkjet.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpInkjet.Location = new System.Drawing.Point(19, 19);
        grpInkjet.Name = "grpInkjet";
        grpInkjet.Padding = new System.Windows.Forms.Padding(20, 28, 20, 12);
        grpInkjet.Size = new System.Drawing.Size(742, 698);
        grpInkjet.TabIndex = 0;
        grpInkjet.TabStop = false;
        grpInkjet.Text = "MK Inkjet Printer";
        //
        // tlpDevices
        //
        tlpDevices.BackColor = System.Drawing.Color.White;
        tlpDevices.ColumnCount = 4;
        tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevices.Controls.Add(lblMk058Status, 0, 0);
        tlpDevices.Controls.Add(lblMk058Badge, 1, 0);
        tlpDevices.Controls.Add(btnMk058Name, 2, 0);
        tlpDevices.Controls.Add(lblMk058IpLabel, 1, 1);
        tlpDevices.Controls.Add(txtMk058Ip, 3, 1);
        tlpDevices.Controls.Add(lblMk059Status, 0, 3);
        tlpDevices.Controls.Add(lblMk059Badge, 1, 3);
        tlpDevices.Controls.Add(btnMk059Name, 2, 3);
        tlpDevices.Controls.Add(lblMk059IpLabel, 1, 4);
        tlpDevices.Controls.Add(txtMk059Ip, 3, 4);
        tlpDevices.Controls.Add(btnCheckStatus, 0, 6);
        tlpDevices.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDevices.Location = new System.Drawing.Point(20, 42);
        tlpDevices.Name = "tlpDevices";
        tlpDevices.RowCount = 7;
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpDevices.Size = new System.Drawing.Size(702, 644);
        tlpDevices.TabIndex = 0;
        //
        // lblMk058Status
        //
        lblMk058Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk058Status.Font = new System.Drawing.Font("Segoe UI", 22F);
        lblMk058Status.ForeColor = System.Drawing.Color.Gray;
        lblMk058Status.Location = new System.Drawing.Point(3, 0);
        lblMk058Status.Name = "lblMk058Status";
        lblMk058Status.Size = new System.Drawing.Size(30, 50);
        lblMk058Status.TabIndex = 0;
        lblMk058Status.Text = "●";
        lblMk058Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblMk058Badge
        //
        lblMk058Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblMk058Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblMk058Badge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblMk058Badge.ForeColor = System.Drawing.Color.White;
        lblMk058Badge.Location = new System.Drawing.Point(39, 7);
        lblMk058Badge.Name = "lblMk058Badge";
        lblMk058Badge.Size = new System.Drawing.Size(120, 36);
        lblMk058Badge.TabIndex = 1;
        lblMk058Badge.Text = "MK-058";
        lblMk058Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnMk058Name
        //
        btnMk058Name.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnMk058Name.BorderWidth = 2F;
        btnMk058Name.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk058Name.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnMk058Name.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk058Name.Location = new System.Drawing.Point(179, 7);
        btnMk058Name.Name = "btnMk058Name";
        btnMk058Name.Radius = 6;
        btnMk058Name.Size = new System.Drawing.Size(56, 36);
        btnMk058Name.TabIndex = 2;
        btnMk058Name.Text = "Edit";
        btnMk058Name.Type = AntdUI.TTypeMini.Default;
        //
        // lblMk058IpLabel
        //
        lblMk058IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk058IpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMk058IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMk058IpLabel.Location = new System.Drawing.Point(39, 50);
        lblMk058IpLabel.Name = "lblMk058IpLabel";
        tlpDevices.SetColumnSpan(lblMk058IpLabel, 2);
        lblMk058IpLabel.Size = new System.Drawing.Size(186, 50);
        lblMk058IpLabel.TabIndex = 3;
        lblMk058IpLabel.Text = "IP Address :";
        lblMk058IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblMk058IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        //
        // txtMk058Ip
        //
        txtMk058Ip.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtMk058Ip.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtMk058Ip.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtMk058Ip.Location = new System.Drawing.Point(229, 55);
        txtMk058Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtMk058Ip.Name = "txtMk058Ip";
        txtMk058Ip.PlaceholderText = "e.g. 192.168.3.77";
        txtMk058Ip.Radius = 4;
        txtMk058Ip.Size = new System.Drawing.Size(470, 40);
        txtMk058Ip.TabIndex = 4;
        //
        // lblMk059Status
        //
        lblMk059Status.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk059Status.Font = new System.Drawing.Font("Segoe UI", 22F);
        lblMk059Status.ForeColor = System.Drawing.Color.Gray;
        lblMk059Status.Location = new System.Drawing.Point(3, 124);
        lblMk059Status.Name = "lblMk059Status";
        lblMk059Status.Size = new System.Drawing.Size(30, 50);
        lblMk059Status.TabIndex = 5;
        lblMk059Status.Text = "●";
        lblMk059Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblMk059Badge
        //
        lblMk059Badge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblMk059Badge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblMk059Badge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblMk059Badge.ForeColor = System.Drawing.Color.White;
        lblMk059Badge.Location = new System.Drawing.Point(39, 131);
        lblMk059Badge.Name = "lblMk059Badge";
        lblMk059Badge.Size = new System.Drawing.Size(120, 36);
        lblMk059Badge.TabIndex = 6;
        lblMk059Badge.Text = "MK-059";
        lblMk059Badge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnMk059Name
        //
        btnMk059Name.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnMk059Name.BorderWidth = 2F;
        btnMk059Name.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk059Name.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnMk059Name.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMk059Name.Location = new System.Drawing.Point(179, 131);
        btnMk059Name.Name = "btnMk059Name";
        btnMk059Name.Radius = 6;
        btnMk059Name.Size = new System.Drawing.Size(56, 36);
        btnMk059Name.TabIndex = 7;
        btnMk059Name.Text = "Edit";
        btnMk059Name.Type = AntdUI.TTypeMini.Default;
        //
        // lblMk059IpLabel
        //
        lblMk059IpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMk059IpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMk059IpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMk059IpLabel.Location = new System.Drawing.Point(39, 174);
        lblMk059IpLabel.Name = "lblMk059IpLabel";
        tlpDevices.SetColumnSpan(lblMk059IpLabel, 2);
        lblMk059IpLabel.Size = new System.Drawing.Size(186, 50);
        lblMk059IpLabel.TabIndex = 8;
        lblMk059IpLabel.Text = "IP Address :";
        lblMk059IpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblMk059IpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        //
        // txtMk059Ip
        //
        txtMk059Ip.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtMk059Ip.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtMk059Ip.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtMk059Ip.Location = new System.Drawing.Point(229, 179);
        txtMk059Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtMk059Ip.Name = "txtMk059Ip";
        txtMk059Ip.PlaceholderText = "e.g. 192.168.3.78";
        txtMk059Ip.Radius = 4;
        txtMk059Ip.Size = new System.Drawing.Size(470, 40);
        txtMk059Ip.TabIndex = 9;
        //
        // btnCheckStatus
        //
        btnCheckStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
        btnCheckStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheckStatus.BorderWidth = 2F;
        btnCheckStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        btnCheckStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheckStatus.Location = new System.Drawing.Point(3, 594);
        btnCheckStatus.Name = "btnCheckStatus";
        btnCheckStatus.Radius = 8;
        tlpDevices.SetColumnSpan(btnCheckStatus, 2);
        btnCheckStatus.Size = new System.Drawing.Size(170, 44);
        btnCheckStatus.TabIndex = 10;
        btnCheckStatus.Text = "Check Status";
        btnCheckStatus.Type = AntdUI.TTypeMini.Default;
        //
        // flpActions
        //
        flpActions.BackColor = System.Drawing.Color.White;
        flpActions.Controls.Add(btnSave);
        flpActions.Controls.Add(btnCancel);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Location = new System.Drawing.Point(16, 720);
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(748, 68);
        flpActions.TabIndex = 1;
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
        // InkjetSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "InkjetSettingUserControl";
        Size = new System.Drawing.Size(780, 800);
        tlpRoot.ResumeLayout(false);
        grpInkjet.ResumeLayout(false);
        tlpDevices.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpInkjet;
    private System.Windows.Forms.TableLayoutPanel tlpDevices;
    private AntdUI.Label lblMk058Status;
    private System.Windows.Forms.Label lblMk058Badge;
    private AntdUI.Button btnMk058Name;
    private AntdUI.Label lblMk058IpLabel;
    private AntdUI.Input txtMk058Ip;
    private AntdUI.Label lblMk059Status;
    private System.Windows.Forms.Label lblMk059Badge;
    private AntdUI.Button btnMk059Name;
    private AntdUI.Label lblMk059IpLabel;
    private AntdUI.Input txtMk059Ip;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
