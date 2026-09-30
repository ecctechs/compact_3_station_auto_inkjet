using System.Windows.Forms;

using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

public enum SendStatus
{
    Ok,

    Failed,

    Cancelled,

    NotConfigured,

    Unreachable,
}

public sealed record MkMachineResult(
    string Name, string? Error, bool Suspended = false, string? Note = null)
{
    public bool Ok => Error == null;
}

public sealed record MkSendResult(SendStatus Status, List<MkMachineResult> Machines);

public sealed record UvSendResult(
    SendStatus Status,
    string MachineName,
    List<string> Done,
    string? FailReason = null,
    string? ProgramFile = null,
    bool UsedDefault = false,
    string Ip = "",
    int Port = 0,
    string? StartWarning = null);

public static class JobSendService
{
    private const int MkPort = 9004;

    private const int MkBlockCount = 5;

    private const int MkFirstDeviceBlock = 6;

    private const int MkBlockGapMs = 30;
    private const int UvDefaultPort = 10086;
    private const int ConnectTimeoutSeconds = 3;

    public static async Task<MkSendResult> SendMkAsync(PatternDetail pattern) // ส่ง Pattern ไปหัว MK ที่งานใช้
    {
        using var busy = MachineBusy.Hold("MK"); // พักการเช็กหัว MK แต่ไม่ขวางปุ่ม UV

        var heads = MkMachines // รวม IP และ Pattern ของหัว MK ทั้งสองตัว
            .Select(m => new MkHead( // สร้างชุดปลายทางของ MK แต่ละหัว
                CustomSettingsManager.Read(m.NameKey, m.Fallback), // อ่านชื่อหัวหรือใช้ชื่อสำรอง
                CustomSettingsManager.Read(m.IpKey), // อ่าน IP ของหัวนี้
                pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == m.Ordinal), // จับ Pattern ให้ตรงลำดับหัว ไม่สลับข้อมูลกัน
                m.Label)) // แนบป้ายหัวไว้ระบุผลส่ง
            .ToList(); // เก็บผลที่กรองแล้วเป็นรายการ

        var outcomes = SameDevice(heads) // ถ้า IP ซ้ำให้ส่งทีละหัว ป้องกันคำสั่งตีกัน
            ? [await SendHeadAsync(heads[0]), await SendHeadAsync(heads[1])] // ใช้ปลายทางเดียวกัน จึงรอหัวแรกก่อน
            : await Task.WhenAll(heads.Select(SendHeadAsync)); // คนละ IP ส่งสองหัวพร้อมกันได้

        var machines = outcomes.Where(o => o.Result != null).Select(o => o.Result!).ToList(); // รวมผลตามลำดับหัว ไม่ตามลำดับที่เสร็จ
        bool anySent = outcomes.Any(o => o.Sent); // ตรวจว่ามีหัวที่รับข้อมูลจริงในรอบนี้หรือไม่
        bool workFailed = outcomes.Any(o => o.Failed); // ดูความล้มเหลวเฉพาะหัวที่ต้องพิมพ์งาน

        if (machines.Count == 0) // ไม่มีหัว MK ที่ได้ติดต่อเลย
            return new MkSendResult(SendStatus.NotConfigured, machines); // รายงานว่ายังไม่มีหัวที่ส่งงานได้

        if (!anySent) // ตรวจว่ามีหัวใดรับข้อมูลงานแล้วบ้าง
            return new MkSendResult(SendStatus.NotConfigured, machines); // รายงานว่ายังไม่มีหัวที่ส่งงานได้

