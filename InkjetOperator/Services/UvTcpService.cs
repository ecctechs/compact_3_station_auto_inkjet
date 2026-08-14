using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace InkjetOperator.Services;

public class UvTcpService
{
    private const int DefaultPort = 10086;
    private const int TimeoutMs = 5000;
    private const int ReadTimeoutMs = 3000;

    public async Task<(bool ok, string log)> LoadAndStartAsync(string ip, int port, string programName)
    {
        if (port <= 0) port = DefaultPort;
        var log = new StringBuilder();

        // KEY:85 — load program
        using (var client85 = new TcpClient())
        {
            try
            {
                await client85.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            }
            catch (Exception ex)
            {
                return (false, $"เชื่อมต่อ {ip}:{port} ไม่สำเร็จ: {ex.Message}");
            }

            var loadCmd = JsonSerializer.Serialize(new { KEY = 85, DATA = $"{programName}.uvdx" });
            await SendJsonAsync(client85.GetStream(), loadCmd);

            var (rs85, _) = await ReadJsonResponseAsync(client85.GetStream());
            log.AppendLine($"โหลดโปรแกรม {programName}.uvdx → {(rs85 == 0 ? "สำเร็จ" : "ล้มเหลว")}");

            if (rs85 != 0)
                return (false, log.ToString());
        }

        await Task.Delay(1000);

        // KEY:83 — start print
        using var clientStart = new TcpClient();
        try
        {
            await clientStart.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
        }
        catch (Exception ex)
        {
            return (false, log.AppendLine($"เชื่อมต่อสำหรับ Start ไม่สำเร็จ: {ex.Message}").ToString());
        }

        var startCmd = JsonSerializer.Serialize(new { KEY = 83 });
        await SendJsonAsync(clientStart.GetStream(), startCmd);

        var (rs83, _) = await ReadJsonResponseAsync(clientStart.GetStream());
        log.AppendLine($"สั่ง Start → {(rs83 == 0 ? "สำเร็จ" : "ล้มเหลว (เครื่องอาจยังไม่พร้อม)")}");

        return (true, log.ToString());
    }

    private static async Task SendJsonAsync(NetworkStream stream, string json)
    {
        var data = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(data);
        await stream.FlushAsync();
    }

    private static async Task<(int rs, string raw)> ReadJsonResponseAsync(NetworkStream stream)
    {
        var buffer = new byte[4096];
        try
        {
            var read = await stream.ReadAsync(buffer).AsTask()
                .WaitAsync(TimeSpan.FromMilliseconds(ReadTimeoutMs));
            var raw = Encoding.UTF8.GetString(buffer, 0, read).Trim();

            using var doc = JsonDocument.Parse(raw);
            var rs = doc.RootElement.GetProperty("RS").GetInt32();
            return (rs, raw);
        }
        catch
        {
            return (-1, "(no response)");
        }
    }
}
