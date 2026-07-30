using System.Data.SQLite;
using InkjetOperator.Services;

namespace InkjetOperator.Tests
{
    /// <summary>
    /// Integration tests ของ GetPlanRoutingAsync กับไฟล์ SQLite จริง (temp .db3)
    /// SqliteDataService อ่าน DB_PATH จาก config ตอน construct → ตั้งค่าผ่าน CustomSettingsManager ก่อนสร้าง
    /// </summary>
    public class SqliteDataServiceTests : IDisposable
    {
        private readonly string _dbFile;

        public SqliteDataServiceTests()
        {
            _dbFile = Path.Combine(Path.GetTempPath(), $"plantest_{Guid.NewGuid():N}.db3");
            CustomSettingsManager.SetValue("DB_PATH", _dbFile);
        }

        public void Dispose()
        {
            try { if (File.Exists(_dbFile)) File.Delete(_dbFile); } catch { /* ignore */ }
        }

        private void SeedDb(string createSql, params string[] inserts)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbFile};Version=3;");
            conn.Open();
            using (var cmd = new SQLiteCommand(createSql, conn)) cmd.ExecuteNonQuery();
            foreach (var ins in inserts)
                using (var cmd = new SQLiteCommand(ins, conn)) cmd.ExecuteNonQuery();
        }

        [Fact]
        public async Task GetPlanRouting_Found_MapsFieldsAndDecodes()
        {
            SeedDb(
                "CREATE TABLE plan_routing (lot_no TEXT, erp_mfg TEXT, marking_method TEXT, process_sequence TEXT);",
                "INSERT INTO plan_routing VALUES ('C260708-023','MFG-1','12','03');");

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("C260708-023");

            Assert.NotNull(plan);
            Assert.Equal("C260708-023", plan!.LotNo);
            Assert.Equal("MFG-1", plan.ErpMfg);
            Assert.Equal("12", plan.MarkingMethod);
            Assert.Equal("03", plan.ProcessSequence);
            // "12" = Shim=UV(UV2), Plate=Inkjet(Inkjet1)
            Assert.True(plan.SendUv2);
            Assert.True(plan.SendInkjet1);
            Assert.False(plan.SendUv1);
            Assert.False(plan.SendInkjet2);
        }

        [Fact]
        public async Task GetPlanRouting_TrimsLotBeforeMatch()
        {
            SeedDb(
                "CREATE TABLE plan_routing (lot_no TEXT, erp_mfg TEXT, marking_method TEXT, process_sequence TEXT);",
                "INSERT INTO plan_routing VALUES ('LOT1','M','01','01');");

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("  LOT1  ");

            Assert.NotNull(plan);
            Assert.Equal("LOT1", plan!.LotNo);
        }

        [Fact]
        public async Task GetPlanRouting_SupportsLogNoColumn()
        {
            // source บางเวอร์ชันใช้ชื่อคอลัมน์ log_no แทน lot_no
            SeedDb(
                "CREATE TABLE plan_routing (log_no TEXT, erp_mfg TEXT, marking_method TEXT, process_sequence TEXT);",
                "INSERT INTO plan_routing VALUES ('LOG9','M','20','02');");

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("LOG9");

            Assert.NotNull(plan);
            Assert.Equal("LOG9", plan!.LotNo);
            Assert.True(plan.SendInkjet2); // Shim=2
        }

        [Fact]
        public async Task GetPlanRouting_NotFound_ReturnsNull()
        {
            SeedDb("CREATE TABLE plan_routing (lot_no TEXT, erp_mfg TEXT, marking_method TEXT, process_sequence TEXT);");

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("NOPE");

            Assert.Null(plan);
        }

        [Fact]
        public async Task GetPlanRouting_NoTable_ReturnsNull()
        {
            SeedDb("CREATE TABLE other (x TEXT);"); // ไฟล์มี แต่ไม่มีตาราง plan_routing

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("X");

            Assert.Null(plan);
        }

        [Fact]
        public async Task GetPlanRouting_FileMissing_ReturnsNull()
        {
            CustomSettingsManager.SetValue("DB_PATH",
                Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.db3"));

            var svc = new SqliteDataService();
            var plan = await svc.GetPlanRoutingAsync("X");

            Assert.Null(plan);
        }
    }
}
