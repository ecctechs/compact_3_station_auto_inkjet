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
    private string? _loadedBarcode;

    public ScanBarcodeUserControl()
    {
        InitializeComponent();
        btnConfirm.Click += BtnConfirm_Click;
        btnCancel.Click += BtnCancel_Click;
        btnEditQty.Click += BtnEditQty_Click;
        txtBarcode.KeyDown += TxtBarcode_KeyDown;
        txtBarcode.TextChanged += TxtBarcode_TextChanged;
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
    public void FocusBarcode()
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            if (IsDisposed || !txtBarcode.Visible) return;
            txtBarcode.Focus();
        });
    }

    private (ApiClient api, SqliteDataService sqlite) GetServices()
    {
        var pcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        _api = new ApiClient($"http://{pcIp}:3000");

        _sqlite = OpenSourceDb();

        return (_api, _sqlite);
    }

    /// <summary>PrintData.db3 ตามที่ตั้งไว้ใน Setting — เปิดแบบอ่านอย่างเดียวเสมอ</summary>
    private static SqliteDataService OpenSourceDb() =>
        new(CustomSettingsManager.Read("DB_PATH", ""));

    // ---- สแกนแล้วดึงข้อมูลมาโชว์ ----

    /// <summary>
    /// สแกนเนอร์แบบ keyboard wedge จบบาร์โค้ดด้วย Enter — ใช้จังหวะนั้นดึงข้อมูล
    /// ของ lot ขึ้นมาโชว์ โดยไม่ต้องให้พนักงานกดอะไรเพิ่ม
    /// </summary>
    private void TxtBarcode_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;

        // กัน beep ของ WinForms ตอนกด Enter ในช่องบรรทัดเดียว
        e.Handled = true;
        e.SuppressKeyPress = true;

        LoadLot();
    }

    /// <summary>
    /// แก้บาร์โค้ดเมื่อไหร่ ข้อมูลที่โชว์อยู่ก็ไม่ใช่ของ lot ในช่องอีกต่อไป
    /// ล้างทิ้งทันทีเพื่อไม่ให้เผลอลงทะเบียนด้วยข้อมูลค้าง
    /// </summary>
    private void TxtBarcode_TextChanged(object? sender, EventArgs e)
    {
        if (_loadedBarcode == null) return;
        if (txtBarcode.Text.Trim() == _loadedBarcode) return;

        ClearLotInfo();
    }

    private void LoadLot()
    {
        var barcode = txtBarcode.Text.Trim();
        if (string.IsNullOrWhiteSpace(barcode))
        {
            ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode");
            return;
        }

        var sqlite = OpenSourceDb();
        if (!sqlite.CanConnect())
        {
            ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting");
            return;
        }

        var lot = sqlite.GetLotSummary(barcode);
        if (lot == null)
        {
            ClearLotInfo();
            ShowWarning($"ไม่พบข้อมูลใน print_data สำหรับ barcode: {barcode}");
            txtBarcode.Focus();
            return;
        }

        // ช่องไหนไม่มีค่าใน DB3 ก็ปล่อยว่างไว้ ไม่เตือน — ช่องว่างบอกตัวมันเองอยู่แล้ว
        // และการเตือนตอนนี้จะไปขวางจังหวะสแกนงานถัดไปของพนักงาน
        _loadedBarcode = barcode;
        txtOrderNo.Text = lot.ErpMfg ?? "";
        txtMarkingMethod.Text = lot.MarkingMethod ?? "";
        txtQty.Text = lot.Qty?.ToString() ?? "";
        btnEditQty.Enabled = true;
    }

    /// <summary>
    /// เปิดหน้าต่างให้แก้ Qty ของงานที่กำลังจะลงทะเบียน
    ///
    /// ค่าที่แก้มีผลเฉพาะ job ที่สร้างจากการกด OK ครั้งนี้เท่านั้น — ไม่เขียนกลับ
    /// PrintData.db3 (เปิดแบบอ่านอย่างเดียวอยู่แล้ว) และไม่แตะ qty ของ uv_job_data
    /// ซึ่งยังเก็บค่าดิบจาก print_data ตามเดิม
    /// </summary>
    private void BtnEditQty_Click(object? sender, EventArgs e)
    {
        using var dlg = new InputDialog("Edit Qty", "Qty:", txtQty.Text.Trim());
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (!int.TryParse(dlg.Value, out var qty) || qty <= 0)
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0");
            return;
        }

        txtQty.Text = qty.ToString();
    }

    private async void BtnConfirm_Click(object? sender, EventArgs e)
    {
        if (!ValidateForm()) return;

        btnConfirm.Loading = true;
        btnConfirm.Enabled = false;
        try
        {
            await ProcessBarcodeAsync(txtBarcode.Text.Trim());
        }
        finally
        {
            btnConfirm.Loading = false;
            btnConfirm.Enabled = true;
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(txtBarcode.Text))
        {
            ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode");
            txtBarcode.Focus();
            return false;
        }

        // ช่องที่เหลืออ่านอย่างเดียว มาจากการสแกน ไม่ใช่จากการพิมพ์ —
        // ถ้ายังไม่ได้ดึงข้อมูลของบาร์โค้ดนี้ ก็ยังไม่มีอะไรให้ลงทะเบียน
        if (_loadedBarcode != txtBarcode.Text.Trim())
        {
            ShowWarning("กรุณาสแกนบาร์โค้ดแล้วกด Enter เพื่อดึงข้อมูลก่อน");
            txtBarcode.Focus();
            return false;
        }

        // qty ใน print_data ว่างหรือเป็น 0 ได้ ให้พนักงานใส่เองผ่านปุ่มดินสอ
        var qtyText = txtQty.Text.Trim();
        if (!int.TryParse(qtyText, out var qty) || qty <= 0)
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0\nกดปุ่มดินสอเพื่อแก้ไข Qty");
            return false;
        }

        return true;
    }

    private async Task ProcessBarcodeAsync(string barcode)
    {
        var (api, sqlite) = GetServices();

        // Pre-flight: check SQLite + backend
        if (!sqlite.CanConnect())
        {
            ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting");
            return;
        }

        if (!await api.PingAsync())
        {
            ShowError("ไม่สามารถเชื่อมต่อ Backend ได้\nกรุณาตรวจสอบ Backend Setting");
            return;
        }

        // ยังไม่ได้เลือก mydatabase.db3 → ระยะแคลมป์จะถูกเก็บเป็นค่าว่าง
        // เตือนแล้วให้เลือกเองว่าจะไปตั้งค่าก่อน หรือลงทะเบียนไปเลย
        if (!ConfirmClampDatabase()) return;

        // Step 1: Query SQLite
        var patternTemplate = sqlite.GetPatternDetail(barcode, 0);
        if (patternTemplate == null)
        {
            ShowWarning($"ไม่พบข้อมูลใน inkjet_data สำหรับ barcode: {barcode}");
            return;
        }

        var uvItems = sqlite.GetUvDetail(barcode);
        var planRouting = sqlite.GetPlanRouting(barcode, 0);

        // Step 2A: POST /job/create
        var jobRequest = new CreateJobRequest
        {
            BarcodeRaw = barcode,
            CreatedBy = "operator",
            OrderNo = txtOrderNo.Text.Trim(),
            // Qty ที่ส่งไปคือค่าที่โชว์อยู่บนจอ ซึ่งอาจถูกแก้ด้วยปุ่มดินสอแล้ว
            // ผลของการแก้จบที่ print_jobs แถวนี้แถวเดียว
            Type = txtMarkingMethod.Text.Trim(),
            Qty = int.TryParse(txtQty.Text.Trim(), out var q) ? q : null,
            StStatus = "0",
        };

        var (job, jobErr) = await api.CreateJobAsync(jobRequest);
        if (job == null)
        {
            ShowError($"สร้าง Job ไม่สำเร็จ\n{jobErr}");
            return;
        }

        // Step 2B: POST /pattern/create
        patternTemplate.JobId = job.Id;
        var (pattern, patErr) = await api.CreatePatternAsync(patternTemplate);
        if (pattern == null)
        {
            await api.DeleteJobAsync(job.Id);
            ShowError($"สร้าง Pattern ไม่สำเร็จ — Job ถูกลบแล้ว\n{patErr}");
            return;
        }

        // Step 2C: POST /uv-job/create (skip if no UV data)
        if (uvItems.Count > 0)
        {
            var uvRequest = new CreateUvJobRequest
            {
                PrintJobsId = job.Id,
                Items = uvItems,
            };
            var (uvOk, uvErr) = await api.CreateUvJobDataAsync(uvRequest);
            if (!uvOk)
            {
                ShowWarning($"บันทึก UV Data ไม่สำเร็จ แต่ Job + Pattern สร้างแล้ว\n{uvErr}");
            }
        }

        // Step 2D: POST /plan-routing/create (skip if lot has no plan_routing row)
        if (planRouting != null)
        {
            planRouting.PrintJobsId = job.Id;
            var (planOk, planErr) = await api.CreatePlanRoutingAsync(planRouting);
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
        await SyncIaiAsync(api, job.Id, uvItems);

        Notify.Success(this, $"สร้าง Job #{job.Id} สำเร็จ");

        ClearForm();
    }

    /// <summary>
    /// เตือนเมื่อยังไม่ได้เลือก mydatabase.db3 — ระยะแคลมป์จะถูกเก็บเป็นค่าว่าง
    /// ไม่บล็อกการลงทะเบียน เพราะงานที่ไม่ผ่าน UV ก็ไม่ต้องใช้ค่านี้
    /// คืน false = ผู้ใช้ขอไปตั้งค่าก่อน
    /// </summary>
    private static bool ConfirmClampDatabase()
    {
        var path = CustomSettingsManager.Read("CLAMP_DB_PATH", "");
        bool ready = !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        if (ready) return true;

        var reason = string.IsNullOrWhiteSpace(path)
            ? "ยังไม่ได้เลือกไฟล์ mydatabase.db3"
            : $"ไม่พบไฟล์ที่ตั้งไว้:\n{path}";

        return Confirm.Ask(null, "ยังไม่ได้ตั้งค่า Clamp Database",
            $"{reason}\n\n" +
            "ระยะแคลมป์ (IAI) ของงานนี้จะถูกบันทึกเป็นค่าว่าง\n" +
            "ตั้งค่าได้ที่ Setting → PLC UV Setting → Browse\n\n" +
            "ต้องการลงทะเบียนต่อไปหรือไม่?");
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
    private static async Task SyncIaiAsync(ApiClient api, int jobId, List<UvJobItem> uvItems)
    {
        var settings = ClampSettings.Load();
        bool canRead = !string.IsNullOrWhiteSpace(settings.DbPath) && File.Exists(settings.DbPath);

        var request = new IaiCreateRequest { PrintJobsId = jobId };

        foreach (var item in uvItems)
        {
            var program = (item.ProgramName ?? "").Trim();
            if (program.Length == 0) continue;

            // แยกช่องด้วย prefix ของชื่อโปรแกรม กฎเดียวกับ backend และระบบเดิม
            bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase);
            var side = isPlate ? ClampSide.Plate : ClampSide.Shim;

            if (isPlate) request.M1ProgramName = program;
            else request.M2ProgramName = program;

            // เก็บครบทุกแกนของฝั่งนี้ — ไม่มี path หรือหาไม่เจอก็เก็บเป็น null
            foreach (var axis in settings.For(side))
            {
                int? value = canRead
                    ? ClampService.Lookup(settings.DbPath, program, axis) is { Found: true } hit
                        ? hit.ValueMm
                        : null
                    : null;

                switch (axis.Key)
                {
                    case "IAIP": request.Iaip = value; break;
                    case "IAIPZ1": request.IaipZ1 = value; break;
                    case "IAIPZ2": request.IaipZ2 = value; break;
                    case "IAI": request.Iai = value; break;
                    case "IAIZ1": request.IaiZ1 = value; break;
                    case "IAIZ2": request.IaiZ2 = value; break;
                }
            }
        }

        // ไม่มีชื่อโปรแกรมเลย = งานนี้ไม่เกี่ยวกับ UV → ไม่ต้องสร้างแถว
        if (request.M1ProgramName == null && request.M2ProgramName == null) return;

        await api.CreateIaiAsync(request);
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        txtBarcode.Text = "";
        ClearLotInfo();
        txtBarcode.Focus();
    }

    /// <summary>ล้างเฉพาะข้อมูลที่ดึงมาจาก DB3 — ช่องบาร์โค้ดไม่ถูกแตะ</summary>
    private void ClearLotInfo()
    {
        _loadedBarcode = null;
        txtOrderNo.Text = "";
        txtMarkingMethod.Text = "";
        txtQty.Text = "";
        btnEditQty.Enabled = false;
    }

    private static void ShowWarning(string msg) =>
        Notify.WarnModal(null, "แจ้งเตือน", msg);

    private static void ShowError(string msg) =>
        Notify.ErrorModal(null, "Error", msg);
}
