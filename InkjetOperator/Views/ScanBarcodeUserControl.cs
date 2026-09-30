using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class ScanBarcodeUserControl : UserControl
{
    private ApiClient? _api;
    private SqliteDataService? _sqlite;

    private string? _loadedBarcode; // จำ Barcode ที่โหลดข้อมูลแล้ว

    private string? _customerName; // เก็บชื่อลูกค้าไว้ส่งตอนสร้าง Job

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

        _autoLookupTimer = new System.Windows.Forms.Timer { Interval = AutoLookupDelayMs }; // ตั้งเวลาค้นอัตโนมัติ
        _autoLookupTimer.Tick += AutoLookupTimer_Tick; // ครบเวลาแล้วเรียกค้น Lot

        Disposed += (_, _) => // เลิกใช้หน้าแล้วหยุดตัวจับเวลา
        {
            _autoLookupTimer?.Stop();
            _autoLookupTimer?.Dispose(); // คืนทรัพยากรของตัวจับเวลา
        };
    }

    public void FocusBarcode() // วางเคอร์เซอร์ให้พร้อมสแกน
    {
        if (!IsHandleCreated) return; // หน้าจอยังไม่พร้อมให้ข้ามก่อน
        BeginInvoke(() => // นัดให้หน้าจอกลับมารับสแกนบนเธรด UI
        {
            if (IsDisposed || !txtBarcode.Visible) return; // ข้ามเมื่อปิดหน้าแล้วหรือช่องสแกนไม่แสดง
            txtBarcode.Focus(); // วางเคอร์เซอร์กลับช่อง Barcode
        });
    }

    private (ApiClient api, SqliteDataService sqlite) GetServices() // เตรียมทางไป Backend และ SQLite
    {
        var pcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1"); // อ่าน IP ของ Backend
        _api = new ApiClient($"http://{pcIp}:3000"); // ต่อ Backend ที่พอร์ต 3000

        _sqlite = OpenSourceDb(); // เตรียมตัวอ่าน PrintData.db3

        return (_api, _sqlite); // ส่งตัวเชื่อม Backend และ SQLite ให้ผู้เรียกใช้
    }

    private static SqliteDataService OpenSourceDb() => // อ่านที่อยู่ไฟล์ฐานข้อมูลต้นทาง
        new(CustomSettingsManager.Read("DB_PATH", "")); // ใช้ DB_PATH ที่ตั้งไว้ใน Setting

    private void TxtBarcode_KeyDown(object? sender, KeyEventArgs e) // รับปุ่ม Enter จากช่อง Barcode
    {
        if (e.KeyCode != Keys.Enter) return; // ปุ่มอื่นไม่ต้องค้นข้อมูล

        e.Handled = true; // ระบุว่าจัดการปุ่ม Enter แล้ว
        e.SuppressKeyPress = true; // ไม่ส่ง Enter ต่อให้ช่องข้อความ

        LoadLot(quiet: false); // ค้นข้อมูล Lot จาก Barcode และแจ้งเตือนถ้าไม่พบ
    }

    private void AutoLookupTimer_Tick(object? sender, EventArgs e) => LoadLot(quiet: true); // ครบ 600 ms ค้นให้โดยไม่เด้งเตือน

    private void TxtBarcode_TextChanged(object? sender, EventArgs e) // รับ Barcode ที่กำลังพิมพ์หรือสแกน
    {
        _autoLookupTimer?.Stop(); // หยุดนับเวลาค้นหาเดิมเมื่อข้อความเปลี่ยน

        if (_loadedBarcode != null && txtBarcode.Text.Trim() != _loadedBarcode) // ถ้าเปลี่ยนจาก Lot ที่เคยโหลด
            ClearLotInfo(); // ล้างข้อมูล Lot เดิมก่อนรับ Barcode ใหม่

        if (txtBarcode.Text.Trim().Length > 0) _autoLookupTimer?.Start(); // มีข้อความแล้วเริ่มนับ 600 ms ใหม่
    }

    private bool LoadLot(bool quiet) // ค้น Lot แล้วเติมข้อมูลบนจอ
    {
        _autoLookupTimer?.Stop(); // หยุดตัวจับเวลาก่อนค้นหา Lot ทันที

        var barcode = txtBarcode.Text.Trim(); // อ่าน Barcode และตัดช่องว่างหัวท้าย
        if (string.IsNullOrWhiteSpace(barcode)) // ถ้า Barcode ว่างหรือมีแต่ช่องว่าง
        {
            if (!quiet) ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode"); // ถ้ากด Enter หรือ OK ให้เตือนว่าต้องกรอก Barcode ก่อน
            return false;
        }

        var sqlite = OpenSourceDb(); // เตรียมอ่านฐานข้อมูลตาม DB_PATH
        if (!sqlite.CanConnect()) // เช็คว่าเปิดฐานข้อมูลเครื่องได้หรือไม่
        {
            if (!quiet) // แจ้งคนใช้เฉพาะการค้นหาที่ไม่ได้สั่งแบบเงียบ
                ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting"); // บอกให้ตรวจที่อยู่ไฟล์ PrintData.db3
            return false;
        }

        var lot = sqlite.GetLotSummary(barcode); // อ่าน Order No, Qty, วิธีพิมพ์และลูกค้า
        if (lot == null) // เช็คว่าพบ Lot ตรงกับ Barcode หรือไม่
        {
            if (quiet) return false; // ค้นอัตโนมัติไม่พบให้จบเงียบ ๆ

            ClearLotInfo(); // ล้าง Lot ที่ค้างอยู่เมื่อค้นหาไม่พบ
            ShowWarning($"ไม่พบข้อมูลใน print_data สำหรับ barcode: {barcode}"); // แจ้ง Barcode ที่หาไม่เจอใน print_data
            txtBarcode.Focus(); // วางเคอร์เซอร์กลับช่อง Barcode
            return false;
        }

        _loadedBarcode = barcode; // จำว่าโหลดข้อมูลของ Barcode นี้แล้ว
        _customerName = lot.Customer; // เก็บลูกค้าไว้ส่งไปกับ Job
        txtErpMfg.Text = lot.ErpMfg ?? ""; // แสดง Order No ถ้าไม่มีให้เว้นว่าง
        txtMarkingMethod.Text = lot.MarkingMethod ?? ""; // แสดงวิธีพิมพ์ของ Lot
        txtQty.Text = lot.Qty?.ToString() ?? ""; // แสดงจำนวนจากฐานข้อมูล
        btnEditQty.Enabled = true; // เปิดให้แก้จำนวนได้
        return true;
    }

    private void BtnEditQty_Click(object? sender, EventArgs e) // แก้จำนวนก่อนสร้างงาน
    {
        using var dlg = new InputDialog("Edit Qty", "Qty:", txtQty.Text.Trim()); // เปิดช่องแก้จำนวนโดยใส่ Qty เดิมไว้ให้
        if (dlg.ShowDialog(this) != DialogResult.OK) return; // ยกเลิกแล้วใช้ค่าเดิม

        if (!int.TryParse(dlg.Value, out var qty) || qty <= 0) // รับเฉพาะจำนวนเต็มที่มากกว่า 0
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0"); // แจ้งให้กรอกจำนวนเต็มมากกว่าศูนย์
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        txtQty.Text = qty.ToString(); // เปลี่ยนค่าบนจอ ยังไม่บันทึก DB
    }

    private async void BtnConfirm_Click(object? sender, EventArgs e) // ผู้ใช้กด OK
    {
        if (!ValidateForm()) return; // ตรวจข้อมูลไม่ผ่านให้หยุดก่อน

        btnConfirm.Loading = true; // แสดงว่ากำลังสร้างงาน
        btnConfirm.Enabled = false; // กันกด OK ซ้ำระหว่างบันทึก
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            await ProcessBarcodeAsync(txtBarcode.Text.Trim()); // ไปสร้างงานและรอให้จบ
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            btnConfirm.Loading = false; // ปิดสถานะกำลังทำงาน
            btnConfirm.Enabled = true; // เปิดปุ่มให้ใช้งานต่อ
        }
    }

    private bool ValidateForm() // ตรวจ Barcode และ Qty ก่อนสร้างงาน
    {
        if (string.IsNullOrWhiteSpace(txtBarcode.Text)) // ยังไม่ได้กรอก Barcode
        {
            ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode"); // เตือนให้ใส่ Barcode ก่อนลงทะเบียน
            txtBarcode.Focus(); // วางเคอร์เซอร์กลับช่อง Barcode
            return false;
        }

        if (_loadedBarcode != txtBarcode.Text.Trim()) // ข้อมูลบนจอยังไม่ใช่ของ Barcode นี้
        {
            if (!LoadLot(quiet: false)) return false; // โหลด Lot ก่อน หาไม่เจอให้หยุด

            Notify.Info(this, "ดึงข้อมูลแล้ว — ตรวจสอบแล้วกด OK อีกครั้งเพื่อลงทะเบียน"); // ให้ตรวจข้อมูลที่เพิ่งโหลดแล้วกด OK อีกครั้ง
            return false;
        }

        var qtyText = txtQty.Text.Trim(); // อ่าน Qty ล่าสุดบนหน้าจอ
        if (!int.TryParse(qtyText, out var qty) || qty <= 0) // จำนวนต้องเป็นจำนวนเต็มมากกว่า 0
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0\nกดปุ่มดินสอเพื่อแก้ไข Qty"); // บอกให้แก้ Qty ผ่านปุ่มดินสอ
            return false;
        }

        return true;
    }

    private async Task ProcessBarcodeAsync(string barcode) // คุมลำดับสร้างงานทั้งหมด
    {
        var (api, sqlite) = GetServices(); // เตรียม Backend และฐานข้อมูลต้นทาง

        if (!sqlite.CanConnect()) // ตรวจฐานข้อมูลเครื่องก่อนสร้างงาน
        {
            ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting"); // แจ้งให้ตรวจไฟล์ฐานข้อมูลใน Setting
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (!await api.PingAsync()) // ตรวจว่า Backend ตอบกลับหรือไม่
        {
            ShowError("ไม่สามารถเชื่อมต่อ Backend ได้\nกรุณาตรวจสอบ Backend Setting"); // แจ้งให้ตรวจการเชื่อมต่อ Backend
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (!ConfirmClampDatabase()) return; // ผู้ใช้ไม่ทำต่อเมื่อไฟล์แคลมป์ไม่พร้อม ให้หยุด

        var patternTemplate = sqlite.GetPatternDetail(barcode, 0); // อ่าน Pattern โดยยังไม่มี Job ID
        if (patternTemplate == null) // ไม่พบข้อมูลตั้งค่าพิมพ์ของ Lot
        {
            ShowWarning($"ไม่พบข้อมูลใน inkjet_data สำหรับ barcode: {barcode}"); // แจ้งว่าไม่พบข้อมูลพิมพ์ของ Barcode นี้
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        var uvItems = sqlite.GetUvDetail(barcode); // อ่านชื่อโปรแกรมและข้อความ UV
        var planRouting = sqlite.GetPlanRouting(barcode, 0); // อ่านแผนงานของ Lot

        var jobRequest = new CreateJobRequest // เตรียมข้อมูลหัวงานส่ง Backend
        {
            BarcodeRaw = barcode, // Barcode ที่ผู้ใช้กำลังลงทะเบียน
            CreatedBy = "operator", // ระบุผู้สร้างงานเป็น operator
            OrderNo = txtErpMfg.Text.Trim(), // ใช้ Order No ที่แสดงบนจอ
            CustomerName = _customerName, // ใช้ลูกค้าที่อ่านมาตอนโหลด Lot
            Type = txtMarkingMethod.Text.Trim(), // ใช้วิธีพิมพ์ที่แสดงบนจอ
            Qty = int.TryParse(txtQty.Text.Trim(), out var q) ? q : null, // ใช้ Qty ล่าสุด รวมค่าที่ผู้ใช้แก้
            StStatus = "0", // ตั้งงานใหม่เป็นสถานะรอเริ่ม
        };

        var (job, jobErr) = await api.CreateJobAsync(jobRequest); // สร้าง Job แล้วรับ Job ID กลับมา
        if (job == null) // เช็คว่า Backend สร้างงานได้หรือไม่
        {
            ShowError($"สร้าง Job ไม่สำเร็จ\n{jobErr}"); // แสดงสาเหตุที่สร้างงานไม่สำเร็จ
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        patternTemplate.JobId = job.Id; // ผูก Pattern กับ Job ที่เพิ่งสร้าง
        var (pattern, patErr) = await api.CreatePatternAsync(patternTemplate); // บันทึก Pattern
        if (pattern == null) // บันทึก Pattern ไม่สำเร็จ
        {
            await api.DeleteJobAsync(job.Id); // พยายามลบ Job โดยตรงนี้ไม่ได้ตรวจผลลบ
            ShowError($"สร้าง Pattern ไม่สำเร็จ — Job ถูกลบแล้ว\n{patErr}"); // แสดงปัญหา Pattern หลังขอลบ Job
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        if (uvItems.Count > 0) // มีข้อมูล UV จึงบันทึก ถ้าไม่มีให้ข้าม
        {
            var uvRequest = new CreateUvJobRequest // เตรียมข้อมูล UV ของงานนี้
            {
                PrintJobsId = job.Id, // ผูกข้อมูล UV กับ Job เดียวกัน
                Items = uvItems, // แนบรายการข้อมูลพิมพ์ UV ของงาน
            };
            var (uvOk, uvErr) = await api.CreateUvJobDataAsync(uvRequest); // ส่งข้อมูล UV ไปบันทึก
            if (!uvOk) // เช็คผลบันทึกข้อมูล UV
            {
                ShowWarning($"บันทึก UV Data ไม่สำเร็จ แต่ Job + Pattern สร้างแล้ว\n{uvErr}"); // แจ้งว่า UV บันทึกไม่ได้ แต่งานกับ Pattern มีแล้ว
            }
        }

        if (planRouting != null) // มีแผนงานต้นทางจึงบันทึก
        {
            planRouting.PrintJobsId = job.Id; // ผูก Routing กับ Job เดียวกัน
            var (planOk, planErr) = await api.CreatePlanRoutingAsync(planRouting); // ส่งแผนงานไปบันทึก
            if (!planOk) // เช็คผลบันทึกลำดับการทำงาน
            {
                ShowWarning($"บันทึก Plan Routing ไม่สำเร็จ แต่ Job + Pattern สร้างแล้ว\n{planErr}"); // แจ้งว่าแผนบันทึกไม่ได้ แต่งานกับ Pattern มีแล้ว
            }
        }
        else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
        {
            ShowWarning($"ไม่พบข้อมูลใน plan_routing สำหรับ barcode: {barcode}\nJob ถูกสร้างแล้วแต่ไม่มีข้อมูล marking_method"); // แจ้งว่างานถูกสร้างโดยยังไม่มีข้อมูลวิธีพิมพ์
        }

        await SyncIaiAsync(api, job.Id, uvItems); // อ่านและเก็บค่าแคลมป์ของงาน

        Notify.Success(this, // แจ้งผลลงทะเบียนงานสำเร็จ
            $"สร้างงาน {Services.JobDisplay.Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id)} สำเร็จ"); // ใส่เลขอ้างอิงงานในข้อความ

        ClearForm(); // ล้างหน้าฟอร์มเพื่อรับ Barcode ถัดไป
    }

    private static bool ConfirmClampDatabase() // ถามผู้ใช้เมื่อไฟล์ข้อมูลแคลมป์ไม่พร้อม
    {
        var path = CustomSettingsManager.Read("CLAMP_DB_PATH", ""); // อ่านที่อยู่ mydatabase.db3
        bool ready = !string.IsNullOrWhiteSpace(path) && File.Exists(path); // ตรวจว่าตั้งที่อยู่และพบไฟล์แล้ว
        if (ready) return true; // มีไฟล์แล้วให้ทำต่อ

        var reason = string.IsNullOrWhiteSpace(path) // แยกสาเหตุที่ใช้ไฟล์ไม่ได้
            ? "ยังไม่ได้เลือกไฟล์ mydatabase.db3" // ยังไม่ได้ตั้งที่อยู่ไฟล์
            : $"ไม่พบไฟล์ที่ตั้งไว้:\n{path}"; // ตั้งแล้วแต่หาไฟล์ไม่พบ

        return Confirm.Ask(null, "ยังไม่ได้ตั้งค่า Clamp Database", // ถามว่าจะทำต่อทั้งที่ยังไม่มีฐานข้อมูล Clamp หรือไม่
            $"{reason}\n\n" + // แสดงสาเหตุที่ใช้งานฐานข้อมูล Clamp ไม่ได้
            "ระยะแคลมป์ (IAI) ของงานนี้จะถูกบันทึกเป็นค่าว่าง\n" + // บอกผลถ้าฝืนลงทะเบียนต่อ
            "ตั้งค่าได้ที่ Setting → PLC UV Setting → Browse\n\n" + // บอกทางไปเลือกไฟล์
            "ต้องการลงทะเบียนต่อไปหรือไม่?"); // รอผู้ใช้เลือกทำต่อหรือหยุด
    }

    private static async Task SyncIaiAsync(ApiClient api, int jobId, List<UvJobItem> uvItems) // อ่านค่า IAI ไปเก็บ ยังไม่สั่ง PLC
    {
        var settings = ClampSettings.Load(); // อ่านไฟล์และแกนที่ตั้งไว้
        bool canRead = !string.IsNullOrWhiteSpace(settings.DbPath) && File.Exists(settings.DbPath); // ตรวจว่ามีไฟล์ให้อ่านค่าแคลมป์

        var request = new IaiCreateRequest { PrintJobsId = jobId }; // เตรียมค่า IAI ผูกกับ Job นี้

        foreach (var item in uvItems) // อ่านชื่อโปรแกรมจากแต่ละรายการ UV
        {
            var program = (item.ProgramName ?? "").Trim(); // ตัดช่องว่างจากชื่อโปรแกรม
            if (program.Length == 0) continue; // ไม่มีชื่อโปรแกรมให้ข้ามรายการนี้

            bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase); // ชื่อขึ้นต้น P- ให้ใช้ฝั่ง Plate
            var side = isPlate ? ClampSide.Plate : ClampSide.Shim; // ชื่ออื่นใช้ฝั่ง Shim

            if (isPlate) request.M1ProgramName = program; // เก็บชื่อโปรแกรม Plate
            else request.M2ProgramName = program; // เก็บชื่อโปรแกรม Shim

            foreach (var axis in settings.For(side)) // ค้นค่าทุกแกนของฝั่งนี้
            {
                int? value = canRead // มีไฟล์จึงลองค้นค่า
                    ? ClampService.Lookup(settings.DbPath, program, axis) is { Found: true } hit // ค้นระยะตามโปรแกรมและแกน
                        ? hit.ValueMm // ใช้ระยะมิลลิเมตรที่ค้นพบ
                        : null // เว้นค่าไว้เมื่อค้นค่าของแกนนี้ไม่พบ
                    : null; // เว้นค่าไว้เมื่อไม่มีไฟล์ฐานข้อมูลให้อ่าน

                switch (axis.Key) // เลือกช่องเก็บค่าตามชื่อแกน IAI
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

        if (request.M1ProgramName == null && request.M2ProgramName == null) return; // ไม่มีชื่อโปรแกรมทั้งสองฝั่ง ไม่สร้างแถว IAI

        await api.CreateIaiAsync(request); // บันทึก IAI โดยไม่ได้ตรวจผลที่คืนมา
    }

    private void BtnClear_Click(object? sender, EventArgs e) // ผู้ใช้กดล้างหน้าจอ
    {
        ClearForm(); // ล้างข้อมูลเมื่อกดปุ่ม Clear
    }

    private void ClearForm() // จบที่หน้าสแกนเดิม ไม่เปิดหน้า Station อื่น
    {
        txtBarcode.Text = ""; // ล้าง Barcode ในช่องรับงาน
        ClearLotInfo(); // ล้างรายละเอียด Lot ของงานเดิม
        txtBarcode.Focus(); // วางเคอร์เซอร์กลับช่อง Barcode
    }

    private void ClearLotInfo() // ล้างข้อมูลประกอบของ Lot เดิม
    {
        _loadedBarcode = null; // ลืม Lot ที่เคยโหลดไว้
        _customerName = null; // ล้างลูกค้าของ Lot เดิม
        txtErpMfg.Text = ""; // ล้าง Order No
        txtMarkingMethod.Text = ""; // ล้างวิธีพิมพ์
        txtQty.Text = ""; // ล้างจำนวนชิ้นงานเดิม
        btnEditQty.Enabled = false; // รอโหลด Lot ใหม่ก่อนให้แก้ Qty
    }

    private static void ShowWarning(string msg) => // แสดงคำเตือนของ Flow สแกน
        Notify.WarnModal(null, "แจ้งเตือน", msg);

    private static void ShowError(string msg) => // แสดงข้อผิดพลาดของ Flow สแกน
        Notify.ErrorModal(null, "Error", msg);
}
