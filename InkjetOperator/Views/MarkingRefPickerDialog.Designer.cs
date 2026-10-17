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
        tlpDialogRoot = new System.Windows.Forms.TableLayoutPanel();
        tlpTitleBar = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new AntdUI.Label();
        btnTitleClose = new System.Windows.Forms.Button();
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

        tlpDialogRoot.SuspendLayout();
        tlpTitleBar.SuspendLayout();
        tlpRoot.SuspendLayout();
        tlpContent.SuspendLayout();
        pnlImages.SuspendLayout();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // tlpDialogRoot — แถบหัวสีน้ำเงินเข้มแบบกล่องยืนยัน (ConfirmDialog) แล้วเนื้อหาเดิมข้างใต้
        //
        tlpDialogRoot.ColumnCount = 1;
        tlpDialogRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Controls.Add(tlpTitleBar, 0, 0);
        tlpDialogRoot.Controls.Add(tlpRoot, 0, 1);
        tlpDialogRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDialogRoot.Location = new System.Drawing.Point(0, 0);
        tlpDialogRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpDialogRoot.Name = "tlpDialogRoot";
        tlpDialogRoot.RowCount = 2;
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Size = new System.Drawing.Size(900, 624);
        tlpDialogRoot.TabIndex = 0;
        //
        // tlpTitleBar
        //
        tlpTitleBar.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        tlpTitleBar.ColumnCount = 2;
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpTitleBar.Controls.Add(lblTitle, 0, 0);
        tlpTitleBar.Controls.Add(btnTitleClose, 1, 0);
        tlpTitleBar.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTitleBar.Margin = new System.Windows.Forms.Padding(0);
        tlpTitleBar.Name = "tlpTitleBar";
        tlpTitleBar.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
        tlpTitleBar.RowCount = 1;
        tlpTitleBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.Size = new System.Drawing.Size(900, 64);
        tlpTitleBar.TabIndex = 0;
        //
        // lblTitle
        //
        lblTitle.BackColor = System.Drawing.Color.Transparent;
        lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.IconGap = 12;
        lblTitle.IconRatio = 1.1F;
        lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.PrefixColor = System.Drawing.Color.White;
        lblTitle.PrefixSvg = "PictureOutlined";
        lblTitle.Size = new System.Drawing.Size(808, 64);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "เลือกรุ่นย่อย";
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnTitleClose
        //
        btnTitleClose.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnTitleClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        btnTitleClose.Dock = System.Windows.Forms.DockStyle.Fill;
        btnTitleClose.FlatAppearance.BorderSize = 0;
        btnTitleClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnTitleClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnTitleClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnTitleClose.Font = new System.Drawing.Font("Segoe UI", 14F);
        btnTitleClose.ForeColor = System.Drawing.Color.White;
        btnTitleClose.Margin = new System.Windows.Forms.Padding(0);
        btnTitleClose.Name = "btnTitleClose";
        btnTitleClose.Size = new System.Drawing.Size(64, 64);
        btnTitleClose.TabIndex = 1;
        btnTitleClose.TabStop = false;
        btnTitleClose.Text = "✕";
        btnTitleClose.UseVisualStyleBackColor = false;
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
        tlpRoot.Location = new System.Drawing.Point(0, 64);
        tlpRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);
        tlpRoot.RowCount = 3;
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
        tlpRoot.Size = new System.Drawing.Size(1125, 700);
        tlpRoot.TabIndex = 0;
        //
        // lblPrompt
        //
        lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrompt.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblPrompt.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrompt.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
        lblPrompt.Name = "lblPrompt";
        lblPrompt.Size = new System.Drawing.Size(1078, 38);
        lblPrompt.TabIndex = 0;
        lblPrompt.Text = "";
        lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // tlpContent - 2 cols: list(280) | images(fill)
        //
        tlpContent.BackColor = System.Drawing.Color.White;
        tlpContent.ColumnCount = 2;
        tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
        tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpContent.Controls.Add(lstOptions, 0, 0);
        tlpContent.Controls.Add(pnlImages, 1, 0);
        tlpContent.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpContent.Margin = new System.Windows.Forms.Padding(0);
        tlpContent.Name = "tlpContent";
        tlpContent.RowCount = 1;
        tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpContent.Size = new System.Drawing.Size(1085, 562);
        tlpContent.TabIndex = 1;
        //
        // lstOptions
        //
        lstOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        lstOptions.Font = new System.Drawing.Font("Segoe UI", 15F);
        lstOptions.IntegralHeight = false;
        lstOptions.ItemHeight = 26;
        lstOptions.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        lstOptions.Name = "lstOptions";
        lstOptions.Size = new System.Drawing.Size(338, 562);
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
        pnlImages.Size = new System.Drawing.Size(735, 562);
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
        flpImages.Size = new System.Drawing.Size(700, 525);
        flpImages.TabIndex = 0;
        flpImages.WrapContents = false;
        //
        // lblEmpty
        //
        lblEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
        lblEmpty.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblEmpty.ForeColor = System.Drawing.Color.Gray;
        lblEmpty.Name = "lblEmpty";
        lblEmpty.Size = new System.Drawing.Size(732, 560);
        lblEmpty.TabIndex = 1;
        lblEmpty.Text = "";
        lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblEmpty.Visible = false;
        //
        // flpButtons
        //
        flpButtons.BackColor = System.Drawing.Color.White;
        flpButtons.Controls.Add(btnCancel);
        flpButtons.Controls.Add(btnOk);
        flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpButtons.Margin = new System.Windows.Forms.Padding(0);
        flpButtons.Name = "flpButtons";
        flpButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
        flpButtons.Size = new System.Drawing.Size(1085, 70);
        flpButtons.TabIndex = 2;
        flpButtons.WrapContents = false;
        //
        // btnOk
        //
        btnOk.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnOk.ForeColor = System.Drawing.Color.White;
        btnOk.IconSvg = "CheckOutlined";
        btnOk.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        btnOk.Name = "btnOk";
        btnOk.Radius = 8;
        btnOk.Size = new System.Drawing.Size(220, 56);
        btnOk.TabIndex = 0;
        btnOk.Text = "ตกลง";
        btnOk.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.BorderWidth = 2F;
        btnCancel.DefaultBack = System.Drawing.Color.White;
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnCancel.Margin = new System.Windows.Forms.Padding(0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(150, 56);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "ยกเลิก";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // MarkingRefPickerDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        BorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        BorderWidth = 1;
        ClientSize = new System.Drawing.Size(900, 624);
        Controls.Add(tlpDialogRoot);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MarkingRefPickerDialog";
        Radius = 8;
        Shadow = 12;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "เลือกรุ่นย่อย";
        tlpDialogRoot.ResumeLayout(false);
        tlpTitleBar.ResumeLayout(false);
        tlpRoot.ResumeLayout(false);
        tlpContent.ResumeLayout(false);
        pnlImages.ResumeLayout(false);
        pnlImages.PerformLayout();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpDialogRoot;
    private System.Windows.Forms.TableLayoutPanel tlpTitleBar;
    private AntdUI.Label lblTitle;
    private System.Windows.Forms.Button btnTitleClose;
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
