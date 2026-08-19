using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class ScanBarcodeUserControl : UserControl
{
    private ApiClient? _api;
    private SqliteDataService? _sqlite;

    public ScanBarcodeUserControl()
    {
        InitializeComponent();
        btnConfirm.Click += BtnConfirm_Click;
        btnCancel.Click += BtnCancel_Click;
    }

    private (ApiClient api, SqliteDataService sqlite) GetServices()
    {
        var pcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        _api = new ApiClient($"http://{pcIp}:3000");

        var dbPath = CustomSettingsManager.Read("DB_PATH", "");
        _sqlite = new SqliteDataService(dbPath);

        return (_api, _sqlite);
    }

    private async void BtnConfirm_Click(object? sender, EventArgs e)
    {
        if (!ValidateForm()) return;

        btnConfirm.Enabled = false;
        try
        {
            await ProcessBarcodeAsync(txtBarcode.Text.Trim());
        }
        finally
        {
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

        if (string.IsNullOrWhiteSpace(txtOrderNo.Text))
        {
            ShowWarning("กรุณากรอก Order No");
            txtOrderNo.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            ShowWarning("กรุณากรอก Customer Name");
            txtCustomerName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtType.Text))
        {
            ShowWarning("กรุณากรอก Type");
            txtType.Focus();
            return false;
        }

        var qtyText = txtQty.Text.Trim();
        if (string.IsNullOrWhiteSpace(qtyText))
        {
            ShowWarning("กรุณากรอก Qty");
            txtQty.Focus();
            return false;
        }

        if (!int.TryParse(qtyText, out var qty) || qty <= 0)
        {
            ShowWarning("Qty ต้องเป็นตัวเลขจำนวนเต็มที่มากกว่า 0");
            txtQty.Focus();
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
            CustomerName = txtCustomerName.Text.Trim(),
            Type = txtType.Text.Trim(),
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
            "ตั้งค่าได้ที่ Setting → Clamp Setting → Browse\n\n" +
            "ต้องการลงทะเบียนต่อไปหรือไม่?");
    }

    /// <summary>
    /// บันทึกระยะแคลมป์ (IAI) ที่งานนี้ใช้ลง backend — 1 job = 1 แถว
    ///
    /// UV1 = Plate (ชื่อขึ้นต้น "P-") → iaip · UV2 = Shim → iai
    /// หาค่าไม่เจอก็ยังส่งขึ้นไป เก็บเป็น null เพื่อบอกว่า "หาแล้วไม่มี"
    /// ต่างจาก "ยังไม่เคยหา" ซึ่งคือไม่มีแถวเลย
    ///
    /// เป็นขั้นตอนเสริม ล้มเหลวก็ไม่กระทบการ register — job สร้างครบไปแล้ว
    /// </summary>
    private static async Task SyncIaiAsync(ApiClient api, int jobId, List<UvJobItem> uvItems)
    {
        var clampDbPath = CustomSettingsManager.Read("CLAMP_DB_PATH", "");
        bool canRead = !string.IsNullOrWhiteSpace(clampDbPath) && File.Exists(clampDbPath);

        var request = new IaiCreateRequest { PrintJobsId = jobId };

        foreach (var item in uvItems)
        {
            var program = (item.ProgramName ?? "").Trim();
            if (program.Length == 0) continue;

            // แยกช่องด้วย prefix ของชื่อโปรแกรม กฎเดียวกับ backend และระบบเดิม
            bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase);

            // ไม่มี path ก็ยังเก็บชื่อโปรแกรมไว้ ค่าเป็น null
            int? value = canRead
                ? ClampService.Lookup(clampDbPath, program) is { Found: true } hit
                    ? hit.ValueMm
                    : null
                : null;

            if (isPlate)
            {
                request.M1ProgramName = program;
                request.Iaip = value;
            }
            else
            {
                request.M2ProgramName = program;
                request.Iai = value;
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
        txtOrderNo.Text = "";
        txtCustomerName.Text = "";
        txtType.Text = "";
        txtQty.Text = "";
        txtBarcode.Focus();
    }

    private static void ShowWarning(string msg) =>
        Notify.Warn(null, msg);

    private static void ShowError(string msg) =>
        Notify.Error(null, msg);
}
