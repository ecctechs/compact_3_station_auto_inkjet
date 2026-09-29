using InkjetOperator.Managers;
using InkjetOperator.Models;

namespace InkjetOperator.Adapters;

public class SqliteInkjetAdapter : IInkjetAdapter
{
    private readonly TcpManager _tcp;
    private readonly string _dbPath;

    public SqliteInkjetAdapter(TcpManager tcp, string dbPath)
    {
        _tcp = tcp;
        _dbPath = dbPath;
    }

    public Task<bool> ConnectAsync()
    {
        return Task.FromResult(false);
    }

    public Task DisconnectAsync()
    {
        _tcp.Disconnect();
        return Task.CompletedTask;
    }

    public bool IsConnected()
    {
        return _tcp.IsConnected();
    }

    public Task<CommandResult> SuspendAsync()
    {
        return Task.FromResult(new CommandResult
        {
            Command = "suspend",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> ResumeAsync()
    {
        return Task.FromResult(new CommandResult
        {
            Command = "resume",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> ChangeProgramAsync(int programNumber)
    {
        return Task.FromResult(new CommandResult
        {
            Command = "change_prog",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
    {
        return Task.FromResult(new CommandResult
        {
            Command = "text_block",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
    {
        return Task.FromResult(new CommandResult
        {
            Command = "config",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }
}
