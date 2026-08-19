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
    private const int ConnectTimeoutMs = 5000;
    private const int ReadTimeoutMs = 3000;

    // โหลดโปรแกรมเป็นงานหนักกว่าสั่งหยุด/เริ่ม เครื่องใช้เวลาตอบนานกว่ามาก
    private const int LoadReadTimeoutMs = 15000;

    /// <summary>KEY:84 — สั่งหยุดเครื่อง</summary>
    public Task<(bool ok, string log)> StopAsync(string ip, int port)
        => SendKeyAsync(ip, port, new { KEY = 84 }, "สั่งหยุดเครื่อง", ReadTimeoutMs);

    /// <summary>KEY:85 — โหลดโปรแกรมอย่างเดียว ไม่สั่งเริ่มพิมพ์ (ใช้ในหน้าทดสอบ)</summary>
    public Task<(bool ok, string log)> LoadAsync(string ip, int port, string programName)
        => SendKeyAsync(ip, port,
            new { KEY = 85, DATA = $"{programName}.uvdx" },
            $"โหลดโปรแกรม {programName}.uvdx",
            LoadReadTimeoutMs);

    /// <summary>KEY:83 — สั่งเริ่มพิมพ์อย่างเดียว (ใช้ในหน้าทดสอบ)</summary>
    public Task<(bool ok, string log)> StartAsync(string ip, int port)
        => SendKeyAsync(ip, port, new { KEY = 83 }, "สั่งเริ่มพิมพ์", ReadTimeoutMs);

    /// <summary>KEY:85 โหลดโปรแกรม รอให้เครื่องโหลดเสร็จ แล้ว KEY:83 สั่งเริ่มพิมพ์</summary>
    public async Task<(bool ok, string log)> LoadAndStartAsync(string ip, int port, string programName)
    {
        var (loadOk, loadLog) = await SendKeyAsync(
            ip, port,
            new { KEY = 85, DATA = $"{programName}.uvdx" },
            $"โหลดโปรแกรม {programName}.uvdx",
            LoadReadTimeoutMs);

        if (!loadOk) return (false, loadLog);

        await Task.Delay(1000);

        var (startOk, startLog) = await SendKeyAsync(
            ip, port, new { KEY = 83 }, "สั่งเริ่มพิมพ์", ReadTimeoutMs);

        // start ที่ไม่ตอบรับไม่นับว่าล้ม — เครื่องอาจยังไม่พร้อมแต่รับคำสั่งไว้แล้ว
        var log = loadLog + (startOk
            ? startLog
            : startLog.TrimEnd() + " (เครื่องอาจยังไม่พร้อม)" + Environment.NewLine);

        return (true, log);
    }

    /// <summary>เปิด TCP ใหม่ ส่ง 1 คำสั่ง แล้วอ่านผลกลับ</summary>
    private static async Task<(bool ok, string log)> SendKeyAsync(
        string ip, int port, object command, string label, int readTimeoutMs)
    {
        if (port <= 0) port = DefaultPort;

        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(ConnectTimeoutMs));
        }
        catch (Exception ex)
        {
            return (false, $"{label} → เชื่อมต่อ {ip}:{port} ไม่สำเร็จ ({ex.Message})" + Environment.NewLine);
        }

        var stream = client.GetStream();
        await SendJsonAsync(stream, JsonSerializer.Serialize(command));

        var (rs, detail) = await ReadJsonResponseAsync(stream, readTimeoutMs);
        var ok = rs == 0;

        var log = ok
            ? $"{label} → สำเร็จ"
            : $"{label} → ล้มเหลว ({detail})";

        return (ok, log + Environment.NewLine);
    }

    private static async Task SendJsonAsync(NetworkStream stream, string json)
    {
        var data = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(data);
        await stream.FlushAsync();
    }

    /// <summary>
    /// อ่านผลกลับ แล้วแยกสาเหตุให้ชัดว่าล้มเพราะอะไร
    /// รวมทุกกรณีเป็น "ล้มเหลว" เฉยๆ ทำให้หน้างานเดาไม่ออกว่าต้องแก้ที่ไหน
    /// </summary>
    private static async Task<(int rs, string detail)> ReadJsonResponseAsync(
        NetworkStream stream, int timeoutMs)
    {
        var buffer = new byte[4096];
        string raw;

        try
        {
            var read = await stream.ReadAsync(buffer).AsTask()
                .WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
            raw = Encoding.UTF8.GetString(buffer, 0, read).Trim();
        }
        catch (TimeoutException)
        {
            return (-1, $"เครื่องไม่ตอบกลับใน {timeoutMs / 1000} วินาที");
        }
        catch (Exception ex)
        {
            return (-1, $"อ่านผลไม่ได้ — {ex.Message}");
        }

        if (raw.Length == 0)
            return (-1, "เครื่องปิดการเชื่อมต่อโดยไม่ตอบกลับ");

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var rs = doc.RootElement.GetProperty("RS").GetInt32();
            return (rs, rs == 0 ? "" : $"เครื่องปฏิเสธคำสั่ง RS={rs}");
        }
        catch
        {
            return (-1, $"ตอบกลับผิดรูปแบบ — {raw}");
        }
    }
}
