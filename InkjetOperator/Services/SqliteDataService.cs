using Microsoft.Data.Sqlite;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

public class SqliteDataService
{
    private readonly string _dbPath;

    public SqliteDataService(string dbPath) // รับที่อยู่ไฟล์จาก DB_PATH
    {
        _dbPath = dbPath; // จำไฟล์ที่จะเปิดอ่าน
    }

    public bool CanConnect() // ตรวจว่าเปิดฐานข้อมูลต้นทางได้
    {
        if (!File.Exists(_dbPath)) return false; // หาไฟล์ไม่พบให้คืนว่าเชื่อมต่อไม่ได้
        try // เริ่มเปิดไฟล์ฐานข้อมูลต้นทาง และดักข้อผิดพลาดไว้
        {
            using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
            return true; // เปิดไฟล์ได้แล้ว หน้าสแกนจึงอ่านข้อมูลต่อได้
        }
        catch { return false; } // เปิดไฟล์ไม่ได้ให้คืน false
    }

    public CreatePatternRequest? GetPatternDetail(string barcode, int jobId) // Flow 7: อ่านค่าพิมพ์ MK และค่าประกอบ
    {
        using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
        using var cmd = conn.CreateCommand(); // เตรียมคำสั่งค้นข้อมูล
        cmd.CommandText = "SELECT * FROM inkjet_data WHERE lot_no = @barcode LIMIT 1"; // ค้นแถวตั้งค่าพิมพ์ของ Lot นี้
        cmd.Parameters.AddWithValue("@barcode", barcode); // ใช้ Barcode นี้ค้น lot_no

        using var reader = cmd.ExecuteReader(); // รันคำสั่งแล้วอ่านแถวที่ได้
        if (!reader.Read()) return null; // ไม่พบแถวให้คืน null

        var pattern = new CreatePatternRequest // เตรียมชุด Pattern ส่ง Backend
        {
            Barcode = barcode, // เก็บ Barcode ของงาน
            JobId = jobId, // ใส่ Job ID ที่ผู้เรียกส่งมา
        };

        // MK1 — ordinal 1
        var mk1 = new InkjetConfigDto // เตรียมค่าหัวพิมพ์ MK1
        {
            Ordinal = 1, // ระบุหัวพิมพ์ตัวที่ 1
            ProgramNumber = ReadInt(reader, "mk1_program_no"), // อ่านเลขโปรแกรม MK1
            ProgramName = ReadStr(reader, "program_name"), // อ่านชื่อโปรแกรม MK1
            Width = ReadInt(reader, "ความกว้าง"), // อ่านความกว้างข้อความ MK1
            Height = ReadInt(reader, "ความสูง"), // อ่านความสูงข้อความ MK1
            TriggerDelay = ReadInt(reader, "การหน่วง_ทริกเกอร์"), // อ่านค่าหน่วงทริกเกอร์ MK1
            Direction = ReadInt(reader, "ทิศทางของข้อความ"), // อ่านทิศทางข้อความ MK1
            PosAct = ReadInt(reader, "pos_act"), // อ่านค่าตำแหน่ง
            Delay = ReadInt(reader, "delay"), // อ่านค่าหน่วง
        };
        for (int b = 1; b <= 5; b++) // อ่านข้อความพิมพ์ทีละบล็อก รวม 5 บล็อก
        {
            var text = ReadStr(reader, $"mk1_block{b}_text"); // อ่านข้อความของบล็อก MK1
            if (string.IsNullOrEmpty(text)) continue; // บล็อกไม่มีข้อความให้ข้าม
            mk1.TextBlocks.Add(new TextBlockDto // เพิ่มบล็อกให้ MK1
            {
                BlockNumber = b, // ระบุลำดับบล็อก
                Text = text, // เก็บข้อความที่จะพิมพ์
                X = ReadInt(reader, $"mk1_block{b}_x"), // อ่านตำแหน่ง Xของบล็อก MK1
                Y = ReadInt(reader, $"mk1_block{b}_y"), // อ่านตำแหน่ง Yของบล็อก MK1
                Size = ReadInt(reader, $"mk1_block{b}_size"), // อ่านขนาดข้อความของบล็อก MK1
                Scale = ReadInt(reader, $"mk1_block{b}_สเกลด้านข้าง"), // อ่านสเกลด้านข้างของบล็อก MK1
            });
        }
        pattern.InkjetConfigs.Add(mk1); // รวม MK1 ไว้ใน Pattern

        // MK2 — ordinal 2
        var mk2 = new InkjetConfigDto // เตรียมค่าหัวพิมพ์ MK2
        {
            Ordinal = 2, // ระบุหัวพิมพ์ตัวที่ 2
            ProgramNumber = ReadInt(reader, "mk2_program_no"), // อ่านเลขโปรแกรม MK2
            ProgramName = ReadStr(reader, "program_name3"), // อ่านชื่อโปรแกรม MK2
            Width = ReadInt(reader, "ความกว้าง14"), // อ่านความกว้างข้อความ MK2
            Height = ReadInt(reader, "ความสูง13"), // อ่านความสูงข้อความ MK2
            TriggerDelay = ReadInt(reader, "การหน่วง_ทริกเกอร์12"), // อ่านค่าหน่วงทริกเกอร์ MK2
            Direction = ReadInt(reader, "ทิศทางของข้อความ15"), // อ่านทิศทางข้อความ MK2
        };
        for (int b = 1; b <= 5; b++) // อ่านข้อความพิมพ์ทีละบล็อก รวม 5 บล็อก
        {
            var text = ReadStr(reader, $"mk2_block{b}_text"); // อ่านข้อความของบล็อก MK2
            if (string.IsNullOrEmpty(text)) continue; // บล็อกไม่มีข้อความให้ข้าม
            mk2.TextBlocks.Add(new TextBlockDto // เพิ่มบล็อกให้ MK2
            {
                BlockNumber = b, // ระบุลำดับบล็อก
                Text = text, // เก็บข้อความที่จะพิมพ์
                X = ReadInt(reader, $"mk2_block{b}_x"), // อ่านตำแหน่ง Xของบล็อก MK2
                Y = ReadInt(reader, $"mk2_block{b}_y"), // อ่านตำแหน่ง Yของบล็อก MK2
                Size = ReadInt(reader, $"mk2_block{b}_size"), // อ่านขนาดข้อความของบล็อก MK2
                Scale = ReadInt(reader, $"mk2_block{b}_สเกลด้านข้าง"), // อ่านสเกลด้านข้างของบล็อก MK2
            });
        }
        pattern.InkjetConfigs.Add(mk2); // รวม MK2 ไว้ใน Pattern

        // Conveyor speeds
        pattern.ConveyorSpeeds = new ConveyorSpeedDto // เตรียมค่าความเร็วสายพาน
        {
            Speed1 = ReadInt(reader, "สายพาน1_inkjet"), // อ่านความเร็วสายพาน 1
            Speed2 = ReadInt(reader, "สายพาน2_feed_เข้า_inkjet"), // อ่านความเร็วสายพาน 2
            Speed3 = ReadInt(reader, "สายพาน3"), // อ่านความเร็วสายพาน 3
        };

        // Servo configs
        pattern.ServoConfigs = new List<ServoConfigDto> // เตรียมค่าของ Servo ทั้งสองตัว
        {
            new() { Ordinal = 1, PostAct = ReadDouble(reader, "pos_act"), Delay = ReadDouble(reader, "delay") }, // อ่านตำแหน่งและค่าหน่วง Servo 1
            new() { Ordinal = 2, PostAct = ReadDouble(reader, "pos_act_16"), Delay = ReadDouble(reader, "delay17") }, // อ่านตำแหน่งและค่าหน่วง Servo 2
        };

        return pattern; // คืน Pattern ให้หน้าสแกนไปบันทึก
    }

