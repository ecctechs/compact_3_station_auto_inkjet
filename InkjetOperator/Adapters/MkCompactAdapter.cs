using System.Globalization;
using System.Text;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Adapters;

/// <summary>
/// MK Compact inkjet adapter — formats FW/FS/F1/FM/SR/SQ commands.
/// Ported from rs232_connector.py + socket_client.py.
/// Can use either Rs232Manager (COM port) or TcpManager (TCP/IP).
/// </summary>
public class MkCompactAdapter : IInkjetAdapter
{
    private readonly Rs232Manager? _rs232;
    private readonly TcpManager? _tcp;
    private int _programNumber;

    /// <summary>
    /// Size conversion dict — from rs232_connector.py lines 8-22.
    /// Maps logical size to device encoding.
    /// </summary>
    /// <summary>ตัวคูณของช่องหน่วงทริกเกอร์ในคำสั่ง FM — เก็บเป็น มม. ส่งเป็นสิบเท่า</summary>
    private const int TriggerDelayScale = 10;

    private static readonly Dictionary<string, string> SizeConversion = new()
    {
        { "1", "0" },
        { "2", "1" },
        { "3", "17" },
        { "4", "3" },
        { "5", "18" },
        { "6", "20" },
        { "7", "5" },
        { "8", "6" },
        { "9", "7" },
        { "10", "8" },
        { "11", "9" },
        { "12", "10" },
        { "13", "11" },
    };

    /// <summary>Constructor for RS232 connection (COM port).</summary>
    public MkCompactAdapter(Rs232Manager rs232)
    {
        _rs232 = rs232;
    }

    /// <summary>Constructor for TCP connection.</summary>
    public MkCompactAdapter(TcpManager tcp)
    {
        _tcp = tcp;
    }

    public Task<bool> ConnectAsync()
    {
        // Connection is managed by the underlying manager
        return Task.FromResult(IsConnected());
    }

    public Task DisconnectAsync()
    {
        _rs232?.CloseSerialPort();
        _tcp?.Disconnect();
        return Task.CompletedTask;
    }

    public bool IsConnected()
    {
        if (_rs232 != null) return _rs232.IsOpen();
        if (_tcp != null) return _tcp.IsConnected();
        return false;
    }

    private async Task<string> SendAsync(string command)
    {
        if (_rs232 != null)
            return await _rs232.SendCommandAsync(command);
        if (_tcp != null)
            return await _tcp.SendCommandAsync(command);
        return "";
    }

    /// <summary>
    /// คำตอบที่แปลว่าเครื่องไม่รับคำสั่ง — ขึ้นต้นด้วย ER แล้วตามด้วยคำสั่งกับรหัส
    ///
    /// <para>
    /// ยิงคำสั่งที่เครื่องไม่รู้จักเข้าไปจะได้ <c>ER,test,00</c> กลับมา ซึ่งเป็นคำตอบ
    /// ที่ไม่ว่าง การนับว่า "ตอบกลับมา = สำเร็จ" จึงทำให้คำสั่งที่เครื่องปฏิเสธถูก
    /// รายงานว่าส่งสำเร็จ แล้วงานก็ไม่เข้าเครื่องโดยไม่มีอะไรบอก
    /// </para>
    /// </summary>
    private static bool IsRejected(string response) =>
        response.StartsWith("ER", StringComparison.OrdinalIgnoreCase);

    private CommandResult MakeResult(string command, string response, int? ordinal = null)
    {
        return new CommandResult
        {
            Command = command,
            Response = response,

            // ว่าง = ต่อไม่ติดหรือเครื่องเงียบ · ขึ้นต้น ER = เครื่องรับคำสั่งแล้วปฏิเสธ
            Success = response != "" && !IsRejected(response),
            SentAt = DateTime.UtcNow.ToString("o"),
            Ordinal = ordinal,
        };
    }

    /// <summary>
    /// Send SR (suspend/stop printing).
    /// From rs232_connector.py send_suspend() lines 46-51: sends "SR\r"
    /// </summary>
    public async Task<CommandResult> SuspendAsync()
    {
        string response = await SendAsync("SR\r");
        return MakeResult("suspend", response);
    }

    /// <summary>
    /// Send SQ (resume printing).
    /// From rs232_connector.py send_resume() lines 54-59: sends "SQ\r"
    /// </summary>
    public async Task<CommandResult> ResumeAsync()
    {
        string response = await SendAsync("SQ\r");
        return MakeResult("resume", response);
    }

