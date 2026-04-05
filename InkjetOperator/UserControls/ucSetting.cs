using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucSetting : UserControl
    {
        // ── Controls Grouping ──
        private TextBox[] txtMkComPorts = new TextBox[4];
        private Label[] lblMkStatus = new Label[4];
        private TextBox[] txtUvIpAddresses = new TextBox[2];
        private TextBox txtPlcIpAddress;
        private TextBox txtPlcPort;
        private TextBox txtPcIpAddress;

        private AppConfig _config;
        private bool _isConnecting = false;

        // เก็บ IP ล่าสุดที่เชื่อมต่อสำเร็จเพื่อเช็คการเปลี่ยนแปลง
        private string[] _lastConnectedIps = new string[4];

        private readonly ApiClient _api = ApiProvider.Instance;

        public ucSetting()
        {
            InitializeComponent();
            InitializeControls();

            _config = AppConfig.Load();
            ApplyConfigToUI();

            LoadSettings();
            SetupEvents();
        }

        private void InitializeControls()
        {
            txtMkComPorts[0] = txtMk058Com;
            txtMkComPorts[1] = txtMk059Com;
            txtMkComPorts[2] = txtMk060Com;
            txtMkComPorts[3] = txtMk061Com;

            lblMkStatus[0] = lblMk058Status;
            lblMkStatus[1] = lblMk059Status;
            lblMkStatus[2] = lblMk060Status;
            lblMkStatus[3] = lblMk061Status;

            txtUvIpAddresses[0] = txtUv001Ip;
            txtUvIpAddresses[1] = txtUv002Ip;

            txtPlcIpAddress = txtPlc001Ip;
            txtPlcPort = txtPlc001Port;
            txtPcIpAddress = txtPcip;
        }

        private void ApplyConfigToUI()
        {
            // กำหนดกลุ่มตัวเลขที่จะให้แสดงแค่ PC Station เท่านั้น
            int[] pcOnlyModes = { 0, 2, 3, 4 };

            // เช็คว่า MenuMode ปัจจุบันอยู่ในกลุ่มนี้หรือไม่
            bool isPcOnly = pcOnlyModes.Contains(_config.MenuMode);

            // ถ้าอยู่ในกลุ่ม PC Only ให้ซ่อนพวก Printer/Plc (Visible = false)
            pnlMkPrinters.Visible = !isPcOnly;
            pnlUvPrinters.Visible = !isPcOnly;
            pnlPlc.Visible = !isPcOnly;

            // ส่วน PC Station จะแสดงผลตรงข้ามกัน
            panelPcStation1.Visible = isPcOnly;
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            // Highlight color when changed
            Action<TextBox> addChangeEffect = (txt) =>
            {
                txt.TextChanged += (s, e) => txt.BackColor = Color.LightYellow;
            };

            foreach (var txt in txtMkComPorts) addChangeEffect(txt);
            foreach (var txt in txtUvIpAddresses) addChangeEffect(txt);
            addChangeEffect(txtPlcIpAddress);
            addChangeEffect(txtPlcPort);
            addChangeEffect(txtPcIpAddress);

            // Edit Name Events (Mapping IDs to Labels)
            btnPC2.Click += (s, e) => EditDeviceName("PC2IP_NAME", lblPC2);
            btnEditMk058.Click += (s, e) => EditDeviceName("MK058_NAME", lblMk058);
            btnEditMk059.Click += (s, e) => EditDeviceName("MK059_NAME", lblMk059);
            btnEditMk060.Click += (s, e) => EditDeviceName("MK060_NAME", lblMk060);
            btnEditMk061.Click += (s, e) => EditDeviceName("MK061_NAME", lblMk061);
        }

        private void EditDeviceName(string key, Label targetLabel)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Edit Device Name";
                dlg.Size = new Size(300, 150);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                var txt = new TextBox { Text = targetLabel.Text, Location = new Point(20, 20), Size = new Size(240, 25) };
                var btn = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(100, 70) };
                dlg.Controls.AddRange(new Control[] { txt, btn });

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    targetLabel.Text = txt.Text;
                    CustomSettingsManager.SetValue(key, txt.Text);
                }
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateSettings()) return;

            // 1. Save to Config
            for (int i = 0; i < 4; i++)
                CustomSettingsManager.SetValue($"MK{(58 + i):000}_COM", txtMkComPorts[i].Text.Trim());

            CustomSettingsManager.SetValue("PC2IP_NAME", lblPC2.Text);
            CustomSettingsManager.SetValue("UV001_IP", txtUvIpAddresses[0].Text.Trim());
            CustomSettingsManager.SetValue("UV002_IP", txtUvIpAddresses[1].Text.Trim());
            CustomSettingsManager.SetValue("PLC_IP", txtPlcIpAddress.Text.Trim());
            CustomSettingsManager.SetValue("PLC_PORT", txtPlcPort.Text.Trim());
            CustomSettingsManager.SetValue("PC_IP", txtPcIpAddress.Text.Trim());

            ResetColors();

            // 2. Reconnect immediately to verify new IPs
            await ConnectAllAsync(forceReconnect: true);

            MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSettings()
        {
            for (int i = 0; i < 4; i++)
                txtMkComPorts[i].Text = CustomSettingsManager.GetValue($"MK{(58 + i):000}_COM") ?? "";

            lblPC2.Text = CustomSettingsManager.GetValue("PC2IP_NAME") ?? "PC2";
            txtUvIpAddresses[0].Text = CustomSettingsManager.GetValue("UV001_IP") ?? "";
            txtUvIpAddresses[1].Text = CustomSettingsManager.GetValue("UV002_IP") ?? "";
            txtPlcIpAddress.Text = CustomSettingsManager.GetValue("PLC_IP") ?? "";
            txtPlcPort.Text = CustomSettingsManager.GetValue("PLC_PORT") ?? "502";
            txtPcIpAddress.Text = CustomSettingsManager.GetValue("PC_IP") ?? "";

            lblMk058.Text = CustomSettingsManager.GetValue("MK058_NAME") ?? "MK-058";
            lblMk059.Text = CustomSettingsManager.GetValue("MK059_NAME") ?? "MK-059";
            lblMk060.Text = CustomSettingsManager.GetValue("MK060_NAME") ?? "MK-060";
            lblMk061.Text = CustomSettingsManager.GetValue("MK061_NAME") ?? "MK-061";

            UpdateStatusUI();
        }

        private bool ValidateSettings()
        {
            var seen = new HashSet<string>();
            foreach (var txt in txtMkComPorts)
            {
                string val = txt.Text.Trim();
                if (string.IsNullOrEmpty(val)) continue;
                if (!seen.Add(val))
                {
                    MessageBox.Show("IP Address/Port ซ้ำกัน กรุณาตรวจสอบ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt.Focus();
                    return false;
                }
            }
            return true;
        }

        private async Task ConnectAllAsync(bool forceReconnect = false)
        {
            if (_isConnecting || this.IsDisposed) return;
            _isConnecting = true;

            try
            {
                var currentAdapters = AdapterRegistry.AllMk;
                var tasks = new List<Task<IInkjetAdapter?>>();

                for (int i = 0; i < 4; i++)
                {
                    string ip = txtMkComPorts[i].Text.Trim();
                    tasks.Add(ConnectSinglePrinterAsync(currentAdapters[i], ip, i, forceReconnect));
                }

                var results = await Task.WhenAll(tasks);

                // Update Global Registry
                AdapterRegistry.MK058 = results[0];
                AdapterRegistry.MK059 = results[1];
                AdapterRegistry.MK060 = results[2];
                AdapterRegistry.MK061 = results[3];

                if (!this.IsDisposed)
                    this.BeginInvoke(new Action(UpdateStatusUI));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConnectAll Error: {ex.Message}");
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private async Task<IInkjetAdapter?> ConnectSinglePrinterAsync(IInkjetAdapter? existingAdapter, string ip, int index, bool force)
        {
            if (string.IsNullOrWhiteSpace(ip)) return null;

            // เช็คว่า IP เปลี่ยนไปจากครั้งล่าสุดที่ต่อสำเร็จหรือไม่
            bool ipChanged = _lastConnectedIps[index] != ip;

            // ถ้าเครื่องเดิมยังต่ออยู่ และ IP เดิม และไม่สั่ง Force -> ใช้ตัวเดิม
            if (!force && !ipChanged && existingAdapter != null && existingAdapter.IsConnected())
            {
                return existingAdapter;
            }

            // ถ้ามีการเปลี่ยน IP หรือสั่ง Force หรือตัวเก่าหลุด -> ตัดการเชื่อมต่อตัวเก่าก่อน
            if (existingAdapter != null)
            {
                try { await existingAdapter.DisconnectAsync(); } catch { /* ignore */ }
            }

            // เริ่มการเชื่อมต่อใหม่
            var tcp = new TcpManager();
            try
            {
                // Timeout สั้น (0.5s) เพื่อไม่ให้ UI ค้างถ้า IP ผิด
                await tcp.ConnectAsync(ip, 9004);

                if (tcp.IsConnected())
                {
                    _lastConnectedIps[index] = ip;
                    return new MkCompactAdapter(tcp);
                }
            }
            catch
            {
                tcp.Dispose();
            }

            _lastConnectedIps[index] = null; // ล้างค่าถ้าต่อไม่ติด
            return null;
        }

        private async void UpdateStatusUI()
        {
            if (this.IsDisposed) return;

            // --- 1. เช็คสถานะ Inkjet Printers (Sync Check จาก Registry) ---
            lblMkStatus[0].BackColor = (AdapterRegistry.MK058?.IsConnected() == true) ? Color.Green : Color.Red;
            lblMkStatus[1].BackColor = (AdapterRegistry.MK059?.IsConnected() == true) ? Color.Green : Color.Red;
            lblMkStatus[2].BackColor = (AdapterRegistry.MK060?.IsConnected() == true) ? Color.Green : Color.Red;
            lblMkStatus[3].BackColor = (AdapterRegistry.MK061?.IsConnected() == true) ? Color.Green : Color.Red;

            // --- 2. เช็คสถานะ Backend PC (Async Check) ---
            // ป้องกันการเรียกซ้ำซ้อนขณะกำลังตรวจสอบ
            string pcIp = txtPcIpAddress.Text.Trim();
            if (string.IsNullOrEmpty(pcIp))
            {
                txtPcIpAddress.ForeColor = Color.Gray;
                return;
            }

            try
            {
                // เรียกใช้ PingAsync ที่คุณเขียนไว้ใน API Service
                // หมายเหตุ: ตัวแปร _api ต้องถูกประกาศและ Initialize ไว้ใน class นี้ด้วย
                bool backendOk = await _api.PingAsync();

                if (!this.IsDisposed) // เช็คอีกครั้งว่า User ยังไม่ปิดหน้าจอนี้ไปก่อน
                {
                    lblPcStatus.BackColor = backendOk ? Color.Green : Color.Red;

                    // ถ้ามี Label แสดงสถานะแยกต่างหาก
                    // lblPcStatus.Text = backendOk ? "Online" : "Offline";
                    // lblPcStatus.ForeColor = backendOk ? Color.Green : Color.Red;
                }
            }
            catch
            {
                txtPcIpAddress.ForeColor = Color.Red;
            }
        }

        private void ResetColors()
        {
            foreach (var t in txtMkComPorts) t.BackColor = Color.White;
            foreach (var t in txtUvIpAddresses) t.BackColor = Color.White;
            txtPlcIpAddress.BackColor = Color.White;
            txtPlcPort.BackColor = Color.White;
            txtPcIpAddress.BackColor = Color.White;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            LoadSettings();
            ResetColors();
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            // 1. หยุด Timer ทันทีเพื่อป้องกัน Re-entry (งานทับซ้อน)
            timer1.Stop();

            try
            {
                // 2. เช็คว่า Form หรือ Control ถูกปิดไปหรือยังก่อนทำงาน
                if (this.IsDisposed) return;

                // 3. ทำงาน Connect โดยตั้งค่า Timeout ในตัว Manager ให้เหมาะสม
                await ConnectAllAsync(forceReconnect: false);
            }
            catch (Exception ex)
            {
                // 4. ต้องมี try-catch เสมอในระดับ Top-level ของ async void
                // เพื่อป้องกันแอพพัง และเก็บ Log ไว้ดูย้อนหลัง
                Console.WriteLine($"Polling Error: {ex.Message}");
            }
            finally
            {
                // 5. เมื่องานเสร็จสิ้น (ไม่ว่าจะสำเร็จหรือ Error) 
                // ค่อยเริ่มนับถอยหลัง Timer ใหม่อีกครั้ง
                if (!this.IsDisposed)
                {
                    timer1.Start();
                }
            }
        }
    }
}