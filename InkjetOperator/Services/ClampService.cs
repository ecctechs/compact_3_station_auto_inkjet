using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

/// <summary>ค่าตั้งของ PLC แคลมป์ — อ่านจาก Setting.config</summary>
public sealed class ClampSettings
{
    public string Ip { get; set; } = "";
    public int Port { get; set; } = 5012;
    public string DbPath { get; set; } = "";

    public string AddrTarget { get; set; } = "D216";
    public string AddrRun { get; set; } = "M700";
    public string AddrReset { get; set; } = "M701";
    public string AddrStatus { get; set; } = "W38";

    public static ClampSettings Load() => new()
    {
        Ip = CustomSettingsManager.Read("CLAMP_PLC_IP", ""),
        Port = int.TryParse(CustomSettingsManager.Read("CLAMP_PLC_PORT", "5012"), out int p) ? p : 5012,
        DbPath = CustomSettingsManager.Read("CLAMP_DB_PATH", ""),
        AddrTarget = CustomSettingsManager.Read("CLAMP_ADDR_TARGET", "D216"),
        AddrRun = CustomSettingsManager.Read("CLAMP_ADDR_RUN", "M700"),
        AddrReset = CustomSettingsManager.Read("CLAMP_ADDR_RESET", "M701"),
        AddrStatus = CustomSettingsManager.Read("CLAMP_ADDR_STATUS", "W38"),
    };

    public void Save()
    {
        CustomSettingsManager.Write("CLAMP_PLC_IP", Ip.Trim());
        CustomSettingsManager.Write("CLAMP_PLC_PORT", Port.ToString());
        CustomSettingsManager.Write("CLAMP_DB_PATH", DbPath.Trim());
        CustomSettingsManager.Write("CLAMP_ADDR_TARGET", AddrTarget.Trim());
        CustomSettingsManager.Write("CLAMP_ADDR_RUN", AddrRun.Trim());
        CustomSettingsManager.Write("CLAMP_ADDR_RESET", AddrReset.Trim());
        CustomSettingsManager.Write("CLAMP_ADDR_STATUS", AddrStatus.Trim());
    }
}

/// <summary>ผลการค้นค่าแคลมป์จากฐานข้อมูล</summary>
public sealed record ClampLookup(bool Found, int ValueMm, string Column, string Error);

/// <summary>ผลการสั่งแคลมป์</summary>
public sealed record ClampResult(bool Ok, int ValueMm, int RawWritten, int? Status, string Log);

/// <summary>
/// ควบคุมแคลมป์ผ่าน PLC (MC Protocol)
///
/// ลำดับตรงตามระบบ Node-RED เดิม:
///   หา IAI จากชื่อโปรแกรม → จำกัดช่วง 0–155 mm → แปลงเป็นค่า raw
///   → เขียน D216 → พัลส์ M700 (ON 100ms OFF) → อ่าน W38
///
/// ยังไม่ผูกกับหน้า Order — เรียกจากหน้าแคลมป์แยกไปก่อน
/// </summary>
public static class ClampService
{
    public const int MinMm = 0;
    public const int MaxMm = 155;

    private const int RunPulseMs = 100;
    private const int ResetPulseMs = 1000;
    private const int SettleMs = 100;

    // ── อ่านค่าแคลมป์จากฐานข้อมูล ──────────────────────────

    /// <summary>
    /// หาค่า IAI ของโปรแกรมจากตาราง MainTable
    /// ชื่อขึ้นต้น "P-" ใช้คู่ m1_program_name / IAIP ที่เหลือใช้ m2_program_name / IAI
    /// </summary>
    public static ClampLookup Lookup(string dbPath, string programName)
    {
        string program = (programName ?? "").Trim();
        if (program.Length == 0)
            return new ClampLookup(false, 0, "", "ยังไม่ได้ระบุชื่อโปรแกรม");

        if (string.IsNullOrWhiteSpace(dbPath))
            return new ClampLookup(false, 0, "", "ยังไม่ได้ตั้ง path ของ mydatabase.db3");

        if (!File.Exists(dbPath))
            return new ClampLookup(false, 0, "", $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}");

        bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase);
        string nameColumn = isPlate ? "m1_program_name" : "m2_program_name";
        string valueColumn = isPlate ? "IAIP" : "IAI";

        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $"SELECT {valueColumn} FROM MainTable WHERE {nameColumn} = @p LIMIT 1";
            cmd.Parameters.AddWithValue("@p", program);