    /// <summary>
    /// Send FW (change program/message number).
    /// From rs232_connector.py send_change_prog() lines 36-43: sends "FW,{n}\r"
    /// </summary>
    public async Task<CommandResult> ChangeProgramAsync(int programNumber)
    {
        _programNumber = programNumber;
        string response = await SendAsync($"FW,{programNumber}\r");
        return MakeResult("change_prog", response);
    }

    /// <summary>
    /// Send FS + F1 (text block content + format).
    /// From rs232_connector.py send_text() lines 62-71:
    ///   FS,{prog},{block},0,{text}\r
    ///   F1,{prog},{block},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r
    /// </summary>
    public async Task<CommandResult> SendTextBlockAsync(TextBlockDto block, int deviceBlock)
    {
        string text = block.Text ?? "";
        string x = (block.X ?? 0).ToString();
        string y = (block.Y ?? 0).ToString();
        string scale = (block.Scale ?? 1).ToString();
        string sizeKey = (block.Size ?? 1).ToString();

        string sizeConverted = SizeConversion.GetValueOrDefault(sizeKey, "0");

        // FS command — set text content
        string fsCmd = $"FS,{_programNumber},{deviceBlock},0,{text}\r";
        string fsResponse = await SendAsync(fsCmd);

        if (fsResponse == "")
        {
            return MakeResult("text_block", "", null);
        }

        // F1 command — set text format
        string f1Cmd = $"F1,{_programNumber},{deviceBlock},{scale},{sizeConverted},{x},{y},1,1,1,0,00,0\r";
        string f1Response = await SendAsync(f1Cmd);

        return MakeResult("text_block", f1Response);
    }

    // ── ทิศทางการพิมพ์ (ปุ่ม ABC) ──────────────────────────

    // ค่าที่เก็บใน inkjet_config.direction มาจากไฟล์ตั้งต้นชุดเดียวกับโปรแกรมเดิม
    // (PySocketClient) ซึ่งใช้ 1 = ปกติ · 2 = กลับหัว ส่วน 0 กับ 3 เป็นค่าที่
    // คำสั่ง FM รับ ตัวอ่านจึงรับทั้งสองชุด เพราะข้อมูลที่มีอยู่ยังไม่ได้ยืนยัน
    // ว่าถูกบันทึกด้วยชุดไหน แต่ตอนส่งออกจะเป็น 0/3 เสมอตามที่เครื่องรู้จัก
    public const int DirectionNormal = 1;
    public const int DirectionFlipped = 2;

    /// <summary>ค่าไหนที่แปลว่ากลับหัว — 2 (ไฟล์ตั้งต้น) หรือ 3 (คำสั่ง FM)</summary>
    public static bool IsFlipped(int? direction) => direction == 2 || direction == 3;

