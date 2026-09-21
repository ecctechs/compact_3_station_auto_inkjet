using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// สวิตช์เปิด/ปิดของที่ไม่ได้เปิดไว้ตลอด — หน้านี้เห็นเฉพาะโหมดทดสอบ
///
/// <para>
/// มีสองรายการ — ปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ของ ST3 ซึ่งเป็น
/// ทางออกตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ และการถือเครื่องไว้ระหว่างรอบของงาน
/// ที่เข้าเครื่องเดิมสองรอบ
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

        chkHoldRound.Checked = StationService.HoldForNextRound;
        chkHoldRound.CheckedChanged += HoldRound_CheckedChanged;
    }

    /// <summary>
    /// งานที่เข้าเครื่องเดิมสองรอบ จะถือเครื่องไว้ระหว่างรอบหรือปล่อยให้คนอื่นแทรก
    ///
    /// <para>
    /// ค่าเริ่มต้นคือถือไว้ ตามที่ตกลงกับหัวหน้างาน เพราะ marking 22 เป็นงานพิเศษ
    /// ที่ทำนาน ๆ ที ยอมให้เครื่องจอดรอดีกว่าเสี่ยงให้ชิ้นงานค้างกลางไลน์
    /// </para>
    /// </summary>
    private void HoldRound_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(StationService.HoldForNextRoundKey, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "ถือเครื่องไว้ให้รอบสอง — งานอื่นแทรกไม่ได้จนกว่าจะพ่นรอบสองเสร็จ"
                : "ปล่อยเครื่องทันทีที่กดปุ่ม — งานอื่นแทรกได้ รอบสองต่อท้ายคิว");
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ติ๊กที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        chkHoldRound.CheckedChanged -= HoldRound_CheckedChanged;
        chkHoldRound.Checked = !e.Value;
        chkHoldRound.CheckedChanged += HoldRound_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
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
