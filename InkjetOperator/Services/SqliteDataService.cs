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

                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));

                Debug.WriteLine($"[DB] inkjet_data columns: {string.Join(", ", columns.OrderBy(c => c))}");

                var detail = new PatternDetail
                {
                    Barcode = GetStr(reader, "lot_no") ?? barcode,
                    Description = GetStr(reader, "รุ่น_รหัสแผน") ?? GetStr(reader, "program_name") ?? "",
                    InkjetConfigs = new List<InkjetConfigDto>
                    {
                        BuildMk(reader, "mk1_", 1, "program_name",
                            "การหน่วง_ทริกเกอร์", "ความสูง", "ความกว้าง", "ทิศทางของข้อความ", "สเกลด้านข้าง"),
                        BuildMk(reader, "mk2_", 2, "program_name3",
                            "การหน่วง_ทริกเกอร์12", "ความสูง13", "ความกว้าง14", "ทิศทางของข้อความ15", "สเกลด้านข้าง")
                    }
                };

                if (columns.Contains("สายพาน1_inkjet"))
                {
                    detail.ConveyorSpeeds = new ConveyorSpeedDto
                    {
                        Speed1 = GetInt(reader, "สายพาน1_inkjet"),
                        Speed2 = GetInt(reader, "สายพาน2_feed_เข้า_inkjet"),
                        Speed3 = GetInt(reader, "สายพาน3"),
                    };
                }

                var servos = new List<ServoConfigDto>();
                if (columns.Contains("pos_act"))
                {
                    servos.Add(new ServoConfigDto
                    {
                        Ordinal = 1,
                        PostAct = GetDouble(reader, "pos_act"),
                        Delay = GetDouble(reader, "delay"),
                    });
                }
                if (columns.Contains("pos_act_16"))
                {
                    servos.Add(new ServoConfigDto
                    {
                        Ordinal = 2,
                        PostAct = GetDouble(reader, "pos_act_16"),
                        Delay = GetDouble(reader, "delay17"),
                    });
                }
                Debug.WriteLine($"[DB] servo count={servos.Count}, has pos_act={columns.Contains("pos_act")}, has pos_act_16={columns.Contains("pos_act_16")}");
                if (servos.Count > 0)
                    detail.ServoConfigs = servos;

                return detail;
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

        private InkjetConfigDto BuildMk(SQLiteDataReader r, string pre, int ord, string pNameCol,
            string triggerDelayCol, string heightCol, string widthCol, string directionCol, string scaleSuffix)
        {
            return new InkjetConfigDto
            {
                Ordinal = ord,
                ProgramNumber = GetInt(r, $"{pre}program_no"),
                ProgramName = GetStr(r, pNameCol),
                Width = GetInt(r, widthCol),
                Height = GetInt(r, heightCol),
                TriggerDelay = GetInt(r, triggerDelayCol),
                Direction = GetInt(r, directionCol),
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
                        Scale = GetInt(r, $"{pre}block{x.b}_{scaleSuffix}")
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
                // UNC path (\\server\share) → System.Data.SQLite ตัด \\ ทิ้ง → ต้องแปลงเป็น file:// URI
                // เช่น \\192.168.1.59\cpi\CPI.db3 → FullUri=file://192.168.1.59/cpi/CPI.db3
                // UNC (\\server\share): SQLite parser ตัด \\ เหลือ \ → ต้อง double backslash ก่อน
                // \\192.168.1.59\cpi\CPI.db3 → \\\\192.168.1.59\\cpi\\CPI.db3 → parser collapse กลับเป็น UNC ถูก
                // ใช้ Data Source= (branch เดียวกับ local/mapped drive ที่เขียนได้)
                string source = dbPath.StartsWith(@"\\") ? dbPath.Replace(@"\", @"\\") : dbPath;
                string connStr = $"Data Source={source};Version=3;Busy Timeout=5000;Journal Mode=Off;Pooling=False;";

                using var conn = new SQLiteConnection(connStr);
                await conn.OpenAsync();

                // อ่านว่าตารางมีคอลัมน์อะไรบ้าง (schema CPI.db3 แต่ละเครื่อง/เวอร์ชันไม่เหมือนกัน:
                // เก่า = id,lot,name / ใหม่ = id,lot,name,text1..5) → เขียนเฉพาะที่มีจริง
                var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var pragma = new SQLiteCommand($"PRAGMA table_info([{table}])", conn))
                using (var pr = (SQLiteDataReader)await pragma.ExecuteReaderAsync())
                    while (await pr.ReadAsync())
                        cols.Add(pr["name"].ToString() ?? "");

                if (cols.Count == 0)
                    return (false, $"{table}: ❌ ไม่พบตาราง {table} ในไฟล์");

                var wanted = new (string col, string val)[]
                {
                    ("lot", d.Lot ?? ""), ("name", d.Name ?? ""),
                    ("text1", d.Text1 ?? ""), ("text2", d.Text2 ?? ""), ("text3", d.Text3 ?? ""),
                    ("text4", d.Text4 ?? ""), ("text5", d.Text5 ?? ""),
                };

                var sets = new List<string>();
                using var cmd = new SQLiteCommand { Connection = conn };
                foreach (var (col, val) in wanted)
                {
                    if (!cols.Contains(col)) continue;   // ข้ามคอลัมน์ที่ไฟล์ไม่มี
                    sets.Add($"{col}=@{col}");
                    cmd.Parameters.AddWithValue("@" + col, val);
                }

                if (sets.Count == 0)
                    return (false, $"{table}: ❌ ไม่มีคอลัมน์ lot/name/text ให้เขียน");

                // ตารางบางเวอร์ชันไม่มีคอลัมน์ id → ใช้ rowid ของแถวแรกแทน
                string where = cols.Contains("id")
                    ? "WHERE id=1"
                    : $"WHERE rowid=(SELECT rowid FROM [{table}] ORDER BY rowid LIMIT 1)";

                cmd.CommandText = $"UPDATE [{table}] SET {string.Join(", ", sets)} {where}";
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return (false, $"{table}: ไม่มีแถวให้อัปเดต (ตารางว่าง? 0 rows)");

                string wrote = string.Join(", ", sets.ConvertAll(s => s.Split('=')[0]));
                return (true, $"{table}: ✅ เขียนสำเร็จ (คอลัมน์: {wrote})");
            }
            catch (SQLiteException ex)
            {
                // เช่น "no such column: id" หรือ "database is locked"
                return (false, $"{table}: ❌ [{ex.ResultCode}] {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{table}: ❌ {ex.Message}");
            }
        }

        /// <summary>รัน SQL อะไรก็ได้ (manual) กับ CPI.db3 — คืนผลเป็นข้อความ (SELECT/PRAGMA โชว์แถว, อื่นๆ โชว์ rows affected)</summary>
        public async Task<string> RunSqlAsync(string dbPath, string sql)
        {
            if (string.IsNullOrWhiteSpace(dbPath)) return "❌ ยังไม่ได้เลือกไฟล์ (ช่อง CPI.db3 ด้านบน)";
            if (!File.Exists(dbPath)) return $"❌ ไม่พบไฟล์:\n{dbPath}";
            if (string.IsNullOrWhiteSpace(sql)) return "❌ ใส่คำสั่ง SQL ก่อน";

            try
            {
                // UNC: double backslash (เหมือน WriteUvToCpiAsync)
                string source = dbPath.StartsWith(@"\\") ? dbPath.Replace(@"\", @"\\") : dbPath;
                using var conn = new SQLiteConnection(
                    $"Data Source={source};Version=3;Busy Timeout=5000;Journal Mode=Off;Pooling=False;");
                await conn.OpenAsync();
                using var cmd = new SQLiteCommand(sql, conn);

                string head = sql.TrimStart();
                bool isQuery = head.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                            || head.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase);

                if (isQuery)
                {
                    using var r = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                    var lines = new List<string>();
                    var cols = new List<string>();
                    for (int i = 0; i < r.FieldCount; i++) cols.Add(r.GetName(i));
                    lines.Add(string.Join(" | ", cols));

                    int n = 0;
                    while (await r.ReadAsync() && n < 100)
                    {
                        var vals = new List<string>();
                        for (int i = 0; i < r.FieldCount; i++)
                            vals.Add(r.IsDBNull(i) ? "NULL" : r.GetValue(i)?.ToString() ?? "");
                        lines.Add(string.Join(" | ", vals));
                        n++;
                    }
                    return $"✅ {n} row(s)\r\n" + string.Join("\r\n", lines);
                }
                else
                {
                    int rows = await cmd.ExecuteNonQueryAsync();
                    return $"✅ สำเร็จ — {rows} row(s) affected";
                }
            }
            catch (SQLiteException ex) { return $"❌ [{ex.ResultCode}] {ex.Message}"; }
            catch (Exception ex) { return $"❌ {ex.Message}"; }
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
                    BuildMk(reader, "", 1, "program_name",
                        "trigger_delay", "height", "width", "text_direction", "scale_side")
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

        private string GetStr(SQLiteDataReader r, string n)
        {
            try
            {
                int ord = r.GetOrdinal(n);
                return r.IsDBNull(ord) ? null : r.GetValue(ord).ToString();
            }
            catch (IndexOutOfRangeException) { return null; }
        }

        private int? GetInt(SQLiteDataReader r, string n) =>
            int.TryParse(GetStr(r, n), out int res) ? res : (int?)null;

        private double? GetDouble(SQLiteDataReader r, string n) =>
            double.TryParse(GetStr(r, n), out double res) ? res : (double?)null;
    }
}