    /// <summary>
    /// Send FM (program/message configuration).
    /// Read the program's current line speed with FL first, then preserve it in FM.
    /// Program name is Unicode-normalized (NFKD) like Python unicodedata.normalize.
    /// </summary>
    public async Task<CommandResult> SendConfigAsync(InkjetConfigDto config)
    {
        string progName = config.ProgramName ?? "";
        // Normalize like Python: unicodedata.normalize('NFKD', ch)
        string normalizedName = progName.Normalize(NormalizationForm.FormKD);

        string direction = IsFlipped(config.Direction) ? "3" : "0";

        // ค่าที่เก็บเป็นมิลลิเมตร แต่ช่องนี้ของคำสั่ง FM รับเป็นหน่วยสิบเท่า
        // จึงต้องคูณ 10 ก่อนส่ง ตรงกับโปรแกรมเดิมที่ทำ int(float(delay) * 10)
        // ลืมคูณแล้วหมึกจะลงเร็วกว่าที่ตั้งไว้สิบเท่า คือผิดตำแหน่งบนชิ้นงานจริง
        //
        // ช่องที่งานไม่ได้กรอก (null) ใช้ค่าที่บันทึกอยู่ในโปรแกรมของเครื่อง (ตกลง 30/10/2026)
        // เดิมฟ้องให้ไปกรอกก่อน — งานเก่าจำนวนมากไม่ได้กรอก แต่โปรแกรมใน MK ตั้งค่าไว้แล้ว
        string? delay = config.TriggerDelay is int d ? (d * TriggerDelayScale).ToString() : null;
        string? height = config.Height?.ToString();
        string? width = config.Width?.ToString();
        UsedMachineValues = [];
        DefaultedValues = [];

        // FL,<program>,0 ส่งค่าที่บันทึกอยู่ของโปรแกรมนี้กลับมา โดย Line Speed คือช่อง j
        // อ่านก่อน FM เพื่อคงค่าเดิมของโปรแกรม และไม่เขียน -1/500 ทับลงไป
        UsedDefaultLineSpeed = false;
        string flResponse = await SendAsync($"FL,{_programNumber},0\r");
        bool saved = TryReadProgramLineSpeed(flResponse, _programNumber, out string lineSpeed);

        // เครื่องเงียบ = ไม่รู้ว่าเกิดอะไร ไม่เดาต่อ
        if (!saved && string.IsNullOrWhiteSpace(flResponse))
            return MakeResult("config", "ER,FL,NO_RESPONSE");

        // ช่องของคำตอบ FL เรียงเหมือนคำสั่ง FM (ยืนยันจากคำตอบจริงของเครื่อง
        // FL,017,0,,-1,0,-1,-1,-1,-1,-1,00300,00300,01000,100,100,0015,000,0,-1)
        // โปรแกรมที่ยังไม่ได้ Save เครื่องก็ตอบช่องพวกนี้มา (เป็นค่าเริ่มต้นของเครื่อง) ใช้ได้เหมือนกัน
        string[] fields = flResponse.TrimEnd('\r', '\n').Split(',');
        bool usable = IsFlOf(fields, _programNumber);
        delay ??= FromMachine(fields, usable, saved, FlTriggerDelay, "Trigger Delay", DefaultTriggerDelay);
        height ??= FromMachine(fields, usable, saved, FlHeight, "Height", DefaultHeight);
        width ??= FromMachine(fields, usable, saved, FlWidth, "Width", DefaultWidth);

        if (UsedMachineValues.Count > 0)
            AppLog.Info("MK.Config", $"โปรแกรม No. {_programNumber} ใช้ค่าในเครื่อง: {string.Join(", ", UsedMachineValues)}");

        if (saved)
        {
            if (DefaultedValues.Count > 0)
                AppLog.Warn("MK.Config", $"โปรแกรม No. {_programNumber} ไม่มีค่า {string.Join(", ", DefaultedValues)} ใช้ค่าเริ่มต้น · FL ตอบ: {flResponse.Trim()}");
            return MakeResult("config", await SendAsync(BuildConfigCommand(normalizedName, direction, delay, height, width, lineSpeed)));
        }

        // เครื่องตอบแต่อ่าน Line Speed ไม่ได้ = ยังไม่มีโปรแกรมเลขนี้ในเครื่อง (ยังไม่ได้ Save ที่ MK)
        //
        // เดิมล้มทั้งการส่ง ข้อความเป็นรหัส ER,FL,INVALID_RESPONSE คนหน้างานไม่รู้ว่าต้องทำอะไร
        // ตกลง 30/10/2026: ส่งต่อด้วยค่าที่โปรแกรมใช้มาก่อนอ่าน FL ได้ (-1 แล้ว 500 ถ้าเครื่องไม่รับ -1)
        // ช่องที่งานไม่ได้กรอกใช้ค่าเริ่มต้นแบบเดียวกัน · FM สร้าง / บันทึกโปรแกรมเลขนี้ให้เอง
        // ผู้เรียกดู UsedDefaultLineSpeed / DefaultedValues แล้วเตือนให้ไป Save ที่ MK
        AppLog.Warn("MK.Config", $"อ่าน Line Speed โปรแกรม No. {_programNumber} ไม่ได้ · FL ตอบ: {flResponse.Trim()} · ส่งด้วยค่าเริ่มต้น");
        UsedDefaultLineSpeed = true;
        string response = await SendAsync(BuildConfigCommand(normalizedName, direction, delay, height, width, "-1"));

        // Firmware บางเครื่องไม่รับ -1 ในช่อง Line Speed (j) และตอบ ER,FM,22 — ส่งซ้ำด้วย 500
        if (IsInvalidFmParameter(response))
            response = await SendAsync(BuildConfigCommand(normalizedName, direction, delay, height, width, "500"));

        return MakeResult("config", response);
    }

