using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.Services;

public class UvTcpService
{
    private const int DefaultPort = 10086;
    private const int TimeoutMs = 5000;

    public async Task<(bool ok, string log)> LoadAndStartAsync(string ip, int port, string programName)
    {
        if (port <= 0) port = DefaultPort;
        var log = new StringBuilder();

        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            log.AppendLine($"เชื่อมต่อ {ip}:{port} สำเร็จ");
        }
        catch (Exception ex)
        {
            return (false, $"เชื่อมต่อ {ip}:{port} ไม่สำเร็จ: {ex.Message}");
        }

        var stream = client.GetStream();
        stream.ReadTimeout = TimeoutMs;
        stream.WriteTimeout = TimeoutMs;

        var loadCmd = $"KEY:85,{programName}";
        var (loadOk, loadReply) = await SendCommandAsync(stream, loadCmd);
        log.AppendLine($"KEY:85 → {loadReply}");
        if (!loadOk) return (false, log.ToString());

        await Task.Delay(500);

        var (startOk, startReply) = await SendCommandAsync(stream, "KEY:84");
        log.AppendLine($"KEY:84 → {startReply}");
        if (!startOk) return (false, log.ToString());

        return (true, log.ToString());
    }

    private static async Task<(bool ok, string reply)> SendCommandAsync(NetworkStream stream, string command)
    {
        try
        {
            var data = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(data);
            await stream.FlushAsync();

            var buffer = new byte[1024];
            var read = await stream.ReadAsync(buffer).AsTask()
                .WaitAsync(TimeSpan.FromMilliseconds(3000));
            var reply = Encoding.ASCII.GetString(buffer, 0, read).Trim();
            return (true, reply);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
