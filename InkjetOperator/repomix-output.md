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
frmSt3.cs
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
  57:         StartWatcher();
  58: 
  59:         _adapterIj1 = new MkCompactAdapter(_rs232Ij1);
  60:         _adapterIj2 = new MkCompactAdapter(_rs232Ij2);
  61:         _adapterIj3 = new SqliteInkjetAdapter(_tcpIj3, ""); // db path TBD
  62:         _adapterIj4 = new SqliteInkjetAdapter(_tcpIj4, ""); // db path TBD
  63: 
  64:         // เพิ่มบรรทัดนี้เพื่อให้ dgvUVBlocks สร้างคอลัมน์ตาม properties ของ TextBlockDto
  65:         dgvUVBlocks.AutoGenerateColumns = true;
  66:         dgvTextBlocks.AutoGenerateColumns = true;
  67: 
  68:         _api = new ApiClient("http://localhost:3000");
  69:     }
  70: 
  71:     // ════════════════════════════════════════
  72:     //  Form events
  73:     // ════════════════════════════════════════
  74: 
  75:     private void StartWatcher()
  76:     {
  77:         string watchPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\document";
  78: 
  79:         FileSystemWatcher watcher = new FileSystemWatcher(watchPath);
  80:         watcher.Filter = "*.uvdx";
  81:         watcher.Created += OnFileCreated;
  82:         watcher.EnableRaisingEvents = true;
  83:     }
  84: 
  85:     private void OnFileCreated(object sender, FileSystemEventArgs e)
  86:     {
  87:         string fileName = Path.GetFileName(e.FullPath);
  88: 
  89:         Thread.Sleep(1000); // กันไฟล์ยัง copy ไม่เสร็จ
  90: 
  91:         BotClickHelper.CopyFile(fileName);
  92: 
  93:         var steps = new[]
  94:         {
  95:         new BotStep { Name = "Document", X = 2325, Y = 59 },
  96:         new BotStep { Name = "Open", X = 2207, Y = 133 },
  97:         new BotStep { Name = "SelectFile", X = 890, Y = 525 },
  98:         new BotStep { Name = "OpenBtn", X = 1652, Y = 946 },
  99:     };
 100: 
 101:         BotClickHelper.RunAsync("uvinkjet", steps, result =>
 102:         {
 103:             BotClickHelper.ClearDocumentFolder();
 104:         });
 105:     }
 106: 
 107:     private async void frmMain_Load(object sender, EventArgs e)
 108:     {
 109:         InitializeDatabase();
 110:         currentStep = 1;
 111:         UpdateStepButtons();
 112:         // Populate COM port dropdowns
 113:         string[] ports = SerialPort.GetPortNames();
 114:         cmbCom1.Items.AddRange(ports);
 115:         cmbCom2.Items.AddRange(ports);
 116:         if (cmbCom1.Items.Count > 0) cmbCom1.SelectedIndex = 0;
 117:         if (cmbCom2.Items.Count > 1) cmbCom2.SelectedIndex = 1;
 118:         else if (cmbCom2.Items.Count > 0) cmbCom2.SelectedIndex = 0;
 119: 
 120:         //await SetupUvTableAsync();
 121:         await LoadUvDataToGrid();
 122: 
 123:         dgvUVBlocks.CellValueChanged += dgvUVBlocks_CellValueChanged;
 124: 
 125:         dgvUVBlocks.CurrentCellDirtyStateChanged += (s, e) =>
 126:         {
 127:             if (dgvUVBlocks.IsCurrentCellDirty)
 128:             {
 129:                 dgvUVBlocks.CommitEdit(DataGridViewDataErrorContexts.Commit);
 130:             }
 131:         };
 132: 
 133:         dgvJobs.AutoGenerateColumns = true;
 134:         dgvJobs.DataSource = _jobBindingList;
 135:         dgvConfigs.DataSource = _configBindingList;
 136:         dgvTextBlocks.DataSource = _textBlockBindingList;
 137:         dgvUVBlocks.AutoGenerateColumns = false;
 138:         dgvUVBlocks.DataSource = _uvBindingList;
 139: 
 140:         dgvUVBlocks.Columns.Clear();
 141: 
 142:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 143:         {
 144:             HeaderText = "Inkjet",
 145:             DataPropertyName = "Inkjet",
 146:             ReadOnly = true,
 147:             Width = 120
 148:         });
 149: 
 150:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 151:         {
 152:             HeaderText = "Lot",
 153:             DataPropertyName = "Lot",
 154:             Width = 150
 155:         });
 156: 
 157:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 158:         {
 159:             HeaderText = "Name",
 160:             DataPropertyName = "Name",
 161:             AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
 162:         });
 163: 
 164:         dgvUVBlocks.Columns.Add(new DataGridViewTextBoxColumn
 165:         {
 166:             HeaderText = "Program",
 167:             DataPropertyName = "Program",
 168:             AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
 169:         });
 170: 
 171:         tmrPoll.Start();
 172:         Log("Application started. Polling every 5s.");
 173:     }
 174: 
 175:     private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
 176:     {
 177:         tmrPoll.Stop();
 178:         _rs232Ij1.CloseSerialPort();
 179:         _rs232Ij2.CloseSerialPort();
 180:         _tcpIj3.Disconnect();
 181:         _tcpIj4.Disconnect();
 182:         _plc.Disconnect();
 183:     }
 184: 
 185:     // ════════════════════════════════════════
 186:     //  Connection handlers
 187:     // ════════════════════════════════════════
 188: 
 189:     private void btnConnectRs232_Click(object sender, EventArgs e)
 190:     {
 191:         try
 192:         {
 193:             if (cmbCom1.SelectedItem != null)
 194:             {
 195:                 _rs232Ij1.ConfigureSerialPort(cmbCom1.SelectedItem.ToString()!);
 196:                 _rs232Ij1.OpenSerialPort();
 197:             }
 198:             if (cmbCom2.SelectedItem != null)
 199:             {
 200:                 _rs232Ij2.ConfigureSerialPort(cmbCom2.SelectedItem.ToString()!);
 201:                 _rs232Ij2.OpenSerialPort();
 202:             }
 203:             UpdateDeviceStatus();
 204:             Log("RS232 connected.");
 205:         }
 206:         catch (Exception ex)
 207:         {
 208:             Log("RS232 connect error: " + ex.Message);
 209:         }
 210:     }
 211: 
 212:     private void btnDisconnectRs232_Click(object sender, EventArgs e)
 213:     {
 214:         _rs232Ij1.CloseSerialPort();
 215:         _rs232Ij2.CloseSerialPort();
 216:         UpdateDeviceStatus();
 217:         Log("RS232 disconnected.");
 218:     }
 219: 
 220:     private async void btnConnectTcp_Click(object sender, EventArgs e)
 221:     {
 222:         try
 223:         {
 224:             string host = txtTcpHost.Text.Trim();
 225:             int port = int.Parse(txtTcpPort.Text.Trim());
 226:             await _tcpIj3.ConnectAsync(host, port);
 227:             await _tcpIj4.ConnectAsync(host, port + 1); // IJ4 on next port
 228:             UpdateDeviceStatus();
 229:             Log("TCP connected.");
 230:         }
 231:         catch (Exception ex)
 232:         {
 233:             Log("TCP connect error: " + ex.Message);
 234:         }
 235:     }
 236: 
 237:     private void btnDisconnectTcp_Click(object sender, EventArgs e)
 238:     {
 239:         _tcpIj3.Disconnect();
 240:         _tcpIj4.Disconnect();
 241:         UpdateDeviceStatus();
 242:         Log("TCP disconnected.");
 243:     }
 244: 
 245:     private void btnApplyApi_Click(object sender, EventArgs e)
 246:     {
 247:         string url = txtApiUrl.Text.Trim();
 248:         _api = new ApiClient(url);
 249:         lblApiStatus.Text = "Applied";
 250:         Log($"API URL set to: {url}");
 251:     }
 252: 
 253:     // ════════════════════════════════════════
 254:     //  Polling
 255:     // ════════════════════════════════════════
 256: 
 257:     private async void tmrPoll_Tick(object sender, EventArgs e)
 258:     {
 259:         tmrPoll.Stop(); // Prevent re-entry
 260:         try
 261:         {
 262:             var jobs = await _api.GetPendingJobsAsync();
 263:             _pendingJobs = jobs;
 264:             UpdateJobGrid();
 265:             LoadLastSentData();
 266:             UpdateDeviceStatus();
 267:             lblApiStatus.Text = "OK";
 268:         }
 269:         catch (Exception ex)
 270:         {
 271:             lblApiStatus.Text = "Error";
 272:             Log("Poll error: " + ex.Message);
 273:         }
 274:         finally
 275:         {
 276:             tmrPoll.Start();
 277:         }
 278:     }
 279: 
 280:     private void btnRefresh_Click(object sender, EventArgs e)
 281:     {
 282:         tmrPoll_Tick(sender, e);
 283:     }
 284: 
 285:     // ════════════════════════════════════════
 286:     //  Job selection
 287:     // ════════════════════════════════════════
 288:     private async void dgvJobs_SelectionChanged(object sender, EventArgs e)
 289:     {
 290:         if (_isRefreshingJobs) return; // 🔥 กันตอน poll
 291: 
 292:         if (dgvJobs.SelectedRows.Count == 0) return;
 293: 
 294:         var row = dgvJobs.SelectedRows[0];
 295:         if (row.Cells["Id"].Value == null) return;
 296: 
 297:         int jobId = (int)row.Cells["Id"].Value;
 298:         string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
 299: 
 300:         if (jobId == _selectedJobId) return;
 301:         _selectedJobId = jobId;
 302: 
 303:         // Helpers
 304:         static bool ReaderHasColumn(SqliteDataReader r, string name)
 305:         {
 306:             for (int i = 0; i < r.FieldCount; i++)
 307:             {
 308:                 if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
 309:                     return true;
 310:             }
 311:             return false;
 312:         }
 313: 
 314:         static string? GetStringSafe(SqliteDataReader r, string name)
 315:         {
 316:             if (!ReaderHasColumn(r, name)) return null;
 317:             int idx = r.GetOrdinal(name);
 318:             return r.IsDBNull(idx) ? null : r.GetString(idx);
 319:         }
 320: 
 321:         static int? GetIntSafe(SqliteDataReader r, string name)
 322:         {
 323:             var s = GetStringSafe(r, name);
 324:             if (string.IsNullOrWhiteSpace(s)) return null;
 325:             return int.TryParse(s, out var v) ? v : null;
 326:         }
 327: 
 328:         try
 329:         {
 330:             string dbPath = @"D:\DB\uv_data.db3";
 331:             string connStr = $"Data Source={dbPath}";
 332:             bool found = false;
 333: 
 334:             try
 335:             {
 336:                 await using var conn = new SqliteConnection(connStr);
 337:                 await conn.OpenAsync();
 338: 
 339:                 await using var cmd = conn.CreateCommand();
 340:                 cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
 341:                 cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
 342: 
 343:                 await using var reader = await cmd.ExecuteReaderAsync();
 344:                 if (await reader.ReadAsync())
 345:                 {
 346:                     // Build PatternDetail from known schema columns.
 347:                     var pattern = new PatternDetail
 348:                     {
 349:                         Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
 350:                         Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
 351:                     };
 352: 
 353:                     // Helper to build MK config (mk1 / mk2)
 354:                     InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
 355:                     {
 356:                         var cfg = new InkjetConfigDto
 357:                         {
 358:                             Ordinal = ordinal,
 359:                             ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
 360:                             ProgramName = GetStringSafe(reader, programNameColumn),
 361:                             Width = GetIntSafe(reader, $"{prefix}width"),
 362:                             Height = GetIntSafe(reader, $"{prefix}height"),
 363:                             TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
 364:                             Direction = GetIntSafe(reader, $"{prefix}text_direction"),
 365:                             SteelType = null,
 366:                             Suspended = false
 367:                         };
 368: 
 369:                         // text blocks 1..5
 370:                         for (int b = 1; b <= 5; b++)
 371:                         {
 372:                             string textCol = $"{prefix}block{b}_text";
 373:                             if (ReaderHasColumn(reader, textCol))
 374:                             {
 375:                                 var text = GetStringSafe(reader, textCol);
 376:                                 if (!string.IsNullOrEmpty(text))
 377:                                 {
 378:                                     var tb = new TextBlockDto
 379:                                     {
 380:                                         BlockNumber = b,
 381:                                         Text = text,
 382:                                         X = GetIntSafe(reader, $"{prefix}block{b}_x"),
 383:                                         Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
 384:                                         Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
 385:                                         Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
 386:                                     };
 387:                                     cfg.TextBlocks.Add(tb);
 388:                                 }
 389:                             }
 390:                         }
 391: 
 392:                         return cfg;
 393:                     }
 394: 
 395:                     // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
 396:                     var mk1 = BuildMkConfig("mk1_", 1, "program_name");
 397:                     // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
 398:                     var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
 399: 
 400:                     pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
 401: 
 402:                     // Conveyor speeds / servo configs - attempt to map if available (optional)
 403:                     var spd1 = GetIntSafe(reader, "belt1_inkjet");
 404:                     var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
 405:                     if (spd1.HasValue || spd2.HasValue)
 406:                     {
 407:                         pattern.ConveyorSpeeds = new ConveyorSpeedDto
 408:                         {
 409:                             Speed1 = spd1,
 410:                             Speed2 = spd2,
 411:                             Speed3 = GetIntSafe(reader, "belt3")
 412:                         };
 413:                     }
 414: 
 415:                     // Minimal job + resolved response
 416:                     _currentResolved = new ResolvedJobResponse
 417:                     {
 418:                         Job = new PrintJob
 419:                         {
 420:                             Id = jobId,
 421:                             BarcodeRaw = rawbarcode,
 422:                             LotNumber = row.Cells["LotNumber"].Value?.ToString(),
 423:                             Status = row.Cells["Status"].Value?.ToString()
 424:                         },
 425:                         Pattern = pattern
 426:                     };
 427: 
 428:                     string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
 429:                     string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ
 430: 
 431:                     // 🔥 background + กันค้าง
 432:                     //_ = Task.Run(async () =>
 433:                     //{
 434:                     //    try
 435:                     //    {
 436:                     //        await UpdateUvData(lot, name);
 437:                     //    }
 438:                     //    catch (Exception ex)
 439:                     //    {
 440:                     //        Log("UV DB error: " + ex.Message);
 441:                     //    }
 442:                     //});
 443: 
 444:                     found = true;
 445:                     UpdateDetailPanel();
 446:                 }
 447: 
 448:                 await conn.CloseAsync();
 449:             }
 450:             catch (Exception dbEx)
 451:             {
 452:                 Log("SQLite query error: " + dbEx.Message);
 453:             }
 454: 
 455:             if (!found)
 456:             {
 457:                 var resolved = await _api.GetResolvedJobAsync(jobId);
 458:                 _currentResolved = resolved;
 459:                 UpdateDetailPanel();
 460:             }
 461:         }
 462:         catch (Exception ex)
 463:         {
 464:             Log("Load job detail error: " + ex.Message);
 465:         }
 466:     }
 467: 
 468:     private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
 469:     {
 470:         if (dgvConfigs.SelectedRows.Count == 0 || _currentResolved?.Pattern == null) return;
 471: 
 472:         int idx = dgvConfigs.SelectedRows[0].Index;
 473:         var configs = _currentResolved.Pattern.InkjetConfigs;
 474: 
 475:         if (configs == null || idx >= configs.Count) return;
 476: 
 477:         var config = configs[idx];
 478: 
 479:         // dgvTextBlocks แสดงตามแถวที่เลือก (ปกติคือ MK1, MK2)
 480:         //dgvTextBlocks.DataSource = null;
 481:         _textBlockBindingList.RaiseListChangedEvents = false;
 482:         _textBlockBindingList.Clear();
 483: 
 484:         //foreach (var b in config.TextBlocks ?? new List<TextBlockDto>())
 485:         //{
 486:         //    _textBlockBindingList.Add(b);
 487:         //}
 488: 
 489:         foreach (var block in config.TextBlocks)
 490:         {
 491:             // --- ส่วนที่แก้ไข/เพิ่มใหม่ ---
 492:             // ใช้ LINQ ค้นหา Pattern ที่ชื่อตรงกับ block.Text (เช่น "DDDD")
 493:             //var pattern = PatternEngine.Patterns.FirstOrDefault(p => p.Name == block.Text);
 494:             string previewText = PatternEngine.Process(txtLot.Text, block.Text);
 495:             //Debug.WriteLine(block.Text);
 496: 
 497:             if (previewText != null)
 498:             {
 499:                 // ถ้าเจอ ให้เอาค่าจาก txtLot (หรือ res.Job.LotNumber) มาแปลงด้วย Rule
 500:                 block.RuleResult = previewText;
 501:             }
 502:             else
 503:             {
 504:                 // ถ้าไม่เจอ ให้แสดงค่าเดิมของมัน
 505:                 block.RuleResult = block.Text;
 506:             }
 507:             // --------------------------
 508: 
 509:             _textBlockBindingList.Add(block);
 510:         }
 511: 
 512:         _textBlockBindingList.RaiseListChangedEvents = true;
 513:         _textBlockBindingList.ResetBindings();
 514: 
 515:         // --- นำ UpdateUvGridOnly(config.TextBlocks) ออก ---
 516:         // เพื่อให้ dgvUVBlocks ไม่เปลี่ยนค่าตามการเลือกใน Grid นี้
 517:     }
 518: 
 519:     // ════════════════════════════════════════
 520:     //  Send to devices
 521:     // ════════════════════════════════════════
 522: 
 523:     private async void btnSend_Click(object sender, EventArgs e)
 524:     {
 525:         if (_currentResolved == null || _selectedJobId < 0)
 526:         {
 527:             MessageBox.Show("No job selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
 528:             return;
 529:         }
 530: 
 531:         btnSend.Enabled = false;
 532:         var commandResults = new List<CommandResult>();
 533: 
 534:         try
 535:         {
 536:             // Mark job as executing
 537:             bool ok = await _api.ExecuteJobAsync(_selectedJobId);
 538:             if (!ok)
 539:             {
 540:                 Log("Failed to mark job as executing.");
 541:                 btnSend.Enabled = true;
 542:                 return;
 543:             }
 544: 
 545:             // Re-fetch resolved data (templates may depend on attempt number)
 546:             var resolved = await _api.GetResolvedJobAsync(_selectedJobId);
 547:             if (resolved?.Pattern == null)
 548:             {
 549:                 Log("Failed to get resolved job.");
 550:                 btnSend.Enabled = true;
 551:                 return;
 552:             }
 553: 
 554:             _currentResolved = resolved;
 555:             UpdateDetailPanel();
 556: 
 557:             // Send to each inkjet by ordinal
 558:             var configs = resolved.Pattern.InkjetConfigs ?? new List<InkjetConfigDto>();
 559:             bool hasError = false;
 560: 
 561:             foreach (var config in configs.OrderBy(c => c.Ordinal))
 562:             {
 563:                 if (config.Suspended)
 564:                 {
 565:                     Log($"IJ{config.Ordinal} suspended — skipping.");
 566:                     continue;
 567:                 }
 568: 
 569:                 IInkjetAdapter adapter = GetAdapterByOrdinal(config.Ordinal);
 570: 
 571:                 if (!adapter.IsConnected())
 572:                 {
 573:                     Log($"IJ{config.Ordinal} not connected — skipping.");
 574:                     commandResults.Add(new CommandResult
 575:                     {
 576:                         Ordinal = config.Ordinal,
 577:                         Command = "connect_check",
 578:                         Success = false,
 579:                         Response = "Not connected",
 580:                         SentAt = DateTime.UtcNow.ToString("o"),
 581:                     });
 582:                     hasError = true;
 583:                     break; // Sequential error — stop (csv_extractor.py line 281)
 584:                 }
 585: 
 586:                 // 1. Change program
 587:                 if (config.ProgramNumber.HasValue)
 588:                 {
 589:                     var r = await adapter.ChangeProgramAsync(config.ProgramNumber.Value);
 590:                     r.Ordinal = config.Ordinal;
 591:                     commandResults.Add(r);
 592:                     Log($"IJ{config.Ordinal} ChangeProgram({config.ProgramNumber}): {(r.Success ? "OK" : "FAIL")}");
 593: 
 594:                     if (!r.Success) { hasError = true; break; }
 595:                 }
 596: 
 597:                 // 2. Send config
 598:                 var cfgResult = await adapter.SendConfigAsync(config);
 599:                 cfgResult.Ordinal = config.Ordinal;
 600:                 commandResults.Add(cfgResult);
 601:                 Log($"IJ{config.Ordinal} SendConfig: {(cfgResult.Success ? "OK" : "FAIL")}");
 602: 
 603:                 if (!cfgResult.Success) { hasError = true; break; }
 604: 
 605:                 // 3. Send text blocks (device blocks 6-10)
 606:                 var blocks = config.TextBlocks ?? new List<TextBlockDto>();
 607:                 foreach (var block in blocks.OrderBy(b => b.BlockNumber))
 608:                 {
 609:                     int deviceBlock = block.BlockNumber + 5; // blocks 6-10
 610:                     var tbResult = await adapter.SendTextBlockAsync(block, deviceBlock);
 611:                     tbResult.Ordinal = config.Ordinal;
 612:                     commandResults.Add(tbResult);
 613:                     Log($"IJ{config.Ordinal} TextBlock({block.BlockNumber}): {(tbResult.Success ? "OK" : "FAIL")}");
 614: 
 615:                     if (!tbResult.Success) { hasError = true; break; }
 616:                     await Task.Delay(30); // 30ms delay between blocks (csv_extractor.py line 274)
 617:                 }
 618: 
 619:                 if (hasError) break;
 620: 
 621:                 // 4. Resume printing
 622:                 var resumeResult = await adapter.ResumeAsync();
 623:                 resumeResult.Ordinal = config.Ordinal;
 624:                 commandResults.Add(resumeResult);
 625:                 Log($"IJ{config.Ordinal} Resume: {(resumeResult.Success ? "OK" : "FAIL")}");
 626: 
 627:                 if (!resumeResult.Success) { hasError = true; break; }
 628:             }
 629: 
 630:             // Send conveyor speed + servo to PLC
 631:             if (!hasError && resolved.Pattern.ConveyorSpeeds != null)
 632:             {
 633:                 var spd = resolved.Pattern.ConveyorSpeeds;
 634:                 await _plc.WriteSpeedAsync(spd.Speed1 ?? 0, spd.Speed2 ?? 0, spd.Speed3 ?? 0);
 635:                 Log($"PLC speed: {spd.Speed1}/{spd.Speed2}/{spd.Speed3}");
 636:             }
 637: 
 638:             if (!hasError && resolved.Pattern.ServoConfigs != null)
 639:             {
 640:                 foreach (var servo in resolved.Pattern.ServoConfigs)
 641:                 {
 642:                     await _plc.WriteServoAsync(servo.Ordinal, servo.Position ?? 0, servo.PostAct ?? 0, servo.Delay ?? 0, servo.Trigger ?? 0);
 643:                     Log($"PLC servo ordinal={servo.Ordinal}: pos={servo.Position}");
 644:                 }
 645:             }
 646: 
 647:             // Post results back to backend
 648:             var payload = new JobResultsPayload
 649:             {
 650:                 Success = !hasError,
 651:                 ErrorMessage = hasError ? "One or more commands failed" : null,
 652:                 Commands = commandResults,
 653:             };
 654: 
 655:             await _api.PostResultsAsync(_selectedJobId, payload);
 656:             Log(hasError ? "Job completed with errors." : "Job completed successfully.");
 657:         }
 658:         catch (Exception ex)
 659:         {
 660:             Log("Send error: " + ex.Message);
 661:         }
 662:         finally
 663:         {
 664:             btnSend.Enabled = true;
 665:         }
 666:     }
 667: 
 668:     private async void btnRetry_Click(object sender, EventArgs e)
 669:     {
 670:         if (_selectedJobId < 0) return;
 671: 
 672:         bool ok = await _api.RetryJobAsync(_selectedJobId);
 673:         Log(ok ? $"Job {_selectedJobId} retried." : $"Retry failed for job {_selectedJobId}.");
 674:     }
 675: 
 676:     // ════════════════════════════════════════
 677:     //  Helpers
 678:     // ════════════════════════════════════════
 679: 
 680:     private IInkjetAdapter GetAdapterByOrdinal(int ordinal)
 681:     {
 682:         return ordinal switch
 683:         {
 684:             1 => _adapterIj1,
 685:             2 => _adapterIj2,
 686:             3 => _adapterIj3,
 687:             4 => _adapterIj4,
 688:             _ => _adapterIj1,
 689:         };
 690:     }
 691: 
 692:     private void UpdateDeviceStatus()
 693:     {
 694:         if (InvokeRequired) { Invoke(UpdateDeviceStatus); return; }
 695: 
 696:         lblStatusIj1.BackColor = _adapterIj1.IsConnected() ? Color.Green : Color.Gray;
 697:         lblStatusIj2.BackColor = _adapterIj2.IsConnected() ? Color.Green : Color.Gray;
 698:         lblStatusIj3.BackColor = _adapterIj3.IsConnected() ? Color.Green : Color.Gray;
 699:         lblStatusIj4.BackColor = _adapterIj4.IsConnected() ? Color.Green : Color.Gray;
 700:     }
 701: 
 702:     private void UpdateJobGrid()
 703:     {
 704:         if (InvokeRequired) { Invoke(UpdateJobGrid); return; }
 705: 
 706:         _isRefreshingJobs = true; // 🔥 กัน event
 707:         _selectedJobId = -1;
 708:         int selectedId = -1;
 709:         if (dgvJobs.CurrentRow != null)
 710:         {
 711:             selectedId = (int)dgvJobs.CurrentRow.Cells["Id"].Value;
 712:         }
 713: 
 714:         _jobBindingList.RaiseListChangedEvents = false;
 715:         _jobBindingList.Clear();
 716: 
 717:         foreach (var j in _pendingJobs)
 718:         {
 719:             _jobBindingList.Add(new JobRow
 720:             {
 721:                 Id = j.Id,
 722:                 BarcodeRaw = j.BarcodeRaw,
 723:                 LotNumber = j.LotNumber,
 724:                 Status = j.Status,
 725:                 Attempt = j.Attempt
 726:             });
 727:         }
 728: 
 729:         _jobBindingList.RaiseListChangedEvents = true;
 730:         _jobBindingList.ResetBindings();
 731: 
 732:         // 🔥 restore selection
 733:         if (selectedId != -1)
 734:         {
 735:             foreach (DataGridViewRow row in dgvJobs.Rows)
 736:             {
 737:                 if ((int)row.Cells["Id"].Value == selectedId)
 738:                 {
 739:                     row.Selected = true;
 740:                     dgvJobs.CurrentCell = row.Cells[0];
 741:                     break;
 742:                 }
 743:             }
 744:         }
 745: 
 746:         _isRefreshingJobs = false; // 🔥 เปิด event กลับ
 747:     }
 748: 
 749:     private void UpdateDetailPanel()
 750:     {
 751:         if (InvokeRequired) { Invoke(UpdateDetailPanel); return; }
 752: 
 753:         if (_currentResolved == null) return;
 754: 
 755:         var job = _currentResolved.Job;
 756:         var pattern = _currentResolved.Pattern;
 757: 
 758:         txtBarcode.Text = job?.BarcodeRaw ?? "";
 759:         txtLot.Text = job?.LotNumber ?? "";
 760:         txtStatus.Text = job?.Status ?? "";
 761:         txtPattern.Text = pattern?.Name ?? "";
 762: 
 763:         _configBindingList.RaiseListChangedEvents = false;
 764: 
 765:         var newList = pattern?.InkjetConfigs ?? new List<InkjetConfigDto>();
 766: 
 767:         // 🔥 sync count
 768:         while (_configBindingList.Count > newList.Count)
 769:         {
 770:             _configBindingList.RemoveAt(_configBindingList.Count - 1);
 771:         }
 772: 
 773:         for (int i = 0; i < newList.Count; i++)
 774:         {
 775:             if (i < _configBindingList.Count)
 776:             {
 777:                 // 🔥 update object reference (สำคัญ)
 778:                 _configBindingList[i] = newList[i];
 779:             }
 780:             else
 781:             {
 782:                 _configBindingList.Add(newList[i]);
 783:             }
 784:         }
 785: 
 786:         _configBindingList.RaiseListChangedEvents = true;
 787:         _configBindingList.ResetBindings();
 788: 
 789:         // --- เพิ่มส่วนนี้: Fix ให้ dgvUVBlocks แสดงเฉพาะ UV1 (Ordinal 3) และ UV2 (Ordinal 4) ---
 790:         if (pattern?.InkjetConfigs != null)
 791:         {
 792:             // ดึงเฉพาะ Config ของ UV (Ordinal 3 และ 4)
 793:             var uvConfigs = pattern.InkjetConfigs
 794:                 .Where(c => c.Ordinal == 3 || c.Ordinal == 4)
 795:                 .OrderBy(c => c.Ordinal)
 796:                 .ToList();
 797: 
 798:             // สร้างรายการสำหรับ Display ใน Grid
 799:             var uvDisplayList = new List<object>();
 800:             foreach (var cfg in uvConfigs)
 801:             {
 802:                 string printerName = cfg.Ordinal == 3 ? "เครื่องพิมพ์ UV1" : "เครื่องพิมพ์ UV2";
 803: 
 804:                 // ดึงค่า Block 1 และ 2 (Lot และ Name)
 805:                 string lotVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 1)?.Text ?? "";
 806:                 string nameVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 2)?.Text ?? "";
 807: 
 808:                 uvDisplayList.Add(new
 809:                 {
 810:                     Printer = printerName,
 811:                     LotText = lotVal,
 812:                     NameText = nameVal,
 813:                     // เก็บ Reference ไว้เผื่อต้องการดึงไปใช้งานต่อ
 814:                     _originalConfig = cfg
 815:                 });
 816:             }
 817:         }
 818:     }
 819: 
 820:     private void Log(string message)
 821:     {
 822:         if (InvokeRequired) { Invoke(() => Log(message)); return; }
 823: 
 824:         string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
 825:         txtLog.AppendText(line + Environment.NewLine);
 826:     }
 827: 
 828:     private void button1_Click(object sender, EventArgs e)
 829:     {
 830:         string fileName = "SPK-LOT.uvdx"; // 🔹 เปลี่ยนเป็นค่าที่คุณต้องการ (อาจมาจาก textbox)
 831: 
 832:         var steps = new[]
 833:         {
 834:         new BotStep { Name = "Document",   X = 2325, Y = 59,  VerifyArea = new Rectangle(2100, 0, 400, 300) },
 835:         new BotStep { Name = "Open",       X = 2207, Y = 133, VerifyArea = new Rectangle(2100, 100, 400, 400) },
 836:         new BotStep { Name = "SelectFile", X = 890, Y = 525, VerifyArea = new Rectangle(800, 400, 800, 600) },
 837:         new BotStep { Name = "OpenBtn",    X = 1652, Y = 946, VerifyArea = new Rectangle(1500, 800, 500, 300) },
 838:     };
 839: 
 840:         // ✅ 1. copy file ก่อนเริ่ม
 841:         BotClickHelper.CopyFile(fileName);
 842: 
 843:         // ✅ 2. run bot
 844:         BotClickHelper.RunAsync("uvinkjet", steps, result =>
 845:         {
 846:             // ✅ 3. ลบไฟล์ทุกครั้ง (สำเร็จ/ไม่สำเร็จ)
 847:             BotClickHelper.ClearDocumentFolder();
 848: 
 849:             if (this.IsHandleCreated)
 850:                 this.Invoke((MethodInvoker)(() =>
 851:                     MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error)));
 852:         });
 853:     }
 854: 
 855:     private void button2_Click(object sender, EventArgs e)
 856:     {
 857:         // ใช้ Invoke เพื่อความปลอดภัยว่าทำงานบน UI Thread (STA)
 858:         this.Invoke(new Action(() =>
 859:         {
 860:             using (frmPatternEditor editor = new frmPatternEditor())
 861:             {
 862:                 editor.StartPosition = FormStartPosition.CenterParent;
 863:                 editor.ShowDialog();
 864:             }
 865:         }));
 866:     }
 867: 
 868:     private async void button3_Click(object sender, EventArgs e)
 869:     {
 870:         //await SendTextBlocksToIjAsync(3);
 871:         var form = new frmCreateJob(_api);
 872:         form.ShowDialog();
 873:     }
 874: 
 875:     private async void button4_Click(object sender, EventArgs e)
 876:     {
 877:         await TestSendToIj3TcpAsync();
 878:     }
 879: 
 880:     // Example usage: send blocks from dgvTextBlocks to IJ3 via TCP (simple demo)
 881:     private async Task TestSendToIj3TcpAsync()
 882:     {
 883:         // read TCP host/port from UI controls
 884:         string host = txtTcpHost.Text.Trim();
 885:         if (!int.TryParse(txtTcpPort.Text.Trim(), out int port))
 886:         {
 887:             Log("Invalid TCP port");
 888:             return;
 889:         }
 890: 
 891:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
 892:         try
 893:         {
 894:             await client.ConnectAsync(host, port);
 895:         }
 896:         catch (Exception ex)
 897:         {
 898:             Log("TCP connect failed: " + ex.Message);
 899:             return;
 900:         }
 901: 
 902:         // Example: change program to 13
 903:         var progResp = await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, 13);
 904:         Log($"ChangeProgram resp: {progResp}");
 905: 
 906:         // Send each row in dgvTextBlocks
 907:         foreach (DataGridViewRow row in dgvTextBlocks.Rows)
 908:         {
 909:             if (row.IsNewRow) continue;
 910: 
 911:             int blockNumber = 1;
 912:             if (dgvTextBlocks.Columns.Contains("BlockNumber") && int.TryParse(row.Cells["BlockNumber"].Value?.ToString(), out var bn))
 913:                 blockNumber = bn;
 914: 
 915:             string text = row.Cells["Text"].Value?.ToString() ?? row.Cells["text"].Value?.ToString() ?? "";
 916:             int x = int.TryParse(row.Cells["X"].Value?.ToString(), out var tx) ? tx : 0;
 917:             int y = int.TryParse(row.Cells["Y"].Value?.ToString(), out var ty) ? ty : 0;
 918:             int size = int.TryParse(row.Cells["Size"].Value?.ToString(), out var ts) ? ts : 1;
 919:             int scale = int.TryParse(row.Cells["Scale"].Value?.ToString(), out var tsc) ? tsc : 1;
 920: 
 921:             var block = new InkjetOperator.Services.SimpleTextBlock(blockNumber, text, x, y, size, scale);
 922:             var (fsResp, f1Resp) = await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, 13, block);
 923:             Log($"IJ3 TextBlock({blockNumber}) FS='{fsResp}' F1='{f1Resp}'");
 924: 
 925:             // If adapter or device returns empty error, you can decide to continue or break.
 926:             await Task.Delay(30);
 927:         }
 928: 
 929:         var resume = await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
 930:         Log($"Resume resp: {resume}");
 931:     }
 932: 
 933:     private async void dgvUVBlocks_CellValueChanged(object sender, DataGridViewCellEventArgs e)
 934:     {
 935:         if (e.RowIndex < 0) return;
 936: 
 937:         var row = dgvUVBlocks.Rows[e.RowIndex];
 938: 
 939:         if (row.DataBoundItem is not UvRow data) return;
 940: 
 941:         try
 942:         {
 943:             await UpdateUvRow(data.Id, data.Lot, data.Name , data.Program);
 944:             Log($"[UV Updated] ID={data.Id}, Lot={data.Lot}, Name={data.Name}");
 945:         }
 946:         catch (Exception ex)
 947:         {
 948:             Log("Update UV error: " + ex.Message);
 949:         }
 950:     }
 951: 
 952:     private async Task UpdateUvRow(int id, string lot, string name , string program)
 953:     {
 954:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 955:         await conn.OpenAsync();
 956: 
 957:         var cmd = conn.CreateCommand();
 958:         cmd.CommandText = @"
 959:         UPDATE uv_print_data
 960:         SET lot = @lot,
 961:             name = @name,
 962:             program_name = @program_name,
 963:             update_at = CURRENT_TIMESTAMP
 964:         WHERE id = @id
 965:     ";
 966: 
 967:         cmd.Parameters.AddWithValue("@id", id);
 968:         cmd.Parameters.AddWithValue("@lot", lot);
 969:         cmd.Parameters.AddWithValue("@name", name);
 970:         cmd.Parameters.AddWithValue("@program_name", program);
 971: 
 972:         await cmd.ExecuteNonQueryAsync();
 973:     }
 974: 
 975:     public void InitializeDatabase()
 976:     {
 977:         string folderPath = @"D:\DB";
 978:         if (!Directory.Exists(folderPath))
 979:         {
 980:             Directory.CreateDirectory(folderPath);
 981:         }
 982: 
 983:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 984:         conn.Open();
 985: 
 986:         // 🔹 1. สร้าง table
 987:         string createTableSql = @"
 988:     CREATE TABLE IF NOT EXISTS uv_print_data (
 989:         id INTEGER PRIMARY KEY AUTOINCREMENT,
 990:         inkjet_name TEXT,
 991:         lot TEXT,
 992:         name TEXT,
 993:         update_at DATETIME DEFAULT CURRENT_TIMESTAMP
 994:     );";
 995: 
 996:         using (var cmd = new SqliteCommand(createTableSql, conn))
 997:         {
 998:             cmd.ExecuteNonQuery();
 999:         }
1000: 
1001:         // 🔹 2. เช็ค column program_name
1002:         bool hasProgramName = false;
1003: 
1004:         using (var checkColumnCmd = new SqliteCommand("PRAGMA table_info(uv_print_data);", conn))
1005:         using (var reader = checkColumnCmd.ExecuteReader())
1006:         {
1007:             while (reader.Read())
1008:             {
1009:                 if (reader["name"].ToString() == "program_name")
1010:                 {
1011:                     hasProgramName = true;
1012:                     break;
1013:                 }
1014:             }
1015:         }
1016: 
1017:         // 🔹 3. เพิ่ม column ถ้ายังไม่มี
1018:         if (!hasProgramName)
1019:         {
1020:             using var alterCmd = new SqliteCommand(
1021:                 "ALTER TABLE uv_print_data ADD COLUMN program_name TEXT;",
1022:                 conn);
1023: 
1024:             alterCmd.ExecuteNonQuery();
1025:         }
1026: 
1027:         // 🔹 4. เช็คข้อมูล
1028:         long count;
1029:         using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM uv_print_data", conn))
1030:         {
1031:             count = (long)checkCmd.ExecuteScalar();
1032:         }
1033: 
1034:         // 🔹 5. Insert default row
1035:         if (count == 0)
1036:         {
1037:             using var insertCmd = new SqliteCommand(@"
1038:         INSERT INTO uv_print_data (id, inkjet_name, lot, name, program_name) VALUES 
1039:         (1, 'เครื่องพิมพ์ UV1', '', '', ''),
1040:         (2, 'เครื่องพิมพ์ UV2', '', '', '')", conn);
1041: 
1042:             insertCmd.ExecuteNonQuery();
1043:         }
1044:     }
1045: 
1046:     private async Task LoadUvDataToGrid()
1047:     {
1048:         try
1049:         {
1050:             using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3");
1051:             await conn.OpenAsync();
1052: 
1053:             var cmd = conn.CreateCommand();
1054:             cmd.CommandText = @"
1055:         SELECT id, inkjet_name, lot, name, update_at , program_name
1056:         FROM uv_print_data
1057:         ORDER BY id
1058:         ";
1059: 
1060:             var reader = await cmd.ExecuteReaderAsync();
1061: 
1062:             var list = new List<UvRow>();
1063: 
1064:             while (await reader.ReadAsync())
1065:             {
1066:                 list.Add(new UvRow
1067:                 {
1068:                     Id = reader.GetInt32(0), // 🔥 ยังต้องมี
1069:                     Inkjet = reader.GetString(1),
1070:                     Lot = reader.IsDBNull(2) ? "" : reader.GetString(2),
1071:                     Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
1072:                     UpdateAt = reader.IsDBNull(4) ? "" : reader.GetString(4),
1073:                     Program = reader.IsDBNull(5) ? "" : reader.GetString(5)
1074:                 });
1075:             }
1076: 
1077:             dgvUVBlocks.Invoke(() =>
1078:             {
1079: 
1080:                 dgvUVBlocks.AutoGenerateColumns = false;
1081: 
1082:                 dgvUVBlocks.ReadOnly = false;
1083:                 dgvUVBlocks.AllowUserToAddRows = false;
1084:                 dgvUVBlocks.EditMode = DataGridViewEditMode.EditOnEnter;
1085:                 dgvUVBlocks.SelectionMode = DataGridViewSelectionMode.CellSelect;
1086: 
1087: 
1088:                 _uvBindingList.RaiseListChangedEvents = false;
1089:                 _uvBindingList.Clear();
1090: 
1091:                 foreach (var item in list)
1092:                 {
1093:                     _uvBindingList.Add(item);
1094:                 }
1095: 
1096:                 _uvBindingList.RaiseListChangedEvents = true;
1097:                 _uvBindingList.ResetBindings();
1098: 
1099:                 dgvUVBlocks.DataSource = _uvBindingList;
1100: 
1101:             });
1102:         }
1103:         catch (Exception ex)
1104:         {
1105:             Log("Load UV error: " + ex.Message);
1106:         }
1107:     }
1108: 
1109:     private void button6_Click(object sender, EventArgs e)
1110:     {
1111:         var firstRow = _uvBindingList.FirstOrDefault();
1112: 
1113:         if (firstRow == null || string.IsNullOrWhiteSpace(firstRow.Program))
1114:         {
1115:             MessageBox.Show("ไม่มีค่า program_name ใน row แรก");
1116:             return;
1117:         }
1118: 
1119:         string fileName = firstRow.Program;
1120: 
1121:         var steps = new[]
1122:         {
1123:         new BotStep { Name = "Document",   X = 2325, Y = 59,  VerifyArea = new Rectangle(2100, 0, 400, 300) },
1124:         new BotStep { Name = "Open",       X = 2207, Y = 133, VerifyArea = new Rectangle(2100, 100, 400, 400) },
1125:         new BotStep { Name = "SelectFile", X = 890,  Y = 525, VerifyArea = new Rectangle(800, 400, 800, 600) },
1126:         new BotStep { Name = "OpenBtn",    X = 1652, Y = 946, VerifyArea = new Rectangle(1500, 800, 500, 300) },
1127:     };
1128: 
1129:         try
1130:         {
1131:             // ✅ 1. ลบไฟล์ก่อน
1132:             BotClickHelper.ClearDocumentFolder();
1133: 
1134:             // ✅ 2. Copy file
1135:             BotClickHelper.CopyFile(fileName);
1136:         }
1137:         catch (Exception ex)
1138:         {
1139:             MessageBox.Show("เตรียมไฟล์ไม่สำเร็จ: " + ex.Message);
1140:             return;
1141:         }
1142: 
1143:         // ✅ 3. Run Bot
1144:         BotClickHelper.RunAsync("uvinkjet", steps, result =>
1145:         {
1146:             if (result.Success)
1147:             {
1148:                 try
1149:                 {
1150:                     // 🔹 4. Update DB
1151:                     var row = _uvBindingList.FirstOrDefault(x => x.Id == 1);
1152: 
1153:                     if (row != null)
1154:                     {
1155:                         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1156: 
1157:                         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1158:                         {
1159:                             conn.Open();
1160: 
1161:                             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1162: 
1163:                             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1164:                             {
1165:                                 cmd.Parameters.AddWithValue("@lot", row.Lot ?? "");
1166:                                 cmd.Parameters.AddWithValue("@name", row.Name ?? "");
1167:                                 cmd.ExecuteNonQuery();
1168:                             }
1169:                         }
1170:                     }
1171: 
1172:                     // 🔹 5. ลบไฟล์หลังสำเร็จ
1173:                     BotClickHelper.ClearDocumentFolder();
1174:                 }
1175:                 catch (Exception ex)
1176:                 {
1177:                     Console.WriteLine("Post-process error: " + ex.Message);
1178:                 }
1179:             }
1180:             else
1181:             {
1182:                 // ❌ Fail → ไม่ลบไฟล์
1183:                 Console.WriteLine("Bot failed: " + result.Error);
1184:             }
1185: 
1186:             // 🔹 UI
1187:             if (this.IsHandleCreated)
1188:             {
1189:                 this.Invoke((MethodInvoker)(() =>
1190:                 {
1191:                     MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error);
1192:                     currentStep = 3;
1193:                     UpdateStepButtons();
1194:                 }));
1195:             }
1196:         });
1197:     }
1198: 
1199:     private void button8_Click(object sender, EventArgs e)
1200:     {
1201:         var row = _uvBindingList.FirstOrDefault(x => x.Id == 2);
1202: 
1203:         if (row == null)
1204:         {
1205:             MessageBox.Show("ไม่พบ row id = 1");
1206:             return;
1207:         }
1208: 
1209:         string lot = row.Lot;
1210:         string name = row.Name;
1211: 
1212:         Log($"Lot={lot}, Name={name}");
1213: 
1214:         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1215: 
1216:         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1217:         {
1218:             conn.Open();
1219: 
1220:             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1221: 
1222:             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1223:             {
1224:                 cmd.Parameters.AddWithValue("@lot", lot);
1225:                 cmd.Parameters.AddWithValue("@name", name);
1226: 
1227:                 int rows = cmd.ExecuteNonQuery();
1228: 
1229:                 MessageBox.Show("Update สำเร็จ: " + rows + " row");
1230:             }
1231:         }
1232:         currentStep = 1;
1233:         UpdateStepButtons();
1234:     }
1235: 
1236: 
1237:     private async void button5_Click(object sender, EventArgs e)
1238:     {
1239:         //if (dgvConfigs.Rows.Count > 0)
1240:         //{
1241:         //    dgvConfigs.ClearSelection();
1242: 
1243:         //    dgvConfigs.Rows[0].Selected = true;
1244:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[0].Cells[0];
1245: 
1246:         //    var config = dgvConfigs.Rows[0].DataBoundItem as InkjetConfigDto;
1247: 
1248:         //    if (config != null)
1249:         //    {
1250:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1251:         //    }
1252:         //}
1253: 
1254:         //await TestSendToIj3TcpAsync();
1255: 
1256:         if (dgvConfigs.Rows.Count > 1)
1257:         {
1258:             dgvConfigs.ClearSelection();
1259: 
1260:             dgvConfigs.Rows[1].Selected = true;
1261:             dgvConfigs.CurrentCell = dgvConfigs.Rows[1].Cells[0];
1262: 
1263:             var config = dgvConfigs.Rows[1].DataBoundItem as InkjetConfigDto;
1264: 
1265:             if (config != null)
1266:             {
1267:                 MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1268:             }
1269:         }
1270: 
1271:         await TestSendToIj3TcpAsync();
1272: 
1273:         currentStep = 2; // ไปขั้นที่ 2
1274:         UpdateStepButtons();
1275: 
1276:     }
1277: 
1278:     private async void button7_Click(object sender, EventArgs e)
1279:     {
1280:         await SendToIj3FromDbAsync("CCRC0291-DEX0663MS");
1281:         //if (dgvConfigs.Rows.Count > 2)
1282:         //{
1283:         //    dgvConfigs.ClearSelection();
1284: 
1285:         //    dgvConfigs.Rows[2].Selected = true;
1286:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[2].Cells[0];
1287: 
1288:         //    var config = dgvConfigs.Rows[2].DataBoundItem as InkjetConfigDto;
1289: 
1290:         //    if (config != null)
1291:         //    {
1292:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1293:         //    }
1294: 
1295:         //    await TestSendToIj3TcpAsync();
1296: 
1297:         currentStep = 4; // ไปขั้นที่ 2
1298:         UpdateStepButtons();
1299:         //}
1300:     }
1301: 
1302:     private void UpdateStepButtons()
1303:     {
1304:         // ปุ่ม 1 (MK1+2)
1305:         button5.Enabled = (currentStep == 1);
1306: 
1307:         // ปุ่ม 2 (UV1)
1308:         //button6.Enabled = (currentStep == 2);
1309: 
1310:         // ปุ่ม 3 (MK2/3)
1311:         button7.Enabled = (currentStep == 3);
1312: 
1313:         // ปุ่ม 4 (UV2)
1314:         button8.Enabled = (currentStep == 4);
1315: 
1316:         // ถ้าอยากให้เห็นชัดเจนว่าอันไหนเสร็จแล้ว อาจจะเปลี่ยนสีปุ่มด้วยก็ได้
1317:         //button5.BackColor = (currentStep > 1) ? Color.LightGreen : SystemColors.Control;
1318:         // ... ทำแบบเดียวกันกับปุ่มอื่นๆ
1319:     }
1320: 
1321:     private async Task SendToIj3FromDbAsync(string barcode)
1322:     {
1323:         // ตัวแปรสำหรับ Block 1
1324:         string text1 = ""; int x1 = 0, y1 = 0, size1 = 1, scale1 = 1;
1325:         // ตัวแปรสำหรับ Block 2
1326:         string text2 = ""; int x2 = 0, y2 = 0, size2 = 1, scale2 = 1;
1327: 
1328:         string dbPath = @"D:\DB\uv_data.db3";
1329:         using (var conn = new SqliteConnection($"Data Source={dbPath}"))
1330:         {
1331:             await conn.OpenAsync();
1332:             string sql = "SELECT * FROM config_data_mk3 WHERE pattern_no_erp = @barcode LIMIT 1";
1333:             using var cmd = new SqliteCommand(sql, conn);
1334:             cmd.Parameters.AddWithValue("@barcode", barcode);
1335:             using var reader = await cmd.ExecuteReaderAsync();
1336: 
1337:             if (reader.Read())
1338:             {
1339:                 // ดึงข้อมูล Block 1
1340:                 text1 = reader.GetString(reader.GetOrdinal("block1_text"));
1341:                 x1 = reader.GetInt32(reader.GetOrdinal("block1_x"));
1342:                 y1 = reader.GetInt32(reader.GetOrdinal("block1_y"));
1343:                 size1 = reader.GetInt32(reader.GetOrdinal("block1_size"));
1344:                 scale1 = reader.GetInt32(reader.GetOrdinal("block1_scale_side"));
1345: 
1346:                 // ดึงข้อมูล Block 2
1347:                 text2 = reader.IsDBNull(reader.GetOrdinal("block2_text")) ? "" : reader.GetString(reader.GetOrdinal("block2_text"));
1348:                 x2 = reader.GetInt32(reader.GetOrdinal("block2_x"));
1349:                 y2 = reader.GetInt32(reader.GetOrdinal("block2_y"));
1350:                 size2 = reader.GetInt32(reader.GetOrdinal("block2_size"));
1351:                 scale2 = reader.GetInt32(reader.GetOrdinal("block2_scale_side"));
1352:             }
1353:         }
1354: 
1355:         // --- ส่วนการส่ง TCP ---
1356:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
1357:         try
1358:         {
1359:             await client.ConnectAsync(txtTcpHost.Text.Trim(), int.Parse(txtTcpPort.Text.Trim()));
1360:             int fixedProgNo = 13;
1361: 
1362:             // 1. เปลี่ยนโปรแกรมเป็น 13
1363:             await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, fixedProgNo);
1364: 
1365:             // 2. ส่ง Block 1
1366:             var block1 = new InkjetOperator.Services.SimpleTextBlock(1, text1, x1, y1, size1, scale1);
1367:             await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block1);
1368:             await Task.Delay(50); // พักช่วงสั้นๆ ระหว่างการส่งแต่ละ Block
1369: 
1370:             // 3. ส่ง Block 2
1371:             if (!string.IsNullOrEmpty(text2))
1372:             {
1373:                 var block2 = new InkjetOperator.Services.SimpleTextBlock(2, text2, x2, y2, size2, scale2);
1374:                 await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block2);
1375:                 await Task.Delay(50);
1376:             }
1377: 
1378:             // 4. สั่ง Resume
1379:             await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
1380:             Log("Step 3: MK3 (2 Blocks) Sent Successfully.");
1381:         }
1382:         catch (Exception ex)
1383:         {
1384:             Log("Error: " + ex.Message);
1385:         }
1386:     }
1387: 
1388:     private void button9_Click(object sender, EventArgs e)
1389:     {
1390:         string lot = "C200521-001" ?? string.Empty;
1391:         string blockText = "DDDD-01";
1392:         string previewText = PatternEngine.Process(lot, blockText);
1393:         //lblPreview.Text = "Preview: " + previewText;
1394:         Log("Preview: " + previewText);
1395:     }
1396: 
1397:     private async void dgvJobs_CellClick(object sender, DataGridViewCellEventArgs e)
1398:     {
1399:         if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1400: 
1401:         if (dgvJobs.SelectedRows.Count == 0) return;
1402: 
1403:         var row = dgvJobs.SelectedRows[0];
1404:         if (row.Cells["Id"].Value == null) return;
1405: 
1406:         int jobId = (int)row.Cells["Id"].Value;
1407:         string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
1408: 
1409:         if (jobId == _selectedJobId) return;
1410:         _selectedJobId = jobId;
1411: 
1412:         // Helpers
1413:         static bool ReaderHasColumn(SqliteDataReader r, string name)
1414:         {
1415:             for (int i = 0; i < r.FieldCount; i++)
1416:             {
1417:                 if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1418:                     return true;
1419:             }
1420:             return false;
1421:         }
1422: 
1423:         static string? GetStringSafe(SqliteDataReader r, string name)
1424:         {
1425:             if (!ReaderHasColumn(r, name)) return null;
1426:             int idx = r.GetOrdinal(name);
1427:             return r.IsDBNull(idx) ? null : r.GetString(idx);
1428:         }
1429: 
1430:         static int? GetIntSafe(SqliteDataReader r, string name)
1431:         {
1432:             var s = GetStringSafe(r, name);
1433:             if (string.IsNullOrWhiteSpace(s)) return null;
1434:             return int.TryParse(s, out var v) ? v : null;
1435:         }
1436: 
1437:         try
1438:         {
1439:             string dbPath = @"D:\DB\uv_data.db3";
1440:             string connStr = $"Data Source={dbPath}";
1441:             bool found = false;
1442: 
1443:             try
1444:             {
1445:                 await using var conn = new SqliteConnection(connStr);
1446:                 await conn.OpenAsync();
1447: 
1448:                 await using var cmd = conn.CreateCommand();
1449:                 cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1450:                 cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1451: 
1452:                 await using var reader = await cmd.ExecuteReaderAsync();
1453:                 if (await reader.ReadAsync())
1454:                 {
1455:                     // Build PatternDetail from known schema columns.
1456:                     var pattern = new PatternDetail
1457:                     {
1458:                         Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1459:                         Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1460:                     };
1461: 
1462:                     // Helper to build MK config (mk1 / mk2)
1463:                     InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1464:                     {
1465:                         var cfg = new InkjetConfigDto
1466:                         {
1467:                             Ordinal = ordinal,
1468:                             ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1469:                             ProgramName = GetStringSafe(reader, programNameColumn),
1470:                             Width = GetIntSafe(reader, $"{prefix}width"),
1471:                             Height = GetIntSafe(reader, $"{prefix}height"),
1472:                             TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1473:                             Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1474:                             SteelType = null,
1475:                             Suspended = false
1476:                         };
1477: 
1478:                         // text blocks 1..5
1479:                         for (int b = 1; b <= 5; b++)
1480:                         {
1481:                             string textCol = $"{prefix}block{b}_text";
1482:                             if (ReaderHasColumn(reader, textCol))
1483:                             {
1484:                                 var text = GetStringSafe(reader, textCol);
1485:                                 if (!string.IsNullOrEmpty(text))
1486:                                 {
1487:                                     var tb = new TextBlockDto
1488:                                     {
1489:                                         BlockNumber = b,
1490:                                         Text = text,
1491:                                         X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1492:                                         Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1493:                                         Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1494:                                         Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1495:                                     };
1496:                                     cfg.TextBlocks.Add(tb);
1497:                                 }
1498:                             }
1499:                         }
1500: 
1501:                         return cfg;
1502:                     }
1503: 
1504:                     // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
1505:                     var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1506:                     // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
1507:                     var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1508: 
1509:                     pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1510: 
1511:                     // Conveyor speeds / servo configs - attempt to map if available (optional)
1512:                     var spd1 = GetIntSafe(reader, "belt1_inkjet");
1513:                     var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1514:                     if (spd1.HasValue || spd2.HasValue)
1515:                     {
1516:                         pattern.ConveyorSpeeds = new ConveyorSpeedDto
1517:                         {
1518:                             Speed1 = spd1,
1519:                             Speed2 = spd2,
1520:                             Speed3 = GetIntSafe(reader, "belt3")
1521:                         };
1522:                     }
1523: 
1524:                     // Minimal job + resolved response
1525:                     _currentResolved = new ResolvedJobResponse
1526:                     {
1527:                         Job = new PrintJob
1528:                         {
1529:                             Id = jobId,
1530:                             BarcodeRaw = rawbarcode,
1531:                             LotNumber = row.Cells["LotNumber"].Value?.ToString(),
1532:                             Status = row.Cells["Status"].Value?.ToString()
1533:                         },
1534:                         Pattern = pattern
1535:                     };
1536: 
1537:                     string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
1538:                     string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ
1539: 
1540:                     found = true;
1541:                     UpdateDetailPanel();
1542:                 }
1543: 
1544:                 await conn.CloseAsync();
1545:             }
1546:             catch (Exception dbEx)
1547:             {
1548:                 Log("SQLite query error: " + dbEx.Message);
1549:             }
1550: 
1551:             if (!found)
1552:             {
1553:                 var resolved = await _api.GetResolvedJobAsync(jobId);
1554:                 _currentResolved = resolved;
1555:                 UpdateDetailPanel();
1556:             }
1557:         }
1558:         catch (Exception ex)
1559:         {
1560:             Log("Load job detail error: " + ex.Message);
1561:         }
1562:     }
1563: 
1564:     private async Task LoadLastSentData()
1565:     {
1566:         try
1567:         {
1568:             // เรียกใช้ API (ระบุ status เป็น sent, หน้า 1, จำนวน 10 รายการ)
1569:             var lastSentJobs = await _api.GetLastSentJobsAsync("sent", 1, 10);
1570: 
1571:             // นำข้อมูลไปผูกกับ BindingSource หรือ DataGridView
1572:             var jobRows = lastSentJobs.Select(j => new InkjetOperator.Models.JobRow
1573:             {
1574:                 Id = j.Id,
1575:                 BarcodeRaw = j.BarcodeRaw,
1576:                 LotNumber = j.LotNumber ?? string.Empty,
1577:                 Status = j.Status,
1578:                 Attempt = j.Attempt
1579:                 // เพิ่ม field อื่นๆ ที่ต้องการแสดงในตาราง history
1580:             }).ToList();
1581: 
1582:             printJobBindingSource.DataSource = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(jobRows);
1583:         }
1584:         catch (Exception ex)
1585:         {
1586:             MessageBox.Show($"โหลดข้อมูลประวัติไม่สำเร็จ: {ex.Message}", "Error");
1587:         }
1588:     }
1589: 
1590:     private async void dataGridView1_SelectionChanged(object sender, EventArgs e)
1591:     {
1592:         ////// Guard: header click / invalid index
1593:         ////if (e.RowIndex < 0) return;
1594:         ////if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1595: 
1596:         ////var row = dataGridView1.Rows[e.RowIndex];
1597:         //int jobId;
1598:         //string rawbarcode;
1599:         //if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedJob)
1600:         //{
1601:         //    jobId = selectedJob.Id;
1602:         //    rawbarcode = selectedJob.BarcodeRaw;
1603: 
1604: 
1605:         //    //Debug.WriteLine($"Clicked row {e.RowIndex}, Id={row.Cells["Id"].Value}, Barcode={row.Cells["BarcodeRaw"].Value}");
1606: 
1607:         //    //// Ensure the grid contains expected columns
1608:         //    //if (!dataGridView1.Columns.Contains("Id") || row.Cells["Id"].Value == null) return;
1609:         //    //if (!dataGridView1.Columns.Contains("BarcodeRaw")) return;
1610: 
1611: 
1612: 
1613:         //    //try
1614:         //    //{
1615: 
1616:         //    //}
1617:         //    //catch
1618:         //    //{
1619:         //    //    // invalid id cell
1620:         //    //    return;
1621:         //    //}
1622: 
1623:         //    //string rawbarcode = "CCRC0291-DEX0663MS";
1624:         //    //if (jobId == _selectedJobId) return;
1625:         //    //_selectedJobId = jobId;
1626: 
1627:         //    // Helpers
1628:         //    static bool ReaderHasColumn(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1629:         //    {
1630:         //        for (int i = 0; i < r.FieldCount; i++)
1631:         //        {
1632:         //            if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1633:         //                return true;
1634:         //        }
1635:         //        return false;
1636:         //    }
1637: 
1638:         //    static string? GetStringSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1639:         //    {
1640:         //        if (!ReaderHasColumn(r, name)) return null;
1641:         //        int idx = r.GetOrdinal(name);
1642:         //        return r.IsDBNull(idx) ? null : r.GetString(idx);
1643:         //    }
1644: 
1645:         //    static int? GetIntSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1646:         //    {
1647:         //        var s = GetStringSafe(r, name);
1648:         //        if (string.IsNullOrWhiteSpace(s)) return null;
1649:         //        return int.TryParse(s, out var v) ? v : null;
1650:         //    }
1651: 
1652:         //    try
1653:         //    {
1654:         //        string dbPath = @"D:\DB\uv_data.db3";
1655:         //        string connStr = $"Data Source={dbPath}";
1656:         //        bool found = false;
1657: 
1658:         //        // optional debug: MessageBox.Show($"Clicked Job ID: {jobId}, Barcode: {rawbarcode}");
1659: 
1660:         //        try
1661:         //        {
1662:         //            await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
1663:         //            await conn.OpenAsync();
1664: 
1665:         //            await using var cmd = conn.CreateCommand();
1666:         //            cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1667:         //            cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1668: 
1669: 
1670:         //            await using var reader = await cmd.ExecuteReaderAsync();
1671:         //            if (await reader.ReadAsync())
1672:         //            {
1673:         //                // Build PatternDetail from known schema columns.
1674:         //                var pattern = new PatternDetail
1675:         //                {
1676:         //                    Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1677:         //                    Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1678:         //                };
1679: 
1680:         //                // Helper to build MK config (mk1 / mk2)
1681:         //                InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1682:         //                {
1683:         //                    var cfg = new InkjetConfigDto
1684:         //                    {
1685:         //                        Ordinal = ordinal,
1686:         //                        ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1687:         //                        ProgramName = GetStringSafe(reader, programNameColumn),
1688:         //                        Width = GetIntSafe(reader, $"{prefix}width"),
1689:         //                        Height = GetIntSafe(reader, $"{prefix}height"),
1690:         //                        TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1691:         //                        Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1692:         //                        SteelType = null,
1693:         //                        Suspended = false
1694:         //                    };
1695: 
1696:         //                    // text blocks 1..5
1697:         //                    for (int b = 1; b <= 5; b++)
1698:         //                    {
1699:         //                        string textCol = $"{prefix}block{b}_text";
1700:         //                        if (ReaderHasColumn(reader, textCol))
1701:         //                        {
1702:         //                            var text = GetStringSafe(reader, textCol);
1703:         //                            if (!string.IsNullOrEmpty(text))
1704:         //                            {
1705:         //                                var tb = new TextBlockDto
1706:         //                                {
1707:         //                                    BlockNumber = b,
1708:         //                                    Text = text,
1709:         //                                    X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1710:         //                                    Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1711:         //                                    Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1712:         //                                    Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1713:         //                                };
1714:         //                                cfg.TextBlocks.Add(tb);
1715:         //                            }
1716:         //                        }
1717:         //                    }
1718: 
1719:         //                    return cfg;
1720:         //                }
1721: 
1722:         //                var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1723:         //                var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1724: 
1725:         //                pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1726: 
1727:         //                var spd1 = GetIntSafe(reader, "belt1_inkjet");
1728:         //                var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1729:         //                if (spd1.HasValue || spd2.HasValue)
1730:         //                {
1731:         //                    pattern.ConveyorSpeeds = new ConveyorSpeedDto
1732:         //                    {
1733:         //                        Speed1 = spd1,
1734:         //                        Speed2 = spd2,
1735:         //                        Speed3 = GetIntSafe(reader, "belt3")
1736:         //                    };
1737:         //                }
1738: 
1739:         //                // Minimal job + resolved response
1740:         //                _currentResolved = new ResolvedJobResponse
1741:         //                {
1742:         //                    Job = new PrintJob
1743:         //                    {
1744:         //                        Id = jobId,
1745:         //                        BarcodeRaw = rawbarcode,
1746:         //                        LotNumber = dataGridView1.Columns.Contains("LotNumber") ? selectedJob.LotNumber?.ToString() : null,
1747:         //                        Status = dataGridView1.Columns.Contains("Status") ? selectedJob.Status?.ToString() : null
1748:         //                    },
1749:         //                    Pattern = pattern
1750:         //                };
1751: 
1752:         //                found = true;
1753:         //                UpdateDetailPanel();
1754:         //            }
1755: 
1756:         //            await conn.CloseAsync();
1757:         //        }
1758:         //        catch (Exception dbEx)
1759:         //        {
1760:         //            Log("SQLite query error: " + dbEx.Message);
1761:         //        }
1762: 
1763: 
1764:         //        if (!found)
1765:         //        {
1766:         //            var resolved = await _api.GetResolvedJobAsync(jobId);
1767:         //            _currentResolved = resolved;
1768:         //            UpdateDetailPanel();
1769:         //        }
1770: 
1771:         //    }
1772:         //    catch (Exception ex)
1773:         //    {
1774:         //        Log("Load job detail error: " + ex.Message);
1775:         //    }
1776:         //}
1777:     }
1778: 
1779:     private async void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
1780:     {
1781:         //// Guard: header click / invalid index
1782:         //if (e.RowIndex < 0) return;
1783:         //if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1784: 
1785:         var row = dataGridView1.Rows[e.RowIndex];
1786:         int jobId;
1787:         string rawbarcode;
1788:         if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedJob)
1789:         {
1790:             jobId = selectedJob.Id;
1791:             rawbarcode = selectedJob.BarcodeRaw;
1792: 
1793: 
1794:             //Debug.WriteLine($"Clicked row {e.RowIndex}, Id={row.Cells["Id"].Value}, Barcode={row.Cells["BarcodeRaw"].Value}");
1795: 
1796:             //// Ensure the grid contains expected columns
1797:             //if (!dataGridView1.Columns.Contains("Id") || row.Cells["Id"].Value == null) return;
1798:             //if (!dataGridView1.Columns.Contains("BarcodeRaw")) return;
1799: 
1800: 
1801: 
1802:             //try
1803:             //{
1804: 
1805:             //}
1806:             //catch
1807:             //{
1808:             //    // invalid id cell
1809:             //    return;
1810:             //}
1811: 
1812:             //string rawbarcode = "CCRC0291-DEX0663MS";
1813:             //if (jobId == _selectedJobId) return;
1814:             //_selectedJobId = jobId;
1815: 
1816:             // Helpers
1817:             static bool ReaderHasColumn(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1818:             {
1819:                 for (int i = 0; i < r.FieldCount; i++)
1820:                 {
1821:                     if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1822:                         return true;
1823:                 }
1824:                 return false;
1825:             }
1826: 
1827:             static string? GetStringSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1828:             {
1829:                 if (!ReaderHasColumn(r, name)) return null;
1830:                 int idx = r.GetOrdinal(name);
1831:                 return r.IsDBNull(idx) ? null : r.GetString(idx);
1832:             }
1833: 
1834:             static int? GetIntSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1835:             {
1836:                 var s = GetStringSafe(r, name);
1837:                 if (string.IsNullOrWhiteSpace(s)) return null;
1838:                 return int.TryParse(s, out var v) ? v : null;
1839:             }
1840: 
1841:             try
1842:             {
1843:                 string dbPath = @"D:\DB\uv_data.db3";
1844:                 string connStr = $"Data Source={dbPath}";
1845:                 bool found = false;
1846: 
1847:                 try
1848:                 {
1849:                     await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
1850:                     await conn.OpenAsync();
1851: 
1852:                     await using var cmd = conn.CreateCommand();
1853:                     cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1854:                     cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1855: 
1856:                 
1857: 
1858:                     await using var reader = await cmd.ExecuteReaderAsync();
1859: 
1860:                
1861:                     if (await reader.ReadAsync())
1862:                     {
1863:                         // Build PatternDetail from known schema columns.
1864:                         var pattern = new PatternDetail
1865:                         {
1866:                             Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1867:                             Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1868:                         };
1869: 
1870:                         // Helper to build MK config (mk1 / mk2)
1871:                         InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1872:                         {
1873:                             var cfg = new InkjetConfigDto
1874:                             {
1875:                                 Ordinal = ordinal,
1876:                                 ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1877:                                 ProgramName = GetStringSafe(reader, programNameColumn),
1878:                                 Width = GetIntSafe(reader, $"{prefix}width"),
1879:                                 Height = GetIntSafe(reader, $"{prefix}height"),
1880:                                 TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1881:                                 Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1882:                                 SteelType = null,
1883:                                 Suspended = false
1884:                             };
1885: 
1886:                             // text blocks 1..5
1887:                             for (int b = 1; b <= 5; b++)
1888:                             {
1889:                                 string textCol = $"{prefix}block{b}_text";
1890:                                 if (ReaderHasColumn(reader, textCol))
1891:                                 {
1892:                                     var text = GetStringSafe(reader, textCol);
1893:                                     if (!string.IsNullOrEmpty(text))
1894:                                     {
1895:                                         var tb = new TextBlockDto
1896:                                         {
1897:                                             BlockNumber = b,
1898:                                             Text = text,
1899:                                             X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1900:                                             Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1901:                                             Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1902:                                             Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1903:                                         };
1904:                                         cfg.TextBlocks.Add(tb);
1905:                                     }
1906:                                 }
1907:                             }
1908: 
1909:                             return cfg;
1910:                         }
1911: 
1912:                         var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1913:                         var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1914: 
1915:                         pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1916: 
1917:                         var spd1 = GetIntSafe(reader, "belt1_inkjet");
1918:                         var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1919:                         if (spd1.HasValue || spd2.HasValue)
1920:                         {
1921:                             pattern.ConveyorSpeeds = new ConveyorSpeedDto
1922:                             {
1923:                                 Speed1 = spd1,
1924:                                 Speed2 = spd2,
1925:                                 Speed3 = GetIntSafe(reader, "belt3")
1926:                             };
1927:                         }
1928: 
1929:                         // Minimal job + resolved response
1930:                         _currentResolved = new ResolvedJobResponse
1931:                         {
1932:                             Job = new PrintJob
1933:                             {
1934:                                 Id = jobId,
1935:                                 BarcodeRaw = rawbarcode,
1936:                                 LotNumber = dataGridView1.Columns.Contains("LotNumber") ? row.Cells["LotNumber"].Value?.ToString() : null,
1937:                                 Status = dataGridView1.Columns.Contains("Status") ? row.Cells["Status"].Value?.ToString() : null
1938:                             },
1939:                             Pattern = pattern
1940:                         };
1941: 
1942:                         found = true;
1943:                         UpdateDetailPanel();
1944:                     }
1945: 
1946:                     await conn.CloseAsync();
1947:                 }
1948:                 catch (Exception dbEx)
1949:                 {
1950:                     Log("SQLite query error: " + dbEx.Message);
1951:                 }
1952: 
1953: 
1954:                 if (!found)
1955:                 {
1956:                     var resolved = await _api.GetResolvedJobAsync(jobId);
1957:                     _currentResolved = resolved;
1958:                     UpdateDetailPanel();
1959:                 }
1960: 
1961:             }
1962:             catch (Exception ex)
1963:             {
1964:                 Log("Load job detail error: " + ex.Message);
1965:             }
1966:         }
1967:     }
1968: }
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

