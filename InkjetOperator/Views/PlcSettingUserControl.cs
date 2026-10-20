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

        // ไฟสถานะต้องตรงกับของจริง ไม่ใช่ภาพนิ่งตั้งแต่ตอนเปิดโปรแกรม
        // กติกาทั้งหมดอยู่ที่ StatusRecheck
        Services.StatusRecheck.Wire(this, tmrAutoCheck, () => CheckStatusAsync());
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
            new AntdUI.Column("AddressStart", "Addr Start", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("AddressStop", "Addr Stop", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("PlcStart", "PLC Start", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("PlcStop", "PLC Stop", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("ListName", "List Name", AntdUI.ColumnAlign.Center) { Editable = true, Width = "18%" },
            new AntdUI.Column("DataType", "Data Type", AntdUI.ColumnAlign.Center) { Editable = true, Width = "9%" },
            new AntdUI.Column("Bit", "Bit", AntdUI.ColumnAlign.Center) { Editable = true, Width = "6%" },
            new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Center) { Width = "8%" },
            new AntdUI.Column("WriteValue", "Write", AntdUI.ColumnAlign.Center) { Editable = true, Width = "8%" },
            new AntdUI.Column("Op", "Action", AntdUI.ColumnAlign.Center) { Width = "15%" },
        };
    }

    private void SetupEvents()
    {
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += BtnCancel_Click;
        btnAddRow.Click += BtnAddRow_Click;

        // เพิ่มแถวใน register map เป็นเรื่องของคนที่รู้ว่า PLC ตัวนี้มี address อะไรบ้าง
        // ไม่ใช่ของคนคุมเครื่อง แถวที่เพิ่มผิดคือส่งค่าไปทับ register อื่นตอนเริ่มงาน
        btnAddRow.Visible = StationService.IsDevMode;

        // ปุ่มทดสอบเลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้น — ของจริงทำเองตอนปล่อยเครื่องแล้วไม่มีคิว
        // ปุ่มนี้มีไว้ลองที่หน้างานโดยไม่ต้องรันงานจริงให้ครบวง
        btnResetPosition.Visible = StationService.IsDevMode;

        // ตัวคูณความเร็วสายพานให้ปรับเฉพาะโหมด Dev — นอกโหมดนี้ซ่อนแถวแล้วคืนความสูงให้ตาราง
        lblConveyorScaleLabel.Visible = StationService.IsDevMode;
        txtConveyorScale.Visible = StationService.IsDevMode;
        if (!StationService.IsDevMode)
        {
            tlpRoot.RowStyles[0].Height -= tlpConn.RowStyles[4].Height;
            tlpConn.RowStyles[4].Height = 0;
        }
        // ปุ่มสแกนช่วง address เป็นเครื่องมือไล่หาว่า register ไหนคือค่าอะไร
        // ไม่ใช่ของที่พนักงานหน้างานต้องใช้ จึงเปิดเฉพาะโหมดทดสอบเหมือน + Add Row
        btnScanRange.Visible = StationService.IsDevMode;

        btnResetPosition.Click += async (_, _) => await ResetPositionAsync();
        btnReadAll.Click += async (_, _) => await ReadAllAsync();
        btnScanRange.Click += async (_, _) => await ScanRangeAsync();
        btnUnlock.Click += (_, _) => ToggleLock();
        btnCheckStatus.Click += async (_, _) => await CheckStatusAsync();
        btnPlcName.Click += (_, _) => EditName();

        txtPlc001Ip.TextChanged += (_, _) => txtPlc001Ip.BackColor = Color.LightYellow;
        txtPlc001Port.TextChanged += (_, _) => txtPlc001Port.BackColor = Color.LightYellow;
        txtHomePosition.TextChanged += (_, _) => txtHomePosition.BackColor = Color.LightYellow;
        NumericInput.DigitsOnly(txtHomePosition);
        txtConveyorScale.TextChanged += (_, _) => txtConveyorScale.BackColor = Color.LightYellow;
        NumericInput.DigitsOnly(txtConveyorScale);

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
        txtHomePosition.Text = PlcOrderService.HomePosition.ToString();
        txtConveyorScale.Text = PlcOrderService.ConveyorScale.ToString();
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
        btnUnlock.Text = _unlocked ? "Lock" : "Unlock";
        btnAddRow.Enabled = _unlocked && StationService.IsDevMode;
        btnSave.Enabled = _unlocked;
        btnCancel.Enabled = _unlocked;
        btnPlcName.Enabled = _unlocked;
        txtPlc001Ip.Enabled = _unlocked;
        txtPlc001Port.Enabled = _unlocked;
        txtHomePosition.Enabled = _unlocked;
        txtConveyorScale.Enabled = _unlocked;

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

        // โหมด Mockup สถานะ (ตัวเลือกหน้างาน) — ขึ้นว่าต่อได้โดยไม่ต่อจริง ใช้ถ่ายรูปคู่มือ
        if (StatusMockup.Enabled)
        {
            SetStatus(StatusGreen);
            Log($"เชื่อมต่อ {ip}:{port} ได้");
            return;
        }

        try
        {
            // ใช้ TcpConnect ไม่ใช่ WhenAny + Delay — แบบเดิมทิ้งการต่อที่ค้างไว้ตอนหมดเวลา
            // พอ socket ถูกปิดมันล้มทีหลังโดยไม่มีใครรับ กลายเป็น ERROR ใน log ทุกรอบตรวจ
            using var tcp = new TcpClient();
            bool ok;
            try
            {
                await Services.TcpConnect.ConnectAsync(tcp, ip, port, TimeSpan.FromSeconds(3));
                ok = tcp.Connected;
            }
            catch (Exception ex) when (ex is TimeoutException or SocketException) { ok = false; }

            if (IsDisposed) return;
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

            var required = RequiredListNames();
            _rows = rows.Select(d => FromDto(d, required)).ToList();
        }
        catch { /* backend not available — keep current rows */ }

        if (_rows.Count == 0)
            _rows = DefaultRows();

        RebindTable();
    }

    /// <summary>
    /// แถวตั้งต้นในหน้า PLC Setting — รวมแถวสายพานสำหรับปุ่ม Read/Write แบบกดเอง
    ///
    /// <para>
    /// เดิมมี 11 แถวตามผังของโปรแกรมเก่า คือหัวละ 4 ช่อง (Position, PostAct, Delay,
    /// Trigger) และสายพาน 3 ตัว แต่ Position กับ Trigger ไม่เคยถูกส่งค่าอื่นนอกจาก 0
    /// และหน้างานมีสายพานเดียว ตอนนี้จึงเหลือ 5 แถวที่ใช้จริง
    /// </para>
    /// <para>
    /// Conveyor Speed 1 คงไว้สำหรับการเขียนแบบกดเอง และไม่ได้อยู่ในแผนส่งงาน MK
    /// </para>
    /// </summary>
    private static List<PlcRow> DefaultRows()
    {
        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        return
        [
            new PlcRow { AddressStart = "0",  AddressStop = "0",  PlcStart = "D0",  PlcStop = "D0",  ListName = $"{mk1} PostAct",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "1",  AddressStop = "1",  PlcStart = "D1",  PlcStop = "D1",  ListName = $"{mk1} Delay",     DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "5",  AddressStop = "5",  PlcStart = "D5",  PlcStop = "D5",  ListName = $"{mk2} PostAct",   DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "6",  AddressStop = "6",  PlcStart = "D6",  PlcStop = "D6",  ListName = $"{mk2} Delay",     DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
            new PlcRow { AddressStart = "10", AddressStop = "10", PlcStart = "D10", PlcStop = "D10", ListName = PlcOrderService.ConveyorListName, DataType = "Int", Bit = "16", IsFixed = true, Op = NewFixedButtons() },
        ];
    }

    private void RebindTable()
    {
        tblPlcMap.DataSource = null;
        tblPlcMap.DataSource = _rows;
    }

    /// <summary>
    /// ทดสอบเลื่อนหัวพิมพ์กลับตำแหน่งเริ่มต้น — เขียน 0 ลงช่องเดียวกับที่ส่งค่าของงาน
    ///
    /// <para>
    /// ถามยืนยันก่อน เพราะเป็นการเขียนค่าลง PLC ของเครื่องที่อาจกำลังเดินอยู่
    /// และบอกให้ครบว่าจะเขียนอะไรลง register ไหน แบบเดียวกับปุ่ม Write ในตาราง
    /// </para>
    /// </summary>
    private async Task ResetPositionAsync()
    {
        int home = PlcOrderService.HomePosition;
        if (!Confirm.Ask(this, new ConfirmSpec
            {
                Title = "รีเซ็ตตำแหน่งหัวพิมพ์",
                Headline = "เลื่อนหัวพ่นทั้งสองตัวกลับตำแหน่งเริ่มต้น?",
                Facts = [("ค่าที่เขียน", home.ToString()), ("ช่อง", "ตำแหน่งหัวพ่นทั้งสองตัว (Servo Post Act.)")],
                Warning = "เขียนลง PLC ทันที — ตรวจว่าเครื่องไม่ได้เดินงานอยู่",
                OkText = "รีเซ็ต",
                Icon = "UndoOutlined",
                OkIcon = "UndoOutlined",
            }))
            return;

        btnResetPosition.Enabled = false;
        var originalText = btnResetPosition.Text;
        btnResetPosition.Text = "กำลังส่ง...";
        try
        {
            var results = await PlcOrderService.ResetPositionAsync(_api);
            if (IsDisposed) return;

            if (results.Count == 0)
            {
                Warn("ตาราง register map ยังไม่มีแถวของหัวพ่น — ไม่มี address ให้เขียน");
                return;
            }

            foreach (var r in results)
            {
                if (r.Error != null)
                {
                    Log($"❌ {r.Name} = {r.Value} — {r.Error}");
                    continue;
                }

                Log(r.ReadBack == r.Value
                    ? $"{r.Name} = {r.Value}"
                    : $"⚠ {r.Name} = {r.Value} · อ่านกลับได้ {r.ReadBack?.ToString() ?? "ไม่ได้"}");
            }

            var failed = results.Where(r => r.Error != null).ToList();
            if (failed.Count > 0)
            {
                Warn("รีเซ็ตไม่สำเร็จ — " + string.Join(" · ", failed.Select(r => r.Error)));
                return;
            }

            Notify.Success(this, $"รีเซ็ตตำแหน่งหัวพิมพ์เป็น {home} แล้ว");
        }
        finally
        {
            if (!IsDisposed)
            {
                btnResetPosition.Text = originalText;
                btnResetPosition.Enabled = true;
            }
        }
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

        // ล็อกอยู่ให้อ่านได้อย่างเดียว การเขียนค่าลง PLC หรือลบแถวต้องปลดล็อกก่อน
        // เพราะเขียนผิด register เดียวก็กระทบเครื่องจักรที่กำลังเดินอยู่
        if (!_unlocked && e.Btn?.Id != "read")
        {
            Warn("ล็อกอยู่ — กด Unlock ก่อนจึงจะส่งค่าไปที่ PLC ได้");
            return;
        }

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
            if (!int.TryParse(row.WriteValue?.Trim(), out int typed))
            {
                Warn($"กรุณากรอกค่าที่ต้องการเขียนลง D{addr}");
                return;
            }

            // แถวสายพานหน้านี้รับเป็น Hz ไม่ใช่ค่าดิบของ register
            // การแปลงเรียก PlcOrderService.HzToRegister ตัวเดียวกับที่หน้านั้นใช้
            bool isConveyor = string.Equals(
                row.ListName?.Trim(), PlcOrderService.ConveyorListName, StringComparison.OrdinalIgnoreCase);

            // กันคนพิมพ์ค่าดิบแบบเดิมลงแถวที่ตอนนี้รับเป็น Hz
            //
            // พิมพ์ 5000 จะกลายเป็น 500000 แล้วถูกตัดเหลือ 32767 ซึ่งคือสั่งสายพาน
            // วิ่งสุดโดยไม่มีอะไรฟ้อง เพดานคิดจากขนาด register ไม่ได้ตั้งเลขเอง
            int maxHz = short.MaxValue / PlcOrderService.ConveyorScale;
            if (isConveyor && (typed < 0 || typed > maxHz))
            {
                Warn($"แถวนี้กรอกเป็น Hz (0-{maxHz}) ไม่ใช่ค่าดิบของ register");
                return;
            }

            int writeVal = isConveyor ? PlcOrderService.HzToRegister(typed) : typed;

            // ทางเดียวกับที่ PlcOrderService.SendAsync ใช้ตอนส่งค่าของงาน — เขียน FC 6
            // แล้วอ่านกลับบนสายเดิม ผลที่ได้จากปุ่มนี้กับจากการส่งงานจริงจึงตรงกันเสมอ
            var (ok, readBack, error) = await ModbusTcpService.WriteAndVerifyAsync(ip, port, addr, writeVal);
            if (ok)
            {
                Log(isConveyor
                    ? $"Write D{addr} ({row.ListName}) = {typed} Hz → {writeVal}"
                    : $"Write D{addr} ({row.ListName}) = {writeVal}");

                if (readBack != null)
                {
                    row.Value = readBack.Value.ToString();
                    Log($"อ่านกลับ D{addr} → {readBack.Value}");

                    // เขียนผ่านแต่ค่าไม่อยู่ = PLC รับคำสั่งแล้วมีอย่างอื่นเขียนทับ
                    // ไม่บอกตรงนี้จะเข้าใจว่าเขียนสำเร็จแล้วทั้งที่ค่าไม่เข้า
                    if (readBack.Value != writeVal)
                        Warn($"D{addr} เขียน {writeVal} แต่อ่านกลับได้ {readBack.Value} — ค่าไม่อยู่");
                }
                else
                {
                    Log($"อ่านกลับ D{addr} ไม่ได้");
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

    // ── สแกนช่วง address เพื่อหาว่า register ไหนคือค่าอะไร ────

    /// <summary>address แรกของช่วงที่สแกน</summary>
    private const int ScanFirst = 1;

    /// <summary>address สุดท้ายของช่วงที่สแกน — Modbus อ่านได้ครั้งละไม่เกิน 125</summary>
    private const int ScanLast = 100;

    /// <summary>ค่าที่อ่านได้จากการสแกนครั้งก่อน — null ในช่องไหนคือ address นั้นอ่านไม่ได้</summary>
    private int?[]? _scanBefore;

    /// <summary>
    /// อ่าน D1-D100 รวดเดียวแล้วเทียบกับรอบก่อน
    ///
    /// <para>
    /// มีไว้หาว่า register ไหนคือค่าอะไร โดยไม่ต้องเดาและไม่ต้องเขียนอะไรลง PLC —
    /// กดครั้งแรกเก็บค่าไว้ ไปปรับค่านั้นที่หน้าเครื่อง (เช่นหมุนความเร็วสายพาน)
    /// แล้วกดอีกครั้ง ตัวที่ค่าเปลี่ยนคือ register ของค่านั้น
    /// </para>
    /// <para>
    /// อ่านอย่างเดียวล้วน ๆ จึงกดตอนไลน์เดินอยู่ได้ ไม่กระทบงานที่กำลังพิมพ์
    /// การไล่เขียนทีละตัวเพื่อเดาว่าตัวไหนคืออะไรทำแบบนั้นไม่ได้ เพราะ D0/D1/D5/D6
    /// เป็น PostAct กับ Delay ที่สั่งหัวพ่นเลื่อนจริง
    /// </para>
    /// </summary>
    private async Task ScanRangeAsync()
    {
        var ip = txtPlc001Ip.Text.Trim();
        if (!int.TryParse(txtPlc001Port.Text.Trim(), out int port) || string.IsNullOrEmpty(ip))
        {
            Warn("กรุณาตั้งค่า IP / Port ก่อน");
            return;
        }

        const int qty = ScanLast - ScanFirst + 1;

        btnScanRange.Loading = true;
        btnScanRange.Enabled = false;

        Log($"สแกน D{ScanFirst}-D{ScanLast} ({qty} registers) — อ่านอย่างเดียว ไม่เขียนอะไรลง PLC");

        var (values, error) = await ReadRangeAsync(ip, port, qty);

        if (IsDisposed) return;
        btnScanRange.Loading = false;
        btnScanRange.Enabled = true;

        if (values == null)
        {
            Log($"❌ สแกนล้มเหลว — {error}");
            Warn($"สแกนไม่สำเร็จ: {error}");
            return;
        }

        int readable = values.Count(v => v != null);
        if (readable == 0)
        {
            Log("❌ อ่านไม่ได้สักตัวเดียวในช่วงนี้");
            Warn("อ่านไม่ได้สักตัวเดียวในช่วงนี้");
            return;
        }

        if (readable < qty)
        {
            var exist = Enumerable.Range(0, qty).Where(i => values[i] != null).ToList();
            Log($"PLC เปิดให้อ่านได้ {readable} จาก {qty} ตัว — ต่ำสุด D{ScanFirst + exist[0]} สูงสุด D{ScanFirst + exist[^1]}");
        }

        if (_scanBefore == null)
        {
            _scanBefore = values;
            LogScanDump(values);
            Log("เก็บค่าชุดแรกไว้แล้ว — ไปเปลี่ยนค่าที่หน้าเครื่อง (เช่นปรับความเร็วสายพาน) แล้วกดสแกนอีกครั้ง");
            return;
        }

        var changed = new List<string>();
        for (int i = 0; i < qty; i++)
        {
            // ตัวที่อ่านไม่ได้ในรอบใดรอบหนึ่งเทียบไม่ได้ ข้ามไป ไม่นับว่าเปลี่ยน
            if (_scanBefore[i] == null || values[i] == null) continue;
            if (_scanBefore[i] != values[i])
                changed.Add($"  D{ScanFirst + i}: {_scanBefore[i]} → {values[i]}");
        }

        if (changed.Count == 0)
        {
            Log("เทียบกับรอบก่อนแล้ว ไม่มี register ไหนเปลี่ยนเลย");
        }
        else
        {
            Log($"register ที่ค่าเปลี่ยนไป {changed.Count} ตัว:");
            foreach (var line in changed) Log(line);
        }

        _scanBefore = values;
    }

    /// <summary>
    /// อ่านทั้งช่วง คืน null ในช่องที่อ่านไม่ได้
    ///
    /// <para>
    /// ลองขอทีเดียวทั้งก้อนก่อนเพราะเร็วกว่ามาก แต่ Modbus ปฏิเสธทั้งคำขอถ้ามี
    /// address ไหนอยู่นอกช่วงที่ PLC เปิดไว้ แม้ตัวอื่นจะอ่านได้จริง (ตอบกลับเป็น
    /// FC=0x83 code=0x02) เจอแบบนั้นค่อยไล่ทีละตัวบนสายเดิม จะได้รู้ว่ามีถึงไหน
    /// </para>
    /// </summary>
    private static async Task<(int?[]? values, string error)> ReadRangeAsync(string ip, int port, int qty)
    {
        var (session, connectError) = await ModbusTcpService.OpenAsync(ip, port);
        if (session == null) return (null, connectError);

        using (session)
        {
            var (ok, block, _) = await session.ReadHoldingRegistersAsync(ScanFirst, qty);
            if (ok && block.Length >= qty)
                return (block.Take(qty).Select(v => (int?)v).ToArray(), "");

            // ไล่ทีละตัวบนสายเดิม ไม่เปิดปิดใหม่ทุกครั้ง ไม่งั้นร้อยรอบคือร้อยการเชื่อมต่อ
            var values = new int?[qty];
            string lastError = "";
            int failures = 0;

            for (int i = 0; i < qty; i++)
            {
                var (one, v, err) = await session.ReadHoldingRegistersAsync(ScanFirst + i, 1);
                if (one && v.Length > 0)
                {
                    values[i] = v[0];
                }
                else
                {
                    lastError = err;
                    failures++;
                }
            }

            // พังหมดทุกตัวแปลว่าสายหลุดหรือ PLC ไม่รับเลย ไม่ใช่แค่ช่วงแคบ
            return failures == qty ? (null, lastError) : (values, "");
        }
    }

    /// <summary>พิมพ์ค่าทั้งช่วงลง log แถวละ 10 ตัว ไม่งั้นร้อยตัวเรียงกันอ่านไม่ออก</summary>
    private void LogScanDump(int?[] values)
    {
        for (int i = 0; i < values.Length; i += 10)
        {
            var cells = values.Skip(i).Take(10)
                .Select(v => (v?.ToString() ?? "-").PadLeft(7));
            Log($"  D{ScanFirst + i,-3}|{string.Concat(cells)}");
        }
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
        if (!TryReadHomePosition(out int home)) return;
        if (!TryReadConveyorScale(out int scale)) return;

        CustomSettingsManager.Write("PLC_IP", txtPlc001Ip.Text.Trim());
        CustomSettingsManager.Write("PLC_PORT", txtPlc001Port.Text.Trim());
        CustomSettingsManager.Write(PlcOrderService.HomePositionKey, home.ToString());
        if (StationService.IsDevMode) // นอกโหมด Dev ช่องถูกซ่อน ไม่เขียนทับค่าที่ตั้งไว้
            CustomSettingsManager.Write(PlcOrderService.ConveyorScaleKey, scale.ToString());

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
            Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", "ตรวจการเชื่อมต่อ Backend แล้วลองใหม่");
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
        txtHomePosition.BackColor = Color.White;
        txtConveyorScale.BackColor = Color.White;
    }

    private bool TryReadConveyorScale(out int scale)
    {
        // ช่องถูกซ่อนนอกโหมด Dev ค่าที่มองไม่เห็นต้องไม่มาขวางการบันทึก
        if (!StationService.IsDevMode)
        {
            scale = PlcOrderService.ConveyorScale;
            return true;
        }

        bool parsed = int.TryParse(txtConveyorScale.Text.Trim(), out scale);

        // 120 เป็นค่าที่วัดแล้วว่าผิดกับเครื่องชุดนี้ บอกให้ชัดตรงนี้ ไม่ใช่ปล่อยให้
        // บันทึกผ่านแล้วโปรแกรมเงียบ ๆ ใช้ 100 แทน ซึ่งคนกรอกจะงงว่าทำไมไม่เป็นไปตามที่ใส่
        if (parsed && scale == 120)
        {
            Notify.WarnModal(this, "ตัวคูณความเร็วสายพาน",
                "ตั้ง 120 ไม่ได้ — วัดที่หน้างานแล้วค่านี้ทำให้สั่ง 50 Hz แต่สายพานเดิน 60 Hz"
                + Environment.NewLine + Environment.NewLine
                + $"ค่าที่ถูกคือ {PlcOrderService.DefaultConveyorScale}");
            return false;
        }

        if (parsed && scale is >= 1 and <= PlcOrderService.MaxConveyorScale)
            return true;

        Notify.WarnModal(this, "ตัวคูณความเร็วสายพาน",
            $"ใส่เลข 1 ถึง {PlcOrderService.MaxConveyorScale}"
            + Environment.NewLine + Environment.NewLine
            + "ค่า Hz ใน Pattern จะถูกคูณด้วยค่านี้ก่อนเขียนลง Conveyor Speed 1");
        return false;
    }

    /// <summary>
    /// อ่านตำแหน่งเริ่มต้นจากช่องกรอก — ผิดแล้วบอกคนกรอก ไม่บันทึกอะไรเลย
    ///
    /// <para>
    /// ไม่รับ 0 เพราะ ladder ของ PLC หน้างานไม่ขยับหัวพิมพ์เมื่อได้ 0 ถ้ายอมให้บันทึก
    /// ปุ่มกลับตำแหน่งเริ่มต้นจะดูเหมือนสำเร็จทั้งที่หัวไม่ได้ไปไหน
    /// </para>
    /// </summary>
    private bool TryReadHomePosition(out int home)
    {
        if (int.TryParse(txtHomePosition.Text.Trim(), out home)
            && home is >= 1 and <= PlcOrderService.MaxHomePosition)
            return true;

        Notify.WarnModal(this, "ตำแหน่งเริ่มต้นหัวพิมพ์",
            $"ใส่เลข 1 ถึง {PlcOrderService.MaxHomePosition}"
            + Environment.NewLine + Environment.NewLine
            + "ใช้ 0 ไม่ได้ เพราะ PLC ไม่ขยับหัวพิมพ์เมื่อได้ค่า 0");
        return false;
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

    /// <summary>
    /// แถวที่โปรแกรมส่งค่าให้จริง — ลบไม่ได้ แต่แก้ address ได้ตลอด
    ///
    /// <para>
    /// ชื่อพวกนี้คือกุญแจที่ <see cref="PlcOrderService"/> ใช้หา address ตอนส่งค่า
    /// เข้า PLC และที่หน้า Order Detail ใช้แสดงว่าแต่ละช่องจะถูกส่งไป register ไหน
    /// ทั้งสองที่จับคู่ด้วย <c>list_name</c> ไม่ใช่ address — ย้าย address ได้อิสระ
    /// แต่ลบแถวไหนทิ้ง ค่านั้นจะเลิกถูกส่งทันทีโดยไม่มีอะไรฟ้อง
    /// </para>
    /// <para>
    /// เดิมตัดสินจาก address ที่ฝังเป็นชุดตัวเลขไว้ในโค้ด ซึ่งขัดกับการที่ address
    /// แก้ได้ — ย้ายแถวไป address นอกชุดแล้วบันทึก แถวเดิมจะกลายเป็นแถวลบได้
    /// และแถวที่เพิ่มเองซึ่งบังเอิญไปตรงเลขในชุดจะกลายเป็นลบไม่ได้
    /// </para>
    /// </summary>
    private static HashSet<string> RequiredListNames()
    {
        var mk1 = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        var mk2 = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"{mk1} PostAct",
            $"{mk1} Delay",
            $"{mk2} PostAct",
            $"{mk2} Delay",
            PlcOrderService.ConveyorListName,
        };
    }

    private static PlcRow FromDto(PlcRegisterMap d, HashSet<string> required)
    {
        bool isFixed = required.Contains((d.ListName ?? "").Trim());
        return new PlcRow
        {
            AddressStart = d.AddressStart.ToString(),
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
