using Microsoft.Data.Sqlite;
using InkjetOperator.Models;

namespace InkjetOperator.Services;

public class SqliteDataService
{
    private readonly string _dbPath;

    public SqliteDataService(string dbPath)
    {
        _dbPath = dbPath;
    }

    public bool CanConnect()
    {
        if (!File.Exists(_dbPath)) return false;
        try
        {
            using var conn = Open();
            return true;
        }
        catch { return false; }
    }

    public CreatePatternRequest? GetPatternDetail(string barcode, int jobId)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM inkjet_data WHERE lot_no = @barcode LIMIT 1";
        cmd.Parameters.AddWithValue("@barcode", barcode);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var pattern = new CreatePatternRequest
        {
            Barcode = barcode,
            JobId = jobId,
        };

        // MK1 — ordinal 1
        var mk1 = new InkjetConfigDto
        {
            Ordinal = 1,
            ProgramNumber = ReadInt(reader, "mk1_program_no"),
            ProgramName = ReadStr(reader, "program_name"),
            Width = ReadInt(reader, "ความกว้าง"),
            Height = ReadInt(reader, "ความสูง"),
            TriggerDelay = ReadInt(reader, "การหน่วง_ทริกเกอร์"),
            Direction = ReadInt(reader, "ทิศทางของข้อความ"),
            PosAct = ReadInt(reader, "pos_act"),
            Delay = ReadInt(reader, "delay"),
        };
        for (int b = 1; b <= 5; b++)
        {
            var text = ReadStr(reader, $"mk1_block{b}_text");
            if (string.IsNullOrEmpty(text)) continue;
            mk1.TextBlocks.Add(new TextBlockDto
            {
                BlockNumber = b,
                Text = text,
                X = ReadInt(reader, $"mk1_block{b}_x"),
                Y = ReadInt(reader, $"mk1_block{b}_y"),
                Size = ReadInt(reader, $"mk1_block{b}_size"),
                Scale = ReadInt(reader, $"mk1_block{b}_สเกลด้านข้าง"),
            });
        }
        pattern.InkjetConfigs.Add(mk1);

        // MK2 — ordinal 2
        var mk2 = new InkjetConfigDto
        {
            Ordinal = 2,
            ProgramNumber = ReadInt(reader, "mk2_program_no"),
            ProgramName = ReadStr(reader, "program_name3"),
            Width = ReadInt(reader, "ความกว้าง14"),
            Height = ReadInt(reader, "ความสูง13"),
            TriggerDelay = ReadInt(reader, "การหน่วง_ทริกเกอร์12"),
            Direction = ReadInt(reader, "ทิศทางของข้อความ15"),
        };
        for (int b = 1; b <= 5; b++)
        {
            var text = ReadStr(reader, $"mk2_block{b}_text");
            if (string.IsNullOrEmpty(text)) continue;
            mk2.TextBlocks.Add(new TextBlockDto
            {
                BlockNumber = b,
                Text = text,
                X = ReadInt(reader, $"mk2_block{b}_x"),
                Y = ReadInt(reader, $"mk2_block{b}_y"),
                Size = ReadInt(reader, $"mk2_block{b}_size"),
                Scale = ReadInt(reader, $"mk2_block{b}_สเกลด้านข้าง"),
            });
        }
        pattern.InkjetConfigs.Add(mk2);

        // Conveyor speeds
        pattern.ConveyorSpeeds = new ConveyorSpeedDto
        {
            Speed1 = ReadInt(reader, "สายพาน1_inkjet"),
            Speed2 = ReadInt(reader, "สายพาน2_feed_เข้า_inkjet"),
            Speed3 = ReadInt(reader, "สายพาน3"),
        };

        // Servo configs
        pattern.ServoConfigs = new List<ServoConfigDto>
        {
            new() { Ordinal = 1, PostAct = ReadDouble(reader, "pos_act"), Delay = ReadDouble(reader, "delay") },
            new() { Ordinal = 2, PostAct = ReadDouble(reader, "pos_act_16"), Delay = ReadDouble(reader, "delay17") },
        };

        return pattern;
    }

    public List<UvJobItem> GetUvDetail(string barcode)
    {
        var items = new List<UvJobItem>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM print_data WHERE lot_no = @barcode";
        cmd.Parameters.AddWithValue("@barcode", barcode);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new UvJobItem
            {
                Machine = ReadStr(reader, "machine") ?? "",
                TableName = ReadStr(reader, "table_name"),
                ProgramName = ReadStr(reader, "program_name"),
                Lot = ReadStr(reader, "lot"),
                Name = ReadStr(reader, "name"),
                Text1 = ReadStr(reader, "text1"),
                Text2 = ReadStr(reader, "text2"),
                Text3 = ReadStr(reader, "text3"),
                Text4 = ReadStr(reader, "text4"),
                Text5 = ReadStr(reader, "text5"),
            });
        }
        return items;
    }

    /// <summary>
    /// อ่าน plan_routing ของ lot นี้จาก source DB — ไม่พบแถว/ไม่มีตาราง คืน null
    /// เก็บค่าดิบทั้งหมด (marking_method เป็น NULL ได้)
    /// </summary>
    public CreatePlanRoutingRequest? GetPlanRouting(string barcode, int jobId)
    {
        try
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM plan_routing WHERE lot_no = @barcode LIMIT 1";
            cmd.Parameters.AddWithValue("@barcode", barcode);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new CreatePlanRoutingRequest
            {
                PrintJobsId = jobId,
                LotNo = ReadStr(reader, "lot_no") ?? barcode,
                ErpMfg = ReadStr(reader, "erp_mfg"),
                MarkingMethod = ReadStr(reader, "marking_method"),
                ProcessSequence = ReadStr(reader, "process_sequence"),
            };
        }
        catch
        {
            return null;
        }
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath};Mode=ReadOnly");
        conn.Open();
        return conn;
    }

    private static string? ReadStr(SqliteDataReader r, string col)
    {
        try
        {
            int idx = r.GetOrdinal(col);
            return r.IsDBNull(idx) ? null : r.GetString(idx);
        }
        catch { return null; }
    }

    private static int? ReadInt(SqliteDataReader r, string col)
    {
        try
        {
            int idx = r.GetOrdinal(col);
            if (r.IsDBNull(idx)) return null;
            var val = r.GetValue(idx);
            return Convert.ToInt32(val);
        }
        catch { return null; }
    }

    private static double? ReadDouble(SqliteDataReader r, string col)
    {
        try
        {
            int idx = r.GetOrdinal(col);
            if (r.IsDBNull(idx)) return null;
            var val = r.GetValue(idx);
            return Convert.ToDouble(val);
        }
        catch { return null; }
    }
}