    /// <summary>
    /// ส่ง Config ครั้งล่าสุดใช้ Line Speed ค่าเริ่มต้น เพราะอ่านค่าของโปรแกรมในเครื่องไม่ได้
    /// (ยังไม่มีโปรแกรมเลขนี้ใน MK) — ส่งผ่าน แต่ต้องเตือนให้ไป Save โปรแกรมที่เครื่อง
    /// </summary>
    public bool UsedDefaultLineSpeed { get; private set; }

    /// <summary>ช่องที่งานไม่ได้กรอก แล้วใช้ค่าที่บันทึกอยู่ในโปรแกรมของเครื่องแทน (ส่ง Config ครั้งล่าสุด)</summary>
    public List<string> UsedMachineValues { get; private set; } = [];

    /// <summary>
    /// ช่องที่งานไม่ได้กรอก และในเครื่องไม่มีค่าที่บันทึกไว้ (ยังไม่ได้ Save โปรแกรมนี้ / ค่าไม่ใช่ตัวเลข)
    /// — ใช้ค่าเริ่มต้นแทน ส่งผ่าน แต่ผู้เรียกต้องเตือน (แบบเดียวกับ Line Speed)
    /// </summary>
    public List<string> DefaultedValues { get; private set; } = [];

    // ตำแหน่งช่องในคำตอบ FL — เรียงเหมือนคำสั่ง FM (BuildConfigCommand)
    private const int FlTriggerDelay = 11;
    private const int FlHeight = 14;
    private const int FlWidth = 15;

    // ค่าเริ่มต้นเมื่อเครื่องไม่มีค่าให้ใช้ — ชุดเดียวกับที่ MK ตอบมาสำหรับโปรแกรมที่ยังไม่ได้ Save
    // (Delay 00300 · Height 100 · Width 100) Delay เป็นหน่วยของคำสั่ง FM แล้ว (สิบเท่าของ มม.)
    private const string DefaultTriggerDelay = "300";
    private const string DefaultHeight = "100";
    private const string DefaultWidth = "100";

    /// <summary>คำตอบ FL ของโปรแกรมเลขนี้จริงไหม — รวมโปรแกรมที่ยังไม่ได้ Save ซึ่งเครื่องตอบค่าเริ่มต้นมา</summary>
    private static bool IsFlOf(string[] fields, int expectedProgram) =>
        fields.Length > FlWidth
        && fields[0].Equals("FL", StringComparison.OrdinalIgnoreCase)
        && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int program)
        && program == expectedProgram;

    /// <summary>
    /// ค่าของช่องนี้จากเครื่อง — โปรแกรมที่ Save แล้ว = ค่าเดิมของโปรแกรม (UsedMachineValues)
    /// ยังไม่ได้ Save / อ่านไม่ได้ = ค่าเริ่มต้น (DefaultedValues) ส่งผ่านทุกกรณี
    /// </summary>
    private string FromMachine(string[] fields, bool usable, bool saved, int index, string name, string fallback)
    {
        if (usable && int.TryParse(fields[index].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int value))
        {
            (saved ? UsedMachineValues : DefaultedValues).Add(name);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        DefaultedValues.Add(name);
        return fallback;
    }

    private static bool IsInvalidFmParameter(string response)
    {
        string[] fields = response.TrimEnd('\r', '\n').Split(',');
        return fields.Length >= 3
            && fields[0].Equals("ER", StringComparison.OrdinalIgnoreCase)
            && fields[1].Equals("FM", StringComparison.OrdinalIgnoreCase)
            && fields[2].Equals("22", StringComparison.Ordinal);
    }

    private static bool TryReadProgramLineSpeed(string response, int expectedProgram, out string lineSpeed)
    {
        lineSpeed = "";
        string[] fields = response.TrimEnd('\r', '\n').Split(',');

        // FL response: FL,program,selector,name,...,i,j,k,... ; j (Line Speed) is field 10.
        if (fields.Length <= 10
            || !fields[0].Equals("FL", StringComparison.OrdinalIgnoreCase)
            || !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int returnedProgram)
            || returnedProgram != expectedProgram
            || fields[2] != "0"
            || !int.TryParse(fields[10].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            return false;
        }

        // Keep the device's original zero-padded representation, e.g. 03000.
        lineSpeed = fields[10].Trim();
        return true;
    }

    private string BuildConfigCommand(
        string normalizedName, string direction, string delay,
        string height, string width, string lineSpeed) =>
        $"FM,{_programNumber},0,{normalizedName},0,{direction},0,0,01,20,{lineSpeed},{delay},300,1000,{height},{width},15,0,0,6\r";

}
