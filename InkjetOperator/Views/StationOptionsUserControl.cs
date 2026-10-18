using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// สวิตช์เปิด/ปิดของที่ไม่ได้เปิดไว้ตลอด — หน้านี้เห็นเฉพาะโหมดทดสอบ
///
/// <para>
/// มีหกรายการ — ปุ่มสำรอง "ขอให้ ST1 ส่ง" ในหน้า Order Detail ของ ST3 ซึ่งเป็น
/// ทางออกตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ · ใครเห็นตัวกรอง In-line / Off-line
/// ในหน้า Order List · Mockup สถานะการเชื่อมต่อสำหรับถ่ายรูปคู่มือ · โหมดส่งไม่ครบ ไม่ส่งเลย
/// ปุ่มรีเซ็ตทุกอย่างกลับเป็นค่าเริ่มต้นสำหรับทดสอบ และชุดข้อมูลงานทดสอบ (บันทึก / นำเข้า / ล้าง)
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

        chkMockupStatus.Checked = StatusMockup.Enabled;
        chkMockupStatus.CheckedChanged += MockupStatus_CheckedChanged;

        chkAllOrNothing.Checked = AllOrNothingSend.Enabled;
        chkAllOrNothing.CheckedChanged += AllOrNothing_CheckedChanged;

        chkUv2Pick.Checked = Uv2PickAtSt3.Enabled;
        chkUv2Pick.CheckedChanged += Uv2Pick_CheckedChanged;

        chkUvRepeat.Checked = UvVariantService.Enabled;
        chkUvRepeat.CheckedChanged += UvRepeat_CheckedChanged;

        chkScanTab.Checked = ScanTabAtSt1.Enabled;
        chkScanTab.CheckedChanged += ScanTab_CheckedChanged;

        btnResetRuntime.Click += async (_, _) => await ResetRuntimeAsync();

        btnSaveJobData.Click += async (_, _) => await SaveJobDataAsync();
        btnImportJobData.Click += async (_, _) => await ImportJobDataAsync();
        btnClearJobData.Click += async (_, _) => await ClearJobDataAsync();
    }

    /// <summary>
    /// เปิด/ปิด Mockup สถานะการเชื่อมต่อ — เซฟทันทีที่ติ๊ก รายละเอียดอยู่ที่ <see cref="StatusMockup"/>
    /// </summary>
    private void MockupStatus_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(StatusMockup.Key, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิด Mockup สถานะแล้ว — กด เช็คการเชื่อมต่อ / Check Status ในแต่ละหน้าเพื่อให้ขึ้นสำเร็จ ถ่ายเสร็จอย่าลืมกลับมาปิด"
                : "ปิด Mockup สถานะแล้ว — กลับไปเช็คการเชื่อมต่อจริง");
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ติ๊กที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        chkMockupStatus.CheckedChanged -= MockupStatus_CheckedChanged;
        chkMockupStatus.Checked = !e.Value;
        chkMockupStatus.CheckedChanged += MockupStatus_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }

    /// <summary>
    /// เปิด/ปิดโหมด "ส่งไม่ครบ ไม่ส่งเลย" — เซฟทันทีที่ติ๊ก รายละเอียดอยู่ที่ <see cref="AllOrNothingSend"/>
    /// </summary>
    private void AllOrNothing_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(AllOrNothingSend.Key, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิดโหมดส่งไม่ครบ ไม่ส่งเลยแล้ว — มีผลกับการส่งรอบถัดไป (ต้องเปิดที่เครื่อง ST1)"
                : "ปิดโหมดส่งไม่ครบ ไม่ส่งเลยแล้ว — กลับไปใช้กฎเดิม (ผลไม่แน่นอนถือคิวไว้ตรวจ)");
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ติ๊กที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        chkAllOrNothing.CheckedChanged -= AllOrNothing_CheckedChanged;
        chkAllOrNothing.Checked = !e.Value;
        chkAllOrNothing.CheckedChanged += AllOrNothing_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }

    /// <summary>
    /// เปิด/ปิด "รุ่นย่อย UV2 ให้ ST3 เลือก" — เซฟทันทีที่ติ๊ก รายละเอียดอยู่ที่ <see cref="Uv2PickAtSt3"/>
    /// </summary>
    private void Uv2Pick_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(Uv2PickAtSt3.Key, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิดแล้ว — งานที่กดเริ่มต่อจากนี้ UV2 ที่มีรุ่นย่อยให้ ST3 เลือก (ต้องเปิดที่เครื่อง ST1)"
                : "ปิดแล้ว — ST1 เลือกรุ่นทุกเครื่องตอนกดเริ่มงาน เหมือนเดิม");
            return;
        }

        // เขียนไฟล์ไม่ผ่าน ติ๊กที่ค้างอยู่จะโกหกว่าเซฟแล้ว ต้องดีดกลับ
        chkUv2Pick.CheckedChanged -= Uv2Pick_CheckedChanged;
        chkUv2Pick.Checked = !e.Value;
        chkUv2Pick.CheckedChanged += Uv2Pick_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }

    /// <summary>เปิด/ปิดการพิมพ์รุ่นย่อย UV เพิ่มให้งานใบเดิม — ดู <see cref="UvVariantService"/></summary>
    private void UvRepeat_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(UvVariantService.Key, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิดแล้ว — พิมพ์เสร็จแล้วกด เลือกรุ่น UV ในแถวงานเพื่อพิมพ์รุ่นถัดไปได้"
                : "ปิดแล้ว — เลือกรุ่นครั้งเดียวตอนกดเริ่มงาน เหมือนเดิม");
            return;
        }

        chkUvRepeat.CheckedChanged -= UvRepeat_CheckedChanged;
        chkUvRepeat.Checked = !e.Value;
        chkUvRepeat.CheckedChanged += UvRepeat_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
    }

    /// <summary>เปิด/ปิดแท็บ Scan Barcode ที่ ST1 — ดู <see cref="ScanTabAtSt1"/></summary>
    private void ScanTab_CheckedChanged(object? sender, AntdUI.BoolEventArgs e)
    {
        if (CustomSettingsManager.Write(ScanTabAtSt1.Key, e.Value ? "1" : "0"))
        {
            Notify.Success(this, e.Value
                ? "เปิดแล้ว — ST1 จะมีแท็บ Scan Barcode หลังเปิดโปรแกรมใหม่"
                : "ปิดแล้ว — ST1 ไม่มีแท็บ Scan Barcode หลังเปิดโปรแกรมใหม่");
            return;
        }

        chkScanTab.CheckedChanged -= ScanTab_CheckedChanged;
        chkScanTab.Checked = !e.Value;
        chkScanTab.CheckedChanged += ScanTab_CheckedChanged;

        Notify.WarnModal(this, "บันทึกไม่สำเร็จ",
            CustomSettingsManager.LastError ?? "เขียนไฟล์ตั้งค่าไม่ได้");
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
        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "รีเซ็ตกลับเป็นค่าเริ่มต้น",
                Headline = "ทุกงานกลับเป็นรอเริ่ม?",
                Facts =
                [
                    ("ล้าง", "คิวเครื่อง · ประวัติคำสั่ง · คำขอที่ ST3 ฝากไว้"),
                    ("ยังอยู่", "ตัวงาน · pattern · ข้อความ · UV" + Environment.NewLine + "แผนการผลิต · ค่าแคลมป์"),
                ],
                Warning = "ย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน",
                OkText = "รีเซ็ต",
                OkIcon = "UndoOutlined",
                Danger = true,
            }))
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
                $"{result.Jobs} งานกลับเป็นรอเริ่ม" + Environment.NewLine + Environment.NewLine
                + $"ลบคิวเครื่อง {result.QueueRemoved} แถว · ลบประวัติคำสั่ง {result.CommandsRemoved} แถว");
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

    // ── ชุดข้อมูลงานทดสอบ ─────────────────────────────────

    /// <summary>โฟลเดอร์เก็บไฟล์ชุดข้อมูล — ข้างไฟล์ตั้งค่า หาง่าย คัดลอกไปเครื่องอื่นได้</summary>
    private static string JobDataFolder => Path.Combine(AppSettingsFile.Folder, "test-data");

    private static ApiClient Backend() =>
        new($"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000");

    /// <summary>ระหว่างคุยกับ backend ปิดทั้งสามปุ่ม กันกดซ้อนกัน (เช่นล้างระหว่างนำเข้า)</summary>
    private void SetJobDataBusy(bool busy)
    {
        btnSaveJobData.Enabled = btnImportJobData.Enabled = btnClearJobData.Enabled = !busy;
    }

    /// <summary>สรุปจำนวนแถวที่คนอ่านแล้วเข้าใจ — งาน / pattern / ข้อมูล UV</summary>
    private static string Summary(IReadOnlyDictionary<string, int> counts)
    {
        int Of(string t) => counts.TryGetValue(t, out var n) ? n : 0;
        return $"งาน {Of("print_jobs")} ใบ · pattern {Of("patterns")} · ข้อความ MK {Of("text_blocks")} ช่อง"
            + $" · ข้อมูล UV {Of("uv_job_data")} แถว · แผนการผลิต {Of("plan_routings")} · ค่าแคลมป์ {Of("iai_clamp_settings")}";
    }

    /// <summary>จำนวนแถวต่อตารางในไฟล์ชุดข้อมูล — null = ไม่ใช่ไฟล์ชุดข้อมูลของโปรแกรมนี้</summary>
    private static (Dictionary<string, int> Counts, string? ExportedAt)? ReadDatasetInfo(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object) return null;
            if (!root.TryGetProperty("format", out var f) || f.ValueKind != System.Text.Json.JsonValueKind.String
                || f.GetString() != "compact-inkjet-job-dataset") return null;
            if (!root.TryGetProperty("tables", out var tables) || tables.ValueKind != System.Text.Json.JsonValueKind.Object) return null;

            var counts = tables.EnumerateObject()
                .Where(t => t.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                .ToDictionary(t => t.Name, t => t.Value.GetArrayLength());
            var at = root.TryGetProperty("exported_at", out var e) && e.ValueKind == System.Text.Json.JsonValueKind.String
                ? e.GetString() : null;
            return (counts, at);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// บันทึกข้อมูลงานทั้งหมดตอนนี้เป็นไฟล์ — อ่านอย่างเดียว ไม่แตะข้อมูลในฐาน
    /// </summary>
    private async Task SaveJobDataAsync()
    {
        SetJobDataBusy(true);
        try
        {
            var (json, error) = await Backend().ExportJobDataAsync();
            if (IsDisposed) return;
            if (json == null || ReadDatasetInfo(json) is not { } info)
            {
                Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", error ?? "backend ส่งข้อมูลกลับมาไม่ถูกรูปแบบ — อัปเดต backend ก่อน");
                return;
            }

            Directory.CreateDirectory(JobDataFolder);
            using var dialog = new SaveFileDialog
            {
                Title = "บันทึกชุดข้อมูลงาน",
                InitialDirectory = JobDataFolder,
                FileName = $"job-dataset-{DateTime.Now.ToString("yyyyMMdd-HHmm", System.Globalization.CultureInfo.InvariantCulture)}.json",
                Filter = "ชุดข้อมูลงาน (*.json)|*.json",
                DefaultExt = "json",
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            File.WriteAllText(dialog.FileName, json, new System.Text.UTF8Encoding(false));
            Notify.SuccessDetail(this, "บันทึกชุดข้อมูลงานแล้ว",
                "คัดลอกไฟล์ไปเครื่องอื่น แล้วกด นำเข้าจากไฟล์" + Environment.NewLine + Environment.NewLine
                + Summary(info.Counts) + Environment.NewLine + dialog.FileName);
        }
        catch (Exception ex)
        {
            Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", ex.Message);
        }
        finally
        {
            if (!IsDisposed) SetJobDataBusy(false);
        }
    }

    /// <summary>
    /// แทนที่ข้อมูลงานทั้งหมดด้วยชุดในไฟล์ — ถามยืนยันพร้อมบอกว่าในไฟล์มีอะไรก่อนลงมือ
    /// backend ทำในทรานแซกชันเดียว ล้มกลางทางข้อมูลเดิมยังอยู่ครบ
    /// </summary>
    private async Task ImportJobDataAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "เลือกไฟล์ชุดข้อมูลงาน",
            InitialDirectory = Directory.Exists(JobDataFolder) ? JobDataFolder : AppSettingsFile.Folder,
            Filter = "ชุดข้อมูลงาน (*.json)|*.json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string json;
        try { json = File.ReadAllText(dialog.FileName); }
        catch (Exception ex)
        {
            Notify.ErrorModal(this, "เปิดไฟล์ไม่ได้", ex.Message);
            return;
        }

        if (ReadDatasetInfo(json) is not { } info)
        {
            Notify.WarnModal(this, "นำเข้าไม่ได้", "ไฟล์นี้ไม่ใช่ชุดข้อมูลงานที่บันทึกจากหน้านี้");
            return;
        }

        var when = DateTime.TryParse(info.ExportedAt, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind, out var at)
            ? at.ToLocalTime().ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture)
            : "-";

        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "นำเข้าชุดข้อมูลงาน",
                Headline = "แทนที่ข้อมูลงานทั้งหมดด้วยไฟล์นี้?",
                Facts = [("ไฟล์", Path.GetFileName(dialog.FileName)), ("บันทึกเมื่อ", when), ("ในไฟล์", Summary(info.Counts))],
                Detail = "นำเข้าแล้วทุกใบเป็นรอเริ่ม ไม่มีคิว",
                Warning = "ข้อมูลงานตอนนี้จะถูกลบทั้งหมด ย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน",
                OkText = "นำเข้า",
                OkIcon = "ImportOutlined",
                Danger = true,
            }))
            return;

        SetJobDataBusy(true);
        try
        {
            var (counts, error) = await Backend().ImportJobDataAsync(json);
            if (IsDisposed) return;
            if (counts == null)
            {
                Notify.ErrorModal(this, "นำเข้าไม่สำเร็จ",
                    "ข้อมูลเดิมยังอยู่ครบ ไม่มีอะไรถูกเปลี่ยน" + Environment.NewLine + Environment.NewLine
                    + (error ?? "ติดต่อ backend ไม่ได้"));
                return;
            }

            Notify.SuccessDetail(this, "นำเข้าชุดข้อมูลงานแล้ว",
                Summary(counts) + Environment.NewLine + Environment.NewLine
                + "กลับไปหน้า Order List แล้วตารางจะอัปเดตเองในรอบถัดไป");
        }
        finally
        {
            if (!IsDisposed) SetJobDataBusy(false);
        }
    }

    /// <summary>ล้างข้อมูลงานทั้งหมด id เริ่มใหม่ที่ 1 — ค่าตั้งของเครื่องไม่ถูกแตะ</summary>
    private async Task ClearJobDataAsync()
    {
        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "ล้างข้อมูลงานทั้งหมด",
                Headline = "ลบข้อมูลงานทุกใบ แล้วเริ่ม id ใหม่ที่ 1?",
                Facts =
                [
                    ("ลบ", "งาน · pattern · ข้อความ MK · UV" + Environment.NewLine + "แผนการผลิต · ค่าแคลมป์ · คิว · ประวัติคำสั่ง"),
                    ("ไม่แตะ", "register map ของ PLC · ไฟล์ Setting"),
                ],
                Detail = "ยังไม่ได้บันทึกไว้? กด ยกเลิก แล้วกด บันทึกข้อมูลงานตอนนี้ ก่อน",
                Warning = "ย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน",
                OkText = "ล้างข้อมูล",
                OkIcon = "DeleteOutlined",
                Danger = true,
            }))
            return;

        SetJobDataBusy(true);
        try
        {
            var (counts, error) = await Backend().ClearJobDataAsync();
            if (IsDisposed) return;
            if (counts == null)
            {
                Notify.ErrorModal(this, "ล้างไม่สำเร็จ",
                    "ยังไม่มีอะไรถูกลบ" + Environment.NewLine + Environment.NewLine + (error ?? "ติดต่อ backend ไม่ได้"));
                return;
            }

            Notify.SuccessDetail(this, "ล้างข้อมูลงานแล้ว",
                "งานที่ลงทะเบียนใหม่จะเริ่ม id ที่ 1" + Environment.NewLine + Environment.NewLine
                + "ลบไป: " + Summary(counts));
        }
        finally
        {
            if (!IsDisposed) SetJobDataBusy(false);
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
