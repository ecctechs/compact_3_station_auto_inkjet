namespace InkjetOperator.Views;

partial class EditPatternUserControl
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
        tlpEditPatternRoot = new System.Windows.Forms.TableLayoutPanel();
        lblEditPatternTitle = new AntdUI.Label();
        tlpEditPatternRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpEditPatternRoot
        //
        tlpEditPatternRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpEditPatternRoot.ColumnCount = 1;
        tlpEditPatternRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpEditPatternRoot.Controls.Add(lblEditPatternTitle, 0, 0);
        tlpEditPatternRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpEditPatternRoot.Location = new System.Drawing.Point(0, 0);
        tlpEditPatternRoot.Name = "tlpEditPatternRoot";
        tlpEditPatternRoot.Padding = new System.Windows.Forms.Padding(40);
        tlpEditPatternRoot.RowCount = 1;
        tlpEditPatternRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpEditPatternRoot.Size = new System.Drawing.Size(1100, 860);
        tlpEditPatternRoot.TabIndex = 0;
        //
        // lblEditPatternTitle
        //
        lblEditPatternTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblEditPatternTitle.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
        lblEditPatternTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblEditPatternTitle.Location = new System.Drawing.Point(43, 43);
        lblEditPatternTitle.Name = "lblEditPatternTitle";
        lblEditPatternTitle.Size = new System.Drawing.Size(1014, 774);
        lblEditPatternTitle.TabIndex = 0;
        lblEditPatternTitle.Text = "Edit Pattern";
        lblEditPatternTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // EditPatternUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpEditPatternRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "EditPatternUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpEditPatternRoot.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpEditPatternRoot;
    private AntdUI.Label lblEditPatternTitle;
}
