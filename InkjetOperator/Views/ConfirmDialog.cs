namespace InkjetOperator.Views;

/// <summary>
/// เนื้อหาของกล่องยืนยันหนึ่งกล่อง — ใส่เฉพาะช่องที่ใช้ ช่องที่ว่างจะไม่ขึ้นในกล่อง
/// </summary>
internal sealed class ConfirmSpec
{
    /// <summary>หัวแถบสีน้ำเงิน เช่น "ยืนยันเริ่มงาน"</summary>
    public required string Title { get; init; }

    /// <summary>ประโยคเดียวบอกว่ากดแล้วจะเกิดอะไร — ตัวใหญ่ อ่านอันนี้อันเดียวก็ต้องรู้เรื่อง</summary>
    public string? Headline { get; init; }

    /// <summary>หัวข้อ / ค่า เรียงลงมา ไม่เกิน 6 แถว (ค่าขึ้นบรรทัดใหม่ได้)</summary>
    public List<(string Caption, string Value)> Facts { get; init; } = [];

    /// <summary>ข้อความธรรมดา — สำหรับกล่องที่ไม่ได้แยกเป็นหัวข้อ/ค่า</summary>
    public string? Message { get; init; }

    /// <summary>รายละเอียดยาว หรือหมายเหตุ — อยู่ในกล่องพื้นอ่อนแยกออกมา</summary>
    public string? Detail { get; init; }

    /// <summary>สิ่งที่ต้องระวังก่อนกด — ตัวแดง</summary>
    public string? Warning { get; init; }

    /// <summary>ชื่อปุ่มยืนยัน บอกการกระทำ เช่น "เริ่มงาน" ไม่ใช่ "Yes"</summary>
    public string OkText { get; init; } = "ยืนยัน";

    public string CancelText { get; init; } = "ยกเลิก";

    /// <summary>ลบ / ยกเลิก / ย้อนไม่ได้ — ปุ่มยืนยันเป็นสีแดง</summary>
    public bool Danger { get; init; }

    /// <summary>
    /// ไอคอนบนแถบหัว (ชื่อไอคอนของ AntdUI เช่น "PlayCircleFilled") — ว่าง = เครื่องหมายคำถาม
    /// หรือเครื่องหมายเตือนถ้าเป็นคำสั่งอันตราย
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>ไอคอนบนปุ่มยืนยัน — ว่าง = เครื่องหมายถูก</summary>
    public string? OkIcon { get; init; }

    /// <summary>false = กล่องแจ้งเฉย ๆ มีปุ่มเดียว (ตกลง) ไม่มีอะไรให้เลือก</summary>
    public bool ShowCancel { get; init; } = true;

    /// <summary>สีแถบหัว — ว่าง = น้ำเงินเข้ม · กล่องแจ้งผิดพลาด/เตือน/สำเร็จ ใช้สีตามระดับ ให้รู้ตั้งแต่เหลือบตา</summary>
    public Color? TitleBack { get; init; }

    /// <summary>
    /// ช่องติ๊กให้เลือกข้ามบางอย่าง — ว่าง = ไม่มีให้เลือก (สูงสุด 3 ช่อง)
    ///
    /// <para>
    /// กล่องเขียนผลกลับลงใน <see cref="ConfirmSkipOption.Skip"/> ของแต่ละตัว
    /// ผู้เรียกอ่านต่อได้หลังกล่องปิด — ค่าที่คืนจาก <c>Ask</c> ยังเป็นแค่ กด/ไม่กด เหมือนเดิม
    /// </para>
    /// </summary>
    public List<ConfirmSkipOption> SkipOptions { get; init; } = [];
}

/// <summary>หนึ่งช่องติ๊ก "ข้ามอันนี้" ในกล่องยืนยัน</summary>
internal sealed class ConfirmSkipOption
{
    /// <summary>ตัวระบุที่ผู้เรียกใช้แยกแยะ เช่นชื่อเครื่อง</summary>
    public required string Key { get; init; }

    /// <summary>ข้อความข้างช่องติ๊ก</summary>
    public required string Text { get; init; }

    /// <summary>true = คนติ๊กว่าให้ข้าม — กล่องเขียนค่านี้ตอนกดยืนยัน</summary>
    public bool Skip { get; set; }
}

