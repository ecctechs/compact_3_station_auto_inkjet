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
/// <param name="Suspended">
/// งานนี้ไม่มีโปรแกรมให้เครื่องนี้ จึงสั่งหยุดพิมพ์แทนการส่งข้อมูล — ไม่ใช่ความล้มเหลว
/// แต่ก็ไม่ใช่การส่งงาน ผู้เรียกต้องแยกข้อความให้คนอ่านรู้ว่าเครื่องนี้ไม่ได้รับงาน
/// </param>
public sealed record MkMachineResult(string Name, string? Error, bool Suspended = false)
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
    int Port = 0);

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
        using var busy = MachineBusy.Hold();

        var machines = new List<MkMachineResult>();
        bool anySent = false;
        bool workFailed = false;

        foreach (var (ipKey, nameKey, fallbackName, ordinal, label) in MkMachines)
        {
            var name = CustomSettingsManager.Read(nameKey, fallbackName);
            var ip = CustomSettingsManager.Read(ipKey);
            var config = pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);

            if (!HasProgram(config))
            {
                // ไม่ได้ตั้ง IP ก็สั่งอะไรไม่ได้ และงานนี้ก็ไม่ได้ใช้เครื่องนี้อยู่แล้ว
                if (string.IsNullOrWhiteSpace(ip)) continue;

                machines.Add(new MkMachineResult(
                    name, await StopOneMkAsync(ip, label), Suspended: true));
                continue;
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                workFailed = true;
                machines.Add(new MkMachineResult(
                    name, $"{label}: ยังไม่ได้ตั้ง IP — ไปตั้งที่ Setting → Inkjet Setting"));
                continue;
            }

            var error = await SendToOneMkAsync(ip, config!, label);
            if (error == null) anySent = true; else workFailed = true;
            machines.Add(new MkMachineResult(name, error));
        }

        if (machines.Count == 0)
            return new MkSendResult(SendStatus.NotConfigured, machines);

        // ไม่มีเครื่องไหนได้รับงานเลย = ขั้นตอนนี้ยังไม่ได้ทำ ต้องไม่ถูกบันทึกว่าสำเร็จ
        // ไม่งั้นงานจะเดินไปขั้นถัดไปทั้งที่ยังไม่ได้พ่นอะไรลงชิ้นงาน
        if (!anySent)
            return new MkSendResult(SendStatus.NotConfigured, machines);

        // ตัดสินสำเร็จ/ล้มเหลวจากเฉพาะเครื่องที่ "มีงาน" เท่านั้น
        //
        // การสั่งหยุดเครื่องที่ไม่มีงานเป็นการกันพลาด ไม่ใช่ตัวงาน เครื่องที่ปิดอยู่
        // หรือถอดสายไว้จะสั่งหยุดไม่ได้เป็นธรรมดา ถ้านับรวมเป็นล้มเหลวด้วย งานที่ใช้
        // เครื่องเดียวจะเดินไม่ได้เลยตลอดกะที่อีกเครื่องไม่ได้เปิด ทั้งที่เครื่องที่
        // ต้องทำงานรับข้อมูลครบและกำลังพิมพ์อยู่แล้ว
        //
        // ยังฟ้องเป็นคำเตือนอยู่ ผู้เรียกต้องแสดงให้เห็น เพราะกรณีสายหลุดขณะเครื่อง
        // ยังเปิดอยู่ เครื่องนั้นจะค้างพิมพ์ของงานก่อนหน้าต่อโดยเราสั่งหยุดไม่ได้
        return new MkSendResult(
            workFailed ? SendStatus.Failed : SendStatus.Ok,
            machines);
    }

    /// <summary>งานนี้มีโปรแกรมให้เครื่องนี้จริงไหม — แถวเปล่าที่มีแต่ ordinal ไม่นับ</summary>
    private static bool HasProgram(InkjetConfigDto? config) =>
        config != null
        && (config.ProgramNumber is > 0 || !string.IsNullOrWhiteSpace(config.ProgramName));

    /// <summary>
    /// สั่งเครื่องที่ไม่มีงานให้หยุดพิมพ์ — กฎเดียวกับโปรแกรมเดิม (PySocketClient)
    ///
    /// <para>
    /// ข้อมูลจาก PrintData.db3 สร้างแถวไว้ให้ทั้งสองเครื่องเสมอ ถึงงานจะใช้เครื่องเดียว
    /// อีกแถวจึงเป็นแถวเปล่า เดิมแถวเปล่านั้นถูกส่งเข้าเครื่องเหมือนงานจริง กลายเป็น
    /// สั่ง <c>FW,1</c> แล้วปิดท้ายด้วย <c>SQ</c> คือสั่งให้เครื่องเริ่มพิมพ์โปรแกรม
    /// เบอร์ 1 ลงชิ้นงานที่วิ่งผ่านมา
    /// </para>
    /// <para>
    /// การไม่ส่งอะไรเลยก็ไม่ปลอดภัย เพราะเครื่องยังค้างโปรแกรมของงานก่อนหน้าแล้ว
    /// พิมพ์ของงานเก่าทับ ต้องสั่งหยุดให้ชัดเจนเท่านั้น
    /// </para>
    /// </summary>
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

    /// <summary>ลำดับคำสั่งของเครื่อง MK — คืน null เมื่อสำเร็จ</summary>
    private static async Task<string?> SendToOneMkAsync(string ip, InkjetConfigDto config, string label)
    {
        var tcp = new TcpManager();
        try
        {
            await tcp.ConnectAsync(ip, MkPort)
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds));
            var adapter = new MkCompactAdapter(tcp);

            var sr = await adapter.SuspendAsync();
            if (!sr.Success) return $"{label}: Suspend ไม่สำเร็จ";

            var fw = await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1);
            if (!fw.Success) return $"{label}: เปลี่ยนโปรแกรมไม่สำเร็จ";

            // ส่งครบทุกช่องเสมอ ช่องที่งานนี้ไม่ได้ใช้ก็ส่งข้อความว่างไปทับ —
            // กฎเดียวกับโปรแกรมเดิม ถ้าข้ามไปเฉย ๆ ข้อความของงานก่อนหน้าจะค้าง
            // อยู่ในช่องนั้นแล้วถูกพิมพ์ติดไปกับงานใหม่
            for (int slot = 1; slot <= MkBlockCount; slot++)
            {
                var block = config.TextBlocks.FirstOrDefault(b => b.BlockNumber == slot)
                    ?? new TextBlockDto { BlockNumber = slot, Text = "" };

                var fb = await adapter.SendTextBlockAsync(block, slot);
                if (!fb.Success) return $"{label}: ส่ง Block {slot} ไม่สำเร็จ";
            }

            // FM ต้องมาหลัง FS/F1 ตามสเปกของเครื่อง (FW -> FS/F1 -> FM)
            // ถ้าส่ง FM ก่อน Block ทิศทางที่ตั้งไว้จะถูก Block ที่ตามมาเขียนทับ
            // ปุ่ม ABC จะกดแล้วเครื่องพิมพ์หัวตั้งเหมือนเดิม
            var fm = await adapter.SendConfigAsync(config);
            if (!fm.Success) return $"{label}: ส่ง Config ไม่สำเร็จ";

            var sq = await adapter.ResumeAsync();
            if (!sq.Success) return $"{label}: Resume ไม่สำเร็จ";

            return null;
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
        string? forcedProgram = null)
    {
        using var busy = MachineBusy.Hold();

        string stepName = uvNumber == 1 ? "UV1" : "UV2";
        string table = uvNumber == 1 ? "MK063" : "MK067";

        var uvName = uvNumber == 1
            ? UvSettingsManager.Read("UV1_NAME", "UV-001")
            : UvSettingsManager.Read("UV2_NAME", "UV-002");

        var done = new List<string>();

        var uvRow = uvData.FirstOrDefault(r => r.Machine == stepName);
        if (uvRow == null)
            return Blocked(uvName, $"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก");

        var cpiPath = UvSettingsManager.GetCpiPath(uvNumber);
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
            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber);
            pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, owner);
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
            var (tcpOk, tcpLog) = await uvTcp.LoadAndStartAsync(ip, port, programFile);
            if (!tcpOk)
                return Stopped(uvName, done, tcpLog.Trim());

            done.Add($"โหลดโปรแกรม {programFile}.uvdx");
            done.Add("สั่งเริ่มพิมพ์");

            return new UvSendResult(
                SendStatus.Ok, uvName, done,
                ProgramFile: programFile, UsedDefault: pick.IsDefault, Ip: ip, Port: port);
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
            await tcp.ConnectAsync(ip, port)
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds));
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