## File: frmSt3.cs
```csharp
  1: using System;
  2: using System.Collections.Generic;
  3: using System.ComponentModel;
  4: using System.Data;
  5: using System.Drawing;
  6: using System.Linq;
  7: using System.Text;
  8: using System.Threading.Tasks;
  9: using System.Windows.Forms;
 10: using InkjetOperator.Services;
 11: 
 12: namespace InkjetOperator
 13: {
 14:     public partial class frmSt3 : Form
 15:     {
 16:         private ApiClient _api;
 17:         public frmSt3()
 18:         {
 19:             InitializeComponent();
 20:             _api = new ApiClient("http://localhost:3000");
 21:         }
 22: 
 23:         private async void frmSt3_Load(object sender, EventArgs e)
 24:         {
 25:             try
 26:             {
 27:                 // Await the async API call, map PrintJob -> JobRow and bind to the BindingSource
 28:                 var jobs = await _api.GetPendingJobsAsync();
 29: 
 30:                 var jobRows = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(
 31:                     jobs.Select(j => new InkjetOperator.Models.JobRow
 32:                     {
 33:                         Id = j.Id,
 34:                         BarcodeRaw = j.BarcodeRaw,
 35:                         LotNumber = j.LotNumber ?? string.Empty,
 36:                         Status = j.Status,
 37:                         Attempt = j.Attempt
 38:                     }).ToList()
 39:                 );
 40: 
 41:                 printJobBindingSource.DataSource = jobRows;
 42:             }
 43:             catch (Exception ex)
 44:             {
 45:                 MessageBox.Show($"Failed to load pending jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
 46:             }
 47: 
 48:         }
 49: 
 50:         private async void button1_Click(object sender, EventArgs e)
 51:         {
 52:             // ดึง Row ที่เลือกจาก BindingSource
 53:             if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedRow)
 54:             {
 55:                 var payload = new
 56:                 {
 57:                     barcode_raw = selectedRow.BarcodeRaw,
 58:                     pattern_id = 41, // ปรับตาม logic ของคุณ
 59:                     lot_number = selectedRow.LotNumber,
 60:                     status = selectedRow.Status,
 61:                     created_by = "operator1",
 62:                     attempt = selectedRow.Attempt,
 63:                     order_no = "ORD-001",
 64:                     customer_name = "Customer A",
 65:                     type = "print",
 66:                     qty = 100,
 67:                     sent_time = DateTime.UtcNow, // ส่งเป็น DateTime ไปเลย PostAsJsonAsync จะจัดการ format ให้
 68:                     st_status = "sent"
 69:                 };
 70: 
 71:                 bool success = await _api.CreateLastSentJobAsync(payload);
 72: 
 73:                 if (success)
 74:                 {
 75:                     MessageBox.Show("ส่งข้อมูลไปที่ LastSent สำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
 76:                 }
 77:                 else
 78:                 {
 79:                     MessageBox.Show("ส่งข้อมูลไม่สำเร็จ กรุณาตรวจสอบ Log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
 80:                 }
 81:             }
 82:         }
 83: 
 84:         private async Task LoadLastSentData()
 85:         {
 86:             try
 87:             {
 88:                 // เรียกใช้ API (ระบุ status เป็น sent, หน้า 1, จำนวน 10 รายการ)
 89:                 var lastSentJobs = await _api.GetLastSentJobsAsync("sent", 1, 10);
 90: 
 91:                 // นำข้อมูลไปผูกกับ BindingSource หรือ DataGridView
 92:                 var jobRows = lastSentJobs.Select(j => new InkjetOperator.Models.JobRow
 93:                 {
 94:                     Id = j.Id,
 95:                     BarcodeRaw = j.BarcodeRaw,
 96:                     LotNumber = j.LotNumber ?? string.Empty,
 97:                     Status = j.Status,
 98:                     Attempt = j.Attempt
 99:                     // เพิ่ม field อื่นๆ ที่ต้องการแสดงในตาราง history
100:                 }).ToList();
101: 
102:                 printJobBindingSource.DataSource = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(jobRows);
103:             }
104:             catch (Exception ex)
105:             {
106:                 MessageBox.Show($"โหลดข้อมูลประวัติไม่สำเร็จ: {ex.Message}", "Error");
107:             }
108:         }
109: 
110:     
111:     }
112: }
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
16: 
17:         public string Program { get; set; } = "";
18:     }
19: }
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
  1: using System.Net.Http;
  2: using System.Net.Http.Json;
  3: using System.Text;
  4: using System.Text.Json;
  5: using System.Text.Json.Serialization;
  6: using System.Net.Http.Headers; // เพิ่มบรรทัดนี้ด้านบนสุด
  7: using InkjetOperator.Models;
  8: 
  9: namespace InkjetOperator.Services;
 10: 
 11: /// <summary>
 12: /// HTTP client for the InkjetBackend REST API.
 13: /// Polls for pending jobs, fetches resolved patterns, posts results.
 14: /// </summary>
 15: public class ApiClient
 16: {
 17:     private readonly HttpClient _http;
 18:     private readonly string _baseUrl;
 19: 
 20:     private static readonly JsonSerializerOptions JsonOptions = new()
 21:     {
 22:         PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
 23:         PropertyNameCaseInsensitive = true,
 24:     };
 25: 
 26:     public ApiClient(string baseUrl)
 27:     {
 28:         _baseUrl = baseUrl.TrimEnd('/');
 29:         _http = new HttpClient
 30:         {
 31:             BaseAddress = new Uri(_baseUrl),
 32:             Timeout = TimeSpan.FromSeconds(10),
 33:         };
 34:     }
 35: 
 36:     public async Task<bool> CreateLastSentJobAsync(object jobData)
 37:     {
 38:         try
 39:         {
 40:             // ใช้ PostAsJsonAsync และส่ง JsonOptions (SnakeCase) เข้าไปด้วย
 41:             var response = await _http.PostAsJsonAsync("/job/lastSent/create", jobData, JsonOptions);
 42: 
 43:             if (!response.IsSuccessStatusCode)
 44:             {
 45:                 string errorBody = await response.Content.ReadAsStringAsync();
 46:                 Console.WriteLine($"CreateLastSentJob failed: {response.StatusCode} - {errorBody}");
 47:                 return false;
 48:             }
 49: 
 50:             return true;
 51:         }
 52:         catch (Exception ex)
 53:         {
 54:             Console.WriteLine("CreateLastSentJob error: " + ex.Message);
 55:             return false;
 56:         }
 57:     }
 58: 
 59:     /// <summary>
 60:     /// GET /job/lastSent/getAll?st_status=sent&page=1&limit=10
 61:     /// ดึงรายการประวัติการส่งงานล่าสุดพร้อมระบบแบ่งหน้า
 62:     /// </summary>
 63:     public async Task<List<PrintJob>> GetLastSentJobsAsync(string stStatus = "sent", int page = 1, int limit = 10)
 64:     {
 65:         try
 66:         {
 67:             // สร้าง Query String สำหรับ Filter และ Pagination
 68:             var url = $"/job/lastSent/getAll?st_status={stStatus}&page={page}&limit={limit}";
 69: 
 70:             var response = await _http.GetAsync(url);
 71: 
 72:             if (!response.IsSuccessStatusCode)
 73:             {
 74:                 string errorBody = await response.Content.ReadAsStringAsync();
 75:                 Console.WriteLine($"GetLastSentJobs failed: {response.StatusCode} - {errorBody}");
 76:                 return new List<PrintJob>();
 77:             }
 78: 
 79:             // ใช้ ReadFromJsonAsync พร้อม JsonOptions (SnakeCase) เหมือน GetPendingJobsAsync
 80:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PrintJob>>>(JsonOptions);
 81: 
 82:             return wrapper?.Data?.Data ?? new List<PrintJob>();
 83:         }
 84:         catch (Exception ex)
 85:         {
 86:             Console.WriteLine("GetLastSentJobs error: " + ex.Message);
 87:             return new List<PrintJob>();
 88:         }
 89:     }
 90: 
 91:     /// <summary>GET /job/getAll?status=pending — polled every 5s by frmMain timer.</summary>
 92:     public async Task<List<PrintJob>> GetPendingJobsAsync()
 93:     {
 94:         try
 95:         {
 96:             var response = await _http.GetAsync("/job/getAll?status=pending");
 97:             response.EnsureSuccessStatusCode();
 98: 
 99:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResult<PrintJob>>>(JsonOptions);
100:             return wrapper?.Data?.Data ?? new List<PrintJob>();
101:         }
102:         catch (Exception ex)
103:         {
104:             Console.WriteLine("GetPendingJobs error: " + ex.Message);
105:             return new List<PrintJob>();
106:         }
107:     }
108: 
109:     /// <summary>GET /job/getById/:id</summary>
110:     public async Task<PrintJob?> GetJobByIdAsync(int jobId)
111:     {
112:         try
113:         {
114:             var response = await _http.GetAsync($"/job/getById/{jobId}");
115:             response.EnsureSuccessStatusCode();
116: 
117:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
118:             return wrapper?.Data;
119:         }
120:         catch (Exception ex)
121:         {
122:             Console.WriteLine("GetJobById error: " + ex.Message);
123:             return null;
124:         }
125:     }
126: 
127:     /// <summary>GET /job/getResolved/:id — returns job + pattern with resolved templates.</summary>
128:     public async Task<ResolvedJobResponse?> GetResolvedJobAsync(int jobId)
129:     {
130:         try
131:         {
132:             var response = await _http.GetAsync($"/job/getResolved/{jobId}");
133:             response.EnsureSuccessStatusCode();
134: 
135:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<ResolvedJobResponse>>(JsonOptions);
136:             return wrapper?.Data;
137:         }
138:         catch (Exception ex)
139:         {
140:             Console.WriteLine("GetResolvedJob error: " + ex.Message);
141:             return null;
142:         }
143:     }
144: 
145:     /// <summary>POST /job/execute/:id — marks job as executing.</summary>
146:     public async Task<bool> ExecuteJobAsync(int jobId)
147:     {
148:         try
149:         {
150:             var response = await _http.PostAsync($"/job/execute/{jobId}", null);
151:             response.EnsureSuccessStatusCode();
152:             return true;
153:         }
154:         catch (Exception ex)
155:         {
156:             Console.WriteLine("ExecuteJob error: " + ex.Message);
157:             return false;
158:         }
159:     }
160: 
161:     /// <summary>POST /job/postResults/:id — posts command results back to backend.</summary>
162:     public async Task<bool> PostResultsAsync(int jobId, JobResultsPayload results)
163:     {
164:         try
165:         {
166:             var response = await _http.PostAsJsonAsync($"/job/postResults/{jobId}", results, JsonOptions);
167:             response.EnsureSuccessStatusCode();
168:             return true;
169:         }
170:         catch (Exception ex)
171:         {
172:             Console.WriteLine("PostResults error: " + ex.Message);
173:             return false;
174:         }
175:     }
176: 
177:     /// <summary>POST /job/retry/:id — resets failed job to pending.</summary>
178:     public async Task<bool> RetryJobAsync(int jobId)
179:     {
180:         try
181:         {
182:             var response = await _http.PostAsync($"/job/retry/{jobId}", null);
183:             response.EnsureSuccessStatusCode();
184:             return true;
185:         }
186:         catch (Exception ex)
187:         {
188:             Console.WriteLine("RetryJob error: " + ex.Message);
189:             return false;
190:         }
191:     }
192: 
193:     /// <summary>POST /job/create — create new job (with improved error logging).</summary>
194:     public async Task<PrintJob?> CreateJobAsync(CreateJobRequest newJob)
195:     {
196:         try
197:         {
198:             var response = await _http.PostAsJsonAsync("/job/create", newJob, JsonOptions);
199: 
200:             if (!response.IsSuccessStatusCode)
201:             {
202:                 string body = await response.Content.ReadAsStringAsync();
203:                 Console.WriteLine($"CreateJob failed: {(int)response.StatusCode} {response.ReasonPhrase} - Body: {body}");
204:                 return null;
205:             }
206: 
207:             var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PrintJob>>(JsonOptions);
208:             return wrapper?.Data;
209:         }
210:         catch (Exception ex)
211:         {
212:             Console.WriteLine("CreateJob error: " + ex.Message);
213:             return null;
214:         }
215:     }
216: }
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
 31:     [DllImport("user32.dll")]
 32:     static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop,
 33:         IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);
 34: 
 35:     private static int stepCount = 0;
 36: 
 37:     private static string sourcePath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\document_backup";
 38:     private static string targetPath = @"\\DESKTOP-KGODCT5\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\document";
 39: 
 40:     const int SW_MAXIMIZE = 3;
 41:     const int MOUSE_DOWN = 0x02, MOUSE_UP = 0x04;
 42:     const int CURSOR_SHOWING = 0x00000001;
 43:     const int DI_NORMAL = 0x0003;
 44: 
 45:     [StructLayout(LayoutKind.Sequential)]
 46:     private struct CURSORINFO
 47:     {
 48:         public int cbSize;
 49:         public int flags;
 50:         public IntPtr hCursor;
 51:         public Point ptScreenPos;
 52:     }
 53: 
 54:     [StructLayout(LayoutKind.Sequential)]
 55:     private struct RECT
 56:     {
 57:         public int Left, Top, Right, Bottom;
 58:         public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
 59:     }
 60: 
 61:     private static IntPtr _targetHWnd = IntPtr.Zero;
 62: 
 63:     /// <summary>
 64:     /// ภาพล่าสุดที่แคปได้ — ให้ form ดึงไปแสดงใน PictureBox ได้
 65:     /// (เหมือน pictureBox1.Image = img ในโค้ดเก่า)
 66:     /// </summary>
 67:     public static Bitmap LastCapture { get; private set; }
 68: 
 69:     // ─── Public API ───
 70: 
 71:     /// <summary>
 72:     /// รันทั้ง bot flow: sleep 3s → activate → click steps ตามลำดับ
 73:     /// เรียกจาก background thread เท่านั้น
 74:     /// (ตรงตามโค้ดเก่า RunBot())
 75:     /// </summary>
 76:     public static BotResult Run(string processName, BotStep[] steps)
 77:     {
 78:         Thread.Sleep(3000); // รอ UI พร้อม (เหมือนโค้ดเก่า)
 79: 
 80:         if (!ActivateWindow(processName, out string error))
 81:             return BotResult.Fail(error);
 82: 
 83:         Thread.Sleep(2000); // รอ UI พร้อม
 84: 
 85:         foreach (var step in steps)
 86:         {
 87:             if (!ClickAndVerify(step))
 88:                 return BotResult.Fail($"{step.Name} failed after {step.MaxRetry} retries");
 89:             Thread.Sleep(step.DelayAfter);
 90:         }
 91: 
 92:         return BotResult.Ok();
 93:     }
 94: 
 95:     /// <summary>
 96:     /// รัน bot flow แบบ async — ไม่บล็อก UI thread
 97:     /// </summary>
 98:     public static void RunAsync(string processName, BotStep[] steps, Action<BotResult> onComplete = null)
 99:     {
100:         var t = new Thread(() =>
101:         {
102:             var result = Run(processName, steps);
103:             onComplete?.Invoke(result);
104:         })
105:         { IsBackground = true };
106:         t.Start();
107:     }
108: 
109:     // ─── Screen Capture ───
110: 
111:     /// <summary>
112:     /// แคปทั้งจอ primary monitor พร้อมวาด cursor ลงภาพ
113:     /// </summary>
114:     public static Bitmap CaptureScreen()
115:     {
116:         Rectangle bounds = Screen.PrimaryScreen.Bounds;
117:         var bmp = new Bitmap(bounds.Width, bounds.Height);
118:         using (var g = Graphics.FromImage(bmp))
119:         {
120:             g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
121:             DrawCursor(g, bounds.Location);
122:         }
123:         return bmp;
124:     }
125: 
126:     /// <summary>
127:     /// แคปเฉพาะ area ที่กำหนด (screen coordinates) พร้อมวาด cursor ถ้าอยู่ใน area
128:     /// </summary>
129:     public static Bitmap CaptureArea(Rectangle area)
130:     {
131:         try
132:         {
133:             // ❌ ถ้า area ไม่ถูกต้อง → fallback ไปแคปทั้งจอแทน
134:             if (area.Width <= 0 || area.Height <= 0)
135:             {
136:                 Console.WriteLine("⚠️ Invalid area → fallback to full screen");
137:                 return CaptureScreen();
138:             }
139: 
140:             Rectangle allScreens = Rectangle.Empty;
141:             foreach (var screen in Screen.AllScreens)
142:             {
143:                 allScreens = Rectangle.Union(allScreens, screen.Bounds);
144:             }
145: 
146:             Rectangle validArea = Rectangle.Intersect(area, allScreens);
147: 
148:             // ❌ ถ้า intersect แล้วพัง → fallback
149:             if (validArea.Width <= 0 || validArea.Height <= 0)
150:             {
151:                 Console.WriteLine("⚠️ Area out of screen → fallback to full screen");
152:                 return CaptureScreen();
153:             }
154: 
155:             var bmp = new Bitmap(validArea.Width, validArea.Height);
156: 
157:             using (var g = Graphics.FromImage(bmp))
158:             {
159:                 g.CopyFromScreen(validArea.Location, Point.Empty, validArea.Size);
160:                 DrawCursor(g, validArea.Location);
161:             }
162: 
163:             return bmp;
164:         }
165:         catch (Exception ex)
166:         {
167:             // ❌ กัน crash ทุกกรณี
168:             Console.WriteLine("❌ CaptureArea error: " + ex.Message);
169: 
170:             return CaptureScreen(); // fallback
171:         }
172:     }
173: 
174:     /// <summary>
175:     /// แคปเฉพาะหน้าต่างโปรแกรมเป้าหมาย (ไม่รวม desktop/taskbar) พร้อมวาด cursor
176:     /// </summary>
177:     public static Bitmap CaptureTargetWindow()
178:     {
179:         if (_targetHWnd == IntPtr.Zero)
180:             return null;
181: 
182:         if (!GetWindowRect(_targetHWnd, out RECT rect))
183:             return null;
184: 
185:         var windowRect = rect.ToRectangle();
186:         if (windowRect.Width <= 0 || windowRect.Height <= 0)
187:             return null;
188: 
189:         var bmp = new Bitmap(windowRect.Width, windowRect.Height);
190:         using (var g = Graphics.FromImage(bmp))
191:         {
192:             g.CopyFromScreen(windowRect.Location, Point.Empty, windowRect.Size);
193:             DrawCursor(g, windowRect.Location);
194:         }
195:         return bmp;
196:     }
197: 
198:     // ─── Core ───
199: 
200:     private static bool ActivateWindow(string processName, out string error)
201:     {
202:         error = null;
203:         var procs = Process.GetProcessesByName(processName);
204:         if (procs.Length == 0) { error = $"Process '{processName}' not found"; return false; }
205: 
206:         IntPtr hWnd = procs[0].MainWindowHandle;
207:         if (hWnd == IntPtr.Zero) { error = "Window handle not found"; return false; }
208: 
209:         _targetHWnd = hWnd;
210: 
211:         ShowWindow(hWnd, SW_MAXIMIZE);
212:         BringWindowToTop(hWnd);
213:         SetForegroundWindow(hWnd);
214:         return true;
215:     }
216: 
217:     /// <summary>
218:     /// Click → compare before/after area → SaveCapture ทั้งจอ → retry ถ้าภาพไม่เปลี่ยน
219:     /// (ตรงตามโค้ดเก่า ClickAndVerify())
220:     /// </summary>
221:     private static bool ClickAndVerify(BotStep step)
222:     {
223:         for (int i = 1; i <= step.MaxRetry; i++)
224:         {
225:             Console.WriteLine($"[{step.Name}] Try {i}");
226: 
227:             // แคป area ก่อน click
228:             Bitmap before = CaptureArea(step.VerifyArea);
229: 
230:             LeftClick(step.X, step.Y);
231: 
232:             Thread.Sleep(step.VerifyDelay);
233: 
234:             // แคป area หลัง click
235:             Bitmap after = CaptureArea(step.VerifyArea);
236: 
237:             // เปรียบเทียบแบบ exact (pixel ต่างเมื่อไหร่ = เปลี่ยน)
238:             bool same = CompareBitmapsFast(before, after);
239: 
240:             // save ทั้งจอลง capture_log (เหมือนโค้ดเก่า)
241:             SaveCapture($"{step.Name}_Try{i}");
242: 
243:             before.Dispose();
244:             after.Dispose();
245: 
246:             if (!same)
247:             {
248:                 Console.WriteLine($"[{step.Name}] ✅ Success");
249:                 return true;
250:             }
251: 
252:             Console.WriteLine($"[{step.Name}] ❌ Retry...");
253:             Thread.Sleep(1000);
254:         }
255: 
256:         Console.WriteLine($"[{step.Name}] ❌ Failed");
257:         return false;
258:     }
259: 
260:     /// <summary>
261:     /// เปรียบเทียบ pixel แบบ fast — เหมือนโค้ดเก่า CompareBitmapsFast()
262:     /// สุ่มทุก 5 pixel, pixel ต่างตัวเดียว = return false (ภาพเปลี่ยน)
263:     /// </summary>
264:     public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
265:     {
266:         if (bmp1.Size != bmp2.Size)
267:             return false;
268: 
269:         for (int x = 0; x < bmp1.Width; x += 5)
270:             for (int y = 0; y < bmp1.Height; y += 5)
271:                 if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
272:                     return false;
273: 
274:         return true;
275:     }
276: 
277:     /// <summary>
278:     /// แคปทั้งจอ → บันทึกลง capture_log/ → เก็บ LastCapture
279:     /// (เหมือนโค้ดเก่า SaveCapture() ที่เรียก CaptureScreen + pictureBox1.Image = img)
280:     /// </summary>
281:     private static void SaveCapture(string stepName)
282:     {
283:         Bitmap img = CaptureScreen();
284: 
285:         var folder = System.IO.Path.Combine(Application.StartupPath, "capture_log");
286:         System.IO.Directory.CreateDirectory(folder);
287: 
288:         var path = System.IO.Path.Combine(
289:             folder, $"{stepName}_{DateTime.Now:HHmmss}.png");
290:         img.Save(path, ImageFormat.Png);
291: 
292:         // เก็บภาพล่าสุดให้ form ดึงไปแสดง
293:         LastCapture?.Dispose();
294:         LastCapture = img;
295:     }
296: 
297:     private static void LeftClick(int x, int y)
298:     {
299:         Cursor.Position = new Point(x, y);
300:         Thread.Sleep(200);
301:         mouse_event(MOUSE_DOWN, x, y, 0, 0);
302:         Thread.Sleep(100);
303:         mouse_event(MOUSE_UP, x, y, 0, 0);
304:     }
305: 
306:     /// <summary>
307:     /// วาด cursor ปัจจุบันลงบน Graphics (ตำแหน่ง offset จาก captureOrigin)
308:     /// </summary>
309:     private static void DrawCursor(Graphics g, Point captureOrigin)
310:     {
311:         var ci = new CURSORINFO();
312:         ci.cbSize = Marshal.SizeOf(ci);
313: 
314:         if (!GetCursorInfo(ref ci))
315:             return;
316: 
317:         if ((ci.flags & CURSOR_SHOWING) == 0)
318:             return;
319: 
320:         IntPtr hdc = g.GetHdc();
321:         try
322:         {
323:             int x = ci.ptScreenPos.X - captureOrigin.X;
324:             int y = ci.ptScreenPos.Y - captureOrigin.Y;
325:             DrawIconEx(hdc, x, y, ci.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
326:         }
327:         finally
328:         {
329:             g.ReleaseHdc(hdc);
330:         }
331:     }
332: 
333:     public static void CopyFile(string fileName)
334:     {
335:         try
336:         {
337:             string sourceFile = Path.Combine(sourcePath, fileName);
338:             string targetFile = Path.Combine(targetPath, fileName);
339: 
340:             if (File.Exists(sourceFile))
341:             {
342:                 File.Copy(sourceFile, targetFile, true);
343:                 Console.WriteLine($"✅ Copy สำเร็จ: {fileName}");
344:             }
345:             else
346:             {
347:                 Console.WriteLine($"❌ ไม่พบไฟล์: {fileName}");
348:             }
349:         }
350:         catch (Exception ex)
351:         {
352:             Console.WriteLine($"❌ Error: {ex.Message}");
353:         }
354: 
355:         // เพิ่ม step ทุกครั้งที่เรียก
356:         stepCount++;
357: 
358:         // ถ้าครบ 4 step → ลบไฟล์
359:         if (stepCount >= 4)
360:         {
361:             ClearDocumentFolder();
362:             stepCount = 0; // reset ใหม่
363:         }
364:     }
365: 
366:     public static void ClearDocumentFolder()
367:     {
368:         try
369:         {
370:             var files = Directory.GetFiles(targetPath);
371: 
372:             foreach (var file in files)
373:             {
374:                 File.Delete(file);
375:             }
376: 
377:             Console.WriteLine("🧹 ลบไฟล์ใน document เรียบร้อยแล้ว");
378:         }
379:         catch (Exception ex)
380:         {
381:             Console.WriteLine($"❌ ลบไฟล์ไม่สำเร็จ: {ex.Message}");
382:         }
383:     }
384: 
385:     public static void WaitForFileReady(string filePath, int timeoutMs = 5000)
386:     {
387:         var start = DateTime.Now;
388: 
389:         while (true)
390:         {
391:             try
392:             {
393:                 using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
394:                 {
395:                     break;
396:                 }
397:             }
398:             catch
399:             {
400:                 if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
401:                     throw new Exception("File not ready (timeout): " + filePath);
402: 
403:                 Thread.Sleep(200);
404:             }
405:         }
406:     }
407: }
408: 
409: // ─── Models ───
410: 
411: public class BotStep
412: {
413:     public string Name { get; set; }
414:     public int X { get; set; }
415:     public int Y { get; set; }
416:     public Rectangle VerifyArea { get; set; }
417:     public int MaxRetry { get; set; } = 3;
418:     public int VerifyDelay { get; set; } = 1500;
419:     public int DelayAfter { get; set; } = 1000;
420: }
421: 
422: public class BotResult
423: {
424:     public bool Success { get; set; }
425:     public string Error { get; set; }
426: 
427:     public static BotResult Ok() => new BotResult { Success = true };
428:     public static BotResult Fail(string err) => new BotResult { Success = false, Error = err };
429: }
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
