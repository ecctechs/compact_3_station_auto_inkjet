namespace InkjetOperator.Services;

public static class ConnectionPreflight
{
    public static async Task<bool[]> CheckAsync( // ตรวจปลายทางหลายเครื่องพร้อมกัน
        IReadOnlyList<(string Host, int Port)> targets, Func<string, int, Task<bool>> connect)
    {
        var checks = new Dictionary<(string Host, int Port), Task<bool>>(); // แชร์ผลตรวจเมื่อมีหลายรายการใช้ IP และพอร์ตเดียวกัน
        var results = new List<Task<bool>>(); // จำผลตามลำดับรายการที่ผู้เรียกส่งมา
        foreach (var (host, port) in targets) // เตรียมตรวจทุกปลายทางที่งานนี้ต้องใช้
        {
            var key = (host.Trim().ToUpperInvariant(), port); // ทำชื่อปลายทางให้เทียบกันได้ก่อนกันรายการซ้ำ
            if (!checks.TryGetValue(key, out var check)) // ปลายทางนี้ยังไม่มีงานตรวจในชุดเดียวกัน
                checks[key] = check = CheckOneAsync(host, port); // เริ่มตรวจทันทีโดยไม่รอปลายทางก่อนหน้า
            results.Add(check); // ผูกผลกลับไปยังเครื่องที่ร้องขอ
        }
        return await Task.WhenAll(results); // รอครบทุกเครื่องก่อนตัดสินให้เริ่มงาน

        async Task<bool> CheckOneAsync(string host, int port)
        {
            try { return await connect(host, port); }
            catch { return false; }
        }
    }
}
