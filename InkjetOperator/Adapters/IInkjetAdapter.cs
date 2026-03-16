using InkjetOperator.Models;

namespace InkjetOperator.Adapters;

public interface IInkjetAdapter
{
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected();
    Task<CommandResult> SuspendAsync();
    Task<CommandResult> ResumeAsync();
    Task<CommandResult> ChangeProgramAsync(int programNumber);
    Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock);
    Task<CommandResult> SendConfigAsync(InkjetConfigDto config);
}
