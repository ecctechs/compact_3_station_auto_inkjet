using System.Net.Sockets;

namespace InkjetOperator.Services;

public enum HealthState
{
    Ok,

    Bad,

    NotConfigured,
}

public sealed record HealthItem(string Group, string Name, HealthState State, string Detail);

public static class HealthMonitor
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    private static readonly object Gate = new();
    private static System.Threading.Timer? _timer;
    private static int _running;

    public static IReadOnlyList<HealthItem> Latest { get; private set; } = [];

    public static event EventHandler<IReadOnlyList<HealthItem>>? Updated;

    public static void Start()
    {
        lock (Gate)
        {
            if (_timer != null) return;
            _timer = new System.Threading.Timer(_ => _ = TickAsync(), null, TimeSpan.Zero, Interval);
        }
    }

    public static void Stop()
    {
        lock (Gate)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    public static void CheckNow() => _ = TickAsync();

    private static async Task TickAsync()
    {
        if (Interlocked.Exchange(ref _running, 1) == 1) return;

        if (MachineBusy.Active)
        {
            Interlocked.Exchange(ref _running, 0);
            return;
        }

        try
        {
            var items = await CheckAllAsync();
            Latest = items;
            Updated?.Invoke(null, items);
        }
        catch
        {
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    private static async Task<IReadOnlyList<HealthItem>> CheckAllAsync()
    {
        const string files = "ไฟล์และโฟลเดอร์";
        const string links = "การเชื่อมต่อ";

        var uv1Name = UvSettingsManager.Read("UV1_NAME", "UV-001");
        var uv2Name = UvSettingsManager.Read("UV2_NAME", "UV-002");

        var network = await Task.WhenAll(
            EndpointAsync(links, "Backend", CustomSettingsManager.Read("PC_IP", "127.0.0.1"), "3000"),
            EndpointAsync(links, CustomSettingsManager.Read("MK058_NAME", "MK-058"),
                CustomSettingsManager.Read("MK058_COM"), "9004"),
            EndpointAsync(links, CustomSettingsManager.Read("MK059_NAME", "MK-059"),
                CustomSettingsManager.Read("MK059_COM"), "9004"),
            EndpointAsync(links, uv1Name,
                CustomSettingsManager.Read("UV001_IP"), CustomSettingsManager.Read("UV001_PORT")),
            EndpointAsync(links, uv2Name,
                CustomSettingsManager.Read("UV002_IP"), CustomSettingsManager.Read("UV002_PORT")),
            EndpointAsync(links, CustomSettingsManager.Read("PLC_NAME", "PLC-001"),
                CustomSettingsManager.Read("PLC_IP"), CustomSettingsManager.Read("PLC_PORT", "502")),
            EndpointAsync(links, CustomSettingsManager.Read("CLAMP_PLC_NAME", "PLC แคลมป์"),
                CustomSettingsManager.Read("CLAMP_PLC_IP"), CustomSettingsManager.Read("CLAMP_PLC_PORT")));

        var onDisk = await Task.WhenAll(
            PathItemAsync(files, "PrintData.db3", CustomSettingsManager.Read("DB_PATH"), folder: false),
            PathItemAsync(files, "mydatabase.db3 (แคลมป์)", CustomSettingsManager.Read("CLAMP_DB_PATH"), folder: false),
            PathItemAsync(files, "โฟลเดอร์รูปอ้างอิง", CustomSettingsManager.Read("MARKING_REF_FOLDER"), folder: true),
            PathItemAsync(files, $"โฟลเดอร์โปรแกรม {uv1Name}", UvSettingsManager.GetDocumentFolder(1), folder: true),
            PathItemAsync(files, $"โฟลเดอร์โปรแกรม {uv2Name}", UvSettingsManager.GetDocumentFolder(2), folder: true),
            BackendFolderItemAsync(files),
            Task.FromResult(SettingsItem(files)));

        var all = new List<HealthItem>();
        all.AddRange(onDisk);
        all.AddRange(network);
        return all;
    }

    private static async Task<HealthItem> EndpointAsync(
        string group, string name, string? ip, string? port)
    {
        var host = (ip ?? "").Trim();
        if (host.Length == 0 || !int.TryParse((port ?? "").Trim(), out int tcpPort) || tcpPort <= 0)
            return new HealthItem(group, name, HealthState.NotConfigured, "ยังไม่ได้ตั้งค่า");

        var where = $"{host}:{tcpPort}";
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, tcpPort).WaitAsync(Timeout);
            return new HealthItem(group, name, HealthState.Ok, where);
        }
        catch (TimeoutException)
        {
            return new HealthItem(group, name, HealthState.Bad, $"{where} — ไม่ตอบใน {Timeout.TotalSeconds:0} วินาที");
        }
        catch (Exception ex)
        {
            return new HealthItem(group, name, HealthState.Bad, $"{where} — {Short(ex)}");
        }
    }

    private static async Task<HealthItem> PathItemAsync(
        string group, string name, string? path, bool folder)
    {
        var result = folder
            ? await PathProbe.FolderAsync(path)
            : await PathProbe.FileAsync(path);

        var state = result.State switch
        {
            PathState.Ok => HealthState.Ok,
            PathState.NotSet => HealthState.NotConfigured,
            _ => HealthState.Bad,
        };

        var detail = result.State == PathState.Missing
            ? $"{result.Detail}: {(path ?? "").Trim()}"
            : result.Detail;

        return new HealthItem(group, name, state, detail);
    }

    private static async Task<HealthItem> BackendFolderItemAsync(string group)
    {
        const string name = "โฟลเดอร์ backend";
        var folder = CustomSettingsManager.Read("BACKEND_PATH", "").Trim();
        if (folder.Length == 0)
            return new HealthItem(group, name, HealthState.NotConfigured, "ยังไม่ได้ตั้งค่า");

        var result = await PathProbe.FileAsync(Path.Combine(folder, "index.js"));
        return result.State switch
        {
            PathState.Ok => new HealthItem(group, name, HealthState.Ok, folder),
            PathState.Missing => new HealthItem(group, name, HealthState.Bad, $"ไม่พบ index.js ใน {folder}"),
            _ => new HealthItem(group, name, HealthState.Bad, result.Detail),
        };
    }

    private static HealthItem SettingsItem(string group)
    {
        var problem = AppSettingsFile.CheckWritable();
        return problem == null
            ? new HealthItem(group, "บันทึกการตั้งค่า", HealthState.Ok, AppSettingsFile.Folder)
            : new HealthItem(group, "บันทึกการตั้งค่า", HealthState.Bad, problem);
    }

    private static string Short(Exception ex)
    {
        var text = (ex.InnerException ?? ex).Message.Trim();
        int stop = text.IndexOf('\n');
        if (stop > 0) text = text[..stop].Trim();
        return text.Length > 90 ? text[..90] + "…" : text;
    }
}
