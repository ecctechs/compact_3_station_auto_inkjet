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
        tlpOptionsRoot.SuspendLayout();
        pnlRemoteSend.SuspendLayout();
        tlpRemoteSend.SuspendLayout();
        SuspendLayout();
        //
        // tlpOptionsRoot
        //
        tlpOptionsRoot.ColumnCount = 1;
        tlpOptionsRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Controls.Add(lblOptionsTitle, 0, 0);
        tlpOptionsRoot.Controls.Add(pnlRemoteSend, 0, 1);
        tlpOptionsRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOptionsRoot.Location = new System.Drawing.Point(32, 32);
        tlpOptionsRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpOptionsRoot.Name = "tlpOptionsRoot";
        tlpOptionsRoot.RowCount = 3;
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
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
        pnlRemoteSend.ResumeLayout(false);
        tlpRemoteSend.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOptionsRoot;
    private AntdUI.Label lblOptionsTitle;
    private AntdUI.Panel pnlRemoteSend;
    private AntdUI.Label lblRemoteSendHeading;
    private System.Windows.Forms.TableLayoutPanel tlpRemoteSend;
    private AntdUI.Checkbox chkManualRemoteSend;
    private AntdUI.Label lblRemoteSendHelp;
}
