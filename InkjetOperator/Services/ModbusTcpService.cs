using System.Net.Sockets;

namespace InkjetOperator.Services;

/// <summary>
/// Minimal Modbus TCP client for FX5U — read/write holding registers only.
/// One TCP connection per call (connect → send → receive → close).
/// </summary>
public static class ModbusTcpService
{
    private const byte UnitId = 1;
    private const int TimeoutMs = 3000;
    private static ushort _transactionId;

    public static async Task<(bool ok, int[] values, string error)> ReadHoldingRegistersAsync(
        string ip, int port, int startAddress, int quantity)
    {
        if (quantity <= 0 || quantity > 125)
            return (false, [], "Quantity ต้องอยู่ระหว่าง 1-125");

        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            var stream = tcp.GetStream();

            var txId = ++_transactionId;
            var request = BuildReadRequest(txId, startAddress, quantity);
            await stream.WriteAsync(request);
            await stream.FlushAsync();

            var header = new byte[9];
            await ReadExactAsync(stream, header, 9);

            var respTxId = (ushort)((header[0] << 8) | header[1]);
            var fc = header[7];

            if ((fc & 0x80) != 0)
            {
                var errorCode = header[8];
                return (false, [], $"Modbus error: FC=0x{fc:X2} code=0x{errorCode:X2}");
            }

            var byteCount = header[8];
            var data = new byte[byteCount];
            await ReadExactAsync(stream, data, byteCount);

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

    public static async Task<(bool ok, string error)> WriteSingleRegisterAsync(
        string ip, int port, int address, int value)
    {
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            var stream = tcp.GetStream();

            var txId = ++_transactionId;
            var request = BuildWriteRequest(txId, address, (ushort)value);
            await stream.WriteAsync(request);
            await stream.FlushAsync();

            var response = new byte[12];
            await ReadExactAsync(stream, response, 12);

            var fc = response[7];
            if ((fc & 0x80) != 0)
            {
                var errorCode = response[8];
                return (false, $"Modbus error: FC=0x{fc:X2} code=0x{errorCode:X2}");
            }

            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static byte[] BuildReadRequest(ushort txId, int startAddress, int quantity)
    {
        // FC 0x03 = Read Holding Registers
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
        // FC 0x06 = Write Single Register
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
