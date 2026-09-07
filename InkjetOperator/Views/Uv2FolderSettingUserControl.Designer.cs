namespace InkjetOperator.Views;

partial class Uv2FolderSettingUserControl
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
        grpUv2Folder = new System.Windows.Forms.GroupBox();
        tlpForm = new System.Windows.Forms.TableLayoutPanel();
        lblUv2FolderLabel = new System.Windows.Forms.Label();
        txtUv2Folder = new AntdUI.Input();
        btnBrowse = new AntdUI.Button();
        lblStatus = new System.Windows.Forms.Label();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpUv2Folder.SuspendLayout();
        tlpForm.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpUv2Folder, 0, 0);
        tlpRoot.Controls.Add(flpActions, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 193F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));
        tlpRoot.Size = new System.Drawing.Size(975, 585);
        tlpRoot.TabIndex = 0;
        //
        // grpUv2Folder
        //
        grpUv2Folder.Controls.Add(tlpForm);
        grpUv2Folder.Dock = System.Windows.Forms.DockStyle.Fill;
        grpUv2Folder.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        grpUv2Folder.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpUv2Folder.Name = "grpUv2Folder";
        grpUv2Folder.Padding = new System.Windows.Forms.Padding(16, 24, 16, 24);
        grpUv2Folder.TabIndex = 0;
        grpUv2Folder.TabStop = false;
        grpUv2Folder.Text = "UV2 Program Folder";
        //
        // tlpForm — 4 cols: label(238) | input(fill) | gap(10) | browse(70)
        //
        tlpForm.BackColor = System.Drawing.Color.White;
        tlpForm.ColumnCount = 4;
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 238F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
        tlpForm.Controls.Add(lblUv2FolderLabel, 0, 0);
        tlpForm.Controls.Add(txtUv2Folder, 1, 0);
        tlpForm.Controls.Add(btnBrowse, 3, 0);
        tlpForm.Controls.Add(lblStatus, 1, 1);
        tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpForm.Name = "tlpForm";
        tlpForm.RowCount = 3;
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.TabIndex = 0;
        //
        // lblUv2FolderLabel
        //
        lblUv2FolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2FolderLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblUv2FolderLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblUv2FolderLabel.Name = "lblUv2FolderLabel";
        lblUv2FolderLabel.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
        lblUv2FolderLabel.TabIndex = 0;
        lblUv2FolderLabel.Text = "UV2 Folder:";
        lblUv2FolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtUv2Folder
        //
        // พิมพ์เองได้ ไม่ล็อกเป็น ReadOnly เหมือนหน้าอื่น เพราะค่าที่ต้องใส่ที่นี่
        // เป็น UNC path ของ share บนเครื่อง ST1 ซึ่งวางจาก clipboard ง่ายกว่าไล่หาใน dialog
        txtUv2Folder.Dock = System.Windows.Forms.DockStyle.Fill;
        txtUv2Folder.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtUv2Folder.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        txtUv2Folder.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtUv2Folder.Name = "txtUv2Folder";
        txtUv2Folder.PlaceholderText = @"\\ST1-PC\UV2";
        txtUv2Folder.Radius = 4;
        txtUv2Folder.TabIndex = 1;
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
        // lblStatus — ใต้ช่องกรอก ตรงแนวเดียวกับช่อง
        //
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblStatus.ForeColor = System.Drawing.Color.Gray;
        lblStatus.Name = "lblStatus";
        tlpForm.SetColumnSpan(lblStatus, 3);
        lblStatus.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "";
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
        // Uv2FolderSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "Uv2FolderSettingUserControl";
        Size = new System.Drawing.Size(975, 585);
        tlpRoot.ResumeLayout(false);
        grpUv2Folder.ResumeLayout(false);
        tlpForm.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpUv2Folder;
    private System.Windows.Forms.TableLayoutPanel tlpForm;
    private System.Windows.Forms.Label lblUv2FolderLabel;
    private AntdUI.Input txtUv2Folder;
    private AntdUI.Button btnBrowse;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
}
