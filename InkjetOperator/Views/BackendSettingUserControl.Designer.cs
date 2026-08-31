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

        lblPcStatus = new System.Windows.Forms.Label();
        lblPcBadge = new System.Windows.Forms.Label();
        btnPcName = new AntdUI.Button();
        lblPcIpLabel = new System.Windows.Forms.Label();
        txtPcIp = new IpAddressInput();

        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        btnCheckStatus = new AntdUI.Button();

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
        tlpRoot.Controls.Add(flpActions, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 205F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));
        tlpRoot.Size = new System.Drawing.Size(975, 505);
        tlpRoot.TabIndex = 0;
        //
        // grpBackend
        //
        grpBackend.Controls.Add(tlpDevice);
        grpBackend.Dock = System.Windows.Forms.DockStyle.Fill;
        grpBackend.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpBackend.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpBackend.Name = "grpBackend";
        grpBackend.Padding = new System.Windows.Forms.Padding(16, 24, 16, 24);
        grpBackend.TabIndex = 0;
        grpBackend.TabStop = false;
        grpBackend.Text = "Backend";
        //
        // tlpDevice — 6 cols matching InkjetSetting grid
        //   dot(36) | badge(140) | label/edit(100) | input(fill) | :(20) | extra(90)
        //
        tlpDevice.BackColor = System.Drawing.Color.White;
        tlpDevice.ColumnCount = 6;
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
        tlpDevice.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
        tlpDevice.Controls.Add(lblPcStatus, 0, 0);
        tlpDevice.Controls.Add(lblPcBadge, 1, 0);
        tlpDevice.Controls.Add(btnPcName, 2, 0);
        tlpDevice.Controls.Add(lblPcIpLabel, 1, 1);
        tlpDevice.Controls.Add(txtPcIp, 3, 1);
        tlpDevice.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDevice.Name = "tlpDevice";
        tlpDevice.RowCount = 3;
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpDevice.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDevice.TabIndex = 0;
        //
        // lblPcStatus
        //
        lblPcStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPcStatus.Font = new System.Drawing.Font("Segoe UI", 25F);
        lblPcStatus.ForeColor = System.Drawing.Color.Gray;
        lblPcStatus.Name = "lblPcStatus";
        lblPcStatus.TabIndex = 0;
        lblPcStatus.Text = "●";
        lblPcStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblPcBadge
        //
        lblPcBadge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblPcBadge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblPcBadge.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
        lblPcBadge.ForeColor = System.Drawing.Color.White;
        lblPcBadge.Name = "lblPcBadge";
        lblPcBadge.Size = new System.Drawing.Size(150, 42);
        lblPcBadge.TabIndex = 1;
        lblPcBadge.Text = "PC";
        lblPcBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnPcName
        //
        btnPcName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnPcName.BorderWidth = 2F;
        btnPcName.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPcName.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnPcName.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPcName.Name = "btnPcName";
        btnPcName.Radius = 6;
        btnPcName.Size = new System.Drawing.Size(100, 42);
        btnPcName.TabIndex = 2;
        btnPcName.Text = "Rename";
        btnPcName.Type = AntdUI.TTypeMini.Default;
        //
        // lblPcIpLabel — spans col1+col2
        //
        lblPcIpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPcIpLabel.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblPcIpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPcIpLabel.Name = "lblPcIpLabel";
        tlpDevice.SetColumnSpan(lblPcIpLabel, 2);
        lblPcIpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblPcIpLabel.TabIndex = 3;
        lblPcIpLabel.Text = "IP Address:";
        lblPcIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPcIp — spans col3+col4+col5
        //
        txtPcIp.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPcIp.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPcIp.Name = "txtPcIp";
        tlpDevice.SetColumnSpan(txtPcIp, 3);
        txtPcIp.TabIndex = 4;
        //
        // flpActions
        //
        flpActions.BackColor = System.Drawing.Color.White;
        flpActions.Controls.Add(btnSave);
        flpActions.Controls.Add(btnCancel);
        flpActions.Controls.Add(btnCheckStatus);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.TabIndex = 1;
        flpActions.WrapContents = false;
        //
        // btnSave
        //
        btnSave.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnSave.ForeColor = System.Drawing.Color.White;
        btnSave.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnSave.Name = "btnSave";
        btnSave.Radius = 8;
        btnSave.Size = new System.Drawing.Size(192, 55);
        btnSave.TabIndex = 0;
        btnSave.Text = "Save";
        btnSave.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.BorderWidth = 2F;
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(192, 55);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // btnCheckStatus
        //
        btnCheckStatus.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnCheckStatus.BorderWidth = 2F;
        btnCheckStatus.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnCheckStatus.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCheckStatus.Margin = new System.Windows.Forms.Padding(16, 12, 3, 12);
        btnCheckStatus.Name = "btnCheckStatus";
        btnCheckStatus.Radius = 8;
        btnCheckStatus.Size = new System.Drawing.Size(212, 55);
        btnCheckStatus.TabIndex = 2;
        btnCheckStatus.Text = "Check Status";
        btnCheckStatus.Type = AntdUI.TTypeMini.Default;
        //
        // BackendSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "BackendSettingUserControl";
        Size = new System.Drawing.Size(975, 505);
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
    private System.Windows.Forms.Label lblPcStatus;
    private System.Windows.Forms.Label lblPcBadge;
    private AntdUI.Button btnPcName;
    private System.Windows.Forms.Label lblPcIpLabel;
    private IpAddressInput txtPcIp;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
    private AntdUI.Button btnCheckStatus;
}