    /// <summary>
    /// อ่านข้อมูลหัวงานที่หน้า Scan Barcode เอาไปโชว์ — ไม่พบแถวใน print_data คืน null
    ///
    /// marking_method อยู่คนละตาราง (plan_routing) และ lot ที่ยังไม่มีแถวตรงนั้นก็มี
    /// จึงปล่อยให้เป็นค่าว่างแทนที่จะถือว่าหาไม่เจอทั้ง lot
    /// </summary>
    public LotSummary? GetLotSummary(string barcode) // Flow 3: อ่านหัวงานมาแสดงบนหน้าสแกน
    {
        using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
        using var cmd = conn.CreateCommand(); // เตรียมคำสั่งค้นข้อมูล
        cmd.CommandText = "SELECT erp_mfg, qty FROM print_data WHERE lot_no = @barcode LIMIT 1"; // หา Order No และ Qty จาก Lot
        cmd.Parameters.AddWithValue("@barcode", barcode); // ใช้ Barcode นี้ค้น lot_no

        using var reader = cmd.ExecuteReader(); // รันคำสั่งแล้วอ่านแถวที่ได้
        if (!reader.Read()) return null; // ไม่พบแถวให้คืน null

        return new LotSummary // คืนข้อมูลที่ใช้แสดงบนจอ
        {
            LotNo = barcode, // เก็บ Lot ที่ค้น
            ErpMfg = ReadStr(reader, "erp_mfg"), // อ่าน Order No
            Qty = ReadInt(reader, "qty"),   // เก็บเป็น TEXT ใน DB3 → ReadInt แปลงให้
            MarkingMethod = GetPlanRouting(barcode, 0)?.MarkingMethod, // อ่านวิธีพิมพ์จาก plan_routing
            Customer = GetCustomer(barcode), // อ่านลูกค้าเก็บไว้ไปกับงาน
        };
    }

