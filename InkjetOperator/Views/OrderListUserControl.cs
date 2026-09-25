using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class OrderListUserControl : UserControl
{
    private static readonly string[] ActiveStatuses = ["Waiting", "Process"];

    /// <summary>งานที่จบแล้ว — ทั้งที่ทำเสร็จจริงและที่ถูกยกเลิก ล้วนไปอยู่แท็บ History</summary>
    private static readonly string[] HistoryStatuses = ["Success", "Cancel"];

    private ApiClient? _api;
    private System.Windows.Forms.Timer? _pollTimer;

    /// <summary>ตัวเฝ้าบิตปุ่มกดหน้างาน — เริ่มเองตอนหน้านี้โหลด</summary>
    private readonly PushButtonWatcher _pushButton = new();

    /// <summary>
    /// กำลังจัดการการกดปุ่มอยู่ — คลุมตั้งแต่รับสัญญาณจนจบ รวมช่วงที่มีหน้าต่างเปิดค้าง
    /// <para>
    /// ต้องมีแยกจาก <c>_sending</c> เพราะ WinForms Timer ยังเดินต่อระหว่างที่กล่อง
    /// modal เปิดอยู่ ถ้าไม่กันไว้ ตัวเฝ้าจะรับสัญญาณรอบใหม่ทับของเดิมได้
    /// </para>
    /// </summary>
    private bool _pushHandling;

    /// <summary>กำลังดึงข้อมูลรอบ poll อยู่ — กันรอบใหม่ทับรอบเก่าตอน backend ช้า</summary>
    private bool _refreshing;

    /// <summary>
    /// กำลังทำงานตามปุ่มในแถวอยู่ — กันกดซ้ำระหว่างรออ่านข้อมูลจาก backend
    ///
    /// <para>
    /// ทุกปุ่มในแถว (เริ่มงาน · จบงาน · แว่น · กากบาท) ต้องอ่านข้อมูลสดก่อนลงมือ
    /// ซึ่งรอได้ถึง 10 วินาทีตอนต่อ backend ไม่ติด ระหว่างนั้นจอไม่มีอะไรบอกเลย
    /// พนักงานจะกดซ้ำ แล้วแต่ละครั้งไปตั้งคำขอใหม่ พอครบกำหนดพร้อมกันก็เด้ง
    /// กล่องผิดพลาดซ้อนกันเป็นพรวด
    /// </para>
    /// </summary>
    private bool _rowBusy;

    /// <summary>ช้ากว่านี้ถึงจะขึ้นการ์ดว่ากำลังโหลด — เร็วกว่านี้การ์ดจะวาบจนรำคาญ</summary>
    private const int SlowLoadMs = 250;
    private bool _showHistory;
    private List<PrintJob> _allJobs = new();
    private string _lastSignature = "";

    public OrderListUserControl()
    {
        InitializeComponent();

        ConfigureColumns();
        SetupEvents();
    }

    private void ConfigureColumns()
    {
        // SortOrder = ให้ AntdUI จัดเรียงคอลัมน์นั้นเองเมื่อคลิกหัวตาราง
        // คอลัมน์ Op ไม่ใส่ เพราะเป็นปุ่ม เรียงแล้วไม่มีความหมาย
        //
        // ความกว้างเป็น % ของตาราง รวมกันพอดี 100 — ทุกคอลัมน์แบ่งพื้นที่ตามสัดส่วน
        // ไม่มีช่องว่างเหลือ และไม่ขยับตอนสลับภาษาเพราะไม่ได้วัดจากข้อความหัวตาราง
        // (ถ้าไม่กำหนด Width เลย AntdUI จะวัดจากหัวตารางให้ ซึ่งเปลี่ยนตามภาษา)
        //
        // ตัวเลขที่เขียนไว้ตรงนี้คือชุดของแท็บ History ซึ่งมีคอลัมน์ครบทุกคอลัมน์
        // แท็บ List ซ่อน End แล้วแจก 9% ที่ว่างคืนให้คอลัมน์อื่น ดู ApplyTabColumns
        //
        // ตัวเลข % เกลี่ยจากระยะห่างระหว่างข้อความหัวตารางกับลูกศร sort ที่ชิดขวา
        // ให้ทุกคอลัมน์ห่างพอ ๆ กัน ไม่ใช่บีบจนลูกศรติดตัวหนังสือบางคอลัมน์
        // ทุกคอลัมน์ยังกว้างกว่าข้อมูลที่ยาวที่สุดของตัวเอง — Start/End ถูกล็อกด้วย
        // ความยาวของวันเวลา ("27/08/26 12:04" = 123px + ขอบ 24px) ต่ำกว่า 9% ไม่ได้
        // เกลี่ยใหม่เมื่อไหร่ต้องเช็คสองอย่างนี้พร้อมกัน ทั้งข้อมูลล้นและระยะลูกศร
        tblOrders.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("Start", "Start", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("End", "End", AntdUI.ColumnAlign.Center) { Width = "9%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("ErpMfg", "ERP MFG", AntdUI.ColumnAlign.Center) { Width = "12%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("LotNo", "Lot Number", AntdUI.ColumnAlign.Center) { Width = "12%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Qty", "Qty", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("ProcessSequence", "Process Sequence", AntdUI.ColumnAlign.Center) { Width = "13%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Plate", "Plate", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Shim", "Shim", AntdUI.ColumnAlign.Center) { Width = "6%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Station", "Station", AntdUI.ColumnAlign.Center) { Width = "7%", SortOrder = true, ColBreak = true },
            new AntdUI.Column("Status", "Status", AntdUI.ColumnAlign.Center) { Width = "8%", SortOrder = true, ColBreak = true },
            // กว้างกว่าคอลัมน์อื่นเพราะแท็บ List ใส่ได้ถึงสามปุ่ม — เริ่ม/จบงาน + ยกเลิก + รายละเอียด
            new AntdUI.Column("Op", "", AntdUI.ColumnAlign.Center) { Width = "12%" },
        };

        // ColBreak above is what centres the titles, and it is not obvious why.
        //
        // A sortable header normally reserves a strip on its right for the sort arrow
        // (SFWidth, derived from the text height - SortOrderSize does not change it),
        // and AntdUI centres the title inside what is left of the cell rather than
        // inside the whole cell. Every title therefore sat about half an arrow-width
        // left of centre.
        //
        // With ColBreak set AND a percentage Width, AntdUI measures the header against
        // the column width and returns early, leaving SFWidth at 0 - so the title is
        // centred across the full cell while the arrow is still drawn. The trade-off is
        // that a title too wide for its column now wraps (mid-word) instead of forcing
        // the column wider. "Order No." is the first to go, once the table falls below
        // roughly 1260px at 100% scaling (1.6x that at 150%) - full screen on the panel
        // PC is about 1660px, so there is room to spare. Any column narrowed from here
        // has to be checked against that: needed width = title width + 24px, divided by
        // the column's share.
        //
        // Both parts are required: dropping either ColBreak or the % Width brings the
        // off-centre title back.

        // Sort arrows default to 60% of the header text height, which at 14pt bold
        // crowds the title. Pin a smaller size that is still easy to read across the room.
        tblOrders.SortOrderSize = 12;

        // แท็บตั้งต้นคือ List — SwitchTab ไม่ได้ถูกเรียกตอนเปิดหน้า
        ApplyTabColumns(showHistory: false);
    }

    /// <summary>
    /// คอลัมน์ที่ความกว้างไม่เท่ากันระหว่างสองแท็บ — (key, กว้างตอน List, กว้างตอน History)
    /// ส่วนที่เหลือกว้างเท่ากันทั้งสองแท็บ จึงไม่ต้องมีในนี้
    /// </summary>
    private static readonly (string Key, string ListWidth, string HistoryWidth)[] TabColumnWidths =
    [
        ("ErpMfg", "14%", "12%"),
        ("LotNo", "14%", "12%"),
        ("Qty", "7%", "6%"),
        ("ProcessSequence", "15%", "13%"),
        ("Plate", "7%", "6%"),
        ("Shim", "7%", "6%"),
    ];

    /// <summary>
    /// แท็บ List ไม่มีคอลัมน์ End เพราะงานที่ยังไม่จบก็ยังไม่มีเวลาจบ — ทั้งคอลัมน์
    /// เป็นขีดทุกแถวอยู่แล้ว เวลาจบงานดูได้ที่แท็บ History ที่เดียว
    ///
    /// ที่ต้องขยับความกว้างด้วย เพราะ AntdUI คิดความกว้างเป็นสัดส่วนของตารางตรง ๆ
    /// (Table.Layout: width = rect.Width × ratio) ไม่ได้เกลี่ยใหม่ให้เมื่อซ่อนคอลัมน์
    /// ซ่อน End เฉย ๆ จะเหลือที่ว่าง 9% ค้างท้ายตาราง จึงแจก 9% นั้นคืนเอง
    ///
    /// ทั้งสองชุดต้องรวมได้ 100 พอดี — แก้ตัวเลขเมื่อไหร่ต้องบวกใหม่ทั้งสองชุด
    /// </summary>
    private void ApplyTabColumns(bool showHistory)
    {
        if (tblOrders.Columns == null) return;

        foreach (var column in tblOrders.Columns)
        {
            if (column.Key == "End") column.Visible = showHistory;

            foreach (var (key, list, history) in TabColumnWidths)
            {
                if (column.Key != key) continue;
                column.Width = showHistory ? history : list;
                break;
            }
        }
    }

    private void SetupEvents()
    {
        btnTabList.Click += (_, _) => SwitchTab(false);
        btnTabHistory.Click += (_, _) => SwitchTab(true);

        // ST3 ไม่มีแท็บ History — ซ่อนปุ่มไปเลย ไม่ใช่แค่กรองรายการให้ว่าง
        // จอหน้างานมีหน้าที่เดียวคือทำงานที่ค้างอยู่ ไม่ได้ใช้ย้อนดูประวัติ
        // ตัวกรองวันที่ผูกกับแท็บนี้อยู่แล้ว จึงไม่โผล่ตามไปด้วย
        btnTabHistory.Visible = !StationService.IsSt3;
        tblOrders.CellButtonClick += TblOrders_CellButtonClick;

        // AntdUI เรียงด้วยการเทียบ "ข้อความในเซลล์" ซึ่งทำให้ 26/08 มาหลัง 02/09
        // จึงดักเทียบเองเฉพาะค่าที่เป็นวันเวลา ที่เหลือปล่อยเป็นการเทียบแบบรู้ตัวเลข
        tblOrders.CustomSort += CompareCellText;
        tblOrders.SetRowStyle += TblOrders_SetRowStyle;

        // เปลี่ยนช่วงวันที่ = ต้องดึงใหม่ ไม่ใช่กรองของที่โหลดไว้ — งานเก่ายังไม่ได้อยู่ในมือ
        // เลือกวันแล้วค้นให้เลย ปุ่มค้นหาเป็นทางกดซ้ำสำหรับคนที่อยากสั่งเอง
        // (เช่นเลือกวันเดิมอีกครั้ง ซึ่ง ValueChanged ไม่ยิงให้)
        dtpHistoryRange.ValueChanged += async (_, _) => await RefreshDataAsync(force: true);
        btnSearchDate.Click += async (_, _) => await RefreshDataAsync(force: true);
        btnClearDate.Click += (_, _) => dtpHistoryRange.Value = null;

        WirePanels();

        WirePushButton();

        Load += OnLoad;
        Disposed += OnDisposed;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        _api = new ApiClient($"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000");
        _ = RefreshDataAsync();
        StartPolling();
        _pushButton.Start();
    }

    private void OnDisposed(object? sender, EventArgs e)
    {
        _pollTimer?.Stop();
        _pollTimer?.Dispose();
        _pushButton.Dispose();
        DisposePanelImages();
    }

    // ── ปุ่มกดหน้างาน ───────────────────────────────────────

    private void WirePushButton()
    {
        // ปุ่มบอกว่า "เครื่องว่างแล้ว" ซึ่งกดได้ตลอดที่เครื่องถืองานอยู่ จึงเฝ้าไว้
        // ตลอดที่หน้านี้เปิด ไม่ต้องรอว่ามีงานในคิวไหม กดตอนไม่มีอะไรถือเครื่องอยู่
        // ก็ไม่เกิดอะไรขึ้น backend คืน released เป็นค่าว่างเฉย ๆ
        // เฝ้าตลอดที่หน้านี้เปิดอยู่ ไม่หยุดอ่านตอนไม่ว่าง
        //
        // เดิมหยุดอ่านตอนกำลังส่งงาน ผลคือการกดที่เกิดในช่วงนั้นหายไปเงียบ ๆ เพราะ
        // พอกลับมาอ่าน ค่าแรกถูกใช้เป็นค่าตั้งต้นเฉย ๆ ไม่นับเป็นการกด คนหน้างาน
        // กดแล้วไม่มีอะไรเกิดขึ้นและไม่รู้ว่าต้องกดใหม่
        _pushButton.ShouldWatch = () => Visible;

        // ลงมือได้ก็ต่อเมื่อหน้าจอว่างจริง ๆ
        //
        // นาฬิกาของ WinForms ยังเดินตอนมีกล่อง modal เปิดค้าง เพราะ WinForms รัน
        // message loop ซ้อนให้ ถ้าไม่กันไว้ การกดปุ่มหน้างานจะไปปล่อยเครื่องและส่ง
        // งานใบถัดไปเข้าเครื่องอยู่หลังกล่องที่ยังไม่มีใครตอบ แล้วสิ่งที่กล่องสรุปไว้
        // ก็ไม่ตรงกับความจริงอีกต่อไป
        _pushButton.CanAct = () =>
            !_sending && !_pushHandling && !_showingRemoteError
            && !MachineBusy.Active && !AnyDialogOpen();

        _pushButton.Pressed += async (_, _) => await OnPushButtonPressedAsync();
        _pushButton.BlockedPress += (_, _) => ShowBlockedPress();

        // แถบสถานีเห็นทุกโหมด — คนหน้างานต้องรู้ว่าเครื่องไหนว่างและมีอะไรรออยู่
        // ส่วนปุ่มจำลองการกดปุ่มหน้างานเหลือเฉพาะโหมดทดสอบเหมือนเดิม
        //
        // ซ่อนปุ่มอย่างเดียวพอ ความสูงของแถบวัดจากของที่แสดงอยู่จริง
        //
        // ห้ามล็อกความสูงเป็นตัวเลขตายตัวจากตรงนี้ ค่าที่ designer ตั้งไว้จะถูกสเกล
        // ตาม DPI ของจอให้เอง แต่ค่าที่เขียนทับจากโค้ดไม่ถูกสเกลด้วย พอไปอยู่บนจอ 4K
        // ที่สเกล 200% แถวป้ายสองแถวบนโตขึ้นเท่าตัวจนกินความสูงที่ล็อกไว้จนหมด
        // แถวปุ่มเลยเหลือเกือบศูนย์ ปุ่มถูกบีบจนอ่านไม่ออก
        bool dev = StationService.IsDevMode;

        btnSimPushDelay.Visible = dev;
        btnSimPushDelay.Click += (_, _) => StartDelayedPushTest();

        btnSimPushMk.Visible = dev;
        btnSimPushUv1.Visible = dev;
        btnSimPushUv2.Visible = dev;

        btnSimPushMk.Click += async (_, _) => await OnPushButtonPressedAsync("MK");
        btnSimPushUv1.Click += async (_, _) => await OnPushButtonPressedAsync("UV1");
        btnSimPushUv2.Click += async (_, _) => await OnPushButtonPressedAsync("UV2");

        // ขาดการติดต่อไม่ใช่เรื่องต้องกดปิด — ใช้ข้อความลอย ไม่ใช่กล่อง modal
        // ตัวเฝ้าแจ้งครั้งเดียวตอนขาด และอีกครั้งตอนกลับมา ไม่ได้แจ้งทุกรอบ
        _pushButton.Trouble += (_, error) =>
        {
            if (IsDisposed) return;

            if (error == null) Notify.Success(this, "ปุ่มกดหน้างาน — กลับมาอ่านค่าได้แล้ว");
            else Notify.Warn(this, $"ปุ่มกดหน้างาน — อ่านค่าจาก PLC ไม่ได้ ({error})");
        };
    }


    private static bool AlreadySent(PrintJob job, string step) =>
        job.Commands?.Any(c => c.Success &&
            string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    /// <summary>
    /// มีคนกดปุ่มหน้างาน — ส่งขั้นตอนถัดไปให้ทันที
    ///
    /// <para>
    /// ไม่ถามยืนยัน คนกดยืนอยู่หน้าเครื่องแล้ว การเด้งกล่องให้เดินมากดตกลงที่จอ
    /// อีกทีทำให้ปุ่มไม่มีประโยชน์ กล่องเลือกโปรแกรม UV ยังเด้งอยู่ถ้าจำเป็น
    /// เพราะนั่นคือการ "เลือก" ไม่ใช่การ "ยืนยัน"
    /// </para>
    /// </summary>
    /// <summary>
    /// เครื่องที่ปุ่มกดของสถานีนี้คุมอยู่ — ST1 คุม MK · ST3 คุม UV2
    ///
    /// ปุ่มของ ST2 (UV1) ยังไม่มีเครื่องคอมเฝ้า เพราะ ST2 ไม่มีจอ
    /// </summary>
    private static string MachineOfThisStation() =>
        StationService.IsSt3 ? "UV2" : "MK";

    /// <summary>
    /// มีคนกดปุ่มหน้างาน = พิมพ์ชิ้นเดิมเสร็จแล้ว ปล่อยเครื่องให้คิวถัดไป
    ///
    /// <para>
    /// ปุ่มนี้ไม่ได้สั่งส่งงานเองอีกต่อไป มันบอกแค่ว่า "เครื่องนี้ว่างแล้ว"
    /// ใครจะได้เครื่องต่อเป็นเรื่องของคิวที่ backend และคนที่หยิบไปส่งคือ ST1
    /// </para>
    /// <para>
    /// ไม่ถามยืนยัน คนกดยืนอยู่หน้าเครื่องแล้ว และไม่มีอะไรให้เลือกด้วย
    /// เพราะลำดับคิวถูกกำหนดไว้ก่อนแล้วตั้งแต่ตอนกดเริ่มงาน
    /// </para>
    /// </summary>
    /// <summary>หน่วงเวลากี่วินาทีก่อนจำลองการกดปุ่มหน้างาน</summary>
    private const int DelayedPushSeconds = 5;

    /// <summary>
    /// จำลองการกดปุ่มหน้างานแบบหน่วงเวลา — โหมดทดสอบเท่านั้น
    ///
    /// <para>
    /// ปุ่มจำลองสามปุ่มข้างบนเรียกตัวจัดการโดยตรง จึงข้ามด่านที่กันการกดตอนจอไม่ว่าง
    /// ไปทั้งหมด และต่อให้ไม่ข้าม พอมีกล่องเปิดค้างก็กดปุ่มบนจอไม่ได้อยู่แล้ว ทดสอบ
    /// เคส "กดปุ่มหน้างานตอนจอมีกล่องค้าง" ด้วยปุ่มพวกนั้นไม่ได้เลย
    /// </para>
    /// <para>
    /// ตัวนี้หน่วงเวลาไว้ก่อน คนทดสอบจึงมีเวลาไปเปิดกล่องยืนยันให้ค้างไว้ แล้วดูว่า
    /// พอถึงเวลาเกิดอะไรขึ้น และเดินผ่านด่านเดียวกับปุ่มจริงทุกประการ ผลที่เห็นจึง
    /// เชื่อถือได้เท่ากับไปยืนกดปุ่มจริงที่หน้าเครื่อง
    /// </para>
    /// </summary>
    private void StartDelayedPushTest()
    {
        Notify.Success(this, $"จะจำลองการกดปุ่มหน้างานในอีก {DelayedPushSeconds} วินาที");

        var timer = new System.Windows.Forms.Timer { Interval = DelayedPushSeconds * 1000 };
        timer.Tick += async (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            if (IsDisposed) return;

            // ด่านชุดเดียวกับที่ตัวเฝ้าปุ่มจริงใช้ ไม่ได้เขียนเงื่อนไขซ้ำ
            if (_pushButton.CanAct?.Invoke() == false)
            {
                ShowBlockedPress();
                return;
            }

            await OnPushButtonPressedAsync();
        };
        timer.Start();
    }

    /// <summary>
    /// มีหน้าต่างแบบ modal เปิดค้างอยู่ไหม
    ///
    /// <para>
    /// ถามจากหน้าต่างที่เปิดอยู่จริง ไม่ใช้ตัวนับ <c>AntdUI.Modal.ModalCount</c> เพราะ
    /// ตัวนับนั้นบวกก่อนเปิดแล้วลบหลังปิด ถ้ามี exception ในกล่อง บรรทัดที่ลบจะไม่ได้
    /// ทำงานและตัวนับจะค้างอยู่ตลอดไป ปุ่มกดหน้างานก็จะตายทั้งวันโดยไม่มีใครรู้สาเหตุ
    /// </para>
    /// <para>
    /// รายการนี้ Windows เป็นคนลบหน้าต่างออกให้เองตอนมันปิด ไม่ว่าจะปิดปกติหรือปิด
    /// เพราะพัง จึงกลับมาทำงานได้เองเสมอ
    /// </para>
    /// <para>
    /// ยกเว้นหน้า Order Detail ซึ่งเป็น modal ในทางเทคนิคแต่เป็นหน้าจอในทางใช้งาน
    /// คนเปิดค้างไว้ดูรายละเอียดหรือแก้ค่าได้เป็นสิบนาที ถ้านับรวมด้วยไลน์จะหยุด
    /// ทั้งช่วงนั้น ส่วนกล่องยืนยันที่เด้งซ้อนขึ้นมาบนหน้านั้นยังนับตามปกติ
    /// เพราะเป็นการตัดสินใจสั้น ๆ ที่ต้องตอบก่อนอย่างอื่นจะเดินต่อ
    /// </para>
    /// </summary>
    private static bool AnyDialogOpen() =>
        Application.OpenForms.Cast<Form>().Any(f =>
            f.Modal && f.Visible && f is not OrderDetailDialog);

    /// <summary>
    /// บอกคนหน้างานว่าการกดไม่ผ่านเพราะจอไม่ว่าง ให้กดใหม่
    ///
    /// ใช้ข้อความลอย ไม่ใช่กล่องที่ต้องกดปิด เพราะตอนนี้อาจมีกล่องอื่นเปิดค้างอยู่แล้ว
    /// การเปิดกล่องซ้อนจะยิ่งทำให้จอตันหนักกว่าเดิม
    /// </summary>
    private void ShowBlockedPress() =>
        Notify.Warn(this, "มีคนกดปุ่มหน้างาน — ระบบกำลังทำงานอื่นอยู่ กรุณากดอีกครั้ง");

    private async Task OnPushButtonPressedAsync(string? machineOverride = null)
    {
        if (_api == null || _pushHandling || IsDisposed) return;

        _pushHandling = true;
        try
        {
            var machine = machineOverride ?? MachineOfThisStation();

            // งานที่เข้าเครื่องเดิมหลายรอบจะถือเครื่องไว้ให้รอบถัดไปหรือไม่
            // เป็นตัวเลือกที่ Setting → ตัวเลือกหน้างาน ค่าเริ่มต้นคือถือไว้
            var (release, error) = await _api.ReleaseMachineAsync(
                machine, StationService.HoldForNextRound);
            if (IsDisposed) return;

            if (release == null)
            {
                Notify.Warn(this, $"ปล่อยเครื่อง {machine} ไม่สำเร็จ — {error}");
                return;
            }

            // ห้ามออกจากเมธอดตรงนี้ — บรรทัดท้ายเมธอดคือตัวที่ไปหยิบงานรอบถัดไป
            // มาส่งเข้าเครื่องทันที ถ้าข้ามไป การส่งจะไปรอนาฬิกา poll รอบหน้า
            // คนกดปุ่มจะเห็นเป็นค้างไปหลายวินาทีก่อนอะไรจะเกิดขึ้น
            if (release.Next != null)
            {
                Notify.Success(this, $"{machine} ว่างแล้ว · งานถัดไปในคิวจะถูกส่งให้");
            }
            else
            {
                // ไม่มีใครรอคิว — เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้น
                //
                // มีคิวรออยู่ไม่ต้องเลื่อน เพราะงานถัดไปเขียนตำแหน่งของมันทับอยู่แล้ว
                // การเลื่อนกลับก่อนแล้วเลื่อนไปใหม่คือขยับหัวสองรอบโดยไม่ได้อะไร
                Notify.Success(this, $"{machine} ว่างแล้ว · ไม่มีงานรอคิว");
                await ResetHeadPositionAsync(machine);
            }
        }
        finally
        {
            _pushHandling = false;
        }

        if (!IsDisposed) await RefreshDataAsync(force: true);
    }

    /// <summary>
    /// อัปเดตแถบบอกว่าแต่ละสถานีถืองานอะไรอยู่ — โหมดทดสอบเท่านั้น
    ///
    /// <para>
    /// "ถืออยู่" อ่านจากคิวที่ backend ไม่ได้ถามเครื่อง เพราะโปรแกรมไม่มีทางรู้ว่า
    /// เครื่องพิมพ์เสร็จหรือยัง สิ่งเดียวที่บอกว่าเครื่องว่างคือคนกดปุ่มหน้างาน
    /// แถบนี้จึงแสดงสิ่งที่คิวเชื่ออยู่ ซึ่งเป็นตัวที่ตัดสินว่างานถัดไปเข้าได้ไหม
    /// </para>
    /// </summary>
    private async Task RefreshStationBarAsync()
    {
        if (_api == null || IsDisposed) return;

        var (rows, error) = await _api.GetMachineQueueAsync();
        if (error != null || IsDisposed) return;

        // เก็บไว้ให้แผง Processing ใช้ต่อ จะได้ไม่ต้องยิงถามคิวซ้ำอีกรอบ
        _queueRows = rows;

        foreach (var (machine, label, queueLabel, button) in StationSlots())
        {
            var holder = rows.FirstOrDefault(r => r.Machine == machine && r.State == "active");

            // เรียงตามลำดับที่จะได้เครื่อง — ตัวแรกในรายการคือคิวที่ 1
            var waiting = rows
                .Where(r => r.Machine == machine && r.State == "pending")
                .OrderBy(r => r.Id)
                .ToList();

            // ปุ่มกดได้ตราบใดที่ยังมีอะไรให้ขยับ — ถืออยู่ก็ปล่อย ว่างแต่มีคนรอก็ยกให้คิว
            //
            // ปุ่มจริงหน้าเครื่องกดได้ตลอดเวลาอยู่แล้ว และสภาพ "ว่างแต่มีงานรออยู่"
            // เกิดได้จริง เช่นปล่อยเครื่องไปแล้วแต่การยกให้คิวถัดไปไม่สำเร็จ
            // ถ้าปุ่มกดไม่ได้ตอนนั้น จะไม่มีทางดันคิวให้เดินต่อได้เลย
            button.Enabled = holder != null || waiting.Count > 0;

            if (holder == null)
            {
                label.Text = $"● {machine} — ว่าง";
                label.ForeColor = waiting.Count > 0 ? WaitingColor : DesignTokens.SuccessText;
            }
            else
            {
                // ถึงคิวแล้วแต่ยังไม่ได้ส่ง กับส่งเข้าเครื่องไปแล้ว เป็นคนละสภาพกัน
                var what = holder.SentAt == null ? "รอ ST1 ส่ง" : "กำลังพิมพ์";

                label.Text = $"● {machine} — {JobName(holder.PrintJobsId)} · {what}";
                label.ForeColor = WaitingColor;
            }

            queueLabel.Text = QueueLine(waiting);
        }
    }

    /// <summary>
    /// บรรทัดบอกคิวของเครื่องหนึ่ง — ใบถัดไปคือใบไหน และมีทั้งหมดกี่ใบ
    ///
    /// บอกชื่องานของใบถัดไปด้วย ไม่ใช่แค่จำนวน เพราะคนหน้างานต้องรู้ว่าเดี๋ยวจะได้
    /// ชิ้นงานของล็อตไหนมาเข้าเครื่องต่อ จะได้เตรียมของถูกใบ
    /// </summary>
    private string QueueLine(List<MachineQueueRow> waiting)
    {
        if (waiting.Count == 0) return "ไม่มีคิวรอ";

        var next = $"คิวถัดไป: {JobName(waiting[0].PrintJobsId)}";
        return waiting.Count == 1 ? next : $"{next}   (รอทั้งหมด {waiting.Count} ใบ)";
    }

    /// <summary>คิวของทุกเครื่องจากรอบล่าสุด — ว่างแปลว่ายังไม่เคยอ่านสำเร็จ</summary>
    private List<MachineQueueRow> _queueRows = [];

    /// <summary>สีของเครื่องที่ไม่ว่าง หรือว่างแต่ยังมีงานค้างคิวอยู่</summary>
    private static readonly Color WaitingColor = Color.FromArgb(214, 108, 0);

    /// <summary>สามช่องของแถบสถานี — ชื่อเครื่อง ป้ายสถานะ ป้ายคิว และปุ่มจำลอง</summary>
    private IEnumerable<(string Machine, AntdUI.Label Label, AntdUI.Label Queue, AntdUI.Button Button)> StationSlots()
    {
        yield return ("MK", lblStationMk, lblQueueMk, btnSimPushMk);
        yield return ("UV1", lblStationUv1, lblQueueUv1, btnSimPushUv1);
        yield return ("UV2", lblStationUv2, lblQueueUv2, btnSimPushUv2);
    }

    /// <summary>
    /// เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นหลังปล่อยเครื่องแล้วไม่มีงานรอคิว
    ///
    /// <para>
    /// ทำเฉพาะเครื่อง MK เพราะช่องตำแหน่งของหัวพ่นอยู่บน PLC ตัวหลัก ส่วน UV ใช้
    /// ชุดแคลมป์คนละตัวและยังไม่ได้ตั้ง address ไว้
    /// </para>
    /// <para>
    /// ยังไม่ได้ตั้ง address ของช่องตำแหน่งก็ไม่ทำอะไรและไม่ฟ้อง เพราะเป็นสภาพปกติ
    /// ของหน้างานที่ยังไม่ได้กรอกตาราง register map ให้ครบ
    /// </para>
    /// </summary>
    private async Task ResetHeadPositionAsync(string machine)
    {
        if (!string.Equals(machine, "MK", StringComparison.OrdinalIgnoreCase)) return;

        var results = await PlcOrderService.ResetPositionAsync(_api);
        if (IsDisposed || results.Count == 0) return;

        var failed = results.Where(r => r.Error != null).ToList();
        if (failed.Count == 0)
        {
            Notify.Success(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นแล้ว");
            return;
        }

        Notify.Warn(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นไม่สำเร็จ — "
            + string.Join(" · ", failed.Select(r => $"{r.Name} {r.Error}")));
    }

    private static bool SentAlready(ResolvedJobResponse resolved, string step) =>
        resolved.Commands?.Any(c => c.Success &&
            string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    private void StartPolling()
    {
        _pollTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _pollTimer.Tick += async (_, _) => await RefreshDataAsync();
        _pollTimer.Start();
    }

    private async Task RefreshDataAsync(bool force = false)
    {
        if (_api == null) return;

        // ระหว่างส่งงานห้ามผูก DataSource ใหม่ ไม่งั้นแถวขยับใต้มือผู้ใช้
        // และกล่องเลือกรุ่นย่อยของ UV อาจถูกวาดทับ
        if (_sending) return;

        // รอบก่อนยังไม่จบก็ข้ามรอบนี้ไป — นาฬิกาเดินทุก 5 วิ แต่ถ้า backend ช้า
        // หรือต่อไม่ติด คำขอหนึ่งรออยู่ได้ถึง 10 วิ ไม่กันไว้รอบใหม่จะทับกันไป
        // เรื่อย ๆ จนมีคำขอค้างพร้อมกันหลายชุด แล้วเด้งกล่องผิดพลาดตามมาเป็นพรวด
        if (_refreshing) return;

        _refreshing = true;
        try
        {
            DateTime? fromUtc = null, toUtc = null;
            if (_showHistory && TryGetDateRange(out var from, out var to))
            {
                fromUtc = ToUtcFromThai(from);
                toUtc = ToUtcFromThai(to);
            }

            var (jobs, error) = await _api.GetAllJobsAsync(100, fromUtc, toUtc);
            if (IsDisposed) return;
            if (error != null)
            {
                tblOrders.EmptyText = $"Error: {error}";
                return;
            }
            _allJobs = jobs;

            // ทำก่อนเช็ค signature เพราะคำขอจาก ST3 ไม่ได้เปลี่ยนอะไรที่ตารางวาด
            // ถ้าไปทำหลังจากนั้น รอบที่หน้าจอไม่มีอะไรเปลี่ยนจะข้ามคำขอไปเลย
            await RecoverAbandonedRemoteStartsAsync();
            if (IsDisposed) return;

            await ProcessRemoteStartsAsync();
            if (IsDisposed) return;

            await ProcessMachineQueueAsync();
            if (IsDisposed) return;

            await RefreshStationBarAsync();
            if (IsDisposed) return;

            await ShowRemoteErrorsAsync();
            if (IsDisposed) return;

            // ผูก DataSource ใหม่ทีไร ตารางจะรีเซ็ตทั้งลำดับที่เรียงไว้และตำแหน่ง scroll
            // รอบ poll ที่ข้อมูลไม่เปลี่ยนจึงไม่ต้องผูกใหม่ ไม่งั้นทุก 5 วิจะกระตุกทีนึง
            var signature = BuildSignature(jobs) + QueueSignature();
            if (!force && signature == _lastSignature) return;

            _lastSignature = signature;
            RebindTable();
            await UpdateProcessingAsync();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                tblOrders.EmptyText = $"Error: {ex.Message}";
        }
        finally
        {
            _refreshing = false;
        }
    }

    /// <summary>
    /// ย่อสภาพคิวให้เหลือข้อความเดียว — ช่องสถานะเอาคิวมาแสดงด้วย ถ้าไม่นับรวม
    /// รอบที่มีแต่คิวเปลี่ยนจะไม่วาดตารางใหม่ แล้วสถานะจะค้างอยู่ของเก่า
    /// </summary>
    private string QueueSignature()
    {
        var sb = new System.Text.StringBuilder(_queueRows.Count * 16);
        foreach (var r in _queueRows.OrderBy(r => r.Id))
            sb.Append(r.Id).Append(r.State).Append('|');
        return sb.ToString();
    }

    /// <summary>ย่อทุกอย่างที่ตารางวาดให้เหลือข้อความเดียว ไว้เทียบว่ารอบนี้มีอะไรเปลี่ยนไหม</summary>
    private static string BuildSignature(List<PrintJob> jobs)
    {
        var sb = new System.Text.StringBuilder(jobs.Count * 48);
        foreach (var j in jobs)
        {
            sb.Append(j.Id).Append('|')
              .Append(j.Status).Append('|')
              .Append(j.OrderNo).Append('|')
              .Append(j.Qty).Append('|')
              .Append(j.StStatus).Append('|')
              .Append(j.CreatedAt?.Ticks).Append('|')
              .Append(j.UpdatedAt?.Ticks).Append('|')
              .Append(j.PlanRouting?.MarkingMethod).Append('|')
              .Append(j.PlanRouting?.ProcessSequence).Append('|')
              .Append(j.Commands?.Count(c => c.Success) ?? 0).Append(';');
        }
        return sb.ToString();
    }

    private void SwitchTab(bool showHistory)
    {
        _showHistory = showHistory;

        ButtonStyles.SetSelected(btnTabList, !showHistory);
        ButtonStyles.SetSelected(btnTabHistory, showHistory);

        // ตัวกรองวันที่มีเฉพาะแท็บ History — ออกจากแท็บแล้วล้างค่าทิ้ง
        // ไม่งั้นกลับเข้ามาใหม่จะเห็นรายการหายไปโดยไม่รู้ว่าโดนกรองอยู่
        lblDateFilter.Visible = showHistory;
        dtpHistoryRange.Visible = showHistory;
        btnSearchDate.Visible = showHistory;
        btnClearDate.Visible = showHistory;
        if (!showHistory) dtpHistoryRange.Value = null;

        // ออกจากแท็บแล้วการเลือกเดิมไม่มีความหมาย ล้างทั้งไฮไลต์และรูป
        _selectedJobId = null;
        ShowPreviewSides(null, null);

        // แผงขวามีเฉพาะแท็บ List
        pnlProcessing.Visible = !showHistory;

        ApplyTabColumns(showHistory);

        RebindTable();
        _ = UpdateProcessingAsync();
    }

    /// <summary>
    /// ลำดับตั้งต้นของตาราง 2 ชั้น — ชั้นแรกสถานะ ชั้นสองเวลารับงานใหม่สุดขึ้นก่อน
    ///
    /// งานที่เดินอยู่ต้องอยู่บนสุดเสมอ เพราะเป็นงานที่ผู้ใช้ต้องแตะ ส่วนงานที่รอ
    /// อยู่ล่าง — ตารางยาวแค่ไหนก็ไม่ต้องเลื่อนหา
    ///
    /// AntdUI เรียงได้ทีละคอลัมน์ ทำ 2 ชั้นในตัวมันไม่ได้ จึงเรียงลิสต์เองก่อน
    /// ส่งเข้าตาราง ถ้าผู้ใช้กดหัวคอลัมน์ การเรียงนั้นจะทับลำดับนี้ทั้งหมด
    /// (กดซ้ำจนลูกศรกลับเป็นเทาก็ได้ลำดับนี้คืน)
    /// </summary>
    private static int StatusRank(PrintJob job) =>
        string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase) ? 0
        : string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase) ? 1
        : 2;

    private void RebindTable()
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses;
        int station = StationService.Current;

        // ST1 เห็นประวัติทั้งสาย รวมงานที่ ST3 ทำจนจบซึ่งตัวเองไม่เคยเห็นในแท็บ List
        // เพราะเป็นจอที่ใช้ตามงานทั้งกระบวนการ
        //
        // ส่วน ST3 ไม่มีแท็บ History ให้กดอยู่แล้ว เงื่อนไข IsSt3 ตรงนี้จึงเป็นแค่
        // ตัวกันไว้ เผื่อวันหลังเปิดแท็บคืนให้ ST3 จะได้ยังกรองเฉพาะงานของตัวเอง
        bool showEveryStation = _showHistory && !StationService.IsSt3;

        var filtered = _allJobs
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase))
            .Where(j => showEveryStation
                || MarkingMethodService.VisibleAt(station, j.PlanRouting?.MarkingMethod))
            .OrderBy(StatusRank)
            .ThenByDescending(j => j.CreatedAt ?? DateTime.MinValue)
            .ToList();

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _);

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList();
        tblOrders.EmptyText = _allJobs.Count == 0
            ? "No orders"
            : dateFiltered && rows.Count == 0
                ? "ไม่มีงานในช่วงวันที่ที่เลือก"
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(JobStatusDisplay.Text))})";
        tblOrders.DataSource = null;
        tblOrders.DataSource = rows;
        ReapplySort();
        RestoreSelection();
    }

    /// <summary>
    /// ตั้ง DataSource ใหม่ทีไร AntdUI ล้างลำดับที่เรียงไว้ทุกที (แต่ลูกศรบนหัวตารางยังค้าง)
    /// เรียงกลับตามคอลัมน์เดิม ไม่งั้นหัวตารางกับข้อมูลจะไม่ตรงกัน
    /// </summary>
    private void ReapplySort()
    {
        if (tblOrders.Columns == null) return;

        foreach (var column in tblOrders.Columns)
        {
            if (column.SortMode == AntdUI.SortMode.NONE) continue;
            tblOrders.Sort(column);
            return;
        }
    }

    /// <summary>ช่วงวันที่ที่เลือก ขยายเป็นทั้งวันตามเวลาไทย (00:00 ถึง 23:59:59)</summary>
    private bool TryGetDateRange(out DateTime from, out DateTime to)
    {
        from = default;
        to = default;

        var value = dtpHistoryRange.Value;
        if (value == null || value.Length < 2) return false;

        var a = value[0].Date;
        var b = value[1].Date;
        if (b < a) (a, b) = (b, a);

        from = a;
        to = b.AddDays(1).AddTicks(-1);
        return true;
    }

    private async void TblOrders_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Record is not OrderRow row) return;
        if (_api == null || _rowBusy) return;

        _rowBusy = true;
        try
        {
            await HandleRowButtonAsync(e.Btn?.Id, row);
        }
        finally
        {
            _rowBusy = false;
        }
    }

    /// <summary>
    /// รออ่านข้อมูลงานสดจาก backend พร้อมขึ้นการ์ดบอกเมื่อรอนานผิดปกติ
    ///
    /// ปกติอ่านเสร็จในหลักสิบมิลลิวินาที ขึ้นการ์ดทุกครั้งจะวาบจนรำคาญ จึงรอ
    /// สักครู่ก่อนแล้วค่อยขึ้น — เห็นการ์ดเมื่อไหร่แปลว่าเครือข่ายมีปัญหาจริง
    /// </summary>
    private async Task<ResolvedJobResponse?> LoadJobAsync(int jobId, string busyText)
    {
        var task = _api!.GetResolvedJobAsync(jobId);
        if (await Task.WhenAny(task, Task.Delay(SlowLoadMs)) == task) return await task;

        ShowSending(busyText);
        try
        {
            return await task;
        }
        finally
        {
            if (!IsDisposed) ShowSending(null);
        }
    }

    private async Task HandleRowButtonAsync(string? buttonId, OrderRow row)
    {
        if (buttonId == "detail")
        {
            var resolved = await LoadJobAsync(row.Id, $"กำลังโหลดข้อมูล · {JobName(row.Id)}");
            if (IsDisposed) return;
            if (resolved == null)
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ {JobName(row.Id)} ได้");
                return;
            }
            await ShowDetailDialogAsync(resolved);
        }
        else if (buttonId == "start")
        {
            await StartJobAsync(row.Id);
        }
        else if (buttonId == "complete")
        {
            await CompleteJobAsync(row.Id);
        }
        else if (buttonId == "cancel")
        {
            await CancelJobAsync(row.Id);
        }
        else if (buttonId == "restore")
        {
            await RestoreJobAsync(row.Id);
        }
    }

    /// <summary>
    /// ชื่อเรียกงานที่พนักงานหน้างานใช้จริง — "ERP MFG (เลขล็อต)"
    ///
    /// ใช้แทนเลข id ในข้อความที่พูดถึง "งานอีกใบ" เพราะ id เป็นเลขในฐานข้อมูล
    /// ที่ไม่ได้อยู่บนใบสั่งงานและไม่มีในตาราง คนอ่านจึงไล่หาไม่เจอว่าเป็นงานไหน
    ///
    /// ขาดค่าไหนก็ตัดออก เหลือเท่าที่รู้ ไม่มีเลยค่อยตกไปใช้ id
    /// </summary>
    /// <summary>
    /// ชื่อเรียกงานจากเลข id — ใช้ในข้อความที่มีแต่ id อยู่ในมือ
    /// หางานในรายการที่โหลดมาไม่เจอค่อยตกไปใช้ id ให้ยังอ้างอิงอะไรได้อยู่
    /// </summary>
    private string JobName(int jobId)
    {
        var job = _allJobs.FirstOrDefault(j => j.Id == jobId);
        return job == null ? $"#{jobId}" : JobLabel(job);
    }

    private static string JobLabel(PrintJob job) => JobDisplay.Label(job);

    /// <summary>ค่าแรกที่ไม่ว่าง — ว่างทั้งคู่คืนขีด ให้เข้าชุดกับคอลัมน์อื่นที่ใช้ขีดแทนช่องว่าง</summary>
    private static string FirstFilled(params string?[] values)
    {
        foreach (var value in values)
        {
            var text = (value ?? "").Trim();
            if (text.Length > 0) return text;
        }
        return Dash;
    }

    /// <summary>งานที่ถูกยกเลิก — แยกจากงานที่จบแล้ว ทั้งที่อยู่แท็บ History เหมือนกัน</summary>
    private static bool IsCancelled(PrintJob job) =>
        string.Equals(job.Status, "Cancel", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// เอางานที่ยกเลิกไว้กลับมาพิมพ์ใหม่ — สถานะกลับเป็น Waiting งานจึงกลับไป
    /// อยู่ในแท็บ List ให้กดเริ่มงานได้ตามปกติ
    ///
    /// ไม่ได้สร้าง job ใหม่โดยตั้งใจ — เลข job, pattern, ข้อมูล UV และระยะแคลมป์
    /// ยังเป็นชุดเดิมทั้งหมด ไม่ต้องสแกนบาร์โค้ดซ้ำ และประวัติการส่งของรอบก่อน
    /// ยังอยู่ครบใน print_job_commands ให้ย้อนดูได้ว่าเคยพิมพ์อะไรไปแล้วบ้าง
    /// </summary>
    private async Task RestoreJobAsync(int jobId)
    {
        if (_api == null) return;

        if (!Confirm.Ask(this, "ยืนยันนำกลับมาพิมพ์ใหม่",
                $"{JobName(jobId)}\n\n"
                + "งานจะกลับไปอยู่ในรายการงาน รอกดเริ่มงานอีกครั้ง\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Waiting");
        if (IsDisposed) return;

        if (!ok)
        {
            Notify.ErrorModal(this, "นำกลับมาไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้");
            return;
        }

        // ล้างคำขอ/ข้อความผิดพลาดที่ค้างจากรอบก่อน ไม่งั้น ST1 อาจหยิบธงเก่าไปส่งทันที
        // ที่งานกลับมาเป็น Waiting ทั้งที่ยังไม่มีใครกดเริ่มงานรอบใหม่
        await _api.SetRemoteStartAsync(jobId, requested: false);
        if (IsDisposed) return;

        Notify.Success(this, $"{JobName(jobId)} กลับไปอยู่ในรายการงานแล้ว");
        await RefreshDataAsync(force: true);
    }

    // ── ยกเลิกงาน ──────────────────────────────────────────

    /// <summary>
    /// ยกเลิกงาน — สถานะไปเป็น "Cancel" งานจึงหลุดจากแท็บ List ไปโผล่ที่ History
    /// เหมือนงานที่จบแล้ว ต่างกันแค่ป้ายสถานะ
    ///
    /// ไม่ส่งอะไรเข้าเครื่องและไม่ย้อนคำสั่งที่ส่งไปแล้ว — สิ่งที่พิมพ์ไปแล้วก็พิมพ์ไปแล้ว
    /// ประวัติการส่งยังอยู่ครบใน print_job_commands ตามเดิม
    /// </summary>
    private async Task CancelJobAsync(int jobId)
    {
        if (_api == null) return;

        if (!Confirm.Ask(this, "ยืนยันยกเลิกงาน",
                $"ยกเลิก {JobName(jobId)}\n\n"
                + "งานจะถูกย้ายออกจากรายการไปอยู่ในประวัติ\n"
                + "ถ้าต้องการทำต่อ กดพิมพ์ใหม่ได้ที่แท็บ History\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        // ล้างคิวก่อนเปลี่ยนสถานะ งานที่ยกเลิกต้องไม่ถือเครื่องหรือค้างคิวไว้
        await _api.ClearMachineQueueAsync(jobId);

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Cancel");
        if (IsDisposed) return;

        if (!ok)
        {
            Notify.ErrorModal(this, "ยกเลิกงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะยกเลิกได้");
            return;
        }

        // ยกเลิกตอนที่ ST3 ฝากคำขอค้างไว้ ธงต้องหายไปด้วย ไม่งั้นมันจะค้างอยู่กับงาน
        // ข้ามไปถึงตอนที่งานถูกนำกลับมาพิมพ์ใหม่
        await _api.SetRemoteStartAsync(jobId, requested: false);
        if (IsDisposed) return;

        Notify.Success(this, $"ยกเลิก {JobName(jobId)} แล้ว");
        await RefreshDataAsync(force: true);
    }

    // ── เริ่มงาน ────────────────────────────────────────────

    /// <summary>กำลังส่งงานอยู่ — กันทั้งการกดซ้ำและการรีเฟรชตารางทับ</summary>
    private bool _sending;

    /// <summary>
    /// งานที่กดเริ่มได้ = ยังไม่ได้เริ่ม · สถานีนี้มีสิทธิ์เริ่ม · และรหัสใช้งานได้จริง
    /// <para>
    /// รหัส "00" ไม่มีขั้นตอนต้องส่งเลย แต่ยังกดเริ่มได้ — เป็นงานที่ทำด้วยมือ
    /// การกดเริ่มแค่เปลี่ยนสถานะให้คนอื่นเห็นว่ามีคนรับไปทำแล้ว
    /// ส่วน "21" เป็นรหัสที่ไม่มีอยู่จริง กดเริ่มไม่ได้ ให้ตกไปใช้ปุ่มจบงานแทน
    /// </para>
    /// </summary>
    /// <summary>
    /// งานที่ยังไม่มีใครเริ่ม และสถานีนี้ไม่ใช่คนที่มีสิทธิ์เริ่ม = ยังไม่ถึงตาสถานีนี้
    ///
    /// <para>
    /// เกิดที่ ST3 กับงาน 11 และ 12 (รวม 13 31 32 33) ซึ่ง ST3 เห็นในตารางแต่ต้อง
    /// ให้ ST1 เป็นคนเริ่ม เดิมพอ <see cref="CanStart"/> คืน false งานพวกนี้จะตกไป
    /// เข้าเงื่อนไขปุ่มจบงานทันที กลายเป็นกดจบงานที่ยังไม่เคยพ่นอะไรลงชิ้นงานได้
    /// </para>
    /// <para>
    /// รหัสที่ไม่มีอยู่จริง (21) ไม่เข้าข่ายนี้โดยตั้งใจ — มันกดเริ่มไม่ได้อยู่แล้ว
    /// และต้องเหลือปุ่มจบงานไว้เป็นทางเคลียร์ออกจากตาราง
    /// </para>
    /// </summary>
    private static bool NotMyTurnYet(PrintJob job)
    {
        bool started = job.Commands?.Any(c => c.Success) == true;
        if (started) return false;

        var method = job.PlanRouting?.MarkingMethod;
        if (MarkingMethodService.Resolve(method).NoCase) return false;

        return !MarkingMethodService.CanStartAt(StationService.Current, method);
    }

    private static bool CanStart(PrintJob job)
    {
        if (!string.Equals(job.Status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return false;

        // มีคำขอจาก ST3 ค้างอยู่ = งานถูกจองไว้แล้ว แม้สถานะจะยังเป็น Waiting
        // (Working จะถูกตั้งก็ต่อเมื่อ ST1 ต่อเครื่องติดและส่งจริง) ถ้าไม่กันตรงนี้
        // จะมีคนกดเริ่มซ้ำระหว่างที่คำขอกำลังรอ ST1 อยู่ กลายเป็นส่งสองรอบ
        if (job.RemoteStart is RemotePending or RemoteSending)
            return false;

        var method = job.PlanRouting?.MarkingMethod;
        if (!MarkingMethodService.CanStartAt(StationService.Current, method))
            return false;

        return !MarkingMethodService.Resolve(method).NoCase;
    }

    /// <summary>
    /// เริ่มงาน: ส่งเข้าสถานีแรกตาม marking method แล้วเปลี่ยนสถานะเป็น Working
    /// <para>
    /// หนึ่งสถานีรับงานได้ทีละงาน ตรงกับความจริงหน้างานที่ชิ้นงานอยู่ที่เครื่อง
    /// ได้ทีละชิ้น ถ้าสถานีแรกไม่ว่างจะไม่ยอมให้เริ่ม
    /// </para>
    /// </summary>
    private async Task StartJobAsync(int jobId)
    {
        if (_api == null || _sending) return;

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}");
        if (IsDisposed) return;
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้");
            return;
        }

        var method = resolved.PlanRouting?.MarkingMethod;
        int station = StationService.Current;

        if (!MarkingMethodService.CanStartAt(station, method))
        {
            Notify.WarnModal(this, "เริ่มงานที่สถานีนี้ไม่ได้",
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + ((method ?? "").Trim() == "10"
                    ? "งาน marking 10 เริ่มได้ที่ ST3 เท่านั้น"
                    : "งานนี้เริ่มได้ที่ ST1 เท่านั้น"));
            return;
        }

        var plan = MarkingMethodService.Resolve(method);
        if (plan.NoCase)
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"{JobName(jobId)} ใช้รหัส marking ที่ไม่มีอยู่จริง ({Method(method)})");
            return;
        }

        // marking 00 ไม่มีคำสั่งต้องส่งเข้าเครื่องเลย — เป็นงานที่ทำด้วยมือล้วน
        // การกดเริ่มจึงแค่เปลี่ยนสถานะให้คนอื่นเห็นว่ามีคนรับไปทำแล้ว
        if (plan.Steps.Count == 0)
        {
            await StartWithoutSendingAsync(jobId, method);
            return;
        }

        // เลือกโปรแกรม UV ให้เสร็จตรงนี้ ก่อนจะสรุปให้ดูและก่อนจองคิว
        //
        // กล่องเลือกรุ่นย่อยกับกล่องยืนยัน default ต้องเด้งที่จอของคนที่กดเริ่ม
        // ไม่ใช่ไปเด้งตอนส่งจริงซึ่งเกิดที่ ST1 เสมอ
        var uvPicks = PickUvPrograms(plan, resolved);
        if (uvPicks == null || IsDisposed) return;

        // สรุปให้ดูก่อนว่าจะส่งอะไรเข้าเครื่องไหนบ้าง แล้วค่อยลงมือ
        //
        // ถามก่อนจอง ไม่ใช่หลังจอง — กดยกเลิกแล้วต้องไม่มีอะไรค้างอยู่ในคิวเลย
        if (!await ConfirmStartAsync(jobId, resolved, plan, uvPicks)) return;
        if (IsDisposed) return;

        // ต่อให้ครบทุกเครื่องก่อน ถึงจะจองคิวและเริ่มส่ง
        //
        // งานที่ใช้หลายเครื่องต้องต่อได้ครบถึงจะเริ่มได้ ถ้าเริ่มทั้งที่เครื่องหลังต่อไม่ติด
        // เครื่องหน้าจะรับงานไปพ่นลงชิ้นงานจริงแล้ว ย้อนคืนไม่ได้ และงานจะค้างครึ่งทาง
        // คือกดเริ่มใหม่ก็ไม่ได้เพราะสถานะเป็นกำลังผลิตไปแล้ว
        //
        // ตรวจหลังกล่องยืนยัน ไม่ใช่ก่อน เพราะการไล่ต่อทุกเครื่องกินเวลาหลายวินาที
        // ไม่ควรให้คนที่แค่เปิดดูแล้วกดยกเลิกต้องรอ
        //
        // ข้ามที่ ST3 เพราะ ST3 ไม่ได้เป็นคนส่ง มันแค่จองคิวไว้ให้ ST1 หยิบไปส่ง
        // เครื่องต่อสายอยู่กับ PC ของ ST1 ที่เดียว และพาธของ CPI.db3 ที่ตรวจด้วยก็
        // เป็นของ ST1 การตรวจที่ ST3 จึงตอบว่าต่อไม่ได้เสมอ ทั้งที่ ST1 ต่อได้อยู่
        //
        // ไม่เสียอะไรด้วย เพราะงานที่ ST3 กดเริ่มได้มีแต่รหัส 10 ซึ่งใช้เครื่องเดียว
        // (UV2) ไม่มีเคสเริ่มไปได้ครึ่งทางแบบงานหลายเครื่องที่ด่านนี้มีไว้กัน และถ้า
        // ST1 ส่งไม่ผ่านจริง มันรายงานกลับมาที่จอ ST3 ให้อยู่แล้ว
        if (!StationService.IsSt3 && await BlockedByUnreachableAsync(jobId, plan, resolved)) return;
        if (IsDisposed) return;

        // จองทุกเครื่องที่แผนของงานนี้ต้องใช้ ในคราวเดียว
        //
        // จองก่อนส่งเสมอ เพราะการจองคือสิ่งที่บอกว่างานนี้มีสิทธิ์ในเครื่องไหนบ้าง
        // เครื่องที่ว่างจะถูกส่งต่อทันทีข้างล่าง ส่วนเครื่องที่ไม่ว่างก็รออยู่ในคิว
        // จนกว่าคนหน้างานจะกดปุ่มปล่อยเครื่อง
        var (queued, queueError) = await _api.EnqueueMachinesAsync(
            jobId, QueueItemsFor(plan.Steps, uvPicks));
        if (IsDisposed) return;

        if (!queued)
        {
            Notify.ErrorModal(this, "จองเครื่องไม่สำเร็จ",
                $"{JobName(jobId)} ยังไม่ได้เข้าคิว" + Environment.NewLine + Environment.NewLine
                + (queueError ?? "ติดต่อ backend ไม่ได้"));
            return;
        }

        // ST3 ไม่ได้ต่อสายเข้าเครื่อง จองแล้วจบ ST1 จะหยิบไปส่งให้เองจากคิว
        if (station == StationService.St3)
        {
            Notify.Success(this, $"{JobName(jobId)} เข้าคิวแล้ว · ST1 จะส่งให้");
            await RefreshDataAsync(force: true);
            return;
        }

        await SendQueuedForJobAsync(jobId, resolved, $"เริ่มงาน {JobName(jobId)}");
    }

    /// <summary>
    /// สรุปสิ่งที่จะถูกส่งเข้าเครื่อง แล้วรอคนยืนยัน — false = กดยกเลิก ไม่ต้องทำต่อ
    ///
    /// <para>
    /// บอกด้วยว่าเครื่องไหนว่างและเครื่องไหนต้องเข้าคิวรอ เพราะกดเริ่มงานหนึ่งครั้ง
    /// อาจได้ผลต่างกันในแต่ละเครื่อง คนกดจะได้รู้ตั้งแต่ก่อนกดว่าอะไรจะเข้าเดี๋ยวนี้
    /// และอะไรต้องรอคนกดปุ่มหน้างานก่อน
    /// </para>
    /// <para>
    /// อ่านคิวก่อนจอง แถวที่เห็นตอนนี้จึงเป็นของงานใบอื่นล้วน ๆ ไม่ใช่ของใบที่กำลังกด
    /// </para>
    /// </summary>
    private async Task<bool> ConfirmStartAsync(
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan,
        Dictionary<string, string> uvPicks)
    {
        var (rows, _) = await _api!.GetMachineQueueAsync();
        if (IsDisposed) return false;

        return Confirm.Ask(this, "ยืนยันเริ่มงาน",
            BuildStartPreview(jobId, resolved, plan, rows, uvPicks));
    }

    /// <summary>ข้อความสรุปที่โชว์ในกล่องยืนยัน — แยกไว้ให้ทดสอบข้อความได้โดยไม่ต้องเปิดกล่อง</summary>
    private string BuildStartPreview(
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan, List<MachineQueueRow> rows,
        Dictionary<string, string> uvPicks)
    {
        var body = new List<string>
        {
            $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}",
            "",
        };

        foreach (var step in plan.Steps)
        {
            int station = JobStationService.StationOf(step) ?? 0;
            var holder = rows.FirstOrDefault(r => r.Machine == step && r.State == "active");

            var state = holder == null
                ? "ว่าง · ส่งเดี๋ยวนี้"
                : $"ไม่ว่าง ({JobName(holder.PrintJobsId)} ค้างอยู่) · เข้าคิวรอปุ่มกดหน้างาน";

            body.Add($"[ {step} · ST{station} ]  {state}");
            body.AddRange(StepPreviewLines(step, resolved, uvPicks).Select(line => "      " + line));
            body.Add("");
        }

        return string.Join(Environment.NewLine, body).TrimEnd();
    }

    /// <summary>ข้อมูลที่จะถูกส่งเข้าเครื่องของขั้นตอนหนึ่ง เขียนให้อ่านจากที่ไกล ๆ ได้</summary>
    private static List<string> StepPreviewLines(
        string step, ResolvedJobResponse resolved, Dictionary<string, string> uvPicks)
    {
        var lines = new List<string>();

        if (string.Equals(step, "MK", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var (nameKey, fallback, ordinal) in MkHeads)
            {
                var head = CustomSettingsManager.Read(nameKey, fallback);
                var config = resolved.Pattern?.InkjetConfigs
                    .FirstOrDefault(c => c.Ordinal == ordinal);

                // เกณฑ์เดียวกับตอนส่งจริง หัวที่งานนี้ไม่ได้ใช้จะถูกสั่งหยุด ไม่ใช่ส่งงานเปล่า
                bool used = config != null
                    && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

                lines.Add(used
                    ? $"{head}: {OrDash(config!.ProgramName)}  (No. {OrDash(config.ProgramNumber?.ToString())})"
                    : $"{head}: ไม่ได้ใช้ — จะสั่งหยุดเครื่อง");
            }

            return lines;
        }

        int uvNumber = string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
        var uvName = UvSettingsManager.Read(
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

        var uv = resolved.UvJobData?.FirstOrDefault(r =>
            string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase));

        if (uv == null)
        {
            lines.Add($"{uvName}: ยังไม่มีข้อมูลของงานนี้");
            return lines;
        }

        lines.Add($"{uvName}: โปรแกรม {OrDash(uv.ProgramName)}");
        lines.Add($"Lot: {OrDash(uv.Lot)}      Name: {OrDash(uv.ErpMfg)}");

        // บอกชื่อไฟล์ที่จะโหลดเข้าเครื่องจริง ซึ่งเลือกไปแล้วก่อนมาถึงกล่องนี้
        //
        // ขั้นที่ยังไม่รู้ต้องบอกสาเหตุให้ตรง ไม่ใช่บอกแค่ว่าจะถามทีหลัง — คนที่ ST3
        // จะได้รู้ว่าต้องไปตั้งโฟลเดอร์ที่เครื่องตัวเอง ไม่ใช่รอให้กล่องเด้งเอง
        if (uvPicks.TryGetValue(step, out var chosen))
            lines.Add($"จะโหลดไฟล์ {chosen}.uvdx เข้าเครื่อง");
        else if (UvSettingsManager.GetDocumentFolder(uvNumber) == null)
            lines.Add($"(เครื่องนี้ยังไม่ได้ตั้งโฟลเดอร์ UV{uvNumber} — ถ้ามีรุ่นย่อย กล่องเลือกจะไปเด้งที่ ST1 ตอนส่ง)");
        else
            lines.Add("(ยังไม่รู้รุ่นย่อย — เครื่องที่ต่อสายจะถามตอนส่ง)");

        return lines;
    }

    /// <summary>UV1 หรือ UV2 คืนเลขเครื่อง · ขั้นอื่น (MK) คืน null</summary>
    private static int? UvNumberOf(string step) =>
        string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1
        : string.Equals(step, "UV2", StringComparison.OrdinalIgnoreCase) ? 2
        : null;

    /// <summary>
    /// เลือกโปรแกรม UV ของทุกขั้นในแผนตั้งแต่ตอนกดเริ่มงาน — null = คนกดยกเลิก
    ///
    /// <para>
    /// เดิมกล่องเลือกรุ่นย่อยกับกล่องยืนยันโปรแกรม default จะเด้งตอนส่งเข้าเครื่องจริง
    /// ซึ่งเกิดที่จอ ST1 เสมอ เพราะสายของเครื่องต่ออยู่ที่นั่นที่เดียว งานที่ ST3 กดเริ่ม
    /// จึงไปค้างรอคนตอบที่จอซึ่งคนกดไม่ได้ยืนอยู่ และงานที่เข้าคิวรอปุ่มกดหน้างาน
    /// ก็เด้งขึ้นตอนที่ไม่มีใครเฝ้า
    /// </para>
    /// <para>
    /// เลือกตรงนี้ทีเดียวจบทุกเครื่องที่แผนต้องใช้ แล้วฝากชื่อที่เลือกไว้กับแถวคิว
    /// ตอนส่งจริงจึงไม่ต้องถามอะไรอีก ไม่ว่าจะส่งทันทีหรือส่งทีหลังหลังกดปุ่มหน้างาน
    /// </para>
    /// <para>
    /// ขั้นที่หาโฟลเดอร์ document ของเครื่องไม่เจอจะไม่ถูกเลือกไว้ตรงนี้ เพราะไม่รู้ว่า
    /// โปรแกรมนั้นมีรุ่นย่อยกี่รุ่น การเดาแล้วฝากชื่อดิบให้ ST1 ส่งเลยอันตรายกว่า
    /// การปล่อยให้ ST1 ถามเองเหมือนเดิม กล่องสรุปจะบอกไว้ว่าขั้นไหนเข้าข่ายนี้
    /// </para>
    /// </summary>
    private Dictionary<string, string>? PickUvPrograms(MarkingPlan plan, ResolvedJobResponse resolved)
    {
        var picks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var step in plan.Steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (UvNumberOf(step) is not int uvNumber) continue;

            var uvRow = resolved.UvJobData?.FirstOrDefault(r =>
                string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase));

            // ไม่มีชื่อโปรแกรมมาให้ = ไม่มีอะไรให้เลือก ปล่อยไปตามทางเดิม
            // ตอนส่งจริงเป็นคนบอกเองว่าข้อมูลของเครื่องนี้ไม่ครบ
            if (string.IsNullOrWhiteSpace(uvRow?.ProgramName)) continue;

            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber);
            if (docFolder == null) continue;

            var pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, this);
            if (pick.Program == null) return null;

            var uvName = UvSettingsManager.Read(
                uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

            if (pick.IsDefault &&
                !UvProgramResolver.ConfirmDefault(uvRow.ProgramName, uvName, this))
                return null;

            picks[step] = pick.Program;
        }

        return picks;
    }

    /// <summary>ช่องว่างให้ขึ้นขีดแทน จะได้ไม่เห็นเป็นบรรทัดแหว่ง</summary>
    private static string OrDash(string? text) =>
        string.IsNullOrWhiteSpace(text) ? Dash : text.Trim();

    /// <summary>หัวพ่นของ MK ทั้งสองตัว — คีย์ชื่อ ชื่อสำรอง และลำดับใน pattern</summary>
    private static readonly (string NameKey, string Fallback, int Ordinal)[] MkHeads =
    [
        ("MK058_NAME", "MK-058", 1),
        ("MK059_NAME", "MK-059", 2),
    ];

    /// <summary>
    /// แปลงลำดับขั้นของแผนเป็นรายการจองเครื่อง
    ///
    /// เครื่องเดียวกันที่โผล่ซ้ำในแผนได้รอบเพิ่มขึ้นทีละหนึ่ง — marking 22 เข้า MK
    /// สองรอบ จึงได้ MK รอบ 1 กับ MK รอบ 2 ซึ่งเป็นคนละคิวกัน
    /// </summary>
    private static List<MachineQueueItem> QueueItemsFor(
        List<string> steps, Dictionary<string, string> uvPicks)
    {
        var rounds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var items = new List<MachineQueueItem>();

        foreach (var step in steps)
        {
            rounds[step] = rounds.TryGetValue(step, out int used) ? used + 1 : 1;
            items.Add(new MachineQueueItem
            {
                Machine = step,
                Round = rounds[step],

                // ชื่อที่คนเลือกไว้ตอนกดเริ่ม — ตอนส่งจริงจะถูกส่งเป็น forcedProgram
                // ซึ่งข้ามทั้งกล่องเลือกรุ่นย่อยและกล่องยืนยัน default ไปเลย
                ProgramName = uvPicks.GetValueOrDefault(step),
            });
        }

        return items;
    }

    /// <summary>
    /// ไล่ส่งงานนี้เข้าเครื่องที่ว่างอยู่ทุกเครื่อง เครื่องที่ไม่ว่างปล่อยไว้ในคิว
    ///
    /// <para>
    /// หยิบทีละเครื่องด้วย claim ซึ่ง backend เป็นคนตัดสินว่าว่างไหมและใครได้ไปก่อน
    /// ฝั่งนี้ไม่เดาเอง เพราะ ST1 กับ ST3 กดพร้อมกันได้
    /// </para>
    /// <para>
    /// เครื่องไหนส่งไม่ผ่าน คืนแถวกลับเป็นรอคิวแล้วบอกคนที่กด — ไม่มีการลองใหม่เอง
    /// เบื้องหลัง คนหน้างานเป็นคนตัดสินว่าจะกดส่งซ้ำไหม
    /// </para>
    /// </summary>
    private async Task SendQueuedForJobAsync(int jobId, ResolvedJobResponse resolved, string title)
    {
        var (rows, _) = await _api!.GetMachineQueueAsync();
        if (IsDisposed) return;

        // เครื่องละครั้งเดียว ไม่ใช่แถวละครั้ง
        //
        // งานที่เข้าเครื่องเดิมหลายรอบ (marking 22) จองไว้หลายแถวบนเครื่องเดียวกัน
        // ถ้าวนตามแถว พอรอบแรกส่งไม่ผ่านแล้วแถวถูกคืนเป็นรอคิว เครื่องจะว่างอีกครั้ง
        // การวนรอบถัดไปก็ขอเครื่องได้และได้แถวเดิมกลับมา กลายเป็นส่งซ้ำเข้าเครื่องจริง
        // สองครั้งจากการกดครั้งเดียว
        //
        // การกดเริ่มงานหนึ่งครั้งควรส่งได้อย่างมากเครื่องละหนึ่งรอบอยู่แล้ว รอบถัดไป
        // ของเครื่องเดิมต้องรอคนกดปุ่มหน้างานเสมอ
        // เรียงตามลำดับที่ชิ้นงานเดินผ่านเครื่องจริง ไม่ใช่ตามที่ backend คืนมา
        //
        // backend เรียงตามชื่อเครื่อง ซึ่งตอนนี้บังเอิญตรงกับลำดับของแผน (MK < UV1 < UV2)
        // แต่ลำดับที่ต้องใช้คือลำดับของแผน ถ้าวันหนึ่งชื่อเครื่องเปลี่ยนหรือแผนสลับลำดับ
        // การพึ่งการเรียงตามตัวอักษรจะพากันผิดแบบเงียบ ๆ
        var planSteps = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps;

        var machines = rows
            .Where(r => r.PrintJobsId == jobId && r.State == "pending")
            .Select(r => r.Machine)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => PlanOrderOf(planSteps, m))
            .ToList();

        var lines = new List<Notify.ResultLine>();
        bool anySent = false;

        // เครื่องไม่ว่าง = เข้าคิวจริง ไม่ใช่ส่งไม่ผ่าน ต้องแยกจากกันให้ชัด
        // ไม่งั้นการจองที่ถูกต้องจะถูกล้างทิ้งตอนจบ แล้วงานจะหายไปจากคิวเงียบ ๆ
        bool anyQueued = false;

        foreach (var machine in machines)
        {
            // ขอเฉพาะแถวของงานใบนี้ — กดเริ่มงานใบไหนต้องได้ใบนั้น ห้ามไปหยิบ
            // ใบอื่นที่บังเอิญรออยู่ในคิวเครื่องเดียวกันมาส่งแทน
            var (claim, claimError) = await _api.ClaimMachineAsync(machine, jobId);
            if (IsDisposed) return;

            if (claimError != null)
            {
                lines.Add(Notify.Bad($"{machine}: {claimError}"));
                continue;
            }

            if (claim?.Claimed == null)
            {
                // เครื่องไม่ว่าง — ไม่ใช่ความผิดพลาด แถวยังรออยู่ในคิวเหมือนเดิม
                anyQueued = true;
                lines.Add(Notify.Careful($"{machine}: เครื่องไม่ว่าง เข้าคิวรอไว้แล้ว"));
                continue;
            }

            var claimed = claim.Claimed;

            _sending = true;
            ShowSending($"กำลังส่งไปที่ {claimed.Machine}");
            StepSendResult sent;
            try
            {
                sent = await SendStepAsync(jobId, claimed.Machine, resolved, claimed.ProgramName);
            }
            finally
            {
                _sending = false;
                if (!IsDisposed) ShowSending(null);
            }

            if (IsDisposed) return;
            lines.AddRange(sent.Lines);

            // ตัดสินจาก "ข้อมูลเข้าเครื่องแล้วไหม" ไม่ใช่จากว่ามีคำเตือนติดมาไหม
            //
            // เดิมนับทุกบรรทัดที่ไม่ใช่สีเขียวเป็นส่งไม่ผ่าน ผลคืองาน marking 12 ที่ใช้
            // หัวพ่นตัวเดียว พอหัวอีกตัวปิดอยู่จนสั่งหยุดไม่ได้ (ขึ้นเป็นคำเตือน) แถวคิว
            // ของ MK จะถูกคืนเป็นรอคิวทั้งที่เครื่องรับงานไปแล้วและกำลังพิมพ์อยู่
            // เครื่องจึงดูเหมือนว่าง งานใบถัดไปเลยแย่งเข้าไปเปลี่ยนโปรแกรมทับได้
            if (sent.Sent)
            {
                // บันทึกไม่ลงต้องฟ้อง ไม่ใช่ปล่อยเงียบ — แถวจะค้างเป็น "ยังไม่ได้ส่ง"
                // แล้วรอบ poll จะส่งซ้ำเข้าเครื่องทุก 5 วินาทีโดยไม่มีใครรู้ว่าทำไม
                anySent = true;

                var (marked, markError) = await _api.UpdateMachineQueueAsync(claimed.Id, sent: true);
                if (!marked)
                    lines.Add(Notify.Bad($"{claimed.Machine}: ส่งเข้าเครื่องแล้วแต่บันทึกคิวไม่ได้ · {markError}"));
            }
            else
            {
                await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending");

                // ส่งเครื่องนี้ไม่ผ่าน = หยุดทั้งการกดครั้งนี้ ไม่ส่งเครื่องที่เหลือต่อ
                //
                // เครื่องเรียงตามลำดับที่ชิ้นงานเดินผ่าน เครื่องที่เหลือจึงเป็นเครื่องที่
                // อยู่หลังเครื่องที่เพิ่งพัง การโหลดโปรแกรมใส่เครื่องหลังไว้ทั้งที่เครื่อง
                // หน้ายังไม่ได้รับงาน ทำให้งานกลายเป็น "เริ่มไปแล้วครึ่งหนึ่ง" ซึ่งกด
                // เริ่มใหม่ไม่ได้ (สถานะเป็น Process) และไม่มีทางสั่งเครื่องหน้าซ้ำด้วย
                //
                // หยุดตรงนี้แทน ทุกอย่างจึงกลับไปเป็น Waiting และกดเริ่มใหม่ได้ทั้งใบ
                var skipped = machines
                    .SkipWhile(m => !string.Equals(m, machine, StringComparison.OrdinalIgnoreCase))
                    .Skip(1)
                    .ToList();

                if (skipped.Count > 0)
                    lines.Add(Notify.Careful(
                        $"ยังไม่ได้ส่ง {string.Join(" ", skipped)} — ต้องแก้ที่ {machine} ให้ได้ก่อน"));

                break;
            }
        }

        if (IsDisposed) return;

        // กดแล้วไม่มีเครื่องไหนรับงานไปได้เลย และงานนี้ก็ไม่เคยพิมพ์อะไรมาก่อน
        // ให้ล้างการจองทิ้ง ไม่เหลือร่องรอยไว้ในคิว
        //
        // เครื่องต่อไม่ติดไม่ใช่การเข้าคิว ไม่มีอะไรถูกส่งไปไหนทั้งนั้น การทิ้งแถวไว้
        // ทำให้งานไปกินที่ในคิวของเครื่องโดยไม่ได้ทำอะไร และหน้าจอก็ดูเหมือนกำลังรอคิว
        // ทั้งที่ความจริงต้องไปแก้ที่เครื่องแล้วกดใหม่
        //
        // งานที่พิมพ์ไปแล้วบางเครื่องไม่เข้าเงื่อนไขนี้ การจองของเครื่องที่เหลือต้องอยู่ต่อ
        // ไม่งั้นขั้นที่ยังไม่ได้ทำจะหายไปจากคิวโดยไม่มีทางเอากลับมา
        //
        // เครื่องไม่ว่างก็ไม่เข้าเงื่อนไขนี้เหมือนกัน นั่นคือการเข้าคิวที่ถูกต้อง งานต้อง
        // รออยู่ในคิวจนกว่าจะมีคนกดปุ่มหน้างานปล่อยเครื่อง ไม่ใช่โดนล้างทิ้ง
        if (!anySent && !anyQueued && !PrintedBefore(resolved))
        {
            await _api.ClearMachineQueueAsync(jobId);
            if (IsDisposed) return;
        }

        if (lines.Count > 0) Notify.Result(this, title, lines);
        if (!IsDisposed) await RefreshDataAsync(force: true);
    }

    /// <summary>
    /// ส่งขั้นตอนหนึ่งเข้าเครื่อง แล้วบันทึกลงประวัติถ้าสำเร็จ
    /// <para>
    /// เปลี่ยนสถานะเป็น Process ก่อนส่ง เพื่อให้แถวขึ้นสีและกันคนอื่นเริ่มงานซ้ำ
    /// ระหว่างที่เครื่องกำลังรับข้อมูลอยู่
    /// </para>
    /// <para>
    /// คืนรายการว่างเมื่อผู้ใช้กดยกเลิกที่กล่องเลือกรุ่นย่อย — ไม่ต้องรายงานอะไร
    /// </para>
    /// <para>
    /// <paramref name="forcedProgram"/> มีค่าเมื่อกำลังส่งแทน ST3 ซึ่งเลือกโปรแกรม
    /// ไว้ให้เสร็จแล้ว — การส่งรอบนั้นจะไม่เด้งหน้าต่างใด ๆ ที่จอ ST1
    /// </para>
    /// </summary>
    /// <summary>
    /// ผลการส่งหนึ่งขั้น — <paramref name="Sent"/> คือ "ข้อมูลเข้าเครื่องแล้วจริงไหม"
    ///
    /// <para>
    /// ต้องแยกจากระดับของบรรทัดที่รายงาน เพราะการส่งที่สำเร็จมีคำเตือนติดมาได้
    /// เช่นหัวพ่นอีกตัวที่งานนี้ไม่ได้ใช้และปิดอยู่ จะสั่งหยุดไม่ได้และขึ้นเป็นคำเตือน
    /// ทั้งที่หัวที่ต้องทำงานรับข้อมูลครบและกำลังพิมพ์อยู่
    /// </para>
    /// </summary>
    private sealed record StepSendResult(bool Sent, List<Notify.ResultLine> Lines);

    private async Task<StepSendResult> SendStepAsync(
        int jobId, string step, ResolvedJobResponse resolved,
        string? forcedProgram = null)
    {
        await _api!.UpdateJobStatusAsync(jobId, "Process");

        if (step == "MK")
        {
            // ตั้งค่าที่ PLC ตรงนี้ ไม่ใช่ตอนกดเริ่มงาน
            //
            // ตำแหน่งหัวพ่นกับความเร็วสายพานต้องพร้อมตอนชิ้นงานวิ่งผ่าน เครื่องที่ยัง
            // ไม่ว่างและได้แค่เข้าคิว ชิ้นงานยังไม่ไปไหน การเขียนค่าลง PLC ตั้งแต่ตอน
            // กดจึงเร็วเกินไป และไปทับค่าของงานที่เครื่องกำลังทำอยู่ด้วย
            //
            // อยู่ตรงนี้จึงได้ทุกทางเข้าเครื่อง ทั้งกดเริ่มงานตอนเครื่องว่าง กดปุ่มหน้างาน
            // ให้คิวเดินต่อ และคำขอจาก ST3
            var lines = new List<Notify.ResultLine>();
            await SendJobPlcAsync(resolved, lines);

            var mk = await JobSendService.SendMkAsync(resolved.Pattern);
            var mkLines = Notify.MkLines(mk.Machines);

            if (mkLines.Count == 0)
                mkLines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้"));

            lines.AddRange(mkLines);

            bool ok = mk.Status == SendStatus.Ok;

            if (ok) await _api.SaveSendStepAsync(jobId, "MK");
            else await UndoStartedStatusAsync(jobId, resolved);

            return new StepSendResult(ok, lines);
        }

        int uvNumber = step == "UV1" ? 1 : 2;
        var uv = await JobSendService.SendUvAsync(this, uvNumber, resolved.UvJobData, forcedProgram);

        if (uv.Status == SendStatus.Ok)
        {
            await _api.SaveSendStepAsync(jobId, step, new
            {
                requested = resolved.UvJobData.FirstOrDefault(r => r.Machine == step)?.ProgramName ?? "",
                program = uv.ProgramFile,
                is_default = uv.UsedDefault,
            });

            return new StepSendResult(true,
                [Notify.Ok($"{uv.MachineName} — ส่งสำเร็จ ({uv.ProgramFile}.uvdx)")]);
        }

        await UndoStartedStatusAsync(jobId, resolved);

        return new StepSendResult(false, uv.Status switch
        {
            // ยกเลิกที่กล่องเลือกรุ่นย่อย ไม่ใช่ความผิดพลาด ไม่ต้องขึ้นกล่องสรุป
            SendStatus.Cancelled => [],
            SendStatus.Unreachable =>
                [Notify.Bad($"{uv.MachineName} — เชื่อมต่อไม่ได้ ({uv.Ip}:{uv.Port})")],
            _ => [Notify.Bad($"{uv.MachineName} — {uv.FailReason}")],
        });
    }

    // ── งานที่ไม่ต้องส่งคำสั่ง (marking 00) ─────────────────

    /// <summary>
    /// เริ่มงานที่ไม่มีขั้นตอนต้องส่งเข้าเครื่อง — เปลี่ยนสถานะอย่างเดียว
    /// ไม่แตะทั้ง Inkjet และ UV เพราะงานแบบนี้ทำด้วยมือทั้งหมด
    /// </summary>
    private async Task StartWithoutSendingAsync(int jobId, string? markingMethod)
    {
        if (!Confirm.Ask(this, "ยืนยันเริ่มงาน",
                $"{JobName(jobId)} — marking {Method(markingMethod)}\n\n"
                + "งานนี้ไม่มีขั้นตอนต้องส่งเข้าเครื่อง จะเปลี่ยนสถานะเป็นกำลังผลิตอย่างเดียว\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        var (ok, err) = await _api!.UpdateJobStatusAsync(jobId, "Process");
        if (IsDisposed) return;

        if (ok) Notify.Success(this, $"เริ่มงาน {JobName(jobId)} แล้ว");
        else Notify.ErrorModal(this, "เริ่มงานไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้");

        await RefreshDataAsync(force: true);
    }

    // ── ST3 ฝากงานให้ ST1 ส่ง ───────────────────────────────

    /// <summary>
    /// ST3 กดเริ่มงาน: เลือกโปรแกรม UV ให้เสร็จตรงนี้ แล้วฝากคำขอไว้ที่ backend
    /// ให้ ST1 เป็นคนส่งเข้าเครื่อง (สาย MK/UV ต่ออยู่กับ PC ของ ST1 ที่เดียว)
    /// <para>
    /// ที่ต้องเลือกโปรแกรมตรงนี้ เพราะถ้าปล่อยให้ ST1 เลือก หน้าต่างเลือกรุ่นย่อย
    /// จะไปเด้งค้างที่จอ ST1 ซึ่งไม่มีคนเฝ้าอยู่ งานก็จะค้างไปเรื่อย ๆ
    /// </para>
    /// </summary>
    /// <param name="askFirst">
    /// false = มาจากปุ่มกดหน้างาน ข้ามกล่องยืนยัน เพราะคนกดยืนอยู่หน้าเครื่องแล้ว
    /// กล่องเลือกโปรแกรม UV กับกล่องยืนยันการใช้ default ยังเด้งอยู่ทั้งคู่
    /// นั่นคือการเลือก ไม่ใช่การยืนยัน และเลือกผิดหมายถึงพิมพ์ผิดแบบลงชิ้นงานจริง
    /// </param>
    private async Task RequestRemoteStartAsync(
        int jobId, string step, ResolvedJobResponse resolved, bool askFirst = true)
    {
        int machineStation = JobStationService.StationOf(step) ?? 0;
        if (StationOwner(machineStation, jobId) is { } busyJob)
        {
            Notify.WarnModal(this, "สถานีไม่ว่าง",
                $"ST{machineStation} มีงาน {JobLabel(busyJob)} อยู่\n\nต้องจบงานนั้นก่อนถึงจะเริ่มงานนี้ได้");
            return;
        }

        int uvNumber = step == "UV1" ? 1 : 2;
        var uvRow = resolved.UvJobData.FirstOrDefault(r => r.Machine == step);

        var pick = UvProgramResolver.Resolve(
            uvRow?.ProgramName, UvSettingsManager.GetDocumentFolder(uvNumber), this);

        if (pick.Program == null) return;   // ผู้ใช้ปิดกล่องเลือกรุ่นย่อย

        var uvName = UvSettingsManager.Read(
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

        if (pick.IsDefault &&
            !UvProgramResolver.ConfirmDefault(uvRow?.ProgramName ?? "", uvName, this))
            return;

        if (askFirst &&!Confirm.Ask(this, "ยืนยันเริ่มงาน",
                $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}\n\n"
                + $"ส่งไป {step} ด้วยโปรแกรม {pick.Program}.uvdx\n"
                + "คำสั่งจะถูกส่งเข้าเครื่องโดยโปรแกรมที่ ST1\n\n"
                + "ยืนยันหรือไม่?"))
            return;

        // ไม่ตั้งสถานะเป็น Working ตรงนี้โดยตั้งใจ — Working แปลว่า "ส่งเข้าเครื่องแล้ว"
        // แต่ตอนนี้ยังไม่มีใครแตะเครื่องเลย ยังไม่รู้ด้วยซ้ำว่า ST1 ต่อ UV ได้ไหม
        //
        // งานคงเป็น Waiting ไว้จนกว่า ST1 จะต่อเครื่องติดและส่งสำเร็จจริง
        // (SendStepAsync เป็นคนตั้ง Process และถอนคืนเองถ้าส่งไม่ผ่านและงานยังไม่เคยพิมพ์)
        var (ok, err) = await _api!.SetRemoteStartAsync(
            jobId, requested: true, pick.Program, step: step);
        if (IsDisposed) return;

        if (!ok)
        {
            await _api.UpdateJobStatusAsync(jobId, "Waiting");
            Notify.ErrorModal(this, "ส่งคำขอไม่สำเร็จ", err ?? "ไม่สามารถฝากคำขอไว้ที่ ST1 ได้");
            await RefreshDataAsync(force: true);
            return;
        }

        // รอผลจริงตรงนี้ ไม่ปล่อยให้ไปโผล่ทีหลังเบื้องหลัง — คนที่กดยืนอยู่หน้าจอนี้
        // ต้องได้คำตอบจากการกดของตัวเอง เหมือนกดที่ ST1 ทุกประการ
        //
        // ระหว่างรอ ตั้ง _sending ไว้ให้รอบ poll หยุด จะได้ไม่มีกล่องจากเบื้องหลัง
        // มาเด้งซ้อนเรื่องเดียวกัน
        _sending = true;
        ShowSending($"กำลังส่งไปที่ ST1 · {JobName(jobId)}");
        try
        {
            await ShowRemoteOutcomeAsync(jobId, step);
        }
        finally
        {
            _sending = false;
            ShowSending(null);
        }

        if (!IsDisposed) await RefreshDataAsync(force: true);
    }

    /// <summary>
    /// การ์ดวงกลมหมุนกลางตาราง บอกว่ากำลังส่งอยู่ — ส่ง null เพื่อปิด
    ///
    /// ปิดตารางไปด้วยระหว่างแสดง เป็นการกันกดซ้ำที่แน่นอนกว่าการหวังให้คนอ่านข้อความทัน
    /// (ข้อความลอยเล็กเกินกว่าจะทันเห็นตอนยืนห่างจากจอ) และทำให้เห็นชัดว่าเครื่องกำลังทำงาน
    /// ไม่ใช่ค้าง
    /// <para>
    /// ข้อความต้องเป็นบรรทัดเดียวเสมอ — AntdUI วาดข้อความของ Spin ด้วย NoWrapEllipsis
    /// (ไม่ตัดบรรทัด) และคำนวณขนาดวงกลมจากความสูงของข้อความ ใส่ขึ้นบรรทัดใหม่เข้าไป
    /// วงกลมจะใหญ่ขึ้นเท่าตัวจนล้นกรอบและโดนตัดหัวตัดท้าย
    /// </para>
    /// </summary>
    private void ShowSending(string? text)
    {
        bool busy = text != null;

        if (busy)
        {
            spinSending.Text = text;

            // จัดกึ่งกลางตอนแสดงทุกครั้ง — Anchor.None รักษาแค่สัดส่วนจากตำแหน่ง
            // ตอนออกแบบ พอจอจริงคนละขนาดการ์ดจะเยื้องไปจากกลาง
            var frame = pnlTableContainer.ClientSize;
            pnlSending.Location = new Point(
                Math.Max(0, (frame.Width - pnlSending.Width) / 2),
                Math.Max(0, (frame.Height - pnlSending.Height) / 2));

            pnlSending.Visible = true;
            pnlSending.BringToFront();
        }
        else
        {
            pnlSending.Visible = false;
        }

        tlpTableInner.Enabled = !busy;
    }

    /// <summary>
    /// นานสุดที่ ST3 ยอมรอผลจาก ST1 — ST1 หยิบคำขอทุกรอบ poll (5 วิ) บวกเวลาที่
    /// เครื่อง UV ใช้หยุด เขียน CPI โหลดโปรแกรม แล้วสั่งพิมพ์อีกหลายวินาที
    /// </summary>
    private static readonly TimeSpan RemoteOutcomeWait = TimeSpan.FromSeconds(40);

    /// <summary>
    /// รอจน ST1 ส่งเสร็จ แล้วแสดงผลแบบเดียวกับที่ ST1 แสดงให้คนที่ยืนตรงนั้นเห็น
    ///
    /// <para>
    /// ST3 ต่อสายไปหา UV ไม่ถึงก็จริง แต่ไม่จำเป็นต้องต่อ — ผลที่ ST1 วัดได้ถูกฝากไว้ที่
    /// backend อยู่แล้วทั้งสองทาง: สำเร็จเห็นเป็นแถวใน print_job_commands · ไม่สำเร็จ
    /// เห็นเป็นข้อความใน remote_error หน้าที่ตรงนี้คือเฝ้าช่องนั้นจนกว่าจะมีคำตอบ
    /// </para>
    /// <para>
    /// ล้าง remote_error ก่อนแสดงเสมอ เพื่อไม่ให้ตัวเฝ้าเบื้องหลังหยิบใบเดิมมาเด้งซ้ำ
    /// </para>
    /// </summary>
    private async Task ShowRemoteOutcomeAsync(int jobId, string step)
    {
        var deadline = DateTime.UtcNow + RemoteOutcomeWait;

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(700);
            if (IsDisposed) return;

            var job = await _api!.GetJobByIdAsync(jobId);
            if (IsDisposed) return;
            if (job == null) continue;

            var failure = job.RemoteError?.Trim();
            if (!string.IsNullOrEmpty(failure))
            {
                await _api.SetRemoteStartAsync(jobId, requested: false);
                if (IsDisposed) return;

                Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Bad(failure)]);
                return;
            }

            bool sent = job.Commands?.Any(c => c.Success &&
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;
            if (!sent) continue;

            var uvName = UvSettingsManager.Read(step == "UV1" ? "UV1_NAME" : "UV2_NAME", step);
            Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Ok($"{uvName} — ส่งสำเร็จ")]);
            return;
        }

        // หมดเวลาแล้วยังเงียบ — ต้องแยกให้ออกว่า "ไม่มีใครหยิบไปทำเลย" กับ
        // "ST1 หยิบไปแล้วแต่ยังส่งไม่เสร็จ" เพราะสองอย่างนี้ต้องจัดการคนละแบบ
        var final = await _api!.GetJobByIdAsync(jobId);
        if (IsDisposed) return;

        if (final?.RemoteStart == RemoteSending)
        {
            Notify.WarnModal(this, "ST1 กำลังส่งอยู่",
                $"{JobName(jobId)}\n\n"
                + "ST1 รับคำขอไปแล้วและกำลังส่งเข้าเครื่อง แต่ใช้เวลานานกว่าปกติ\n\n"
                + "งานยังเดินอยู่ ไม่ต้องกดซ้ำ — รอผลอีกสักครู่");
            return;
        }

        // ไม่มีใครหยิบเลย = โปรแกรมที่ ST1 ไม่ได้เปิด หรือต่อ Backend ไม่ได้
        // ต้องตีงานกลับเป็น Waiting ไม่งั้นค้างเป็น Working ตลอดกาลทั้งที่ยังไม่ได้พิมพ์
        // ปิดโปรแกรมเปิดใหม่ก็ยังเห็นเป็น Working และกดเริ่มใหม่ไม่ได้
        await _api.SetRemoteStartAsync(jobId, requested: false);
        await _api.UpdateJobStatusAsync(jobId, "Waiting");
        if (IsDisposed) return;

        Notify.WarnModal(this, "ST1 ไม่รับคำขอ",
            $"{JobName(jobId)}\n\n"
            + $"รอมา {RemoteOutcomeWait.TotalSeconds:0} วินาทีแล้วยังไม่มีใครรับไปส่ง\n"
            + "งานถูกตีกลับเป็นรอเริ่มแล้ว ยังไม่มีอะไรถูกส่งเข้าเครื่อง\n\n"
            + "ตรวจว่าโปรแกรมที่เครื่อง ST1 เปิดอยู่และต่อ Backend ได้ แล้วกดเริ่มงานใหม่");
    }

    // ── ST1 หยิบคำขอของ ST3 ไปส่ง ───────────────────────────

    /// <summary>ST3 ฝากไว้ ยังไม่มีใครหยิบ — ตีกลับเป็น Waiting ได้</summary>
    private const string RemotePending = "1";

    /// <summary>ST1 หยิบไปแล้วกำลังส่งเข้าเครื่อง — ห้ามแตะ งานกำลังเดินอยู่จริง</summary>
    private const string RemoteSending = "2";

    /// <summary>
    /// งานที่กำลังส่งแทน ST3 อยู่ — กันไม่ให้รอบ poll ถัดไปหยิบงานเดิมไปส่งซ้ำ
    /// ระหว่างที่รอบนี้ยังส่งไม่เสร็จ (รอบ poll ทุก 5 วิ แต่การส่ง UV ใช้เวลานานกว่านั้นได้)
    /// </summary>
    private readonly HashSet<int> _remoteInFlight = new();

    /// <summary>
    /// เก็บกวาดคำขอที่ค้างสถานะ "กำลังส่ง" ทั้งที่ ST1 ตัวนี้ไม่ได้ส่งอยู่
    ///
    /// เกิดตอนโปรแกรม ST1 ถูกปิดหรือดับกลางคันระหว่างส่ง — ธงค้างเป็น "2" ตลอดกาล
    /// ไม่มีใครหยิบไปทำต่อ (ตัวหยิบมองเฉพาะ "1") และ ST3 ก็จะเห็นว่า "กำลังส่งอยู่"
    /// ทั้งที่ไม่มีใครส่ง คืนงานกลับเป็นรอเริ่มเพื่อให้กดใหม่ได้
    /// </summary>
    private async Task RecoverAbandonedRemoteStartsAsync()
    {
        if (_api == null || StationService.IsSt3) return;

        var abandoned = _allJobs
            .Where(j => j.RemoteStart == RemoteSending && !_remoteInFlight.Contains(j.Id))
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in abandoned)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false,
                failure: "ST1 ปิดกลางคันระหว่างส่ง — งานถูกคืนเป็นรอเริ่ม");
            await _api.UpdateJobStatusAsync(jobId, "Waiting");

            if (IsDisposed) return;
        }
    }

    /// <summary>
    /// ST1 กวาดหาคำขอที่ ST3 ฝากไว้ แล้วส่งเข้าเครื่องให้ — ทำเงียบ ๆ ไม่มีหน้าต่างเด้ง
    /// เพราะโปรแกรมที่ ST3 เลือกไว้แล้วถูกส่งมากับคำขอ
    /// </summary>
    /// <summary>
    /// ST1 ไล่หยิบงานที่รออยู่ในคิวไปส่งเข้าเครื่อง — เรียกทุกรอบ poll
    ///
    /// <para>
    /// เครื่องต่อสายอยู่กับ PC ของ ST1 ที่เดียว การส่งจริงจึงเกิดที่นี่เสมอ
    /// ไม่ว่าคนที่กดเริ่มงานจะยืนอยู่สถานีไหน งานที่ ST3 จองไว้ก็มาถึงทางนี้
    /// และงานที่รอเครื่องว่างอยู่ก็ถูกหยิบต่อทันทีที่มีคนกดปุ่มปล่อยเครื่อง
    /// </para>
    /// <para>
    /// ถามทีละเครื่อง เครื่องไหนไม่ว่าง backend จะไม่ให้หยิบเอง ฝั่งนี้ไม่ต้องเดา
    /// </para>
    /// </summary>
    private async Task ProcessMachineQueueAsync()
    {
        // ST3 เป็นฝ่ายจอง ไม่ใช่ฝ่ายส่ง · ระหว่างที่คนกดส่งเองอยู่ก็ไม่แทรก
        if (_api == null || StationService.IsSt3 || _sending) return;

        var (rows, error) = await _api.GetMachineQueueAsync();
        if (error != null || IsDisposed) return;

        // ส่งเฉพาะแถวที่ "ถึงคิวแล้วแต่ยังไม่ได้ส่ง" เท่านั้น และไม่หยิบคิวเองเด็ดขาด
        //
        // แถวจะมาอยู่ในสภาพนี้ได้จากการกดของคนเท่านั้น — กดเริ่มงาน หรือกดปุ่ม
        // หน้างานปล่อยเครื่องแล้ว backend ยกเครื่องให้คิวถัดไป รอบนี้จึงเป็นแค่
        // "มือที่ไปส่งแทน ST3" ไม่ใช่ตัวตัดสินว่างานไหนได้เข้าเครื่อง
        //
        // เดิมตรงนี้ไล่หยิบคิวเองทุก 5 วิ ผลคืองานที่ส่งไม่ผ่านแล้วถูกคืนเป็นรอคิว
        // จะถูกหยิบมายิงใหม่ไม่มีวันจบ และงานใบอื่นที่ไม่มีใครกดก็ถูกส่งออกไปด้วย
        var ready = rows
            .Where(r => r.State == "active" && r.SentAt == null)
            .OrderBy(r => r.Id)
            .ToList();

        foreach (var row in ready)
        {
            await SendClaimedAsync(row);
            if (IsDisposed) return;
        }
    }

    /// <summary>
    /// ส่งงานที่หยิบมาได้เข้าเครื่อง แล้วรายงานผล — ส่งไม่ผ่านคืนแถวกลับเข้าคิว
    /// </summary>
    private async Task SendClaimedAsync(MachineQueueRow claimed)
    {
        var resolved = await _api!.GetResolvedJobAsync(claimed.PrintJobsId);
        if (resolved == null || IsDisposed)
        {
            // อ่านงานไม่ได้ อย่าถือเครื่องค้างไว้
            if (_api != null) await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending");
            return;
        }

        _sending = true;
        ShowSending($"กำลังส่งไปที่ {claimed.Machine} · {JobName(claimed.PrintJobsId)}");
        StepSendResult sent;
        try
        {
            sent = await SendStepAsync(
                claimed.PrintJobsId, claimed.Machine, resolved, claimed.ProgramName);
        }
        finally
        {
            _sending = false;
            if (!IsDisposed) ShowSending(null);
        }

        if (IsDisposed) return;

        // ส่งไม่ผ่าน = คืนแถวกลับไปรอคิว แล้วจบตรงนั้น ไม่มีใครมาลองใหม่ให้เอง
        // ต้องมีคนกดเริ่มงานใบนั้นอีกครั้ง ถึงจะยิงซ้ำ
        if (sent.Sent)
        {
            // เหมือนกับตอนกดเริ่มงาน — บันทึกไม่ลงแปลว่ารอบหน้าจะส่งซ้ำ ต้องให้เห็น
            var (marked, markError) = await _api.UpdateMachineQueueAsync(claimed.Id, sent: true);
            if (!marked)
                sent.Lines.Add(Notify.Bad($"{claimed.Machine}: ส่งเข้าเครื่องแล้วแต่บันทึกคิวไม่ได้ · {markError}"));
        }
        else
        {
            await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending");
        }

        if (sent.Lines.Count > 0)
            Notify.Result(this, $"ส่ง {claimed.Machine} · {JobName(claimed.PrintJobsId)}", sent.Lines);
    }

    private async Task ProcessRemoteStartsAsync()
    {
        // ST3 เป็นฝ่ายฝาก ไม่ใช่ฝ่ายส่ง · ระหว่างที่คนที่ ST1 กดส่งเองอยู่ก็ไม่แทรก
        if (_api == null || StationService.IsSt3 || _sending) return;

        var pending = _allJobs
            .Where(j => j.RemoteStart == RemotePending && !_remoteInFlight.Contains(j.Id))
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in pending)
        {
            _remoteInFlight.Add(jobId);
            _sending = true;
            try
            {
                await RunRemoteStartAsync(jobId);
            }
            finally
            {
                _sending = false;
                _remoteInFlight.Remove(jobId);
            }

            if (IsDisposed) return;
        }
    }

    /// <summary>
    /// ส่งคำขอหนึ่งใบ — ล้างธงทุกเส้นทางที่ออกจากเมธอดนี้ ไม่ว่าจะส่งได้หรือไม่
    /// ธงที่ค้างคือสาเหตุเดียวที่จะทำให้รอบถัดไปส่งซ้ำ
    /// </summary>
    private async Task RunRemoteStartAsync(int jobId)
    {
        var resolved = await _api!.GetResolvedJobAsync(jobId);
        if (resolved == null) return;   // อ่านไม่ได้ = คงธงไว้ให้รอบหน้าลองใหม่

        // งานต้องยังไม่ถูกยกเลิกหรือจบไปแล้ว ถ้าไม่ดักตรงนี้ ธงที่ค้างจะสั่งพิมพ์
        // งานที่ถูกยกเลิกไปแล้ว
        //
        // รับทั้ง Waiting และ Process — คำขอจาก ST3 มาถึงตอนงานยังเป็น Waiting
        // เพราะ Working จะถูกตั้งก็ต่อเมื่อต่อเครื่องติดและส่งจริงแล้วเท่านั้น
        var status = resolved.Job.Status;
        bool live = string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase);

        if (!live)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false);
            return;
        }

        var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod);

        // ขั้นตอนที่จะส่งมาจากตัวคำขอ ไม่ใช่การเดาเอาว่าเป็นขั้นแรกเสมอ
        //
        // งานหนึ่งใบมีได้หลายขั้น เช่น marking 32 คือ MK แล้วต่อ UV2 ถ้าหยิบขั้นแรก
        // ตายตัวแบบเดิม คำขอที่ ST3 ฝากไว้ว่าขอ UV2 จะกลายเป็นส่ง MK ซ้ำ
        //
        // คำขอที่ไม่ได้ระบุขั้นถือว่าเป็นขั้นแรก — ใบที่ค้างอยู่ตอนอัปเดตโปรแกรม
        // จึงยังทำงานถูกเหมือนเดิม
        var requested = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteStep;
        var step = string.IsNullOrWhiteSpace(requested)
            ? plan.Steps.FirstOrDefault()
            : plan.Steps.FirstOrDefault(x =>
                string.Equals(x, requested.Trim(), StringComparison.OrdinalIgnoreCase));

        // ขั้นที่ขอมาไม่มีอยู่ในแผนของงานนี้ = คำขอใช้ไม่ได้ ทิ้งไปพร้อมบอกสาเหตุ
        if (step == null)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false,
                failure: string.IsNullOrWhiteSpace(requested)
                    ? null
                    : $"งานนี้ไม่มีขั้นตอน {requested.Trim()} ให้ส่ง");
            return;
        }

        // อ่านสดจาก backend แล้วเช็คว่าขั้นตอนนี้ส่งสำเร็จไปแล้วหรือยัง —
        // ด่านสุดท้ายที่กันการส่งซ้ำ ถ้าธงค้างเพราะเหตุอื่น เช่นโปรแกรมถูกปิดกลางคัน
        if (resolved.Commands?.Any(c => c.Success &&
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true)
        {
            await _api.SetRemoteStartAsync(jobId, requested: false);
            return;
        }

        // เครื่องปลายทางไม่ว่าง — คงธงไว้ให้รอบ poll ถัดไปลองใหม่ ไม่ต้องรบกวนใคร
        int machineStation = JobStationService.StationOf(step) ?? 0;
        if (StationOwner(machineStation, jobId) != null) return;

        var program = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteProgram;

        // จองไว้ก่อนลงมือ — ตั้งแต่บรรทัดนี้ไป ST3 จะเห็นว่า "กำลังส่งอยู่" ไม่ใช่
        // "ไม่มีใครรับ" จึงไม่ตีงานกลับเป็น Waiting ทับงานที่เครื่องกำลังรับข้อมูล
        await _api.ClaimRemoteStartAsync(jobId, program, step);
        if (IsDisposed) return;

        var lines = (await SendStepAsync(jobId, step, resolved, program)).Lines;

        // ล้มเหลวแล้วฝากสาเหตุกลับไปให้ ST3 ด้วย — คนที่กดเริ่มงานอยู่ที่นั่น
        // ไม่ได้เห็นจอนี้ ถ้าไม่ฝากไว้เขาจะเห็นแค่งานเด้งกลับเป็น Waiting เฉย ๆ
        bool failed = lines.Any(l => l.Kind == Notify.ResultKind.Error);
        var failure = failed
            ? string.Join(" · ", lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text))
            : null;

        await _api.SetRemoteStartAsync(jobId, requested: false, failure: failure);

        if (IsDisposed || lines.Count == 0) return;

        // ต้องเป็นข้อความลอย ไม่ใช่กล่องที่ต้องกดปิด — จอ ST1 ไม่มีคนเฝ้าอยู่
        // กล่อง modal จะค้างหน้าจอและหยุดรอบ poll ไปจนกว่าจะมีคนมากด
        var text = $"{JobName(jobId)} — {lines[0].Text} (คำขอจาก ST3)";

        if (failed) Notify.Warn(this, text);
        else Notify.Success(this, text);
    }

    // ── ST3 รับผลกลับจาก ST1 ────────────────────────────────

    /// <summary>กล่องแจ้งผลเปิดค้างอยู่ — กันไม่ให้รอบ poll ถัดไปเปิดซ้อนขึ้นมาอีกใบ</summary>
    private bool _showingRemoteError;

    /// <summary>
    /// ST3 หยิบสาเหตุที่ ST1 ส่งไม่สำเร็จมาแสดงที่จอตัวเอง แล้วล้างทิ้งทันที
    ///
    /// ล้างทันทีที่แสดง จึงไม่ต้องจำว่าเคยแสดงใบไหนไปแล้ว และไม่เด้งซ้ำตอนเปิดโปรแกรมใหม่
    /// งานที่ยังไม่เคยพิมพ์อะไรเลยถูกถอนสถานะกลับเป็น Waiting ไว้แล้ว กดเริ่มใหม่ได้เลย
    /// </summary>
    private async Task ShowRemoteErrorsAsync()
    {
        if (_api == null || !StationService.IsSt3 || _showingRemoteError) return;

        var failed = _allJobs.FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.RemoteError));
        if (failed == null) return;

        var message = failed.RemoteError!;

        // ล้างก่อนเปิดกล่อง — ระหว่างกล่องเปิดค้าง รอบ poll ยังเดินอยู่หลังกล่อง
        await _api.SetRemoteStartAsync(failed.Id, requested: false);
        if (IsDisposed) return;

        _showingRemoteError = true;
        try
        {
            Notify.ErrorModal(this, "ST1 ส่งงานไม่สำเร็จ",
                $"{JobLabel(failed)}\n\n{message}\n\n"
                + "งานถูกตีกลับเป็นรอเริ่ม กดเริ่มงานใหม่ได้");
        }
        finally
        {
            _showingRemoteError = false;
        }
    }

    /// <summary>งานที่จองสถานีนี้อยู่ — null = ว่าง</summary>
    private PrintJob? StationOwner(int station, int exceptJobId)
    {
        if (station == 0) return null;

        foreach (var job in _allJobs)
        {
            if (job.Id == exceptJobId) continue;
            if (!string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)) continue;
            if (JobStationService.Current(job.Commands) == station) return job;
        }

        return null;
    }

    private async Task CompleteJobAsync(int jobId)
    {
        if (_api == null) return;

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}");
        if (IsDisposed) return;
        if (resolved == null)
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้");
            return;
        }

        var method = resolved.PlanRouting?.MarkingMethod;
        if (!MarkingMethodService.CanCompleteAt(StationService.Current, method))
        {
            Notify.WarnModal(this, "จบงานที่สถานีนี้ไม่ได้",
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + "งาน marking 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น");
            return;
        }

        var steps = CheckSteps(method, resolved.Commands);

        // งานยังไม่ครบก็จบได้ ถ้าผู้ใช้ยืนยันเอง — บันทึกไว้ว่าเป็นการจบด้วยมือ
        bool manual = !steps.Complete;
        if (manual)
        {
            var list = string.Join(", ", steps.Missing);
            if (!Confirm.Ask(this, "งานยังส่งไม่ครบ",
                    $"{JobName(jobId)} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\n" +
                    "ยืนยันจบงานทั้งที่ยังส่งไม่ครบหรือไม่?"))
                return;
        }
        else if (!Confirm.Ask(this, "ยืนยันจบงาน",
                     $"จบงาน {JobName(jobId)}\n\nยืนยันหรือไม่?"))
        {
            return;
        }

        if (manual)
            await _api.SaveSendStepAsync(jobId, "MANUAL_COMPLETE");

        // จบงานแล้วคิวที่เหลือของงานนี้ไม่มีความหมาย ล้างทิ้งไม่ให้ไปกันเครื่องคนอื่น
        await _api.ClearMachineQueueAsync(jobId);

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success");
        if (ok)
        {
            Notify.Success(this, manual
                ? $"{JobName(jobId)} จบงานแล้ว (ยืนยันด้วยมือ)"
                : $"{JobName(jobId)} จบงานแล้ว");
            await RefreshDataAsync();
        }
        else
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้");
        }
    }

    // ── ตรวจความครบของงาน ──────────────────────────────────

    /// <summary>ผลตรวจว่างานส่งครบทุกขั้นตอนแล้วหรือยัง</summary>
    private readonly record struct StepStatus(bool Complete, List<string> Missing);

    /// <summary>
    /// ใช้ร่วมกันทั้งตอนระบายสีปุ่มและตอนกดจบงาน เพื่อไม่ให้สองที่ตัดสินคนละแบบ
    ///
    /// รหัส "22" เคยถูกดักเป็นกรณีพิเศษว่าต้องส่ง MK สำเร็จ 2 ครั้งถึงจะครบ
    /// ตอนนี้เลิกแล้ว ส่ง MK ครั้งเดียวก็จบงานได้ เหมือนรหัสอื่นที่ใช้ MK อย่างเดียว
    /// </summary>
    private static StepStatus CheckSteps(string? markingMethod, List<CommandResult>? commands)
    {
        var need = MarkingMethodService.MissingSteps(markingMethod, commands);
        return new StepStatus(need.Count == 0, need);
    }

    /// <summary>
    /// พื้นหลังของแถว — งานที่กำลังเดินอยู่ (Working) ระบายเขียวอ่อนให้สะดุดตา
    /// จากระยะไกล
    ///
    /// คืนเฉพาะ BackColor ไม่แตะ ForeColor เพราะถ้าใส่ ForeColor มา AntdUI จะเอา
    /// สีนั้นทาทับทุกเซลล์ในแถว แล้วสีของคอลัมน์ Status ที่บอกสถานะด้วยสีจะหายไป
    /// </summary>
    private static AntdUI.Table.CellStyleInfo? TblOrders_SetRowStyle(
        object sender, AntdUI.TableSetRowStyleEventArgs e)
    {
        if (e.Record is not OrderRow row || row.Back is not Color back) return null;
        return new AntdUI.Table.CellStyleInfo { BackColor = back };
    }

    /// <summary>
    /// กฎอยู่ที่ <see cref="MarkingMethodService"/> ที่เดียว หน้า Order Detail ใช้ตัวเดียวกัน
    /// เดิมที่นี่ไม่รู้จักรหัส 21 ทำให้งาน 21 ถูกมองว่ายังต้องส่ง MK ทั้งที่กดส่งไม่ได้
    /// </summary>
    private static List<string> GetRequiredSteps(string markingMethod) =>
        MarkingMethodService.Resolve(markingMethod).Steps;

    /// <summary>
    /// เปิดกล่อง Order Detail แล้วทำตามสิ่งที่คนในกล่องขอไว้ตอนปิด
    ///
    /// ตอนนี้มีอย่างเดียวคือปุ่มสำรอง "ขอให้ ST1 ส่ง" ซึ่งเป็นทางออกตอนปุ่มกดหน้างาน
    /// ใช้ไม่ได้ ตัวคำขอยิงจากที่นี่ ไม่ใช่จากในกล่อง เพราะด่านตรวจทั้งหมดอยู่ที่นี่
    /// </summary>
    private async Task ShowDetailDialogAsync(ResolvedJobResponse resolved)
    {
        string? requestedStep;

        using (var dlg = new OrderDetailDialog())
        {
            // ชื่อเดียวกับหัวที่อยู่ในหน้า ไม่ประกอบเอง ไม่งั้นสองที่จะขึ้นคนละเลข
            dlg.TitleText = $"{OrderDetailUserControl.JobTitle(resolved.Job)} — Order Detail";
            dlg.Text = dlg.TitleText;
            dlg.LoadDetail(resolved, _api);
            dlg.ShowDialog(this);
            requestedStep = dlg.RemoteStartStep;
        }

        if (requestedStep == null || _api == null || IsDisposed) return;

        await RequestRemoteStartFromDetailAsync(resolved.Job.Id);
    }

    /// <summary>
    /// ทำต่อจากปุ่มสำรองในหน้า Order Detail — เดินทางเดียวกับปุ่มกดหน้างานทุกประการ
    ///
    /// <para>
    /// อ่านงานสดใหม่ก่อนเสมอ เพราะกล่อง Order Detail เปิดค้างได้นาน ระหว่างนั้น
    /// ปุ่มกดหน้างานหรืออีกสถานีอาจส่งขั้นนั้นไปแล้ว ถ้าเชื่อค่าที่อ่านไว้ตอนเปิดกล่อง
    /// จะกลายเป็นส่งซ้ำลงชิ้นงานจริง
    /// </para>
    /// </summary>
    private async Task RequestRemoteStartFromDetailAsync(int jobId)
    {
        var resolved = await LoadJobAsync(jobId, $"กำลังตรวจสอบงาน · {JobName(jobId)}");
        if (resolved == null || IsDisposed) return;

        var steps = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps;
        int next = steps.FindIndex(step => !SentAlready(resolved, step));

        // -1 = ส่งครบแล้ว · 0 = ยังไม่ได้เริ่มเลย ซึ่งเป็นหน้าที่ของปุ่มเริ่มงาน ไม่ใช่ปุ่มนี้
        if (next <= 0)
        {
            Notify.WarnModal(this, "ไม่มีขั้นที่ต้องส่ง",
                $"{JobName(jobId)} ไม่มีขั้นถัดไปที่รอ ST1 ส่งแล้ว\n\n"
                + "อาจมีคนกดปุ่มหน้างานไปก่อนหน้านี้");
            return;
        }

        await RequestRemoteStartAsync(jobId, steps[next], resolved, askFirst: true);
    }


    // ── เวลา ───────────────────────────────────────────────

    // การแปลงเวลาไทยทั้งหมดอยู่ที่ ThaiTime — หน้านี้กับหัวหน้า Order Detail
    // ต้องบอกวันเดียวกันของงานเดียวกัน จึงต้องใช้ตัวแปลงตัวเดียวกัน
    private const string TimeFormat = ThaiTime.Format;

    private const string Dash = "-";

    /// <summary>เวลาไทย → UTC สำหรับส่งเป็นเงื่อนไขให้ backend</summary>
    private static DateTime ToUtcFromThai(DateTime thai) => ThaiTime.ToUtc(thai);

    private static string FormatThaiTime(DateTime? utc) => ThaiTime.Text(utc, TimeFormat, Dash);

    /// <summary>
    /// ตัวเทียบของ AntdUI ได้มาแค่ข้อความในเซลล์ ถ้าทั้งคู่เป็นเวลาก็เทียบเป็นเวลา
    /// นอกนั้นส่งต่อให้การเทียบแบบรู้ตัวเลข (เพื่อให้ Qty 9 มาก่อน 10)
    /// </summary>
    private static int CompareCellText(string x, string y)
    {
        if (TryParseCellTime(x, out var tx) && TryParseCellTime(y, out var ty))
            return tx.CompareTo(ty);

        return CompareNatural(x, y);
    }

    private static bool TryParseCellTime(string? text, out DateTime value) =>
        DateTime.TryParseExact(
            (text ?? "").Trim(), TimeFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out value);

    /// <summary>เทียบข้อความโดยอ่านกลุ่มตัวเลขเป็นจำนวน ไม่ใช่ทีละตัวอักษร</summary>
    private static int CompareNatural(string? a, string? b)
    {
        string x = a ?? "", y = b ?? "";
        int i = 0, j = 0;

        while (i < x.Length && j < y.Length)
        {
            if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
            {
                int si = i, sj = j;
                while (i < x.Length && char.IsDigit(x[i])) i++;
                while (j < y.Length && char.IsDigit(y[j])) j++;

                var nx = x.Substring(si, i - si).TrimStart('0');
                var ny = y.Substring(sj, j - sj).TrimStart('0');

                if (nx.Length != ny.Length) return nx.Length - ny.Length;
                int digits = string.CompareOrdinal(nx, ny);
                if (digits != 0) return digits;
            }
            else
            {
                int ch = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (ch != 0) return ch;
                i++;
                j++;
            }
        }

        return (x.Length - i) - (y.Length - j);
    }

    /// <summary>
    /// เลข marking_method ดิบ เช่น "12" "02" "22" — ไม่แปลเป็นชื่อเครื่อง
    /// เพราะหน้านี้ใช้ไล่เทียบกับใบสั่งงาน ซึ่งเขียนเป็นตัวเลขเหมือนกัน
    /// ความหมายของแต่ละหลักดูได้ที่ Order Detail บรรทัด Plate / Shim
    /// </summary>
    private static string Method(string? markingMethod)
    {
        var value = (markingMethod ?? "").Trim();
        return value.Length == 0 ? Dash : value;
    }

    /// <summary>
    /// เครื่องที่มาร์กด้านนั้น ในตารางใช้ขีดแทน "None" ให้เข้าชุดกับคอลัมน์ End
    /// ที่ใช้ขีดแทนช่องว่างอยู่แล้ว
    /// </summary>
    private static string MachineCell(MarkingMachine machine) =>
        machine == MarkingMachine.None ? Dash : MarkingMethodService.Label(machine);

    /// <summary>
    /// สถานีล่าสุดที่กดส่งสำเร็จ — ยังไม่เคยส่งเป็นขีด เข้าชุดกับคอลัมน์ End
    /// งานที่จบแล้วยังค้างอยู่ที่สถานีสุดท้ายของมัน ไม่ได้ล้างทิ้ง
    /// </summary>
    private static string OrDashStation(int? station)
    {
        var label = JobStationService.Label(station);
        return label.Length == 0 ? Dash : label;
    }

    private OrderRow ToRow(PrintJob job, bool isHistory)
    {
        var plan = MarkingMethodService.Resolve(job.PlanRouting?.MarkingMethod);

        // งานที่กดจบทั้งที่ยังส่งไม่ครบต้องดูออกจากในตาราง ไม่ต้องเปิดเข้าไปดูทีละงาน
        var (statusLabel, statusColor) = JobStatusDisplay.Resolve(
            job.Status,
            MarkingMethodService.FinishedIncomplete(
                job.Status, job.PlanRouting?.MarkingMethod, job.Commands));
        // ไม่ต่อท้ายอะไรตรงนี้ — ตารางต้องอ่านจากที่ไกล ๆ จึงเหลือคำสถานะคำเดียวล้วน ๆ
        //
        // รายละเอียดว่างานรออะไรอยู่ (คิวที่เท่าไร หรือกำลังทำด้านไหน) ไปโชว์ที่หน้า
        // Order Detail ซึ่งเปิดดูทีละงานอยู่แล้ว ดู JobStageService

        var statusText = new AntdUI.CellText(statusLabel) { Fore = statusColor };

        var buttons = new List<AntdUI.CellButton>();
        if (!isHistory)
        {
            // คอลัมน์ปุ่มยัดปุ่มข้อความสองอันไม่ลง จึงสลับปุ่มแรกตามสถานะแทน
            // งานที่ยังไม่เริ่ม = เริ่มงาน · งานที่เดินอยู่ = จบงาน
            if (CanStart(job))
            {
                buttons.Add(new AntdUI.CellButton("start", "เริ่มงาน", AntdUI.TTypeMini.Primary)
                { Radius = 6 });
            }
            else if (NotMyTurnYet(job))
            {
                // ยังไม่ถึงตาสถานีนี้ — ไม่ใส่ปุ่มข้อความ เหลือแค่ยกเลิกกับดูรายละเอียด
            }
            else if (MarkingMethodService.CanCompleteAt(
                         StationService.Current, job.PlanRouting?.MarkingMethod))
            {
                // เขียว = ส่งครบแล้วจบได้เลย · ส้ม = ยังไม่ครบ กดได้แต่จะเตือนก่อน
                // commands / plan_routing มาจาก /job/getAll ที่ include ไว้ให้แล้ว
                var steps = CheckSteps(job.PlanRouting?.MarkingMethod, job.Commands);
                buttons.Add(new AntdUI.CellButton("complete", "จบงาน",
                    steps.Complete ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Warn)
                { Radius = 6 });
            }

            // ยกเลิกได้ทุกงานที่ยังไม่จบ ไม่ว่าเริ่มไปแล้วหรือยัง — เป็นทางออกเดียว
            // ของงานที่ยกเลิกหน้างานแล้วไม่ควรค้างอยู่ในตารางให้คนอื่นสับสน
            buttons.Add(new AntdUI.CellButton("cancel", "", AntdUI.TTypeMini.Error)
            { Radius = 6, IconSvg = "CloseOutlined" });
        }
        else if (IsCancelled(job))
        {
            // งานที่ยกเลิกไปแล้วยังเอากลับมาพิมพ์ใหม่ได้ ต่างจากงานที่จบไปแล้วจริง ๆ
            // ซึ่งไม่มีปุ่มนี้ — เพราะการยกเลิกคือ "ไม่ได้ทำ" ไม่ใช่ "ทำเสร็จแล้ว"
            buttons.Add(new AntdUI.CellButton("restore", "พิมพ์ใหม่", AntdUI.TTypeMini.Primary)
            { Radius = 6 });
        }
        buttons.Add(new AntdUI.CellButton("detail", "", AntdUI.TTypeMini.Default) { Radius = 6, IconSvg = "SearchOutlined" });

        // End มีความหมายเฉพาะงานที่จบแล้ว — งานที่ยังวิ่งอยู่ updated_at คือเวลาแก้ล่าสุด ไม่ใช่เวลาจบ
        // งานที่ถูกยกเลิกก็นับว่าจบ updated_at ของมันคือเวลาที่กดยกเลิก
        bool finished =
            string.Equals(job.Status, "Success", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(job.Status, "Cancel", StringComparison.OrdinalIgnoreCase);

        return new OrderRow
        {
            Id = job.Id,
            Start = FormatThaiTime(job.CreatedAt),
            End = finished ? FormatThaiTime(job.UpdatedAt) : Dash,
            ErpMfg = job.OrderNo ?? "",
            // barcode ที่สแกนเข้ามา = เลขล็อต — backend เก็บซ้ำไว้ใน lot_number ด้วย
            // ใช้ตัวที่มีค่าจริง เผื่องานเก่าที่กรอกมาคนละทาง
            LotNo = FirstFilled(job.LotNumber, job.BarcodeRaw),
            Qty = job.Qty?.ToString() ?? "",
            // ค่าดิบจาก plan_routing.process_sequence เช่น "online" / "offline"
            // โชว์ตามที่ database ส่งมาตรง ๆ ไม่แปลง ไม่ normalize ตัวพิมพ์
            ProcessSequence = job.PlanRouting?.ProcessSequence ?? "",
            Plate = plan.NoCase ? Dash : MachineCell(plan.Plate),
            Shim = plan.NoCase ? Dash : MachineCell(plan.Shim),
            Station = OrDashStation(JobStationService.Current(job.Commands)),
            Status = statusText,
            Op = buttons.ToArray(),
            Back = string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)
                ? DesignTokens.RowSuccess
                : null,
        };
    }

    // ── แผง Preview / Processing ───────────────────────────

    /// <summary>job ที่ผู้ใช้กดเลือกในตาราง — แผงซ้ายผูกกับตัวนี้</summary>
    private int? _selectedJobId;

    /// <summary>job ที่แผงขวากำลังแสดง กับเวลาแก้ล่าสุดของมัน ไว้กันโหลดซ้ำทุกรอบ poll</summary>
    private int? _processingJobId;
    private long _processingStamp;

    private void WirePanels()
    {
        tblOrders.CellClick += TblOrders_CellClick;

        picPrevPlate.Click += (_, _) => OpenSidePicker(picPrevPlate, lblPrevPlateCaption);
        picPrevShim.Click += (_, _) => OpenSidePicker(picPrevShim, lblPrevShimCaption);
        picProcPlate.Click += (_, _) => OpenSidePicker(picProcPlate, lblProcPlateCaption);
        picProcShim.Click += (_, _) => OpenSidePicker(picProcShim, lblProcShimCaption);

        ShowPreviewSides(null, null);
        ShowProcessingSides(null, null);
    }

    /// <summary>
    /// คลิกแถวไหนก็แสดงรูปของงานนั้นในแผงซ้าย
    /// ข้ามคอลัมน์ Op เพราะเป็นปุ่ม — ปล่อยให้ CellButtonClick จัดการไปตามเดิม
    /// </summary>
    private async void TblOrders_CellClick(object? sender, AntdUI.TableClickEventArgs e)
    {
        if (e.RowType != AntdUI.RowType.None) return;
        if (e.Column?.Key == "Op") return;
        if (e.Record is not OrderRow row) return;
        if (_selectedJobId == row.Id) return;

        _selectedJobId = row.Id;
        await UpdatePreviewAsync();
    }

    /// <summary>แผงซ้าย — รูปของงานที่เลือกอยู่</summary>
    private async Task UpdatePreviewAsync()
    {
        var job = _selectedJobId is int id
            ? _allJobs.FirstOrDefault(j => j.Id == id)
            : null;

        if (job == null)
        {
            ShowPreviewSides(null, null);
            return;
        }

        var sides = await BuildSidesAsync(job);
        if (IsDisposed) return;

        ShowPreviewSides(Find(sides, "Plate"), Find(sides, "Shim"));
    }

    /// <summary>
    /// แผงขวา — งานที่อยู่ในเครื่องของสถานีนี้ ไม่เกี่ยวกับแถวที่เลือกในตาราง
    ///
    /// <para>
    /// ยึดตามเครื่องที่สถานีนี้คุมอยู่ (ST1 ดู MK · ST3 ดู UV2) ไม่ใช่ตามงานที่ขยับล่าสุด
    /// เพราะพอมีงานเดินพร้อมกันหลายใบ งานที่เพิ่งกดเริ่มจะแย่งแผงไปทันที ทั้งที่เครื่อง
    /// ตรงหน้ายังพิมพ์ใบเดิมอยู่ คนที่ยืนอยู่หน้าเครื่องต้องเห็นของเครื่องตัวเอง
    /// </para>
    /// <para>
    /// คิวยังไม่มีข้อมูล (อ่านไม่สำเร็จ หรือเป็นงานที่เริ่มไว้ก่อนมีระบบคิว) ค่อยตกกลับ
    /// ไปใช้เกณฑ์เดิมคือ Process ที่แก้ล่าสุด
    /// </para>
    /// <para>
    /// โหลดใหม่เฉพาะตอนเปลี่ยนตัวหรือ updated_at ขยับ ไม่ใช่ทุกรอบ poll
    /// </para>
    /// </summary>
    private async Task UpdateProcessingAsync()
    {
        var job = _showHistory ? null : JobInMyMachine() ?? NewestProcessJob();

        if (job == null)
        {
            _processingJobId = null;
            _processingStamp = 0;
            ShowProcessingSides(null, null);
            return;
        }

        long stamp = job.UpdatedAt?.Ticks ?? 0;
        if (_processingJobId == job.Id && _processingStamp == stamp) return;

        _processingJobId = job.Id;
        _processingStamp = stamp;

        var sides = await BuildSidesAsync(job);
        if (IsDisposed) return;

        // แผงนี้บอกว่า "ส่งอะไรเข้าเครื่องไปแล้ว" ไม่ใช่ "งานนี้ต้องทำอะไรบ้าง"
        // method 12 กดส่ง MK อย่างเดียวต้องเห็นแค่ด้านของ MK จนกว่าจะกดส่ง UV2
        sides = OnlySent(sides, job.Commands);

        ShowProcessingSides(Find(sides, "Plate"), Find(sides, "Shim"));
    }

    /// <summary>
    /// มีเครื่องที่ต่อไม่ติดไหม — true = บอกผู้ใช้ไปแล้ว ห้ามเริ่มงานนี้
    ///
    /// <para>
    /// ไม่แตะทั้งสถานะงานและคิว งานยังเป็น Waiting เหมือนไม่เคยกด พอแก้เรื่องการ
    /// เชื่อมต่อได้แล้วกดเริ่มใหม่ได้ทันที
    /// </para>
    /// </summary>
    private async Task<bool> BlockedByUnreachableAsync(
        int jobId, MarkingPlan plan, ResolvedJobResponse resolved)
    {
        ShowSending($"กำลังตรวจการเชื่อมต่อ · {JobName(jobId)}");
        List<JobSendService.UnreachableMachine> bad;
        try
        {
            bad = await JobSendService.UnreachableAsync(
                plan.Steps, resolved.Pattern, resolved.UvJobData);
        }
        finally
        {
            if (!IsDisposed) ShowSending(null);
        }

        if (bad.Count == 0) return false;
        if (IsDisposed) return true;

        Notify.ErrorModal(this, "เริ่มงานไม่ได้ — ต่อเครื่องไม่ครบ",
            $"{JobName(jobId)} ต้องใช้ {plan.Steps.Count} ขั้นตอน และต้องต่อได้ครบทุกเครื่อง"
            + Environment.NewLine + Environment.NewLine
            + string.Join(Environment.NewLine, bad.Select(m => $"• {m.Name} — {m.Reason}"))
            + Environment.NewLine + Environment.NewLine
            + "ยังไม่มีอะไรถูกส่งเข้าเครื่อง และงานยังไม่เข้าคิว"
            + Environment.NewLine
            + "แก้แล้วกดเริ่มงานใหม่ได้เลย");

        return true;
    }

    /// <summary>
    /// ส่งค่าของงานเข้า PLC ตอนงานกำลังจะเข้าเครื่อง — ตำแหน่งหัวพ่นและความเร็วสายพาน
    ///
    /// <para>
    /// ไม่ตรวจว่าช่องไหนว่าง ส่งไปตามที่มี ช่องว่างจะกลายเป็น 0 ตามเดิม ต่างจากปุ่ม
    /// ทดสอบในหน้า Order Detail ที่ตั้งใจให้ฟ้องก่อน เพราะปุ่มนั้นคนกดเพื่อลองค่า
    /// ส่วนตรงนี้คือทางเดินของงานจริงซึ่งต้องไม่ถูกขวาง
    /// </para>
    /// <para>
    /// ส่งไม่ผ่านก็ไม่หยุดการส่งเข้าเครื่อง แค่แจ้งเป็นคำเตือนไปในกล่องสรุปผลเดียวกัน
    /// PLC กับเครื่องพิมพ์เป็นคนละสายกัน ตัวหนึ่งล่มไม่ได้แปลว่าอีกตัวทำงานไม่ได้
    /// </para>
    /// <para>
    /// เอาเฉพาะหัวที่งานนี้ใช้ หัวที่ไม่ได้ใช้ไม่ต้องไปเขียนทับค่าใน PLC
    /// </para>
    /// </summary>
    private async Task SendJobPlcAsync(ResolvedJobResponse resolved, List<Notify.ResultLine> lines)
    {
        var plan = await PlcOrderService.BuildPlanAsync(_api, resolved.Pattern, usedHeadsOnly: true);
        if (IsDisposed || plan.Count == 0) return;

        var results = await PlcOrderService.SendAsync(plan);
        if (IsDisposed) return;

        foreach (var r in results)
        {
            if (r.Error != null)
            {
                lines.Add(Notify.Careful($"PLC {r.Name} — {r.Error}"));
                continue;
            }

            // เขียนผ่านแต่ค่าไม่เข้าก็ต้องเห็น ไม่ใช่รายงานว่าสำเร็จ
            if (r.ReadBack != r.Value)
                lines.Add(Notify.Careful(
                    $"PLC {r.Name} = {r.Value} · อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}"));
        }
    }

    /// <summary>ลำดับของเครื่องในแผน — เครื่องที่ไม่อยู่ในแผนไปต่อท้าย</summary>
    private static int PlanOrderOf(List<string> planSteps, string machine)
    {
        int index = planSteps.FindIndex(step =>
            string.Equals(step, machine, StringComparison.OrdinalIgnoreCase));

        return index < 0 ? int.MaxValue : index;
    }

    /// <summary>
    /// ส่งไม่ผ่าน — ถอนสถานะ "เริ่มแล้ว" ทิ้ง เฉพาะงานที่ยังไม่เคยพิมพ์อะไรเลย
    ///
    /// <para>
    /// ก่อนส่งทุกครั้งสถานะถูกตั้งเป็น Process ไว้ กันไม่ให้อีกสถานีกดเริ่มซ้ำระหว่างที่
    /// เครื่องกำลังรับข้อมูลอยู่ พอส่งไม่ผ่านก็ต้องถอนคืน ไม่งั้นงานที่ยังไม่ได้เริ่มเลย
    /// จะค้างเป็น "กำลังผลิต" และกดเริ่มใหม่ไม่ได้อีกเลย
    /// </para>
    /// <para>
    /// งานที่เคยพ่นลงชิ้นงานไปแล้วบางขั้นห้ามถอน ถ้าตีกลับเป็น Waiting งานจะกดเริ่ม
    /// ใหม่ได้ แล้วการกดเริ่มจะส่งขั้นแรกซ้ำ = พ่นซ้ำลงของจริง ปล่อยให้คงเป็น Process
    /// ไว้ ขั้นที่ยังขาดใช้ปุ่มกดหน้างานสั่งต่อได้
    /// </para>
    /// <para>
    /// เดิมตรงนี้ตัดสินจากพารามิเตอร์ว่าเป็นขั้นแรกของแผนไหม ซึ่งผู้เรียกสองในสามที่
    /// ไม่เคยส่งค่ามาเลย ทุกขั้นจึงถือเป็นขั้นแรกหมด งาน marking 11 ที่ส่ง UV1 สำเร็จ
    /// แล้ว UV2 ไม่ผ่าน จะถูกตีกลับเป็น Waiting ทั้งที่ UV1 พ่นไปแล้วจริง
    /// </para>
    /// </summary>
    private async Task UndoStartedStatusAsync(int jobId, ResolvedJobResponse resolved)
    {
        if (PrintedBefore(resolved)) return;
        await _api!.UpdateJobStatusAsync(jobId, "Waiting");
    }

    /// <summary>งานนี้เคยส่งเข้าเครื่องสำเร็จมาก่อนไหม — ดูจากประวัติคำสั่งที่บันทึกไว้</summary>
    private static bool PrintedBefore(ResolvedJobResponse resolved) =>
        resolved.Commands?.Any(c => c.Success) == true;

    /// <summary>งานที่ถือเครื่องของสถานีนี้อยู่ตอนนี้ — null เมื่อเครื่องว่างหรือยังไม่รู้คิว</summary>
    private PrintJob? JobInMyMachine()
    {
        var machine = MachineOfThisStation();

        var holder = _queueRows.FirstOrDefault(r =>
            r.State == "active"
            && string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase));

        return holder == null
            ? null
            : _allJobs.FirstOrDefault(j => j.Id == holder.PrintJobsId);
    }

    /// <summary>เกณฑ์สำรองแบบเดิม — งานที่เดินอยู่และแก้ล่าสุด</summary>
    private PrintJob? NewestProcessJob() =>
        _allJobs
            .Where(j => string.Equals(j.Status, "Process", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(j => j.UpdatedAt ?? DateTime.MinValue)
            .FirstOrDefault();

    private static MarkingRefSide? Find(List<MarkingRefSide> sides, string side) =>
        sides.FirstOrDefault(s => s.Side == side);

    /// <summary>
    /// กรองเหลือเฉพาะด้านที่ step ของมันถูกกดส่งสำเร็จไปแล้ว
    ///
    /// ข้อจำกัด: marking 22 ที่ MK ทำทั้งสองด้านมี command "MK" อันเดียว
    /// ระบบแยกไม่ออกว่ารอบแรกทำด้านไหน จึงขึ้นทั้งสองด้านพร้อมกัน
    /// </summary>
    private static List<MarkingRefSide> OnlySent(
        List<MarkingRefSide> sides, List<CommandResult>? commands)
    {
        var sent = (commands ?? new List<CommandResult>())
            .Where(c => c.Success)
            .Select(c => c.Command)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ของเก่าบางงานบันทึกเป็น MK1 / MK2 แทน MK
        bool mkSent = sent.Contains("MK") || sent.Contains("MK1") || sent.Contains("MK2");

        return sides
            .Where(s => s.Step == "MK" ? mkSent : sent.Contains(s.Step))
            .ToList();
    }

    /// <summary>
    /// ฝั่ง MK ใช้ erp_mfg ที่ติดมากับ /job/getAll อยู่แล้ว
    /// ฝั่ง UV ต้องรู้ชื่อโปรแกรมซึ่งอยู่ใน uv_job_data จึงต้องยิง getResolved
    /// เรียกเฉพาะงานที่ใช้ UV จริง และเฉพาะตอนที่งานเปลี่ยนเท่านั้น
    /// </summary>
    private async Task<List<MarkingRefSide>> BuildSidesAsync(PrintJob job)
    {
        var method = job.PlanRouting?.MarkingMethod;
        var plan = MarkingMethodService.Resolve(method);

        UvProgramInfo? uv1 = null, uv2 = null;
        bool needsUv = plan.Plate == MarkingMachine.Uv1 || plan.Shim == MarkingMachine.Uv2;

        if (needsUv && _api != null)
        {
            var resolved = await _api.GetResolvedJobAsync(job.Id);
            if (resolved != null)
            {
                uv1 = PickUvProgram(resolved, "UV1");
                uv2 = PickUvProgram(resolved, "UV2");
            }
        }

        return MarkingRefResolver.Resolve(method, job.PlanRouting?.ErpMfg, uv1, uv2);
    }

    /// <summary>
    /// ส่งไปแล้ว → ใช้รุ่นย่อยที่เลือกจริง (เก็บใน payload ของ command) ถือว่ายืนยันแล้ว
    /// ยังไม่ได้ส่ง → ได้แค่ชื่อฐานจากข้อมูลงาน ยังตอบไม่ได้ว่าจะพิมพ์รุ่นไหน
    /// </summary>
    private static UvProgramInfo PickUvProgram(ResolvedJobResponse resolved, string machine)
    {
        var sent = resolved.Commands
            .LastOrDefault(c => c.Success &&
                string.Equals(c.Command, machine, StringComparison.OrdinalIgnoreCase));

        if (sent?.Payload != null && sent.Payload.TryGetValue("program", out var value))
        {
            var chosen = value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(chosen)) return new UvProgramInfo(chosen, true);
        }

        var baseName = resolved.UvJobData
            .FirstOrDefault(u => string.Equals(u.Machine, machine, StringComparison.OrdinalIgnoreCase))
            ?.ProgramName;

        return new UvProgramInfo(baseName, false);
    }

    // ── แผงรูปทั้งแผง ──────────────────────────────────────

    private void ShowPreviewSides(MarkingRefSide? plate, MarkingRefSide? shim)
    {
        ApplySide(plate, picPrevPlate, lblPrevPlateCaption);
        ApplySide(shim, picPrevShim, lblPrevShimCaption);
        BalanceSlots(tlpPreviewSlots, picPrevPlate, picPrevShim);
    }

    private void ShowProcessingSides(MarkingRefSide? plate, MarkingRefSide? shim)
    {
        ApplySide(plate, picProcPlate, lblProcPlateCaption);
        ApplySide(shim, picProcShim, lblProcShimCaption);
        BalanceSlots(tlpProcessingSlots, picProcPlate, picProcShim);
    }

    /// <summary>
    /// สองช่องแบ่งกรอบคนละครึ่งตายตัว พองานใช้ด้านเดียว (เช่น method 02 มีแต่ Plate)
    /// รูปเลยไปเกาะครึ่งซ้าย เหลือครึ่งขวาว่าง ดูเหมือนวางผิดที่มากกว่าจะดูตั้งใจ
    ///
    /// ยุบคอลัมน์ที่ไม่ได้ใช้เหลือ 0 ด้านที่เหลือจะกินเต็มกรอบ และ PictureBox โหมด
    /// Zoom จัดรูปไว้กลางกรอบให้เอง โดยไม่ยืดรูปผิดสัดส่วน
    /// </summary>
    private static void BalanceSlots(TableLayoutPanel slots, PictureBox left, PictureBox right)
    {
        bool hasLeft = left.Visible, hasRight = right.Visible;

        // ไม่เหลือด้านไหนเลยก็คืนเป็นครึ่งต่อครึ่ง กรอบว่างจะได้ไม่เพี้ยน
        if (hasLeft == hasRight)
        {
            slots.ColumnStyles[0].Width = 50F;
            slots.ColumnStyles[1].Width = 50F;
            return;
        }

        slots.ColumnStyles[0].Width = hasLeft ? 100F : 0F;
        slots.ColumnStyles[1].Width = hasRight ? 100F : 0F;
    }

    // ── ช่องรูปหนึ่งช่อง ───────────────────────────────────

    private static void ClearSlot(PictureBox box, Label caption)
    {
        box.Image?.Dispose();
        box.Image = null;
        box.Tag = null;
        box.Visible = false;
        caption.Text = "";
        caption.Visible = false;
    }

    /// <summary>
    /// ด้านที่ marking method ไม่ได้ใช้ → ซ่อนทั้งช่อง
    /// ด้านที่ใช้แต่ไม่มีรูป → ยังโชว์ป้ายไว้ พร้อมบอกสาเหตุ ไม่ปล่อยว่างเปล่า
    /// </summary>
    private static void ApplySide(MarkingRefSide? side, PictureBox box, Label caption)
    {
        box.Image?.Dispose();
        box.Image = null;

        if (side == null)
        {
            ClearSlot(box, caption);
            return;
        }

        box.Tag = side;
        box.Visible = true;
        caption.Visible = true;

        var path = side.Images.FirstOrDefault();
        if (path == null)
        {
            var reason = side.LookupName == null
                ? "ไม่มี ERP MFG"
                : side.NearMiss > 0
                    ? $"ไม่พบรูปชื่อ {side.LookupName} (มี {side.NearMiss} ไฟล์ชื่อใกล้เคียง)"
                    : MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder());
            caption.Text = $"{side.Side} · {side.Machine} · {reason}";
            box.Image = MarkingRefImageService.Placeholder();
            return;
        }

        // ยังไม่ได้เลือกรุ่นย่อย — หยิบใบไหนมาโชว์ก็ดูเหมือนระบบเลือกไว้ให้แล้ว
        // ปล่อยกรอบว่างไว้แล้วบอกตรง ๆ ว่ามีให้เลือกกี่แบบ (กดที่กรอบดูตัวอย่างได้)
        if (side.Pending)
        {
            caption.Text = $"{side.Side} · {side.Machine} · ยังไม่ได้เลือกรุ่น ({side.Images.Count} แบบ)";
            return;
        }

        box.Image = MarkingRefImageService.LoadImageNoLock(path);
        if (box.Image == null)
        {
            caption.Text = $"{side.Side} · {side.Machine} · เปิดไฟล์รูปไม่ได้";
            box.Image = MarkingRefImageService.Placeholder();
            return;
        }

        var more = side.Images.Count > 1 ? $"  (+{side.Images.Count - 1})" : "";
        caption.Text = $"{side.Side} · {side.Machine} · {Path.GetFileName(path)}{more}";
    }

    /// <summary>
    /// คลิกรูปเพื่อดูใหญ่และเลือกใบอื่นของด้านเดียวกัน
    /// ฝั่งนี้เลือกแล้วเปลี่ยนแค่รูปที่ดู ไม่กระทบงานที่พิมพ์ ข้อความจึงต้องบอกให้ชัด
    /// </summary>
    private void OpenSidePicker(PictureBox box, Label caption)
    {
        if (box.Tag is not MarkingRefSide side || side.Images.Count == 0) return;

        var options = side.Images
            .Select(path => new MarkingRefOption(path, Path.GetFileName(path), new List<string> { path }))
            .ToList();

        var chosen = MarkingRefPickerDialog.Pick(
            this,
            "เลือกรูปอ้างอิง",
            $"{side.Side} · {side.Machine} — เลือกรูปที่จะดู (ไม่มีผลกับงานที่พิมพ์)",
            options);

        if (chosen == null) return;

        var reordered = new List<string> { chosen };
        reordered.AddRange(side.Images.Where(path => path != chosen));
        ApplySide(side with { Images = reordered, Pending = false }, box, caption);

        // เปิดดูเองตอนที่ยังไม่ได้เลือกรุ่น ต้องไม่ให้เข้าใจว่านี่คือรุ่นที่จะพิมพ์
        if (side.Pending) caption.Text += "  (ตัวอย่าง)";
    }

    /// <summary>
    /// ผูก DataSource ใหม่ทีไร AntdUI ล้าง selection ทิ้งเสมอ ต้องเลือกกลับให้เอง
    /// ไม่งั้นไฮไลต์กับรูปจะหายทุกครั้งที่มีงานไหนก็ตามเปลี่ยนสถานะ
    /// งานที่หลุดจากลิสต์แล้วจริง ๆ ค่อยล้างทั้งไฮไลต์และรูปพร้อมกัน
    /// </summary>
    private void RestoreSelection()
    {
        if (_selectedJobId is not int id) return;

        var display = tblOrders.SortList();
        int index = Array.FindIndex(display, r => r is OrderRow row && row.Id == id);

        if (index < 0)
        {
            _selectedJobId = null;
            ShowPreviewSides(null, null);
            return;
        }

        if (tblOrders.SelectedIndex != index) tblOrders.SelectedIndex = index;
    }

    private void DisposePanelImages()
    {
        foreach (var box in new[] { picPrevPlate, picPrevShim, picProcPlate, picProcShim })
        {
            box.Image?.Dispose();
            box.Image = null;
        }
    }
}

internal class OrderRow : AntdUI.NotifyProperty
{
    public int Id { get; set; }
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public string ErpMfg { get; set; } = "";
    public string LotNo { get; set; } = "";
    public string Qty { get; set; } = "";
    public string ProcessSequence { get; set; } = "";
    public string Plate { get; set; } = "";
    public string Shim { get; set; } = "";
    public string Station { get; set; } = "";
    public AntdUI.CellText? Status { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = [];
    public Color? Back { get; set; }
}
