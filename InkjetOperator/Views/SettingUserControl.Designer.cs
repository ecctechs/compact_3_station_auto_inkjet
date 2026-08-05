namespace InkjetOperator.Views;

partial class SettingUserControl
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tlpSettingRoot = new System.Windows.Forms.TableLayoutPanel();
        pnlSidebar = new AntdUI.Panel();
        tlpSidebar = new System.Windows.Forms.TableLayoutPanel();
        btnDatabaseSetting = new AntdUI.Button();
        btnDB3Setting = new AntdUI.Button();
        btnPLCSetting = new AntdUI.Button();
        btnUVPrinterSetting = new AntdUI.Button();
        pnlContent = new AntdUI.Panel();
        pnlContentArea = new System.Windows.Forms.Panel();
        tlpSettingRoot.SuspendLayout();
        pnlSidebar.SuspendLayout();
        tlpSidebar.SuspendLayout();
        pnlContent.SuspendLayout();
        SuspendLayout();
        //
        // tlpSettingRoot
        //
        tlpSettingRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpSettingRoot.ColumnCount = 2;
        tlpSettingRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
        tlpSettingRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSettingRoot.Controls.Add(pnlSidebar, 0, 0);
        tlpSettingRoot.Controls.Add(pnlContent, 1, 0);
        tlpSettingRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpSettingRoot.Location = new System.Drawing.Point(0, 0);
        tlpSettingRoot.Name = "tlpSettingRoot";
        tlpSettingRoot.Padding = new System.Windows.Forms.Padding(32);
        tlpSettingRoot.RowCount = 1;
        tlpSettingRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSettingRoot.Size = new System.Drawing.Size(1100, 860);
        tlpSettingRoot.TabIndex = 0;
        //
        // pnlSidebar
        //
        pnlSidebar.Back = System.Drawing.Color.FromArgb(220, 233, 245);
        pnlSidebar.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlSidebar.BorderWidth = 2F;
        pnlSidebar.Controls.Add(tlpSidebar);
        pnlSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlSidebar.Location = new System.Drawing.Point(19, 19);
        pnlSidebar.Margin = new System.Windows.Forms.Padding(3, 3, 8, 3);
        pnlSidebar.Name = "pnlSidebar";
        pnlSidebar.Padding = new System.Windows.Forms.Padding(0);
        pnlSidebar.Radius = 12;
        pnlSidebar.Size = new System.Drawing.Size(253, 822);
        pnlSidebar.TabIndex = 0;
        //
        // tlpSidebar
        //
        tlpSidebar.BackColor = System.Drawing.Color.Transparent;
        tlpSidebar.ColumnCount = 1;
        tlpSidebar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSidebar.Controls.Add(btnDatabaseSetting, 0, 0);
        tlpSidebar.Controls.Add(btnDB3Setting, 0, 1);
        tlpSidebar.Controls.Add(btnPLCSetting, 0, 2);
        tlpSidebar.Controls.Add(btnUVPrinterSetting, 0, 3);
        tlpSidebar.Dock = System.Windows.Forms.DockStyle.Top;
        tlpSidebar.Location = new System.Drawing.Point(0, 0);
        tlpSidebar.Margin = new System.Windows.Forms.Padding(0);
        tlpSidebar.Name = "tlpSidebar";
        tlpSidebar.RowCount = 4;
        tlpSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpSidebar.Size = new System.Drawing.Size(253, 224);
        tlpSidebar.TabIndex = 0;
        //
        // btnDatabaseSetting
        //
        btnDatabaseSetting.Dock = System.Windows.Forms.DockStyle.Fill;
        btnDatabaseSetting.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnDatabaseSetting.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnDatabaseSetting.Location = new System.Drawing.Point(0, 0);
        btnDatabaseSetting.Margin = new System.Windows.Forms.Padding(0);
        btnDatabaseSetting.Name = "btnDatabaseSetting";
        btnDatabaseSetting.Radius = 0;
        btnDatabaseSetting.Size = new System.Drawing.Size(253, 56);
        btnDatabaseSetting.TabIndex = 0;
        btnDatabaseSetting.Text = "Database Setting";
        btnDatabaseSetting.Type = AntdUI.TTypeMini.Default;
        //
        // btnDB3Setting
        //
        btnDB3Setting.Dock = System.Windows.Forms.DockStyle.Fill;
        btnDB3Setting.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnDB3Setting.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnDB3Setting.Location = new System.Drawing.Point(0, 56);
        btnDB3Setting.Margin = new System.Windows.Forms.Padding(0);
        btnDB3Setting.Name = "btnDB3Setting";
        btnDB3Setting.Radius = 0;
        btnDB3Setting.Size = new System.Drawing.Size(253, 56);
        btnDB3Setting.TabIndex = 1;
        btnDB3Setting.Text = "DB3 Setting";
        btnDB3Setting.Type = AntdUI.TTypeMini.Default;
        //
        // btnPLCSetting
        //
        btnPLCSetting.Dock = System.Windows.Forms.DockStyle.Fill;
        btnPLCSetting.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnPLCSetting.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnPLCSetting.Location = new System.Drawing.Point(0, 112);
        btnPLCSetting.Margin = new System.Windows.Forms.Padding(0);
        btnPLCSetting.Name = "btnPLCSetting";
        btnPLCSetting.Radius = 0;
        btnPLCSetting.Size = new System.Drawing.Size(253, 56);
        btnPLCSetting.TabIndex = 2;
        btnPLCSetting.Text = "PLC Setting";
        btnPLCSetting.Type = AntdUI.TTypeMini.Default;
        //
        // btnUVPrinterSetting
        //
        btnUVPrinterSetting.Dock = System.Windows.Forms.DockStyle.Fill;
        btnUVPrinterSetting.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnUVPrinterSetting.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnUVPrinterSetting.Location = new System.Drawing.Point(0, 168);
        btnUVPrinterSetting.Margin = new System.Windows.Forms.Padding(0);
        btnUVPrinterSetting.Name = "btnUVPrinterSetting";
        btnUVPrinterSetting.Radius = 0;
        btnUVPrinterSetting.Size = new System.Drawing.Size(253, 56);
        btnUVPrinterSetting.TabIndex = 3;
        btnUVPrinterSetting.Text = "UV Printer Setting";
        btnUVPrinterSetting.Type = AntdUI.TTypeMini.Default;
        //
        // pnlContent
        //
        pnlContent.Back = System.Drawing.Color.White;
        pnlContent.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlContent.BorderWidth = 2F;
        pnlContent.Controls.Add(pnlContentArea);
        pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlContent.Location = new System.Drawing.Point(283, 19);
        pnlContent.Margin = new System.Windows.Forms.Padding(3);
        pnlContent.Name = "pnlContent";
        pnlContent.Padding = new System.Windows.Forms.Padding(8);
        pnlContent.Radius = 12;
        pnlContent.Size = new System.Drawing.Size(798, 822);
        pnlContent.TabIndex = 1;
        //
        // pnlContentArea
        //
        pnlContentArea.BackColor = System.Drawing.Color.White;
        pnlContentArea.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlContentArea.Location = new System.Drawing.Point(8, 8);
        pnlContentArea.Name = "pnlContentArea";
        pnlContentArea.Size = new System.Drawing.Size(782, 806);
        pnlContentArea.TabIndex = 0;
        //
        // SettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpSettingRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "SettingUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpSettingRoot.ResumeLayout(false);
        pnlSidebar.ResumeLayout(false);
        tlpSidebar.ResumeLayout(false);
        pnlContent.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpSettingRoot;
    private AntdUI.Panel pnlSidebar;
    private System.Windows.Forms.TableLayoutPanel tlpSidebar;
    private AntdUI.Button btnDatabaseSetting;
    private AntdUI.Button btnDB3Setting;
    private AntdUI.Button btnPLCSetting;
    private AntdUI.Button btnUVPrinterSetting;
    private AntdUI.Panel pnlContent;
    private System.Windows.Forms.Panel pnlContentArea;
}
