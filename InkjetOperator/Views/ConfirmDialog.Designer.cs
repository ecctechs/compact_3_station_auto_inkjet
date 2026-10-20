namespace InkjetOperator.Views;

partial class ConfirmDialog
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
        lblTitle = new AntdUI.Label();
        btnTitleClose = new System.Windows.Forms.Button();
        tlpBody = new System.Windows.Forms.TableLayoutPanel();
        lblHeadline = new System.Windows.Forms.Label();
        tlpFacts = new System.Windows.Forms.TableLayoutPanel();
        tlpSkip = new System.Windows.Forms.TableLayoutPanel();
        lblSkipHeading = new AntdUI.Label();
        chkSkip0 = new AntdUI.Checkbox();
        chkSkip1 = new AntdUI.Checkbox();
        chkSkip2 = new AntdUI.Checkbox();
        lblFactCaption1 = new System.Windows.Forms.Label();
        lblFactValue1 = new System.Windows.Forms.Label();
        lblFactCaption2 = new System.Windows.Forms.Label();
        lblFactValue2 = new System.Windows.Forms.Label();
        lblFactCaption3 = new System.Windows.Forms.Label();
        lblFactValue3 = new System.Windows.Forms.Label();
        lblFactCaption4 = new System.Windows.Forms.Label();
        lblFactValue4 = new System.Windows.Forms.Label();
        lblFactCaption5 = new System.Windows.Forms.Label();
        lblFactValue5 = new System.Windows.Forms.Label();
        lblFactCaption6 = new System.Windows.Forms.Label();
        lblFactValue6 = new System.Windows.Forms.Label();
        lblMessage = new System.Windows.Forms.Label();
        pnlDetail = new System.Windows.Forms.Panel();
        lblDetail = new System.Windows.Forms.Label();
        lblWarning = new System.Windows.Forms.Label();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnCancel = new AntdUI.Button();
        btnOk = new AntdUI.Button();
        tlpDialogRoot.SuspendLayout();
        tlpTitleBar.SuspendLayout();
        tlpBody.SuspendLayout();
        tlpFacts.SuspendLayout();
        tlpSkip.SuspendLayout();
        pnlDetail.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpDialogRoot — กว้างคงที่ สูงตามเนื้อหา (ฟอร์ม AutoSize)
        //
        tlpDialogRoot.AutoSize = true;
        tlpDialogRoot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpDialogRoot.ColumnCount = 1;
        tlpDialogRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 720F));
        tlpDialogRoot.Controls.Add(tlpTitleBar, 0, 0);
        tlpDialogRoot.Controls.Add(tlpBody, 0, 1);
        tlpDialogRoot.Location = new System.Drawing.Point(0, 0);
        tlpDialogRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpDialogRoot.Name = "tlpDialogRoot";
        tlpDialogRoot.RowCount = 2;
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpDialogRoot.Size = new System.Drawing.Size(720, 520);
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
        tlpTitleBar.Location = new System.Drawing.Point(0, 0);
        tlpTitleBar.Margin = new System.Windows.Forms.Padding(0);
        tlpTitleBar.Name = "tlpTitleBar";
        tlpTitleBar.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
        tlpTitleBar.RowCount = 1;
        tlpTitleBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.Size = new System.Drawing.Size(720, 64);
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
        lblTitle.PrefixColor = System.Drawing.Color.White;
        lblTitle.PrefixSvg = "QuestionCircleFilled";
        lblTitle.Location = new System.Drawing.Point(20, 0);
        lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(628, 64);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Title";
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
        btnTitleClose.Location = new System.Drawing.Point(656, 0);
        btnTitleClose.Margin = new System.Windows.Forms.Padding(0);
        btnTitleClose.Name = "btnTitleClose";
        btnTitleClose.Size = new System.Drawing.Size(64, 64);
        btnTitleClose.TabIndex = 1;
        btnTitleClose.TabStop = false;
        btnTitleClose.Text = "✕";
        btnTitleClose.UseVisualStyleBackColor = false;
        //
        // tlpBody — ทุกแถวสูงตามเนื้อหา ช่องที่ไม่ใช้ถูกซ่อน แถวยุบหายเอง
        //
        tlpBody.AutoSize = true;
        tlpBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpBody.BackColor = System.Drawing.Color.White;
        tlpBody.ColumnCount = 1;
        tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBody.Controls.Add(lblHeadline, 0, 0);
        tlpBody.Controls.Add(tlpFacts, 0, 1);
        tlpBody.Controls.Add(tlpSkip, 0, 2);
        tlpBody.Controls.Add(lblMessage, 0, 3);
        tlpBody.Controls.Add(pnlDetail, 0, 4);
        tlpBody.Controls.Add(lblWarning, 0, 5);
        tlpBody.Controls.Add(flpActions, 0, 6);
        tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpBody.Location = new System.Drawing.Point(0, 64);
        tlpBody.Margin = new System.Windows.Forms.Padding(0);
        tlpBody.Name = "tlpBody";
        tlpBody.Padding = new System.Windows.Forms.Padding(28, 22, 28, 24);
        tlpBody.RowCount = 7;
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpBody.Size = new System.Drawing.Size(720, 456);
        tlpBody.TabIndex = 1;
        //
        // lblHeadline — ประโยคเดียวบอกว่ากดแล้วจะเกิดอะไร
        //
        lblHeadline.AutoSize = true;
        lblHeadline.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
        lblHeadline.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblHeadline.Location = new System.Drawing.Point(28, 22);
        lblHeadline.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
        lblHeadline.MaximumSize = new System.Drawing.Size(664, 0);
        lblHeadline.Name = "lblHeadline";
        lblHeadline.Size = new System.Drawing.Size(200, 31);
        lblHeadline.TabIndex = 0;
        lblHeadline.Text = "Headline";
        //
        // tlpFacts — หัวข้อสีเทาด้านซ้าย ค่าสีน้ำเงินตัวหนาด้านขวา (แบบหน้ายืนยันของ NastoKeyence)
        //
        tlpFacts.AutoSize = true;
        tlpFacts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpFacts.ColumnCount = 2;
        tlpFacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
        tlpFacts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpFacts.Controls.Add(lblFactCaption1, 0, 0);
        tlpFacts.Controls.Add(lblFactValue1, 1, 0);
        tlpFacts.Controls.Add(lblFactCaption2, 0, 1);
        tlpFacts.Controls.Add(lblFactValue2, 1, 1);
        tlpFacts.Controls.Add(lblFactCaption3, 0, 2);
        tlpFacts.Controls.Add(lblFactValue3, 1, 2);
        tlpFacts.Controls.Add(lblFactCaption4, 0, 3);
        tlpFacts.Controls.Add(lblFactValue4, 1, 3);
        tlpFacts.Controls.Add(lblFactCaption5, 0, 4);
        tlpFacts.Controls.Add(lblFactValue5, 1, 4);
        tlpFacts.Controls.Add(lblFactCaption6, 0, 5);
        tlpFacts.Controls.Add(lblFactValue6, 1, 5);
        tlpFacts.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpFacts.Location = new System.Drawing.Point(28, 67);
        tlpFacts.Margin = new System.Windows.Forms.Padding(0);
        tlpFacts.Name = "tlpFacts";
        tlpFacts.RowCount = 6;
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpFacts.Size = new System.Drawing.Size(664, 240);
        tlpFacts.TabIndex = 1;
        //
        // lblFactCaption1
        //
        lblFactCaption1.AutoSize = true;
        lblFactCaption1.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption1.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption1.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption1.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption1.Name = "lblFactCaption1";
        lblFactCaption1.TabIndex = 0;
        lblFactCaption1.Text = "Caption";
        //
        // lblFactValue1
        //
        lblFactValue1.AutoSize = true;
        lblFactValue1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue1.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue1.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue1.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue1.Name = "lblFactValue1";
        lblFactValue1.TabIndex = 1;
        lblFactValue1.Text = "Value";
        //
        // lblFactCaption2
        //
        lblFactCaption2.AutoSize = true;
        lblFactCaption2.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption2.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption2.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption2.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption2.Name = "lblFactCaption2";
        lblFactCaption2.TabIndex = 2;
        lblFactCaption2.Text = "Caption";
        //
        // lblFactValue2
        //
        lblFactValue2.AutoSize = true;
        lblFactValue2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue2.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue2.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue2.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue2.Name = "lblFactValue2";
        lblFactValue2.TabIndex = 3;
        lblFactValue2.Text = "Value";
        //
        // lblFactCaption3
        //
        lblFactCaption3.AutoSize = true;
        lblFactCaption3.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption3.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption3.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption3.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption3.Name = "lblFactCaption3";
        lblFactCaption3.TabIndex = 4;
        lblFactCaption3.Text = "Caption";
        //
        // lblFactValue3
        //
        lblFactValue3.AutoSize = true;
        lblFactValue3.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue3.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue3.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue3.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue3.Name = "lblFactValue3";
        lblFactValue3.TabIndex = 5;
        lblFactValue3.Text = "Value";
        //
        // lblFactCaption4
        //
        lblFactCaption4.AutoSize = true;
        lblFactCaption4.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption4.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption4.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption4.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption4.Name = "lblFactCaption4";
        lblFactCaption4.TabIndex = 6;
        lblFactCaption4.Text = "Caption";
        //
        // lblFactValue4
        //
        lblFactValue4.AutoSize = true;
        lblFactValue4.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue4.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue4.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue4.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue4.Name = "lblFactValue4";
        lblFactValue4.TabIndex = 7;
        lblFactValue4.Text = "Value";
        //
        // lblFactCaption5
        //
        lblFactCaption5.AutoSize = true;
        lblFactCaption5.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption5.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption5.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption5.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption5.Name = "lblFactCaption5";
        lblFactCaption5.TabIndex = 8;
        lblFactCaption5.Text = "Caption";
        //
        // lblFactValue5
        //
        lblFactValue5.AutoSize = true;
        lblFactValue5.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue5.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue5.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue5.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue5.Name = "lblFactValue5";
        lblFactValue5.TabIndex = 9;
        lblFactValue5.Text = "Value";
        //
        // lblFactCaption6
        //
        lblFactCaption6.AutoSize = true;
        lblFactCaption6.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblFactCaption6.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblFactCaption6.Margin = new System.Windows.Forms.Padding(0, 9, 12, 9);
        lblFactCaption6.MaximumSize = new System.Drawing.Size(138, 0);
        lblFactCaption6.Name = "lblFactCaption6";
        lblFactCaption6.TabIndex = 10;
        lblFactCaption6.Text = "Caption";
        //
        // lblFactValue6
        //
        lblFactValue6.AutoSize = true;
        lblFactValue6.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblFactValue6.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblFactValue6.Margin = new System.Windows.Forms.Padding(0, 7, 0, 7);
        lblFactValue6.MaximumSize = new System.Drawing.Size(510, 0);
        lblFactValue6.Name = "lblFactValue6";
        lblFactValue6.TabIndex = 11;
        lblFactValue6.Text = "Value";
        //
        //
        // tlpSkip
        //
        tlpSkip.AutoSize = true;
        tlpSkip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tlpSkip.ColumnCount = 1;
        tlpSkip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpSkip.Controls.Add(lblSkipHeading, 0, 0);
        tlpSkip.Controls.Add(chkSkip0, 0, 1);
        tlpSkip.Controls.Add(chkSkip1, 0, 2);
        tlpSkip.Controls.Add(chkSkip2, 0, 3);
        tlpSkip.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpSkip.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
        tlpSkip.Name = "tlpSkip";
        tlpSkip.RowCount = 4;
        tlpSkip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpSkip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpSkip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpSkip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpSkip.Size = new System.Drawing.Size(680, 130);
        tlpSkip.TabIndex = 2;
        tlpSkip.Visible = false;
        //
        // lblSkipHeading
        //
        lblSkipHeading.AutoSizeMode = AntdUI.TAutoSize.Height;
        lblSkipHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblSkipHeading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblSkipHeading.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblSkipHeading.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblSkipHeading.Name = "lblSkipHeading";
        lblSkipHeading.Size = new System.Drawing.Size(680, 26);
        lblSkipHeading.TabIndex = 0;
        lblSkipHeading.Text = "ติ้กเครื่องที่ไม่ต้องส่งรอบนี้";
        //
        // chkSkip0
        //
        chkSkip0.AutoSizeMode = AntdUI.TAutoSize.Height;
        chkSkip0.Dock = System.Windows.Forms.DockStyle.Fill;
        chkSkip0.Font = new System.Drawing.Font("Segoe UI", 13F);
        chkSkip0.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkSkip0.Margin = new System.Windows.Forms.Padding(4, 2, 0, 2);
        chkSkip0.Name = "chkSkip0";
        chkSkip0.Size = new System.Drawing.Size(676, 30);
        chkSkip0.TabIndex = 1;
        chkSkip0.Visible = false;
        //
        // chkSkip1
        //
        chkSkip1.AutoSizeMode = AntdUI.TAutoSize.Height;
        chkSkip1.Dock = System.Windows.Forms.DockStyle.Fill;
        chkSkip1.Font = new System.Drawing.Font("Segoe UI", 13F);
        chkSkip1.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkSkip1.Margin = new System.Windows.Forms.Padding(4, 2, 0, 2);
        chkSkip1.Name = "chkSkip1";
        chkSkip1.Size = new System.Drawing.Size(676, 30);
        chkSkip1.TabIndex = 2;
        chkSkip1.Visible = false;
        //
        // chkSkip2
        //
        chkSkip2.AutoSizeMode = AntdUI.TAutoSize.Height;
        chkSkip2.Dock = System.Windows.Forms.DockStyle.Fill;
        chkSkip2.Font = new System.Drawing.Font("Segoe UI", 13F);
        chkSkip2.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkSkip2.Margin = new System.Windows.Forms.Padding(4, 2, 0, 2);
        chkSkip2.Name = "chkSkip2";
        chkSkip2.Size = new System.Drawing.Size(676, 30);
        chkSkip2.TabIndex = 3;
        chkSkip2.Visible = false;
        // lblMessage — ข้อความทั่วไป (กล่องที่ยังไม่ได้แยกเป็นหัวข้อ/ค่า)
        //
        lblMessage.AutoSize = true;
        lblMessage.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblMessage.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblMessage.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
        lblMessage.MaximumSize = new System.Drawing.Size(664, 0);
        lblMessage.Name = "lblMessage";
        lblMessage.TabIndex = 2;
        lblMessage.Text = "Message";
        //
        // pnlDetail — กล่องพื้นอ่อน สำหรับรายละเอียดยาว / หมายเหตุ
        //
        pnlDetail.AutoSize = true;
        pnlDetail.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        pnlDetail.BackColor = System.Drawing.Color.FromArgb(237, 243, 249);
        pnlDetail.Controls.Add(lblDetail);
        pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlDetail.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
        pnlDetail.Name = "pnlDetail";
        pnlDetail.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
        pnlDetail.Size = new System.Drawing.Size(664, 52);
        pnlDetail.TabIndex = 3;
        //
        // lblDetail
        //
        lblDetail.AutoSize = true;
        lblDetail.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblDetail.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblDetail.Location = new System.Drawing.Point(16, 12);
        lblDetail.Margin = new System.Windows.Forms.Padding(0);
        lblDetail.MaximumSize = new System.Drawing.Size(632, 0);
        lblDetail.Name = "lblDetail";
        lblDetail.TabIndex = 0;
        lblDetail.Text = "Detail";
        //
        // lblWarning — สิ่งที่ต้องระวังก่อนกด ขึ้นเฉพาะเมื่อมี
        //
        lblWarning.AutoSize = true;
        lblWarning.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblWarning.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        lblWarning.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
        lblWarning.MaximumSize = new System.Drawing.Size(664, 0);
        lblWarning.Name = "lblWarning";
        lblWarning.TabIndex = 4;
        lblWarning.Text = "Warning";
        //
        // flpActions
        //
        flpActions.AutoSize = true;
        flpActions.Controls.Add(btnCancel);
        flpActions.Controls.Add(btnOk);
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Margin = new System.Windows.Forms.Padding(0, 24, 0, 0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(664, 56);
        flpActions.TabIndex = 5;
        flpActions.WrapContents = false;
        //
        // btnCancel
        //
        btnCancel.BorderWidth = 2F;
        btnCancel.DefaultBack = System.Drawing.Color.White;
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnCancel.Margin = new System.Windows.Forms.Padding(0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(150, 56);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "ยกเลิก";
        //
        // btnOk
        //
        btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
        btnOk.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnOk.IconSvg = "CheckOutlined";
        btnOk.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        btnOk.Name = "btnOk";
        btnOk.Radius = 8;
        btnOk.Size = new System.Drawing.Size(220, 56);
        btnOk.TabIndex = 0;
        btnOk.Text = "ยืนยัน";
        btnOk.Type = AntdUI.TTypeMini.Primary;
        //
        // ConfirmDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        BackColor = System.Drawing.Color.White;
        BorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        BorderWidth = 1;
        CancelButton = btnCancel;
        ClientSize = new System.Drawing.Size(720, 520);
        Controls.Add(tlpDialogRoot);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ConfirmDialog";
        Radius = 8;
        Shadow = 12;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "ConfirmDialog";
        tlpDialogRoot.ResumeLayout(false);
        tlpDialogRoot.PerformLayout();
        tlpTitleBar.ResumeLayout(false);
        tlpBody.ResumeLayout(false);
        tlpBody.PerformLayout();
        tlpFacts.ResumeLayout(false);
        tlpSkip.ResumeLayout(false);
        tlpSkip.PerformLayout();
        tlpFacts.PerformLayout();
        pnlDetail.ResumeLayout(false);
        pnlDetail.PerformLayout();
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpDialogRoot;
    private System.Windows.Forms.TableLayoutPanel tlpTitleBar;
    private AntdUI.Label lblTitle;
    private System.Windows.Forms.Button btnTitleClose;
    private System.Windows.Forms.TableLayoutPanel tlpBody;
    private System.Windows.Forms.Label lblHeadline;
    private System.Windows.Forms.TableLayoutPanel tlpFacts;
    private System.Windows.Forms.TableLayoutPanel tlpSkip;
    private AntdUI.Label lblSkipHeading;
    private AntdUI.Checkbox chkSkip0;
    private AntdUI.Checkbox chkSkip1;
    private AntdUI.Checkbox chkSkip2;
    private System.Windows.Forms.Label lblFactCaption1;
    private System.Windows.Forms.Label lblFactValue1;
    private System.Windows.Forms.Label lblFactCaption2;
    private System.Windows.Forms.Label lblFactValue2;
    private System.Windows.Forms.Label lblFactCaption3;
    private System.Windows.Forms.Label lblFactValue3;
    private System.Windows.Forms.Label lblFactCaption4;
    private System.Windows.Forms.Label lblFactValue4;
    private System.Windows.Forms.Label lblFactCaption5;
    private System.Windows.Forms.Label lblFactValue5;
    private System.Windows.Forms.Label lblFactCaption6;
    private System.Windows.Forms.Label lblFactValue6;
    private System.Windows.Forms.Label lblMessage;
    private System.Windows.Forms.Panel pnlDetail;
    private System.Windows.Forms.Label lblDetail;
    private System.Windows.Forms.Label lblWarning;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnCancel;
    private AntdUI.Button btnOk;
}
