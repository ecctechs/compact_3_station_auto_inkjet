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
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnStart = new AntdUI.Button();
        tlpOrderListRoot.SuspendLayout();
        pnlTableContainer.SuspendLayout();
        tlpTableInner.SuspendLayout();
        flpTabs.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpOrderListRoot
        //
        tlpOrderListRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpOrderListRoot.ColumnCount = 1;
        tlpOrderListRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.Controls.Add(pnlTableContainer, 0, 0);
        tlpOrderListRoot.Controls.Add(flpActions, 0, 1);
        tlpOrderListRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOrderListRoot.Location = new System.Drawing.Point(0, 0);
        tlpOrderListRoot.Name = "tlpOrderListRoot";
        tlpOrderListRoot.Padding = new System.Windows.Forms.Padding(32);
        tlpOrderListRoot.RowCount = 2;
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
        tlpOrderListRoot.Size = new System.Drawing.Size(1265, 989);
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
        pnlTableContainer.Size = new System.Drawing.Size(1184, 812);
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
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTableInner.Size = new System.Drawing.Size(1180, 807);
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
        flpTabs.Size = new System.Drawing.Size(1180, 57);
        flpTabs.TabIndex = 0;
        flpTabs.WrapContents = false;
        //
        // btnTabList
        //
        btnTabList.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
        btnTabList.ForeColor = System.Drawing.Color.White;
        btnTabList.Location = new System.Drawing.Point(9, 9);
        btnTabList.Margin = new System.Windows.Forms.Padding(3);
        btnTabList.Name = "btnTabList";
        btnTabList.Radius = 6;
        btnTabList.Size = new System.Drawing.Size(126, 41);
        btnTabList.TabIndex = 0;
        btnTabList.Text = "List";
        btnTabList.Type = AntdUI.TTypeMini.Primary;
        //
        // btnTabHistory
        //
        btnTabHistory.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnTabHistory.BorderWidth = 1F;
        btnTabHistory.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
        btnTabHistory.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnTabHistory.Location = new System.Drawing.Point(125, 9);
        btnTabHistory.Margin = new System.Windows.Forms.Padding(3);
        btnTabHistory.Name = "btnTabHistory";
        btnTabHistory.Radius = 6;
        btnTabHistory.Size = new System.Drawing.Size(126, 41);
        btnTabHistory.TabIndex = 1;
        btnTabHistory.Text = "History";
        btnTabHistory.Type = AntdUI.TTypeMini.Default;
        //
        // lblDateFilter
        //
        lblDateFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
        lblDateFilter.AutoSize = true;
        lblDateFilter.Font = new System.Drawing.Font("Segoe UI", 11.5F);
        lblDateFilter.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDateFilter.Margin = new System.Windows.Forms.Padding(20, 3, 4, 3);
        lblDateFilter.Name = "lblDateFilter";
        lblDateFilter.Size = new System.Drawing.Size(80, 41);
        lblDateFilter.TabIndex = 2;
        lblDateFilter.Text = "ช่วงวันที่ :";
        lblDateFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblDateFilter.Visible = false;
        //
        // dtpHistoryRange
        //
        dtpHistoryRange.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        dtpHistoryRange.Font = new System.Drawing.Font("Segoe UI", 11.5F);
        dtpHistoryRange.Format = "dd/MM/yyyy";
        dtpHistoryRange.Margin = new System.Windows.Forms.Padding(3);
        dtpHistoryRange.Name = "dtpHistoryRange";
        dtpHistoryRange.PlaceholderStart = "จากวันที่";
        dtpHistoryRange.PlaceholderEnd = "ถึงวันที่";
        dtpHistoryRange.Radius = 6;
        dtpHistoryRange.Size = new System.Drawing.Size(288, 41);
        dtpHistoryRange.TabIndex = 3;
        dtpHistoryRange.Visible = false;
        //
        // btnClearDate
        //
        btnClearDate.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnClearDate.BorderWidth = 1F;
        btnClearDate.Font = new System.Drawing.Font("Segoe UI", 11.5F);
        btnClearDate.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnClearDate.Margin = new System.Windows.Forms.Padding(3);
        btnClearDate.Name = "btnClearDate";
        btnClearDate.Radius = 6;
        btnClearDate.Size = new System.Drawing.Size(80, 41);
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
        tblOrders.ColumnFont = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
        tblOrders.ColumnFore = System.Drawing.Color.White;
        tblOrders.Dock = System.Windows.Forms.DockStyle.Fill;
        tblOrders.EmptyText = "No orders";
        tblOrders.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        tblOrders.Location = new System.Drawing.Point(0, 54);
        tblOrders.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
        tblOrders.Name = "tblOrders";
        tblOrders.Radius = 0;
        tblOrders.RowHeight = 78;
        tblOrders.Size = new System.Drawing.Size(1180, 745);
        tblOrders.TabIndex = 1;
        //
        // flpActions
        //
        flpActions.Anchor = System.Windows.Forms.AnchorStyles.None;
        flpActions.AutoSize = true;
        flpActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        flpActions.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        flpActions.Controls.Add(btnStart);
        flpActions.Location = new System.Drawing.Point(462, 760);
        flpActions.Margin = new System.Windows.Forms.Padding(0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(202, 69);
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
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnStart;
}