    /// <summary>
    /// ชื่อลูกค้าของ lot นี้ — อยู่ใน inkjet_data คนละตารางกับช่องอื่นของหน้า Scan Barcode
    /// ไม่พบแถวหรือไม่มีคอลัมน์ก็คืน null ไม่ถือว่าผิด งานยังลงทะเบียนได้ตามปกติ
    /// </summary>
    private string? GetCustomer(string barcode) // อ่านลูกค้าจากข้อมูลตั้งค่าพิมพ์
    {
        try // เริ่มอ่านชื่อลูกค้าของ Lot และดักข้อผิดพลาดไว้
        {
            using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
            using var cmd = conn.CreateCommand(); // เตรียมคำสั่งค้นข้อมูล
            cmd.CommandText = "SELECT customer FROM inkjet_data WHERE lot_no = @barcode LIMIT 1"; // ค้นชื่อลูกค้าของ Lot
            cmd.Parameters.AddWithValue("@barcode", barcode); // ใช้ Barcode นี้ค้น lot_no

            using var reader = cmd.ExecuteReader(); // รันคำสั่งแล้วอ่านแถวที่ได้
            return reader.Read() ? ReadStr(reader, "customer") : null; // มีแถวให้อ่านลูกค้า ไม่มีให้คืนค่าว่าง
        }
        catch // จัดการปัญหาระหว่างอ่านชื่อลูกค้าของ Lot
        {
            return null; // อ่านชื่อลูกค้าไม่ได้ ให้หัวงานใช้ค่าว่าง
        }
    }

    /// <summary>
    /// อ่าน UV detail ของ lot นี้จาก print_data — 1 lot ได้สูงสุด 2 แถว (UV1/UV2)
    /// เครื่องที่ไม่มีทั้งชื่อโปรแกรมและข้อความ = ไม่มีงาน UV → ไม่เก็บ
    /// </summary>
    public List<UvJobItem> GetUvDetail(string barcode) // Flow 7: อ่านข้อมูล UV1 และ UV2
    {
        var items = new List<UvJobItem>(); // เตรียมรายการ UV ของ Lot
        using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
        using var cmd = conn.CreateCommand(); // เตรียมคำสั่งค้นข้อมูล
        cmd.CommandText = "SELECT * FROM print_data WHERE lot_no = @barcode"; // ค้นทุกแถวของ Lot นี้
        cmd.Parameters.AddWithValue("@barcode", barcode); // ใช้ Barcode นี้ค้น lot_no

        using var reader = cmd.ExecuteReader(); // รันคำสั่งแล้วอ่านแถวที่ได้
        while (reader.Read()) // อ่านทีละแถวที่ค้นพบ
        {
            var lot = ReadStr(reader, "lot_no") ?? barcode; // ใช้ Lot จากแถว ถ้าไม่มีใช้ Barcode เดิม
            var erpMfg = ReadStr(reader, "erp_mfg"); // อ่าน Order No ของแถวนี้
            var qty = ReadInt(reader, "qty");   // เก็บเป็น TEXT ใน DB3 → ReadInt แปลงให้

            AddIfHasData(items, BuildUv(reader, "UV1", "MK063", "m1_", lot, erpMfg, qty)); // เก็บข้อมูล m1 เป็นงาน UV1
            AddIfHasData(items, BuildUv(reader, "UV2", "MK067", "m2_", lot, erpMfg, qty)); // เก็บข้อมูล m2 เป็นงาน UV2
        }
        return items; // คืนรายการ UV ให้หน้าสแกน
    }

