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
    public partial class ucInputOrder : UserControl
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

            _api = ApiProvider.Instance; // 🔥 ใช้จาก global
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
            picBarcode.Invalidate();
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
            // 0. Pre-flight: เช็ค Backend + DB ก่อนทำอะไรทั้งหมด
            btnOK.Enabled = false;
            try
            {
                if (!await CheckConnectionsAsync())
                    return;
            }
            finally
            {
                btnOK.Enabled = true;
            }

            // 1. Validate ข้อมูลทั้งหมดในฟังก์ชันเดียว
            if (!ValidateInput(out int qty))
                return;

            string barcodeRaw = txtBarcode.Text.Trim();
            string patternNo = GetPatternNo(barcodeRaw);

            // 2. ตรวจสอบข้อมูลใน SQLite
            var pattern = await _sqliteService.GetPatternDetailAsync(patternNo);
            if (pattern == null)
            {
                MessageBox.Show($"Pattern '{patternNo}' not registered", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Sync Pattern ไปยัง Backend
            bool patternReady = await SyncPatternAsync(pattern, patternNo);
            if (!patternReady)
            {
                MessageBox.Show("ไม่สามารถจัดเตรียมข้อมูล Pattern ในระบบหลักได้ กรุณาตรวจสอบการเชื่อมต่อหรือข้อมูล", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. สร้าง Job
            btnOK.Enabled = false;
            try
            {
                await ProcessCreateJobAsync(barcodeRaw, qty);
            }
            finally
            {
                btnOK.Enabled = true;
            }
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

            // 4. รูปแบบ Barcode ต้องมี '-' อย่างน้อย 1 ตัว
            string barcode = txtBarcode.Text.Trim();
            if (!barcode.Contains('-'))
            {
                ShowError(
                    "รูปแบบ Barcode ไม่ถูกต้อง!\n\n" +
                    "ต้องมีรูปแบบดังนี้:\n" +
                    "1. [Pattern]-[Sub]-[Lot] (เช่น xxxx-xxx-yyyyyy)\n" +
                    "2. [Pattern]-[Lot] (เช่น xxxxxx-yyyyy)\n\n" +
                    "* เครื่องหมาย '-' ตัวสุดท้ายจะถูกใช้เพื่อแยก Lot ออกจาก Pattern");
                txtBarcode.Focus();
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
        /// จัดการตัดสตริงเอาเฉพาะ Pattern Number
        /// </summary>
        private string GetPatternNo(string barcode)
        {
            int lastDashIndex = barcode.LastIndexOf('-');
            // ถ้าผ่าน Validation มาแล้ว lastDashIndex จะไม่มีทางเป็น -1
            return barcode.Substring(0, lastDashIndex);
        }

        /// <summary>
        /// ทำความสะอาดข้อมูลและส่ง Pattern ไปยัง Backend
        /// </summary>
        private async Task<bool> SyncPatternAsync(PatternDetail pattern, string pattern_barcode)
        {
            // 1. ลองหาใน Backend ก่อนว่ามี Pattern นี้หรือยัง
            var existing = await _api.GetPatternByBarcodeAsync(pattern_barcode);

            // ถ้ามีอยู่แล้ว ไม่ต้อง Add ซ้ำ ให้ถือว่าการ Sync สำเร็จ (Ready)
            if (existing != null)
            {
                Debug.WriteLine($"[SYNC] Pattern '{pattern_barcode}' already exists in backend. Skipping create.");

                // (Option) ถ้าคุณต้องการเอาข้อมูลจาก Backend มาทับตัวแปร local เพื่อใช้ค่าที่ Resolve แล้ว
                // pattern.InkjetConfigs = existing.InkjetConfigs; 

                return true;
            }

            // 2. ถ้าไม่มีใน Backend (existing == null) ให้เตรียมข้อมูลเพื่อ Create
            if (pattern.InkjetConfigs == null || pattern.InkjetConfigs.Count == 0)
                return false;

            // กรองและปรับจูนข้อมูล (Data Cleaning) ก่อนส่งไป Create
            pattern.InkjetConfigs = pattern.InkjetConfigs
                .Where(cfg => cfg.ProgramNumber.HasValue && cfg.ProgramNumber > 0)
                .Select(cfg =>
                {
                    if (cfg.TriggerDelay < 10) cfg.TriggerDelay = 10;
                    if (cfg.Direction != 0 && cfg.Direction != 3) cfg.Direction = 0;
                    if (cfg.SteelType == null) cfg.SteelType = "";
                    return cfg;
                }).ToList();

            // เช็คอีกครั้งหลังกรอง ถ้าว่างเปล่าไม่ควรส่ง
            if (pattern.InkjetConfigs.Count == 0) return false;

            // 3. ส่งไปที่ API เพื่อสร้าง Pattern ใหม่
            Debug.WriteLine($"[SYNC] Pattern '{pattern_barcode}' not found. Creating new pattern...");

            // ตรงนี้เรียก CreatePatternAsync ซึ่งคืนค่า bool (ตามโครงสร้างเดิมของคุณ)
            return await _api.CreatePatternAsync(pattern);
        }

        /// <summary>
        /// จัดการสร้าง Job และเรียก Event แจ้งเตือน
        /// </summary>
        private async Task ProcessCreateJobAsync(string barcode, int qty)
        {
            var req = new CreateJobRequest
            {
                BarcodeRaw = barcode,
                OrderNo = txtOrderNo.Text,
                CustomerName = txtCustomerName.Text,
                Type = txtType.Text,
                Qty = qty,
                CreatedBy = "operator",
                st_status = "0"
            };

            var success = await _api.CreateJobAsync(req);

            if (success)
            {
                MessageBox.Show("Create job success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //BarcodeScanned?.Invoke(this, new BarcodeScanEventArgs
                //{
                //    Barcode = req.BarcodeRaw,
                //    OrderNo = req.OrderNo,
                //    CustomerName = req.CustomerName,
                //    Type = req.Type,
                //    Qty = req.Qty.ToString()
                //});

                ClearForm();
            }
            else
            {
                MessageBox.Show("Create job failed: ไม่สามารถสร้างงานในระบบหลักได้", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
            ClearForm();
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
            lblScanStatus.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        }

        private void picBarcode_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new SolidBrush(picBarcode.BackColor))
            {
                g.FillRectangle(brush, picBarcode.ClientRectangle);
            }

            using (var pen = new Pen(Color.White, 3))
            {
                var rect = new Rectangle(1, 1, picBarcode.Width - 3, picBarcode.Height - 3);
                using (var path = GetRoundedRect(rect, 20))
                {
                    g.DrawPath(pen, path);
                }
            }

            DrawBarcodeLines(g);
        }

        private void DrawBarcodeLines(Graphics g)
        {
            int startX = 50;
            int startY = 25;
            int height = 50;
            Random rand = new Random(42);

            for (int i = 0; i < 20; i++)
            {
                int width = rand.Next(2, 6);
                using (var brush = new SolidBrush(Color.Black))
                {
                    g.FillRectangle(brush, startX + (i * 10), startY, width, height);
                }
            }

            string displayText = string.IsNullOrEmpty(_lastScannedBarcode)
                ? "SCAN HERE"
                : _lastScannedBarcode.Substring(0, Math.Min(15, _lastScannedBarcode.Length));

            using (var font = new Font("Consolas", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.Black))
            {
                var size = g.MeasureString(displayText, font);
                g.DrawString(displayText, font, brush,
                    (picBarcode.Width - size.Width) / 2, startY + height + 8);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {

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