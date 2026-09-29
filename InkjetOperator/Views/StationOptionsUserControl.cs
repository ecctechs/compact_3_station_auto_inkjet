using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class StationOptionsUserControl : UserControl
{
    public StationOptionsUserControl()
    {
        InitializeComponent();

        chkManualRemoteSend.Checked = StationService.ManualRemoteSendEnabled;
        chkManualRemoteSend.CheckedChanged += ManualRemoteSend_CheckedChanged;

        chkHoldRound.Checked = StationService.HoldForNextRound;
        chkHoldRound.CheckedChanged += HoldRound_CheckedChanged;

        _processTabsSaved = StationService.ProcessTabs;
        ProcessTabsRadio(_processTabsSaved).Checked = true;
        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged += ProcessTabs_CheckedChanged;

        btnResetRuntime.Click += async (_, _) => await ResetRuntimeAsync();
    }

    private StationService.ProcessTabsMode _processTabsSaved;

    private AntdUI.Radio[] ProcessTabsRadios =>
        [rdoProcessTabsStations, rdoProcessTabsDev, rdoProcessTabsOff];

    private AntdUI.Radio ProcessTabsRadio(StationService.ProcessTabsMode mode) => mode switch
    {
        StationService.ProcessTabsMode.Stations => rdoProcessTabsStations,
        StationService.ProcessTabsMode.Off => rdoProcessTabsOff,
        _ => rdoProcessTabsDev,
    };

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
                StationService.ProcessTabsMode.Stations => "เปิดแท็บ Online / Offline ที่ ST1 และ ST3 แล้ว",
                StationService.ProcessTabsMode.Off => "ปิดแท็บ Online / Offline ทุกเครื่องแล้ว",
                _ => "แท็บ Online / Offline เห็นเฉพาะโหมด Dev",
            });
            return;
        }

        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged -= ProcessTabs_CheckedChanged;
        ProcessTabsRadio(_processTabsSaved).Checked = true;
        foreach (var radio in ProcessTabsRadios) radio.CheckedChanged += ProcessTabs_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }

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

    private void HoldRound_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(StationService.HoldForNextRoundKey, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "ถือเครื่องไว้ให้รอบสอง — งานอื่นแทรกไม่ได้จนกว่าจะพ่นรอบสองเสร็จ"
                : "ปล่อยเครื่องทันทีที่กดปุ่ม — งานอื่นแทรกได้ รอบสองต่อท้ายคิว");
            return;
        }

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

        chkManualRemoteSend.CheckedChanged -= ManualRemoteSend_CheckedChanged;
        chkManualRemoteSend.Checked = !e.Value;
        chkManualRemoteSend.CheckedChanged += ManualRemoteSend_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }
}