    // m1_* → UV1/MK063 (Plate), m2_* → UV2/MK067 (Shim)
    // ยืนยันจากชื่อโปรแกรมในข้อมูลจริง: m1 ขึ้นต้น "P-" (Plate), m2 ขึ้นต้น "S-" (Shim)
    private static UvJobItem BuildUv( // รวมค่าของเครื่อง UV หนึ่งรายการ
        SqliteDataReader r, string machine, string table, string prefix, // รับแถวข้อมูล เครื่อง และคำนำหน้าคอลัมน์
        string lot, string? erpMfg, int? qty) // รับ Lot, Order No และ Qty ต้นทาง
    {
        return new UvJobItem // คืนข้อมูล UV ที่ประกอบแล้ว
        {
            Machine = machine, // ระบุ UV1 หรือ UV2
            TableName = table, // ระบุ MK063 หรือ MK067
            ProgramName = ReadStr(r, $"{prefix}program_name"), // อ่านชื่อโปรแกรมของฝั่งนี้
            Lot = lot, // เก็บ Lot ของงาน
            ErpMfg = erpMfg, // เก็บ Order No ของงาน
            Qty = qty, // ใช้ Qty ต้นทาง ไม่ใช่ค่าที่แก้บนจอ
            Text1 = ReadStr(r, $"{prefix}block_text1"), // อ่านข้อความ UV ช่อง 1
            Text2 = ReadStr(r, $"{prefix}block_text2"), // อ่านข้อความ UV ช่อง 2
            Text3 = ReadStr(r, $"{prefix}block_text3"), // อ่านข้อความ UV ช่อง 3
            Text4 = ReadStr(r, $"{prefix}block_text4"), // อ่านข้อความ UV ช่อง 4
            Text5 = ReadStr(r, $"{prefix}block_text5"), // อ่านข้อความ UV ช่อง 5
        };
    }

    private static void AddIfHasData(List<UvJobItem> items, UvJobItem item) // ข้ามเครื่อง UV ที่ไม่มีข้อมูลใช้งาน
    {
        bool hasData = // มีชื่อโปรแกรมหรือข้อความอย่างใดอย่างหนึ่งก็เก็บ
            !string.IsNullOrWhiteSpace(item.ProgramName) || // ตรวจว่ามีชื่อโปรแกรม
            !string.IsNullOrWhiteSpace(item.Text1) || // ตรวจว่ามีข้อความช่อง 1
            !string.IsNullOrWhiteSpace(item.Text2) || // ตรวจว่ามีข้อความช่อง 2
            !string.IsNullOrWhiteSpace(item.Text3) || // ตรวจว่ามีข้อความช่อง 3
            !string.IsNullOrWhiteSpace(item.Text4) || // ตรวจว่ามีข้อความช่อง 4
            !string.IsNullOrWhiteSpace(item.Text5); // ตรวจว่ามีข้อความช่อง 5

        if (hasData) items.Add(item); // เพิ่มเฉพาะรายการที่มีข้อมูล
    }

