namespace InkjetOperator.Services;

/// <summary>
/// เฝ้าดูบิตของปุ่มกดหน้างาน แล้วบอกเมื่อมีคนกด
///
/// <para>
/// PLC เป็นฝ่ายตั้งบิตเป็น 1 ค้างไว้ 1-2 วินาทีแล้วปล่อยกลับเป็น 0 เอง ฝั่งนี้อ่าน
/// อย่างเดียว ไม่เขียนกลับ จะได้ไม่แย่งกันคุมบิตเดียวกัน
/// </para>
/// <para>
/// <b>จับเฉพาะขอบขาขึ้น</b> คือจังหวะที่ค่าเปลี่ยนจาก 0 เป็น 1 เท่านั้น ถ้าจับที่ค่า
/// เป็น 1 เฉย ๆ การกดหนึ่งครั้งที่ค้างสองวินาทีจะกลายเป็นสั่งงานหกรอบ
/// </para>
/// <para>
/// ตั้งค่าที่หน้า PLC UV Setting หัวข้อปุ่มกดหน้างาน ใช้ PLC ตัวเดียวกับแคลมป์
/// </para>
/// </summary>
public sealed class PushButtonWatcher : IDisposable
{
    /// <summary>อ่านพลาดติดกันกี่ครั้งถึงจะถือว่าขาดการติดต่อจริง ไม่ใช่แค่สะดุด</summary>
    private const int FailuresBeforeTrouble = 3;

    /// <summary>ตอนขาดการติดต่อ ถ่างรอบให้ห่างขึ้น จะได้ไม่รัวใส่ PLC ที่ไม่ตอบอยู่แล้ว</summary>
    private const int RetryMs = 5000;

    /// <summary>
    /// ช่วงที่ยอมอ่านรวดเดียว — กว้างกว่านี้ถือว่าปุ่มอยู่กันคนละโซน อ่านทีละปุ่มแทน
    /// M4000-M4003 ของหน้างานกินแค่ 4 จุด จึงอ่านรวดเดียวได้เสมอ
    /// </summary>
    private const int MaxBatchPoints = 64;

    private readonly System.Windows.Forms.Timer _timer = new();

    private PushButtonSettings _settings = new();

    /// <summary>ปุ่มที่เครื่องนี้เฝ้าอยู่ — ว่างแปลว่ายังไม่ได้เริ่ม</summary>
    private (string Address, string Machine)[] _buttons = [];

    /// <summary>ค่าบิตรอบก่อนของแต่ละปุ่ม เรียงตรงกับ <see cref="_buttons"/></summary>
    private bool[] _lastOn = [];

    /// <summary>ตำแหน่งของแต่ละปุ่มในผลที่อ่านรวดเดียว — ใช้เมื่อ <see cref="_batchCount"/> &gt; 0</summary>
    private int[] _offsets = [];

    private int _batchStart;

    /// <summary>0 = อ่านรวดเดียวไม่ได้ ต้องไล่อ่านทีละปุ่ม</summary>
    private int _batchCount;

    private bool _reading;
    private int _failures;
    private int _generation;
    private bool _disposed;

    /// <summary>
    /// ครั้งแรกหลังเริ่มเฝ้าหรือหลังกลับมาจากช่วงพัก ให้จำค่าไว้เฉย ๆ ไม่ถือเป็นการกด
    ///
    /// <para>
    /// ถ้าไม่มีตัวนี้ เปิดโปรแกรมมาตอนที่บิตค้างเป็น 1 อยู่พอดี หรือเพิ่งส่งงานเสร็จ
    /// แล้วบิตยังไม่ตก จะถูกนับเป็นการกดทันทีทั้งที่ไม่มีใครกด
    /// </para>
    /// </summary>
    private bool _resync = true;

    public PushButtonWatcher()
    {
        _timer.Tick += async (_, _) => await TickAsync();
    }

    /// <summary>มีคนกดปุ่มหน้างาน — ค่าที่ส่งมาคือเครื่องที่ต้องปล่อย (MK / UV1 / UV2)</summary>
    public event EventHandler<string>? Pressed;

    /// <summary>ขาดการติดต่อกับ PLC — ส่ง null เมื่อกลับมาอ่านได้แล้ว</summary>
    public event EventHandler<string?>? Trouble;

