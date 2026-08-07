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
        tlpRoot = new System.Windows.Forms.TableLayoutPanel();
        grpDatabase = new System.Windows.Forms.GroupBox();
        tlpForm = new System.Windows.Forms.TableLayoutPanel();
        lblDot = new System.Windows.Forms.Label();
        lblDbPathLabel = new System.Windows.Forms.Label();
        txtDbPath = new AntdUI.Input();
        btnBrowse = new AntdUI.Button();
        lblStatus = new System.Windows.Forms.Label();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        btnCheckStatus = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        grpDatabase.SuspendLayout();
        tlpForm.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(grpDatabase, 0, 0);
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
        // grpDatabase
        //
        grpDatabase.Controls.Add(tlpForm);
        grpDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
        grpDatabase.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        grpDatabase.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        grpDatabase.Name = "grpDatabase";
        grpDatabase.Padding = new System.Windows.Forms.Padding(16, 24, 16, 8);
        grpDatabase.TabIndex = 0;
        grpDatabase.TabStop = false;
        grpDatabase.Text = "Database";
        //
        // tlpForm — 4 cols: dot(36) | label(130) | input(fill) | browse(100)
        //
        tlpForm.BackColor = System.Drawing.Color.White;
        tlpForm.ColumnCount = 4;
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
        tlpForm.Controls.Add(lblDot, 0, 0);
        tlpForm.Controls.Add(lblDbPathLabel, 1, 0);
        tlpForm.Controls.Add(txtDbPath, 2, 0);
        tlpForm.Controls.Add(btnBrowse, 3, 0);
        tlpForm.Controls.Add(lblStatus, 1, 1);
        tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpForm.Name = "tlpForm";
        tlpForm.RowCount = 3;
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
        tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpForm.TabIndex = 0;
        //
        // lblDot
        //
        lblDot.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDot.Font = new System.Drawing.Font("Segoe UI", 20F);
        lblDot.ForeColor = System.Drawing.Color.Gray;
        lblDot.Name = "lblDot";
        lblDot.TabIndex = 0;
        lblDot.Text = "●";
        lblDot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblDbPathLabel
        //
        lblDbPathLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDbPathLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblDbPathLabel.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDbPathLabel.Name = "lblDbPathLabel";
        lblDbPathLabel.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblDbPathLabel.TabIndex = 1;
        lblDbPathLabel.Text = "Database Path :";
        lblDbPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        // txtDbPath
        //
        txtDbPath.Dock = System.Windows.Forms.DockStyle.Fill;
        txtDbPath.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtDbPath.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtDbPath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
        txtDbPath.Name = "txtDbPath";
        txtDbPath.PlaceholderText = "Select PrintData.db3 file...";
        txtDbPath.Radius = 4;
        txtDbPath.ReadOnly = true;
        txtDbPath.TabIndex = 2;
        //
        // btnBrowse
        //
        btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnBrowse.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnBrowse.BorderWidth = 2F;
        btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnBrowse.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Radius = 6;
        btnBrowse.Size = new System.Drawing.Size(90, 34);
        btnBrowse.TabIndex = 3;
        btnBrowse.Text = "Browse...";
        btnBrowse.Type = AntdUI.TTypeMini.Default;
        //
        // lblStatus
        //
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatus.ForeColor = System.Drawing.Color.Gray;
        lblStatus.Name = "lblStatus";
        tlpForm.SetColumnSpan(lblStatus, 3);
        lblStatus.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
        lblStatus.TabIndex = 4;
        lblStatus.Text = "";
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
        // DatabaseSettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpRoot);
        Name = "DatabaseSettingUserControl";
        Size = new System.Drawing.Size(780, 800);
        tlpRoot.ResumeLayout(false);
        grpDatabase.ResumeLayout(false);
        tlpForm.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.GroupBox grpDatabase;
    private System.Windows.Forms.TableLayoutPanel tlpForm;
    private System.Windows.Forms.Label lblDot;
    private System.Windows.Forms.Label lblDbPathLabel;
    private AntdUI.Input txtDbPath;
    private AntdUI.Button btnBrowse;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnSave;
    private AntdUI.Button btnCancel;
    private AntdUI.Button btnCheckStatus;
}
