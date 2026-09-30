using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace InkjetOperator.Services;

public class UvTcpService
{
    private const int DefaultPort = 10086;
    private const int ConnectTimeoutMs = 5000;
    private const int ReadTimeoutMs = 3000;

    private const int LoadReadTimeoutMs = 15000;

    public Task<(bool ok, string log)> StopAsync(string ip, int port)
        => SendKeyAsync(ip, port, new { KEY = 84 }, "สั่งหยุดเครื่อง", ReadTimeoutMs);

    public Task<(bool ok, string log)> LoadAsync(string ip, int port, string programName)
        => SendKeyAsync(ip, port,
            new { KEY = 85, DATA = $"{programName}.uvdx" },
            $"โหลดโปรแกรม {programName}.uvdx",
            LoadReadTimeoutMs);

    public Task<(bool ok, string log)> StartAsync(string ip, int port)
        => SendKeyAsync(ip, port, new { KEY = 83 }, "สั่งเริ่มพิมพ์", ReadTimeoutMs);

    public async Task<(bool ok, string log, string? startWarning)> LoadAndStartAsync(string ip, int port, string programName) // โหลดข้อมูลก่อนสั่ง Start แล้วแยกผลสองขั้น
    {
        var (loadOk, loadLog) = await SendKeyAsync( // ส่งคำสั่ง Load และเก็บผลตอบ
            ip, port, // ใช้ปลายทาง UV ที่ผู้เรียกระบุ
            new { KEY = 85, DATA = $"{programName}.uvdx" }, // KEY 85 โหลดไฟล์ชื่อโปรแกรมนี้
            $"โหลดโปรแกรม {programName}.uvdx", // แนบชื่อโปรแกรมไว้ใน log
            LoadReadTimeoutMs); // ใช้เวลารอเฉพาะคำสั่ง Load

        if (!loadOk) return (false, loadLog, null); // Load ไม่ผ่าน จึงยังไม่ส่ง Start

        await Task.Delay(1000); // เว้นหนึ่งวินาทีหลังโหลดก่อนสั่งเริ่ม

        var (startOk, startLog) = await SendKeyAsync( // ส่งคำสั่ง Start แยกจากการโหลด
            ip, port, new { KEY = 83 }, "สั่งเริ่มพิมพ์", ReadTimeoutMs); // สั่ง Start แล้วเก็บผลแยกจากการโหลดข้อมูล
        return (true, loadLog + startLog, startOk ? null : startLog.Trim()); // Load ผ่านถือว่าส่งข้อมูลแล้ว; Start ไม่ผ่านเก็บเป็นคำเตือน
    }

