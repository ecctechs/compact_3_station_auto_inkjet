using System.Net.Sockets;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// PLC settings sub-page (under the Setting hub's "PLC" tab).
/// IP/Port are stored locally in Setting.config; the register map is stored in
/// the backend table plc_register_maps and replaced wholesale via bulkSave.
/// </summary>
public partial class PlcSettingUserControl : UserControl
{
    private static readonly Color StatusGray = Color.Gray;
    private static readonly Color StatusGreen = Color.FromArgb(76, 175, 80);
    private static readonly Color StatusRed = Color.FromArgb(220, 38, 38);

    private readonly ApiClient _api;
    private List<PlcRow> _rows = new();

    public PlcSettingUserControl()
    {
        InitializeComponent();
        _api = new ApiClient(BuildBaseUrl());
        ConfigurePlcColumns();
        SetupEvents();
        LoadSettings();

        lblPlcStatus.ForeColor = StatusGray;   // unknown until checked
        CheckPlcStatusAsync();                  // fire-and-forget status probe
        _ = LoadTableAsync();                   // fire-and-forget table load
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
            new AntdUI.Column("AddressStart", "Addr Start", AntdUI.ColumnAlign.Center) { Editable = true, Width = "110" },
            new AntdUI.Column("AddressStop", "Addr Stop", AntdUI.ColumnAlign.Center) { Editable = true, Width = "110" },
            new AntdUI.Column("PlcStart", "PLC Start") { Editable = true, Width = "120" },
            new AntdUI.Column("PlcStop", "PLC Stop") { Editable = true, Width = "120" },
            new AntdUI.Column("ListName", "List Name") { Editable = true, Width = "170" },
            new AntdUI.Column("DataType", "Data Type", AntdUI.ColumnAlign.Center) { Editable = true, Width = "100" },
            new AntdUI.Column("Bit", "Bit", AntdUI.ColumnAlign.Center) { Editable = true, Width = "80" },
            new AntdUI.Column("Op", "Action", AntdUI.ColumnAlign.Center) { Width = "90" },
        };
    }

    private void SetupEvents()
    {
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += BtnCancel_Click;
        btnAddRow.Click += BtnAddRow_Click;
        btnCheckStatus.Click += (_, _) => CheckPlcStatusAsync();

        txtPlc001Ip.TextChanged += (_, _) => txtPlc001Ip.BackColor = Color.LightYellow;
        txtPlc001Port.TextChanged += (_, _) => txtPlc001Port.BackColor = Color.LightYellow;

        tblPlcMap.CellButtonClick += TblPlcMap_CellButtonClick;
        tblPlcMap.CellEndEdit += TblPlcMap_CellEndEdit;
    }

    private void LoadSettings()
    {
        txtPlc001Ip.Text = CustomSettingsManager.Read("PLC_IP", "");
        txtPlc001Port.Text = CustomSettingsManager.Read("PLC_PORT", "502");
        ResetColors();
    }

    // ── Status light ───────────────────────────────────────

    private async void CheckPlcStatusAsync()
    {
        var ip = txtPlc001Ip.Text.Trim();
        if (string.IsNullOrEmpty(ip) || !int.TryParse(txtPlc001Port.Text.Trim(), out int port))
        {
            SetStatus(StatusRed);
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
        }
        catch
        {
            if (IsDisposed) return;
            SetStatus(StatusRed);
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
        var rows = await _api.GetAllPlcSettingsAsync();
        if (IsDisposed) return;

        _rows = rows.Select(FromDto).ToList();
        RebindTable();
    }

    private void RebindTable()
    {
        tblPlcMap.DataSource = null;
        tblPlcMap.DataSource = _rows;
    }

    private void BtnAddRow_Click(object? sender, EventArgs e)
    {
        _rows.Add(new PlcRow { Op = NewDeleteButtons() });
        RebindTable();
    }

    private void TblPlcMap_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Btn?.Id == "del" && e.Record is PlcRow row)
        {
            _rows.Remove(row);
            RebindTable();
        }
    }

    private bool TblPlcMap_CellEndEdit(object? sender, AntdUI.TableEndEditEventArgs e)
    {
        if (e.Record is PlcRow row)
        {
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

        // Cross-row: address ranges must not overlap.
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
        MessageBox.Show(message, "ตรวจสอบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ── Save / Cancel ──────────────────────────────────────

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!ValidateRows()) return;

        CustomSettingsManager.Write("PLC_IP", txtPlc001Ip.Text.Trim());
        CustomSettingsManager.Write("PLC_PORT", txtPlc001Port.Text.Trim());

        lblPlcStatus.ForeColor = StatusGray;
        CheckPlcStatusAsync();

        // SortOrder follows the current row order.
        var dtos = _rows.Select((r, i) => ToDto(r, i)).ToList();

        btnSave.Enabled = false;
        bool ok = await _api.BulkSavePlcSettingsAsync(dtos);
        if (IsDisposed) return;
        btnSave.Enabled = true;

        ResetColors();

        if (ok)
        {
            await LoadTableAsync();   // reload so new rows get their backend Id
            if (IsDisposed) return;
            MessageBox.Show("บันทึกเรียบร้อย", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("บันทึกไม่สำเร็จ กรุณาตรวจสอบการเชื่อมต่อ Backend",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private static AntdUI.CellButton[] NewDeleteButtons() =>
        new[] { new AntdUI.CellButton("del", "Delete", AntdUI.TTypeMini.Error) { Radius = 6 } };

    private static PlcRow FromDto(PlcRegisterMap d) => new()
    {
        AddressStart = d.AddressStart.ToString(),
        AddressStop = d.AddressStop.ToString(),
        PlcStart = d.PlcStart ?? "",
        PlcStop = d.PlcStop ?? "",
        ListName = d.ListName ?? "",
        DataType = string.IsNullOrEmpty(d.DataType) ? "Int" : d.DataType,
        Bit = d.Bit.ToString(),
        Op = NewDeleteButtons(),
    };

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

/// <summary>Row view-model bound to the AntdUI.Table (string cells + delete button).</summary>
internal class PlcRow
{
    public string AddressStart { get; set; } = "0";
    public string AddressStop { get; set; } = "0";
    public string PlcStart { get; set; } = "";
    public string PlcStop { get; set; } = "";
    public string ListName { get; set; } = "";
    public string DataType { get; set; } = "Int";
    public string Bit { get; set; } = "32";
    public AntdUI.CellButton[] Op { get; set; } = Array.Empty<AntdUI.CellButton>();
}
