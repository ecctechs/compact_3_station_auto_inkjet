using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

/// <summary>
/// เขียนข้อความลง CPI.db3 ของเครื่อง UV
///
/// <para>
/// ไฟล์นี้เสี่ยงกว่าไฟล์อื่นในระบบสามข้อพร้อมกัน: อยู่บนแชร์ของเครื่องอื่น ·
/// เป็นไฟล์ที่เรา "เขียน" ไม่ใช่แค่อ่าน · และซอฟต์แวร์ของเครื่อง UV ก็เปิดไฟล์
/// เดียวกันนี้อยู่ตลอดเวลา การล็อกไฟล์ของ SQLite พึ่งกลไกของระบบไฟล์ ซึ่งบน SMB
/// เชื่อถือได้ไม่เต็มร้อย และเน็ตสะดุดกลางการเขียนทำให้ไฟล์เสียได้จริง
/// </para>
/// <para>
/// จึงกันไว้สี่ชั้น: ถามก่อนว่าเครื่องปลายทางยังอยู่ไหม · สำรองไฟล์ลงเครื่องตัวเอง
/// ก่อนเขียนทุกครั้ง · เปิด rollback journal ให้ SQLite ย้อนกลับได้ถ้าเขียนค้าง ·
/// และลองซ้ำเมื่อโดนล็อกหรือ I/O สะดุด
/// </para>
/// </summary>
public static class CpiWriteService
{
    /// <summary>จำนวนครั้งที่ลองเขียนเมื่อเจอปัญหาชั่วคราว</summary>
    private const int MaxAttempts = 3;

    /// <summary>จำนวนไฟล์สำรองที่เก็บไว้ต่อหนึ่งตาราง</summary>
    private const int KeepBackups = 20;

    /// <summary>ที่เก็บไฟล์สำรอง — บนเครื่องตัวเองเสมอ ไม่ใช่บนแชร์ที่กำลังจะเขียนทับ</summary>
    public static string BackupFolder =>
        Path.Combine(AppSettingsFile.Folder, "backup", "CPI");

    public static async Task<(bool ok, string msg)> WriteAsync(
        string dbPath, string table, string? lot, string? name,
        string? text1, string? text2, string? text3, string? text4, string? text5)
    {
        // เครื่องที่เก็บไฟล์ไม่ตอบก็จบตรงนี้ ไม่ต้องไปค้างรอ SMB 20-60 วินาที
        // กลางการส่งงานจริง เหตุผลเต็ม ๆ อยู่ที่ PathProbe
        if (PathProbe.HostOf(dbPath) is string host && !await PathProbe.HostUpAsync(host))
            return (false, $"เขียน CPI.db3 ไม่สำเร็จ: เครื่อง {host} ไม่ตอบ");

        var backupNote = await BackupAsync(dbPath, table);

        // ลองแบบมี journal ก่อนเสมอ เพราะเป็นทางเดียวที่ SQLite ย้อนกลับได้ถ้าเขียน
        // ค้างกลางคัน แต่ journal ต้องสร้างไฟล์ข้าง ๆ ฐานข้อมูลได้ ถ้าโฟลเดอร์บนแชร์
        // ไม่ให้สร้าง ค่อยถอยไปแบบเดิมที่ไม่มี journal — ดีกว่าเขียนไม่ได้เลย
        var result = await TryWriteAsync(dbPath, table, journalOff: false,
            lot, name, text1, text2, text3, text4, text5);

        if (!result.ok && LooksLikeJournalBlocked(result.msg))
        {
            result = await TryWriteAsync(dbPath, table, journalOff: true,
                lot, name, text1, text2, text3, text4, text5);
            if (result.ok) result.msg += " (ไม่มี journal)";
        }

        return (result.ok, result.msg + backupNote);
    }

    private static async Task<(bool ok, string msg)> TryWriteAsync(
        string dbPath, string table, bool journalOff, string? lot, string? name,
        string? text1, string? text2, string? text3, string? text4, string? text5)
    {
        for (int attempt = 1; ; attempt++)
        {
        try
        {
            await using var conn = new SqliteConnection(SqlitePath.ReadWrite(dbPath));
            await conn.OpenAsync();

            await using var pragma = conn.CreateCommand();
            pragma.CommandText = journalOff
                ? "PRAGMA journal_mode=Off; PRAGMA busy_timeout=5000;"
                : "PRAGMA busy_timeout=5000;";
            await pragma.ExecuteNonQueryAsync();

            var existingCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var info = conn.CreateCommand();
            info.CommandText = $"PRAGMA table_info({table})";
            await using var reader = await info.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                existingCols.Add(reader.GetString(1));

            var sets = new List<string>();
            var cmd = conn.CreateCommand();

            void AddCol(string col, string? val)
            {
                if (!existingCols.Contains(col)) return;
                sets.Add($"{col} = @{col}");
                cmd.Parameters.AddWithValue($"@{col}", (object?)val ?? DBNull.Value);
            }

            AddCol("lot", lot);
            AddCol("name", name);
            AddCol("text1", text1);
            AddCol("text2", text2);
            AddCol("text3", text3);
            AddCol("text4", text4);
            AddCol("text5", text5);

            if (sets.Count == 0)
                return (false, $"ตาราง {table} ไม่มีคอลัมน์ที่ตรงกัน");

            cmd.CommandText = $"UPDATE {table} SET {string.Join(", ", sets)} WHERE id = 1";
            var affected = await cmd.ExecuteNonQueryAsync();

            return affected > 0
                ? (true, $"เขียน CPI.db3 ({table}) สำเร็จ")
                : (false, $"ไม่พบแถว id=1 ในตาราง {table}");
        }
        catch (Exception ex) when (attempt < MaxAttempts && IsTransient(ex))
        {
            // โดนล็อกจากซอฟต์แวร์ UV หรือ I/O บนแชร์สะดุดชั่วครู่ — ถอยแล้วลองใหม่
            await Task.Delay(attempt * 300);
        }
        catch (Exception ex)
        {
            return (false, $"เขียน CPI.db3 ไม่สำเร็จ: {ex.Message}");
        }
        }
    }

