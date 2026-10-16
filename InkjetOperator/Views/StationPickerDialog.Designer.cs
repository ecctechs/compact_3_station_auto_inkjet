namespace InkjetOperator.Views;

partial class StationPickerDialog
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
        tlpTitleBar = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        btnTitleClose = new System.Windows.Forms.Button();
        tlpBody = new System.Windows.Forms.TableLayoutPanel();
        lblPrompt = new AntdUI.Label();
        btnScanBarcode = new AntdUI.Button();
        btnStation1 = new AntdUI.Button();
        btnStation3 = new AntdUI.Button();
        lblNote = new AntdUI.Label();
        tlpDialogRoot.SuspendLayout();
        tlpTitleBar.SuspendLayout();
        tlpBody.SuspendLayout();
        SuspendLayout();
        //
        // tlpDialogRoot
        //
        tlpDialogRoot.ColumnCount = 1;
        tlpDialogRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Controls.Add(tlpTitleBar, 0, 0);
        tlpDialogRoot.Controls.Add(tlpBody, 0, 1);
        tlpDialogRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDialogRoot.Location = new System.Drawing.Point(0, 0);
        tlpDialogRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpDialogRoot.Name = "tlpDialogRoot";
        tlpDialogRoot.RowCount = 2;
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Size = new System.Drawing.Size(640, 560);
        tlpDialogRoot.TabIndex = 0;
        //
        // tlpTitleBar
        //
        tlpTitleBar.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        tlpTitleBar.ColumnCount = 2;
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        tlpTitleBar.Controls.Add(lblTitle, 0, 0);
        tlpTitleBar.Controls.Add(btnTitleClose, 1, 0);
        tlpTitleBar.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTitleBar.Location = new System.Drawing.Point(0, 0);
        tlpTitleBar.Margin = new System.Windows.Forms.Padding(0);
        tlpTitleBar.Name = "tlpTitleBar";
        tlpTitleBar.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
        tlpTitleBar.RowCount = 1;
        tlpTitleBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.Size = new System.Drawing.Size(640, 65);
        tlpTitleBar.TabIndex = 0;
        //
        // lblTitle
        //
        lblTitle.BackColor = System.Drawing.Color.Transparent;
        lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 17.5F);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(19, 0);
        lblTitle.Margin = new System.Windows.Forms.Padding(3, 0, 8, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(553, 65);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "เลือกสถานีของเครื่องนี้";
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
        btnTitleClose.Location = new System.Drawing.Point(580, 0);
        btnTitleClose.Margin = new System.Windows.Forms.Padding(0);
        btnTitleClose.Name = "btnTitleClose";
        btnTitleClose.Size = new System.Drawing.Size(60, 65);
        btnTitleClose.TabIndex = 1;
        btnTitleClose.TabStop = false;
        btnTitleClose.Text = "✕";
        btnTitleClose.UseVisualStyleBackColor = false;
        //
        // tlpBody
        //
        tlpBody.BackColor = System.Drawing.Color.White;
        tlpBody.ColumnCount = 1;
        tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBody.Controls.Add(lblPrompt, 0, 0);
        tlpBody.Controls.Add(btnScanBarcode, 0, 1);
        tlpBody.Controls.Add(btnStation1, 0, 2);
        tlpBody.Controls.Add(btnStation3, 0, 3);
        tlpBody.Controls.Add(lblNote, 0, 4);
        tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpBody.Location = new System.Drawing.Point(0, 65);
        tlpBody.Margin = new System.Windows.Forms.Padding(0);
        tlpBody.Name = "tlpBody";
        tlpBody.Padding = new System.Windows.Forms.Padding(32, 20, 32, 20);
        tlpBody.RowCount = 5;
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBody.Size = new System.Drawing.Size(640, 495);
        tlpBody.TabIndex = 1;
        //
        // lblPrompt
        //
        lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrompt.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblPrompt.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrompt.Location = new System.Drawing.Point(35, 20);
        lblPrompt.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblPrompt.Name = "lblPrompt";
        lblPrompt.Size = new System.Drawing.Size(570, 72);
        lblPrompt.TabIndex = 0;
        lblPrompt.Text = "เปิดโปรแกรมครั้งแรกบนเครื่องนี้ — เครื่องนี้ใช้เป็นสถานีไหน";
        lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnScanBarcode
        //
        btnScanBarcode.BorderWidth = 2F;
        btnScanBarcode.DefaultBack = System.Drawing.Color.White;
        btnScanBarcode.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnScanBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
        btnScanBarcode.Font = new System.Drawing.Font("Segoe UI", 17F);
        btnScanBarcode.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnScanBarcode.Location = new System.Drawing.Point(32, 104);
        btnScanBarcode.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
        btnScanBarcode.Name = "btnScanBarcode";
        btnScanBarcode.Radius = 8;
        btnScanBarcode.Size = new System.Drawing.Size(576, 84);
        btnScanBarcode.TabIndex = 1;
        btnScanBarcode.Text = "Scan Barcode  ·  ลงทะเบียนงาน";
        //
        // btnStation1
        //
        btnStation1.BorderWidth = 2F;
        btnStation1.DefaultBack = System.Drawing.Color.White;
        btnStation1.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnStation1.Dock = System.Windows.Forms.DockStyle.Fill;
        btnStation1.Font = new System.Drawing.Font("Segoe UI", 17F);
        btnStation1.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnStation1.Location = new System.Drawing.Point(32, 200);
        btnStation1.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
        btnStation1.Name = "btnStation1";
        btnStation1.Radius = 8;
        btnStation1.Size = new System.Drawing.Size(576, 84);
        btnStation1.TabIndex = 2;
        btnStation1.Text = "Station 1  ·  ส่งงานเข้า MK / UV / PLC";
        //
        // btnStation3
        //
        btnStation3.BorderWidth = 2F;
        btnStation3.DefaultBack = System.Drawing.Color.White;
        btnStation3.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnStation3.Dock = System.Windows.Forms.DockStyle.Fill;
        btnStation3.Font = new System.Drawing.Font("Segoe UI", 17F);
        btnStation3.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnStation3.Location = new System.Drawing.Point(32, 296);
        btnStation3.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
        btnStation3.Name = "btnStation3";
        btnStation3.Radius = 8;
        btnStation3.Size = new System.Drawing.Size(576, 84);
        btnStation3.TabIndex = 3;
        btnStation3.Text = "Station 3  ·  งานที่ผ่าน UV2";
        //
        // lblNote
        //
        lblNote.Dock = System.Windows.Forms.DockStyle.Fill;
        lblNote.Font = new System.Drawing.Font("Segoe UI", 12F);
        lblNote.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblNote.Location = new System.Drawing.Point(35, 392);
        lblNote.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        lblNote.Name = "lblNote";
        lblNote.Size = new System.Drawing.Size(570, 83);
        lblNote.TabIndex = 4;
        lblNote.Text = "ถามครั้งเดียว ค่าเก็บที่ MENU_LEVEL ใน C:\\ProgramData\\CompactInkjet\\Setting.config\r\nถ้าเลือกผิด แก้ค่าในไฟล์นั้นแล้วเปิดโปรแกรมใหม่";
        lblNote.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // StationPickerDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        BorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        BorderWidth = 1;
        CancelButton = btnTitleClose;
        ClientSize = new System.Drawing.Size(640, 560);
        Controls.Add(tlpDialogRoot);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "StationPickerDialog";
        Radius = 8;
        Shadow = 12;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "เลือกสถานีของเครื่องนี้";
        TopMost = true;
        tlpDialogRoot.ResumeLayout(false);
        tlpTitleBar.ResumeLayout(false);
        tlpBody.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpDialogRoot;
    private System.Windows.Forms.TableLayoutPanel tlpTitleBar;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnTitleClose;
    private System.Windows.Forms.TableLayoutPanel tlpBody;
    private AntdUI.Label lblPrompt;
    private AntdUI.Button btnScanBarcode;
    private AntdUI.Button btnStation1;
    private AntdUI.Button btnStation3;
    private AntdUI.Label lblNote;
}
