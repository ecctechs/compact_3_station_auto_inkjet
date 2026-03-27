using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Xml.Linq;
using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.PlcAdapter;
using InkjetOperator.Services;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.Logging;

namespace InkjetOperator;

/// <summary>
/// Main operator form — polls backend for pending jobs, displays pattern detail,
/// sends commands to inkjet adapters, posts results back.
/// Follows Linx frmMain pattern: timer polling, Invoke() for thread safety,
/// manager instances created directly (no DI).
/// </summary>
public partial class frmMain : Form
{
    // ── Hardware managers ──
    private Rs232Manager _rs232Ij1 = new();
    private Rs232Manager _rs232Ij2 = new();
    private TcpManager _tcpIj3 = new();
    private TcpManager _tcpIj4 = new();
    private PlcManager _plc = new();

    // ── Adapters ──
    private MkCompactAdapter _adapterIj1;
    private MkCompactAdapter _adapterIj2;
    private SqliteInkjetAdapter _adapterIj3;
    private SqliteInkjetAdapter _adapterIj4;

    // ── API ──
    private ApiClient _api;

    // ── State ──
    private List<PrintJob> _pendingJobs = new();
    private ResolvedJobResponse? _currentResolved;
    private int _selectedJobId = -1;

    private bool _isUpdatingUv = false;
    private BindingList<JobRow> _jobBindingList = new();
    private BindingList<InkjetConfigDto> _configBindingList = new();
    private BindingList<TextBlockDto> _textBlockBindingList = new();
    private BindingList<UvRow> _uvBindingList = new();

    private bool _isRefreshingJobs = false;

    private int currentStep = 1;

    public frmMain()
    {
        InitializeComponent();

        _adapterIj1 = new MkCompactAdapter(_rs232Ij1);
        _adapterIj2 = new MkCompactAdapter(_rs232Ij2);
        _adapterIj3 = new SqliteInkjetAdapter(_tcpIj3, ""); // db path TBD
        _adapterIj4 = new SqliteInkjetAdapter(_tcpIj4, ""); // db path TBD

        // เพิ่มบรรทัดนี้เพื่อให้ dgvUVBlocks สร้างคอลัมน์ตาม properties ของ TextBlockDto
        dgvUVBlocks.AutoGenerateColumns = true;
        dgvTextBlocks.AutoGenerateColumns = true;

        _api = new ApiClient("http://localhost:3000");
    }

    // ════════════════════════════════════════
    //  Form events
    // ════════════════════════════════════════

    private async void frmMain_Load(object sender, EventArgs e)
    {
        InitializeDatabase();
        currentStep = 1;
        UpdateStepButtons();
        // Populate COM port dropdowns
        string[] ports = SerialPort.GetPortNames();
        cmbCom1.Items.AddRange(ports);
        cmbCom2.Items.AddRange(ports);
        if (cmbCom1.Items.Count > 0) cmbCom1.SelectedIndex = 0;
        if (cmbCom2.Items.Count > 1) cmbCom2.SelectedIndex = 1;
        else if (cmbCom2.Items.Count > 0) cmbCom2.SelectedIndex = 0;

        //await SetupUvTableAsync();
        await LoadUvDataToGrid();

        dgvUVBlocks.CellValueChanged += dgvUVBlocks_CellValueChanged;

        dgvUVBlocks.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (dgvUVBlocks.IsCurrentCellDirty)
            {
                dgvUVBlocks.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        dgvJobs.AutoGenerateColumns = true;
        dgvJobs.DataSource = _jobBindingList;
        dgvConfigs.DataSource = _configBindingList;
        dgvTextBlocks.DataSource = _textBlockBindingList;
        dgvUVBlocks.AutoGenerateColumns = false;
        dgvUVBlocks.DataSource = _uvBindingList;

        dgvUVBlocks.Columns.Clear();

        dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Inkjet",
            DataPropertyName = "Inkjet",
            ReadOnly = true,
            Width = 120
        });

        dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Lot",
            DataPropertyName = "Lot",
            Width = 150
        });

        dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Name",
            DataPropertyName = "Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        tmrPoll.Start();
        Log("Application started. Polling every 5s.");
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        tmrPoll.Stop();
        _rs232Ij1.CloseSerialPort();
        _rs232Ij2.CloseSerialPort();
        _tcpIj3.Disconnect();
        _tcpIj4.Disconnect();
        _plc.Disconnect();
    }

    // ════════════════════════════════════════
    //  Connection handlers
    // ════════════════════════════════════════

    private void btnConnectRs232_Click(object sender, EventArgs e)
    {
        try
        {
            if (cmbCom1.SelectedItem != null)
            {
                _rs232Ij1.ConfigureSerialPort(cmbCom1.SelectedItem.ToString()!);
                _rs232Ij1.OpenSerialPort();
            }
            if (cmbCom2.SelectedItem != null)
            {
                _rs232Ij2.ConfigureSerialPort(cmbCom2.SelectedItem.ToString()!);
                _rs232Ij2.OpenSerialPort();
            }
            UpdateDeviceStatus();
            Log("RS232 connected.");
        }
        catch (Exception ex)
        {
            Log("RS232 connect error: " + ex.Message);
        }
    }

    private void btnDisconnectRs232_Click(object sender, EventArgs e)
    {
        _rs232Ij1.CloseSerialPort();
        _rs232Ij2.CloseSerialPort();
        UpdateDeviceStatus();
        Log("RS232 disconnected.");
    }

    private async void btnConnectTcp_Click(object sender, EventArgs e)
    {
        try
        {
            string host = txtTcpHost.Text.Trim();
            int port = int.Parse(txtTcpPort.Text.Trim());
            await _tcpIj3.ConnectAsync(host, port);
            await _tcpIj4.ConnectAsync(host, port + 1); // IJ4 on next port
            UpdateDeviceStatus();
            Log("TCP connected.");
        }
        catch (Exception ex)
        {
            Log("TCP connect error: " + ex.Message);
        }
    }

    private void btnDisconnectTcp_Click(object sender, EventArgs e)
    {
        _tcpIj3.Disconnect();
        _tcpIj4.Disconnect();
        UpdateDeviceStatus();
        Log("TCP disconnected.");
    }

    private void btnApplyApi_Click(object sender, EventArgs e)
    {
        string url = txtApiUrl.Text.Trim();
        _api = new ApiClient(url);
        lblApiStatus.Text = "Applied";
        Log($"API URL set to: {url}");
    }

    // ════════════════════════════════════════
    //  Polling
    // ════════════════════════════════════════

    private async void tmrPoll_Tick(object sender, EventArgs e)
    {
        tmrPoll.Stop(); // Prevent re-entry
        try
        {
            var jobs = await _api.GetPendingJobsAsync();
            _pendingJobs = jobs;
            UpdateJobGrid();
            UpdateDeviceStatus();
            lblApiStatus.Text = "OK";
        }
        catch (Exception ex)
        {
            lblApiStatus.Text = "Error";
            Log("Poll error: " + ex.Message);
        }
        finally
        {
            tmrPoll.Start();
        }
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        tmrPoll_Tick(sender, e);
    }

    // ════════════════════════════════════════
    //  Job selection
    // ════════════════════════════════════════
    private async void dgvJobs_SelectionChanged(object sender, EventArgs e)
    {
        if (_isRefreshingJobs) return; // 🔥 กันตอน poll

        if (dgvJobs.SelectedRows.Count == 0) return;

        var row = dgvJobs.SelectedRows[0];
        if (row.Cells["Id"].Value == null) return;

        int jobId = (int)row.Cells["Id"].Value;
        string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
        if (jobId == _selectedJobId) return;
        _selectedJobId = jobId;

        // Helpers
        static bool ReaderHasColumn(SqliteDataReader r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static string? GetStringSafe(SqliteDataReader r, string name)
        {
            if (!ReaderHasColumn(r, name)) return null;
            int idx = r.GetOrdinal(name);
            return r.IsDBNull(idx) ? null : r.GetString(idx);
        }

        static int? GetIntSafe(SqliteDataReader r, string name)
        {
            var s = GetStringSafe(r, name);
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s, out var v) ? v : null;
        }

        try
        {
            string dbPath = @"D:\DB\uv_data.db3";
            string connStr = $"Data Source={dbPath}";
            bool found = false;

            try
            {
                await using var conn = new SqliteConnection(connStr);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
                cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Build PatternDetail from known schema columns.
                    var pattern = new PatternDetail
                    {
                        Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
                        Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
                    };

                    // Helper to build MK config (mk1 / mk2)
                    InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
                    {
                        var cfg = new InkjetConfigDto
                        {
                            Ordinal = ordinal,
                            ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
                            ProgramName = GetStringSafe(reader, programNameColumn),
                            Width = GetIntSafe(reader, $"{prefix}width"),
                            Height = GetIntSafe(reader, $"{prefix}height"),
                            TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
                            Direction = GetIntSafe(reader, $"{prefix}text_direction"),
                            SteelType = null,
                            Suspended = false
                        };

                        // text blocks 1..5
                        for (int b = 1; b <= 5; b++)
                        {
                            string textCol = $"{prefix}block{b}_text";
                            if (ReaderHasColumn(reader, textCol))
                            {
                                var text = GetStringSafe(reader, textCol);
                                if (!string.IsNullOrEmpty(text))
                                {
                                    var tb = new TextBlockDto
                                    {
                                        BlockNumber = b,
                                        Text = text,
                                        X = GetIntSafe(reader, $"{prefix}block{b}_x"),
                                        Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
                                        Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
                                        Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
                                    };
                                    cfg.TextBlocks.Add(tb);
                                }
                            }
                        }

                        return cfg;
                    }

                    // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
                    var mk1 = BuildMkConfig("mk1_", 1, "program_name");
                    // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
                    var mk2 = BuildMkConfig("mk2_", 2, "program_name3");

                    pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };

                    // Conveyor speeds / servo configs - attempt to map if available (optional)
                    var spd1 = GetIntSafe(reader, "belt1_inkjet");
                    var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
                    if (spd1.HasValue || spd2.HasValue)
                    {
                        pattern.ConveyorSpeeds = new ConveyorSpeedDto
                        {
                            Speed1 = spd1,
                            Speed2 = spd2,
                            Speed3 = GetIntSafe(reader, "belt3")
                        };
                    }

                    // Minimal job + resolved response
                    _currentResolved = new ResolvedJobResponse
                    {
                        Job = new PrintJob
                        {
                            Id = jobId,
                            BarcodeRaw = rawbarcode,
                            LotNumber = row.Cells["LotNumber"].Value?.ToString(),
                            Status = row.Cells["Status"].Value?.ToString()
                        },
                        Pattern = pattern
                    };

                    string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
                    string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ

                    // 🔥 background + กันค้าง
                    //_ = Task.Run(async () =>
                    //{
                    //    try
                    //    {
                    //        await UpdateUvData(lot, name);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Log("UV DB error: " + ex.Message);
                    //    }
                    //});

                    found = true;
                    UpdateDetailPanel();
                }

                await conn.CloseAsync();
            }
            catch (Exception dbEx)
            {
                Log("SQLite query error: " + dbEx.Message);
            }

            if (!found)
            {
                var resolved = await _api.GetResolvedJobAsync(jobId);
                _currentResolved = resolved;
                UpdateDetailPanel();
            }
        }
        catch (Exception ex)
        {
            Log("Load job detail error: " + ex.Message);
        }
    }

    private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvConfigs.SelectedRows.Count == 0 || _currentResolved?.Pattern == null) return;

        int idx = dgvConfigs.SelectedRows[0].Index;
        var configs = _currentResolved.Pattern.InkjetConfigs;

        if (configs == null || idx >= configs.Count) return;

        var config = configs[idx];

        // dgvTextBlocks แสดงตามแถวที่เลือก (ปกติคือ MK1, MK2)
        //dgvTextBlocks.DataSource = null;
        _textBlockBindingList.RaiseListChangedEvents = false;
        _textBlockBindingList.Clear();

        foreach (var b in config.TextBlocks ?? new List<TextBlockDto>())
        {
            _textBlockBindingList.Add(b);
        }

        foreach (var block in config.TextBlocks)
        {
            // --- ส่วนที่แก้ไข/เพิ่มใหม่ ---
            // ใช้ LINQ ค้นหา Pattern ที่ชื่อตรงกับ block.Text (เช่น "DDDD")
            //var pattern = PatternEngine.Patterns.FirstOrDefault(p => p.Name == block.Text);
            string previewText = PatternEngine.Process(txtLot.Text, block.Text);
            //Debug.WriteLine(block.Text);

            if (previewText != null)
            {
                // ถ้าเจอ ให้เอาค่าจาก txtLot (หรือ res.Job.LotNumber) มาแปลงด้วย Rule
                block.RuleResult = previewText;
            }
            else
            {
                // ถ้าไม่เจอ ให้แสดงค่าเดิมของมัน
                block.RuleResult = block.Text;
            }
            // --------------------------

            _textBlockBindingList.Add(block);
        }

        _textBlockBindingList.RaiseListChangedEvents = true;
        _textBlockBindingList.ResetBindings();

        // --- นำ UpdateUvGridOnly(config.TextBlocks) ออก ---
        // เพื่อให้ dgvUVBlocks ไม่เปลี่ยนค่าตามการเลือกใน Grid นี้
    }

    // ════════════════════════════════════════
    //  Send to devices
    // ════════════════════════════════════════

    private async void btnSend_Click(object sender, EventArgs e)
    {
        if (_currentResolved == null || _selectedJobId < 0)
        {
            MessageBox.Show("No job selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnSend.Enabled = false;
        var commandResults = new List<CommandResult>();

        try
        {
            // Mark job as executing
            bool ok = await _api.ExecuteJobAsync(_selectedJobId);
            if (!ok)
            {
                Log("Failed to mark job as executing.");
                btnSend.Enabled = true;
                return;
            }

            // Re-fetch resolved data (templates may depend on attempt number)
            var resolved = await _api.GetResolvedJobAsync(_selectedJobId);
            if (resolved?.Pattern == null)
            {
                Log("Failed to get resolved job.");
                btnSend.Enabled = true;
                return;
            }

            _currentResolved = resolved;
            UpdateDetailPanel();

            // Send to each inkjet by ordinal
            var configs = resolved.Pattern.InkjetConfigs ?? new List<InkjetConfigDto>();
            bool hasError = false;

            foreach (var config in configs.OrderBy(c => c.Ordinal))
            {
                if (config.Suspended)
                {
                    Log($"IJ{config.Ordinal} suspended — skipping.");
                    continue;
                }

                IInkjetAdapter adapter = GetAdapterByOrdinal(config.Ordinal);

                if (!adapter.IsConnected())
                {
                    Log($"IJ{config.Ordinal} not connected — skipping.");
                    commandResults.Add(new CommandResult
                    {
                        Ordinal = config.Ordinal,
                        Command = "connect_check",
                        Success = false,
                        Response = "Not connected",
                        SentAt = DateTime.UtcNow.ToString("o"),
                    });
                    hasError = true;
                    break; // Sequential error — stop (csv_extractor.py line 281)
                }

                // 1. Change program
                if (config.ProgramNumber.HasValue)
                {
                    var r = await adapter.ChangeProgramAsync(config.ProgramNumber.Value);
                    r.Ordinal = config.Ordinal;
                    commandResults.Add(r);
                    Log($"IJ{config.Ordinal} ChangeProgram({config.ProgramNumber}): {(r.Success ? "OK" : "FAIL")}");

                    if (!r.Success) { hasError = true; break; }
                }

                // 2. Send config
                var cfgResult = await adapter.SendConfigAsync(config);
                cfgResult.Ordinal = config.Ordinal;
                commandResults.Add(cfgResult);
                Log($"IJ{config.Ordinal} SendConfig: {(cfgResult.Success ? "OK" : "FAIL")}");

                if (!cfgResult.Success) { hasError = true; break; }

                // 3. Send text blocks (device blocks 6-10)
                var blocks = config.TextBlocks ?? new List<TextBlockDto>();
                foreach (var block in blocks.OrderBy(b => b.BlockNumber))
                {
                    int deviceBlock = block.BlockNumber + 5; // blocks 6-10
                    var tbResult = await adapter.SendTextBlockAsync(block, deviceBlock);
                    tbResult.Ordinal = config.Ordinal;
                    commandResults.Add(tbResult);
                    Log($"IJ{config.Ordinal} TextBlock({block.BlockNumber}): {(tbResult.Success ? "OK" : "FAIL")}");

                    if (!tbResult.Success) { hasError = true; break; }
                    await Task.Delay(30); // 30ms delay between blocks (csv_extractor.py line 274)
                }

                if (hasError) break;

                // 4. Resume printing
                var resumeResult = await adapter.ResumeAsync();
                resumeResult.Ordinal = config.Ordinal;
                commandResults.Add(resumeResult);
                Log($"IJ{config.Ordinal} Resume: {(resumeResult.Success ? "OK" : "FAIL")}");

                if (!resumeResult.Success) { hasError = true; break; }
            }

            // Send conveyor speed + servo to PLC
            if (!hasError && resolved.Pattern.ConveyorSpeeds != null)
            {
                var spd = resolved.Pattern.ConveyorSpeeds;
                await _plc.WriteSpeedAsync(spd.Speed1 ?? 0, spd.Speed2 ?? 0, spd.Speed3 ?? 0);
                Log($"PLC speed: {spd.Speed1}/{spd.Speed2}/{spd.Speed3}");
            }

            if (!hasError && resolved.Pattern.ServoConfigs != null)
            {
                foreach (var servo in resolved.Pattern.ServoConfigs)
                {
                    await _plc.WriteServoAsync(servo.Ordinal, servo.Position ?? 0, servo.PostAct ?? 0, servo.Delay ?? 0, servo.Trigger ?? 0);
                    Log($"PLC servo ordinal={servo.Ordinal}: pos={servo.Position}");
                }
            }

            // Post results back to backend
            var payload = new JobResultsPayload
            {
                Success = !hasError,
                ErrorMessage = hasError ? "One or more commands failed" : null,
                Commands = commandResults,
            };

            await _api.PostResultsAsync(_selectedJobId, payload);
            Log(hasError ? "Job completed with errors." : "Job completed successfully.");
        }
        catch (Exception ex)
        {
            Log("Send error: " + ex.Message);
        }
        finally
        {
            btnSend.Enabled = true;
        }
    }

    private async void btnRetry_Click(object sender, EventArgs e)
    {
        if (_selectedJobId < 0) return;

        bool ok = await _api.RetryJobAsync(_selectedJobId);
        Log(ok ? $"Job {_selectedJobId} retried." : $"Retry failed for job {_selectedJobId}.");
    }

    // ════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════

    private IInkjetAdapter GetAdapterByOrdinal(int ordinal)
    {
        return ordinal switch
        {
            1 => _adapterIj1,
            2 => _adapterIj2,
            3 => _adapterIj3,
            4 => _adapterIj4,
            _ => _adapterIj1,
        };
    }

    private void UpdateDeviceStatus()
    {
        if (InvokeRequired) { Invoke(UpdateDeviceStatus); return; }

        lblStatusIj1.BackColor = _adapterIj1.IsConnected() ? Color.Green : Color.Gray;
        lblStatusIj2.BackColor = _adapterIj2.IsConnected() ? Color.Green : Color.Gray;
        lblStatusIj3.BackColor = _adapterIj3.IsConnected() ? Color.Green : Color.Gray;
        lblStatusIj4.BackColor = _adapterIj4.IsConnected() ? Color.Green : Color.Gray;
    }

    private void UpdateJobGrid()
    {
        if (InvokeRequired) { Invoke(UpdateJobGrid); return; }

        _isRefreshingJobs = true; // 🔥 กัน event

        int selectedId = -1;
        if (dgvJobs.CurrentRow != null)
        {
            selectedId = (int)dgvJobs.CurrentRow.Cells["Id"].Value;
        }

        _jobBindingList.RaiseListChangedEvents = false;
        _jobBindingList.Clear();

        foreach (var j in _pendingJobs)
        {
            _jobBindingList.Add(new JobRow
            {
                Id = j.Id,
                BarcodeRaw = j.BarcodeRaw,
                LotNumber = j.LotNumber,
                Status = j.Status,
                Attempt = j.Attempt
            });
        }

        _jobBindingList.RaiseListChangedEvents = true;
        _jobBindingList.ResetBindings();

        // 🔥 restore selection
        if (selectedId != -1)
        {
            foreach (DataGridViewRow row in dgvJobs.Rows)
            {
                if ((int)row.Cells["Id"].Value == selectedId)
                {
                    row.Selected = true;
                    dgvJobs.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        _isRefreshingJobs = false; // 🔥 เปิด event กลับ
    }

    private void UpdateDetailPanel()
    {
        if (InvokeRequired) { Invoke(UpdateDetailPanel); return; }

        if (_currentResolved == null) return;

        var job = _currentResolved.Job;
        var pattern = _currentResolved.Pattern;

        txtBarcode.Text = job?.BarcodeRaw ?? "";
        txtLot.Text = job?.LotNumber ?? "";
        txtStatus.Text = job?.Status ?? "";
        txtPattern.Text = pattern?.Name ?? "";

        _configBindingList.RaiseListChangedEvents = false;

        var newList = pattern?.InkjetConfigs ?? new List<InkjetConfigDto>();

        // 🔥 sync count
        while (_configBindingList.Count > newList.Count)
        {
            _configBindingList.RemoveAt(_configBindingList.Count - 1);
        }

        for (int i = 0; i < newList.Count; i++)
        {
            if (i < _configBindingList.Count)
            {
                // 🔥 update object reference (สำคัญ)
                _configBindingList[i] = newList[i];
            }
            else
            {
                _configBindingList.Add(newList[i]);
            }
        }

        _configBindingList.RaiseListChangedEvents = true;
        _configBindingList.ResetBindings();

        // --- เพิ่มส่วนนี้: Fix ให้ dgvUVBlocks แสดงเฉพาะ UV1 (Ordinal 3) และ UV2 (Ordinal 4) ---
        if (pattern?.InkjetConfigs != null)
        {
            // ดึงเฉพาะ Config ของ UV (Ordinal 3 และ 4)
            var uvConfigs = pattern.InkjetConfigs
                .Where(c => c.Ordinal == 3 || c.Ordinal == 4)
                .OrderBy(c => c.Ordinal)
                .ToList();

            // สร้างรายการสำหรับ Display ใน Grid
            var uvDisplayList = new List<object>();
            foreach (var cfg in uvConfigs)
            {
                string printerName = cfg.Ordinal == 3 ? "เครื่องพิมพ์ UV1" : "เครื่องพิมพ์ UV2";

                // ดึงค่า Block 1 และ 2 (Lot และ Name)
                string lotVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 1)?.Text ?? "";
                string nameVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 2)?.Text ?? "";

                uvDisplayList.Add(new
                {
                    Printer = printerName,
                    LotText = lotVal,
                    NameText = nameVal,
                    // เก็บ Reference ไว้เผื่อต้องการดึงไปใช้งานต่อ
                    _originalConfig = cfg
                });
            }
        }
    }

    private void Log(string message)
    {
        if (InvokeRequired) { Invoke(() => Log(message)); return; }

        string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        txtLog.AppendText(line + Environment.NewLine);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        var steps = new[]
        {
        new BotStep { Name = "Document",   X = 2325, Y = 59,  VerifyArea = new Rectangle(2100, 0, 400, 300) },
        new BotStep { Name = "Open",       X = 2207, Y = 133, VerifyArea = new Rectangle(2100, 100, 400, 400) },
        new BotStep { Name = "SelectFile", X = 1506, Y = 654, VerifyArea = new Rectangle(800, 400, 800, 600) },
        new BotStep { Name = "OpenBtn",    X = 1652, Y = 946, VerifyArea = new Rectangle(1500, 800, 500, 300) },
    };

        BotClickHelper.RunAsync("uvinkjet", steps, result =>
        {
            if (this.IsHandleCreated)
                this.Invoke((MethodInvoker)(() =>
                    MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error)));
        });
    }

    private void button2_Click(object sender, EventArgs e)
    {
        // ใช้ Invoke เพื่อความปลอดภัยว่าทำงานบน UI Thread (STA)
        this.Invoke(new Action(() =>
        {
            using (frmPatternEditor editor = new frmPatternEditor())
            {
                editor.StartPosition = FormStartPosition.CenterParent;
                editor.ShowDialog();
            }
        }));
    }

    private async void button3_Click(object sender, EventArgs e)
    {
        //await SendTextBlocksToIjAsync(3);
        var form = new frmCreateJob(_api);
        form.ShowDialog();
    }

    private async void button4_Click(object sender, EventArgs e)
    {
        await TestSendToIj3TcpAsync();
    }

    // Example usage: send blocks from dgvTextBlocks to IJ3 via TCP (simple demo)
    private async Task TestSendToIj3TcpAsync()
    {
        // read TCP host/port from UI controls
        string host = txtTcpHost.Text.Trim();
        if (!int.TryParse(txtTcpPort.Text.Trim(), out int port))
        {
            Log("Invalid TCP port");
            return;
        }

        using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
        try
        {
            await client.ConnectAsync(host, port);
        }
        catch (Exception ex)
        {
            Log("TCP connect failed: " + ex.Message);
            return;
        }

        // Example: change program to 13
        var progResp = await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, 13);
        Log($"ChangeProgram resp: {progResp}");

        // Send each row in dgvTextBlocks
        foreach (DataGridViewRow row in dgvTextBlocks.Rows)
        {
            if (row.IsNewRow) continue;

            int blockNumber = 1;
            if (dgvTextBlocks.Columns.Contains("BlockNumber") && int.TryParse(row.Cells["BlockNumber"].Value?.ToString(), out var bn))
                blockNumber = bn;

            string text = row.Cells["Text"].Value?.ToString() ?? row.Cells["text"].Value?.ToString() ?? "";
            int x = int.TryParse(row.Cells["X"].Value?.ToString(), out var tx) ? tx : 0;
            int y = int.TryParse(row.Cells["Y"].Value?.ToString(), out var ty) ? ty : 0;
            int size = int.TryParse(row.Cells["Size"].Value?.ToString(), out var ts) ? ts : 1;
            int scale = int.TryParse(row.Cells["Scale"].Value?.ToString(), out var tsc) ? tsc : 1;

            var block = new InkjetOperator.Services.SimpleTextBlock(blockNumber, text, x, y, size, scale);
            var (fsResp, f1Resp) = await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, 13, block);
            Log($"IJ3 TextBlock({blockNumber}) FS='{fsResp}' F1='{f1Resp}'");

            // If adapter or device returns empty error, you can decide to continue or break.
            await Task.Delay(30);
        }

        var resume = await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
        Log($"Resume resp: {resume}");
    }

    private async void dgvUVBlocks_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvUVBlocks.Rows[e.RowIndex];

        if (row.DataBoundItem is not UvRow data) return;

        try
        {
            await UpdateUvRow(data.Id, data.Lot, data.Name);
            Log($"[UV Updated] ID={data.Id}, Lot={data.Lot}, Name={data.Name}");
        }
        catch (Exception ex)
        {
            Log("Update UV error: " + ex.Message);
        }
    }

    private async Task UpdateUvRow(int id, string lot, string name)
    {
        using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        UPDATE uv_print_data
        SET lot = @lot,
            name = @name,
            update_at = CURRENT_TIMESTAMP
        WHERE id = @id
    ";

        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@lot", lot);
        cmd.Parameters.AddWithValue("@name", name);

        await cmd.ExecuteNonQueryAsync();
    }

    public void InitializeDatabase()
    {
        string folderPath = @"D:\DB";
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
        conn.Open();

        string createTableSql = @"
    CREATE TABLE IF NOT EXISTS uv_print_data (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        inkjet_name TEXT,
        lot TEXT,
        name TEXT,
        update_at DATETIME DEFAULT CURRENT_TIMESTAMP
    );";

        using (var command = new SqliteCommand(createTableSql, conn))
        {
            command.ExecuteNonQuery();
        }

        // เช็คว่ามีข้อมูลหรือยัง ถ้าไม่มีให้ Insert แถวสำหรับ UV1 และ UV2 รอไว้เลย
        var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM uv_print_data", conn);
        long count = (long)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            var insertCmd = new SqliteCommand(@"
            INSERT INTO uv_print_data (id, inkjet_name, lot, name) VALUES 
            (1, 'เครื่องพิมพ์ UV1', '', ''),
            (2, 'เครื่องพิมพ์ UV2', '', '')", conn);
            insertCmd.ExecuteNonQuery();
        }
    }

    private async Task LoadUvDataToGrid()
    {
        try
        {
            using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3");
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT id, inkjet_name, lot, name, update_at
        FROM uv_print_data
        ORDER BY id
        ";

            var reader = await cmd.ExecuteReaderAsync();

            var list = new List<UvRow>();

            while (await reader.ReadAsync())
            {
                list.Add(new UvRow
                {
                    Id = reader.GetInt32(0), // 🔥 ยังต้องมี
                    Inkjet = reader.GetString(1),
                    Lot = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    UpdateAt = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }

            dgvUVBlocks.Invoke(() =>
            {

                dgvUVBlocks.AutoGenerateColumns = false;

                dgvUVBlocks.ReadOnly = false;
                dgvUVBlocks.AllowUserToAddRows = false;
                dgvUVBlocks.EditMode = DataGridViewEditMode.EditOnEnter;
                dgvUVBlocks.SelectionMode = DataGridViewSelectionMode.CellSelect;


                _uvBindingList.RaiseListChangedEvents = false;
                _uvBindingList.Clear();

                foreach (var item in list)
                {
                    _uvBindingList.Add(item);
                }

                _uvBindingList.RaiseListChangedEvents = true;
                _uvBindingList.ResetBindings();

            });
        }
        catch (Exception ex)
        {
            Log("Load UV error: " + ex.Message);
        }
    }

    private void button6_Click(object sender, EventArgs e)
    {
        var row = _uvBindingList.FirstOrDefault(x => x.Id == 1);

        if (row == null)
        {
            MessageBox.Show("ไม่พบ row id = 1");
            return;
        }

        string lot = row.Lot;
        string name = row.Name;

        Log($"Lot={lot}, Name={name}");

        string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";

        using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
        {
            conn.Open();

            string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";

            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@lot", lot);
                cmd.Parameters.AddWithValue("@name", name);

                int rows = cmd.ExecuteNonQuery();

                MessageBox.Show("Update สำเร็จ: " + rows + " row");
            }
        }

        currentStep = 3; // ไปขั้นที่ 2
        UpdateStepButtons();
    }

    private void button8_Click(object sender, EventArgs e)
    {
        var row = _uvBindingList.FirstOrDefault(x => x.Id == 2);

        if (row == null)
        {
            MessageBox.Show("ไม่พบ row id = 1");
            return;
        }

        string lot = row.Lot;
        string name = row.Name;

        Log($"Lot={lot}, Name={name}");

        string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";

        using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
        {
            conn.Open();

            string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";

            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@lot", lot);
                cmd.Parameters.AddWithValue("@name", name);

                int rows = cmd.ExecuteNonQuery();

                MessageBox.Show("Update สำเร็จ: " + rows + " row");
            }
        }
        currentStep = 1;
        UpdateStepButtons();
    }


    private async void button5_Click(object sender, EventArgs e)
    {
        //if (dgvConfigs.Rows.Count > 0)
        //{
        //    dgvConfigs.ClearSelection();

        //    dgvConfigs.Rows[0].Selected = true;
        //    dgvConfigs.CurrentCell = dgvConfigs.Rows[0].Cells[0];

        //    var config = dgvConfigs.Rows[0].DataBoundItem as InkjetConfigDto;

        //    if (config != null)
        //    {
        //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
        //    }
        //}

        //await TestSendToIj3TcpAsync();

        if (dgvConfigs.Rows.Count > 1)
        {
            dgvConfigs.ClearSelection();

            dgvConfigs.Rows[1].Selected = true;
            dgvConfigs.CurrentCell = dgvConfigs.Rows[1].Cells[0];

            var config = dgvConfigs.Rows[1].DataBoundItem as InkjetConfigDto;

            if (config != null)
            {
                MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
            }
        }

        await TestSendToIj3TcpAsync();

        currentStep = 2; // ไปขั้นที่ 2
        UpdateStepButtons();

    }

    private async void button7_Click(object sender, EventArgs e)
    {
        await SendToIj3FromDbAsync("CCRC0291-DEX0663MS");
        //if (dgvConfigs.Rows.Count > 2)
        //{
        //    dgvConfigs.ClearSelection();

        //    dgvConfigs.Rows[2].Selected = true;
        //    dgvConfigs.CurrentCell = dgvConfigs.Rows[2].Cells[0];

        //    var config = dgvConfigs.Rows[2].DataBoundItem as InkjetConfigDto;

        //    if (config != null)
        //    {
        //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
        //    }

        //    await TestSendToIj3TcpAsync();

        currentStep = 4; // ไปขั้นที่ 2
        UpdateStepButtons();
        //}
    }

    private void UpdateStepButtons()
    {
        // ปุ่ม 1 (MK1+2)
        button5.Enabled = (currentStep == 1);

        // ปุ่ม 2 (UV1)
        button6.Enabled = (currentStep == 2);

        // ปุ่ม 3 (MK2/3)
        button7.Enabled = (currentStep == 3);

        // ปุ่ม 4 (UV2)
        button8.Enabled = (currentStep == 4);

        // ถ้าอยากให้เห็นชัดเจนว่าอันไหนเสร็จแล้ว อาจจะเปลี่ยนสีปุ่มด้วยก็ได้
        //button5.BackColor = (currentStep > 1) ? Color.LightGreen : SystemColors.Control;
        // ... ทำแบบเดียวกันกับปุ่มอื่นๆ
    }

    private async Task SendToIj3FromDbAsync(string barcode)
    {
        // ตัวแปรสำหรับ Block 1
        string text1 = ""; int x1 = 0, y1 = 0, size1 = 1, scale1 = 1;
        // ตัวแปรสำหรับ Block 2
        string text2 = ""; int x2 = 0, y2 = 0, size2 = 1, scale2 = 1;

        string dbPath = @"D:\DB\uv_data.db3";
        using (var conn = new SqliteConnection($"Data Source={dbPath}"))
        {
            await conn.OpenAsync();
            string sql = "SELECT * FROM config_data_mk3 WHERE pattern_no_erp = @barcode LIMIT 1";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@barcode", barcode);
            using var reader = await cmd.ExecuteReaderAsync();

            if (reader.Read())
            {
                // ดึงข้อมูล Block 1
                text1 = reader.GetString(reader.GetOrdinal("block1_text"));
                x1 = reader.GetInt32(reader.GetOrdinal("block1_x"));
                y1 = reader.GetInt32(reader.GetOrdinal("block1_y"));
                size1 = reader.GetInt32(reader.GetOrdinal("block1_size"));
                scale1 = reader.GetInt32(reader.GetOrdinal("block1_scale_side"));

                // ดึงข้อมูล Block 2
                text2 = reader.IsDBNull(reader.GetOrdinal("block2_text")) ? "" : reader.GetString(reader.GetOrdinal("block2_text"));
                x2 = reader.GetInt32(reader.GetOrdinal("block2_x"));
                y2 = reader.GetInt32(reader.GetOrdinal("block2_y"));
                size2 = reader.GetInt32(reader.GetOrdinal("block2_size"));
                scale2 = reader.GetInt32(reader.GetOrdinal("block2_scale_side"));
            }
        }

        // --- ส่วนการส่ง TCP ---
        using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
        try
        {
            await client.ConnectAsync(txtTcpHost.Text.Trim(), int.Parse(txtTcpPort.Text.Trim()));
            int fixedProgNo = 13;

            // 1. เปลี่ยนโปรแกรมเป็น 13
            await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, fixedProgNo);

            // 2. ส่ง Block 1
            var block1 = new InkjetOperator.Services.SimpleTextBlock(1, text1, x1, y1, size1, scale1);
            await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block1);
            await Task.Delay(50); // พักช่วงสั้นๆ ระหว่างการส่งแต่ละ Block

            // 3. ส่ง Block 2
            if (!string.IsNullOrEmpty(text2))
            {
                var block2 = new InkjetOperator.Services.SimpleTextBlock(2, text2, x2, y2, size2, scale2);
                await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block2);
                await Task.Delay(50);
            }

            // 4. สั่ง Resume
            await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
            Log("Step 3: MK3 (2 Blocks) Sent Successfully.");
        }
        catch (Exception ex)
        {
            Log("Error: " + ex.Message);
        }
    }

    private void button9_Click(object sender, EventArgs e)
    {
        string lot = "C200521-001" ?? string.Empty;
        string blockText = "DDDD-01";
        string previewText = PatternEngine.Process(lot, blockText);
        //lblPreview.Text = "Preview: " + previewText;
        Log("Preview: " + previewText);
    }
}
