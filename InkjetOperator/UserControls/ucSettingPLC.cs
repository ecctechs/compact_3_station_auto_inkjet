using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucSettingPLC : UserControl
    {
        private BindingList<PlcRegisterMap> _rows = new();
        private readonly ApiClient _api = ApiProvider.Instance;

        public ucSettingPLC()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadSettings();
            SetupEvents();

            _ = LoadTableAsync();
        }

        private void SetupDataGridView()
        {
            UiStyle.ApplyGrid(dgvPlcMap);

            dgvPlcMap.AutoGenerateColumns = false;
            colAddressStart.DataPropertyName = "AddressStart";
            colAddressStop.DataPropertyName = "AddressStop";
            colPlcStart.DataPropertyName = "PlcStart";
            colPlcStop.DataPropertyName = "PlcStop";
            colListName.DataPropertyName = "ListName";
            colDataType.DataPropertyName = "DataType";
            colBit.DataPropertyName = "Bit";
        }

        private void SetupEvents()
        {
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAddRow.Click += BtnAddRow_Click;

            // กัน error dialog เด้งเมื่อพิมพ์ค่าที่แปลงไม่ได้ (เช่น ตัวอักษรในช่องตัวเลข)
            dgvPlcMap.DataError += (s, e) => e.ThrowException = false;

            // Highlight color when changed
            txtPlc001Ip.TextChanged += (s, e) => txtPlc001Ip.BackColor = Color.LightYellow;
            txtPlc001Port.TextChanged += (s, e) => txtPlc001Port.BackColor = Color.LightYellow;
        }

        private void LoadSettings()
        {
            txtPlc001Ip.Text = CustomSettingsManager.GetValue("PLC_IP") ?? "";
            txtPlc001Port.Text = CustomSettingsManager.GetValue("PLC_PORT") ?? "502";
        }

        /// <summary>โหลดตาราง register mapping จาก Backend</summary>
        private async Task LoadTableAsync()
        {
            var rows = await _api.GetAllPlcSettingsAsync();
            if (this.IsDisposed) return;

            _rows = new BindingList<PlcRegisterMap>(rows);
            dgvPlcMap.DataSource = _rows;
        }

        private void BtnAddRow_Click(object? sender, EventArgs e)
        {
            _rows.Add(new PlcRegisterMap { DataType = "Int", Bit = 32 });

            // เลื่อนไปแถวใหม่ให้กรอกต่อได้เลย
            if (dgvPlcMap.Rows.Count > 0)
                dgvPlcMap.CurrentCell = dgvPlcMap.Rows[dgvPlcMap.Rows.Count - 1].Cells[0];
        }

        private void dgvPlcMap_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // ปุ่มลบในแถว
            if (e.RowIndex >= 0 && dgvPlcMap.Columns[e.ColumnIndex].Name == "colDelete")
            {
                _rows.RemoveAt(e.RowIndex);
            }
        }

        private bool ValidateRows()
        {
            // เช็คชื่อ List ซ้ำ (ไม่สนตัวพิมพ์เล็ก/ใหญ่)
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _rows.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(_rows[i].ListName))
                {
                    MessageBox.Show($"แถวที่ {i + 1}: กรุณากรอกชื่อ List", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_rows[i].PlcStart) || string.IsNullOrWhiteSpace(_rows[i].PlcStop))
                {
                    MessageBox.Show($"แถวที่ {i + 1}: กรุณากรอก PLC Start / PLC Stop", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!seenNames.Add(_rows[i].ListName.Trim()))
                {
                    MessageBox.Show($"แถวที่ {i + 1}: ชื่อ List \"{_rows[i].ListName.Trim()}\" ซ้ำกับแถวอื่น",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (_rows[i].AddressStart > _rows[i].AddressStop)
                {
                    MessageBox.Show($"แถวที่ {i + 1}: Address Start ต้องไม่มากกว่า Address Stop",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // เช็คช่วง Address ทับซ้อนกันระหว่างแถว
            for (int i = 0; i < _rows.Count; i++)
            {
                for (int j = i + 1; j < _rows.Count; j++)
                {
                    if (_rows[i].AddressStart <= _rows[j].AddressStop &&
                        _rows[j].AddressStart <= _rows[i].AddressStop)
                    {
                        MessageBox.Show(
                            $"แถวที่ {i + 1} และแถวที่ {j + 1}: ช่วง Address ทับซ้อนกัน " +
                            $"({_rows[i].AddressStart}-{_rows[i].AddressStop} กับ {_rows[j].AddressStart}-{_rows[j].AddressStop})",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // ปิด editor ของ cell ที่ค้างอยู่ให้ค่าลง BindingList ก่อน
            dgvPlcMap.EndEdit();

            if (!ValidateRows()) return;

            // 1. Save IP/Port ลง Setting.config
            CustomSettingsManager.SetValue("PLC_IP", txtPlc001Ip.Text.Trim());
            CustomSettingsManager.SetValue("PLC_PORT", txtPlc001Port.Text.Trim());

            // 2. Save ตารางทั้งชุดลง Backend (sort_order ตามลำดับแถวปัจจุบัน)
            for (int i = 0; i < _rows.Count; i++)
                _rows[i].SortOrder = i;

            btnSave.Enabled = false;
            bool ok = await _api.BulkSavePlcSettingsAsync(_rows.ToList());
            if (this.IsDisposed) return;
            btnSave.Enabled = true;

            ResetColors();

            if (ok)
            {
                // โหลดกลับเพื่อให้ได้ id ใหม่จาก Backend
                await LoadTableAsync();
                MessageBox.Show("บันทึกเรียบร้อย", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("บันทึกตาราง PLC ไม่สำเร็จ กรุณาตรวจสอบการเชื่อมต่อ Backend",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCancel_Click(object? sender, EventArgs e)
        {
            LoadSettings();
            ResetColors();

            // ทิ้งค่าที่แก้ค้างไว้ แล้วโหลดตารางจาก Backend ใหม่
            await LoadTableAsync();
        }

        private void ResetColors()
        {
            txtPlc001Ip.BackColor = Color.White;
            txtPlc001Port.BackColor = Color.White;
        }
    }
}