    /// <summary>ปัญหาชั่วคราวที่ลองใหม่แล้วมีโอกาสผ่าน — ไฟล์ถูกล็อกอยู่ หรือ I/O สะดุด</summary>
    private static bool IsTransient(Exception ex) => ex switch
    {
        SqliteException s => s.SqliteErrorCode is 5 or 6 or 10,   // BUSY / LOCKED / IOERR
        IOException => true,
        _ => false,
    };

    /// <summary>สร้างไฟล์ journal ข้าง ๆ ฐานข้อมูลไม่ได้ — มักเป็นเรื่องสิทธิ์บนแชร์</summary>
    private static bool LooksLikeJournalBlocked(string msg) =>
        msg.Contains("readonly", StringComparison.OrdinalIgnoreCase)
        || msg.Contains("read-only", StringComparison.OrdinalIgnoreCase)
        || msg.Contains("unable to open database file", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// ก๊อปไฟล์เก็บไว้บนเครื่องตัวเองก่อนเขียนทับ — คืนข้อความต่อท้ายผลลัพธ์
    ///
    /// <para>
    /// CPI.db3 มีขนาดหลักสิบกิโลไบต์ ก๊อปทุกครั้งที่ส่งงานจึงแทบไม่มีราคา แต่เป็น
    /// ทางเดียวที่กู้กลับได้จริงถ้าไฟล์บนแชร์เสีย
    /// </para>
    /// <para>
    /// สำรองไม่สำเร็จไม่ขวางการเขียน งานต้องเดินต่อได้ แค่บอกไว้ในข้อความผลลัพธ์
    /// ว่ารอบนี้ไม่มีตัวสำรอง
    /// </para>
    /// </summary>
    private static async Task<string> BackupAsync(string dbPath, string table)
    {
        try
        {
            var folder = Path.Combine(BackupFolder, table);

            // ต้องบังคับ InvariantCulture — Windows ภาษาไทยให้ปีพุทธศักราช ชื่อไฟล์
            // จะกลายเป็น 2569 แทน 2026 ซึ่งอ่านยากตอนต้องไล่หาไฟล์มากู้
            var stamp = DateTime.Now.ToString(
                "yyyyMMdd-HHmmss-fff", System.Globalization.CultureInfo.InvariantCulture);

            await Task.Run(() =>
            {
                Directory.CreateDirectory(folder);
                File.Copy(dbPath, Path.Combine(folder, $"CPI-{stamp}.db3"), overwrite: false);

                // เก็บเท่าที่จำเป็น ตัวเก่าสุดหลุดออกไปเรื่อย ๆ ชื่อไฟล์เรียงตามเวลาอยู่แล้ว
                foreach (var old in new DirectoryInfo(folder)
                             .GetFiles("CPI-*.db3")
                             .OrderByDescending(f => f.Name)
                             .Skip(KeepBackups))
                {
                    try { old.Delete(); } catch { /* ลบไม่ได้ก็ปล่อยไว้ รอบหน้าค่อยว่ากัน */ }
                }
            });

            return "";
        }
        catch (Exception ex)
        {
            return $" (สำรองไฟล์ไม่สำเร็จ: {ex.Message})";
        }
    }

    /// <summary>ค่าที่อยู่ในแถว id=1 ของตาราง CPI ตอนนี้</summary>
    public sealed record CpiRow(
        string? Lot, string? Name,
        string? Text1, string? Text2, string? Text3, string? Text4, string? Text5);

    /// <summary>
    /// อ่านค่าปัจจุบันจาก CPI.db3 — ใช้ตอนทดสอบเพื่อดูว่าตอนนี้เครื่องถืออะไรอยู่ก่อนเขียนทับ
    /// เปิดแบบ ReadOnly เพราะซอฟต์แวร์ UV อาจเปิดไฟล์ค้างอยู่
    /// </summary>
    public static async Task<(bool ok, CpiRow? row, string msg)> ReadAsync(string dbPath, string table)
    {
        try
        {
            await using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
            await conn.OpenAsync();

            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var info = conn.CreateCommand())
            {
                info.CommandText = $"PRAGMA table_info({table})";
                await using var r = await info.ExecuteReaderAsync();
                while (await r.ReadAsync()) cols.Add(r.GetString(1));
            }

            if (cols.Count == 0)
                return (false, null, $"ไม่พบตาราง {table} ในไฟล์");

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {table} WHERE id = 1 LIMIT 1";
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return (false, null, $"ไม่พบแถว id=1 ในตาราง {table}");

            string? Get(string col)
            {
                if (!cols.Contains(col)) return null;
                int i = reader.GetOrdinal(col);
                return reader.IsDBNull(i) ? null : reader.GetValue(i)?.ToString();
            }

            var row = new CpiRow(
                Get("lot"), Get("name"),
                Get("text1"), Get("text2"), Get("text3"), Get("text4"), Get("text5"));

            return (true, row, $"อ่าน CPI.db3 ({table}) สำเร็จ");
        }
        catch (Exception ex)
        {
            return (false, null, $"อ่าน CPI.db3 ไม่สำเร็จ: {ex.Message}");
        }
    }
}
