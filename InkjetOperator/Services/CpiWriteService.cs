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
}
