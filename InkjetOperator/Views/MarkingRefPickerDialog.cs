using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>ตัวเลือกหนึ่งรายการใน <see cref="MarkingRefPickerDialog"/></summary>
/// <param name="Key">ค่าที่คืนกลับเมื่อผู้ใช้เลือกรายการนี้</param>
/// <param name="Display">ข้อความที่แสดงในรายการ</param>
/// <param name="ImagePaths">รูปของรายการนี้ อาจว่างได้</param>
public sealed record MarkingRefOption(string Key, string Display, List<string> ImagePaths);

/// <summary>
/// ให้เลือก 1 รายการ พร้อมดูรูปอ้างอิงของรายการที่กำลังไฮไลต์
///
/// ใช้ 2 ที่ที่ความหมายไม่เหมือนกัน จึงให้ผู้เรียกส่งหัวเรื่องกับคำอธิบายมาเอง:
///   · ฝั่ง UV เลือกไฟล์ .uvdx ที่จะโหลดเข้าเครื่อง — เลือกผิดคือพิมพ์ผิดแบบ
///   · ฝั่ง MK เลือกแค่รูปที่จะดู — ไม่กระทบงานที่พิมพ์
/// </summary>
internal sealed partial class MarkingRefPickerDialog : Form
{
    private const int ThumbHeight = 300;
    private const int ThumbMaxWidth = 420;

    private List<MarkingRefOption> _options = new();

    public MarkingRefPickerDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        lstOptions.SelectedIndexChanged += (_, _) => ShowImagesForSelection();

