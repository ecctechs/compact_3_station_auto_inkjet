namespace InkjetOperator.Views;

partial class ScanBarcodeUserControl
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanBarcodeUserControl));
        tlpScanBarcodeRoot = new TableLayoutPanel();
        lblScanBarcodeTitle = new AntdUI.Label();
        tlpBarcodeCenter = new TableLayoutPanel();
        pnlBarcodeContainer = new AntdUI.Panel();
        picBarcode = new PictureBox();
        tlpOrderCenter = new TableLayoutPanel();
        pnlOrderInformation = new AntdUI.Panel();
        tlpOrderInformation = new TableLayoutPanel();
        lblBarcode = new AntdUI.Label();
        lblErpMfg = new AntdUI.Label();
        lblMarkingMethod = new AntdUI.Label();
        lblQty = new AntdUI.Label();
        txtBarcode = new AntdUI.Input();
        txtErpMfg = new ReadOnlyInput();
        txtMarkingMethod = new ReadOnlyInput();
        tlpQty = new TableLayoutPanel();
        txtQty = new ReadOnlyInput();
        btnEditQty = new AntdUI.Button();
        flpActions = new FlowLayoutPanel();
        btnConfirm = new AntdUI.Button();
        btnClear = new AntdUI.Button();
        tlpScanBarcodeRoot.SuspendLayout();
        tlpBarcodeCenter.SuspendLayout();
        pnlBarcodeContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picBarcode).BeginInit();
        tlpOrderCenter.SuspendLayout();
        pnlOrderInformation.SuspendLayout();
        tlpOrderInformation.SuspendLayout();
        tlpQty.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        // 
        // tlpScanBarcodeRoot
        // 
        tlpScanBarcodeRoot.BackColor = Color.FromArgb(91, 155, 213);
        tlpScanBarcodeRoot.ColumnCount = 1;
        tlpScanBarcodeRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpScanBarcodeRoot.Controls.Add(lblScanBarcodeTitle, 0, 1);
        tlpScanBarcodeRoot.Controls.Add(tlpBarcodeCenter, 0, 3);
        tlpScanBarcodeRoot.Controls.Add(tlpOrderCenter, 0, 5);
        tlpScanBarcodeRoot.Controls.Add(flpActions, 0, 7);
        tlpScanBarcodeRoot.Dock = DockStyle.Fill;
        tlpScanBarcodeRoot.Location = new Point(0, 0);
        tlpScanBarcodeRoot.Margin = new Padding(4);
        tlpScanBarcodeRoot.Name = "tlpScanBarcodeRoot";
        tlpScanBarcodeRoot.RowCount = 9;
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 8.04F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 172F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 11.67F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 270F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 28.15F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 580F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 26.07F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 117F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 26.07F));
        tlpScanBarcodeRoot.Size = new Size(2062, 1612);
        tlpScanBarcodeRoot.TabIndex = 0;
        // 
        // lblScanBarcodeTitle
        // 
        lblScanBarcodeTitle.Dock = DockStyle.Fill;
        lblScanBarcodeTitle.Font = new Font("Segoe UI", 35F, FontStyle.Bold);
        lblScanBarcodeTitle.ForeColor = Color.FromArgb(17, 17, 17);
        lblScanBarcodeTitle.Location = new Point(0, 38);
        lblScanBarcodeTitle.Margin = new Padding(0);
        lblScanBarcodeTitle.Name = "lblScanBarcodeTitle";
        lblScanBarcodeTitle.Size = new Size(2062, 172);
        lblScanBarcodeTitle.TabIndex = 0;
        lblScanBarcodeTitle.Text = "Scan Barcode";
        lblScanBarcodeTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // tlpBarcodeCenter
        // 
        tlpBarcodeCenter.BackColor = Color.FromArgb(91, 155, 213);
        tlpBarcodeCenter.ColumnCount = 1;
        tlpBarcodeCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpBarcodeCenter.Controls.Add(pnlBarcodeContainer, 0, 0);
        tlpBarcodeCenter.Dock = DockStyle.Fill;
        tlpBarcodeCenter.Location = new Point(0, 265);
        tlpBarcodeCenter.Margin = new Padding(0);
        tlpBarcodeCenter.Name = "tlpBarcodeCenter";
        tlpBarcodeCenter.RowCount = 1;
        tlpBarcodeCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpBarcodeCenter.Size = new Size(2062, 270);
        tlpBarcodeCenter.TabIndex = 1;
        // 
        // pnlBarcodeContainer
        // 
        pnlBarcodeContainer.Anchor = AnchorStyles.None;
        pnlBarcodeContainer.Back = Color.FromArgb(220, 233, 245);
        pnlBarcodeContainer.BorderColor = Color.White;
        pnlBarcodeContainer.BorderWidth = 3F;
        pnlBarcodeContainer.Controls.Add(picBarcode);
        pnlBarcodeContainer.Location = new Point(611, 0);
        pnlBarcodeContainer.Margin = new Padding(0);
        pnlBarcodeContainer.Name = "pnlBarcodeContainer";
        pnlBarcodeContainer.Padding = new Padding(24);
        pnlBarcodeContainer.Radius = 24;
        pnlBarcodeContainer.Size = new Size(840, 270);
        pnlBarcodeContainer.TabIndex = 0;
        // 
        // picBarcode
        // 
        picBarcode.BackColor = Color.Transparent;
        picBarcode.Dock = DockStyle.Fill;
        picBarcode.Image = (Image)resources.GetObject("picBarcode.Image");
        picBarcode.Location = new Point(29, 29);
        picBarcode.Margin = new Padding(4);
        picBarcode.Name = "picBarcode";
        picBarcode.Size = new Size(782, 212);
        picBarcode.SizeMode = PictureBoxSizeMode.Zoom;
        picBarcode.TabIndex = 0;
        picBarcode.TabStop = false;
        // 
        // tlpOrderCenter
        // 
        tlpOrderCenter.BackColor = Color.FromArgb(91, 155, 213);
        tlpOrderCenter.ColumnCount = 3;
        tlpOrderCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
        tlpOrderCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
        tlpOrderCenter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
        tlpOrderCenter.Controls.Add(pnlOrderInformation, 1, 0);
        tlpOrderCenter.Dock = DockStyle.Fill;
        tlpOrderCenter.Location = new Point(0, 668);
        tlpOrderCenter.Margin = new Padding(0);
        tlpOrderCenter.Name = "tlpOrderCenter";
        tlpOrderCenter.RowCount = 1;
        tlpOrderCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpOrderCenter.Size = new Size(2062, 580);
        tlpOrderCenter.TabIndex = 2;
        // 
        // pnlOrderInformation
        // 
        pnlOrderInformation.Back = Color.White;
        pnlOrderInformation.BorderColor = Color.FromArgb(36, 71, 101);
        pnlOrderInformation.BorderWidth = 4F;
        pnlOrderInformation.Controls.Add(tlpOrderInformation);
        pnlOrderInformation.Dock = DockStyle.Fill;
        pnlOrderInformation.Location = new Point(453, 0);
        pnlOrderInformation.Margin = new Padding(0);
        pnlOrderInformation.Name = "pnlOrderInformation";
        pnlOrderInformation.Padding = new Padding(48);
        pnlOrderInformation.Radius = 22;
        pnlOrderInformation.Size = new Size(1154, 580);
        pnlOrderInformation.TabIndex = 0;
        // 
        // tlpOrderInformation
        // 
        tlpOrderInformation.BackColor = Color.White;
        tlpOrderInformation.ColumnCount = 2;
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22.2F));
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 77.8F));
        tlpOrderInformation.Controls.Add(lblBarcode, 0, 0);
        tlpOrderInformation.Controls.Add(lblErpMfg, 0, 1);
        tlpOrderInformation.Controls.Add(lblMarkingMethod, 0, 2);
        tlpOrderInformation.Controls.Add(lblQty, 0, 3);
        tlpOrderInformation.Controls.Add(txtBarcode, 1, 0);
        tlpOrderInformation.Controls.Add(txtErpMfg, 1, 1);
        tlpOrderInformation.Controls.Add(txtMarkingMethod, 1, 2);
        tlpOrderInformation.Controls.Add(tlpQty, 1, 3);
        tlpOrderInformation.Dock = DockStyle.Fill;
        tlpOrderInformation.Location = new Point(54, 54);
        tlpOrderInformation.Margin = new Padding(4);
        tlpOrderInformation.Name = "tlpOrderInformation";
        tlpOrderInformation.RowCount = 4;
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        tlpOrderInformation.Size = new Size(1046, 472);
        tlpOrderInformation.TabIndex = 0;
        // 
        // lblBarcode
        // 
        lblBarcode.Dock = DockStyle.Fill;
        lblBarcode.Font = new Font("Segoe UI", 12F);
        lblBarcode.ForeColor = Color.FromArgb(17, 17, 17);
        lblBarcode.Location = new Point(4, 0);
        lblBarcode.Margin = new Padding(4, 0, 30, 0);
        lblBarcode.Name = "lblBarcode";
        lblBarcode.Size = new Size(198, 118);
        lblBarcode.TabIndex = 0;
        lblBarcode.Text = "Barcode:";
        lblBarcode.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblErpMfg
        // 
        lblErpMfg.Dock = DockStyle.Fill;
        lblErpMfg.Font = new Font("Segoe UI", 12F);
        lblErpMfg.ForeColor = Color.FromArgb(17, 17, 17);
        lblErpMfg.Location = new Point(4, 118);
        lblErpMfg.Margin = new Padding(4, 0, 30, 0);
        lblErpMfg.Name = "lblErpMfg";
        lblErpMfg.Size = new Size(198, 118);
        lblErpMfg.TabIndex = 1;
        lblErpMfg.Text = "ERP MFG:";
        lblErpMfg.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblMarkingMethod
        // 
        lblMarkingMethod.Dock = DockStyle.Fill;
        lblMarkingMethod.Font = new Font("Segoe UI", 12F);
        lblMarkingMethod.ForeColor = Color.FromArgb(17, 17, 17);
        lblMarkingMethod.Location = new Point(4, 236);
        lblMarkingMethod.Margin = new Padding(4, 0, 30, 0);
        lblMarkingMethod.Name = "lblMarkingMethod";
        lblMarkingMethod.Size = new Size(198, 118);
        lblMarkingMethod.TabIndex = 2;
        lblMarkingMethod.Text = "Marking Method:";
        lblMarkingMethod.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblQty
        // 
        lblQty.Dock = DockStyle.Fill;
        lblQty.Font = new Font("Segoe UI", 12F);
        lblQty.ForeColor = Color.FromArgb(17, 17, 17);
        lblQty.Location = new Point(4, 354);
        lblQty.Margin = new Padding(4, 0, 30, 0);
        lblQty.Name = "lblQty";
        lblQty.Size = new Size(198, 118);
        lblQty.TabIndex = 3;
        lblQty.Text = "Qty:";
        lblQty.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtBarcode
        // 
        txtBarcode.BorderColor = Color.FromArgb(91, 155, 213);
        txtBarcode.Dock = DockStyle.Fill;
        txtBarcode.Font = new Font("Segoe UI", 12F);
        txtBarcode.Location = new Point(236, 22);
        txtBarcode.Margin = new Padding(4, 22, 22, 22);
        txtBarcode.Name = "txtBarcode";
        txtBarcode.Radius = 8;
        txtBarcode.Size = new Size(788, 74);
        txtBarcode.TabIndex = 4;
        // 
        // txtErpMfg
        // 
        txtErpMfg.BackColor = Color.FromArgb(242, 242, 242);
        txtErpMfg.BorderColor = Color.FromArgb(191, 191, 191);
        txtErpMfg.CaretVisible = false;
        txtErpMfg.Dock = DockStyle.Fill;
        txtErpMfg.Font = new Font("Segoe UI", 12F);
        txtErpMfg.ForeColor = Color.FromArgb(89, 89, 89);
        txtErpMfg.Location = new Point(236, 140);
        txtErpMfg.Margin = new Padding(4, 22, 22, 22);
        txtErpMfg.Name = "txtErpMfg";
        txtErpMfg.Radius = 8;
        txtErpMfg.ReadOnly = true;
        txtErpMfg.Size = new Size(788, 74);
        txtErpMfg.TabIndex = 5;
        txtErpMfg.TabStop = false;
        // 
        // txtMarkingMethod
        // 
        txtMarkingMethod.BackColor = Color.FromArgb(242, 242, 242);
        txtMarkingMethod.BorderColor = Color.FromArgb(191, 191, 191);
        txtMarkingMethod.CaretVisible = false;
        txtMarkingMethod.Dock = DockStyle.Fill;
        txtMarkingMethod.Font = new Font("Segoe UI", 12F);
        txtMarkingMethod.ForeColor = Color.FromArgb(89, 89, 89);
        txtMarkingMethod.Location = new Point(236, 258);
        txtMarkingMethod.Margin = new Padding(4, 22, 22, 22);
        txtMarkingMethod.Name = "txtMarkingMethod";
        txtMarkingMethod.Radius = 8;
        txtMarkingMethod.ReadOnly = true;
        txtMarkingMethod.Size = new Size(788, 74);
        txtMarkingMethod.TabIndex = 6;
        txtMarkingMethod.TabStop = false;
        // 
        // tlpQty
        // 
        tlpQty.BackColor = Color.White;
        tlpQty.ColumnCount = 2;
        tlpQty.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpQty.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
        tlpQty.Controls.Add(txtQty, 0, 0);
        tlpQty.Controls.Add(btnEditQty, 1, 0);
        tlpQty.Dock = DockStyle.Fill;
        tlpQty.Location = new Point(236, 376);
        tlpQty.Margin = new Padding(4, 22, 22, 22);
        tlpQty.Name = "tlpQty";
        tlpQty.RowCount = 1;
        tlpQty.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpQty.Size = new Size(788, 74);
        tlpQty.TabIndex = 7;
        // 
        // txtQty
        // 
        txtQty.BackColor = Color.FromArgb(242, 242, 242);
        txtQty.BorderColor = Color.FromArgb(191, 191, 191);
        txtQty.CaretVisible = false;
        txtQty.Dock = DockStyle.Fill;
        txtQty.Font = new Font("Segoe UI", 12F);
        txtQty.ForeColor = Color.FromArgb(89, 89, 89);
        txtQty.Location = new Point(0, 0);
        txtQty.Margin = new Padding(0);
        txtQty.Name = "txtQty";
        txtQty.Radius = 8;
        txtQty.ReadOnly = true;
        txtQty.Size = new Size(660, 74);
        txtQty.TabIndex = 0;
        txtQty.TabStop = false;
        // 
        // btnEditQty
        // 
        btnEditQty.Dock = DockStyle.Fill;
        btnEditQty.Enabled = false;
        btnEditQty.IconSvg = "EditOutlined";
        btnEditQty.Location = new Point(676, 0);
        btnEditQty.Margin = new Padding(16, 0, 0, 0);
        btnEditQty.Name = "btnEditQty";
        btnEditQty.Radius = 8;
        btnEditQty.Size = new Size(112, 74);
        btnEditQty.TabIndex = 1;
        btnEditQty.Type = AntdUI.TTypeMini.Primary;
        btnEditQty.Click += btnEditQty_Click_1;
        // 
        // flpActions
        // 
        flpActions.Anchor = AnchorStyles.None;
        flpActions.AutoSize = true;
        flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flpActions.BackColor = Color.FromArgb(91, 155, 213);
        flpActions.Controls.Add(btnConfirm);
        flpActions.Controls.Add(btnClear);
        flpActions.Location = new Point(604, 1371);
        flpActions.Margin = new Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new Size(854, 117);
        flpActions.TabIndex = 3;
        flpActions.WrapContents = false;
        // 
        // btnConfirm
        // 
        btnConfirm.Font = new Font("Segoe UI", 19F);
        btnConfirm.ForeColor = Color.White;
        btnConfirm.Location = new Point(0, 0);
        btnConfirm.Margin = new Padding(0, 0, 52, 0);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Radius = 12;
        btnConfirm.Size = new Size(375, 117);
        btnConfirm.TabIndex = 0;
        btnConfirm.Text = "OK";
        btnConfirm.Type = AntdUI.TTypeMini.Success;
        // 
        // btnClear
        // 
        btnClear.Font = new Font("Segoe UI", 19F);
        btnClear.ForeColor = Color.White;
        btnClear.Location = new Point(479, 0);
        btnClear.Margin = new Padding(52, 0, 0, 0);
        btnClear.Name = "btnClear";
        btnClear.Radius = 12;
        btnClear.Size = new Size(375, 117);
        btnClear.TabIndex = 1;
        btnClear.Text = "Clear";
        btnClear.Type = AntdUI.TTypeMini.Error;
        // 
        // ScanBarcodeUserControl
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpScanBarcodeRoot);
        Margin = new Padding(4);
        MinimumSize = new Size(1230, 1020);
        Name = "ScanBarcodeUserControl";
        Size = new Size(2062, 1612);
        tlpScanBarcodeRoot.ResumeLayout(false);
        tlpScanBarcodeRoot.PerformLayout();
        tlpBarcodeCenter.ResumeLayout(false);
        pnlBarcodeContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picBarcode).EndInit();
        tlpOrderCenter.ResumeLayout(false);
        pnlOrderInformation.ResumeLayout(false);
        tlpOrderInformation.ResumeLayout(false);
        tlpQty.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpScanBarcodeRoot;
    private AntdUI.Label lblScanBarcodeTitle;
    private System.Windows.Forms.TableLayoutPanel tlpBarcodeCenter;
    private AntdUI.Panel pnlBarcodeContainer;
    private System.Windows.Forms.PictureBox picBarcode;
    private System.Windows.Forms.TableLayoutPanel tlpOrderCenter;
    private AntdUI.Panel pnlOrderInformation;
    private System.Windows.Forms.TableLayoutPanel tlpOrderInformation;
    private AntdUI.Label lblBarcode;
    private AntdUI.Label lblErpMfg;
    private AntdUI.Label lblMarkingMethod;
    private AntdUI.Label lblQty;
    private AntdUI.Input txtBarcode;
    private ReadOnlyInput txtErpMfg;
    private ReadOnlyInput txtMarkingMethod;
    private System.Windows.Forms.TableLayoutPanel tlpQty;
    private ReadOnlyInput txtQty;
    private AntdUI.Button btnEditQty;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnConfirm;
    private AntdUI.Button btnClear;
}
