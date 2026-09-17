using System.Net.Sockets;

namespace InkjetOperator.Services;

/// <summary>ผลของการเช็คหนึ่งรายการ</summary>
public enum HealthState
{
    /// <summary>ใช้งานได้</summary>
    Ok,

    /// <summary>ตั้งค่าไว้แล้วแต่ใช้ไม่ได้ — อันนี้เท่านั้นที่ถือว่ามีปัญหา</summary>
    Bad,

    /// <summary>ยังไม่ได้ตั้งค่า ไม่นับว่าพัง เครื่องนี้อาจไม่ได้ใช้ของชิ้นนี้</summary>
    NotConfigured,
}

/// <param name="Group">หัวข้อที่จัดกลุ่มบนจอ</param>
/// <param name="Name">ชื่อที่พนักงานเรียก</param>
/// <param name="Detail">ที่อยู่หรือเหตุผล — ว่างได้</param>
public sealed record HealthItem(string Group, string Name, HealthState State, string Detail);

/// <summary>
/// เฝ้าดูว่าไฟล์ โฟลเดอร์ และเครื่องปลายทางทั้งหมดยังใช้งานได้อยู่ไหม
///
/// <para>
/// <b>ห้ามบล็อกอะไรทั้งนั้น</b> — ใช้นาฬิกาของเธรดพูล ไม่ใช่ของ WinForms จึงไม่มี
/// จังหวะไหนที่แตะเธรดของหน้าจอเลย ผู้ฟังเป็นคนพาผลกลับไปเธรดตัวเองเอง
/// </para>
/// <para>
/// <b>ห้ามเด้งกล่อง</b> — ที่นี่ไม่รู้จักหน้าจอ รู้แค่ว่าผลเป็นอะไร ใครอยากแสดง
/// อย่างไรก็ไปตัดสินใจเอง เป็นกฎเดียวกับที่ชั้น Services ทั้งหมดยึดอยู่แล้ว
/// </para>
/// <para>
/// <b>ห้ามฟ้องของที่ยังไม่ได้ตั้งค่า</b> — แต่ละสถานีใช้ของไม่ครบทุกชิ้น เครื่องที่
/// ไม่ได้ต่อ PLC ก็ไม่ควรขึ้นแดงว่า PLC พัง ไม่งั้นจอจะแดงตลอดจนคนเลิกมอง
/// </para>
/// </summary>
public static class HealthMonitor
{
    /// <summary>ถี่แค่ไหน — เห็นปัญหาไว แต่ไม่กวนเครือข่ายปลายทางบ่อยเกิน</summary>
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// รอปลายทางนานสุดต่อหนึ่งราย
    ///
    /// สั้นไว้โดยตั้งใจ เพราะทุกรายวิ่งพร้อมกัน รอบหนึ่งจึงจบใน 2 วินาทีเสมอ
    /// ไม่ว่าจะมีปลายทางที่ต่อไม่ติดกี่ตัวก็ตาม
    /// </summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    private static readonly object Gate = new();
    private static System.Threading.Timer? _timer;
    private static int _running;

    /// <summary>ผลรอบล่าสุด — ว่างแปลว่ายังไม่เคยเช็คเสร็จสักรอบ</summary>
    public static IReadOnlyList<HealthItem> Latest { get; private set; } = [];

    /// <summary>
    /// มีผลรอบใหม่แล้ว — ยิงจากเธรดพูล ผู้ฟังที่เป็นหน้าจอต้อง Invoke กลับเอง
    /// ยิงทุกรอบแม้ผลไม่เปลี่ยน เพื่อให้หน้าที่เพิ่งเปิดได้ค่าล่าสุดโดยไม่ต้องรอ
    /// </summary>
    public static event EventHandler<IReadOnlyList<HealthItem>>? Updated;

    /// <summary>เริ่มเฝ้า — เรียกซ้ำไม่มีผล เช็ครอบแรกทันทีโดยไม่ต้องรอครบรอบ</summary>
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

    /// <summary>เช็คเดี๋ยวนี้เลยโดยไม่รอรอบถัดไป — ใช้ตอนผู้ใช้เพิ่งกด Save ค่าใหม่</summary>
    public static void CheckNow() => _ = TickAsync();

    private static async Task TickAsync()
    {
        // รอบก่อนยังไม่จบก็ข้ามรอบนี้ ไม่ให้คำขอค้างซ้อนกันตอนปลายทางอืด
        if (Interlocked.Exchange(ref _running, 1) == 1) return;

        // กำลังส่งงานเข้าเครื่องอยู่ก็ข้ามไปเหมือนกัน เหตุผลอยู่ที่ MachineBusy
        // ผลรอบก่อนยังค้างอยู่ใน Latest หน้าสถานะจึงไม่กะพริบเป็นช่องว่าง
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
            // ตัวเฝ้าล้มเงียบ ๆ ดีกว่าไปล้มโปรแกรมทั้งตัว รอบหน้าค่อยลองใหม่
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

        // ยิงทุกปลายทางพร้อมกัน รอบหนึ่งจึงใช้เวลาเท่ากับรายที่ช้าที่สุดรายเดียว
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

        // ไฟล์กับโฟลเดอร์ยิงพร้อมกันเหมือนปลายทางบนเครือข่าย
        //
        // เดิมทั้งเจ็ดรายการอยู่ใน Task.Run ก้อนเดียว เรียงกันทีละอัน และใช้เวลารอ
        // ร่วมกันก้อนเดียว พอ path เกือบทั้งหมดชี้ไปเครื่องอื่น เครื่องปลายทางดับ
        // แค่เครื่องเดียว รายการแรกที่ค้างก็กินเวลาที่มีไปคนเดียว อีกหกรายการ
        // ไม่ได้ถูกเช็คด้วยซ้ำแต่ขึ้น "ตรวจไม่สำเร็จ" ตามไปทั้งแถบ
        //
        // PathProbe เช็คตัวเครื่องก่อนแตะไฟล์ เครื่องดับจึงรู้ใน 1 วินาทีและไม่ไป
        // แตะไฟล์เลย รายละเอียดอยู่ในคลาสนั้น
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

    // ── ปลายทางบนเครือข่าย ─────────────────────────────────

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

    // ── ไฟล์และโฟลเดอร์ ────────────────────────────────────

    /// <summary>
    /// แปลงผลของ <see cref="PathProbe"/> เป็นแถวในตาราง
    ///
    /// "เครื่องไม่ตอบ" กับ "ไม่มีไฟล์" ขึ้นเป็นข้อความคนละอันโดยตั้งใจ — สองอย่างนี้
    /// แก้กันคนละวิธี คนอ่านต้องแยกออกตั้งแต่บรรทัดแรกโดยไม่ต้องเดา
    /// </summary>
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

    /// <summary>โฟลเดอร์ backend ต้องมี index.js อยู่จริง ไม่ใช่แค่มีโฟลเดอร์</summary>
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

    // ── ตัวช่วย ────────────────────────────────────────────

    /// <summary>ข้อความของ .NET ยาวเกินกว่าจะใส่ในตาราง เอาแค่ประโยคแรก</summary>
    private static string Short(Exception ex)
    {
        var text = (ex.InnerException ?? ex).Message.Trim();
        int stop = text.IndexOf('\n');
        if (stop > 0) text = text[..stop].Trim();
        return text.Length > 90 ? text[..90] + "…" : text;
    }
}
