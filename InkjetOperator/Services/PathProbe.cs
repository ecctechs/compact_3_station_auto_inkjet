using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace InkjetOperator.Services;

public enum PathState
{
    NotSet,

    Ok,

    Missing,

    HostDown,

    Slow,
}

public readonly record struct PathResult(PathState State, string Detail);

public static class PathProbe
{
    private const int SmbPort = 445;

    private static readonly TimeSpan HostTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(2);

    private static readonly TimeSpan HostCacheFor = TimeSpan.FromSeconds(5);

    private static readonly ConcurrentDictionary<string, (bool Up, DateTime At)> Hosts =
        new(StringComparer.OrdinalIgnoreCase);

    public static Task<PathResult> FileAsync(string? path) => CheckAsync(path, folder: false);

    public static Task<PathResult> FolderAsync(string? path) => CheckAsync(path, folder: true);

    public static string? HostOf(string? path)
    {
        var value = (path ?? "").Trim();
        if (value.Length == 0) return null;

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
            return null;
        }
    }
}
