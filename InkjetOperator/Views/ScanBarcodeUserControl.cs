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

    /// <summary>
    /// ชื่อลูกค้าจาก inkjet_data.customer ของ lot ที่สแกน
    ///
    /// หน้านี้ไม่มีช่องให้เห็นและไม่ให้แก้ — แค่ติดไปกับงานตอนลงทะเบียน
    /// แล้วไปโผล่ที่หน้า Order Detail ที่เดียว
    /// </summary>
    private string? _customerName;

    /// <summary>
    /// หยุดพิมพ์นานเท่านี้ (มิลลิวินาที) แล้วโปรแกรมจะดึงข้อมูลให้เอง
    ///
    /// ยาวพอให้พิมพ์เลขทีละตัวด้วยนิ้วบนทัชสกรีนไม่โดนขัดจังหวะ และสั้นพอที่จะ
    /// ไม่รู้สึกว่าต้องรอ · เครื่องสแกนพิมพ์รวดเดียวจบแล้วปิดท้ายด้วย Enter อยู่แล้ว
    /// จึงไม่ต้องรอครบเวลานี้
    /// </summary>
    private const int AutoLookupDelayMs = 600;

    private System.Windows.Forms.Timer? _autoLookupTimer;

    public ScanBarcodeUserControl()
    {
        InitializeComponent();
        btnConfirm.Click += BtnConfirm_Click;
        btnClear.Click += BtnClear_Click;
        btnEditQty.Click += BtnEditQty_Click;
        txtBarcode.KeyDown += TxtBarcode_KeyDown;
        txtBarcode.TextChanged += TxtBarcode_TextChanged;
        txtBarcode.GotFocus += (_, _) => UseEnglishKeyboard();
        txtBarcode.LostFocus += (_, _) => RestoreKeyboard();

        // จอที่หน้างานเป็นทัชสกรีน ไม่มีคีย์บอร์ดให้กด Enter — โปรแกรมจึงต้องดึงข้อมูล
        // ให้เองเมื่อพนักงานพิมพ์เลขเสร็จ ไม่ใช่รอให้สั่ง
        _autoLookupTimer = new System.Windows.Forms.Timer { Interval = AutoLookupDelayMs };
        _autoLookupTimer.Tick += AutoLookupTimer_Tick;

        Disposed += (_, _) =>
        {
            _autoLookupTimer?.Stop();
            _autoLookupTimer?.Dispose();
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
    public void FocusBarcode()
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            if (IsDisposed || !txtBarcode.Visible) return;
            txtBarcode.Focus();
        });
    }

    /// <summary>
    /// คืนเคอร์เซอร์ให้ช่องบาร์โค้ด และเลือกข้อความเดิมไว้ทั้งหมด
    /// <para>
    /// เรียกหลังจบทุกการกระทำบนหน้านี้ (ดึงข้อมูล, กด OK, แก้ Qty, กล่องเตือน)
    /// เดิมหลังกดปุ่มหรือปิดกล่อง โฟกัสค้างอยู่ที่ปุ่ม — โดยเฉพาะตอนกด OK ซึ่งปุ่มถูกปิด
    /// ชั่วคราวระหว่างทำงาน โฟกัสเลยกระโดดไปปุ่ม Clear ยิงบาร์โค้ดใบถัดไปแล้วหายเงียบ
    /// </para>
    /// <para>
    /// เลือกข้อความเดิมไว้ เพราะเครื่องสแกนพิมพ์ต่อท้ายของที่อยู่ในช่อง ยิงใบใหม่ทับ
    /// ใบที่หาไม่เจอหรือใบที่ดึงข้อมูลแล้วจะกลายเป็น "C260721-001C260721-002"
    /// การเลือกไว้ทำให้ใบใหม่เขียนทับแทน พนักงานไม่ต้องกด Clear ก่อน
    /// </para>
    /// </summary>
    private void ReturnToBarcode()
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            if (IsDisposed || !txtBarcode.Visible) return;
            txtBarcode.Focus();
            txtBarcode.SelectAll();
        });
    }

    /// <summary>ภาษาแป้นพิมพ์ก่อนช่องบาร์โค้ดได้โฟกัส — คืนให้ตอนออกจากช่อง</summary>
    private InputLanguage? _keyboardBeforeScan;

    /// <summary>
    /// สลับแป้นพิมพ์เป็นภาษาอังกฤษตอนอยู่ในช่องบาร์โค้ด
    /// <para>
    /// เครื่องสแกนแบบ keyboard wedge ส่ง "ปุ่มที่กด" ไม่ใช่ตัวอักษร ถ้า Windows อยู่ภาษาไทย
    /// C260721-001 จะกลายเป็น "ฉ/ุจึ/ๅขจจๅ" แล้วหาไม่เจอทุกใบ (ทดสอบแล้วเป็นแบบนี้จริง)
    /// </para>
    /// <para>
    /// ไม่มีภาษาอังกฤษติดตั้งในเครื่องก็ไม่ทำอะไร สลับไม่ได้ก็ปล่อยไว้ ไม่ให้ช่องใช้ไม่ได้
    /// </para>
    /// </summary>
    private void UseEnglishKeyboard()
    {
        try
        {
            var current = InputLanguage.CurrentInputLanguage;
            if (current.Culture.TwoLetterISOLanguageName == "en") return;

            var english = InputLanguage.InstalledInputLanguages.Cast<InputLanguage>()
                .FirstOrDefault(l => l.Culture.TwoLetterISOLanguageName == "en");
            if (english == null) return;

            _keyboardBeforeScan = current;
            InputLanguage.CurrentInputLanguage = english;
        }
        catch
        {
            // สลับไม่ได้ก็พิมพ์ด้วยภาษาเดิมไป
        }
    }

    /// <summary>ออกจากช่องบาร์โค้ด — คืนภาษาเดิมที่ผู้ใช้ตั้งไว้</summary>
    private void RestoreKeyboard()
    {
        var previous = _keyboardBeforeScan;
        _keyboardBeforeScan = null;
        if (previous == null) return;

        try { InputLanguage.CurrentInputLanguage = previous; }
        catch { }
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

        LoadLot(quiet: false);
    }

    /// <summary>
    /// พิมพ์เสร็จแล้ว (หยุดพิมพ์ครบเวลา) — ลองดึงข้อมูลให้เองแบบเงียบ ๆ
    /// หาไม่เจอก็ไม่ต้องบอกอะไร เพราะอาจแค่ยังพิมพ์ไม่ครบ
    /// </summary>
    private void AutoLookupTimer_Tick(object? sender, EventArgs e) => LoadLot(quiet: true);

    /// <summary>
    /// แก้บาร์โค้ดเมื่อไหร่ ข้อมูลที่โชว์อยู่ก็ไม่ใช่ของ lot ในช่องอีกต่อไป
    /// ล้างทิ้งทันทีเพื่อไม่ให้เผลอลงทะเบียนด้วยข้อมูลค้าง
    /// </summary>
    private void TxtBarcode_TextChanged(object? sender, EventArgs e)
    {
        // ตั้งนาฬิกาใหม่ทุกตัวอักษร — จะยิงก็ต่อเมื่อหยุดพิมพ์จริง ๆ
        // เครื่องสแกนพิมพ์รัวจึงไม่มีทางยิงกลางคัน
        _autoLookupTimer?.Stop();

        if (_loadedBarcode != null && txtBarcode.Text.Trim() != _loadedBarcode)
            ClearLotInfo();

        if (txtBarcode.Text.Trim().Length > 0) _autoLookupTimer?.Start();
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
    private bool LoadLot(bool quiet)
    {
        _autoLookupTimer?.Stop();

        var barcode = txtBarcode.Text.Trim();
        if (string.IsNullOrWhiteSpace(barcode))
        {
            if (!quiet)
            {
                ShowWarning("กรุณาสแกนหรือพิมพ์ Barcode");
                ReturnToBarcode();
            }
            return false;
        }

        var sqlite = OpenSourceDb();
        if (!sqlite.CanConnect())
        {
            if (!quiet)
            {
                ShowError("ไม่สามารถเชื่อมต่อ PrintData.db3 ได้\nกรุณาตรวจสอบ Database Path ใน Setting");
                ReturnToBarcode();
            }
            return false;
        }

        LotSummary? lot;
        try
        {
            lot = sqlite.GetLotSummary(barcode);
        }
        catch (Exception ex)
        {
            // PrintData.db3 อยู่บนไดรฟ์เครือข่าย หลุดกลางคันได้แม้ CanConnect เพิ่งผ่าน
            AppLog.Error("ScanBarcode.LoadLot", ex);
            if (!quiet)
            {
                ShowError(ReadFailedMessage(ex));
                ReturnToBarcode();
            }
            return false;
        }

        if (lot == null)
        {
            if (quiet) return false;

            ClearLotInfo();
            ShowWarning($"ไม่พบข้อมูลใน print_data สำหรับ barcode: {barcode}");
            ReturnToBarcode();
            return false;
        }

        // ช่องไหนไม่มีค่าใน DB3 ก็ปล่อยว่างไว้ ไม่เตือน — ช่องว่างบอกตัวมันเองอยู่แล้ว
        // และการเตือนตอนนี้จะไปขวางจังหวะสแกนงานถัดไปของพนักงาน
        _loadedBarcode = barcode;
        _customerName = lot.Customer;
        txtErpMfg.Text = lot.ErpMfg ?? "";
        txtMarkingMethod.Text = lot.MarkingMethod ?? "";
        txtQty.Text = lot.Qty?.ToString() ?? "";
        btnEditQty.Enabled = true;

        // ยิงใบถัดไปได้เลย เขียนทับใบนี้ ไม่ต่อท้าย
        ReturnToBarcode();
        return true;
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
        try
        {
            EditQty();
        }
        finally
        {
            ReturnToBarcode();
        }
    }

    private void EditQty()
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
        if (!ValidateForm())
        {
            // ยังไม่ลงทะเบียน (ดึงข้อมูลให้ดูก่อน / Qty ไม่ถูก) — โฟกัสยังค้างที่ปุ่ม OK อยู่
            ReturnToBarcode();
            return;
        }

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
            ReturnToBarcode();
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

        // ยังไม่ได้ดึงข้อมูลของบาร์โค้ดนี้ — ดึงให้เลยตรงนี้ ไม่ต้องให้ไปกด Enter
        // (จอทัชสกรีนไม่มีคีย์บอร์ด) หาไม่เจอ LoadLot จะบอกสาเหตุเอง
        if (_loadedBarcode != txtBarcode.Text.Trim())
        {
            if (!LoadLot(quiet: false)) return false;

            // เจอแล้วแต่ยังไม่ลงทะเบียนรอบนี้ ให้ดูข้อมูลที่เพิ่งขึ้นมาก่อน
            // แล้วค่อยกด OK อีกครั้ง — กันลงทะเบียนงานที่ยังไม่มีใครเห็นตัวเลข
            Notify.Info(this, "ดึงข้อมูลแล้ว — ตรวจสอบแล้วกด OK อีกครั้งเพื่อลงทะเบียน");
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
        //
        // อ่านให้ครบทั้งสามส่วนก่อนสร้างอะไรใน backend — ถ้าไดรฟ์หลุดกลางทาง
        // จะได้หยุดก่อนมีงานครึ่ง ๆ กลาง ๆ ค้างอยู่ในระบบ
        CreatePatternRequest? patternTemplate;
        List<UvJobItem> uvItems;
        CreatePlanRoutingRequest? planRouting;
        try
        {
            patternTemplate = sqlite.GetPatternDetail(barcode, 0);
            uvItems = sqlite.GetUvDetail(barcode);
            planRouting = sqlite.GetPlanRouting(barcode, 0);
        }
        catch (Exception ex)
        {
            AppLog.Error("ScanBarcode.ProcessBarcode", ex);
            ShowError(ReadFailedMessage(ex));
            return;
        }

        if (patternTemplate == null)
        {
            ShowWarning($"ไม่พบข้อมูลใน inkjet_data สำหรับ barcode: {barcode}");
            return;
        }

        // Step 2A: POST /job/create
        var jobRequest = new CreateJobRequest
        {
            BarcodeRaw = barcode,
            CreatedBy = "operator",
            OrderNo = txtErpMfg.Text.Trim(),
            // ชื่อลูกค้าไม่มีช่องบนหน้านี้ ดึงมาจาก inkjet_data.customer ตอนสแกน
            // แล้วติดไปกับงานเฉย ๆ ไปโผล่ที่หน้า Order Detail
            CustomerName = _customerName,
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

        Notify.Success(this,
            $"สร้างงาน {Services.JobDisplay.Label(job.OrderNo, job.LotNumber ?? job.BarcodeRaw, job.Id)} สำเร็จ");

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
            : $"ไม่พบไฟล์ที่ตั้งไว้: {path}";

        return Confirm.Ask(null, new ConfirmSpec
        {
            Title = "ยังไม่ได้ตั้งค่า Clamp Database",
            Headline = "ลงทะเบียนต่อโดยไม่มีระยะแคลมป์?",
            Message = reason,
            Detail = "ระยะแคลมป์ (IAI) ของงานนี้จะเป็นค่าว่าง" + Environment.NewLine
                + "ตั้งค่าได้ที่ Setting → PLC UV Setting → Browse",
            OkText = "ลงทะเบียนต่อ",
            CancelText = "ไปตั้งค่าก่อน",
            Icon = "ExclamationCircleFilled",
        });
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

    private void BtnClear_Click(object? sender, EventArgs e)
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
        _customerName = null;
        txtErpMfg.Text = "";
        txtMarkingMethod.Text = "";
        txtQty.Text = "";
        btnEditQty.Enabled = false;
    }

    private static string ReadFailedMessage(Exception ex) =>
        "อ่าน PrintData.db3 ไม่ได้\n\n"
        + "ตรวจว่าไดรฟ์ที่เก็บไฟล์ยังเชื่อมต่ออยู่ แล้วลองสแกนใหม่\n"
        + ex.Message;

    private static void ShowWarning(string msg) =>
        Notify.WarnModal(null, "แจ้งเตือน", msg);

    private static void ShowError(string msg) =>
        Notify.ErrorModal(null, "เกิดข้อผิดพลาด", msg);
}
