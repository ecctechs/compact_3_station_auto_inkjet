This file is a merged representation of a subset of the codebase, containing specifically included files and files not matching ignore patterns, combined into a single document by Repomix.
The content has been processed where line numbers have been added.

# File Summary

## Purpose
This file contains a packed representation of a subset of the repository's contents that is considered the most important context.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.

## File Format
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  a. A header with the file path (## File: path/to/file)
  b. The full contents of the file in a code block

## Usage Guidelines
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.

## Notes
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Only files matching these patterns are included: **/*.cs, **/*.csproj, **/*.sln
- Files matching these patterns are excluded: bin/**, obj/**, Properties/**, *.resx, *.Designer.cs, *.user, *.db3, repomix-output.md, repomix-output.xml
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Line numbers have been added to the beginning of each line
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
```
Adapters/IInkjetAdapter.cs
Adapters/InkjetProtocol.cs
Adapters/MkCompactAdapter.cs
Adapters/SimpleDeviceClient.cs
Adapters/SqliteInkjetAdapter.cs
Controls/PatternEditorControl.cs
FormEditPattern.cs
frmCreateJob.cs
frmMain.cs
frmPatternEditor.cs
InkjetOperator.csproj
Managers/Rs232Manager.cs
Managers/TcpManager.cs
Models/ApiModels.cs
Models/JobRow.cs
Models/Pattern.cs
Models/Rule.cs
Models/TransformRuleType.cs
Models/UvRow.cs
PlcAdapter/PlcManager.cs
Program.cs
Services/ApiClient.cs
Services/BotClickHelper.cs
Services/PatternEngine.cs
Services/PatternStore.cs
```

# Files

## File: Adapters/IInkjetAdapter.cs
```csharp
 1: using InkjetOperator.Models;
 2: 
 3: namespace InkjetOperator.Adapters;
 4: 
 5: public interface IInkjetAdapter
 6: {
 7:     Task<bool> ConnectAsync();
 8:     Task DisconnectAsync();
 9:     bool IsConnected();
10:     Task<CommandResult> SuspendAsync();
11:     Task<CommandResult> ResumeAsync();
12:     Task<CommandResult> ChangeProgramAsync(int programNumber);
13:     Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock);
14:     Task<CommandResult> SendConfigAsync(InkjetConfigDto config);
15: }
```

## File: Adapters/InkjetProtocol.cs
```csharp
 1: using System.Text;
 2: using InkjetOperator.Adapters.Simple;
 3: 
 4: namespace InkjetOperator.Services
 5: {
 6:     // Simple DTO for a text block (matches existing TextBlockDto shape)
 7:     public record SimpleTextBlock(int BlockNumber, string Text, int X = 0, int Y = 0, int Size = 1, int Scale = 1);
 8: 
 9:     public static class InkjetProtocol
10:     {
11:         // Change program: FW,<prog>
12:         public static async Task<string> SendChangeProgramAsync(IDeviceClient client, int programNumber, CancellationToken ct = default)
13:         {
14:             return await client.SendCommandAsync($"FW,{programNumber}", 2000, ct);
15:         }
16: 
17:         // FS: set text content (FS,{prog},{deviceBlock},0,{text})
18:         public static async Task<string> SendFsAsync(IDeviceClient client, int programNumber, int deviceBlock, string text, CancellationToken ct = default)
19:         {
20:             // ensure ASCII-safe text or sanitize as required
21:             string safe = text ?? "";
22:             return await client.SendCommandAsync($"FS,{programNumber},{deviceBlock},0,{safe}", 2000, ct);
23:         }
24: 
25:         // F1: format: F1,{prog},{block},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0
26:         // here sizeConverted is kept equal to size for simplicity; adapt conversion if needed.
27:         public static async Task<string> SendF1Async(IDeviceClient client, int programNumber, int deviceBlock, int scale, int size, int x = 0, int y = 0, CancellationToken ct = default)
28:         {
29:             int sizeConverted = size; // map if required
30:             return await client.SendCommandAsync($"F1,{programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0", 2000, ct);
31:         }
32: 
33:         // Send a single text block (FS then F1)
34:         public static async Task<(string fsResp, string f1Resp)> SendTextBlockAsync(IDeviceClient client, int programNumber, SimpleTextBlock block, CancellationToken ct = default)
35:         {
36:             int deviceBlock = block.BlockNumber + 5; // mapping 1->6
37:             string fs = await SendFsAsync(client, programNumber, deviceBlock, block.Text, ct);
38:             if (string.IsNullOrEmpty(fs)) { /* no response but continue */ }
39: 
40:             string f1 = await SendF1Async(client, programNumber, deviceBlock, block.Scale, block.Size, block.X, block.Y, ct);
41:             return (fs, f1);
42:         }
43: 
44:         // Send config (FM) — simplified form, adapt parameters as needed
45:         public static async Task<string> SendConfigAsync(IDeviceClient client, int programNumber, string programName, int triggerDelay = 0, int height = 100, int width = 200, int direction = 0, CancellationToken ct = default)
46:         {
47:             // Program name normalization is not necessary here; ensure no commas that break protocol.
48:             string safeName = programName?.Replace(",", " ") ?? "";
49:             string fm = $"FM,{programNumber},0,{safeName},0,{direction},0,0,01,20,500,{triggerDelay},300,1000,{height},{width},15,0,0,6";
50:             return await client.SendCommandAsync(fm, 2000, ct);
51:         }
52: 
53:         public static Task<string> SendSuspendAsync(IDeviceClient client, CancellationToken ct = default)
54:             => client.SendCommandAsync("SR", 1000, ct);
55: 
56:         public static Task<string> SendResumeAsync(IDeviceClient client, CancellationToken ct = default)
57:             => client.SendCommandAsync("SQ", 1000, ct);
58:     }
59: }
```

## File: Adapters/MkCompactAdapter.cs
```csharp
  1: using System.Globalization;
  2: using System.Text;
  3: using InkjetOperator.Managers;
  4: using InkjetOperator.Models;
  5: 
  6: namespace InkjetOperator.Adapters;
  7: 
  8: /// <summary>
  9: /// MK Compact inkjet adapter — formats FW/FS/F1/FM/SR/SQ commands.
 10: /// Ported from rs232_connector.py + socket_client.py.
 11: /// Can use either Rs232Manager (COM port) or TcpManager (TCP/IP).
 12: /// </summary>
 13: public class MkCompactAdapter : IInkjetAdapter
 14: {
 15:     private readonly Rs232Manager? _rs232;
 16:     private readonly TcpManager? _tcp;
 17:     private int _programNumber;
 18: 
 19:     /// <summary>
 20:     /// Size conversion dict — from rs232_connector.py lines 8-22.
 21:     /// Maps logical size to device encoding.
 22:     /// </summary>
 23:     private static readonly Dictionary<string, string> SizeConversion = new()
 24:     {
 25:         { "1", "0" },
 26:         { "2", "1" },
 27:         { "3", "17" },
 28:         { "4", "3" },
 29:         { "5", "18" },
 30:         { "6", "20" },
 31:         { "7", "5" },
 32:         { "8", "6" },
 33:         { "9", "7" },
 34:         { "10", "8" },
 35:         { "11", "9" },
 36:         { "12", "10" },
 37:         { "13", "11" },
 38:     };
 39: 
 40:     /// <summary>Constructor for RS232 connection (COM port).</summary>
 41:     public MkCompactAdapter(Rs232Manager rs232)
 42:     {
 43:         _rs232 = rs232;
 44:     }
 45: 
 46:     /// <summary>Constructor for TCP connection.</summary>
 47:     public MkCompactAdapter(TcpManager tcp)
 48:     {
 49:         _tcp = tcp;
 50:     }
 51: 
 52:     public Task<bool> ConnectAsync()
 53:     {
 54:         // Connection is managed by the underlying manager
 55:         return Task.FromResult(IsConnected());
 56:     }
 57: 
 58:     public Task DisconnectAsync()
 59:     {
 60:         _rs232?.CloseSerialPort();
 61:         _tcp?.Disconnect();
 62:         return Task.CompletedTask;
 63:     }
 64: 
 65:     public bool IsConnected()
 66:     {
 67:         if (_rs232 != null) return _rs232.IsOpen();
 68:         if (_tcp != null) return _tcp.IsConnected();
 69:         return false;
 70:     }
 71: 
 72:     private async Task<string> SendAsync(string command)
 73:     {
 74:         if (_rs232 != null)
 75:             return await _rs232.SendCommandAsync(command);
 76:         if (_tcp != null)
 77:             return await _tcp.SendCommandAsync(command);
 78:         return "";
 79:     }
 80: 
 81:     private CommandResult MakeResult(string command, string response, int? ordinal = null)
 82:     {
 83:         return new CommandResult
 84:         {
 85:             Command = command,
 86:             Response = response,
 87:             Success = response != "", // empty = connection problem (rs232_connector.py line 33)
 88:             SentAt = DateTime.UtcNow.ToString("o"),
 89:             Ordinal = ordinal,
 90:         };
 91:     }
 92: 
 93:     /// <summary>
 94:     /// Send SR (suspend/stop printing).
 95:     /// From rs232_connector.py send_suspend() lines 46-51: sends "SR\r"
 96:     /// </summary>
 97:     public async Task<CommandResult> SuspendAsync()
 98:     {
 99:         string response = await SendAsync("SR\r");
100:         return MakeResult("suspend", response);
101:     }
102: 
103:     /// <summary>
104:     /// Send SQ (resume printing).
105:     /// From rs232_connector.py send_resume() lines 54-59: sends "SQ\r"
106:     /// </summary>
107:     public async Task<CommandResult> ResumeAsync()
108:     {
109:         string response = await SendAsync("SQ\r");
110:         return MakeResult("resume", response);
111:     }
112: 
113:     /// <summary>
114:     /// Send FW (change program/message number).
115:     /// From rs232_connector.py send_change_prog() lines 36-43: sends "FW,{n}\r"
116:     /// </summary>
117:     public async Task<CommandResult> ChangeProgramAsync(int programNumber)
118:     {
119:         _programNumber = programNumber;
120:         string response = await SendAsync($"FW,{programNumber}\r");
121:         return MakeResult("change_prog", response);
122:     }
123: 
124:     /// <summary>
125:     /// Send FS + F1 (text block content + format).
126:     /// From rs232_connector.py send_text() lines 62-71:
127:     ///   FS,{prog},{block},0,{text}\r
128:     ///   F1,{prog},{block},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r
129:     /// </summary>
130:     public async Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
131:     {
132:         string text = block.Text ?? "";
133:         string x = (block.X ?? 0).ToString();
134:         string y = (block.Y ?? 0).ToString();
135:         string scale = (block.Scale ?? 1).ToString();
136:         string sizeKey = (block.Size ?? 1).ToString();
137: 
138:         string sizeConverted = SizeConversion.GetValueOrDefault(sizeKey, "0");
139: 
140:         // FS command — set text content
141:         string fsCmd = $"FS,{_programNumber},{deviceBlock},0,{text}\r";
142:         string fsResponse = await SendAsync(fsCmd);
143: 
144:         if (fsResponse == "")
145:         {
146:             return MakeResult("text_block", "", null);
147:         }
148: 
149:         // F1 command — set text format
150:         string f1Cmd = $"F1,{_programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r";
151:         string f1Response = await SendAsync(f1Cmd);
152: 
153:         return MakeResult("text_block", f1Response);
154:     }
155: 
156:     /// <summary>
157:     /// Send FM (program/message configuration).
158:     /// From rs232_connector.py send_config() lines 74-83:
159:     ///   FM,{prog},0,{name},0,{dir},0,0,01,20,500,{delay},300,1000,{height},{width},15,0,0,6\r
160:     /// Program name is Unicode-normalized (NFKD) like Python unicodedata.normalize.
161:     /// </summary>
162:     public async Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
163:     {
164:         string progName = config.ProgramName ?? "";
165:         // Normalize like Python: unicodedata.normalize('NFKD', ch)
166:         string normalizedName = progName.Normalize(NormalizationForm.FormKD);
167: 
168:         string direction = (config.Direction ?? 0).ToString();
169:         string delay = (config.TriggerDelay ?? 0).ToString();
170:         string height = (config.Height ?? 100).ToString();
171:         string width = (config.Width ?? 200).ToString();
172: 
173:         string fmCmd = $"FM,{_programNumber},0,{normalizedName},0,{direction},0,0,01,20,500,{delay},300,1000,{height},{width},15,0,0,6\r";
174:         string response = await SendAsync(fmCmd);
175: 
176:         return MakeResult("config", response);
177:     }
178: }
```

## File: Adapters/SimpleDeviceClient.cs
```csharp
  1: using System.Diagnostics;
  2: using System.IO.Ports;
  3: using System.Net.Sockets;
  4: using System.Text;
  5: 
  6: namespace InkjetOperator.Adapters.Simple
  7: {
  8:     public interface IDeviceClient : IDisposable
  9:     {
 10:         Task ConnectAsync(string hostOrPort, int? port = null, CancellationToken ct = default);
 11:         Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default);
 12:         bool IsConnected { get; }
 13:     }
 14: 
 15:     // TCP socket implementation (port required)
 16:     public class SocketDeviceClient : IDeviceClient
 17:     {
 18:         private TcpClient? _tcp;
 19:         private NetworkStream? _stream;
 20:         private readonly Encoding _enc = Encoding.ASCII;
 21: 
 22:         public bool IsConnected => _tcp?.Connected ?? false;
 23: 
 24:         public async Task ConnectAsync(string host, int? port = null, CancellationToken ct = default)
 25:         {
 26:             if (!port.HasValue) throw new ArgumentException("port required for SocketDeviceClient");
 27:             _tcp = new TcpClient();
 28:             await _tcp.ConnectAsync(host, port.Value, ct);
 29:             _stream = _tcp.GetStream();
 30:             _stream.ReadTimeout = 4000;
 31:             _stream.WriteTimeout = 4000;
 32:         }
 33: 
 34:         public async Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default)
 35:         {
 36:             if (_stream == null) throw new InvalidOperationException("Not connected");
 37:             byte[] data = _enc.GetBytes(command + "\r");
 38:             await _stream.WriteAsync(data, 0, data.Length, ct);
 39: 
 40:             // read response (non-blocking with timeout)
 41:             var sb = new StringBuilder();
 42:             var buffer = new byte[1024];
 43:             var readCts = new CancellationTokenSource(timeoutMs);
 44:             using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, readCts.Token);
 45:             try
 46:             {
 47:                 // read available bytes until timeout or newline/carriage return
 48:                 while (!linked.IsCancellationRequested)
 49:                 {
 50:                     if (_stream.DataAvailable)
 51:                     {
 52:                         int n = await _stream.ReadAsync(buffer, 0, buffer.Length, linked.Token);
 53:                         if (n == 0) break;
 54:                         sb.Append(_enc.GetString(buffer, 0, n));
 55:                         // break early if CR or LF seen
 56:                         if (sb.ToString().IndexOf('\r') >= 0 || sb.ToString().IndexOf('\n') >= 0) break;
 57:                     }
 58:                     else
 59:                     {
 60:                         await Task.Delay(20, linked.Token);
 61:                     }
 62:                 }
 63:             }
 64:             catch (OperationCanceledException) { /* timeout or cancel */ }
 65: 
 66:             return sb.ToString().Trim();
 67:         }
 68: 
 69:         public void Dispose()
 70:         {
 71:             try { _stream?.Close(); } catch { }
 72:             try { _tcp?.Close(); } catch { }
 73:             _stream = null;
 74:             _tcp = null;
 75:         }
 76:     }
 77: 
 78:     // Serial (RS232) implementation using System.IO.Ports.SerialPort
 79:     public class SerialDeviceClient : IDeviceClient
 80:     {
 81:         private SerialPort? _port;
 82:         private readonly Encoding _enc = Encoding.ASCII;
 83: 
 84:         public bool IsConnected => _port != null && _port.IsOpen;
 85: 
 86:         // hostOrPort for serial is COM port name, port parameter unused
 87:         public Task ConnectAsync(string comName, int? unused = null, CancellationToken ct = default)
 88:         {
 89:             _port = new SerialPort(comName)
 90:             {
 91:                 BaudRate = 9600,
 92:                 DataBits = 8,
 93:                 Parity = Parity.None,
 94:                 StopBits = StopBits.One,
 95:                 ReadTimeout = 3000,
 96:                 WriteTimeout = 3000,
 97:                 Encoding = _enc
 98:             };
 99:             _port.Open();
100:             return Task.CompletedTask;
101:         }
102: 
103:         public Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default)
104:         {
105:             if (_port == null || !_port.IsOpen) throw new InvalidOperationException("Serial port not open");
106:             string cmd = command + "\r";
107:             _port.DiscardInBuffer();
108:             _port.Write(cmd);
109:             var sw = Stopwatch.StartNew();
110:             var sb = new StringBuilder();
111:             while (sw.ElapsedMilliseconds < timeoutMs)
112:             {
113:                 try
114:                 {
115:                     string? line = _port.ReadExisting();
116:                     if (!string.IsNullOrEmpty(line))
117:                     {
118:                         sb.Append(line);
119:                         if (line.IndexOf('\r') >= 0 || line.IndexOf('\n') >= 0) break;
120:                     }
121:                 }
122:                 catch (TimeoutException) { }
123:                 Thread.Sleep(20);
124:             }
125:             return Task.FromResult(sb.ToString().Trim());
126:         }
127: 
128:         public void Dispose()
129:         {
130:             try { if (_port != null && _port.IsOpen) _port.Close(); } catch { }
131:             _port = null;
132:         }
133:     }
134: }
```

## File: Adapters/SqliteInkjetAdapter.cs
```csharp
 1: using InkjetOperator.Managers;
 2: using InkjetOperator.Models;
 3: 
 4: namespace InkjetOperator.Adapters;
 5: 
 6: /// <summary>
 7: /// Stub adapter for inkjet 3 & 4 (different brand, TCP → SQLite .db3).
 8: /// Implements IInkjetAdapter so it can be used interchangeably with MkCompactAdapter.
 9: /// Actual implementation TBD — needs: exact table/column names from the machine's .db3 schema.
