using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace InkjetOperator.Services;

/// <summary>ผลการตรวจ path หนึ่งเส้น</summary>
public enum PathState
{
    /// <summary>ยังไม่ได้ตั้งค่า — ไม่ใช่ความผิดพลาด</summary>
    NotSet,

    /// <summary>มีอยู่จริง</summary>
    Ok,

    /// <summary>เครื่องอยู่ แต่ไม่มีไฟล์หรือโฟลเดอร์นี้</summary>
    Missing,

    /// <summary>เครื่องที่เก็บไฟล์ไม่ตอบ — ไฟล์อาจยังอยู่ครบ บอกไม่ได้</summary>
    HostDown,

    /// <summary>เครื่องรับการเชื่อมต่อ แต่อ่านไดเรกทอรีไม่ทันเวลา</summary>
    Slow,
}

public readonly record struct PathResult(PathState State, string Detail);

/// <summary>
/// ตรวจว่า path ที่ตั้งไว้ยังใช้ได้อยู่ไหม โดยไม่ทำให้โปรแกรมค้าง
///
/// <para>
/// path เกือบทั้งหมดของระบบนี้ชี้ไปเครื่องอื่น ซึ่งเปลี่ยนกติกาไปจากไฟล์ในเครื่องตัวเอง
/// สองข้อ:
/// </para>
/// <para>
/// หนึ่ง — <c>File.Exists</c> บนแชร์ที่เครื่องปลายทางดับ <b>ค้างได้ 20–60 วินาที</b>
/// และยกเลิกไม่ได้ ตั้ง timeout ได้แค่ "เลิกรอ" แต่เธรดข้างในยังค้างต่อจนกว่า SMB
/// จะยอมแพ้เอง ยิงซ้ำทุกรอบก็สะสมเธรดค้างไปเรื่อย ๆ
/// </para>
/// <para>
/// สอง — แชร์ที่หลุด <c>File.Exists</c> <b>คืน false เฉย ๆ ไม่โยน exception</b>
/// ถ้าเชื่อค่านั้นตรง ๆ จอจะขึ้นว่า "ไม่พบไฟล์" ทั้งที่ของจริงคือเครื่องปลายทางดับ
/// และไฟล์ยังอยู่ครบ คนหน้างานจะไปนั่งหาไฟล์ที่ไม่ได้หายไปไหน
/// </para>
/// <para>
/// จึงเช็คตัวเครื่องก่อนด้วย TCP 445 (SMB) ที่ยกเลิกได้จริงและมี timeout สั้น
/// เครื่องไม่ตอบก็จบตรงนั้น ไม่แตะไฟล์เลย — ไม่ค้าง ไม่สะสมเธรด และแยกออกว่า
/// "เครื่องดับ" กับ "ไฟล์หาย" เป็นคนละเรื่องกัน
/// </para>
/// </summary>
public static class PathProbe
{
    private const int SmbPort = 445;

    private static readonly TimeSpan HostTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// อายุของผลเช็คเครื่อง — สั้น ๆ พอให้ path หลายเส้นที่อยู่บนเครื่องเดียวกัน
    /// ในรอบเดียวใช้ผลร่วมกัน ไม่ใช่ยิงหาเครื่องเดิมซ้ำเส้นละครั้ง
    /// </summary>
    private static readonly TimeSpan HostCacheFor = TimeSpan.FromSeconds(5);

    private static readonly ConcurrentDictionary<string, (bool Up, DateTime At)> Hosts =
        new(StringComparer.OrdinalIgnoreCase);

    public static Task<PathResult> FileAsync(string? path) => CheckAsync(path, folder: false);

    public static Task<PathResult> FolderAsync(string? path) => CheckAsync(path, folder: true);

    /// <summary>
    /// ชื่อเครื่องที่เก็บ path นี้ — null คือไฟล์ในเครื่องตัวเอง ไม่ต้องเช็คเครื่องก่อน
    /// </summary>
    public static string? HostOf(string? path)
    {
        var value = (path ?? "").Trim();
        if (value.Length == 0) return null;

        // รูปยาว \?\UNC\server\share — ตัดหัวให้เหลือรูปปกติก่อน
        const string LongUnc = @"\\?\UNC\";
        if (value.StartsWith(LongUnc, StringComparison.OrdinalIgnoreCase))
            value = @"\\" + value[LongUnc.Length..];

        if (value.StartsWith(@"\\", StringComparison.Ordinal))
        {
            var rest = value[2..];
            int cut = rest.IndexOfAny(['\\', '/']);
            var host = cut < 0 ? rest : rest[..cut];
            return host.Length == 0 ? null : host;
        }

        return MappedDriveHost(value);
    }

