namespace InkjetOperator.Views;

partial class OrderListUserControl
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
        tlpOrderListRoot = new System.Windows.Forms.TableLayoutPanel();
        pnlTableContainer = new AntdUI.Panel();
        tlpTableInner = new System.Windows.Forms.TableLayoutPanel();
        flpTabs = new System.Windows.Forms.FlowLayoutPanel();
        btnTabList = new AntdUI.Button();
        btnTabHistory = new AntdUI.Button();
        lblDateFilter = new System.Windows.Forms.Label();
        dtpHistoryRange = new AntdUI.DatePickerRange();
        btnClearDate = new AntdUI.Button();
        tblOrders = new AntdUI.Table();
        tlpBottom = new System.Windows.Forms.TableLayoutPanel();
        pnlPreview = new AntdUI.Panel();
        tlpPreview = new System.Windows.Forms.TableLayoutPanel();
        lblPreviewTitle = new System.Windows.Forms.Label();
        tlpPreviewSlots = new System.Windows.Forms.TableLayoutPanel();
        lblPrevPlateCaption = new System.Windows.Forms.Label();
        lblPrevShimCaption = new System.Windows.Forms.Label();
        picPrevPlate = new System.Windows.Forms.PictureBox();
        picPrevShim = new System.Windows.Forms.PictureBox();
        pnlProcessing = new AntdUI.Panel();
        tlpProcessing = new System.Windows.Forms.TableLayoutPanel();
        lblProcessingTitle = new System.Windows.Forms.Label();
        tlpProcessingSlots = new System.Windows.Forms.TableLayoutPanel();
        lblProcPlateCaption = new System.Windows.Forms.Label();
        lblProcShimCaption = new System.Windows.Forms.Label();
        picProcPlate = new System.Windows.Forms.PictureBox();
        picProcShim = new System.Windows.Forms.PictureBox();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnStart = new AntdUI.Button();
        tlpOrderListRoot.SuspendLayout();
        pnlTableContainer.SuspendLayout();
        tlpTableInner.SuspendLayout();
        flpTabs.SuspendLayout();
        tlpBottom.SuspendLayout();
        pnlPreview.SuspendLayout();
        tlpPreview.SuspendLayout();
        tlpPreviewSlots.SuspendLayout();
        pnlProcessing.SuspendLayout();
        tlpProcessing.SuspendLayout();
        tlpProcessingSlots.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpOrderListRoot
        //
        tlpOrderListRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpOrderListRoot.ColumnCount = 1;
        tlpOrderListRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.Controls.Add(pnlTableContainer, 0, 0);
        tlpOrderListRoot.Controls.Add(tlpBottom, 0, 1);
        tlpOrderListRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOrderListRoot.Location = new System.Drawing.Point(0, 0);
        tlpOrderListRoot.Name = "tlpOrderListRoot";
        tlpOrderListRoot.Padding = new System.Windows.Forms.Padding(32);
        tlpOrderListRoot.RowCount = 2;
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 312F));
        tlpOrderListRoot.Size = new System.Drawing.Size(1375, 1075);
        tlpOrderListRoot.TabIndex = 0;
        //
        // pnlTableContainer
        //
        pnlTableContainer.Back = System.Drawing.Color.White;
        pnlTableContainer.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlTableContainer.BorderWidth = 2F;
        pnlTableContainer.Controls.Add(tlpTableInner);
        pnlTableContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlTableContainer.Location = new System.Drawing.Point(35, 35);
        pnlTableContainer.Name = "pnlTableContainer";
        pnlTableContainer.Padding = new System.Windows.Forms.Padding(2);
        pnlTableContainer.Radius = 12;
        pnlTableContainer.Size = new System.Drawing.Size(1288, 882);
        pnlTableContainer.TabIndex = 0;
        //
        // tlpTableInner
        //
        tlpTableInner.BackColor = System.Drawing.Color.White;
        tlpTableInner.ColumnCount = 1;
        tlpTableInner.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTableInner.Controls.Add(flpTabs, 0, 0);
        tlpTableInner.Controls.Add(tblOrders, 0, 1);
        tlpTableInner.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTableInner.Location = new System.Drawing.Point(2, 2);
        tlpTableInner.Name = "tlpTableInner";
        tlpTableInner.RowCount = 2;
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTableInner.Size = new System.Drawing.Size(1282, 878);
        tlpTableInner.TabIndex = 0;
        //
        // flpTabs
        //
        flpTabs.BackColor = System.Drawing.Color.White;
        flpTabs.Controls.Add(btnTabList);
        flpTabs.Controls.Add(btnTabHistory);
        flpTabs.Controls.Add(lblDateFilter);
        flpTabs.Controls.Add(dtpHistoryRange);
        flpTabs.Controls.Add(btnClearDate);
        flpTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        flpTabs.Location = new System.Drawing.Point(0, 0);
        flpTabs.Margin = new System.Windows.Forms.Padding(0);
        flpTabs.Name = "flpTabs";
        flpTabs.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
        flpTabs.Size = new System.Drawing.Size(1282, 62);
        flpTabs.TabIndex = 0;
        flpTabs.WrapContents = false;
        //
        // btnTabList
        //
        btnTabList.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnTabList.ForeColor = System.Drawing.Color.White;
        btnTabList.Location = new System.Drawing.Point(9, 9);
        btnTabList.Margin = new System.Windows.Forms.Padding(3);
        btnTabList.Name = "btnTabList";
        btnTabList.Radius = 6;
        btnTabList.Size = new System.Drawing.Size(138, 45);
        btnTabList.TabIndex = 0;
        btnTabList.Text = "List";
        btnTabList.Type = AntdUI.TTypeMini.Primary;
        //
        // btnTabHistory
        //
        btnTabHistory.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnTabHistory.BorderWidth = 1F;
        btnTabHistory.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnTabHistory.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnTabHistory.Location = new System.Drawing.Point(125, 9);
        btnTabHistory.Margin = new System.Windows.Forms.Padding(3);
        btnTabHistory.Name = "btnTabHistory";
        btnTabHistory.Radius = 6;
        btnTabHistory.Size = new System.Drawing.Size(138, 45);
        btnTabHistory.TabIndex = 1;
        btnTabHistory.Text = "History";
        btnTabHistory.Type = AntdUI.TTypeMini.Default;
        //
        // lblDateFilter
        //
        lblDateFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblDateFilter.AutoSize = true;
        lblDateFilter.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        lblDateFilter.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDateFilter.Margin = new System.Windows.Forms.Padding(20, 3, 4, 3);
        lblDateFilter.Name = "lblDateFilter";
        lblDateFilter.Size = new System.Drawing.Size(88, 45);
        lblDateFilter.TabIndex = 2;
        lblDateFilter.Text = "ช่วงวันที่ :";
        lblDateFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblDateFilter.Visible = false;
        //
        // dtpHistoryRange
        //
        dtpHistoryRange.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        dtpHistoryRange.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        dtpHistoryRange.Format = "dd/MM/yyyy";
        dtpHistoryRange.Margin = new System.Windows.Forms.Padding(3);
        dtpHistoryRange.Name = "dtpHistoryRange";
        dtpHistoryRange.PlaceholderStart = "จากวันที่";
        dtpHistoryRange.PlaceholderEnd = "ถึงวันที่";
        dtpHistoryRange.Radius = 6;
        dtpHistoryRange.Size = new System.Drawing.Size(312, 45);
        dtpHistoryRange.TabIndex = 3;
        dtpHistoryRange.Visible = false;
        //
        // btnClearDate
        //
        btnClearDate.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnClearDate.BorderWidth = 1F;
        btnClearDate.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnClearDate.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnClearDate.Margin = new System.Windows.Forms.Padding(3);
        btnClearDate.Name = "btnClearDate";
        btnClearDate.Radius = 6;
        btnClearDate.Size = new System.Drawing.Size(88, 45);
        btnClearDate.TabIndex = 4;
        btnClearDate.Text = "ล้าง";
        btnClearDate.Type = AntdUI.TTypeMini.Default;
        btnClearDate.Visible = false;
        //
        // tblOrders
        //
        tblOrders.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        tblOrders.Bordered = true;
        tblOrders.ColumnBack = System.Drawing.Color.FromArgb(30, 30, 30);
        tblOrders.ColumnFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        tblOrders.ColumnFore = System.Drawing.Color.White;
        tblOrders.Dock = System.Windows.Forms.DockStyle.Fill;
        tblOrders.EmptyText = "No orders";
        tblOrders.Font = new System.Drawing.Font("Segoe UI", 14F);
        tblOrders.Location = new System.Drawing.Point(0, 54);
        tblOrders.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
        tblOrders.Name = "tblOrders";
        tblOrders.Radius = 0;
        tblOrders.RowHeight = 85;
        tblOrders.Size = new System.Drawing.Size(1282, 810);
        tblOrders.TabIndex = 1;
        //
        // tlpBottom - Preview | Start | Processing
        //
        tlpBottom.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpBottom.ColumnCount = 3;
        tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
        tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpBottom.Controls.Add(pnlPreview, 0, 0);
        tlpBottom.Controls.Add(flpActions, 1, 0);
        tlpBottom.Controls.Add(pnlProcessing, 2, 0);
        tlpBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpBottom.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
        tlpBottom.Name = "tlpBottom";
        tlpBottom.RowCount = 1;
        tlpBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBottom.Size = new System.Drawing.Size(1295, 300);
        tlpBottom.TabIndex = 1;
        //
        // pnlPreview
        //
        pnlPreview.Back = System.Drawing.Color.White;
        pnlPreview.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlPreview.BorderWidth = 2F;
        pnlPreview.Controls.Add(tlpPreview);
        pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlPreview.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
        pnlPreview.Name = "pnlPreview";
        pnlPreview.Padding = new System.Windows.Forms.Padding(8);
        pnlPreview.Radius = 12;
        pnlPreview.Size = new System.Drawing.Size(515, 300);
        pnlPreview.TabIndex = 0;
        //
        // tlpPreview
        //
        tlpPreview.BackColor = System.Drawing.Color.White;
        tlpPreview.ColumnCount = 1;
        tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpPreview.Controls.Add(lblPreviewTitle, 0, 0);
        tlpPreview.Controls.Add(tlpPreviewSlots, 0, 1);
        tlpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpPreview.Margin = new System.Windows.Forms.Padding(0);
        tlpPreview.Name = "tlpPreview";
        tlpPreview.RowCount = 2;
        tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpPreview.Size = new System.Drawing.Size(495, 280);
        tlpPreview.TabIndex = 0;
        //
        // lblPreviewTitle
        //
        lblPreviewTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblPreviewTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblPreviewTitle.Name = "lblPreviewTitle";
        lblPreviewTitle.Size = new System.Drawing.Size(488, 35);
        lblPreviewTitle.TabIndex = 0;
        lblPreviewTitle.Text = "Preview";
        lblPreviewTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // tlpPreviewSlots - Plate | Shim
        //
        tlpPreviewSlots.BackColor = System.Drawing.Color.White;
        tlpPreviewSlots.ColumnCount = 2;
        tlpPreviewSlots.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpPreviewSlots.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpPreviewSlots.Controls.Add(lblPrevPlateCaption, 0, 0);
        tlpPreviewSlots.Controls.Add(lblPrevShimCaption, 1, 0);
        tlpPreviewSlots.Controls.Add(picPrevPlate, 0, 1);
        tlpPreviewSlots.Controls.Add(picPrevShim, 1, 1);
        tlpPreviewSlots.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpPreviewSlots.Margin = new System.Windows.Forms.Padding(0);
        tlpPreviewSlots.Name = "tlpPreviewSlots";
        tlpPreviewSlots.RowCount = 2;
        tlpPreviewSlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpPreviewSlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpPreviewSlots.Size = new System.Drawing.Size(495, 245);
        tlpPreviewSlots.TabIndex = 1;
        //
        // lblPrevPlateCaption
        //
        lblPrevPlateCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrevPlateCaption.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPrevPlateCaption.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrevPlateCaption.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblPrevPlateCaption.Name = "lblPrevPlateCaption";
        lblPrevPlateCaption.Size = new System.Drawing.Size(240, 42);
        lblPrevPlateCaption.TabIndex = 0;
        lblPrevPlateCaption.Text = "";
        lblPrevPlateCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblPrevShimCaption
        //
        lblPrevShimCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrevShimCaption.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPrevShimCaption.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrevShimCaption.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblPrevShimCaption.Name = "lblPrevShimCaption";
        lblPrevShimCaption.Size = new System.Drawing.Size(240, 42);
        lblPrevShimCaption.TabIndex = 1;
        lblPrevShimCaption.Text = "";
        lblPrevShimCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // picPrevPlate
        //
        picPrevPlate.BackColor = System.Drawing.Color.FromArgb(245, 249, 253);
        picPrevPlate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        picPrevPlate.Dock = System.Windows.Forms.DockStyle.Fill;
        picPrevPlate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        picPrevPlate.Name = "picPrevPlate";
        picPrevPlate.Size = new System.Drawing.Size(240, 199);
        picPrevPlate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picPrevPlate.TabIndex = 2;
        picPrevPlate.TabStop = false;
        //
        // picPrevShim
        //
        picPrevShim.BackColor = System.Drawing.Color.FromArgb(245, 249, 253);
        picPrevShim.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        picPrevShim.Dock = System.Windows.Forms.DockStyle.Fill;
        picPrevShim.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        picPrevShim.Name = "picPrevShim";
        picPrevShim.Size = new System.Drawing.Size(240, 199);
        picPrevShim.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picPrevShim.TabIndex = 3;
        picPrevShim.TabStop = false;
        //
        // pnlProcessing
        //
        pnlProcessing.Back = System.Drawing.Color.White;
        pnlProcessing.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlProcessing.BorderWidth = 2F;
        pnlProcessing.Controls.Add(tlpProcessing);
        pnlProcessing.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlProcessing.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
        pnlProcessing.Name = "pnlProcessing";
        pnlProcessing.Padding = new System.Windows.Forms.Padding(8);
        pnlProcessing.Radius = 12;
        pnlProcessing.Size = new System.Drawing.Size(515, 300);
        pnlProcessing.TabIndex = 0;
        //
        // tlpProcessing
        //
        tlpProcessing.BackColor = System.Drawing.Color.White;
        tlpProcessing.ColumnCount = 1;
        tlpProcessing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessing.Controls.Add(lblProcessingTitle, 0, 0);
        tlpProcessing.Controls.Add(tlpProcessingSlots, 0, 1);
        tlpProcessing.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpProcessing.Margin = new System.Windows.Forms.Padding(0);
        tlpProcessing.Name = "tlpProcessing";
        tlpProcessing.RowCount = 2;
        tlpProcessing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
        tlpProcessing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessing.Size = new System.Drawing.Size(495, 280);
        tlpProcessing.TabIndex = 0;
        //
        // lblProcessingTitle
        //
        lblProcessingTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcessingTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblProcessingTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblProcessingTitle.Name = "lblProcessingTitle";
        lblProcessingTitle.Size = new System.Drawing.Size(488, 35);
        lblProcessingTitle.TabIndex = 0;
        lblProcessingTitle.Text = "Processing";
        lblProcessingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // tlpProcessingSlots - Plate | Shim
        //
        tlpProcessingSlots.BackColor = System.Drawing.Color.White;
        tlpProcessingSlots.ColumnCount = 2;
        tlpProcessingSlots.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpProcessingSlots.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        tlpProcessingSlots.Controls.Add(lblProcPlateCaption, 0, 0);
        tlpProcessingSlots.Controls.Add(lblProcShimCaption, 1, 0);
        tlpProcessingSlots.Controls.Add(picProcPlate, 0, 1);
        tlpProcessingSlots.Controls.Add(picProcShim, 1, 1);
        tlpProcessingSlots.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpProcessingSlots.Margin = new System.Windows.Forms.Padding(0);
        tlpProcessingSlots.Name = "tlpProcessingSlots";
        tlpProcessingSlots.RowCount = 2;
        tlpProcessingSlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpProcessingSlots.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessingSlots.Size = new System.Drawing.Size(495, 245);
        tlpProcessingSlots.TabIndex = 1;
        //
        // lblProcPlateCaption
        //
        lblProcPlateCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcPlateCaption.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblProcPlateCaption.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblProcPlateCaption.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblProcPlateCaption.Name = "lblProcPlateCaption";
        lblProcPlateCaption.Size = new System.Drawing.Size(240, 42);
        lblProcPlateCaption.TabIndex = 0;
        lblProcPlateCaption.Text = "";
        lblProcPlateCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblProcShimCaption
        //
        lblProcShimCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcShimCaption.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblProcShimCaption.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblProcShimCaption.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblProcShimCaption.Name = "lblProcShimCaption";
        lblProcShimCaption.Size = new System.Drawing.Size(240, 42);
        lblProcShimCaption.TabIndex = 1;
        lblProcShimCaption.Text = "";
        lblProcShimCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // picProcPlate
        //
        picProcPlate.BackColor = System.Drawing.Color.FromArgb(245, 249, 253);
        picProcPlate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        picProcPlate.Dock = System.Windows.Forms.DockStyle.Fill;
        picProcPlate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        picProcPlate.Name = "picProcPlate";
        picProcPlate.Size = new System.Drawing.Size(240, 199);
        picProcPlate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picProcPlate.TabIndex = 2;
        picProcPlate.TabStop = false;
        //
        // picProcShim
        //
        picProcShim.BackColor = System.Drawing.Color.FromArgb(245, 249, 253);
        picProcShim.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        picProcShim.Dock = System.Windows.Forms.DockStyle.Fill;
        picProcShim.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        picProcShim.Name = "picProcShim";
        picProcShim.Size = new System.Drawing.Size(240, 199);
        picProcShim.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picProcShim.TabIndex = 3;
        picProcShim.TabStop = false;
        //
        // flpActions
        //
        flpActions.Anchor = System.Windows.Forms.AnchorStyles.None;
        flpActions.AutoSize = true;
        flpActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        flpActions.BackColor = System.Drawing.Color.Transparent;
        flpActions.Controls.Add(btnStart);
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(220, 75);
        flpActions.TabIndex = 1;
        //
        // btnStart
        //
        btnStart.Visible = false;
        btnStart.Size = new System.Drawing.Size(0, 0);
        btnStart.Name = "btnStart";
        btnStart.TabIndex = 0;
        //
        // OrderListUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpOrderListRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "OrderListUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpOrderListRoot.ResumeLayout(false);
        tlpOrderListRoot.PerformLayout();
        pnlTableContainer.ResumeLayout(false);
        tlpTableInner.ResumeLayout(false);
        flpTabs.ResumeLayout(false);
        tlpBottom.ResumeLayout(false);
        pnlPreview.ResumeLayout(false);
        tlpPreview.ResumeLayout(false);
        tlpPreviewSlots.ResumeLayout(false);
        pnlProcessing.ResumeLayout(false);
        tlpProcessing.ResumeLayout(false);
        tlpProcessingSlots.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOrderListRoot;
    private AntdUI.Panel pnlTableContainer;
    private System.Windows.Forms.TableLayoutPanel tlpTableInner;
    private System.Windows.Forms.FlowLayoutPanel flpTabs;
    private AntdUI.Button btnTabList;
    private AntdUI.Button btnTabHistory;
    private System.Windows.Forms.Label lblDateFilter;
    private AntdUI.DatePickerRange dtpHistoryRange;
    private AntdUI.Button btnClearDate;
    private AntdUI.Table tblOrders;
    private System.Windows.Forms.TableLayoutPanel tlpBottom;
    private AntdUI.Panel pnlPreview;
    private System.Windows.Forms.TableLayoutPanel tlpPreview;
    private System.Windows.Forms.Label lblPreviewTitle;
    private System.Windows.Forms.TableLayoutPanel tlpPreviewSlots;
    private System.Windows.Forms.Label lblPrevPlateCaption;
    private System.Windows.Forms.Label lblPrevShimCaption;
    private System.Windows.Forms.PictureBox picPrevPlate;
    private System.Windows.Forms.PictureBox picPrevShim;
    private AntdUI.Panel pnlProcessing;
    private System.Windows.Forms.TableLayoutPanel tlpProcessing;
    private System.Windows.Forms.Label lblProcessingTitle;
    private System.Windows.Forms.TableLayoutPanel tlpProcessingSlots;
    private System.Windows.Forms.Label lblProcPlateCaption;
    private System.Windows.Forms.Label lblProcShimCaption;
    private System.Windows.Forms.PictureBox picProcPlate;
    private System.Windows.Forms.PictureBox picProcShim;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnStart;
}
