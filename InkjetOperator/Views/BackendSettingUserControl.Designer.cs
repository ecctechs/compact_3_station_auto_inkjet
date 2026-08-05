namespace InkjetOperator.Views;

partial class BackendSettingUserControl
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
        grpBackend = new System.Windows.Forms.GroupBox();
        tlpDevice = new System.Windows.Forms.TableLayoutPanel();

        lblPcStatus = new AntdUI.Label();
        lblPcBadge = new System.Windows.Forms.Label();
        btnPcName = new AntdUI.Button();
        lblPcIpLabel = new AntdUI.Label();
        txtPcIp = new AntdUI.Input();
        lblApiUrlLabel = new AntdUI.Label();
        lblApiUrlValue = new AntdUI.Label();

        btnCheckStatus = new AntdUI.Button();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpBackend.SuspendLayout();
        tlpDevice.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpBackend, 0, 0);
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
        // grpBackend
        //
        grpBackend.Controls.Add(tlpDevice);
        grpBackend.Dock = System.Windows.Forms.DockStyle.Fill;
        grpBackend.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        grpBackend.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpBackend.Location = new System.Drawing.Point(19, 19);
        grpBackend.Name = "grpBackend";
        grpBackend.Padding = new System.Windows.Forms.Padding(20, 28, 20, 12);
        grpBackend.Size = new System.Drawing.Size(742, 698);
        grpBackend.TabIndex = 0;
        grpBackend.TabStop = false;
        grpBackend.Text = "Backend";
        //
        // tlpDevice
        //
        tlpDevice.BackColor = System.Drawing.Color.White;
        tlpDevice.ColumnCount = 4;
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevice.Controls.Add(lblPcStatus, 0, 0);
        tlpDevice.Controls.Add(lblPcBadge, 1, 0);
        tlpDevice.Controls.Add(btnPcName, 2, 0);
        tlpDevice.Controls.Add(lblPcIpLabel, 1, 1);
        tlpDevice.Controls.Add(txtPcIp, 3, 1);
        tlpDevice.Controls.Add(lblApiUrlLabel, 1, 2);
        tlpDevice.Controls.Add(lblApiUrlValue, 3, 2);
        tlpDevice.Controls.Add(btnCheckStatus, 0, 4);
        tlpDevice.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDevice.Location = new System.Drawing.Point(20, 42);
        tlpDevice.Name = "tlpDevice";
        tlpDevice.RowCount = 5;
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpDevice.Size = new System.Drawing.Size(702, 644);
        tlpDevice.TabIndex = 0;
        //
        // lblPcStatus
        //
        lblPcStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPcStatus.Font = new System.Drawing.Font("Segoe UI", 22F);
        lblPcStatus.ForeColor = System.Drawing.Color.Gray;
        lblPcStatus.Location = new System.Drawing.Point(3, 0);
        lblPcStatus.Name = "lblPcStatus";
        lblPcStatus.Size = new System.Drawing.Size(30, 50);
        lblPcStatus.TabIndex = 0;
        lblPcStatus.Text = "●";
        lblPcStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblPcBadge
        //
        lblPcBadge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblPcBadge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblPcBadge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        lblPcBadge.ForeColor = System.Drawing.Color.White;
        lblPcBadge.Location = new System.Drawing.Point(39, 7);
        lblPcBadge.Name = "lblPcBadge";
        lblPcBadge.Size = new System.Drawing.Size(120, 36);
        lblPcBadge.TabIndex = 1;
        lblPcBadge.Text = "PC";
        lblPcBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnPcName
        //
        btnPcName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnPcName.BorderWidth = 2F;
        btnPcName.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPcName.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnPcName.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPcName.Location = new System.Drawing.Point(179, 7);
        btnPcName.Name = "btnPcName";
        btnPcName.Radius = 6;
        btnPcName.Size = new System.Drawing.Size(56, 36);
        btnPcName.TabIndex = 2;
        btnPcName.Text = "Edit";
        btnPcName.Type = AntdUI.TTypeMini.Default;
        //
        // lblPcIpLabel
        //
        lblPcIpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPcIpLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPcIpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPcIpLabel.Location = new System.Drawing.Point(39, 50);
        lblPcIpLabel.Name = "lblPcIpLabel";
        tlpDevice.SetColumnSpan(lblPcIpLabel, 2);
        lblPcIpLabel.Size = new System.Drawing.Size(186, 50);
        lblPcIpLabel.TabIndex = 3;
        lblPcIpLabel.Text = "IP Address :";
        lblPcIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblPcIpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        //
        // txtPcIp
        //
        txtPcIp.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtPcIp.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPcIp.Font = new System.Drawing.Font("Segoe UI", 11F);
        txtPcIp.Location = new System.Drawing.Point(229, 55);
        txtPcIp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPcIp.Name = "txtPcIp";
        txtPcIp.PlaceholderText = "e.g. 127.0.0.1";
        txtPcIp.Radius = 4;
        txtPcIp.Size = new System.Drawing.Size(470, 40);
        txtPcIp.TabIndex = 4;
        //
        // lblApiUrlLabel
        //
        lblApiUrlLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblApiUrlLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblApiUrlLabel.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblApiUrlLabel.Location = new System.Drawing.Point(39, 100);
        lblApiUrlLabel.Name = "lblApiUrlLabel";
        tlpDevice.SetColumnSpan(lblApiUrlLabel, 2);
        lblApiUrlLabel.Size = new System.Drawing.Size(186, 40);
        lblApiUrlLabel.TabIndex = 5;
        lblApiUrlLabel.Text = "API URL :";
        lblApiUrlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        lblApiUrlLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        //
        // lblApiUrlValue
        //
        lblApiUrlValue.Dock = System.Windows.Forms.DockStyle.Fill;
        lblApiUrlValue.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblApiUrlValue.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblApiUrlValue.Location = new System.Drawing.Point(229, 100);
        lblApiUrlValue.Name = "lblApiUrlValue";
        lblApiUrlValue.Size = new System.Drawing.Size(470, 40);
        lblApiUrlValue.TabIndex = 6;
        lblApiUrlValue.Text = "http://127.0.0.1:3000";
        lblApiUrlValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
        tlpDevice.SetColumnSpan(btnCheckStatus, 2);
        btnCheckStatus.Size = new System.Drawing.Size(170, 44);
        btnCheckStatus.TabIndex = 7;
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
        // BackendSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "BackendSettingUserControl";
        Size = new System.Drawing.Size(780, 800);
        tlpRoot.ResumeLayout(false);
        grpBackend.ResumeLayout(false);
        tlpDevice.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpBackend;
    private System.Windows.Forms.TableLayoutPanel tlpDevice;
    private AntdUI.Label lblPcStatus;
    private System.Windows.Forms.Label lblPcBadge;
    private AntdUI.Button btnPcName;
    private AntdUI.Label lblPcIpLabel;
    private AntdUI.Input txtPcIp;
    private AntdUI.Label lblApiUrlLabel;
    private AntdUI.Label lblApiUrlValue;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
