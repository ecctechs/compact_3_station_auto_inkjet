using System.Windows.Forms;

using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>ผลรวมของการส่งหนึ่งขั้นตอน</summary>
public enum SendStatus
{
    Ok,

    /// <summary>ต่อเครื่องได้แต่ทำไม่สำเร็จกลางทาง</summary>
    Failed,

    /// <summary>ผู้ใช้กดยกเลิกที่กล่องเลือกโปรแกรม — ไม่ใช่ความผิดพลาด ไม่ต้องรายงาน</summary>
    Cancelled,

    /// <summary>ยังตั้งค่าไม่ครบ เช่น ไม่มี IP หรือหาไฟล์ CPI.db3 ไม่เจอ</summary>
    NotConfigured,

    /// <summary>ต่อเครื่องไม่ติดตั้งแต่แรก</summary>
    Unreachable,
}

/// <summary>ผลของเครื่อง MK หนึ่งตัว</summary>
/// <param name="Suspended">เก็บไว้รองรับผลลัพธ์เดิมของ UI</param>
/// <param name="Note">
/// คำเตือนที่ไม่ได้ทำให้การส่งล้มเหลว เช่นเครื่องไม่ยอมรับคำสั่งเริ่มพิมพ์
/// ข้อมูลของงานยังเข้าเครื่องครบ แต่ต้องให้คนหน้างานเห็นว่ามีอะไรผิดปกติ
/// </param>
public sealed record MkMachineResult(
    string Name, string? Error, bool Suspended = false, string? Note = null)
{
    public bool Ok => Error == null;
}

public sealed record MkSendResult(SendStatus Status, List<MkMachineResult> Machines);

/// <summary>
/// ผลของการส่ง UV หนึ่งเครื่อง
/// <para><paramref name="Done"/> คือขั้นที่ผ่านไปแล้ว ใช้บอกผู้ใช้ว่าค้างตรงไหน</para>
/// </summary>
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

/// <summary>
/// ส่งงานเข้าเครื่อง MK / UV — ตรรกะล้วน ไม่ผูกกับหน้าจอไหน
/// <para>
/// แยกออกมาเพื่อให้หน้า Order List สั่งส่งได้ตอนกดปุ่มเริ่มงาน ไม่ใช่เรียกได้
/// เฉพาะจากปุ่มในหน้า Order Detail
/// </para>
/// <para>
/// <b>ไม่เรียก <c>Notify</c></b> ตามกติกาของโปรเจค — คืนผลเป็นโครงสร้างให้หน้าจอที่
/// เรียกเป็นคนเล่าให้ผู้ใช้ฟังเอง ยกเว้นกล่องเลือกรุ่นย่อยของ UV ที่ต้องถามผู้ใช้
/// ระหว่างทางจริง ๆ (<see cref="UvProgramResolver"/> เปิดเอง เหมือนที่เคยเป็น)
/// </para>
/// </summary>
public static class JobSendService
{
    private const int MkPort = 9004;

    /// <summary>จำนวนช่องข้อความต่อเครื่องหนึ่งตัว — ตรงกับที่ PrintData.db3 เก็บไว้</summary>
    private const int MkBlockCount = 5;

    /// <summary>
    /// ช่องข้อความช่องแรกในเครื่อง — ข้อมูลเก็บเป็นบล็อก 1-5 แต่เครื่องใช้ช่อง 6-10
    ///
    /// <para>
    /// ยกมาจากโปรแกรมเดิม (PySocketClient/csv_extractor.py) ที่ส่งด้วย <c>str(i+6)</c>
    /// เมื่อ i วน 0 ถึง 4 การส่งไปที่ช่อง 1-5 เครื่องรับคำสั่งโดยไม่ฟ้อง แต่เป็นคนละช่อง
    /// กับที่โปรแกรมในเครื่องใช้พิมพ์จริง ข้อความที่ส่งไปจึงไม่มีผลกับงานที่พ่นออกมา
    /// </para>
    /// </summary>
    private const int MkFirstDeviceBlock = 6;

