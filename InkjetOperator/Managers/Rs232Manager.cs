using System.IO.Ports;
using System.Text;

namespace InkjetOperator.Managers;

/// <summary>
/// Serial port manager for MK Compact inkjets — follows Linx SerialPortManager pattern.
/// MK Compact uses text-based protocol: send command string, read until '\r'.
/// Ported from rs232_connector.py send_data() (lines 25-34).
/// </summary>
public class Rs232Manager
{
    private SerialPort _serialPort;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    public Rs232Manager()
    {
        _serialPort = new SerialPort();
    }

    public void ConfigureSerialPort(string portName, int baudRate = 9600,
        int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None)
    {
        try
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stopBits;
            _serialPort.Parity = parity;
            _serialPort.ReadTimeout = 2000; // 2s timeout, matching Python s.timeout = 2
            _serialPort.Encoding = Encoding.ASCII;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Configuration error: " + ex.Message);
        }
    }

    public void OpenSerialPort()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error opening serial port: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void CloseSerialPort()
    {
        try
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error closing serial port: " + ex.Message);
        }
    }

    public bool IsOpen()
    {
        return _serialPort.IsOpen;
    }

    /// <summary>
    /// Send a command string and read response until '\r'.
    /// Ported from rs232_connector.py send_data():
    ///   s.write(msg)
    ///   data = s.read_until(expected=b'\r')
    ///   if data == '': return False  # connection problem
    /// </summary>
    public async Task<string> SendCommandAsync(string command)
    {
        if (!_serialPort.IsOpen)
        {
            return "";
        }

        try
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            _serialPort.Write(commandBytes, 0, commandBytes.Length);
            await Task.Delay(5);

            // Read until \r
            string response = "";
            try
            {
                response = _serialPort.ReadTo("\r");
            }
            catch (TimeoutException)
            {
                // No response within timeout — connection problem
                return "";
            }

            OnDataReceived(response);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Send error: " + ex.Message);
            return "";
        }
    }

    private void OnDataReceived(string data)
    {
        DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
    }
}

public class DataReceivedEventArgs : EventArgs
{
    public string Data { get; private set; }
    public DataReceivedEventArgs(string data) { Data = data; }
}
