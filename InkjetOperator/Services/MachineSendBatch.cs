namespace InkjetOperator.Services;

/// <summary>เริ่มส่งคนละเครื่องพร้อมกัน แต่หนึ่งชุดหยิบได้เครื่องละหนึ่งรอบ</summary>
public static class MachineSendBatch
{
    public static async Task<TResult[]> RunAsync<TItem, TResult>(
        IEnumerable<TItem> items, Func<TItem, string> machine,
        Func<TItem, Task<TResult>> send, Func<TItem, Exception, TResult> failed)
    {
        // 22 มี MK สองรอบ ห้ามส่งรอบสองตามรอบแรกไปโดยไม่มีคนปล่อยเครื่อง
        var firstPerMachine = items.GroupBy(machine, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        return await Task.WhenAll(firstPerMachine.Select(SendOneAsync));

        async Task<TResult> SendOneAsync(TItem item)
        {
            try { return await send(item); }
            catch (Exception ex) { return failed(item, ex); }
        }
    }
}
