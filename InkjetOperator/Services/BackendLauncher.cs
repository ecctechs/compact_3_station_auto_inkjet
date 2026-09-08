using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace InkjetOperator.Services;

/// <summary>
/// เปิด backend ให้เองตอนโปรแกรมเริ่ม
///
/// <para>
/// หน้างานมีแค่จอเดียวและไม่มีใครนั่งเฝ้า การให้พนักงานเปิด terminal หรือ VS Code
/// แล้วพิมพ์คำสั่งก่อนใช้โปรแกรมทุกเช้าเป็นขั้นตอนที่ลืมได้ และพอลืมแล้วอาการที่เห็น
/// คือหน้าจอว่างเปล่าโดยไม่บอกสาเหตุ ในเมื่อยังไงก็ต้องเปิดโปรแกรมนี้อยู่แล้ว
/// ก็ให้มันเปิด backend ให้เลย
/// </para>
/// <para>
/// เปิดเฉพาะเมื่อ backend อยู่เครื่องเดียวกันเท่านั้น ดูจาก <c>PC_IP</c> ที่หน้า
/// Backend Setting — ถ้าชี้ไปเครื่องอื่น (เช่น mini PC) จะไม่ทำอะไร เพราะสั่งเปิด
/// โปรเซสข้ามเครื่องไม่ได้ และไม่ควรทำด้วย
/// </para>
/// <para>
/// ไม่ปิด backend ตอนปิดโปรแกรม เพราะอีกสถานีอาจกำลังใช้ตัวเดียวกันอยู่
/// </para>
/// </summary>
public static class BackendLauncher
{
    /// <summary>พอร์ตที่ backend ฟัง — ตรงกับที่ทุกหน้าใช้ต่อ (<c>http://ip:3000</c>)</summary>
    public const int Port = 3000;

    private const int StartupWaitMs = 20000;
    private const int PollMs = 300;

    /// <summary>
    /// เปิดถ้าจำเป็น — คืนข้อความปัญหา หรือ null เมื่อไม่มีอะไรต้องบอก
    ///
    /// <para>
    /// คืน null ทั้งกรณีที่เปิดสำเร็จ กรณีที่มีคนเปิดค้างไว้อยู่แล้ว และกรณีที่
    /// backend อยู่เครื่องอื่นซึ่งไม่ใช่หน้าที่เรา
    /// </para>
    /// </summary>
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
            // node index.js ไม่ใช่ npm run dev — dev เป็น nodemon ที่คอยรีสตาร์ท
            // เวลาไฟล์เปลี่ยน ซึ่งมีไว้ตอนเขียนโปรแกรม ไม่ใช่ตอนใช้งานจริง
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

    /// <summary>
    /// backend อยู่เครื่องนี้ไหม — ดูจากที่อยู่ที่ทุกหน้าใช้ต่อ
    /// ตั้งไม่ตรงกับความจริงคือตั้งค่าผิด ไม่ใช่เรื่องที่ตรงนี้จะเดาให้
    /// </summary>
    private static bool BackendIsLocal()
    {
        var ip = CustomSettingsManager.Read("PC_IP", "127.0.0.1").Trim();

        if (ip.Length == 0) return true;     // ยังไม่ตั้ง = ค่าเริ่มต้นคือเครื่องนี้
        if (string.Equals(ip, "localhost", StringComparison.OrdinalIgnoreCase)) return true;

        return IPAddress.TryParse(ip, out var parsed) && IPAddress.IsLoopback(parsed);
    }

    /// <summary>
    /// มีใครฟังพอร์ตนี้อยู่แล้วหรือยัง
    ///
    /// <para>
    /// ต้องเช็คก่อนเสมอ ไม่งั้นเปิดโปรแกรมสองสถานีบนเครื่องเดียวกัน หรือเปิดโปรแกรม
    /// ซ้ำสองครั้ง จะได้ node ตัวที่สองที่ล้มทันทีด้วย EADDRINUSE
    /// </para>
    /// </summary>
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
