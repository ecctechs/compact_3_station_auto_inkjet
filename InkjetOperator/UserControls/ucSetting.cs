using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSetting : UserControl
    {
        private TextBox[] txtMkComPorts = new TextBox[4];
        private TextBox[] txtMkBaudRates = new TextBox[4];
        private Label[] lblMkStatus = new Label[4];

        private TextBox[] txtUvIpAddresses = new TextBox[2];
        private TextBox txtPlcIpAddress = new TextBox();
        private TextBox txtPcIpAddress = new TextBox();

        private AppConfig _config;

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

            txtMkBaudRates[0] = txtMk058Baud;
            txtMkBaudRates[1] = txtMk059Baud;
            txtMkBaudRates[2] = txtMk060Baud;
            txtMkBaudRates[3] = txtMk061Baud;

            lblMkStatus[0] = lblMk058Status;
            lblMkStatus[1] = lblMk059Status;
            lblMkStatus[2] = lblMk060Status;
            lblMkStatus[3] = lblMk061Status;

            txtUvIpAddresses[0] = txtUv001Ip;
            txtUvIpAddresses[1] = txtUv002Ip;

            txtPlcIpAddress = txtPlc001Ip;

            txtPcIpAddress = txtPcip;
        }

        private void ApplyConfigToUI()
        {
            if (_config.MenuMode == 1)
            {
                pnlLeftMenu.Visible = false;
                pnlMkPrinters.Visible = false;
                pnlUvPrinters.Visible = false;
                pnlPlc.Visible = false;
                panelPcStation1.Visible = true;
            }
            else
            {
                panelPcStation1.Visible = false;
                pnlLeftMenu.Visible = true;
                pnlMkPrinters.Visible = true;
                pnlUvPrinters.Visible = true;
                pnlPlc.Visible = true;
            }
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            foreach (var txt in txtMkComPorts)
                txt.TextChanged += (s, e) => txt.BackColor = Color.LightYellow;

            foreach (var txt in txtMkBaudRates)
                txt.TextChanged += (s, e) => txt.BackColor = Color.LightYellow;

            foreach (var txt in txtUvIpAddresses)
                txt.TextChanged += (s, e) => txt.BackColor = Color.LightYellow;

            txtPlcIpAddress.TextChanged += (s, e) => txtPlcIpAddress.BackColor = Color.LightYellow;

            txtPcIpAddress.TextChanged += (s, e) => txtPcIpAddress.BackColor = Color.LightYellow;
        }

        // ================= SAVE =================
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateSettings()) return;

            for (int i = 0; i < 4; i++)
            {
                CustomSettingsManager.SetValue($"MK{(58 + i):000}_COM", txtMkComPorts[i].Text);
                CustomSettingsManager.SetValue($"MK{(58 + i):000}_BAUD", txtMkBaudRates[i].Text);
            }

            CustomSettingsManager.SetValue("UV001_IP", txtUvIpAddresses[0].Text);
            CustomSettingsManager.SetValue("UV002_IP", txtUvIpAddresses[1].Text);
            CustomSettingsManager.SetValue("PLC_IP", txtPlcIpAddress.Text);

            CustomSettingsManager.SetValue("PC_IP", txtPcIpAddress.Text);

            ResetColors();
            UpdateStatus();

            MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ================= LOAD =================
        private void LoadSettings()
        {
            for (int i = 0; i < 4; i++)
            {
                txtMkComPorts[i].Text =
                    CustomSettingsManager.GetValue($"MK{(58 + i):000}_COM") ?? $"COM{i + 1}";

                txtMkBaudRates[i].Text =
                    CustomSettingsManager.GetValue($"MK{(58 + i):000}_BAUD") ?? "9600";
            }

            txtUvIpAddresses[0].Text = CustomSettingsManager.GetValue("UV001_IP") ?? "";
            txtUvIpAddresses[1].Text = CustomSettingsManager.GetValue("UV002_IP") ?? "";
            txtPlcIpAddress.Text = CustomSettingsManager.GetValue("PLC_IP") ?? "";

            txtPcIpAddress.Text = CustomSettingsManager.GetValue("PC_IP") ?? "";

            UpdateStatus();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            LoadSettings();
            ResetColors();
        }

        // ================= VALIDATE =================
        private bool ValidateSettings()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (!string.IsNullOrEmpty(txtMkComPorts[i].Text) &&
                        txtMkComPorts[i].Text == txtMkComPorts[j].Text)
                    {
                        MessageBox.Show("COM Port ซ้ำกัน", "Error");
                        return false;
                    }
                }
            }

            foreach (var txt in txtMkBaudRates)
            {
                if (!int.TryParse(txt.Text, out _))
                {
                    MessageBox.Show("Baud ต้องเป็นตัวเลข");
                    return false;
                }
            }

            return true;
        }

        // ================= UI =================
        private void UpdateStatus()
        {
            for (int i = 0; i < 4; i++)
            {
                bool ok = !string.IsNullOrEmpty(txtMkComPorts[i].Text) &&
                          !string.IsNullOrEmpty(txtMkBaudRates[i].Text);

                lblMkStatus[i].BackColor = ok ? Color.Green : Color.Red;
            }
        }

        private void ResetColors()
        {
            foreach (var t in txtMkComPorts) t.BackColor = Color.White;
            foreach (var t in txtMkBaudRates) t.BackColor = Color.White;
            foreach (var t in txtUvIpAddresses) t.BackColor = Color.White;
            txtPlcIpAddress.BackColor = Color.White;
        }
    }
}