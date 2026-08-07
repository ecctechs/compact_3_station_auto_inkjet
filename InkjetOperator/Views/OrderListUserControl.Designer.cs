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
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
        tlpOrderListRoot.Size = new System.Drawing.Size(1100, 860);
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
        pnlTableContainer.Size = new System.Drawing.Size(1030, 706);
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
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpTableInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTableInner.Size = new System.Drawing.Size(1026, 702);
        tlpTableInner.TabIndex = 0;
        //
        // flpTabs
        //
        flpTabs.BackColor = System.Drawing.Color.White;
        flpTabs.Controls.Add(btnTabList);
        flpTabs.Controls.Add(btnTabHistory);
        flpTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        flpTabs.Location = new System.Drawing.Point(0, 0);
        flpTabs.Margin = new System.Windows.Forms.Padding(0);
        flpTabs.Name = "flpTabs";
        flpTabs.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
        flpTabs.Size = new System.Drawing.Size(1026, 50);
        flpTabs.TabIndex = 0;
        flpTabs.WrapContents = false;
        //
        // btnTabList
        //
        btnTabList.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnTabList.ForeColor = System.Drawing.Color.White;
        btnTabList.Location = new System.Drawing.Point(9, 9);
        btnTabList.Margin = new System.Windows.Forms.Padding(3);
        btnTabList.Name = "btnTabList";
        btnTabList.Radius = 6;
        btnTabList.Size = new System.Drawing.Size(110, 36);
        btnTabList.TabIndex = 0;
        btnTabList.Text = "List";
        btnTabList.Type = AntdUI.TTypeMini.Primary;
        //
        // btnTabHistory
        //
        btnTabHistory.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnTabHistory.BorderWidth = 1F;
        btnTabHistory.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnTabHistory.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnTabHistory.Location = new System.Drawing.Point(125, 9);
        btnTabHistory.Margin = new System.Windows.Forms.Padding(3);
        btnTabHistory.Name = "btnTabHistory";
        btnTabHistory.Radius = 6;
        btnTabHistory.Size = new System.Drawing.Size(110, 36);
        btnTabHistory.TabIndex = 1;
        btnTabHistory.Text = "History";
        btnTabHistory.Type = AntdUI.TTypeMini.Default;
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
        tblOrders.Font = new System.Drawing.Font("Segoe UI", 13F);
        tblOrders.Location = new System.Drawing.Point(0, 54);
        tblOrders.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
        tblOrders.Name = "tblOrders";
        tblOrders.Radius = 0;
        tblOrders.RowHeight = 68;
        tblOrders.Size = new System.Drawing.Size(1026, 648);
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
        flpActions.Size = new System.Drawing.Size(176, 60);
        flpActions.TabIndex = 1;
        //
        // btnStart
        //
        btnStart.DefaultBack = System.Drawing.Color.FromArgb(168, 201, 122);
        btnStart.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnStart.ForeColor = System.Drawing.Color.White;
        btnStart.Location = new System.Drawing.Point(0, 0);
        btnStart.Margin = new System.Windows.Forms.Padding(0);
        btnStart.Name = "btnStart";
        btnStart.Radius = 8;
        btnStart.Size = new System.Drawing.Size(176, 60);
        btnStart.TabIndex = 0;
        btnStart.Text = "Start";
        btnStart.Type = AntdUI.TTypeMini.Default;
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
    private AntdUI.Table tblOrders;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnStart;
}
