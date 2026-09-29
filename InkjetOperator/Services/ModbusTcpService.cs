using System.Net.Sockets;

namespace InkjetOperator.Services;

public static class ModbusTcpService
{
    private const byte UnitId = 1;
    private const int TimeoutMs = 3000;
    private static ushort _transactionId;

    public sealed class Session : IDisposable
    {
        private readonly TcpClient _tcp;
        private readonly NetworkStream _stream;

        private Session(TcpClient tcp)
        {
            _tcp = tcp;
            _stream = tcp.GetStream();
        }

        internal static async Task<(Session? session, string error)> OpenAsync(string ip, int port)
        {
            var tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
                return (new Session(tcp), "");
            }
            catch (Exception ex)
            {
                tcp.Dispose();
                return (null, ex.Message);
            }
        }

        public async Task<(bool ok, int[] values, string error)> ReadHoldingRegistersAsync(
            int startAddress, int quantity)
        {
            if (quantity <= 0 || quantity > 125)
                return (false, [], "Quantity ต้องอยู่ระหว่าง 1-125");

            try
            {
                await SendAsync(BuildReadRequest(++_transactionId, startAddress, quantity));

                var header = new byte[9];
                await ReadExactAsync(_stream, header, 9);

                if (ErrorIn(header) is string problem) return (false, [], problem);

                var byteCount = header[8];
                var data = new byte[byteCount];
                await ReadExactAsync(_stream, data, byteCount);

                var values = new int[quantity];
                for (int i = 0; i < quantity; i++)
                    values[i] = (short)((data[i * 2] << 8) | data[i * 2 + 1]);

                return (true, values, "");
            }
            catch (Exception ex)
            {
                return (false, [], ex.Message);
            }
        }

        public Task<(bool ok, string error)> WriteSingleRegisterAsync(int address, int value) =>
            WriteAsync(BuildWriteRequest(++_transactionId, address, (ushort)value));

        public Task<(bool ok, string error)> WriteMultipleRegistersAsync(
            int startAddress, IReadOnlyList<int> values) =>
            values.Count is 0 or > 123
                ? Task.FromResult((false, "จำนวน register ต้องอยู่ระหว่าง 1-123"))
                : WriteAsync(BuildWriteMultipleRequest(++_transactionId, startAddress, values));

        private async Task<(bool ok, string error)> WriteAsync(byte[] request)
        {
            try
            {
                await SendAsync(request);

                var response = new byte[12];
                await ReadExactAsync(_stream, response, 12);

                return ErrorIn(response) is string problem ? (false, problem) : (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task SendAsync(byte[] request)
        {
            await _stream.WriteAsync(request);
            await _stream.FlushAsync();
        }

        private static string? ErrorIn(byte[] response) =>
            (response[7] & 0x80) == 0
                ? null
                : $"Modbus error: FC=0x{response[7]:X2} code=0x{response[8]:X2}";

        public void Dispose() => _tcp.Dispose();
    }

    public static Task<(Session? session, string error)> OpenAsync(string ip, int port) =>
        Session.OpenAsync(ip, port);

    public static async Task<(bool ok, int[] values, string error)> ReadHoldingRegistersAsync(
        string ip, int port, int startAddress, int quantity)
    {
        var (session, error) = await Session.OpenAsync(ip, port);
        if (session == null) return (false, [], error);

        using (session) return await session.ReadHoldingRegistersAsync(startAddress, quantity);
    }

    public static async Task<(bool ok, string error)> WriteSingleRegisterAsync(
        string ip, int port, int address, int value)
    {
        var (session, error) = await Session.OpenAsync(ip, port);
        if (session == null) return (false, error);

        using (session) return await session.WriteSingleRegisterAsync(address, value);
    }

    public static async Task<(bool ok, string error)> WriteMultipleRegistersAsync(
        string ip, int port, int startAddress, IReadOnlyList<int> values)
    {
        var (session, error) = await Session.OpenAsync(ip, port);
        if (session == null) return (false, error);

        using (session) return await session.WriteMultipleRegistersAsync(startAddress, values);
    }

    private static byte[] BuildReadRequest(ushort txId, int startAddress, int quantity)
    {
        return
        [
            (byte)(txId >> 8), (byte)(txId & 0xFF),     // Transaction ID
            0x00, 0x00,                                   // Protocol ID
            0x00, 0x06,                                   // Length
            UnitId,                                       // Unit ID
            0x03,                                         // Function Code
            (byte)(startAddress >> 8), (byte)(startAddress & 0xFF),
            (byte)(quantity >> 8), (byte)(quantity & 0xFF),
        ];
    }

    private static byte[] BuildWriteRequest(ushort txId, int address, ushort value)
    {
        return
        [
            (byte)(txId >> 8), (byte)(txId & 0xFF),
            0x00, 0x00,
            0x00, 0x06,
            UnitId,
            0x06,
            (byte)(address >> 8), (byte)(address & 0xFF),
            (byte)(value >> 8), (byte)(value & 0xFF),
        ];
    }

    private static byte[] BuildWriteMultipleRequest(
        ushort txId, int startAddress, IReadOnlyList<int> values)
    {
        byte count = (byte)values.Count;
        byte byteCount = (byte)(count * 2);
        int length = 7 + byteCount;   // unit + fc + addr + qty + bytecount + data

        var request = new byte[6 + length];
        request[0] = (byte)(txId >> 8);
        request[1] = (byte)(txId & 0xFF);
        request[2] = 0x00;
        request[3] = 0x00;
        request[4] = (byte)(length >> 8);
        request[5] = (byte)(length & 0xFF);
        request[6] = UnitId;
        request[7] = 0x10;
        request[8] = (byte)(startAddress >> 8);
        request[9] = (byte)(startAddress & 0xFF);
        request[10] = 0x00;
        request[11] = count;
        request[12] = byteCount;

        for (int i = 0; i < values.Count; i++)
        {
            ushort value = (ushort)(short)values[i];
            request[13 + i * 2] = (byte)(value >> 8);
            request[14 + i * 2] = (byte)(value & 0xFF);
        }

        return request;
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset))
                .AsTask().WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            if (read == 0) throw new IOException("Connection closed");
            offset += read;
        }
    }
}
