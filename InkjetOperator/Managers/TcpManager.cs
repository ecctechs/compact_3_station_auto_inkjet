using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.Managers;

public class TcpManager
{
    private TcpClient? _client;
    private NetworkStream? _stream;

    private const int CommandTimeoutMs = 3000;
    private readonly SemaphoreSlim _sendGate = new(1, 1);

    public event EventHandler<TcpDataReceivedEventArgs>? DataReceived;

    public TcpManager() { }

    public async Task ConnectAsync(string ipAddress, int port)
    {
        try
        {
            _client = new TcpClient();
            using var timeout = new CancellationTokenSource(CommandTimeoutMs);
            await _client.ConnectAsync(ipAddress, port, timeout.Token);
            _stream = _client.GetStream();
        }
        catch (Exception ex)
        {
            Disconnect();
            Console.WriteLine("TCP connection error: " + ex.Message);
            throw;
        }
    }

    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("TCP disconnect error: " + ex.Message);
        }
    }

    public bool IsConnected()
    {
        return _client?.Connected ?? false;
    }

    public async Task<string> SendCommandAsync(string command)
    {
        var stream = _stream;
        if (stream == null || !IsConnected()) return "";
        await _sendGate.WaitAsync();
        try
        {
            if (!ReferenceEquals(stream, _stream) || !IsConnected()) return "";
            using var timeout = new CancellationTokenSource(CommandTimeoutMs);
            byte[] cmd = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(cmd.AsMemory(), timeout.Token);
            await stream.FlushAsync(timeout.Token);

            byte[] buffer = new byte[16];
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(), timeout.Token);
            if (bytesRead == 0) throw new IOException("MK ปิดการเชื่อมต่อโดยไม่ตอบกลับ");
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\r', '\n');
            OnDataReceived(response);
            await Task.Delay(50); // เว้นช่วงคำสั่งตามเดิม
            return response;
        }
        catch (OperationCanceledException)
        {
            if (ReferenceEquals(stream, _stream)) Disconnect();
            throw new TimeoutException("MK ไม่ตอบกลับภายใน 3 วินาที — ตรวจสอบผลก่อนส่งซ้ำ");
        }
        catch
        {
            if (ReferenceEquals(stream, _stream)) Disconnect();
            throw;
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private void OnDataReceived(string data)
    {
        DataReceived?.Invoke(this, new TcpDataReceivedEventArgs(data));
    }
}

public class TcpDataReceivedEventArgs : EventArgs
{
    public string Data { get; private set; }
    public TcpDataReceivedEventArgs(string data) { Data = data; }
}
