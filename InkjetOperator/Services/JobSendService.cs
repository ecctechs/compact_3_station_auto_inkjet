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
public sealed record MkMachineResult(string Name, string? Error)
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
    private const int UvDefaultPort = 10086;
    private const int ConnectTimeoutSeconds = 3;

    // ── MK ─────────────────────────────────────────────────

    /// <summary>
    /// ส่งเข้าเครื่อง MK ทุกตัวที่ตั้ง IP ไว้
    /// <para>
    /// เก็บผลแยกทีละเครื่อง เพราะเครื่องหนึ่งสำเร็จอีกเครื่องพลาดเป็นเรื่องปกติ
    /// รวมเป็นบรรทัดเดียวแล้วจะไม่รู้ว่าเครื่องไหนไม่ผ่าน
    /// </para>
    /// </summary>
    public static async Task<MkSendResult> SendMkAsync(PatternDetail pattern)
    {
        var machines = new List<MkMachineResult>();

        foreach (var (ipKey, nameKey, fallbackName, ordinal, label) in MkMachines)
        {
            var ip = CustomSettingsManager.Read(ipKey);
            if (string.IsNullOrWhiteSpace(ip)) continue;

            var config = pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal);
            if (config == null) continue;

            var name = CustomSettingsManager.Read(nameKey, fallbackName);
            machines.Add(new MkMachineResult(name, await SendToOneMkAsync(ip, config, label)));
        }

        if (machines.Count == 0)
            return new MkSendResult(SendStatus.NotConfigured, machines);

        return new MkSendResult(
            machines.All(m => m.Ok) ? SendStatus.Ok : SendStatus.Failed,
            machines);
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

            foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber))
            {
                var fb = await adapter.SendTextBlockAsync(block, block.BlockNumber);
                if (!fb.Success) return $"{label}: ส่ง Block {block.BlockNumber} ไม่สำเร็จ";
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
    /// </summary>
    public static async Task<UvSendResult> SendUvAsync(
        IWin32Window? owner, int uvNumber, List<UvJobDataDto> uvData)
    {
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

        var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber);
        var pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, owner);

        var programFile = pick.Program;
        if (programFile == null)
            return new UvSendResult(SendStatus.Cancelled, uvName, done);

        if (pick.IsDefault &&
            !UvProgramResolver.ConfirmDefault(uvRow.ProgramName ?? "", uvName, owner))
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