    /// <summary>
    /// มีคนกดปุ่มหน้างานตอนที่หน้าจอไม่ว่าง — การกดนั้นถูกทิ้ง ไม่ได้ลงมือทำอะไร
    ///
    /// <para>
    /// ต้องแยกจาก <see cref="Pressed"/> เพื่อให้ผู้เรียกบอกคนหน้างานได้ว่าให้กดใหม่
    /// ไม่งั้นการกดจะหายเงียบ ๆ แล้วคนกดยืนรอโดยไม่รู้ว่าต้องทำอะไรต่อ
    /// </para>
    /// </summary>
    public event EventHandler<string>? BlockedPress;

    /// <summary>
    /// ตอนนี้ควรเฝ้าอยู่ไหม — คืน false แล้วจะข้ามรอบนั้นไปโดยไม่แตะ PLC
    ///
    /// <para>
    /// ผู้เรียกใช้ปิดไว้ตอนกำลังส่งงาน ตอนมีหน้าต่างเปิดค้าง หรือตอนไม่มีงานที่
    /// ต้องรอปุ่มกดอยู่เลย
    /// </para>
    /// </summary>
    public Func<bool>? ShouldWatch { get; set; }

    /// <summary>
    /// ตอนนี้ลงมือได้ไหม — false = ยังอ่านบิตต่อ แต่การกดที่เจอจะถูกทิ้งและแจ้งแทน
    ///
    /// <para>
    /// ต่างจาก <see cref="ShouldWatch"/> ตรงที่ตัวนั้นหยุดอ่านไปเลย ซึ่งทำให้การกด
    /// ที่เกิดในช่วงนั้นหายไปโดยไม่มีใครรู้ ตัวนี้ยังอ่านอยู่จึงรู้ว่ามีคนกด แล้วเลือก
    /// ที่จะไม่ลงมือ — ใช้ตอนมีกล่องเปิดค้างหรือกำลังส่งงานอยู่
    /// </para>
    /// </summary>
    public Func<string, bool>? CanAct { get; set; }

    public bool Running => _timer.Enabled;

    /// <summary>ที่อยู่ที่กำลังเฝ้าอยู่ เรียงตามสถานี — ว่างแปลว่ายังไม่ได้เริ่ม</summary>
    public string Watching =>
        Running ? string.Join(" · ", _buttons.Select(b => $"{b.Address}→{b.Machine}")) : "";

    /// <summary>
    /// เริ่มเฝ้า — อ่านค่าตั้งใหม่ทุกครั้ง เผื่อผู้ใช้เพิ่งไปแก้ที่หน้า Setting มา
    /// ปิดใช้งานอยู่ ไม่มีปุ่มให้เฝ้า หรือกรอกที่อยู่ผิด ก็ไม่เริ่ม
    /// </summary>
    public void Start()
    {
        if (_disposed) return;
        Stop();

        // ต้องฟังต่อแม้ปิดใช้งานอยู่ เพื่อเริ่มอ่านได้ทันทีเมื่อบันทึกเปิดใช้งาน
        PushButtonSettings.Saved += OnSettingsSaved;
        _settings = PushButtonSettings.Load();
        if (!_settings.IsReady || _settings.Validate() != null) return;

        _buttons = _settings.Watched().Select(w => (w.Address, w.Machine)).ToArray();
        if (_buttons.Length == 0) return;

        _lastOn = new bool[_buttons.Length];
        PlanBatchRead();

        _failures = 0;
        _resync = true;
        _timer.Interval = _settings.PollMs;
        _timer.Start();
    }

    /// <summary>
    /// ดูว่าอ่านทุกปุ่มรวดเดียวได้ไหม แล้วจำตำแหน่งของแต่ละปุ่มไว้
    ///
    /// <para>
    /// อ่านรวดเดียวดีกว่าสองทาง: เปิดปิด TCP รอบละครั้งแทนที่จะเท่าจำนวนปุ่ม (ตัวเฝ้า
    /// ยิงทุก 300 ms ตลอดกะ) และทุกปุ่มถูกอ่าน ณ จังหวะเดียวกัน จึงไม่มีกรณีที่ปุ่มหลัง
    /// ถูกอ่านช้ากว่าปุ่มแรกจนจับขอบขาขึ้นเพี้ยน
    /// </para>
    /// <para>
    /// ที่อยู่ปุ่มกดถูกบังคับให้เป็น M อยู่แล้วตอนตรวจค่า แต่ยังเช็คซ้ำตรงนี้
    /// เพราะถ้าวันหนึ่งกฎนั้นเปลี่ยน การอ่านรวมข้าม device จะได้ค่าผิดแบบเงียบ ๆ
    /// </para>
    /// </summary>
    private void PlanBatchRead()
    {
        _batchCount = 0;
        _offsets = new int[_buttons.Length];

        var numbers = new int[_buttons.Length];
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (!_buttons[i].Address.StartsWith("M", StringComparison.OrdinalIgnoreCase)) return;
            if (!McProtocolService.TryParseAddress(_buttons[i].Address, out _, out numbers[i], out _)) return;
        }

