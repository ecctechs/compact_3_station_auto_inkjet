using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

/// <summary>ฝั่งวัสดุ — กำหนดว่าใช้คอลัมน์ชื่อโปรแกรมตัวไหนใน MainTable</summary>
public enum ClampSide
{
    /// <summary>Plate — m1_program_name (ชื่อขึ้นต้น "P-") · UV1 / MK063 · ชุด IAI 2</summary>
    Plate,

    /// <summary>Shim — m2_program_name · UV2 / MK067 · ชุด IAI 1</summary>
    Shim,
}

/// <summary>
/// แกนแคลมป์ 1 ตัว — จากแผนภาพหน้างานมี 6 แกน (Plate/Shim × X/Z1/Z2)
/// ทุกแกนอยู่บน PLC ตัวเดียวกัน ต่างกันแค่ register
/// </summary>
public sealed class ClampAxis
{
    /// <summary>คีย์ที่ใช้เก็บใน config เช่น "IAIP", "IAIZ1"</summary>
    public string Key { get; init; } = "";

    public ClampSide Side { get; init; }

    /// <summary>"X" / "Z1" / "Z2"</summary>
    public string AxisLabel { get; init; } = "";

    /// <summary>คอลัมน์ค่าใน MainTable</summary>
    public string Column { get; init; } = "";

    public string AddrTarget { get; set; } = "";
    public string AddrRun { get; set; } = "";
    public string AddrReset { get; set; } = "";
    public string AddrStatus { get; set; } = "";

    /// <summary>
    /// ค่าตั้งต้นของสูตรแปลงระยะ: raw = (Offset − mm) × 100 — ดู <see cref="ClampService.ToRaw"/>
    ///
    /// <para>
    /// แต่ละแกนไม่เท่ากัน วัดหน้างาน 1/11/2026 (ตำแหน่งบน HMI เทียบความสูงที่วัดจริง):
    /// แกน X ใช้ 155 ตามสูตรเดิมของ Node-RED · Z1 = 70 · Z2 = 90
    /// ให้หน้างานกรอกเองในหน้า Clamp Setting (คุณภาสกรเสนอ) — เปลี่ยนชุดแคลมป์แล้วไม่ต้องแก้โปรแกรม
    /// </para>
    /// </summary>
    public int OffsetMm { get; set; } = ClampService.DefaultOffsetMm;

    /// <summary>ชื่อที่โชว์ให้คน เช่น "Plate X"</summary>
    public string Display => $"{(Side == ClampSide.Plate ? "Plate" : "Shim")} {AxisLabel}";

    /// <summary>
    /// พร้อมสั่งงานไหม — ต้องมีทั้ง register ค่าเป้าหมายและพัลส์สั่งวิ่ง
    /// แผนภาพยังเขียน DXXX/MXXX ไว้ 5 แกน = ยังไม่ได้กำหนด จึงต้องกันไม่ให้ยิงคำสั่ง
    /// </summary>
    public bool IsConfigured =>
        AddrTarget.Trim().Length > 0 && AddrRun.Trim().Length > 0;

    public string NameColumn =>
        Side == ClampSide.Plate ? "m1_program_name" : "m2_program_name";
}

/// <summary>ค่าตั้งของ PLC แคลมป์ — PLC ตัวเดียว คุม 6 แกน</summary>
public sealed class ClampSettings
{
    public string Ip { get; set; } = "";
    public int Port { get; set; } = 5012;
    public string DbPath { get; set; } = "";

    public List<ClampAxis> Axes { get; set; } = [];

