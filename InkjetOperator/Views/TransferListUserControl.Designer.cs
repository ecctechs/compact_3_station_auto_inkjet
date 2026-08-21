namespace InkjetOperator.Views;

partial class TransferListUserControl
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
        tlpTransferRoot = new System.Windows.Forms.TableLayoutPanel();
        pnlTransferContainer = new AntdUI.Panel();
        tlpTransferInner = new System.Windows.Forms.TableLayoutPanel();
        flpTabs = new System.Windows.Forms.FlowLayoutPanel();
        btnTabPending = new AntdUI.Button();
        btnTabSent = new AntdUI.Button();
        tblTransfer = new AntdUI.Table();
        tlpTransferRoot.SuspendLayout();
        pnlTransferContainer.SuspendLayout();
        tlpTransferInner.SuspendLayout();
        flpTabs.SuspendLayout();
        SuspendLayout();
        //
        // tlpTransferRoot
        //
        tlpTransferRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpTransferRoot.ColumnCount = 1;
        tlpTransferRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransferRoot.Controls.Add(pnlTransferContainer, 0, 0);
        tlpTransferRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTransferRoot.Location = new System.Drawing.Point(0, 0);
        tlpTransferRoot.Name = "tlpTransferRoot";
        tlpTransferRoot.Padding = new System.Windows.Forms.Padding(32);
        tlpTransferRoot.RowCount = 1;
        tlpTransferRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransferRoot.Size = new System.Drawing.Size(1100, 860);
        tlpTransferRoot.TabIndex = 0;
        //
        // pnlTransferContainer
        //
        pnlTransferContainer.Back = System.Drawing.Color.White;
        pnlTransferContainer.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlTransferContainer.BorderWidth = 2F;
        pnlTransferContainer.Controls.Add(tlpTransferInner);
        pnlTransferContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlTransferContainer.Location = new System.Drawing.Point(35, 35);
        pnlTransferContainer.Name = "pnlTransferContainer";
        pnlTransferContainer.Padding = new System.Windows.Forms.Padding(2);
        pnlTransferContainer.Radius = 12;
        pnlTransferContainer.Size = new System.Drawing.Size(1030, 790);
        pnlTransferContainer.TabIndex = 0;
        //
        // tlpTransferInner
        //
        tlpTransferInner.BackColor = System.Drawing.Color.White;
        tlpTransferInner.ColumnCount = 1;
        tlpTransferInner.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransferInner.Controls.Add(flpTabs, 0, 0);
        tlpTransferInner.Controls.Add(tblTransfer, 0, 1);
        tlpTransferInner.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTransferInner.Location = new System.Drawing.Point(2, 2);
        tlpTransferInner.Name = "tlpTransferInner";
        tlpTransferInner.RowCount = 2;
        tlpTransferInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        tlpTransferInner.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransferInner.Size = new System.Drawing.Size(1026, 786);
        tlpTransferInner.TabIndex = 0;
        //
        // flpTabs
        //
        flpTabs.BackColor = System.Drawing.Color.White;
        flpTabs.Controls.Add(btnTabPending);
        flpTabs.Controls.Add(btnTabSent);
        flpTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        flpTabs.Location = new System.Drawing.Point(0, 0);
        flpTabs.Margin = new System.Windows.Forms.Padding(0);
        flpTabs.Name = "flpTabs";
        flpTabs.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
        flpTabs.Size = new System.Drawing.Size(1026, 50);
        flpTabs.TabIndex = 0;
        flpTabs.WrapContents = false;
        //
        // btnTabPending
        //
        btnTabPending.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnTabPending.ForeColor = System.Drawing.Color.White;
        btnTabPending.Location = new System.Drawing.Point(9, 9);
        btnTabPending.Margin = new System.Windows.Forms.Padding(3);
        btnTabPending.Name = "btnTabPending";
        btnTabPending.Radius = 6;
        btnTabPending.Size = new System.Drawing.Size(110, 36);
        btnTabPending.TabIndex = 0;
        btnTabPending.Text = "รอส่ง";
        btnTabPending.Type = AntdUI.TTypeMini.Primary;
        //
        // btnTabSent
        //
        btnTabSent.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnTabSent.BorderWidth = 1F;
        btnTabSent.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        btnTabSent.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnTabSent.Location = new System.Drawing.Point(125, 9);
        btnTabSent.Margin = new System.Windows.Forms.Padding(3);
        btnTabSent.Name = "btnTabSent";
        btnTabSent.Radius = 6;
        btnTabSent.Size = new System.Drawing.Size(110, 36);
        btnTabSent.TabIndex = 1;
        btnTabSent.Text = "ส่งแล้ว";
        btnTabSent.Type = AntdUI.TTypeMini.Default;
        //
        // tblTransfer
        //
        tblTransfer.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        tblTransfer.Bordered = true;
        tblTransfer.ColumnBack = System.Drawing.Color.FromArgb(30, 30, 30);
        tblTransfer.ColumnFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        tblTransfer.ColumnFore = System.Drawing.Color.White;
        tblTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
        tblTransfer.EmptyText = "No orders";
        tblTransfer.Font = new System.Drawing.Font("Segoe UI", 13F);
        tblTransfer.Location = new System.Drawing.Point(0, 54);
        tblTransfer.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
        tblTransfer.Name = "tblTransfer";
        tblTransfer.Radius = 0;
        tblTransfer.RowHeight = 68;
        tblTransfer.Size = new System.Drawing.Size(1026, 732);
        tblTransfer.TabIndex = 1;
        //
        // TransferListUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpTransferRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "TransferListUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpTransferRoot.ResumeLayout(false);
        pnlTransferContainer.ResumeLayout(false);
        tlpTransferInner.ResumeLayout(false);
        flpTabs.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpTransferRoot;
    private AntdUI.Panel pnlTransferContainer;
    private System.Windows.Forms.TableLayoutPanel tlpTransferInner;
    private System.Windows.Forms.FlowLayoutPanel flpTabs;
    private AntdUI.Button btnTabPending;
    private AntdUI.Button btnTabSent;
    private AntdUI.Table tblTransfer;
}