        // ย่อ/ขยายหน้าต่างแล้วรูปต้องยังอยู่กลางกรอบเหมือนเดิม
        pnlImages.SizeChanged += (_, _) => CenterImages();
        lstOptions.DoubleClick += (_, _) => AcceptIfSelected();
        btnOk.Click += (_, _) => AcceptIfSelected();
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        AcceptButton = null;
        CancelButton = null;
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            else if (e.KeyCode == Keys.Enter) AcceptIfSelected();
        };

        // กรอบรูปได้ขนาดจริงตอนหน้าต่างถูกวางเสร็จ ซึ่งอาจช้ากว่าตอนใส่รูปเข้าไป
        Shown += (_, _) => CenterImages();

        FormClosed += (_, _) => ClearImages();
    }

    /// <summary>รายการที่ผู้ใช้เลือก — null เมื่อยกเลิก</summary>
    public string? SelectedKey { get; private set; }

    /// <summary>
    /// เปิด dialog แล้วคืน Key ของรายการที่เลือก · null = ยกเลิก
    /// </summary>
    public static string? Pick(
        IWin32Window? owner, string title, string prompt, List<MarkingRefOption> options)
    {
        if (options.Count == 0) return null;

        using var dlg = new MarkingRefPickerDialog();
        dlg.Text = title;
        dlg.lblPrompt.Text = prompt;
        dlg.SetOptions(options);

        var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
        return result == DialogResult.OK ? dlg.SelectedKey : null;
    }

    /// <summary>
    /// เปิดดูรูปอย่างเดียว ไม่มีอะไรให้เลือก
    ///
    /// ใช้ตอนกดชื่อรูปอ้างอิงในหน้า Order Detail — ชื่อถูกกำหนดมาจาก erp_mfg แล้ว
    /// ผู้ใช้แค่อยากเห็นว่ารูปหน้าตายังไง ไม่ได้กำลังตัดสินใจอะไร
    /// </summary>
    public static void View(
        IWin32Window? owner, string title, string prompt, List<string> images)
    {
        using var dlg = new MarkingRefPickerDialog();
        dlg.Text = title;
        dlg.lblPrompt.Text = prompt;
        dlg.SetViewOnly();
        dlg.SetOptions([new MarkingRefOption("", "", images)]);

        if (owner == null) dlg.ShowDialog();
        else dlg.ShowDialog(owner);
    }

    /// <summary>ยุบคอลัมน์รายการทิ้ง เหลือรูปเต็มกรอบกับปุ่มปิดปุ่มเดียว</summary>
    private void SetViewOnly()
    {
        lstOptions.Visible = false;
        tlpContent.ColumnStyles[0].Width = 0F;
        btnCancel.Visible = false;

        // โหมดนี้ไม่มีอะไรให้เลือก ปุ่มเดียวที่เหลือคือทางออก — แต่งให้เหมือน
        // ปุ่มปิดที่อื่นทั้งระบบ ไม่ใช่ปุ่มตกลงสีน้ำเงินที่ดูเหมือนต้องตัดสินใจอะไร
        Theme.ButtonStyles.Close(btnOk, fontSize: 15f);
    }

    private void SetOptions(List<MarkingRefOption> options)
    {
        _options = options;

        lstOptions.BeginUpdate();
        lstOptions.Items.Clear();
        foreach (var option in options) lstOptions.Items.Add(option.Display);
        lstOptions.EndUpdate();

        lstOptions.SelectedIndex = 0;
    }

    private void AcceptIfSelected()
    {
        int index = lstOptions.SelectedIndex;
        if (index < 0 || index >= _options.Count) return;

        SelectedKey = _options[index].Key;
        DialogResult = DialogResult.OK;
        Close();
    }

    // ── รูป ────────────────────────────────────────────────

    private void ShowImagesForSelection()
    {
        ClearImages();

        int index = lstOptions.SelectedIndex;
        if (index < 0 || index >= _options.Count) return;

        var paths = _options[index].ImagePaths;
        if (paths.Count == 0)
        {
            // แยกให้ชัดว่าไม่มีรูป กับตั้งค่าโฟลเดอร์ไม่ถูก คนละเรื่องกัน
            lblEmpty.Text = MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder());
            lblEmpty.Visible = true;
            flpImages.Visible = false;
            return;
        }

        lblEmpty.Visible = false;
        flpImages.Visible = true;

        foreach (var path in paths)
        {
            var image = MarkingRefImageService.LoadImageNoLock(path);
            if (image == null) continue;

            int width = image.Height == 0
                ? ThumbMaxWidth
                : (int)(image.Width * (ThumbHeight / (double)image.Height));
            width = Math.Clamp(width, 80, ThumbMaxWidth);

            var box = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = width,
                Height = ThumbHeight,
                Margin = new Padding(4),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            flpImages.Controls.Add(box);
        }

        // ทุกใบโหลดไม่ขึ้น (ไฟล์เสีย / share หลุดกลางคัน) ต้องบอก ไม่ใช่ปล่อยว่าง
        if (flpImages.Controls.Count == 0)
        {
            lblEmpty.Text = "เปิดไฟล์รูปไม่ได้";
            lblEmpty.Visible = true;
            flpImages.Visible = false;
            return;
        }

        // ต้องบังคับให้จัดวางก่อน ไม่งั้น flpImages.Width ยังเป็นขนาดของรูปชุดก่อนหน้า
        // แล้วจะคำนวณตำแหน่งกลางจากขนาดที่ผิด
        flpImages.PerformLayout();
        pnlImages.AutoScrollPosition = Point.Empty;
        CenterImages();
    }

    /// <summary>กำลังจัดกลางอยู่ — กันเรียกซ้อนตอน scrollbar โผล่/หายแล้วยิง SizeChanged กลับมา</summary>
    private bool _centering;

    /// <summary>
    /// วางแถบรูปไว้กลางกรอบทั้งแนวนอนและแนวตั้ง
    ///
    /// <c>flpImages</c> เป็น AutoSize อยู่แล้ว ขนาดจึงพอดีกับรูปที่มี การจัดกลางคือ
    /// เลื่อนตำแหน่งมันเองในกรอบ ไม่ใช่ไปยุ่งกับรูปข้างใน — รูปใบเดียวจึงอยู่กลางจริง
    /// ไม่ชิดซ้ายเหมือนพฤติกรรมปกติของ FlowLayoutPanel ที่เรียงจากซ้ายเสมอ
    ///
    /// รูปกว้าง/สูงเกินกรอบ ตำแหน่งจะถูกหนีบไว้ที่ 0 แล้วปล่อยให้ scrollbar ของ
    /// <c>pnlImages</c> ทำงานแทน — เริ่มดูจากมุมบนซ้ายซึ่งเป็นสิ่งที่ถูกต้องในกรณีนั้น
    /// </summary>
    private void CenterImages()
    {
        if (_centering || !flpImages.Visible) return;

        _centering = true;
        try
        {
            // อ่านจาก PreferredSize ไม่ใช่ Width/Height — ตอนเพิ่งใส่รูปเข้าไป
            // ขนาดจริงอาจยังไม่ทันอัปเดตตาม AutoSize
            var frame = pnlImages.ClientSize;
            var content = flpImages.PreferredSize;

            flpImages.Location = new Point(
                Math.Max(0, (frame.Width - content.Width) / 2),
                Math.Max(0, (frame.Height - content.Height) / 2));
        }
        finally
        {
            _centering = false;
        }
    }

    private void ClearImages()
    {
        // ต้องถ่ายออกมาก่อน — Dispose() ถอด control ออกจาก Controls ระหว่างวนอยู่
        var current = flpImages.Controls.Cast<Control>().ToArray();
        flpImages.Controls.Clear();

        foreach (var control in current)
        {
            if (control is PictureBox box) box.Image?.Dispose();
            control.Dispose();
        }
    }
}