    private static async Task<(bool ok, string log)> SendKeyAsync( // ส่งคำสั่งหนึ่งชุดแล้วอ่านผลจาก UV
        string ip, int port, object command, string label, int readTimeoutMs) // รับปลายทาง คำสั่ง ชื่อขั้น และเวลารอ
    {
        if (port <= 0) port = DefaultPort; // พอร์ตไม่ถูกต้องให้ใช้ค่าเริ่มต้น

        using var client = new TcpClient(); // สร้าง TCP แล้วปิดให้อัตโนมัติเมื่อจบ
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(ConnectTimeoutMs)); // ต่อ UV โดยมีเวลารอสูงสุด
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return (false, $"{label} → เชื่อมต่อ {ip}:{port} ไม่สำเร็จ ({ex.Message})" + Environment.NewLine); // ระบุ IP และพอร์ตที่เชื่อมต่อไม่ได้
        }

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var stream = client.GetStream(); // เปิดช่องรับส่งข้อมูลของ TCP นี้
            await SendJsonAsync(stream, JsonSerializer.Serialize(command)); // แปลงคำสั่งเป็น JSON แล้วส่งไป UV

            var (rs, detail) = await ReadJsonResponseAsync(stream, readTimeoutMs); // อ่านรหัสตอบและรายละเอียดจากเครื่อง
            var ok = rs == 0; // เครื่องยืนยันคำสั่งเมื่อ RS เป็นศูนย์เท่านั้น

            var log = ok // เลือกข้อความ log ตามผลตอบ UV
                ? $"{label} → สำเร็จ" // บันทึกว่าคำสั่งนี้เครื่องตอบรับ
                : $"{label} → ล้มเหลว ({detail})"; // บันทึกเหตุที่เครื่องไม่รับคำสั่ง

            return (ok, log + Environment.NewLine); // ส่งทั้งสถานะและ log ให้ผู้เรียก
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return (false, $"{label} → ส่งคำสั่งไม่ได้ ({ex.Message})" + Environment.NewLine); // ระบุเหตุที่ส่งหรืออ่านคำสั่งไม่ได้
        }
    }

    private static async Task SendJsonAsync(NetworkStream stream, string json) // ส่ง JSON ผ่านช่อง TCP ที่เปิดไว้
    {
        var data = Encoding.UTF8.GetBytes(json); // แปลง JSON เป็นไบต์ UTF-8
        using var timeout = new CancellationTokenSource(ReadTimeoutMs); // ตั้งเวลาสูงสุดสำหรับเขียนข้อมูล
        await stream.WriteAsync(data.AsMemory(), timeout.Token); // ส่งไบต์ทั้งหมดพร้อมตัวคุมเวลา
        await stream.FlushAsync(timeout.Token); // ผลักข้อมูลที่ค้างในช่องส่งออกให้ครบ
    }

    private static async Task<(int rs, string detail)> ReadJsonResponseAsync( // อ่าน JSON ตอบกลับและแยกรหัส RS
        NetworkStream stream, int timeoutMs) // รับช่อง TCP กับเวลารอผล
    {
        var buffer = new byte[4096]; // เตรียมพื้นที่รับข้อมูล 4096 ไบต์
        string raw; // เก็บข้อความดิบที่เครื่องตอบมา

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var read = await stream.ReadAsync(buffer).AsTask() // รออ่านข้อมูลจากเครื่อง
                .WaitAsync(TimeSpan.FromMilliseconds(timeoutMs)); // ยกเลิกรอเมื่อเกินเวลาที่กำหนด
            raw = Encoding.UTF8.GetString(buffer, 0, read).Trim(); // อ่านเฉพาะไบต์ที่ได้รับแล้วตัดช่องว่าง
        }
        catch (TimeoutException) // จับกรณีเครื่องตอบช้ากว่าเวลารอ
        {
            return (-1, $"เครื่องไม่ตอบกลับใน {timeoutMs / 1000} วินาที"); // บอกเวลาที่รอแล้วไม่ได้คำตอบ
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return (-1, $"อ่านผลไม่ได้ — {ex.Message}"); // แนบเหตุที่อ่านคำตอบไม่ได้
        }

        if (raw.Length == 0) // ตรวจว่าเครื่องไม่ส่งข้อความกลับมาเลย
            return (-1, "เครื่องปิดการเชื่อมต่อโดยไม่ตอบกลับ"); // ระบุว่าเครื่องปิด TCP โดยไม่มีคำตอบ

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            using var doc = JsonDocument.Parse(raw); // แปลงข้อความตอบกลับเป็น JSON
            var rs = doc.RootElement.GetProperty("RS").GetInt32(); // อ่านรหัสผล RS จากคำตอบเครื่อง
            return (rs, rs == 0 ? "" : $"เครื่องปฏิเสธคำสั่ง RS={rs}"); // RS ไม่เป็นศูนย์ให้ระบุว่าเครื่องปฏิเสธ
        }
        catch // เข้าทางนี้เมื่อทำรายการไม่สำเร็จ
        {
            return (-1, $"ตอบกลับผิดรูปแบบ — {raw}"); // แนบคำตอบดิบเมื่อรูปแบบไม่ถูกต้อง
        }
    }
}
