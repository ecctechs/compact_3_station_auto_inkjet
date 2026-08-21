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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainShellForm));
        titleBar = new AppTitleBarUserControl();
        tlpShellRoot = new TableLayoutPanel();
        tlpMenuBar = new TableLayoutPanel();
        btnInputOrder = new AntdUI.Button();
        btnOrderList = new AntdUI.Button();
        btnEditPattern = new AntdUI.Button();
        btnSetting = new AntdUI.Button();
        picLogo = new PictureBox();
        btnLang = new AntdUI.Button();
        pnlContent = new Panel();
        scanBarcodePage = new ScanBarcodeUserControl();
        orderListPage = new OrderListUserControl();
        editPatternPage = new EditPatternUserControl();
        settingPage = new SettingUserControl();
        transferListPage = new TransferListUserControl();
        btnTransfer = new AntdUI.Button();
        tlpShellRoot.SuspendLayout();
        tlpMenuBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        pnlContent.SuspendLayout();
        SuspendLayout();
        // 
        // tlpShellRoot
        // 
        tlpShellRoot.BackColor = Color.White;
        tlpShellRoot.ColumnCount = 1;
        tlpShellRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpShellRoot.Controls.Add(titleBar, 0, 0);
        tlpShellRoot.Controls.Add(tlpMenuBar, 0, 1);
        tlpShellRoot.Controls.Add(pnlContent, 0, 2);
        tlpShellRoot.Dock = DockStyle.Fill;
        tlpShellRoot.Location = new Point(0, 0);
        tlpShellRoot.Margin = new Padding(3, 3, 3, 3);
        tlpShellRoot.Name = "tlpShellRoot";
        tlpShellRoot.RowCount = 3;
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpShellRoot.Size = new Size(1200, 900);
        tlpShellRoot.TabIndex = 0;
        // 
        // titleBar
        // 
        titleBar.Dock = DockStyle.Fill;
        titleBar.Location = new Point(0, 0);
        titleBar.Margin = new Padding(0);
        titleBar.Name = "titleBar";
        titleBar.Size = new Size(1200, 40);
        titleBar.TabIndex = 0;
        titleBar.TitleText = "Compact Inkjet";
        // 
        // tlpMenuBar
        // 
        tlpMenuBar.BackColor = Color.White;
        tlpMenuBar.ColumnCount = 8;
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        tlpMenuBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        tlpMenuBar.Controls.Add(btnInputOrder, 0, 0);
        tlpMenuBar.Controls.Add(btnOrderList, 1, 0);
        tlpMenuBar.Controls.Add(btnEditPattern, 2, 0);
        tlpMenuBar.Controls.Add(btnSetting, 3, 0);
        tlpMenuBar.Controls.Add(btnTransfer, 4, 0);
        tlpMenuBar.Controls.Add(picLogo, 6, 0);
        tlpMenuBar.Controls.Add(btnLang, 7, 0);
        tlpMenuBar.Dock = DockStyle.Fill;
        tlpMenuBar.Location = new Point(0, 0);
        tlpMenuBar.Margin = new Padding(0);
        tlpMenuBar.Name = "tlpMenuBar";
        tlpMenuBar.RowCount = 1;
        tlpMenuBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMenuBar.Size = new Size(1200, 72);
        tlpMenuBar.TabIndex = 0;
        // 
        // btnInputOrder
        // 
        btnInputOrder.DefaultBack = Color.FromArgb(91, 155, 213);
        btnInputOrder.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnInputOrder.Dock = DockStyle.Fill;
        btnInputOrder.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnInputOrder.ForeColor = Color.White;
        btnInputOrder.Location = new Point(3, 3);
        btnInputOrder.Margin = new Padding(3, 3, 3, 3);
        btnInputOrder.Name = "btnInputOrder";
        btnInputOrder.Radius = 0;
        btnInputOrder.Size = new Size(195, 67);
        btnInputOrder.TabIndex = 0;
        btnInputOrder.Text = "Input Order";
        btnInputOrder.Click += btnInputOrder_Click;
        // 
        // btnOrderList
        // 
        btnOrderList.DefaultBack = Color.FromArgb(176, 176, 176);
        btnOrderList.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnOrderList.Dock = DockStyle.Fill;
        btnOrderList.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnOrderList.ForeColor = Color.White;
        btnOrderList.Location = new Point(203, 3);
        btnOrderList.Margin = new Padding(3, 3, 3, 3);
        btnOrderList.Name = "btnOrderList";
        btnOrderList.Radius = 0;
        btnOrderList.Size = new Size(195, 67);
        btnOrderList.TabIndex = 1;
        btnOrderList.Text = "Order List";
        btnOrderList.Click += btnOrderList_Click;
        // 
        // btnEditPattern
        // 
        btnEditPattern.DefaultBack = Color.FromArgb(176, 176, 176);
        btnEditPattern.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnEditPattern.Dock = DockStyle.Fill;
        btnEditPattern.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnEditPattern.ForeColor = Color.White;
        btnEditPattern.Location = new Point(403, 3);
        btnEditPattern.Margin = new Padding(3, 3, 3, 3);
        btnEditPattern.Name = "btnEditPattern";
        btnEditPattern.Radius = 0;
        btnEditPattern.Size = new Size(195, 67);
        btnEditPattern.TabIndex = 2;
        btnEditPattern.Text = "Edit Pattern";
        btnEditPattern.Click += btnEditPattern_Click;
        // 
        // btnSetting
        // 
        btnSetting.DefaultBack = Color.FromArgb(176, 176, 176);
        btnSetting.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnSetting.Dock = DockStyle.Fill;
        btnSetting.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnSetting.ForeColor = Color.White;
        btnSetting.Location = new Point(603, 3);
        btnSetting.Margin = new Padding(3, 3, 3, 3);
        btnSetting.Name = "btnSetting";
        btnSetting.Radius = 0;
        btnSetting.Size = new Size(195, 67);
        btnSetting.TabIndex = 3;
        btnSetting.Text = "Setting";
        btnSetting.Click += btnSetting_Click;
        //
        // btnTransfer
        //
        btnTransfer.DefaultBack = Color.FromArgb(176, 176, 176);
        btnTransfer.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnTransfer.Dock = DockStyle.Fill;
        btnTransfer.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnTransfer.ForeColor = Color.White;
        btnTransfer.Location = new Point(803, 3);
        btnTransfer.Margin = new Padding(3, 3, 3, 3);
        btnTransfer.Name = "btnTransfer";
        btnTransfer.Radius = 0;
        btnTransfer.Size = new Size(195, 67);
        btnTransfer.TabIndex = 4;
        btnTransfer.Text = "Transfer ST1";
        btnTransfer.Click += btnTransfer_Click;
        //
        // picLogo
        // 
        picLogo.BackColor = Color.Transparent;
        picLogo.Dock = DockStyle.Fill;
        picLogo.Image = (Image)resources.GetObject("picLogo.Image");
        picLogo.Location = new Point(963, 8);
        picLogo.Margin = new Padding(3, 8, 3, 8);
        picLogo.Name = "picLogo";
        picLogo.Size = new Size(155, 56);
        picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picLogo.TabIndex = 4;
        picLogo.TabStop = false;
        // 
        // btnLang
        // 
        btnLang.DefaultBack = Color.FromArgb(26, 26, 26);
        btnLang.DefaultBorderColor = Color.FromArgb(26, 26, 26);
        btnLang.Dock = DockStyle.Fill;
        btnLang.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnLang.ForeColor = Color.White;
        btnLang.Location = new Point(1123, 14);
        btnLang.Margin = new Padding(3, 14, 12, 14);
        btnLang.Name = "btnLang";
        btnLang.Radius = 4;
        btnLang.Size = new Size(65, 44);
        btnLang.TabIndex = 5;
        btnLang.Text = "EN";
        // 
        // pnlContent
        // 
        pnlContent.BackColor = Color.FromArgb(91, 155, 213);
        pnlContent.Controls.Add(scanBarcodePage);
        pnlContent.Controls.Add(orderListPage);
        pnlContent.Controls.Add(editPatternPage);
        pnlContent.Controls.Add(transferListPage);
        pnlContent.Controls.Add(settingPage);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Location = new Point(0, 72);
        pnlContent.Margin = new Padding(0);
        pnlContent.Name = "pnlContent";
        pnlContent.Size = new Size(1200, 828);
        pnlContent.TabIndex = 1;
        // 
        // scanBarcodePage
        // 
        scanBarcodePage.Dock = DockStyle.Fill;
        scanBarcodePage.Location = new Point(0, 0);
        scanBarcodePage.Margin = new Padding(4, 4, 4, 4);
        scanBarcodePage.MinimumSize = new Size(820, 680);
        scanBarcodePage.Name = "scanBarcodePage";
        scanBarcodePage.Size = new Size(1200, 828);
        scanBarcodePage.TabIndex = 0;
        // 
        // orderListPage
        // 
        orderListPage.Dock = DockStyle.Fill;
        orderListPage.Location = new Point(0, 0);
        orderListPage.Margin = new Padding(4, 4, 4, 4);
        orderListPage.MinimumSize = new Size(820, 680);
        orderListPage.Name = "orderListPage";
        orderListPage.Size = new Size(1200, 828);
        orderListPage.TabIndex = 1;
        // 
        // editPatternPage
        // 
        editPatternPage.Dock = DockStyle.Fill;
        editPatternPage.Location = new Point(0, 0);
        editPatternPage.Margin = new Padding(4, 4, 4, 4);
        editPatternPage.MinimumSize = new Size(820, 680);
        editPatternPage.Name = "editPatternPage";
        editPatternPage.Size = new Size(1200, 828);
        editPatternPage.TabIndex = 2;
        // 
        // settingPage
        // 
        settingPage.Dock = DockStyle.Fill;
        settingPage.Location = new Point(0, 0);
        settingPage.Margin = new Padding(4, 4, 4, 4);
        settingPage.MinimumSize = new Size(820, 680);
        settingPage.Name = "settingPage";
        settingPage.Size = new Size(1200, 828);
        settingPage.TabIndex = 3;
        //
        // transferListPage
        //
        transferListPage.Dock = DockStyle.Fill;
        transferListPage.Location = new Point(0, 0);
        transferListPage.Margin = new Padding(4, 4, 4, 4);
        transferListPage.MinimumSize = new Size(820, 680);
        transferListPage.Name = "transferListPage";
        transferListPage.Size = new Size(1200, 828);
        transferListPage.TabIndex = 4;
        //
        // MainShellForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1200, 900);
        Controls.Add(tlpShellRoot);
        Margin = new Padding(3, 3, 3, 3);
        MinimumSize = new Size(933, 701);
        Name = "MainShellForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Compact Inkjet";
        WindowState = FormWindowState.Maximized;
        tlpShellRoot.ResumeLayout(false);
        tlpMenuBar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        pnlContent.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private AppTitleBarUserControl titleBar;
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
    private TransferListUserControl transferListPage;
    private AntdUI.Button btnTransfer;
}
