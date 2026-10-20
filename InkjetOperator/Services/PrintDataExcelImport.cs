using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

/// <summary>
/// แปลงไฟล์ Excel ของลูกค้าเป็น PrintData.db3 — ลูกค้าอัปเดตข้อมูลทุกวันเป็น Excel
/// ไม่สะดวกแปลงเป็น db3 เอง
///
/// <para>
/// รูปแบบ Excel: หนึ่งไฟล์ สามชีต ชื่อ <c>print_data</c> · <c>inkjet_data</c> ·
/// <c>plan_routing</c> แถวแรกเป็นหัวคอลัมน์ ชื่อต้องตรงกับ PrintData.db3 ที่ใช้อยู่ทุกตัว
/// (ลำดับสลับได้) ขาดหรือเกินแม้แต่คอลัมน์เดียวไม่แปลง — คอลัมน์ที่ไม่ตรงคือข้อมูลที่
/// โปรแกรมอ่านไม่เจอ ปล่อยผ่านไปเท่ากับพ่นงานด้วยค่าว่าง
/// ปุ่ม "สร้างไฟล์ Excel ต้นแบบ" (<see cref="ExportTemplate"/>) ทำไฟล์ที่ถูกรูปแบบให้ลูกค้าใช้
/// </para>
/// <para>
/// แปลงเป็นทั้งไฟล์ ไม่ได้แก้ทีละแถว — ข้อมูลทั้งหมดมาจาก Excel ฉบับเดียว
/// ทำไฟล์ใหม่ในเครื่องตัวเองจนตรวจผ่านก่อน แล้วค่อยสลับเข้าไปแทนไฟล์บนแชร์ทีเดียว
/// ไฟล์เดิมเก็บไว้เป็น <c>PrintData.db3.bak</c> พลาดขั้นไหนไฟล์เดิมยังอยู่ครบ
/// เครื่องอื่นใช้ข้อมูลเดิมต่อได้
/// </para>
/// <para>
/// ให้แปลงเครื่องเดียว (เครื่อง Scan Barcode) ด้วยค่า <see cref="ImportAtStartKey"/>
/// เครื่องอื่นอ่านไฟล์บนแชร์ตัวเดียวกันอยู่แล้ว
/// </para>
/// </summary>
public static class PrintDataExcelImport
{
    /// <summary>ที่อยู่ไฟล์ Excel ของลูกค้า</summary>
    public const string ExcelPathKey = "EXCEL_PATH";

    /// <summary>"1" = เครื่องนี้แปลง Excel ตอนเปิดโปรแกรม</summary>
    public const string ImportAtStartKey = "EXCEL_IMPORT_AT_START";

    /// <summary>ตารางที่มาจาก Excel — ชื่อชีตต้องตรงกับชื่อตาราง</summary>
    public static readonly string[] Tables = ["print_data", "inkjet_data", "plan_routing"];

    private const string MetaTable = "_sync_meta";
    private const string MetaExcelFile = "excel_file";
    private const string MetaExcelModified = "excel_modified_utc";
    private const string MetaExcelSize = "excel_size";
    private const string MetaLastSync = "last_sync";
    private const string MetaImportedBy = "imported_by";

    /// <summary>บอกปัญหาไม่เกินนี้ — ที่เกินบอกเป็นจำนวน คนแก้ทีละไม่กี่จุดอยู่แล้ว</summary>
    private const int MaxProblems = 8;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>กันกดแปลงซ้อนกับที่กำลังแปลงตอนเปิดโปรแกรม</summary>
    private static readonly object Gate = new();

    public enum Outcome
    {
        /// <summary>แปลงแล้ว สลับไฟล์ใหม่เข้าไปแล้ว</summary>
        Imported,
        /// <summary>Excel ไม่เปลี่ยนจากครั้งที่แปลงล่าสุด</summary>
        Unchanged,
        /// <summary>Excel ผิดรูปแบบ ไม่ได้แตะไฟล์เดิม</summary>
        Rejected,
        /// <summary>แถวน้อยลงผิดปกติ ต้องให้คนยืนยันก่อน ไม่ได้แตะไฟล์เดิม</summary>
        NeedsConfirm,
        /// <summary>อ่าน / เขียน / สลับไฟล์ไม่ได้ ไม่ได้แตะไฟล์เดิม</summary>
        Failed,
    }

