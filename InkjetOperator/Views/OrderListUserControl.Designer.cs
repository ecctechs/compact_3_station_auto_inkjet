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
        lblOrderListTitle = new AntdUI.Label();
        tlpOrderListRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpOrderListRoot
        //
        tlpOrderListRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpOrderListRoot.ColumnCount = 1;
        tlpOrderListRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.Controls.Add(lblOrderListTitle, 0, 0);
        tlpOrderListRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOrderListRoot.Location = new System.Drawing.Point(0, 0);
        tlpOrderListRoot.Name = "tlpOrderListRoot";
        tlpOrderListRoot.Padding = new System.Windows.Forms.Padding(40);
        tlpOrderListRoot.RowCount = 1;
        tlpOrderListRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOrderListRoot.Size = new System.Drawing.Size(1100, 860);
        tlpOrderListRoot.TabIndex = 0;
        //
        // lblOrderListTitle
        //
        lblOrderListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOrderListTitle.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
        lblOrderListTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblOrderListTitle.Location = new System.Drawing.Point(43, 43);
        lblOrderListTitle.Name = "lblOrderListTitle";
        lblOrderListTitle.Size = new System.Drawing.Size(1014, 774);
        lblOrderListTitle.TabIndex = 0;
        lblOrderListTitle.Text = "Order List";
        lblOrderListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOrderListRoot;
    private AntdUI.Label lblOrderListTitle;
}
