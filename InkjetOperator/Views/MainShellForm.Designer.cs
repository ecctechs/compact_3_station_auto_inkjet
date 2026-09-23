namespace InkjetOperator.Views;

partial class MainShellForm
{
    /// <summary>
    /// เก็บส่วนประกอบที่ Designer ดูแล
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// คืนทรัพยากรของหน้าจอเมื่อเลิกใช้งาน
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
    /// Designer ใช้สร้างและจัดหน้าจอ ควรปรับ Layout ผ่าน Designer
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
        // พักการจัดหน้าจอไว้ก่อน จนกว่าจะตั้งค่าทุกส่วนครบ
        tlpShellRoot.SuspendLayout();
        tlpMenuBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        pnlContent.SuspendLayout();
        SuspendLayout();
        // 
        // titleBar — แถบชื่อโปรแกรมและปุ่มหน้าต่าง
        // 
        titleBar.BackColor = Color.FromArgb(36, 71, 101);
        titleBar.Dock = DockStyle.Fill;
        titleBar.Location = new Point(0, 0);
        titleBar.Margin = new Padding(0);
        titleBar.Name = "titleBar";
        titleBar.ShowMinimizeButton = true;
        titleBar.Size = new Size(1920, 40);
        titleBar.TabIndex = 0;
        titleBar.TitleText = "Compact Inkjet";
        // 
        // tlpShellRoot — โครงหลัก: แถบชื่อ เมนู และพื้นที่แสดงหน้า
        // 
        tlpShellRoot.BackColor = Color.White;
        tlpShellRoot.ColumnCount = 1;
        tlpShellRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpShellRoot.Controls.Add(titleBar, 0, 0);
        tlpShellRoot.Controls.Add(tlpMenuBar, 0, 1);
        tlpShellRoot.Controls.Add(pnlContent, 0, 2);
        tlpShellRoot.Dock = DockStyle.Fill;
        tlpShellRoot.Location = new Point(0, 0);
        tlpShellRoot.Name = "tlpShellRoot";
        tlpShellRoot.RowCount = 3;
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        tlpShellRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpShellRoot.Size = new Size(1194, 739);
        tlpShellRoot.TabIndex = 0;
        // 
        // tlpMenuBar — แถบเมนู พร้อมโลโก้และปุ่มภาษา
        // 
        tlpMenuBar.BackColor = Color.White;
        tlpMenuBar.ColumnCount = 7;
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
        tlpMenuBar.Controls.Add(picLogo, 5, 0);
        tlpMenuBar.Controls.Add(btnLang, 6, 0);
        tlpMenuBar.Dock = DockStyle.Fill;
        tlpMenuBar.Location = new Point(0, 40);
        tlpMenuBar.Margin = new Padding(0);
        tlpMenuBar.Padding = new Padding(0);
        tlpMenuBar.Name = "tlpMenuBar";
        tlpMenuBar.RowCount = 1;
        tlpMenuBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMenuBar.Size = new Size(1920, 64);
        tlpMenuBar.TabIndex = 0;
        // 
        // btnInputOrder — ปุ่มเปิดหน้ารับ Barcode
        // 
        btnInputOrder.DefaultBack = Color.FromArgb(91, 155, 213);
        btnInputOrder.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnInputOrder.Dock = DockStyle.Fill;
        btnInputOrder.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        btnInputOrder.ForeColor = Color.White;
        btnInputOrder.Location = new Point(3, 3);
        btnInputOrder.Name = "btnInputOrder";
        btnInputOrder.Margin = new Padding(0);
        btnInputOrder.Radius = 0;
        btnInputOrder.Size = new Size(194, 66);
        btnInputOrder.TabIndex = 0;
        btnInputOrder.Text = "Input Order";
        btnInputOrder.Click += btnInputOrder_Click;
        // 
        // btnOrderList — ปุ่มเปิดรายการงาน
        // 
        btnOrderList.DefaultBack = Color.FromArgb(176, 176, 176);
        btnOrderList.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnOrderList.Dock = DockStyle.Fill;
        btnOrderList.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        btnOrderList.ForeColor = Color.White;
        btnOrderList.Location = new Point(203, 3);
        btnOrderList.Name = "btnOrderList";
        btnOrderList.Margin = new Padding(0);
        btnOrderList.Radius = 0;
        btnOrderList.Size = new Size(194, 66);
        btnOrderList.TabIndex = 1;
        btnOrderList.Text = "Order List";
        btnOrderList.Click += btnOrderList_Click;
        // 
        // btnEditPattern — ปุ่มแก้ Pattern (ซ่อนตอนรัน)
        // 
        btnEditPattern.DefaultBack = Color.FromArgb(176, 176, 176);
        btnEditPattern.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnEditPattern.Dock = DockStyle.Fill;
        btnEditPattern.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        btnEditPattern.ForeColor = Color.White;
        btnEditPattern.Location = new Point(403, 3);
        btnEditPattern.Name = "btnEditPattern";
        btnEditPattern.Margin = new Padding(0);
        btnEditPattern.Radius = 0;
        btnEditPattern.Size = new Size(194, 66);
        btnEditPattern.TabIndex = 2;
        btnEditPattern.Text = "Edit Pattern";
        btnEditPattern.Click += btnEditPattern_Click;
        // 
        // btnSetting — ปุ่มเปิดหน้าตั้งค่า
        // 
        btnSetting.DefaultBack = Color.FromArgb(176, 176, 176);
        btnSetting.DefaultBorderColor = Color.FromArgb(36, 71, 101);
        btnSetting.Dock = DockStyle.Fill;
        btnSetting.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        btnSetting.ForeColor = Color.White;
        btnSetting.Location = new Point(803, 3);
        btnSetting.Name = "btnSetting";
        btnSetting.Margin = new Padding(0);
        btnSetting.Radius = 0;
        btnSetting.Size = new Size(194, 66);
        btnSetting.TabIndex = 3;
        btnSetting.Text = "Setting";
        btnSetting.Click += btnSetting_Click;
        // 
        // picLogo — รูปโลโก้ ย่อขยายตามกรอบ
        // 
        picLogo.BackColor = Color.Transparent;
        picLogo.Dock = DockStyle.Fill;
        picLogo.Image = (Image)resources.GetObject("picLogo.Image");
        picLogo.Location = new Point(957, 8);
        picLogo.Margin = new Padding(3, 8, 3, 8);
        picLogo.Name = "picLogo";
        picLogo.Size = new Size(140, 46);
        picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picLogo.TabIndex = 4;
        picLogo.TabStop = false;
        // 
        // btnLang — ปุ่มสลับภาษา
        // 
        btnLang.DefaultBack = Color.FromArgb(26, 26, 26);
        btnLang.DefaultBorderColor = Color.FromArgb(26, 26, 26);
        btnLang.Dock = DockStyle.Fill;
        btnLang.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        btnLang.ForeColor = Color.White;
        btnLang.Location = new Point(1117, 14);
        btnLang.Margin = new Padding(3, 14, 12, 14);
        btnLang.Name = "btnLang";
        btnLang.Radius = 4;
        btnLang.Size = new Size(72, 40);
        btnLang.TabIndex = 5;
        btnLang.Text = "EN";
        // 
        // pnlContent — พื้นที่รวมทุกหน้า สลับหน้าด้วย BringToFront
        // 
        // พื้นที่ไม่พอให้เลื่อนแทนการบีบหน้าให้เล็กกว่า MinimumSize
        pnlContent.AutoScroll = true;
        pnlContent.BackColor = Color.FromArgb(91, 155, 213);
        pnlContent.Controls.Add(scanBarcodePage);
        pnlContent.Controls.Add(orderListPage);
        pnlContent.Controls.Add(editPatternPage);
        pnlContent.Controls.Add(settingPage);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Location = new Point(0, 112);
        pnlContent.Margin = new Padding(0);
        pnlContent.Name = "pnlContent";
        pnlContent.Size = new Size(1920, 928);
        pnlContent.TabIndex = 1;
        // 
        // scanBarcodePage — หน้ารับ Barcode
        // 
        scanBarcodePage.Dock = DockStyle.Fill;
        scanBarcodePage.Location = new Point(0, 0);
        scanBarcodePage.Margin = new Padding(4);
        scanBarcodePage.MinimumSize = new Size(820, 680);
        scanBarcodePage.Name = "scanBarcodePage";
        scanBarcodePage.Size = new Size(1920, 928);
        scanBarcodePage.TabIndex = 0;
        // 
        // orderListPage — หน้ารายการงาน
        // 
        orderListPage.Dock = DockStyle.Fill;
        orderListPage.Location = new Point(0, 0);
        orderListPage.Margin = new Padding(4);
        orderListPage.MinimumSize = new Size(820, 680);
        orderListPage.Name = "orderListPage";
        orderListPage.Size = new Size(1920, 928);
        orderListPage.TabIndex = 1;
        // 
        // editPatternPage — หน้าแก้ Pattern
        // 
        editPatternPage.Dock = DockStyle.Fill;
        editPatternPage.Location = new Point(0, 0);
        editPatternPage.Margin = new Padding(4);
        editPatternPage.MinimumSize = new Size(820, 680);
        editPatternPage.Name = "editPatternPage";
        editPatternPage.Size = new Size(1920, 928);
        editPatternPage.TabIndex = 2;
        // 
        // settingPage — หน้าตั้งค่า
        // 
        settingPage.Dock = DockStyle.Fill;
        settingPage.Location = new Point(0, 0);
        settingPage.Margin = new Padding(4);
        settingPage.MinimumSize = new Size(820, 680);
        settingPage.Name = "settingPage";
        settingPage.Size = new Size(1920, 928);
        settingPage.TabIndex = 3;
        // 
        // MainShellForm — ขนาด ฟอนต์ และตำแหน่งเริ่มต้นของหน้าหลัก
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1920, 1032);
        Controls.Add(tlpShellRoot);
        Font = new Font("Segoe UI", 7.5F);
        MinimumSize = new Size(1280, 800);
        Name = "MainShellForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Compact Inkjet";
        WindowState = FormWindowState.Maximized;
        // ตั้งค่าครบแล้ว ให้แต่ละส่วนจัดหน้าจอตามค่าที่กำหนด
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
}
