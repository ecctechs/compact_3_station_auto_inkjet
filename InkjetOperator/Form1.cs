using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.UserControls;

namespace InkjetOperator
{
    public partial class Form1 : Form
    {
        private AppConfig _config;
        private ucInputOrder? _ucInput;
        //private ucSetting? _ucSetting;
        private ucSettingMenu? _ucSettingMenu;

        // เก็บ reference ของปุ่มเมนู
        private Button? _btnInput;
        private Button? _btnOrder;
        private Button? _btnEdit;
        private Button? _btnSetting;
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appconfig.json");

        public Form1()
        {
            InitializeComponent();

            Debug.WriteLine($"[DEBUG] Config Path: {ConfigPath}");

            // โหลด config
            _config = AppConfig.Load();

            // สร้างเมนูตาม config
            CreateMenuByConfig();

            // แสดงหน้าแรก
            ShowFirstAvailablePage();
        }

        private void CreateMenuByConfig()
        {
            // ลบเฉพาะปุ่มเมนูเดิม (Button) ออกจาก pnlMenu
            for (int i = pnlMenu.Controls.Count - 1; i >= 0; i--)
            {
                if (pnlMenu.Controls[i] is Button)
                    pnlMenu.Controls.RemoveAt(i);
            }

            int x = 10;
            int index = 0;

            Debug.WriteLine($"[DEBUG] MenuMode: {_config.MenuMode}");


            // Input Order (แสดงเสมอในโหมด 1 และ 2)
            if (_config.ShouldShowMenu("input"))
            {
                _btnInput = CreateMenuButton("Input Order", x, index == 0);
                _btnInput.Click += (s, e) => { ShowInputOrder(); SetActiveButton(_btnInput); };
                x += 140;
                index++;
            }

            // Order List (แสดงเฉพาะโหมด 2)
            if (_config.ShouldShowMenu("order"))
            {
                _btnOrder = CreateMenuButton("Order List", x, index == 0);
                _btnOrder.Click += (s, e) => { ShowOrderList(); SetActiveButton(_btnOrder); };
                x += 140;
                index++;
            }

            // Edit Pattern (แสดงเฉพาะโหมด 2)
            if (_config.ShouldShowMenu("edit"))
            {
                _btnEdit = CreateMenuButton("Edit Pattern", x, index == 0);
                _btnEdit.Click += (s, e) => { ShowEditPattern(); SetActiveButton(_btnEdit); };
                x += 140;
                index++;
            }

            // Setting (แสดงเสมอในโหมด 1 และ 2)
            if (_config.ShouldShowMenu("setting"))
            {
                _btnSetting = CreateMenuButton("Setting", x, index == 0);
                _btnSetting.Click += (s, e) => { ShowSetting(); SetActiveButton(_btnSetting); };
            }

            // อัปเดตข้อความ title ตาม config
            this.Text = _config.AppName;
        }

        private Button CreateMenuButton(string text, int x, bool isActive)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, 10);
            btn.Size = new Size(130, 40);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btn.ForeColor = Color.White;
            btn.BackColor = isActive
                ? Color.FromArgb(108, 147, 204)  // ฟ้า = active
                : Color.FromArgb(160, 160, 160);   // เทา = inactive

            pnlMenu.Controls.Add(btn);
            return btn;
        }

        private void SetActiveButton(Button? activeBtn)
        {
            foreach (Control c in pnlMenu.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = (btn == activeBtn)
                        ? Color.FromArgb(108, 147, 204)
                        : Color.FromArgb(160, 160, 160);
                }
            }
        }

        private void ShowFirstAvailablePage()
        {
            // แสดงหน้าแรกที่มีในเมนู
            if (_btnInput != null)
                ShowInputOrder();
            else if (_btnSetting != null)
                ShowSetting();
        }

        // ========== Show Methods ==========

        private void ShowInputOrder()
        {
            pnlContent.Controls.Clear();

            _ucInput ??= new ucInputOrder();
            _ucInput.Dock = DockStyle.Fill;
            _ucInput.BarcodeScanned -= OnBarcodeScanned;
            _ucInput.BarcodeScanned += OnBarcodeScanned;

            pnlContent.Controls.Add(_ucInput);
            this.ActiveControl = _ucInput;

            SetActiveButton(_btnInput);
        }

        private void ShowOrderList()
        {
            pnlContent.Controls.Clear();
            var lbl = new Label
            {
                Text = "Order List - Coming Soon",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlContent.Controls.Add(lbl);
            SetActiveButton(_btnOrder);
        }

        private void ShowEditPattern()
        {
            pnlContent.Controls.Clear();
            var lbl = new Label
            {
                Text = "Edit Pattern - Coming Soon",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlContent.Controls.Add(lbl);
            SetActiveButton(_btnEdit);
        }

        private void ShowSetting()
        {
            pnlContent.Controls.Clear();

            // เปลี่ยนจาก ucSetting เป็น ucSettingMenu
            _ucSettingMenu ??= new ucSettingMenu();
            _ucSettingMenu.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(_ucSettingMenu);

            SetActiveButton(_btnSetting);
        }

        private void OnBarcodeScanned(object? sender, BarcodeScanEventArgs e)
        {
            MessageBox.Show(
                $"Barcode: {e.Barcode}\nOrder: {e.OrderNo}\nCustomer: {e.CustomerName}",
                "Scanned",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // เปลี่ยนโหมบ while running (ถ้าต้องการ)
        public void ReloadConfig()
        {
            _config = AppConfig.Load();
            CreateMenuByConfig();
            ShowFirstAvailablePage();
        }
    }
}