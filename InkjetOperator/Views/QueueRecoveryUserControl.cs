namespace InkjetOperator.Views;

public partial class QueueRecoveryUserControl : UserControl
{
    public QueueRecoveryUserControl() => InitializeComponent();

    public void SetJob(string text) => lblJob.Text = text;
    public string Outcome => radioSent.Checked ? "sent" : "not_sent";
    public string OperatorName => txtOperator.Text.Trim();
    public string Reason => txtReason.Text.Trim();

    public bool ValidateRecovery()
    {
        string? error = !chkStopped.Checked ? "ต้องหยุดผู้ส่งอื่นและตรวจเครื่องก่อน"
            : radioSent.Checked == radioNotSent.Checked ? "เลือกผลตรวจเครื่องหนึ่งข้อ"
            : OperatorName.Length is < 1 or > 100 ? "กรอกชื่อผู้ตรวจไม่เกิน 100 ตัวอักษร"
            : Reason.Length is < 5 or > 1000 ? "กรอกสิ่งที่ตรวจพบ 5–1,000 ตัวอักษร" : null;
        lblError.Text = error ?? "";
        return error == null;
    }
}