        return new MkSendResult( // รวมผลการส่งของหัวที่ใช้
            workFailed ? SendStatus.Failed : SendStatus.Ok, // หัวที่ต้องใช้ล้มเหลวแม้เพียงหัวเดียว ให้ผลรวมไม่สำเร็จ
            machines); // แนบผลแยกรายหัวกลับผู้เรียก
    }

    public sealed record UnreachableMachine(string Name, string Reason);

    public static async Task<List<UnreachableMachine>> UnreachableAsync( // ตรวจการเชื่อมต่อเครื่องที่อยู่ในแผน
        IEnumerable<string> steps, PatternDetail? pattern, List<UvJobDataDto>? uvData) // รับแผนเครื่อง Pattern และชุดข้อมูล UV
    {
        var bad = new List<UnreachableMachine>(); // เตรียมรายชื่อปลายทางที่ยังไม่พร้อม
        var targets = new List<(string Name, string Host, int Port)>(); // รวมปลายทางที่จะตรวจให้ครบก่อนจองคิว

        foreach (var step in steps.Distinct(StringComparer.OrdinalIgnoreCase)) // ตรวจเครื่องซ้ำเพียงครั้งเดียวในแผน
        {
            if (string.Equals(step, "MK", StringComparison.OrdinalIgnoreCase)) // แยกตรวจค่าตั้งสำหรับ MK
            {
                foreach (var (ipKey, nameKey, fallbackName, ordinal, _) in MkMachines) // ตรวจ IP และ Pattern ของ MK ทีละหัว
                {
                    var config = pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal); // จับค่าพิมพ์ตามลำดับหัว
                    if (!HasProgram(config)) continue; // หัวที่ไม่ได้ใช้ไม่ต้องผ่านด่านตรวจการเชื่อมต่อ

                    var name = CustomSettingsManager.Read(nameKey, fallbackName); // อ่านชื่อหัวพิมพ์ไว้รายงานผล
                    var ip = CustomSettingsManager.Read(ipKey); // อ่าน IP ของหัวที่จะติดต่อ

                    if (string.IsNullOrWhiteSpace(ip)) // ตรวจว่าตั้ง IP ของหัวที่จะส่งแล้วไหม
                        bad.Add(new UnreachableMachine(name, "ยังไม่ได้ตั้ง IP")); // เพิ่มหัวที่ยังไม่มี IP ในรายการปัญหา
                    else targets.Add((name, ip, MkPort)); // เพิ่มหัว MK ที่มีงานเข้าในรายการตรวจ TCP
                }

                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            int uvNumber = string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1 : 2; // แยกว่าเป็น UV1 หรือ UV2
            var uvName = UvSettingsManager.Read( // อ่านชื่อเครื่อง UV ตามค่าตั้ง
                uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}"); // เลือกชุดชื่อให้ตรงกับ UV1 หรือ UV2

            if (uvData?.Any(r => string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase)) != true) // ตรวจว่ามีข้อมูลของ UV เครื่องนี้ใน Job
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่มีข้อมูล {step} ของงานนี้")); // จดว่า Job ยังไม่มีชุดข้อมูล UV นี้
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            if (UvSettingsManager.GetCpiPath(uvNumber) == null) // ตรวจที่อยู่ CPI ของ UV ที่ต้องใช้
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่ได้ตั้งโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3")); // จดว่าโฟลเดอร์ UV หรือ CPI ยังไม่พร้อม
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            var uvIp = CustomSettingsManager.Read($"UV00{uvNumber}_IP"); // อ่าน IP ของ UV ที่เลือก
            if (string.IsNullOrWhiteSpace(uvIp)) // ตรวจว่ามี IP ของ UV แล้วไหม
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่ได้ตั้ง IP ของ UV{uvNumber}")); // จดว่า UV ยังไม่ได้ตั้ง IP
                continue; // ข้ามรายการนี้ไปตัวถัดไป
            }

            int uvPort = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p) // อ่านพอร์ต UV แล้วลองแปลงเป็นตัวเลข
                ? p // ใช้พอร์ตที่แปลงได้จาก Setting
                : UvDefaultPort; // ถ้าแปลงไม่ได้ให้ใช้พอร์ต UV เริ่มต้น

            targets.Add((uvName, uvIp, uvPort)); // เพิ่ม UV ที่แผนต้องใช้เข้าในรายการตรวจ
        }

        var connected = await ConnectionPreflight.CheckAsync( // ตรวจหลายปลายทางพร้อมกันเพื่อลดเวลารอ
            targets.Select(t => (t.Host, t.Port)).ToList(), CanConnectAsync); // ปลายทางซ้ำตรวจครั้งเดียวในชุดนี้
        for (int i = 0; i < targets.Count; i++) // ตรวจผลเชื่อมต่อให้ครบทุกปลายทาง
            if (!connected[i]) // ปลายทางนี้เชื่อมต่อไม่ผ่าน
                bad.Add(new UnreachableMachine(targets[i].Name, $"ต่อไม่ติด ({targets[i].Host}:{targets[i].Port})")); // เก็บชื่อ IP และพอร์ตที่ติดต่อไม่ได้
        return bad; // ส่งรายการที่ต้องแก้ก่อนเริ่มงาน
    }

    private static bool HasProgram(InkjetConfigDto? config) =>
        config != null
        && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

    private static async Task<string?> StopOneMkAsync(string ip, string label)
    {
        var tcp = new TcpManager();
        try
        {
            await tcp.ConnectAsync(ip, MkPort)
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds));

            var sr = await new MkCompactAdapter(tcp).SuspendAsync();
            return sr.Success ? null : $"{label}: สั่งหยุดพิมพ์ไม่สำเร็จ";
        }
        catch (Exception ex)
        {
            return $"{label}: {ex.Message}";
        }
        finally
        {
            tcp.Disconnect();
        }
    }

    private static readonly (string IpKey, string NameKey, string Fallback, int Ordinal, string Label)[]
        MkMachines =
        [
            ("MK058_COM", "MK058_NAME", "MK-058", 1, "MK1"),
            ("MK059_COM", "MK059_NAME", "MK-059", 2, "MK2"),
        ];

    private sealed record MkHead(string Name, string Ip, InkjetConfigDto? Config, string Label);

    private readonly record struct MkHeadOutcome(MkMachineResult? Result, bool Sent, bool Failed);

    private static async Task<MkHeadOutcome> SendHeadAsync(MkHead head) // ส่งงานหรือสั่งพัก MK หนึ่งหัว
    {
        if (!HasProgram(head.Config)) // ตรวจว่าหัวนี้ไม่มีโปรแกรมให้พิมพ์
        {
            if (string.IsNullOrWhiteSpace(head.Ip)) return new(null, false, false); // หัวไม่ใช้และไม่มี IP ให้ข้ามไป

            return new(new MkMachineResult( // เตรียมผลการพักหัวที่ไม่ได้ใช้
                head.Name, await StopOneMkAsync(head.Ip, head.Label), Suspended: true), false, false); // สั่งหยุดหัวนี้และระบุว่าเป็นหัวพัก
        }

        if (string.IsNullOrWhiteSpace(head.Ip)) // ตรวจว่าตั้ง IP ของหัวที่จะส่งแล้วไหม
        {
            return new(new MkMachineResult( // เตรียมผลว่าหัวที่ต้องใช้ส่งไม่ได้
                head.Name, $"{head.Label}: ยังไม่ได้ตั้ง IP — ไปตั้งที่ Setting → Inkjet Setting"), // บอกให้ตั้ง IP ของหัวนี้ก่อน
                false, true); // ระบุว่ายังไม่ส่งและหัวที่ใช้มีปัญหา
        }

        var (error, note) = await SendToOneMkAsync(head.Ip, head.Config!, head.Label); // ส่งโปรแกรมและข้อความไปหัวที่ใช้ในงาน
        return new(new MkMachineResult(head.Name, error, Note: note), error == null, error != null); // แนบผลส่งและคำเตือนแยกของหัวนี้
    }

    private static bool SameDevice(List<MkHead> heads) =>
        heads.Count == 2
        && !string.IsNullOrWhiteSpace(heads[0].Ip)
        && string.Equals(heads[0].Ip.Trim(), heads[1].Ip.Trim(), StringComparison.OrdinalIgnoreCase);

    private static readonly (string Name, int Min, int Max)[] MachineRanges =
    [
        ("Width", 10, 500),
        ("Height", 50, 200),
        ("Trigger Delay", 1, 9999),
    ];

    private static readonly (string Name, int Min, int Max)[] BlockRanges =
    [
        ("X", 0, 4095),
        ("Y", 0, 31),
        ("Size", 0, 22),
        ("Scale", 1, 10),
    ];

    private static string? InvalidConfig(InkjetConfigDto config, string label) // ตรวจค่าที่ MK รับได้ก่อนแตะเครื่อง
    {
        foreach (var (name, _, _) in MachineRanges) // ตรวจค่าจำเป็นของ MK ก่อนเริ่ม TCP
        {
            bool filled = name switch // ตรวจช่องจำเป็นตามชื่อค่า
            {
                "Width" => config.Width.HasValue, // ต้องกรอกความกว้าง ไม่ใส่ค่าแทนให้เอง
                "Height" => config.Height.HasValue, // ต้องกรอกความสูงก่อนส่งหัวนี้
                _ => config.TriggerDelay.HasValue, // ต้องมี Trigger Delay ของงาน
            };

            if (!filled) // มีค่าจำเป็นที่ยังไม่ได้กรอก
                return $"{label}: ยังไม่ได้กรอก {name} — กรอกที่หน้า Order Detail ก่อนส่ง"; // บอกชื่อช่องที่ต้องกรอกใน Detail
        }

        foreach (var (name, min, max) in MachineRanges) // ตรวจช่วงต่ำสุดสูงสุดของค่าระดับเครื่อง
        {
            int? value = name switch // เลือกค่ามาตรวจตามชื่อฟิลด์
            {
                "Width" => config.Width, // ตรวจความกว้างที่ตั้งไว้
                "Height" => config.Height, // ตรวจความสูงที่ตั้งไว้
                _ => config.TriggerDelay, // ตรวจ Trigger Delay ของหัวนี้
            };

            if (value is int v && (v < min || v > max)) // ตรวจค่าที่อยู่นอกช่วงของเครื่อง
                return $"{label}: {name} = {v} อยู่นอกช่วง {min}-{max} ที่เครื่องรับได้"; // บอกชื่อค่าและช่วงที่เครื่องรับได้
        }

        foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber)) // ตรวจบล็อกข้อความตามลำดับช่อง
        {
            foreach (var (name, min, max) in BlockRanges) // ตรวจช่วงที่รองรับของแต่ละค่าบล็อก
            {
                int? value = name switch // เลือกค่ามาตรวจตามชื่อฟิลด์
                {
                    "X" => block.X, // ตรวจตำแหน่งแนวนอนของบล็อก
                    "Y" => block.Y, // ตรวจตำแหน่งแนวตั้งของบล็อก
                    "Size" => block.Size, // ตรวจขนาดตัวอักษรของบล็อก
                    _ => block.Scale, // ตรวจอัตราขยายของบล็อก
                };

                if (value is int v && (v < min || v > max)) // ตรวจค่าบล็อกที่เกินขอบเขต
                {
                    return $"{label}: Block {block.BlockNumber} มี {name} = {v} " // ระบุบล็อกและค่าที่ต้องแก้
                         + $"อยู่นอกช่วง {min}-{max} — แก้ที่หน้า Order Detail"; // บอกช่วงที่ใช้ได้และหน้าแก้ข้อมูล
                }
            }
        }

        return null; // จบโดยไม่มีข้อมูลให้ใช้ต่อ
    }

    private static string? Note(List<string> notes) =>
        notes.Count == 0 ? null : string.Join(" · ", notes);

    private static string Reject(string label, string step, CommandResult result)
    {
        var reply = (result.Response ?? "").Trim();
        return reply.Length == 0
            ? $"{label}: {step} — เครื่องไม่ตอบ"
            : $"{label}: {step} — เครื่องปฏิเสธ ({reply})";
    }

    private static async Task<(string? Error, string? Note)> SendToOneMkAsync( // ส่งโปรแกรม ข้อความ และค่าพิมพ์ให้ MK หนึ่งหัว
        string ip, InkjetConfigDto config, string label) // รับ IP ค่าพิมพ์ และชื่อหัวสำหรับรายงาน
    {
        if (InvalidConfig(config, label) is string bad) return (bad, null); // ตรวจค่าพิมพ์ก่อนเชื่อมต่อ ป้องกันส่งไปได้เพียงบางส่วน

        var tcp = new TcpManager(); // สร้างตัวเชื่อม TCP สำหรับหัวนี้
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            await tcp.ConnectAsync(ip, MkPort) // เปิดการเชื่อมต่อไป MK
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds)); // จำกัดเวลารอเชื่อมต่อ
            var adapter = new MkCompactAdapter(tcp); // ใช้ชุดคำสั่งของเครื่อง MK ผ่าน TCP ที่เปิดไว้

            var notes = new List<string>(); // เก็บคำเตือนประกอบผลส่งของหัวนี้

            await adapter.ResumeAsync(); // ส่ง SQ ก่อนเปลี่ยนโปรแกรม โดยโค้ดนี้ไม่ใช้ผลตอบ SQ ตัดสิน

            var fw = await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1); // ส่ง FW เลือกโปรแกรม ถ้าไม่มีเลขให้ใช้ 1
            if (!fw.Success) // ตรวจผลเปลี่ยนโปรแกรมก่อนส่งข้อความ
                return (Reject(label, $"เปลี่ยนไปโปรแกรม {config.ProgramNumber}", fw), Note(notes)); // จบที่คำสั่ง FW พร้อมเหตุที่เครื่องปฏิเสธ

            for (int slot = 1; slot <= MkBlockCount; slot++) // ส่งครบทุกช่องข้อความ รวมช่องที่งานนี้ไม่ได้ใช้
            {
                var block = config.TextBlocks.FirstOrDefault(b => b.BlockNumber == slot) // หาข้อความของช่องปัจจุบันจาก Pattern
                    ?? new TextBlockDto { BlockNumber = slot, Text = "" }; // ช่องที่ไม่มีข้อความให้ส่งค่าว่างไปทับ

                await Task.Delay(MkBlockGapMs); // เว้นจังหวะก่อนส่งช่องถัดไป

                int deviceBlock = slot + MkFirstDeviceBlock - 1; // แปลงเลขช่องในงานเป็นเลขช่องที่เครื่องใช้
                var fb = await adapter.SendTextBlockAsync(block, deviceBlock); // ส่งข้อความและค่าบล็อกด้วย FS / F1
                if (!fb.Success) return (Reject(label, $"ส่ง Block {slot}", fb), Note(notes)); // เครื่องไม่รับบล็อกนี้ ให้หยุดก่อนส่งช่องอื่น
            }

            var fm = await adapter.SendConfigAsync(config); // ส่ง FM หลังครบทุกบล็อก เพื่อไม่ให้ค่าถูกบล็อกเขียนทับ
            if (!fm.Success) return (Reject(label, "ส่ง Config", fm), Note(notes)); // ส่งค่าพิมพ์ไม่ผ่าน ให้รายงานจุดที่หยุด

            return (null, Note(notes)); // ส่งผลว่าครบทุกคำสั่ง พร้อมคำเตือนที่เก็บไว้
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return ($"{label}: {ex.Message}", null); // ระบุหัวและเหตุที่ส่งไม่จบ
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            tcp.Disconnect(); // ปิด TCP หลังจบรอบส่งหัวนี้
        }
    }

    public static async Task<UvSendResult> SendUvAsync( // เขียน CPI แล้วสั่ง UV โหลดโปรแกรม
        IWin32Window? owner, int uvNumber, List<UvJobDataDto> uvData, // รับหน้าต่างเจ้าของงาน เครื่อง UV และข้อมูล
        string? forcedProgram = null, bool allowPrompt = true) // รับชื่อโปรแกรมที่เลือกไว้และสิทธิ์เปิดกล่องเลือก
    {
        string stepName = uvNumber == 1 ? "UV1" : "UV2"; // ระบุว่าจะใช้ข้อมูลของ UV1 หรือ UV2

        using var busy = MachineBusy.Hold(stepName); // กันงานอื่นใช้ UV นี้ซ้อนระหว่างส่ง

        string table = uvNumber == 1 ? "MK063" : "MK067"; // UV1 เขียน MK063 ส่วน UV2 เขียน MK067 ใน CPI.db3

        var uvName = uvNumber == 1 // อ่านชื่อเครื่อง UV สำหรับแสดงผล
            ? UvSettingsManager.Read("UV1_NAME", "UV-001") // ใช้ชื่อที่ตั้งไว้สำหรับ UV1
            : UvSettingsManager.Read("UV2_NAME", "UV-002"); // ใช้ชื่อที่ตั้งไว้สำหรับ UV2

        var done = new List<string>(); // เก็บรายการขั้นที่ทำไปแล้วไว้รายงาน

        var uvRow = uvData.FirstOrDefault(r => r.Machine == stepName); // หาแถวข้อมูลของ UV ที่ต้องส่ง
        if (uvRow == null) // Job ยังไม่มีข้อมูลของเครื่องนี้
            return Blocked(uvName, $"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก"); // หยุดเพราะ Job ไม่มีข้อมูล UV เครื่องนี้

        var cpiPath = UvSettingsManager.GetCpiPath(uvNumber); // หาไฟล์ CPI.db3 ตามชุดตั้งค่าของ UV
        if (cpiPath == null) // หาไฟล์ CPI ไม่พบหรือยังไม่ตั้งโฟลเดอร์
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3"); // หยุดเพราะยังไม่มีไฟล์ CPI พร้อมเขียน

        var ip = CustomSettingsManager.Read($"UV00{uvNumber}_IP"); // อ่าน IP ของเครื่อง UV ที่เลือก
        if (string.IsNullOrWhiteSpace(ip)) // ตรวจ IP ของ UV ก่อนเปิด TCP
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่า IP ของ UV{uvNumber}"); // ระบุว่าต้องตั้ง IP ของ UV ก่อน

        int port = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p) // อ่านพอร์ต UV จากค่าตั้ง
            ? p // ใช้พอร์ตที่อ่านได้จาก Setting
            : UvDefaultPort; // ใช้พอร์ตเริ่มต้นเมื่อค่าที่ตั้งแปลงไม่ได้

        if (!await CanConnectAsync(ip, port)) // ลองเชื่อมต่อปลายทางก่อนแก้ข้อมูล CPI
            return new UvSendResult(SendStatus.Unreachable, uvName, done, Ip: ip, Port: port); // รายงานปลายทาง UV ที่ต่อไม่ติด

        UvProgramPick pick; // เก็บผลเลือกโปรแกรมและการใช้โปรแกรมสำรอง
        if (string.IsNullOrWhiteSpace(forcedProgram)) // ยังไม่มีชื่อโปรแกรมที่เลือกไว้จากผู้เริ่มงานหรือคิว
        {
            if (!allowPrompt) return Blocked(uvName, "ยังไม่ได้เลือกโปรแกรม UV ก่อนส่ง"); // ไม่อนุญาตให้เปิดกล่องเลือกในจังหวะส่งนี้
            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber); // หาโฟลเดอร์เก็บไฟล์โปรแกรมของ UV นี้
            pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, owner); // ค้นโปรแกรมตามชื่อในงาน หรือเปิดให้เลือกรุ่นย่อย
        }
        else // กรณีไม่เข้าเงื่อนไขก่อนหน้า
        {
            pick = new UvProgramPick(forcedProgram.Trim(), false); // ใช้โปรแกรมที่ผู้ขอส่งเลือกไว้แล้ว ไม่ถามเลือกซ้ำ
        }

        var programFile = pick.Program; // อ่านชื่อโปรแกรมที่ได้จากการเลือก
        if (programFile == null) // ผู้ใช้ยังไม่ได้เลือกโปรแกรมที่จะส่ง
            return new UvSendResult(SendStatus.Cancelled, uvName, done); // จบเพราะผู้ใช้ยังไม่เลือกโปรแกรม

        if (pick.IsDefault && // ได้โปรแกรมสำรองแทนชื่อเดิมในงาน
            !UvProgramResolver.ConfirmDefault(uvRow.ProgramName ?? "", uvName, owner as Control)) // ให้ผู้ใช้ยืนยันก่อนใช้โปรแกรมสำรอง
            return new UvSendResult(SendStatus.Cancelled, uvName, done); // จบเพราะผู้ใช้ไม่ยืนยันโปรแกรมสำรอง

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var uvTcp = new UvTcpService(); // เตรียมชุดคำสั่ง TCP ของ UV

            var (stopOk, _) = await uvTcp.StopAsync(ip, port); // ขอหยุด UV ก่อนเขียนข้อมูล โดยผลหยุดไม่ใช้บล็อกขั้นถัดไป
            done.Add(stopOk ? "สั่งหยุดเครื่อง" : "สั่งหยุดเครื่อง (ไม่ตอบรับ — ทำต่อ)"); // เก็บว่าคำสั่งหยุดได้รับคำตอบหรือไม่

            var (writeOk, writeMsg) = await CpiWriteService.WriteAsync( // เขียนข้อมูลพิมพ์ของ Job ลงฐานข้อมูล CPI
                cpiPath, table, // เลือกไฟล์และตาราง CPI ของเครื่องนี้
                uvRow.Lot, uvRow.ErpMfg, // เขียน Lot และชื่อ ERP ของงานนี้
                uvRow.Text1, uvRow.Text2, uvRow.Text3, uvRow.Text4, uvRow.Text5); // เขียนข้อความพิมพ์ทั้งห้าช่องของงาน

            if (!writeOk) // ตรวจว่าข้อความลง CPI สำเร็จหรือไม่
                return Stopped(uvName, done, $"เขียน CPI.db3 ({table}) — {writeMsg}"); // หยุดก่อน Load เมื่อเขียน CPI ไม่ผ่าน

            done.Add($"เขียน CPI.db3 ({table})" // เก็บผลเขียน CPI ไว้ในรายงานการส่ง
                + $"\n    Lot: {Dashed(uvRow.Lot)}" // แนบ Lot ที่เขียนลง CPI ไว้ตรวจ
                + $"\n    Name: {Dashed(uvRow.ErpMfg)}"); // แนบ ERP ของข้อมูลที่เขียน

            var (tcpOk, tcpLog, startWarning) = await uvTcp.LoadAndStartAsync(ip, port, programFile); // โหลดโปรแกรมแล้วรับผล Start แยกกัน
            if (!tcpOk) // ตรวจว่า UV รับข้อมูลโปรแกรมหรือไม่
                return Stopped(uvName, done, tcpLog.Trim()); // หยุดพร้อม log เมื่อ Load ไม่ผ่าน

            done.Add($"โหลดโปรแกรม {programFile}.uvdx"); // เก็บชื่อโปรแกรมที่สั่งโหลดไว้ในผลส่ง
            if (startWarning == null) done.Add("เครื่องตอบรับคำสั่งเริ่มพิมพ์"); // จดผล Start เฉพาะตอนเครื่องตอบรับ

            return new UvSendResult( // เตรียมผลส่ง UV ให้ผู้เรียก
                SendStatus.Ok, uvName, done, // ส่งผลสำเร็จพร้อมรายการขั้นที่ทำไป
                ProgramFile: programFile, UsedDefault: pick.IsDefault, Ip: ip, Port: port, StartWarning: startWarning); // แนบโปรแกรม ปลายทาง และคำเตือน Start
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return Stopped(uvName, done, ex.Message); // ส่งเหตุที่ขั้น UV ทำงานไม่จบ
        }
    }

    private static UvSendResult Blocked(string uvName, string reason) =>
        new(SendStatus.NotConfigured, uvName, [], FailReason: reason);

    private static UvSendResult Stopped(string uvName, List<string> done, string reason) =>
        new(SendStatus.Failed, uvName, done, FailReason: reason);

    private static async Task<bool> CanConnectAsync(string ip, int port) // ลองเปิด TCP เพื่อตรวจปลายทาง
    {
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            using var tcp = new System.Net.Sockets.TcpClient(); // สร้างตัวเชื่อมเฉพาะรอบตรวจนี้
            await tcp.ConnectAsync(ip, port) // ลองเชื่อมต่อ IP และพอร์ตที่รับมา
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds)); // จำกัดเวลารอปลายทางตอบ
            return true;
        }
        catch // เข้าทางนี้เมื่อทำรายการไม่สำเร็จ
        {
            return false;
        }
    }

    private static string Dashed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;
}
