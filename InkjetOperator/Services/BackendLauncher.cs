using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace InkjetOperator.Services;

public static class BackendLauncher
{
    public const int Port = 3000;

    private const int StartupWaitMs = 20000;
    private const int PollMs = 300;

    public static async Task<string?> EnsureRunningAsync()
    {
        if (!BackendIsLocal()) return null;
        if (IsListening()) return null;      // เปิดค้างไว้อยู่แล้ว หรืออีกสถานีเปิดไว้

        var folder = CustomSettingsManager.Read("BACKEND_PATH", "").Trim();
        if (folder.Length == 0)
            return "ยังไม่ได้ตั้งโฟลเดอร์ backend ที่หน้า Backend Setting";

        var entry = Path.Combine(folder, "index.js");
        if (!File.Exists(entry))
            return $"ไม่พบไฟล์ index.js ในโฟลเดอร์ที่ตั้งไว้\n{folder}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "index.js",
                WorkingDirectory = folder,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch (Exception ex)
        {
            return $"เปิด backend ไม่ได้: {ex.Message}\n\nตรวจว่าติดตั้ง Node.js แล้วหรือยัง";
        }

        return await WaitUntilListeningAsync()
            ? null
            : $"เปิด backend แล้วแต่ยังไม่ตอบใน {StartupWaitMs / 1000} วินาที\n"
              + "ตรวจว่า PostgreSQL เปิดอยู่ และไฟล์ .env ของ backend ถูกต้อง";
    }

    private static bool BackendIsLocal()
    {
        var ip = CustomSettingsManager.Read("PC_IP", "127.0.0.1").Trim();

        if (ip.Length == 0) return true;     // ยังไม่ตั้ง = ค่าเริ่มต้นคือเครื่องนี้
        if (string.Equals(ip, "localhost", StringComparison.OrdinalIgnoreCase)) return true;

        if (!IPAddress.TryParse(ip, out var parsed)) return false;
        if (IPAddress.IsLoopback(parsed)) return true;

        return IsOwnAddress(parsed);
    }

    private static bool IsOwnAddress(IPAddress address)
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Any(unicast => unicast.Address.Equals(address));
        }
        catch
        {
            return false;   // ถามระบบไม่ได้ ถือว่าเป็นเครื่องอื่นเหมือนเดิม
        }
    }

    private static bool IsListening()
    {
        try
        {
            return IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(endpoint => endpoint.Port == Port);
        }
        catch
        {
            return false;   // ถามระบบไม่ได้ก็ลองเปิดไปเลย ดีกว่าไม่เปิดแล้วใช้งานไม่ได้
        }
    }

    private static async Task<bool> WaitUntilListeningAsync()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(StartupWaitMs);

        while (DateTime.UtcNow < deadline)
        {
            if (IsListening()) return true;
            await Task.Delay(PollMs);
        }

        return IsListening();
    }
}
