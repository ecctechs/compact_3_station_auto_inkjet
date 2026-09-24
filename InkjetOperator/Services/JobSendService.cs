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
/// <param name="Note">
/// คำเตือนที่ไม่ได้ทำให้การส่งล้มเหลว เช่นเครื่องไม่ยอมรับคำสั่งหยุด/เริ่มพิมพ์
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
        using var busy = MachineBusy.Hold(); // พักการอ่านสถานะเครื่องระหว่างส่งข้อมูล MK

        var machines = new List<MkMachineResult>(); // รวมผลแต่ละหัวเพื่อส่งกลับหน้า Order List
        bool anySent = false; // ยังไม่มีหัวที่รับข้อมูลสำเร็จในรอบนี้
        bool workFailed = false; // จำว่ามีหัวที่ต้องใช้แต่ส่งไม่ผ่านหรือไม่

        foreach (var (ipKey, nameKey, fallbackName, ordinal, label) in MkMachines) // ตรวจหัว MK ทีละตัวตามชุดตั้งค่า
        {
            var name = CustomSettingsManager.Read(nameKey, fallbackName); // อ่านชื่อหัวพิมพ์ไว้รายงานผล
            var ip = CustomSettingsManager.Read(ipKey); // อ่าน IP ของหัวที่จะติดต่อ
            var config = pattern.InkjetConfigs.FirstOrDefault(c => c.Ordinal == ordinal); // หา Config ของหัวนี้จาก Pattern งาน

            if (!HasProgram(config)) // หัวนี้ไม่มีโปรแกรมที่ต้องใช้กับงาน
            {
                // ไม่ได้ตั้ง IP ก็สั่งอะไรไม่ได้ และงานนี้ก็ไม่ได้ใช้เครื่องนี้อยู่แล้ว
                if (string.IsNullOrWhiteSpace(ip)) continue; // หัวที่ไม่ใช้และไม่มี IP ไม่ต้องติดต่อ

                machines.Add(new MkMachineResult( // เก็บผลหยุดหัวที่ไม่ได้ใช้แยกจากหัวที่ต้องส่งงาน
                    name, await StopOneMkAsync(ip, label), Suspended: true)); // สั่งหยุดหัวที่ไม่ได้ใช้ แต่ไม่นับเป็นหัวทำงานล้มเหลว
                continue; // หัวนี้ไม่ใช้พิมพ์ จึงไปตรวจหัวถัดไป
            }

            if (string.IsNullOrWhiteSpace(ip)) // ต้องใช้หัวนี้แต่ยังไม่มี IP
            {
                workFailed = true; // นับว่ามีหัวที่ต้องทำงานแต่ส่งไม่ได้
                machines.Add(new MkMachineResult( // เก็บเหตุที่หัวนี้ยังรับงานไม่ได้
                    name, $"{label}: ยังไม่ได้ตั้ง IP — ไปตั้งที่ Setting → Inkjet Setting"));
                continue; // ไม่มี IP จึงยังส่งข้อมูลหัวนี้ไม่ได้
            }

            var (error, note) = await SendToOneMkAsync(ip, config!, label); // ส่งโปรแกรม ข้อความ และค่าพิมพ์ให้หัวนี้
            if (error == null) anySent = true; else workFailed = true; // รวมผลว่ามีหัวรับข้อมูลและมีหัวที่ส่งล้มเหลวหรือไม่
            machines.Add(new MkMachineResult(name, error, Note: note)); // เก็บผลของหัวนี้ไว้แสดงพร้อมหัวอื่น
        }

        if (machines.Count == 0) // ไม่มีหัว MK ที่ได้ติดต่อเลย
            return new MkSendResult(SendStatus.NotConfigured, machines); // แจ้งกลับว่ายังไม่มีชุดหัวที่พร้อมส่ง

        // ไม่มีเครื่องไหนได้รับงานเลย = ขั้นตอนนี้ยังไม่ได้ทำ ต้องไม่ถูกบันทึกว่าสำเร็จ
        // ไม่งั้นงานจะเดินไปขั้นถัดไปทั้งที่ยังไม่ได้พ่นอะไรลงชิ้นงาน
        if (!anySent) // ไม่มีหัวทำงานที่รับข้อมูลสำเร็จ
            return new MkSendResult(SendStatus.NotConfigured, machines); // แจ้งว่ารอบนี้ยังไม่มีหัวส่งงานได้ พร้อมรายละเอียดแต่ละหัว

        // ตัดสินสำเร็จ/ล้มเหลวจากเฉพาะเครื่องที่ "มีงาน" เท่านั้น
        //
        // การสั่งหยุดเครื่องที่ไม่มีงานเป็นการกันพลาด ไม่ใช่ตัวงาน เครื่องที่ปิดอยู่
        // หรือถอดสายไว้จะสั่งหยุดไม่ได้เป็นธรรมดา ถ้านับรวมเป็นล้มเหลวด้วย งานที่ใช้
        // เครื่องเดียวจะเดินไม่ได้เลยตลอดกะที่อีกเครื่องไม่ได้เปิด ทั้งที่เครื่องที่
        // ต้องทำงานรับข้อมูลครบและกำลังพิมพ์อยู่แล้ว
        //
        // ยังฟ้องเป็นคำเตือนอยู่ ผู้เรียกต้องแสดงให้เห็น เพราะกรณีสายหลุดขณะเครื่อง
        // ยังเปิดอยู่ เครื่องนั้นจะค้างพิมพ์ของงานก่อนหน้าต่อโดยเราสั่งหยุดไม่ได้
        return new MkSendResult( // สรุปผล MK ทั้งชุดกลับไปยังตัวคุมคิว
            workFailed ? SendStatus.Failed : SendStatus.Ok, // หัวที่ต้องใช้ล้มเหลวแม้เพียงหัวเดียว ให้ผลรวมไม่สำเร็จ
            machines); // ส่งรายละเอียดทุกหัวกลับไปแสดง
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

    /// <summary>
    /// ค่าที่เครื่องรับไม่ได้ — คืนข้อความบอกว่าช่องไหนผิด หรือ null เมื่อผ่านหมด
    ///
    /// <para>
    /// ตรวจเฉพาะช่องที่กรอกค่าไว้แล้ว ช่องที่ยังว่างไม่นับว่าผิด เพราะงานเก่าจำนวนมาก
    /// ไม่เคยกรอกค่าพวกนี้และส่งเข้าเครื่องได้มาตลอด การบังคับให้กรอกครบตอนนี้จะทำให้
    /// งานที่เคยส่งได้กลับส่งไม่ได้
    /// </para>
    /// </summary>
    private static string? InvalidConfig(InkjetConfigDto config, string label)
    {
        foreach (var (name, min, max) in MachineRanges)
        {
            int? value = name switch
            {
                "Width" => config.Width,
                "Height" => config.Height,
                _ => config.TriggerDelay,
            };

            if (value is int v && (v < min || v > max))
                return $"{label}: {name} = {v} อยู่นอกช่วง {min}-{max} ที่เครื่องรับได้";
        }

        foreach (var block in config.TextBlocks.OrderBy(b => b.BlockNumber))
        {
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
        if (InvalidConfig(config, label) is string bad) return (bad, null); // ตรวจค่าพิมพ์ก่อนเชื่อมต่อ ป้องกันส่งไปได้เพียงบางส่วน

        var tcp = new TcpManager(); // เตรียมช่อง TCP สำหรับหัว MK นี้
        try
        {
            await tcp.ConnectAsync(ip, MkPort) // เชื่อมต่อ IP หัว MK ที่พอร์ตของเครื่อง
                .WaitAsync(TimeSpan.FromSeconds(ConnectTimeoutSeconds)); // จำกัดเวลารอเชื่อมต่อ ไม่ให้ค้างไม่สิ้นสุด
            var adapter = new MkCompactAdapter(tcp); // ใช้ชุดคำสั่งของเครื่อง MK ผ่าน TCP ที่เปิดไว้

            // ลำดับคำสั่งยกมาจากโปรแกรมเดิมทั้งชุด — SQ ก่อน แล้วค่อย FW / FS+F1 / FM
            //
            // เครื่องที่มีงานจะถูกสั่ง "เริ่มพิมพ์" ก่อนเปลี่ยนโปรแกรม ไม่ใช่สั่งหยุด
            // ส่วนการสั่งหยุดเป็นของเครื่องที่งานนี้ไม่ได้ใช้เท่านั้น (ดู StopOneMkAsync)
            // และไม่มีการสั่งเริ่มพิมพ์ปิดท้ายอีกครั้ง
            //
            // ของเดิมสั่งหยุดก่อนแล้วปิดท้ายด้วยสั่งเริ่ม ซึ่งเครื่องหน้างานปฏิเสธทั้งคู่
            // (ER,SR,01 · ER,SQ,01) เพราะสั่งผิดจังหวะกับสภาพของเครื่อง
            //
            // คำสั่งคุมการพิมพ์ไม่ผ่านไม่ล้มทั้งการส่ง ตัวที่ตัดสินว่างานเข้าเครื่องหรือไม่
            // คือ FW / FS / F1 / FM
            var notes = new List<string>(); // เก็บคำเตือนประกอบผลส่งของหัวนี้

            // ไม่รายงานผลของคำสั่งนี้
            //
            // เครื่องชุดนี้ตอบ ER,SQ,01 ทุกครั้ง คือรู้จักคำสั่งแต่ไม่ให้สั่งเริ่มพิมพ์
            // จากระยะไกล ซึ่งเป็นสภาพปกติของมัน ไม่ใช่ความผิดพลาดของงาน โปรแกรมเดิม
            // ก็สั่งตัวนี้ทุกครั้งและไม่เคยดูคำตอบเลย งานก็พิมพ์ออกมาได้ตามปกติ
            //
            // ถ้าเครื่องเงียบไปจริง ๆ (สายหลุด) คำสั่งที่เป็นตัวงานถัดจากนี้จะฟ้องเอง
            // จึงไม่ต้องกันไว้ตรงนี้ซ้ำ
            await adapter.ResumeAsync(); // ส่ง SQ ก่อนเปลี่ยนโปรแกรม โดยโค้ดนี้ไม่ใช้ผลตอบ SQ ตัดสิน

            var fw = await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1); // ส่ง FW เลือกโปรแกรม ถ้าไม่มีเลขให้ใช้ 1
            if (!fw.Success) // เครื่องไม่รับคำสั่งเปลี่ยนโปรแกรม
                return (Reject(label, $"เปลี่ยนไปโปรแกรม {config.ProgramNumber}", fw), Note(notes)); // หยุดส่งหัวนี้พร้อมเหตุที่เปลี่ยนโปรแกรมไม่ได้

            // ส่งครบทุกช่องเสมอ ช่องที่งานนี้ไม่ได้ใช้ก็ส่งข้อความว่างไปทับ —
            // กฎเดียวกับโปรแกรมเดิม ถ้าข้ามไปเฉย ๆ ข้อความของงานก่อนหน้าจะค้าง
            // อยู่ในช่องนั้นแล้วถูกพิมพ์ติดไปกับงานใหม่
            for (int slot = 1; slot <= MkBlockCount; slot++) // ส่งครบทุกช่องข้อความ รวมช่องที่งานนี้ไม่ได้ใช้
            {
                var block = config.TextBlocks.FirstOrDefault(b => b.BlockNumber == slot) // หาข้อความของช่องปัจจุบันจาก Pattern
                    ?? new TextBlockDto { BlockNumber = slot, Text = "" }; // ไม่มีข้อความให้ส่งค่าว่างทับ เพื่อไม่เหลือข้อความงานเก่า

                await Task.Delay(MkBlockGapMs); // เว้นจังหวะก่อนส่งช่องถัดไป

                int deviceBlock = slot + MkFirstDeviceBlock - 1; // แปลงเลขช่องในงานเป็นเลขช่องที่เครื่องใช้
                var fb = await adapter.SendTextBlockAsync(block, deviceBlock); // ส่งข้อความและค่าบล็อกด้วย FS / F1
                if (!fb.Success) return (Reject(label, $"ส่ง Block {slot}", fb), Note(notes)); // เครื่องไม่รับบล็อกนี้ ให้หยุดก่อนส่งช่องอื่น
            }

            // FM ต้องมาหลัง FS/F1 ตามสเปกของเครื่อง (FW -> FS/F1 -> FM)
            // ถ้าส่ง FM ก่อน Block ทิศทางที่ตั้งไว้จะถูก Block ที่ตามมาเขียนทับ
            // ปุ่ม ABC จะกดแล้วเครื่องพิมพ์หัวตั้งเหมือนเดิม
            var fm = await adapter.SendConfigAsync(config); // ส่ง FM หลังครบทุกบล็อก เพื่อไม่ให้ค่าถูกบล็อกเขียนทับ
            if (!fm.Success) return (Reject(label, "ส่ง Config", fm), Note(notes)); // ส่งค่าพิมพ์ไม่ผ่าน ให้รายงานจุดที่หยุด

            return (null, Note(notes)); // หัวนี้รับข้อมูลครบ ให้ตัวคุม MK รวมผลต่อ
        }
        catch (Exception ex)
        {
            return ($"{label}: {ex.Message}", null); // ส่งเหตุการเชื่อมต่อหรือข้อผิดพลาดกลับไปแสดง
        }
        finally
        {
            tcp.Disconnect(); // ปิด TCP ของหัวนี้ทุกครั้งเมื่อจบรอบส่ง
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
        using var busy = MachineBusy.Hold(); // พักการอ่านสถานะเครื่องระหว่างส่งข้อมูล UV

        string stepName = uvNumber == 1 ? "UV1" : "UV2"; // ระบุว่าจะใช้ข้อมูลของ UV1 หรือ UV2
        string table = uvNumber == 1 ? "MK063" : "MK067"; // UV1 เขียน MK063 ส่วน UV2 เขียน MK067 ใน CPI.db3

        var uvName = uvNumber == 1 // อ่านชื่อเครื่อง UV สำหรับแสดงผล
            ? UvSettingsManager.Read("UV1_NAME", "UV-001") // ใช้ชื่อ UV1 ที่ตั้งไว้
            : UvSettingsManager.Read("UV2_NAME", "UV-002"); // ใช้ชื่อ UV2 ที่ตั้งไว้

        var done = new List<string>(); // เก็บรายการขั้นที่ทำไปแล้วไว้รายงาน

        var uvRow = uvData.FirstOrDefault(r => r.Machine == stepName); // หาแถวข้อมูลของ UV ที่ต้องส่ง
        if (uvRow == null) // Job ยังไม่มีข้อมูลของเครื่องนี้
            return Blocked(uvName, $"ยังไม่มีข้อมูล {stepName} ของงานที่เลือก"); // หยุดก่อนส่ง เพราะไม่มีข้อมูล UV ของงาน

        var cpiPath = UvSettingsManager.GetCpiPath(uvNumber); // หาไฟล์ CPI.db3 ตามชุดตั้งค่าของ UV
        if (cpiPath == null) // หาไฟล์ CPI ไม่พบหรือยังไม่ตั้งโฟลเดอร์
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} หรือไม่พบ CPI.db3"); // แจ้งให้แก้ตำแหน่ง CPI ก่อนส่ง

        var ip = CustomSettingsManager.Read($"UV00{uvNumber}_IP"); // อ่าน IP ของเครื่อง UV ที่เลือก
        if (string.IsNullOrWhiteSpace(ip)) // ยังไม่มี IP สำหรับติดต่อเครื่อง
            return Blocked(uvName, $"ยังไม่ได้ตั้งค่า IP ของ UV{uvNumber}"); // หยุดก่อนเริ่ม TCP และบอกให้ตั้ง IP

        int port = int.TryParse(CustomSettingsManager.Read($"UV00{uvNumber}_PORT"), out var p) // อ่านพอร์ต UV จากค่าตั้ง
            ? p // ใช้พอร์ตที่ผู้ใช้ตั้งไว้เมื่ออ่านได้
            : UvDefaultPort; // อ่านพอร์ตไม่ได้ ใช้พอร์ตมาตรฐานของระบบ

        if (!await CanConnectAsync(ip, port)) // ลองเชื่อมต่อปลายทางก่อนแก้ข้อมูล CPI
            return new UvSendResult(SendStatus.Unreachable, uvName, done, Ip: ip, Port: port); // แจ้ง IP และพอร์ตที่ติดต่อไม่ได้กลับไปยังหน้า Order List

        UvProgramPick pick; // เก็บผลเลือกโปรแกรมและการใช้โปรแกรมสำรอง
        if (string.IsNullOrWhiteSpace(forcedProgram)) // ยังไม่มีโปรแกรมที่ ST3 เลือกฝากมา
        {
            var docFolder = UvSettingsManager.GetDocumentFolder(uvNumber); // หาโฟลเดอร์เก็บไฟล์โปรแกรมของ UV นี้
            pick = UvProgramResolver.Resolve(uvRow.ProgramName, docFolder, owner); // ค้นโปรแกรมตามชื่อในงาน หรือเปิดให้เลือกรุ่นย่อย
        }
        else
        {
            // เลือกมาแล้วจากที่อื่น — ถือว่าผ่านการยืนยันของคนมาเรียบร้อย
            pick = new UvProgramPick(forcedProgram.Trim(), false); // ใช้โปรแกรมที่ผู้ขอส่งเลือกไว้แล้ว ไม่ถามเลือกซ้ำ
        }

        var programFile = pick.Program; // อ่านชื่อโปรแกรมที่ได้จากการเลือก
        if (programFile == null) // ผู้ใช้ยังไม่ได้เลือกโปรแกรมที่จะส่ง
            return new UvSendResult(SendStatus.Cancelled, uvName, done); // จบการส่งแบบยกเลิก โดยยังไม่แตะ CPI

        if (pick.IsDefault && // ได้โปรแกรมสำรองแทนชื่อเดิมในงาน
            !UvProgramResolver.ConfirmDefault(uvRow.ProgramName ?? "", uvName, owner as Control)) // ให้ผู้ใช้ยืนยันก่อนใช้โปรแกรมสำรอง
            return new UvSendResult(SendStatus.Cancelled, uvName, done); // ไม่ยืนยันโปรแกรมสำรอง จึงหยุดก่อนเขียน CPI

        try
        {
            var uvTcp = new UvTcpService(); // เตรียมชุดคำสั่ง TCP ของ UV

            // 1. หยุดเครื่องก่อนเสมอ — ไม่ตอบรับก็ไปต่อ เพราะเครื่องอาจหยุดอยู่แล้ว
            var (stopOk, _) = await uvTcp.StopAsync(ip, port); // ขอหยุด UV ก่อนเขียนข้อมูล โดยผลหยุดไม่ใช้บล็อกขั้นถัดไป
            done.Add(stopOk ? "สั่งหยุดเครื่อง" : "สั่งหยุดเครื่อง (ไม่ตอบรับ — ทำต่อ)"); // เก็บว่าคำสั่งหยุดได้รับคำตอบหรือไม่

            // 2. เขียนข้อความลง CPI.db3
            var (writeOk, writeMsg) = await CpiWriteService.WriteAsync( // เขียนข้อมูลพิมพ์ของ Job ลงฐานข้อมูล CPI
                cpiPath, table, // ระบุไฟล์และตารางของ UV ที่เลือก
                uvRow.Lot, uvRow.ErpMfg, // เขียน Lot และชื่อ ERP ของงานนี้
                uvRow.Text1, uvRow.Text2, uvRow.Text3, uvRow.Text4, uvRow.Text5); // เขียนข้อความพิมพ์ทั้งห้าช่องของงาน

            if (!writeOk) // เขียน CPI ไม่สำเร็จ
                return Stopped(uvName, done, $"เขียน CPI.db3 ({table}) — {writeMsg}"); // หยุดก่อนโหลดโปรแกรม เพื่อไม่เริ่มด้วยข้อมูลที่เขียนไม่ครบ

            done.Add($"เขียน CPI.db3 ({table})" // เก็บผลเขียน CPI ไว้ในรายงานการส่ง
                + $"\n    Lot: {Dashed(uvRow.Lot)}"
                + $"\n    Name: {Dashed(uvRow.ErpMfg)}");

            // 3. โหลดโปรแกรม แล้วสั่งเริ่มพิมพ์
            var (tcpOk, tcpLog) = await uvTcp.LoadAndStartAsync(ip, port, programFile); // ส่งคำสั่งโหลดไฟล์โปรแกรมแล้วเริ่ม UV
            if (!tcpOk) // ชุดคำสั่งโหลดและเริ่มไม่สำเร็จ
                return Stopped(uvName, done, tcpLog.Trim()); // ส่งบันทึกเหตุจาก UV กลับไปแสดง

            done.Add($"โหลดโปรแกรม {programFile}.uvdx"); // เก็บชื่อโปรแกรมที่สั่งโหลดไว้ในผลส่ง
            done.Add("สั่งเริ่มพิมพ์"); // เก็บว่าผ่านขั้นสั่งเริ่มพิมพ์แล้ว

            return new UvSendResult( // แจ้งตัวคุมคิวว่าการส่ง UV ผ่านตามเงื่อนไข Service
                SendStatus.Ok, uvName, done, // ส่งผลสำเร็จพร้อมรายการขั้นที่ทำไป
                ProgramFile: programFile, UsedDefault: pick.IsDefault, Ip: ip, Port: port); // เก็บโปรแกรมจริง การใช้สำรอง และปลายทางที่ส่ง
        }
        catch (Exception ex)
        {
            return Stopped(uvName, done, ex.Message); // รายงานข้อผิดพลาดพร้อมขั้นที่ทำสำเร็จไปก่อนหน้า
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
