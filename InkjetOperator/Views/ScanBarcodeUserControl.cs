using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class ScanBarcodeUserControl : UserControl
{
    private ApiClient? _api;
    private SqliteDataService? _sqlite;

    /// <summary>
    /// บาร์โค้ดที่ดึงข้อมูลขึ้นมาโชว์แล้ว — ใช้เทียบกับสิ่งที่อยู่ในช่องตอนกด OK
    /// เพื่อกันไม่ให้ลงทะเบียนด้วยข้อมูลของ lot ก่อนหน้าที่ค้างอยู่บนจอ
    /// </summary>
    private string? _loadedBarcode; // จำ Barcode ที่โหลดข้อมูลแล้ว

    /// <summary>
    /// ชื่อลูกค้าจาก inkjet_data.customer ของ lot ที่สแกน
    ///
    /// หน้านี้ไม่มีช่องให้เห็นและไม่ให้แก้ — แค่ติดไปกับงานตอนลงทะเบียน
    /// แล้วไปโผล่ที่หน้า Order Detail ที่เดียว
    /// </summary>
    private string? _customerName; // เก็บชื่อลูกค้าไว้ส่งตอนสร้าง Job

    /// <summary>
    /// หยุดพิมพ์นานเท่านี้ (มิลลิวินาที) แล้วโปรแกรมจะดึงข้อมูลให้เอง
    ///
    /// ยาวพอให้พิมพ์เลขทีละตัวด้วยนิ้วบนทัชสกรีนไม่โดนขัดจังหวะ และสั้นพอที่จะ
    /// ไม่รู้สึกว่าต้องรอ · เครื่องสแกนพิมพ์รวดเดียวจบแล้วปิดท้ายด้วย Enter อยู่แล้ว
    /// จึงไม่ต้องรอครบเวลานี้
    /// </summary>
    private const int AutoLookupDelayMs = 600; // หยุดพิมพ์ 600 ms แล้วค้นให้

    private System.Windows.Forms.Timer? _autoLookupTimer; // ตัวจับเวลาหลังพิมพ์ Barcode

    public ScanBarcodeUserControl() // เตรียมหน้าสแกนและผูกปุ่ม
    {
        InitializeComponent(); // สร้างหน้าจอจาก Designer
        btnConfirm.Click += BtnConfirm_Click; // กด OK ไปตรวจและสร้างงาน
        btnClear.Click += BtnClear_Click; // กด Clear ไปล้างหน้าจอ
        btnEditQty.Click += BtnEditQty_Click; // กดดินสอไปแก้ Qty
        txtBarcode.KeyDown += TxtBarcode_KeyDown; // กด Enter ไปค้น Lot
        txtBarcode.TextChanged += TxtBarcode_TextChanged; // ข้อความเปลี่ยนให้เริ่มจับเวลาใหม่

        // จอที่หน้างานเป็นทัชสกรีน ไม่มีคีย์บอร์ดให้กด Enter — โปรแกรมจึงต้องดึงข้อมูล
        // ให้เองเมื่อพนักงานพิมพ์เลขเสร็จ ไม่ใช่รอให้สั่ง
        _autoLookupTimer = new System.Windows.Forms.Timer { Interval = AutoLookupDelayMs }; // ตั้งเวลาค้นอัตโนมัติ
        _autoLookupTimer.Tick += AutoLookupTimer_Tick; // ครบเวลาแล้วเรียกค้น Lot

        Disposed += (_, _) => // เลิกใช้หน้าแล้วหยุดตัวจับเวลา
        {
            _autoLookupTimer?.Stop();
            _autoLookupTimer?.Dispose(); // คืนทรัพยากรของตัวจับเวลา
        };
    }

    /// <summary>
    /// วางเคอร์เซอร์ไว้ที่ช่องบาร์โค้ด เพื่อให้ยิงสแกนเนอร์ได้เลยโดยไม่ต้องคลิกก่อน
    /// <para>
    /// สแกนเนอร์แบบ keyboard wedge พิมพ์ตัวอักษรลงช่องที่กำลังโฟกัสอยู่ ถ้าไม่มี
    /// ช่องไหนโฟกัส บาร์โค้ดจะหายไปเฉย ๆ หรือไปโผล่ผิดช่อง
    /// </para>
    /// <para>
    /// เลื่อนไปทำทีหลังด้วย BeginInvoke เพราะตอนที่หน้าถูกเรียกให้แสดง คอนโทรล
    /// อาจยังไม่พร้อมรับโฟกัส สั่งตรง ๆ ตอนนั้นจะไม่มีผล
    /// </para>
    /// </summary>
    public void FocusBarcode() // วางเคอร์เซอร์ให้พร้อมสแกน
    {
        if (!IsHandleCreated) return; // หน้าจอยังไม่พร้อมให้ข้ามก่อน
        BeginInvoke(() =>
        {
            if (IsDisposed || !txtBarcode.Visible) return;
            txtBarcode.Focus();
        });
    }

    private (ApiClient api, SqliteDataService sqlite) GetServices() // เตรียมทางไป Backend และ SQLite
    {
        var pcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1"); // อ่าน IP ของ Backend
        _api = new ApiClient($"http://{pcIp}:3000"); // ต่อ Backend ที่พอร์ต 3000

        _sqlite = OpenSourceDb(); // เตรียมตัวอ่าน PrintData.db3

        return (_api, _sqlite);
    }

    /// <summary>PrintData.db3 ตามที่ตั้งไว้ใน Setting — เปิดแบบอ่านอย่างเดียวเสมอ</summary>
    private static SqliteDataService OpenSourceDb() => // อ่านที่อยู่ไฟล์ฐานข้อมูลต้นทาง
        new(CustomSettingsManager.Read("DB_PATH", "")); // ใช้ DB_PATH ที่ตั้งไว้ใน Setting

    // ---- สแกนแล้วดึงข้อมูลมาโชว์ ----

    /// <summary>
    /// สแกนเนอร์แบบ keyboard wedge จบบาร์โค้ดด้วย Enter — ใช้จังหวะนั้นดึงข้อมูล
    /// ของ lot ขึ้นมาโชว์ โดยไม่ต้องให้พนักงานกดอะไรเพิ่ม
    /// </summary>
    private void TxtBarcode_KeyDown(object? sender, KeyEventArgs e) // รับปุ่ม Enter จากช่อง Barcode
    {
        if (e.KeyCode != Keys.Enter) return; // ปุ่มอื่นไม่ต้องค้นข้อมูล

        // กัน beep ของ WinForms ตอนกด Enter ในช่องบรรทัดเดียว
        e.Handled = true;
        e.SuppressKeyPress = true; // ไม่ส่ง Enter ต่อให้ช่องข้อความ

        LoadLot(quiet: false); // ค้นข้อมูล Lot จาก Barcode และแจ้งเตือนถ้าไม่พบ
    }

    /// <summary>
    /// พิมพ์เสร็จแล้ว (หยุดพิมพ์ครบเวลา) — ลองดึงข้อมูลให้เองแบบเงียบ ๆ
    /// หาไม่เจอก็ไม่ต้องบอกอะไร เพราะอาจแค่ยังพิมพ์ไม่ครบ
    /// </summary>
    private void AutoLookupTimer_Tick(object? sender, EventArgs e) => LoadLot(quiet: true); // ครบ 600 ms ค้นให้โดยไม่เด้งเตือน

    /// <summary>
    /// แก้บาร์โค้ดเมื่อไหร่ ข้อมูลที่โชว์อยู่ก็ไม่ใช่ของ lot ในช่องอีกต่อไป
    /// ล้างทิ้งทันทีเพื่อไม่ให้เผลอลงทะเบียนด้วยข้อมูลค้าง
    /// </summary>
    private void TxtBarcode_TextChanged(object? sender, EventArgs e) // รับ Barcode ที่กำลังพิมพ์หรือสแกน
    {
        // ตั้งนาฬิกาใหม่ทุกตัวอักษร — จะยิงก็ต่อเมื่อหยุดพิมพ์จริง ๆ
        // เครื่องสแกนพิมพ์รัวจึงไม่มีทางยิงกลางคัน
        _autoLookupTimer?.Stop();

        if (_loadedBarcode != null && txtBarcode.Text.Trim() != _loadedBarcode) // ถ้าเปลี่ยนจาก Lot ที่เคยโหลด
            ClearLotInfo();

        if (txtBarcode.Text.Trim().Length > 0) _autoLookupTimer?.Start(); // มีข้อความแล้วเริ่มนับ 600 ms ใหม่
    }

    /// <summary>
    /// ดึงข้อมูลของ lot ขึ้นมาโชว์
    /// </summary>
    /// <param name="quiet">
    /// true = ไม่ต้องเด้งเตือนเมื่อหาไม่เจอ ใช้ตอนที่โปรแกรมลองหาเองระหว่างพนักงาน
    /// พิมพ์อยู่ — เลขที่พิมพ์ไปได้ครึ่งเดียวย่อมหาไม่เจอเป็นธรรมดา ไม่ใช่ความผิดพลาด
    /// เตือนเฉพาะตอนที่พนักงานสั่งเองเท่านั้น (กด Enter หรือกด OK)
    /// </param>
    /// <returns>true = เจอและโชว์ข้อมูลแล้ว</returns>
    private bool LoadLot(bool quiet) // ค้น Lot แล้วเติมข้อมูลบนจอ
    {
        _autoLookupTimer?.Stop();

        var barcode = txtBarcode.Text.Trim(); // อ่าน Barcode และตัดช่องว่างหัวท้าย
        if (string.IsNullOrWhiteSpace(barcode)) // ถ้า Barcode ว่างหรือมีแต่ช่องว่าง
        {
            if (!quiet) ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode"); // ถ้ากด Enter หรือ OK ให้เตือนว่าต้องกรอก Barcode ก่อน
            return false;
        }

        var sqlite = OpenSourceDb(); // เตรียมอ่านฐานข้อมูลตาม DB_PATH
        if (!sqlite.CanConnect())
        {
            if (!quiet)
                ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting");
            return false;
        }

        var lot = sqlite.GetLotSummary(barcode); // อ่าน Order No, Qty, วิธีพิมพ์และลูกค้า
        if (lot == null)
        {
            if (quiet) return false; // ค้นอัตโนมัติไม่พบให้จบเงียบ ๆ

            ClearLotInfo();
            ShowWarning($"ไม่พบข้อมูลใน print_data สำหรับ barcode: {barcode}");
            txtBarcode.Focus();
            return false;
        }

        // ช่องไหนไม่มีค่าใน DB3 ก็ปล่อยว่างไว้ ไม่เตือน — ช่องว่างบอกตัวมันเองอยู่แล้ว
        // และการเตือนตอนนี้จะไปขวางจังหวะสแกนงานถัดไปของพนักงาน
        _loadedBarcode = barcode; // จำว่าโหลดข้อมูลของ Barcode นี้แล้ว
        _customerName = lot.Customer; // เก็บลูกค้าไว้ส่งไปกับ Job
        txtErpMfg.Text = lot.ErpMfg ?? ""; // แสดง Order No ถ้าไม่มีให้เว้นว่าง
        txtMarkingMethod.Text = lot.MarkingMethod ?? ""; // แสดงวิธีพิมพ์ของ Lot
        txtQty.Text = lot.Qty?.ToString() ?? ""; // แสดงจำนวนจากฐานข้อมูล
        btnEditQty.Enabled = true; // เปิดให้แก้จำนวนได้
        return true;
    }

    /// <summary>
    /// เปิดหน้าต่างให้แก้ Qty ของงานที่กำลังจะลงทะเบียน
    ///
    /// ค่าที่แก้มีผลเฉพาะ job ที่สร้างจากการกด OK ครั้งนี้เท่านั้น — ไม่เขียนกลับ
    /// PrintData.db3 (เปิดแบบอ่านอย่างเดียวอยู่แล้ว) และไม่แตะ qty ของ uv_job_data
    /// ซึ่งยังเก็บค่าดิบจาก print_data ตามเดิม
    /// </summary>
    private void BtnEditQty_Click(object? sender, EventArgs e) // แก้จำนวนก่อนสร้างงาน
    {
        using var dlg = new InputDialog("Edit Qty", "Qty:", txtQty.Text.Trim());
        if (dlg.ShowDialog(this) != DialogResult.OK) return; // ยกเลิกแล้วใช้ค่าเดิม

        if (!int.TryParse(dlg.Value, out var qty) || qty <= 0) // รับเฉพาะจำนวนเต็มที่มากกว่า 0
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0");
            return;
        }

        txtQty.Text = qty.ToString(); // เปลี่ยนค่าบนจอ ยังไม่บันทึก DB
    }

    private async void BtnConfirm_Click(object? sender, EventArgs e) // ผู้ใช้กด OK
    {
        if (!ValidateForm()) return; // ตรวจข้อมูลไม่ผ่านให้หยุดก่อน

        btnConfirm.Loading = true; // แสดงว่ากำลังสร้างงาน
        btnConfirm.Enabled = false; // กันกด OK ซ้ำระหว่างบันทึก
        try
        {
            await ProcessBarcodeAsync(txtBarcode.Text.Trim()); // ไปสร้างงานและรอให้จบ
        }
        finally
        {
            btnConfirm.Loading = false; // ปิดสถานะกำลังทำงาน
            btnConfirm.Enabled = true; // เปิดปุ่มให้ใช้งานต่อ
        }
    }

    private bool ValidateForm() // ตรวจ Barcode และ Qty ก่อนสร้างงาน
    {
        if (string.IsNullOrWhiteSpace(txtBarcode.Text)) // ยังไม่ได้กรอก Barcode
        {
            ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode");
            txtBarcode.Focus();
            return false;
        }

        // ยังไม่ได้ดึงข้อมูลของบาร์โค้ดนี้ — ดึงให้เลยตรงนี้ ไม่ต้องให้ไปกด Enter
        // (จอทัชสกรีนไม่มีคีย์บอร์ด) หาไม่เจอ LoadLot จะบอกสาเหตุเอง
        if (_loadedBarcode != txtBarcode.Text.Trim()) // ข้อมูลบนจอยังไม่ใช่ของ Barcode นี้
        {
            if (!LoadLot(quiet: false)) return false; // โหลด Lot ก่อน หาไม่เจอให้หยุด

            // เจอแล้วแต่ยังไม่ลงทะเบียนรอบนี้ ให้ดูข้อมูลที่เพิ่งขึ้นมาก่อน
            // แล้วค่อยกด OK อีกครั้ง — กันลงทะเบียนงานที่ยังไม่มีใครเห็นตัวเลข
            Notify.Info(this, "ดึงข้อมูลแล้ว — ตรวจสอบแล้วกด OK อีกครั้งเพื่อลงทะเบียน");
            return false;
        }

        // qty ใน print_data ว่างหรือเป็น 0 ได้ ให้พนักงานใส่เองผ่านปุ่มดินสอ
        var qtyText = txtQty.Text.Trim(); // อ่าน Qty ล่าสุดบนหน้าจอ
        if (!int.TryParse(qtyText, out var qty) || qty <= 0) // จำนวนต้องเป็นจำนวนเต็มมากกว่า 0
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0\nกดปุ่มดินสอเพื่อแก้ไข Qty");
            return false;
        }

        return true;
    }

    private async Task ProcessBarcodeAsync(string barcode) // คุมลำดับสร้างงานทั้งหมด
    {
        var (api, sqlite) = GetServices(); // เตรียม Backend และฐานข้อมูลต้นทาง

        // Pre-flight: check SQLite + backend
        if (!sqlite.CanConnect())
        {
            ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting");
            return;
        }

        if (!await api.PingAsync()) // ตรวจว่า Backend ตอบกลับหรือไม่
        {
            ShowError("ไม่สามารถเชื่อมต่อ Backend ได้\nกรุณาตรวจสอบ Backend Setting");
            return;
        }

        // ยังไม่ได้เลือก mydatabase.db3 → ระยะแคลมป์จะถูกเก็บเป็นค่าว่าง
        // เตือนแล้วให้เลือกเองว่าจะไปตั้งค่าก่อน หรือลงทะเบียนไปเลย
        if (!ConfirmClampDatabase()) return; // ผู้ใช้ไม่ทำต่อเมื่อไฟล์แคลมป์ไม่พร้อม ให้หยุด

        // Step 1: Query SQLite
        var patternTemplate = sqlite.GetPatternDetail(barcode, 0); // อ่าน Pattern โดยยังไม่มี Job ID
        if (patternTemplate == null) // ไม่พบข้อมูลตั้งค่าพิมพ์ของ Lot
        {
            ShowWarning($"ไม่พบข้อมูลใน inkjet_data สำหรับ barcode: {barcode}");
            return;
        }

        var uvItems = sqlite.GetUvDetail(barcode); // อ่านชื่อโปรแกรมและข้อความ UV
        var planRouting = sqlite.GetPlanRouting(barcode, 0); // อ่านแผนงานของ Lot

        // Step 2A: POST /job/create
        var jobRequest = new CreateJobRequest // เตรียมข้อมูลหัวงานส่ง Backend
        {
            BarcodeRaw = barcode, // Barcode ที่ผู้ใช้กำลังลงทะเบียน
            CreatedBy = "operator", // ระบุผู้สร้างงานเป็น operator
            OrderNo = txtErpMfg.Text.Trim(), // ใช้ Order No ที่แสดงบนจอ
            // ชื่อลูกค้าไม่มีช่องบนหน้านี้ ดึงมาจาก inkjet_data.customer ตอนสแกน
            // แล้วติดไปกับงานเฉย ๆ ไปโผล่ที่หน้า Order Detail
            CustomerName = _customerName, // ใช้ลูกค้าที่อ่านมาตอนโหลด Lot
            // Qty ที่ส่งไปคือค่าที่โชว์อยู่บนจอ ซึ่งอาจถูกแก้ด้วยปุ่มดินสอแล้ว
            // ผลของการแก้จบที่ print_jobs แถวนี้แถวเดียว
            Type = txtMarkingMethod.Text.Trim(), // ใช้วิธีพิมพ์ที่แสดงบนจอ
            Qty = int.TryParse(txtQty.Text.Trim(), out var q) ? q : null, // ใช้ Qty ล่าสุด รวมค่าที่ผู้ใช้แก้
            StStatus = "0",
        };

        var (job, jobErr) = await api.CreateJobAsync(jobRequest); // สร้าง Job แล้วรับ Job ID กลับมา
        if (job == null)
        {
            ShowError($"สร้าง Job ไม่สำเร็จ\n{jobErr}");
            return;
        }

        // Step 2B: POST /pattern/create
        patternTemplate.JobId = job.Id; // ผูก Pattern กับ Job ที่เพิ่งสร้าง
        var (pattern, patErr) = await api.CreatePatternAsync(patternTemplate); // บันทึก Pattern
        if (pattern == null) // บันทึก Pattern ไม่สำเร็จ
        {
            await api.DeleteJobAsync(job.Id); // พยายามลบ Job โดยตรงนี้ไม่ได้ตรวจผลลบ
            ShowError($"สร้าง Pattern ไม่สำเร็จ — Job ถูกลบแล้ว\n{patErr}");
            return;
        }

        // Step 2C: POST /uv-job/create (skip if no UV data)
        if (uvItems.Count > 0) // มีข้อมูล UV จึงบันทึก ถ้าไม่มีให้ข้าม
        {
            var uvRequest = new CreateUvJobRequest // เตรียมข้อมูล UV ของงานนี้
            {
                PrintJobsId = job.Id, // ผูกข้อมูล UV กับ Job เดียวกัน
                Items = uvItems,
            };
            var (uvOk, uvErr) = await api.CreateUvJobDataAsync(uvRequest); // ส่งข้อมูล UV ไปบันทึก
            if (!uvOk)
            {
                ShowWarning($"บันทึก UV Data ไม่สำเร็จ แต่ Job + Pattern สร้างแล้ว\n{uvErr}");
            }
        }

        // Step 2D: POST /plan-routing/create (skip if lot has no plan_routing row)
        if (planRouting != null) // มีแผนงานต้นทางจึงบันทึก
        {
            planRouting.PrintJobsId = job.Id; // ผูก Routing กับ Job เดียวกัน
            var (planOk, planErr) = await api.CreatePlanRoutingAsync(planRouting); // ส่งแผนงานไปบันทึก
            if (!planOk)
            {
                ShowWarning($"บันทึก Plan Routing ไม่สำเร็จ แต่ Job + Pattern สร้างแล้ว\n{planErr}");
            }
        }
        else
        {
            ShowWarning($"ไม่พบข้อมูลใน plan_routing สำหรับ barcode: {barcode}\nJob ถูกสร้างแล้วแต่ไม่มีข้อมูล marking_method");
        }

        // Step 2E: บันทึกระยะแคลมป์ (IAI) ของงานนี้ลง backend
        await SyncIaiAsync(api, job.Id, uvItems); // อ่านและเก็บค่าแคลมป์ของงาน

        Notify.Success(this,
            $"สร้างงาน {Services.JobDisplay.Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id)} สำเร็จ"); // ใส่เลขอ้างอิงงานในข้อความ

        ClearForm();
    }

    /// <summary>
    /// เตือนเมื่อยังไม่ได้เลือก mydatabase.db3 — ระยะแคลมป์จะถูกเก็บเป็นค่าว่าง
    /// ไม่บล็อกการลงทะเบียน เพราะงานที่ไม่ผ่าน UV ก็ไม่ต้องใช้ค่านี้
    /// คืน false = ผู้ใช้ขอไปตั้งค่าก่อน
    /// </summary>
    private static bool ConfirmClampDatabase() // ถามผู้ใช้เมื่อไฟล์ข้อมูลแคลมป์ไม่พร้อม
    {
        var path = CustomSettingsManager.Read("CLAMP_DB_PATH", ""); // อ่านที่อยู่ mydatabase.db3
        bool ready = !string.IsNullOrWhiteSpace(path) && File.Exists(path); // ตรวจว่าตั้งที่อยู่และพบไฟล์แล้ว
        if (ready) return true; // มีไฟล์แล้วให้ทำต่อ

        var reason = string.IsNullOrWhiteSpace(path) // แยกสาเหตุที่ใช้ไฟล์ไม่ได้
            ? "ยังไม่ได้เลือกไฟล์ mydatabase.db3" // ยังไม่ได้ตั้งที่อยู่ไฟล์
            : $"ไม่พบไฟล์ที่ตั้งไว้:\n{path}"; // ตั้งแล้วแต่หาไฟล์ไม่พบ

        return Confirm.Ask(null, "ยังไม่ได้ตั้งค่า Clamp Database",
            $"{reason}\n\n" +
            "ระยะแคลมป์ (IAI) ของงานนี้จะถูกบันทึกเป็นค่าว่าง\n" + // บอกผลถ้าฝืนลงทะเบียนต่อ
            "ตั้งค่าได้ที่ Setting → PLC UV Setting → Browse\n\n" + // บอกทางไปเลือกไฟล์
            "ต้องการลงทะเบียนต่อไปหรือไม่?"); // รอผู้ใช้เลือกทำต่อหรือหยุด
    }

    /// <summary>
    /// บันทึกระยะแคลมป์ (IAI) ที่งานนี้ใช้ลง backend — 1 job = 1 แถว
    ///
    /// UV1 = Plate (ชื่อขึ้นต้น "P-") → iaip/iaip_z1/iaip_z2 · UV2 = Shim → iai/iai_z1/iai_z2
    /// หาค่าไม่เจอก็ยังส่งขึ้นไป เก็บเป็น null เพื่อบอกว่า "หาแล้วไม่มี"
    /// ต่างจาก "ยังไม่เคยหา" ซึ่งคือไม่มีแถวเลย
    ///
    /// เป็นขั้นตอนเสริม ล้มเหลวก็ไม่กระทบการ register — job สร้างครบไปแล้ว
    /// </summary>
    private static async Task SyncIaiAsync(ApiClient api, int jobId, List<UvJobItem> uvItems) // อ่านค่า IAI ไปเก็บ ยังไม่สั่ง PLC
    {
        var settings = ClampSettings.Load(); // อ่านไฟล์และแกนที่ตั้งไว้
        bool canRead = !string.IsNullOrWhiteSpace(settings.DbPath) && File.Exists(settings.DbPath); // ตรวจว่ามีไฟล์ให้อ่านค่าแคลมป์

        var request = new IaiCreateRequest { PrintJobsId = jobId }; // เตรียมค่า IAI ผูกกับ Job นี้

        foreach (var item in uvItems) // อ่านชื่อโปรแกรมจากแต่ละรายการ UV
        {
            var program = (item.ProgramName ?? "").Trim(); // ตัดช่องว่างจากชื่อโปรแกรม
            if (program.Length == 0) continue; // ไม่มีชื่อโปรแกรมให้ข้ามรายการนี้

            // แยกช่องด้วย prefix ของชื่อโปรแกรม กฎเดียวกับ backend และระบบเดิม
            bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase); // ชื่อขึ้นต้น P- ให้ใช้ฝั่ง Plate
            var side = isPlate ? ClampSide.Plate : ClampSide.Shim; // ชื่ออื่นใช้ฝั่ง Shim

            if (isPlate) request.M1ProgramName = program; // เก็บชื่อโปรแกรม Plate
            else request.M2ProgramName = program; // เก็บชื่อโปรแกรม Shim

            // เก็บครบทุกแกนของฝั่งนี้ — ไม่มี path หรือหาไม่เจอก็เก็บเป็น null
            foreach (var axis in settings.For(side)) // ค้นค่าทุกแกนของฝั่งนี้
            {
                int? value = canRead // มีไฟล์จึงลองค้นค่า
                    ? ClampService.Lookup(settings.DbPath, program, axis) is { Found: true } hit // ค้นระยะตามโปรแกรมและแกน
                        ? hit.ValueMm
                        : null
                    : null;

                switch (axis.Key)
                {
                    case "IAIP": request.Iaip = value; break; // Plate แกน X
                    case "IAIPZ1": request.IaipZ1 = value; break; // Plate แกน Z1
                    case "IAIPZ2": request.IaipZ2 = value; break; // Plate แกน Z2
                    case "IAI": request.Iai = value; break; // Shim แกน X
                    case "IAIZ1": request.IaiZ1 = value; break; // Shim แกน Z1
                    case "IAIZ2": request.IaiZ2 = value; break; // Shim แกน Z2
                }
            }
        }

        // ไม่มีชื่อโปรแกรมเลย = งานนี้ไม่เกี่ยวกับ UV → ไม่ต้องสร้างแถว
        if (request.M1ProgramName == null && request.M2ProgramName == null) return; // ไม่มีชื่อโปรแกรมทั้งสองฝั่ง ไม่สร้างแถว IAI

        await api.CreateIaiAsync(request); // บันทึก IAI โดยไม่ได้ตรวจผลที่คืนมา
    }

    private void BtnClear_Click(object? sender, EventArgs e) // ผู้ใช้กดล้างหน้าจอ
    {
        ClearForm();
    }

    private void ClearForm() // จบที่หน้าสแกนเดิม ไม่เปิดหน้า Station อื่น
    {
        txtBarcode.Text = ""; // ล้าง Barcode ในช่องรับงาน
        ClearLotInfo();
        txtBarcode.Focus();
    }

    /// <summary>ล้างเฉพาะข้อมูลที่ดึงมาจาก DB3 — ช่องบาร์โค้ดไม่ถูกแตะ</summary>
    private void ClearLotInfo() // ล้างข้อมูลประกอบของ Lot เดิม
    {
        _loadedBarcode = null; // ลืม Lot ที่เคยโหลดไว้
        _customerName = null; // ล้างลูกค้าของ Lot เดิม
        txtErpMfg.Text = ""; // ล้าง Order No
        txtMarkingMethod.Text = ""; // ล้างวิธีพิมพ์
        txtQty.Text = "";
        btnEditQty.Enabled = false; // รอโหลด Lot ใหม่ก่อนให้แก้ Qty
    }

    private static void ShowWarning(string msg) => // แสดงคำเตือนของ Flow สแกน
        Notify.WarnModal(null, "แจ้งเตือน", msg);

    private static void ShowError(string msg) => // แสดงข้อผิดพลาดของ Flow สแกน
        Notify.ErrorModal(null, "Error", msg);
}
