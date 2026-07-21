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

        /// <summary>เช็คว่าไฟล์ DB มีอยู่และเปิดได้จริง — โหลด path ใหม่ทุกครั้งจาก Setting</summary>
        public bool CanConnect()
        {
            // อ่าน path ล่าสุดจาก config ทุกครั้ง (ไม่ใช้ _dbPath ที่ cache ไว้)
            string freshPath = CustomSettingsManager.GetValue("DB_PATH") ?? "";

            if (string.IsNullOrEmpty(freshPath) || !File.Exists(freshPath))
                return false;

            try
            {
                using var conn = new SQLiteConnection($"Data Source={freshPath};Version=3;");
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PatternDetail> GetPatternDetailAsync(string barcode)
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    Console.WriteLine($"Database file not found at: {_dbPath}");
                    return null;
                }

                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                await conn.OpenAsync();

                using var checkCmd = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='inkjet_data';", conn);
                var tableExists = await checkCmd.ExecuteScalarAsync();

                if (tableExists == null)
                {
                    Console.WriteLine("Error: Table 'inkjet_data' missing in database.");
                    return null;
                }

                using var cmd = new SQLiteCommand("SELECT * FROM inkjet_data WHERE lot_no = @p LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@p", barcode);

                using var reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                return new PatternDetail
                {
                    Barcode = GetStr(reader, "lot_no") ?? barcode,
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
                Console.WriteLine($"SQLite Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
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

        /// <summary>
        /// ตอน register: query print_data ด้วย lot_no → คืน UV detail 2 แถว (UV1/UV2)
        /// UV1 = m1_* (Plate/MK063), UV2 = m2_* (Shim/MK067). ไม่พบ lot → คืน list ว่าง
        /// </summary>
        public async Task<List<UvJobData>> GetUvDetailAsync(string lot)
        {
            var list = new List<UvJobData>();

            if (!File.Exists(_dbPath) || string.IsNullOrWhiteSpace(lot))
                return list;

            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                await conn.OpenAsync();

                using var cmd = new SQLiteCommand(
                    "SELECT * FROM print_data WHERE lot_no = @lot LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@lot", lot.Trim());

                using var r = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return list;

                string lotNo = GetStr(r, "lot_no") ?? lot;
                string erp = GetStr(r, "erp_mfg") ?? "";

                list.Add(BuildUv(r, "UV1", "MK063", "m1_program_name", "m1_block_text", lotNo, erp));
                list.Add(BuildUv(r, "UV2", "MK067", "m2_program_name", "m2_block_text", lotNo, erp));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUvDetailAsync Error: {ex.Message}");
            }

            return list;
        }

        private UvJobData BuildUv(SQLiteDataReader r, string machine, string table,
                                  string programCol, string blockPrefix, string lot, string name)
        {
            return new UvJobData
            {
                Machine = machine,
                TableName = table,
                ProgramName = GetStr(r, programCol) ?? "",
                Lot = lot,
                Name = name,
                Text1 = GetStr(r, $"{blockPrefix}1"),
                Text2 = GetStr(r, $"{blockPrefix}2"),
                Text3 = GetStr(r, $"{blockPrefix}3"),
                Text4 = GetStr(r, $"{blockPrefix}4"),
                Text5 = GetStr(r, $"{blockPrefix}5"),
            };
        }

        /// <summary>
        /// M2: เขียน UV detail ลง CPI.db3 (ตาราง MK063/MK067) แถว id=1 — lot, name, text1..5
        /// คืน (สำเร็จไหม, ข้อความผล) เพื่อโชว์ให้ operator
        /// </summary>
        public async Task<(bool ok, string msg)> WriteUvToCpiAsync(string dbPath, string table, UvJobData d)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                return (false, $"{table}: ยังไม่ได้ตั้ง path (browse ใน UV Printer Setting)");

            if (!File.Exists(dbPath))
                return (false, $"{table}: ไม่พบไฟล์\n{dbPath}");

            try
            {
                using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;Busy Timeout=5000;");
                await conn.OpenAsync();

                string sql =
                    $"UPDATE [{table}] SET lot=@lot, name=@name, " +
                    "text1=@t1, text2=@t2, text3=@t3, text4=@t4, text5=@t5 WHERE id=1";

                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@lot", d.Lot ?? "");
                cmd.Parameters.AddWithValue("@name", d.Name ?? "");
                cmd.Parameters.AddWithValue("@t1", d.Text1 ?? "");
                cmd.Parameters.AddWithValue("@t2", d.Text2 ?? "");
                cmd.Parameters.AddWithValue("@t3", d.Text3 ?? "");
                cmd.Parameters.AddWithValue("@t4", d.Text4 ?? "");
                cmd.Parameters.AddWithValue("@t5", d.Text5 ?? "");

                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return (false, $"{table}: ไม่มีแถว id=1 ให้อัปเดต (0 rows)");

                return (true, $"{table}: ✅ เขียนสำเร็จ (lot={d.Lot}, name={d.Name})");
            }
            catch (SQLiteException ex)
            {
                // เช่น "no such table: MK067" หรือ "database is locked"
                return (false, $"{table}: ❌ {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{table}: ❌ {ex.Message}");
            }
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