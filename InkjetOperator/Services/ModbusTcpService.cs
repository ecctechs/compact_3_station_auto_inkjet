using System.Net.Sockets;

namespace InkjetOperator.Services;

/// <summary>
/// Minimal Modbus TCP client for FX5U — read/write holding registers only.
///
/// <para>
/// เรียกแบบครั้งเดียวจบ (เปิดสาย → ส่ง → รับ → ปิด) ใช้กับปุ่มเดี่ยว ๆ เช่นปุ่ม Write
/// ในตาราง register map ถ้าต้องยิงหลายคำสั่งติดกัน ให้เปิด <see cref="Session"/>
/// ค้างไว้แทน ไม่งั้นค่าเสียเวลาเปิดสายจะคูณตามจำนวนคำสั่ง ซึ่งเห็นชัดมากตอน PLC
/// ไม่ตอบ เพราะทุกครั้งต้องรอจนครบ timeout
/// </para>
/// </summary>
public static class ModbusTcpService
{
    private const byte UnitId = 1;
    private const int TimeoutMs = 3000;
    private static ushort _transactionId;

    /// <summary>
    /// สายที่เปิดค้างไว้ ยิงได้หลายคำสั่งต่อกัน
    ///
    /// <para>
    /// หนึ่งงานต้องเขียน 5 register แล้วอ่านกลับทีละตัว ถ้าเปิดปิดสายทุกคำสั่งจะเป็น
    /// 10 รอบ ตอน PLC ต่อไม่ติดคือรอ timeout 3 วินาที 10 ครั้ง = 30 วินาทีต่อการ
    /// กดส่งหนึ่งครั้ง เปิดสายเดียวแล้วใช้ซ้ำทำให้เหลือรอครั้งเดียว
    /// </para>
    /// <para>
    /// ตัวคำสั่งกับลำดับบนสายไม่เปลี่ยน ยังเป็น FC 6 เขียนทีละตัวแล้ว FC 3 อ่านกลับ
    /// เหมือนเดิมทุกประการ ต่างแค่ไม่ปิดสายคั่นกลาง
    /// </para>
    /// </summary>
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

        /// <summary>บิตบนสุดของ function code ติด = PLC ตอบว่าไม่รับคำสั่ง</summary>
        private static string? ErrorIn(byte[] response) =>
            (response[7] & 0x80) == 0
                ? null
                : $"Modbus error: FC=0x{response[7]:X2} code=0x{response[8]:X2}";

        public void Dispose() => _tcp.Dispose();
    }

    /// <summary>เปิดสายค้างไว้เพื่อยิงหลายคำสั่ง — คืน null พร้อมสาเหตุเมื่อต่อไม่ติด</summary>
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

    /// <summary>
    /// เขียนหลาย register ติดกันในคำสั่งเดียว (FC 16)
    ///
    /// ค่าที่ส่งให้ PLC ของสายนี้เป็นชุด เช่น servo ของเครื่องหนึ่งคือ 4 ตัวเรียงกัน
    /// ถ้าเขียนทีละตัวด้วย FC 6 PLC จะเห็นค่าครึ่ง ๆ กลาง ๆ ระหว่างทาง
    /// </summary>
    public static async Task<(bool ok, string error)> WriteMultipleRegistersAsync(
        string ip, int port, int startAddress, IReadOnlyList<int> values)
    {
        var (session, error) = await Session.OpenAsync(ip, port);
        if (session == null) return (false, error);

        using (session) return await session.WriteMultipleRegistersAsync(startAddress, values);
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

    private static byte[] BuildWriteMultipleRequest(
        ushort txId, int startAddress, IReadOnlyList<int> values)
    {
        // FC 0x10 = Write Multiple Registers
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
            // ค่าลบส่งเป็น two's complement 16 บิต ให้ตรงกับที่ฝั่งอ่านตีความเป็น short
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