            object? raw = cmd.ExecuteScalar();
            if (raw is null || raw is DBNull)
                return new ClampLookup(false, 0, valueColumn,
                    $"ไม่พบโปรแกรม \"{program}\" ใน MainTable");

            // เก็บเป็น TEXT ในฐานข้อมูล ค่าว่างแปลว่ายังไม่ได้ setup
            string text = raw.ToString()?.Trim() ?? "";
            if (text.Length == 0)
                return new ClampLookup(false, 0, valueColumn,
                    $"โปรแกรม \"{program}\" ยังไม่ได้ setup ({valueColumn} ว่าง)");

            if (!double.TryParse(text, out double value))
                return new ClampLookup(false, 0, valueColumn,
                    $"{valueColumn} = \"{text}\" ไม่ใช่ตัวเลข");

            return new ClampLookup(true, ClampMm(value), valueColumn, "");
        }
        catch (Exception ex)
        {
            return new ClampLookup(false, 0, valueColumn, ex.Message);
        }
    }

    /// <summary>
    /// บันทึกระยะที่ปรับแล้วกลับลง MainTable (ปุ่ม Upload ของระบบเดิม)
    ///
    /// เป็น UPDATE ล้วน ไม่มี INSERT — โปรแกรมที่ยังไม่มีแถวต้องสร้างด้วย "Save as Model" ก่อน
    /// จึงคืนข้อความบอกชัดเมื่อไม่มีแถวถูกแก้ แทนที่จะเงียบเหมือนของเดิม
    /// </summary>
    public static (bool ok, string message) Upload(string dbPath, string programName, int valueMm, string? valueColumnOverride = null)
    {
        string program = (programName ?? "").Trim();
        if (program.Length == 0)
            return (false, "ยังไม่ได้ระบุชื่อโปรแกรม");

        if (string.IsNullOrWhiteSpace(dbPath))
            return (false, "ยังไม่ได้ตั้ง path ของ mydatabase.db3");

        if (!File.Exists(dbPath))
            return (false, $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}");

        bool isPlate = program.StartsWith("P-", StringComparison.OrdinalIgnoreCase);
        string nameColumn = isPlate ? "m1_program_name" : "m2_program_name";
        string valueColumn = valueColumnOverride ?? (isPlate ? "IAIP" : "IAI");
        int mm = ClampMm(valueMm);

        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $"UPDATE MainTable SET {valueColumn} = @v WHERE {nameColumn} = @p";
            cmd.Parameters.AddWithValue("@v", mm.ToString());
            cmd.Parameters.AddWithValue("@p", program);

            int affected = cmd.ExecuteNonQuery();

            return affected > 0
                ? (true, $"บันทึก {valueColumn} = {mm} ให้ \"{program}\" แล้ว")
                : (false, $"ไม่พบโปรแกรม \"{program}\" ใน {nameColumn}\n" +
                          "ต้องสร้างรายการก่อนจึงจะบันทึกได้");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── การแปลงค่า ─────────────────────────────────────────

    /// <summary>จำกัดระยะให้อยู่ในช่วงที่แคลมป์รับได้ (0–155 mm)</summary>
    public static int ClampMm(double value) =>
        (int)Math.Max(MinMm, Math.Min(MaxMm, Math.Round(value)));

    /// <summary>
    /// แปลงระยะ mm เป็นค่า raw ที่เขียนลง PLC — ห้ามแก้สูตรเอง ค่านี้กำหนดตำแหน่งแคลมป์จริงบนไลน์
    ///
    /// ลอกจาก action "send" ของ Clamp UI router (ปุ่ม Sent ที่ operator กดจริง):
    ///     Math.round((149 - (next - 6)) * 100)  ==  (155 - mm) * 100
    ///     &gt; 14900 → 14900 · &lt; 0 → 100
    ///
    /// ระวัง: ในระบบเดิมมีสูตรที่สองอยู่ใน function 31 (เส้นทางอัตโนมัติหลังโหลดค่าจาก SQL)
    /// ใช้ (149 - mm) * 100 และตัดที่ 13500 ซึ่งให้ผลต่างกัน 600 (= 6 mm)
    /// ที่เลือกตัวนี้เพราะเป็นเส้นทางที่ operator ใช้จริง และช่วงผลลัพธ์ 0–15500
    /// สอดคล้องกับ input ที่จำกัดไว้ 0–155
    /// </summary>
    public static int ToRaw(int valueMm)
    {
        int raw = (MaxMm - valueMm) * 100;
        if (raw > 14900) raw = 14900;
        if (raw < 0) raw = 100;
        return raw;
    }

    // ── สั่งงาน PLC ────────────────────────────────────────

    /// <summary>เขียนค่าเป้าหมายแล้วพัลส์สั่งวิ่ง จากนั้นอ่านสถานะกลับ</summary>
    public static async Task<ClampResult> ApplyAsync(ClampSettings s, int valueMm)
    {
        int mm = ClampMm(valueMm);
        int raw = ToRaw(mm);
        var log = new List<string>();

        var (wOk, wErr) = await McProtocolService.WriteWordAsync(s.Ip, s.Port, s.AddrTarget, raw);
        log.Add($"เขียน {s.AddrTarget} = {raw} → {(wOk ? "OK" : "❌ " + wErr)}");
        if (!wOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        // Node-RED หน่วง 100ms ตรงนี้ก่อนพัลส์ — ให้ PLC รับค่าเข้า D ก่อนเห็นขอบขาขึ้นของ M
        await Task.Delay(SettleMs);

        var (pOk, pErr) = await PulseAsync(s, s.AddrRun, RunPulseMs, log);
        if (!pOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        var (rOk, status, rErr) = await McProtocolService.ReadWordAsync(s.Ip, s.Port, s.AddrStatus);
        log.Add($"อ่าน {s.AddrStatus} → {(rOk ? status.ToString() : "❌ " + rErr)}");

        return new ClampResult(true, mm, raw, rOk ? status : null, string.Join("\n", log));
    }

    /// <summary>พัลส์รีเซ็ต — หน่วงนานกว่าพัลส์สั่งวิ่ง</summary>
    public static async Task<(bool ok, string log)> ResetAsync(ClampSettings s)
    {
        var log = new List<string>();
        var (ok, _) = await PulseAsync(s, s.AddrReset, ResetPulseMs, log);
        return (ok, string.Join("\n", log));
    }

    /// <summary>อ่านสถานะอย่างเดียว ไม่สั่งอะไร</summary>
    public static Task<(bool ok, int value, string error)> ReadStatusAsync(ClampSettings s) =>
        McProtocolService.ReadWordAsync(s.Ip, s.Port, s.AddrStatus);

    /// <summary>
    /// พัลส์ ON → หน่วง → OFF
    /// ต้องส่ง OFF เสมอ ค้าง ON ไว้ PLC จะไม่รับคำสั่งรอบถัดไป
    /// </summary>
    private static async Task<(bool ok, string error)> PulseAsync(
        ClampSettings s, string address, int holdMs, List<string> log)
    {
        var (onOk, onErr) = await McProtocolService.WriteBitAsync(s.Ip, s.Port, address, true);
        log.Add($"{address} ON → {(onOk ? "OK" : "❌ " + onErr)}");
        if (!onOk) return (false, onErr);

        await Task.Delay(holdMs);

        var (offOk, offErr) = await McProtocolService.WriteBitAsync(s.Ip, s.Port, address, false);
        log.Add($"{address} OFF → {(offOk ? "OK" : "❌ " + offErr)}");
        return offOk ? (true, "") : (false, offErr);
    }
}
