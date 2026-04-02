using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InkjetOperator.Models;

namespace InkjetOperator.Services
{
    public class SqliteDataService
    {
        private readonly string _dbPath;

        public SqliteDataService()
        {
            _dbPath = CustomSettingsManager.GetValue("DB_PATH") ?? "";
        }

        public async Task<PatternDetail> GetPatternDetailAsync(string patternNo)
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    // แทนที่จะเด้ง ให้ Log หรือแจ้งเตือนแทน
                    Console.WriteLine($"Database file not found at: {_dbPath}");
                    return null;
                }

                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                await conn.OpenAsync();

                // ตรวจสอบว่ามี Table นี้อยู่จริงไหมก่อน Query (Optional แต่ปลอดภัยมาก)
                using var checkCmd = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='config_data';", conn);
                var tableExists = await checkCmd.ExecuteScalarAsync();

                if (tableExists == null)
                {
                    Console.WriteLine("Error: Table 'config_data' missing in database.");
                    return null;
                }

                using var cmd = new SQLiteCommand("SELECT * FROM config_data WHERE pattern_no_erp = @p LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@p", patternNo);

                using var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                return new PatternDetail
                {
                    Barcode = GetStr(reader, "pattern_no_erp") ?? patternNo,
                    Description = GetStr(reader, "model_plan_code") ?? GetStr(reader, "program_name") ?? "",
                    InkjetConfigs = new List<InkjetConfigDto>
            {
                BuildMk(reader, "mk1_", 1, "program_name"),
                BuildMk(reader, "mk2_", 2, "program_name3")
            }
                };
            }
            catch (SQLiteException ex)
            {
                // เมื่อเกิด SQL Error โปรแกรมจะวิ่งมาที่นี่แทนการเด้งออก
                Console.WriteLine($"SQLite Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // ดักจับ Error อื่นๆ ทั่วไป
                Console.WriteLine($"General Error: {ex.Message}");
                return null;
            }
        }

        private InkjetConfigDto BuildMk(SQLiteDataReader r, string pre, int ord, string pNameCol)
        {
            return new InkjetConfigDto
            {
                Ordinal = ord,
                ProgramNumber = GetInt(r, $"{pre}program_no"),
                ProgramName = GetStr(r, pNameCol),
                Width = GetInt(r, $"{pre}width"),
                Height = GetInt(r, $"{pre}height"),
                TriggerDelay = GetInt(r, $"{pre}trigger_delay"),
                Direction = GetInt(r, $"{pre}text_direction"),
                TextBlocks = Enumerable.Range(1, 5)
                    .Select(b => new { b, txt = GetStr(r, $"{pre}block{b}_text") })
                    .Where(x => !string.IsNullOrEmpty(x.txt))
                    .Select(x => new TextBlockDto
                    {
                        BlockNumber = x.b,
                        Text = x.txt,
                        X = GetInt(r, $"{pre}block{x.b}_x"),
                        Y = GetInt(r, $"{pre}block{x.b}_y"),
                        Size = GetInt(r, $"{pre}block{x.b}_size"),
                        Scale = GetInt(r, $"{pre}block{x.b}_scale_side")
                    }).ToList()
            };
        }

        // เพิ่มในไฟล์ SqliteDataService.cs
        public async Task<List<UVinkjet>> GetUvPrintDataAsync()
        {
            // ตรวจสอบว่าไฟล์ DB มีอยู่จริงไหม
            if (!File.Exists(_dbPath)) return new List<UVinkjet>();

            var list = new List<UVinkjet>();

            // สร้าง Connection (แนะนำให้ใช้ using เพื่อคืนทรัพยากร)
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            await conn.OpenAsync();

            // SQL Query: ดึงข้อมูลทั้งหมดจาก uv_print_data
            // เรียงตาม update_at ล่าสุดขึ้นก่อน (ถ้าต้องการ)
            string sql = "SELECT * FROM uv_print_data ORDER BY update_at DESC";

            using var cmd = new SQLiteCommand(sql, conn);

            using var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new UVinkjet
                {
                    // Map คอลัมน์ให้ตรงกับ Schema ของตาราง uv_print_data
                    Id = GetInt(reader, "id") ?? 0,
                    InkjetName = GetStr(reader, "inkjet_name"),
                    Lot = GetStr(reader, "lot"),
                    Name = GetStr(reader, "name"),
                    ProgramName = GetStr(reader, "program_name"),
                    // ถ้าใน Class UVinkjet มีฟิลด์เวลา
                    // UpdateAt = GetStr(reader, "update_at") 
                });
            }
            return list;
        }

        private string GetStr(SQLiteDataReader r, string n) =>
            r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetValue(r.GetOrdinal(n)).ToString();

        private int? GetInt(SQLiteDataReader r, string n) =>
            int.TryParse(GetStr(r, n), out int res) ? res : (int?)null;
    }
}