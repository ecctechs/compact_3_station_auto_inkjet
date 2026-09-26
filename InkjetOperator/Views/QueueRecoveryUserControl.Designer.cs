namespace InkjetOperator.Views;

partial class QueueRecoveryUserControl
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        layout = new TableLayoutPanel();
        lblJob = new AntdUI.Label();
        chkStopped = new AntdUI.Checkbox();
        radioSent = new AntdUI.Radio();
        radioNotSent = new AntdUI.Radio();
        txtOperator = new AntdUI.Input();
        txtReason = new AntdUI.Input();
        lblError = new AntdUI.Label();
        layout.SuspendLayout();
        SuspendLayout();
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(16);
        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowCount = 7;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Controls.Add(lblJob, 0, 0);
        layout.Controls.Add(chkStopped, 0, 1);
        layout.Controls.Add(radioSent, 0, 2);
        layout.Controls.Add(radioNotSent, 0, 3);
        layout.Controls.Add(txtOperator, 0, 4);
        layout.Controls.Add(txtReason, 0, 5);
        layout.Controls.Add(lblError, 0, 6);
        lblJob.Dock = DockStyle.Fill;
        lblJob.ForeColor = Color.FromArgb(36, 71, 101);
        lblJob.Text = "ตรวจผลส่งของเครื่องและงานที่เลือก";
        chkStopped.Dock = DockStyle.Fill;
        chkStopped.Text = "ตรวจเครื่องแล้ว และหยุดโปรแกรมตัวอื่นที่อาจกำลังส่งงาน\nหากยังไม่แน่ใจ ให้ยกเลิกและคงคิวไว้";
        radioSent.Dock = DockStyle.Fill;
        radioSent.Text = "เครื่องรับงานและพร้อมพิมพ์แล้ว — บันทึกผล ไม่ส่งซ้ำ";
        radioNotSent.Dock = DockStyle.Fill;
        radioNotSent.Text = "ยืนยันว่าไม่เริ่มพิมพ์และพร้อมส่งใหม่ — คืนเป็นรอคิว";
        txtOperator.Dock = DockStyle.Fill;
        txtOperator.PlaceholderText = "ชื่อผู้ตรวจ";
        txtReason.Dock = DockStyle.Fill;
        txtReason.Multiline = true;
        txtReason.PlaceholderText = "สิ่งที่ตรวจพบ / เหตุผลที่ยืนยันผลนี้";
        lblError.Dock = DockStyle.Fill;
        lblError.ForeColor = Color.FromArgb(185, 28, 28);
        lblError.Text = "";
        Controls.Add(layout);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 12F);
        Size = new Size(720, 470);
        Name = "QueueRecoveryUserControl";
        layout.ResumeLayout(false);
        ResumeLayout(false);
    }

    private TableLayoutPanel layout;
    private AntdUI.Label lblJob;
    private AntdUI.Checkbox chkStopped;
    private AntdUI.Radio radioSent;
    private AntdUI.Radio radioNotSent;
    private AntdUI.Input txtOperator;
    private AntdUI.Input txtReason;
    private AntdUI.Label lblError;
}