    /// <summary>เครื่องนี้รับการเชื่อมต่อแชร์ไฟล์อยู่ไหม — ผลถูกแคชไว้สั้น ๆ</summary>
    public static async Task<bool> HostUpAsync(string host)
    {
        if (Hosts.TryGetValue(host, out var hit) && DateTime.UtcNow - hit.At < HostCacheFor)
            return hit.Up;

        bool up = await ConnectAsync(host);
        Hosts[host] = (up, DateTime.UtcNow);
        return up;
    }

    private static async Task<bool> ConnectAsync(string host)
    {
        var tcp = new TcpClient();
        var connect = tcp.ConnectAsync(host, SmbPort);

        // ตอน timeout เราเดินต่อโดยทิ้ง task ไว้ ต้องมีคนรับ exception ของมัน
        // ไม่งั้นกลายเป็น unobserved exception ลอยอยู่ทุกรอบที่เครื่องปลายทางดับ
        _ = connect.ContinueWith(static t => _ = t.Exception,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

        try
        {
            await connect.WaitAsync(HostTimeout);
            return tcp.Connected;
        }
        catch
        {
            return false;
        }
        finally
        {
            tcp.Close();
        }
    }

    private static async Task<PathResult> CheckAsync(string? path, bool folder)
    {
        var value = (path ?? "").Trim();
        if (value.Length == 0) return new PathResult(PathState.NotSet, "ยังไม่ได้ตั้งค่า");

        if (HostOf(value) is string host && !await HostUpAsync(host))
            return new PathResult(PathState.HostDown, $"เครื่อง {host} ไม่ตอบ");

        var what = folder ? "โฟลเดอร์" : "ไฟล์";
        try
        {
            // ถึงเครื่องจะตอบแล้วก็ยังต้องอยู่บนเธรดพื้นหลังและมีเพดานเวลา
            // แชร์ที่กำลังจะหลุดตอบ TCP ได้แต่อ่านไดเรกทอรีไม่ได้ก็มี
            bool exists = await Task.Run(() => folder ? Directory.Exists(value) : File.Exists(value))
                .WaitAsync(ReadTimeout);

            return exists
                ? new PathResult(PathState.Ok, value)
                : new PathResult(PathState.Missing, $"ไม่พบ{what}นี้");
        }
        catch (TimeoutException)
        {
            return new PathResult(PathState.Slow, $"อ่านไม่ทันใน {ReadTimeout.TotalSeconds:0} วินาที");
        }
        catch (Exception ex)
        {
            return new PathResult(PathState.Missing, (ex.InnerException ?? ex).Message);
        }
    }

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(
        string localName, System.Text.StringBuilder remoteName, ref int length);

    /// <summary>
    /// ไดรฟ์ที่ map ไว้ (Z:\) ก็คือแชร์ของเครื่องอื่นเหมือนกัน ถามระบบว่าอยู่เครื่องไหน
    ///
    /// อ่าน DriveType อย่างเดียวไม่แตะเครือข่าย (มาจากตารางไดรฟ์ในเครื่อง) ต่างจาก
    /// DriveInfo.IsReady ที่ค้างได้ถ้าปลายทางดับ จึงเรียกตรงนี้ได้โดยไม่ต้องกลัว
    /// </summary>
    private static string? MappedDriveHost(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (root == null || root.Length < 2 || root[1] != ':') return null;
            if (new DriveInfo(root).DriveType != DriveType.Network) return null;

            var remote = new System.Text.StringBuilder(512);
            int length = remote.Capacity;
            if (WNetGetConnection(root[..2], remote, ref length) != 0) return null;

            return HostOf(remote.ToString());
        }
        catch
        {
            // ถามไม่ได้ก็ถือว่าไม่รู้ว่าอยู่เครื่องไหน ตกไปใช้ทางที่มี timeout แทน
            return null;
        }
    }
}
