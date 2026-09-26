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

    private readonly System.Windows.Forms.Timer _timer = new();

    private PushButtonSettings _settings = new();
    private bool _reading;
    private bool _lastOn;
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

    /// <summary>มีคนกดปุ่มหน้างาน</summary>
    public event EventHandler? Pressed;

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
    public event EventHandler? BlockedPress;

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
    public Func<bool>? CanAct { get; set; }

    public bool Running => _timer.Enabled;

    /// <summary>ที่อยู่ที่กำลังเฝ้าอยู่ — ว่างแปลว่ายังไม่ได้เริ่ม</summary>
    public string Address => Running ? _settings.Address : "";

    /// <summary>
    /// เริ่มเฝ้า — อ่านค่าตั้งใหม่ทุกครั้ง เผื่อผู้ใช้เพิ่งไปแก้ที่หน้า Setting มา
    /// ปิดใช้งานอยู่หรือยังไม่ได้กรอกที่อยู่ ก็ไม่เริ่ม
    /// </summary>
    public void Start()
    {
        if (_disposed) return;
        Stop();

        // ต้องฟังต่อแม้ปิดใช้งานอยู่ เพื่อเริ่มอ่านได้ทันทีเมื่อบันทึกเปิดใช้งาน
        PushButtonSettings.Saved += OnSettingsSaved;
        _settings = PushButtonSettings.Load();
        if (!_settings.IsReady || _settings.Validate() != null) return;

        _failures = 0;
        _resync = true;
        _timer.Interval = _settings.PollMs;
        _timer.Start();
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
            var (ok, on, error) = await McProtocolService.ReadBitAsync(
                _settings.Ip, _settings.Port, _settings.Address);

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
                _lastOn = on;
                _resync = false;
                return;
            }

            bool rising = on && !_lastOn;
            _lastOn = on;

            if (!rising) return;

            // จอไม่ว่าง — ไม่ลงมือ แต่ต้องบอกให้รู้ว่ามีคนกด
            //
            // ห้ามเก็บไว้ทำทีหลัง เพราะคนกดอาจเดินออกไปแล้ว พอมีคนมาปิดกล่องอีก
            // สิบวินาทีต่อมา เครื่องจะขยับเองตอนไม่มีใครยืนอยู่ตรงนั้น
            if (CanAct?.Invoke() == false) BlockedPress?.Invoke(this, EventArgs.Empty);
            else Pressed?.Invoke(this, EventArgs.Empty);
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
