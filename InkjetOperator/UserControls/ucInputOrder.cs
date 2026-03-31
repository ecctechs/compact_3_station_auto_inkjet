using System;
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

            _api = new ApiClient("http://localhost:3000");
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

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (ValidateInput())
            {
                BarcodeScanned?.Invoke(this, new BarcodeScanEventArgs
                {
                    Barcode = BarcodeRaw,
                    OrderNo = OrderNo,
                    CustomerName = CustomerName,
                    Type = Type,
                    Qty = Qty
                });

                ClearForm();
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

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                ShowError("กรุณาสแกนบาร์โค้ด");
                txtBarcode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOrderNo.Text))
            {
                ShowError("กรุณาระบุ Order No");
                txtOrderNo.Focus();
                return false;
            }

            return true;
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

        private async void btnOK_Click_1(object sender, EventArgs e)
        {
            await CreateJob();
        }

        private async Task CreateJob()
        {
            var req = new CreateJobRequest
            {
                BarcodeRaw = "CMSS0297-DPX0839MCS-LOT12345",
                OrderNo = "PO0022",
                CustomerName = "ABC Corp",
                Type = "I",
                Qty = 100,
                CreatedBy = "operator"
            };

            var success = await _api.CreateJobAsync(req);

            if (success)
            {
                MessageBox.Show("Create job success");
            }
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