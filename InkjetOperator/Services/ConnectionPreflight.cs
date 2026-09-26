namespace InkjetOperator.Services;

/// <summary>ตรวจคนละปลายทางพร้อมกัน แต่ไม่เปิดซ็อกเก็ตซ้ำไป IP/port เดียวกันในชุดเดียว</summary>
public static class ConnectionPreflight
{
    public static async Task<bool[]> CheckAsync(
        IReadOnlyList<(string Host, int Port)> targets, Func<string, int, Task<bool>> connect)
    {
        var checks = new Dictionary<(string Host, int Port), Task<bool>>();
        var results = new List<Task<bool>>();
        foreach (var (host, port) in targets)
        {
            var key = (host.Trim().ToUpperInvariant(), port);
            if (!checks.TryGetValue(key, out var check))
                checks[key] = check = CheckOneAsync(host, port);
            results.Add(check);
        }
        return await Task.WhenAll(results);

        async Task<bool> CheckOneAsync(string host, int port)
        {
            try { return await connect(host, port); }
            catch { return false; }
        }
    }
}
