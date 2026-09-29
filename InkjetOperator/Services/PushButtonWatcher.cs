namespace InkjetOperator.Services;

public sealed class PushButtonWatcher : IDisposable
{
    private const int FailuresBeforeTrouble = 3;

    private const int RetryMs = 5000;

    private const int MaxBatchPoints = 64;

    private readonly System.Windows.Forms.Timer _timer = new();

    private PushButtonSettings _settings = new();

    private (string Address, string Machine)[] _buttons = [];

    private bool[] _lastOn = [];

    private int[] _offsets = [];

    private int _batchStart;

    private int _batchCount;

    private bool _reading;
    private int _failures;
    private int _generation;
    private bool _disposed;

    private bool _resync = true;

    public PushButtonWatcher()
    {
        _timer.Tick += async (_, _) => await TickAsync();
    }

    public event EventHandler<string>? Pressed;

    public event EventHandler<string?>? Trouble;

    public event EventHandler<string>? BlockedPress;

    public Func<bool>? ShouldWatch { get; set; }

    public Func<string, bool>? CanAct { get; set; }

    public bool Running => _timer.Enabled;

    public string Watching =>
        Running ? string.Join(" · ", _buttons.Select(b => $"{b.Address}→{b.Machine}")) : "";

    public void Start() // โหลดค่าปุ่มแล้วเริ่มอ่าน PLC
    {
        if (_disposed) return;
        Stop();

        PushButtonSettings.Saved += OnSettingsSaved; // รับการเปลี่ยนค่าปุ่มแม้ตอนนี้ยังปิดใช้งานอยู่
        _settings = PushButtonSettings.Load(); // โหลดบิตและปลายทาง PLC ชุดล่าสุด
        if (!_settings.IsReady || _settings.Validate() != null) return;

        _buttons = _settings.Watched().Select(w => (w.Address, w.Machine)).ToArray(); // ST1 เฝ้าสามปุ่ม ส่วน ST3 ไม่มีรายการอ่าน
        if (_buttons.Length == 0) return;

        _lastOn = new bool[_buttons.Length]; // จำค่าปุ่มแยกรายเครื่องสำหรับจับการกดใหม่
        PlanBatchRead(); // วางช่วงอ่านรวม ถ้าบิต M อยู่ไม่ห่างกันเกินกำหนด

        _failures = 0;
        _resync = true; // ให้ค่าแรกเป็นฐาน ไม่ถือว่ากดปุ่ม
        _timer.Interval = _settings.PollMs; // ใช้รอบอ่านจากค่าตั้ง
        _timer.Start(); // เริ่มอ่านปุ่มตามเวลา
    }

    private void PlanBatchRead() // หาช่วงบิต M ที่อ่านรวมได้
    {
        _batchCount = 0; // เริ่มจากอ่านแยกไว้ก่อน จนตรวจว่ารวมช่วงได้
        _offsets = new int[_buttons.Length]; // จำตำแหน่งของแต่ละปุ่มในชุดบิตที่อ่านรวม

        var numbers = new int[_buttons.Length]; // เก็บเลข M ของแต่ละปุ่มเพื่อหาช่วงอ่าน
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (!_buttons[i].Address.StartsWith("M", StringComparison.OrdinalIgnoreCase)) return;
            if (!McProtocolService.TryParseAddress(_buttons[i].Address, out _, out numbers[i], out _)) return;
        }

        int min = numbers.Min(), max = numbers.Max(); // หาบิตแรกและบิตสุดท้ายที่ต้องอ่าน
        if (max - min + 1 > MaxBatchPoints) return; // ช่วงกว้างเกินกำหนดให้กลับไปอ่านทีละปุ่ม

