namespace InkjetOperator.Views;

partial class MainShellForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tlpShellRoot = new System.Windows.Forms.TableLayoutPanel();
        tlpMenuBar = new System.Windows.Forms.TableLayoutPanel();
        btnInputOrder = new AntdUI.Button();
        btnOrderList = new AntdUI.Button();
        btnEditPattern = new AntdUI.Button();
        btnSetting = new AntdUI.Button();
        picLogo = new System.Windows.Forms.PictureBox();
        btnLang = new AntdUI.Button();
        pnlContent = new System.Windows.Forms.Panel();
        settingPage = new SettingUserControl();
        editPatternPage = new EditPatternUserControl();
        orderListPage = new OrderListUserControl();
        scanBarcodePage = new ScanBarcodeUserControl();
        tlpShellRoot.SuspendLayout();
        tlpMenuBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        pnlContent.SuspendLayout();
        SuspendLayout();
        //
        // tlpShellRoot
        //
        tlpShellRoot.BackColor = System.Drawing.Color.White;
        tlpShellRoot.ColumnCount = 1;
        tlpShellRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpShellRoot.Controls.Add(tlpMenuBar, 0, 0);
        tlpShellRoot.Controls.Add(pnlContent, 0, 1);
        tlpShellRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpShellRoot.Location = new System.Drawing.Point(0, 0);
        tlpShellRoot.Name = "tlpShellRoot";
        tlpShellRoot.RowCount = 2;
        tlpShellRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
        tlpShellRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpShellRoot.Size = new System.Drawing.Size(1200, 900);
        tlpShellRoot.TabIndex = 0;
        //
        // tlpMenuBar
        //
        tlpMenuBar.BackColor = System.Drawing.Color.White;
        tlpMenuBar.ColumnCount = 7;
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
        tlpMenuBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
        tlpMenuBar.Controls.Add(btnInputOrder, 0, 0);
        tlpMenuBar.Controls.Add(btnOrderList, 1, 0);
        tlpMenuBar.Controls.Add(btnEditPattern, 2, 0);
        tlpMenuBar.Controls.Add(btnSetting, 3, 0);
        tlpMenuBar.Controls.Add(picLogo, 5, 0);
        tlpMenuBar.Controls.Add(btnLang, 6, 0);
        tlpMenuBar.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMenuBar.Location = new System.Drawing.Point(0, 0);
        tlpMenuBar.Margin = new System.Windows.Forms.Padding(0);
        tlpMenuBar.Name = "tlpMenuBar";
        tlpMenuBar.RowCount = 1;
        tlpMenuBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMenuBar.Size = new System.Drawing.Size(1200, 72);
        tlpMenuBar.TabIndex = 0;
        //
        // btnInputOrder
        //
        btnInputOrder.DefaultBack = System.Drawing.Color.FromArgb(91, 155, 213);
        btnInputOrder.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnInputOrder.Dock = System.Windows.Forms.DockStyle.Fill;
        btnInputOrder.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnInputOrder.ForeColor = System.Drawing.Color.White;
        btnInputOrder.Location = new System.Drawing.Point(3, 3);
        btnInputOrder.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        btnInputOrder.Name = "btnInputOrder";
        btnInputOrder.Radius = 0;
        btnInputOrder.Size = new System.Drawing.Size(194, 66);
        btnInputOrder.TabIndex = 0;
        btnInputOrder.Text = "Input Order";
        btnInputOrder.Click += btnInputOrder_Click;
        //
        // btnOrderList
        //
        btnOrderList.DefaultBack = System.Drawing.Color.FromArgb(176, 176, 176);
        btnOrderList.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnOrderList.Dock = System.Windows.Forms.DockStyle.Fill;
        btnOrderList.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnOrderList.ForeColor = System.Drawing.Color.White;
        btnOrderList.Location = new System.Drawing.Point(203, 3);
        btnOrderList.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        btnOrderList.Name = "btnOrderList";
        btnOrderList.Radius = 0;
        btnOrderList.Size = new System.Drawing.Size(194, 66);
        btnOrderList.TabIndex = 1;
        btnOrderList.Text = "Order List";
        btnOrderList.Click += btnOrderList_Click;
        //
        // btnEditPattern
        //
        btnEditPattern.DefaultBack = System.Drawing.Color.FromArgb(176, 176, 176);
        btnEditPattern.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnEditPattern.Dock = System.Windows.Forms.DockStyle.Fill;
        btnEditPattern.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnEditPattern.ForeColor = System.Drawing.Color.White;
        btnEditPattern.Location = new System.Drawing.Point(403, 3);
        btnEditPattern.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        btnEditPattern.Name = "btnEditPattern";
        btnEditPattern.Radius = 0;
        btnEditPattern.Size = new System.Drawing.Size(194, 66);
        btnEditPattern.TabIndex = 2;
        btnEditPattern.Text = "Edit Pattern";
        btnEditPattern.Click += btnEditPattern_Click;
        //
        // btnSetting
        //
        btnSetting.DefaultBack = System.Drawing.Color.FromArgb(176, 176, 176);
        btnSetting.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnSetting.Dock = System.Windows.Forms.DockStyle.Fill;
        btnSetting.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnSetting.ForeColor = System.Drawing.Color.White;
        btnSetting.Location = new System.Drawing.Point(603, 3);
        btnSetting.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
        btnSetting.Name = "btnSetting";
        btnSetting.Radius = 0;
        btnSetting.Size = new System.Drawing.Size(194, 66);
        btnSetting.TabIndex = 3;
        btnSetting.Text = "Setting";
        btnSetting.Click += btnSetting_Click;
        //
        // picLogo
        //
        picLogo.BackColor = System.Drawing.Color.Transparent;
        picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
        picLogo.Location = new System.Drawing.Point(963, 8);
        picLogo.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        picLogo.Name = "picLogo";
        picLogo.Size = new System.Drawing.Size(154, 56);
        picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picLogo.TabIndex = 4;
        picLogo.TabStop = false;
        //
        // btnLang
        //
        btnLang.DefaultBack = System.Drawing.Color.FromArgb(26, 26, 26);
        btnLang.DefaultBorderColor = System.Drawing.Color.FromArgb(26, 26, 26);
        btnLang.Dock = System.Windows.Forms.DockStyle.Fill;
        btnLang.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnLang.ForeColor = System.Drawing.Color.White;
        btnLang.Location = new System.Drawing.Point(1123, 14);
        btnLang.Margin = new System.Windows.Forms.Padding(3, 14, 12, 14);
        btnLang.Name = "btnLang";
        btnLang.Radius = 4;
        btnLang.Size = new System.Drawing.Size(65, 44);
        btnLang.TabIndex = 5;
        btnLang.Text = "EN";
        //
        // pnlContent
        //
        pnlContent.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        pnlContent.Controls.Add(scanBarcodePage);
        pnlContent.Controls.Add(orderListPage);
        pnlContent.Controls.Add(editPatternPage);
        pnlContent.Controls.Add(settingPage);
        pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlContent.Location = new System.Drawing.Point(0, 72);
        pnlContent.Margin = new System.Windows.Forms.Padding(0);
        pnlContent.Name = "pnlContent";
        pnlContent.Size = new System.Drawing.Size(1200, 828);
        pnlContent.TabIndex = 1;
        //
        // settingPage
        //
        settingPage.Dock = System.Windows.Forms.DockStyle.Fill;
        settingPage.Location = new System.Drawing.Point(0, 0);
        settingPage.MinimumSize = new System.Drawing.Size(820, 680);
        settingPage.Name = "settingPage";
        settingPage.Size = new System.Drawing.Size(1200, 828);
        settingPage.TabIndex = 3;
        //
        // editPatternPage
        //
        editPatternPage.Dock = System.Windows.Forms.DockStyle.Fill;
        editPatternPage.Location = new System.Drawing.Point(0, 0);
        editPatternPage.MinimumSize = new System.Drawing.Size(820, 680);
        editPatternPage.Name = "editPatternPage";
        editPatternPage.Size = new System.Drawing.Size(1200, 828);
        editPatternPage.TabIndex = 2;
        //
        // orderListPage
        //
        orderListPage.Dock = System.Windows.Forms.DockStyle.Fill;
        orderListPage.Location = new System.Drawing.Point(0, 0);
        orderListPage.MinimumSize = new System.Drawing.Size(820, 680);
        orderListPage.Name = "orderListPage";
        orderListPage.Size = new System.Drawing.Size(1200, 828);
        orderListPage.TabIndex = 1;
        //
        // scanBarcodePage
        //
        scanBarcodePage.Dock = System.Windows.Forms.DockStyle.Fill;
        scanBarcodePage.Location = new System.Drawing.Point(0, 0);
        scanBarcodePage.MinimumSize = new System.Drawing.Size(820, 680);
        scanBarcodePage.Name = "scanBarcodePage";
        scanBarcodePage.Size = new System.Drawing.Size(1200, 828);
        scanBarcodePage.TabIndex = 0;
        //
        // MainShellForm
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(1200, 900);
        Controls.Add(tlpShellRoot);
        MinimumSize = new System.Drawing.Size(940, 720);
        Name = "MainShellForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Compact Inkjet";
        WindowState = System.Windows.Forms.FormWindowState.Maximized;
        tlpShellRoot.ResumeLayout(false);
        tlpMenuBar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        pnlContent.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpShellRoot;
    private System.Windows.Forms.TableLayoutPanel tlpMenuBar;
    private AntdUI.Button btnInputOrder;
    private AntdUI.Button btnOrderList;
    private AntdUI.Button btnEditPattern;
    private AntdUI.Button btnSetting;
    private System.Windows.Forms.PictureBox picLogo;
    private AntdUI.Button btnLang;
    private System.Windows.Forms.Panel pnlContent;
    private ScanBarcodeUserControl scanBarcodePage;
    private OrderListUserControl orderListPage;
    private EditPatternUserControl editPatternPage;
    private SettingUserControl settingPage;
}