    public sealed record ImportResult(Outcome Outcome, string Message);

    /// <summary>
    /// แปลงตอนเปิดโปรแกรม — null เมื่อเครื่องนี้ไม่ได้ตั้งให้แปลง
    /// แปลงเฉพาะเมื่อ Excel เปลี่ยน และไม่ยอมให้แถวหายเกินครึ่งโดยไม่มีคนยืนยัน
    /// </summary>
    public static ImportResult? RunAtStartup()
    {
        if (CustomSettingsManager.Read(ImportAtStartKey, "0") != "1") return null;

        return Import(
            CustomSettingsManager.Read(ExcelPathKey, ""),
            CustomSettingsManager.Read("DB_PATH", ""),
            force: false, allowShrink: false);
    }

    /// <summary>
    /// แปลง Excel เป็น PrintData.db3 แทนไฟล์เดิมทั้งไฟล์
    /// </summary>
    /// <param name="force">แปลงแม้ Excel ไม่เปลี่ยนจากครั้งล่าสุด (ปุ่ม "แปลงตอนนี้")</param>
    /// <param name="allowShrink">คนยืนยันแล้วว่าแถวน้อยลงมากเป็นเรื่องตั้งใจ</param>
    public static ImportResult Import(string excelPath, string dbPath, bool force, bool allowShrink)
    {
        excelPath = (excelPath ?? "").Trim();
        dbPath = (dbPath ?? "").Trim();

        if (excelPath.Length == 0) return Fail(Outcome.Failed, "ยังไม่ได้ตั้งไฟล์ Excel");
        if (dbPath.Length == 0) return Fail(Outcome.Failed, "ยังไม่ได้ตั้งไฟล์ PrintData.db3");

        if (!Monitor.TryEnter(Gate)) return Fail(Outcome.Failed, "กำลังแปลงไฟล์อยู่ รอสักครู่แล้วลองใหม่");
        string? localTemp = null;
        try
        {
            if (!File.Exists(excelPath)) return Fail(Outcome.Failed, $"ไม่พบไฟล์ Excel\n{excelPath}");

            // แม่แบบคอลัมน์มาจากไฟล์ที่ใช้อยู่ ไฟล์หลักหายใช้ตัวสำรองแทน
            var source = File.Exists(dbPath) ? dbPath : File.Exists(BackupPath(dbPath)) ? BackupPath(dbPath) : null;
            if (source == null)
                return Fail(Outcome.Failed, $"ไม่พบ PrintData.db3 ที่ใช้อยู่ — ต้องมีไฟล์เดิมเป็นแม่แบบคอลัมน์\n{dbPath}");

            var current = ReadCurrent(source);
            if (Tables.FirstOrDefault(t => !current.Tables.ContainsKey(t)) is { } missingTable)
                return Fail(Outcome.Failed, $"PrintData.db3 ที่ใช้อยู่ไม่มีตาราง {missingTable}");

            var excel = new FileInfo(excelPath);
            var stamp = excel.LastWriteTimeUtc.ToString("o", Inv);
            var size = excel.Length.ToString(Inv);
            if (!force
                && current.Meta.GetValueOrDefault(MetaExcelModified) == stamp
                && current.Meta.GetValueOrDefault(MetaExcelSize) == size)
                return new ImportResult(Outcome.Unchanged, "ไฟล์ Excel ไม่เปลี่ยนจากครั้งที่แปลงล่าสุด");

            var (data, problems) = ReadExcel(excelPath, current);
            if (problems.Count > 0) return Fail(Outcome.Rejected, "ไฟล์ Excel ไม่ตรงรูปแบบ ไม่ได้แปลง\n" + Summarize(problems));

            if (!allowShrink && Shrunk(current, data) is { Count: > 0 } shrunk)
                return Fail(Outcome.NeedsConfirm,
                    "ข้อมูลใน Excel น้อยกว่าไฟล์เดิมเกินครึ่ง ไม่ได้แปลง\n" + string.Join("\n", shrunk));

            localTemp = Path.Combine(Path.GetTempPath(), $"PrintData.{Guid.NewGuid():N}.db3");
            Build(localTemp, current, data, new Dictionary<string, string>
            {
                [MetaLastSync] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Inv),
                [MetaExcelFile] = excel.Name,
                [MetaExcelModified] = stamp,
                [MetaExcelSize] = size,
                [MetaImportedBy] = Environment.MachineName,
            });
            Verify(localTemp, data);

            if (SwapIn(localTemp, dbPath) is { } swapProblem) return Fail(Outcome.Failed, swapProblem);

            var counts = string.Join(" · ", Tables.Select(t => $"{t} {data[t].Count:N0} แถว"));
            AppLog.Info("ExcelImport", $"แปลง {excelPath} → {dbPath} สำเร็จ · {counts}");
            return new ImportResult(Outcome.Imported, $"แปลงไฟล์ Excel เป็น PrintData.db3 แล้ว\n{counts}");
        }
        catch (Exception ex)
        {
            AppLog.Error("ExcelImport", ex);
            return new ImportResult(Outcome.Failed, $"แปลงไม่สำเร็จ ใช้ข้อมูลเดิมต่อ\n{ex.Message}");
        }
        finally
        {
            if (localTemp != null) TryDelete(localTemp);
            Monitor.Exit(Gate);
        }
    }

    /// <summary>
    /// สร้าง Excel ต้นแบบจาก PrintData.db3 ที่ใช้อยู่ — ชื่อชีตและหัวคอลัมน์ถูกต้องครบ
    ///
    /// <para>
    /// ทุกช่องเป็นข้อความ และตั้งทั้งคอลัมน์เป็นรูปแบบข้อความ ค่าที่พิมพ์เพิ่มทีหลังจะไม่ถูก
    /// Excel แปลงเป็นตัวเลข — <c>marking_method</c> "02" ต้องยังเป็น "02" ไม่ใช่ 2
    /// </para>
    /// </summary>
    public static void ExportTemplate(string dbPath, string excelPath)
    {
        using var readLock = PrintDataDbLock.Read();
        var current = ReadCurrent(dbPath);
        using var book = new XLWorkbook();

        using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
        conn.Open();

        foreach (var table in Tables)
        {
            if (!current.Tables.TryGetValue(table, out var schema))
                throw new InvalidOperationException($"PrintData.db3 ไม่มีตาราง {table}");

            var sheet = book.AddWorksheet(table);
            sheet.Columns(1, schema.Columns.Count).Style.NumberFormat.Format = "@";
            sheet.Columns(1, schema.Columns.Count).Width = 18;

            for (int c = 0; c < schema.Columns.Count; c++)
                sheet.Cell(1, c + 1).Value = schema.Columns[c];
            sheet.Row(1).Style.Font.Bold = true;
            sheet.SheetView.FreezeRows(1);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT {string.Join(", ", schema.Columns.Select(Quote))} FROM {Quote(table)}";
            using var reader = cmd.ExecuteReader();
            int row = 2;
            while (reader.Read())
            {
                for (int c = 0; c < schema.Columns.Count; c++)
                    if (!reader.IsDBNull(c))
                        sheet.Cell(row, c + 1).Value = Convert.ToString(reader.GetValue(c), Inv);
                row++;
            }
        }

        book.SaveAs(excelPath);
    }

    /// <summary>
    /// ข้อความบอกว่าแปลงจาก Excel ครั้งล่าสุดเมื่อไร — null เมื่อไฟล์นี้ไม่เคยมาจาก Excel
    /// </summary>
    public static string? LastImport(string dbPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath)) return null;
            var meta = ReadCurrent(dbPath).Meta;
            if (!meta.TryGetValue(MetaExcelFile, out var file)) return null;

            var when = meta.GetValueOrDefault(MetaLastSync) ?? "";
            if (DateTime.TryParseExact(when, "yyyy-MM-dd HH:mm:ss", Inv, DateTimeStyles.None, out var at))
                when = at.ToString("dd/MM/yyyy HH:mm", Inv);
            return $"✓  แปลงล่าสุด {when} · {file}"
                   + (meta.TryGetValue(MetaImportedBy, out var by) ? $" · เครื่อง {by}" : "");
        }
        catch
        {
            return null;
        }
    }

    public static string BackupPath(string dbPath) => dbPath + ".bak";

    // ─── อ่านไฟล์ที่ใช้อยู่ ──────────────────────────────────────────────

    private sealed record TableSchema(string Name, string Sql, List<string> Columns, List<string> Key, int Rows);

    private sealed record CurrentDb(
        Dictionary<string, TableSchema> Tables,
        List<string> OtherObjects,
        Dictionary<string, List<object?[]>> OtherRows,
        Dictionary<string, string> Meta);

    /// <summary>
    /// โครงสร้างทั้งหมดของไฟล์ที่ใช้อยู่ — อ่านเสร็จปิดไฟล์ทันที ไม่ถือไว้ระหว่างแปลง
    /// ตารางอื่นนอกจากสามตารางของ Excel (เช่น <c>_sync_meta</c>) ยกข้อมูลไปไฟล์ใหม่ด้วย
    /// </summary>
    private static CurrentDb ReadCurrent(string dbPath)
    {
        using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
        conn.Open();

        var tables = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);
        var others = new List<string>();
        var otherRows = new Dictionary<string, List<object?[]>>(StringComparer.OrdinalIgnoreCase);
        var meta = new Dictionary<string, string>(StringComparer.Ordinal);

        var objects = new List<(string Type, string Name, string Sql)>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT type, name, sql FROM sqlite_master WHERE sql IS NOT NULL AND name NOT LIKE 'sqlite_%'";
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) objects.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        foreach (var (type, name, sql) in objects)
        {
            if (type != "table")
            {
                others.Add(sql);
                continue;
            }

            var columns = new List<string>();
            var key = new List<(int Order, string Column)>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({Quote(name)})";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(1));
                    int pk = reader.GetInt32(5);
                    if (pk > 0) key.Add((pk, reader.GetString(1)));
                }
            }

            int rows;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(*) FROM {Quote(name)}";
                rows = Convert.ToInt32(cmd.ExecuteScalar(), Inv);
            }

            tables[name] = new TableSchema(name, sql, columns, key.OrderBy(k => k.Order).Select(k => k.Column).ToList(), rows);

            if (Tables.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;

            var copied = new List<object?[]>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM {Quote(name)}";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var values = new object?[reader.FieldCount];
                    for (int i = 0; i < values.Length; i++) values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    copied.Add(values);
                }
            }
            otherRows[name] = copied;

            if (string.Equals(name, MetaTable, StringComparison.OrdinalIgnoreCase)
                && columns.Count >= 2 && columns[0] == "key" && columns[1] == "value")
                foreach (var row in copied)
                    if (row[0] is string k && row[1] != null) meta[k] = Convert.ToString(row[1], Inv)!;
        }

        return new CurrentDb(tables, others, otherRows, meta);
    }

    // ─── อ่าน Excel ─────────────────────────────────────────────────────

    /// <summary>
    /// ข้อมูลของสามชีต เรียงคอลัมน์ตามตารางใน db3 — พร้อมรายการปัญหาที่ทำให้แปลงไม่ได้
    /// </summary>
    private static (Dictionary<string, List<string?[]>> Data, List<string> Problems) ReadExcel(
        string excelPath, CurrentDb current)
    {
        var data = new Dictionary<string, List<string?[]>>(StringComparer.OrdinalIgnoreCase);
        var problems = new List<string>();

        // อ่านทั้งไฟล์เข้าหน่วยความจำก่อน — เปิดได้แม้ลูกค้าเปิด Excel ค้างไว้ และไม่ถือไฟล์
        // บนแชร์ไว้ระหว่างแปลง
        using var memory = new MemoryStream();
        using (var file = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            file.CopyTo(memory);
        memory.Position = 0;

        using var book = new XLWorkbook(memory);

        foreach (var table in Tables)
        {
            var schema = current.Tables[table];
            var sheet = book.Worksheets.FirstOrDefault(w => string.Equals(w.Name.Trim(), table, StringComparison.OrdinalIgnoreCase));
            if (sheet == null)
            {
                problems.Add($"ไม่มีชีต {table}");
                continue;
            }

            var rows = ReadSheet(sheet, schema, problems);
            if (rows != null) data[table] = rows;
        }

        return (data, problems);
    }

    private static List<string?[]>? ReadSheet(IXLWorksheet sheet, TableSchema schema, List<string> problems)
    {
        var name = schema.Name;
        var lastColumn = sheet.LastColumnUsed(XLCellsUsedOptions.Contents)?.ColumnNumber() ?? 0;
        var lastRow = sheet.LastRowUsed(XLCellsUsedOptions.Contents)?.RowNumber() ?? 0;

        // หัวคอลัมน์ → เลขคอลัมน์ใน Excel
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        var duplicate = new List<string>();
        var untitled = new List<string>();
        for (int c = 1; c <= lastColumn; c++)
        {
            var header = sheet.Cell(1, c).GetString().Trim();
            if (header.Length == 0)
            {
                if (!sheet.Column(c).CellsUsed(XLCellsUsedOptions.Contents).Any()) continue;
                untitled.Add(sheet.Cell(1, c).Address.ColumnLetter);
                continue;
            }
            if (!map.TryAdd(header, c)) duplicate.Add(header);
        }

        var missing = schema.Columns.Where(c => !map.ContainsKey(c)).ToList();
        var extra = map.Keys.Where(h => !schema.Columns.Contains(h, StringComparer.Ordinal)).ToList();

        int before = problems.Count;
        if (missing.Count > 0) problems.Add($"ชีต {name}: ไม่มีคอลัมน์ {string.Join(", ", missing)}");
        if (extra.Count > 0) problems.Add($"ชีต {name}: มีคอลัมน์ที่ไม่รู้จัก {string.Join(", ", extra)}");
        if (duplicate.Count > 0) problems.Add($"ชีต {name}: หัวคอลัมน์ซ้ำ {string.Join(", ", duplicate.Distinct())}");
        if (untitled.Count > 0) problems.Add($"ชีต {name}: คอลัมน์ {string.Join(", ", untitled)} มีข้อมูลแต่ไม่มีชื่อหัว");
        if (problems.Count > before) return null;

        var rows = new List<string?[]>();
        var keys = new Dictionary<string, int>(StringComparer.Ordinal);
        int lotIndex = schema.Columns.IndexOf("lot_no");
        int markingIndex = name == "plan_routing" ? schema.Columns.IndexOf("marking_method") : -1;
        var keyIndexes = schema.Key.Select(k => schema.Columns.IndexOf(k)).ToList();

        for (int r = 2; r <= lastRow; r++)
        {
            var values = new string?[schema.Columns.Count];
            for (int c = 0; c < values.Length; c++)
            {
                var cell = sheet.Cell(r, map[schema.Columns[c]]);
                values[c] = CellText(cell, out var error);
                if (error != null) problems.Add($"ชีต {name} ช่อง {cell.Address}: {error}");
            }

            // แถวว่างทั้งแถวข้ามไป — Excel มักมีแถวว่างคั่นหรือค้างท้ายตาราง
            if (values.All(v => v == null)) continue;

            if (lotIndex >= 0 && values[lotIndex] == null)
                problems.Add($"ชีต {name} แถว {r}: ไม่มี lot_no");

            // marking_method เป็นรหัสสองหลักเสมอ — "02" ที่ถูก Excel ตัดเหลือ "2"
            // ทำให้อ่านสายการผลิตผิด (MarkingMethodService อ่านทีละหลัก)
            if (markingIndex >= 0 && values[markingIndex] is { } marking && !Regex.IsMatch(marking, @"^\d{2}$"))
                problems.Add($"ชีต {name} แถว {r}: marking_method ต้องเป็นเลขสองหลัก เช่น 02 แต่ได้ \"{marking}\" (ตั้งช่องนี้ใน Excel เป็นข้อความ)");

            if (keyIndexes.Count > 0 && keyIndexes.All(i => i >= 0 && values[i] != null))
            {
                var key = string.Join("\u001f", keyIndexes.Select(i => values[i]));
                if (keys.TryGetValue(key, out var firstRow))
                    problems.Add($"ชีต {name} แถว {r}: {string.Join(", ", keyIndexes.Select(i => values[i]))} ซ้ำกับแถว {firstRow}");
                else
                    keys[key] = r;
            }

            rows.Add(values);
        }

        return rows;
    }

    /// <summary>
    /// ค่าในช่องเป็นข้อความแบบที่ PrintData.db3 เก็บ — ช่องว่างเป็น NULL
    /// ตัวเลขไม่มี ".0" ห้อยท้าย ส่วนรูปแบบเลขศูนย์นำหน้า ("00") คงศูนย์ไว้ตามที่เห็นใน Excel
    /// </summary>
    private static string? CellText(IXLCell cell, out string? error)
    {
        error = null;

        // ช่องสูตรใช้ผลที่ Excel คำนวณเก็บไว้ในไฟล์ — ไฟล์ที่โปรแกรมอื่นสร้างอาจไม่มีผลเก็บไว้
        // ต้องคำนวณเอง ไม่งั้นช่องนั้นกลายเป็นค่าว่างเงียบ ๆ
        XLCellValue value;
        if (!cell.HasFormula)
            value = cell.Value;
        else
        {
            value = cell.CachedValue;
            if (value.IsBlank)
            {
                try { value = cell.Value; }
                catch { error = "คำนวณสูตรไม่ได้ — เปิดไฟล์ใน Excel แล้วเซฟใหม่ หรือใส่เป็นค่าแทนสูตร"; return null; }
            }
        }

        switch (value.Type)
        {
            case XLDataType.Blank:
                return null;
            case XLDataType.Text:
                var text = value.GetText();
                return text.Length == 0 ? null : text;
            case XLDataType.Number:
                return NumberText(value.GetNumber(), cell.Style.NumberFormat.Format);
            case XLDataType.Boolean:
                return value.GetBoolean() ? "TRUE" : "FALSE";
            case XLDataType.DateTime:
                var date = value.GetDateTime();
                return date.TimeOfDay == TimeSpan.Zero
                    ? date.ToString("yyyy-MM-dd", Inv)
                    : date.ToString("yyyy-MM-dd HH:mm:ss", Inv);
            case XLDataType.TimeSpan:
                return value.GetTimeSpan().ToString("c", Inv);
            case XLDataType.Error:
                error = $"เป็นค่า error {value.GetError()} — แก้สูตรหรือลบค่าออก";
                return null;
            default:
                return value.ToString();
        }
    }

    private static string NumberText(double number, string? format)
    {
        bool whole = Math.Abs(number) < 1e15 && number == Math.Floor(number);
        if (!whole) return number.ToString("R", Inv);

        var integer = (long)number;
        return format is { Length: > 1 } && format.All(ch => ch == '0')
            ? integer.ToString(format, Inv)
            : integer.ToString(Inv);
    }

    /// <summary>ตารางที่แถวหายเกินครึ่งจากไฟล์เดิม — ส่วนใหญ่คือส่งไฟล์ผิดหรือชีตไม่ครบ</summary>
    private static List<string> Shrunk(CurrentDb current, Dictionary<string, List<string?[]>> data) =>
        Tables
            .Where(t => current.Tables[t].Rows > 0 && data[t].Count * 2 < current.Tables[t].Rows)
            .Select(t => $"{t}: เดิม {current.Tables[t].Rows:N0} แถว · Excel {data[t].Count:N0} แถว")
            .ToList();

    // ─── สร้างไฟล์ใหม่ ──────────────────────────────────────────────────

    /// <summary>
    /// สร้าง db3 ใหม่ในเครื่องตัวเอง — โครงสร้างเดียวกับไฟล์เดิมทุกตาราง (ใช้ CREATE เดิม)
    /// ข้อมูลสามตารางจาก Excel ตารางอื่นยกจากไฟล์เดิม แล้วบันทึกว่ามาจาก Excel ไฟล์ไหน
    /// </summary>
    private static void Build(string path, CurrentDb current, Dictionary<string, List<string?[]>> data,
        Dictionary<string, string> meta)
    {
        using var conn = new SqliteConnection(SqlitePath.ReadWriteCreate(path));
        conn.Open();
        using var tx = conn.BeginTransaction();

        foreach (var schema in current.Tables.Values)
        {
            Execute(conn, tx, schema.Sql);

            var rows = data.TryGetValue(schema.Name, out var fromExcel)
                ? fromExcel.Select(r => r.Cast<object?>().ToArray())
                : current.OtherRows.GetValueOrDefault(schema.Name) ?? [];
            Insert(conn, tx, schema, rows);
        }

        // ไฟล์ที่ไม่มีตารางนี้ก็สร้างให้ ไม่งั้นเปิดโปรแกรมทุกครั้งจะไม่รู้ว่าแปลง Excel ตัวนี้ไปแล้ว
        if (!current.Tables.ContainsKey(MetaTable))
            Execute(conn, tx, $"CREATE TABLE {Quote(MetaTable)} (key TEXT PRIMARY KEY, value TEXT)");

        foreach (var (key, value) in meta)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = $"INSERT OR REPLACE INTO {Quote(MetaTable)} (key, value) VALUES ($k, $v)";
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$v", value);
            cmd.ExecuteNonQuery();
        }

        foreach (var sql in current.OtherObjects) Execute(conn, tx, sql);

        tx.Commit();
    }

    private static void Insert(SqliteConnection conn, SqliteTransaction tx, TableSchema schema, IEnumerable<object?[]> rows)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText =
            $"INSERT INTO {Quote(schema.Name)} ({string.Join(", ", schema.Columns.Select(Quote))}) " +
            $"VALUES ({string.Join(", ", schema.Columns.Select((_, i) => "$p" + i))})";
        var parameters = schema.Columns.Select((_, i) => cmd.Parameters.Add("$p" + i, SqliteType.Text)).ToArray();
        cmd.Prepare();

        foreach (var row in rows)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var value = i < row.Length ? row[i] : null;
                parameters[i].SqliteType = value switch
                {
                    long or int => SqliteType.Integer,
                    double => SqliteType.Real,
                    byte[] => SqliteType.Blob,
                    _ => SqliteType.Text,
                };
                parameters[i].Value = value ?? DBNull.Value;
            }
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>เปิดไฟล์ที่เพิ่งสร้างใหม่ ไฟล์ต้องสมบูรณ์และจำนวนแถวตรงกับ Excel</summary>
    private static void Verify(string path, Dictionary<string, List<string?[]>> data)
    {
        using var conn = new SqliteConnection(SqlitePath.ReadOnly(path));
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA integrity_check";
            var result = Convert.ToString(cmd.ExecuteScalar(), Inv);
            if (result != "ok") throw new InvalidOperationException($"ไฟล์ที่สร้างใหม่ไม่สมบูรณ์: {result}");
        }

        foreach (var table in Tables)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {Quote(table)}";
            var count = Convert.ToInt32(cmd.ExecuteScalar(), Inv);
            if (count != data[table].Count)
                throw new InvalidOperationException($"ตาราง {table} ได้ {count} แถว แต่ Excel มี {data[table].Count} แถว");
        }
    }

    // ─── สลับไฟล์ ───────────────────────────────────────────────────────

    /// <summary>
    /// เอาไฟล์ใหม่ไปวางแทนไฟล์บนแชร์ — คืนข้อความปัญหา หรือ null เมื่อสำเร็จ
    ///
    /// <para>
    /// ก๊อปไปไว้ข้างไฟล์จริงก่อน แล้วค่อยเปลี่ยนชื่อทับ การเปลี่ยนชื่อในโฟลเดอร์เดียวกัน
    /// เกิดทีเดียวทั้งไฟล์ เครื่องอื่นไม่มีทางเปิดเจอไฟล์ที่เขียนไปครึ่งเดียว
    /// ไฟล์เดิมก๊อปเก็บเป็น .bak ก่อน — ใช้ก๊อป ไม่ใช่ย้าย ไฟล์จริงจึงไม่มีช่วงที่หายไป
    /// </para>
    /// <para>
    /// เครื่องอื่นเปิดไฟล์อยู่ (กำลังสแกน หรือรันเวอร์ชันเก่าที่ถือไฟล์ค้าง) Windows ไม่ให้ทับ
    /// รอแล้วลองใหม่ไม่กี่ครั้ง ยังไม่ได้ก็เลิก ไฟล์เดิมไม่ถูกแตะ
    /// </para>
    /// </summary>
    private static string? SwapIn(string localFile, string dbPath)
    {
        var staged = $"{dbPath}.{Environment.MachineName}.importing";
        try
        {
            File.Copy(localFile, staged, overwrite: true);
            if (new FileInfo(staged).Length != new FileInfo(localFile).Length)
                return "ก๊อปไฟล์ใหม่ไปที่แชร์ไม่ครบ ใช้ข้อมูลเดิมต่อ";

            if (File.Exists(dbPath)) File.Copy(dbPath, BackupPath(dbPath), overwrite: true);

            // รอให้การอ่านของโปรแกรมนี้จบก่อนสลับไฟล์ ลดการชนกับหน้าสแกน/ส่งออก
            // การล็อกนี้ครอบคลุมเฉพาะโปรเซสนี้; เครื่องอื่นยังอาจกำลังเปิดไฟล์อยู่
            using var writeLock = PrintDataDbLock.Write();
            SqliteConnection.ClearAllPools();
            const int attempts = 5;
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.Move(staged, dbPath, overwrite: true);
                    return null;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException && attempt < attempts)
                {
                    Thread.Sleep(1000);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    AppLog.Warn("ExcelImport", $"สลับไฟล์ไม่ได้: {ex.Message}");
                    return "PrintData.db3 ถูกเปิดใช้อยู่ที่เครื่องอื่น สลับไฟล์ใหม่ไม่ได้ ใช้ข้อมูลเดิมต่อ\n"
                           + "ตรวจว่าทุกเครื่องอัปเดตโปรแกรมแล้ว และไม่มีใครเปิดไฟล์นี้ค้างไว้ แล้วกดแปลงอีกครั้ง";
                }
            }
        }
        finally
        {
            TryDelete(staged);
        }
    }

    // ─── ตัวช่วย ────────────────────────────────────────────────────────

    private static ImportResult Fail(Outcome outcome, string message)
    {
        if (outcome != Outcome.Unchanged) AppLog.Warn("ExcelImport", message.Replace('\n', ' '));
        return new ImportResult(outcome, message);
    }

    private static string Summarize(List<string> problems)
    {
        var shown = problems.Take(MaxProblems).Select(p => "• " + p);
        var more = problems.Count > MaxProblems ? $"\n…และอีก {problems.Count - MaxProblems} จุด" : "";
        return string.Join("\n", shown) + more;
    }

    private static void Execute(SqliteConnection conn, SqliteTransaction tx, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