        _batchStart = min; // ตั้งบิตเริ่มอ่านรวม
        _batchCount = max - min + 1; // อ่านให้ครอบคลุมทุกปุ่มในครั้งเดียว
        for (int i = 0; i < numbers.Length; i++) _offsets[i] = numbers[i] - min; // จำว่าค่าปุ่มแต่ละตัวอยู่ช่องใดในผลอ่าน
    }

    private async Task<(bool ok, bool[] on, string error)> ReadAllAsync() // อ่านค่าปุ่มที่เฝ้าจาก PLC
    {
        if (_batchCount > 0)
        {
            var (ok, bits, error) = await McProtocolService.ReadBitsAsync( // อ่านหลายบิตจาก PLC ในคำขอเดียว
                _settings.Ip, _settings.Port, $"M{_batchStart}", _batchCount); // ใช้ช่วง M ที่คำนวณไว้จากค่าปุ่ม

            if (!ok) return (false, [], error);

            var picked = new bool[_buttons.Length];
            for (int i = 0; i < picked.Length; i++) picked[i] = bits[_offsets[i]]; // ดึงเฉพาะบิตของปุ่มที่เฝ้า ไม่ใช้บิตคั่นกลาง
            return (true, picked, "");
        }

        var result = new bool[_buttons.Length];
        for (int i = 0; i < _buttons.Length; i++)
        {
            var (ok, on, error) = await McProtocolService.ReadBitAsync(
                _settings.Ip, _settings.Port, _buttons[i].Address);

            if (!ok) return (false, [], error);
            result[i] = on;
        }

        return (true, result, "");
    }

    public void Stop()
    {
        _generation++; // ทำให้ผลอ่านที่เริ่มด้วยค่าเก่าหมดอายุ
        PushButtonSettings.Saved -= OnSettingsSaved; // ถอดการรับเหตุการณ์เดิมก่อนหยุดหรือโหลดใหม่
        _timer.Stop();
        _resync = true;
    }

    private void OnSettingsSaved(object? sender, EventArgs e) => Start(); // เปลี่ยนค่าตั้งแล้วเริ่มอ่านด้วยค่าใหม่

    private async Task TickAsync() // อ่านรอบใหม่แล้วจับปุ่มที่เปลี่ยนจาก 0 เป็น 1
    {
        if (_reading || !Running || _disposed) return; // ไม่อ่านซ้อนหรืออ่านต่อหลังหยุด

        if (ShouldWatch?.Invoke() == false)
        {
            _resync = true;
            return;
        }

        _reading = true; // กันตัวจับเวลาเริ่มอ่านซ้อน
        int generation = _generation; // จำว่ารอบนี้เริ่มอ่านด้วยค่าตั้งชุดใด
        try
        {
            var (ok, on, error) = await ReadAllAsync(); // อ่านค่าปุ่มทุกเครื่องที่ PC นี้รับผิดชอบ

            if (generation != _generation || _disposed || !Running) return; // ไม่ใช้ผลอ่านเก่าหลังเปลี่ยน Address หรือหยุดเฝ้า

            if (!ok)
            {
                OnFailure(error);
                return;
            }

            OnSuccess(); // คืนรอบอ่านปกติเมื่อ PLC ตอบแล้ว

            if (_resync) // รอบแรกหลังเริ่มใหม่ยังไม่นับการกด
            {
                Array.Copy(on, _lastOn, on.Length); // รอบแรกจำค่าไว้ก่อน ไม่นับบิตค้างเป็นการกด
                _resync = false; // รอบหน้าเริ่มเทียบกับค่าครั้งนี้
                return;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                bool rising = on[i] && !_lastOn[i]; // นับเฉพาะปุ่มที่เปลี่ยนจาก 0 เป็น 1

                _lastOn[i] = on[i]; // จำค่าก่อนแจ้งเหตุการณ์ เพื่อไม่ปล่อยซ้ำตอนกดค้าง
                if (!rising) continue;

                var machine = _buttons[i].Machine; // เลือกเครื่องที่ตรงกับปุ่มนี้

                if (CanAct?.Invoke(machine) == false) BlockedPress?.Invoke(this, machine); // เครื่องนี้ยังส่งอยู่ ให้แจ้งกดใหม่ ไม่สะสมไว้ทำทีหลัง
                else Pressed?.Invoke(this, machine); // ให้ Order List ปล่อยคิวเฉพาะเครื่องที่ถูกกด
            }
        }
        finally
        {
            _reading = false; // เปิดให้รอบถัดไปอ่านค่าได้
        }
    }

    private void OnFailure(string error)
    {
        _failures++;
        if (_failures != FailuresBeforeTrouble) return;

        _timer.Interval = RetryMs;
        Trouble?.Invoke(this, error);
    }

    private void OnSuccess()
    {
        if (_failures == 0) return;

        bool wasTrouble = _failures >= FailuresBeforeTrouble;
        _failures = 0;
        _timer.Interval = _settings.PollMs;

        _resync = true;

        if (wasTrouble) Trouble?.Invoke(this, null);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _timer.Dispose();
    }
}
