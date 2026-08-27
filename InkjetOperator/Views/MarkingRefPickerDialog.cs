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
