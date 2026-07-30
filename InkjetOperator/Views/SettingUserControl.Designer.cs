namespace InkjetOperator.Views;

partial class SettingUserControl
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
        tlpSettingRoot = new System.Windows.Forms.TableLayoutPanel();
        lblSettingTitle = new AntdUI.Label();
        tlpSettingRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpSettingRoot
        //
        tlpSettingRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpSettingRoot.ColumnCount = 1;
        tlpSettingRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSettingRoot.Controls.Add(lblSettingTitle, 0, 0);
        tlpSettingRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpSettingRoot.Location = new System.Drawing.Point(0, 0);
        tlpSettingRoot.Name = "tlpSettingRoot";
        tlpSettingRoot.Padding = new System.Windows.Forms.Padding(40);
        tlpSettingRoot.RowCount = 1;
        tlpSettingRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSettingRoot.Size = new System.Drawing.Size(1100, 860);
        tlpSettingRoot.TabIndex = 0;
        //
        // lblSettingTitle
        //
        lblSettingTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblSettingTitle.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
        lblSettingTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblSettingTitle.Location = new System.Drawing.Point(43, 43);
        lblSettingTitle.Name = "lblSettingTitle";
        lblSettingTitle.Size = new System.Drawing.Size(1014, 774);
        lblSettingTitle.TabIndex = 0;
        lblSettingTitle.Text = "Setting";
        lblSettingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // SettingUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpSettingRoot);
        MinimumSize = new System.Drawing.Size(820, 680);
        Name = "SettingUserControl";
        Size = new System.Drawing.Size(1100, 860);
        tlpSettingRoot.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpSettingRoot;
    private AntdUI.Label lblSettingTitle;
}
