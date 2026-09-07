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
        btnUvTest = new AntdUI.Button();
        btnUv2Folder = new AntdUI.Button();
        pnlContent = new AntdUI.Panel();
        pnlContentArea = new BufferedPanel();
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
        tlpSettingRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        tlpSettingRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpSettingRoot.Controls.Add(pnlSidebar, 0, 0);
        tlpSettingRoot.Controls.Add(pnlContent, 1, 0);
        tlpSettingRoot.Dock = DockStyle.Fill;
        tlpSettingRoot.Location = new Point(0, 0);
        tlpSettingRoot.Margin = new Padding(3);
        tlpSettingRoot.Name = "tlpSettingRoot";
        tlpSettingRoot.Padding = new Padding(16);
        tlpSettingRoot.RowCount = 1;
        tlpSettingRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpSettingRoot.Size = new Size(1100, 860);
        tlpSettingRoot.TabIndex = 0;
        // 
        // pnlSidebar
        // 
        pnlSidebar.Back = Color.FromArgb(220, 233, 245);
        pnlSidebar.BorderColor = Color.FromArgb(36, 71, 101);
        pnlSidebar.BorderWidth = 2F;
        pnlSidebar.Controls.Add(tlpSidebar);
        pnlSidebar.Dock = DockStyle.Fill;
        pnlSidebar.Location = new Point(35, 35);
        pnlSidebar.Margin = new Padding(0, 0, 8, 0);
        pnlSidebar.Name = "pnlSidebar";
        pnlSidebar.Radius = 12;
        pnlSidebar.Size = new Size(269, 791);
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
        tlpSidebar.Controls.Add(btnUvTest, 0, 5);
        tlpSidebar.Controls.Add(btnUv2Folder, 0, 6);
        tlpSidebar.Dock = DockStyle.Top;
        tlpSidebar.Location = new Point(2, 2);
        tlpSidebar.Margin = new Padding(0);
        tlpSidebar.Name = "tlpSidebar";
        tlpSidebar.RowCount = 7;
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tlpSidebar.Size = new Size(265, 392);
        tlpSidebar.TabIndex = 0;
        // 
        // btnDatabaseSetting
        // 
        btnDatabaseSetting.Dock = DockStyle.Fill;
        btnDatabaseSetting.Font = new Font("Segoe UI", 15F);
        btnDatabaseSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDatabaseSetting.Location = new Point(0, 0);
        btnDatabaseSetting.Margin = new Padding(0);
        btnDatabaseSetting.Name = "btnDatabaseSetting";
        btnDatabaseSetting.Radius = 0;
        btnDatabaseSetting.Size = new Size(265, 56);
        btnDatabaseSetting.TabIndex = 0;
        btnDatabaseSetting.Text = "Printer Setting";
        //
        // btnDbPathSetting
        //
        btnDbPathSetting.Dock = DockStyle.Fill;
        btnDbPathSetting.Font = new Font("Segoe UI", 15F);
        btnDbPathSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDbPathSetting.Location = new Point(0, 56);
        btnDbPathSetting.Margin = new Padding(0);
        btnDbPathSetting.Name = "btnDbPathSetting";
        btnDbPathSetting.Radius = 0;
        btnDbPathSetting.Size = new Size(265, 56);
        btnDbPathSetting.TabIndex = 1;
        btnDbPathSetting.Text = "Database Setting";
        //
        // btnDB3Setting
        // 
        btnDB3Setting.Dock = DockStyle.Fill;
        btnDB3Setting.Font = new Font("Segoe UI", 15F);
        btnDB3Setting.ForeColor = Color.FromArgb(51, 51, 51);
        btnDB3Setting.Location = new Point(0, 112);
        btnDB3Setting.Margin = new Padding(0);
        btnDB3Setting.Name = "btnDB3Setting";
        btnDB3Setting.Radius = 0;
        btnDB3Setting.Size = new Size(265, 56);
        btnDB3Setting.TabIndex = 2;
        btnDB3Setting.Text = "Backend DB Setting";
        // 
        // btnPLCSetting
        // 
        btnPLCSetting.Dock = DockStyle.Fill;
        btnPLCSetting.Font = new Font("Segoe UI", 15F);
        btnPLCSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnPLCSetting.Location = new Point(0, 168);
        btnPLCSetting.Margin = new Padding(0);
        btnPLCSetting.Name = "btnPLCSetting";
        btnPLCSetting.Radius = 0;
        btnPLCSetting.Size = new Size(265, 56);
        btnPLCSetting.TabIndex = 3;
        btnPLCSetting.Text = "PLC MK Setting";
        //
        // btnClampSetting
        //
        btnClampSetting.Dock = DockStyle.Fill;
        btnClampSetting.Font = new Font("Segoe UI", 15F);
        btnClampSetting.ForeColor = Color.FromArgb(51, 51, 51);
        btnClampSetting.Location = new Point(0, 224);
        btnClampSetting.Margin = new Padding(0);
        btnClampSetting.Name = "btnClampSetting";
        btnClampSetting.Radius = 0;
        btnClampSetting.Size = new Size(265, 56);
        btnClampSetting.TabIndex = 4;
        btnClampSetting.Text = "PLC UV Setting";
        //
        // btnUvTest
        //
        btnUvTest.Dock = DockStyle.Fill;
        btnUvTest.Font = new Font("Segoe UI", 15F);
        btnUvTest.ForeColor = Color.FromArgb(51, 51, 51);
        btnUvTest.Location = new Point(0, 280);
        btnUvTest.Margin = new Padding(0);
        btnUvTest.Name = "btnUvTest";
        btnUvTest.Radius = 0;
        btnUvTest.Size = new Size(265, 56);
        btnUvTest.TabIndex = 5;
        btnUvTest.Text = "UV Test";
        //
        // btnUv2Folder
        //
        btnUv2Folder.Dock = DockStyle.Fill;
        btnUv2Folder.Font = new Font("Segoe UI", 15F);
        btnUv2Folder.ForeColor = Color.FromArgb(51, 51, 51);
        btnUv2Folder.Location = new Point(0, 336);
        btnUv2Folder.Margin = new Padding(0);
        btnUv2Folder.Name = "btnUv2Folder";
        btnUv2Folder.Radius = 0;
        btnUv2Folder.Size = new Size(265, 56);
        btnUv2Folder.TabIndex = 6;
        btnUv2Folder.Text = "UV2 Folder";
        // 
        // pnlContent
        // 
        pnlContent.Back = Color.White;
        pnlContent.BorderColor = Color.FromArgb(36, 71, 101);
        pnlContent.BorderWidth = 2F;
        pnlContent.Controls.Add(pnlContentArea);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Location = new Point(315, 35);
        pnlContent.Margin = new Padding(0);
        pnlContent.Name = "pnlContent";
        pnlContent.Padding = new Padding(8);
        pnlContent.Radius = 12;
        pnlContent.Size = new Size(751, 791);
        pnlContent.TabIndex = 1;
        // 
        // pnlContentArea
        // 
        pnlContentArea.AutoScroll = true;
        pnlContentArea.BackColor = Color.White;
        pnlContentArea.Dock = DockStyle.Fill;
        pnlContentArea.Location = new Point(10, 10);
        pnlContentArea.Margin = new Padding(3);
        pnlContentArea.Name = "pnlContentArea";
        pnlContentArea.Size = new Size(731, 771);
        pnlContentArea.TabIndex = 0;
        // 
        // SettingUserControl
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpSettingRoot);
        Margin = new Padding(3);
        MinimumSize = new Size(820, 680);
        Name = "SettingUserControl";
        Size = new Size(1100, 860);
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
    private AntdUI.Button btnUvTest;
    private AntdUI.Button btnUv2Folder;
    private AntdUI.Panel pnlContent;
    private BufferedPanel pnlContentArea;
}
