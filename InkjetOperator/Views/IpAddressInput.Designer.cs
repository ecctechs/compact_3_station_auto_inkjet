namespace InkjetOperator.Views;

partial class IpAddressInput
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        tlpOctets = new System.Windows.Forms.TableLayoutPanel();
        txtOctet1 = new AntdUI.Input();
        lblDot1 = new System.Windows.Forms.Label();
        txtOctet2 = new AntdUI.Input();
        lblDot2 = new System.Windows.Forms.Label();
        txtOctet3 = new AntdUI.Input();
        lblDot3 = new System.Windows.Forms.Label();
        txtOctet4 = new AntdUI.Input();

        tlpOctets.SuspendLayout();
        SuspendLayout();
        //
        // tlpOctets - box . box . box . box
        //
        tlpOctets.BackColor = System.Drawing.Color.Transparent;
        tlpOctets.ColumnCount = 7;
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
        tlpOctets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpOctets.Controls.Add(txtOctet1, 0, 0);
        tlpOctets.Controls.Add(lblDot1, 1, 0);
        tlpOctets.Controls.Add(txtOctet2, 2, 0);
        tlpOctets.Controls.Add(lblDot2, 3, 0);
        tlpOctets.Controls.Add(txtOctet3, 4, 0);
        tlpOctets.Controls.Add(lblDot3, 5, 0);
        tlpOctets.Controls.Add(txtOctet4, 6, 0);
        tlpOctets.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOctets.Location = new System.Drawing.Point(0, 0);
        tlpOctets.Margin = new System.Windows.Forms.Padding(0);
        tlpOctets.Name = "tlpOctets";
        tlpOctets.RowCount = 1;
        tlpOctets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOctets.Size = new System.Drawing.Size(320, 34);
        tlpOctets.TabIndex = 0;
        //
        // txtOctet1
        //
        txtOctet1.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtOctet1.Dock = System.Windows.Forms.DockStyle.Fill;
        txtOctet1.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtOctet1.Margin = new System.Windows.Forms.Padding(0);
        txtOctet1.MaxLength = 3;
        txtOctet1.Name = "txtOctet1";
        txtOctet1.Radius = 4;
        txtOctet1.Size = new System.Drawing.Size(69, 34);
        txtOctet1.TabIndex = 0;
        txtOctet1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        //
        // lblDot1
        //
        lblDot1.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDot1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblDot1.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDot1.Margin = new System.Windows.Forms.Padding(0);
        lblDot1.Name = "lblDot1";
        lblDot1.Size = new System.Drawing.Size(14, 34);
        lblDot1.TabIndex = 1;
        lblDot1.Text = ".";
        lblDot1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtOctet2
        //
        txtOctet2.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtOctet2.Dock = System.Windows.Forms.DockStyle.Fill;
        txtOctet2.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtOctet2.Margin = new System.Windows.Forms.Padding(0);
        txtOctet2.MaxLength = 3;
        txtOctet2.Name = "txtOctet2";
        txtOctet2.Radius = 4;
        txtOctet2.Size = new System.Drawing.Size(69, 34);
        txtOctet2.TabIndex = 2;
        txtOctet2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        //
        // lblDot2
        //
        lblDot2.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDot2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblDot2.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDot2.Margin = new System.Windows.Forms.Padding(0);
        lblDot2.Name = "lblDot2";
        lblDot2.Size = new System.Drawing.Size(14, 34);
        lblDot2.TabIndex = 3;
        lblDot2.Text = ".";
        lblDot2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtOctet3
        //
        txtOctet3.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtOctet3.Dock = System.Windows.Forms.DockStyle.Fill;
        txtOctet3.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtOctet3.Margin = new System.Windows.Forms.Padding(0);
        txtOctet3.MaxLength = 3;
        txtOctet3.Name = "txtOctet3";
        txtOctet3.Radius = 4;
        txtOctet3.Size = new System.Drawing.Size(69, 34);
        txtOctet3.TabIndex = 4;
        txtOctet3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        //
        // lblDot3
        //
        lblDot3.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDot3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblDot3.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDot3.Margin = new System.Windows.Forms.Padding(0);
        lblDot3.Name = "lblDot3";
        lblDot3.Size = new System.Drawing.Size(14, 34);
        lblDot3.TabIndex = 5;
        lblDot3.Text = ".";
        lblDot3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // txtOctet4
        //
        txtOctet4.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtOctet4.Dock = System.Windows.Forms.DockStyle.Fill;
        txtOctet4.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtOctet4.Margin = new System.Windows.Forms.Padding(0);
        txtOctet4.MaxLength = 3;
        txtOctet4.Name = "txtOctet4";
        txtOctet4.Radius = 4;
        txtOctet4.Size = new System.Drawing.Size(69, 34);
        txtOctet4.TabIndex = 6;
        txtOctet4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        //
        // IpAddressInput
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Transparent;
        Controls.Add(tlpOctets);
        Name = "IpAddressInput";
        Size = new System.Drawing.Size(320, 34);
        tlpOctets.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOctets;
    private AntdUI.Input txtOctet1;
    private System.Windows.Forms.Label lblDot1;
    private AntdUI.Input txtOctet2;
    private System.Windows.Forms.Label lblDot2;
    private AntdUI.Input txtOctet3;
    private System.Windows.Forms.Label lblDot3;
    private AntdUI.Input txtOctet4;
}
