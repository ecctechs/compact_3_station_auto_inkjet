using System;
using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucTestPLC : UserControl
    {
        private readonly PlcTcpService _plc = new PlcTcpService();

        public ucTestPLC()
        {
            InitializeComponent();

            string savedIp = CustomSettingsManager.GetValue("PLC_IP") ?? "";
            string savedPort = CustomSettingsManager.GetValue("PLC_PORT") ?? "";
            if (!string.IsNullOrEmpty(savedIp)) txtIp.Text = savedIp;
            if (!string.IsNullOrEmpty(savedPort)) txtPort.Text = savedPort;

            UiStyle.ApplyGrid(dgvRegisters);
            dgvRegisters.DataError += (s, e) => e.ThrowException = false;
            dgvRegisters.CellContentClick += DgvRegisters_CellContentClick;

            btnConnect.Click += BtnConnect_Click;
            btnReadAll.Click += BtnReadAll_Click;
            btnWriteAll.Click += BtnWriteAll_Click;
            btnAddRow.Click += BtnAddRow_Click;

            PrefillRegisters();
        }

        private void PrefillRegisters()
        {
            AddRegisterRow(5000, 5009, "D5000", "D5009", "Model Name", "String", 64);
            for (int i = 0; i < 10; i++)
            {
                int plc = 5010 + i * 2;
                AddRegisterRow(plc, plc + 1, $"D{plc}", $"D{plc + 1}", $"Speed Conveyor {i + 1}", "Int", 32);
            }
            AddRegisterRow(5030, 5031, "D5030", "D5031", "Speed IAI 1", "Int", 32);
            AddRegisterRow(5032, 5033, "D5032", "D5033", "Position IAI 1", "Int", 32);
            AddRegisterRow(5034, 5035, "D5034", "D5035", "Speed IAI 2", "Int", 32);
            AddRegisterRow(5036, 5037, "D5036", "D5037", "Position IAI 2", "Int", 32);
        }

        private void AddRegisterRow(int addrStart, int addrStop, string plcStart, string plcStop,
            string listName, string type, int bit)
        {
            dgvRegisters.Rows.Add(addrStart, addrStop, plcStart, plcStop, listName, type, bit, "", "R", "W", "X");
        }

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (_plc.IsConnected)
            {
                _plc.Disconnect();
                lblStatus.BackColor = Color.Gray;
                btnConnect.Text = "Connect";
                AppendLog("Disconnected");
                return;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port <= 0)
            {
                AppendLog("Port ไม่ถูกต้อง");
                return;
            }

            var (ok, log) = _plc.Connect(txtIp.Text.Trim(), port);
            lblStatus.BackColor = ok ? Color.Green : Color.Red;
            btnConnect.Text = ok ? "Disconnect" : "Connect";
            AppendLog(log);
        }

        private void BtnReadAll_Click(object? sender, EventArgs e)
        {
            if (!_plc.IsConnected) { AppendLog("ยังไม่ได้เชื่อมต่อ"); return; }

            foreach (DataGridViewRow row in dgvRegisters.Rows)
                ReadRow(row);
        }

        private void BtnWriteAll_Click(object? sender, EventArgs e)
        {
            if (!_plc.IsConnected) { AppendLog("ยังไม่ได้เชื่อมต่อ"); return; }

            foreach (DataGridViewRow row in dgvRegisters.Rows)
                WriteRow(row);
        }

        private void BtnAddRow_Click(object? sender, EventArgs e)
        {
            dgvRegisters.Rows.Add(0, 1, "D0", "D1", "", "Int", 32, "", "R", "W", "X");
            dgvRegisters.CurrentCell = dgvRegisters.Rows[dgvRegisters.Rows.Count - 1].Cells[0];
        }

        private void DgvRegisters_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvRegisters.Rows[e.RowIndex];
            string colName = dgvRegisters.Columns[e.ColumnIndex].Name;

            if (colName == "colRead")
            {
                if (!_plc.IsConnected) { AppendLog("ยังไม่ได้เชื่อมต่อ"); return; }
                ReadRow(row);
            }
            else if (colName == "colWrite")
            {
                if (!_plc.IsConnected) { AppendLog("ยังไม่ได้เชื่อมต่อ"); return; }
                WriteRow(row);
            }
            else if (colName == "colDelete")
            {
                dgvRegisters.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void ReadRow(DataGridViewRow row)
        {
            if (!TryParseRow(row, out int addr, out int count, out string type, out string listName))
                return;

            if (type == "String")
            {
                var (ok, log, val) = _plc.ReadString(addr, count);
                row.Cells["colValue"].Value = val;
                AppendLog($"{(ok ? "✔" : "✖")} Read {listName} ({row.Cells["colPlcStart"].Value}) = \"{val}\"");
            }
            else
            {
                var (ok, log, val) = _plc.ReadInt32(addr);
                row.Cells["colValue"].Value = val.ToString();
                AppendLog($"{(ok ? "✔" : "✖")} Read {listName} ({row.Cells["colPlcStart"].Value}) = {val}");
            }
        }

        private void WriteRow(DataGridViewRow row)
        {
            if (!TryParseRow(row, out int addr, out int count, out string type, out string listName))
                return;

            string valueStr = row.Cells["colValue"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(valueStr))
            {
                AppendLog($"⚠ {listName}: ไม่มีค่าที่จะเขียน");
                return;
            }

            if (type == "String")
            {
                var (ok, log) = _plc.WriteString(addr, valueStr, count);
                AppendLog($"{(ok ? "✔" : "✖")} Write {listName} ({row.Cells["colPlcStart"].Value}) = \"{valueStr}\"");
            }
            else
            {
                if (!int.TryParse(valueStr, out int intVal))
                {
                    AppendLog($"⚠ {listName}: ค่า \"{valueStr}\" ไม่ใช่ตัวเลข");
                    return;
                }
                var (ok, log) = _plc.WriteInt32(addr, intVal);
                AppendLog($"{(ok ? "✔" : "✖")} Write {listName} ({row.Cells["colPlcStart"].Value}) = {intVal}");
            }
        }

        private bool TryParseRow(DataGridViewRow row, out int addr, out int count, out string type, out string listName)
        {
            addr = 0; count = 0; type = "Int"; listName = "";

            var addrStartVal = row.Cells["colAddrStart"].Value;
            var addrStopVal = row.Cells["colAddrStop"].Value;
            if (addrStartVal == null || addrStopVal == null) return false;

            if (!int.TryParse(addrStartVal.ToString(), out int start) ||
                !int.TryParse(addrStopVal.ToString(), out int stop))
            {
                AppendLog("⚠ Address ไม่ถูกต้อง");
                return false;
            }

            addr = start;
            count = stop - start + 1;
            type = row.Cells["colType"].Value?.ToString() ?? "Int";
            listName = row.Cells["colListName"].Value?.ToString() ?? "";
            return true;
        }

        private void AppendLog(string msg)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }
    }
}
