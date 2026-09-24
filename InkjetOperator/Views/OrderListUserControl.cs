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
        _api = new ApiClient($"http://{CustomSettingsManager.Read("PC_IP", "127.0.0.1")}:3000"); // ชี้ไป Backend กลางตาม IP ที่ตั้งไว้
        _ = RefreshDataAsync(); // อ่านรายการงานทันทีที่เปิดหน้า
        StartPolling(); // เริ่มอ่านงานซ้ำทุก 5 วินาที
        _pushButton.Start(); // เริ่มฟังปุ่มหน้างานจาก PLC
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
        _pushButton.ShouldWatch = () =>
            !_sending && !_pushHandling && !_showingRemoteError && Visible;

        _pushButton.Pressed += async (_, _) => await OnPushButtonPressedAsync();

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
    private async Task OnPushButtonPressedAsync(string? machineOverride = null)
    {
        if (_api == null || _pushHandling || IsDisposed) return; // ยังไม่พร้อมหรือกำลังปล่อยเครื่องอยู่ ให้ข้ามการกดครั้งนี้

        _pushHandling = true; // กันปุ่มหน้างานสั่งปล่อยเครื่องซ้อน
        try
        {
            var machine = machineOverride ?? MachineOfThisStation(); // ใช้เครื่องที่ทดสอบระบุมา หรือ MK ของ ST1 / UV2 ของ ST3

            // งานที่เข้าเครื่องเดิมหลายรอบจะถือเครื่องไว้ให้รอบถัดไปหรือไม่
            // เป็นตัวเลือกที่ Setting → ตัวเลือกหน้างาน ค่าเริ่มต้นคือถือไว้
            var (release, error) = await _api.ReleaseMachineAsync( // ขอปล่อยคิวปัจจุบันและรับคิวถัดไปจาก Backend
                machine, StationService.HoldForNextRound); // กำหนดว่าจะให้รอบถัดไปของงานเดิมถือเครื่องต่อหรือไม่
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

            if (release == null) // Backend ไม่คืนผลการปล่อยเครื่อง
            {
                Notify.Warn(this, $"ปล่อยเครื่อง {machine} ไม่สำเร็จ — {error}"); // บอกเหตุที่ปล่อยเครื่องไม่ได้
                return; // ยังปล่อยไม่สำเร็จ จึงไม่รีเซ็ตหัวพิมพ์
            }

            // ห้ามออกจากเมธอดตรงนี้ — บรรทัดท้ายเมธอดคือตัวที่ไปหยิบงานรอบถัดไป
            // มาส่งเข้าเครื่องทันที ถ้าข้ามไป การส่งจะไปรอนาฬิกา poll รอบหน้า
            // คนกดปุ่มจะเห็นเป็นค้างไปหลายวินาทีก่อนอะไรจะเกิดขึ้น
            if (release.Next != null) // มีงานถัดไปได้รับสิทธิ์ใช้เครื่องแล้ว
            {
                Notify.Success(this, $"{machine} ว่างแล้ว · งานถัดไปในคิวจะถูกส่งให้"); // แจ้งว่าคิวถัดไปพร้อมให้ ST1 ส่งข้อมูล
            }
            else
            {
                // ไม่มีใครรอคิว — เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้น
                //
                // มีคิวรออยู่ไม่ต้องเลื่อน เพราะงานถัดไปเขียนตำแหน่งของมันทับอยู่แล้ว
                // การเลื่อนกลับก่อนแล้วเลื่อนไปใหม่คือขยับหัวสองรอบโดยไม่ได้อะไร
                Notify.Success(this, $"{machine} ว่างแล้ว · ไม่มีงานรอคิว"); // แจ้งว่าเครื่องไม่มีคิวรอแล้ว
                await ResetHeadPositionAsync(machine); // ขอให้ PLC พาหัวพิมพ์กลับตำแหน่งเริ่มต้น
            }
        }
        finally
        {
            _pushHandling = false; // เปิดรับการกดปุ่มหน้างานครั้งถัดไป
        }

        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านคิวใหม่ทันที เพื่อให้ ST1 ส่งงานที่เพิ่งได้สิทธิ์
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
        if (!string.Equals(machine, "MK", StringComparison.OrdinalIgnoreCase)) return; // รีเซ็ตตำแหน่งเฉพาะ MK; UV1 / UV2 ข้ามขั้นนี้

        var results = await PlcOrderService.ResetPositionAsync(_api); // สั่ง PLC คืนตำแหน่งหัว MK ผ่านค่าในระบบ
        if (IsDisposed || results.Count == 0) return; // หน้าปิดแล้วหรือไม่มีผลเครื่องให้รายงาน จึงจบการรีเซ็ต

        var failed = results.Where(r => r.Error != null).ToList(); // เลือกเฉพาะหัวที่รีเซ็ตไม่ได้มาแจ้ง
        if (failed.Count == 0) // ทุกหัวที่มีผลตอบกลับไม่มีข้อผิดพลาด
        {
            Notify.Success(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นแล้ว"); // แจ้งว่าคำสั่งคืนตำแหน่งหัว MK สำเร็จ
            return; // รีเซ็ตผ่านแล้ว ไม่แสดงคำเตือนต่อ
        }

        Notify.Warn(this, "เลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้นไม่สำเร็จ — " // แจ้งหัวที่ยังคืนตำแหน่งไม่ได้
            + string.Join(" · ", failed.Select(r => $"{r.Name} {r.Error}")));
    }

    private static bool SentAlready(ResolvedJobResponse resolved, string step) =>
        resolved.Commands?.Any(c => c.Success &&
            string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true;

    private void StartPolling() // Flow 14: ตั้งรอบอ่านงานของ Station
    {
        _pollTimer = new System.Windows.Forms.Timer { Interval = 5000 }; // ตั้งรอบอ่านทุก 5 วินาที
        _pollTimer.Tick += async (_, _) => await RefreshDataAsync(); // ครบเวลาแล้วไปอ่านรายการงาน
        _pollTimer.Start(); // เริ่มนับรอบ
    }

    private async Task RefreshDataAsync(bool force = false) // อ่าน Backend แล้วอัปเดตรายการบนหน้านี้
    {
        if (_api == null) return; // ยังไม่มีตัวเรียก Backend ให้ข้าม

        // ระหว่างส่งงานห้ามผูก DataSource ใหม่ ไม่งั้นแถวขยับใต้มือผู้ใช้
        // และกล่องเลือกรุ่นย่อยของ UV อาจถูกวาดทับ
        if (_sending) return; // กำลังส่งเครื่องอยู่ ยังไม่เปลี่ยนรายการบนจอ

        // รอบก่อนยังไม่จบก็ข้ามรอบนี้ไป — นาฬิกาเดินทุก 5 วิ แต่ถ้า backend ช้า
        // หรือต่อไม่ติด คำขอหนึ่งรออยู่ได้ถึง 10 วิ ไม่กันไว้รอบใหม่จะทับกันไป
        // เรื่อย ๆ จนมีคำขอค้างพร้อมกันหลายชุด แล้วเด้งกล่องผิดพลาดตามมาเป็นพรวด
        if (_refreshing) return; // รอบก่อนยังไม่จบ ไม่เริ่มซ้อน

        _refreshing = true; // จองรอบอ่านข้อมูลไว้
        try // เริ่มอ่านงานและอัปเดตหน้า Order List และดักข้อผิดพลาดไว้
        {
            DateTime? fromUtc = null, toUtc = null; // เริ่มจากไม่กรองช่วงเวลา
            if (_showHistory && TryGetDateRange(out var from, out var to)) // หน้า History มีช่วงวันที่ที่เลือกไว้
            {
                fromUtc = ToUtcFromThai(from); // แปลงเวลาเริ่มเป็น UTC
                toUtc = ToUtcFromThai(to); // แปลงเวลาสิ้นสุดเป็น UTC
            }

            var (jobs, error) = await _api.GetAllJobsAsync(100, fromUtc, toUtc); // อ่านงานจาก Backend รวมงานที่ Barcode เพิ่งสร้าง
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้
            if (error != null) // อ่านรายการงานไม่สำเร็จ
            {
                tblOrders.EmptyText = $"Error: {error}"; // แสดงสาเหตุในตาราง
                return; // อ่านงานไม่ได้ จึงไม่อัปเดตรายการรอบนี้
            }
            _allJobs = jobs; // เก็บงานที่ Backend ส่งกลับ

            // ทำก่อนเช็ค signature เพราะคำขอจาก ST3 ไม่ได้เปลี่ยนอะไรที่ตารางวาด
            // ถ้าไปทำหลังจากนั้น รอบที่หน้าจอไม่มีอะไรเปลี่ยนจะข้ามคำขอไปเลย
            await RecoverAbandonedRemoteStartsAsync(); // ตรวจคำขอเริ่มงานที่ค้างอยู่
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้

            await ProcessRemoteStartsAsync(); // จัดการคำขอเริ่มงานจากอีก Station
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้

            await ProcessMachineQueueAsync(); // จัดการงานที่รอคิวเครื่อง
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้

            await RefreshStationBarAsync(); // อัปเดตสถานะงานของ Station
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้

            await ShowRemoteErrorsAsync(); // แสดงข้อผิดพลาดที่อีก Station ส่งกลับ
            if (IsDisposed) return; // หน้าถูกปิดแล้วให้หยุดรอบนี้

            // ผูก DataSource ใหม่ทีไร ตารางจะรีเซ็ตทั้งลำดับที่เรียงไว้และตำแหน่ง scroll
            // รอบ poll ที่ข้อมูลไม่เปลี่ยนจึงไม่ต้องผูกใหม่ ไม่งั้นทุก 5 วิจะกระตุกทีนึง
            var signature = BuildSignature(jobs) + QueueSignature(); // รวมข้อมูลไว้เทียบว่ารายการเปลี่ยนหรือยัง
            if (!force && signature == _lastSignature) return; // ข้อมูลเดิมและไม่ได้บังคับ ไม่วาดตารางใหม่

            _lastSignature = signature; // จำข้อมูลรอบนี้ไว้เทียบครั้งหน้า
            RebindTable(); // กรองงานของ Station แล้วใส่ตาราง
            await UpdateProcessingAsync(); // อัปเดตส่วนแสดงงานที่กำลังทำ
        }
        catch (Exception ex) // จัดการปัญหาระหว่างอ่านงานและอัปเดตหน้า Order List
        {
            if (!IsDisposed) // ยังมีหน้าจอให้แสดงข้อผิดพลาด
                tblOrders.EmptyText = $"Error: {ex.Message}"; // แสดงข้อความที่ทำให้รอบอ่านล้มเหลว
        }
        finally // จบรอบอ่านแล้ว ต้องเปิดให้รอบถัดไปทำงานได้
        {
            _refreshing = false; // ปลดให้รอบถัดไปอ่านข้อมูลได้
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

    private void RebindTable() // เลือกรายการที่ Station นี้ต้องเห็น
    {
        var statuses = _showHistory ? HistoryStatuses : ActiveStatuses; // เลือกกลุ่มสถานะตามแท็บที่เปิด
        int station = StationService.Current; // อ่านว่าเครื่องนี้เป็น Station ใด

        // ST1 เห็นประวัติทั้งสาย รวมงานที่ ST3 ทำจนจบซึ่งตัวเองไม่เคยเห็นในแท็บ List
        // เพราะเป็นจอที่ใช้ตามงานทั้งกระบวนการ
        //
        // ส่วน ST3 ไม่มีแท็บ History ให้กดอยู่แล้ว เงื่อนไข IsSt3 ตรงนี้จึงเป็นแค่
        // ตัวกันไว้ เผื่อวันหลังเปิดแท็บคืนให้ ST3 จะได้ยังกรองเฉพาะงานของตัวเอง
        bool showEveryStation = _showHistory && !StationService.IsSt3; // History ของ ST1 ดูงานได้ทุก Station

        var filtered = _allJobs // เริ่มกรองจากงานที่ Backend ส่งมา
            .Where(j => statuses.Contains(j.Status, StringComparer.OrdinalIgnoreCase)) // เก็บงานที่สถานะตรงกับแท็บ
            .Where(j => showEveryStation // ถ้าไม่ได้ดูประวัติทุก Station ให้เช็กวิธีพิมพ์
                || MarkingMethodService.VisibleAt(station, j.PlanRouting?.MarkingMethod)) // เก็บงานที่กฎอนุญาตให้ Station นี้เห็น
            .OrderBy(StatusRank) // เรียงตามกลุ่มสถานะ
            .ThenByDescending(j => j.CreatedAt ?? DateTime.MinValue) // ในสถานะเดียวกันให้งานใหม่อยู่ก่อน
            .ToList(); // เก็บผลกรองเป็นรายการ

        bool dateFiltered = _showHistory && TryGetDateRange(out _, out _); // จำว่ากำลังใช้ตัวกรองวันที่หรือไม่

        var rows = filtered.Select(j => ToRow(j, _showHistory)).ToList(); // แปลง Job เป็นแถวบนตาราง
        tblOrders.EmptyText = _allJobs.Count == 0 // เลือกข้อความเมื่อไม่มีแถวให้แสดง
            ? "No orders" // Backend ไม่มีรายการงาน
            : dateFiltered && rows.Count == 0 // มีตัวกรองวันที่แต่ไม่พบงาน
                ? "ไม่มีงานในช่วงวันที่ที่เลือก" // แจ้งว่าช่วงวันที่นี้ไม่มีงาน
                : $"No orders (total {_allJobs.Count}, filter: {string.Join("/", statuses.Select(JobStatusDisplay.Text))})"; // แสดงจำนวนงานและสถานะที่ใช้กรอง
        tblOrders.DataSource = null; // ถอดรายการเดิมออกก่อน
        tblOrders.DataSource = rows; // แสดงงานที่กรองแล้วบน Order List
        ReapplySort(); // คืนลำดับเรียงที่ผู้ใช้เลือกไว้
        RestoreSelection(); // คืนแถวที่ผู้ใช้เลือกไว้
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
        if (e.Record is not OrderRow row) return; // รับเฉพาะปุ่มในแถวงานที่อ่านได้
        if (_api == null || _rowBusy) return; // Backend ยังไม่พร้อมหรือปุ่มก่อนหน้ายังทำไม่จบ ให้ข้าม

        _rowBusy = true; // กันการกดปุ่มในตารางซ้อนกัน
        try
        {
            await HandleRowButtonAsync(e.Btn?.Id, row); // แยกไปทำตามปุ่มที่กดและ Job ของแถวนั้น
        }
        finally
        {
            _rowBusy = false; // เปิดให้กดปุ่มในตารางได้อีกครั้ง
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
        var task = _api!.GetResolvedJobAsync(jobId); // ขอข้อมูล Job พร้อม Pattern, UV และประวัติส่งจาก Backend
        if (await Task.WhenAny(task, Task.Delay(SlowLoadMs)) == task) return await task; // ถ้าอ่านทันภายในเวลาที่กำหนด ใช้ข้อมูลได้เลยโดยไม่ขึ้นหน้ารอ

        ShowSending(busyText); // ขึ้นข้อความรอเมื่ออ่านรายละเอียดช้า
        try
        {
            return await task; // รอข้อมูลชุดเดิมให้จบ ไม่ยิงคำขอซ้ำ
        }
        finally
        {
            if (!IsDisposed) ShowSending(null); // ปิดข้อความรอเมื่ออ่านเสร็จ
        }
    }

    private async Task HandleRowButtonAsync(string? buttonId, OrderRow row)
    {
        if (buttonId == "detail") // ปุ่มรายละเอียดเปิดข้อมูลของแถวที่เลือก
        {
            var resolved = await LoadJobAsync(row.Id, $"กำลังโหลดข้อมูล · {JobName(row.Id)}"); // อ่านรายละเอียดล่าสุดก่อนเปิดหน้าต่าง
            if (IsDisposed) return; // หน้ารายการถูกปิดแล้ว จึงไม่เปิด Detail ต่อ
            if (resolved == null) // ไม่มีข้อมูลให้เปิดดู
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลด Detail ของ {JobName(row.Id)} ได้"); // แจ้งว่าอ่านรายละเอียดงานไม่ได้
                return; // หยุดก่อนเปิดหน้าต่างที่ไม่มีข้อมูล
            }
            await ShowDetailDialogAsync(resolved); // เปิด Order Detail แล้วรอให้ผู้ใช้ปิด
        }
        else if (buttonId == "start") // ปุ่มเริ่มงานเข้าลำดับตรวจแผนและจองเครื่อง
        {
            await StartJobAsync(row.Id); // เริ่ม Job ของแถวที่กด
        }
        else if (buttonId == "complete") // ปุ่มจบงานเข้าลำดับตรวจขั้นที่ส่งแล้ว
        {
            await CompleteJobAsync(row.Id); // ตรวจและบันทึกจบ Job ที่เลือก
        }
        else if (buttonId == "cancel") // ปุ่มยกเลิกเข้าลำดับยืนยันยกเลิกงาน
        {
            await CancelJobAsync(row.Id); // ยกเลิก Job ของแถวที่กด
        }
        else if (buttonId == "restore") // ปุ่มพิมพ์ใหม่ใน History นำงานเดิมกลับมารอ
        {
            await RestoreJobAsync(row.Id); // คืน Job เดิมเป็น Waiting
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
        if (_api == null) return; // ยังไม่มีตัวเชื่อม Backend จึงนำงานกลับไม่ได้

        if (!Confirm.Ask(this, "ยืนยันนำกลับมาพิมพ์ใหม่", // ให้ผู้ใช้ยืนยันก่อนเปลี่ยนงานใน History กลับมารอ
                $"{JobName(jobId)}\n\n"
                + "งานจะกลับไปอยู่ในรายการงาน รอกดเริ่มงานอีกครั้ง\n\n"
                + "ยืนยันหรือไม่?"))
            return; // ผู้ใช้ไม่ยืนยัน จึงเก็บสถานะเดิมไว้

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Waiting"); // เปลี่ยน Job เดิมเป็นรอเริ่ม โดยไม่ได้สร้าง Job ใหม่
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        if (!ok) // Backend เปลี่ยนสถานะไม่สำเร็จ
        {
            Notify.ErrorModal(this, "นำกลับมาไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้"); // แสดงเหตุที่นำงานกลับมารอไม่ได้
            return; // หยุดก่อนแจ้งว่านำงานกลับสำเร็จ
        }

        // ล้างคำขอ/ข้อความผิดพลาดที่ค้างจากรอบก่อน ไม่งั้น ST1 อาจหยิบธงเก่าไปส่งทันที
        // ที่งานกลับมาเป็น Waiting ทั้งที่ยังไม่มีใครกดเริ่มงานรอบใหม่
        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอส่งจาก ST3 ที่อาจค้างอยู่
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        Notify.Success(this, $"{JobName(jobId)} กลับไปอยู่ในรายการงานแล้ว"); // แจ้งว่างานกลับมาอยู่ในรายการรอแล้ว
        await RefreshDataAsync(force: true); // อ่านรายการใหม่ให้เห็นสถานะ Waiting
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
        if (_api == null) return; // ยังไม่มีตัวเชื่อม Backend จึงยกเลิกงานไม่ได้

        if (!Confirm.Ask(this, "ยืนยันยกเลิกงาน", // ให้ผู้ใช้ยืนยันก่อนย้ายงานออกจากรายการผลิต
                $"ยกเลิก {JobName(jobId)}\n\n"
                + "งานจะถูกย้ายออกจากรายการไปอยู่ในประวัติ\n"
                + "ถ้าต้องการทำต่อ กดพิมพ์ใหม่ได้ที่แท็บ History\n\n"
                + "ยืนยันหรือไม่?"))
            return; // ผู้ใช้ไม่ยืนยัน จึงยังเก็บงานไว้ตามเดิม

        // ล้างคิวก่อนเปลี่ยนสถานะ งานที่ยกเลิกต้องไม่ถือเครื่องหรือค้างคิวไว้
        await _api.ClearMachineQueueAsync(jobId); // ล้างคิวเครื่องของ Job ที่กำลังยกเลิก

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Cancel"); // บันทึกสถานะยกเลิกใน Backend
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        if (!ok) // Backend บันทึกยกเลิกไม่สำเร็จ
        {
            Notify.ErrorModal(this, "ยกเลิกงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะยกเลิกได้"); // แสดงเหตุที่เปลี่ยนสถานะไม่ได้
            return; // หยุดก่อนแจ้งว่ายกเลิกสำเร็จ
        }

        // ยกเลิกตอนที่ ST3 ฝากคำขอค้างไว้ ธงต้องหายไปด้วย ไม่งั้นมันจะค้างอยู่กับงาน
        // ข้ามไปถึงตอนที่งานถูกนำกลับมาพิมพ์ใหม่
        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอส่งที่ค้าง ไม่ให้ ST1 หยิบคำขอเดิมต่อ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        Notify.Success(this, $"ยกเลิก {JobName(jobId)} แล้ว"); // แจ้งผลยกเลิก Job ที่เลือก
        await RefreshDataAsync(force: true); // อ่านรายการใหม่ให้งานที่ยกเลิกหายจาก List
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
        if (_api == null || _sending) return; // ยังไม่พร้อมหรือกำลังส่งเข้าเครื่องอยู่ ให้ข้ามการเริ่มซ้ำ

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่าน Job ล่าสุด เพราะข้อมูลในตารางอาจยังไม่อัปเดต
        if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่เริ่มส่งงานต่อ
        if (resolved == null) // อ่านข้อมูล Job ไม่ได้
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้"); // แจ้งผู้ใช้ว่าโหลดงานไม่สำเร็จ
            return; // ไม่มีรายละเอียดงาน จึงยังจองเครื่องไม่ได้
        }

        var method = resolved.PlanRouting?.MarkingMethod; // อ่านรหัสวิธีพิมพ์จาก Routing ของงาน
        int station = StationService.Current; // ดูว่าจอนี้ทำงานเป็น Station ใด

        if (!MarkingMethodService.CanStartAt(station, method)) // ตรวจว่างานรหัสนี้เริ่มที่ Station ปัจจุบันได้หรือไม่
        {
            Notify.WarnModal(this, "เริ่มงานที่สถานีนี้ไม่ได้", // บอกผู้ใช้ให้ไปเริ่มที่ Station ที่รับผิดชอบ
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + ((method ?? "").Trim() == "10"
                    ? "งาน marking 10 เริ่มได้ที่ ST3 เท่านั้น"
                    : "งานนี้เริ่มได้ที่ ST1 เท่านั้น"));
            return; // ผิด Station จึงไม่สร้างคิวเครื่อง
        }

        var plan = MarkingMethodService.Resolve(method); // แปลงรหัสวิธีพิมพ์เป็นลำดับ MK / UV1 / UV2
        if (plan.NoCase) // รหัสนี้ไม่มีแผนพิมพ์ที่ระบบรองรับ
        {
            Notify.WarnModal(this, "แจ้งเตือน", // แจ้งให้ตรวจรหัสวิธีพิมพ์ของงาน
                $"{JobName(jobId)} ใช้รหัส marking ที่ไม่มีอยู่จริง ({Method(method)})");
            return; // ไม่มีแผนที่ใช้ได้ จึงไม่ส่งเข้าเครื่อง
        }

        // marking 00 ไม่มีคำสั่งต้องส่งเข้าเครื่องเลย — เป็นงานที่ทำด้วยมือล้วน
        // การกดเริ่มจึงแค่เปลี่ยนสถานะให้คนอื่นเห็นว่ามีคนรับไปทำแล้ว
        if (plan.Steps.Count == 0) // ไม่มีขั้นส่งเครื่อง เช่น marking 00
        {
            await StartWithoutSendingAsync(jobId, method); // เปลี่ยนเป็นกำลังผลิตอย่างเดียวสำหรับงานไม่มีขั้นส่ง
            return; // งานนี้ไม่ต้องจองหรือส่งเครื่องต่อ
        }

        // สรุปให้ดูก่อนว่าจะส่งอะไรเข้าเครื่องไหนบ้าง แล้วค่อยลงมือ
        //
        // ถามก่อนจอง ไม่ใช่หลังจอง — กดยกเลิกแล้วต้องไม่มีอะไรค้างอยู่ในคิวเลย
        if (!await ConfirmStartAsync(jobId, resolved, plan)) return; // ให้ยืนยันแผนก่อนสร้างคิว ถ้ายกเลิกก็หยุดตรงนี้
        if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่จองเครื่องต่อ

        // จองทุกเครื่องที่แผนของงานนี้ต้องใช้ ในคราวเดียว
        //
        // จองก่อนส่งเสมอ เพราะการจองคือสิ่งที่บอกว่างานนี้มีสิทธิ์ในเครื่องไหนบ้าง
        // เครื่องที่ว่างจะถูกส่งต่อทันทีข้างล่าง ส่วนเครื่องที่ไม่ว่างก็รออยู่ในคิว
        // จนกว่าคนหน้างานจะกดปุ่มปล่อยเครื่อง
        var (queued, queueError) = await _api.EnqueueMachinesAsync(jobId, QueueItemsFor(plan.Steps)); // จองทุกเครื่องพร้อมเลขรอบ เช่น marking 22 มี MK สองรอบ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        if (!queued) // Backend สร้างคิวไม่สำเร็จ
        {
            Notify.ErrorModal(this, "จองเครื่องไม่สำเร็จ", // แจ้งเหตุที่งานยังเข้าคิวไม่ได้
                $"{JobName(jobId)} ยังไม่ได้เข้าคิว" + Environment.NewLine + Environment.NewLine
                + (queueError ?? "ติดต่อ backend ไม่ได้"));
            return; // ยังไม่มีคิว จึงไม่ขอสิทธิ์ส่งเครื่อง
        }

        // ST3 ไม่ได้ต่อสายเข้าเครื่อง จองแล้วจบ ST1 จะหยิบไปส่งให้เองจากคิว
        if (station == StationService.St3) // ST3 จบขั้นเริ่มงานที่การจองคิว
        {
            Notify.Success(this, $"{JobName(jobId)} เข้าคิวแล้ว · ST1 จะส่งให้"); // แจ้งว่าเข้าคิวแล้ว ยังไม่ใช่ผลส่งข้อมูลเข้าเครื่อง
            await RefreshDataAsync(force: true); // อ่านสถานะล่าสุดกลับมาแสดง
            return; // ST3 ไม่เรียกส่งเข้าเครื่องจากปุ่มเริ่มนี้
        }

        await SendQueuedForJobAsync(jobId, resolved, $"เริ่มงาน {JobName(jobId)}"); // ST1 ขอสิทธิ์เครื่องแล้วส่งข้อมูลของ Job ที่กด
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
    private async Task<bool> ConfirmStartAsync(int jobId, ResolvedJobResponse resolved, MarkingPlan plan)
    {
        var (rows, _) = await _api!.GetMachineQueueAsync(); // อ่านคิวไว้สรุปให้ผู้ใช้ดูก่อนเริ่มงาน
        if (IsDisposed) return false; // หน้าถูกปิดแล้ว จึงไม่เปิดกล่องยืนยัน

        return Confirm.Ask(this, "ยืนยันเริ่มงาน", // ให้ผู้ใช้ยืนยันก่อนสร้างคิวเครื่อง
            BuildStartPreview(jobId, resolved, plan, rows)); // แสดงแผนเครื่องและคิวที่เกี่ยวข้องกับ Job นี้
    }

    /// <summary>ข้อความสรุปที่โชว์ในกล่องยืนยัน — แยกไว้ให้ทดสอบข้อความได้โดยไม่ต้องเปิดกล่อง</summary>
    private string BuildStartPreview(
        int jobId, ResolvedJobResponse resolved, MarkingPlan plan, List<MachineQueueRow> rows)
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
            body.AddRange(StepPreviewLines(step, resolved).Select(line => "      " + line));
            body.Add("");
        }

        return string.Join(Environment.NewLine, body).TrimEnd();
    }

    /// <summary>ข้อมูลที่จะถูกส่งเข้าเครื่องของขั้นตอนหนึ่ง เขียนให้อ่านจากที่ไกล ๆ ได้</summary>
    private static List<string> StepPreviewLines(string step, ResolvedJobResponse resolved)
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

        // รุ่นย่อยของโปรแกรมยังไม่รู้ตอนนี้ กล่องให้เลือกจะเด้งตอนส่งจริง
        lines.Add("(ถ้ามีรุ่นย่อยให้เลือก จะถามอีกครั้งตอนส่ง)");

        return lines;
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
    private static List<MachineQueueItem> QueueItemsFor(List<string> steps)
    {
        var rounds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // นับเลขรอบแยกตามเครื่อง
        var items = new List<MachineQueueItem>(); // เตรียมรายการคิวที่จะส่งไป Backend

        foreach (var step in steps) // สร้างคิวตามทุกขั้นในแผน
        {
            rounds[step] = rounds.TryGetValue(step, out int used) ? used + 1 : 1; // เครื่องเดิมปรากฏซ้ำให้เป็นรอบถัดไป เช่น MK รอบ 2
            items.Add(new MachineQueueItem { Machine = step, Round = rounds[step] }); // เก็บชื่อเครื่องพร้อมเลขรอบที่ต้องจอง
        }

        return items; // ส่งชุดคิวให้ EnqueueMachinesAsync บันทึก
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
        var (rows, _) = await _api!.GetMachineQueueAsync(); // อ่านคิวทั้งหมดเพื่อหาคิวของ Job นี้
        if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่ส่งงานต่อ

        // เครื่องละครั้งเดียว ไม่ใช่แถวละครั้ง
        //
        // งานที่เข้าเครื่องเดิมหลายรอบ (marking 22) จองไว้หลายแถวบนเครื่องเดียวกัน
        // ถ้าวนตามแถว พอรอบแรกส่งไม่ผ่านแล้วแถวถูกคืนเป็นรอคิว เครื่องจะว่างอีกครั้ง
        // การวนรอบถัดไปก็ขอเครื่องได้และได้แถวเดิมกลับมา กลายเป็นส่งซ้ำเข้าเครื่องจริง
        // สองครั้งจากการกดครั้งเดียว
        //
        // การกดเริ่มงานหนึ่งครั้งควรส่งได้อย่างมากเครื่องละหนึ่งรอบอยู่แล้ว รอบถัดไป
        // ของเครื่องเดิมต้องรอคนกดปุ่มหน้างานเสมอ
        var machines = rows // เตรียมรายชื่อเครื่องที่จะขอใช้ครั้งนี้
            .Where(r => r.PrintJobsId == jobId && r.State == "pending") // เอาเฉพาะคิวที่ยังรอของ Job นี้
            .OrderBy(r => r.Round) // ให้รอบแรกของเครื่องมาก่อนรอบถัดไป
            .Select(r => r.Machine) // ใช้ชื่อเครื่องเป็นตัวขอสิทธิ์
            .Distinct(StringComparer.OrdinalIgnoreCase) // หนึ่งการกดเริ่มขอเครื่องละหนึ่งครั้ง แม้มีหลายรอบ
            .ToList(); // เก็บชุดเครื่องที่จะวนส่งในครั้งนี้

        var lines = new List<Notify.ResultLine>(); // รวมผลของแต่ละเครื่องไว้แสดงพร้อมกัน
        bool anySent = false; // เริ่มจากยังไม่มีเครื่องไหนรับข้อมูลสำเร็จ

        foreach (var machine in machines) // ขอใช้และส่งทีละเครื่องในแผน
        {
            // ขอเฉพาะแถวของงานใบนี้ — กดเริ่มงานใบไหนต้องได้ใบนั้น ห้ามไปหยิบ
            // ใบอื่นที่บังเอิญรออยู่ในคิวเครื่องเดียวกันมาส่งแทน
            var (claim, claimError) = await _api.ClaimMachineAsync(machine, jobId); // ขอสิทธิ์เฉพาะคิวของ Job นี้ ไม่หยิบงานอื่น
            if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่ส่งงานต่อ

            if (claimError != null) // ขอสิทธิ์เครื่องแล้ว Backend แจ้งข้อผิดพลาด
            {
                lines.Add(Notify.Bad($"{machine}: {claimError}")); // เก็บเหตุที่ขอใช้เครื่องนี้ไม่ได้
                continue; // ข้ามเครื่องนี้แล้วลองเครื่องอื่นในแผน
            }

            if (claim?.Claimed == null) // ยังไม่ได้สิทธิ์ใช้เครื่อง เช่น เครื่องกำลังมีงาน
            {
                // เครื่องไม่ว่าง — ไม่ใช่ความผิดพลาด แถวยังรออยู่ในคิวเหมือนเดิม
                lines.Add(Notify.Careful($"{machine}: เครื่องไม่ว่าง เข้าคิวรอไว้แล้ว")); // เก็บคำเตือนว่าเครื่องยังไม่ว่าง
                continue; // ยังไม่มีสิทธิ์ จึงไม่ส่งข้อมูลทับเครื่องนี้
            }

            var claimed = claim.Claimed; // ใช้แถวคิวที่ Backend ให้สิทธิ์มา

            _sending = true; // กันรอบอ่านงานเข้ามาส่งซ้อนระหว่างส่งเครื่อง
            ShowSending($"กำลังส่งไปที่ {claimed.Machine}"); // แสดงว่าเครื่องใดกำลังรับข้อมูล
            StepSendResult sent; // เก็บทั้งผลส่งจริงและข้อความแจ้งผู้ใช้
            try
            {
                sent = await SendStepAsync(jobId, claimed.Machine, resolved, claimed.ProgramName); // ส่งตามเครื่องและโปรแกรมที่อยู่ในคิว
            }
            finally
            {
                _sending = false; // คืนสิทธิ์ให้รอบอ่านงานทำต่อได้
                if (!IsDisposed) ShowSending(null); // ปิดข้อความกำลังส่งเมื่อหน้าจอยังเปิดอยู่
            }

            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่สรุปผลบนจอ
            lines.AddRange(sent.Lines); // รวมผลส่งของเครื่องนี้เข้ารายการสรุป

            // ตัดสินจาก "ข้อมูลเข้าเครื่องแล้วไหม" ไม่ใช่จากว่ามีคำเตือนติดมาไหม
            //
            // เดิมนับทุกบรรทัดที่ไม่ใช่สีเขียวเป็นส่งไม่ผ่าน ผลคืองาน marking 12 ที่ใช้
            // หัวพ่นตัวเดียว พอหัวอีกตัวปิดอยู่จนสั่งหยุดไม่ได้ (ขึ้นเป็นคำเตือน) แถวคิว
            // ของ MK จะถูกคืนเป็นรอคิวทั้งที่เครื่องรับงานไปแล้วและกำลังพิมพ์อยู่
            // เครื่องจึงดูเหมือนว่าง งานใบถัดไปเลยแย่งเข้าไปเปลี่ยนโปรแกรมทับได้
            if (sent.Sent) // ดูผลรับข้อมูล ไม่ใช้สีคำเตือนมาตัดสินว่าส่งล้มเหลว
            {
                // บันทึกไม่ลงต้องฟ้อง ไม่ใช่ปล่อยเงียบ — แถวจะค้างเป็น "ยังไม่ได้ส่ง"
                // แล้วรอบ poll จะส่งซ้ำเข้าเครื่องทุก 5 วินาทีโดยไม่มีใครรู้ว่าทำไม
                anySent = true; // จำว่ามีอย่างน้อยหนึ่งเครื่องรับข้อมูลแล้ว

                var (marked, markError) = await _api.UpdateMachineQueueAsync(claimed.Id, sent: true); // บันทึกว่าคิวนี้ส่งแล้ว เพื่อไม่ให้รอบอ่านงานส่งซ้ำ
                if (!marked) // ข้อมูลเข้าเครื่องแล้ว แต่บันทึกผลคิวไม่ได้
                    lines.Add(Notify.Bad($"{claimed.Machine}: ส่งเข้าเครื่องแล้วแต่บันทึกคิวไม่ได้ · {markError}")); // แจ้งจุดที่เสี่ยงส่งซ้ำในรอบอ่านงานถัดไป
            }
            else
            {
                await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending"); // ส่งไม่สำเร็จ คืนแถวนี้กลับไปรอคิว
            }
        }

        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        // กดแล้วไม่มีเครื่องไหนรับงานไปได้เลย และงานนี้ก็ไม่เคยพิมพ์อะไรมาก่อน
        // ให้ล้างการจองทิ้ง ไม่เหลือร่องรอยไว้ในคิว
        //
        // เครื่องต่อไม่ติดไม่ใช่การเข้าคิว ไม่มีอะไรถูกส่งไปไหนทั้งนั้น การทิ้งแถวไว้
        // ทำให้งานไปกินที่ในคิวของเครื่องโดยไม่ได้ทำอะไร และหน้าจอก็ดูเหมือนกำลังรอคิว
        // ทั้งที่ความจริงต้องไปแก้ที่เครื่องแล้วกดใหม่
        //
        // งานที่พิมพ์ไปแล้วบางเครื่องไม่เข้าเงื่อนไขนี้ การจองของเครื่องที่เหลือต้องอยู่ต่อ
        // ไม่งั้นขั้นที่ยังไม่ได้ทำจะหายไปจากคิวโดยไม่มีทางเอากลับมา
        if (!anySent && !PrintedBefore(resolved)) // ไม่มีเครื่องรับข้อมูลครั้งนี้ และไม่มีประวัติส่งสำเร็จเดิม
        {
            await _api.ClearMachineQueueAsync(jobId); // ล้างคิวทั้ง Job ตามเงื่อนไขข้างบน รวมกรณีเครื่องไม่ว่างทั้งหมด
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ
        }

        if (lines.Count > 0) Notify.Result(this, title, lines); // แสดงผลรวมทั้งเครื่องที่ส่งได้และเครื่องที่มีปัญหา
        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านงานและคิวใหม่หลังจบรอบส่ง
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
    /// <param name="isFirstStep">
    /// ส่งไม่สำเร็จแล้วจะคืนสถานะเป็น Waiting ได้เฉพาะขั้นแรกเท่านั้น
    /// <para>
    /// ขั้นหลัง ๆ มีขั้นก่อนหน้าที่พ่นลงชิ้นงานไปแล้ว ถ้าตีกลับเป็น Waiting
    /// งานจะกลายเป็นกดเริ่มใหม่ได้ แล้วการกดเริ่มจะส่งขั้นแรกซ้ำ = พ่นซ้ำของจริง
    /// ปล่อยให้คงเป็น Working ไว้ คนหน้างานกดปุ่มส่งขั้นเดิมใหม่ได้เลย
    /// </para>
    /// </param>
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
        string? forcedProgram = null, bool isFirstStep = true)
    {
        await _api!.UpdateJobStatusAsync(jobId, "Process"); // ขอเปลี่ยนงานเป็น Process ก่อนเริ่มส่งข้อมูล

        if (step == "MK") // แยกทางส่งเครื่อง Inkjet MK
        {
            var mk = await JobSendService.SendMkAsync(resolved.Pattern); // ส่ง Pattern ไปยังหัว MK ที่ตั้งค่าไว้
            var lines = Notify.MkLines(mk.Machines); // แปลงผลแต่ละหัวเป็นข้อความสรุป

            if (lines.Count == 0) // ไม่มีผลของหัว MK ให้แสดง
                lines.Add(Notify.Careful("ไม่มีเครื่อง MK ที่ตั้งค่า IP ไว้")); // เตือนให้ตรวจ IP ของหัว MK

            bool ok = mk.Status == SendStatus.Ok; // ใช้ผลรวมการส่ง MK เป็นตัวตัดสินความสำเร็จ

            if (ok) // MK รับชุดข้อมูลสำเร็จแล้ว
                await _api.SaveSendStepAsync(jobId, "MK"); // บันทึกประวัติส่ง MK ของ Job นี้
            else if (isFirstStep) // ส่งไม่ผ่านและยังเป็นขั้นแรกของการส่งครั้งนี้
                await _api.UpdateJobStatusAsync(jobId, "Waiting"); // คืนงานไปรอเริ่มเมื่อขั้นแรกส่งไม่ผ่าน

            return new StepSendResult(ok, lines); // ส่งผล MK กลับให้ตัวคุมคิวบันทึกต่อ
        }

        int uvNumber = step == "UV1" ? 1 : 2; // เลือกชุดตั้งค่าของ UV1 หรือ UV2
        var uv = await JobSendService.SendUvAsync(this, uvNumber, resolved.UvJobData, forcedProgram); // ส่งข้อมูล UV โดยใช้โปรแกรมที่ฝากมา ถ้ามี

        if (uv.Status == SendStatus.Ok) // UV ส่งผ่านครบตามเงื่อนไขใน Service
        {
            await _api.SaveSendStepAsync(jobId, step, new // บันทึกประวัติส่ง UV พร้อมชื่อโปรแกรมที่ใช้จริง
            {
                requested = resolved.UvJobData.FirstOrDefault(r => r.Machine == step)?.ProgramName ?? "", // เก็บชื่อโปรแกรมต้นทางไว้เทียบกับที่เลือกใช้
                program = uv.ProgramFile, // เก็บโปรแกรมที่ส่งเข้าเครื่องจริง
                is_default = uv.UsedDefault, // เก็บว่าใช้โปรแกรมสำรองหรือไม่
            });

            return new StepSendResult(true, // แจ้งตัวคุมคิวว่า UV ส่งข้อมูลสำเร็จ
                [Notify.Ok($"{uv.MachineName} — ส่งสำเร็จ ({uv.ProgramFile}.uvdx)")]);
        }

        if (isFirstStep) await _api.UpdateJobStatusAsync(jobId, "Waiting"); // ส่ง UV ไม่ผ่านในขั้นแรก ให้คืนสถานะรอเริ่ม

        return new StepSendResult(false, uv.Status switch // แยกเหตุที่ส่ง UV ไม่สำเร็จให้จอแสดง
        {
            // ยกเลิกที่กล่องเลือกรุ่นย่อย ไม่ใช่ความผิดพลาด ไม่ต้องขึ้นกล่องสรุป
            SendStatus.Cancelled => [], // ผู้ใช้ยกเลิกเลือกโปรแกรม จึงไม่ต้องเด้งข้อความผิดพลาด
            SendStatus.Unreachable => // ต่อ UV ไม่ได้ ให้รายงานปลายทางที่ติดต่อ
                [Notify.Bad($"{uv.MachineName} — เชื่อมต่อไม่ได้ ({uv.Ip}:{uv.Port})")],
            _ => [Notify.Bad($"{uv.MachineName} — {uv.FailReason}")], // ปัญหาอื่นใช้เหตุที่ Service ส่งกลับมา
        });
    }

    // ── งานที่ไม่ต้องส่งคำสั่ง (marking 00) ─────────────────

    /// <summary>
    /// เริ่มงานที่ไม่มีขั้นตอนต้องส่งเข้าเครื่อง — เปลี่ยนสถานะอย่างเดียว
    /// ไม่แตะทั้ง Inkjet และ UV เพราะงานแบบนี้ทำด้วยมือทั้งหมด
    /// </summary>
    private async Task StartWithoutSendingAsync(int jobId, string? markingMethod)
    {
        if (!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ยืนยันเริ่มงานที่ไม่มีขั้นส่งเครื่อง
                $"{JobName(jobId)} — marking {Method(markingMethod)}\n\n"
                + "งานนี้ไม่มีขั้นตอนต้องส่งเข้าเครื่อง จะเปลี่ยนสถานะเป็นกำลังผลิตอย่างเดียว\n\n"
                + "ยืนยันหรือไม่?"))
            return; // ผู้ใช้ยกเลิก จึงไม่เปลี่ยนงานเป็น Process

        var (ok, err) = await _api!.UpdateJobStatusAsync(jobId, "Process"); // บันทึกว่างานเริ่มผลิตแล้ว โดยไม่ส่งคำสั่งเครื่อง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        if (ok) Notify.Success(this, $"เริ่มงาน {JobName(jobId)} แล้ว"); // แจ้งว่าบันทึกเริ่มงานได้แล้ว
        else Notify.ErrorModal(this, "เริ่มงานไม่สำเร็จ", err ?? "ไม่สามารถเปลี่ยนสถานะได้"); // แจ้งเหตุที่บันทึกเริ่มงานไม่ได้

        await RefreshDataAsync(force: true); // อ่านสถานะล่าสุดกลับมาแสดงในรายการ
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
        int machineStation = JobStationService.StationOf(step) ?? 0; // หา Station เจ้าของเครื่องที่กำลังฝากส่ง
        if (StationOwner(machineStation, jobId) is { } busyJob) // ตรวจว่ามี Job อื่นครอง Station นั้นอยู่หรือไม่
        {
            Notify.WarnModal(this, "สถานีไม่ว่าง", // แจ้งงานที่ต้องจัดการก่อนฝากส่ง
                $"ST{machineStation} มีงาน {JobLabel(busyJob)} อยู่\n\nต้องจบงานนั้นก่อนถึงจะเริ่มงานนี้ได้");
            return; // Station ยังติดงานอื่น จึงไม่ตั้งคำขอใหม่
        }

        int uvNumber = step == "UV1" ? 1 : 2; // เลือกชุดโปรแกรมของ UV1 หรือ UV2
        var uvRow = resolved.UvJobData.FirstOrDefault(r => r.Machine == step); // อ่านข้อมูล UV ของขั้นที่ต้องส่ง

        var pick = UvProgramResolver.Resolve( // หาโปรแกรมจริงหรือให้ผู้ใช้เลือกรุ่นย่อยก่อนฝากส่ง
            uvRow?.ProgramName, UvSettingsManager.GetDocumentFolder(uvNumber), this); // ค้นจากโฟลเดอร์ UV ของจอที่กำลังส่งคำขอ

        if (pick.Program == null) return;   // ผู้ใช้ปิดกล่องเลือกรุ่นย่อย

        var uvName = UvSettingsManager.Read( // อ่านชื่อเครื่องไว้แสดงตอนยืนยัน
            uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

        if (pick.IsDefault && // ถ้าต้องใช้โปรแกรมสำรอง ต้องให้ผู้ใช้รับทราบก่อน
            !UvProgramResolver.ConfirmDefault(uvRow?.ProgramName ?? "", uvName, this)) // ยืนยันว่าใช้โปรแกรมสำรองแทนชื่อจากงานได้
            return; // ผู้ใช้ไม่ยอมรับโปรแกรมสำรอง จึงไม่ฝากส่ง

        if (askFirst &&!Confirm.Ask(this, "ยืนยันเริ่มงาน", // ถามยืนยัน Job และโปรแกรมที่จะให้ ST1 ส่ง
                $"{JobName(jobId)} — marking {Method(resolved.PlanRouting?.MarkingMethod)}\n\n"
                + $"ส่งไป {step} ด้วยโปรแกรม {pick.Program}.uvdx\n"
                + "คำสั่งจะถูกส่งเข้าเครื่องโดยโปรแกรมที่ ST1\n\n"
                + "ยืนยันหรือไม่?"))
            return; // ผู้ใช้ยกเลิก จึงยังไม่สร้างคำขอ

        // ไม่ตั้งสถานะเป็น Working ตรงนี้โดยตั้งใจ — Working แปลว่า "ส่งเข้าเครื่องแล้ว"
        // แต่ตอนนี้ยังไม่มีใครแตะเครื่องเลย ยังไม่รู้ด้วยซ้ำว่า ST1 ต่อ UV ได้ไหม
        //
        // งานคงเป็น Waiting ไว้จนกว่า ST1 จะต่อเครื่องติดและส่งสำเร็จจริง
        // (SendStepAsync เป็นคนตั้ง Working และตีกลับเป็น Waiting เองถ้าขั้นแรกส่งไม่ผ่าน)
        var (ok, err) = await _api!.SetRemoteStartAsync( // บันทึกคำขอส่งไว้ใน Backend กลาง
            jobId, requested: true, pick.Program, step: step); // ฝากทั้ง Job, โปรแกรม และขั้น UV ที่ต้องการให้ ST1 ส่ง
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่แสดงผลต่อ

        if (!ok) // Backend รับคำขอไม่สำเร็จ
        {
            await _api.UpdateJobStatusAsync(jobId, "Waiting"); // คืนงานเป็น Waiting เมื่อฝากคำขอไม่ได้
            Notify.ErrorModal(this, "ส่งคำขอไม่สำเร็จ", err ?? "ไม่สามารถฝากคำขอไว้ที่ ST1 ได้"); // แจ้งเหตุที่ ST1 ยังไม่ได้รับคำขอ
            await RefreshDataAsync(force: true); // อ่านสถานะล่าสุดให้ตรงกับ Backend
            return; // ไม่มีคำขอสำเร็จ จึงไม่รอผลจาก ST1
        }

        // รอผลจริงตรงนี้ ไม่ปล่อยให้ไปโผล่ทีหลังเบื้องหลัง — คนที่กดยืนอยู่หน้าจอนี้
        // ต้องได้คำตอบจากการกดของตัวเอง เหมือนกดที่ ST1 ทุกประการ
        //
        // ระหว่างรอ ตั้ง _sending ไว้ให้รอบ poll หยุด จะได้ไม่มีกล่องจากเบื้องหลัง
        // มาเด้งซ้อนเรื่องเดียวกัน
        _sending = true; // กันการกดส่งซ้ำระหว่างรอ ST1
        ShowSending($"กำลังส่งไปที่ ST1 · {JobName(jobId)}"); // บอกผู้ใช้ว่ากำลังรอให้ ST1 ส่ง
        try
        {
            await ShowRemoteOutcomeAsync(jobId, step); // ติดตามประวัติส่งและข้อผิดพลาดที่ ST1 ฝากกลับมา
        }
        finally
        {
            _sending = false; // เปิดให้ใช้งานปุ่มส่งได้อีกครั้งหลังจบรอบรอ
            ShowSending(null); // ปิดข้อความรอ ST1
        }

        if (!IsDisposed) await RefreshDataAsync(force: true); // อ่านงานใหม่หลังจบการติดตามคำขอ
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
        var deadline = DateTime.UtcNow + RemoteOutcomeWait; // กำหนดเวลาสูงสุดที่หน้าจอจะรอผลรอบนี้

        while (DateTime.UtcNow < deadline) // ติดตามผลจนสำเร็จ มีปัญหา หรือครบเวลารอ
        {
            await Task.Delay(700); // เว้น 700 ms ก่อนอ่านผลครั้งถัดไป
            if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงหยุดรอผล

            var job = await _api!.GetJobByIdAsync(jobId); // อ่าน Job ล่าสุดรวมประวัติส่งและ remote_error
            if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ
            if (job == null) continue; // อ่าน Job ไม่ได้ในรอบนี้ ให้ลองใหม่ภายในเวลาที่เหลือ

            var failure = job.RemoteError?.Trim(); // อ่านเหตุที่ ST1 ส่งไม่สำเร็จ
            if (!string.IsNullOrEmpty(failure)) // มีข้อผิดพลาดที่ ST1 ฝากกลับมา
            {
                await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างธงคำขอหลังรับข้อผิดพลาดแล้ว
                if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

                Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Bad(failure)]); // แสดงเหตุที่ ST1 ส่งไม่ผ่านให้คนที่ ST3 เห็น
                return; // ได้ผลล้มเหลวแล้ว จึงหยุดรอรอบนี้
            }

            bool sent = job.Commands?.Any(c => c.Success && // ค้นประวัติส่งสำเร็จของขั้นที่ขอ
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true; // เทียบชื่อขั้นเพื่อไม่ใช้ผลของเครื่องอื่น
            if (!sent) continue; // ยังไม่มีประวัติสำเร็จของขั้นนี้ ให้รอต่อ

            var uvName = UvSettingsManager.Read(step == "UV1" ? "UV1_NAME" : "UV2_NAME", step); // ใช้ชื่อ UV ที่ตั้งไว้แสดงผล
            Notify.Result(this, $"เริ่มงาน {JobName(jobId)}", [Notify.Ok($"{uvName} — ส่งสำเร็จ")]); // แจ้งว่าพบประวัติส่งสำเร็จจาก ST1 แล้ว
            return; // ได้ผลสำเร็จแล้ว จึงหยุดรอ
        }

        // หมดเวลาแล้วยังเงียบ — ต้องแยกให้ออกว่า "ไม่มีใครหยิบไปทำเลย" กับ
        // "ST1 หยิบไปแล้วแต่ยังส่งไม่เสร็จ" เพราะสองอย่างนี้ต้องจัดการคนละแบบ
        var final = await _api!.GetJobByIdAsync(jobId); // ครบเวลารอแล้ว อ่านสถานะอีกครั้งก่อนตัดสิน
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        if (final?.RemoteStart == RemoteSending) // ST1 รับคำขอไปแล้วแต่ยังไม่จบการส่ง
        {
            Notify.WarnModal(this, "ST1 กำลังส่งอยู่", // แจ้งให้รอโดยไม่กดซ้ำ
                $"{JobName(jobId)}\n\n"
                + "ST1 รับคำขอไปแล้วและกำลังส่งเข้าเครื่อง แต่ใช้เวลานานกว่าปกติ\n\n"
                + "งานยังเดินอยู่ ไม่ต้องกดซ้ำ — รอผลอีกสักครู่");
            return; // คงคำขอที่ ST1 กำลังส่งไว้ ไม่คืนเป็น Waiting
        }

        // ไม่มีใครหยิบเลย = โปรแกรมที่ ST1 ไม่ได้เปิด หรือต่อ Backend ไม่ได้
        // ต้องตีงานกลับเป็น Waiting ไม่งั้นค้างเป็น Working ตลอดกาลทั้งที่ยังไม่ได้พิมพ์
        // ปิดโปรแกรมเปิดใหม่ก็ยังเห็นเป็น Working และกดเริ่มใหม่ไม่ได้
        await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอที่ยังไม่มีสถานะกำลังส่งเมื่อหมดเวลา
        await _api.UpdateJobStatusAsync(jobId, "Waiting"); // คืนงานให้รอเริ่มใหม่
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่อัปเดตจอต่อ

        Notify.WarnModal(this, "ST1 ไม่รับคำขอ", // แจ้งให้ตรวจการเปิดโปรแกรมและ Backend ที่ ST1
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
        if (_api == null || StationService.IsSt3 || _sending) return; // ให้ ST1 ส่งคิวเท่านั้น และไม่แทรกตอนมีการส่งอื่น

        var (rows, error) = await _api.GetMachineQueueAsync(); // อ่านสถานะคิวจาก Backend กลาง
        if (error != null || IsDisposed) return; // อ่านคิวไม่ได้หรือปิดหน้าแล้ว จึงไม่ส่งต่อ

        // ส่งเฉพาะแถวที่ "ถึงคิวแล้วแต่ยังไม่ได้ส่ง" เท่านั้น และไม่หยิบคิวเองเด็ดขาด
        //
        // แถวจะมาอยู่ในสภาพนี้ได้จากการกดของคนเท่านั้น — กดเริ่มงาน หรือกดปุ่ม
        // หน้างานปล่อยเครื่องแล้ว backend ยกเครื่องให้คิวถัดไป รอบนี้จึงเป็นแค่
        // "มือที่ไปส่งแทน ST3" ไม่ใช่ตัวตัดสินว่างานไหนได้เข้าเครื่อง
        //
        // เดิมตรงนี้ไล่หยิบคิวเองทุก 5 วิ ผลคืองานที่ส่งไม่ผ่านแล้วถูกคืนเป็นรอคิว
        // จะถูกหยิบมายิงใหม่ไม่มีวันจบ และงานใบอื่นที่ไม่มีใครกดก็ถูกส่งออกไปด้วย
        var ready = rows // เตรียมคิวที่พร้อมให้ส่งในรอบนี้
            .Where(r => r.State == "active" && r.SentAt == null) // เอาเฉพาะคิวที่ได้สิทธิ์แล้วและยังไม่บันทึกว่าส่งสำเร็จ
            .OrderBy(r => r.Id) // ไล่คิวที่ได้สิทธิ์ตามเลขรายการ
            .ToList(); // เก็บชุดคิวสำหรับรอบอ่านงานนี้

        foreach (var row in ready) // ส่งแต่ละคิวที่ได้สิทธิ์แล้ว
        {
            await SendClaimedAsync(row); // อ่านรายละเอียด Job แล้วส่งเข้าเครื่องตามแถวคิว
            if (IsDisposed) return; // หน้าถูกปิดแล้ว หยุดส่งรายการถัดไป
        }
    }

    /// <summary>
    /// ส่งงานที่หยิบมาได้เข้าเครื่อง แล้วรายงานผล — ส่งไม่ผ่านคืนแถวกลับเข้าคิว
    /// </summary>
    private async Task SendClaimedAsync(MachineQueueRow claimed)
    {
        var resolved = await _api!.GetResolvedJobAsync(claimed.PrintJobsId); // อ่านรายละเอียดล่าสุดของ Job ที่อยู่ในคิว
        if (resolved == null || IsDisposed) // ไม่มีข้อมูล Job หรือหน้าถูกปิดแล้ว
        {
            // อ่านงานไม่ได้ อย่าถือเครื่องค้างไว้
            if (_api != null) await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending"); // คืนคิวไปรอ เพื่อไม่ถือเครื่องไว้ทั้งที่ยังส่งไม่ได้
            return; // ไม่มีข้อมูลพร้อมส่ง จึงหยุดที่คิวนี้
        }

        _sending = true; // กันการส่งซ้อนขณะทำคิวนี้
        ShowSending($"กำลังส่งไปที่ {claimed.Machine} · {JobName(claimed.PrintJobsId)}"); // แสดงชื่อเครื่องและงานที่กำลังส่ง
        StepSendResult sent; // เก็บผลส่งสำหรับอัปเดตคิว
        try
        {
            sent = await SendStepAsync( // ส่งข้อมูลของขั้นที่แถวคิวระบุ
                claimed.PrintJobsId, claimed.Machine, resolved, claimed.ProgramName); // ใช้ Job, เครื่อง และโปรแกรมจากคิวที่ได้รับสิทธิ์
        }
        finally
        {
            _sending = false; // เปิดให้รอบอ่านงานทำงานต่อได้
            if (!IsDisposed) ShowSending(null); // ปิดข้อความกำลังส่ง
        }

        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่แสดงผลต่อ

        // ส่งไม่ผ่าน = คืนแถวกลับไปรอคิว แล้วจบตรงนั้น ไม่มีใครมาลองใหม่ให้เอง
        // ต้องมีคนกดเริ่มงานใบนั้นอีกครั้ง ถึงจะยิงซ้ำ
        if (sent.Sent) // เครื่องรับข้อมูลสำเร็จแล้ว
        {
            // เหมือนกับตอนกดเริ่มงาน — บันทึกไม่ลงแปลว่ารอบหน้าจะส่งซ้ำ ต้องให้เห็น
            var (marked, markError) = await _api.UpdateMachineQueueAsync(claimed.Id, sent: true); // ทำเครื่องหมายว่าคิวส่งแล้ว เพื่อกันส่งซ้ำในรอบถัดไป
            if (!marked) // บันทึกผลคิวไม่สำเร็จทั้งที่เครื่องรับข้อมูลแล้ว
                sent.Lines.Add(Notify.Bad($"{claimed.Machine}: ส่งเข้าเครื่องแล้วแต่บันทึกคิวไม่ได้ · {markError}")); // แจ้งความเสี่ยงที่คิวเดิมจะถูกส่งซ้ำ
        }
        else
        {
            await _api.UpdateMachineQueueAsync(claimed.Id, state: "pending"); // ส่งไม่ผ่าน คืนคิวไปรอ ไม่ลองยิงซ้ำเองทันที
        }

        if (sent.Lines.Count > 0) // มีข้อความผลส่งให้ผู้ใช้ดู
            Notify.Result(this, $"ส่ง {claimed.Machine} · {JobName(claimed.PrintJobsId)}", sent.Lines); // แสดงผลส่งของ Job และเครื่องนี้ที่จอ ST1
    }

    private async Task ProcessRemoteStartsAsync()
    {
        // ST3 เป็นฝ่ายฝาก ไม่ใช่ฝ่ายส่ง · ระหว่างที่คนที่ ST1 กดส่งเองอยู่ก็ไม่แทรก
        if (_api == null || StationService.IsSt3 || _sending) return; // ให้ ST1 รับคำขอ และไม่แทรกการส่งที่กำลังทำอยู่

        var pending = _allJobs // เตรียม Job ที่มีคำขอจาก ST3
            .Where(j => j.RemoteStart == RemotePending && !_remoteInFlight.Contains(j.Id)) // เลือกธงรอส่งที่โปรแกรมนี้ยังไม่ได้กำลังจัดการ
            .Select(j => j.Id) // ใช้เลข Job ไปอ่านข้อมูลล่าสุดอีกที
            .ToList(); // เก็บชุดคำขอสำหรับรอบนี้

        foreach (var jobId in pending) // จัดการคำขอทีละ Job
        {
            _remoteInFlight.Add(jobId); // จำว่าโปรแกรมนี้กำลังจัดการ Job นี้อยู่
            _sending = true; // กันงานส่งอื่นเข้ามาซ้อน
            try
            {
                await RunRemoteStartAsync(jobId); // ตรวจคำขอแล้วส่งขั้นที่ ST3 ระบุ
            }
            finally
            {
                _sending = false; // เปิดให้ทำงานส่งอื่นต่อได้
                _remoteInFlight.Remove(jobId); // เอา Job ออกจากชุดที่โปรแกรมนี้กำลังจัดการ
            }

            if (IsDisposed) return; // หน้าถูกปิดแล้ว หยุดรับคำขอถัดไป
        }
    }

    /// <summary>
    /// ส่งคำขอหนึ่งใบ — ล้างธงทุกเส้นทางที่ออกจากเมธอดนี้ ไม่ว่าจะส่งได้หรือไม่
    /// ธงที่ค้างคือสาเหตุเดียวที่จะทำให้รอบถัดไปส่งซ้ำ
    /// </summary>
    private async Task RunRemoteStartAsync(int jobId)
    {
        var resolved = await _api!.GetResolvedJobAsync(jobId); // อ่าน Job ล่าสุดก่อนรับคำขอจาก ST3
        if (resolved == null) return;   // อ่านไม่ได้ = คงธงไว้ให้รอบหน้าลองใหม่

        // งานต้องยังไม่ถูกยกเลิกหรือจบไปแล้ว ถ้าไม่ดักตรงนี้ ธงที่ค้างจะสั่งพิมพ์
        // งานที่ถูกยกเลิกไปแล้ว
        //
        // รับทั้ง Waiting และ Process — คำขอจาก ST3 มาถึงตอนงานยังเป็น Waiting
        // เพราะ Working จะถูกตั้งก็ต่อเมื่อต่อเครื่องติดและส่งจริงแล้วเท่านั้น
        var status = resolved.Job.Status; // ตรวจสถานะจริงของ Job จาก Backend
        bool live = string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase) // รับคำขอของงานที่ยังรอเริ่ม
                 || string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase); // หรือของงานที่กำลังผลิตและมีขั้นถัดไป

        if (!live) // งานถูกจบหรือยกเลิกไปแล้ว
        {
            await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอที่ไม่ควรส่งต่อแล้ว
            return; // ไม่ส่งเครื่องให้ Job ที่ไม่ได้กำลังทำงาน
        }

        var plan = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod); // อ่านแผนเครื่องที่ Job นี้ใช้จริง

        // ขั้นตอนที่จะส่งมาจากตัวคำขอ ไม่ใช่การเดาเอาว่าเป็นขั้นแรกเสมอ
        //
        // งานหนึ่งใบมีได้หลายขั้น เช่น marking 32 คือ MK แล้วต่อ UV2 ถ้าหยิบขั้นแรก
        // ตายตัวแบบเดิม คำขอที่ ST3 ฝากไว้ว่าขอ UV2 จะกลายเป็นส่ง MK ซ้ำ
        //
        // คำขอที่ไม่ได้ระบุขั้นถือว่าเป็นขั้นแรก — ใบที่ค้างอยู่ตอนอัปเดตโปรแกรม
        // จึงยังทำงานถูกเหมือนเดิม
        var requested = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteStep; // อ่านชื่อขั้นที่ ST3 ฝากมา
        var step = string.IsNullOrWhiteSpace(requested) // ดูว่าคำขอระบุขั้นมาหรือไม่
            ? plan.Steps.FirstOrDefault() // คำขอเก่าที่ไม่ระบุขั้น ใช้ขั้นแรกในแผน
            : plan.Steps.FirstOrDefault(x => // คำขอที่ระบุขั้นต้องหาขั้นนั้นในแผน
                string.Equals(x, requested.Trim(), StringComparison.OrdinalIgnoreCase)); // เทียบชื่อเครื่องโดยไม่สนตัวพิมพ์ใหญ่เล็ก

        // ขั้นที่ขอมาไม่มีอยู่ในแผนของงานนี้ = คำขอใช้ไม่ได้ ทิ้งไปพร้อมบอกสาเหตุ
        if (step == null) // ไม่พบขั้นที่ขอในแผนของ Job
        {
            await _api.SetRemoteStartAsync(jobId, requested: false, // ล้างคำขอที่ส่งต่อไม่ได้ พร้อมฝากเหตุถ้ามี
                failure: string.IsNullOrWhiteSpace(requested)
                    ? null
                    : $"งานนี้ไม่มีขั้นตอน {requested.Trim()} ให้ส่ง");
            return; // ไม่มีขั้นที่ถูกต้อง จึงไม่ส่งเครื่อง
        }

        // อ่านสดจาก backend แล้วเช็คว่าขั้นตอนนี้ส่งสำเร็จไปแล้วหรือยัง —
        // ด่านสุดท้ายที่กันการส่งซ้ำ ถ้าธงค้างเพราะเหตุอื่น เช่นโปรแกรมถูกปิดกลางคัน
        if (resolved.Commands?.Any(c => c.Success && // ตรวจประวัติที่อ่านสดว่าขั้นนี้ส่งสำเร็จแล้วหรือไม่
                string.Equals(c.Command, step, StringComparison.OrdinalIgnoreCase)) == true) // เทียบกับขั้นที่ขอ เพื่อกันการส่งขั้นเดิมซ้ำ
        {
            await _api.SetRemoteStartAsync(jobId, requested: false); // ล้างคำขอของขั้นที่มีประวัติสำเร็จแล้ว
            return; // เคยส่งขั้นนี้แล้ว จึงไม่ส่งซ้ำ
        }

        // เครื่องปลายทางไม่ว่าง — คงธงไว้ให้รอบ poll ถัดไปลองใหม่ ไม่ต้องรบกวนใคร
        int machineStation = JobStationService.StationOf(step) ?? 0; // หา Station เจ้าของเครื่องในขั้นที่ขอ
        if (StationOwner(machineStation, jobId) != null) return; // ยังมี Job อื่นครอง Station ให้คงคำขอไว้รอรอบหน้า

        var program = _allJobs.FirstOrDefault(j => j.Id == jobId)?.RemoteProgram; // ใช้ชื่อโปรแกรมที่ ST3 เลือกฝากไว้

        // จองไว้ก่อนลงมือ — ตั้งแต่บรรทัดนี้ไป ST3 จะเห็นว่า "กำลังส่งอยู่" ไม่ใช่
        // "ไม่มีใครรับ" จึงไม่ตีงานกลับเป็น Waiting ทับงานที่เครื่องกำลังรับข้อมูล
        await _api.ClaimRemoteStartAsync(jobId, program, step); // เปลี่ยนธงเป็นกำลังส่ง เพื่อให้ ST3 รู้ว่ารับแล้ว
        if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่ส่งเครื่องต่อ

        // ขั้นแรกหรือไม่ ตัดสินจากแผนของงาน ไม่ใช่จากว่าใครเป็นคนขอ
        var lines = (await SendStepAsync( // ส่งขั้นที่ขอผ่านทางส่ง MK / UV ร่วมกัน
            jobId, step, resolved, program, isFirstStep: plan.Steps.IndexOf(step) == 0)).Lines; // ระบุว่าเป็นขั้นแรกหรือไม่ เพื่อเลือกการคืน Waiting เมื่อผิดพลาด

        // ล้มเหลวแล้วฝากสาเหตุกลับไปให้ ST3 ด้วย — คนที่กดเริ่มงานอยู่ที่นั่น
        // ไม่ได้เห็นจอนี้ ถ้าไม่ฝากไว้เขาจะเห็นแค่งานเด้งกลับเป็น Waiting เฉย ๆ
        bool failed = lines.Any(l => l.Kind == Notify.ResultKind.Error); // ดูว่าผลส่งมีข้อความผิดพลาดหรือไม่
        var failure = failed // เตรียมเหตุที่ต้องส่งกลับให้ ST3
            ? string.Join(" · ", lines.Where(l => l.Kind == Notify.ResultKind.Error).Select(l => l.Text)) // รวมเหตุจากเครื่องเป็นข้อความเดียว
            : null; // ไม่มีข้อผิดพลาดให้ฝากกลับ

        await _api.SetRemoteStartAsync(jobId, requested: false, failure: failure); // ล้างธงคำขอและเก็บเหตุที่ส่งไม่สำเร็จ

        if (IsDisposed || lines.Count == 0) return; // ไม่มีจอหรือไม่มีข้อความให้รายงาน จึงจบการรับคำขอ

        // ต้องเป็นข้อความลอย ไม่ใช่กล่องที่ต้องกดปิด — จอ ST1 ไม่มีคนเฝ้าอยู่
        // กล่อง modal จะค้างหน้าจอและหยุดรอบ poll ไปจนกว่าจะมีคนมากด
        var text = $"{JobName(jobId)} — {lines[0].Text} (คำขอจาก ST3)"; // ระบุ Job และผลส่งว่าเป็นคำขอจาก ST3

        if (failed) Notify.Warn(this, text); // แจ้งเตือนแบบไม่ค้างรอคนปิดที่ ST1
        else Notify.Success(this, text); // แจ้งส่งสำเร็จแบบไม่หยุดรอบอ่านงาน
    }

    // ── ST3 รับผลกลับจาก ST1 ────────────────────────────────

    /// <summary>กล่องแจ้งผลเปิดค้างอยู่ — กันไม่ให้รอบ poll ถัดไปเปิดซ้อนขึ้นมาอีกใบ</summary>
    private bool _showingRemoteError;

    /// <summary>
    /// ST3 หยิบสาเหตุที่ ST1 ส่งไม่สำเร็จมาแสดงที่จอตัวเอง แล้วล้างทิ้งทันที
    ///
    /// ล้างทันทีที่แสดง จึงไม่ต้องจำว่าเคยแสดงใบไหนไปแล้ว และไม่เด้งซ้ำตอนเปิดโปรแกรมใหม่
    /// ตัวงานเองถูก SendStepAsync ตีกลับเป็น Waiting ไว้แล้ว กดเริ่มใหม่ได้เลย
    /// </summary>
    private async Task ShowRemoteErrorsAsync()
    {
        if (_api == null || !StationService.IsSt3 || _showingRemoteError) return; // แสดงเหตุส่งไม่ผ่านที่ ST3 เท่านั้น และไม่เปิดกล่องซ้อน

        var failed = _allJobs.FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.RemoteError)); // หางานที่ ST1 ฝากเหตุส่งไม่สำเร็จไว้
        if (failed == null) return; // ไม่มีเหตุผิดพลาดค้างให้แสดง

        var message = failed.RemoteError!; // เก็บข้อความไว้ก่อนล้างค่าที่ Backend

        // ล้างก่อนเปิดกล่อง — ระหว่างกล่องเปิดค้าง รอบ poll ยังเดินอยู่หลังกล่อง
        await _api.SetRemoteStartAsync(failed.Id, requested: false); // ล้างธงและข้อความค้างก่อนเปิดกล่อง เพื่อไม่แจ้งซ้ำ
        if (IsDisposed) return; // หน้าถูกปิดแล้ว ไม่เปิดกล่องแจ้งเตือน

        _showingRemoteError = true; // กันรอบอ่านงานเปิดข้อความผิดพลาดซ้อน
        try
        {
            Notify.ErrorModal(this, "ST1 ส่งงานไม่สำเร็จ", // แสดงเหตุที่ ST1 ส่งไม่ได้ให้คนที่ ST3 ทราบ
                $"{JobLabel(failed)}\n\n{message}\n\n"
                + "งานถูกตีกลับเป็นรอเริ่ม กดเริ่มงานใหม่ได้");
        }
        finally
        {
            _showingRemoteError = false; // เปิดให้แจ้งปัญหางานถัดไปได้เมื่อปิดกล่องแล้ว
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
        if (_api == null) return; // ยังไม่มีตัวเชื่อม Backend จึงจบงานไม่ได้

        // อ่านสดก่อนตัดสินใจ — ตารางอาจค้างได้ถึง 5 วิตามรอบ poll
        var resolved = await LoadJobAsync(jobId, $"กำลังโหลดข้อมูล · {JobName(jobId)}"); // อ่านประวัติส่งล่าสุดก่อนตัดสินว่าจบงานได้หรือไม่
        if (IsDisposed) return; // หน้าถูกปิดแล้ว จึงไม่ดำเนินการต่อ
        if (resolved == null) // ไม่มีรายละเอียดงานให้ตรวจ
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่สามารถโหลดข้อมูล {JobName(jobId)} ได้"); // แจ้งว่าอ่านงานที่จะจบไม่สำเร็จ
            return; // ยังตรวจงานไม่ได้ จึงไม่เปลี่ยนเป็น Success
        }

        var method = resolved.PlanRouting?.MarkingMethod; // อ่านวิธีพิมพ์เพื่อหาว่างานต้องจบที่ Station ใด
        if (!MarkingMethodService.CanCompleteAt(StationService.Current, method)) // ตรวจสิทธิ์จบงานของ Station ปัจจุบัน
        {
            Notify.WarnModal(this, "จบงานที่สถานีนี้ไม่ได้", // แจ้งให้ไปจบงานที่ Station ที่รับผิดชอบ
                $"{JobName(jobId)} — marking {Method(method)}\n\n"
                + "งาน marking 10 / 11 / 12 จบได้ที่ ST3 เท่านั้น");
            return; // ผิด Station จึงไม่บันทึกจบงาน
        }

        var steps = CheckSteps(method, resolved.Commands); // เทียบแผนกับประวัติส่ง รวมจำนวนรอบของเครื่องเดิม

        // งานยังไม่ครบก็จบได้ ถ้าผู้ใช้ยืนยันเอง — บันทึกไว้ว่าเป็นการจบด้วยมือ
        bool manual = !steps.Complete; // ถ้าประวัติยังไม่ครบ ต้องจบแบบยืนยันด้วยมือ
        if (manual) // แยกไปเตือนขั้นที่ยังขาด
        {
            var list = string.Join(", ", steps.Missing); // รวมชื่อขั้นที่ยังไม่มีประวัติครบ
            if (!Confirm.Ask(this, "งานยังส่งไม่ครบ", // ให้ยืนยันว่าจะจบทั้งที่ส่งยังไม่ครบ
                    $"{JobName(jobId)} ยังส่งไม่ครบ\n\nยังขาด: {list}\n\n" +
                    "ยืนยันจบงานทั้งที่ยังส่งไม่ครบหรือไม่?"))
                return; // ผู้ใช้ไม่ยืนยัน จึงคงงานไว้ให้ทำต่อ
        }
        else if (!Confirm.Ask(this, "ยืนยันจบงาน", // ส่งครบแล้วก็ยังต้องยืนยันจบ Job
                     $"จบงาน {JobName(jobId)}\n\nยืนยันหรือไม่?"))
        {
            return; // ผู้ใช้ยกเลิกการจบงาน จึงเก็บสถานะเดิม
        }

        if (manual) // เป็นการจบทั้งที่ประวัติส่งยังไม่ครบ
            await _api.SaveSendStepAsync(jobId, "MANUAL_COMPLETE"); // บันทึกร่องรอยว่าผู้ใช้ยืนยันจบด้วยมือ

        // จบงานแล้วคิวที่เหลือของงานนี้ไม่มีความหมาย ล้างทิ้งไม่ให้ไปกันเครื่องคนอื่น
        await _api.ClearMachineQueueAsync(jobId); // ล้างคิวของงานที่กำลังจบเพื่อไม่กันเครื่องไว้

        var (ok, err) = await _api.UpdateJobStatusAsync(jobId, "Success"); // บันทึกสถานะจบงานใน Backend
        if (ok) // Backend ยืนยันว่าบันทึก Success ได้แล้ว
        {
            Notify.Success(this, manual // แจ้งผลโดยแยกจบตามปกติกับจบด้วยมือ
                ? $"{JobName(jobId)} จบงานแล้ว (ยืนยันด้วยมือ)"
                : $"{JobName(jobId)} จบงานแล้ว");
            await RefreshDataAsync(); // อ่านรายการใหม่ให้งานที่จบออกจาก List
        }
        else
        {
            Notify.ErrorModal(this, "จบงานไม่สำเร็จ", err ?? "ไม่สามารถบันทึกสถานะจบงานได้"); // แจ้งเหตุที่ยังบันทึกจบงานไม่ได้
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
        var need = MarkingMethodService.MissingSteps(markingMethod, commands); // ตรวจขั้นที่ยังขาด โดยนับประวัติให้ครบจำนวนรอบ
        return new StepStatus(need.Count == 0, need); // ส่งทั้งผลว่าครบหรือยังและชื่อขั้นที่ขาดให้ปุ่มจบงาน
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
        string? requestedStep; // เก็บขั้นที่ผู้ใช้ขอให้ ST1 ส่งจากหน้า Detail

        using (var dlg = new OrderDetailDialog()) // เปิดหน้าต่างรายละเอียดเฉพาะ Job นี้
        {
            // ชื่อเดียวกับหัวที่อยู่ในหน้า ไม่ประกอบเอง ไม่งั้นสองที่จะขึ้นคนละเลข
            dlg.TitleText = $"{OrderDetailUserControl.JobTitle(resolved.Job)} — Order Detail"; // ตั้งหัวหน้าต่างให้บอกงานที่กำลังดู
            dlg.Text = dlg.TitleText; // ใช้ชื่อเดียวกันที่แถบหน้าต่าง
            dlg.LoadDetail(resolved, _api); // เติมรายละเอียดงานและตัวเชื่อม Backend ให้หน้ารายละเอียด
            dlg.ShowDialog(this); // รอให้ผู้ใช้ดูรายละเอียดหรือกดขอส่งแล้วปิดหน้าต่าง
            requestedStep = dlg.RemoteStartStep; // รับขั้นที่ผู้ใช้กดขอให้ ST1 ส่ง
        }

        if (requestedStep == null || _api == null || IsDisposed) return; // ปิดเฉย ๆ หรือหน้าหลักไม่พร้อม จึงไม่ฝากคำขอ

        await RequestRemoteStartFromDetailAsync(resolved.Job.Id); // อ่านข้อมูลสดและตรวจขั้นอีกครั้งก่อนฝากส่ง
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
        var resolved = await LoadJobAsync(jobId, $"กำลังตรวจสอบงาน · {JobName(jobId)}"); // อ่านประวัติใหม่ เผื่อมีคนส่งไปแล้วระหว่างเปิด Detail
        if (resolved == null || IsDisposed) return; // อ่านงานไม่ได้หรือปิดหน้าแล้ว จึงไม่สร้างคำขอ

        var steps = MarkingMethodService.Resolve(resolved.PlanRouting?.MarkingMethod).Steps; // อ่านขั้นที่งานนี้ต้องส่งจากวิธีพิมพ์
        int next = steps.FindIndex(step => !SentAlready(resolved, step)); // หาขั้นแรกที่ยังไม่มีประวัติส่งสำเร็จ

        // -1 = ส่งครบแล้ว · 0 = ยังไม่ได้เริ่มเลย ซึ่งเป็นหน้าที่ของปุ่มเริ่มงาน ไม่ใช่ปุ่มนี้
        if (next <= 0) // ไม่มีขั้นค้าง หรือยังเป็นขั้นแรกที่ต้องเริ่มจาก List
        {
            Notify.WarnModal(this, "ไม่มีขั้นที่ต้องส่ง", // แจ้งว่าไม่มีขั้นถัดไปให้ฝากส่งจาก Detail
                $"{JobName(jobId)} ไม่มีขั้นถัดไปที่รอ ST1 ส่งแล้ว\n\n"
                + "อาจมีคนกดปุ่มหน้างานไปก่อนหน้านี้");
            return; // ไม่ส่งคำขอซ้ำหรือข้ามขั้นเริ่มงาน
        }

        await RequestRemoteStartAsync(jobId, steps[next], resolved, askFirst: true); // ให้เลือกโปรแกรมและยืนยันก่อนฝากขั้นถัดไปให้ ST1
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
        // งานใบเดียวแตะได้หลายเครื่อง บางเครื่องเดินแล้วบางเครื่องยังรอคิวอยู่
        //
        // คำเดียวในช่องนี้บอกได้แค่ว่า "เดินอยู่" ซึ่งจริงแต่ไม่ครบ พอเห็นสองใบขึ้น
        // Working พร้อมกันจะดูเหมือนผิด ทั้งที่ใบหนึ่งทำไปแล้วครึ่งเดียว
        var waiting = PendingMachines(job.Id);
        if (waiting.Count > 0)
        {
            // แยก "ต่อแถวรอเครื่องที่ไม่ว่าง" ออกจาก "เครื่องว่างแต่ยังไม่ได้ส่ง"
            //
            // สองอย่างนี้หน้าตาเหมือนกันในตารางคิว (แถวรอเหมือนกัน) แต่คนละเรื่องกัน
            // สำหรับคนหน้างาน — อย่างแรกต้องรอให้เขากดปุ่มปล่อยเครื่อง อย่างที่สอง
            // ไม่มีอะไรมาขวางเลย แค่ส่งไม่ผ่าน เช่นเครื่องยังไม่ได้เปิด ต้องกดใหม่
            //
            // ถ้าใช้คำว่า Queued กับทั้งสองอย่าง คนจะนั่งรอเครื่องที่ว่างอยู่แล้ว
            var blocked = waiting.Where(BusyNow).ToList();
            var idle = waiting.Where(m => !BusyNow(m)).ToList();

            if (blocked.Count > 0)
            {
                statusLabel = string.Equals(job.Status, "Process", StringComparison.OrdinalIgnoreCase)
                    ? $"{statusLabel} · wait {string.Join(" ", blocked)}"
                    : $"Queued {string.Join(" ", blocked)}";
            }

            if (idle.Count > 0)
                statusLabel = $"Not sent {string.Join(" ", idle)}";
        }

        // งานที่พ่น plate เสร็จแล้วและกำลังรอเอาไปติด shim นอกไลน์
        //
        // ช่วงนี้กินเวลานานและชิ้นงานไม่ได้อยู่ในไลน์ คนหน้าจอต้องแยกออกจากงานที่
        // เครื่องกำลังพ่นอยู่จริง ไม่งั้นเห็นแค่ว่ากำลังทำ แล้วนึกว่าเครื่องเดินอยู่
        if (WaitingForShim(job)) statusLabel = "Waiting shim";

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
    /// งานนี้พ่นรอบแรกเสร็จแล้วและกำลังรอติด shim อยู่นอกไลน์ไหม
    ///
    /// <para>
    /// ใช้กับงานที่เข้าเครื่องเดิมสองรอบ (marking 22) รู้ได้จากแถวคิวที่ยังไม่ปล่อย
    /// ของรอบที่สองขึ้นไป — แถวของรอบแรกจะถูกปิดไปแล้วตอนคนกดปุ่มหน้างาน
    /// </para>
    /// </summary>
    private bool WaitingForShim(PrintJob job) =>
        _queueRows.Any(r => r.PrintJobsId == job.Id && r.Round >= 2 && r.State == "active");

    /// <summary>งานนี้เคยส่งเข้าเครื่องสำเร็จมาก่อนไหม — ดูจากประวัติคำสั่งที่บันทึกไว้</summary>
    private static bool PrintedBefore(ResolvedJobResponse resolved) =>
        resolved.Commands?.Any(c => c.Success) == true;

    /// <summary>เครื่องนี้มีงานถืออยู่ตอนนี้ไหม — ไม่มี แปลว่าว่าง ไม่มีอะไรขวางคิว</summary>
    private bool BusyNow(string machine) =>
        _queueRows.Any(r => r.State == "active"
            && string.Equals(r.Machine, machine, StringComparison.OrdinalIgnoreCase));

    /// <summary>เครื่องที่งานใบนี้จองไว้แล้วแต่ยังไม่ถึงคิว เรียงตามลำดับที่จะได้เครื่อง</summary>
    private List<string> PendingMachines(int jobId) =>
        _queueRows
            .Where(r => r.PrintJobsId == jobId && r.State == "pending")
            .OrderBy(r => r.Id)
            .Select(r => r.Machine)
            .ToList();

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
