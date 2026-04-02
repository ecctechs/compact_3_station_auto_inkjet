using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InkjetOperator.Models;

namespace InkjetOperator.Services
{
    public class SqliteDataService
    {
        private readonly string _dbPath;
        private readonly string _dbPathUV_1;

        public SqliteDataService()
        {
            _dbPath = CustomSettingsManager.GetValue("DB_PATH") ?? "";
            _dbPathUV_1 = UvSettingsManager.GetValue("UV1DB3_PATH") ?? "";
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

        // เพิ่ม Method นี้ลงใน Class SqliteDataService
        public async Task<bool> UpdateUvLocalDatabaseAsync(string lot, string name)
        {
            try
            {
                // 1. เช็คว่า Path ว่างไหม หรือไฟล์มีอยู่จริงไหม
                if (string.IsNullOrEmpty(_dbPathUV_1))
                {
                    Debug.WriteLine("Error: DB_PATH is empty.");
                    return false;
                }

                if (!File.Exists(_dbPathUV_1))
                {
                    Debug.WriteLine($"Error: File not found at {_dbPathUV_1}");
                    return false;
                }

                // 2. ใช้ Connection String แบบระบุโหมด (เผื่อไฟล์ถูกอ่านอยู่)
                // เพิ่ม "Journal Mode=Off" หรือ "Pooling=True" เพื่อลดปัญหาไฟล์ถูกล็อก
                using var conn = new SQLiteConnection($"Data Source={_dbPathUV_1};Version=3;New=False;");
                await conn.OpenAsync();

                // 3. ใช้ SQL ที่ระบุชื่อตารางและคอลัมน์ให้ตรงตามไฟล์ CPI.db3 เป๊ะๆ
                // แนะนำให้ใส่ [ ] ครอบชื่อตารางเพื่อป้องกัน Error เรื่องชื่อซ้ำกับ Keyword
                string sql = "UPDATE [MK063] SET [lot] = @lot, [name] = @name WHERE [id] = 1";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@lot", lot ?? "");
                cmd.Parameters.AddWithValue("@name", name ?? "");

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                Debug.WriteLine($"Update SQLite Success: {rowsAffected} row(s) updated.");
                return rowsAffected > 0;
            }
            catch (SQLiteException ex)
            {
                // ตรงนี้สำคัญมาก! มันจะบอกว่า "Database is locked" หรือ "No such table"
                MessageBox.Show($"SQLite Error ({ex.ErrorCode}): {ex.Message}\nPath: {_dbPathUV_1}", "DB Error");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}");
                return false;
            }
        }

        // เพิ่มฟังก์ชันสำหรับ Query ตาราง config_data_mk3
        public async Task<PatternDetail> GetPatternDetailMk3Async(string patternNo)
        {
            try
            {
                if (!File.Exists(_dbPath)) return null;

                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;Busy Timeout=5000;");
                await conn.OpenAsync();

                string sql = "SELECT * FROM config_data_mk3 WHERE pattern_no_erp = @barcode LIMIT 1";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@barcode", patternNo.Trim());

                using var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new PatternDetail
                    {
                        Barcode = GetStr(reader, "pattern_no_erp") ?? patternNo,
                        // เปลี่ยนเป็นดึงชื่อจาก program_name หรือ model_plan_code ตามที่มีใน DB
                        Description = GetStr(reader, "program_name") ?? "",

                        InkjetConfigs = new List<InkjetConfigDto>
                {
                    // *** จุดสำคัญ: เปลี่ยน "mk1_" เป็น "" (ค่าว่าง) ***
                    // เพราะในตาราง mk3 ไม่มีคำว่า mk1_ นำหน้าคอลัมน์
                    BuildMk(reader, "", 1, "program_name")
                }
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private string GetStr(SQLiteDataReader r, string n) =>
            r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetValue(r.GetOrdinal(n)).ToString();

        private int? GetInt(SQLiteDataReader r, string n) =>
            int.TryParse(GetStr(r, n), out int res) ? res : (int?)null;
    }
}