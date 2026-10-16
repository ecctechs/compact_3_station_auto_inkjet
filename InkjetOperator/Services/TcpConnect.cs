using System.Net.Sockets;

namespace InkjetOperator.Services;

/// <summary>
/// ต่อ TCP แบบมีเพดานเวลา ที่ยกเลิกการต่อจริงเมื่อหมดเวลา
/// <para>
/// เดิมทุกที่เขียน <c>tcp.ConnectAsync(...).WaitAsync(timeout)</c> ซึ่งแค่ "เลิกรอ"
/// ตัวการต่อข้างในยังวิ่งอยู่ พอ TcpClient ถูก Dispose มันล้มทีหลังโดยไม่มีใครรับผล
/// กลายเป็น unobserved exception ลง log ทุกครั้งที่เครื่องปลายทางไม่ได้เปิด
/// (เจอใน log วันเดียวเกือบ 300 บรรทัด) และค้าง socket ไว้จนกว่า GC จะเก็บ
/// </para>
/// <para>
/// ตัวนี้ส่ง CancellationToken เข้าไปให้ socket ยกเลิกเอง จึงไม่มีงานค้าง
/// หมดเวลาแล้วโยน TimeoutException ข้อความเดียวกับ WaitAsync ผู้เรียกจึงจับ error
/// และแสดงข้อความได้เหมือนเดิมทุกประการ
/// </para>
/// </summary>
public static class TcpConnect
{
    public static async Task ConnectAsync(TcpClient client, string host, int port, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await client.ConnectAsync(host, port, cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }
}
