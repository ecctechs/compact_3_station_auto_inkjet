using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSetting : UserControl
    {
        // MK Inkjet Printers (4 เครื่อง)
        private TextBox[] txtMkComPorts = new TextBox[4];
        private TextBox[] txtMkBaudRates = new TextBox[4];
        private Label[] lblMkStatus = new Label[4];

        // UV Printers (2 เครื่อง)
        private TextBox[] txtUvIpAddresses = new TextBox[2];

        // PLC (1 เครื่อง)
        private TextBox txtPlcIpAddress = new TextBox();

        // Settings data
        public class PrinterSettings
        {
            public string[] MkComPorts { get; set; } = new string[4];
            public int[] MkBaudRates { get; set; } = new int[4];
            public string[] UvIpAddresses { get; set; } = new string[2];
            public string PlcIpAddress { get; set; } = "";
        }

        public ucSetting()
        {
            InitializeComponent();
            InitializeControls();
            LoadSettings();
            SetupEvents();
        }

        private void InitializeControls()
        {
            // เก็บ reference จาก Designer
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

            // ตั้งค่า default
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // MK Baud Rate default = 9600
            foreach (var txt in txtMkBaudRates)
            {
                if (string.IsNullOrEmpty(txt.Text))
                    txt.Text = "9600";
            }

            // อัปเดตสถานะเริ่มต้น
            UpdateStatusIndicators();
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            // เปลี่ยนสีเมื่อแก้ไขค่า
            foreach (var txt in txtMkComPorts)
            {
                txt.TextChanged += (s, e) => MarkAsModified(txt);
            }
            foreach (var txt in txtMkBaudRates)
            {
                txt.TextChanged += (s, e) => MarkAsModified(txt);
            }
            foreach (var txt in txtUvIpAddresses)
            {
                txt.TextChanged += (s, e) => MarkAsModified(txt);
            }
            txtPlcIpAddress.TextChanged += (s, e) => MarkAsModified(txtPlcIpAddress);

            // คลิกปุ่มแก้ไขชื่อ
            btnEditMk058.Click += (s, e) => EditDeviceName("MK-058");
            btnEditMk059.Click += (s, e) => EditDeviceName("MK-059");
            btnEditMk060.Click += (s, e) => EditDeviceName("MK-060");
            btnEditMk061.Click += (s, e) => EditDeviceName("MK-061");
            btnEditUv001.Click += (s, e) => EditDeviceName("UV-001");
            btnEditUv002.Click += (s, e) => EditDeviceName("UV-002");
            btnEditPlc001.Click += (s, e) => EditDeviceName("PLC-001");
        }

        private void MarkAsModified(Control control)
        {
            control.BackColor = Color.FromArgb(255, 255, 200); // เหลืองอ่อน
        }

        private void EditDeviceName(string currentName)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Edit Device Name";
                dlg.Size = new Size(300, 150);
                dlg.StartPosition = FormStartPosition.CenterParent;

                var txt = new TextBox();
                txt.Text = currentName;
                txt.Location = new Point(20, 20);
                txt.Size = new Size(240, 25);
                dlg.Controls.Add(txt);

                var btn = new Button();
                btn.Text = "OK";
                btn.DialogResult = DialogResult.OK;
                btn.Location = new Point(100, 70);
                dlg.Controls.Add(btn);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // อัปเดตชื่อใน UI
                    UpdateDeviceName(currentName, txt.Text);
                }
            }
        }

        private void UpdateDeviceName(string oldName, string newName)
        {
            // หา Label ที่มีชื่อเดิมแล้มเปลี่ยน
            foreach (Control c in pnlMkPrinters.Controls)
            {
                if (c is Label lbl && lbl.Text == oldName)
                {
                    lbl.Text = newName;
                    return;
                }
            }
            foreach (Control c in pnlUvPrinters.Controls)
            {
                if (c is Label lbl && lbl.Text == oldName)
                {
                    lbl.Text = newName;
                    return;
                }
            }
            foreach (Control c in pnlPlc.Controls)
            {
                if (c is Label lbl && lbl.Text == oldName)
                {
                    lbl.Text = newName;
                    return;
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateSettings()) return;

            var settings = new PrinterSettings();
            for (int i = 0; i < 4; i++)
            {
                settings.MkComPorts[i] = txtMkComPorts[i].Text;
                settings.MkBaudRates[i] = int.TryParse(txtMkBaudRates[i].Text, out int baud) ? baud : 9600;
            }
            settings.UvIpAddresses[0] = txtUvIpAddresses[0].Text;
            settings.UvIpAddresses[1] = txtUvIpAddresses[1].Text;
            settings.PlcIpAddress = txtPlcIpAddress.Text;

            // TODO: บันทึกลงไฟล์ หรือ Database
            SaveToFile(settings);

            // รีเซ็ตสี
            ResetModifiedColors();
            UpdateStatusIndicators();

            MessageBox.Show("Settings saved successfully!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            // โหลดค่าเดิมกลับมา
            LoadSettings();
            ResetModifiedColors();
        }

        private bool ValidateSettings()
        {
            // ตรวจสอบ COM Port ซ้ำ
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (!string.IsNullOrEmpty(txtMkComPorts[i].Text) &&
                        txtMkComPorts[i].Text == txtMkComPorts[j].Text)
                    {
                        MessageBox.Show($"COM Port ซ้ำกัน: {txtMkComPorts[i].Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            // ตรวจสอบ Baud Rate เป็นตัวเลข
            foreach (var txt in txtMkBaudRates)
            {
                if (!string.IsNullOrEmpty(txt.Text) && !int.TryParse(txt.Text, out _))
                {
                    MessageBox.Show($"Baud Rate ต้องเป็นตัวเลข: {txt.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt.Focus();
                    return false;
                }
            }

            return true;
        }

        private void SaveToFile(PrinterSettings settings)
        {
            // TODO: บันทึกเป็น JSON หรือ XML
            // File.WriteAllText("settings.json", JsonSerializer.Serialize(settings));
        }

        private void LoadSettings()
        {
            // TODO: โหลดจากไฟล์ หรือใช้ค่า default
            // ตัวอย่างค่าเริ่มต้น
            if (string.IsNullOrEmpty(txtMk058Com.Text)) txtMk058Com.Text = "COM1";
            if (string.IsNullOrEmpty(txtMk059Com.Text)) txtMk059Com.Text = "COM2";
            if (string.IsNullOrEmpty(txtMk060Com.Text)) txtMk060Com.Text = "COM3";
            if (string.IsNullOrEmpty(txtMk061Com.Text)) txtMk061Com.Text = "COM4";

            // ดึง COM Ports ที่มีจริง
            RefreshAvailablePorts();
        }

        private void RefreshAvailablePorts()
        {
            var ports = SerialPort.GetPortNames();
            // TODO: แสดงใน ComboBox หรือ Auto-complete
        }

        private void UpdateStatusIndicators()
        {
            // อัปเดตสีจุดสถานะ (เขียว = มีค่า, แดง = ว่าง)
            for (int i = 0; i < 4; i++)
            {
                bool hasValue = !string.IsNullOrEmpty(txtMkComPorts[i].Text) &&
                               !string.IsNullOrEmpty(txtMkBaudRates[i].Text);
                lblMkStatus[i].BackColor = hasValue ? Color.FromArgb(100, 200, 100) : Color.FromArgb(220, 80, 50);
            }
        }

        private void ResetModifiedColors()
        {
            foreach (var txt in txtMkComPorts) txt.BackColor = Color.White;
            foreach (var txt in txtMkBaudRates) txt.BackColor = Color.White;
            foreach (var txt in txtUvIpAddresses) txt.BackColor = Color.White;
            txtPlcIpAddress.BackColor = Color.White;
        }

        // Public method สำหรับดึงค่าไปใช้ที่อื่น
        public PrinterSettings GetSettings()
        {
            return new PrinterSettings
            {
                MkComPorts = new[] { txtMk058Com.Text, txtMk059Com.Text, txtMk060Com.Text, txtMk061Com.Text },
                MkBaudRates = new[] {
                    int.TryParse(txtMk058Baud.Text, out int b1) ? b1 : 9600,
                    int.TryParse(txtMk059Baud.Text, out int b2) ? b2 : 9600,
                    int.TryParse(txtMk060Baud.Text, out int b3) ? b3 : 9600,
                    int.TryParse(txtMk061Baud.Text, out int b4) ? b4 : 9600
                },
                UvIpAddresses = new[] { txtUv001Ip.Text, txtUv002Ip.Text },
                PlcIpAddress = txtPlc001Ip.Text
            };
        }
    }
}