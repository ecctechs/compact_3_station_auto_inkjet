using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.PlcAdapter;

/// <summary>
/// Stub PLC manager — TCP connection to PLC for servo + conveyor speed.
/// Command format TBD from subcontractor.
/// Replaces plc_interface.py Modbus write_multiple_registers.
/// </summary>
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

    /// <summary>
    /// Write servo parameters to PLC.
    /// From plc_interface.py write_servo(): writes 4 registers (position, post_act, delay, trigger).
    /// Command format TBD — currently stubbed.
    /// </summary>
    public Task<bool> WriteServoAsync(int ordinal, int position, int postAct, int delay, int trigger)
    {
        // TODO: Implement when PLC TCP command format is provided by subcontractor
        Console.WriteLine($"PLC WriteServo stub: ordinal={ordinal} pos={position} postAct={postAct} delay={delay} trigger={trigger}");
        return Task.FromResult(false);
    }

    /// <summary>
    /// Write conveyor speed to PLC.
    /// From plc_interface.py write_speed(): writes 3 registers (speed1, speed2, speed3).
    /// Command format TBD — currently stubbed.
    /// </summary>
    public Task<bool> WriteSpeedAsync(int speed1, int speed2, int speed3)
    {
        // TODO: Implement when PLC TCP command format is provided by subcontractor
        Console.WriteLine($"PLC WriteSpeed stub: s1={speed1} s2={speed2} s3={speed3}");
        return Task.FromResult(false);
    }
}
