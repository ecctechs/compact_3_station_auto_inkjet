namespace InkjetOperator.Views;

partial class PlcSettingUserControl
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
        lblPlcStatus = new System.Windows.Forms.Label();
        lblPlcBadge = new System.Windows.Forms.Label();
        btnPlcName = new AntdUI.Button();
        lblIpLabel = new System.Windows.Forms.Label();
        txtPlc001Ip = new AntdUI.Input();
        lblPortLabel = new System.Windows.Forms.Label();
        txtPlc001Port = new AntdUI.Input();
        grpRegisterMap = new System.Windows.Forms.GroupBox();
        tlpMap = new System.Windows.Forms.TableLayoutPanel();
        tlpMapHeader = new System.Windows.Forms.TableLayoutPanel();
        lblMapHint = new System.Windows.Forms.Label();
        btnAddRow = new AntdUI.Button();
        tblPlcMap = new AntdUI.Table();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        btnCheckStatus = new AntdUI.Button();
        tlpRoot.SuspendLayout();
        grpConnection.SuspendLayout();
        tlpConn.SuspendLayout();
        grpRegisterMap.SuspendLayout();
        tlpMap.SuspendLayout();
        tlpMapHeader.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpConnection, 0, 0);
        tlpRoot.Controls.Add(grpRegisterMap, 0, 1);
        tlpRoot.Controls.Add(flpActions, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
        tlpRoot.Size = new System.Drawing.Size(960, 820);
        tlpRoot.TabIndex = 0;
        //
        // grpConnection
        //
        grpConnection.Controls.Add(tlpConn);
        grpConnection.Dock = System.Windows.Forms.DockStyle.Fill;
        grpConnection.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpConnection.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpConnection.Name = "grpConnection";
        grpConnection.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "PLC Connection";
        //
        // tlpConn — 6 cols matching template
        //   dot(36) | badge(140) | label/edit(100) | input(fill) | :(20) | extra(90)
        //
        tlpConn.BackColor = System.Drawing.Color.White;
        tlpConn.ColumnCount = 6;
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tlpConn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
        tlpConn.Controls.Add(lblPlcStatus, 0, 0);
        tlpConn.Controls.Add(lblPlcBadge, 1, 0);
        tlpConn.Controls.Add(btnPlcName, 2, 0);
        tlpConn.Controls.Add(lblIpLabel, 1, 1);
        tlpConn.Controls.Add(txtPlc001Ip, 3, 1);
        tlpConn.Controls.Add(lblPortLabel, 1, 2);
        tlpConn.Controls.Add(txtPlc001Port, 3, 2);
        tlpConn.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpConn.Name = "tlpConn";
        tlpConn.RowCount = 4;
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpConn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpConn.TabIndex = 0;
        //
        // lblPlcStatus
        //
        lblPlcStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPlcStatus.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblPlcStatus.ForeColor = System.Drawing.Color.Gray;
        lblPlcStatus.Name = "lblPlcStatus";
        lblPlcStatus.TabIndex = 0;
        lblPlcStatus.Text = "●";
        lblPlcStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblPlcBadge
        //
        lblPlcBadge.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblPlcBadge.BackColor = System.Drawing.Color.FromArgb(33, 33, 33);
        lblPlcBadge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblPlcBadge.ForeColor = System.Drawing.Color.White;
        lblPlcBadge.Name = "lblPlcBadge";
        lblPlcBadge.Size = new System.Drawing.Size(120, 34);
        lblPlcBadge.TabIndex = 1;
        lblPlcBadge.Text = "PLC-001";
        lblPlcBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnPlcName
        //
        btnPlcName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnPlcName.BorderWidth = 2F;
        btnPlcName.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPlcName.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnPlcName.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnPlcName.Name = "btnPlcName";
        btnPlcName.Radius = 6;
        btnPlcName.Size = new System.Drawing.Size(50, 34);
        btnPlcName.TabIndex = 2;
        btnPlcName.Text = "Edit";
        btnPlcName.Type = AntdUI.TTypeMini.Default;
        //
        // lblIpLabel — spans col1+col2
        //
        lblIpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblIpLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblIpLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblIpLabel.Name = "lblIpLabel";
        tlpConn.SetColumnSpan(lblIpLabel, 2);
        lblIpLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblIpLabel.TabIndex = 3;
        lblIpLabel.Text = "IP Address :";
        lblIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPlc001Ip — spans col3+col4+col5
        //
        txtPlc001Ip.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPlc001Ip.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPlc001Ip.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtPlc001Ip.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPlc001Ip.Name = "txtPlc001Ip";
        txtPlc001Ip.PlaceholderText = "e.g. 192.168.1.10";
        txtPlc001Ip.Radius = 4;
        tlpConn.SetColumnSpan(txtPlc001Ip, 3);
        txtPlc001Ip.TabIndex = 4;
        //
        // lblPortLabel — spans col1+col2
        //
        lblPortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPortLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblPortLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPortLabel.Name = "lblPortLabel";
        tlpConn.SetColumnSpan(lblPortLabel, 2);
        lblPortLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblPortLabel.TabIndex = 5;
        lblPortLabel.Text = "Port :";
        lblPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtPlc001Port
        //
        txtPlc001Port.Dock = System.Windows.Forms.DockStyle.Fill;
        txtPlc001Port.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPlc001Port.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtPlc001Port.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtPlc001Port.Name = "txtPlc001Port";
        txtPlc001Port.PlaceholderText = "502";
        txtPlc001Port.Radius = 4;
        txtPlc001Port.TabIndex = 6;
        //
        // grpRegisterMap
        //
        grpRegisterMap.Controls.Add(tlpMap);
        grpRegisterMap.Dock = System.Windows.Forms.DockStyle.Fill;
        grpRegisterMap.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpRegisterMap.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpRegisterMap.Name = "grpRegisterMap";
        grpRegisterMap.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpRegisterMap.TabIndex = 1;
        grpRegisterMap.TabStop = false;
        grpRegisterMap.Text = "Register Map";
        //
        // tlpMap
        //
        tlpMap.BackColor = System.Drawing.Color.White;
        tlpMap.ColumnCount = 1;
        tlpMap.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMap.Controls.Add(tlpMapHeader, 0, 0);
        tlpMap.Controls.Add(tblPlcMap, 0, 1);
        tlpMap.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMap.Name = "tlpMap";
        tlpMap.RowCount = 2;
        tlpMap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
        tlpMap.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMap.TabIndex = 0;
        //
        // tlpMapHeader
        //
        tlpMapHeader.BackColor = System.Drawing.Color.White;
        tlpMapHeader.ColumnCount = 2;
        tlpMapHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMapHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpMapHeader.Controls.Add(lblMapHint, 0, 0);
        tlpMapHeader.Controls.Add(btnAddRow, 1, 0);
        tlpMapHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMapHeader.Margin = new System.Windows.Forms.Padding(0);
        tlpMapHeader.Name = "tlpMapHeader";
        tlpMapHeader.RowCount = 1;
        tlpMapHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMapHeader.TabIndex = 0;
        //
        // lblMapHint
        //
        lblMapHint.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMapHint.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblMapHint.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblMapHint.Name = "lblMapHint";
        lblMapHint.TabIndex = 0;
        lblMapHint.Text = "Address ranges must not overlap; List names must be unique.";
        lblMapHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnAddRow
        //
        btnAddRow.Anchor = System.Windows.Forms.AnchorStyles.Right;
        btnAddRow.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnAddRow.ForeColor = System.Drawing.Color.White;
        btnAddRow.Name = "btnAddRow";
        btnAddRow.Radius = 8;
        btnAddRow.Size = new System.Drawing.Size(134, 42);
        btnAddRow.TabIndex = 1;
        btnAddRow.Text = "+ Add Row";
        btnAddRow.Type = AntdUI.TTypeMini.Primary;
        //
        // tblPlcMap
        //
        tblPlcMap.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        tblPlcMap.Bordered = true;
        tblPlcMap.ColumnBack = System.Drawing.Color.FromArgb(240, 245, 251);
        tblPlcMap.ColumnFont = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        tblPlcMap.ColumnFore = System.Drawing.Color.FromArgb(36, 71, 101);
        tblPlcMap.Dock = System.Windows.Forms.DockStyle.Fill;
        tblPlcMap.EditMode = AntdUI.TEditMode.Click;
        tblPlcMap.EmptyText = "No register maps";
        tblPlcMap.Font = new System.Drawing.Font("Segoe UI", 11F);
        tblPlcMap.Margin = new System.Windows.Forms.Padding(0);
        tblPlcMap.Name = "tblPlcMap";
        tblPlcMap.Radius = 8;
        tblPlcMap.RowHeight = 46;
        tblPlcMap.TabIndex = 1;
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
        // PlcSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "PlcSettingUserControl";
        Size = new System.Drawing.Size(960, 820);
        tlpRoot.ResumeLayout(false);
        grpConnection.ResumeLayout(false);
        tlpConn.ResumeLayout(false);
        grpRegisterMap.ResumeLayout(false);
        tlpMap.ResumeLayout(false);
        tlpMapHeader.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.TableLayoutPanel tlpConn;
    private System.Windows.Forms.Label lblPlcStatus;
    private System.Windows.Forms.Label lblPlcBadge;
    private AntdUI.Button btnPlcName;
    private System.Windows.Forms.Label lblIpLabel;
    private AntdUI.Input txtPlc001Ip;
    private System.Windows.Forms.Label lblPortLabel;
    private AntdUI.Input txtPlc001Port;
    private AntdUI.Button btnCheckStatus;
    private System.Windows.Forms.GroupBox grpRegisterMap;
    private System.Windows.Forms.TableLayoutPanel tlpMap;
    private System.Windows.Forms.TableLayoutPanel tlpMapHeader;
    private System.Windows.Forms.Label lblMapHint;
    private AntdUI.Button btnAddRow;
    private AntdUI.Table tblPlcMap;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
