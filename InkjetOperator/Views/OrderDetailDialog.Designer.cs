namespace InkjetOperator.Views;

partial class OrderDetailDialog
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
        tlpDialogRoot = new System.Windows.Forms.TableLayoutPanel();
        titleBar = new AppTitleBarUserControl();
        detailPage = new OrderDetailUserControl();
        tlpDialogRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpDialogRoot
        //
        tlpDialogRoot.ColumnCount = 1;
        tlpDialogRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Controls.Add(titleBar, 0, 0);
        tlpDialogRoot.Controls.Add(detailPage, 0, 1);
        tlpDialogRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDialogRoot.Location = new System.Drawing.Point(0, 0);
        tlpDialogRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpDialogRoot.Name = "tlpDialogRoot";
        tlpDialogRoot.RowCount = 2;
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Size = new System.Drawing.Size(1610, 1150);
        tlpDialogRoot.TabIndex = 0;
        //
        // titleBar
        //
        titleBar.Dock = System.Windows.Forms.DockStyle.Fill;
        titleBar.Location = new System.Drawing.Point(0, 0);
        titleBar.Margin = new System.Windows.Forms.Padding(0);
        titleBar.Name = "titleBar";
        titleBar.ShowMinimizeButton = false;
        titleBar.Size = new System.Drawing.Size(1610, 46);
        titleBar.TabIndex = 0;
        titleBar.TitleText = "Order Detail";
        //
        // detailPage
        //
        detailPage.Dock = System.Windows.Forms.DockStyle.Fill;
        detailPage.Location = new System.Drawing.Point(0, 40);
        detailPage.Margin = new System.Windows.Forms.Padding(0);
        detailPage.Name = "detailPage";
        detailPage.Size = new System.Drawing.Size(1610, 1104);
        detailPage.TabIndex = 1;
        //
        // OrderDetailDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.FromArgb(234, 241, 248);
        BorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        BorderWidth = 1;
        // The body needs 1320px of width to show a section without cramping
        // (tlpSections 1272 + pnlBody padding). 1400 clears that plus the vertical
        // scrollbar. OnLoad clamps this to the screen and centres it - the window is
        // deliberately wide but not maximised.
        ClientSize = new System.Drawing.Size(1400, 1000);
        Controls.Add(tlpDialogRoot);
        Name = "OrderDetailDialog";
        Radius = 8;
        Shadow = 12;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Order Detail";
        tlpDialogRoot.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpDialogRoot;
    private AppTitleBarUserControl titleBar;
    private OrderDetailUserControl detailPage;
}