    /// <summary>
    /// อ่าน plan_routing ของ lot นี้จาก source DB — ไม่พบแถว/ไม่มีตาราง คืน null
    /// เก็บค่าดิบทั้งหมด (marking_method เป็น NULL ได้)
    /// </summary>
    public CreatePlanRoutingRequest? GetPlanRouting(string barcode, int jobId) // อ่านวิธีพิมพ์และลำดับกระบวนการ
    {
        try // เริ่มอ่านแผนงานของ Lot และดักข้อผิดพลาดไว้
        {
            using var conn = Open(); // เปิด SQLite แบบอ่านอย่างเดียว ใช้เสร็จปิดให้
            using var cmd = conn.CreateCommand(); // เตรียมคำสั่งค้นข้อมูล
            cmd.CommandText = "SELECT * FROM plan_routing WHERE lot_no = @barcode LIMIT 1"; // ค้นแผนงานของ Lot นี้
            cmd.Parameters.AddWithValue("@barcode", barcode); // ใช้ Barcode นี้ค้น lot_no

            using var reader = cmd.ExecuteReader(); // รันคำสั่งแล้วอ่านแถวที่ได้
            if (!reader.Read()) return null; // ไม่พบแถวให้คืน null

            return new CreatePlanRoutingRequest // เตรียม Routing สำหรับส่ง Backend
            {
                PrintJobsId = jobId, // ระบุ Job ที่จะผูก Routing
                LotNo = ReadStr(reader, "lot_no") ?? barcode, // อ่าน Lot ถ้าไม่มีใช้ Barcode เดิม
                ErpMfg = ReadStr(reader, "erp_mfg"), // อ่าน Order No
                MarkingMethod = ReadStr(reader, "marking_method"), // อ่านรหัสวิธีพิมพ์
                ProcessSequence = ReadStr(reader, "process_sequence"), // อ่านลำดับกระบวนการตามต้นทาง
            };
        }
        catch // จัดการปัญหาระหว่างอ่านแผนงานของ Lot
        {
            return null; // อ่านแผนงานไม่ได้ ให้ผู้เรียกจัดการกรณีไม่มี Routing
        }
    }

    private SqliteConnection Open() // ทุกคำสั่งอ่าน DB3 ใช้การเปิดแบบนี้
    {
        var conn = new SqliteConnection(SqlitePath.ReadOnly(_dbPath)); // ตั้ง SQLite ให้อ่านอย่างเดียว
        conn.Open(); // เปิดการเชื่อมต่อไฟล์
        return conn; // ส่งการเชื่อมต่อกลับให้ผู้เรียก
    }

    private static string? ReadStr(SqliteDataReader r, string col) // อ่านคอลัมน์เป็นข้อความ
    {
        try // เริ่มอ่านข้อความจากคอลัมน์ และดักข้อผิดพลาดไว้
        {
            int idx = r.GetOrdinal(col); // หาตำแหน่งคอลัมน์ตามชื่อ
            return r.IsDBNull(idx) ? null : r.GetString(idx); // ค่าว่างคืน null ถ้ามีให้อ่านข้อความ
        }
        catch { return null; } // ไม่มีคอลัมน์หรือแปลงไม่ได้ให้คืนค่าว่าง
    }

    private static int? ReadInt(SqliteDataReader r, string col) // อ่านคอลัมน์เป็นจำนวนเต็ม
    {
        try // เริ่มอ่านจำนวนเต็มจากคอลัมน์ และดักข้อผิดพลาดไว้
        {
            int idx = r.GetOrdinal(col); // หาตำแหน่งคอลัมน์ตามชื่อ
            if (r.IsDBNull(idx)) return null; // ค่าว่างใน DB ให้คืน null
            var val = r.GetValue(idx); // อ่านค่าดิบในคอลัมน์
            return Convert.ToInt32(val); // แปลงค่าที่อ่านเป็นจำนวนเต็ม
        }
        catch { return null; } // ไม่มีคอลัมน์หรือแปลงไม่ได้ให้คืนค่าว่าง
    }

    private static double? ReadDouble(SqliteDataReader r, string col) // อ่านคอลัมน์เป็นเลขทศนิยม
    {
        try // เริ่มอ่านเลขทศนิยมจากคอลัมน์ และดักข้อผิดพลาดไว้
        {
            int idx = r.GetOrdinal(col); // หาตำแหน่งคอลัมน์ตามชื่อ
            if (r.IsDBNull(idx)) return null; // ค่าว่างใน DB ให้คืน null
            var val = r.GetValue(idx); // อ่านค่าดิบในคอลัมน์
            return Convert.ToDouble(val); // แปลงค่าที่อ่านเป็นเลขทศนิยม
        }
        catch { return null; } // ไม่มีคอลัมน์หรือแปลงไม่ได้ให้คืนค่าว่าง
    }
}
