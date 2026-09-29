using System.Net.Sockets;

namespace InkjetOperator.Services;

public static class McProtocolService
{
    private const int TimeoutMs = 3000;

    private const byte DevD = 0xA8; // Data register   — เลขที่อยู่เป็นฐานสิบ
    private const byte DevM = 0x90; // Internal relay  — เลขที่อยู่เป็นฐานสิบ
    private const byte DevW = 0xB4; // Link register   — เลขที่อยู่เป็นฐานสิบหก

    private const ushort CmdBatchRead = 0x0401;
    private const ushort CmdBatchWrite = 0x1401;
    private const ushort SubWord = 0x0000;
    private const ushort SubBit = 0x0001;

    public static async Task<(bool ok, string error)> WriteWordAsync(
        string ip, int port, string address, int value)
    {
        if (!TryParseAddress(address, out byte code, out int number, out string parseError))
            return (false, parseError);

        var data = new List<byte>(DeviceSpec(code, number, 1))
        {
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF),
        };

        var (ok, _, error) = await SendAsync(ip, port, CmdBatchWrite, SubWord, data.ToArray(), 0);
        return (ok, error);
    }

    public static async Task<(bool ok, string error)> WriteBitAsync(
        string ip, int port, string address, bool on)
    {
        if (!TryParseAddress(address, out byte code, out int number, out string parseError))
            return (false, parseError);

        var data = new List<byte>(DeviceSpec(code, number, 1)) { (byte)(on ? 0x10 : 0x00) };

        var (ok, _, error) = await SendAsync(ip, port, CmdBatchWrite, SubBit, data.ToArray(), 0);
        return (ok, error);
    }

    public static async Task<(bool ok, int value, string error)> ReadWordAsync(
        string ip, int port, string address)
    {
        if (!TryParseAddress(address, out byte code, out int number, out string parseError))
            return (false, 0, parseError);

        var (ok, payload, error) = await SendAsync(
            ip, port, CmdBatchRead, SubWord, DeviceSpec(code, number, 1), 2);

        if (!ok) return (false, 0, error);
        if (payload.Length < 2) return (false, 0, "ตอบกลับสั้นกว่าที่ควร");

        return (true, (short)(payload[0] | (payload[1] << 8)), "");
    }

    public static async Task<(bool ok, bool on, string error)> ReadBitAsync(
        string ip, int port, string address)
    {
        var (ok, bits, error) = await ReadBitsAsync(ip, port, address, 1);
        return (ok, ok && bits[0], error);
    }

    public static async Task<(bool ok, bool[] bits, string error)> ReadBitsAsync(
        string ip, int port, string address, int count)
    {
        if (count < 1) return (false, [], "จำนวนจุดต้องมากกว่า 0");

        if (!TryParseAddress(address, out byte code, out int number, out string parseError))
            return (false, [], parseError);

        int expected = (count + 1) / 2;

        var (ok, payload, error) = await SendAsync(
            ip, port, CmdBatchRead, SubBit, DeviceSpec(code, number, (ushort)count), expected);

        if (!ok) return (false, [], error);
        if (payload.Length < expected) return (false, [], "ตอบกลับสั้นกว่าที่ควร");

        var bits = new bool[count];
        for (int i = 0; i < count; i++)
        {
            byte pair = payload[i / 2];
            bits[i] = (i % 2 == 0 ? pair >> 4 : pair & 0x0F) != 0;
        }

        return (true, bits, "");
    }

    public static bool TryParseAddress(string? text, out byte code, out int number, out string error)
    {
        code = 0;
        number = 0;
        error = "";

        string s = (text ?? "").Trim().ToUpperInvariant();
        if (s.Length < 2)
        {
            error = $"ที่อยู่ไม่ถูกต้อง: \"{text}\"";
            return false;
        }

        char dev = s[0];
        string digits = s[1..];

        int radix;
        switch (dev)
        {
            case 'D': code = DevD; radix = 10; break;
            case 'M': code = DevM; radix = 10; break;
            case 'W': code = DevW; radix = 16; break;
            default:
                error = $"ยังไม่รองรับ device \"{dev}\" (รองรับ D, M, W)";
                return false;
        }

        try
        {
            number = Convert.ToInt32(digits, radix);
        }
        catch
        {
            error = $"เลขที่อยู่ไม่ถูกต้อง: \"{text}\"" +
                    (radix == 16 ? " (W ต้องเป็นเลขฐานสิบหก)" : "");
            return false;
        }

        if (number < 0)
        {
            error = $"เลขที่อยู่ติดลบ: \"{text}\"";
            return false;
        }

        return true;
    }

    private static byte[] DeviceSpec(byte code, int number, ushort points) =>
    [
        (byte)(number & 0xFF),
        (byte)((number >> 8) & 0xFF),
        (byte)((number >> 16) & 0xFF),
        code,
        (byte)(points & 0xFF),
        (byte)(points >> 8),
    ];

    private static byte[] BuildFrame(ushort command, ushort subcommand, byte[] requestData)
    {
        int dataLen = 6 + requestData.Length;

        var frame = new List<byte>(11 + dataLen)
        {
            0x50, 0x00,       // subheader (request)
            0x00,             // network no
            0xFF,             // PC no
            0xFF, 0x03,       // request destination module I/O no (0x03FF)
            0x00,             // request destination module station no
            (byte)(dataLen & 0xFF), (byte)(dataLen >> 8),
            0x10, 0x00,       // monitoring timer = 16 × 250ms = 4 วินาที
            (byte)(command & 0xFF), (byte)(command >> 8),
            (byte)(subcommand & 0xFF), (byte)(subcommand >> 8),
        };

        frame.AddRange(requestData);
        return frame.ToArray();
    }

    private static async Task<(bool ok, byte[] payload, string error)> SendAsync(
        string ip, int port, ushort command, ushort subcommand, byte[] requestData, int expectedPayload)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return (false, [], "ยังไม่ได้ตั้ง IP ของ PLC แคลมป์");

        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            var stream = tcp.GetStream();

            await stream.WriteAsync(BuildFrame(command, subcommand, requestData));
            await stream.FlushAsync();

            var header = new byte[11];
            await ReadExactAsync(stream, header, 11);

            if (header[0] != 0xD0)
                return (false, [], $"ตอบกลับผิดรูปแบบ (subheader=0x{header[0]:X2})");

            int respLen = header[7] | (header[8] << 8);      // นับตั้งแต่ end code
            int endCode = header[9] | (header[10] << 8);

            int remaining = Math.Max(0, respLen - 2);
            var payload = new byte[remaining];
            if (remaining > 0)
                await ReadExactAsync(stream, payload, remaining);

            if (endCode != 0)
                return (false, [], $"PLC ตอบ error code 0x{endCode:X4}");

            if (expectedPayload > 0 && payload.Length < expectedPayload)
                return (false, [], $"ข้อมูลกลับมา {payload.Length} ไบต์ คาดไว้ {expectedPayload}");

            return (true, payload, "");
        }
        catch (Exception ex)
        {
            return (false, [], ex.Message);
        }
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset))
                .AsTask().WaitAsync(TimeSpan.FromMilliseconds(TimeoutMs));
            if (read == 0) throw new IOException("PLC ปิดการเชื่อมต่อกลางคัน");
            offset += read;
        }
    }
}
