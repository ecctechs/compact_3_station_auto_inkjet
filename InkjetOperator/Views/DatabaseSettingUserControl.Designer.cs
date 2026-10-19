namespace InkjetOperator.Views;

partial class DatabaseSettingUserControl
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
        grpDatabase = new System.Windows.Forms.GroupBox();
        tlpForm = new System.Windows.Forms.TableLayoutPanel();
        lblDbPathLabel = new System.Windows.Forms.Label();
        txtDbPath = new AntdUI.Input();
        btnBrowse = new AntdUI.Button();
        lblStatus = new System.Windows.Forms.Label();
        lblClampPathLabel = new System.Windows.Forms.Label();
        txtClampPath = new AntdUI.Input();
        btnBrowseClamp = new AntdUI.Button();
        lblClampStatus = new System.Windows.Forms.Label();
        lblExcelPathLabel = new System.Windows.Forms.Label();
        txtExcelPath = new AntdUI.Input();
        btnBrowseExcel = new AntdUI.Button();
        lblExcelStatus = new System.Windows.Forms.Label();
        chkImportAtStart = new AntdUI.Checkbox();
        flpExcelActions = new System.Windows.Forms.FlowLayoutPanel();
        btnImportNow = new AntdUI.Button();
        btnExportTemplate = new AntdUI.Button();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpDatabase.SuspendLayout();
        tlpForm.SuspendLayout();
        flpExcelActions.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpDatabase, 0, 0);
        tlpRoot.Controls.Add(flpActions, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 515F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));
        tlpRoot.Size = new System.Drawing.Size(975, 650);
        tlpRoot.TabIndex = 0;
        //
        // grpDatabase
        //
        grpDatabase.Controls.Add(tlpForm);
        grpDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
        grpDatabase.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpDatabase.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpDatabase.Name = "grpDatabase";
        grpDatabase.Padding = new System.Windows.Forms.Padding(16, 24, 16, 24);
        grpDatabase.TabIndex = 0;
        grpDatabase.TabStop = false;
        grpDatabase.Text = "Database";
        //
        // tlpForm — 4 cols: label(190) | input(fill) | gap(8) | browse(90)
        //
        tlpForm.BackColor = System.Drawing.Color.White;
        tlpForm.ColumnCount = 4;
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 238F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
        tlpForm.Controls.Add(lblDbPathLabel, 0, 0);
        tlpForm.Controls.Add(txtDbPath, 1, 0);
        tlpForm.Controls.Add(btnBrowse, 3, 0);
        tlpForm.Controls.Add(lblStatus, 1, 1);
        tlpForm.Controls.Add(lblClampPathLabel, 0, 2);
        tlpForm.Controls.Add(txtClampPath, 1, 2);
        tlpForm.Controls.Add(btnBrowseClamp, 3, 2);
        tlpForm.Controls.Add(lblClampStatus, 1, 3);
        tlpForm.Controls.Add(lblExcelPathLabel, 0, 4);
        tlpForm.Controls.Add(txtExcelPath, 1, 4);
        tlpForm.Controls.Add(btnBrowseExcel, 3, 4);
        tlpForm.Controls.Add(lblExcelStatus, 1, 5);
        tlpForm.Controls.Add(chkImportAtStart, 1, 6);
        tlpForm.Controls.Add(flpExcelActions, 1, 7);
        tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpForm.Name = "tlpForm";
        tlpForm.RowCount = 9;
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.TabIndex = 0;
        //
        // lblDbPathLabel
        //
        lblDbPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDbPathLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblDbPathLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDbPathLabel.Name = "lblDbPathLabel";
        lblDbPathLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblDbPathLabel.TabIndex = 0;
        lblDbPathLabel.Text = "Printing Database Path:";
        lblDbPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtDbPath
        //
        txtDbPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtDbPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtDbPath.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtDbPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtDbPath.Name = "txtDbPath";
        txtDbPath.PlaceholderText = "Select PrintData.db3 file...";
        txtDbPath.Radius = 4;
        txtDbPath.ReadOnly = true;
        txtDbPath.TabIndex = 1;
        //
        // btnBrowse
        //
        btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowse.BorderWidth = 2F;
        btnBrowse.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnBrowse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowse.IconSvg = "FolderOpenFilled";
        btnBrowse.IconRatio = 1.2F;
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Radius = 6;
        btnBrowse.Size = new System.Drawing.Size(52, 42);
        btnBrowse.TabIndex = 2;
        btnBrowse.Type = AntdUI.TTypeMini.Default;
        //
        // lblStatus — under the input, aligned with it
        //
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblStatus.ForeColor = System.Drawing.Color.Gray;
        lblStatus.Name = "lblStatus";
        tlpForm.SetColumnSpan(lblStatus, 3);
        tlpForm.SetColumnSpan(lblClampStatus, 3);
        lblStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "";
        //
        // lblClampPathLabel
        //
        lblClampPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblClampPathLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblClampPathLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblClampPathLabel.Name = "lblClampPathLabel";
        lblClampPathLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblClampPathLabel.TabIndex = 4;
        lblClampPathLabel.Text = "Clamp Database Path:";
        lblClampPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtClampPath
        //
        txtClampPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtClampPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtClampPath.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtClampPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtClampPath.Name = "txtClampPath";
        txtClampPath.PlaceholderText = "Select mydatabase.db3 file...";
        txtClampPath.Radius = 4;
        txtClampPath.ReadOnly = true;
        txtClampPath.TabIndex = 5;
        //
        // btnBrowseClamp
        //
        btnBrowseClamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowseClamp.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowseClamp.BorderWidth = 2F;
        btnBrowseClamp.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnBrowseClamp.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowseClamp.IconSvg = "FolderOpenFilled";
        btnBrowseClamp.IconRatio = 1.2F;
        btnBrowseClamp.Name = "btnBrowseClamp";
        btnBrowseClamp.Radius = 6;
        btnBrowseClamp.Size = new System.Drawing.Size(52, 42);
        btnBrowseClamp.TabIndex = 6;
        btnBrowseClamp.Type = AntdUI.TTypeMini.Default;
        //
        // lblClampStatus
        //
        lblClampStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblClampStatus.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblClampStatus.ForeColor = System.Drawing.Color.Gray;
        lblClampStatus.Name = "lblClampStatus";
        lblClampStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblClampStatus.TabIndex = 7;
        lblClampStatus.Text = "";
        //
        // lblExcelPathLabel
        //
        lblExcelPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblExcelPathLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblExcelPathLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblExcelPathLabel.Name = "lblExcelPathLabel";
        lblExcelPathLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblExcelPathLabel.TabIndex = 8;
        lblExcelPathLabel.Text = "Printing Data Excel:";
        lblExcelPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtExcelPath
        //
        txtExcelPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtExcelPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtExcelPath.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtExcelPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtExcelPath.Name = "txtExcelPath";
        txtExcelPath.PlaceholderText = "Select the customer's Excel file (.xlsx)...";
        txtExcelPath.Radius = 4;
        txtExcelPath.ReadOnly = true;
        txtExcelPath.TabIndex = 9;
        //
        // btnBrowseExcel
        //
        btnBrowseExcel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowseExcel.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowseExcel.BorderWidth = 2F;
        btnBrowseExcel.Font = new System.Drawing.Font("Segoe UI", 11F);
        btnBrowseExcel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowseExcel.IconSvg = "FolderOpenFilled";
        btnBrowseExcel.IconRatio = 1.2F;
        btnBrowseExcel.Name = "btnBrowseExcel";
        btnBrowseExcel.Radius = 6;
        btnBrowseExcel.Size = new System.Drawing.Size(52, 42);
        btnBrowseExcel.TabIndex = 10;
        btnBrowseExcel.Type = AntdUI.TTypeMini.Default;
        //
        // lblExcelStatus — แปลงล่าสุดเมื่อไร จากไฟล์ไหน
        //
        lblExcelStatus.AutoEllipsis = true;
        lblExcelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblExcelStatus.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblExcelStatus.ForeColor = System.Drawing.Color.Gray;
        lblExcelStatus.Name = "lblExcelStatus";
        tlpForm.SetColumnSpan(lblExcelStatus, 3);
        lblExcelStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblExcelStatus.TabIndex = 11;
        lblExcelStatus.Text = "";
        //
        // chkImportAtStart — เปิดที่เครื่องเดียว (เครื่อง Scan Barcode)
        //
        chkImportAtStart.Dock = System.Windows.Forms.DockStyle.Fill;
        chkImportAtStart.Font = new System.Drawing.Font("Segoe UI", 14F);
        chkImportAtStart.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        chkImportAtStart.Name = "chkImportAtStart";
        tlpForm.SetColumnSpan(chkImportAtStart, 3);
        chkImportAtStart.TabIndex = 12;
        chkImportAtStart.Text = "Convert Excel to PrintData.db3 every time the program opens";
        //
        // flpExcelActions
        //
        flpExcelActions.BackColor = System.Drawing.Color.White;
        flpExcelActions.Controls.Add(btnImportNow);
        flpExcelActions.Controls.Add(btnExportTemplate);
        flpExcelActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpExcelActions.Margin = new System.Windows.Forms.Padding(0);
        flpExcelActions.Name = "flpExcelActions";
        tlpForm.SetColumnSpan(flpExcelActions, 3);
        flpExcelActions.TabIndex = 13;
        flpExcelActions.WrapContents = false;
        //
        // btnImportNow
        //
        btnImportNow.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnImportNow.BorderWidth = 2F;
        btnImportNow.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnImportNow.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnImportNow.Margin = new System.Windows.Forms.Padding(3, 8, 12, 8);
        btnImportNow.Name = "btnImportNow";
        btnImportNow.Radius = 8;
        btnImportNow.Size = new System.Drawing.Size(220, 46);
        btnImportNow.TabIndex = 0;
        btnImportNow.Text = "Convert Now";
        btnImportNow.Type = AntdUI.TTypeMini.Default;
        //
        // btnExportTemplate
        //
        btnExportTemplate.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnExportTemplate.BorderWidth = 2F;
        btnExportTemplate.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnExportTemplate.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnExportTemplate.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        btnExportTemplate.Name = "btnExportTemplate";
        btnExportTemplate.Radius = 8;
        btnExportTemplate.Size = new System.Drawing.Size(260, 46);
        btnExportTemplate.TabIndex = 1;
        btnExportTemplate.Text = "Create Excel Template";
        btnExportTemplate.Type = AntdUI.TTypeMini.Default;
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
        // DatabaseSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "DatabaseSettingUserControl";
        Size = new System.Drawing.Size(975, 650);
        tlpRoot.ResumeLayout(false);
        grpDatabase.ResumeLayout(false);
        tlpForm.ResumeLayout(false);
        flpExcelActions.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        //
        // tmrAutoCheck - ตรวจสถานะซ้ำเองระหว่างที่เปิดหน้านี้ค้างอยู่
        //
        tmrAutoCheck.Interval = 15000;
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpDatabase;
    private System.Windows.Forms.TableLayoutPanel tlpForm;
    private System.Windows.Forms.Label lblDbPathLabel;
    private AntdUI.Input txtDbPath;
    private AntdUI.Button btnBrowse;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Label lblClampPathLabel;
    private AntdUI.Input txtClampPath;
    private AntdUI.Button btnBrowseClamp;
    private System.Windows.Forms.Label lblClampStatus;
    private System.Windows.Forms.Label lblExcelPathLabel;
    private AntdUI.Input txtExcelPath;
    private AntdUI.Button btnBrowseExcel;
    private System.Windows.Forms.Label lblExcelStatus;
    private AntdUI.Checkbox chkImportAtStart;
    private System.Windows.Forms.FlowLayoutPanel flpExcelActions;
    private AntdUI.Button btnImportNow;
    private AntdUI.Button btnExportTemplate;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
    private System.Windows.Forms.Timer tmrAutoCheck;
}
