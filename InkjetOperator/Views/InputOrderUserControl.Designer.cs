namespace InkjetOperator.Views;

partial class InputOrderUserControl
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
        tlpInputOrderRoot = new System.Windows.Forms.TableLayoutPanel();
        lblInputOrderTitle = new AntdUI.Label();
        tlpInputOrderRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpInputOrderRoot
        //
        tlpInputOrderRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpInputOrderRoot.ColumnCount = 1;
        tlpInputOrderRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpInputOrderRoot.Controls.Add(lblInputOrderTitle, 0, 0);
        tlpInputOrderRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpInputOrderRoot.Location = new System.Drawing.Point(0, 0);
        tlpInputOrderRoot.Name = "tlpInputOrderRoot";
        tlpInputOrderRoot.Padding = new System.Windows.Forms.Padding(40);
        tlpInputOrderRoot.RowCount = 1;
        tlpInputOrderRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpInputOrderRoot.Size = new System.Drawing.Size(1100, 860);
        tlpInputOrderRoot.TabIndex = 0;
        //
        // lblInputOrderTitle
        //
        lblInputOrderTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblInputOrderTitle.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
        lblInputOrderTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblInputOrderTitle.Location = new System.Drawing.Point(43, 43);
        lblInputOrderTitle.Name = "lblInputOrderTitle";
        lblInputOrderTitle.Size = new System.Drawing.Size(1014, 774);
        lblInputOrderTitle.TabIndex = 0;
        lblInputOrderTitle.Text = "Input Order";
        lblInputOrderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // InputOrderUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpInputOrderRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "InputOrderUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpInputOrderRoot.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpInputOrderRoot;
    private AntdUI.Label lblInputOrderTitle;
}
