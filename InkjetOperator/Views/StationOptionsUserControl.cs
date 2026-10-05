using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// สวิตช์เปิด/ปิดของที่ไม่ได้เปิดไว้ตลอด — หน้านี้เห็นเฉพาะโหมดทดสอบ
///
/// <para>
/// มีสามรายการ — ปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ของ ST3 ซึ่งเป็น
/// ทางออกตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ · ใครเห็นตัวกรอง In-line / Off-line
/// ในหน้า Order List · และปุ่มรีเซ็ตทุกอย่างกลับเป็นค่าเริ่มต้นสำหรับทดสอบ
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

        _processTabsSaved = StationService.ProcessTabs;
        ProcessTabsRadio(_processTabsSaved).Checked = true;
        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged += ProcessTabs_CheckedChanged;

        btnResetRuntime.Click += async (_, _) => await ResetRuntimeAsync();
    }

    /// <summary>ค่าที่บันทึกลงไฟล์ได้ล่าสุด — ใช้ดีดตัวเลือกกลับเมื่อบันทึกไม่ผ่าน</summary>
    private StationService.ProcessTabsMode _processTabsSaved;

    private AntdUI.Radio[] ProcessTabsRadios =>
        [rdoProcessTabsStations, rdoProcessTabsDev, rdoProcessTabsOff];

    private AntdUI.Radio ProcessTabsRadio(StationService.ProcessTabsMode mode) => mode switch
    {
        StationService.ProcessTabsMode.Stations => rdoProcessTabsStations,
        StationService.ProcessTabsMode.Off => rdoProcessTabsOff,
        _ => rdoProcessTabsDev,
    };

    /// <summary>
    /// ใครเห็นตัวกรอง In-line / Off-line ในหน้า Order List — เซฟทันทีที่เลือก
    ///
    /// <para>
    /// Radio ที่ถูกเลือกทำให้ตัวอื่นในกลุ่มหลุดเอง ซึ่งยิงเหตุการณ์ออกมาด้วยค่า false
    /// รับเฉพาะตัวที่ถูกเลือก ไม่งั้นการเลือกหนึ่งครั้งจะเขียนไฟล์สามรอบ
    /// </para>
    /// </summary>
    private void ProcessTabs_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (!e.Value) return;

        var mode = ReferenceEquals(sender, rdoProcessTabsStations) ? StationService.ProcessTabsMode.Stations
            : ReferenceEquals(sender, rdoProcessTabsOff) ? StationService.ProcessTabsMode.Off
            : StationService.ProcessTabsMode.DevOnly;

        if (mode == _processTabsSaved) return;

        if (CustomSettingsManager.Write(StationService.ProcessTabsKey, StationService.ProcessTabsValue(mode)))
        {
            _processTabsSaved = mode;
            Notify.Success(this, mode switch
            {
                StationService.ProcessTabsMode.Stations => "เปิดตัวกรอง In-line / Off-line ที่ ST1 และ ST3 แล้ว",
                StationService.ProcessTabsMode.Off => "ปิดตัวกรอง In-line / Off-line ทุกเครื่องแล้ว",
                _ => "ตัวกรอง In-line / Off-line เห็นเฉพาะโหมด Dev",
            });
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ตัวเลือกที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged -= ProcessTabs_CheckedChanged;
        ProcessTabsRadio(_processTabsSaved).Checked = true;
        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged += ProcessTabs_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
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