    public ClampAxis? Find(string key) =>
        Axes.FirstOrDefault(a => a.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ClampAxis> For(ClampSide side) => Axes.Where(a => a.Side == side);

    /// <summary>
    /// นิยามแกนทั้งหมดตามแผนภาพ — ค่า address เริ่มต้นใส่เฉพาะแกนที่รู้จริง
    /// อีก 5 แกนปล่อยว่างจนกว่าจะได้ address จากลูกค้า (ว่าง = ปุ่มถูกปิด)
    /// </summary>
    private static readonly (string Key, ClampSide Side, string Axis, string Column)[] Layout =
    [
        ("IAIP",   ClampSide.Plate, "X",  "IAIP"),
        ("IAIPZ1", ClampSide.Plate, "Z1", "IAIPZ1"),
        ("IAIPZ2", ClampSide.Plate, "Z2", "IAIPZ2"),
        ("IAI",    ClampSide.Shim,  "X",  "IAI"),
        ("IAIZ1",  ClampSide.Shim,  "Z1", "IAIZ1"),
        ("IAIZ2",  ClampSide.Shim,  "Z2", "IAIZ2"),
    ];

    public static ClampSettings Load()
    {
        var s = new ClampSettings
        {
            Ip = CustomSettingsManager.Read("CLAMP_PLC_IP", ""),
            Port = int.TryParse(CustomSettingsManager.Read("CLAMP_PLC_PORT", "5012"), out int p) ? p : 5012,
            DbPath = CustomSettingsManager.Read("CLAMP_DB_PATH", ""),
        };

        foreach (var (key, side, axis, column) in Layout)
        {
            s.Axes.Add(new ClampAxis
            {
                Key = key,
                Side = side,
                AxisLabel = axis,
                Column = column,
                AddrTarget = ReadAddr(key, "TARGET"),
                AddrRun = ReadAddr(key, "RUN"),
                AddrReset = ReadAddr(key, "RESET"),
                AddrStatus = ReadAddr(key, "STATUS"),
                OffsetMm = ReadOffset(key),
            });
        }

        return s;
    }

    /// <summary>
    /// อ่าน address ของแกน — ถ้ายังไม่มีคีย์ใหม่ ให้ตกไปใช้คีย์เดิมของแกน Shim X
    /// (เวอร์ชันก่อนรองรับแกนเดียว เก็บไว้ที่ CLAMP_ADDR_TARGET ฯลฯ)
    /// </summary>
    private static string ReadAddr(string key, string part)
    {
        var value = CustomSettingsManager.Read($"CLAMP_ADDR_{key}_{part}", "");
        if (value.Length > 0) return value;

        if (!key.Equals("IAI", StringComparison.OrdinalIgnoreCase)) return "";

        var legacy = CustomSettingsManager.Read($"CLAMP_ADDR_{part}", "");
        if (legacy.Length > 0) return legacy;

        // ค่าที่ระบบเดิมใช้จริง — มีแค่แกนนี้แกนเดียวที่ยืนยันแล้ว
        return part switch
        {
            "TARGET" => "D216",
            "RUN" => "M700",
            "RESET" => "M701",
            "STATUS" => "W38",
            _ => "",
        };
    }

    /// <summary>
    /// ยังไม่เคยตั้ง / ค่าเสีย → 155 ค่าเดิมที่ทุกแกนใช้มาก่อนมีช่องนี้
    /// ลงโปรแกรมใหม่แล้วเครื่องจึงยังวิ่งเหมือนเดิม จนกว่าหน้างานจะกรอกค่าของแกน Z
    /// </summary>
    private static int ReadOffset(string key) =>
        int.TryParse(CustomSettingsManager.Read($"CLAMP_OFFSET_{key}", "").Trim(), out int v)
        && ClampService.IsValidOffset(v)
            ? v
            : ClampService.DefaultOffsetMm;

    public void Save()
    {
        CustomSettingsManager.Write("CLAMP_PLC_IP", Ip.Trim());
        CustomSettingsManager.Write("CLAMP_PLC_PORT", Port.ToString());
        CustomSettingsManager.Write("CLAMP_DB_PATH", DbPath.Trim());

        foreach (var a in Axes)
        {
            CustomSettingsManager.Write($"CLAMP_ADDR_{a.Key}_TARGET", a.AddrTarget.Trim());
            CustomSettingsManager.Write($"CLAMP_ADDR_{a.Key}_RUN", a.AddrRun.Trim());
            CustomSettingsManager.Write($"CLAMP_ADDR_{a.Key}_RESET", a.AddrReset.Trim());
            CustomSettingsManager.Write($"CLAMP_ADDR_{a.Key}_STATUS", a.AddrStatus.Trim());
            CustomSettingsManager.Write($"CLAMP_OFFSET_{a.Key}", a.OffsetMm.ToString());
        }
    }
}

/// <summary>ผลการค้นค่าแคลมป์ของแกนหนึ่ง</summary>
public sealed record ClampLookup(bool Found, int ValueMm, string Column, string Error);

/// <summary>ผลการสั่งแคลมป์ของแกนหนึ่ง</summary>
public sealed record ClampResult(bool Ok, int ValueMm, int RawWritten, int? Status, string Log);

/// <summary>
/// ควบคุมแคลมป์ผ่าน PLC (MC Protocol)
///
/// จากแผนภาพหน้างาน: PLC ตัวเดียว (10.10.100.100:5012) คุม 6 แกน
///   Plate → IAIP / IAIPZ1 / IAIPZ2   (คู่กับ m1_program_name)
///   Shim  → IAI  / IAIZ1  / IAIZ2    (คู่กับ m2_program_name)
///
/// ลำดับต่อแกนตรงตาม Node-RED เดิม:
///   เขียนค่าเป้าหมาย → หน่วง 100ms → พัลส์สั่งวิ่ง (ON 100ms OFF) → อ่านสถานะ
/// </summary>
public static class ClampService
{
    public const int MinMm = 0;
    public const int MaxMm = 155;

