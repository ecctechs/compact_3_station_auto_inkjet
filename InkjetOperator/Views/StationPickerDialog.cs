namespace InkjetOperator.Views;

/// <summary>
/// ถามว่าเครื่องนี้เป็นสถานีไหน — ขึ้นครั้งเดียวตอนเปิดโปรแกรมครั้งแรกหลังติดตั้ง
/// <para>
/// เดิมเครื่องใหม่ได้ <c>MENU_LEVEL = 99</c> (โหมดนักพัฒนา) จากไฟล์ค่าตั้งต้น
/// เครื่องลูกค้าจึงเปิดมาเห็นทุกหน้า มีปุ่มส่งตรงเข้าเครื่องของโหมดทดสอบ
/// และสวิตช์ Mockup ที่ทำให้ไฟสถานะเขียวทั้งที่ไม่ได้ต่อเครื่องจริง
/// ถ้าไม่มีใครไปแก้ไฟล์ตั้งค่าเองก็จะค้างอยู่แบบนั้นตลอด
/// </para>
/// <para>
/// มีแค่สามสถานีที่ใช้ในการผลิต ระดับ 9 กับ 99 ยังตั้งได้จากไฟล์ตามเดิม
/// แต่ไม่ให้เลือกจากหน้านี้ เพราะคนที่ติดตั้งไม่ควรเลือกโหมดทดสอบโดยไม่ตั้งใจ
/// </para>
/// </summary>
internal sealed partial class StationPickerDialog : AntdUI.BorderlessForm
{
    /// <summary>
    /// Design-time constructor. The WinForms designer instantiates the form to
    /// render the surface and can only do that through a parameterless constructor.
    /// </summary>
    public StationPickerDialog()
    {
        InitializeComponent();

        // A borderless form has no system title bar, so the header has to move the
        // window itself.
        tlpTitleBar.MouseDown += TitleBar_MouseDown;
        lblTitle.MouseDown += TitleBar_MouseDown;

        btnScanBarcode.Click += (_, _) => Choose(0, "Scan Barcode");
        btnStation1.Click += (_, _) => Choose(1, "Station 1");
        btnStation3.Click += (_, _) => Choose(3, "Station 3");
    }

    /// <summary>ค่า MENU_LEVEL ที่เลือก — ใช้ได้เมื่อ DialogResult เป็น OK</summary>
    public int SelectedLevel { get; private set; } = -1;

    /// <summary>
    /// ยืนยันก่อนปิด เพราะเลือกแล้วจะไม่ถามอีก ถ้ากดพลาดต้องไปแก้ไฟล์ตั้งค่าเอง
    /// </summary>
    private void Choose(int level, string name)
    {
        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "ยืนยันสถานี",
                Headline = $"ตั้งเครื่องนี้เป็น {name}?",
                Detail = "เลือกแล้วจะไม่ถามอีก",
                OkText = "ยืนยัน",
                Icon = "DesktopOutlined",
            }))
            return;

        SelectedLevel = level;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            DraggableMouseDown();
        }
    }
}
