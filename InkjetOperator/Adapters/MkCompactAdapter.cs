using System.Globalization;
using System.Text;
using InkjetOperator.Managers;
using InkjetOperator.Models;

namespace InkjetOperator.Adapters;

public class MkCompactAdapter : IInkjetAdapter
{
    private readonly Rs232Manager? _rs232;
    private readonly TcpManager? _tcp;
    private int _programNumber;

    private const int TriggerDelayScale = 10;

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

    public MkCompactAdapter(Rs232Manager rs232)
    {
        _rs232 = rs232;
    }

    public MkCompactAdapter(TcpManager tcp)
    {
        _tcp = tcp;
    }

    public Task<bool> ConnectAsync()
    {
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

    private static bool IsRejected(string response) =>
        response.StartsWith("ER", StringComparison.OrdinalIgnoreCase);

    private CommandResult MakeResult(string command, string response, int? ordinal = null)
    {
        return new CommandResult
        {
            Command = command,
            Response = response,

            Success = response != "" && !IsRejected(response),
            SentAt = DateTime.UtcNow.ToString("o"),
            Ordinal = ordinal,
        };
    }

    public async Task<CommandResult> SuspendAsync()
    {
        string response = await SendAsync("SR\r");
        return MakeResult("suspend", response);
    }

    public async Task<CommandResult> ResumeAsync()
    {
        string response = await SendAsync("SQ\r");
        return MakeResult("resume", response);
    }

    public async Task<CommandResult> ChangeProgramAsync(int programNumber)
    {
        _programNumber = programNumber;
        string response = await SendAsync($"FW,{programNumber}\r");
        return MakeResult("change_prog", response);
    }

    public async Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
    {
        string text = block.Text ?? "";
        string x = (block.X ?? 0).ToString();
        string y = (block.Y ?? 0).ToString();
        string scale = (block.Scale ?? 1).ToString();
        string sizeKey = (block.Size ?? 1).ToString();

        string sizeConverted = SizeConversion.GetValueOrDefault(sizeKey, "0");

        string fsCmd = $"FS,{_programNumber},{deviceBlock},0,{text}\r";
        string fsResponse = await SendAsync(fsCmd);

        if (fsResponse == "")
        {
            return MakeResult("text_block", "", null);
        }

        string f1Cmd = $"F1,{_programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r";
        string f1Response = await SendAsync(f1Cmd);

        return MakeResult("text_block", f1Response);
    }

    public const int DirectionNormal = 1;
    public const int DirectionFlipped = 2;

    public static bool IsFlipped(int? direction) => direction == 2 || direction == 3;

    public async Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
    {
        string progName = config.ProgramName ?? "";
        string normalizedName = progName.Normalize(NormalizationForm.FormKD);

        string direction = IsFlipped(config.Direction) ? "3" : "0";

        string delay = ((config.TriggerDelay ?? 0) * TriggerDelayScale).ToString();
        string height = (config.Height ?? 100).ToString();
        string width = (config.Width ?? 200).ToString();

        string fmCmd = $"FM,{_programNumber},0,{normalizedName},0,{direction},0,0,01,20,500,{delay},300,1000,{height},{width},15,0,0,6\r";
        string response = await SendAsync(fmCmd);

        return MakeResult("config", response);
    }
}
