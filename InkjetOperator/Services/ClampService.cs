using InkjetOperator.Models;
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
    /// ค่าตั้งต้นของสูตรแปลงระยะ: raw = (Offset − mm) × 100 — ดู <see cref="ToRaw"/>
    ///
    /// <para>
    /// แต่ละแกนไม่เท่ากัน วัดหน้างาน 1/11/2026 (ตำแหน่งบน HMI เทียบความสูงที่วัดจริง):
    /// แกน X ใช้ 155 ตามสูตรเดิมของ Node-RED · Z1 = 70 · Z2 = 90
    /// ให้หน้างานกรอกเองในหน้า Clamp Setting (คุณภาสกรเสนอ) — เปลี่ยนชุดแคลมป์แล้วไม่ต้องแก้โปรแกรม
    /// </para>
    /// </summary>
    public int OffsetMm { get; set; } = ClampService.DefaultOffsetMm;

    // ── ค่าตั้งตามเอกสาร address ของ PLC (ลูกค้าส่งมา 2/11/2026) ──────────────
    // เอกสารบอกสูตร (True Value), ชนิดข้อมูล (Type) และพัลส์ของแต่ละแกน ไม่เท่ากันทุกแกน
    // เช่นแกน X ใช้ (149 − (X − 6)) × 100 แต่แกน Z ใช้ X × 100 — ให้หน้างานกรอกตามเอกสารเอง
    // เปลี่ยนชุดแคลมป์ / PLC แล้วไม่ต้องแก้โปรแกรม · ค่าเริ่มต้นทุกตัว = พฤติกรรมเดิมก่อนมีช่องเหล่านี้

    /// <summary>true = raw = (Offset − mm) × ตัวคูณ (กลับทิศ เหมือนเดิม) · false = (Offset + mm) × ตัวคูณ</summary>
    public bool Reverse { get; set; } = true;

    /// <summary>ตัวคูณในสูตร — เอกสารเขียน "× 100" ตำแหน่งบน HMI ทศนิยม 2 หลัก</summary>
    public int Scale { get; set; } = ClampService.DefaultScale;

    /// <summary>Type ในเอกสาร — true = INT 32 เขียน / อ่าน 2 word ติดกัน · false = INT 16 word เดียว (เดิม)</summary>
    public bool Int32 { get; set; }

    /// <summary>ค่า raw ต่ำสุด / สูงสุดที่ยอมเขียน — นอกช่วงปัดเข้าขอบ (เดิมล็อก 100 / 14900)</summary>
    public int MinRaw { get; set; } = ClampService.DefaultMinRaw;
    public int MaxRaw { get; set; } = ClampService.DefaultMaxRaw;

    /// <summary>ตำแหน่งบน HMI ที่ถอยไปตอนคิว UV ว่าง — ดู <see cref="ClampIdlePark"/> · เขียน raw = ค่านี้ × ตัวคูณ</summary>
    public int ParkPosition { get; set; }

    /// <summary>เวลาพัลส์ Start / Reset (ms) — Unit "Pulse" ในเอกสาร</summary>
    public int RunPulseMs { get; set; } = ClampService.DefaultRunPulseMs;
    public int ResetPulseMs { get; set; } = ClampService.DefaultResetPulseMs;

    /// <summary>แปลงระยะ mm เป็นค่าที่เขียนลง PLC ตามสูตรของแกนนี้ แล้วปัดเข้าช่วงที่ยอม</summary>
    public int ToRaw(int valueMm)
    {
        long raw = (long)(Reverse ? OffsetMm - valueMm : OffsetMm + valueMm) * Scale;
        // ไม่ใช้ Math.Clamp — ระหว่างพิมพ์ในตารางค่าต่ำสุดอาจมากกว่าสูงสุดชั่วคราว (Validate กันตอน Save)
        if (raw > MaxRaw) raw = MaxRaw;
        if (raw < MinRaw) raw = MinRaw;
        return (int)raw;
    }

    /// <summary>ค่าที่เขียนตอนถอยไปตำแหน่งพัก — ไม่ปัดเข้าช่วง ตำแหน่งพักตั้งตรง ๆ บน HMI</summary>
    public int ParkRaw => ParkPosition * Scale;

    /// <summary>สูตรแบบอ่านง่าย ไว้เทียบกับคอลัมน์ True Value ในเอกสาร เช่น "(155 − X) × 100"</summary>
    public string Formula
    {
        get
        {
            string inner = Reverse ? $"({OffsetMm} − X)"
                : OffsetMm == 0 ? "X"
                : OffsetMm > 0 ? $"({OffsetMm} + X)"
                : $"(X − {-OffsetMm})";
            return Scale == 1 ? inner : $"{inner} × {Scale}";
        }
    }

    /// <summary>ตรวจค่าตั้งที่ต้องดูหลายช่องพร้อมกัน — คืน null = ใช้ได้</summary>
    public string? Validate()
    {
        if (!ClampService.IsValidOffset(OffsetMm))
            return $"Offset ต้องอยู่ระหว่าง {ClampService.MinOffsetMm} ถึง {ClampService.MaxOffsetMm}";
        if (Scale < 1 || Scale > ClampService.MaxScale)
            return $"ตัวคูณต้องอยู่ระหว่าง 1 ถึง {ClampService.MaxScale}";
        if (MinRaw > MaxRaw)
            return $"ค่าต่ำสุด ({MinRaw}) มากกว่าค่าสูงสุด ({MaxRaw})";
        if (!Int32 && (MinRaw < short.MinValue || MaxRaw > short.MaxValue))
            return $"INT 16 รับได้ {short.MinValue} ถึง {short.MaxValue} — ค่าเกินนี้ต้องเลือก INT 32";
        long park = (long)ParkPosition * Scale;
        long parkMin = Int32 ? int.MinValue : short.MinValue;
        long parkMax = Int32 ? int.MaxValue : short.MaxValue;
        if (park < parkMin || park > parkMax)
            return $"ตำแหน่งพัก {ParkPosition} × {Scale} เกินช่วงของ INT {(Int32 ? 32 : 16)}";
        if (!ClampService.IsValidPulse(RunPulseMs) || !ClampService.IsValidPulse(ResetPulseMs))
            return $"เวลาพัลส์ต้องอยู่ระหว่าง {ClampService.MinPulseMs} ถึง {ClampService.MaxPulseMs} ms";
        return null;
    }

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
                Reverse = CustomSettingsManager.Read($"CLAMP_DIR_{key}", "-").Trim() != "+",
                Scale = ReadInt($"CLAMP_SCALE_{key}", ClampService.DefaultScale, 1, ClampService.MaxScale),
                Int32 = CustomSettingsManager.Read($"CLAMP_TYPE_{key}", "16").Trim() == "32",
                MinRaw = ReadInt($"CLAMP_RAWMIN_{key}", ClampService.DefaultMinRaw, int.MinValue, int.MaxValue),
                MaxRaw = ReadInt($"CLAMP_RAWMAX_{key}", ClampService.DefaultMaxRaw, int.MinValue, int.MaxValue),
                ParkPosition = ReadInt($"CLAMP_PARKPOS_{key}", 0, int.MinValue, int.MaxValue),
                RunPulseMs = ReadInt($"CLAMP_PULSE_RUN_{key}", ClampService.DefaultRunPulseMs,
                    ClampService.MinPulseMs, ClampService.MaxPulseMs),
                ResetPulseMs = ReadInt($"CLAMP_PULSE_RESET_{key}", ClampService.DefaultResetPulseMs,
                    ClampService.MinPulseMs, ClampService.MaxPulseMs),
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

    /// <summary>ยังไม่เคยตั้ง / อ่านไม่ออก / นอกช่วง → ค่าเริ่มต้น (= พฤติกรรมเดิม)</summary>
    private static int ReadInt(string key, int fallback, int min, int max) =>
        int.TryParse(CustomSettingsManager.Read(key, "").Trim(), out int v) && v >= min && v <= max
            ? v
            : fallback;

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
            CustomSettingsManager.Write($"CLAMP_DIR_{a.Key}", a.Reverse ? "-" : "+");
            CustomSettingsManager.Write($"CLAMP_SCALE_{a.Key}", a.Scale.ToString());
            CustomSettingsManager.Write($"CLAMP_TYPE_{a.Key}", a.Int32 ? "32" : "16");
            CustomSettingsManager.Write($"CLAMP_RAWMIN_{a.Key}", a.MinRaw.ToString());
            CustomSettingsManager.Write($"CLAMP_RAWMAX_{a.Key}", a.MaxRaw.ToString());
            CustomSettingsManager.Write($"CLAMP_PARKPOS_{a.Key}", a.ParkPosition.ToString());
            CustomSettingsManager.Write($"CLAMP_PULSE_RUN_{a.Key}", a.RunPulseMs.ToString());
            CustomSettingsManager.Write($"CLAMP_PULSE_RESET_{a.Key}", a.ResetPulseMs.ToString());
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

    /// <summary>
    /// ติดลบได้ — สูตรแบบ "(X − 6) × 100" คือ Offset −6 ไม่กลับทิศ · 0 = "X × 100" ของแกน Z ในเอกสาร
    /// </summary>
    public const int MinOffsetMm = -999;
    public const int MaxOffsetMm = 999;

    public static bool IsValidOffset(int offsetMm) =>
        offsetMm >= MinOffsetMm && offsetMm <= MaxOffsetMm;

    // ค่าเริ่มต้นของช่องตั้งค่าตามเอกสาร PLC — ตรงกับค่าที่เคยล็อกในโค้ด ลงโปรแกรมใหม่แล้วเครื่องวิ่งเหมือนเดิม
    public const int DefaultScale = 100;
    public const int MaxScale = 10000;
    public const int DefaultMinRaw = 100;
    public const int DefaultMaxRaw = 14900;
    public const int DefaultRunPulseMs = 100;
    public const int DefaultResetPulseMs = 1000;
    public const int MinPulseMs = 10;
    public const int MaxPulseMs = 10000;

    public static bool IsValidPulse(int ms) => ms >= MinPulseMs && ms <= MaxPulseMs;

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

    // สูตรแปลง mm → raw อยู่ที่ ClampAxis.ToRaw — ค่าเริ่มต้นตรงกับ action "send" ของ Clamp UI router
    // (Node-RED เดิม): Math.round((149 - (next - 6)) * 100) == (155 - mm) * 100 ปัดเข้า 100–14900
    // ส่วนที่ต่างกันต่อแกน (Offset, ทิศ, ตัวคูณ, เพดาน) หน้างานกรอกตามเอกสาร PLC ในหน้า Clamp Setting

    // ── สั่งงาน PLC ────────────────────────────────────────

    /// <summary>เขียนค่าเป้าหมายของแกนหนึ่ง แล้วพัลส์สั่งวิ่ง จากนั้นอ่านสถานะกลับ</summary>
    public static Task<ClampResult> ApplyAsync(ClampSettings s, ClampAxis axis, int valueMm)
    {
        int mm = ClampMm(valueMm);
        return SendRawAsync(s, axis, mm, axis.ToRaw(mm), $"── {axis.Display} ({axis.Column}) ──");
    }

    // ── ส่งค่าของงานตอนส่ง UV ──────────────────────────────

    /// <summary>
    /// ฝั่งแคลมป์ของขั้น UV — UV1 = Plate · UV2 = Shim · ขั้นอื่น (MK) ไม่มีแคลมป์
    /// <para>
    /// ไม่ตีความรหัส marking เอง ขั้น UV1 / UV2 มาจาก MarkingMethodService ที่เดียว
    /// (01 → UV1 · 10 → UV2 · 11 → ทั้งคู่ · 12 → UV2 · 02 / 20 / 22 ไม่มี UV)
    /// </para>
    /// </summary>
    public static ClampSide? SideOfStep(string? step) => (step ?? "").Trim().ToUpperInvariant() switch
    {
        "UV1" => ClampSide.Plate,
        "UV2" => ClampSide.Shim,
        _ => null,
    };

    /// <summary>ระยะ (mm) ของแกนนี้ในแถวระยะแคลมป์ของงาน — null = ยังไม่มีค่า ซึ่งไม่ใช่ 0</summary>
    public static int? JobValue(IaiClampSettingDto iai, ClampAxis axis) => axis.Key switch
    {
        "IAIP" => iai.Iaip,
        "IAIPZ1" => iai.IaipZ1,
        "IAIPZ2" => iai.IaipZ2,
        "IAI" => iai.Iai,
        "IAIZ1" => iai.IaiZ1,
        "IAIZ2" => iai.IaiZ2,
        _ => null,
    };

    /// <summary>เหตุที่แกนนี้ส่งไม่ผ่าน — บรรทัด ❌ บรรทัดสุดท้ายใน log เช่นต่อ PLC ไม่ได้</summary>
    public static string FailReason(ClampResult r) =>
        r.Log.Split('\n').LastOrDefault(l => l.Contains('❌'))?.Replace("❌ ", "").Replace("❌", "").Trim() ?? "";

    /// <summary>
    /// ถอยทุกแกนของฝั่งนี้ไปตำแหน่งพัก — เขียนตำแหน่ง HMI ตรง ๆ ไม่ผ่านสูตร Offset
    /// (<see cref="ClampAxis.ParkPosition"/> ค่าเริ่มต้น 0) แกนที่ยังไม่ได้ตั้ง address ข้ามไป ไม่นับว่าพลาด
    /// </summary>
    public static async Task<List<ClampResult>> ParkAsync(ClampSettings s, ClampSide side)
    {
        var results = new List<ClampResult>();
        foreach (var axis in s.For(side).Where(a => a.IsConfigured))
        {
            // ระยะ mm ที่ตรงกับตำแหน่งพัก (ย้อนสูตร) — เก็บไว้ใน ValueMm ให้ log อ่านเข้าใจ
            int mm = axis.Reverse ? axis.OffsetMm - axis.ParkPosition : axis.ParkPosition - axis.OffsetMm;
            results.Add(await SendRawAsync(s, axis, mm, axis.ParkRaw,
                $"── {axis.Display} ({axis.Column}) กลับตำแหน่ง {axis.ParkPosition} ──"));
        }
        return results;
    }

    private static async Task<ClampResult> SendRawAsync(ClampSettings s, ClampAxis axis, int mm, int raw, string header)
    {
        var log = new List<string> { header };

        if (!axis.IsConfigured)
        {
            log.Add("❌ ยังไม่ได้กำหนด address ของแกนนี้");
            return new ClampResult(false, mm, raw, null, string.Join("\n", log));
        }

        var (wOk, wErr) = axis.Int32
            ? await McProtocolService.WriteDWordAsync(s.Ip, s.Port, axis.AddrTarget, raw)
            : await McProtocolService.WriteWordAsync(s.Ip, s.Port, axis.AddrTarget, raw);
        log.Add($"เขียน {axis.AddrTarget} = {raw}{(axis.Int32 ? " (INT 32)" : "")} → {(wOk ? "OK" : "❌ " + wErr)}");
        if (!wOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        // Node-RED หน่วงตรงนี้ก่อนพัลส์ — ให้ PLC รับค่าเข้า D ก่อนเห็นขอบขาขึ้นของ M
        await Task.Delay(SettleMs);

        var (pOk, _) = await PulseAsync(s, axis.AddrRun, axis.RunPulseMs, log);
        if (!pOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log));

        int? status = null;
        if (axis.AddrStatus.Trim().Length > 0)
        {
            var (rOk, value, rErr) = await ReadStatusAsync(s, axis);
            log.Add($"อ่าน {axis.AddrStatus} → {(rOk ? StatusText(axis, value) : "❌ " + rErr)}");
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

        var (ok, _) = await PulseAsync(s, axis.AddrReset, axis.ResetPulseMs, log);
        return (ok, string.Join("\n", log));
    }

    /// <summary>อ่านสถานะของแกนหนึ่ง ไม่สั่งอะไร — INT 32 อ่าน 2 word ตามชนิดของแกน</summary>
    public static async Task<(bool ok, int value, string error)> ReadStatusAsync(
        ClampSettings s, ClampAxis axis)
    {
        if (axis.AddrStatus.Trim().Length == 0)
            return (false, 0, "ยังไม่ได้กำหนด address อ่านสถานะของแกนนี้");

        return axis.Int32
            ? await McProtocolService.ReadDWordAsync(s.Ip, s.Port, axis.AddrStatus)
            : await McProtocolService.ReadWordAsync(s.Ip, s.Port, axis.AddrStatus);
    }

    /// <summary>ค่าที่อ่านได้พร้อมตำแหน่งบน HMI (÷ ตัวคูณ) — เอกสารเขียน Position Real Value = X / 100</summary>
    public static string StatusText(ClampAxis axis, int value) =>
        axis.Scale == 1 ? value.ToString() : $"{value} ({(double)value / axis.Scale:0.##})";

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