    /// <summary>
    /// เว้นจังหวะก่อนส่งข้อความแต่ละช่อง — โปรแกรมเดิมหน่วง 30 ms ทุกครั้ง
    /// ยกมาทั้งค่าและตำแหน่ง เพราะจังหวะนี้พิสูจน์กับเครื่องจริงมาแล้วว่าใช้ได้
    /// </summary>
    private const int MkBlockGapMs = 30;
    private const int UvDefaultPort = 10086;
    private const int ConnectTimeoutSeconds = 3;

    // ── MK ─────────────────────────────────────────────────

    /// <summary>
    /// ส่งเข้าเครื่อง MK ตามที่งานกำหนดไว้
    /// <para>
    /// เก็บผลแยกทีละเครื่อง เพราะเครื่องหนึ่งสำเร็จอีกเครื่องพลาดเป็นเรื่องปกติ
    /// รวมเป็นบรรทัดเดียวแล้วจะไม่รู้ว่าเครื่องไหนไม่ผ่าน
    /// </para>
    /// <para>
    /// <b>เครื่องที่งานสั่งให้ใช้ แต่ยังไม่ได้ตั้ง IP ต้องฟ้อง ไม่ใช่ข้ามเงียบ ๆ</b> —
    /// เดิมข้ามไปเฉย ๆ พนักงานที่กด SWAP ให้งานไปเข้าอีกเครื่องจึงไม่รู้เลยว่า
    /// เครื่องปลายทางไม่ได้รับอะไร เห็นแต่ผลของเครื่องที่ตั้ง IP ไว้ แล้วเข้าใจว่า
    /// การสลับไม่ทำงาน
    /// </para>
    /// </summary>
    public static async Task<MkSendResult> SendMkAsync(PatternDetail pattern)
    {
        // กันไฟสถานะตามหน้าจอไม่ให้เปิดซ็อกเก็ตไปแย่งคิวเครื่องระหว่างส่งงานจริง
        // บอกชื่อเครื่องไปด้วย เพื่อให้ปุ่มกดหน้างานของเครื่องอื่นไม่ถูกขวางไปด้วย
        using var busy = MachineBusy.Hold("MK");

        var heads = MkMachines
            .Select(m => new MkHead(
                CustomSettingsManager.Read(m.NameKey, m.Fallback),
                CustomSettingsManager.Read(m.IpKey),
                pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == m.Ordinal),
                m.Label))
            .ToList();

        // สองหัวส่งพร้อมกัน ไม่ต้องรอหัวแรกเสร็จก่อน
        //
        // หัวหนึ่งคือ 12 คำสั่ง (SQ · FW · FS+F1 ห้าบล็อก) ที่ต้องรอคำตอบทีละคำสั่ง
        // พร้อมช่วงเว้นตามโปรแกรมเดิม
        // สองหัวเป็นเครื่องคนละตัว สายคนละเส้น ไม่มีเหตุให้ต้องรอกัน
        //
        // ยกเว้นตั้ง IP ซ้ำกัน = เครื่องเดียวกัน ห้ามเปิดสองสายเข้าไปพร้อมกัน เครื่องจะ
        // ได้คำสั่งของสองหัวสลับกันไปมา จึงถอยกลับไปทำทีละหัวเหมือนเดิม
        var outcomes = SameDevice(heads)
            ? [await SendHeadAsync(heads[0]), await SendHeadAsync(heads[1])]
            : await Task.WhenAll(heads.Select(SendHeadAsync));

        // เรียงผลตามหัวเสมอ ไม่ใช่ตามว่าหัวไหนเสร็จก่อน
        var machines = outcomes.Where(o => o.Result != null).Select(o => o.Result!).ToList();
        bool anySent = outcomes.Any(o => o.Sent);
        bool workFailed = outcomes.Any(o => o.Failed);

        if (machines.Count == 0)
            return new MkSendResult(SendStatus.NotConfigured, machines);