10: /// </summary>
11: public class SqliteInkjetAdapter : IInkjetAdapter
12: {
13:     private readonly TcpManager _tcp;
14:     private readonly string _dbPath;
15: 
16:     public SqliteInkjetAdapter(TcpManager tcp, string dbPath)
17:     {
18:         _tcp = tcp;
19:         _dbPath = dbPath;
20:     }
21: 
22:     public Task<bool> ConnectAsync()
23:     {
24:         // TODO: Connect to inkjet machine via TCP, then open .db3 database
25:         return Task.FromResult(false);
26:     }
27: 
28:     public Task DisconnectAsync()
29:     {
30:         _tcp.Disconnect();
31:         return Task.CompletedTask;
32:     }
33: 
34:     public bool IsConnected()
35:     {
36:         return _tcp.IsConnected();
37:     }
38: 
39:     public Task<CommandResult> SuspendAsync()
40:     {
41:         // TODO: SQL UPDATE to pause printing
42:         return Task.FromResult(new CommandResult
43:         {
44:             Command = "suspend",
45:             Success = false,
46:             Response = "SqliteInkjetAdapter not implemented",
47:         });
48:     }
49: 
50:     public Task<CommandResult> ResumeAsync()
51:     {
52:         // TODO: SQL UPDATE to resume printing
53:         return Task.FromResult(new CommandResult
54:         {
55:             Command = "resume",
56:             Success = false,
57:             Response = "SqliteInkjetAdapter not implemented",
58:         });
59:     }
60: 
61:     public Task<CommandResult> ChangeProgramAsync(int programNumber)
62:     {
63:         // TODO: SQL UPDATE to change active message/program
64:         return Task.FromResult(new CommandResult
65:         {
66:             Command = "change_prog",
67:             Success = false,
68:             Response = "SqliteInkjetAdapter not implemented",
69:         });
70:     }
71: 
72:     public Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
73:     {
74:         // TODO: SQL UPDATE on the .db3 text content table
75:         return Task.FromResult(new CommandResult
76:         {
77:             Command = "text_block",
78:             Success = false,
79:             Response = "SqliteInkjetAdapter not implemented",
80:         });
81:     }
82: 
83:     public Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
84:     {
85:         // TODO: SQL UPDATE for print configuration
86:         return Task.FromResult(new CommandResult
87:         {
88:             Command = "config",
89:             Success = false,
90:             Response = "SqliteInkjetAdapter not implemented",
91:         });
92:     }
93: }
```

## File: Controls/PatternEditorControl.cs
```csharp
1: 
```

## File: FormEditPattern.cs
```csharp
1: 
```

## File: frmCreateJob.cs
```csharp
 1: using InkjetOperator.Models;
 2: using InkjetOperator.Services;
 3: 
 4: namespace InkjetOperator
 5: {
 6:     public partial class frmCreateJob : Form
 7:     {
 8:         private ApiClient _api;
 9: 
10:         public frmCreateJob(ApiClient api)
11:         {
12:             InitializeComponent();
13:             _api = api;
14:         }
15: 
16:         private void frmCreateJob_Load(object sender, EventArgs e)
17:         {
18:             cmbType.Items.AddRange(new string[]
19:             {
20:                 "กล่อง",
21:                 "ถุง",
22:                 "ขวด",
23:                 "อื่นๆ"
24:             });
25: 
26:             cmbType.SelectedIndex = 0;
27:         }
28: 
29:         private async void btnCreate_Click(object sender, EventArgs e)
30:         {
31:             try
32:             {
33:                 var req = new CreateJobRequest
34:                 {
35:                     // แก้ไขจาก "BARCODE123" เป็นค่าจาก TextBox
36:                     BarcodeRaw = txtBarcodeRaw.Text.Trim(),
37:                     CreatedBy = "operator",
38: 
39:                     OrderNo = txtOrderNo.Text.Trim(),
40:                     CustomerName = txtCustomerName.Text.Trim(),
41:                     Type = cmbType.SelectedItem?.ToString(),
42:                     Qty = (int)numQty.Value
43:                 };
44: 
45:                 // เพิ่ม Validation สำหรับ BarcodeRaw
46:                 if (string.IsNullOrWhiteSpace(req.BarcodeRaw))
47:                 {
48:                     MessageBox.Show("กรุณาใส่ Raw Barcode");
49:                     return;
50:                 }
51: 
52:                 if (string.IsNullOrWhiteSpace(req.OrderNo))
53:                 {
54:                     MessageBox.Show("กรุณาใส่ Order No");
55:                     return;
56:                 }
57: 
58:                 var created = await _api.CreateJobAsync(req);
59: 
60:                 if (created != null)
61:                 {
62:                     MessageBox.Show($"สร้าง Job สำเร็จ ID: {created.Id}");
63:                     ClearForm();
64:                 }
65:             }
66:             catch (Exception ex)
67:             {
68:                 MessageBox.Show("Error: " + ex.Message);
69:             }
70:         }
71: 
72:         private void btnReset_Click(object sender, EventArgs e)
73:         {
74:             ClearForm();
75:         }
76: 
77:         private void ClearForm()
78:         {
79:             txtOrderNo.Text = "";
80:             txtCustomerName.Text = "";
81:             txtBarcodeRaw.Text = ""; // ล้างค่า Barcode
82:             cmbType.SelectedIndex = 0;
83:             numQty.Value = 1;
84:         }
85:     }
86: }
```

## File: frmMain.cs
```csharp
   1: using System.ComponentModel;
   2: using System.Diagnostics;
   3: using System.IO.Ports;
   4: using System.Xml.Linq;
   5: using InkjetOperator.Adapters;
   6: using InkjetOperator.Managers;
   7: using InkjetOperator.Models;
   8: using InkjetOperator.PlcAdapter;
   9: using InkjetOperator.Services;
  10: using Microsoft.Data.Sqlite;
  11: using Microsoft.VisualBasic.Logging;
  12: 
  13: namespace InkjetOperator;
  14: 
  15: /// <summary>
  16: /// Main operator form — polls backend for pending jobs, displays pattern detail,
  17: /// sends commands to inkjet adapters, posts results back.
  18: /// Follows Linx frmMain pattern: timer polling, Invoke() for thread safety,
  19: /// manager instances created directly (no DI).
  20: /// </summary>
  21: public partial class frmMain : Form
  22: {
  23:     // ── Hardware managers ──
  24:     private Rs232Manager _rs232Ij1 = new();
  25:     private Rs232Manager _rs232Ij2 = new();
  26:     private TcpManager _tcpIj3 = new();
  27:     private TcpManager _tcpIj4 = new();
  28:     private PlcManager _plc = new();
  29: 
  30:     // ── Adapters ──
  31:     private MkCompactAdapter _adapterIj1;
  32:     private MkCompactAdapter _adapterIj2;
  33:     private SqliteInkjetAdapter _adapterIj3;
  34:     private SqliteInkjetAdapter _adapterIj4;
  35: 
  36:     // ── API ──
  37:     private ApiClient _api;
  38: 
  39:     // ── State ──
  40:     private List<PrintJob> _pendingJobs = new();
  41:     private ResolvedJobResponse? _currentResolved;
  42:     private int _selectedJobId = -1;
  43: 
  44:     private bool _isUpdatingUv = false;
  45:     private BindingList<JobRow> _jobBindingList = new();
  46:     private BindingList<InkjetConfigDto> _configBindingList = new();
  47:     private BindingList<TextBlockDto> _textBlockBindingList = new();
  48:     private BindingList<UvRow> _uvBindingList = new();
  49: 
  50:     private bool _isRefreshingJobs = false;
  51: 
  52:     private int currentStep = 1;
  53: 
  54:     public frmMain()
  55:     {
  56:         InitializeComponent();
  57: 
  58:         _adapterIj1 = new MkCompactAdapter(_rs232Ij1);
  59:         _adapterIj2 = new MkCompactAdapter(_rs232Ij2);
  60:         _adapterIj3 = new SqliteInkjetAdapter(_tcpIj3, ""); // db path TBD
  61:         _adapterIj4 = new SqliteInkjetAdapter(_tcpIj4, ""); // db path TBD
  62: 
  63:         // เพิ่มบรรทัดนี้เพื่อให้ dgvUVBlocks สร้างคอลัมน์ตาม properties ของ TextBlockDto
  64:         dgvUVBlocks.AutoGenerateColumns = true;
  65:         dgvTextBlocks.AutoGenerateColumns = true;
  66: 
  67:         _api = new ApiClient("http://localhost:3000");
  68:     }
  69: 
  70:     // ════════════════════════════════════════
  71:     //  Form events
  72:     // ════════════════════════════════════════
  73: 
  74:     private async void frmMain_Load(object sender, EventArgs e)
  75:     {
  76:         InitializeDatabase();
  77:         currentStep = 1;
  78:         UpdateStepButtons();
  79:         // Populate COM port dropdowns
  80:         string[] ports = SerialPort.GetPortNames();
  81:         cmbCom1.Items.AddRange(ports);
  82:         cmbCom2.Items.AddRange(ports);
  83:         if (cmbCom1.Items.Count > 0) cmbCom1.SelectedIndex = 0;
  84:         if (cmbCom2.Items.Count > 1) cmbCom2.SelectedIndex = 1;
  85:         else if (cmbCom2.Items.Count > 0) cmbCom2.SelectedIndex = 0;
  86: 
  87:         //await SetupUvTableAsync();
  88:         await LoadUvDataToGrid();
  89: 
  90:         dgvUVBlocks.CellValueChanged += dgvUVBlocks_CellValueChanged;
  91: 
  92:         dgvUVBlocks.CurrentCellDirtyStateChanged += (s, e) =>
  93:         {
  94:             if (dgvUVBlocks.IsCurrentCellDirty)
  95:             {
  96:                 dgvUVBlocks.CommitEdit(DataGridViewDataErrorContexts.Commit);
  97:             }
  98:         };
  99: 
 100:         dgvJobs.AutoGenerateColumns = true;
 101:         dgvJobs.DataSource = _jobBindingList;
 102:         dgvConfigs.DataSource = _configBindingList;
 103:         dgvTextBlocks.DataSource = _textBlockBindingList;
 104:         dgvUVBlocks.AutoGenerateColumns = false;
 105:         dgvUVBlocks.DataSource = _uvBindingList;
 106: 
 107:         dgvUVBlocks.Columns.Clear();
 108: 
 109:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 110:         {
 111:             HeaderText = "Inkjet",
 112:             DataPropertyName = "Inkjet",
 113:             ReadOnly = true,
 114:             Width = 120
 115:         });
 116: 
 117:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 118:         {
 119:             HeaderText = "Lot",
 120:             DataPropertyName = "Lot",
 121:             Width = 150
 122:         });
 123: 
 124:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 125:         {
 126:             HeaderText = "Name",
 127:             DataPropertyName = "Name",
 128:             AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
 129:         });
 130: 
 131:         tmrPoll.Start();
 132:         Log("Application started. Polling every 5s.");
 133:     }
 134: 
 135:     private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
 136:     {
 137:         tmrPoll.Stop();
 138:         _rs232Ij1.CloseSerialPort();
 139:         _rs232Ij2.CloseSerialPort();
 140:         _tcpIj3.Disconnect();
 141:         _tcpIj4.Disconnect();
 142:         _plc.Disconnect();
 143:     }
 144: 
 145:     // ════════════════════════════════════════
 146:     //  Connection handlers
 147:     // ════════════════════════════════════════
 148: 
 149:     private void btnConnectRs232_Click(object sender, EventArgs e)
 150:     {
 151:         try
 152:         {
 153:             if (cmbCom1.SelectedItem != null)
 154:             {
 155:                 _rs232Ij1.ConfigureSerialPort(cmbCom1.SelectedItem.ToString()!);
 156:                 _rs232Ij1.OpenSerialPort();
 157:             }
 158:             if (cmbCom2.SelectedItem != null)
 159:             {
 160:                 _rs232Ij2.ConfigureSerialPort(cmbCom2.SelectedItem.ToString()!);
 161:                 _rs232Ij2.OpenSerialPort();
 162:             }
 163:             UpdateDeviceStatus();
 164:             Log("RS232 connected.");
 165:         }
 166:         catch (Exception ex)
 167:         {
 168:             Log("RS232 connect error: " + ex.Message);
 169:         }
 170:     }
 171: 
 172:     private void btnDisconnectRs232_Click(object sender, EventArgs e)
 173:     {
 174:         _rs232Ij1.CloseSerialPort();
 175:         _rs232Ij2.CloseSerialPort();
 176:         UpdateDeviceStatus();
 177:         Log("RS232 disconnected.");
 178:     }
 179: 
 180:     private async void btnConnectTcp_Click(object sender, EventArgs e)
 181:     {
 182:         try
 183:         {
 184:             string host = txtTcpHost.Text.Trim();
 185:             int port = int.Parse(txtTcpPort.Text.Trim());
 186:             await _tcpIj3.ConnectAsync(host, port);
 187:             await _tcpIj4.ConnectAsync(host, port + 1); // IJ4 on next port
 188:             UpdateDeviceStatus();
 189:             Log("TCP connected.");
 190:         }
 191:         catch (Exception ex)
 192:         {
 193:             Log("TCP connect error: " + ex.Message);
 194:         }
 195:     }
 196: 
 197:     private void btnDisconnectTcp_Click(object sender, EventArgs e)
 198:     {
 199:         _tcpIj3.Disconnect();
 200:         _tcpIj4.Disconnect();
 201:         UpdateDeviceStatus();
 202:         Log("TCP disconnected.");
 203:     }
 204: 
 205:     private void btnApplyApi_Click(object sender, EventArgs e)
 206:     {
 207:         string url = txtApiUrl.Text.Trim();
 208:         _api = new ApiClient(url);
 209:         lblApiStatus.Text = "Applied";
 210:         Log($"API URL set to: {url}");
 211:     }
 212: 
 213:     // ════════════════════════════════════════
 214:     //  Polling
 215:     // ════════════════════════════════════════
 216: 
 217:     private async void tmrPoll_Tick(object sender, EventArgs e)
 218:     {
 219:         tmrPoll.Stop(); // Prevent re-entry
 220:         try
 221:         {
 222:             var jobs = await _api.GetPendingJobsAsync();
 223:             _pendingJobs = jobs;
 224:             UpdateJobGrid();
 225:             UpdateDeviceStatus();
 226:             lblApiStatus.Text = "OK";
 227:         }
 228:         catch (Exception ex)
 229:         {
 230:             lblApiStatus.Text = "Error";
 231:             Log("Poll error: " + ex.Message);
 232:         }
 233:         finally
 234:         {
 235:             tmrPoll.Start();
 236:         }
 237:     }
 238: 
 239:     private void btnRefresh_Click(object sender, EventArgs e)
 240:     {
 241:         tmrPoll_Tick(sender, e);
 242:     }
 243: 
 244:     // ════════════════════════════════════════
 245:     //  Job selection
 246:     // ════════════════════════════════════════
 247:     private async void dgvJobs_SelectionChanged(object sender, EventArgs e)
 248:     {
 249:         if (_isRefreshingJobs) return; // 🔥 กันตอน poll
 250: 
 251:         if (dgvJobs.SelectedRows.Count == 0) return;
 252: 
 253:         var row = dgvJobs.SelectedRows[0];
 254:         if (row.Cells["Id"].Value == null) return;
 255: 
 256:         int jobId = (int)row.Cells["Id"].Value;
 257:         string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
 258:         if (jobId == _selectedJobId) return;
 259:         _selectedJobId = jobId;
 260: 
 261:         // Helpers
 262:         static bool ReaderHasColumn(SqliteDataReader r, string name)
 263:         {
 264:             for (int i = 0; i < r.FieldCount; i++)
 265:             {
 266:                 if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
 267:                     return true;
 268:             }
 269:             return false;
 270:         }
 271: 
 272:         static string? GetStringSafe(SqliteDataReader r, string name)
 273:         {
 274:             if (!ReaderHasColumn(r, name)) return null;
 275:             int idx = r.GetOrdinal(name);
 276:             return r.IsDBNull(idx) ? null : r.GetString(idx);
 277:         }
 278: 
 279:         static int? GetIntSafe(SqliteDataReader r, string name)
 280:         {
 281:             var s = GetStringSafe(r, name);
 282:             if (string.IsNullOrWhiteSpace(s)) return null;
 283:             return int.TryParse(s, out var v) ? v : null;
 284:         }
 285: 
 286:         try
 287:         {
 288:             string dbPath = @"D:\DB\uv_data.db3";
 289:             string connStr = $"Data Source={dbPath}";
 290:             bool found = false;
 291: 
 292:             try
 293:             {
 294:                 await using var conn = new SqliteConnection(connStr);
 295:                 await conn.OpenAsync();
 296: 
 297:                 await using var cmd = conn.CreateCommand();
 298:                 cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
 299:                 cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
 300: 
 301:                 await using var reader = await cmd.ExecuteReaderAsync();
 302:                 if (await reader.ReadAsync())
 303:                 {
 304:                     // Build PatternDetail from known schema columns.
 305:                     var pattern = new PatternDetail
 306:                     {
 307:                         Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
 308:                         Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
 309:                     };
 310: 
 311:                     // Helper to build MK config (mk1 / mk2)
 312:                     InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
 313:                     {
 314:                         var cfg = new InkjetConfigDto
 315:                         {
 316:                             Ordinal = ordinal,
 317:                             ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
 318:                             ProgramName = GetStringSafe(reader, programNameColumn),
 319:                             Width = GetIntSafe(reader, $"{prefix}width"),
 320:                             Height = GetIntSafe(reader, $"{prefix}height"),
 321:                             TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
 322:                             Direction = GetIntSafe(reader, $"{prefix}text_direction"),
 323:                             SteelType = null,
 324:                             Suspended = false
 325:                         };
 326: 
 327:                         // text blocks 1..5
 328:                         for (int b = 1; b <= 5; b++)
 329:                         {
 330:                             string textCol = $"{prefix}block{b}_text";
 331:                             if (ReaderHasColumn(reader, textCol))
 332:                             {
 333:                                 var text = GetStringSafe(reader, textCol);
 334:                                 if (!string.IsNullOrEmpty(text))
 335:                                 {
 336:                                     var tb = new TextBlockDto
 337:                                     {
 338:                                         BlockNumber = b,
 339:                                         Text = text,
 340:                                         X = GetIntSafe(reader, $"{prefix}block{b}_x"),
 341:                                         Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
 342:                                         Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
 343:                                         Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
 344:                                     };
 345:                                     cfg.TextBlocks.Add(tb);
 346:                                 }
 347:                             }
 348:                         }
 349: 
 350:                         return cfg;
 351:                     }
 352: 
 353:                     // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
 354:                     var mk1 = BuildMkConfig("mk1_", 1, "program_name");
 355:                     // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
 356:                     var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
 357: 
 358:                     pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
 359: 
 360:                     // Conveyor speeds / servo configs - attempt to map if available (optional)
 361:                     var spd1 = GetIntSafe(reader, "belt1_inkjet");
 362:                     var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
 363:                     if (spd1.HasValue || spd2.HasValue)
 364:                     {
 365:                         pattern.ConveyorSpeeds = new ConveyorSpeedDto
 366:                         {
 367:                             Speed1 = spd1,
 368:                             Speed2 = spd2,
 369:                             Speed3 = GetIntSafe(reader, "belt3")
 370:                         };
 371:                     }
 372: 
 373:                     // Minimal job + resolved response
 374:                     _currentResolved = new ResolvedJobResponse
 375:                     {
 376:                         Job = new PrintJob
 377:                         {
 378:                             Id = jobId,
 379:                             BarcodeRaw = rawbarcode,
 380:                             LotNumber = row.Cells["LotNumber"].Value?.ToString(),
 381:                             Status = row.Cells["Status"].Value?.ToString()
 382:                         },
 383:                         Pattern = pattern
 384:                     };
 385: 
 386:                     string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
 387:                     string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ
 388: 
 389:                     // 🔥 background + กันค้าง
 390:                     //_ = Task.Run(async () =>
 391:                     //{
 392:                     //    try
 393:                     //    {
 394:                     //        await UpdateUvData(lot, name);
 395:                     //    }
 396:                     //    catch (Exception ex)
 397:                     //    {
 398:                     //        Log("UV DB error: " + ex.Message);
 399:                     //    }
 400:                     //});
 401: 
 402:                     found = true;
 403:                     UpdateDetailPanel();
 404:                 }
 405: 
 406:                 await conn.CloseAsync();
 407:             }
 408:             catch (Exception dbEx)
 409:             {
 410:                 Log("SQLite query error: " + dbEx.Message);
 411:             }
 412: 
 413:             if (!found)
 414:             {
 415:                 var resolved = await _api.GetResolvedJobAsync(jobId);
 416:                 _currentResolved = resolved;
 417:                 UpdateDetailPanel();
 418:             }
 419:         }
 420:         catch (Exception ex)
 421:         {
 422:             Log("Load job detail error: " + ex.Message);
 423:         }
 424:     }
 425: 
 426:     private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
 427:     {
 428:         if (dgvConfigs.SelectedRows.Count == 0 || _currentResolved?.Pattern == null) return;
 429: 
 430:         int idx = dgvConfigs.SelectedRows[0].Index;
 431:         var configs = _currentResolved.Pattern.InkjetConfigs;
 432: 
 433:         if (configs == null || idx >= configs.Count) return;
 434: 
 435:         var config = configs[idx];
 436: 
 437:         // dgvTextBlocks แสดงตามแถวที่เลือก (ปกติคือ MK1, MK2)
 438:         //dgvTextBlocks.DataSource = null;
 439:         _textBlockBindingList.RaiseListChangedEvents = false;
 440:         _textBlockBindingList.Clear();
 441: 
 442:         //foreach (var b in config.TextBlocks ?? new List<TextBlockDto>())
 443:         //{
 444:         //    _textBlockBindingList.Add(b);
 445:         //}
 446: 
 447:         foreach (var block in config.TextBlocks)
 448:         {
 449:             // --- ส่วนที่แก้ไข/เพิ่มใหม่ ---
 450:             // ใช้ LINQ ค้นหา Pattern ที่ชื่อตรงกับ block.Text (เช่น "DDDD")
 451:             //var pattern = PatternEngine.Patterns.FirstOrDefault(p => p.Name == block.Text);
 452:             string previewText = PatternEngine.Process(txtLot.Text, block.Text);
 453:             //Debug.WriteLine(block.Text);
 454: 
 455:             if (previewText != null)
 456:             {
 457:                 // ถ้าเจอ ให้เอาค่าจาก txtLot (หรือ res.Job.LotNumber) มาแปลงด้วย Rule
 458:                 block.RuleResult = previewText;
 459:             }
 460:             else
 461:             {
 462:                 // ถ้าไม่เจอ ให้แสดงค่าเดิมของมัน
 463:                 block.RuleResult = block.Text;
 464:             }
 465:             // --------------------------
 466: 
 467:             _textBlockBindingList.Add(block);
 468:         }
 469: 
 470:         _textBlockBindingList.RaiseListChangedEvents = true;
 471:         _textBlockBindingList.ResetBindings();
 472: 
 473:         // --- นำ UpdateUvGridOnly(config.TextBlocks) ออก ---
 474:         // เพื่อให้ dgvUVBlocks ไม่เปลี่ยนค่าตามการเลือกใน Grid นี้
 475:     }
 476: 
 477:     // ════════════════════════════════════════
 478:     //  Send to devices
 479:     // ════════════════════════════════════════
 480: 
 481:     private async void btnSend_Click(object sender, EventArgs e)
 482:     {
 483:         if (_currentResolved == null || _selectedJobId < 0)
 484:         {
 485:             MessageBox.Show("No job selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
 486:             return;
 487:         }
 488: 
 489:         btnSend.Enabled = false;
 490:         var commandResults = new List<CommandResult>();
 491: 
 492:         try
 493:         {
 494:             // Mark job as executing
 495:             bool ok = await _api.ExecuteJobAsync(_selectedJobId);
 496:             if (!ok)
 497:             {
 498:                 Log("Failed to mark job as executing.");
 499:                 btnSend.Enabled = true;
 500:                 return;
 501:             }
 502: 
 503:             // Re-fetch resolved data (templates may depend on attempt number)
 504:             var resolved = await _api.GetResolvedJobAsync(_selectedJobId);
 505:             if (resolved?.Pattern == null)
 506:             {
 507:                 Log("Failed to get resolved job.");
 508:                 btnSend.Enabled = true;
 509:                 return;
 510:             }
 511: 
 512:             _currentResolved = resolved;
 513:             UpdateDetailPanel();
 514: 
 515:             // Send to each inkjet by ordinal
 516:             var configs = resolved.Pattern.InkjetConfigs ?? new List<InkjetConfigDto>();
 517:             bool hasError = false;
 518: 
 519:             foreach (var config in configs.OrderBy(c => c.Ordinal))
 520:             {
 521:                 if (config.Suspended)
 522:                 {
 523:                     Log($"IJ{config.Ordinal} suspended — skipping.");
 524:                     continue;
 525:                 }
 526: 
 527:                 IInkjetAdapter adapter = GetAdapterByOrdinal(config.Ordinal);
 528: 
 529:                 if (!adapter.IsConnected())
 530:                 {
 531:                     Log($"IJ{config.Ordinal} not connected — skipping.");
 532:                     commandResults.Add(new CommandResult
 533:                     {
 534:                         Ordinal = config.Ordinal,
 535:                         Command = "connect_check",
 536:                         Success = false,
 537:                         Response = "Not connected",
 538:                         SentAt = DateTime.UtcNow.ToString("o"),
 539:                     });
 540:                     hasError = true;
 541:                     break; // Sequential error — stop (csv_extractor.py line 281)
 542:                 }
 543: 
 544:                 // 1. Change program
 545:                 if (config.ProgramNumber.HasValue)
 546:                 {
 547:                     var r = await adapter.ChangeProgramAsync(config.ProgramNumber.Value);
 548:                     r.Ordinal = config.Ordinal;
 549:                     commandResults.Add(r);
 550:                     Log($"IJ{config.Ordinal} ChangeProgram({config.ProgramNumber}): {(r.Success ? "OK" : "FAIL")}");
 551: 
 552:                     if (!r.Success) { hasError = true; break; }
 553:                 }
 554: 
 555:                 // 2. Send config
 556:                 var cfgResult = await adapter.SendConfigAsync(config);
 557:                 cfgResult.Ordinal = config.Ordinal;
 558:                 commandResults.Add(cfgResult);
 559:                 Log($"IJ{config.Ordinal} SendConfig: {(cfgResult.Success ? "OK" : "FAIL")}");
 560: 
 561:                 if (!cfgResult.Success) { hasError = true; break; }
 562: 
 563:                 // 3. Send text blocks (device blocks 6-10)
 564:                 var blocks = config.TextBlocks ?? new List<TextBlockDto>();
 565:                 foreach (var block in blocks.OrderBy(b => b.BlockNumber))
 566:                 {
 567:                     int deviceBlock = block.BlockNumber + 5; // blocks 6-10
 568:                     var tbResult = await adapter.SendTextBlockAsync(block, deviceBlock);
 569:                     tbResult.Ordinal = config.Ordinal;
 570:                     commandResults.Add(tbResult);
 571:                     Log($"IJ{config.Ordinal} TextBlock({block.BlockNumber}): {(tbResult.Success ? "OK" : "FAIL")}");
 572: 
 573:                     if (!tbResult.Success) { hasError = true; break; }
 574:                     await Task.Delay(30); // 30ms delay between blocks (csv_extractor.py line 274)
 575:                 }
 576: 
 577:                 if (hasError) break;
 578: 
 579:                 // 4. Resume printing
 580:                 var resumeResult = await adapter.ResumeAsync();
 581:                 resumeResult.Ordinal = config.Ordinal;
 582:                 commandResults.Add(resumeResult);
 583:                 Log($"IJ{config.Ordinal} Resume: {(resumeResult.Success ? "OK" : "FAIL")}");
 584: 
 585:                 if (!resumeResult.Success) { hasError = true; break; }
 586:             }
 587: 
 588:             // Send conveyor speed + servo to PLC
 589:             if (!hasError && resolved.Pattern.ConveyorSpeeds != null)
 590:             {
 591:                 var spd = resolved.Pattern.ConveyorSpeeds;
 592:                 await _plc.WriteSpeedAsync(spd.Speed1 ?? 0, spd.Speed2 ?? 0, spd.Speed3 ?? 0);
 593:                 Log($"PLC speed: {spd.Speed1}/{spd.Speed2}/{spd.Speed3}");
 594:             }
 595: 
 596:             if (!hasError && resolved.Pattern.ServoConfigs != null)
 597:             {
 598:                 foreach (var servo in resolved.Pattern.ServoConfigs)
 599:                 {
 600:                     await _plc.WriteServoAsync(servo.Ordinal, servo.Position ?? 0, servo.PostAct ?? 0, servo.Delay ?? 0, servo.Trigger ?? 0);
 601:                     Log($"PLC servo ordinal={servo.Ordinal}: pos={servo.Position}");
 602:                 }
 603:             }
 604: 
 605:             // Post results back to backend
 606:             var payload = new JobResultsPayload
 607:             {
 608:                 Success = !hasError,
 609:                 ErrorMessage = hasError ? "One or more commands failed" : null,
 610:                 Commands = commandResults,
 611:             };
 612: 
 613:             await _api.PostResultsAsync(_selectedJobId, payload);
 614:             Log(hasError ? "Job completed with errors." : "Job completed successfully.");
 615:         }
 616:         catch (Exception ex)
 617:         {
 618:             Log("Send error: " + ex.Message);
 619:         }
 620:         finally
 621:         {
 622:             btnSend.Enabled = true;
 623:         }
 624:     }
 625: 
 626:     private async void btnRetry_Click(object sender, EventArgs e)
 627:     {
 628:         if (_selectedJobId < 0) return;
 629: 
 630:         bool ok = await _api.RetryJobAsync(_selectedJobId);
 631:         Log(ok ? $"Job {_selectedJobId} retried." : $"Retry failed for job {_selectedJobId}.");
 632:     }
 633: 
 634:     // ════════════════════════════════════════
 635:     //  Helpers
 636:     // ════════════════════════════════════════
 637: 
 638:     private IInkjetAdapter GetAdapterByOrdinal(int ordinal)
 639:     {
 640:         return ordinal switch
 641:         {
 642:             1 => _adapterIj1,
 643:             2 => _adapterIj2,
 644:             3 => _adapterIj3,
 645:             4 => _adapterIj4,
 646:             _ => _adapterIj1,
 647:         };
 648:     }
 649: 
 650:     private void UpdateDeviceStatus()
 651:     {
 652:         if (InvokeRequired) { Invoke(UpdateDeviceStatus); return; }
 653: 
 654:         lblStatusIj1.BackColor = _adapterIj1.IsConnected() ? Color.Green : Color.Gray;
 655:         lblStatusIj2.BackColor = _adapterIj2.IsConnected() ? Color.Green : Color.Gray;
 656:         lblStatusIj3.BackColor = _adapterIj3.IsConnected() ? Color.Green : Color.Gray;
 657:         lblStatusIj4.BackColor = _adapterIj4.IsConnected() ? Color.Green : Color.Gray;
 658:     }
 659: 
 660:     private void UpdateJobGrid()
 661:     {
 662:         if (InvokeRequired) { Invoke(UpdateJobGrid); return; }
 663: 
 664:         _isRefreshingJobs = true; // 🔥 กัน event
 665: 
 666:         int selectedId = -1;
 667:         if (dgvJobs.CurrentRow != null)
 668:         {
 669:             selectedId = (int)dgvJobs.CurrentRow.Cells["Id"].Value;
 670:         }
 671: 
 672:         _jobBindingList.RaiseListChangedEvents = false;
 673:         _jobBindingList.Clear();
 674: 
 675:         foreach (var j in _pendingJobs)
 676:         {
 677:             _jobBindingList.Add(new JobRow
 678:             {
 679:                 Id = j.Id,
 680:                 BarcodeRaw = j.BarcodeRaw,
 681:                 LotNumber = j.LotNumber,
 682:                 Status = j.Status,
 683:                 Attempt = j.Attempt
 684:             });
 685:         }
 686: 
 687:         _jobBindingList.RaiseListChangedEvents = true;
 688:         _jobBindingList.ResetBindings();
 689: 
 690:         // 🔥 restore selection
 691:         if (selectedId != -1)
 692:         {
 693:             foreach (DataGridViewRow row in dgvJobs.Rows)
 694:             {
 695:                 if ((int)row.Cells["Id"].Value == selectedId)
 696:                 {
 697:                     row.Selected = true;
 698:                     dgvJobs.CurrentCell = row.Cells[0];
 699:                     break;
 700:                 }
 701:             }
 702:         }
 703: 
 704:         _isRefreshingJobs = false; // 🔥 เปิด event กลับ
 705:     }
 706: 
 707:     private void UpdateDetailPanel()
 708:     {
 709:         if (InvokeRequired) { Invoke(UpdateDetailPanel); return; }
 710: 
 711:         if (_currentResolved == null) return;
 712: 
 713:         var job = _currentResolved.Job;
 714:         var pattern = _currentResolved.Pattern;
 715: 
 716:         txtBarcode.Text = job?.BarcodeRaw ?? "";
 717:         txtLot.Text = job?.LotNumber ?? "";
 718:         txtStatus.Text = job?.Status ?? "";
 719:         txtPattern.Text = pattern?.Name ?? "";
 720: 
 721:         _configBindingList.RaiseListChangedEvents = false;
 722: 
 723:         var newList = pattern?.InkjetConfigs ?? new List<InkjetConfigDto>();
 724: 
 725:         // 🔥 sync count
 726:         while (_configBindingList.Count > newList.Count)
 727:         {
 728:             _configBindingList.RemoveAt(_configBindingList.Count - 1);
 729:         }
 730: 
 731:         for (int i = 0; i < newList.Count; i++)
 732:         {
 733:             if (i < _configBindingList.Count)
 734:             {
 735:                 // 🔥 update object reference (สำคัญ)
 736:                 _configBindingList[i] = newList[i];
 737:             }
 738:             else
 739:             {
 740:                 _configBindingList.Add(newList[i]);
 741:             }
 742:         }
 743: 
 744:         _configBindingList.RaiseListChangedEvents = true;
 745:         _configBindingList.ResetBindings();
 746: 
 747:         // --- เพิ่มส่วนนี้: Fix ให้ dgvUVBlocks แสดงเฉพาะ UV1 (Ordinal 3) และ UV2 (Ordinal 4) ---
 748:         if (pattern?.InkjetConfigs != null)
 749:         {
 750:             // ดึงเฉพาะ Config ของ UV (Ordinal 3 และ 4)
 751:             var uvConfigs = pattern.InkjetConfigs
 752:                 .Where(c => c.Ordinal == 3 || c.Ordinal == 4)
 753:                 .OrderBy(c => c.Ordinal)
 754:                 .ToList();
 755: 
 756:             // สร้างรายการสำหรับ Display ใน Grid
 757:             var uvDisplayList = new List<object>();
 758:             foreach (var cfg in uvConfigs)
 759:             {
 760:                 string printerName = cfg.Ordinal == 3 ? "เครื่องพิมพ์ UV1" : "เครื่องพิมพ์ UV2";
 761: 
 762:                 // ดึงค่า Block 1 และ 2 (Lot และ Name)
 763:                 string lotVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 1)?.Text ?? "";
 764:                 string nameVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 2)?.Text ?? "";
 765: 
 766:                 uvDisplayList.Add(new
 767:                 {
 768:                     Printer = printerName,
 769:                     LotText = lotVal,
 770:                     NameText = nameVal,
 771:                     // เก็บ Reference ไว้เผื่อต้องการดึงไปใช้งานต่อ
 772:                     _originalConfig = cfg
 773:                 });
 774:             }
 775:         }
 776:     }
 777: 
 778:     private void Log(string message)
 779:     {
 780:         if (InvokeRequired) { Invoke(() => Log(message)); return; }
 781: 
 782:         string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
 783:         txtLog.AppendText(line + Environment.NewLine);
 784:     }
 785: 
 786:     private void button1_Click(object sender, EventArgs e)
 787:     {
 788:         var steps = new[]
 789:         {
 790:         new BotStep { Name = "Document",   X = 2325, Y = 59,  VerifyArea = new Rectangle(2100, 0, 400, 300) },
 791:         new BotStep { Name = "Open",       X = 2207, Y = 133, VerifyArea = new Rectangle(2100, 100, 400, 400) },
 792:         new BotStep { Name = "SelectFile", X = 1506, Y = 654, VerifyArea = new Rectangle(800, 400, 800, 600) },
 793:         new BotStep { Name = "OpenBtn",    X = 1652, Y = 946, VerifyArea = new Rectangle(1500, 800, 500, 300) },
 794:     };
 795: 
 796:         BotClickHelper.RunAsync("uvinkjet", steps, result =>
 797:         {
 798:             if (this.IsHandleCreated)
 799:                 this.Invoke((MethodInvoker)(() =>
 800:                     MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error)));
 801:         });
 802:     }
 803: 
 804:     private void button2_Click(object sender, EventArgs e)
 805:     {
 806:         // ใช้ Invoke เพื่อความปลอดภัยว่าทำงานบน UI Thread (STA)
 807:         this.Invoke(new Action(() =>
 808:         {
 809:             using (frmPatternEditor editor = new frmPatternEditor())
 810:             {
 811:                 editor.StartPosition = FormStartPosition.CenterParent;
 812:                 editor.ShowDialog();
 813:             }
 814:         }));
 815:     }
 816: 
 817:     private async void button3_Click(object sender, EventArgs e)
 818:     {
 819:         //await SendTextBlocksToIjAsync(3);
 820:         var form = new frmCreateJob(_api);
 821:         form.ShowDialog();
 822:     }
 823: 
 824:     private async void button4_Click(object sender, EventArgs e)
 825:     {
 826:         await TestSendToIj3TcpAsync();
 827:     }
 828: 
 829:     // Example usage: send blocks from dgvTextBlocks to IJ3 via TCP (simple demo)
 830:     private async Task TestSendToIj3TcpAsync()
 831:     {
 832:         // read TCP host/port from UI controls
 833:         string host = txtTcpHost.Text.Trim();
 834:         if (!int.TryParse(txtTcpPort.Text.Trim(), out int port))
 835:         {
 836:             Log("Invalid TCP port");
 837:             return;
 838:         }
 839: 
 840:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
 841:         try
 842:         {
 843:             await client.ConnectAsync(host, port);
 844:         }
 845:         catch (Exception ex)
 846:         {
 847:             Log("TCP connect failed: " + ex.Message);
 848:             return;
 849:         }
 850: 
 851:         // Example: change program to 13
 852:         var progResp = await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, 13);
 853:         Log($"ChangeProgram resp: {progResp}");
 854: 
 855:         // Send each row in dgvTextBlocks
 856:         foreach (DataGridViewRow row in dgvTextBlocks.Rows)
 857:         {
 858:             if (row.IsNewRow) continue;
 859: 
 860:             int blockNumber = 1;
 861:             if (dgvTextBlocks.Columns.Contains("BlockNumber") && int.TryParse(row.Cells["BlockNumber"].Value?.ToString(), out var bn))
 862:                 blockNumber = bn;
 863: 
 864:             string text = row.Cells["Text"].Value?.ToString() ?? row.Cells["text"].Value?.ToString() ?? "";
 865:             int x = int.TryParse(row.Cells["X"].Value?.ToString(), out var tx) ? tx : 0;
 866:             int y = int.TryParse(row.Cells["Y"].Value?.ToString(), out var ty) ? ty : 0;
 867:             int size = int.TryParse(row.Cells["Size"].Value?.ToString(), out var ts) ? ts : 1;
 868:             int scale = int.TryParse(row.Cells["Scale"].Value?.ToString(), out var tsc) ? tsc : 1;
 869: 
 870:             var block = new InkjetOperator.Services.SimpleTextBlock(blockNumber, text, x, y, size, scale);
 871:             var (fsResp, f1Resp) = await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, 13, block);
 872:             Log($"IJ3 TextBlock({blockNumber}) FS='{fsResp}' F1='{f1Resp}'");
 873: 
 874:             // If adapter or device returns empty error, you can decide to continue or break.
 875:             await Task.Delay(30);
 876:         }
 877: 
 878:         var resume = await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
 879:         Log($"Resume resp: {resume}");
 880:     }
 881: 
 882:     private async void dgvUVBlocks_CellValueChanged(object sender, DataGridViewCellEventArgs e)
 883:     {
 884:         if (e.RowIndex < 0) return;
 885: 
 886:         var row = dgvUVBlocks.Rows[e.RowIndex];
 887: 
 888:         if (row.DataBoundItem is not UvRow data) return;
 889: 
 890:         try
 891:         {
 892:             await UpdateUvRow(data.Id, data.Lot, data.Name);
 893:             Log($"[UV Updated] ID={data.Id}, Lot={data.Lot}, Name={data.Name}");
 894:         }
 895:         catch (Exception ex)
 896:         {
 897:             Log("Update UV error: " + ex.Message);
 898:         }
 899:     }
 900: 
 901:     private async Task UpdateUvRow(int id, string lot, string name)
 902:     {
 903:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 904:         await conn.OpenAsync();
 905: 
 906:         var cmd = conn.CreateCommand();
 907:         cmd.CommandText = @"
 908:         UPDATE uv_print_data
 909:         SET lot = @lot,
 910:             name = @name,
 911:             update_at = CURRENT_TIMESTAMP
 912:         WHERE id = @id
 913:     ";
 914: 
 915:         cmd.Parameters.AddWithValue("@id", id);
 916:         cmd.Parameters.AddWithValue("@lot", lot);
 917:         cmd.Parameters.AddWithValue("@name", name);
 918: 
 919:         await cmd.ExecuteNonQueryAsync();
 920:     }
 921: 
 922:     public void InitializeDatabase()
 923:     {
 924:         string folderPath = @"D:\DB";
 925:         if (!System.IO.Directory.Exists(folderPath))
 926:         {
 927:             System.IO.Directory.CreateDirectory(folderPath);
 928:         }
 929: 
 930:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 931:         conn.Open();
 932: 
 933:         string createTableSql = @"
 934:     CREATE TABLE IF NOT EXISTS uv_print_data (
 935:         id INTEGER PRIMARY KEY AUTOINCREMENT,
 936:         inkjet_name TEXT,
 937:         lot TEXT,
 938:         name TEXT,
 939:         update_at DATETIME DEFAULT CURRENT_TIMESTAMP
 940:     );";
 941: 
 942:         using (var command = new SqliteCommand(createTableSql, conn))
 943:         {
 944:             command.ExecuteNonQuery();
 945:         }
 946: 
 947:         // เช็คว่ามีข้อมูลหรือยัง ถ้าไม่มีให้ Insert แถวสำหรับ UV1 และ UV2 รอไว้เลย
 948:         var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM uv_print_data", conn);
 949:         long count = (long)checkCmd.ExecuteScalar();
 950:         if (count == 0)
 951:         {
 952:             var insertCmd = new SqliteCommand(@"
 953:             INSERT INTO uv_print_data (id, inkjet_name, lot, name) VALUES 
 954:             (1, 'เครื่องพิมพ์ UV1', '', ''),
 955:             (2, 'เครื่องพิมพ์ UV2', '', '')", conn);
 956:             insertCmd.ExecuteNonQuery();
 957:         }
 958:     }
 959: 
 960:     private async Task LoadUvDataToGrid()
 961:     {
 962:         try
 963:         {
 964:             using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3");
 965:             await conn.OpenAsync();
 966: 
 967:             var cmd = conn.CreateCommand();
 968:             cmd.CommandText = @"
 969:         SELECT id, inkjet_name, lot, name, update_at
 970:         FROM uv_print_data
 971:         ORDER BY id
 972:         ";
 973: 
 974:             var reader = await cmd.ExecuteReaderAsync();
 975: 
 976:             var list = new List<UvRow>();
 977: 
 978:             while (await reader.ReadAsync())
 979:             {
 980:                 list.Add(new UvRow
 981:                 {
 982:                     Id = reader.GetInt32(0), // 🔥 ยังต้องมี
 983:                     Inkjet = reader.GetString(1),
 984:                     Lot = reader.IsDBNull(2) ? "" : reader.GetString(2),
 985:                     Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
 986:                     UpdateAt = reader.IsDBNull(4) ? "" : reader.GetString(4)
 987:                 });
 988:             }
 989: 
 990:             dgvUVBlocks.Invoke(() =>
 991:             {
 992: 
 993:                 dgvUVBlocks.AutoGenerateColumns = false;
 994: 
 995:                 dgvUVBlocks.ReadOnly = false;
 996:                 dgvUVBlocks.AllowUserToAddRows = false;
 997:                 dgvUVBlocks.EditMode = DataGridViewEditMode.EditOnEnter;
 998:                 dgvUVBlocks.SelectionMode = DataGridViewSelectionMode.CellSelect;
 999: 
1000: 
1001:                 _uvBindingList.RaiseListChangedEvents = false;
1002:                 _uvBindingList.Clear();
1003: 
1004:                 foreach (var item in list)
1005:                 {
1006:                     _uvBindingList.Add(item);
1007:                 }
1008: 
1009:                 _uvBindingList.RaiseListChangedEvents = true;
1010:                 _uvBindingList.ResetBindings();
1011: 
1012:             });
1013:         }
1014:         catch (Exception ex)
1015:         {
1016:             Log("Load UV error: " + ex.Message);
1017:         }
1018:     }
1019: 
1020:     private void button6_Click(object sender, EventArgs e)
1021:     {
1022:         var row = _uvBindingList.FirstOrDefault(x => x.Id == 1);
1023: 
1024:         if (row == null)
1025:         {
1026:             MessageBox.Show("ไม่พบ row id = 1");
1027:             return;
1028:         }
1029: 
1030:         string lot = row.Lot;
1031:         string name = row.Name;
1032: 
1033:         Log($"Lot={lot}, Name={name}");
1034: 
1035:         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1036: 
1037:         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1038:         {
1039:             conn.Open();
1040: 
1041:             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1042: 
1043:             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1044:             {
1045:                 cmd.Parameters.AddWithValue("@lot", lot);
1046:                 cmd.Parameters.AddWithValue("@name", name);
1047: 
1048:                 int rows = cmd.ExecuteNonQuery();
1049: 
1050:                 MessageBox.Show("Update สำเร็จ: " + rows + " row");
1051:             }
1052:         }
1053: 
1054:         currentStep = 3; // ไปขั้นที่ 2
1055:         UpdateStepButtons();
1056:     }
1057: 
1058:     private void button8_Click(object sender, EventArgs e)
1059:     {
1060:         var row = _uvBindingList.FirstOrDefault(x => x.Id == 2);
1061: 
1062:         if (row == null)
1063:         {
1064:             MessageBox.Show("ไม่พบ row id = 1");
1065:             return;
1066:         }
1067: 
1068:         string lot = row.Lot;
1069:         string name = row.Name;
1070: 
1071:         Log($"Lot={lot}, Name={name}");
1072: 
1073:         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1074: 
1075:         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1076:         {
1077:             conn.Open();
1078: 
1079:             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1080: 
1081:             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1082:             {
1083:                 cmd.Parameters.AddWithValue("@lot", lot);
1084:                 cmd.Parameters.AddWithValue("@name", name);
1085: 
1086:                 int rows = cmd.ExecuteNonQuery();
1087: 
1088:                 MessageBox.Show("Update สำเร็จ: " + rows + " row");
1089:             }
1090:         }
1091:         currentStep = 1;
1092:         UpdateStepButtons();
1093:     }
1094: 
1095: 
1096:     private async void button5_Click(object sender, EventArgs e)
1097:     {
1098:         //if (dgvConfigs.Rows.Count > 0)
1099:         //{
1100:         //    dgvConfigs.ClearSelection();
1101: 
1102:         //    dgvConfigs.Rows[0].Selected = true;
1103:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[0].Cells[0];
1104: 
1105:         //    var config = dgvConfigs.Rows[0].DataBoundItem as InkjetConfigDto;
1106: 
1107:         //    if (config != null)
1108:         //    {
1109:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1110:         //    }
1111:         //}
1112: 
1113:         //await TestSendToIj3TcpAsync();
1114: 
1115:         if (dgvConfigs.Rows.Count > 1)
1116:         {
1117:             dgvConfigs.ClearSelection();
1118: 
1119:             dgvConfigs.Rows[1].Selected = true;
1120:             dgvConfigs.CurrentCell = dgvConfigs.Rows[1].Cells[0];
1121: 
1122:             var config = dgvConfigs.Rows[1].DataBoundItem as InkjetConfigDto;
1123: 
1124:             if (config != null)
1125:             {
1126:                 MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1127:             }
1128:         }
1129: 
1130:         await TestSendToIj3TcpAsync();
1131: 
1132:         currentStep = 2; // ไปขั้นที่ 2
1133:         UpdateStepButtons();
1134: 
1135:     }
1136: 
1137:     private async void button7_Click(object sender, EventArgs e)
1138:     {
1139:         await SendToIj3FromDbAsync("CCRC0291-DEX0663MS");
1140:         //if (dgvConfigs.Rows.Count > 2)
1141:         //{
1142:         //    dgvConfigs.ClearSelection();
1143: 
1144:         //    dgvConfigs.Rows[2].Selected = true;
1145:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[2].Cells[0];
1146: 
1147:         //    var config = dgvConfigs.Rows[2].DataBoundItem as InkjetConfigDto;
1148: 
1149:         //    if (config != null)
1150:         //    {
1151:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1152:         //    }
1153: 
1154:         //    await TestSendToIj3TcpAsync();
1155: 
1156:         currentStep = 4; // ไปขั้นที่ 2
1157:         UpdateStepButtons();
1158:         //}
1159:     }
1160: 
1161:     private void UpdateStepButtons()
1162:     {
1163:         // ปุ่ม 1 (MK1+2)
1164:         button5.Enabled = (currentStep == 1);
1165: 
1166:         // ปุ่ม 2 (UV1)
1167:         button6.Enabled = (currentStep == 2);
1168: 
1169:         // ปุ่ม 3 (MK2/3)
1170:         button7.Enabled = (currentStep == 3);
1171: 
1172:         // ปุ่ม 4 (UV2)
1173:         button8.Enabled = (currentStep == 4);
1174: 
1175:         // ถ้าอยากให้เห็นชัดเจนว่าอันไหนเสร็จแล้ว อาจจะเปลี่ยนสีปุ่มด้วยก็ได้
1176:         //button5.BackColor = (currentStep > 1) ? Color.LightGreen : SystemColors.Control;
1177:         // ... ทำแบบเดียวกันกับปุ่มอื่นๆ
1178:     }
1179: 
1180:     private async Task SendToIj3FromDbAsync(string barcode)
1181:     {
1182:         // ตัวแปรสำหรับ Block 1
1183:         string text1 = ""; int x1 = 0, y1 = 0, size1 = 1, scale1 = 1;
1184:         // ตัวแปรสำหรับ Block 2
1185:         string text2 = ""; int x2 = 0, y2 = 0, size2 = 1, scale2 = 1;
1186: 
1187:         string dbPath = @"D:\DB\uv_data.db3";
1188:         using (var conn = new SqliteConnection($"Data Source={dbPath}"))
1189:         {
1190:             await conn.OpenAsync();
1191:             string sql = "SELECT * FROM config_data_mk3 WHERE pattern_no_erp = @barcode LIMIT 1";
1192:             using var cmd = new SqliteCommand(sql, conn);
1193:             cmd.Parameters.AddWithValue("@barcode", barcode);
1194:             using var reader = await cmd.ExecuteReaderAsync();
1195: 
1196:             if (reader.Read())
1197:             {
1198:                 // ดึงข้อมูล Block 1
1199:                 text1 = reader.GetString(reader.GetOrdinal("block1_text"));
1200:                 x1 = reader.GetInt32(reader.GetOrdinal("block1_x"));
1201:                 y1 = reader.GetInt32(reader.GetOrdinal("block1_y"));
1202:                 size1 = reader.GetInt32(reader.GetOrdinal("block1_size"));
1203:                 scale1 = reader.GetInt32(reader.GetOrdinal("block1_scale_side"));
1204: 
1205:                 // ดึงข้อมูล Block 2
1206:                 text2 = reader.IsDBNull(reader.GetOrdinal("block2_text")) ? "" : reader.GetString(reader.GetOrdinal("block2_text"));
1207:                 x2 = reader.GetInt32(reader.GetOrdinal("block2_x"));
1208:                 y2 = reader.GetInt32(reader.GetOrdinal("block2_y"));
1209:                 size2 = reader.GetInt32(reader.GetOrdinal("block2_size"));
1210:                 scale2 = reader.GetInt32(reader.GetOrdinal("block2_scale_side"));
1211:             }
1212:         }
1213: 
1214:         // --- ส่วนการส่ง TCP ---
1215:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
1216:         try
1217:         {
1218:             await client.ConnectAsync(txtTcpHost.Text.Trim(), int.Parse(txtTcpPort.Text.Trim()));
1219:             int fixedProgNo = 13;
1220: 
1221:             // 1. เปลี่ยนโปรแกรมเป็น 13
1222:             await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, fixedProgNo);
1223: 
1224:             // 2. ส่ง Block 1
1225:             var block1 = new InkjetOperator.Services.SimpleTextBlock(1, text1, x1, y1, size1, scale1);
1226:             await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block1);
1227:             await Task.Delay(50); // พักช่วงสั้นๆ ระหว่างการส่งแต่ละ Block
1228: 
1229:             // 3. ส่ง Block 2
1230:             if (!string.IsNullOrEmpty(text2))
1231:             {
1232:                 var block2 = new InkjetOperator.Services.SimpleTextBlock(2, text2, x2, y2, size2, scale2);
1233:                 await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block2);
1234:                 await Task.Delay(50);
1235:             }
1236: 
1237:             // 4. สั่ง Resume
1238:             await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
1239:             Log("Step 3: MK3 (2 Blocks) Sent Successfully.");
1240:         }
1241:         catch (Exception ex)
1242:         {
1243:             Log("Error: " + ex.Message);
1244:         }
1245:     }
1246: 
1247:     private void button9_Click(object sender, EventArgs e)
1248:     {
1249:         string lot = "C200521-001" ?? string.Empty;
1250:         string blockText = "DDDD-01";
1251:         string previewText = PatternEngine.Process(lot, blockText);
1252:         //lblPreview.Text = "Preview: " + previewText;
1253:         Log("Preview: " + previewText);
1254:     }
1255: }
```

## File: frmPatternEditor.cs
```csharp
  1: using System;
  2: using System.ComponentModel;
  3: using System.IO;
  4: using System.Windows.Forms;
  5: using InkjetOperator.Models;
  6: using InkjetOperator.Services;
  7: 
  8: namespace InkjetOperator
  9: {
 10:     public partial class frmPatternEditor : Form
 11:     {
 12:         private BindingList<Pattern>? _patterns;
 13:         private BindingList<Rule>? _currentRules;
 14:         private Pattern? _selectedPattern;
 15: 
 16:         public frmPatternEditor()
 17:         {
 18:             InitializeComponent();
 19:             SetupDataGridView();
 20:             LoadData();
 21:         }
 22: 
 23:         private void SetupDataGridView()
 24:         {
 25:             dgvRules.AutoGenerateColumns = false;
 26:             colFrom.DataPropertyName = "SourceStart";
 27:             colTo.DataPropertyName = "SourceEnd";
 28:             colTransform.DataPropertyName = "TransformRule";
 29:             colParameter.DataPropertyName = "Parameter";
 30: 
 31:             // ตั้งค่า Enum ให้กับ ComboBox ในตาราง
 32:             // แก้ไขส่วนนี้: เพื่อให้ ComboBox แสดงชื่อตาม Description
 33:             colTransform.DataSource = Enum.GetValues(typeof(TransformRuleType))
 34:                 .Cast<TransformRuleType>()
 35:                 .Select(e => new
 36:                 {
 37:                     Value = e,
 38:                     Name = GetEnumDescription(e)
 39:                 }).ToList();
 40:             colTransform.ValueMember = "Value";
 41:             colTransform.DisplayMember = "Name";
 42:         }
 43: 
 44:         // เพิ่มฟังก์ชันช่วยอ่านค่า Description ไว้ข้างล่าง SetupDataGridView
 45:         private string GetEnumDescription(Enum value)
 46:         {
 47:             var field = value.GetType().GetField(value.ToString());
 48:             var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
 49:                                   .FirstOrDefault() as DescriptionAttribute;
 50:             return attribute != null ? attribute.Description : value.ToString();
 51:         }
 52: 
 53:         private void LoadData()
 54:         {
 55:             _patterns = new BindingList<Pattern>(PatternStore.Patterns);
 56:             lstPatterns.DataSource = _patterns;
 57:             lstPatterns.DisplayMember = "Name";
 58:         }
 59: 
 60:         private void lstPatterns_SelectedIndexChanged(object sender, EventArgs e)
 61:         {
 62:             _selectedPattern = lstPatterns.SelectedItem as Pattern;
 63:             if (_selectedPattern == null) return;
 64: 
 65:             // ดึงค่าจาก Object มาแสดง (รวมถึงค่าที่โหลดจาก XML)
 66:             txtPatternName.Text = _selectedPattern.Name;
 67:             txtDescription.Text = _selectedPattern.Description;
 68:             txtBarcodeTest.Text = _selectedPattern.TestBarcode;
 69:             txtBlockText.Text = _selectedPattern.TestBlockText;
 70: 
 71:             _currentRules = new BindingList<Rule>(_selectedPattern.Rules);
 72:             dgvRules.DataSource = _currentRules;
 73: 
 74:             UpdatePreview();
 75:         }
 76: 
 77:         private void UpdatePreview()
 78:         {
 79:             if (_selectedPattern == null) return;
 80: 
 81:             // จำลองการใช้กฎกับ Barcode ที่กรอกในช่อง Lot test
 82:             string result = _selectedPattern.Apply(txtBarcodeTest.Text);
 83: 
 84:             // ถ้าใน Block text มีชื่อ Pattern อยู่ ให้แทนที่ด้วยผลลัพธ์
 85:             if (!string.IsNullOrEmpty(txtPatternName.Text) && txtBlockText.Text.Contains(txtPatternName.Text))
 86:                 lblPreview.Text = txtBlockText.Text.Replace(txtPatternName.Text, result);
 87:             else
 88:                 lblPreview.Text = result;
 89:         }
 90: 
 91:         private void dgvRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
 92:         {
 93:             // ตรวจสอบว่าคลิกที่ปุ่มในคอลัมน์ colDelete หรือไม่
 94:             if (e.RowIndex >= 0 && dgvRules.Columns[e.ColumnIndex].Name == "colDelete")
 95:             {
 96:                 _currentRules?.RemoveAt(e.RowIndex);
 97:                 UpdatePreview();
 98:             }
 99:         }
100: 
101:         private void btnSave_Click(object sender, EventArgs e)
102:         {
103:             if (_selectedPattern != null)
104:             {
105:                 _selectedPattern.Name = txtPatternName.Text;
106:                 _selectedPattern.Description = txtDescription.Text;
107:                 _selectedPattern.TestBarcode = txtBarcodeTest.Text; // บันทึกค่าทดสอบลง XML
108:                 _selectedPattern.TestBlockText = txtBlockText.Text;   // บันทึกค่าทดสอบลง XML
109:             }
110: 
111:             string xmlPath = Path.Combine(Application.StartupPath, "patterns.xml");
112:             PatternStore.Save(xmlPath);
113:             MessageBox.Show("บันทึกข้อมูลรูปแบบและค่าทดสอบลงไฟล์เรียบร้อยแล้ว", "บันทึกสำเร็จ");
114:         }
115: 
116:         private void btnAddRule_Click(object sender, EventArgs e)
117:         {
118:             _currentRules?.Add(new Rule());
119:         }
120: 
121:         private void btnAddPattern_Click(object sender, EventArgs e)
122:         {
123:             var newP = new Pattern { Name = "NEW_PATTERN" };
124:             _patterns?.Add(newP);
125:             lstPatterns.SelectedItem = newP;
126:         }
127: 
128:         private void btnDeletePattern_Click(object sender, EventArgs e)
129:         {
130:             if (_selectedPattern != null && MessageBox.Show("ยืนยันการลบ Pattern นี้?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
131:                 _patterns?.Remove(_selectedPattern);
132:         }
133: 
134:         private void InputChanged(object sender, EventArgs e) => UpdatePreview();
135:         private void dgvRules_CellValueChanged(object sender, DataGridViewCellEventArgs e) => UpdatePreview();
136:     }
137: }
```

## File: InkjetOperator.csproj
```
 1: <Project Sdk="Microsoft.NET.Sdk">
 2: 
 3:   <PropertyGroup>
 4:     <OutputType>WinExe</OutputType>
 5:     <TargetFramework>net8.0-windows</TargetFramework>
 6:     <UseWindowsForms>true</UseWindowsForms>
 7:     <Nullable>enable</Nullable>
 8:     <ImplicitUsings>enable</ImplicitUsings>
 9:   </PropertyGroup>
10: 
11:   <ItemGroup>
12:     <PackageReference Include="System.IO.Ports" Version="8.0.0" />
13:     <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
14:   </ItemGroup>
15: 
16: </Project>
```

## File: Managers/Rs232Manager.cs
```csharp
  1: using System.IO.Ports;
  2: using System.Text;
  3: 
  4: namespace InkjetOperator.Managers;
  5: 
  6: /// <summary>
  7: /// Serial port manager for MK Compact inkjets — follows Linx SerialPortManager pattern.
  8: /// MK Compact uses text-based protocol: send command string, read until '\r'.
  9: /// Ported from rs232_connector.py send_data() (lines 25-34).
 10: /// </summary>
 11: public class Rs232Manager
 12: {
 13:     private SerialPort _serialPort;
 14: 
 15:     public event EventHandler<DataReceivedEventArgs>? DataReceived;
 16: 
 17:     public Rs232Manager()
 18:     {
 19:         _serialPort = new SerialPort();
 20:     }
 21: 
 22:     public void ConfigureSerialPort(string portName, int baudRate = 9600,
 23:         int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None)
 24:     {
 25:         try
 26:         {
 27:             _serialPort.PortName = portName;
 28:             _serialPort.BaudRate = baudRate;
 29:             _serialPort.DataBits = dataBits;
 30:             _serialPort.StopBits = stopBits;
 31:             _serialPort.Parity = parity;
 32:             _serialPort.ReadTimeout = 2000; // 2s timeout, matching Python s.timeout = 2
 33:             _serialPort.Encoding = Encoding.ASCII;
 34:         }
 35:         catch (Exception ex)
 36:         {
 37:             Console.WriteLine("Configuration error: " + ex.Message);
 38:         }
 39:     }
 40: 
 41:     public void OpenSerialPort()
 42:     {
 43:         try
 44:         {
 45:             if (!_serialPort.IsOpen)
 46:             {
 47:                 _serialPort.Open();
 48:             }
 49:         }
 50:         catch (Exception ex)
 51:         {
 52:             MessageBox.Show("Error opening serial port: " + ex.Message, "Error",
 53:                 MessageBoxButtons.OK, MessageBoxIcon.Error);
 54:         }
 55:     }
 56: 
 57:     public void CloseSerialPort()
 58:     {
 59:         try
 60:         {
 61:             if (_serialPort.IsOpen)
 62:             {
 63:                 _serialPort.Close();
 64:             }
 65:         }
 66:         catch (Exception ex)
 67:         {
 68:             Console.WriteLine("Error closing serial port: " + ex.Message);
 69:         }
 70:     }
 71: 
 72:     public bool IsOpen()
 73:     {
 74:         return _serialPort.IsOpen;
 75:     }
 76: 
 77:     /// <summary>
 78:     /// Send a command string and read response until '\r'.
 79:     /// Ported from rs232_connector.py send_data():
 80:     ///   s.write(msg)
 81:     ///   data = s.read_until(expected=b'\r')
 82:     ///   if data == '': return False  # connection problem
 83:     /// </summary>
 84:     public async Task<string> SendCommandAsync(string command)
 85:     {
 86:         if (!_serialPort.IsOpen)
 87:         {
 88:             return "";
 89:         }
 90: 
 91:         try
 92:         {
 93:             byte[] commandBytes = Encoding.ASCII.GetBytes(command);
 94:             _serialPort.Write(commandBytes, 0, commandBytes.Length);
 95:             await Task.Delay(5);
 96: 
 97:             // Read until \r
 98:             string response = "";
 99:             try
100:             {
101:                 response = _serialPort.ReadTo("\r");
102:             }
103:             catch (TimeoutException)
104:             {
105:                 // No response within timeout — connection problem
106:                 return "";
107:             }
108: 
109:             OnDataReceived(response);
110:             return response;
111:         }
112:         catch (Exception ex)
113:         {
114:             Console.WriteLine("Send error: " + ex.Message);
115:             return "";
116:         }
117:     }
118: 
119:     private void OnDataReceived(string data)
120:     {
121:         DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
122:     }
123: }
124: 
125: public class DataReceivedEventArgs : EventArgs
126: {
127:     public string Data { get; private set; }
128:     public DataReceivedEventArgs(string data) { Data = data; }
129: }
```

## File: Managers/TcpManager.cs
```csharp
  1: using System.Net.Sockets;
  2: using System.Text;
  3: 
  4: namespace InkjetOperator.Managers;
  5: 
  6: /// <summary>
  7: /// TCP socket manager — follows Linx TcpClientManager pattern.
  8: /// Used for MK Compact inkjets over TCP (same protocol as RS232, different transport).
  9: /// Ported from socket_client.py.
 10: /// </summary>
 11: public class TcpManager
 12: {
 13:     private TcpClient? _client;
 14:     private NetworkStream? _stream;
 15: 
 16:     private readonly Queue<byte[]> _sendQueue = new();
 17:     private bool _isSending = false;
 18:     private readonly object _sendLock = new();
 19: 
 20:     public event EventHandler<TcpDataReceivedEventArgs>? DataReceived;
 21: 
 22:     public TcpManager() { }
 23: 
 24:     /// <summary>
 25:     /// Connect to TCP endpoint.
 26:     /// From socket_client.py lines 33-49:
 27:     ///   socket.socket(AF_INET, SOCK_STREAM), settimeout(0.1), connect(address)
 28:     /// </summary>
 29:     public async Task ConnectAsync(string ipAddress, int port)
 30:     {
 31:         try
 32:         {
 33:             _client = new TcpClient();
 34:             _client.ReceiveTimeout = 100; // 0.1s timeout matching Python
 35:             _client.SendTimeout = 100;
 36:             await _client.ConnectAsync(ipAddress, port);
 37:             _stream = _client.GetStream();
 38:         }
 39:         catch (Exception ex)
 40:         {
 41:             Console.WriteLine("TCP connection error: " + ex.Message);
 42:             throw;
 43:         }
 44:     }
 45: 
 46:     public void Disconnect()
 47:     {
 48:         try
 49:         {
 50:             _stream?.Close();
 51:             _client?.Close();
 52:             _stream = null;
 53:             _client = null;
 54:         }
 55:         catch (Exception ex)
 56:         {
 57:             Console.WriteLine("TCP disconnect error: " + ex.Message);
 58:         }
 59:     }
 60: 
 61:     public bool IsConnected()
 62:     {
 63:         return _client?.Connected ?? false;
 64:     }
 65: 
 66:     /// <summary>
 67:     /// Send command and receive response.
 68:     /// From socket_client.py: sock.sendall(msg), data = sock.recv(16)
 69:     /// Uses queue pattern from Linx TcpClientManager to prevent send collisions.
 70:     /// </summary>
 71:     public async Task<string> SendCommandAsync(string command)
 72:     {
 73:         if (_stream == null || !IsConnected())
 74:         {
 75:             return "";
 76:         }
 77: 
 78:         byte[] commandBytes = Encoding.ASCII.GetBytes(command);
 79: 
 80:         lock (_sendLock)
 81:         {
 82:             _sendQueue.Enqueue(commandBytes);
 83:         }
 84: 
 85:         return await ProcessSendQueueAsync();
 86:     }
 87: 
 88:     private async Task<string> ProcessSendQueueAsync()
 89:     {
 90:         if (_isSending) return "";
 91:         _isSending = true;
 92: 
 93:         string lastResponse = "";
 94: 
 95:         try
 96:         {
 97:             while (true)
 98:             {
 99:                 byte[] cmd;
100:                 lock (_sendLock)
101:                 {
102:                     if (_sendQueue.Count == 0) break;
103:                     cmd = _sendQueue.Dequeue();
104:                 }
105: 
106:                 await _stream!.WriteAsync(cmd, 0, cmd.Length);
107:                 await _stream.FlushAsync();
108: 
109:                 // Read response (recv(16) in Python)
110:                 byte[] buffer = new byte[16];
111:                 try
112:                 {
113:                     int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
114:                     if (bytesRead > 0)
115:                     {
116:                         lastResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\r', '\n');
117:                         OnDataReceived(lastResponse);
118:                     }
119:                 }
120:                 catch (IOException)
121:                 {
122:                     // Read timeout — no response
123:                     lastResponse = "";
124:                 }
125: 
126:                 await Task.Delay(50); // Prevent rapid succession (Linx pattern)
127:             }
128:         }
129:         finally
130:         {
131:             _isSending = false;
132:         }
133: 
134:         return lastResponse;
135:     }
136: 
137:     private void OnDataReceived(string data)
138:     {
139:         DataReceived?.Invoke(this, new TcpDataReceivedEventArgs(data));
140:     }
141: }
142: 
143: public class TcpDataReceivedEventArgs : EventArgs
144: {
145:     public string Data { get; private set; }
146:     public TcpDataReceivedEventArgs(string data) { Data = data; }
147: }
```

## File: Models/ApiModels.cs
```csharp
  1: using System.Text.Json.Serialization;
  2: 
  3: namespace InkjetOperator.Models;
  4: 
  5: /// <summary>
  6: /// Standard backend response wrapper — matches ResponseManager { statusCode, data }.
  7: /// </summary>
  8: public class ApiResponse<T>
  9: {
 10:     [JsonPropertyName("statusCode")]
 11:     public int StatusCode { get; set; }
 12: 
 13:     [JsonPropertyName("data")]
 14:     public T? Data { get; set; }
 15: }
 16: 
 17: public class PaginatedResult<T>
 18: {
 19:     [JsonPropertyName("data")]
 20:     public List<T> Data { get; set; } = new();
 21: 
 22:     [JsonPropertyName("total")]
 23:     public int Total { get; set; }
 24: }
 25: 
 26: // --- Job models ---
 27: 
 28: public class PrintJob
 29: {
 30:     [JsonPropertyName("id")]
 31:     public int Id { get; set; }
 32: 
 33:     [JsonPropertyName("barcode_raw")]
 34:     public string BarcodeRaw { get; set; } = "";
 35: 
 36:     [JsonPropertyName("pattern_id")]
 37:     public int? PatternId { get; set; }
 38: 
 39:     [JsonPropertyName("lot_number")]
 40:     public string? LotNumber { get; set; }
 41: 
 42:     [JsonPropertyName("status")]
 43:     public string Status { get; set; } = "pending";
 44: 
 45:     [JsonPropertyName("error_message")]
 46:     public string? ErrorMessage { get; set; }
 47: 
 48:     [JsonPropertyName("created_by")]
 49:     public string? CreatedBy { get; set; }
 50: 
 51:     [JsonPropertyName("attempt")]
 52:     public int Attempt { get; set; }
 53: 
 54:     [JsonPropertyName("warning")]
 55:     public string? Warning { get; set; }
 56: }
 57: 
 58: // --- Pattern models (from GET /job/getResolved) ---
 59: 
 60: public class ResolvedJobResponse
 61: {
 62:     [JsonPropertyName("job")]
 63:     public PrintJob Job { get; set; } = new();
 64: 
 65:     [JsonPropertyName("pattern")]
 66:     public PatternDetail Pattern { get; set; } = new();
 67: }
 68: 
 69: public class PatternDetail
 70: {
 71:     [JsonPropertyName("id")]
 72:     public int Id { get; set; }
 73: 
 74:     [JsonPropertyName("name")]
 75:     public string Name { get; set; } = "";
 76: 
 77:     [JsonPropertyName("barcode")]
 78:     public string Barcode { get; set; } = "";
 79: 
 80:     [JsonPropertyName("description")]
 81:     public string? Description { get; set; }
 82: 
 83:     [JsonPropertyName("inkjet_configs")]
 84:     public List<InkjetConfigDto> InkjetConfigs { get; set; } = new();
 85: 
 86:     [JsonPropertyName("conveyor_speeds")]
 87:     public ConveyorSpeedDto? ConveyorSpeeds { get; set; }
 88: 
 89:     [JsonPropertyName("servo_configs")]
 90:     public List<ServoConfigDto> ServoConfigs { get; set; } = new();
 91: }
 92: 
 93: public class InkjetConfigDto
 94: {
 95:     [JsonPropertyName("ordinal")]
 96:     public int Ordinal { get; set; }
 97: 
 98:     [JsonPropertyName("program_number")]
 99:     public int? ProgramNumber { get; set; }
100: 
101:     [JsonPropertyName("program_name")]
102:     public string? ProgramName { get; set; }
103: 
104:     [JsonPropertyName("width")]
105:     public int? Width { get; set; }
106: 
107:     [JsonPropertyName("height")]
108:     public int? Height { get; set; }
109: 
110:     [JsonPropertyName("trigger_delay")]
111:     public int? TriggerDelay { get; set; }
112: 
113:     [JsonPropertyName("direction")]
114:     public int? Direction { get; set; }
115: 
116:     [JsonPropertyName("steel_type")]
117:     public string? SteelType { get; set; }
118: 
119:     [JsonPropertyName("suspended")]
120:     public bool Suspended { get; set; }
121: 
122:     [JsonPropertyName("text_blocks")]
123:     public List<TextBlockDto> TextBlocks { get; set; } = new();
124: }
125: 
126: public class TextBlockDto
127: {
128:     [JsonPropertyName("block_number")]
129:     public int BlockNumber { get; set; }
130: 
131:     [JsonPropertyName("text")]
132:     public string? Text { get; set; }
133: 
134:     [JsonPropertyName("x")]
135:     public int? X { get; set; }
136: 
137:     [JsonPropertyName("y")]
138:     public int? Y { get; set; }
139: 
140:     [JsonPropertyName("size")]
141:     public int? Size { get; set; }
142: 
143:     [JsonPropertyName("scale")]
144:     public int? Scale { get; set; }
145: 
146:     // เพิ่มบรรทัดนี้ (บรรทัดที่ 50 โดยประมาณ)
147:     public string RuleResult { get; set; }
148: }
149: 
150: public class ConveyorSpeedDto
151: {
152:     [JsonPropertyName("speed1")]
153:     public int? Speed1 { get; set; }
154: 
155:     [JsonPropertyName("speed2")]
156:     public int? Speed2 { get; set; }
157: 
158:     [JsonPropertyName("speed3")]
159:     public int? Speed3 { get; set; }
160: }
161: 
162: public class ServoConfigDto
163: {
164:     [JsonPropertyName("ordinal")]
165:     public int Ordinal { get; set; }
166: 
167:     [JsonPropertyName("position")]
168:     public int? Position { get; set; }
169: 
170:     [JsonPropertyName("post_act")]
171:     public int? PostAct { get; set; }
172: 
173:     [JsonPropertyName("delay")]
174:     public int? Delay { get; set; }
175: 
176:     [JsonPropertyName("trigger")]
177:     public int? Trigger { get; set; }
178: }
179: 
180: // --- Command result (POST /job/postResults) ---
181: 
182: public class JobResultsPayload
183: {
184:     [JsonPropertyName("success")]
185:     public bool Success { get; set; }
186: 
187:     [JsonPropertyName("error_message")]
188:     public string? ErrorMessage { get; set; }
189: 
190:     [JsonPropertyName("commands")]
191:     public List<CommandResult> Commands { get; set; } = new();
192: }
193: 
194: public class CommandResult
195: {
196:     [JsonPropertyName("ordinal")]
197:     public int? Ordinal { get; set; }
198: 
199:     [JsonPropertyName("command")]
200:     public string Command { get; set; } = "";
201: 
202:     [JsonPropertyName("payload")]
203:     public Dictionary<string, object>? Payload { get; set; }
204: 
205:     [JsonPropertyName("response")]
206:     public string? Response { get; set; }
207: 
208:     [JsonPropertyName("success")]
209:     public bool Success { get; set; }
210: 
211:     [JsonPropertyName("sent_at")]
212:     public string? SentAt { get; set; }
213: }
214: 
215: // เพิ่ม DTO สำหรับสร้าง job
216: public class CreateJobRequest
217: {
218:     [JsonPropertyName("barcode_raw")]
219:     public string BarcodeRaw { get; set; }
220: 
221:     [JsonPropertyName("created_by")]
222:     public string CreatedBy { get; set; }
223: 
224:     [JsonPropertyName("order_no")]
225:     public string OrderNo { get; set; }
226: 
227:     [JsonPropertyName("customer_name")]
228:     public string CustomerName { get; set; }
229: 
230:     [JsonPropertyName("type")]
231:     public string Type { get; set; }
232: 
233:     [JsonPropertyName("qty")]
234:     public int Qty { get; set; }
235: }
```

## File: Models/JobRow.cs
```csharp
 1: using System;
 2: using System.Collections.Generic;
 3: using System.Linq;
 4: using System.Text;
 5: using System.Threading.Tasks;
 6: 
 7: namespace InkjetOperator.Models
 8: {
 9:     public class JobRow
10:     {
11:         public int Id { get; set; }
12:         public string BarcodeRaw { get; set; }
13:         public string LotNumber { get; set; }
14:         public string Status { get; set; }
15:         public int Attempt { get; set; }
16:     }
17: }
```

## File: Models/Pattern.cs
```csharp
 1: using System.Collections.Generic;
 2: 
 3: namespace InkjetOperator.Models
 4: {
 5:     public class Pattern
 6:     {
 7:         public string Name { get; set; }
 8:         public string Description { get; set; }
 9:         public List<Rule> Rules { get; set; }
10:         public string TestBarcode { get; set; }
11:         public string TestBlockText { get; set; }
12:         public string TestPreview { get; set; }
13: 
14:         public Pattern()
15:         {
16:             Rules = new List<Rule>();
17:             TestBarcode = string.Empty;
18:             TestBlockText = string.Empty;
19:             TestPreview = string.Empty;
20:         }
21: 
22:         public string Apply(string input)
23:         {
24:             input = input ?? string.Empty;
25:             var parts = new List<string>();
26:             foreach (var r in Rules)
27:             {
28:                 try { parts.Add(r.Apply(input)); }
29:                 catch { parts.Add(string.Empty); }
30:             }
31:             return string.Concat(parts);
32:         }
33:     }
34: }
```

## File: Models/Rule.cs
```csharp
 1: using System;
 2: 
 3: namespace InkjetOperator.Models
 4: {
 5:     public class Rule
 6:     {
 7:         public int SourceStart { get; set; }
 8:         public int SourceEnd { get; set; }
 9:         public TransformRuleType TransformRule { get; set; }
10:         public string Parameter { get; set; }
11:         public bool IsActive { get; set; }
12: 
13:         public Rule() { Parameter = string.Empty; IsActive = true; }
14: 
15:         public string Apply(string input)
16:         {
17:             if (!IsActive) return string.Empty;
18:             input = input ?? string.Empty;
19:             string ext = Extract(input);
20:             switch (TransformRule)
21:             {
22:                 case TransformRuleType.DELETE: return string.Empty;
23:                 case TransformRuleType.FIX_TEXT: return Parameter ?? string.Empty;
24:                 case TransformRuleType.COPY: return ext;
25:                 case TransformRuleType.PAD_LEFT: return (Parameter ?? "") + ext;
26:                 case TransformRuleType.PAD_RIGHT: return ext + (Parameter ?? "");
27:                 case TransformRuleType.AZ_LOWER: return SwapAZ(ext, true);
28:                 case TransformRuleType.AZ_UPPER: return SwapAZ(ext, false);
29:                 case TransformRuleType.TAKE_RIGHT: return TakeRight(ext);
30:                 case TransformRuleType.TAKE_LEFT: return TakeLeft(ext);
31:                 default: return ext;
32:             }
33:         }
34: 
35:         private string Extract(string input)
36:         {
37:             int s = Math.Max(1, SourceStart);
38:             int e = Math.Max(0, SourceEnd);
39:             if (s > e || s > input.Length) return string.Empty;
40:             int idx = s - 1;
41:             int len = Math.Min(e, input.Length) - idx;
42:             return (len > 0) ? input.Substring(idx, len) : string.Empty;
43:         }
44: 
45:         private string SwapAZ(string ext, bool lower)
46:         {
47:             int num;
48:             if (!int.TryParse(ext, out num)) return string.Empty;
49:             int baseVal = 0;
50:             if (!string.IsNullOrEmpty(Parameter)) int.TryParse(Parameter, out baseVal);
51:             int offset = num - baseVal;
52:             if (offset < 0) return string.Empty;
53:             string az = OffsetToAZ(offset);
54:             return lower ? az.ToLower() : az.ToUpper();
55:         }
56: 
57:         private string TakeRight(string s)
58:         {
59:             int n;
60:             if (!int.TryParse(Parameter, out n) || n <= 0) return s;
61:             return n >= s.Length ? s : s.Substring(s.Length - n);
62:         }
63: 
64:         private string TakeLeft(string s)
65:         {
66:             int n;
67:             if (!int.TryParse(Parameter, out n) || n <= 0) return s;
68:             return n >= s.Length ? s : s.Substring(0, n);
69:         }
70: 
71:         private static string OffsetToAZ(int offset)
72:         {
73:             int n = offset + 1; string s = string.Empty;
74:             while (n > 0) { n--; s = (char)('A' + (n % 26)) + s; n /= 26; }
75:             return s;
76:         }
77:     }
78: }
```

## File: Models/TransformRuleType.cs
```csharp
 1: using System.ComponentModel;
 2: 
 3: namespace InkjetOperator.Models
 4: {
 5:     public enum TransformRuleType
 6:     {
 7:         [Description("Delete")] // เพิ่มบรรทัดนี้
 8:         DELETE,
 9:         FIX_TEXT,
10:         COPY,
11:         [Description("Keep + Pad Left")] // เพิ่มบรรทัดนี้
12:         PAD_LEFT,
13:         [Description("Keep + Pad Right")] // เพิ่มบรรทัดนี้
14:         PAD_RIGHT,
15:         [Description("Swap a-z")] // เพิ่มบรรทัดนี้
16:         AZ_LOWER,
17:         [Description("Swap A-Z")] // เพิ่มบรรทัดนี้
18:         AZ_UPPER,
19:         TAKE_RIGHT,
20:         TAKE_LEFT
21:     }
22: }
```

## File: Models/UvRow.cs
```csharp
 1: using System;
 2: using System.Collections.Generic;
 3: using System.Linq;
 4: using System.Text;
 5: using System.Threading.Tasks;
 6: 
 7: namespace InkjetOperator.Models
 8: {
 9:     public class UvRow
10:     {
11:         public int Id { get; set; }
12:         public string Inkjet { get; set; } = "";
13:         public string Lot { get; set; } = "";
14:         public string Name { get; set; } = "";
15:         public string UpdateAt { get; set; } = "";
16:     }
17: }
```

## File: PlcAdapter/PlcManager.cs
```csharp
 1: using System.Net.Sockets;
 2: using System.Text;
 3: 
 4: namespace InkjetOperator.PlcAdapter;
 5: 
 6: /// <summary>
 7: /// Stub PLC manager — TCP connection to PLC for servo + conveyor speed.
 8: /// Command format TBD from subcontractor.
 9: /// Replaces plc_interface.py Modbus write_multiple_registers.
10: /// </summary>
11: public class PlcManager
12: {
13:     private TcpClient? _client;
14:     private NetworkStream? _stream;
15: 
16:     public async Task ConnectAsync(string host, int port)
17:     {
18:         try
19:         {
20:             _client = new TcpClient();
21:             await _client.ConnectAsync(host, port);
22:             _stream = _client.GetStream();
23:         }
24:         catch (Exception ex)
25:         {
26:             Console.WriteLine("PLC connection error: " + ex.Message);
27:             throw;
28:         }
29:     }
30: 
31:     public void Disconnect()
32:     {
33:         try
34:         {
35:             _stream?.Close();
36:             _client?.Close();
37:             _stream = null;
38:             _client = null;
39:         }
40:         catch (Exception ex)
41:         {
42:             Console.WriteLine("PLC disconnect error: " + ex.Message);
43:         }
44:     }
45: 
46:     public bool IsConnected()
47:     {
48:         return _client?.Connected ?? false;
49:     }
50: 
51:     /// <summary>
52:     /// Write servo parameters to PLC.
53:     /// From plc_interface.py write_servo(): writes 4 registers (position, post_act, delay, trigger).
54:     /// Command format TBD — currently stubbed.
55:     /// </summary>
56:     public Task<bool> WriteServoAsync(int ordinal, int position, int postAct, int delay, int trigger)
57:     {
58:         // TODO: Implement when PLC TCP command format is provided by subcontractor
59:         Console.WriteLine($"PLC WriteServo stub: ordinal={ordinal} pos={position} postAct={postAct} delay={delay} trigger={trigger}");
60:         return Task.FromResult(false);
61:     }
62: 
63:     /// <summary>
64:     /// Write conveyor speed to PLC.
65:     /// From plc_interface.py write_speed(): writes 3 registers (speed1, speed2, speed3).
66:     /// Command format TBD — currently stubbed.
67:     /// </summary>
68:     public Task<bool> WriteSpeedAsync(int speed1, int speed2, int speed3)
69:     {
70:         // TODO: Implement when PLC TCP command format is provided by subcontractor
71:         Console.WriteLine($"PLC WriteSpeed stub: s1={speed1} s2={speed2} s3={speed3}");
72:         return Task.FromResult(false);
73:     }
74: }
```

## File: Program.cs
```csharp
 1: using System.Runtime.InteropServices;
 2: using InkjetOperator.Services;
 3: 
 4: namespace InkjetOperator;
 5: 
 6: static class Program
 7: {
 8:     [DllImport("user32.dll")]
 9:     private static extern bool SetProcessDPIAware();
10:     [STAThread]
11:     static void Main()
12:     {
13:         SetProcessDPIAware();
14:         Application.EnableVisualStyles();
15:         Application.SetCompatibleTextRenderingDefault(false);
16:         // โหลด patterns (สร้าง default ถ้ายังไม่มี)
17:         string xmlPath = Path.Combine(Application.StartupPath, "patterns.xml");
18:         PatternStore.SeedDefaults(xmlPath); // สร้างไฟล์ + default patterns ถ้ายังไม่มี
19:         PatternStore.Load(xmlPath);         // โหลด patterns จาก XML เข้า memory
20: 
21:         // ถ้า Load ล้มเหลว (XML เสีย → ถูกลบ) สร้าง + โหลดใหม่
22:         if (PatternStore.Patterns.Count == 0)
23:         {
24:             PatternStore.SeedDefaults(xmlPath);
25:             PatternStore.Load(xmlPath);
26:         }
27: 
28:         Application.Run(new frmMain());
29:     }
30: }
```

## File: Services/ApiClient.cs
```csharp
  1: using System.Net.Http.Json;
  2: using System.Text.Json;
  3: using InkjetOperator.Models;
  4: 
  5: namespace InkjetOperator.Services;
  6: 
  7: /// <summary>
  8: /// HTTP client for the InkjetBackend REST API.
  9: /// Polls for pending jobs, fetches resolved patterns, posts results.
 10: /// </summary>
 11: public class ApiClient
 12: {
 13:     private readonly HttpClient _http;
 14:     private readonly string _baseUrl;
 15: 
 16:     private static readonly JsonSerializerOptions JsonOptions = new()
 17:     {
 18:         PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
 19:         PropertyNameCaseInsensitive = true,
 20:     };
 21: 
 22:     public ApiClient(string baseUrl)
 23:     {
 24:         _baseUrl = baseUrl.TrimEnd('/');
 25:         _http = new HttpClient
 26:         {
 27:             BaseAddress = new Uri(_baseUrl),
 28:             Timeout = TimeSpan.FromSeconds(10),
 29:         };
 30:     }
 31: 
 32:     /// <summary>GET /job/getAll?status=pending — polled every 5s by frmMain timer.</summary>
 33:     public async Task<List<PrintJob>> GetPendingJobsAsync()
 34:     {
 35:         try
 36:         {
 37:             var response = await _http.GetAsync("/job/getAll?status=pending");
 38:             response.EnsureSuccessStatusCode();
 39: 
 40:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PrintJob>>>(JsonOptions);
 41:             return wrapper?.Data?.Data ?? new List<PrintJob>();
 42:         }
 43:         catch (Exception ex)
 44:         {
 45:             Console.WriteLine("GetPendingJobs error: " + ex.Message);
 46:             return new List<PrintJob>();
 47:         }
 48:     }
 49: 
 50:     /// <summary>GET /job/getById/:id</summary>
 51:     public async Task<PrintJob?> GetJobByIdAsync(int jobId)
 52:     {
 53:         try
 54:         {
 55:             var response = await _http.GetAsync($"/job/getById/{jobId}");
 56:             response.EnsureSuccessStatusCode();
 57: 
 58:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
 59:             return wrapper?.Data;
 60:         }
 61:         catch (Exception ex)
 62:         {
 63:             Console.WriteLine("GetJobById error: " + ex.Message);
 64:             return null;
 65:         }
 66:     }
 67: 
 68:     /// <summary>GET /job/getResolved/:id — returns job + pattern with resolved templates.</summary>
 69:     public async Task<ResolvedJobResponse?> GetResolvedJobAsync(int jobId)
 70:     {
 71:         try
 72:         {
 73:             var response = await _http.GetAsync($"/job/getResolved/{jobId}");
 74:             response.EnsureSuccessStatusCode();
 75: 
 76:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<ResolvedJobResponse>>(JsonOptions);
 77:             return wrapper?.Data;
 78:         }
 79:         catch (Exception ex)
 80:         {
 81:             Console.WriteLine("GetResolvedJob error: " + ex.Message);
 82:             return null;
 83:         }
 84:     }
 85: 
 86:     /// <summary>POST /job/execute/:id — marks job as executing.</summary>
 87:     public async Task<bool> ExecuteJobAsync(int jobId)
 88:     {
 89:         try
 90:         {
 91:             var response = await _http.PostAsync($"/job/execute/{jobId}", null);
 92:             response.EnsureSuccessStatusCode();
 93:             return true;
 94:         }
 95:         catch (Exception ex)
 96:         {
 97:             Console.WriteLine("ExecuteJob error: " + ex.Message);
 98:             return false;
 99:         }
100:     }
101: 
102:     /// <summary>POST /job/postResults/:id — posts command results back to backend.</summary>
103:     public async Task<bool> PostResultsAsync(int jobId, JobResultsPayload results)
104:     {
105:         try
106:         {
107:             var response = await _http.PostAsJsonAsync($"/job/postResults/{jobId}", results, JsonOptions);
108:             response.EnsureSuccessStatusCode();
109:             return true;
110:         }
111:         catch (Exception ex)
112:         {
113:             Console.WriteLine("PostResults error: " + ex.Message);
114:             return false;
115:         }
116:     }
117: 
118:     /// <summary>POST /job/retry/:id — resets failed job to pending.</summary>
119:     public async Task<bool> RetryJobAsync(int jobId)
120:     {
121:         try
122:         {
123:             var response = await _http.PostAsync($"/job/retry/{jobId}", null);
124:             response.EnsureSuccessStatusCode();
125:             return true;
126:         }
127:         catch (Exception ex)
128:         {
129:             Console.WriteLine("RetryJob error: " + ex.Message);
130:             return false;
131:         }
132:     }
133: 
134:     /// <summary>POST /job/create — create new job (with improved error logging).</summary>
135:     public async Task<PrintJob?> CreateJobAsync(CreateJobRequest newJob)
136:     {
137:         try
138:         {
139:             var response = await _http.PostAsJsonAsync("/job/create", newJob, JsonOptions);
140: 
141:             if (!response.IsSuccessStatusCode)
142:             {
143:                 string body = await response.Content.ReadAsStringAsync();
144:                 Console.WriteLine($"CreateJob failed: {(int)response.StatusCode} {response.ReasonPhrase} - Body: {body}");
145:                 return null;
146:             }
147: 
148:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
149:             return wrapper?.Data;
150:         }
151:         catch (Exception ex)
152:         {
153:             Console.WriteLine("CreateJob error: " + ex.Message);
154:             return null;
155:         }
156:     }
157: }
```

## File: Services/BotClickHelper.cs
```csharp
  1: using System;
  2: using System.Diagnostics;
  3: using System.Drawing;
  4: using System.Drawing.Imaging;
  5: using System.Runtime.InteropServices;
  6: using System.Threading;
  7: using System.Windows.Forms;
  8: 
  9: /// <summary>
 10: /// BotClick — คลิกอัตโนมัติ + verify ด้วยการเทียบภาพ
 11: /// ใช้สำหรับควบคุมโปรแกรม uvinkjet ผ่าน UI automation
 12: /// Ported จาก BotClickApp.Form1 (โค้ดเก่า)
 13: ///
 14: /// ระบบ Screen Capture:
 15: ///   • CaptureScreen() — แคปทั้งจอ primary monitor
 16: ///   • CaptureArea()   — แคปเฉพาะ area ที่สนใจ
 17: ///   • CaptureTargetWindow() — แคปเฉพาะหน้าต่างเป้าหมาย
 18: ///   • เปรียบเทียบภาพ before/after เพื่อ retry หาก UI ไม่เปลี่ยน
 19: ///   • บันทึก log เป็นรูปภาพลง capture_log/
 20: /// </summary>
 21: public static class BotClickHelper
 22: {
 23:     // ─── Win32 API ───
 24: 
 25:     [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
 26:     [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
 27:     [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr hWnd);
 28:     [DllImport("user32.dll")] static extern void mouse_event(int f, int x, int y, int d, int e);
 29:     [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
 30:     [DllImport("user32.dll")] static extern bool GetCursorInfo(ref CURSORINFO pci);
 31:     [DllImport("user32.dll")] static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop,
 32:         IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);
 33: 
 34:     const int SW_MAXIMIZE = 3;
 35:     const int MOUSE_DOWN = 0x02, MOUSE_UP = 0x04;
 36:     const int CURSOR_SHOWING = 0x00000001;
 37:     const int DI_NORMAL = 0x0003;
 38: 
 39:     [StructLayout(LayoutKind.Sequential)]
 40:     private struct CURSORINFO
 41:     {
 42:         public int cbSize;
 43:         public int flags;
 44:         public IntPtr hCursor;
 45:         public Point ptScreenPos;
 46:     }
 47: 
 48:     [StructLayout(LayoutKind.Sequential)]
 49:     private struct RECT
 50:     {
 51:         public int Left, Top, Right, Bottom;
 52:         public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
 53:     }
 54: 
 55:     private static IntPtr _targetHWnd = IntPtr.Zero;
 56: 
 57:     /// <summary>
 58:     /// ภาพล่าสุดที่แคปได้ — ให้ form ดึงไปแสดงใน PictureBox ได้
 59:     /// (เหมือน pictureBox1.Image = img ในโค้ดเก่า)
 60:     /// </summary>
 61:     public static Bitmap LastCapture { get; private set; }
 62: 
 63:     // ─── Public API ───
 64: 
 65:     /// <summary>
 66:     /// รันทั้ง bot flow: sleep 3s → activate → click steps ตามลำดับ
 67:     /// เรียกจาก background thread เท่านั้น
 68:     /// (ตรงตามโค้ดเก่า RunBot())
 69:     /// </summary>
 70:     public static BotResult Run(string processName, BotStep[] steps)
 71:     {
 72:         Thread.Sleep(3000); // รอ UI พร้อม (เหมือนโค้ดเก่า)
 73: 
 74:         if (!ActivateWindow(processName, out string error))
 75:             return BotResult.Fail(error);
 76: 
 77:         Thread.Sleep(2000); // รอ UI พร้อม
 78: 
 79:         foreach (var step in steps)
 80:         {
 81:             if (!ClickAndVerify(step))
 82:                 return BotResult.Fail($"{step.Name} failed after {step.MaxRetry} retries");
 83:             Thread.Sleep(step.DelayAfter);
 84:         }
 85: 
 86:         return BotResult.Ok();
 87:     }
 88: 
 89:     /// <summary>
 90:     /// รัน bot flow แบบ async — ไม่บล็อก UI thread
 91:     /// </summary>
 92:     public static void RunAsync(string processName, BotStep[] steps, Action<BotResult> onComplete = null)
 93:     {
 94:         var t = new Thread(() =>
 95:         {
 96:             var result = Run(processName, steps);
 97:             onComplete?.Invoke(result);
 98:         })
 99:         { IsBackground = true };
100:         t.Start();
101:     }
102: 
103:     // ─── Screen Capture ───
104: 
105:     /// <summary>
106:     /// แคปทั้งจอ primary monitor พร้อมวาด cursor ลงภาพ
107:     /// </summary>
108:     public static Bitmap CaptureScreen()
109:     {
110:         Rectangle bounds = Screen.PrimaryScreen.Bounds;
111:         var bmp = new Bitmap(bounds.Width, bounds.Height);
112:         using (var g = Graphics.FromImage(bmp))
113:         {
114:             g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
115:             DrawCursor(g, bounds.Location);
116:         }
117:         return bmp;
118:     }
119: 
120:     /// <summary>
121:     /// แคปเฉพาะ area ที่กำหนด (screen coordinates) พร้อมวาด cursor ถ้าอยู่ใน area
122:     /// </summary>
123:     public static Bitmap CaptureArea(Rectangle area)
124:     {
125:         var bmp = new Bitmap(area.Width, area.Height);
126:         using (var g = Graphics.FromImage(bmp))
127:         {
128:             g.CopyFromScreen(area.Location, Point.Empty, area.Size);
129:             DrawCursor(g, area.Location);
130:         }
131:         return bmp;
132:     }
133: 
134:     /// <summary>
135:     /// แคปเฉพาะหน้าต่างโปรแกรมเป้าหมาย (ไม่รวม desktop/taskbar) พร้อมวาด cursor
136:     /// </summary>
137:     public static Bitmap CaptureTargetWindow()
138:     {
139:         if (_targetHWnd == IntPtr.Zero)
140:             return null;
141: 
142:         if (!GetWindowRect(_targetHWnd, out RECT rect))
143:             return null;
144: 
145:         var windowRect = rect.ToRectangle();
146:         if (windowRect.Width <= 0 || windowRect.Height <= 0)
147:             return null;
148: 
149:         var bmp = new Bitmap(windowRect.Width, windowRect.Height);
150:         using (var g = Graphics.FromImage(bmp))
151:         {
152:             g.CopyFromScreen(windowRect.Location, Point.Empty, windowRect.Size);
153:             DrawCursor(g, windowRect.Location);
154:         }
155:         return bmp;
156:     }
157: 
158:     // ─── Core ───
159: 
160:     private static bool ActivateWindow(string processName, out string error)
161:     {
162:         error = null;
163:         var procs = Process.GetProcessesByName(processName);
164:         if (procs.Length == 0) { error = $"Process '{processName}' not found"; return false; }
165: 
166:         IntPtr hWnd = procs[0].MainWindowHandle;
167:         if (hWnd == IntPtr.Zero) { error = "Window handle not found"; return false; }
168: 
169:         _targetHWnd = hWnd;
170: 
171:         ShowWindow(hWnd, SW_MAXIMIZE);
172:         BringWindowToTop(hWnd);
173:         SetForegroundWindow(hWnd);
174:         return true;
175:     }
176: 
177:     /// <summary>
178:     /// Click → compare before/after area → SaveCapture ทั้งจอ → retry ถ้าภาพไม่เปลี่ยน
179:     /// (ตรงตามโค้ดเก่า ClickAndVerify())
180:     /// </summary>
181:     private static bool ClickAndVerify(BotStep step)
182:     {
183:         for (int i = 1; i <= step.MaxRetry; i++)
184:         {
185:             Console.WriteLine($"[{step.Name}] Try {i}");
186: 
187:             // แคป area ก่อน click
188:             Bitmap before = CaptureArea(step.VerifyArea);
189: 
190:             LeftClick(step.X, step.Y);
191: 
192:             Thread.Sleep(step.VerifyDelay);
193: 
194:             // แคป area หลัง click
195:             Bitmap after = CaptureArea(step.VerifyArea);
196: 
197:             // เปรียบเทียบแบบ exact (pixel ต่างเมื่อไหร่ = เปลี่ยน)
198:             bool same = CompareBitmapsFast(before, after);
199: 
200:             // save ทั้งจอลง capture_log (เหมือนโค้ดเก่า)
201:             SaveCapture($"{step.Name}_Try{i}");
202: 
203:             before.Dispose();
204:             after.Dispose();
205: 
206:             if (!same)
207:             {
208:                 Console.WriteLine($"[{step.Name}] ✅ Success");
209:                 return true;
210:             }
211: 
212:             Console.WriteLine($"[{step.Name}] ❌ Retry...");
213:             Thread.Sleep(1000);
214:         }
215: 
216:         Console.WriteLine($"[{step.Name}] ❌ Failed");
217:         return false;
218:     }
219: 
220:     /// <summary>
221:     /// เปรียบเทียบ pixel แบบ fast — เหมือนโค้ดเก่า CompareBitmapsFast()
222:     /// สุ่มทุก 5 pixel, pixel ต่างตัวเดียว = return false (ภาพเปลี่ยน)
223:     /// </summary>
224:     public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
225:     {
226:         if (bmp1.Size != bmp2.Size)
227:             return false;
228: 
229:         for (int x = 0; x < bmp1.Width; x += 5)
230:             for (int y = 0; y < bmp1.Height; y += 5)
231:                 if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
232:                     return false;
233: 
234:         return true;
235:     }
236: 
237:     /// <summary>
238:     /// แคปทั้งจอ → บันทึกลง capture_log/ → เก็บ LastCapture
239:     /// (เหมือนโค้ดเก่า SaveCapture() ที่เรียก CaptureScreen + pictureBox1.Image = img)
240:     /// </summary>
241:     private static void SaveCapture(string stepName)
242:     {
243:         Bitmap img = CaptureScreen();
244: 
245:         var folder = System.IO.Path.Combine(Application.StartupPath, "capture_log");
246:         System.IO.Directory.CreateDirectory(folder);
247: 
248:         var path = System.IO.Path.Combine(
249:             folder, $"{stepName}_{DateTime.Now:HHmmss}.png");
250:         img.Save(path, ImageFormat.Png);
251: 
252:         // เก็บภาพล่าสุดให้ form ดึงไปแสดง
253:         LastCapture?.Dispose();
254:         LastCapture = img;
255:     }
256: 
257:     private static void LeftClick(int x, int y)
258:     {
259:         Cursor.Position = new Point(x, y);
260:         Thread.Sleep(200);
261:         mouse_event(MOUSE_DOWN, x, y, 0, 0);
262:         Thread.Sleep(100);
263:         mouse_event(MOUSE_UP, x, y, 0, 0);
264:     }
265: 
266:     /// <summary>
267:     /// วาด cursor ปัจจุบันลงบน Graphics (ตำแหน่ง offset จาก captureOrigin)
268:     /// </summary>
269:     private static void DrawCursor(Graphics g, Point captureOrigin)
270:     {
271:         var ci = new CURSORINFO();
272:         ci.cbSize = Marshal.SizeOf(ci);
273: 
274:         if (!GetCursorInfo(ref ci))
275:             return;
276: 
277:         if ((ci.flags & CURSOR_SHOWING) == 0)
278:             return;
279: 
280:         IntPtr hdc = g.GetHdc();
281:         try
282:         {
283:             int x = ci.ptScreenPos.X - captureOrigin.X;
284:             int y = ci.ptScreenPos.Y - captureOrigin.Y;
285:             DrawIconEx(hdc, x, y, ci.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
286:         }
287:         finally
288:         {
289:             g.ReleaseHdc(hdc);
290:         }
291:     }
292: }
293: 
294: // ─── Models ───
295: 
296: public class BotStep
297: {
298:     public string Name { get; set; }
299:     public int X { get; set; }
300:     public int Y { get; set; }
301:     public Rectangle VerifyArea { get; set; }
302:     public int MaxRetry { get; set; } = 3;
303:     public int VerifyDelay { get; set; } = 1500;
304:     public int DelayAfter { get; set; } = 1000;
305: }
306: 
307: public class BotResult
308: {
309:     public bool Success { get; set; }
310:     public string Error { get; set; }
311: 
312:     public static BotResult Ok() => new BotResult { Success = true };
313:     public static BotResult Fail(string err) => new BotResult { Success = false, Error = err };
314: }
```

## File: Services/PatternEngine.cs
```csharp
 1: using InkjetOperator.Models;
 2: 
 3: namespace InkjetOperator.Services
 4: {
 5:     public static class PatternEngine
 6:     {
 7:         public static string Process(string barcode, string blockText)
 8:         {
 9:             if (string.IsNullOrEmpty(barcode)) return blockText ?? string.Empty;
10:             if (string.IsNullOrEmpty(blockText)) return string.Empty;
11:             foreach (var p in PatternStore.Patterns)
12:             {
13:                 if (!string.IsNullOrEmpty(p.Name) && blockText.Contains(p.Name))
14:                     return blockText.Replace(p.Name, p.Apply(barcode));
15:             }
16:             return blockText;
17:         }
18: 
19:         public static string[] ProcessBlocks(string barcode, string[] blockTexts)
20:         {
21:             if (blockTexts == null) return new string[0];
22:             var results = new string[blockTexts.Length];
23:             for (int i = 0; i < blockTexts.Length; i++)
24:                 results[i] = Process(barcode, blockTexts[i]);
25:             return results;
26:         }
27:     }
28: }
```

## File: Services/PatternStore.cs
```csharp
 1: using System.Collections.Generic;
 2: using System.IO;
 3: using System.Xml.Serialization;
 4: using InkjetOperator.Models;
 5: 
 6: namespace InkjetOperator.Services
 7: {
 8:     public static class PatternStore
 9:     {
10:         public static List<Pattern> Patterns { get; } = new List<Pattern>();
11: 
12:         public static void Save(string path)
13:         {
14:             var ser = new XmlSerializer(typeof(List<Pattern>));
15:             using (var fs = new FileStream(path, FileMode.Create))
16:                 ser.Serialize(fs, Patterns);
17:         }
18: 
19:         public static void Load(string path)
20:         {
21:             if (!File.Exists(path)) return;
22:             try
23:             {
24:                 var ser = new XmlSerializer(typeof(List<Pattern>));
25:                 using (var fs = new FileStream(path, FileMode.Open))
26:                 {
27:                     var list = (List<Pattern>)ser.Deserialize(fs);
28:                     Patterns.Clear();
29:                     Patterns.AddRange(list);
30:                 }
31:             }
32:             catch
33:             {
34:                 // XML เสีย — ลบแล้วให้ SeedDefaults สร้างใหม่
35:                 try { File.Delete(path); } catch { }
36:             }
37:         }
38: 
39:         /// <summary>สร้าง default CCCC + DDDD ถ้ายังไม่มีไฟล์ (ตรงกับ XML จริง)</summary>
40:         public static void SeedDefaults(string path)
41:         {
42:             if (File.Exists(path)) return;
43: 
44:             // CCCC: copy barcode ทั้งตัว
45:             var cccc = new Pattern
46:             {
47:                 Name = "CCCC",
48:                 TestBarcode = "C240801-027",
49:                 TestBlockText = "CCCC-01 CPI291",
50:                 TestPreview = "C240801-027-01 CPI291",
51:             };
52:             cccc.Rules.Add(new Rule { SourceStart = 1, SourceEnd = 999, TransformRule = TransformRuleType.COPY });
53:             Patterns.Add(cccc);
54: 
55:             // DDDD: C200521-001 → FE21-01
56:             var dddd = new Pattern
57:             {
58:                 Name = "DDDD",
59:                 TestBarcode = "C200521-001",
60:                 TestBlockText = "DDDD-01",
61:                 TestPreview = "FE21-01-01",
62:             };
63:             dddd.Rules.AddRange(new[]
64:             {
65:                 new Rule { SourceStart = 1,  SourceEnd = 1,  TransformRule = TransformRuleType.DELETE },
66:                 new Rule { SourceStart = 2,  SourceEnd = 3,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "15" },
67:                 new Rule { SourceStart = 4,  SourceEnd = 5,  TransformRule = TransformRuleType.AZ_UPPER, Parameter = "1" },
68:                 new Rule { SourceStart = 6,  SourceEnd = 7,  TransformRule = TransformRuleType.COPY },
69:                 new Rule { SourceStart = 8,  SourceEnd = 8,  TransformRule = TransformRuleType.COPY },
70:                 new Rule { SourceStart = 9,  SourceEnd = 11, TransformRule = TransformRuleType.TAKE_RIGHT, Parameter = "2" },
71:             });
72:             Patterns.Add(dddd);
73: 
74:             Save(path);
75:         }
76:     }
77: }
```
