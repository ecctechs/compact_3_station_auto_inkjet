using InkjetOperator.Managers;
using InkjetOperator.Models;

namespace InkjetOperator.Adapters;

/// <summary>
/// Stub adapter for inkjet 3 & 4 (different brand, TCP → SQLite .db3).
/// Implements IInkjetAdapter so it can be used interchangeably with MkCompactAdapter.
/// Actual implementation TBD — needs: exact table/column names from the machine's .db3 schema.
/// </summary>
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
        // TODO: Connect to inkjet machine via TCP, then open .db3 database
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
        // TODO: SQL UPDATE to pause printing
        return Task.FromResult(new CommandResult
        {
            Command = "suspend",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> ResumeAsync()
    {
        // TODO: SQL UPDATE to resume printing
        return Task.FromResult(new CommandResult
        {
            Command = "resume",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> ChangeProgramAsync(int programNumber)
    {
        // TODO: SQL UPDATE to change active message/program
        return Task.FromResult(new CommandResult
        {
            Command = "change_prog",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
    {
        // TODO: SQL UPDATE on the .db3 text content table
        return Task.FromResult(new CommandResult
        {
            Command = "text_block",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }

    public Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
    {
        // TODO: SQL UPDATE for print configuration
        return Task.FromResult(new CommandResult
        {
            Command = "config",
            Success = false,
            Response = "SqliteInkjetAdapter not implemented",
        });
    }
}
