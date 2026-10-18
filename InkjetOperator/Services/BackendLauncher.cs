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
    /// ปิด backend ที่รันอยู่บนเครื่องนี้แล้วเปิดใหม่ — ปุ่มในหน้า System Health (โหมด Dev)
    /// คืนข้อความปัญหา หรือ null เมื่อเปิดใหม่สำเร็จ
    ///
    /// <para>
    /// มีไว้หลังอัปเดตไฟล์ backend — <see cref="EnsureRunningAsync"/> เปิดให้เฉพาะตอนไม่มีตัวไหน
    /// รันอยู่ ถ้าตัวเก่ายังรันค้าง ก็ใช้ตัวเก่าต่อไปจนกว่าจะปิดเครื่อง โดยไม่มีใครรู้
    /// </para>
    /// <para>
    /// ทำได้เฉพาะเครื่องที่ backend อยู่ และปิดเฉพาะ node ที่ฟังพอร์ต 3000 — ถ้าพอร์ตนี้เป็นของ
    /// โปรแกรมอื่นจะไม่ปิดให้ · ตรวจโฟลเดอร์ backend ก่อนปิด ปิดได้แต่เปิดไม่ได้จะแย่กว่าเดิม
    /// </para>
    /// </summary>
    public static async Task<string?> RestartAsync()
    {
        if (!BackendIsLocal())
            return "backend อยู่เครื่องอื่น — สั่งรีสตาร์ทข้ามเครื่องไม่ได้ ต้องทำที่เครื่องนั้น";

        var folder = CustomSettingsManager.Read("BACKEND_PATH", "").Trim();
        if (folder.Length == 0)
            return "ยังไม่ได้ตั้งโฟลเดอร์ backend ที่หน้า Backend Setting — ไม่ปิดตัวเดิม เพราะจะเปิดใหม่ไม่ได้";
        if (!File.Exists(Path.Combine(folder, "index.js")))
            return $"ไม่พบไฟล์ index.js ในโฟลเดอร์ที่ตั้งไว้ — ไม่ปิดตัวเดิม เพราะจะเปิดใหม่ไม่ได้\n{folder}";

        // กันตัวเฝ้า (watchdog) เห็นพอร์ตว่างช่วงรอยต่อแล้วเปิดซ้อน
        if (Interlocked.Exchange(ref _restarting, 1) == 1)
            return "กำลังเปิด backend อยู่แล้ว รอสักครู่แล้วดูหน้านี้อีกครั้ง";

        try
        {
            var problem = await Task.Run(StopRunning);
            if (problem != null)
            {
                AppLog.Warn("Backend", "รีสตาร์ท backend ไม่สำเร็จ: " + problem);
                return problem;
            }

            AppLog.Info("Backend", "รีสตาร์ท backend: ปิดตัวเดิมแล้ว กำลังเปิดใหม่");
            problem = await EnsureRunningAsync();
            if (problem == null) AppLog.Info("Backend", "รีสตาร์ท backend เสร็จ");
            else AppLog.Warn("Backend", "รีสตาร์ท backend: " + problem);
            return problem;
        }
        catch (Exception ex)
        {
            // เช่น เรียก netstat ไม่ได้ — บอกเป็นข้อความ ไม่ให้หลุดไปถึงปุ่ม
            AppLog.Error("Backend.Restart", ex);
            return $"รีสตาร์ท backend ไม่สำเร็จ: {ex.Message}";
        }
        finally
        {
            Interlocked.Exchange(ref _restarting, 0);
        }
    }

    /// <summary>
    /// เคลียร์พอร์ต 3000 อย่างเดียว ไม่เปิดตัวใหม่ให้ — null = พอร์ตว่างแล้ว
    ///
    /// <para>
    /// ต่างจาก <see cref="RestartAsync"/> ตรงที่ไม่ต้องมีโฟลเดอร์ backend ที่ใช้ได้
    /// เพราะไม่ได้จะเปิดใหม่ ใช้ตอนมี node ค้างจากรอบก่อนแล้วเปิดตัวใหม่ไม่ขึ้น
    /// ซึ่งโฟลเดอร์อาจยังตั้งไม่เสร็จหรือชี้ผิดอยู่
    /// </para>
    /// <para>
    /// ยังปิดเฉพาะ node ที่ฟังพอร์ตนี้เหมือนเดิม ถ้าเป็นโปรแกรมอื่นจะบอกชื่อกลับมา
    /// แล้วไม่แตะ — พอร์ต 3000 เป็นพอร์ตยอดนิยม ปิดมั่วอาจไปปิดของที่ไม่เกี่ยวกัน
    /// </para>
    /// <para>
    /// หยุดตัวเฝ้าไว้ระหว่างทำ ไม่งั้นมันเห็นพอร์ตว่างแล้วเปิด backend ขึ้นมาใหม่ทันที
    /// คนกดจะงงว่าเคลียร์แล้วทำไมยังไม่ว่าง
    /// </para>
    /// </summary>
    public static async Task<string?> ClearPortAsync()
    {
        if (!BackendIsLocal())
            return $"backend อยู่เครื่องอื่น — เคลียร์พอร์ต {Port} ข้ามเครื่องไม่ได้ ต้องทำที่เครื่องนั้น";

        if (Interlocked.Exchange(ref _restarting, 1) == 1)
            return "กำลังเปิดหรือปิด backend อยู่แล้ว รอสักครู่แล้วลองใหม่";

        try
        {
            if (!IsListening())
                return null;

            var problem = await Task.Run(StopRunning);
            if (problem != null)
            {
                AppLog.Warn("Backend", $"เคลียร์พอร์ต {Port} ไม่สำเร็จ: " + problem);
                return problem;
            }

            AppLog.Info("Backend", $"เคลียร์พอร์ต {Port} แล้ว");
            return null;
        }
        catch (Exception ex)
        {
            AppLog.Error("Backend.ClearPort", ex);
            return $"เคลียร์พอร์ต {Port} ไม่สำเร็จ: {ex.Message}";
        }
        finally
        {
            Interlocked.Exchange(ref _restarting, 0);
        }
    }

    /// <summary>ปิด node ทุกตัวที่ฟังพอร์ต 3000 แล้วรอจนพอร์ตว่าง — null = เรียบร้อย</summary>
    private static string? StopRunning()
    {
        var running = new List<Process>();
        try
        {
            foreach (var pid in ListenerPids())
            {
                try { running.Add(Process.GetProcessById(pid)); }
                catch (ArgumentException) { /* ปิดไปเองแล้ว */ }
            }

            // ตรวจครบทุกตัวก่อนลงมือ ไม่ปิดไปครึ่งเดียวแล้วค่อยเจอว่าอีกตัวไม่ใช่ backend
            var other = running.FirstOrDefault(p => !string.Equals(p.ProcessName, "node", StringComparison.OrdinalIgnoreCase));
            if (other != null)
                return $"พอร์ต {Port} ถูกโปรแกรม {other.ProcessName} (PID {other.Id}) ใช้อยู่ ไม่ใช่ backend — ไม่ปิดให้";

            foreach (var p in running)
            {
                try
                {
                    p.Kill();
                    p.WaitForExit(5000);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    return $"ปิด backend ตัวเดิมไม่ได้: {ex.Message}\n\n"
                        + "อาจเปิดไว้ด้วยสิทธิ์ admin หรือผู้ใช้อื่น — ปิดเครื่องแล้วเปิดใหม่แทน";
                }
                catch (InvalidOperationException)
                {
                    // ปิดไปเองระหว่างทาง
                }
            }
        }
        finally
        {
            foreach (var p in running) p.Dispose();
        }

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (IsListening() && DateTime.UtcNow < deadline) Thread.Sleep(PollMs);
        return IsListening() ? $"ปิด backend ตัวเดิมแล้วแต่พอร์ต {Port} ยังไม่ว่าง ลองกดอีกครั้ง" : null;
    }

    /// <summary>
    /// PID ของโปรเซสที่ฟังพอร์ต 3000 — อ่านจาก netstat -ano
    /// <para>
    /// ดูจากที่อยู่ปลายทางเป็น 0.0.0.0:0 / [::]:0 (ตัวที่รอรับ) ไม่ใช้คำว่า LISTENING
    /// ซึ่งอาจเป็นภาษาอื่นตามภาษาของ Windows
    /// </para>
    /// </summary>
    private static List<int> ListenerPids()
    {
        using var netstat = Process.Start(new ProcessStartInfo("netstat", "-ano")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        })!;
        var output = netstat.StandardOutput.ReadToEnd();
        netstat.WaitForExit(5000);

        var pids = new HashSet<int>();
        foreach (var line in output.Split('\n'))
        {
            var cols = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (cols.Length < 5 || !cols[0].Equals("TCP", StringComparison.OrdinalIgnoreCase)) continue;
            if (!cols[1].EndsWith($":{Port}", StringComparison.Ordinal)) continue;
            if (cols[2] is not ("0.0.0.0:0" or "[::]:0")) continue;
            if (int.TryParse(cols[^1], out var pid) && pid > 0) pids.Add(pid);
        }
        return pids.ToList();
    }

    /// <summary>ตรวจทุกกี่มิลลิวินาทีว่า backend ยังอยู่</summary>
    private const int WatchdogMs = 15000;

    private static System.Threading.Timer? _watchdog;

    /// <summary>1 = กำลังเปิด backend ใหม่อยู่ — กันรอบตรวจถัดไปเปิดซ้อน</summary>
    private static int _restarting;

    /// <summary>ปัญหาล่าสุดที่บันทึกไว้ — บันทึกซ้ำเฉพาะตอนเปลี่ยน ไม่ให้ log ท่วม</summary>
    private static string? _lastProblem;

    /// <summary>
    /// คอยดูว่า backend ยังทำงานอยู่ ถ้าหายไปให้เปิดใหม่ — ทำเฉพาะเครื่องที่ backend อยู่
    /// <para>
    /// เดิมเปิดให้ครั้งเดียวตอนเปิดโปรแกรม ถ้า backend ล้มกลางกะ ทั้งสามสถานีต่อไม่ได้
    /// จนกว่าจะมีคนปิดเปิดโปรแกรม ST1 ใหม่ ซึ่งหน้างานไม่รู้ว่าต้องทำ
    /// </para>
    /// <para>
    /// ดูแค่ว่ามีใครฟังพอร์ตอยู่ไหม ไม่ได้ถามว่าฐานข้อมูลพร้อมหรือยัง — backend
    /// ลองต่อฐานข้อมูลใหม่เองอยู่แล้ว ตรงนี้มีหน้าที่แค่ไม่ให้ process หายไปเฉย ๆ
    /// </para>
    /// </summary>
    public static void StartWatchdog()
    {
        if (_watchdog != null || !BackendIsLocal()) return;
        _watchdog = new System.Threading.Timer(_ => _ = CheckAliveAsync(), null, WatchdogMs, WatchdogMs);
    }

    public static void StopWatchdog()
    {
        _watchdog?.Dispose();
        _watchdog = null;
    }

    private static async Task CheckAliveAsync()
    {
        if (IsListening()) return;
        if (Interlocked.Exchange(ref _restarting, 1) == 1) return;

        try
        {
            if (_lastProblem == null) AppLog.Warn("Backend", "backend ไม่ตอบที่พอร์ต 3000 — กำลังเปิดใหม่");

            var problem = await EnsureRunningAsync();
            if (problem == null)
            {
                AppLog.Info("Backend", "เปิด backend ใหม่แล้ว");
            }
            else if (problem != _lastProblem)
            {
                AppLog.Warn("Backend", problem);
            }
            _lastProblem = problem;
        }
        catch (Exception ex)
        {
            AppLog.Error("Backend.Watchdog", ex);
        }
        finally
        {
            Interlocked.Exchange(ref _restarting, 0);
        }
    }

    /// <summary>backend อยู่เครื่องนี้ไหม — หน้า System Health ใช้ตัดสินว่าปุ่มรีสตาร์ทกดได้ไหม</summary>
    public static bool IsLocal() => BackendIsLocal();

    /// <summary>
    /// backend อยู่เครื่องนี้ไหม — ดูจากที่อยู่ที่ทุกหน้าใช้ต่อ
    /// ตั้งไม่ตรงกับความจริงคือตั้งค่าผิด ไม่ใช่เรื่องที่ตรงนี้จะเดาให้
    ///
    /// <para>
    /// นับ IP วงแลนของเครื่องนี้เองด้วย ไม่ใช่แค่ 127.0.0.1 — เครื่องที่เป็นตัวหลัก
    /// มักตั้ง <c>PC_IP</c> เป็น IP แลนของตัวเอง (เช่น 192.168.1.72) ให้ตรงกับที่
    /// สถานีอื่นใช้ต่อเข้ามา เดิมกรณีนี้ถูกนับเป็นเครื่องอื่นแล้วข้ามการเปิดไปเงียบ ๆ
    /// </para>
    /// </summary>
    private static bool BackendIsLocal()
    {
        var ip = CustomSettingsManager.Read("PC_IP", "127.0.0.1").Trim();

        if (ip.Length == 0) return true;     // ยังไม่ตั้ง = ค่าเริ่มต้นคือเครื่องนี้
        if (string.Equals(ip, "localhost", StringComparison.OrdinalIgnoreCase)) return true;

        if (!IPAddress.TryParse(ip, out var parsed)) return false;
        if (IPAddress.IsLoopback(parsed)) return true;

        return IsOwnAddress(parsed);
    }

    /// <summary>IP นี้เป็นของการ์ดแลนตัวใดตัวหนึ่งบนเครื่องนี้ไหม</summary>
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
