namespace InkjetOperator.Views;

partial class MarkingRefPickerDialog
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
        tlpRoot = new System.Windows.Forms.TableLayoutPanel();
        lblPrompt = new System.Windows.Forms.Label();
        tlpContent = new System.Windows.Forms.TableLayoutPanel();
        lstOptions = new System.Windows.Forms.ListBox();
        pnlImages = new System.Windows.Forms.Panel();
        flpImages = new System.Windows.Forms.FlowLayoutPanel();
        lblEmpty = new System.Windows.Forms.Label();
        flpButtons = new System.Windows.Forms.FlowLayoutPanel();
        btnOk = new AntdUI.Button();
        btnCancel = new AntdUI.Button();

        tlpRoot.SuspendLayout();
        tlpContent.SuspendLayout();
        pnlImages.SuspendLayout();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.BackColor = System.Drawing.Color.White;
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.Controls.Add(lblPrompt, 0, 0);
        tlpRoot.Controls.Add(tlpContent, 0, 1);
        tlpRoot.Controls.Add(flpButtons, 0, 2);
        tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRoot.Location = new System.Drawing.Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        tlpRoot.Size = new System.Drawing.Size(900, 560);
        tlpRoot.TabIndex = 0;
        //
        // lblPrompt
        //
        lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrompt.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblPrompt.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrompt.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
        lblPrompt.Name = "lblPrompt";
        lblPrompt.Size = new System.Drawing.Size(862, 30);
        lblPrompt.TabIndex = 0;
        lblPrompt.Text = "";
        lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // tlpContent - 2 cols: list(280) | images(fill)
        //
        tlpContent.BackColor = System.Drawing.Color.White;
        tlpContent.ColumnCount = 2;
        tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
        tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpContent.Controls.Add(lstOptions, 0, 0);
        tlpContent.Controls.Add(pnlImages, 1, 0);
        tlpContent.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpContent.Margin = new System.Windows.Forms.Padding(0);
        tlpContent.Name = "tlpContent";
        tlpContent.RowCount = 1;
        tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpContent.Size = new System.Drawing.Size(868, 450);
        tlpContent.TabIndex = 1;
        //
        // lstOptions
        //
        lstOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        lstOptions.Font = new System.Drawing.Font("Segoe UI", 12F);
        lstOptions.IntegralHeight = false;
        lstOptions.ItemHeight = 26;
        lstOptions.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        lstOptions.Name = "lstOptions";
        lstOptions.Size = new System.Drawing.Size(270, 450);
        lstOptions.TabIndex = 0;
        //
        // pnlImages
        //
        pnlImages.AutoScroll = true;
        pnlImages.BackColor = System.Drawing.Color.FromArgb(245, 249, 253);
        pnlImages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        pnlImages.Controls.Add(flpImages);
        pnlImages.Controls.Add(lblEmpty);
        pnlImages.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlImages.Margin = new System.Windows.Forms.Padding(0);
        pnlImages.Name = "pnlImages";
        pnlImages.Size = new System.Drawing.Size(588, 450);
        pnlImages.TabIndex = 1;
        //
        // flpImages
        //
        flpImages.AutoSize = true;
        flpImages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        flpImages.BackColor = System.Drawing.Color.Transparent;
        flpImages.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        flpImages.Location = new System.Drawing.Point(0, 0);
        flpImages.Margin = new System.Windows.Forms.Padding(0);
        flpImages.Name = "flpImages";
        flpImages.Padding = new System.Windows.Forms.Padding(8);
        flpImages.Size = new System.Drawing.Size(560, 420);
        flpImages.TabIndex = 0;
        flpImages.WrapContents = false;
        //
        // lblEmpty
        //
        lblEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
        lblEmpty.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblEmpty.ForeColor = System.Drawing.Color.Gray;
        lblEmpty.Name = "lblEmpty";
        lblEmpty.Size = new System.Drawing.Size(586, 448);
        lblEmpty.TabIndex = 1;
        lblEmpty.Text = "";
        lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblEmpty.Visible = false;
        //
        // flpButtons
        //
        flpButtons.BackColor = System.Drawing.Color.White;
        flpButtons.Controls.Add(btnOk);
        flpButtons.Controls.Add(btnCancel);
        flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpButtons.Margin = new System.Windows.Forms.Padding(0);
        flpButtons.Name = "flpButtons";
        flpButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
        flpButtons.Size = new System.Drawing.Size(868, 56);
        flpButtons.TabIndex = 2;
        flpButtons.WrapContents = false;
        //
        // btnOk
        //
        btnOk.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnOk.ForeColor = System.Drawing.Color.White;
        btnOk.Margin = new System.Windows.Forms.Padding(3);
        btnOk.Name = "btnOk";
        btnOk.Radius = 8;
        btnOk.Size = new System.Drawing.Size(120, 38);
        btnOk.TabIndex = 0;
        btnOk.Text = "ตกลง";
        btnOk.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.BorderWidth = 2F;
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Margin = new System.Windows.Forms.Padding(3);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(120, 38);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "ยกเลิก";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // MarkingRefPickerDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(900, 560);
        Controls.Add(tlpRoot);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MarkingRefPickerDialog";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "เลือกรุ่นย่อย";
        tlpRoot.ResumeLayout(false);
        tlpContent.ResumeLayout(false);
        pnlImages.ResumeLayout(false);
        pnlImages.PerformLayout();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpRoot;
    private System.Windows.Forms.Label lblPrompt;
    private System.Windows.Forms.TableLayoutPanel tlpContent;
    private System.Windows.Forms.ListBox lstOptions;
    private System.Windows.Forms.Panel pnlImages;
    private System.Windows.Forms.FlowLayoutPanel flpImages;
    private System.Windows.Forms.Label lblEmpty;
    private System.Windows.Forms.FlowLayoutPanel flpButtons;
    private AntdUI.Button btnOk;
    private AntdUI.Button btnCancel;
}
