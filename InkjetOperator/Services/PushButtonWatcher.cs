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
        if (_disposed) return; // ตัวอ่านถูกปิดถาวรแล้ว ไม่เริ่มใหม่
        Stop(); // หยุดรอบเดิมก่อนโหลดค่าชุดใหม่

        PushButtonSettings.Saved += OnSettingsSaved; // รับการเปลี่ยนค่าปุ่มแม้ตอนนี้ยังปิดใช้งานอยู่
        _settings = PushButtonSettings.Load(); // โหลดบิตและปลายทาง PLC ชุดล่าสุด
        if (!_settings.IsReady || _settings.Validate() != null) return; // ค่าปุ่มไม่พร้อมหรือไม่ถูกต้อง ให้หยุดก่อน

        _buttons = _settings.Watched().Select(w => (w.Address, w.Machine)).ToArray(); // ST1 เฝ้าสามปุ่ม ส่วน ST3 ไม่มีรายการอ่าน
        if (_buttons.Length == 0) return; // PC นี้ไม่มีปุ่มที่ต้องอ่าน

        _lastOn = new bool[_buttons.Length]; // จำค่าปุ่มแยกรายเครื่องสำหรับจับการกดใหม่
        PlanBatchRead(); // วางช่วงอ่านรวม ถ้าบิต M อยู่ไม่ห่างกันเกินกำหนด

        _failures = 0; // เริ่มนับการอ่านพลาดใหม่
        _resync = true; // ให้ค่าแรกเป็นฐาน ไม่ถือว่ากดปุ่ม
        _timer.Interval = _settings.PollMs; // ใช้รอบอ่านจากค่าตั้ง
        _timer.Start(); // เริ่มอ่านปุ่มตามเวลา
    }

    private void PlanBatchRead() // หาช่วงบิต M ที่อ่านรวมได้
    {
        _batchCount = 0; // เริ่มจากอ่านแยกไว้ก่อน จนตรวจว่ารวมช่วงได้
        _offsets = new int[_buttons.Length]; // จำตำแหน่งของแต่ละปุ่มในชุดบิตที่อ่านรวม

        var numbers = new int[_buttons.Length]; // เก็บเลข M ของแต่ละปุ่มเพื่อหาช่วงอ่าน
        for (int i = 0; i < _buttons.Length; i++) // ตรวจ address ของทุกปุ่มที่เฝ้า
        {
            if (!_buttons[i].Address.StartsWith("M", StringComparison.OrdinalIgnoreCase)) return; // รวมอ่านได้เฉพาะปุ่มที่เป็นบิต M
            if (!McProtocolService.TryParseAddress(_buttons[i].Address, out _, out numbers[i], out _)) return; // แปลงเลขบิตไม่ได้ ให้ใช้ทางอ่านแยก
        }

        int min = numbers.Min(), max = numbers.Max(); // หาบิตแรกและบิตสุดท้ายที่ต้องอ่าน
        if (max - min + 1 > MaxBatchPoints) return; // ช่วงกว้างเกินกำหนดให้กลับไปอ่านทีละปุ่ม

        _batchStart = min; // ตั้งบิตเริ่มอ่านรวม
        _batchCount = max - min + 1; // อ่านให้ครอบคลุมทุกปุ่มในครั้งเดียว
        for (int i = 0; i < numbers.Length; i++) _offsets[i] = numbers[i] - min; // จำว่าค่าปุ่มแต่ละตัวอยู่ช่องใดในผลอ่าน
    }

    private async Task<(bool ok, bool[] on, string error)> ReadAllAsync() // อ่านค่าปุ่มที่เฝ้าจาก PLC
    {
        if (_batchCount > 0) // มีช่วงบิตที่อ่านรวมได้แล้ว
        {
            var (ok, bits, error) = await McProtocolService.ReadBitsAsync( // อ่านหลายบิตจาก PLC ในคำขอเดียว
                _settings.Ip, _settings.Port, $"M{_batchStart}", _batchCount); // ใช้ช่วง M ที่คำนวณไว้จากค่าปุ่ม

            if (!ok) return (false, [], error); // ส่งเหตุที่อ่าน PLC ไม่ได้ให้ผู้เรียก

            var picked = new bool[_buttons.Length]; // เตรียมผลตามจำนวนปุ่มที่ใช้จริง
            for (int i = 0; i < picked.Length; i++) picked[i] = bits[_offsets[i]]; // ดึงเฉพาะบิตของปุ่มที่เฝ้า ไม่ใช้บิตคั่นกลาง
            return (true, picked, ""); // ส่งผลปุ่มที่แยกออกจากชุดบิตแล้ว
        }

        var result = new bool[_buttons.Length]; // เตรียมผลสำหรับการอ่านทีละปุ่ม
        for (int i = 0; i < _buttons.Length; i++) // อ่านให้ครบทุกปุ่มตามลำดับ
        {
            var (ok, on, error) = await McProtocolService.ReadBitAsync( // ขอค่าบิตของปุ่มปัจจุบัน
                _settings.Ip, _settings.Port, _buttons[i].Address); // ใช้ IP พอร์ต และ address ของปุ่มนี้

            if (!ok) return (false, [], error); // ส่งเหตุที่อ่าน PLC ไม่ได้ให้ผู้เรียก
            result[i] = on; // จำค่า ON หรือ OFF ของปุ่มที่อ่านได้
        }

        return (true, result, ""); // ส่งค่าทุกปุ่มเมื่ออ่านครบ
    }

    public void Stop() // หยุดอ่านและยกเลิกค่ารอบเก่า
    {
        _generation++; // ทำให้ผลอ่านที่เริ่มด้วยค่าเก่าหมดอายุ
        PushButtonSettings.Saved -= OnSettingsSaved; // ถอดการรับเหตุการณ์เดิมก่อนหยุดหรือโหลดใหม่
        _timer.Stop(); // หยุดตัวจับเวลาอ่าน PLC
        _resync = true; // ให้ค่าอ่านครั้งถัดไปเป็นฐาน ไม่ถือว่ากดปุ่ม
    }

    private void OnSettingsSaved(object? sender, EventArgs e) => Start(); // เปลี่ยนค่าตั้งแล้วเริ่มอ่านด้วยค่าใหม่

    private async Task TickAsync() // อ่านรอบใหม่แล้วจับปุ่มที่เปลี่ยนจาก 0 เป็น 1
    {
        if (_reading || !Running || _disposed) return; // ไม่อ่านซ้อนหรืออ่านต่อหลังหยุด

        if (ShouldWatch?.Invoke() == false) // ผู้ใช้หน้านี้ยังไม่อนุญาตให้เฝ้าปุ่ม
        {
            _resync = true; // ให้ค่าอ่านครั้งถัดไปเป็นฐาน ไม่ถือว่ากดปุ่ม
            return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
        }

        _reading = true; // กันตัวจับเวลาเริ่มอ่านซ้อน
        int generation = _generation; // จำว่ารอบนี้เริ่มอ่านด้วยค่าตั้งชุดใด
        try // ดักข้อผิดพลาดของขั้นนี้
        {
            var (ok, on, error) = await ReadAllAsync(); // อ่านค่าปุ่มทุกเครื่องที่ PC นี้รับผิดชอบ

            if (generation != _generation || _disposed || !Running) return; // ไม่ใช้ผลอ่านเก่าหลังเปลี่ยน Address หรือหยุดเฝ้า

            if (!ok) // ตรวจกรณีทำรายการไม่ผ่าน
            {
                OnFailure(error); // นับรอบพลาดและแจ้งปัญหาตามเกณฑ์
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            OnSuccess(); // คืนรอบอ่านปกติเมื่อ PLC ตอบแล้ว

            if (_resync) // รอบแรกหลังเริ่มใหม่ยังไม่นับการกด
            {
                Array.Copy(on, _lastOn, on.Length); // รอบแรกจำค่าไว้ก่อน ไม่นับบิตค้างเป็นการกด
                _resync = false; // รอบหน้าเริ่มเทียบกับค่าครั้งนี้
                return; // จบขั้นนี้ ไม่ทำส่วนถัดไป
            }

            for (int i = 0; i < _buttons.Length; i++) // ตรวจการเปลี่ยนค่าทีละปุ่ม
            {
                bool rising = on[i] && !_lastOn[i]; // นับเฉพาะปุ่มที่เปลี่ยนจาก 0 เป็น 1

                _lastOn[i] = on[i]; // จำค่าก่อนแจ้งเหตุการณ์ เพื่อไม่ปล่อยซ้ำตอนกดค้าง
                if (!rising) continue; // ค่าไม่ได้เปลี่ยนเป็น ON ให้ข้าม

                var machine = _buttons[i].Machine; // เลือกเครื่องที่ตรงกับปุ่มนี้

                if (CanAct?.Invoke(machine) == false) BlockedPress?.Invoke(this, machine); // เครื่องนี้ยังส่งอยู่ ให้แจ้งกดใหม่ ไม่สะสมไว้ทำทีหลัง
                else Pressed?.Invoke(this, machine); // ให้ Order List ปล่อยคิวเฉพาะเครื่องที่ถูกกด
            }
        }
        finally // ทำส่วนนี้เสมอ แม้ขั้นก่อนหน้ามีปัญหา
        {
            _reading = false; // เปิดให้รอบถัดไปอ่านค่าได้
        }
    }

    private void OnFailure(string error) // จัดการรอบที่อ่าน PLC ไม่สำเร็จ
    {
        _failures++; // เพิ่มจำนวนครั้งที่อ่านพลาดติดกัน
        if (_failures != FailuresBeforeTrouble) return; // ยังไม่ถึงเกณฑ์หรือแจ้งแล้ว ไม่แจ้งซ้ำ

        _timer.Interval = RetryMs; // อ่านห่างขึ้นระหว่าง PLC มีปัญหา
        Trouble?.Invoke(this, error); // แจ้งหน้า Order List ว่าอ่านปุ่มไม่ได้
    }

    private void OnSuccess() // คืนรอบอ่านปกติเมื่อ PLC กลับมาตอบ
    {
        if (_failures == 0) return; // ไม่ได้มีปัญหาก่อนหน้า ไม่ต้องคืนสถานะ

        bool wasTrouble = _failures >= FailuresBeforeTrouble; // จำว่ารอบก่อนถึงเกณฑ์แจ้งปัญหาแล้วไหม
        _failures = 0; // ล้างจำนวนครั้งที่อ่านพลาด
        _timer.Interval = _settings.PollMs; // กลับมาใช้รอบอ่านตาม Setting

        _resync = true; // ให้ค่าอ่านครั้งถัดไปเป็นฐาน ไม่ถือว่ากดปุ่ม

        if (wasTrouble) Trouble?.Invoke(this, null); // แจ้งว่าอ่านปุ่มได้อีกครั้ง
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _timer.Dispose();
    }
}