        // ไม่มีเครื่องไหนได้รับงานเลย = ขั้นตอนนี้ยังไม่ได้ทำ ต้องไม่ถูกบันทึกว่าสำเร็จ
        // ไม่งั้นงานจะเดินไปขั้นถัดไปทั้งที่ยังไม่ได้พ่นอะไรลงชิ้นงาน
        if (!anySent)
            return new MkSendResult(SendStatus.NotConfigured, machines);

        // ตัดสินสำเร็จ/ล้มเหลวจากเฉพาะเครื่องที่ "มีงาน" เท่านั้น
        //
        // เครื่องที่งานไม่ได้ใช้จะไม่ได้รับคำสั่งจาก flow นี้
        return new MkSendResult(
            workFailed ? SendStatus.Failed : SendStatus.Ok,
            machines);
    }

    /// <summary>
    /// เครื่องที่ต่อไม่ติด พร้อมเหตุผล — Name คือชื่อที่คนหน้างานเรียกกัน · Step คือขั้นในแผน (MK / UV1 / UV2)
    /// ที่จะถูกข้ามถ้าคนยืนยันข้ามเครื่องนี้ (หัว MK สองหัวเป็นขั้น MK เดียวกัน)
    /// </summary>
    public sealed record UnreachableMachine(string Name, string Reason, string Step = "");

    /// <summary>
    /// ลองต่อทุกเครื่องที่แผนของงานนี้ต้องใช้ ก่อนจะลงมือส่งอะไรจริง
    ///
    /// <para>
    /// งานที่ใช้หลายเครื่องต้องต่อได้ครบทุกเครื่องถึงจะเริ่มได้ ถ้าปล่อยให้เริ่มทั้งที่
    /// เครื่องหลังต่อไม่ติด เครื่องหน้าจะรับงานไปพ่นลงชิ้นงานจริงแล้ว ย้อนคืนไม่ได้
    /// และงานจะค้างครึ่งทาง — กดเริ่มใหม่ก็ไม่ได้เพราะสถานะเป็นกำลังผลิตไปแล้ว
    /// </para>
    /// <para>
    /// ตรวจเฉพาะหัวพ่นที่งานนี้ใช้จริง หัวที่ไม่มีโปรแกรมจะไม่ถูกส่งคำสั่ง
    /// ต่อไม่ติดก็เป็นแค่คำเตือน ไม่ใช่เหตุให้ทั้งงานเริ่มไม่ได้
    /// </para>
    /// </summary>
    public static async Task<List<UnreachableMachine>> UnreachableAsync(
        IEnumerable<string> steps, PatternDetail? pattern, List<UvJobDataDto>? uvData)
    {
        var bad = new List<UnreachableMachine>();
        var targets = new List<(string Name, string Host, int Port, string Step)>();

        foreach (var step in steps.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.Equals(step, "MK", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (ipKey, nameKey, fallbackName, ordinal, _) in MkMachines)
                {
                    var config = pattern?.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
                    if (!HasProgram(config)) continue;

                    var name = CustomSettingsManager.Read(nameKey, fallbackName);
                    var ip = CustomSettingsManager.Read(ipKey);

                    if (string.IsNullOrWhiteSpace(ip))
                        bad.Add(new UnreachableMachine(name, "ยังไม่ได้ตั้ง IP", "MK"));
                    else targets.Add((name, ip, MkPort, "MK"));
                }

                continue;
            }

            int uvNumber = string.Equals(step, "UV1", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
            var uvName = UvSettingsManager.Read(
                uvNumber == 1 ? "UV1_NAME" : "UV2_NAME", $"UV-00{uvNumber}");

            // ไม่มีข้อมูลของเครื่องนี้ = ส่งไม่ได้อยู่แล้ว ไม่ต้องรอไปเจอตอนส่ง
            if (uvData?.Any(r => string.Equals(r.Machine, step, StringComparison.OrdinalIgnoreCase)) != true)
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่มีข้อมูล {step} ของงานนี้", step.ToUpperInvariant()));
                continue;
            }

            // เช็คไฟล์บนเครื่อง UV ผ่านเครือข่าย — ทำเบื้องหลัง ไม่ให้หน้าจอรอ
            if (await UvSettingsManager.GetCpiPathAsync(uvNumber) == null)
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่ได้ตั้งโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3", step.ToUpperInvariant()));
                continue;
            }

            var uvIp = CustomSettingsManager.Read($"UV00{uvNumber}_IP");
            if (string.IsNullOrWhiteSpace(uvIp))
            {
                bad.Add(new UnreachableMachine(uvName, $"ยังไม่ได้ตั้ง IP ของ UV{uvNumber}", step.ToUpperInvariant()));
                continue;
            }

            int uvPort = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p)
                ? p
                : UvDefaultPort;

            targets.Add((uvName, uvIp, uvPort, step.ToUpperInvariant()));
        }

        var connected = await ConnectionPreflight.CheckAsync(
            targets.Select(t => (t.Host, t.Port)).ToList(), CanConnectAsync);
        for (int i = 0; i < targets.Count; i++)
            if (!connected[i])
                bad.Add(new UnreachableMachine(targets[i].Name, $"ต่อไม่ติด ({targets[i].Host}:{targets[i].Port})", targets[i].Step));
        return bad;
    }

    /// <summary>งานนี้มีโปรแกรมให้เครื่องนี้จริงไหม — แถวเปล่าที่มีแต่ ordinal ไม่นับ</summary>
    private static bool HasProgram(InkjetConfigDto? config) =>
        config != null
        && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

    private static readonly (string IpKey, string NameKey, string Fallback, int Ordinal, string Label)[]
        MkMachines =
        [
            ("MK058_COM", "MK058_NAME", "MK-058", 1, "MK1"),
            ("MK059_COM", "MK059_NAME", "MK-059", 2, "MK2"),
        ];

    /// <summary>หัวพ่นหนึ่งหัวที่จะส่ง — อ่านค่าตั้งไว้ก่อนเริ่มยิง</summary>
    private sealed record MkHead(string Name, string Ip, InkjetConfigDto? Config, string Label);

    /// <summary>
    /// ผลของหัวหนึ่ง — <c>Result</c> เป็น null เมื่อไม่มีอะไรต้องรายงาน
    /// (งานไม่ได้ใช้หัวนี้และหัวนี้ก็ยังไม่ได้ตั้ง IP)
    /// </summary>
    private readonly record struct MkHeadOutcome(MkMachineResult? Result, bool Sent, bool Failed);

    /// <summary>ส่งหัวที่มีงาน ส่วนหัวที่งานไม่ได้ใช้จะไม่ได้รับคำสั่ง</summary>
    private static async Task<MkHeadOutcome> SendHeadAsync(MkHead head)
    {
        if (!HasProgram(head.Config))
            return new(null, false, false);

        if (string.IsNullOrWhiteSpace(head.Ip))
        {
            return new(new MkMachineResult(
                head.Name, $"{head.Label}: ยังไม่ได้ตั้ง IP — ไปตั้งที่ Setting → Inkjet Setting"),
                false, true);
        }

        var (error, note) = await SendToOneMkAsync(head.Ip, head.Config!, head.Label);
        return new(new MkMachineResult(head.Name, error, Note: note), error == null, error != null);
    }

    /// <summary>สองหัวตั้ง IP เดียวกัน = ที่จริงคือเครื่องเดียว ต้องส่งทีละหัว</summary>
    private static bool SameDevice(List<MkHead> heads) =>
        heads.Count == 2
        && !string.IsNullOrWhiteSpace(heads[0].Ip)
        && string.Equals(heads[0].Ip.Trim(), heads[1].Ip.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>ลำดับคำสั่งของเครื่อง MK — คืน null เมื่อสำเร็จ</summary>
    /// <summary>
    /// ค่าของบล็อกที่เครื่องรับไม่ได้ — คืนข้อความบอกว่าช่องไหนผิด หรือ null เมื่อผ่านหมด
    ///
    /// <para>
    /// Scale คือตัวคูณขนาดตัวอักษร ค่า 0 แปลว่าไม่มีขนาด เครื่องจึงปฏิเสธด้วย ER,F1,22
    /// ช่องที่ไม่ได้กรอกไว้เลยไม่นับ เพราะตัวส่งใส่ 1 ให้อยู่แล้ว
    /// </para>
    /// </summary>
    /// <summary>
    /// ช่วงค่าที่เครื่องรับได้ ยกมาจากโปรแกรมเดิมทั้งชุด
    ///
    /// <para>
    /// โปรแกรมเดิม (PySocketClient/csv_extractor.py) ตรวจก่อนส่งทุกครั้งและฟ้องเป็น
    /// ข้อความบอกช่วงที่ถูกต้อง ของเราไม่เคยตรวจเลย ค่าที่เกินช่วงจึงหลุดไปถึงเครื่อง
    /// แล้วได้รหัสกลับมาแบบเดาไม่ออก เช่น ER,F1,22 ตอนที่ Scale เป็น 0
    /// </para>
    /// </summary>
    private static readonly (string Name, int Min, int Max)[] BlockRanges =
    [
        ("X", 0, 4095),
        ("Y", 0, 31),
        ("Size", 0, 22),
        ("Scale", 1, 10),
    ];

    /// <summary>
    /// ค่าที่เครื่องรับไม่ได้ — คืนข้อความบอกว่าช่องไหนผิด หรือ null เมื่อผ่านหมด
    ///
    /// <para>
    /// ตรวจเฉพาะช่องที่กรอกค่าไว้แล้ว ช่องที่ยังว่างไม่นับว่าผิด เพราะงานเก่าจำนวนมาก
    /// ไม่เคยกรอกค่าพวกนี้และส่งเข้าเครื่องได้มาตลอด การบังคับให้กรอกครบตอนนี้จะทำให้
    /// งานที่เคยส่งได้กลับส่งไม่ได้
    /// </para>
    /// </summary>
    /// <summary>
    /// ตรวจค่าของหัวพ่น MK ทุกตัวที่แผนนี้จะใช้ — null = ผ่าน
    ///
    /// <para>
    /// มีไว้เรียก <b>ก่อนจองคิว</b> ไม่ใช่ตอนส่ง ค่าที่ผิดต้องไม่ทำให้งานได้แถวคิวไป
    /// ถือเครื่องไว้ แล้วไปตายตอนส่ง ซึ่งคนต้องมาไล่ปล่อยเครื่องเองทีหลัง
    /// </para>
    /// <para>
    /// ตรวจชุดเดียวกับที่ <see cref="SendToOneMkAsync"/> ตรวจก่อนแตะเครื่อง จึงไม่มี
    /// ทางที่ด่านสองชั้นจะตอบไม่ตรงกัน
    /// </para>
    /// </summary>
    public static string? InvalidMkConfig(PatternDetail? pattern, IEnumerable<string> steps)
    {
        if (pattern == null) return null;

        foreach (var ordinal in new[] { 1, 2 })
        {
            // ขั้นของ MK ในแผนคือ "MK" เฉย ๆ ไม่ได้แยกหัว — ตรวจหัวที่มีโปรแกรมจริง
            if (!steps.Contains("MK", StringComparer.OrdinalIgnoreCase)) break;

            var config = pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
            if (config == null || !HasProgramName(config)) continue;

            var label = CustomSettingsManager.Read(
                ordinal == 1 ? "MK058_NAME" : "MK059_NAME", ordinal == 1 ? "MK-058" : "MK-059");

            if (InvalidConfig(config, label) is string bad) return bad;
        }

        return null;
    }

    /// <summary>หัวนี้มีโปรแกรมให้พ่นจริงไหม — ไม่มีก็ไม่ต้องตรวจค่าของมัน</summary>
    private static bool HasProgramName(InkjetConfigDto config) =>
        !string.IsNullOrWhiteSpace(config.ProgramName);

    private static string? InvalidConfig(InkjetConfigDto config, string label)
    {
        foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber))
        {
            // X กับ Scale เป็น 0 พร้อมกัน = บล็อกนี้ยังไม่ได้ตั้งค่า
            //
            // ส่งไปแล้วเครื่องรับคำสั่งตามปกติ แต่พ่นออกมาผิดตำแหน่งและผิดขนาด
            // ซึ่งรู้ตัวตอนเห็นชิ้นงานแล้ว ย้อนไม่ได้ จึงกันไว้ก่อนส่ง
            //
            // เช็คก่อนกฎช่วงข้างล่าง เพราะ Scale = 0 ติดกฎช่วง 1-10 อยู่แล้วแต่ได้
            // ข้อความว่า "อยู่นอกช่วง" ซึ่งไม่ได้บอกว่าที่จริงคือยังไม่ได้ตั้งค่าทั้งบล็อก
            if (block.X == 0 && block.Scale == 0)
            {
                return $"{label}: Block {block.BlockNumber} ยังไม่ได้ตั้งค่า (X = 0 และ Scale = 0) "
                     + "— แก้ที่หน้า Order Detail ก่อนส่ง";
            }

            foreach (var (name, min, max) in BlockRanges)
            {
                int? value = name switch
                {
                    "X" => block.X,
                    "Y" => block.Y,
                    "Size" => block.Size,
                    _ => block.Scale,
                };

                if (value is int v && (v < min || v > max))
                {
                    return $"{label}: Block {block.BlockNumber} มี {name} = {v} "
                         + $"อยู่นอกช่วง {min}-{max} — แก้ที่หน้า Order Detail";
                }
            }
        }

        return null;
    }

    /// <summary>คำเตือนทั้งหมดรวมเป็นบรรทัดเดียว — ไม่มีเลยคืน null</summary>
    private static string? Note(List<string> notes) =>
        notes.Count == 0 ? null : string.Join(" · ", notes);

    /// <summary>
    /// ข้อความบอกว่าขั้นไหนไม่ผ่าน พร้อมคำตอบดิบจากเครื่อง
    ///
    /// ต้องแนบคำตอบมาด้วยเสมอ เพราะเครื่องตอบเป็นรหัส เช่น ER,FW,00 ซึ่งบอกได้ว่า
    /// ติดที่อะไร ถ้าบอกแค่ "ไม่สำเร็จ" คนหน้างานได้แต่เดา
    /// </summary>
    private static string Reject(string label, string step, CommandResult result)
    {
        var reply = (result.Response ?? "").Trim();
        return reply.Length == 0
            ? $"{label}: {step} — เครื่องไม่ตอบ"
            : $"{label}: {step} — เครื่องปฏิเสธ ({reply})";
    }

    private static async Task<(string? Error, string? Note)> SendToOneMkAsync(
        string ip, InkjetConfigDto config, string label)
    {
        // ตรวจช่วงค่าก่อนแตะเครื่อง ชุดเดียวกับที่โปรแกรมเดิมตรวจ
        //
        // ตรวจตั้งแต่ยังไม่ต่อสาย เพราะค่าที่ผิดไม่มีเหตุให้ต้องไปรบกวนเครื่องเลย
        // ถ้าปล่อยไปเจอตอนส่ง บล็อกก่อนหน้าจะถูกเขียนลงเครื่องไปแล้วครึ่งทาง และ
        // คนอ่านก็ได้แต่รหัสจากเครื่องมาเดาเอง เช่น ER,F1,22 ตอนที่ Scale เป็น 0
        if (InvalidConfig(config, label) is string bad) return (bad, null);

        var tcp = new TcpManager();
        try
        {
            await tcp.ConnectAsync(ip, MkPort)
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds));
            var adapter = new MkCompactAdapter(tcp);

            // ส่งเฉพาะ SQ ก่อน แล้ว FW และ FS/F1 ของแต่ละ Block — ไม่ส่ง SR หรือ FM
            //
            // SQ เป็นคำสั่งคุมการพิมพ์ ตัวที่ตัดสินว่างานเข้าเครื่องคือ FW / FS / F1
            var notes = new List<string>();

            // ไม่รายงานผลของคำสั่งนี้
            //
            // เครื่องชุดนี้ตอบ ER,SQ,01 ทุกครั้ง คือรู้จักคำสั่งแต่ไม่ให้สั่งเริ่มพิมพ์
            // จากระยะไกล ซึ่งเป็นสภาพปกติของมัน ไม่ใช่ความผิดพลาดของงาน โปรแกรมเดิม
            // ก็สั่งตัวนี้ทุกครั้งและไม่เคยดูคำตอบเลย งานก็พิมพ์ออกมาได้ตามปกติ
            //
            // ถ้าเครื่องเงียบไปจริง ๆ (สายหลุด) คำสั่งที่เป็นตัวงานถัดจากนี้จะฟ้องเอง
            // จึงไม่ต้องกันไว้ตรงนี้ซ้ำ
            await adapter.ResumeAsync();

            var fw = await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1);
            if (!fw.Success)
                return (Reject(label, $"เปลี่ยนไปโปรแกรม {config.ProgramNumber}", fw), Note(notes));

            // ส่งครบทุกช่องเสมอ ช่องที่งานนี้ไม่ได้ใช้ก็ส่งข้อความว่างไปทับ —
            // กฎเดียวกับโปรแกรมเดิม ถ้าข้ามไปเฉย ๆ ข้อความของงานก่อนหน้าจะค้าง
            // อยู่ในช่องนั้นแล้วถูกพิมพ์ติดไปกับงานใหม่
            for (int slot = 1; slot <= MkBlockCount; slot++)
            {
                var block = config.TextBlocks.FirstOrDefault(b => b.BlockNumber == slot)
                    ?? new TextBlockDto { BlockNumber = slot, Text = "" };

                await Task.Delay(MkBlockGapMs);

                int deviceBlock = slot + MkFirstDeviceBlock - 1;
                var fb = await adapter.SendTextBlockAsync(block, deviceBlock);
                if (!fb.Success) return (Reject(label, $"ส่ง Block {slot}", fb), Note(notes));
            }

            return (null, Note(notes));
        }
        catch (Exception ex)
        {
            return ($"{label}: {ex.Message}", null);
        }
        finally
        {
            tcp.Disconnect();
        }
    }

    // ── UV ─────────────────────────────────────────────────

    /// <summary>
    /// ส่งเข้าเครื่อง UV: หยุดเครื่อง → เขียนข้อความลง CPI.db3 → โหลดโปรแกรม → เริ่มพิมพ์
    /// <para>
    /// กล่องเลือกรุ่นย่อย (.uvdx) จะเด้งขึ้นถ้าชื่อโปรแกรมตรงกับหลายไฟล์ — ผูกกับ
    /// <paramref name="owner"/> ที่ส่งมา จึงขึ้นบนหน้าจอที่สั่งส่ง ไม่ว่าจะเป็นหน้าไหน
    /// </para>
    /// <para>
    /// ส่ง <paramref name="forcedProgram"/> มาเมื่อมีคนเลือกโปรแกรมไว้ให้แล้วที่อื่น
    /// (ST3 กดเริ่มงานแล้วฝากให้ ST1 ส่ง) — ข้ามทั้งกล่องเลือกรุ่นย่อยและกล่องยืนยัน
    /// default ทำให้ส่งได้เงียบ ๆ โดยไม่มีหน้าต่างเด้งค้างที่จอ ST1 ซึ่งไม่มีคนเฝ้า
    /// </para>
    /// </summary>
    public static async Task<UvSendResult> SendUvAsync(
        IWin32Window? owner, int uvNumber, List<UvJobDataDto> uvData,
        string? forcedProgram = null, bool allowPrompt = true)
    {
        string stepName = uvNumber == 1 ? "UV1" : "UV2";

        using var busy = MachineBusy.Hold(stepName);

        string table = uvNumber == 1 ? "MK063" : "MK067";

        var uvName = uvNumber == 1
            ? UvSettingsManager.Read("UV1_NAME", "UV-001")
            : UvSettingsManager.Read("UV2_NAME", "UV-002");

        var done = new List<string>();

        var uvRow = uvData.FirstOrDefault(r => r.Machine == stepName);
        if (uvRow == null)
            return Blocked(uvName, $"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก");

        var cpiPath = await UvSettingsManager.GetCpiPathAsync(uvNumber);
        if (cpiPath == null)
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3");

        var ip = CustomSettingsManager.Read($"UV00{uvNumber}_IP");
        if (string.IsNullOrWhiteSpace(ip))
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่า IP ของ UV{uvNumber}");

        int port = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p)
            ? p
            : UvDefaultPort;

        if (!await CanConnectAsync(ip, port))
            return new UvSendResult(SendStatus.Unreachable, uvName, done, Ip: ip, Port: port);

        UvProgramPick pick;
        if (string.IsNullOrWhiteSpace(forcedProgram))
        {
            if (!allowPrompt) return Blocked(uvName, "ยังไม่ได้เลือกโปรแกรม UV ก่อนส่ง");
            pick = await UvProgramResolver.ResolveAsync(uvRow.ProgramName, uvNumber, owner);
        }
        else
        {
            // เลือกมาแล้วจากที่อื่น — ถือว่าผ่านการยืนยันของคนมาเรียบร้อย
            pick = new UvProgramPick(forcedProgram.Trim(), false);
        }

        var programFile = pick.Program;
        if (programFile == null)
            return new UvSendResult(SendStatus.Cancelled, uvName, done);

        if (pick.IsDefault &&
            !UvProgramResolver.ConfirmDefault(uvRow.ProgramName ?? "", uvName, owner as Control))
            return new UvSendResult(SendStatus.Cancelled, uvName, done);

        try
        {
            var uvTcp = new UvTcpService();

            // 1. หยุดเครื่องก่อนเสมอ — ไม่ตอบรับก็ไปต่อ เพราะเครื่องอาจหยุดอยู่แล้ว
            var (stopOk, _) = await uvTcp.StopAsync(ip, port);
            done.Add(stopOk ? "สั่งหยุดเครื่อง" : "สั่งหยุดเครื่อง (ไม่ตอบรับ — ทำต่อ)");

            // 2. เขียนข้อความลง CPI.db3
            var (writeOk, writeMsg) = await CpiWriteService.WriteAsync(
                cpiPath, table,
                uvRow.Lot, uvRow.ErpMfg,
                uvRow.Text1, uvRow.Text2, uvRow.Text3, uvRow.Text4, uvRow.Text5);

            if (!writeOk)
                return Stopped(uvName, done, $"เขียน CPI.db3 ({table}) — {writeMsg}");

            done.Add($"เขียน CPI.db3 ({table})"
                + $"\n    Lot: {Dashed(uvRow.Lot)}"
                + $"\n    Name: {Dashed(uvRow.ErpMfg)}");

            // 3. โหลดโปรแกรม แล้วสั่งเริ่มพิมพ์
            var (tcpOk, tcpLog, startWarning) = await uvTcp.LoadAndStartAsync(ip, port, programFile);
            if (!tcpOk)
                return Stopped(uvName, done, tcpLog.Trim());

            done.Add($"โหลดโปรแกรม {programFile}.uvdx");
            if (startWarning == null) done.Add("เครื่องตอบรับคำสั่งเริ่มพิมพ์");

            return new UvSendResult(
                SendStatus.Ok, uvName, done,
                ProgramFile: programFile, UsedDefault: pick.IsDefault, Ip: ip, Port: port, StartWarning: startWarning);
        }
        catch (Exception ex)
        {
            return Stopped(uvName, done, ex.Message);
        }
    }

    private static UvSendResult Blocked(string uvName, string reason) =>
        new(SendStatus.NotConfigured, uvName, [], FailReason: reason);

    private static UvSendResult Stopped(string uvName, List<string> done, string reason) =>
        new(SendStatus.Failed, uvName, done, FailReason: reason);

    private static async Task<bool> CanConnectAsync(string ip, int port)
    {
        try
        {
            using var tcp = new System.Net.Sockets.TcpClient();
            await TcpConnect.ConnectAsync(tcp, ip, port, TimeSpan.FromSeconds(ConnectTimeoutSeconds));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string Dashed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;
}
