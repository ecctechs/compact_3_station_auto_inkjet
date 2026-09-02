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
        lblOrderNo = new AntdUI.Label();
        lblCustomerName = new AntdUI.Label();
        lblType = new AntdUI.Label();
        lblQty = new AntdUI.Label();
        txtBarcode = new AntdUI.Input();
        txtOrderNo = new AntdUI.Input();
        txtCustomerName = new AntdUI.Input();
        txtType = new AntdUI.Input();
        txtQty = new AntdUI.Input();
        flpActions = new FlowLayoutPanel();
        btnConfirm = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        tlpScanBarcodeRoot.SuspendLayout();
        tlpBarcodeCenter.SuspendLayout();
        pnlBarcodeContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picBarcode).BeginInit();
        tlpOrderCenter.SuspendLayout();
        pnlOrderInformation.SuspendLayout();
        tlpOrderInformation.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        // 
        // tlpScanBarcodeRoot
        // 
        tlpScanBarcodeRoot.BackColor = Color.FromArgb(91, 155, 213);
        tlpScanBarcodeRoot.ColumnCount = 1;
        tlpScanBarcodeRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpScanBarcodeRoot.Controls.Add(lblScanBarcodeTitle, 0, 0);
        tlpScanBarcodeRoot.Controls.Add(tlpBarcodeCenter, 0, 1);
        tlpScanBarcodeRoot.Controls.Add(tlpOrderCenter, 0, 2);
        tlpScanBarcodeRoot.Controls.Add(flpActions, 0, 3);
        tlpScanBarcodeRoot.Dock = DockStyle.Fill;
        tlpScanBarcodeRoot.Location = new Point(0, 0);
        tlpScanBarcodeRoot.Margin = new Padding(4, 4, 4, 4);
        tlpScanBarcodeRoot.Name = "tlpScanBarcodeRoot";
        tlpScanBarcodeRoot.Padding = new Padding(60, 60, 60, 60);
        tlpScanBarcodeRoot.RowCount = 4;
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 16F));
        tlpScanBarcodeRoot.Size = new Size(2062, 1612);
        tlpScanBarcodeRoot.TabIndex = 0;
        // 
        // lblScanBarcodeTitle
        // 
        lblScanBarcodeTitle.Dock = DockStyle.Fill;
        lblScanBarcodeTitle.Font = new Font("Segoe UI", 52.5F, FontStyle.Bold);
        lblScanBarcodeTitle.ForeColor = Color.FromArgb(17, 17, 17);
        lblScanBarcodeTitle.Location = new Point(60, 66);
        lblScanBarcodeTitle.Margin = new Padding(0, 6, 0, 0);
        lblScanBarcodeTitle.Name = "lblScanBarcodeTitle";
        lblScanBarcodeTitle.Size = new Size(1942, 173);
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
        tlpBarcodeCenter.Location = new Point(60, 239);
        tlpBarcodeCenter.Margin = new Padding(0);
        tlpBarcodeCenter.Name = "tlpBarcodeCenter";
        tlpBarcodeCenter.RowCount = 1;
        tlpBarcodeCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpBarcodeCenter.Size = new Size(1942, 417);
        tlpBarcodeCenter.TabIndex = 1;
        // 
        // pnlBarcodeContainer
        // 
        pnlBarcodeContainer.Anchor = AnchorStyles.None;
        pnlBarcodeContainer.Back = Color.FromArgb(220, 233, 245);
        pnlBarcodeContainer.BorderColor = Color.White;
        pnlBarcodeContainer.BorderWidth = 3F;
        pnlBarcodeContainer.Controls.Add(picBarcode);
        pnlBarcodeContainer.Location = new Point(551, 73);
        pnlBarcodeContainer.Margin = new Padding(4, 4, 4, 4);
        pnlBarcodeContainer.Name = "pnlBarcodeContainer";
        pnlBarcodeContainer.Padding = new Padding(24, 24, 24, 24);
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
        picBarcode.Margin = new Padding(4, 4, 4, 4);
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
        tlpOrderCenter.Location = new Point(60, 656);
        tlpOrderCenter.Margin = new Padding(0);
        tlpOrderCenter.Name = "tlpOrderCenter";
        tlpOrderCenter.RowCount = 1;
        tlpOrderCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpOrderCenter.Size = new Size(1942, 656);
        tlpOrderCenter.TabIndex = 2;
        // 
        // pnlOrderInformation
        // 
        pnlOrderInformation.Back = Color.White;
        pnlOrderInformation.BorderColor = Color.FromArgb(36, 71, 101);
        pnlOrderInformation.BorderWidth = 4F;
        pnlOrderInformation.Controls.Add(tlpOrderInformation);
        pnlOrderInformation.Dock = DockStyle.Fill;
        pnlOrderInformation.Location = new Point(431, 4);
        pnlOrderInformation.Margin = new Padding(4, 4, 4, 4);
        pnlOrderInformation.Name = "pnlOrderInformation";
        pnlOrderInformation.Padding = new Padding(48, 48, 48, 48);
        pnlOrderInformation.Radius = 22;
        pnlOrderInformation.Size = new Size(1079, 648);
        pnlOrderInformation.TabIndex = 0;
        // 
        // tlpOrderInformation
        // 
        tlpOrderInformation.BackColor = Color.White;
        tlpOrderInformation.ColumnCount = 2;
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
        tlpOrderInformation.Controls.Add(lblBarcode, 0, 0);
        tlpOrderInformation.Controls.Add(lblOrderNo, 0, 1);
        tlpOrderInformation.Controls.Add(lblCustomerName, 0, 2);
        tlpOrderInformation.Controls.Add(lblType, 0, 3);
        tlpOrderInformation.Controls.Add(lblQty, 0, 4);
        tlpOrderInformation.Controls.Add(txtBarcode, 1, 0);
        tlpOrderInformation.Controls.Add(txtOrderNo, 1, 1);
        tlpOrderInformation.Controls.Add(txtCustomerName, 1, 2);
        tlpOrderInformation.Controls.Add(txtType, 1, 3);
        tlpOrderInformation.Controls.Add(txtQty, 1, 4);
        tlpOrderInformation.Dock = DockStyle.Fill;
        tlpOrderInformation.Location = new Point(54, 54);
        tlpOrderInformation.Margin = new Padding(4, 4, 4, 4);
        tlpOrderInformation.Name = "tlpOrderInformation";
        tlpOrderInformation.RowCount = 5;
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.Size = new Size(971, 540);
        tlpOrderInformation.TabIndex = 0;
        // 
        // lblBarcode
        // 
        lblBarcode.Dock = DockStyle.Fill;
        lblBarcode.Font = new Font("Segoe UI", 18F);
        lblBarcode.ForeColor = Color.FromArgb(17, 17, 17);
        lblBarcode.Location = new Point(4, 0);
        lblBarcode.Margin = new Padding(4, 0, 30, 0);
        lblBarcode.Name = "lblBarcode";
        lblBarcode.Size = new Size(237, 108);
        lblBarcode.TabIndex = 0;
        lblBarcode.Text = "Barcode:";
        lblBarcode.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblOrderNo
        // 
        lblOrderNo.Dock = DockStyle.Fill;
        lblOrderNo.Font = new Font("Segoe UI", 18F);
        lblOrderNo.ForeColor = Color.FromArgb(17, 17, 17);
        lblOrderNo.Location = new Point(4, 108);
        lblOrderNo.Margin = new Padding(4, 0, 30, 0);
        lblOrderNo.Name = "lblOrderNo";
        lblOrderNo.Size = new Size(237, 108);
        lblOrderNo.TabIndex = 1;
        lblOrderNo.Text = "Order No:";
        lblOrderNo.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblCustomerName
        // 
        lblCustomerName.Dock = DockStyle.Fill;
        lblCustomerName.Font = new Font("Segoe UI", 18F);
        lblCustomerName.ForeColor = Color.FromArgb(17, 17, 17);
        lblCustomerName.Location = new Point(4, 216);
        lblCustomerName.Margin = new Padding(4, 0, 30, 0);
        lblCustomerName.Name = "lblCustomerName";
        lblCustomerName.Size = new Size(237, 108);
        lblCustomerName.TabIndex = 2;
        lblCustomerName.Text = "Customer Name:";
        lblCustomerName.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblType
        // 
        lblType.Dock = DockStyle.Fill;
        lblType.Font = new Font("Segoe UI", 18F);
        lblType.ForeColor = Color.FromArgb(17, 17, 17);
        lblType.Location = new Point(4, 324);
        lblType.Margin = new Padding(4, 0, 30, 0);
        lblType.Name = "lblType";
        lblType.Size = new Size(237, 108);
        lblType.TabIndex = 3;
        lblType.Text = "Type:";
        lblType.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblQty
        // 
        lblQty.Dock = DockStyle.Fill;
        lblQty.Font = new Font("Segoe UI", 18F);
        lblQty.ForeColor = Color.FromArgb(17, 17, 17);
        lblQty.Location = new Point(4, 432);
        lblQty.Margin = new Padding(4, 0, 30, 0);
        lblQty.Name = "lblQty";
        lblQty.Size = new Size(237, 108);
        lblQty.TabIndex = 4;
        lblQty.Text = "Qty:";
        lblQty.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtBarcode
        // 
        txtBarcode.BorderColor = Color.FromArgb(91, 155, 213);
        txtBarcode.Dock = DockStyle.Fill;
        txtBarcode.Font = new Font("Segoe UI", 18F);
        txtBarcode.Location = new Point(275, 14);
        txtBarcode.Margin = new Padding(4, 14, 4, 14);
        txtBarcode.Name = "txtBarcode";
        txtBarcode.PlaceholderText = "Scan or type barcode...";
        txtBarcode.Radius = 8;
        txtBarcode.Size = new Size(692, 80);
        txtBarcode.TabIndex = 5;
        // 
        // txtOrderNo
        // 
        txtOrderNo.BorderColor = Color.FromArgb(91, 155, 213);
        txtOrderNo.Dock = DockStyle.Fill;
        txtOrderNo.Font = new Font("Segoe UI", 18F);
        txtOrderNo.Location = new Point(275, 122);
        txtOrderNo.Margin = new Padding(4, 14, 4, 14);
        txtOrderNo.Name = "txtOrderNo";
        txtOrderNo.Radius = 8;
        txtOrderNo.Size = new Size(692, 80);
        txtOrderNo.TabIndex = 6;
        // 
        // txtCustomerName
        // 
        txtCustomerName.BorderColor = Color.FromArgb(91, 155, 213);
        txtCustomerName.Dock = DockStyle.Fill;
        txtCustomerName.Font = new Font("Segoe UI", 18F);
        txtCustomerName.Location = new Point(275, 230);
        txtCustomerName.Margin = new Padding(4, 14, 4, 14);
        txtCustomerName.Name = "txtCustomerName";
        txtCustomerName.Radius = 8;
        txtCustomerName.Size = new Size(692, 80);
        txtCustomerName.TabIndex = 7;
        // 
        // txtType
        // 
        txtType.BorderColor = Color.FromArgb(91, 155, 213);
        txtType.Dock = DockStyle.Fill;
        txtType.Font = new Font("Segoe UI", 18F);
        txtType.Location = new Point(275, 338);
        txtType.Margin = new Padding(4, 14, 4, 14);
        txtType.Name = "txtType";
        txtType.Radius = 8;
        txtType.Size = new Size(692, 80);
        txtType.TabIndex = 8;
        // 
        // txtQty
        // 
        txtQty.BorderColor = Color.FromArgb(91, 155, 213);
        txtQty.Dock = DockStyle.Fill;
        txtQty.Font = new Font("Segoe UI", 18F);
        txtQty.Location = new Point(275, 446);
        txtQty.Margin = new Padding(4, 14, 4, 14);
        txtQty.Name = "txtQty";
        txtQty.Radius = 8;
        txtQty.Size = new Size(692, 80);
        txtQty.TabIndex = 9;
        // 
        // flpActions
        // 
        flpActions.Anchor = AnchorStyles.None;
        flpActions.AutoSize = true;
        flpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flpActions.BackColor = Color.FromArgb(91, 155, 213);
        flpActions.Controls.Add(btnConfirm);
        flpActions.Controls.Add(btnCancel);
        flpActions.Location = new Point(604, 1373);
        flpActions.Margin = new Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new Size(854, 117);
        flpActions.TabIndex = 3;
        flpActions.WrapContents = false;
        // 
        // btnConfirm
        // 
        btnConfirm.Font = new Font("Segoe UI", 28.5F);
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
        // btnCancel
        // 
        btnCancel.Font = new Font("Segoe UI", 28.5F);
        btnCancel.ForeColor = Color.White;
        btnCancel.Location = new Point(479, 0);
        btnCancel.Margin = new Padding(52, 0, 0, 0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 12;
        btnCancel.Size = new Size(375, 117);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Error;
        // 
        // ScanBarcodeUserControl
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpScanBarcodeRoot);
        Font = new Font("Segoe UI", 13.5F);
        Margin = new Padding(4, 4, 4, 4);
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
    private AntdUI.Label lblOrderNo;
    private AntdUI.Label lblCustomerName;
    private AntdUI.Label lblType;
    private AntdUI.Label lblQty;
    private AntdUI.Input txtBarcode;
    private AntdUI.Input txtOrderNo;
    private AntdUI.Input txtCustomerName;
    private AntdUI.Input txtType;
    private AntdUI.Input txtQty;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnConfirm;
    private AntdUI.Button btnCancel;
}
