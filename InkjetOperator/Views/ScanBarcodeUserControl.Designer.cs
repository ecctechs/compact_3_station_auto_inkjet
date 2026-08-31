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
        tlpScanBarcodeRoot.Name = "tlpScanBarcodeRoot";
        tlpScanBarcodeRoot.Padding = new Padding(40);
        tlpScanBarcodeRoot.RowCount = 4;
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        tlpScanBarcodeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 16F));
        tlpScanBarcodeRoot.Size = new Size(1375, 1075);
        tlpScanBarcodeRoot.TabIndex = 0;
        // 
        // lblScanBarcodeTitle
        // 
        lblScanBarcodeTitle.Dock = DockStyle.Fill;
        lblScanBarcodeTitle.Font = new Font("Segoe UI", 35F, FontStyle.Bold);
        lblScanBarcodeTitle.ForeColor = Color.FromArgb(17, 17, 17);
        lblScanBarcodeTitle.Location = new Point(40, 44);
        lblScanBarcodeTitle.Margin = new Padding(0, 4, 0, 0);
        lblScanBarcodeTitle.Name = "lblScanBarcodeTitle";
        lblScanBarcodeTitle.Size = new Size(1295, 137);
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
        tlpBarcodeCenter.Location = new Point(40, 189);
        tlpBarcodeCenter.Margin = new Padding(0);
        tlpBarcodeCenter.Name = "tlpBarcodeCenter";
        tlpBarcodeCenter.RowCount = 1;
        tlpBarcodeCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpBarcodeCenter.Size = new Size(1295, 208);
        tlpBarcodeCenter.TabIndex = 1;
        // 
        // pnlBarcodeContainer
        // 
        pnlBarcodeContainer.Anchor = AnchorStyles.None;
        pnlBarcodeContainer.Back = Color.FromArgb(220, 233, 245);
        pnlBarcodeContainer.BorderColor = Color.White;
        pnlBarcodeContainer.BorderWidth = 3F;
        pnlBarcodeContainer.Controls.Add(picBarcode);
        pnlBarcodeContainer.Location = new Point(435, 5);
        pnlBarcodeContainer.Name = "pnlBarcodeContainer";
        pnlBarcodeContainer.Padding = new Padding(16);
        pnlBarcodeContainer.Radius = 24;
        pnlBarcodeContainer.Size = new Size(560, 180);
        pnlBarcodeContainer.TabIndex = 0;
        // 
        // picBarcode
        // 
        picBarcode.BackColor = Color.Transparent;
        picBarcode.Dock = DockStyle.Fill;
        picBarcode.Image = (Image)resources.GetObject("picBarcode.Image");
        picBarcode.Location = new Point(21, 21);
        picBarcode.Name = "picBarcode";
        picBarcode.Size = new Size(383, 156);
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
        tlpOrderCenter.Location = new Point(40, 397);
        tlpOrderCenter.Margin = new Padding(0);
        tlpOrderCenter.Name = "tlpOrderCenter";
        tlpOrderCenter.RowCount = 1;
        tlpOrderCenter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpOrderCenter.Size = new Size(1295, 457);
        tlpOrderCenter.TabIndex = 2;
        // 
        // pnlOrderInformation
        // 
        pnlOrderInformation.Back = Color.White;
        pnlOrderInformation.BorderColor = Color.FromArgb(36, 71, 101);
        pnlOrderInformation.BorderWidth = 4F;
        pnlOrderInformation.Controls.Add(tlpOrderInformation);
        pnlOrderInformation.Dock = DockStyle.Fill;
        pnlOrderInformation.Location = new Point(197, 3);
        pnlOrderInformation.Name = "pnlOrderInformation";
        pnlOrderInformation.Padding = new Padding(32);
        pnlOrderInformation.Radius = 22;
        pnlOrderInformation.Size = new Size(900, 451);
        pnlOrderInformation.TabIndex = 0;
        // 
        // tlpOrderInformation
        // 
        tlpOrderInformation.BackColor = Color.White;
        tlpOrderInformation.ColumnCount = 2;
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
        tlpOrderInformation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78F));
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
        tlpOrderInformation.Location = new Point(36, 36);
        tlpOrderInformation.Name = "tlpOrderInformation";
        tlpOrderInformation.RowCount = 5;
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        tlpOrderInformation.Size = new Size(828, 379);
        tlpOrderInformation.TabIndex = 0;
        // 
        // lblBarcode
        // 
        lblBarcode.Dock = DockStyle.Fill;
        lblBarcode.Font = new Font("Segoe UI", 15F);
        lblBarcode.ForeColor = Color.FromArgb(17, 17, 17);
        lblBarcode.Location = new Point(3, 0);
        lblBarcode.Margin = new Padding(3, 0, 20, 0);
        lblBarcode.TextAlign = ContentAlignment.MiddleRight;
        lblBarcode.Name = "lblBarcode";
        lblBarcode.Size = new Size(208, 75);
        lblBarcode.TabIndex = 0;
        lblBarcode.Text = "Barcode :";
        // 
        // lblOrderNo
        // 
        lblOrderNo.Dock = DockStyle.Fill;
        lblOrderNo.Font = new Font("Segoe UI", 15F);
        lblOrderNo.ForeColor = Color.FromArgb(17, 17, 17);
        lblOrderNo.Location = new Point(3, 75);
        lblOrderNo.Margin = new Padding(3, 0, 20, 0);
        lblOrderNo.TextAlign = ContentAlignment.MiddleRight;
        lblOrderNo.Name = "lblOrderNo";
        lblOrderNo.Size = new Size(208, 75);
        lblOrderNo.TabIndex = 1;
        lblOrderNo.Text = "Order No :";
        // 
        // lblCustomerName
        // 
        lblCustomerName.Dock = DockStyle.Fill;
        lblCustomerName.Font = new Font("Segoe UI", 15F);
        lblCustomerName.ForeColor = Color.FromArgb(17, 17, 17);
        lblCustomerName.Location = new Point(3, 150);
        lblCustomerName.Margin = new Padding(3, 0, 20, 0);
        lblCustomerName.TextAlign = ContentAlignment.MiddleRight;
        lblCustomerName.Name = "lblCustomerName";
        lblCustomerName.Size = new Size(208, 75);
        lblCustomerName.TabIndex = 2;
        lblCustomerName.Text = "Customer Name :";
        // 
        // lblType
        // 
        lblType.Dock = DockStyle.Fill;
        lblType.Font = new Font("Segoe UI", 15F);
        lblType.ForeColor = Color.FromArgb(17, 17, 17);
        lblType.Location = new Point(3, 225);
        lblType.Margin = new Padding(3, 0, 20, 0);
        lblType.TextAlign = ContentAlignment.MiddleRight;
        lblType.Name = "lblType";
        lblType.Size = new Size(208, 75);
        lblType.TabIndex = 3;
        lblType.Text = "Type :";
        // 
        // lblQty
        // 
        lblQty.Dock = DockStyle.Fill;
        lblQty.Font = new Font("Segoe UI", 15F);
        lblQty.ForeColor = Color.FromArgb(17, 17, 17);
        lblQty.Location = new Point(3, 300);
        lblQty.Margin = new Padding(3, 0, 20, 0);
        lblQty.TextAlign = ContentAlignment.MiddleRight;
        lblQty.Name = "lblQty";
        lblQty.Size = new Size(208, 79);
        lblQty.TabIndex = 4;
        lblQty.Text = "Qty :";
        // 
        // txtBarcode
        // 
        txtBarcode.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtBarcode.BorderColor = Color.FromArgb(91, 155, 213);
        txtBarcode.Font = new Font("Segoe UI", 15F);
        txtBarcode.Location = new Point(234, 8);
        txtBarcode.Margin = new Padding(3, 8, 3, 8);
        txtBarcode.Name = "txtBarcode";
        txtBarcode.PlaceholderText = "Scan or type barcode...";
        txtBarcode.Radius = 8;
        txtBarcode.Size = new Size(591, 58);
        txtBarcode.TabIndex = 5;
        // 
        // txtOrderNo
        // 
        txtOrderNo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtOrderNo.BorderColor = Color.FromArgb(91, 155, 213);
        txtOrderNo.Font = new Font("Segoe UI", 15F);
        txtOrderNo.Location = new Point(234, 83);
        txtOrderNo.Margin = new Padding(3, 8, 3, 8);
        txtOrderNo.Name = "txtOrderNo";
        txtOrderNo.Radius = 8;
        txtOrderNo.Size = new Size(591, 58);
        txtOrderNo.TabIndex = 6;
        // 
        // txtCustomerName
        // 
        txtCustomerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtCustomerName.BorderColor = Color.FromArgb(91, 155, 213);
        txtCustomerName.Font = new Font("Segoe UI", 15F);
        txtCustomerName.Location = new Point(234, 158);
        txtCustomerName.Margin = new Padding(3, 8, 3, 8);
        txtCustomerName.Name = "txtCustomerName";
        txtCustomerName.Radius = 8;
        txtCustomerName.Size = new Size(591, 58);
        txtCustomerName.TabIndex = 7;
        // 
        // txtType
        // 
        txtType.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtType.BorderColor = Color.FromArgb(91, 155, 213);
        txtType.Font = new Font("Segoe UI", 15F);
        txtType.Location = new Point(234, 233);
        txtType.Margin = new Padding(3, 8, 3, 8);
        txtType.Name = "txtType";
        txtType.Radius = 8;
        txtType.Size = new Size(591, 58);
        txtType.TabIndex = 8;
        // 
        // txtQty
        // 
        txtQty.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtQty.BorderColor = Color.FromArgb(91, 155, 213);
        txtQty.Font = new Font("Segoe UI", 15F);
        txtQty.Location = new Point(234, 310);
        txtQty.Margin = new Padding(3, 8, 3, 8);
        txtQty.Name = "txtQty";
        txtQty.Radius = 8;
        txtQty.Size = new Size(591, 58);
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
        flpActions.Location = new Point(402, 905);
        flpActions.Margin = new Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new Size(570, 78);
        flpActions.TabIndex = 3;
        flpActions.WrapContents = false;
        // 
        // btnConfirm
        // 
        btnConfirm.Font = new Font("Segoe UI", 19F);
        btnConfirm.ForeColor = Color.White;
        btnConfirm.Location = new Point(0, 0);
        btnConfirm.Margin = new Padding(0, 0, 35, 0);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Radius = 12;
        btnConfirm.Size = new Size(250, 78);
        btnConfirm.TabIndex = 0;
        btnConfirm.Text = "OK";
        btnConfirm.Type = AntdUI.TTypeMini.Success;
        // 
        // btnCancel
        // 
        btnCancel.Font = new Font("Segoe UI", 19F);
        btnCancel.ForeColor = Color.White;
        btnCancel.Location = new Point(320, 0);
        btnCancel.Margin = new Padding(35, 0, 0, 0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 12;
        btnCancel.Size = new Size(250, 78);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Error;
        // 
        // ScanBarcodeUserControl
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpScanBarcodeRoot);
        MinimumSize = new Size(820, 680);
        Name = "ScanBarcodeUserControl";
        Size = new Size(1375, 1075);
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
