using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// สวิตช์เปิด/ปิดของที่ไม่ได้เปิดไว้ตลอด — หน้านี้เห็นเฉพาะโหมดทดสอบ
///
/// <para>
/// มีสามรายการ — ปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ของ ST3 ซึ่งเป็น
/// ทางออกตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ · การถือเครื่องไว้ระหว่างรอบของงาน
/// ที่เข้าเครื่องเดิมสองรอบ · และปุ่มรีเซ็ตทุกอย่างกลับเป็นค่าเริ่มต้นสำหรับทดสอบ
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

        btnResetRuntime.Click += async (_, _) => await ResetRuntimeAsync();
    }

    /// <summary>
    /// ล้างร่องรอยการเดินงานทั้งหมด ให้ทุกใบกลับไปเป็นรอเริ่ม
    ///
    /// <para>
    /// ถามยืนยันก่อนเสมอ และบอกให้ครบว่าอะไรจะหายอะไรจะอยู่ เพราะย้อนกลับไม่ได้
    /// และมีผลกับทุกเครื่องที่ต่ออยู่กับ backend เดียวกัน ไม่ใช่แค่เครื่องที่กด
    /// </para>
    /// </summary>
    private async Task ResetRuntimeAsync()
    {
        if (!Confirm.Ask(this, "รีเซ็ตกลับเป็นค่าเริ่มต้น",
                "จะล้างของพวกนี้ทิ้งทั้งหมด" + Environment.NewLine
                + "  · คิวเครื่องทุกแถว" + Environment.NewLine
                + "  · ประวัติคำสั่งที่ส่งเข้าเครื่องทุกแถว" + Environment.NewLine
                + "  · ธงคำขอที่ ST3 ฝากไว้" + Environment.NewLine + Environment.NewLine
                + "แล้วตั้งสถานะทุกงานกลับเป็นรอเริ่ม" + Environment.NewLine + Environment.NewLine
                + "ตัวงาน ข้อมูล pattern ข้อความ UV แผนการผลิต และค่าแคลมป์ ยังอยู่ครบ"
                + Environment.NewLine + Environment.NewLine
                + "ย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน"
                + Environment.NewLine + Environment.NewLine + "ยืนยันหรือไม่?"))
            return;

        btnResetRuntime.Enabled = false;
        var originalText = btnResetRuntime.Text;
        btnResetRuntime.Text = "กำลังล้าง...";
        try
        {
            var api = new ApiClient(
                $"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000");

            var (result, error) = await api.ResetRuntimeAsync();
            if (IsDisposed) return;

            if (result == null)
            {
                Notify.ErrorModal(this, "รีเซ็ตไม่สำเร็จ",
                    "ยังไม่มีอะไรถูกล้าง" + Environment.NewLine + Environment.NewLine
                    + (error ?? "ติดต่อ backend ไม่ได้"));
                return;
            }

            Notify.SuccessDetail(this, "รีเซ็ตเรียบร้อย",
                $"ตั้งสถานะกลับเป็นรอเริ่ม {result.Jobs} งาน" + Environment.NewLine
                + $"ลบคิวเครื่อง {result.QueueRemoved} แถว" + Environment.NewLine
                + $"ลบประวัติคำสั่ง {result.CommandsRemoved} แถว" + Environment.NewLine + Environment.NewLine
                + "กลับไปหน้า Order List แล้วตารางจะอัปเดตเองในรอบถัดไป");
        }
        finally
        {
            if (!IsDisposed)
            {
                btnResetRuntime.Text = originalText;
                btnResetRuntime.Enabled = true;
            }
        }
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
