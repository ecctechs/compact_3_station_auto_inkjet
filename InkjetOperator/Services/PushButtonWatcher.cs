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
    /// ตอนนี้ควรเฝ้าอยู่ไหม — คืน false แล้วจะข้ามรอบนั้นไปโดยไม่แตะ PLC
    ///
    /// <para>
    /// ผู้เรียกใช้ปิดไว้ตอนกำลังส่งงาน ตอนมีหน้าต่างเปิดค้าง หรือตอนไม่มีงานที่
    /// ต้องรอปุ่มกดอยู่เลย
    /// </para>
    /// </summary>
    public Func<bool>? ShouldWatch { get; set; }

    public bool Running => _timer.Enabled;

    /// <summary>ที่อยู่ที่กำลังเฝ้าอยู่ — ว่างแปลว่ายังไม่ได้เริ่ม</summary>
    public string Address => Running ? _settings.Address : "";

    /// <summary>
    /// เริ่มเฝ้า — อ่านค่าตั้งใหม่ทุกครั้ง เผื่อผู้ใช้เพิ่งไปแก้ที่หน้า Setting มา
    /// ปิดใช้งานอยู่หรือยังไม่ได้กรอกที่อยู่ ก็ไม่เริ่ม
    /// </summary>
    public void Start()
    {
        Stop();

        _settings = PushButtonSettings.Load(); // อ่าน IP, พอร์ต และบิตปุ่มหน้างานที่ตั้งไว้
        if (!_settings.IsReady || _settings.Validate() != null) return; // ค่าปุ่มยังไม่พร้อมหรือไม่ถูกต้อง จึงยังไม่เริ่มอ่าน PLC

        _failures = 0; // เริ่มนับปัญหาการอ่าน PLC ใหม่
        _resync = true; // รอบแรกต้องจำค่าปุ่มก่อน ไม่ถือว่าปุ่มค้างคือการกดใหม่
        _timer.Interval = _settings.PollMs; // ใช้ความถี่อ่านปุ่มตามค่าตั้ง
        _timer.Start(); // เริ่มจับเวลาสำหรับอ่านบิตปุ่มซ้ำ
    }

    public void Stop()
    {
        _timer.Stop(); // หยุดอ่านปุ่มเมื่อหน้าไม่ต้องใช้งานแล้ว
        _resync = true; // เมื่อกลับมาอ่าน ให้จำสถานะเริ่มต้นใหม่
    }

    private async Task TickAsync()
    {
        // กันอ่านซ้อน — รอบก่อนอาจยังคุยกับ PLC ไม่เสร็จ หรือคนที่รับ Pressed
        // ไปกำลังเปิดหน้าต่างค้างอยู่
        if (_reading) return; // รอบก่อนยังอ่านไม่เสร็จ จึงไม่เปิดคำขอ PLC ซ้อน

        if (ShouldWatch?.Invoke() == false) // หน้า Order List ยังไม่พร้อมรับปุ่ม เช่น กำลังส่งเครื่อง
        {
            // ระหว่างพัก บิตอาจถูกกดและปล่อยไปแล้ว หรือยังค้างอยู่ ค่าที่จำไว้จึง
            // เชื่อไม่ได้ ต้องไปเริ่มจำใหม่ตอนกลับมา
            _resync = true; // กลับมาเมื่อไรต้องอ่านค่าตั้งต้นใหม่
            return; // พักอ่านปุ่มจนกว่าหน้ารายการพร้อม
        }

        _reading = true; // กันรอบเวลาถัดไปเข้ามาอ่านซ้อน
        try
        {
            var (ok, on, error) = await McProtocolService.ReadBitAsync( // อ่านบิตปุ่มจาก PLC ผ่าน MC Protocol
                _settings.Ip, _settings.Port, _settings.Address); // ใช้ปลายทางและตำแหน่งบิตจากค่าตั้งหน้างาน

            if (!ok) // PLC ไม่คืนค่าบิตที่อ่านได้
            {
                OnFailure(error); // นับการอ่านล้มเหลวและแจ้งเมื่อครบเกณฑ์
                return; // ยังไม่มีค่าปุ่มใหม่ จึงไม่สร้างเหตุการณ์กด
            }

            OnSuccess(); // คืนความถี่อ่านตามปกติถ้าเพิ่งเชื่อมต่อกลับมาได้

            if (_resync) // เป็นรอบเริ่มต้นหรือเพิ่งกลับจากพักอ่าน
            {
                _lastOn = on; // จำค่าปุ่มตอนนี้ไว้เป็นฐานเทียบ
                _resync = false; // รอบถัดไปเริ่มตรวจการเปลี่ยนบิตได้
                return; // จำค่าอย่างเดียวในรอบแรก ไม่ปล่อยเครื่องทันที
            }

            bool rising = on && !_lastOn; // นับเป็นการกดเฉพาะตอนบิตเปลี่ยนจาก 0 เป็น 1
            _lastOn = on; // จำค่ารอบนี้สำหรับเทียบกับรอบหน้า

            if (rising) Pressed?.Invoke(this, EventArgs.Empty); // ส่งเหตุการณ์ให้ Order List ไปปล่อยคิวเครื่อง
        }
        finally
        {
            _reading = false; // เปิดให้รอบเวลาถัดไปอ่าน PLC ได้
        }
    }

    private void OnFailure(string error)
    {
        _failures++; // นับจำนวนครั้งที่อ่าน PLC ไม่สำเร็จติดกัน
        if (_failures != FailuresBeforeTrouble) return; // ยังไม่ถึงจังหวะแจ้งปัญหา จึงไม่แจ้งซ้ำทุกรอบ

        // แจ้งครั้งเดียวตอนข้ามเส้น ไม่ใช่ทุกรอบ ไม่งั้นจอจะเต็มไปด้วยข้อความเดิม
        _timer.Interval = RetryMs; // ลดความถี่เป็นช่วงลองเชื่อมต่อใหม่
        Trouble?.Invoke(this, error); // แจ้งปัญหาปุ่มให้หน้ารายการแสดง
    }

    private void OnSuccess()
    {
        if (_failures == 0) return; // อ่านได้ต่อเนื่องอยู่แล้ว ไม่ต้องรีเซ็ตสถานะเชื่อมต่อ

        bool wasTrouble = _failures >= FailuresBeforeTrouble; // จำว่าเคยแจ้งปัญหาการเชื่อมต่อไว้หรือไม่
        _failures = 0; // อ่านได้แล้ว เริ่มนับความผิดพลาดใหม่
        _timer.Interval = _settings.PollMs; // กลับไปใช้ความถี่อ่านตามค่าตั้ง

        // กลับมาแล้วต้องเริ่มจำค่าใหม่ ช่วงที่อ่านไม่ได้อาจมีคนกดไปแล้วก็ได้
        _resync = true; // จำค่าปุ่มใหม่ เพื่อไม่ตีความบิตช่วงสายหลุดเป็นการกด

        if (wasTrouble) Trouble?.Invoke(this, null); // ล้างข้อความปัญหาเมื่อกลับมาอ่าน PLC ได้แล้ว
    }

    public void Dispose()
    {
        _timer.Stop(); // หยุดรอบอ่าน PLC ก่อนปิดตัวฟังปุ่ม
        _timer.Dispose(); // คืนทรัพยากรตัวจับเวลาของปุ่ม
    }
}
