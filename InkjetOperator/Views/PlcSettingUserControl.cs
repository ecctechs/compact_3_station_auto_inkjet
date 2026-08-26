using System.Net.Sockets;
using InkjetOperator.Models;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class PlcSettingUserControl : UserControl
{
    private static readonly Color StatusGray = Color.Gray;
    private static readonly Color StatusGreen = DesignTokens.Success;
    private static readonly Color StatusRed = DesignTokens.Danger;

    private static readonly HashSet<string> FixedAddresses = new()
        { "0", "1", "2", "3", "5", "6", "7", "8", "10", "11", "12" };

    private readonly ApiClient _api;
    private List<PlcRow> _rows = new();
    private bool _unlocked;

    public PlcSettingUserControl()
    {
        InitializeComponent();
        _api = new ApiClient(BuildBaseUrl());
        ConfigurePlcColumns();
        SetupEvents();
        LoadSettings();
        ApplyLockState();

        lblPlcStatus.ForeColor = StatusGray;
        _ = CheckStatusAsync();
        _ = LoadTableAsync();
    }

    private static string BuildBaseUrl()
    {
        var pcIp = CustomSettingsManager.Read("PC_IP", "127.0.0.1");
        return $"http://{pcIp}:3000";
    }

    // ── Setup ──────────────────────────────────────────────

    private void ConfigurePlcColumns()
    {
        tblPlcMap.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("AddressStart", "Addr Start", AntdUI.ColumnAlign.Center) { Editable = true, Width = "103" },
            new AntdUI.Column("AddressStop", "Addr Stop", AntdUI.ColumnAlign.Center) { Editable = true, Width = "103" },
            new AntdUI.Column("PlcStart", "PLC Start", AntdUI.ColumnAlign.Center) { Editable = true, Width = "103" },
            new AntdUI.Column("PlcStop", "PLC Stop", AntdUI.ColumnAlign.Center) { Editable = true, Width = "103" },
            new AntdUI.Column("ListName", "List Name", AntdUI.ColumnAlign.Center) { Editable = true, Width = "172" },
            new AntdUI.Column("DataType", "Data Type", AntdUI.ColumnAlign.Center) { Editable = true, Width = "92" },
            new AntdUI.Column("Bit", "Bit", AntdUI.ColumnAlign.Center) { Editable = true, Width = "69" },
            new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Center) { Width = "92" },
            new AntdUI.Column("WriteValue", "Write", AntdUI.ColumnAlign.Center) { Editable = true, Width = "92" },
            new AntdUI.Column("Op", "Action", AntdUI.ColumnAlign.Center) { Width = "230" },
        };
    }

    private void SetupEvents()
    {
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += BtnCancel_Click;
        btnAddRow.Click += BtnAddRow_Click;
        btnReadAll.Click += async (_, _) => await ReadAllAsync();
        btnUnlock.Click += (_, _) => ToggleLock();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnPlcName.Click += (_, _) => EditName();

        txtPlc001Ip.TextChanged += (_, _) => txtPlc001Ip.BackColor = Color.LightYellow;
        txtPlc001Port.TextChanged += (_, _) => txtPlc001Port.BackColor = Color.LightYellow;

        tblPlcMap.CellButtonClick += TblPlcMap_CellButtonClick;
        tblPlcMap.CellEndEdit += TblPlcMap_CellEndEdit;
    }

    private void EditName()
    {
        var current = CustomSettingsManager.Read("PLC_NAME", "PLC-001");
        using var dlg = new InputDialog("Rename", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write("PLC_NAME", dlg.Value);
        lblPlcBadge.Text = dlg.Value;
    }

    private void LoadSettings()
    {
        txtPlc001Ip.Text = CustomSettingsManager.Read("PLC_IP", "");
        txtPlc001Port.Text = CustomSettingsManager.Read("PLC_PORT", "502");
        lblPlcBadge.Text = CustomSettingsManager.Read("PLC_NAME", "PLC-001");
        ResetColors();
    }

    // ── Lock / Unlock ──────────────────────────────────────

    private void ToggleLock()
    {
        if (_unlocked)
        {
            _unlocked = false;
            ApplyLockState();
            Log("ล็อกแล้ว — แก้ไขตารางไม่ได้");
            return;
        }

        var password = CustomSettingsManager.Read("PLC_PASSWORD", "1234");
        using var dlg = new InputDialog("Unlock", "กรุณาใส่รหัสผ่าน:", "");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (dlg.Value != password)
        {
            Notify.WarnModal(this, "แจ้งเตือน", "รหัสผ่านไม่ถูกต้อง");
            return;
        }

        _unlocked = true;
        ApplyLockState();
        Log("ปลดล็อกแล้ว — แก้ไขตารางได้");
    }

    private void ApplyLockState()
    {
        btnUnlock.Text = _unlocked ? "🔓 Lock" : "🔒 Unlock";
        btnAddRow.Enabled = _unlocked;
        btnSave.Enabled = _unlocked;
        btnCancel.Enabled = _unlocked;
        btnPlcName.Enabled = _unlocked;
        txtPlc001Ip.Enabled = _unlocked;
        txtPlc001Port.Enabled = _unlocked;

        tblPlcMap.EditMode = _unlocked ? AntdUI.TEditMode.Click : AntdUI.TEditMode.None;
    }

    // ── Status light ───────────────────────────────────────

    public async Task CheckStatusAsync()
    {
        var ip = txtPlc001Ip.Text.Trim();
        var portText = txtPlc001Port.Text.Trim();

        if (string.IsNullOrEmpty(ip) || !int.TryParse(portText, out int port))
        {
            SetStatus(string.IsNullOrEmpty(ip) ? StatusGray : StatusRed);
            return;
        }

        try
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(ip, port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(3000));

            if (IsDisposed) return;
            bool ok = completed == connectTask && !connectTask.IsFaulted && tcp.Connected;
            SetStatus(ok ? StatusGreen : StatusRed);
            Log(ok
                ? $"เชื่อมต่อ {ip}:{port} ได้"
                : $"เชื่อมต่อ {ip}:{port} ไม่ได้");
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            SetStatus(StatusRed);
            Log($"เชื่อมต่อ {ip}:{port} ไม่ได้ — {ex.Message}");
        }
    }

    private void SetStatus(Color color)
    {
        if (IsDisposed) return;
        if (lblPlcStatus.InvokeRequired)
            lblPlcStatus.Invoke(() => { if (!IsDisposed) lblPlcStatus.ForeColor = color; });
        else
            lblPlcStatus.ForeColor = color;
    }

    // ── Register map table ─────────────────────────────────

    private async Task LoadTableAsync()
    {
        try
        {
            var rows = await _api.GetAllPlcSettingsAsync();
            if (IsDisposed) return;

            _rows = rows.Select(FromDto).ToList();
        }
        catch { /* backend not available — keep current rows */ }

        if (_rows.Count == 0)
            _rows = DefaultRows();

        RebindTable();
    }

    private static List<PlcRow> DefaultRows()
    {
        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        return
        [
            new PlcRow { AddressStart = "0",  AddressStop = "0",  PlcStart = "D0",  PlcStop = "D0",  ListName = $"{mk1} Position",  DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "1",  AddressStop = "1",  PlcStart = "D1",  PlcStop = "D1",  ListName = $"{mk1} PostAct",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "2",  AddressStop = "2",  PlcStart = "D2",  PlcStop = "D2",  ListName = $"{mk1} Delay",     DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "3",  AddressStop = "3",  PlcStart = "D3",  PlcStop = "D3",  ListName = $"{mk1} Trigger",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "5",  AddressStop = "5",  PlcStart = "D5",  PlcStop = "D5",  ListName = $"{mk2} Position",  DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "6",  AddressStop = "6",  PlcStart = "D6",  PlcStop = "D6",  ListName = $"{mk2} PostAct",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "7",  AddressStop = "7",  PlcStart = "D7",  PlcStop = "D7",  ListName = $"{mk2} Delay",     DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "8",  AddressStop = "8",  PlcStart = "D8",  PlcStop = "D8",  ListName = $"{mk2} Trigger",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "10", AddressStop = "10", PlcStart = "D10", PlcStop = "D10", ListName = "Conveyor Speed 1",  DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "11", AddressStop = "11", PlcStart = "D11", PlcStop = "D11", ListName = "Conveyor Speed 2",  DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "12", AddressStop = "12", PlcStart = "D12", PlcStop = "D12", ListName = "Conveyor Speed 3",  DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
        ];
    }

    private void RebindTable()
    {
        tblPlcMap.DataSource = null;
        tblPlcMap.DataSource = _rows;
    }

    private void BtnAddRow_Click(object? sender, EventArgs e)
    {
        _rows.Add(new PlcRow { Op = NewRowButtons() });
        RebindTable();
        Log($"เพิ่มแถวใหม่ (รวม {_rows.Count} แถว) — กด Save เพื่อบันทึก");
    }

    private async void TblPlcMap_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Record is not PlcRow row) return;

        if (e.Btn?.Id == "del")
        {
            if (row.IsFixed) return;
            _rows.Remove(row);
            RebindTable();
            Log($"ลบแถว {row.ListName} — กด Save เพื่อบันทึก");
            return;
        }

        var ip = txtPlc001Ip.Text.Trim();
        if (!int.TryParse(txtPlc001Port.Text.Trim(), out int port) || string.IsNullOrEmpty(ip))
        {
            Warn("กรุณาตั้งค่า IP / Port ก่อน");
            return;
        }
        if (!int.TryParse(row.AddressStart, out int addr))
        {
            Warn("Address ไม่ถูกต้อง");
            return;
        }

        if (e.Btn?.Id == "read")
        {
            var (ok, values, error) = await ModbusTcpService.ReadHoldingRegistersAsync(ip, port, addr, 1);
            if (ok && values.Length > 0)
            {
                row.Value = values[0].ToString();
                RebindTable();
                Log($"Read D{addr} ({row.ListName}) → {values[0]}");
            }
            else
            {
                Log($"❌ Read D{addr} ล้มเหลว — {error}");
                Warn($"Read D{addr} ล้มเหลว: {error}");
            }
        }
        else if (e.Btn?.Id == "write")
        {
            if (!int.TryParse(row.WriteValue?.Trim(), out int writeVal))
            {
                Warn($"กรุณากรอกค่าที่ต้องการเขียนลง D{addr}");
                return;
            }

            var (ok, error) = await ModbusTcpService.WriteSingleRegisterAsync(ip, port, addr, writeVal);
            if (ok)
            {
                Log($"Write D{addr} ({row.ListName}) = {writeVal}");

                var (rOk, rVal, _) = await ModbusTcpService.ReadHoldingRegistersAsync(ip, port, addr, 1);
                if (rOk && rVal.Length > 0)
                {
                    row.Value = rVal[0].ToString();
                    Log($"อ่านกลับ D{addr} → {rVal[0]}");
                }
                RebindTable();
            }
            else
            {
                Log($"❌ Write D{addr} ล้มเหลว — {error}");
                Warn($"Write D{addr} ล้มเหลว: {error}");
            }
        }
    }

    private bool TblPlcMap_CellEndEdit(object? sender, AntdUI.TableEndEditEventArgs e)
    {
        if (e.Record is PlcRow row)
        {
            if (e.Column?.Key == "WriteValue")
            {
                row.WriteValue = e.Value ?? "";
                return true;
            }

            if (!_unlocked) return false;

            switch (e.Column?.Key)
            {
                case "AddressStart": row.AddressStart = e.Value ?? ""; break;
                case "AddressStop": row.AddressStop = e.Value ?? ""; break;
                case "PlcStart": row.PlcStart = e.Value ?? ""; break;
                case "PlcStop": row.PlcStop = e.Value ?? ""; break;
                case "ListName": row.ListName = e.Value ?? ""; break;
                case "DataType": row.DataType = e.Value ?? ""; break;
                case "Bit": row.Bit = e.Value ?? ""; break;
            }
        }
        return true;
    }

    // ── Read All ──────────────────────────────────────────

    private async Task ReadAllAsync()
    {
        var ip = txtPlc001Ip.Text.Trim();
        if (!int.TryParse(txtPlc001Port.Text.Trim(), out int port) || string.IsNullOrEmpty(ip))
        {
            Warn("กรุณาตั้งค่า IP / Port ก่อน");
            return;
        }

        if (_rows.Count == 0) return;

        btnReadAll.Loading = true;
        btnReadAll.Enabled = false;

        var addresses = _rows
            .Select(r => int.TryParse(r.AddressStart, out int a) ? a : -1)
            .Where(a => a >= 0)
            .ToList();

        if (addresses.Count == 0)
        {
            btnReadAll.Loading = false;
            btnReadAll.Enabled = true;
            return;
        }

        int minAddr = addresses.Min();
        int maxAddr = addresses.Max();
        int qty = maxAddr - minAddr + 1;

        if (qty > 125)
        {
            Warn("ช่วง Address กว้างเกินไป (สูงสุด 125 registers)");
            btnReadAll.Loading = false;
            btnReadAll.Enabled = true;
            return;
        }

        Log($"Read All — D{minAddr}-D{maxAddr} ({qty} registers)");
        var (ok, values, error) = await ModbusTcpService.ReadHoldingRegistersAsync(ip, port, minAddr, qty);

        if (IsDisposed) return;
        btnReadAll.Loading = false;
        btnReadAll.Enabled = true;

        if (!ok)
        {
            Log($"❌ Read All ล้มเหลว — {error}");
            Warn($"อ่านไม่สำเร็จ: {error}");
            return;
        }

        foreach (var row in _rows)
        {
            if (int.TryParse(row.AddressStart, out int addr))
            {
                int idx = addr - minAddr;
                if (idx >= 0 && idx < values.Length)
                {
                    row.Value = values[idx].ToString();
                    Log($"  D{addr} ({row.ListName}) → {values[idx]}");
                }
            }
        }

        RebindTable();
    }

    // ── Validation ─────────────────────────────────────────

    private bool ValidateRows()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < _rows.Count; i++)
        {
            int n = i + 1;
            var row = _rows[i];
            string listName = (row.ListName ?? "").Trim();

            if (listName.Length == 0)
            {
                Warn($"แถวที่ {n}: กรุณากรอกชื่อ List");
                return false;
            }
            if (string.IsNullOrWhiteSpace(row.PlcStart) || string.IsNullOrWhiteSpace(row.PlcStop))
            {
                Warn($"แถวที่ {n}: กรุณากรอก PLC Start / PLC Stop");
                return false;
            }
            if (!seen.Add(listName))
            {
                Warn($"แถวที่ {n}: ชื่อ List \"{listName}\" ซ้ำกับแถวอื่น");
                return false;
            }

            int start = ParseInt(row.AddressStart);
            int stop = ParseInt(row.AddressStop);
            if (start > stop)
            {
                Warn($"แถวที่ {n}: Address Start ต้องไม่มากกว่า Address Stop");
                return false;
            }
        }

        for (int i = 0; i < _rows.Count; i++)
        {
            for (int j = i + 1; j < _rows.Count; j++)
            {
                int aStart = ParseInt(_rows[i].AddressStart), aStop = ParseInt(_rows[i].AddressStop);
                int bStart = ParseInt(_rows[j].AddressStart), bStop = ParseInt(_rows[j].AddressStop);

                if (aStart <= bStop && bStart <= aStop)
                {
                    Warn($"แถวที่ {i + 1} และแถวที่ {j + 1}: ช่วง Address ทับซ้อนกัน ({aStart}-{aStop} กับ {bStart}-{bStop})");
                    return false;
                }
            }
        }

        return true;
    }

    private static void Warn(string message) =>
        Notify.WarnModal(null, "แจ้งเตือน", message);

    /// <summary>
    /// บันทึกทุกคำสั่งที่ยิงออก PLC — หน้างานต้องดูย้อนหลังได้ว่าสั่งอะไรไปบ้าง
    /// ต้อง marshal เอง เพราะ CheckStatusAsync ถูกเรียกจาก thread อื่นได้
    /// </summary>
    private void Log(string message)
    {
        if (IsDisposed || string.IsNullOrWhiteSpace(message)) return;

        if (txtLog.InvokeRequired)
        {
            txtLog.BeginInvoke(() => Log(message));
            return;
        }

        foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line.TrimEnd()}{Environment.NewLine}");
    }

    // ── Save / Cancel ──────────────────────────────────────

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!ValidateRows()) return;

        CustomSettingsManager.Write("PLC_IP", txtPlc001Ip.Text.Trim());
        CustomSettingsManager.Write("PLC_PORT", txtPlc001Port.Text.Trim());

        lblPlcStatus.ForeColor = StatusGray;
        _ = CheckStatusAsync();

        var dtos = _rows.Select((r, i) => ToDto(r, i)).ToList();

        btnSave.Enabled = false;
        bool ok = await _api.BulkSavePlcSettingsAsync(dtos);
        if (IsDisposed) return;
        btnSave.Enabled = true;

        ResetColors();

        if (ok)
        {
            Log($"บันทึก register map {dtos.Count} แถวเรียบร้อย");
            await LoadTableAsync();
            if (IsDisposed) return;
            Notify.Success(this, "บันทึกเรียบร้อย");
        }
        else
        {
            Log("❌ บันทึกไม่สำเร็จ — ติดต่อ Backend ไม่ได้");
            Notify.ErrorModal(this, "Error", "บันทึกไม่สำเร็จ กรุณาตรวจสอบการเชื่อมต่อ Backend");
        }
    }

    private async void BtnCancel_Click(object? sender, EventArgs e)
    {
        LoadSettings();
        ResetColors();
        await LoadTableAsync();
    }

    private void ResetColors()
    {
        txtPlc001Ip.BackColor = Color.White;
        txtPlc001Port.BackColor = Color.White;
    }

    // ── Mapping helpers ────────────────────────────────────

    private static AntdUI.CellButton[] NewFixedButtons() =>
    [
        new AntdUI.CellButton("read", "Read", AntdUI.TTypeMini.Default) { Radius = 6 },
        new AntdUI.CellButton("write", "Write", AntdUI.TTypeMini.Primary) { Radius = 6 },
    ];

    private static AntdUI.CellButton[] NewRowButtons() =>
    [
        new AntdUI.CellButton("read", "Read", AntdUI.TTypeMini.Default) { Radius = 6 },
        new AntdUI.CellButton("write", "Write", AntdUI.TTypeMini.Primary) { Radius = 6 },
        new AntdUI.CellButton("del", "Del", AntdUI.TTypeMini.Error) { Radius = 6 },
    ];

    private static PlcRow FromDto(PlcRegisterMap d)
    {
        var addr = d.AddressStart.ToString();
        bool isFixed = FixedAddresses.Contains(addr);
        return new PlcRow
        {
            AddressStart = addr,
            AddressStop = d.AddressStop.ToString(),
            PlcStart = d.PlcStart ?? "",
            PlcStop = d.PlcStop ?? "",
            ListName = d.ListName ?? "",
            DataType = string.IsNullOrEmpty(d.DataType) ? "Int" : d.DataType,
            Bit = d.Bit.ToString(),
            IsFixed = isFixed,
            Op = isFixed ? NewFixedButtons() : NewRowButtons(),
        };
    }

    private static PlcRegisterMap ToDto(PlcRow r, int sortOrder) => new()
    {
        AddressStart = ParseInt(r.AddressStart),
        AddressStop = ParseInt(r.AddressStop),
        PlcStart = (r.PlcStart ?? "").Trim(),
        PlcStop = (r.PlcStop ?? "").Trim(),
        ListName = (r.ListName ?? "").Trim(),
        DataType = string.IsNullOrWhiteSpace(r.DataType) ? "Int" : r.DataType.Trim(),
        Bit = ParseIntOr(r.Bit, 32),
        SortOrder = sortOrder,
    };

    private static int ParseInt(string? s) => int.TryParse((s ?? "").Trim(), out int v) ? v : 0;

    private static int ParseIntOr(string? s, int fallback) => int.TryParse((s ?? "").Trim(), out int v) ? v : fallback;
}

internal class PlcRow
{
    public string AddressStart { get; set; } = "0";
    public string AddressStop { get; set; } = "0";
    public string PlcStart { get; set; } = "";
    public string PlcStop { get; set; } = "";
    public string ListName { get; set; } = "";
    public string DataType { get; set; } = "Int";
    public string Bit { get; set; } = "32";
    public string Value { get; set; } = "-";
    public string WriteValue { get; set; } = "";
    public bool IsFixed { get; set; }
    public AntdUI.CellButton[] Op { get; set; } = Array.Empty<AntdUI.CellButton>();
}
