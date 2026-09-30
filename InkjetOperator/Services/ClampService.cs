using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

public enum ClampSide
{
    Plate,

    Shim,
}

public sealed class ClampAxis
{
    public string Key { get; init; } = "";

    public ClampSide Side { get; init; }

    public string AxisLabel { get; init; } = "";

    public string Column { get; init; } = "";

    public string AddrTarget { get; set; } = "";
    public string AddrRun { get; set; } = "";
    public string AddrReset { get; set; } = "";
    public string AddrStatus { get; set; } = "";

    public string Display => $"{(Side == ClampSide.Plate ? "Plate" : "Shim")} {AxisLabel}";

    public bool IsConfigured =>
        AddrTarget.Trim().Length > 0 && AddrRun.Trim().Length > 0;

    public string NameColumn =>
        Side == ClampSide.Plate ? "m1_program_name" : "m2_program_name";
}

public sealed class ClampSettings
{
    public string Ip { get; set; } = "";
    public int Port { get; set; } = 5012;
    public string DbPath { get; set; } = "";

    public List<ClampAxis> Axes { get; set; } = [];

    public ClampAxis? Find(string key) =>
        Axes.FirstOrDefault(a => a.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ClampAxis> For(ClampSide side) => Axes.Where(a => a.Side == side);

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
            });
        }

        return s;
    }

    private static string ReadAddr(string key, string part)
    {
        var value = CustomSettingsManager.Read($"CLAMP_ADDR_{key}_{part}", "");
        if (value.Length > 0) return value;

        if (!key.Equals("IAI", StringComparison.OrdinalIgnoreCase)) return "";

        var legacy = CustomSettingsManager.Read($"CLAMP_ADDR_{part}", "");
        if (legacy.Length > 0) return legacy;

        return part switch
        {
            "TARGET" => "D216",
            "RUN" => "M700",
            "RESET" => "M701",
            "STATUS" => "W38",
            _ => "",
        };
    }

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
        }
    }
}

public sealed record ClampLookup(bool Found, int ValueMm, string Column, string Error);

public sealed record ClampResult(bool Ok, int ValueMm, int RawWritten, int? Status, string Log);

public static class ClampService
{
    public const int MinMm = 0;
    public const int MaxMm = 155;

    private const int RunPulseMs = 100;
    private const int ResetPulseMs = 1000;
    private const int SettleMs = 100;

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

