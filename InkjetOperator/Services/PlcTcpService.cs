using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NModbus;

namespace InkjetOperator.Services
{
    public class PlcTcpService : IDisposable
    {
        private TcpClient? _tcp;
        private IModbusMaster? _master;

        public bool IsConnected => _tcp?.Connected == true;

        public (bool ok, string log) Connect(string ip, int port)
        {
            try
            {
                Disconnect();
                _tcp = new TcpClient();
                _tcp.Connect(ip, port);
                var factory = new ModbusFactory();
                _master = factory.CreateMaster(_tcp);
                _master.Transport.ReadTimeout = 3000;
                _master.Transport.WriteTimeout = 3000;
                return (true, $"Connected to {ip}:{port}");
            }
            catch (Exception ex)
            {
                return (false, $"Connect failed: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _master?.Dispose();
            _master = null;
            _tcp?.Close();
            _tcp = null;
        }

        public (bool ok, string log, ushort[]? data) ReadRegisters(int startAddress, int count)
        {
            if (_master == null)
                return (false, "Not connected", null);

            try
            {
                var data = _master.ReadHoldingRegisters(0, (ushort)startAddress, (ushort)count);
                return (true, $"Read {count} registers from {startAddress}", data);
            }
            catch (Exception ex)
            {
                return (false, $"Read error: {ex.Message}", null);
            }
        }

        public (bool ok, string log) WriteRegisters(int startAddress, ushort[] values)
        {
            if (_master == null)
                return (false, "Not connected");

            try
            {
                _master.WriteMultipleRegisters(0, (ushort)startAddress, values);
                return (true, $"Write {values.Length} registers at {startAddress}");
            }
            catch (Exception ex)
            {
                return (false, $"Write error: {ex.Message}");
            }
        }

        public (bool ok, string log, string value) ReadString(int startAddress, int registerCount)
        {
            var (ok, log, data) = ReadRegisters(startAddress, registerCount);
            if (!ok || data == null)
                return (false, log, "");

            var bytes = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                bytes[i * 2] = (byte)(data[i] >> 8);
                bytes[i * 2 + 1] = (byte)(data[i] & 0xFF);
            }
            string text = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
            return (true, log, text);
        }

        public (bool ok, string log) WriteString(int startAddress, string text, int registerCount)
        {
            var bytes = new byte[registerCount * 2];
            var src = Encoding.ASCII.GetBytes(text);
            Array.Copy(src, bytes, Math.Min(src.Length, bytes.Length));

            var regs = new ushort[registerCount];
            for (int i = 0; i < registerCount; i++)
                regs[i] = (ushort)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);

            return WriteRegisters(startAddress, regs);
        }

        public (bool ok, string log, int value) ReadInt32(int startAddress)
        {
            var (ok, log, data) = ReadRegisters(startAddress, 2);
            if (!ok || data == null)
                return (false, log, 0);

            int val = (data[0] << 16) | data[1];
            return (true, log, val);
        }

        public (bool ok, string log) WriteInt32(int startAddress, int value)
        {
            var regs = new ushort[]
            {
                (ushort)(value >> 16),
                (ushort)(value & 0xFFFF)
            };
            return WriteRegisters(startAddress, regs);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
