namespace InkjetOperator.Views;

partial class StationOptionsUserControl
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
        tlpOptionsRoot = new System.Windows.Forms.TableLayoutPanel();
        lblOptionsTitle = new AntdUI.Label();
        pnlRemoteSend = new AntdUI.Panel();
        lblRemoteSendHeading = new AntdUI.Label();
        tlpRemoteSend = new System.Windows.Forms.TableLayoutPanel();
        chkManualRemoteSend = new AntdUI.Checkbox();
        lblRemoteSendHelp = new AntdUI.Label();
        pnlHoldRound = new AntdUI.Panel();
        tlpHoldRound = new System.Windows.Forms.TableLayoutPanel();
        lblHoldRoundHeading = new AntdUI.Label();
        chkHoldRound = new AntdUI.Checkbox();
        lblHoldRoundHelp = new AntdUI.Label();
        pnlReset = new AntdUI.Panel();
        tlpReset = new System.Windows.Forms.TableLayoutPanel();
        lblResetHeading = new AntdUI.Label();
        btnResetRuntime = new AntdUI.Button();
        lblResetHelp = new AntdUI.Label();
        tlpOptionsRoot.SuspendLayout();
        pnlRemoteSend.SuspendLayout();
        pnlHoldRound.SuspendLayout();
        pnlReset.SuspendLayout();
        tlpReset.SuspendLayout();
        tlpHoldRound.SuspendLayout();
        tlpRemoteSend.SuspendLayout();
        SuspendLayout();
        //
        // tlpOptionsRoot
        //
        tlpOptionsRoot.ColumnCount = 1;
        tlpOptionsRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Controls.Add(lblOptionsTitle, 0, 0);
        tlpOptionsRoot.Controls.Add(pnlRemoteSend, 0, 1);
        tlpOptionsRoot.Controls.Add(pnlHoldRound, 0, 2);
        tlpOptionsRoot.Controls.Add(pnlReset, 0, 3);
        tlpOptionsRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOptionsRoot.Location = new System.Drawing.Point(32, 32);
        tlpOptionsRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpOptionsRoot.Name = "tlpOptionsRoot";
        tlpOptionsRoot.RowCount = 5;
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 252F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 214F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Size = new System.Drawing.Size(1216, 736);
        tlpOptionsRoot.TabIndex = 0;
        //
        // lblOptionsTitle
        //
        lblOptionsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOptionsTitle.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
        lblOptionsTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblOptionsTitle.Location = new System.Drawing.Point(0, 0);
        lblOptionsTitle.Margin = new System.Windows.Forms.Padding(0);
        lblOptionsTitle.Name = "lblOptionsTitle";
        lblOptionsTitle.Size = new System.Drawing.Size(1216, 66);
        lblOptionsTitle.TabIndex = 0;
        lblOptionsTitle.Text = "ตัวเลือกหน้างาน";
        //
        // pnlRemoteSend
        //
        pnlRemoteSend.Back = System.Drawing.Color.White;
        pnlRemoteSend.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlRemoteSend.BorderWidth = 2F;
        pnlRemoteSend.Controls.Add(tlpRemoteSend);
        pnlRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlRemoteSend.Location = new System.Drawing.Point(0, 66);
        pnlRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        pnlRemoteSend.Name = "pnlRemoteSend";
        pnlRemoteSend.Padding = new System.Windows.Forms.Padding(24);
        pnlRemoteSend.Radius = 10;
        pnlRemoteSend.Size = new System.Drawing.Size(1216, 210);
        pnlRemoteSend.TabIndex = 1;
        //
        // tlpRemoteSend
        //
        tlpRemoteSend.ColumnCount = 1;
        tlpRemoteSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.Controls.Add(lblRemoteSendHeading, 0, 0);
        tlpRemoteSend.Controls.Add(chkManualRemoteSend, 0, 1);
        tlpRemoteSend.Controls.Add(lblRemoteSendHelp, 0, 2);
        tlpRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRemoteSend.Location = new System.Drawing.Point(24, 24);
        tlpRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        tlpRemoteSend.Name = "tlpRemoteSend";
        tlpRemoteSend.RowCount = 3;
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.Size = new System.Drawing.Size(1168, 162);
        tlpRemoteSend.TabIndex = 0;
        //
        // pnlHoldRound
        //
        pnlHoldRound.Back = System.Drawing.Color.White;
        pnlHoldRound.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlHoldRound.BorderWidth = 2F;
        pnlHoldRound.Controls.Add(tlpHoldRound);
        pnlHoldRound.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlHoldRound.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlHoldRound.Name = "pnlHoldRound";
        pnlHoldRound.Padding = new System.Windows.Forms.Padding(24);
        pnlHoldRound.Radius = 10;
        pnlHoldRound.Size = new System.Drawing.Size(1216, 236);
        pnlHoldRound.TabIndex = 2;
        //
        // tlpHoldRound
        //
        tlpHoldRound.ColumnCount = 1;
        tlpHoldRound.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHoldRound.Controls.Add(lblHoldRoundHeading, 0, 0);
        tlpHoldRound.Controls.Add(chkHoldRound, 0, 1);
        tlpHoldRound.Controls.Add(lblHoldRoundHelp, 0, 2);
        tlpHoldRound.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpHoldRound.Margin = new System.Windows.Forms.Padding(0);
        tlpHoldRound.Name = "tlpHoldRound";
        tlpHoldRound.RowCount = 3;
        tlpHoldRound.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpHoldRound.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpHoldRound.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHoldRound.Size = new System.Drawing.Size(1168, 188);
        tlpHoldRound.TabIndex = 0;
        //
        // lblHoldRoundHeading
        //
        lblHoldRoundHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHoldRoundHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblHoldRoundHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblHoldRoundHeading.Margin = new System.Windows.Forms.Padding(0);
        lblHoldRoundHeading.Name = "lblHoldRoundHeading";
        lblHoldRoundHeading.Size = new System.Drawing.Size(1168, 42);
        lblHoldRoundHeading.TabIndex = 0;
        lblHoldRoundHeading.Text = "งานที่เข้าเครื่องเดิมสองรอบ (marking 22)";
        //
        // chkHoldRound
        //
        chkHoldRound.Dock = System.Windows.Forms.DockStyle.Fill;
        chkHoldRound.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkHoldRound.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkHoldRound.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkHoldRound.Name = "chkHoldRound";
        chkHoldRound.Size = new System.Drawing.Size(1168, 32);
        chkHoldRound.TabIndex = 1;
        chkHoldRound.Text = "ถือเครื่องไว้ให้รอบสอง ห้ามงานอื่นแทรก";
        //
        // lblHoldRoundHelp
        //
        lblHoldRoundHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHoldRoundHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblHoldRoundHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblHoldRoundHelp.Margin = new System.Windows.Forms.Padding(0);
        lblHoldRoundHelp.Name = "lblHoldRoundHelp";
        lblHoldRoundHelp.Size = new System.Drawing.Size(1168, 102);
        lblHoldRoundHelp.TabIndex = 2;
        lblHoldRoundHelp.Text = "ก. ถือเครื่องไว้ (ค่าเริ่มต้น ตามที่ตกลงกับหัวหน้างาน) — พ่นรอบแรกเสร็จแล้วกดปุ่มหน้างาน เครื่องยังเป็นของงานใบนี้อยู่ งานใบอื่นที่รอคิวเครื่องนี้แทรกไม่ได้ จนกว่าชิ้นงานจะกลับมาพ่นรอบสองเสร็จ เครื่องจะจอดรอระหว่างที่คนเอางานออกไปติด shim นอกไลน์\r\nข. ปล่อยเครื่อง (ติ๊กออก) — กดปุ่มหน้างานแล้วเครื่องว่างทันที งานใบอื่นแทรกเข้ามาทำได้ เครื่องไม่จอดเปล่า แต่พอชิ้นงานกลับมา รอบสองต้องไปต่อท้ายคิว อาจค้างกลางไลน์นานกว่าที่คิด\r\nมีผลเฉพาะงานที่เข้าเครื่องเดิมมากกว่าหนึ่งรอบ (marking 22) งานอื่นไม่เกี่ยว";
        //
        // pnlReset
        //
        pnlReset.Back = System.Drawing.Color.White;
        pnlReset.BorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        pnlReset.BorderWidth = 2F;
        pnlReset.Controls.Add(tlpReset);
        pnlReset.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlReset.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlReset.Name = "pnlReset";
        pnlReset.Padding = new System.Windows.Forms.Padding(24);
        pnlReset.Radius = 10;
        pnlReset.Size = new System.Drawing.Size(1216, 198);
        pnlReset.TabIndex = 3;
        //
        // tlpReset
        //
        tlpReset.ColumnCount = 1;
        tlpReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpReset.Controls.Add(lblResetHeading, 0, 0);
        tlpReset.Controls.Add(lblResetHelp, 0, 1);
        tlpReset.Controls.Add(btnResetRuntime, 0, 2);
        tlpReset.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpReset.Margin = new System.Windows.Forms.Padding(0);
        tlpReset.Name = "tlpReset";
        tlpReset.RowCount = 3;
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpReset.Size = new System.Drawing.Size(1168, 150);
        tlpReset.TabIndex = 0;
        //
        // lblResetHeading
        //
        lblResetHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblResetHeading.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblResetHeading.Margin = new System.Windows.Forms.Padding(0);
        lblResetHeading.Name = "lblResetHeading";
        lblResetHeading.Size = new System.Drawing.Size(1168, 42);
        lblResetHeading.TabIndex = 0;
        lblResetHeading.Text = "รีเซ็ตกลับเป็นค่าเริ่มต้น";
        //
        // lblResetHelp
        //
        lblResetHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblResetHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblResetHelp.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblResetHelp.Name = "lblResetHelp";
        lblResetHelp.Size = new System.Drawing.Size(1168, 44);
        lblResetHelp.TabIndex = 1;
        lblResetHelp.Text = "ล้างทุกอย่างที่บอกว่างานเดินไปถึงไหนแล้ว แล้วให้ทุกใบกลับไปเป็นรอเริ่ม ใช้ตอนทดสอบเมื่ออยากเริ่มนับหนึ่งใหม่ทั้งกระดาน\r\nลบ: คิวเครื่องทุกแถว · ประวัติคำสั่งที่ส่งเข้าเครื่องทุกแถว · ธงคำขอที่ ST3 ฝากไว้\r\nไม่แตะ: ตัวงาน ข้อมูล pattern ข้อความ UV แผนการผลิต และค่าแคลมป์ ยังอยู่ครบเหมือนเดิม\r\nย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่ออยู่กับ backend เดียวกัน ไม่ใช่แค่เครื่องนี้";
        //
        // btnResetRuntime
        //
        btnResetRuntime.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnResetRuntime.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnResetRuntime.ForeColor = System.Drawing.Color.White;
        btnResetRuntime.Margin = new System.Windows.Forms.Padding(0);
        btnResetRuntime.Name = "btnResetRuntime";
        btnResetRuntime.Radius = 8;
        btnResetRuntime.Size = new System.Drawing.Size(300, 52);
        btnResetRuntime.TabIndex = 2;
        btnResetRuntime.Text = "รีเซ็ตกลับเป็นค่าเริ่มต้น";
        btnResetRuntime.Type = AntdUI.TTypeMini.Error;
        //
        // lblRemoteSendHeading
        //
        lblRemoteSendHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblRemoteSendHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblRemoteSendHeading.Location = new System.Drawing.Point(0, 0);
        lblRemoteSendHeading.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHeading.Name = "lblRemoteSendHeading";
        lblRemoteSendHeading.Size = new System.Drawing.Size(1168, 42);
        lblRemoteSendHeading.TabIndex = 0;
        lblRemoteSendHeading.Text = "ปุ่มสำรองของปุ่มกดหน้างาน (ST3)";
        //
        // chkManualRemoteSend
        //
        chkManualRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        chkManualRemoteSend.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkManualRemoteSend.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkManualRemoteSend.Location = new System.Drawing.Point(0, 42);
        chkManualRemoteSend.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkManualRemoteSend.Name = "chkManualRemoteSend";
        chkManualRemoteSend.Size = new System.Drawing.Size(1168, 32);
        chkManualRemoteSend.TabIndex = 1;
        chkManualRemoteSend.Text = "โชว์ปุ่ม \"ขอให้ ST1 ส่ง\" ในหน้า Order Detail";
        //
        // lblRemoteSendHelp
        //
        lblRemoteSendHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHelp.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblRemoteSendHelp.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblRemoteSendHelp.Location = new System.Drawing.Point(0, 86);
        lblRemoteSendHelp.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHelp.Name = "lblRemoteSendHelp";
        lblRemoteSendHelp.Size = new System.Drawing.Size(1168, 76);
        lblRemoteSendHelp.TabIndex = 2;
        lblRemoteSendHelp.Text = "ใช้แทนปุ่มกดหน้างานตอนปุ่มกดหรือ PLC ใช้ไม่ได้ ขึ้นเฉพาะเครื่องของ ST3 และเฉพาะงานที่ส่งขั้นแรกไปแล้วและยังเหลือขั้นถัดไป\r\nก่อนกด ต้องแน่ใจว่าชิ้นงานอยู่ในเครื่องพร้อมพิมพ์แล้ว เพราะการกดที่จอไม่ได้ยืนยันเรื่องนี้เหมือนการเดินไปกดปุ่มหน้าเครื่อง";
        lblRemoteSendHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // StationOptionsUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpOptionsRoot);
        Name = "StationOptionsUserControl";
        Padding = new System.Windows.Forms.Padding(32);
        Size = new System.Drawing.Size(1280, 800);
        tlpOptionsRoot.ResumeLayout(false);
        tlpReset.ResumeLayout(false);
        pnlReset.ResumeLayout(false);
        tlpHoldRound.ResumeLayout(false);
        pnlHoldRound.ResumeLayout(false);
        pnlRemoteSend.ResumeLayout(false);
        tlpRemoteSend.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOptionsRoot;
    private AntdUI.Label lblOptionsTitle;
    private AntdUI.Panel pnlRemoteSend;
    private AntdUI.Label lblRemoteSendHeading;
    private AntdUI.Panel pnlHoldRound;
    private System.Windows.Forms.TableLayoutPanel tlpHoldRound;
    private AntdUI.Label lblHoldRoundHeading;
    private AntdUI.Checkbox chkHoldRound;
    private AntdUI.Label lblHoldRoundHelp;
    private AntdUI.Panel pnlReset;
    private System.Windows.Forms.TableLayoutPanel tlpReset;
    private AntdUI.Label lblResetHeading;
    private AntdUI.Button btnResetRuntime;
    private AntdUI.Label lblResetHelp;
    private System.Windows.Forms.TableLayoutPanel tlpRemoteSend;
    private AntdUI.Checkbox chkManualRemoteSend;
    private AntdUI.Label lblRemoteSendHelp;
}
