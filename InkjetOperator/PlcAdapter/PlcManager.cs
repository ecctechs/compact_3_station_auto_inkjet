using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.PlcAdapter;

public class PlcManager
{
    private TcpClient? _client;
    private NetworkStream? _stream;

    public async Task ConnectAsync(string host, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine("PLC connection error: " + ex.Message);
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
            Console.WriteLine("PLC disconnect error: " + ex.Message);
        }
    }

    public bool IsConnected()
    {
        return _client?.Connected ?? false;
    }

    public Task<bool> WriteServoAsync(int ordinal, int position, int postAct, int delay, int trigger)
    {
        Console.WriteLine($"PLC WriteServo stub: ordinal={ordinal} pos={position} postAct={postAct} delay={delay} trigger={trigger}");
        return Task.FromResult(false);
    }

    public Task<bool> WriteSpeedAsync(int speed1, int speed2, int speed3)
    {
        Console.WriteLine($"PLC WriteSpeed stub: s1={speed1} s2={speed2} s3={speed3}");
        return Task.FromResult(false);
    }
}