/// <summary>
/// กล่องยืนยันของทั้งโปรแกรม — แบบเดียวกับหน้ายืนยันส่งข้อมูลของ NastoKeyence:
/// แถบหัวสีน้ำเงินเข้ม เนื้อหาเป็นหัวข้อสีเทา / ค่าสีน้ำเงินตัวหนา ปุ่มบอกการกระทำ
/// <para>
/// เรียกผ่าน <see cref="Confirm.Ask(Control?, ConfirmSpec)"/> เท่านั้น ที่นั่นจัดการเธรด เจ้าของ และพื้นหลังมืด
/// </para>
/// <para>
/// ไม่ตั้ง AcceptButton โดยตั้งใจ — สแกนเนอร์บาร์โค้ดส่ง Enter ตามท้าย ถ้ายิงตอนกล่องเปิดอยู่
/// จะกลายเป็นกดยืนยันโดยไม่มีใครตั้งใจ Esc ยังเป็นยกเลิกเหมือนเดิม
/// </para>
/// </summary>
internal sealed partial class ConfirmDialog : AntdUI.BorderlessForm
{
    /// <summary>ให้ Designer เปิดหน้าต่างนี้ได้</summary>
    public ConfirmDialog()
    {
        InitializeComponent();

        // ฟอร์มไม่มีแถบหัวของระบบ ต้องลากจากแถบหัวของเราเอง
        tlpTitleBar.MouseDown += TitleBar_MouseDown;
        lblTitle.MouseDown += TitleBar_MouseDown;

        // โฟกัสเริ่มที่ปุ่มยกเลิก — กด Enter / Space โดยไม่ตั้งใจจะไม่ทำอะไรที่ย้อนไม่ได้
        Shown += (_, _) => (btnCancel.Visible ? btnCancel : btnOk).Focus();

        // เขียนผลช่องติ๊กกลับให้ผู้เรียกเฉพาะตอนกดยืนยัน
        //
        // กดยกเลิกแล้วไม่เขียน เพราะค่าที่ติ๊กไว้ระหว่างลังเลไม่ควรมีผลกับอะไรทั้งนั้น
        FormClosing += (_, _) =>
        {
            if (DialogResult != DialogResult.OK) return;

            var boxes = new[] { chkSkip0, chkSkip1, chkSkip2 };
            for (int i = 0; i < _skipOptions.Count && i < boxes.Length; i++)
                _skipOptions[i].Skip = boxes[i].Checked;
        };
    }

    /// <summary>ช่องติ๊กของกล่องนี้ — เก็บไว้เขียนผลกลับตอนปิด</summary>
    private List<ConfirmSkipOption> _skipOptions = [];

    public ConfirmDialog(ConfirmSpec spec)
        : this()
    {
        Bind(spec);
    }

    private void Bind(ConfirmSpec spec)
    {
        Text = spec.Title;
        lblTitle.Text = spec.Title;

        Show(lblHeadline, spec.Headline);
        Show(lblMessage, spec.Message);
        Show(lblWarning, string.IsNullOrWhiteSpace(spec.Warning) ? null : "⚠  " + spec.Warning.Trim());
        Show(lblDetail, spec.Detail);
        // อ่าน Visible ของ label กลับไม่ได้ — ก่อนหน้าต่างขึ้นจอ ค่านี้เป็น false เสมอ
        pnlDetail.Visible = !string.IsNullOrWhiteSpace(spec.Detail);

        var captions = new[] { lblFactCaption1, lblFactCaption2, lblFactCaption3, lblFactCaption4, lblFactCaption5, lblFactCaption6 };
        var values = new[] { lblFactValue1, lblFactValue2, lblFactValue3, lblFactValue4, lblFactValue5, lblFactValue6 };
        for (int i = 0; i < captions.Length; i++)
        {
            bool used = i < spec.Facts.Count;
            captions[i].Visible = values[i].Visible = used;
            if (!used) continue;
            captions[i].Text = spec.Facts[i].Caption;
            values[i].Text = spec.Facts[i].Value;
        }
        tlpFacts.Visible = spec.Facts.Count > 0;

        // ช่องติ๊กข้ามเครื่อง — สร้างไว้ครบใน Designer แล้วโชว์เท่าที่ใช้ แบบเดียวกับแถว Facts
        _skipOptions = spec.SkipOptions;
        var boxes = new[] { chkSkip0, chkSkip1, chkSkip2 };
        for (int i = 0; i < boxes.Length; i++)
        {
            bool used = i < _skipOptions.Count;
            boxes[i].Visible = used;
            if (!used) continue;
            boxes[i].Text = _skipOptions[i].Text;
            boxes[i].Checked = _skipOptions[i].Skip;
        }
        tlpSkip.Visible = _skipOptions.Count > 0;

        lblTitle.PrefixSvg = spec.Icon ?? (spec.Danger ? "WarningFilled" : "QuestionCircleFilled");
        btnOk.IconSvg = spec.OkIcon ?? "CheckOutlined";
        btnOk.Text = spec.OkText;
        btnCancel.Text = spec.CancelText;

        // กล่องแจ้ง: ปุ่มเดียว Esc / Enter ปิดได้ — ไม่มีอะไรย้อนไม่ได้ให้กดพลาด
        if (!spec.ShowCancel)
        {
            btnCancel.Visible = false;
            btnOk.Margin = new Padding(0);
            AcceptButton = btnOk;
            CancelButton = btnOk;
        }

        if (spec.TitleBack is { } back)
        {
            tlpTitleBar.BackColor = back;
            btnTitleClose.BackColor = back;
        }
        if (spec.Danger) btnOk.Type = AntdUI.TTypeMini.Error;
    }

    /// <summary>ว่าง = ซ่อน แถวยุบหาย ไม่เหลือช่องว่าง</summary>
    private static void Show(Label label, string? text)
    {
        label.Text = text?.Trim() ?? "";
        label.Visible = label.Text.Length > 0;
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            DraggableMouseDown();
        }
    }
}
