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

    private readonly Queue<byte[]> _sendQueue = new();
    private bool _isSending = false;
    private readonly object _sendLock = new();

    public event EventHandler<TcpDataReceivedEventArgs>? DataReceived;

    public TcpManager() { }

    /// <summary>
    /// Connect to TCP endpoint.
    /// From socket_client.py lines 33-49:
    ///   socket.socket(AF_INET, SOCK_STREAM), settimeout(0.1), connect(address)
    /// </summary>
    public async Task ConnectAsync(string ipAddress, int port)
    {
        try
        {
            _client = new TcpClient();
            _client.ReceiveTimeout = 100; // 0.1s timeout matching Python
            _client.SendTimeout = 100;
            await _client.ConnectAsync(ipAddress, port);
            _stream = _client.GetStream();
        }
        catch (Exception ex)
        {
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
    /// Uses queue pattern from Linx TcpClientManager to prevent send collisions.
    /// </summary>
    public async Task<string> SendCommandAsync(string command)
    {
        if (_stream == null || !IsConnected())
        {
            return "";
        }

        byte[] commandBytes = Encoding.ASCII.GetBytes(command);

        lock (_sendLock)
        {
            _sendQueue.Enqueue(commandBytes);
        }

        return await ProcessSendQueueAsync();
    }

    private async Task<string> ProcessSendQueueAsync()
    {
        if (_isSending) return "";
        _isSending = true;

        string lastResponse = "";

        try
        {
            while (true)
            {
                byte[] cmd;
                lock (_sendLock)
                {
                    if (_sendQueue.Count == 0) break;
                    cmd = _sendQueue.Dequeue();
                }

                await _stream!.WriteAsync(cmd, 0, cmd.Length);
                await _stream.FlushAsync();

                // Read response (recv(16) in Python)
                byte[] buffer = new byte[16];
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        lastResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\r', '\n');
                        OnDataReceived(lastResponse);
                    }
                }
                catch (IOException)
                {
                    // Read timeout — no response
                    lastResponse = "";
                }

                await Task.Delay(50); // Prevent rapid succession (Linx pattern)
            }
        }
        finally
        {
            _isSending = false;
        }

        return lastResponse;
    }

    private void OnDataReceived(string data)
    {
        DataReceived?.Invoke(this, new TcpDataReceivedEventArgs(data));
    }

    public void Dispose()
    {
        Disconnect();
    }
}

public class TcpDataReceivedEventArgs : EventArgs
{
    public string Data { get; private set; }
    public TcpDataReceivedEventArgs(string data) { Data = data; }
}
