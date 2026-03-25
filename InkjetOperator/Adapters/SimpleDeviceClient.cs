using System.Diagnostics;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;

namespace InkjetOperator.Adapters.Simple
{
    public interface IDeviceClient : IDisposable
    {
        Task ConnectAsync(string hostOrPort, int? port = null, CancellationToken ct = default);
        Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default);
        bool IsConnected { get; }
    }

    // TCP socket implementation (port required)
    public class SocketDeviceClient : IDeviceClient
    {
        private TcpClient? _tcp;
        private NetworkStream? _stream;
        private readonly Encoding _enc = Encoding.ASCII;

        public bool IsConnected => _tcp?.Connected ?? false;

        public async Task ConnectAsync(string host, int? port = null, CancellationToken ct = default)
        {
            if (!port.HasValue) throw new ArgumentException("port required for SocketDeviceClient");
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(host, port.Value, ct);
            _stream = _tcp.GetStream();
            _stream.ReadTimeout = 4000;
            _stream.WriteTimeout = 4000;
        }

        public async Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default)
        {
            if (_stream == null) throw new InvalidOperationException("Not connected");
            byte[] data = _enc.GetBytes(command + "\r");
            await _stream.WriteAsync(data, 0, data.Length, ct);

            // read response (non-blocking with timeout)
            var sb = new StringBuilder();
            var buffer = new byte[1024];
            var readCts = new CancellationTokenSource(timeoutMs);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, readCts.Token);
            try
            {
                // read available bytes until timeout or newline/carriage return
                while (!linked.IsCancellationRequested)
                {
                    if (_stream.DataAvailable)
                    {
                        int n = await _stream.ReadAsync(buffer, 0, buffer.Length, linked.Token);
                        if (n == 0) break;
                        sb.Append(_enc.GetString(buffer, 0, n));
                        // break early if CR or LF seen
                        if (sb.ToString().IndexOf('\r') >= 0 || sb.ToString().IndexOf('\n') >= 0) break;
                    }
                    else
                    {
                        await Task.Delay(20, linked.Token);
                    }
                }
            }
            catch (OperationCanceledException) { /* timeout or cancel */ }

            return sb.ToString().Trim();
        }

        public void Dispose()
        {
            try { _stream?.Close(); } catch { }
            try { _tcp?.Close(); } catch { }
            _stream = null;
            _tcp = null;
        }
    }

    // Serial (RS232) implementation using System.IO.Ports.SerialPort
    public class SerialDeviceClient : IDeviceClient
    {
        private SerialPort? _port;
        private readonly Encoding _enc = Encoding.ASCII;

        public bool IsConnected => _port != null && _port.IsOpen;

        // hostOrPort for serial is COM port name, port parameter unused
        public Task ConnectAsync(string comName, int? unused = null, CancellationToken ct = default)
        {
            _port = new SerialPort(comName)
            {
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 3000,
                WriteTimeout = 3000,
                Encoding = _enc
            };
            _port.Open();
            return Task.CompletedTask;
        }

        public Task<string> SendCommandAsync(string command, int timeoutMs = 2000, CancellationToken ct = default)
        {
            if (_port == null || !_port.IsOpen) throw new InvalidOperationException("Serial port not open");
            string cmd = command + "\r";
            _port.DiscardInBuffer();
            _port.Write(cmd);
            var sw = Stopwatch.StartNew();
            var sb = new StringBuilder();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    string? line = _port.ReadExisting();
                    if (!string.IsNullOrEmpty(line))
                    {
                        sb.Append(line);
                        if (line.IndexOf('\r') >= 0 || line.IndexOf('\n') >= 0) break;
                    }
                }
                catch (TimeoutException) { }
                Thread.Sleep(20);
            }
            return Task.FromResult(sb.ToString().Trim());
        }

        public void Dispose()
        {
            try { if (_port != null && _port.IsOpen) _port.Close(); } catch { }
            _port = null;
        }
    }
}