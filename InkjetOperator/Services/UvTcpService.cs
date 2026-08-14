using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace InkjetOperator.Services;

/// <summary>
/// สั่งเครื่อง UV ผ่าน TCP (ค่าเริ่มต้นพอร์ต 10086) ด้วยคำสั่ง KEY
///   KEY:85 + DATA = โหลดโปรแกรม (.uvdx)
///   KEY:84 = หยุด
///   KEY:83 = เริ่มพิมพ์
/// เครื่องปิด connection หลังตอบทุกครั้ง จึงต้องเปิดใหม่ต่อ 1 คำสั่ง
/// </summary>
public class UvTcpService
{
    private const int DefaultPort = 10086;
    private const int TimeoutMs = 5000;
    private const int ReadTimeoutMs = 3000;

    /// <summary>KEY:84 — สั่งหยุดเครื่อง</summary>
    public Task<(bool ok, string log)> StopAsync(string ip, int port)
        => SendKeyAsync(ip, port, new { KEY = 84 }, "สั่งหยุดเครื่อง");

    /// <summary>KEY:85 โหลดโปรแกรม รอให้เครื่องโหลดเสร็จ แล้ว KEY:83 สั่งเริ่มพิมพ์</summary>
    public async Task<(bool ok, string log)> LoadAndStartAsync(string ip, int port, string programName)
    {
        var (loadOk, loadLog) = await SendKeyAsync(
            ip, port,
            new { KEY = 85, DATA = $"{programName}.uvdx" },
            $"โหลดโปรแกรม {programName}.uvdx");

        if (!loadOk) return (false, loadLog);

        await Task.Delay(1000);

        var (startOk, startLog) = await SendKeyAsync(ip, port, new { KEY = 83 }, "สั่งเริ่มพิมพ์");

        // start ที่ไม่ตอบรับไม่นับว่าล้ม — เครื่องอาจยังไม่พร้อมแต่รับคำสั่งไว้แล้ว
        var log = loadLog + (startOk
            ? startLog
            : startLog.TrimEnd() + " (เครื่องอาจยังไม่พร้อม)" + Environment.NewLine);

        return (true, log);
    }

    /// <summary>เปิด TCP ใหม่ ส่ง 1 คำสั่ง แล้วอ่านผลกลับ</summary>
    private static async Task<(bool ok, string log)> SendKeyAsync(
        string ip, int port, object command, string label)
    {
        if (port <= 0) port = DefaultPort;

        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
        }
        catch (Exception ex)
        {
            return (false, $"{label} → เชื่อมต่อ {ip}:{port} ไม่สำเร็จ ({ex.Message})" + Environment.NewLine);
        }

        var stream = client.GetStream();
        await SendJsonAsync(stream, JsonSerializer.Serialize(command));

        var (rs, _) = await ReadJsonResponseAsync(stream);
        var ok = rs == 0;

        return (ok, $"{label} → {(ok ? "สำเร็จ" : "ล้มเหลว")}" + Environment.NewLine);
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
