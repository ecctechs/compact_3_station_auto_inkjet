namespace InkjetOperator.Services;

public sealed class PushButtonSettings
{
    public static event EventHandler? Saved;
    public const int MaxPollMs = 900;

    private const int MinPollMs = 100;
    private const int DefaultPollMs = 300;

    public bool Enabled { get; set; }

    public string AddressSt1 { get; set; } = "";

    public string AddressSt2 { get; set; } = "";

    public string AddressSt3 { get; set; } = "";

    public string AddressFor(int station) => station switch
    {
        1 => AddressSt1.Trim(),
        2 => AddressSt2.Trim(),
        _ => AddressSt3.Trim(),
    };

    public static string MachineFor(int station) => station switch
    {
        1 => "MK",
        2 => "UV1",
        _ => "UV2",
    };

    public IEnumerable<(int Station, string Address, string Machine)> Watched() // เลือกปุ่มที่ PC นี้ต้องอ่าน
    {
        if (StationService.IsSt3) yield break; // ST3 ไม่อ่านปุ่มซ้ำ เพราะ ST1 อ่านทั้งสามปุ่มให้แล้ว

        foreach (var (station, address) in Addresses())
        {
            if (address.Length == 0) continue;
            yield return (station, address, MachineFor(station)); // ผูกบิตแต่ละปุ่มกับเครื่องที่ต้องปล่อยคิว
        }
    }

    public int PollMs { get; set; } = DefaultPollMs;

    public string Ip => CustomSettingsManager.Read("CLAMP_PLC_IP", "").Trim();

    public int Port =>
        int.TryParse(CustomSettingsManager.Read("CLAMP_PLC_PORT", "5012"), out int p) ? p : 5012;

    public bool IsReady => Enabled && Ip.Length > 0 && Watched().Any();

    public static PushButtonSettings Load() => new()
    {
        Enabled = CustomSettingsManager.Read("PUSHBTN_ENABLED", "0").Trim() == "1",
        AddressSt1 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST1", "").Trim(),
        AddressSt2 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST2", "").Trim(),

        AddressSt3 = CustomSettingsManager.Read("PUSHBTN_ADDRESS_ST3", "").Trim() is { Length: > 0 } st3
            ? st3
            : CustomSettingsManager.Read("PUSHBTN_ADDRESS", "").Trim(),

        PollMs = Clamp(CustomSettingsManager.Read("PUSHBTN_POLL_MS", "")),
    };

    public void Save() // บันทึกค่าปุ่มแล้วแจ้งตัวอ่านให้โหลดใหม่
    {
        CustomSettingsManager.Write("PUSHBTN_ENABLED", Enabled ? "1" : "0");
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST1", AddressSt1.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST2", AddressSt2.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_ADDRESS_ST3", AddressSt3.Trim().ToUpperInvariant());
        CustomSettingsManager.Write("PUSHBTN_POLL_MS", Clamp(PollMs.ToString()).ToString());
        Saved?.Invoke(this, EventArgs.Empty); // แจ้งตัวฟังปุ่มให้ใช้ค่าที่เพิ่งบันทึกโดยไม่ต้องเปิดใหม่
    }

    public string? Validate()
    {
        if (!Enabled) return null;

        foreach (var (station, address) in Addresses())
        {
            if (address.Length == 0) continue;
            if (CheckOne(station, address) is string problem) return problem;
        }

        if (StationService.IsSt3) return null;

        if (!Watched().Any())
            return "เปิดใช้งานปุ่มกดหน้างานแล้ว แต่ยังไม่ได้กรอก address ของปุ่มไหนเลย";

        if (Ip.Length == 0)
            return "เปิดใช้งานปุ่มกดหน้างานแล้ว แต่ยังไม่ได้ตั้ง IP ของ PLC แคลมป์ "
                 + "— กรอกที่หัวข้อ \"การเชื่อมต่อ\" ด้านบนของหน้านี้ก่อน";

        return null;
    }

    public IEnumerable<(int Station, string Address)> Addresses()
    {
        yield return (1, AddressSt1.Trim());
        yield return (2, AddressSt2.Trim());
        yield return (3, AddressSt3.Trim());
    }

    private static string? CheckOne(int station, string address)
    {
        if (!address.StartsWith("M", StringComparison.OrdinalIgnoreCase))
            return $"address ปุ่มกดของ ST{station} ต้องเป็น M เท่านั้น เช่น M800 (กรอกมาว่า \"{address}\")";

        return McProtocolService.TryParseAddress(address, out _, out _, out string error)
            ? null
            : $"ST{station}: {error}";
    }

    private static int Clamp(string text)
    {
        if (!int.TryParse(text.Trim(), out int ms)) return DefaultPollMs;
        return Math.Clamp(ms, MinPollMs, MaxPollMs);
    }
}
