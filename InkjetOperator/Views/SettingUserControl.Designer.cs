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
        tlpSettingRoot = new TableLayoutPanel();
        pnlSidebar = new AntdUI.Panel();
        tlpSidebar = new TableLayoutPanel();
        btnDatabaseSetting = new AntdUI.Button();
        btnDbPathSetting = new AntdUI.Button();
        btnDB3Setting = new AntdUI.Button();
        btnPLCSetting = new AntdUI.Button();
        btnClampSetting = new AntdUI.Button();
        pnlContent = new AntdUI.Panel();
        pnlContentArea = new Panel();
        tlpSettingRoot.SuspendLayout();
        pnlSidebar.SuspendLayout();
        tlpSidebar.SuspendLayout();
        pnlContent.SuspendLayout();
        SuspendLayout();
        // 
        // tlpSettingRoot
        // 
        tlpSettingRoot.BackColor = Color.FromArgb(91, 155, 213);
        tlpSettingRoot.ColumnCount = 2;
        tlpSettingRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F));
        tlpSettingRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpSettingRoot.Controls.Add(pnlSidebar, 0, 0);
        tlpSettingRoot.Controls.Add(pnlContent, 1, 0);
        tlpSettingRoot.Dock = DockStyle.Fill;
        tlpSettingRoot.Location = new Point(0, 0);
        tlpSettingRoot.Margin = new Padding(4);
        tlpSettingRoot.Name = "tlpSettingRoot";
        tlpSettingRoot.Padding = new Padding(48);
        tlpSettingRoot.RowCount = 1;
        tlpSettingRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpSettingRoot.Size = new Size(1650, 1290);
        tlpSettingRoot.TabIndex = 0;
        // 
        // pnlSidebar
        // 
        pnlSidebar.Back = Color.FromArgb(220, 233, 245);
        pnlSidebar.BorderColor = Color.FromArgb(36, 71, 101);
        pnlSidebar.BorderWidth = 2F;
        pnlSidebar.Controls.Add(tlpSidebar);
        pnlSidebar.Dock = DockStyle.Fill;
        pnlSidebar.Location = new Point(52, 52);
        pnlSidebar.Margin = new Padding(4, 4, 12, 4);
        pnlSidebar.Name = "pnlSidebar";
        pnlSidebar.Radius = 12;
        pnlSidebar.Size = new Size(404, 1186);
        pnlSidebar.TabIndex = 0;
        // 
        // tlpSidebar
        // 
        tlpSidebar.BackColor = Color.Transparent;
        tlpSidebar.ColumnCount = 1;
        tlpSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpSidebar.Controls.Add(btnDatabaseSetting, 0, 0);
        tlpSidebar.Controls.Add(btnDbPathSetting, 0, 1);
        tlpSidebar.Controls.Add(btnDB3Setting, 0, 2);
        tlpSidebar.Controls.Add(btnPLCSetting, 0, 3);
        tlpSidebar.Controls.Add(btnClampSetting, 0, 4);
        tlpSidebar.Dock = DockStyle.Top;
        tlpSidebar.Location = new Point(3, 3);
        tlpSidebar.Margin = new Padding(0);
        tlpSidebar.Name = "tlpSidebar";
        tlpSidebar.RowCount = 5;
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tlpSidebar.Size = new Size(398, 420);
        tlpSidebar.TabIndex = 0;
        // 
        // btnDatabaseSetting
        // 
        btnDatabaseSetting.Dock = DockStyle.Fill;
        btnDatabaseSetting.Font = new Font("Segoe UI", 12F);
        btnDatabaseSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDatabaseSetting.Location = new Point(0, 0);
        btnDatabaseSetting.Margin = new Padding(0);
        btnDatabaseSetting.Name = "btnDatabaseSetting";
        btnDatabaseSetting.Radius = 0;
        btnDatabaseSetting.Size = new Size(398, 84);
        btnDatabaseSetting.TabIndex = 0;
        btnDatabaseSetting.Text = "Printer Setting";
        //
        // btnDbPathSetting
        //
        btnDbPathSetting.Dock = DockStyle.Fill;
        btnDbPathSetting.Font = new Font("Segoe UI", 12F);
        btnDbPathSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDbPathSetting.Location = new Point(0, 84);
        btnDbPathSetting.Margin = new Padding(0);
        btnDbPathSetting.Name = "btnDbPathSetting";
        btnDbPathSetting.Radius = 0;
        btnDbPathSetting.Size = new Size(398, 84);
        btnDbPathSetting.TabIndex = 1;
        btnDbPathSetting.Text = "Database Setting";
        //
        // btnDB3Setting
        // 
        btnDB3Setting.Dock = DockStyle.Fill;
        btnDB3Setting.Font = new Font("Segoe UI", 12F);
        btnDB3Setting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDB3Setting.Location = new Point(0, 168);
        btnDB3Setting.Margin = new Padding(0);
        btnDB3Setting.Name = "btnDB3Setting";
        btnDB3Setting.Radius = 0;
        btnDB3Setting.Size = new Size(398, 84);
        btnDB3Setting.TabIndex = 2;
        btnDB3Setting.Text = "Backend DB Setting";
        // 
        // btnPLCSetting
        // 
        btnPLCSetting.Dock = DockStyle.Fill;
        btnPLCSetting.Font = new Font("Segoe UI", 12F);
        btnPLCSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnPLCSetting.Location = new Point(0, 252);
        btnPLCSetting.Margin = new Padding(0);
        btnPLCSetting.Name = "btnPLCSetting";
        btnPLCSetting.Radius = 0;
        btnPLCSetting.Size = new Size(398, 84);
        btnPLCSetting.TabIndex = 3;
        btnPLCSetting.Text = "PLC Setting";
        //
        // btnClampSetting
        //
        btnClampSetting.Dock = DockStyle.Fill;
        btnClampSetting.Font = new Font("Segoe UI", 12F);
        btnClampSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnClampSetting.Location = new Point(0, 336);
        btnClampSetting.Margin = new Padding(0);
        btnClampSetting.Name = "btnClampSetting";
        btnClampSetting.Radius = 0;
        btnClampSetting.Size = new Size(398, 84);
        btnClampSetting.TabIndex = 4;
        btnClampSetting.Text = "Clamp Setting";
        // 
        // pnlContent
        // 
        pnlContent.Back = Color.White;
        pnlContent.BorderColor = Color.FromArgb(36, 71, 101);
        pnlContent.BorderWidth = 2F;
        pnlContent.Controls.Add(pnlContentArea);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Location = new Point(472, 52);
        pnlContent.Margin = new Padding(4);
        pnlContent.Name = "pnlContent";
        pnlContent.Padding = new Padding(12);
        pnlContent.Radius = 12;
        pnlContent.Size = new Size(1126, 1186);
        pnlContent.TabIndex = 1;
        // 
        // pnlContentArea
        // 
        pnlContentArea.AutoScroll = true;
        pnlContentArea.BackColor = Color.White;
        pnlContentArea.Dock = DockStyle.Fill;
        pnlContentArea.Location = new Point(15, 15);
        pnlContentArea.Margin = new Padding(4);
        pnlContentArea.Name = "pnlContentArea";
        pnlContentArea.Size = new Size(1096, 1156);
        pnlContentArea.TabIndex = 0;
        // 
        // SettingUserControl
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpSettingRoot);
        Margin = new Padding(4);
        MinimumSize = new Size(1230, 1020);
        Name = "SettingUserControl";
        Size = new Size(1650, 1290);
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
    private AntdUI.Button btnDbPathSetting;
    private AntdUI.Button btnDB3Setting;
    private AntdUI.Button btnPLCSetting;
    private AntdUI.Button btnClampSetting;
    private AntdUI.Panel pnlContent;
    private System.Windows.Forms.Panel pnlContentArea;
}