    public static ClampLookup Lookup(string dbPath, string programName, ClampAxis axis) // ค้นระยะของแกนนี้จากไฟล์แคลมป์
    {
        string program = (programName ?? "").Trim(); // ตัดช่องว่างของชื่อโปรแกรมแคลมป์
        if (program.Length == 0) // ไม่มีชื่อโปรแกรมให้ค้น
            return new ClampLookup(false, 0, axis.Column, "ยังไม่ได้ระบุชื่อโปรแกรม"); // ระบุว่าขาดชื่อโปรแกรมที่ใช้ค้นระยะ

        if (string.IsNullOrWhiteSpace(dbPath)) // ตรวจว่ากำหนดไฟล์ฐานแคลมป์แล้วหรือยัง
            return new ClampLookup(false, 0, axis.Column, "ยังไม่ได้ตั้ง path ของ mydatabase.db3"); // ระบุว่ายังไม่มีที่อยู่ฐานแคลมป์

        if (!File.Exists(dbPath)) // ตรวจว่ามีไฟล์ฐานตามที่ตั้งไว้
            return new ClampLookup(false, 0, axis.Column, $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}"); // ระบุไฟล์ฐานแคลมป์ที่หาไม่พบ

        var columns = ReadColumns(dbPath); // อ่านรายชื่อคอลัมน์ของฐานแคลมป์
        if (columns.Count > 0 && !columns.Contains(axis.Column)) // ตรวจว่าฐานนี้รองรับแกนที่เลือก
            return new ClampLookup(false, 0, axis.Column, // เตรียมผลว่าฐานไม่มีคอลัมน์ของแกนนี้
                $"ฐานข้อมูลนี้ไม่มีคอลัมน์ {axis.Column}"); // ระบุคอลัมน์ที่ไม่มี

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath)); // เปิดฐานแคลมป์แบบอ่านอย่างเดียว
            conn.Open(); // เชื่อมต่อไฟล์ SQLite ที่เลือกไว้

            using var cmd = conn.CreateCommand(); // เตรียมคำสั่ง SQL บนฐานที่เปิดไว้
            cmd.CommandText = // กำหนด SQL สำหรับอ่านระยะของโปรแกรม
                $"SELECT {axis.Column} FROM MainTable WHERE {axis.NameColumn} = @p LIMIT 1"; // อ่านค่าแกนจาก MainTable แถวแรกที่ตรง
            cmd.Parameters.AddWithValue("@p", program); // ค้นเฉพาะชื่อโปรแกรมที่รับมา

            object? raw = cmd.ExecuteScalar(); // อ่านค่าช่องเดียวที่ค้นได้
            if (raw is null or DBNull) // ไม่พบแถวหรือค่าใน DB เป็น null
                return new ClampLookup(false, 0, axis.Column, // เตรียมผลว่าไม่พบโปรแกรมที่ค้น
                    $"ไม่พบ \"{program}\" ใน {axis.NameColumn}"); // แจ้งชื่อโปรแกรมที่ค้นไม่พบ

            string text = raw.ToString()?.Trim() ?? ""; // แปลงค่าดิบเป็นข้อความและตัดช่องว่าง
            if (text.Length == 0) // พบแถวแต่ยังไม่ได้ใส่ค่า
                return new ClampLookup(false, 0, axis.Column, $"{axis.Column} ยังไม่ได้ setup"); // ระบุว่าแกนนี้ยังไม่มีค่าระยะ

            if (!double.TryParse(text, out double value)) // ตรวจว่าค่าที่อ่านเป็นตัวเลข
                return new ClampLookup(false, 0, axis.Column, // เตรียมผลว่าค่าระยะแปลงเป็นตัวเลขไม่ได้
                    $"{axis.Column} = \"{text}\" ไม่ใช่ตัวเลข"); // บอกค่าที่แปลงเป็นตัวเลขไม่ได้

            return new ClampLookup(true, ClampMm(value), axis.Column, ""); // ส่งระยะที่อ่านได้หลังจำกัดช่วงแล้ว
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return new ClampLookup(false, 0, axis.Column, ex.Message); // ส่งเหตุที่อ่านฐานแคลมป์ไม่ได้
        }
    }

    public static (bool ok, string message) Upload( // อัปเดตระยะของโปรแกรมในฐานแคลมป์
        string dbPath, string programName, ClampAxis axis, int valueMm) // รับไฟล์ฐาน ชื่อโปรแกรม แกน และระยะใหม่
    {
        string program = (programName ?? "").Trim(); // ตัดช่องว่างของชื่อโปรแกรมแคลมป์
        if (program.Length == 0) return (false, "ยังไม่ได้ระบุชื่อโปรแกรม"); // ต้องมีชื่อโปรแกรมก่อนอัปเดตระยะ

        if (string.IsNullOrWhiteSpace(dbPath)) // ตรวจว่ากำหนดไฟล์ฐานแคลมป์แล้วหรือยัง
            return (false, "ยังไม่ได้ตั้ง path ของ mydatabase.db3"); // ระบุว่ายังไม่มีที่อยู่ฐานแคลมป์

        if (!File.Exists(dbPath)) // ตรวจว่ามีไฟล์ฐานตามที่ตั้งไว้
            return (false, $"ไม่พบไฟล์ฐานข้อมูล:\n{dbPath}"); // ระบุไฟล์ฐานแคลมป์ที่หาไม่พบ

        var columns = ReadColumns(dbPath); // อ่านรายชื่อคอลัมน์ของฐานแคลมป์
        if (columns.Count > 0 && !columns.Contains(axis.Column)) // ตรวจว่าฐานนี้รองรับแกนที่เลือก
            return (false, $"ฐานข้อมูลนี้ไม่มีคอลัมน์ {axis.Column}"); // ระบุชื่อคอลัมน์แกนที่ฐานนี้ไม่มี

        int mm = ClampMm(valueMm); // จำกัดระยะให้อยู่ในช่วงที่ตั้งไว้

        try // ดักข้อผิดพลาดของขั้นนี้
        {
            using var conn = new SqliteConnection(SqlitePath.ReadWrite(dbPath)); // เปิดฐานแคลมป์แบบเขียนแก้ไขได้
            conn.Open(); // เชื่อมต่อไฟล์ SQLite ที่เลือกไว้

            using var cmd = conn.CreateCommand(); // เตรียมคำสั่ง SQL บนฐานที่เปิดไว้
            cmd.CommandText = // กำหนดคำสั่งอัปเดตระยะ
                $"UPDATE MainTable SET {axis.Column} = @v WHERE {axis.NameColumn} = @p"; // แก้เฉพาะแถวที่ชื่อโปรแกรมตรงกัน
            cmd.Parameters.AddWithValue("@v", mm.ToString()); // ผูกระยะใหม่กับค่าที่ใช้ใน SQL
            cmd.Parameters.AddWithValue("@p", program); // ค้นเฉพาะชื่อโปรแกรมที่รับมา

            int affected = cmd.ExecuteNonQuery(); // รันคำสั่งและนับแถวที่แก้ได้

            return affected > 0 // ตรวจว่ามีแถวถูกอัปเดตจริงไหม
                ? (true, $"บันทึก {axis.Column} = {mm} ให้ \"{program}\" แล้ว") // บอกโปรแกรมและระยะที่บันทึกแล้ว
                : (false, $"ไม่พบ \"{program}\" ใน {axis.NameColumn} — ต้องสร้างรายการก่อน"); // ไม่สร้างแถวใหม่ให้ ถ้ายังไม่มีโปรแกรมนี้
        }
        catch (Exception ex) // รับรายละเอียดข้อผิดพลาดไว้แจ้งต่อ
        {
            return (false, ex.Message); // ส่งเหตุที่อัปเดตฐานแคลมป์ไม่ได้
        }
    }

    public static int ClampMm(double value) => // ปรับระยะให้อยู่ในขอบเขตที่ใช้ได้
        (int)Math.Max(MinMm, Math.Min(MaxMm, Math.Round(value))); // ปัดเป็นจำนวนเต็มแล้วจำกัดค่าต่ำสุดสูงสุด

    public static int ToRaw(int valueMm) // แปลงระยะมิลลิเมตรเป็นค่าของ PLC
    {
        int raw = (MaxMm - valueMm) * 100; // คำนวณระยะกลับด้านแล้วคูณ 100
        if (raw > 14900) raw = 14900; // จำกัดค่าสูงสุดไว้ที่ 14900
        if (raw < 0) raw = 100; // ค่าติดลบให้ใช้ 100 ตามสูตรเดิม
        return raw; // ส่งค่าที่ PLC จะรับไปใช้ต่อ
    }

    public static async Task<ClampResult> ApplyAsync(ClampSettings s, ClampAxis axis, int valueMm) // ส่งระยะและพัลส์ Run ให้แกนแคลมป์
    {
        int mm = ClampMm(valueMm); // จำกัดระยะให้อยู่ในช่วงที่ตั้งไว้
        int raw = ToRaw(mm); // แปลงระยะเป็นค่าที่ PLC รับตามสูตรของระบบเดิม
        var log = new List<string> { $"── {axis.Display} ({axis.Column}) ──" }; // เริ่ม log โดยระบุแกนที่กำลังสั่ง

        if (!axis.IsConfigured) // ตรวจว่าแกนมี Target และ Run ครบไหม
        {
            log.Add("❌ ยังไม่ได้กำหนด address ของแกนนี้"); // จดว่าแกนยังไม่มี address พร้อมใช้
            return new ClampResult(false, mm, raw, null, string.Join("\n", log)); // จบคำสั่งพร้อม log ของแกนที่ตั้งค่าไม่ครบ
        }

        var (wOk, wErr) = await McProtocolService.WriteWordAsync(s.Ip, s.Port, axis.AddrTarget, raw); // เขียนระยะลง D ของแกนนี้ก่อนสั่งวิ่ง
        log.Add($"เขียน {axis.AddrTarget} = {raw} → {(wOk ? "OK" : "❌ " + wErr)}"); // จดค่าและผลที่เขียนลง Target
        if (!wOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log)); // เขียนระยะไม่ผ่าน จึงยังไม่พัลส์ Run

        await Task.Delay(SettleMs); // รอให้ PLC รับระยะก่อนพัลส์ Run

        var (pOk, _) = await PulseAsync(s, axis.AddrRun, RunPulseMs, log); // สั่ง Run เป็นพัลส์ ไม่ค้างบิตไว้
        if (!pOk) return new ClampResult(false, mm, raw, null, string.Join("\n", log)); // พัลส์ Run ไม่ผ่าน ให้หยุดและส่ง log กลับ

        int? status = null; // ยังไม่มีค่าสถานะจนกว่าจะอ่านกลับได้
        if (axis.AddrStatus.Trim().Length > 0) // อ่านสถานะกลับเฉพาะแกนที่กำหนด address ไว้
        {
            var (rOk, value, rErr) = await McProtocolService.ReadWordAsync(s.Ip, s.Port, axis.AddrStatus); // อ่านไม่ผ่านจะอยู่ใน log แต่ไม่เปลี่ยนผลส่งก่อนหน้า
            log.Add($"อ่าน {axis.AddrStatus} → {(rOk ? value.ToString() : "❌ " + rErr)}"); // จดผลอ่านสถานะพร้อมเหตุที่อ่านไม่ได้
            if (rOk) status = value; // เก็บสถานะเฉพาะรอบที่อ่านได้จริง
        }

        return new ClampResult(true, mm, raw, status, string.Join("\n", log)); // ส่งผลคำสั่งกับสถานะและ log ให้หน้า Detail
    }

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

    public static async Task<(bool ok, int value, string error)> ReadStatusAsync(
        ClampSettings s, ClampAxis axis)
    {
        if (axis.AddrStatus.Trim().Length == 0)
            return (false, 0, "ยังไม่ได้กำหนด address อ่านสถานะของแกนนี้");

        return await McProtocolService.ReadWordAsync(s.Ip, s.Port, axis.AddrStatus);
    }

    private static async Task<(bool ok, string error)> PulseAsync( // ยกบิตแล้วลดกลับตามเวลาพัลส์
        ClampSettings s, string address, int holdMs, List<string> log) // รับปลายทาง บิต ระยะเวลาค้าง และ log
    {
        var (onOk, onErr) = await McProtocolService.WriteBitAsync(s.Ip, s.Port, address, true); // ยกบิต Run หรือ Reset ของแกนที่เลือก
        log.Add($"{address} ON → {(onOk ? "OK" : "❌ " + onErr)}"); // จดผลการยกบิตเป็น ON
        if (!onOk) return (false, onErr); // ยกบิตไม่ได้ ให้จบก่อนรอเวลาพัลส์

        await Task.Delay(holdMs); // ค้างบิตตามเวลาพัลส์ก่อนลดกลับ

        var (offOk, offErr) = await McProtocolService.WriteBitAsync(s.Ip, s.Port, address, false); // ลดบิตเพื่อให้ PLC รับพัลส์รอบถัดไป
        log.Add($"{address} OFF → {(offOk ? "OK" : "❌ " + offErr)}"); // จดผลการลดบิตกลับเป็น OFF
        return offOk ? (true, "") : (false, offErr); // ใช้ผลลดบิตเป็นผลจบของพัลส์นี้
    }
}
