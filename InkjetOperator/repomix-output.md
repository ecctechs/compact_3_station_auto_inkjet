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
 225:             LoadLastSentData();
 226:             UpdateDeviceStatus();
 227:             lblApiStatus.Text = "OK";
 228:         }
 229:         catch (Exception ex)
 230:         {
 231:             lblApiStatus.Text = "Error";
 232:             Log("Poll error: " + ex.Message);
 233:         }
 234:         finally
 235:         {
 236:             tmrPoll.Start();
 237:         }
 238:     }
 239: 
 240:     private void btnRefresh_Click(object sender, EventArgs e)
 241:     {
 242:         tmrPoll_Tick(sender, e);
 243:     }
 244: 
 245:     // ════════════════════════════════════════
 246:     //  Job selection
 247:     // ════════════════════════════════════════
 248:     private async void dgvJobs_SelectionChanged(object sender, EventArgs e)
 249:     {
 250:         if (_isRefreshingJobs) return; // 🔥 กันตอน poll
 251: 
 252:         if (dgvJobs.SelectedRows.Count == 0) return;
 253: 
 254:         var row = dgvJobs.SelectedRows[0];
 255:         if (row.Cells["Id"].Value == null) return;
 256: 
 257:         int jobId = (int)row.Cells["Id"].Value;
 258:         string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
 259: 
 260:         if (jobId == _selectedJobId) return;
 261:         _selectedJobId = jobId;
 262: 
 263:         // Helpers
 264:         static bool ReaderHasColumn(SqliteDataReader r, string name)
 265:         {
 266:             for (int i = 0; i < r.FieldCount; i++)
 267:             {
 268:                 if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
 269:                     return true;
 270:             }
 271:             return false;
 272:         }
 273: 
 274:         static string? GetStringSafe(SqliteDataReader r, string name)
 275:         {
 276:             if (!ReaderHasColumn(r, name)) return null;
 277:             int idx = r.GetOrdinal(name);
 278:             return r.IsDBNull(idx) ? null : r.GetString(idx);
 279:         }
 280: 
 281:         static int? GetIntSafe(SqliteDataReader r, string name)
 282:         {
 283:             var s = GetStringSafe(r, name);
 284:             if (string.IsNullOrWhiteSpace(s)) return null;
 285:             return int.TryParse(s, out var v) ? v : null;
 286:         }
 287: 
 288:         try
 289:         {
 290:             string dbPath = @"D:\DB\uv_data.db3";
 291:             string connStr = $"Data Source={dbPath}";
 292:             bool found = false;
 293: 
 294:             try
 295:             {
 296:                 await using var conn = new SqliteConnection(connStr);
 297:                 await conn.OpenAsync();
 298: 
 299:                 await using var cmd = conn.CreateCommand();
 300:                 cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
 301:                 cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
 302: 
 303:                 await using var reader = await cmd.ExecuteReaderAsync();
 304:                 if (await reader.ReadAsync())
 305:                 {
 306:                     // Build PatternDetail from known schema columns.
 307:                     var pattern = new PatternDetail
 308:                     {
 309:                         Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
 310:                         Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
 311:                     };
 312: 
 313:                     // Helper to build MK config (mk1 / mk2)
 314:                     InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
 315:                     {
 316:                         var cfg = new InkjetConfigDto
 317:                         {
 318:                             Ordinal = ordinal,
 319:                             ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
 320:                             ProgramName = GetStringSafe(reader, programNameColumn),
 321:                             Width = GetIntSafe(reader, $"{prefix}width"),
 322:                             Height = GetIntSafe(reader, $"{prefix}height"),
 323:                             TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
 324:                             Direction = GetIntSafe(reader, $"{prefix}text_direction"),
 325:                             SteelType = null,
 326:                             Suspended = false
 327:                         };
 328: 
 329:                         // text blocks 1..5
 330:                         for (int b = 1; b <= 5; b++)
 331:                         {
 332:                             string textCol = $"{prefix}block{b}_text";
 333:                             if (ReaderHasColumn(reader, textCol))
 334:                             {
 335:                                 var text = GetStringSafe(reader, textCol);
 336:                                 if (!string.IsNullOrEmpty(text))
 337:                                 {
 338:                                     var tb = new TextBlockDto
 339:                                     {
 340:                                         BlockNumber = b,
 341:                                         Text = text,
 342:                                         X = GetIntSafe(reader, $"{prefix}block{b}_x"),
 343:                                         Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
 344:                                         Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
 345:                                         Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
 346:                                     };
 347:                                     cfg.TextBlocks.Add(tb);
 348:                                 }
 349:                             }
 350:                         }
 351: 
 352:                         return cfg;
 353:                     }
 354: 
 355:                     // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
 356:                     var mk1 = BuildMkConfig("mk1_", 1, "program_name");
 357:                     // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
 358:                     var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
 359: 
 360:                     pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
 361: 
 362:                     // Conveyor speeds / servo configs - attempt to map if available (optional)
 363:                     var spd1 = GetIntSafe(reader, "belt1_inkjet");
 364:                     var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
 365:                     if (spd1.HasValue || spd2.HasValue)
 366:                     {
 367:                         pattern.ConveyorSpeeds = new ConveyorSpeedDto
 368:                         {
 369:                             Speed1 = spd1,
 370:                             Speed2 = spd2,
 371:                             Speed3 = GetIntSafe(reader, "belt3")
 372:                         };
 373:                     }
 374: 
 375:                     // Minimal job + resolved response
 376:                     _currentResolved = new ResolvedJobResponse
 377:                     {
 378:                         Job = new PrintJob
 379:                         {
 380:                             Id = jobId,
 381:                             BarcodeRaw = rawbarcode,
 382:                             LotNumber = row.Cells["LotNumber"].Value?.ToString(),
 383:                             Status = row.Cells["Status"].Value?.ToString()
 384:                         },
 385:                         Pattern = pattern
 386:                     };
 387: 
 388:                     string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
 389:                     string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ
 390: 
 391:                     // 🔥 background + กันค้าง
 392:                     //_ = Task.Run(async () =>
 393:                     //{
 394:                     //    try
 395:                     //    {
 396:                     //        await UpdateUvData(lot, name);
 397:                     //    }
 398:                     //    catch (Exception ex)
 399:                     //    {
 400:                     //        Log("UV DB error: " + ex.Message);
 401:                     //    }
 402:                     //});
 403: 
 404:                     found = true;
 405:                     UpdateDetailPanel();
 406:                 }
 407: 
 408:                 await conn.CloseAsync();
 409:             }
 410:             catch (Exception dbEx)
 411:             {
 412:                 Log("SQLite query error: " + dbEx.Message);
 413:             }
 414: 
 415:             if (!found)
 416:             {
 417:                 var resolved = await _api.GetResolvedJobAsync(jobId);
 418:                 _currentResolved = resolved;
 419:                 UpdateDetailPanel();
 420:             }
 421:         }
 422:         catch (Exception ex)
 423:         {
 424:             Log("Load job detail error: " + ex.Message);
 425:         }
 426:     }
 427: 
 428:     private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
 429:     {
 430:         if (dgvConfigs.SelectedRows.Count == 0 || _currentResolved?.Pattern == null) return;
 431: 
 432:         int idx = dgvConfigs.SelectedRows[0].Index;
 433:         var configs = _currentResolved.Pattern.InkjetConfigs;
 434: 
 435:         if (configs == null || idx >= configs.Count) return;
 436: 
 437:         var config = configs[idx];
 438: 
 439:         // dgvTextBlocks แสดงตามแถวที่เลือก (ปกติคือ MK1, MK2)
 440:         //dgvTextBlocks.DataSource = null;
 441:         _textBlockBindingList.RaiseListChangedEvents = false;
 442:         _textBlockBindingList.Clear();
 443: 
 444:         //foreach (var b in config.TextBlocks ?? new List<TextBlockDto>())
 445:         //{
 446:         //    _textBlockBindingList.Add(b);
 447:         //}
 448: 
 449:         foreach (var block in config.TextBlocks)
 450:         {
 451:             // --- ส่วนที่แก้ไข/เพิ่มใหม่ ---
 452:             // ใช้ LINQ ค้นหา Pattern ที่ชื่อตรงกับ block.Text (เช่น "DDDD")
 453:             //var pattern = PatternEngine.Patterns.FirstOrDefault(p => p.Name == block.Text);
 454:             string previewText = PatternEngine.Process(txtLot.Text, block.Text);
 455:             //Debug.WriteLine(block.Text);
 456: 
 457:             if (previewText != null)
 458:             {
 459:                 // ถ้าเจอ ให้เอาค่าจาก txtLot (หรือ res.Job.LotNumber) มาแปลงด้วย Rule
 460:                 block.RuleResult = previewText;
 461:             }
 462:             else
 463:             {
 464:                 // ถ้าไม่เจอ ให้แสดงค่าเดิมของมัน
 465:                 block.RuleResult = block.Text;
 466:             }
 467:             // --------------------------
 468: 
 469:             _textBlockBindingList.Add(block);
 470:         }
 471: 
 472:         _textBlockBindingList.RaiseListChangedEvents = true;
 473:         _textBlockBindingList.ResetBindings();
 474: 
 475:         // --- นำ UpdateUvGridOnly(config.TextBlocks) ออก ---
 476:         // เพื่อให้ dgvUVBlocks ไม่เปลี่ยนค่าตามการเลือกใน Grid นี้
 477:     }
 478: 
 479:     // ════════════════════════════════════════
 480:     //  Send to devices
 481:     // ════════════════════════════════════════
 482: 
 483:     private async void btnSend_Click(object sender, EventArgs e)
 484:     {
 485:         if (_currentResolved == null || _selectedJobId < 0)
 486:         {
 487:             MessageBox.Show("No job selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
 488:             return;
 489:         }
 490: 
 491:         btnSend.Enabled = false;
 492:         var commandResults = new List<CommandResult>();
 493: 
 494:         try
 495:         {
 496:             // Mark job as executing
 497:             bool ok = await _api.ExecuteJobAsync(_selectedJobId);
 498:             if (!ok)
 499:             {
 500:                 Log("Failed to mark job as executing.");
 501:                 btnSend.Enabled = true;
 502:                 return;
 503:             }
 504: 
 505:             // Re-fetch resolved data (templates may depend on attempt number)
 506:             var resolved = await _api.GetResolvedJobAsync(_selectedJobId);
 507:             if (resolved?.Pattern == null)
 508:             {
 509:                 Log("Failed to get resolved job.");
 510:                 btnSend.Enabled = true;
 511:                 return;
 512:             }
 513: 
 514:             _currentResolved = resolved;
 515:             UpdateDetailPanel();
 516: 
 517:             // Send to each inkjet by ordinal
 518:             var configs = resolved.Pattern.InkjetConfigs ?? new List<InkjetConfigDto>();
 519:             bool hasError = false;
 520: 
 521:             foreach (var config in configs.OrderBy(c => c.Ordinal))
 522:             {
 523:                 if (config.Suspended)
 524:                 {
 525:                     Log($"IJ{config.Ordinal} suspended — skipping.");
 526:                     continue;
 527:                 }
 528: 
 529:                 IInkjetAdapter adapter = GetAdapterByOrdinal(config.Ordinal);
 530: 
 531:                 if (!adapter.IsConnected())
 532:                 {
 533:                     Log($"IJ{config.Ordinal} not connected — skipping.");
 534:                     commandResults.Add(new CommandResult
 535:                     {
 536:                         Ordinal = config.Ordinal,
 537:                         Command = "connect_check",
 538:                         Success = false,
 539:                         Response = "Not connected",
 540:                         SentAt = DateTime.UtcNow.ToString("o"),
 541:                     });
 542:                     hasError = true;
 543:                     break; // Sequential error — stop (csv_extractor.py line 281)
 544:                 }
 545: 
 546:                 // 1. Change program
 547:                 if (config.ProgramNumber.HasValue)
 548:                 {
 549:                     var r = await adapter.ChangeProgramAsync(config.ProgramNumber.Value);
 550:                     r.Ordinal = config.Ordinal;
 551:                     commandResults.Add(r);
 552:                     Log($"IJ{config.Ordinal} ChangeProgram({config.ProgramNumber}): {(r.Success ? "OK" : "FAIL")}");
 553: 
 554:                     if (!r.Success) { hasError = true; break; }
 555:                 }
 556: 
 557:                 // 2. Send config
 558:                 var cfgResult = await adapter.SendConfigAsync(config);
 559:                 cfgResult.Ordinal = config.Ordinal;
 560:                 commandResults.Add(cfgResult);
 561:                 Log($"IJ{config.Ordinal} SendConfig: {(cfgResult.Success ? "OK" : "FAIL")}");
 562: 
 563:                 if (!cfgResult.Success) { hasError = true; break; }
 564: 
 565:                 // 3. Send text blocks (device blocks 6-10)
 566:                 var blocks = config.TextBlocks ?? new List<TextBlockDto>();
 567:                 foreach (var block in blocks.OrderBy(b => b.BlockNumber))
 568:                 {
 569:                     int deviceBlock = block.BlockNumber + 5; // blocks 6-10
 570:                     var tbResult = await adapter.SendTextBlockAsync(block, deviceBlock);
 571:                     tbResult.Ordinal = config.Ordinal;
 572:                     commandResults.Add(tbResult);
 573:                     Log($"IJ{config.Ordinal} TextBlock({block.BlockNumber}): {(tbResult.Success ? "OK" : "FAIL")}");
 574: 
 575:                     if (!tbResult.Success) { hasError = true; break; }
 576:                     await Task.Delay(30); // 30ms delay between blocks (csv_extractor.py line 274)
 577:                 }
 578: 
 579:                 if (hasError) break;
 580: 
 581:                 // 4. Resume printing
 582:                 var resumeResult = await adapter.ResumeAsync();
 583:                 resumeResult.Ordinal = config.Ordinal;
 584:                 commandResults.Add(resumeResult);
 585:                 Log($"IJ{config.Ordinal} Resume: {(resumeResult.Success ? "OK" : "FAIL")}");
 586: 
 587:                 if (!resumeResult.Success) { hasError = true; break; }
 588:             }
 589: 
 590:             // Send conveyor speed + servo to PLC
 591:             if (!hasError && resolved.Pattern.ConveyorSpeeds != null)
 592:             {
 593:                 var spd = resolved.Pattern.ConveyorSpeeds;
 594:                 await _plc.WriteSpeedAsync(spd.Speed1 ?? 0, spd.Speed2 ?? 0, spd.Speed3 ?? 0);
 595:                 Log($"PLC speed: {spd.Speed1}/{spd.Speed2}/{spd.Speed3}");
 596:             }
 597: 
 598:             if (!hasError && resolved.Pattern.ServoConfigs != null)
 599:             {
 600:                 foreach (var servo in resolved.Pattern.ServoConfigs)
 601:                 {
 602:                     await _plc.WriteServoAsync(servo.Ordinal, servo.Position ?? 0, servo.PostAct ?? 0, servo.Delay ?? 0, servo.Trigger ?? 0);
 603:                     Log($"PLC servo ordinal={servo.Ordinal}: pos={servo.Position}");
 604:                 }
 605:             }
 606: 
 607:             // Post results back to backend
 608:             var payload = new JobResultsPayload
 609:             {
 610:                 Success = !hasError,
 611:                 ErrorMessage = hasError ? "One or more commands failed" : null,
 612:                 Commands = commandResults,
 613:             };
 614: 
 615:             await _api.PostResultsAsync(_selectedJobId, payload);
 616:             Log(hasError ? "Job completed with errors." : "Job completed successfully.");
 617:         }
 618:         catch (Exception ex)
 619:         {
 620:             Log("Send error: " + ex.Message);
 621:         }
 622:         finally
 623:         {
 624:             btnSend.Enabled = true;
 625:         }
 626:     }
 627: 
 628:     private async void btnRetry_Click(object sender, EventArgs e)
 629:     {
 630:         if (_selectedJobId < 0) return;
 631: 
 632:         bool ok = await _api.RetryJobAsync(_selectedJobId);
 633:         Log(ok ? $"Job {_selectedJobId} retried." : $"Retry failed for job {_selectedJobId}.");
 634:     }
 635: 
 636:     // ════════════════════════════════════════
 637:     //  Helpers
 638:     // ════════════════════════════════════════
 639: 
 640:     private IInkjetAdapter GetAdapterByOrdinal(int ordinal)
 641:     {
 642:         return ordinal switch
 643:         {
 644:             1 => _adapterIj1,
 645:             2 => _adapterIj2,
 646:             3 => _adapterIj3,
 647:             4 => _adapterIj4,
 648:             _ => _adapterIj1,
 649:         };
 650:     }
 651: 
 652:     private void UpdateDeviceStatus()
 653:     {
 654:         if (InvokeRequired) { Invoke(UpdateDeviceStatus); return; }
 655: 
 656:         lblStatusIj1.BackColor = _adapterIj1.IsConnected() ? Color.Green : Color.Gray;
 657:         lblStatusIj2.BackColor = _adapterIj2.IsConnected() ? Color.Green : Color.Gray;
 658:         lblStatusIj3.BackColor = _adapterIj3.IsConnected() ? Color.Green : Color.Gray;
 659:         lblStatusIj4.BackColor = _adapterIj4.IsConnected() ? Color.Green : Color.Gray;
 660:     }
 661: 
 662:     private void UpdateJobGrid()
 663:     {
 664:         if (InvokeRequired) { Invoke(UpdateJobGrid); return; }
 665: 
 666:         _isRefreshingJobs = true; // 🔥 กัน event
 667:         _selectedJobId = -1;
 668:         int selectedId = -1;
 669:         if (dgvJobs.CurrentRow != null)
 670:         {
 671:             selectedId = (int)dgvJobs.CurrentRow.Cells["Id"].Value;
 672:         }
 673: 
 674:         _jobBindingList.RaiseListChangedEvents = false;
 675:         _jobBindingList.Clear();
 676: 
 677:         foreach (var j in _pendingJobs)
 678:         {
 679:             _jobBindingList.Add(new JobRow
 680:             {
 681:                 Id = j.Id,
 682:                 BarcodeRaw = j.BarcodeRaw,
 683:                 LotNumber = j.LotNumber,
 684:                 Status = j.Status,
 685:                 Attempt = j.Attempt
 686:             });
 687:         }
 688: 
 689:         _jobBindingList.RaiseListChangedEvents = true;
 690:         _jobBindingList.ResetBindings();
 691: 
 692:         // 🔥 restore selection
 693:         if (selectedId != -1)
 694:         {
 695:             foreach (DataGridViewRow row in dgvJobs.Rows)
 696:             {
 697:                 if ((int)row.Cells["Id"].Value == selectedId)
 698:                 {
 699:                     row.Selected = true;
 700:                     dgvJobs.CurrentCell = row.Cells[0];
 701:                     break;
 702:                 }
 703:             }
 704:         }
 705: 
 706:         _isRefreshingJobs = false; // 🔥 เปิด event กลับ
 707:     }
 708: 
 709:     private void UpdateDetailPanel()
 710:     {
 711:         if (InvokeRequired) { Invoke(UpdateDetailPanel); return; }
 712: 
 713:         if (_currentResolved == null) return;
 714: 
 715:         var job = _currentResolved.Job;
 716:         var pattern = _currentResolved.Pattern;
 717: 
 718:         txtBarcode.Text = job?.BarcodeRaw ?? "";
 719:         txtLot.Text = job?.LotNumber ?? "";
 720:         txtStatus.Text = job?.Status ?? "";
 721:         txtPattern.Text = pattern?.Name ?? "";
 722: 
 723:         _configBindingList.RaiseListChangedEvents = false;
 724: 
 725:         var newList = pattern?.InkjetConfigs ?? new List<InkjetConfigDto>();
 726: 
 727:         // 🔥 sync count
 728:         while (_configBindingList.Count > newList.Count)
 729:         {
 730:             _configBindingList.RemoveAt(_configBindingList.Count - 1);
 731:         }
 732: 
 733:         for (int i = 0; i < newList.Count; i++)
 734:         {
 735:             if (i < _configBindingList.Count)
 736:             {
 737:                 // 🔥 update object reference (สำคัญ)
 738:                 _configBindingList[i] = newList[i];
 739:             }
 740:             else
 741:             {
 742:                 _configBindingList.Add(newList[i]);
 743:             }
 744:         }
 745: 
 746:         _configBindingList.RaiseListChangedEvents = true;
 747:         _configBindingList.ResetBindings();
 748: 
 749:         // --- เพิ่มส่วนนี้: Fix ให้ dgvUVBlocks แสดงเฉพาะ UV1 (Ordinal 3) และ UV2 (Ordinal 4) ---
 750:         if (pattern?.InkjetConfigs != null)
 751:         {
 752:             // ดึงเฉพาะ Config ของ UV (Ordinal 3 และ 4)
 753:             var uvConfigs = pattern.InkjetConfigs
 754:                 .Where(c => c.Ordinal == 3 || c.Ordinal == 4)
 755:                 .OrderBy(c => c.Ordinal)
 756:                 .ToList();
 757: 
 758:             // สร้างรายการสำหรับ Display ใน Grid
 759:             var uvDisplayList = new List<object>();
 760:             foreach (var cfg in uvConfigs)
 761:             {
 762:                 string printerName = cfg.Ordinal == 3 ? "เครื่องพิมพ์ UV1" : "เครื่องพิมพ์ UV2";
 763: 
 764:                 // ดึงค่า Block 1 และ 2 (Lot และ Name)
 765:                 string lotVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 1)?.Text ?? "";
 766:                 string nameVal = cfg.TextBlocks.FirstOrDefault(b => b.BlockNumber == 2)?.Text ?? "";
 767: 
 768:                 uvDisplayList.Add(new
 769:                 {
 770:                     Printer = printerName,
 771:                     LotText = lotVal,
 772:                     NameText = nameVal,
 773:                     // เก็บ Reference ไว้เผื่อต้องการดึงไปใช้งานต่อ
 774:                     _originalConfig = cfg
 775:                 });
 776:             }
 777:         }
 778:     }
 779: 
 780:     private void Log(string message)
 781:     {
 782:         if (InvokeRequired) { Invoke(() => Log(message)); return; }
 783: 
 784:         string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
 785:         txtLog.AppendText(line + Environment.NewLine);
 786:     }
 787: 
 788:     private void button1_Click(object sender, EventArgs e)
 789:     {
 790:         var steps = new[]
 791:         {
 792:         new BotStep { Name = "Document",   X = 2325, Y = 59,  VerifyArea = new Rectangle(2100, 0, 400, 300) },
 793:         new BotStep { Name = "Open",       X = 2207, Y = 133, VerifyArea = new Rectangle(2100, 100, 400, 400) },
 794:         new BotStep { Name = "SelectFile", X = 889, Y = 521, VerifyArea = new Rectangle(800, 400, 800, 600) },
 795:         new BotStep { Name = "OpenBtn",    X = 1652, Y = 946, VerifyArea = new Rectangle(1500, 800, 500, 300) },
 796:     };
 797: 
 798:         BotClickHelper.RunAsync("uvinkjet", steps, result =>
 799:         {
 800:             if (this.IsHandleCreated)
 801:                 this.Invoke((MethodInvoker)(() =>
 802:                     MessageBox.Show(result.Success ? "สำเร็จ!" : result.Error)));
 803:         });
 804:     }
 805: 
 806:     private void button2_Click(object sender, EventArgs e)
 807:     {
 808:         // ใช้ Invoke เพื่อความปลอดภัยว่าทำงานบน UI Thread (STA)
 809:         this.Invoke(new Action(() =>
 810:         {
 811:             using (frmPatternEditor editor = new frmPatternEditor())
 812:             {
 813:                 editor.StartPosition = FormStartPosition.CenterParent;
 814:                 editor.ShowDialog();
 815:             }
 816:         }));
 817:     }
 818: 
 819:     private async void button3_Click(object sender, EventArgs e)
 820:     {
 821:         //await SendTextBlocksToIjAsync(3);
 822:         var form = new frmCreateJob(_api);
 823:         form.ShowDialog();
 824:     }
 825: 
 826:     private async void button4_Click(object sender, EventArgs e)
 827:     {
 828:         await TestSendToIj3TcpAsync();
 829:     }
 830: 
 831:     // Example usage: send blocks from dgvTextBlocks to IJ3 via TCP (simple demo)
 832:     private async Task TestSendToIj3TcpAsync()
 833:     {
 834:         // read TCP host/port from UI controls
 835:         string host = txtTcpHost.Text.Trim();
 836:         if (!int.TryParse(txtTcpPort.Text.Trim(), out int port))
 837:         {
 838:             Log("Invalid TCP port");
 839:             return;
 840:         }
 841: 
 842:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
 843:         try
 844:         {
 845:             await client.ConnectAsync(host, port);
 846:         }
 847:         catch (Exception ex)
 848:         {
 849:             Log("TCP connect failed: " + ex.Message);
 850:             return;
 851:         }
 852: 
 853:         // Example: change program to 13
 854:         var progResp = await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, 13);
 855:         Log($"ChangeProgram resp: {progResp}");
 856: 
 857:         // Send each row in dgvTextBlocks
 858:         foreach (DataGridViewRow row in dgvTextBlocks.Rows)
 859:         {
 860:             if (row.IsNewRow) continue;
 861: 
 862:             int blockNumber = 1;
 863:             if (dgvTextBlocks.Columns.Contains("BlockNumber") && int.TryParse(row.Cells["BlockNumber"].Value?.ToString(), out var bn))
 864:                 blockNumber = bn;
 865: 
 866:             string text = row.Cells["Text"].Value?.ToString() ?? row.Cells["text"].Value?.ToString() ?? "";
 867:             int x = int.TryParse(row.Cells["X"].Value?.ToString(), out var tx) ? tx : 0;
 868:             int y = int.TryParse(row.Cells["Y"].Value?.ToString(), out var ty) ? ty : 0;
 869:             int size = int.TryParse(row.Cells["Size"].Value?.ToString(), out var ts) ? ts : 1;
 870:             int scale = int.TryParse(row.Cells["Scale"].Value?.ToString(), out var tsc) ? tsc : 1;
 871: 
 872:             var block = new InkjetOperator.Services.SimpleTextBlock(blockNumber, text, x, y, size, scale);
 873:             var (fsResp, f1Resp) = await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, 13, block);
 874:             Log($"IJ3 TextBlock({blockNumber}) FS='{fsResp}' F1='{f1Resp}'");
 875: 
 876:             // If adapter or device returns empty error, you can decide to continue or break.
 877:             await Task.Delay(30);
 878:         }
 879: 
 880:         var resume = await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
 881:         Log($"Resume resp: {resume}");
 882:     }
 883: 
 884:     private async void dgvUVBlocks_CellValueChanged(object sender, DataGridViewCellEventArgs e)
 885:     {
 886:         if (e.RowIndex < 0) return;
 887: 
 888:         var row = dgvUVBlocks.Rows[e.RowIndex];
 889: 
 890:         if (row.DataBoundItem is not UvRow data) return;
 891: 
 892:         try
 893:         {
 894:             await UpdateUvRow(data.Id, data.Lot, data.Name);
 895:             Log($"[UV Updated] ID={data.Id}, Lot={data.Lot}, Name={data.Name}");
 896:         }
 897:         catch (Exception ex)
 898:         {
 899:             Log("Update UV error: " + ex.Message);
 900:         }
 901:     }
 902: 
 903:     private async Task UpdateUvRow(int id, string lot, string name)
 904:     {
 905:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 906:         await conn.OpenAsync();
 907: 
 908:         var cmd = conn.CreateCommand();
 909:         cmd.CommandText = @"
 910:         UPDATE uv_print_data
 911:         SET lot = @lot,
 912:             name = @name,
 913:             update_at = CURRENT_TIMESTAMP
 914:         WHERE id = @id
 915:     ";
 916: 
 917:         cmd.Parameters.AddWithValue("@id", id);
 918:         cmd.Parameters.AddWithValue("@lot", lot);
 919:         cmd.Parameters.AddWithValue("@name", name);
 920: 
 921:         await cmd.ExecuteNonQueryAsync();
 922:     }
 923: 
 924:     public void InitializeDatabase()
 925:     {
 926:         string folderPath = @"D:\DB";
 927:         if (!System.IO.Directory.Exists(folderPath))
 928:         {
 929:             System.IO.Directory.CreateDirectory(folderPath);
 930:         }
 931: 
 932:         using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3;Default Timeout=5;");
 933:         conn.Open();
 934: 
 935:         string createTableSql = @"
 936:     CREATE TABLE IF NOT EXISTS uv_print_data (
 937:         id INTEGER PRIMARY KEY AUTOINCREMENT,
 938:         inkjet_name TEXT,
 939:         lot TEXT,
 940:         name TEXT,
 941:         update_at DATETIME DEFAULT CURRENT_TIMESTAMP
 942:     );";
 943: 
 944:         using (var command = new SqliteCommand(createTableSql, conn))
 945:         {
 946:             command.ExecuteNonQuery();
 947:         }
 948: 
 949:         // เช็คว่ามีข้อมูลหรือยัง ถ้าไม่มีให้ Insert แถวสำหรับ UV1 และ UV2 รอไว้เลย
 950:         var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM uv_print_data", conn);
 951:         long count = (long)checkCmd.ExecuteScalar();
 952:         if (count == 0)
 953:         {
 954:             var insertCmd = new SqliteCommand(@"
 955:             INSERT INTO uv_print_data (id, inkjet_name, lot, name) VALUES 
 956:             (1, 'เครื่องพิมพ์ UV1', '', ''),
 957:             (2, 'เครื่องพิมพ์ UV2', '', '')", conn);
 958:             insertCmd.ExecuteNonQuery();
 959:         }
 960:     }
 961: 
 962:     private async Task LoadUvDataToGrid()
 963:     {
 964:         try
 965:         {
 966:             using var conn = new SqliteConnection("Data Source=D:\\DB\\uv_data.db3");
 967:             await conn.OpenAsync();
 968: 
 969:             var cmd = conn.CreateCommand();
 970:             cmd.CommandText = @"
 971:         SELECT id, inkjet_name, lot, name, update_at
 972:         FROM uv_print_data
 973:         ORDER BY id
 974:         ";
 975: 
 976:             var reader = await cmd.ExecuteReaderAsync();
 977: 
 978:             var list = new List<UvRow>();
 979: 
 980:             while (await reader.ReadAsync())
 981:             {
 982:                 list.Add(new UvRow
 983:                 {
 984:                     Id = reader.GetInt32(0), // 🔥 ยังต้องมี
 985:                     Inkjet = reader.GetString(1),
 986:                     Lot = reader.IsDBNull(2) ? "" : reader.GetString(2),
 987:                     Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
 988:                     UpdateAt = reader.IsDBNull(4) ? "" : reader.GetString(4)
 989:                 });
 990:             }
 991: 
 992:             dgvUVBlocks.Invoke(() =>
 993:             {
 994: 
 995:                 dgvUVBlocks.AutoGenerateColumns = false;
 996: 
 997:                 dgvUVBlocks.ReadOnly = false;
 998:                 dgvUVBlocks.AllowUserToAddRows = false;
 999:                 dgvUVBlocks.EditMode = DataGridViewEditMode.EditOnEnter;
1000:                 dgvUVBlocks.SelectionMode = DataGridViewSelectionMode.CellSelect;
1001: 
1002: 
1003:                 _uvBindingList.RaiseListChangedEvents = false;
1004:                 _uvBindingList.Clear();
1005: 
1006:                 foreach (var item in list)
1007:                 {
1008:                     _uvBindingList.Add(item);
1009:                 }
1010: 
1011:                 _uvBindingList.RaiseListChangedEvents = true;
1012:                 _uvBindingList.ResetBindings();
1013: 
1014:             });
1015:         }
1016:         catch (Exception ex)
1017:         {
1018:             Log("Load UV error: " + ex.Message);
1019:         }
1020:     }
1021: 
1022:     private void button6_Click(object sender, EventArgs e)
1023:     {
1024:         var row = _uvBindingList.FirstOrDefault(x => x.Id == 1);
1025: 
1026:         if (row == null)
1027:         {
1028:             MessageBox.Show("ไม่พบ row id = 1");
1029:             return;
1030:         }
1031: 
1032:         string lot = row.Lot;
1033:         string name = row.Name;
1034: 
1035:         Log($"Lot={lot}, Name={name}");
1036: 
1037:         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1038: 
1039:         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1040:         {
1041:             conn.Open();
1042: 
1043:             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1044: 
1045:             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1046:             {
1047:                 cmd.Parameters.AddWithValue("@lot", lot);
1048:                 cmd.Parameters.AddWithValue("@name", name);
1049: 
1050:                 int rows = cmd.ExecuteNonQuery();
1051: 
1052:                 MessageBox.Show("Update สำเร็จ: " + rows + " row");
1053:             }
1054:         }
1055: 
1056:         currentStep = 3; // ไปขั้นที่ 2
1057:         UpdateStepButtons();
1058:     }
1059: 
1060:     private void button8_Click(object sender, EventArgs e)
1061:     {
1062:         var row = _uvBindingList.FirstOrDefault(x => x.Id == 2);
1063: 
1064:         if (row == null)
1065:         {
1066:             MessageBox.Show("ไม่พบ row id = 1");
1067:             return;
1068:         }
1069: 
1070:         string lot = row.Lot;
1071:         string name = row.Name;
1072: 
1073:         Log($"Lot={lot}, Name={name}");
1074: 
1075:         string dbPath = @"C:\Users\theer\Downloads\uvinkjet-250702-new\uvinkjet-250702-new\database\sys\CPI.db3";
1076: 
1077:         using (SqliteConnection conn = new SqliteConnection($"Data Source={dbPath}"))
1078:         {
1079:             conn.Open();
1080: 
1081:             string sql = "UPDATE MK063 SET lot = @lot, name = @name WHERE id = 1";
1082: 
1083:             using (SqliteCommand cmd = new SqliteCommand(sql, conn))
1084:             {
1085:                 cmd.Parameters.AddWithValue("@lot", lot);
1086:                 cmd.Parameters.AddWithValue("@name", name);
1087: 
1088:                 int rows = cmd.ExecuteNonQuery();
1089: 
1090:                 MessageBox.Show("Update สำเร็จ: " + rows + " row");
1091:             }
1092:         }
1093:         currentStep = 1;
1094:         UpdateStepButtons();
1095:     }
1096: 
1097: 
1098:     private async void button5_Click(object sender, EventArgs e)
1099:     {
1100:         //if (dgvConfigs.Rows.Count > 0)
1101:         //{
1102:         //    dgvConfigs.ClearSelection();
1103: 
1104:         //    dgvConfigs.Rows[0].Selected = true;
1105:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[0].Cells[0];
1106: 
1107:         //    var config = dgvConfigs.Rows[0].DataBoundItem as InkjetConfigDto;
1108: 
1109:         //    if (config != null)
1110:         //    {
1111:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1112:         //    }
1113:         //}
1114: 
1115:         //await TestSendToIj3TcpAsync();
1116: 
1117:         if (dgvConfigs.Rows.Count > 1)
1118:         {
1119:             dgvConfigs.ClearSelection();
1120: 
1121:             dgvConfigs.Rows[1].Selected = true;
1122:             dgvConfigs.CurrentCell = dgvConfigs.Rows[1].Cells[0];
1123: 
1124:             var config = dgvConfigs.Rows[1].DataBoundItem as InkjetConfigDto;
1125: 
1126:             if (config != null)
1127:             {
1128:                 MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1129:             }
1130:         }
1131: 
1132:         await TestSendToIj3TcpAsync();
1133: 
1134:         currentStep = 2; // ไปขั้นที่ 2
1135:         UpdateStepButtons();
1136: 
1137:     }
1138: 
1139:     private async void button7_Click(object sender, EventArgs e)
1140:     {
1141:         await SendToIj3FromDbAsync("CCRC0291-DEX0663MS");
1142:         //if (dgvConfigs.Rows.Count > 2)
1143:         //{
1144:         //    dgvConfigs.ClearSelection();
1145: 
1146:         //    dgvConfigs.Rows[2].Selected = true;
1147:         //    dgvConfigs.CurrentCell = dgvConfigs.Rows[2].Cells[0];
1148: 
1149:         //    var config = dgvConfigs.Rows[2].DataBoundItem as InkjetConfigDto;
1150: 
1151:         //    if (config != null)
1152:         //    {
1153:         //        MessageBox.Show($"ProgramNo: {config.ProgramNumber}");
1154:         //    }
1155: 
1156:         //    await TestSendToIj3TcpAsync();
1157: 
1158:         currentStep = 4; // ไปขั้นที่ 2
1159:         UpdateStepButtons();
1160:         //}
1161:     }
1162: 
1163:     private void UpdateStepButtons()
1164:     {
1165:         // ปุ่ม 1 (MK1+2)
1166:         button5.Enabled = (currentStep == 1);
1167: 
1168:         // ปุ่ม 2 (UV1)
1169:         //button6.Enabled = (currentStep == 2);
1170: 
1171:         // ปุ่ม 3 (MK2/3)
1172:         button7.Enabled = (currentStep == 3);
1173: 
1174:         // ปุ่ม 4 (UV2)
1175:         button8.Enabled = (currentStep == 4);
1176: 
1177:         // ถ้าอยากให้เห็นชัดเจนว่าอันไหนเสร็จแล้ว อาจจะเปลี่ยนสีปุ่มด้วยก็ได้
1178:         //button5.BackColor = (currentStep > 1) ? Color.LightGreen : SystemColors.Control;
1179:         // ... ทำแบบเดียวกันกับปุ่มอื่นๆ
1180:     }
1181: 
1182:     private async Task SendToIj3FromDbAsync(string barcode)
1183:     {
1184:         // ตัวแปรสำหรับ Block 1
1185:         string text1 = ""; int x1 = 0, y1 = 0, size1 = 1, scale1 = 1;
1186:         // ตัวแปรสำหรับ Block 2
1187:         string text2 = ""; int x2 = 0, y2 = 0, size2 = 1, scale2 = 1;
1188: 
1189:         string dbPath = @"D:\DB\uv_data.db3";
1190:         using (var conn = new SqliteConnection($"Data Source={dbPath}"))
1191:         {
1192:             await conn.OpenAsync();
1193:             string sql = "SELECT * FROM config_data_mk3 WHERE pattern_no_erp = @barcode LIMIT 1";
1194:             using var cmd = new SqliteCommand(sql, conn);
1195:             cmd.Parameters.AddWithValue("@barcode", barcode);
1196:             using var reader = await cmd.ExecuteReaderAsync();
1197: 
1198:             if (reader.Read())
1199:             {
1200:                 // ดึงข้อมูล Block 1
1201:                 text1 = reader.GetString(reader.GetOrdinal("block1_text"));
1202:                 x1 = reader.GetInt32(reader.GetOrdinal("block1_x"));
1203:                 y1 = reader.GetInt32(reader.GetOrdinal("block1_y"));
1204:                 size1 = reader.GetInt32(reader.GetOrdinal("block1_size"));
1205:                 scale1 = reader.GetInt32(reader.GetOrdinal("block1_scale_side"));
1206: 
1207:                 // ดึงข้อมูล Block 2
1208:                 text2 = reader.IsDBNull(reader.GetOrdinal("block2_text")) ? "" : reader.GetString(reader.GetOrdinal("block2_text"));
1209:                 x2 = reader.GetInt32(reader.GetOrdinal("block2_x"));
1210:                 y2 = reader.GetInt32(reader.GetOrdinal("block2_y"));
1211:                 size2 = reader.GetInt32(reader.GetOrdinal("block2_size"));
1212:                 scale2 = reader.GetInt32(reader.GetOrdinal("block2_scale_side"));
1213:             }
1214:         }
1215: 
1216:         // --- ส่วนการส่ง TCP ---
1217:         using var client = new InkjetOperator.Adapters.Simple.SocketDeviceClient();
1218:         try
1219:         {
1220:             await client.ConnectAsync(txtTcpHost.Text.Trim(), int.Parse(txtTcpPort.Text.Trim()));
1221:             int fixedProgNo = 13;
1222: 
1223:             // 1. เปลี่ยนโปรแกรมเป็น 13
1224:             await InkjetOperator.Services.InkjetProtocol.SendChangeProgramAsync(client, fixedProgNo);
1225: 
1226:             // 2. ส่ง Block 1
1227:             var block1 = new InkjetOperator.Services.SimpleTextBlock(1, text1, x1, y1, size1, scale1);
1228:             await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block1);
1229:             await Task.Delay(50); // พักช่วงสั้นๆ ระหว่างการส่งแต่ละ Block
1230: 
1231:             // 3. ส่ง Block 2
1232:             if (!string.IsNullOrEmpty(text2))
1233:             {
1234:                 var block2 = new InkjetOperator.Services.SimpleTextBlock(2, text2, x2, y2, size2, scale2);
1235:                 await InkjetOperator.Services.InkjetProtocol.SendTextBlockAsync(client, fixedProgNo, block2);
1236:                 await Task.Delay(50);
1237:             }
1238: 
1239:             // 4. สั่ง Resume
1240:             await InkjetOperator.Services.InkjetProtocol.SendResumeAsync(client);
1241:             Log("Step 3: MK3 (2 Blocks) Sent Successfully.");
1242:         }
1243:         catch (Exception ex)
1244:         {
1245:             Log("Error: " + ex.Message);
1246:         }
1247:     }
1248: 
1249:     private void button9_Click(object sender, EventArgs e)
1250:     {
1251:         string lot = "C200521-001" ?? string.Empty;
1252:         string blockText = "DDDD-01";
1253:         string previewText = PatternEngine.Process(lot, blockText);
1254:         //lblPreview.Text = "Preview: " + previewText;
1255:         Log("Preview: " + previewText);
1256:     }
1257: 
1258:     private async void dgvJobs_CellClick(object sender, DataGridViewCellEventArgs e)
1259:     {
1260:         if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1261: 
1262:         if (dgvJobs.SelectedRows.Count == 0) return;
1263: 
1264:         var row = dgvJobs.SelectedRows[0];
1265:         if (row.Cells["Id"].Value == null) return;
1266: 
1267:         int jobId = (int)row.Cells["Id"].Value;
1268:         string rawbarcode = row.Cells["BarcodeRaw"].Value?.ToString() ?? "";
1269: 
1270:         if (jobId == _selectedJobId) return;
1271:         _selectedJobId = jobId;
1272: 
1273:         // Helpers
1274:         static bool ReaderHasColumn(SqliteDataReader r, string name)
1275:         {
1276:             for (int i = 0; i < r.FieldCount; i++)
1277:             {
1278:                 if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1279:                     return true;
1280:             }
1281:             return false;
1282:         }
1283: 
1284:         static string? GetStringSafe(SqliteDataReader r, string name)
1285:         {
1286:             if (!ReaderHasColumn(r, name)) return null;
1287:             int idx = r.GetOrdinal(name);
1288:             return r.IsDBNull(idx) ? null : r.GetString(idx);
1289:         }
1290: 
1291:         static int? GetIntSafe(SqliteDataReader r, string name)
1292:         {
1293:             var s = GetStringSafe(r, name);
1294:             if (string.IsNullOrWhiteSpace(s)) return null;
1295:             return int.TryParse(s, out var v) ? v : null;
1296:         }
1297: 
1298:         try
1299:         {
1300:             string dbPath = @"D:\DB\uv_data.db3";
1301:             string connStr = $"Data Source={dbPath}";
1302:             bool found = false;
1303: 
1304:             try
1305:             {
1306:                 await using var conn = new SqliteConnection(connStr);
1307:                 await conn.OpenAsync();
1308: 
1309:                 await using var cmd = conn.CreateCommand();
1310:                 cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1311:                 cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1312: 
1313:                 await using var reader = await cmd.ExecuteReaderAsync();
1314:                 if (await reader.ReadAsync())
1315:                 {
1316:                     // Build PatternDetail from known schema columns.
1317:                     var pattern = new PatternDetail
1318:                     {
1319:                         Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1320:                         Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1321:                     };
1322: 
1323:                     // Helper to build MK config (mk1 / mk2)
1324:                     InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1325:                     {
1326:                         var cfg = new InkjetConfigDto
1327:                         {
1328:                             Ordinal = ordinal,
1329:                             ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1330:                             ProgramName = GetStringSafe(reader, programNameColumn),
1331:                             Width = GetIntSafe(reader, $"{prefix}width"),
1332:                             Height = GetIntSafe(reader, $"{prefix}height"),
1333:                             TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1334:                             Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1335:                             SteelType = null,
1336:                             Suspended = false
1337:                         };
1338: 
1339:                         // text blocks 1..5
1340:                         for (int b = 1; b <= 5; b++)
1341:                         {
1342:                             string textCol = $"{prefix}block{b}_text";
1343:                             if (ReaderHasColumn(reader, textCol))
1344:                             {
1345:                                 var text = GetStringSafe(reader, textCol);
1346:                                 if (!string.IsNullOrEmpty(text))
1347:                                 {
1348:                                     var tb = new TextBlockDto
1349:                                     {
1350:                                         BlockNumber = b,
1351:                                         Text = text,
1352:                                         X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1353:                                         Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1354:                                         Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1355:                                         Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1356:                                     };
1357:                                     cfg.TextBlocks.Add(tb);
1358:                                 }
1359:                             }
1360:                         }
1361: 
1362:                         return cfg;
1363:                     }
1364: 
1365:                     // mk1 fields use prefix "mk1_"; program name fallback to "program_name"
1366:                     var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1367:                     // mk2 fields use prefix "mk2_"; program name fallback to "program_name3"
1368:                     var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1369: 
1370:                     pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1371: 
1372:                     // Conveyor speeds / servo configs - attempt to map if available (optional)
1373:                     var spd1 = GetIntSafe(reader, "belt1_inkjet");
1374:                     var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1375:                     if (spd1.HasValue || spd2.HasValue)
1376:                     {
1377:                         pattern.ConveyorSpeeds = new ConveyorSpeedDto
1378:                         {
1379:                             Speed1 = spd1,
1380:                             Speed2 = spd2,
1381:                             Speed3 = GetIntSafe(reader, "belt3")
1382:                         };
1383:                     }
1384: 
1385:                     // Minimal job + resolved response
1386:                     _currentResolved = new ResolvedJobResponse
1387:                     {
1388:                         Job = new PrintJob
1389:                         {
1390:                             Id = jobId,
1391:                             BarcodeRaw = rawbarcode,
1392:                             LotNumber = row.Cells["LotNumber"].Value?.ToString(),
1393:                             Status = row.Cells["Status"].Value?.ToString()
1394:                         },
1395:                         Pattern = pattern
1396:                     };
1397: 
1398:                     string lot = row.Cells["LotNumber"].Value?.ToString() ?? "";
1399:                     string name = pattern.Description ?? ""; // หรือ field ที่คุณต้องการ
1400: 
1401:                     found = true;
1402:                     UpdateDetailPanel();
1403:                 }
1404: 
1405:                 await conn.CloseAsync();
1406:             }
1407:             catch (Exception dbEx)
1408:             {
1409:                 Log("SQLite query error: " + dbEx.Message);
1410:             }
1411: 
1412:             if (!found)
1413:             {
1414:                 var resolved = await _api.GetResolvedJobAsync(jobId);
1415:                 _currentResolved = resolved;
1416:                 UpdateDetailPanel();
1417:             }
1418:         }
1419:         catch (Exception ex)
1420:         {
1421:             Log("Load job detail error: " + ex.Message);
1422:         }
1423:     }
1424: 
1425:     private async Task LoadLastSentData()
1426:     {
1427:         try
1428:         {
1429:             // เรียกใช้ API (ระบุ status เป็น sent, หน้า 1, จำนวน 10 รายการ)
1430:             var lastSentJobs = await _api.GetLastSentJobsAsync("sent", 1, 10);
1431: 
1432:             // นำข้อมูลไปผูกกับ BindingSource หรือ DataGridView
1433:             var jobRows = lastSentJobs.Select(j => new InkjetOperator.Models.JobRow
1434:             {
1435:                 Id = j.Id,
1436:                 BarcodeRaw = j.BarcodeRaw,
1437:                 LotNumber = j.LotNumber ?? string.Empty,
1438:                 Status = j.Status,
1439:                 Attempt = j.Attempt
1440:                 // เพิ่ม field อื่นๆ ที่ต้องการแสดงในตาราง history
1441:             }).ToList();
1442: 
1443:             printJobBindingSource.DataSource = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(jobRows);
1444:         }
1445:         catch (Exception ex)
1446:         {
1447:             MessageBox.Show($"โหลดข้อมูลประวัติไม่สำเร็จ: {ex.Message}", "Error");
1448:         }
1449:     }
1450: 
1451:     private async void dataGridView1_SelectionChanged(object sender, EventArgs e)
1452:     {
1453:         ////// Guard: header click / invalid index
1454:         ////if (e.RowIndex < 0) return;
1455:         ////if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1456: 
1457:         ////var row = dataGridView1.Rows[e.RowIndex];
1458:         //int jobId;
1459:         //string rawbarcode;
1460:         //if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedJob)
1461:         //{
1462:         //    jobId = selectedJob.Id;
1463:         //    rawbarcode = selectedJob.BarcodeRaw;
1464: 
1465: 
1466:         //    //Debug.WriteLine($"Clicked row {e.RowIndex}, Id={row.Cells["Id"].Value}, Barcode={row.Cells["BarcodeRaw"].Value}");
1467: 
1468:         //    //// Ensure the grid contains expected columns
1469:         //    //if (!dataGridView1.Columns.Contains("Id") || row.Cells["Id"].Value == null) return;
1470:         //    //if (!dataGridView1.Columns.Contains("BarcodeRaw")) return;
1471: 
1472: 
1473: 
1474:         //    //try
1475:         //    //{
1476: 
1477:         //    //}
1478:         //    //catch
1479:         //    //{
1480:         //    //    // invalid id cell
1481:         //    //    return;
1482:         //    //}
1483: 
1484:         //    //string rawbarcode = "CCRC0291-DEX0663MS";
1485:         //    //if (jobId == _selectedJobId) return;
1486:         //    //_selectedJobId = jobId;
1487: 
1488:         //    // Helpers
1489:         //    static bool ReaderHasColumn(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1490:         //    {
1491:         //        for (int i = 0; i < r.FieldCount; i++)
1492:         //        {
1493:         //            if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1494:         //                return true;
1495:         //        }
1496:         //        return false;
1497:         //    }
1498: 
1499:         //    static string? GetStringSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1500:         //    {
1501:         //        if (!ReaderHasColumn(r, name)) return null;
1502:         //        int idx = r.GetOrdinal(name);
1503:         //        return r.IsDBNull(idx) ? null : r.GetString(idx);
1504:         //    }
1505: 
1506:         //    static int? GetIntSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1507:         //    {
1508:         //        var s = GetStringSafe(r, name);
1509:         //        if (string.IsNullOrWhiteSpace(s)) return null;
1510:         //        return int.TryParse(s, out var v) ? v : null;
1511:         //    }
1512: 
1513:         //    try
1514:         //    {
1515:         //        string dbPath = @"D:\DB\uv_data.db3";
1516:         //        string connStr = $"Data Source={dbPath}";
1517:         //        bool found = false;
1518: 
1519:         //        // optional debug: MessageBox.Show($"Clicked Job ID: {jobId}, Barcode: {rawbarcode}");
1520: 
1521:         //        try
1522:         //        {
1523:         //            await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
1524:         //            await conn.OpenAsync();
1525: 
1526:         //            await using var cmd = conn.CreateCommand();
1527:         //            cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1528:         //            cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1529: 
1530: 
1531:         //            await using var reader = await cmd.ExecuteReaderAsync();
1532:         //            if (await reader.ReadAsync())
1533:         //            {
1534:         //                // Build PatternDetail from known schema columns.
1535:         //                var pattern = new PatternDetail
1536:         //                {
1537:         //                    Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1538:         //                    Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1539:         //                };
1540: 
1541:         //                // Helper to build MK config (mk1 / mk2)
1542:         //                InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1543:         //                {
1544:         //                    var cfg = new InkjetConfigDto
1545:         //                    {
1546:         //                        Ordinal = ordinal,
1547:         //                        ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1548:         //                        ProgramName = GetStringSafe(reader, programNameColumn),
1549:         //                        Width = GetIntSafe(reader, $"{prefix}width"),
1550:         //                        Height = GetIntSafe(reader, $"{prefix}height"),
1551:         //                        TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1552:         //                        Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1553:         //                        SteelType = null,
1554:         //                        Suspended = false
1555:         //                    };
1556: 
1557:         //                    // text blocks 1..5
1558:         //                    for (int b = 1; b <= 5; b++)
1559:         //                    {
1560:         //                        string textCol = $"{prefix}block{b}_text";
1561:         //                        if (ReaderHasColumn(reader, textCol))
1562:         //                        {
1563:         //                            var text = GetStringSafe(reader, textCol);
1564:         //                            if (!string.IsNullOrEmpty(text))
1565:         //                            {
1566:         //                                var tb = new TextBlockDto
1567:         //                                {
1568:         //                                    BlockNumber = b,
1569:         //                                    Text = text,
1570:         //                                    X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1571:         //                                    Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1572:         //                                    Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1573:         //                                    Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1574:         //                                };
1575:         //                                cfg.TextBlocks.Add(tb);
1576:         //                            }
1577:         //                        }
1578:         //                    }
1579: 
1580:         //                    return cfg;
1581:         //                }
1582: 
1583:         //                var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1584:         //                var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1585: 
1586:         //                pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1587: 
1588:         //                var spd1 = GetIntSafe(reader, "belt1_inkjet");
1589:         //                var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1590:         //                if (spd1.HasValue || spd2.HasValue)
1591:         //                {
1592:         //                    pattern.ConveyorSpeeds = new ConveyorSpeedDto
1593:         //                    {
1594:         //                        Speed1 = spd1,
1595:         //                        Speed2 = spd2,
1596:         //                        Speed3 = GetIntSafe(reader, "belt3")
1597:         //                    };
1598:         //                }
1599: 
1600:         //                // Minimal job + resolved response
1601:         //                _currentResolved = new ResolvedJobResponse
1602:         //                {
1603:         //                    Job = new PrintJob
1604:         //                    {
1605:         //                        Id = jobId,
1606:         //                        BarcodeRaw = rawbarcode,
1607:         //                        LotNumber = dataGridView1.Columns.Contains("LotNumber") ? selectedJob.LotNumber?.ToString() : null,
1608:         //                        Status = dataGridView1.Columns.Contains("Status") ? selectedJob.Status?.ToString() : null
1609:         //                    },
1610:         //                    Pattern = pattern
1611:         //                };
1612: 
1613:         //                found = true;
1614:         //                UpdateDetailPanel();
1615:         //            }
1616: 
1617:         //            await conn.CloseAsync();
1618:         //        }
1619:         //        catch (Exception dbEx)
1620:         //        {
1621:         //            Log("SQLite query error: " + dbEx.Message);
1622:         //        }
1623: 
1624: 
1625:         //        if (!found)
1626:         //        {
1627:         //            var resolved = await _api.GetResolvedJobAsync(jobId);
1628:         //            _currentResolved = resolved;
1629:         //            UpdateDetailPanel();
1630:         //        }
1631: 
1632:         //    }
1633:         //    catch (Exception ex)
1634:         //    {
1635:         //        Log("Load job detail error: " + ex.Message);
1636:         //    }
1637:         //}
1638:     }
1639: 
1640:     private async void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
1641:     {
1642:         //// Guard: header click / invalid index
1643:         //if (e.RowIndex < 0) return;
1644:         //if (_isRefreshingJobs) return; // 🔥 กันตอน poll
1645: 
1646:         var row = dataGridView1.Rows[e.RowIndex];
1647:         int jobId;
1648:         string rawbarcode;
1649:         if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedJob)
1650:         {
1651:             jobId = selectedJob.Id;
1652:             rawbarcode = selectedJob.BarcodeRaw;
1653: 
1654: 
1655:             //Debug.WriteLine($"Clicked row {e.RowIndex}, Id={row.Cells["Id"].Value}, Barcode={row.Cells["BarcodeRaw"].Value}");
1656: 
1657:             //// Ensure the grid contains expected columns
1658:             //if (!dataGridView1.Columns.Contains("Id") || row.Cells["Id"].Value == null) return;
1659:             //if (!dataGridView1.Columns.Contains("BarcodeRaw")) return;
1660: 
1661: 
1662: 
1663:             //try
1664:             //{
1665: 
1666:             //}
1667:             //catch
1668:             //{
1669:             //    // invalid id cell
1670:             //    return;
1671:             //}
1672: 
1673:             //string rawbarcode = "CCRC0291-DEX0663MS";
1674:             //if (jobId == _selectedJobId) return;
1675:             //_selectedJobId = jobId;
1676: 
1677:             // Helpers
1678:             static bool ReaderHasColumn(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1679:             {
1680:                 for (int i = 0; i < r.FieldCount; i++)
1681:                 {
1682:                     if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
1683:                         return true;
1684:                 }
1685:                 return false;
1686:             }
1687: 
1688:             static string? GetStringSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1689:             {
1690:                 if (!ReaderHasColumn(r, name)) return null;
1691:                 int idx = r.GetOrdinal(name);
1692:                 return r.IsDBNull(idx) ? null : r.GetString(idx);
1693:             }
1694: 
1695:             static int? GetIntSafe(Microsoft.Data.Sqlite.SqliteDataReader r, string name)
1696:             {
1697:                 var s = GetStringSafe(r, name);
1698:                 if (string.IsNullOrWhiteSpace(s)) return null;
1699:                 return int.TryParse(s, out var v) ? v : null;
1700:             }
1701: 
1702:             try
1703:             {
1704:                 string dbPath = @"D:\DB\uv_data.db3";
1705:                 string connStr = $"Data Source={dbPath}";
1706:                 bool found = false;
1707: 
1708:                 try
1709:                 {
1710:                     await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
1711:                     await conn.OpenAsync();
1712: 
1713:                     await using var cmd = conn.CreateCommand();
1714:                     cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @rawbarcode LIMIT 1";
1715:                     cmd.Parameters.AddWithValue("@rawbarcode", rawbarcode);
1716: 
1717:                 
1718: 
1719:                     await using var reader = await cmd.ExecuteReaderAsync();
1720: 
1721:                
1722:                     if (await reader.ReadAsync())
1723:                     {
1724:                         // Build PatternDetail from known schema columns.
1725:                         var pattern = new PatternDetail
1726:                         {
1727:                             Barcode = GetStringSafe(reader, "pattern_no_erp") ?? GetStringSafe(reader, "pattern_no_erp2") ?? rawbarcode,
1728:                             Description = GetStringSafe(reader, "model_plan_code") ?? GetStringSafe(reader, "program_name") ?? ""
1729:                         };
1730: 
1731:                         // Helper to build MK config (mk1 / mk2)
1732:                         InkjetConfigDto BuildMkConfig(string prefix, int ordinal, string programNameColumn)
1733:                         {
1734:                             var cfg = new InkjetConfigDto
1735:                             {
1736:                                 Ordinal = ordinal,
1737:                                 ProgramNumber = GetIntSafe(reader, $"{prefix}program_no") ?? GetIntSafe(reader, $"{prefix}program_no") ?? null,
1738:                                 ProgramName = GetStringSafe(reader, programNameColumn),
1739:                                 Width = GetIntSafe(reader, $"{prefix}width"),
1740:                                 Height = GetIntSafe(reader, $"{prefix}height"),
1741:                                 TriggerDelay = GetIntSafe(reader, $"{prefix}trigger_delay"),
1742:                                 Direction = GetIntSafe(reader, $"{prefix}text_direction"),
1743:                                 SteelType = null,
1744:                                 Suspended = false
1745:                             };
1746: 
1747:                             // text blocks 1..5
1748:                             for (int b = 1; b <= 5; b++)
1749:                             {
1750:                                 string textCol = $"{prefix}block{b}_text";
1751:                                 if (ReaderHasColumn(reader, textCol))
1752:                                 {
1753:                                     var text = GetStringSafe(reader, textCol);
1754:                                     if (!string.IsNullOrEmpty(text))
1755:                                     {
1756:                                         var tb = new TextBlockDto
1757:                                         {
1758:                                             BlockNumber = b,
1759:                                             Text = text,
1760:                                             X = GetIntSafe(reader, $"{prefix}block{b}_x"),
1761:                                             Y = GetIntSafe(reader, $"{prefix}block{b}_y"),
1762:                                             Size = GetIntSafe(reader, $"{prefix}block{b}_size"),
1763:                                             Scale = GetIntSafe(reader, $"{prefix}block{b}_scale_side")
1764:                                         };
1765:                                         cfg.TextBlocks.Add(tb);
1766:                                     }
1767:                                 }
1768:                             }
1769: 
1770:                             return cfg;
1771:                         }
1772: 
1773:                         var mk1 = BuildMkConfig("mk1_", 1, "program_name");
1774:                         var mk2 = BuildMkConfig("mk2_", 2, "program_name3");
1775: 
1776:                         pattern.InkjetConfigs = new List<InkjetConfigDto> { mk1, mk2 };
1777: 
1778:                         var spd1 = GetIntSafe(reader, "belt1_inkjet");
1779:                         var spd2 = GetIntSafe(reader, "belt2_feed_inkjet");
1780:                         if (spd1.HasValue || spd2.HasValue)
1781:                         {
1782:                             pattern.ConveyorSpeeds = new ConveyorSpeedDto
1783:                             {
1784:                                 Speed1 = spd1,
1785:                                 Speed2 = spd2,
1786:                                 Speed3 = GetIntSafe(reader, "belt3")
1787:                             };
1788:                         }
1789: 
1790:                         // Minimal job + resolved response
1791:                         _currentResolved = new ResolvedJobResponse
1792:                         {
1793:                             Job = new PrintJob
1794:                             {
1795:                                 Id = jobId,
1796:                                 BarcodeRaw = rawbarcode,
1797:                                 LotNumber = dataGridView1.Columns.Contains("LotNumber") ? row.Cells["LotNumber"].Value?.ToString() : null,
1798:                                 Status = dataGridView1.Columns.Contains("Status") ? row.Cells["Status"].Value?.ToString() : null
1799:                             },
1800:                             Pattern = pattern
1801:                         };
1802: 
1803:                         found = true;
1804:                         UpdateDetailPanel();
1805:                     }
1806: 
1807:                     await conn.CloseAsync();
1808:                 }
1809:                 catch (Exception dbEx)
1810:                 {
1811:                     Log("SQLite query error: " + dbEx.Message);
1812:                 }
1813: 
1814: 
1815:                 if (!found)
1816:                 {
1817:                     var resolved = await _api.GetResolvedJobAsync(jobId);
1818:                     _currentResolved = resolved;
1819:                     UpdateDetailPanel();
1820:                 }
1821: 
1822:             }
1823:             catch (Exception ex)
1824:             {
1825:                 Log("Load job detail error: " + ex.Message);
1826:             }
1827:         }
1828:     }
1829: }
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
