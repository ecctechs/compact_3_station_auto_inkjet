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
        private TextBox[] txtMkComPorts = new TextBox[2];
        private Label[] lblMkStatus = new Label[2];
        private TextBox txtPcIpAddress;

        private AppConfig _config;
        private bool _isConnecting = false;

        private string[] _lastConnectedIps = new string[2];

        private readonly ApiClient _api = ApiProvider.Instance;

        public ucSetting()
        {
            InitializeComponent();
            InitializeControls();

            _config = AppConfig.Load();
            ApplyConfigToUI();

            LoadSettings();
            SetupEvents();

            timer1.Enabled = false;
            lblPcStatus.BackColor = Color.Gray;
            foreach (var lbl in lblMkStatus) lbl.BackColor = Color.Gray;
            CheckAllStatusAsync();
        }

        private async void CheckAllStatusAsync()
        {
            try
            {
                var backendTask = _api.PingAsync();
                var mkTask = ConnectAllAsync(forceReconnect: false);
                await Task.WhenAll(backendTask, mkTask);

                if (!this.IsDisposed)
                    lblPcStatus.BackColor = backendTask.Result ? Color.Green : Color.Red;
            }
            catch
            {
                if (!this.IsDisposed)
                    lblPcStatus.BackColor = Color.Red;
            }
        }

        private void InitializeControls()
        {
            txtMkComPorts[0] = txtMk058Com;
            txtMkComPorts[1] = txtMk059Com;

            lblMkStatus[0] = lblMk058Status;
            lblMkStatus[1] = lblMk059Status;

            txtPcIpAddress = txtPcip;
        }

        private void ApplyConfigToUI()
        {
            int[] pcOnlyModes = { 0, 2, 3, 4 };
            bool isPcOnly = pcOnlyModes.Contains(_config.MenuMode);

            pnlMkPrinters.Visible = !isPcOnly;
            panelPcStation1.Visible = isPcOnly;
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            Action<TextBox> addChangeEffect = (txt) =>
            {
                txt.TextChanged += (s, e) => txt.BackColor = Color.LightYellow;
            };

            foreach (var txt in txtMkComPorts) addChangeEffect(txt);
            addChangeEffect(txtPcIpAddress);

            btnPC2.Click += (s, e) => EditDeviceName("PC2IP_NAME", lblPC2);
            btnEditMk058.Click += (s, e) => EditDeviceName("MK058_NAME", lblMk058);
            btnEditMk059.Click += (s, e) => EditDeviceName("MK059_NAME", lblMk059);
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

            CustomSettingsManager.SetValue("MK058_COM", txtMkComPorts[0].Text.Trim());
            CustomSettingsManager.SetValue("MK059_COM", txtMkComPorts[1].Text.Trim());

            CustomSettingsManager.SetValue("PC2IP_NAME", lblPC2.Text);
            CustomSettingsManager.SetValue("PC_IP", txtPcIpAddress.Text.Trim());

            ResetColors();

            lblPcStatus.BackColor = Color.Gray;
            foreach (var lbl in lblMkStatus) lbl.BackColor = Color.Gray;
            btnSave.Enabled = false;
            try
            {
                var backendTask = _api.PingAsync();
                var mkTask = ConnectAllAsync(forceReconnect: true);
                await Task.WhenAll(backendTask, mkTask);

                lblPcStatus.BackColor = backendTask.Result ? Color.Green : Color.Red;
            }
            catch
            {
                lblPcStatus.BackColor = Color.Red;
            }
            finally
            {
                btnSave.Enabled = true;
            }

            MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSettings()
        {
            txtMkComPorts[0].Text = CustomSettingsManager.GetValue("MK058_COM") ?? "";
            txtMkComPorts[1].Text = CustomSettingsManager.GetValue("MK059_COM") ?? "";

            lblPC2.Text = CustomSettingsManager.GetValue("PC2IP_NAME") ?? "PC2";
            txtPcIpAddress.Text = CustomSettingsManager.GetValue("PC_IP") ?? "";

            lblMk058.Text = CustomSettingsManager.GetValue("MK058_NAME") ?? "MK-058";
            lblMk059.Text = CustomSettingsManager.GetValue("MK059_NAME") ?? "MK-059";

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

                for (int i = 0; i < 2; i++)
                {
                    string ip = txtMkComPorts[i].Text.Trim();
                    tasks.Add(ConnectSinglePrinterAsync(currentAdapters[i], ip, i, forceReconnect));
                }

                var results = await Task.WhenAll(tasks);

                AdapterRegistry.MK058 = results[0];
                AdapterRegistry.MK059 = results[1];

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

            bool ipChanged = _lastConnectedIps[index] != ip;

            if (!force && !ipChanged && existingAdapter != null && existingAdapter.IsConnected())
            {
                return existingAdapter;
            }

            if (existingAdapter != null)
            {
                try { await existingAdapter.DisconnectAsync(); } catch { /* ignore */ }
            }

            var tcp = new TcpManager();
            try
            {
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

            _lastConnectedIps[index] = null;
            return null;
        }

        private void UpdateStatusUI()
        {
            if (this.IsDisposed) return;

            lblMkStatus[0].BackColor = (AdapterRegistry.MK058?.IsConnected() == true) ? Color.Green : Color.Red;
            lblMkStatus[1].BackColor = (AdapterRegistry.MK059?.IsConnected() == true) ? Color.Green : Color.Red;
        }

        private void ResetColors()
        {
            foreach (var t in txtMkComPorts) t.BackColor = Color.White;
            txtPcIpAddress.BackColor = Color.White;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            LoadSettings();
            ResetColors();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }
    }
}
