using System;
using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSettingPLC : UserControl
    {
        public ucSettingPLC()
        {
            InitializeComponent();
            LoadSettings();
            SetupEvents();
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            // Highlight color when changed
            txtPlc001Ip.TextChanged += (s, e) => txtPlc001Ip.BackColor = Color.LightYellow;
            txtPlc001Port.TextChanged += (s, e) => txtPlc001Port.BackColor = Color.LightYellow;
        }

        private void LoadSettings()
        {
            txtPlc001Ip.Text = CustomSettingsManager.GetValue("PLC_IP") ?? "";
            txtPlc001Port.Text = CustomSettingsManager.GetValue("PLC_PORT") ?? "502";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            CustomSettingsManager.SetValue("PLC_IP", txtPlc001Ip.Text.Trim());
            CustomSettingsManager.SetValue("PLC_PORT", txtPlc001Port.Text.Trim());

            ResetColors();

            MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            LoadSettings();
            ResetColors();
        }

        private void ResetColors()
        {
            txtPlc001Ip.BackColor = Color.White;
            txtPlc001Port.BackColor = Color.White;
        }
    }
}
