namespace InkjetOperator.Views;

partial class SystemHealthUserControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        tlpHealthRoot = new System.Windows.Forms.TableLayoutPanel();
        lblHealthTitle = new AntdUI.Label();
        lblHealthSummary = new AntdUI.Label();
        tblHealth = new AntdUI.Table();
        tlpHealthRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpHealthRoot
        //
        tlpHealthRoot.ColumnCount = 1;
        tlpHealthRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHealthRoot.Controls.Add(lblHealthTitle, 0, 0);
        tlpHealthRoot.Controls.Add(lblHealthSummary, 0, 1);
        tlpHealthRoot.Controls.Add(tblHealth, 0, 2);
        tlpHealthRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpHealthRoot.Location = new System.Drawing.Point(32, 32);
        tlpHealthRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpHealthRoot.Name = "tlpHealthRoot";
        tlpHealthRoot.RowCount = 3;
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHealthRoot.Size = new System.Drawing.Size(1216, 736);
        tlpHealthRoot.TabIndex = 0;
        //
        // lblHealthTitle
        //
        lblHealthTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHealthTitle.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
        lblHealthTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblHealthTitle.Location = new System.Drawing.Point(0, 0);
        lblHealthTitle.Margin = new System.Windows.Forms.Padding(0);
        lblHealthTitle.Name = "lblHealthTitle";
        lblHealthTitle.Size = new System.Drawing.Size(1216, 54);
        lblHealthTitle.TabIndex = 0;
        lblHealthTitle.Text = "สถานะระบบ";
        //
        // lblHealthSummary
        //
        lblHealthSummary.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHealthSummary.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblHealthSummary.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblHealthSummary.Location = new System.Drawing.Point(0, 54);
        lblHealthSummary.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        lblHealthSummary.Name = "lblHealthSummary";
        lblHealthSummary.Size = new System.Drawing.Size(1216, 32);
        lblHealthSummary.TabIndex = 1;
        lblHealthSummary.Text = "กำลังตรวจสอบ...";
        //
        // tblHealth
        //
        tblHealth.Dock = System.Windows.Forms.DockStyle.Fill;
        tblHealth.EmptyText = "กำลังตรวจสอบ...";
        tblHealth.Font = new System.Drawing.Font("Segoe UI", 14F);
        tblHealth.Location = new System.Drawing.Point(0, 98);
        tblHealth.Margin = new System.Windows.Forms.Padding(0);
        tblHealth.Name = "tblHealth";
        tblHealth.Radius = 10;
        tblHealth.RowHeight = 46;
        tblHealth.Size = new System.Drawing.Size(1216, 638);
        tblHealth.TabIndex = 2;
        //
        // SystemHealthUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpHealthRoot);
        Name = "SystemHealthUserControl";
        Padding = new System.Windows.Forms.Padding(32);
        Size = new System.Drawing.Size(1280, 800);
        tlpHealthRoot.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpHealthRoot;
    private AntdUI.Label lblHealthTitle;
    private AntdUI.Label lblHealthSummary;
    private AntdUI.Table tblHealth;
}
