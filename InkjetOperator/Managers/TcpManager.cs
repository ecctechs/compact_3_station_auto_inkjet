using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.Managers;

/// <summary>
/// TCP socket manager — follows Linx TcpClientManager pattern.
/// Used for MK Compact inkjets over TCP (same protocol as RS232, different transport).
/// Ported from socket_client.py.
/// </summary>
public class TcpManager
{
    private TcpClient? _client;
    private NetworkStream? _stream;

    private const int CommandTimeoutMs = 3000;
    private readonly SemaphoreSlim _sendGate = new(1, 1);

    public event EventHandler<TcpDataReceivedEventArgs>? DataReceived;

    public TcpManager() { }

    /// <summary>
    /// Connect to TCP endpoint with a bounded asynchronous timeout.
    /// </summary>
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

    /// <summary>
    /// Send command and receive response.
    /// From socket_client.py: sock.sendall(msg), data = sock.recv(16)
    /// รอทีละคำสั่ง เพื่อให้ผู้เรียกได้คำตอบของตัวเอง
    /// </summary>
    public async Task<string> SendCommandAsync(string command)
    {
        var stream = _stream;
        if (stream == null || !IsConnected()) return "";
        await _sendGate.WaitAsync();
        try
        {
            // คำสั่งที่รออยู่ห้ามข้ามไปส่งบน connection ใหม่หลังรอบก่อนหมดเวลา
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
            // ปิด socket เพื่อไม่ให้คำตอบที่มาช้าถูกนับเป็นคำตอบของคำสั่งถัดไป
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
