using System.Globalization;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using System.Text;

namespace InkjetOperator.Adapters;

/// <summary>
/// MK Compact inkjet adapter — formats FW/FS/F1/FM/SR/SQ commands.
/// Ported from rs232_connector.py + socket_client.py.
/// Can use either Rs232Manager (COM port) or TcpManager (TCP/IP).
/// </summary>
public class MkCompactAdapter : IInkjetAdapter
{
    private readonly Rs232Manager? _rs232;
    private readonly TcpManager? _tcp;
    private int _programNumber;

    /// <summary>
    /// Size conversion dict — from rs232_connector.py lines 8-22.
    /// Maps logical size to device encoding.
    /// </summary>
    private static readonly Dictionary<string, string> SizeConversion = new()
    {
        { "1", "0" },
        { "2", "1" },
        { "3", "17" },
        { "4", "3" },
        { "5", "18" },
        { "6", "20" },
        { "7", "5" },
        { "8", "6" },
        { "9", "7" },
        { "10", "8" },
        { "11", "9" },
        { "12", "10" },
        { "13", "11" },
    };

    /// <summary>Constructor for RS232 connection (COM port).</summary>
    public MkCompactAdapter(Rs232Manager rs232)
    {
        _rs232 = rs232;
    }

    /// <summary>Constructor for TCP connection.</summary>
    public MkCompactAdapter(TcpManager tcp)
    {
        _tcp = tcp;
    }

    public Task<bool> ConnectAsync()
    {
        // Connection is managed by the underlying manager
        return Task.FromResult(IsConnected());
    }

    public Task DisconnectAsync()
    {
        _rs232?.CloseSerialPort();
        _tcp?.Disconnect();
        return Task.CompletedTask;
    }

    public bool IsConnected()
    {
        if (_rs232 != null) return _rs232.IsOpen();
        if (_tcp != null) return _tcp.IsConnected();
        return false;
    }

    private async Task<string> SendAsync(string command)
    {
        if (_rs232 != null)
            return await _rs232.SendCommandAsync(command);
        if (_tcp != null)
            return await _tcp.SendCommandAsync(command);
        return "";
    }

    private CommandResult MakeResult(string command, string response, int? ordinal = null)
    {
        return new CommandResult
        {
            Command = command,
            Response = response,
            Success = response != "", // empty = connection problem (rs232_connector.py line 33)
            SentAt = DateTime.UtcNow.ToString("o"),
            Ordinal = ordinal,
        };
    }

    /// <summary>
    /// Send SR (suspend/stop printing).
    /// From rs232_connector.py send_suspend() lines 46-51: sends "SR\r"
    /// </summary>
    public async Task<CommandResult> SuspendAsync()
    {
        string response = await SendAsync("SR\r");
        return MakeResult("suspend", response);
    }

    /// <summary>
    /// Send SQ (resume printing).
    /// From rs232_connector.py send_resume() lines 54-59: sends "SQ\r"
    /// </summary>
    public async Task<CommandResult> ResumeAsync()
    {
        string response = await SendAsync("SQ\r");
        return MakeResult("resume", response);
    }

    /// <summary>
    /// Send FW (change program/message number).
    /// From rs232_connector.py send_change_prog() lines 36-43: sends "FW,{n}\r"
    /// </summary>
    public async Task<CommandResult> ChangeProgramAsync(int programNumber)
    {
        _programNumber = programNumber;
        string response = await SendAsync($"FW,{programNumber}\r");
        return MakeResult("change_prog", response);
    }

    /// <summary>
    /// Send FS + F1 (text block content + format).
    /// From rs232_connector.py send_text() lines 62-71:
    ///   FS,{prog},{block},0,{text}\r
    ///   F1,{prog},{block},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r
    /// </summary>
    public async Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
    {
        string text = block.Text ?? "";
        string x = (block.X ?? 0).ToString();
        string y = (block.Y ?? 0).ToString();
        string scale = (block.Scale ?? 1).ToString();
        string sizeKey = (block.Size ?? 1).ToString();

        string sizeConverted = SizeConversion.GetValueOrDefault(sizeKey, "0");

        // FS command — set text content
        string fsCmd = $"FS,{_programNumber},{deviceBlock},0,{text}\r";
        string fsResponse = await SendAsync(fsCmd);

        if (fsResponse == "")
        {
            return MakeResult("text_block", "", null);
        }

        // F1 command — set text format
        string f1Cmd = $"F1,{_programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r";
        string f1Response = await SendAsync(f1Cmd);

        return MakeResult("text_block", f1Response);
    }

    /// <summary>
    /// Send FM (program/message configuration).
    /// From rs232_connector.py send_config() lines 74-83:
    ///   FM,{prog},0,{name},0,{dir},0,0,01,20,500,{delay},300,1000,{height},{width},15,0,0,6\r
    /// Program name is Unicode-normalized (NFKD) like Python unicodedata.normalize.
    /// </summary>
    public async Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
    {
        string progName = config.ProgramName ?? "";
        // Normalize like Python: unicodedata.normalize('NFKD', ch)
        string normalizedName = progName.Normalize(NormalizationForm.FormKD);

        string direction = (config.Direction ?? 0).ToString();
        string delay = (config.TriggerDelay ?? 0).ToString();
        string height = (config.Height ?? 100).ToString();
        string width = (config.Width ?? 200).ToString();

        string fmCmd = $"FM,{_programNumber},0,{normalizedName},0,{direction},0,0,01,20,500,{delay},300,1000,{height},{width},15,0,0,6\r";
        //MessageBox.Show(fmCmd);
        string response = await SendAsync(fmCmd);

        return MakeResult("config", response);
    }
}
