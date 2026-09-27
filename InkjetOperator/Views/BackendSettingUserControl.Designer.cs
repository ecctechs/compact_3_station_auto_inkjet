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
        components = new System.ComponentModel.Container();
        tmrAutoCheck = new System.Windows.Forms.Timer(components);
        tlpRoot = new System.Windows.Forms.TableLayoutPanel();
        grpBackend = new System.Windows.Forms.GroupBox();
        tlpDevice = new System.Windows.Forms.TableLayoutPanel();

        lblPcStatus = new System.Windows.Forms.Label();
        lblPcBadge = new System.Windows.Forms.Label();
        btnPcName = new AntdUI.Button();
        lblPcIpLabel = new System.Windows.Forms.Label();
        txtPcIp = new IpAddressInput();

        grpDev = new System.Windows.Forms.GroupBox();
        tlpDev = new System.Windows.Forms.TableLayoutPanel();
        lblBackendPathLabel = new System.Windows.Forms.Label();
        txtBackendPath = new AntdUI.Input();
        btnBrowseBackend = new AntdUI.Button();
        lblBackendPathStatus = new System.Windows.Forms.Label();

        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        btnCheckStatus = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpBackend.SuspendLayout();
        tlpDevice.SuspendLayout();
        grpDev.SuspendLayout();
        tlpDev.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpBackend, 0, 0);
        tlpRoot.Controls.Add(grpDev, 0, 1);
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
        // grpDev — โฟลเดอร์ backend สำหรับโหมดทดสอบเท่านั้น
        //
        // อยู่ในแถวที่เดิมเป็นที่ว่างของ tlpRoot และ Dock=Top ไม่ใช่ Fill
        // ซ่อนแล้วหน้าจึงกลับไปเหมือนเดิมทุกประการ โดยไม่ต้องไปแก้ความสูงของแถว
        // จากโค้ด (ค่าที่เขียนทับจากโค้ดไม่ถูกสเกลตาม DPI ของจอ)
        //
        grpDev.Controls.Add(tlpDev);
        grpDev.Dock = System.Windows.Forms.DockStyle.Top;
        grpDev.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpDev.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpDev.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
        grpDev.Name = "grpDev";
        grpDev.Padding = new System.Windows.Forms.Padding(16, 20, 16, 16);
        grpDev.Size = new System.Drawing.Size(943, 186);
        grpDev.TabIndex = 1;
        grpDev.TabStop = false;
        grpDev.Text = "Backend Folder";
        //
        // tlpDev — คอลัมน์ชุดเดียวกับ tlpDevice ช่องกรอกจึงเริ่มตรงกับช่อง IP ด้านบน
        //
        tlpDev.BackColor = System.Drawing.Color.White;
        tlpDev.ColumnCount = 6;
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
        tlpDev.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
        tlpDev.Controls.Add(lblBackendPathLabel, 1, 0);
        tlpDev.Controls.Add(txtBackendPath, 3, 0);
        tlpDev.Controls.Add(btnBrowseBackend, 5, 0);
        tlpDev.Controls.Add(lblBackendPathStatus, 3, 1);
        tlpDev.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDev.Name = "tlpDev";
        tlpDev.RowCount = 3;
        tlpDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpDev.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDev.TabIndex = 0;
        //
        // lblBackendPathLabel — spans col1+col2
        //
        lblBackendPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblBackendPathLabel.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblBackendPathLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblBackendPathLabel.Name = "lblBackendPathLabel";
        tlpDev.SetColumnSpan(lblBackendPathLabel, 2);
        lblBackendPathLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblBackendPathLabel.TabIndex = 0;
        lblBackendPathLabel.Text = "Backend Folder:";
        lblBackendPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtBackendPath — spans col3+col4
        //
        txtBackendPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtBackendPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtBackendPath.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtBackendPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtBackendPath.Name = "txtBackendPath";
        txtBackendPath.PlaceholderText = "Select the folder that contains index.js...";
        txtBackendPath.Radius = 4;
        txtBackendPath.ReadOnly = true;
        tlpDev.SetColumnSpan(txtBackendPath, 2);
        txtBackendPath.TabIndex = 1;
        //
        // btnBrowseBackend
        //
        btnBrowseBackend.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowseBackend.BorderWidth = 2F;
        btnBrowseBackend.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowseBackend.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnBrowseBackend.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowseBackend.IconRatio = 1.2F;
        btnBrowseBackend.IconSvg = "FolderOpenFilled";
        btnBrowseBackend.Name = "btnBrowseBackend";
        btnBrowseBackend.Radius = 6;
        btnBrowseBackend.Size = new System.Drawing.Size(52, 42);
        btnBrowseBackend.TabIndex = 2;
        btnBrowseBackend.Type = AntdUI.TTypeMini.Default;
        //
        // lblBackendPathStatus — under the input, aligned with it
        //
        lblBackendPathStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblBackendPathStatus.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblBackendPathStatus.ForeColor = System.Drawing.Color.Gray;
        lblBackendPathStatus.Name = "lblBackendPathStatus";
        tlpDev.SetColumnSpan(lblBackendPathStatus, 3);
        lblBackendPathStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblBackendPathStatus.TabIndex = 3;
        lblBackendPathStatus.Text = "";
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
        grpDev.ResumeLayout(false);
        tlpDev.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        //
        // tmrAutoCheck - ตรวจสถานะซ้ำเองระหว่างที่เปิดหน้านี้ค้างอยู่
        //
        tmrAutoCheck.Interval = 15000;
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
    private System.Windows.Forms.GroupBox grpDev;
    private System.Windows.Forms.TableLayoutPanel tlpDev;
    private System.Windows.Forms.Label lblBackendPathLabel;
    private AntdUI.Input txtBackendPath;
    private AntdUI.Button btnBrowseBackend;
    private System.Windows.Forms.Label lblBackendPathStatus;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.Timer tmrAutoCheck;
}