        int min = numbers.Min(), max = numbers.Max();
        if (max - min + 1 > MaxBatchPoints) return;

        _batchStart = min;
        _batchCount = max - min + 1;
        for (int i = 0; i < numbers.Length; i++) _offsets[i] = numbers[i] - min;
    }

    /// <summary>อ่านค่าของทุกปุ่มที่เฝ้าอยู่ เรียงตรงกับ <see cref="_buttons"/></summary>
    private async Task<(bool ok, bool[] on, string error)> ReadAllAsync()
    {
        if (_batchCount > 0)
        {
            var (ok, bits, error) = await McProtocolService.ReadBitsAsync(
                _settings.Ip, _settings.Port, $"M{_batchStart}", _batchCount);

            if (!ok) return (false, [], error);

            var picked = new bool[_buttons.Length];
            for (int i = 0; i < picked.Length; i++) picked[i] = bits[_offsets[i]];
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
        _generation++;
        PushButtonSettings.Saved -= OnSettingsSaved;
        _timer.Stop();
        _resync = true;
    }

    private void OnSettingsSaved(object? sender, EventArgs e) => Start();

    private async Task TickAsync()
    {
        // กันอ่านซ้อน — รอบก่อนอาจยังคุยกับ PLC ไม่เสร็จ หรือคนที่รับ Pressed
        // ไปกำลังเปิดหน้าต่างค้างอยู่
        if (_reading || !Running || _disposed) return;

        if (ShouldWatch?.Invoke() == false)
        {
            // ระหว่างพัก บิตอาจถูกกดและปล่อยไปแล้ว หรือยังค้างอยู่ ค่าที่จำไว้จึง
            // เชื่อไม่ได้ ต้องไปเริ่มจำใหม่ตอนกลับมา
            _resync = true;
            return;
        }

        _reading = true;
        int generation = _generation;
        try
        {
            var (ok, on, error) = await ReadAllAsync();

            // เปลี่ยน Address / หยุดเฝ้าระหว่างรอ ห้ามใช้คำตอบจากการอ่านรอบเก่า
            if (generation != _generation || _disposed || !Running) return;

            if (!ok)
            {
                OnFailure(error);
                return;
            }

            OnSuccess();

            if (_resync)
            {
                Array.Copy(on, _lastOn, on.Length);
                _resync = false;
                return;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                bool rising = on[i] && !_lastOn[i];

                // จำค่าไว้ก่อนแจ้ง — บิตค้างเป็น 1 อยู่ 2 วินาทีจะได้ไม่ถูกนับซ้ำ
                // ในรอบถัดไป ซึ่งที่ 300 ms ต่อรอบคือนับซ้ำอีกหกครั้ง
                _lastOn[i] = on[i];
                if (!rising) continue;

                var machine = _buttons[i].Machine;

                // จอไม่ว่าง — ไม่ลงมือ แต่ต้องบอกให้รู้ว่ามีคนกด
                //
                // ห้ามเก็บไว้ทำทีหลัง เพราะคนกดอาจเดินออกไปแล้ว พอมีคนมาปิดกล่องอีก
                // สิบวินาทีต่อมา เครื่องจะขยับเองตอนไม่มีใครยืนอยู่ตรงนั้น
                if (CanAct?.Invoke(machine) == false) BlockedPress?.Invoke(this, machine);
                else Pressed?.Invoke(this, machine);
            }
        }
        finally
        {
            _reading = false;
        }
    }

    private void OnFailure(string error)
    {
        _failures++;
        if (_failures != FailuresBeforeTrouble) return;

        // แจ้งครั้งเดียวตอนข้ามเส้น ไม่ใช่ทุกรอบ ไม่งั้นจอจะเต็มไปด้วยข้อความเดิม
        _timer.Interval = RetryMs;
        Trouble?.Invoke(this, error);
    }

    private void OnSuccess()
    {
        if (_failures == 0) return;

        bool wasTrouble = _failures >= FailuresBeforeTrouble;
        _failures = 0;
        _timer.Interval = _settings.PollMs;

        // กลับมาแล้วต้องเริ่มจำค่าใหม่ ช่วงที่อ่านไม่ได้อาจมีคนกดไปแล้วก็ได้
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
