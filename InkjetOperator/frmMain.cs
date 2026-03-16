using System.IO.Ports;
using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.PlcAdapter;
using InkjetOperator.Services;

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

    public frmMain()
    {
        InitializeComponent();

        _adapterIj1 = new MkCompactAdapter(_rs232Ij1);
        _adapterIj2 = new MkCompactAdapter(_rs232Ij2);
        _adapterIj3 = new SqliteInkjetAdapter(_tcpIj3, ""); // db path TBD
        _adapterIj4 = new SqliteInkjetAdapter(_tcpIj4, ""); // db path TBD

        _api = new ApiClient("http://localhost:3000");
    }

    // ════════════════════════════════════════
    //  Form events
    // ════════════════════════════════════════

    private void frmMain_Load(object sender, EventArgs e)
    {
        // Populate COM port dropdowns
        string[] ports = SerialPort.GetPortNames();
        cmbCom1.Items.AddRange(ports);
        cmbCom2.Items.AddRange(ports);
        if (cmbCom1.Items.Count > 0) cmbCom1.SelectedIndex = 0;
        if (cmbCom2.Items.Count > 1) cmbCom2.SelectedIndex = 1;
        else if (cmbCom2.Items.Count > 0) cmbCom2.SelectedIndex = 0;

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
        if (dgvJobs.SelectedRows.Count == 0) return;

        var row = dgvJobs.SelectedRows[0];
        if (row.Cells["Id"].Value == null) return;

        int jobId = (int)row.Cells["Id"].Value;
        if (jobId == _selectedJobId) return;
        _selectedJobId = jobId;

        try
        {
            var resolved = await _api.GetResolvedJobAsync(jobId);
            _currentResolved = resolved;
            UpdateDetailPanel();
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
        dgvTextBlocks.DataSource = config.TextBlocks;
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
            if (!hasError && resolved.Pattern.ConveyorSpeed != null)
            {
                var spd = resolved.Pattern.ConveyorSpeed;
                await _plc.WriteSpeedAsync(spd.Speed1, spd.Speed2, spd.Speed3);
                Log($"PLC speed: {spd.Speed1}/{spd.Speed2}/{spd.Speed3}");
            }

            if (!hasError && resolved.Pattern.ServoConfigs != null)
            {
                foreach (var servo in resolved.Pattern.ServoConfigs)
                {
                    await _plc.WriteServoAsync(servo.Ordinal, servo.Position, servo.PostAct, servo.Delay, servo.Trigger);
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

        dgvJobs.DataSource = null;
        dgvJobs.DataSource = _pendingJobs.Select(j => new
        {
            j.Id,
            j.BarcodeRaw,
            j.LotNumber,
            j.Status,
            j.Attempt,
        }).ToList();
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
        txtPattern.Text = pattern?.Barcode ?? "";

        dgvConfigs.DataSource = pattern?.InkjetConfigs?.Select(c => new
        {
            c.Ordinal,
            c.ProgramNumber,
            c.ProgramName,
            c.Width,
            c.Height,
            c.TriggerDelay,
            Dir = c.Direction,
            c.Suspended,
            Blocks = c.TextBlocks?.Count ?? 0,
        }).ToList();

        dgvTextBlocks.DataSource = null;
    }

    private void Log(string message)
    {
        if (InvokeRequired) { Invoke(() => Log(message)); return; }

        string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        txtLog.AppendText(line + Environment.NewLine);
    }
}
