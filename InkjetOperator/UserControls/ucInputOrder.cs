using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucInputOrder : UserControl, ILocalizable
    {
        public event EventHandler<BarcodeScanEventArgs>? BarcodeScanned;
        public event EventHandler? Cancelled;
        private readonly SqliteDataService _sqliteService = new SqliteDataService();

        private ApiClient _api;

        public string BarcodeRaw => txtBarcode.Text.Trim();
        public string OrderNo => txtOrderNo.Text.Trim();
        public string CustomerName => txtCustomerName.Text.Trim();
        public string Type => txtType.Text.Trim();
        public string Qty => txtQty.Text.Trim();

        private string _lastScannedBarcode = "";
        private StringBuilder _barcodeBuffer = new StringBuilder();
        private DateTime _lastKeyTime;

        public ucInputOrder()
        {
            InitializeComponent();
            SetupEvents();
            this.Dock = DockStyle.Fill;

            _api = ApiProvider.Instance;
            ApplyLanguage();

            pnlMain.Resize += (s, e) => CenterControls();
            CenterControls();
        }

        private void CenterControls()
        {
            int cx = pnlMain.Width / 2;
            lblTitle.Left = cx - lblTitle.Width / 2;
            picBarcode.Left = cx - picBarcode.Width / 2;
            pnlFormContainer.Left = cx - pnlFormContainer.Width / 2;
            pnlButtons.Left = cx - pnlButtons.Width / 2;
        }

        public void ApplyLanguage()
        {
            lblTitle.Text = Lang.Get("input.title");
            lblBarcode.Text = Lang.Get("input.barcode");
            lblOrderNo.Text = Lang.Get("input.order_no");
            lblCustomerName.Text = Lang.Get("input.customer");
            lblType.Text = Lang.Get("input.type");
            lblQty.Text = Lang.Get("input.qty");
            btnOK.Text = Lang.Get("input.ok");
            btnCancel.Text = Lang.Get("input.cancel");
            lblScanStatus.Text = Lang.Get("input.scan_wait");
        }

        private void SetupEvents()
        {
            this.KeyDown += ucInputOrder_KeyDown;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            txtBarcode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtOrderNo.Focus(); };
            txtOrderNo.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtCustomerName.Focus(); };
            txtCustomerName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtType.Focus(); };
            txtType.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtQty.Focus(); };
            txtQty.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnOK.PerformClick(); };
        }

        private void ucInputOrder_KeyDown(object? sender, KeyEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - _lastKeyTime;
            _lastKeyTime = DateTime.Now;

            bool isScanner = elapsed.TotalMilliseconds < 100 && _barcodeBuffer.Length > 0;

            if (e.KeyCode == Keys.Enter)
            {
                string scanned = _barcodeBuffer.ToString();
                _barcodeBuffer.Clear();

                if (!string.IsNullOrEmpty(scanned))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    ProcessScannedBarcode(scanned, isScanner);
                }
            }
            else if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.Z) ||
                     (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
            {
                char c = (char)e.KeyValue;
                if (!e.Shift) c = char.ToLower(c);
                _barcodeBuffer.Append(c);
            }
            else if (e.KeyCode == Keys.Space)
            {
                _barcodeBuffer.Append(' ');
            }
        }

        private void ProcessScannedBarcode(string barcode, bool fromScanner)
        {
            _lastScannedBarcode = barcode;
            txtBarcode.Text = barcode;
            txtBarcode.BackColor = Color.FromArgb(220, 255, 220);

            UpdateScanStatus(barcode, fromScanner);
            System.Media.SystemSounds.Beep.Play();
            AutoFillFromBarcode(barcode);
        }

        private void UpdateScanStatus(string barcode, bool fromScanner)
        {
            lblScanStatus.Text = $"✓ สแกนแล้ว: {barcode.Substring(0, Math.Min(25, barcode.Length))}";
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.BackColor = Color.FromArgb(0, 150, 0);
            lblScanStatus.Padding = new Padding(5);

            var flashTimer = new System.Windows.Forms.Timer { Interval = 150 };
            int flashCount = 0;

            flashTimer.Tick += (s, e) =>
            {
                flashCount++;
                lblScanStatus.BackColor = flashCount % 2 == 0
                    ? Color.FromArgb(0, 150, 0)
                    : Color.FromArgb(50, 200, 50);

                if (flashCount >= 6)
                {
                    flashTimer.Stop();
                    flashTimer.Dispose();
                    lblScanStatus.BackColor = Color.FromArgb(0, 150, 0);
                }
            };
            flashTimer.Start();

            picBarcode.BackColor = fromScanner
                ? Color.FromArgb(200, 255, 200)
                : Color.FromArgb(230, 240, 250);
        }

        private void AutoFillFromBarcode(string barcode)
        {
            var parts = barcode.Split('|');

            if (parts.Length >= 1)
                txtOrderNo.Text = parts[0];
            if (parts.Length >= 2)
                txtCustomerName.Text = parts[1];
            if (parts.Length >= 3)
                txtType.Text = parts[2];
            if (parts.Length >= 4 && int.TryParse(parts[3], out _))
                txtQty.Text = parts[3];

            if (string.IsNullOrEmpty(txtCustomerName.Text))
                txtCustomerName.Focus();
            else if (string.IsNullOrEmpty(txtType.Text))
                txtType.Focus();
            else if (string.IsNullOrEmpty(txtQty.Text))
                txtQty.Focus();
            else
                btnOK.Focus();
        }

        private async void BtnOK_Click(object? sender, EventArgs e)
        {
            btnOK.Enabled = false;
            try
            {
                // 1. Pre-flight: เช็ค Backend + DB ก่อนทำอะไรทั้งหมด
                if (!await CheckConnectionsAsync())
                    return;

                // 2. Validate ข้อมูลหน้าจอ
                if (!ValidateInput(out int qty))
                    return;

                string barcodeRaw = txtBarcode.Text.Trim();

                // 3. ต้องมีข้อมูลใน inkjet_data (MK) หรือ print_data (UV) อย่างน้อยหนึ่ง
                var pattern = await _sqliteService.GetPatternDetailAsync(barcodeRaw);
                var uvRows = await _sqliteService.GetUvDetailAsync(barcodeRaw);

                if (pattern == null && uvRows.Count == 0)
                {
                    MessageBox.Show(
                        $"ไม่พบข้อมูล Barcode '{barcodeRaw}'\n\n" +
                        "ไม่มีทั้งใน inkjet_data (MK) และ print_data (UV)\n\n" +
                        "สาเหตุที่เป็นไปได้:\n" +
                        "• ยังไม่ได้ลงทะเบียน lot_no นี้ในระบบ\n" +
                        "• พิมพ์/สแกน Barcode ผิด\n" +
                        "• ข้อมูลถูกลบหรือยังไม่ได้ Sync",
                        "ไม่พบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RegisterJobAsync(barcodeRaw, qty, pattern, uvRows);
            }
            finally
            {
                btnOK.Enabled = true;
            }
        }

        /// <summary>
        /// สร้าง job → สร้าง pattern (สำเนาค่าที่ใช้พิมพ์) ผูกกับ job → เก็บ UV detail
        ///
        /// ลำดับสำคัญ: job ต้องมาก่อน เพราะ pattern ผูกด้วย job_id
        /// pattern ล้ม = ลบ job ทิ้ง ไม่ปล่อยงานที่ไม่มีค่าพิมพ์ค้างให้ operator กดส่ง
        /// </summary>
        private async Task RegisterJobAsync(string barcode, int qty,
                                            PatternDetail? pattern, List<UvJobData> uvRows)
        {
            var job = await _api.CreateJobAsync(new CreateJobRequest
            {
                BarcodeRaw = barcode,
                OrderNo = txtOrderNo.Text,
                CustomerName = txtCustomerName.Text,
                Type = txtType.Text,
                Qty = qty,
                CreatedBy = "operator",
                st_status = "0"
            });

            if (job == null)
            {
                MessageBox.Show($"สร้างงานไม่สำเร็จ:\n{_api.LastError}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // งาน UV-only ไม่มีข้อมูลใน inkjet_data → ผูก pattern เปล่าไว้ให้ครบโครงสร้าง
            pattern ??= new PatternDetail { Barcode = barcode };
            pattern.JobId = job.Id;

            string? patternError = await SyncPatternAsync(pattern, barcode);
            if (patternError != null)
            {
                await _api.DeleteJobAsync(job.Id);
                MessageBox.Show($"{patternError}\n\n(ยกเลิกงานที่สร้างไว้แล้ว)",
                    "Pattern Sync Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // UV detail — ไม่มีก็ข้ามได้ ไม่ล้ม register
            await SyncUvJobDataAsync(job.Id, barcode);

            MessageBox.Show("Create job success", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        /// <summary>Validate ข้อมูลหน้าจอทั้งหมด: Barcode, OrderNo, Qty, รูปแบบ Barcode</summary>
        private bool ValidateInput(out int qty)
        {
            qty = 0;

            // 1. Barcode ต้องไม่ว่าง
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                ShowError("กรุณาสแกนบาร์โค้ด");
                txtBarcode.Focus();
                return false;
            }

            // 2. Order No ต้องไม่ว่าง
            if (string.IsNullOrWhiteSpace(txtOrderNo.Text))
            {
                ShowError("กรุณาระบุ Order No");
                txtOrderNo.Focus();
                return false;
            }

            // 3. Qty ต้องเป็นตัวเลข
            if (!int.TryParse(txtQty.Text, out qty) || qty <= 0)
            {
                ShowError("Qty ต้องเป็นตัวเลขที่มากกว่า 0");
                txtQty.Focus();
                return false;
            }

            return true;
        }

        /// <summary>เช็ค Backend API + SQLite DB ก่อนดำเนินการ</summary>
        private async Task<bool> CheckConnectionsAsync()
        {
            // 1. เช็ค SQLite DB Path (sync — เร็วมาก)
            if (!_sqliteService.CanConnect())
            {
                string dbPath = CustomSettingsManager.GetValue("DB_PATH") ?? "(ไม่ได้ตั้งค่า)";
                MessageBox.Show(
                    $"❌ ไม่สามารถเชื่อมต่อฐานข้อมูล SQLite ได้\n\n" +
                    $"DB Path: {dbPath}\n\n" +
                    "กรุณาตรวจสอบ:\n" +
                    "• ไฟล์ .db3 มีอยู่จริงหรือไม่\n" +
                    "• ตั้งค่า DB_PATH ในหน้า Setting ถูกต้องหรือไม่",
                    "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 2. เช็ค Backend API (async — อาจใช้เวลา 1-2 วินาที)
            bool backendOk = await _api.PingAsync();
            if (!backendOk)
            {
                string pcIp = AppConfig.PcIp;
                MessageBox.Show(
                    $"❌ ไม่สามารถเชื่อมต่อ Backend Server ได้\n\n" +
                    $"PC IP: {pcIp}\n" +
                    $"URL: {AppConfig.ApiUrl}\n\n" +
                    "กรุณาตรวจสอบ:\n" +
                    "• Backend Server เปิดอยู่หรือไม่\n" +
                    "• ตั้งค่า PC_IP ในหน้า Setting ถูกต้องหรือไม่\n" +
                    "• เครือข่ายเชื่อมต่อได้หรือไม่",
                    "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// สร้าง pattern ใหม่เสมอ — 1 job มี 1 pattern เป็นสำเนาของตัวเอง
        /// lot เดิม register ซ้ำจึงได้ค่าล่าสุดจาก inkjet_data ทุกครั้ง ไม่ใช้ของเก่าค้าง
        /// คืน null = สำเร็จ, คืน string = สาเหตุที่ล้มเหลว (เอาไปโชว์ได้เลย)
        /// </summary>
        private async Task<string?> SyncPatternAsync(PatternDetail pattern, string pattern_barcode)
        {
            // งาน UV-only อาจไม่มี MK config → pattern เปล่าก็สร้างได้ ไม่ block การ register
            pattern.InkjetConfigs = (pattern.InkjetConfigs ?? new())
                .Where(cfg => cfg.ProgramNumber.HasValue && cfg.ProgramNumber > 0)
                .Select(cfg =>
                {
                    if (cfg.TriggerDelay < 10) cfg.TriggerDelay = 10;
                    if (cfg.Direction != 0 && cfg.Direction != 3) cfg.Direction = 0;
                    if (cfg.SteelType == null) cfg.SteelType = "";
                    return cfg;
                }).ToList();

            Debug.WriteLine($"[SYNC] Creating pattern '{pattern_barcode}' for job {pattern.JobId} — " +
                $"{pattern.InkjetConfigs.Count} MK config(s), " +
                $"conveyor={pattern.ConveyorSpeeds != null}, servos={pattern.ServoConfigs?.Count ?? 0}");

            bool ok = await _api.CreatePatternAsync(pattern);
            if (!ok)
                return $"สร้าง Pattern '{pattern_barcode}' ไม่สำเร็จที่ backend:\n{_api.LastError}";

            return null;
        }

        /// <summary>query print_data (lot_no) → เก็บ UV detail (UV1/UV2) ลง backend ผูกกับ job — ไม่มีข้อมูล UV ก็ข้ามได้ ไม่ล้ม register</summary>
        private async Task SyncUvJobDataAsync(int jobId, string lot)
        {
            try
            {
                var uvRows = await _sqliteService.GetUvDetailAsync(lot);
                if (uvRows.Count == 0) return; // ไม่มี lot ใน print_data → ข้าม UV

                await _api.CreateUvJobDataAsync(jobId, uvRows);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncUvJobData error: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
        }

        private void ClearForm()
        {
            txtBarcode.Clear();
            txtOrderNo.Clear();
            txtCustomerName.Clear();
            txtType.Clear();
            txtQty.Clear();
            txtBarcode.BackColor = Color.WhiteSmoke;
            lblScanStatus.Text = "📷 รอสแกนบาร์โค้ด...";
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.BackColor = Color.Transparent;
            _lastScannedBarcode = "";
            txtBarcode.Focus();
        }

        private void ShowError(string message)
        {
            lblScanStatus.Text = $"✗ {message}";
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.BackColor = Color.FromArgb(200, 50, 50);
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void SimulateBarcodeScan(string barcode)
        {
            ProcessScannedBarcode(barcode, false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtBarcode.Focus();
            lblScanStatus.Text = "📷 รอสแกนบาร์โค้ด...";
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.BackColor = Color.Transparent;
            lblScanStatus.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        }


        private void btnOK_Click_1(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "ต้องการล้างข้อมูลทั้งหมดหรือไม่?",
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                ClearForm();
        }
    }

    public class BarcodeScanEventArgs : EventArgs
    {
        public string Barcode { get; set; } = "";
        public string OrderNo { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Type { get; set; } = "";
        public string Qty { get; set; } = "";
    }
}