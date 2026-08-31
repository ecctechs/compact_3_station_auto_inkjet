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
        tlpScanBarcodeRoot = new System.Windows.Forms.TableLayoutPanel();
        lblScanBarcodeTitle = new AntdUI.Label();
        tlpBarcodeCenter = new System.Windows.Forms.TableLayoutPanel();
        pnlBarcodeContainer = new AntdUI.Panel();
        picBarcode = new System.Windows.Forms.PictureBox();
        tlpOrderCenter = new System.Windows.Forms.TableLayoutPanel();
        pnlOrderInformation = new AntdUI.Panel();
        tlpOrderInformation = new System.Windows.Forms.TableLayoutPanel();
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
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
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
        tlpScanBarcodeRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpScanBarcodeRoot.ColumnCount = 1;
        tlpScanBarcodeRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpScanBarcodeRoot.Controls.Add(lblScanBarcodeTitle, 0, 0);
        tlpScanBarcodeRoot.Controls.Add(tlpBarcodeCenter, 0, 1);
        tlpScanBarcodeRoot.Controls.Add(tlpOrderCenter, 0, 2);
        tlpScanBarcodeRoot.Controls.Add(flpActions, 0, 3);
        tlpScanBarcodeRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpScanBarcodeRoot.Location = new System.Drawing.Point(0, 0);
        tlpScanBarcodeRoot.Name = "tlpScanBarcodeRoot";
        tlpScanBarcodeRoot.Padding = new System.Windows.Forms.Padding(40);
        tlpScanBarcodeRoot.RowCount = 4;
        tlpScanBarcodeRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
        tlpScanBarcodeRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21F));
        tlpScanBarcodeRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46F));
        tlpScanBarcodeRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18F));
        tlpScanBarcodeRoot.Size = new System.Drawing.Size(1375, 1075);
        tlpScanBarcodeRoot.TabIndex = 0;
        //
        // lblScanBarcodeTitle
        //
        lblScanBarcodeTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblScanBarcodeTitle.Font = new System.Drawing.Font("Segoe UI", 35F, System.Drawing.FontStyle.Bold);
        lblScanBarcodeTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblScanBarcodeTitle.Location = new System.Drawing.Point(43, 43);
        lblScanBarcodeTitle.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
        lblScanBarcodeTitle.Name = "lblScanBarcodeTitle";
        lblScanBarcodeTitle.Size = new System.Drawing.Size(1268, 131);
        lblScanBarcodeTitle.TabIndex = 0;
        lblScanBarcodeTitle.Text = "Scan Barcode";
        lblScanBarcodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // tlpBarcodeCenter
        //
        tlpBarcodeCenter.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpBarcodeCenter.ColumnCount = 1;
        tlpBarcodeCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBarcodeCenter.Controls.Add(pnlBarcodeContainer, 0, 0);
        tlpBarcodeCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpBarcodeCenter.Location = new System.Drawing.Point(40, 166);
        tlpBarcodeCenter.Margin = new System.Windows.Forms.Padding(0);
        tlpBarcodeCenter.Name = "tlpBarcodeCenter";
        tlpBarcodeCenter.RowCount = 1;
        tlpBarcodeCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBarcodeCenter.Size = new System.Drawing.Size(1275, 198);
        tlpBarcodeCenter.TabIndex = 1;
        //
        // pnlBarcodeContainer
        //
        pnlBarcodeContainer.Anchor = System.Windows.Forms.AnchorStyles.None;
        pnlBarcodeContainer.Back = System.Drawing.Color.FromArgb(220, 233, 245);
        pnlBarcodeContainer.BorderColor = System.Drawing.Color.White;
        pnlBarcodeContainer.BorderWidth = 3F;
        pnlBarcodeContainer.Controls.Add(picBarcode);
        pnlBarcodeContainer.Location = new System.Drawing.Point(340, 0);
        pnlBarcodeContainer.Name = "pnlBarcodeContainer";
        pnlBarcodeContainer.Padding = new System.Windows.Forms.Padding(18);
        pnlBarcodeContainer.Radius = 24;
        pnlBarcodeContainer.Size = new System.Drawing.Size(425, 198);
        pnlBarcodeContainer.TabIndex = 0;
        //
        // picBarcode
        //
        picBarcode.BackColor = System.Drawing.Color.Transparent;
        picBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
        picBarcode.Image = (System.Drawing.Image)resources.GetObject("picBarcode.Image");
        picBarcode.Location = new System.Drawing.Point(18, 18);
        picBarcode.Name = "picBarcode";
        picBarcode.Size = new System.Drawing.Size(380, 152);
        picBarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picBarcode.TabIndex = 0;
        picBarcode.TabStop = false;
        //
        // tlpOrderCenter
        //
        tlpOrderCenter.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpOrderCenter.ColumnCount = 3;
        tlpOrderCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
        tlpOrderCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
        tlpOrderCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
        tlpOrderCenter.Controls.Add(pnlOrderInformation, 1, 0);
        tlpOrderCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOrderCenter.Location = new System.Drawing.Point(40, 324);
        tlpOrderCenter.Margin = new System.Windows.Forms.Padding(0);
        tlpOrderCenter.Name = "tlpOrderCenter";
        tlpOrderCenter.RowCount = 1;
        tlpOrderCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderCenter.Size = new System.Drawing.Size(1275, 475);
        tlpOrderCenter.TabIndex = 2;
        //
        // pnlOrderInformation
        //
        pnlOrderInformation.Back = System.Drawing.Color.White;
        pnlOrderInformation.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlOrderInformation.BorderWidth = 4F;
        pnlOrderInformation.Controls.Add(tlpOrderInformation);
        pnlOrderInformation.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlOrderInformation.Location = new System.Drawing.Point(156, 3);
        pnlOrderInformation.Margin = new System.Windows.Forms.Padding(3);
        pnlOrderInformation.Name = "pnlOrderInformation";
        pnlOrderInformation.Padding = new System.Windows.Forms.Padding(32);
        pnlOrderInformation.Radius = 22;
        pnlOrderInformation.Size = new System.Drawing.Size(885, 468);
        pnlOrderInformation.TabIndex = 0;
        //
        // tlpOrderInformation
        //
        tlpOrderInformation.BackColor = System.Drawing.Color.White;
        tlpOrderInformation.ColumnCount = 2;
        tlpOrderInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
        tlpOrderInformation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72F));
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
        tlpOrderInformation.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOrderInformation.Location = new System.Drawing.Point(32, 32);
        tlpOrderInformation.Name = "tlpOrderInformation";
        tlpOrderInformation.RowCount = 5;
        tlpOrderInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        tlpOrderInformation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
        tlpOrderInformation.Size = new System.Drawing.Size(805, 388);
        tlpOrderInformation.TabIndex = 0;
        //
        // lblBarcode
        //
        lblBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
        lblBarcode.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblBarcode.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblBarcode.Location = new System.Drawing.Point(3, 0);
        lblBarcode.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
        lblBarcode.Name = "lblBarcode";
        lblBarcode.Size = new System.Drawing.Size(196, 78);
        lblBarcode.TabIndex = 0;
        lblBarcode.Text = "Barcode :";
        lblBarcode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblOrderNo
        //
        lblOrderNo.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOrderNo.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblOrderNo.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblOrderNo.Location = new System.Drawing.Point(3, 62);
        lblOrderNo.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
        lblOrderNo.Name = "lblOrderNo";
        lblOrderNo.Size = new System.Drawing.Size(196, 78);
        lblOrderNo.TabIndex = 1;
        lblOrderNo.Text = "Order No :";
        lblOrderNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblCustomerName
        //
        lblCustomerName.Dock = System.Windows.Forms.DockStyle.Fill;
        lblCustomerName.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblCustomerName.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblCustomerName.Location = new System.Drawing.Point(3, 124);
        lblCustomerName.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
        lblCustomerName.Name = "lblCustomerName";
        lblCustomerName.Size = new System.Drawing.Size(196, 78);
        lblCustomerName.TabIndex = 2;
        lblCustomerName.Text = "Customer Name :";
        lblCustomerName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblType
        //
        lblType.Dock = System.Windows.Forms.DockStyle.Fill;
        lblType.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblType.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblType.Location = new System.Drawing.Point(3, 186);
        lblType.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
        lblType.Name = "lblType";
        lblType.Size = new System.Drawing.Size(196, 78);
        lblType.TabIndex = 3;
        lblType.Text = "Type :";
        lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblQty
        //
        lblQty.Dock = System.Windows.Forms.DockStyle.Fill;
        lblQty.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblQty.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblQty.Location = new System.Drawing.Point(3, 248);
        lblQty.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
        lblQty.Name = "lblQty";
        lblQty.Size = new System.Drawing.Size(196, 78);
        lblQty.TabIndex = 4;
        lblQty.Text = "Qty :";
        lblQty.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // txtBarcode
        //
        txtBarcode.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtBarcode.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtBarcode.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtBarcode.Location = new System.Drawing.Point(183, 8);
        txtBarcode.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtBarcode.Name = "txtBarcode";
        txtBarcode.PlaceholderText = "Scan or type barcode...";
        txtBarcode.Radius = 8;
        txtBarcode.Size = new System.Drawing.Size(572, 58);
        txtBarcode.TabIndex = 5;
        //
        // txtOrderNo
        //
        txtOrderNo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtOrderNo.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtOrderNo.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtOrderNo.Location = new System.Drawing.Point(183, 70);
        txtOrderNo.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtOrderNo.Name = "txtOrderNo";
        txtOrderNo.Radius = 8;
        txtOrderNo.Size = new System.Drawing.Size(572, 58);
        txtOrderNo.TabIndex = 6;
        //
        // txtCustomerName
        //
        txtCustomerName.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtCustomerName.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtCustomerName.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtCustomerName.Location = new System.Drawing.Point(183, 132);
        txtCustomerName.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtCustomerName.Name = "txtCustomerName";
        txtCustomerName.Radius = 8;
        txtCustomerName.Size = new System.Drawing.Size(572, 58);
        txtCustomerName.TabIndex = 7;
        //
        // txtType
        //
        txtType.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtType.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtType.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtType.Location = new System.Drawing.Point(183, 194);
        txtType.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtType.Name = "txtType";
        txtType.Radius = 8;
        txtType.Size = new System.Drawing.Size(572, 58);
        txtType.TabIndex = 8;
        //
        // txtQty
        //
        txtQty.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtQty.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtQty.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtQty.Location = new System.Drawing.Point(183, 256);
        txtQty.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtQty.Name = "txtQty";
        txtQty.Radius = 8;
        txtQty.Size = new System.Drawing.Size(572, 58);
        txtQty.TabIndex = 9;
        //
        // flpActions
        //
        flpActions.Anchor = System.Windows.Forms.AnchorStyles.None;
        flpActions.AutoSize = true;
        flpActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        flpActions.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        flpActions.Controls.Add(btnConfirm);
        flpActions.Controls.Add(btnCancel);
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpActions.Location = new System.Drawing.Point(305, 731);
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(612, 78);
        flpActions.TabIndex = 3;
        flpActions.WrapContents = false;
        //
        // btnConfirm
        //
        btnConfirm.Font = new System.Drawing.Font("Segoe UI", 19F);
        btnConfirm.ForeColor = System.Drawing.Color.White;
        btnConfirm.Location = new System.Drawing.Point(0, 0);
        btnConfirm.Margin = new System.Windows.Forms.Padding(0, 0, 35, 0);
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Radius = 12;
        btnConfirm.Size = new System.Drawing.Size(250, 78);
        btnConfirm.TabIndex = 0;
        btnConfirm.Text = "OK";
        btnConfirm.Type = AntdUI.TTypeMini.Success;
        //
        // btnCancel
        //
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 19F);
        btnCancel.ForeColor = System.Drawing.Color.White;
        btnCancel.Location = new System.Drawing.Point(255, 0);
        btnCancel.Margin = new System.Windows.Forms.Padding(35, 0, 0, 0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 12;
        btnCancel.Size = new System.Drawing.Size(250, 78);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Error;
        //
        // ScanBarcodeUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpScanBarcodeRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "ScanBarcodeUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpScanBarcodeRoot.ResumeLayout(false);
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
