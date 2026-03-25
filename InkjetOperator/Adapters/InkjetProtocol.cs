using System.Text;
using InkjetOperator.Adapters.Simple;

namespace InkjetOperator.Services
{
    // Simple DTO for a text block (matches existing TextBlockDto shape)
    public record SimpleTextBlock(int BlockNumber, string Text, int X = 0, int Y = 0, int Size = 1, int Scale = 1);

    public static class InkjetProtocol
    {
        // Change program: FW,<prog>
        public static async Task<string> SendChangeProgramAsync(IDeviceClient client, int programNumber, CancellationToken ct = default)
        {
            return await client.SendCommandAsync($"FW,{programNumber}", 2000, ct);
        }

        // FS: set text content (FS,{prog},{deviceBlock},0,{text})
        public static async Task<string> SendFsAsync(IDeviceClient client, int programNumber, int deviceBlock, string text, CancellationToken ct = default)
        {
            // ensure ASCII-safe text or sanitize as required
            string safe = text ?? "";
            return await client.SendCommandAsync($"FS,{programNumber},{deviceBlock},0,{safe}", 2000, ct);
        }

        // F1: format: F1,{prog},{block},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0
        // here sizeConverted is kept equal to size for simplicity; adapt conversion if needed.
        public static async Task<string> SendF1Async(IDeviceClient client, int programNumber, int deviceBlock, int scale, int size, int x = 0, int y = 0, CancellationToken ct = default)
        {
            int sizeConverted = size; // map if required
            return await client.SendCommandAsync($"F1,{programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0", 2000, ct);
        }

        // Send a single text block (FS then F1)
        public static async Task<(string fsResp, string f1Resp)> SendTextBlockAsync(IDeviceClient client, int programNumber, SimpleTextBlock block, CancellationToken ct = default)
        {
            int deviceBlock = block.BlockNumber + 5; // mapping 1->6
            string fs = await SendFsAsync(client, programNumber, deviceBlock, block.Text, ct);
            if (string.IsNullOrEmpty(fs)) { /* no response but continue */ }

            string f1 = await SendF1Async(client, programNumber, deviceBlock, block.Scale, block.Size, block.X, block.Y, ct);
            return (fs, f1);
        }

        // Send config (FM) — simplified form, adapt parameters as needed
        public static async Task<string> SendConfigAsync(IDeviceClient client, int programNumber, string programName, int triggerDelay = 0, int height = 100, int width = 200, int direction = 0, CancellationToken ct = default)
        {
            // Program name normalization is not necessary here; ensure no commas that break protocol.
            string safeName = programName?.Replace(",", " ") ?? "";
            string fm = $"FM,{programNumber},0,{safeName},0,{direction},0,0,01,20,500,{triggerDelay},300,1000,{height},{width},15,0,0,6";
            return await client.SendCommandAsync(fm, 2000, ct);
        }

        public static Task<string> SendSuspendAsync(IDeviceClient client, CancellationToken ct = default)
            => client.SendCommandAsync("SR", 1000, ct);

        public static Task<string> SendResumeAsync(IDeviceClient client, CancellationToken ct = default)
            => client.SendCommandAsync("SQ", 1000, ct);
    }
}