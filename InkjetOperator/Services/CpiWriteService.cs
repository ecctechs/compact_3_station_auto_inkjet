using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

public static class CpiWriteService
{
    public static async Task<(bool ok, string msg)> WriteAsync(
        string dbPath, string table, string? lot, string? name,
        string? text1, string? text2, string? text3, string? text4, string? text5)
    {
        try
        {
            var connPath = dbPath.StartsWith(@"\\")
                ? dbPath.Replace(@"\", "/")
                : dbPath;

            var connStr = $"Data Source={connPath};Mode=ReadWrite";
            await using var conn = new SqliteConnection(connStr);
            await conn.OpenAsync();

            await using var pragma = conn.CreateCommand();
            pragma.CommandText = "PRAGMA journal_mode=Off; PRAGMA busy_timeout=5000;";
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
        catch (Exception ex)
        {
            return (false, $"เขียน CPI.db3 ไม่สำเร็จ: {ex.Message}");
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
            var connPath = dbPath.StartsWith(@"\\") ? dbPath.Replace(@"\", "/") : dbPath;

            await using var conn = new SqliteConnection($"Data Source={connPath};Mode=ReadOnly");
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
