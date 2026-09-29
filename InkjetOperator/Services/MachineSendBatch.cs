namespace InkjetOperator.Services;

public static class MachineSendBatch
{
    public static async Task<TResult[]> RunAsync<TItem, TResult>( // ส่งพร้อมกันได้เครื่องละหนึ่งรอบ
        IEnumerable<TItem> items, Func<TItem, string> machine,
        Func<TItem, Task<TResult>> send, Func<TItem, Exception, TResult> failed)
    {
        var firstPerMachine = items.GroupBy(machine, StringComparer.OrdinalIgnoreCase) // จัดคิวตามเครื่อง เพื่อไม่ส่งเครื่องเดียวสองรอบพร้อมกัน
            .Select(group => group.First()).ToArray(); // เอาแค่รอบแรกของแต่ละเครื่องในชุดนี้
        return await Task.WhenAll(firstPerMachine.Select(SendOneAsync)); // คนละเครื่องส่งพร้อมกันและเก็บผลแยกกัน

        async Task<TResult> SendOneAsync(TItem item)
        {
            try { return await send(item); }
            catch (Exception ex) { return failed(item, ex); } // เครื่องหนึ่งพังยังเก็บผลของเครื่องอื่นได้
        }
    }
}