    /// <summary>Offset เดิมที่ใช้กับทุกแกนก่อนมีช่องกรอก — มาจากสูตรของ Node-RED</summary>
    public const int DefaultOffsetMm = 155;

    public const int MinOffsetMm = 1;
    public const int MaxOffsetMm = 999;

    public static bool IsValidOffset(int offsetMm) =>
        offsetMm >= MinOffsetMm && offsetMm <= MaxOffsetMm;

    private const int RunPulseMs = 100;
    private const int ResetPulseMs = 1000;
    private const int SettleMs = 100;

    // ── อ่านค่าจากฐานข้อมูล ────────────────────────────────

    /// <summary>คอลัมน์ที่มีจริงใน MainTable — schema แต่ละเครื่องไม่เท่ากัน (ตัวเก่าไม่มีคอลัมน์ Z)</summary>
    public static HashSet<string> ReadColumns(string dbPath)
    {
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath)) return cols;

        try
        {
            using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA table_info(MainTable)";
            using var r = cmd.ExecuteReader();
            while (r.Read()) cols.Add(r.GetString(1));
        }
        catch { /* คืนเซ็ตว่าง = ถือว่าไม่รู้ schema */ }

        return cols;
    }

    /// <summary>หาค่าของแกนหนึ่งจาก MainTable</summary>
    public static ClampLookup Lookup(string dbPath, string programName, ClampAxis axis)
    {
        string program = (programName ?? "").Trim();
        if (program.Length == 0)
            return new ClampLookup(false, 0, axis.Column, "ยังไม่ได้ระบุชื่อโปรแกรม");

        if (string.IsNullOrWhiteSpace(dbPath))
            return new ClampLookup(false, 0, axis.Column, "ยังไม่ได้ตั้ง path ของ mydatabase.db3");

        if (!File.Exists(dbPath))
            return new ClampLookup(false, 0, axis.Column, $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}");

        var columns = ReadColumns(dbPath);
        if (columns.Count > 0 && !columns.Contains(axis.Column))
            return new ClampLookup(false, 0, axis.Column,
                $"ฐานข้อมูลนี้ไม่มีคอลัมน์ {axis.Column}");

        try
        {
            using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $"SELECT {axis.Column} FROM MainTable WHERE {axis.NameColumn} = @p LIMIT 1";
            cmd.Parameters.AddWithValue("@p", program);

            object? raw = cmd.ExecuteScalar();
            if (raw is null or DBNull)
                return new ClampLookup(false, 0, axis.Column,
                    $"ไม่พบ \"{program}\" ใน {axis.NameColumn}");

            // เก็บเป็น TEXT ค่าว่างแปลว่ายังไม่ได้ setup
            string text = raw.ToString()?.Trim() ?? "";
            if (text.Length == 0)
                return new ClampLookup(false, 0, axis.Column, $"{axis.Column} ยังไม่ได้ setup");

            if (!double.TryParse(text, out double value))
                return new ClampLookup(false, 0, axis.Column,
                    $"{axis.Column} = \"{text}\" ไม่ใช่ตัวเลข");

            return new ClampLookup(true, ClampMm(value), axis.Column, "");
        }
        catch (Exception ex)
        {
            return new ClampLookup(false, 0, axis.Column, ex.Message);
        }
    }

    /// <summary>บันทึกค่าของแกนหนึ่งกลับลง MainTable — UPDATE ล้วน ไม่สร้างแถวใหม่</summary>
    public static (bool ok, string message) Upload(
        string dbPath, string programName, ClampAxis axis, int valueMm)
    {
        string program = (programName ?? "").Trim();
        if (program.Length == 0) return (false, "ยังไม่ได้ระบุชื่อโปรแกรม");

        if (string.IsNullOrWhiteSpace(dbPath))
            return (false, "ยังไม่ได้ตั้ง path ของ mydatabase.db3");

        if (!File.Exists(dbPath))
            return (false, $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}");

        var columns = ReadColumns(dbPath);
        if (columns.Count > 0 && !columns.Contains(axis.Column))
            return (false, $"ฐานข้อมูลนี้ไม่มีคอลัมน์ {axis.Column}");

        int mm = ClampMm(valueMm);

        try
        {
            using var conn = new SqliteConnection(SqlitePath.ReadWrite(dbPath));
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $"UPDATE MainTable SET {axis.Column} = @v WHERE {axis.NameColumn} = @p";
            cmd.Parameters.AddWithValue("@v", mm.ToString());
            cmd.Parameters.AddWithValue("@p", program);

            int affected = cmd.ExecuteNonQuery();

            return affected > 0
                ? (true, $"บันทึก {axis.Column} = {mm} ให้ \"{program}\" แล้ว")
                : (false, $"ไม่พบ \"{program}\" ใน {axis.NameColumn} — ต้องสร้างรายการก่อน");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── การแปลงค่า ─────────────────────────────────────────

    public static int ClampMm(double value) =>
        (int)Math.Max(MinMm, Math.Min(MaxMm, Math.Round(value)));

    /// <summary>
    /// แปลงระยะ mm เป็นค่า raw ที่เขียนลง PLC — ห้ามแก้สูตรเอง
    ///
    /// ลอกจาก action "send" ของ Clamp UI router (ปุ่ม Sent ที่ operator กดจริง):
    ///     Math.round((149 - (next - 6)) * 100)  ==  (155 - mm) * 100
    ///
    /// 155 ใช้ได้แค่แกน X — แกน Z วัดหน้างานแล้วได้ Z1 = 70, Z2 = 90 จึงรับ Offset
    /// ของแต่ละแกนเข้ามาแทน (<see cref="ClampAxis.OffsetMm"/>) ส่วน × 100 และเพดานคงเดิม
    /// </summary>
    public static int ToRaw(int valueMm, int offsetMm)
    {
        int raw = (offsetMm - valueMm) * 100;
        if (raw > 14900) raw = 14900;
        if (raw < 0) raw = 100;
        return raw;
    }

    // ── สั่งงาน PLC ────────────────────────────────────────

    /// <summary>เขียนค่าเป้าหมายของแกนหนึ่ง แล้วพัลส์สั่งวิ่ง จากนั้นอ่านสถานะกลับ</summary>
    public static async Task<ClampResult> ApplyAsync(ClampSettings s, ClampAxis axis, int valueMm)
    {
        int mm = ClampMm(valueMm);
        int raw = ToRaw(mm, axis.OffsetMm);
        var log = new List<string> { $"── {axis.Display} ({axis.Column}) ──" };

        if (!axis.IsConfigured)
        {
            log.Add("❌ ยังไม่ได้กำหนด address ของแกนนี้");
            return new ClampResult(false, mm, raw, null, string.Join("\n", log));
        }

        var (wOk, wErr) = await McProtocolService.WriteWordAsync(s.Ip, s.Port, axis.AddrTarget, raw);
        log.Add($"เขียน {axis.AddrTarget} = {raw} → {(wOk ? "OK" : "❌ " + wErr)}");
        if (!wOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        // Node-RED หน่วงตรงนี้ก่อนพัลส์ — ให้ PLC รับค่าเข้า D ก่อนเห็นขอบขาขึ้นของ M
        await Task.Delay(SettleMs);

        var (pOk, _) = await PulseAsync(s, axis.AddrRun, RunPulseMs, log);
        if (!pOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        int? status = null;
        if (axis.AddrStatus.Trim().Length > 0)
        {
            var (rOk, value, rErr) = await McProtocolService.ReadWordAsync(s.Ip, s.Port, axis.AddrStatus);
            log.Add($"อ่าน {axis.AddrStatus} → {(rOk ? value.ToString() : "❌ " + rErr)}");
            if (rOk) status = value;
        }

        return new ClampResult(true, mm, raw, status, string.Join("\n", log));
    }

    /// <summary>พัลส์รีเซ็ตของแกนหนึ่ง — หน่วงนานกว่าพัลส์สั่งวิ่ง</summary>
    public static async Task<(bool ok, string log)> ResetAsync(ClampSettings s, ClampAxis axis)
    {
        var log = new List<string> { $"── {axis.Display} reset ──" };

        if (axis.AddrReset.Trim().Length == 0)
        {
            log.Add("❌ ยังไม่ได้กำหนด address รีเซ็ตของแกนนี้");
            return (false, string.Join("\n", log));
        }

        var (ok, _) = await PulseAsync(s, axis.AddrReset, ResetPulseMs, log);
        return (ok, string.Join("\n", log));
    }

    /// <summary>อ่านสถานะของแกนหนึ่ง ไม่สั่งอะไร</summary>
    public static async Task<(bool ok, int value, string error)> ReadStatusAsync(
        ClampSettings s, ClampAxis axis)
    {
        if (axis.AddrStatus.Trim().Length == 0)
            return (false, 0, "ยังไม่ได้กำหนด address อ่านสถานะของแกนนี้");

        return await McProtocolService.ReadWordAsync(s.Ip, s.Port, axis.AddrStatus);
    }

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
