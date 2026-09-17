using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// สวิตช์เปิด/ปิดของที่ไม่ได้เปิดไว้ตลอด — หน้านี้เห็นเฉพาะโหมดทดสอบ
///
/// <para>
/// ตอนนี้มีรายการเดียวคือปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ของ ST3
/// ซึ่งเป็นทางออกตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ ปกติปิดไว้เพราะการกดที่จอ
/// ไม่ได้ยืนยันว่าชิ้นงานอยู่ในเครื่องแล้วจริงเหมือนการเดินไปกดปุ่มหน้าเครื่อง
/// </para>
/// <para>
/// เซฟทันทีที่กด ไม่มีปุ่ม Save — มีช่องเดียวและเป็นค่า เปิด/ปิด กดแล้วลืมกดเซฟ
/// จะกลายเป็นว่าเดินไปหน้างานแล้วปุ่มไม่ขึ้นโดยไม่รู้สาเหตุ
/// </para>
/// </summary>
public partial class StationOptionsUserControl : UserControl
{
    public StationOptionsUserControl()
    {
        InitializeComponent();

        chkManualRemoteSend.Checked = StationService.ManualRemoteSendEnabled;
        chkManualRemoteSend.CheckedChanged += ManualRemoteSend_CheckedChanged;
    }

    private void ManualRemoteSend_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(StationService.ManualRemoteSendKey, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิดปุ่มสำรองแล้ว — เปิดหน้า Order Detail ใหม่จะเห็นปุ่ม"
                : "ปิดปุ่มสำรองแล้ว");
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ติ๊กที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        chkManualRemoteSend.CheckedChanged -= ManualRemoteSend_CheckedChanged;
        chkManualRemoteSend.Checked = !e.Value;
        chkManualRemoteSend.CheckedChanged += ManualRemoteSend_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }
}
