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

        // Step 1: Query SQLite
        var patternTemplate = sqlite.GetPatternDetail(barcode, 0);
        if (patternTemplate == null)
        {
            ShowWarning($"ไม่พบข้อมูลใน inkjet_data สำหรับ barcode: {barcode}");
            return;
        }

        var uvItems = sqlite.GetUvDetail(barcode);

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

        MessageBox.Show(
            $"สร้าง Job #{job.Id} สำเร็จ",
            "สำเร็จ",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        ClearForm();
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
        MessageBox.Show(msg, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void ShowError(string msg) =>
        MessageBox.Show(msg, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
