namespace InkjetOperator.Services;

/// <summary>
/// สลับภาษาหน้าจอทั้งระบบระหว่างอังกฤษกับไทย
///
/// วิธีทำงาน: เดินไล่ control ทั้งต้นไม้แล้วสลับ <c>Text</c> ตามพจนานุกรมด้านล่าง
/// คำที่ไม่มีในพจนานุกรมจะถูกปล่อยไว้เฉย ๆ จึงปลอดภัยกับข้อความที่เป็นข้อมูล
///
/// ไม่ได้ใช้ resx ของ .NET เพราะต้องเปิด Localizable ทุกหน้าและจะทำให้
/// .Designer.cs บวมจนแก้ในตัว Designer ลำบาก ซึ่งผิดข้อกำหนดของโปรเจกต์
///
/// สิ่งที่ไม่แปลโดยตั้งใจ:
///   · ชื่อเครื่องและรหัส (MK-058, UV-001, PLC-001, IAI, Z1)
///   · ชื่อผลิตภัณฑ์ (Compact Inkjet)
///   · ข้อความที่โค้ดเซ็ตตอนรันซึ่งเป็นภาษาไทยอยู่แล้ว (แจ้งเตือน, ผลการส่งงาน)
/// </summary>
public static class LanguageService
{
    private const string SettingKey = "UI_LANG";

    /// <summary>ภาษาปัจจุบันเป็นไทยหรือไม่</summary>
    public static bool IsThai { get; private set; }

    /// <summary>อ่านภาษาที่เคยเลือกไว้ เรียกครั้งเดียวตอนเปิดโปรแกรม</summary>
    public static void Init() =>
        IsThai = CustomSettingsManager.Read(SettingKey, "en")
            .Equals("th", StringComparison.OrdinalIgnoreCase);

    /// <summary>สลับภาษาแล้วจำไว้ให้รอบหน้า</summary>
    public static void Toggle()
    {
        IsThai = !IsThai;
        CustomSettingsManager.Write(SettingKey, IsThai ? "th" : "en");
    }

    /// <summary>ข้อความเดี่ยว ๆ ที่โค้ดสร้างเอง เช่นหัวคอลัมน์ที่ตั้งตอนรัน</summary>
    public static string T(string english) =>
        IsThai && Words.TryGetValue(english, out var thai) ? thai : english;

    /// <summary>
    /// แปลทั้ง subtree ตามภาษาปัจจุบัน เรียกซ้ำได้ไม่มีผลข้างเคียง
    /// เรียกหลังเปิดหน้าใหม่หรือ dialog ใหม่ทุกครั้ง เพราะหน้าพวกนั้นสร้างทีหลัง
    /// </summary>
    public static void Apply(Control? root)
    {
        if (root == null) return;

        var map = IsThai ? Words : Reverse;
        Walk(root, map);
    }

    private static void Walk(Control control, Dictionary<string, string> map)
    {
        // ช่องกรอกเก็บ "ข้อมูล" ไม่ใช่ "ป้าย" — แปลแล้วข้อมูลจะเพี้ยน
        bool isDataField = control is AntdUI.Input or TextBox or ComboBox or AntdUI.Select;

        if (!isDataField && !string.IsNullOrEmpty(control.Text)
            && map.TryGetValue(control.Text, out var translated))
        {
            control.Text = translated;
        }

        // หัวคอลัมน์ของตารางถูกตั้งในโค้ด ไม่ได้อยู่ใน .Designer.cs
        if (control is AntdUI.Table { Columns: not null } table)
        {
            foreach (var column in table.Columns)
            {
                if (!string.IsNullOrEmpty(column.Title)
                    && map.TryGetValue(column.Title, out var head))
                {
                    column.Title = head;
                }
            }
        }

        foreach (Control child in control.Controls) Walk(child, map);
    }

    /// <summary>อังกฤษ → ไทย · คำที่ไม่มีในนี้จะไม่ถูกแตะ</summary>
    private static readonly Dictionary<string, string> Words = new(StringComparer.Ordinal)
    {
        // ── เมนูหลัก ──
        ["Input Order"] = "รับออเดอร์",
        ["Order List"] = "รายการออเดอร์",
        ["Edit Pattern"] = "แก้ไขแพทเทิร์น",
        ["Transfer ST1"] = "ส่งต่อ ST1",
        ["Setting"] = "ตั้งค่า",

        // ── ปุ่มที่ใช้ร่วมกันทั้งระบบ ──
        ["Save"] = "บันทึก",
        ["Cancel"] = "ยกเลิก",
        ["OK"] = "ตกลง",
        ["Delete"] = "ลบ",
        ["Reset"] = "รีเซ็ต",
        ["Send"] = "ส่ง",
        ["Upload"] = "อัปโหลด",
        ["Rename"] = "เปลี่ยนชื่อ",
        ["Browse"] = "เลือกไฟล์",
        ["Browse..."] = "เลือกไฟล์...",
        ["Check Status"] = "ตรวจสถานะ",
        ["Read All"] = "อ่านทั้งหมด",

        // ── หน้าสแกนบาร์โค้ด ──
        ["Scan Barcode"] = "สแกนบาร์โค้ด",
        ["Barcode / Lot No."] = "บาร์โค้ด / เลขล็อต",
        ["Barcode:"] = "บาร์โค้ด:",
        ["Order No:"] = "เลขออเดอร์:",
        ["Marking Method:"] = "รูปแบบการมาร์ก:",
        ["Qty:"] = "จำนวน:",
        ["Lot:"] = "ล็อต:",

        // ── ตารางออเดอร์ ──
        ["List"] = "รายการ",
        ["History"] = "ประวัติ",
        ["Order No."] = "เลขออเดอร์",
        ["Customer"] = "ลูกค้า",
        ["Type"] = "ประเภท",
        ["Qty"] = "จำนวน",
        ["Process Sequence"] = "ลำดับกระบวนการ",
        ["Status"] = "สถานะ",
        ["Start"] = "เริ่ม",
        ["End"] = "จบ",
        ["Preview"] = "ตัวอย่าง",
        ["Processing"] = "กำลังผลิต",

        // ── หน้ารายละเอียดงาน ──
        ["Job Information"] = "ข้อมูลงาน",
        ["Marking Method"] = "รูปแบบการมาร์ก",
        ["Plate"] = "เพลต",
        ["Shim"] = "ชิม",
        ["Station"] = "สถานี",

        // ตาราง register map ของหน้า PLC ทั้งสองหน้า
        ["Action"] = "คำสั่ง",
        ["Axis"] = "แกน",
        ["DB Column"] = "คอลัมน์ DB",
        ["Value (mm)"] = "ค่า (mm)",
        ["Target (D)"] = "ปลายทาง (D)",
        ["Run (M)"] = "สั่งวิ่ง (M)",
        ["Reset (M)"] = "รีเซ็ต (M)",
        ["Raw"] = "ค่าที่เขียน",
        ["Addr Start"] = "แอดเดรสเริ่ม",
        ["Addr Stop"] = "แอดเดรสจบ",
        ["PLC Start"] = "PLC เริ่ม",
        ["PLC Stop"] = "PLC จบ",
        ["List Name"] = "ชื่อรายการ",
        ["Data Type"] = "ชนิดข้อมูล",
        ["Write"] = "เขียน",
        ["MK Section (MK Inkjet)"] = "โซน MK (MK Inkjet)",
        ["UV Section"] = "โซน UV",
        ["Program Name"] = "ชื่อโปรแกรม",
        ["Program No."] = "เลขโปรแกรม",
        ["Width"] = "กว้าง",
        ["Height"] = "สูง",
        ["Position"] = "ตำแหน่ง",
        ["Trigger Delay"] = "ดีเลย์ทริกเกอร์",
        ["Servo Post Act."] = "เซอร์โว Post Act.",
        ["Delay (mm.)"] = "ดีเลย์ (มม.)",
        ["Conveyor Speed"] = "ความเร็วสายพาน",
        ["Conveyor 1 (Hz)"] = "สายพาน 1 (Hz)",
        ["Conveyor 2 (Hz)"] = "สายพาน 2 (Hz)",
        ["Conveyor 3 (Hz)"] = "สายพาน 3 (Hz)",
        ["Qty (Shared)"] = "จำนวน (ใช้ร่วม)",
        ["Clamp (mm)"] = "แคลมป์ (มม.)",
        ["SWAP"] = "สลับ",
        ["Block"] = "บล็อก",
        ["Text"] = "ข้อความ",
        ["Field"] = "ฟิลด์",
        ["Value"] = "ค่า",

        // ── หน้าแก้ไขแพทเทิร์น ──
        ["Transform Rules"] = "กฎแปลงข้อความ",
        ["RULES"] = "รายการกฎ",
        ["+ Add Rule"] = "+ เพิ่มกฎ",
        ["+ New Rule"] = "+ กฎใหม่",
        ["Save Rule"] = "บันทึกกฎ",
        ["Rule Name:"] = "ชื่อกฎ:",
        ["Description:"] = "คำอธิบาย:",
        ["Preview:"] = "ตัวอย่าง:",
        ["Lot Test:"] = "ล็อตทดสอบ:",
        ["Block Text:"] = "ข้อความบล็อก:",
        ["From"] = "จาก",
        ["To"] = "เป็น",
        ["Rule"] = "กฎ",
        ["Action"] = "จัดการ",

        // ── หน้าตั้งค่า ──
        ["Database Setting"] = "ตั้งค่าฐานข้อมูล",
        ["Backend DB Setting"] = "ตั้งค่า Backend DB",
        ["PLC MK Setting"] = "ตั้งค่า PLC MK",
        ["PLC UV Setting"] = "ตั้งค่า PLC UV",
        ["Printer Setting"] = "ตั้งค่าเครื่องพิมพ์",
        ["UV Test"] = "ทดสอบ UV",
        ["UV2 Folder"] = "โฟลเดอร์ UV2",
        ["UV2 Folder:"] = "โฟลเดอร์ UV2:",
        ["UV2 Program Folder"] = "โฟลเดอร์โปรแกรม UV2",
        ["Database"] = "ฐานข้อมูล",
        ["Printing Database Path:"] = "พาธฐานข้อมูลงานพิมพ์:",
        ["Database Path:"] = "พาธฐานข้อมูล:",
        ["Clamp Database Path:"] = "พาธฐานข้อมูลแคลมป์:",
        ["Clamp Database:"] = "ฐานข้อมูลแคลมป์:",
        ["IP Address:"] = "หมายเลข IP:",
        ["IP:"] = "IP:",
        ["Port:"] = "พอร์ต:",
        ["Name:"] = "ชื่อ:",
        ["Folder:"] = "โฟลเดอร์:",
        ["Program:"] = "โปรแกรม:",
        ["UV Software Folder:"] = "โฟลเดอร์ซอฟต์แวร์ UV:",
        ["Marking Reference Image"] = "รูปอ้างอิงการมาร์ก",
        ["MK Inkjet Printer"] = "เครื่องพิมพ์ MK Inkjet",
        ["UV Printer"] = "เครื่องพิมพ์ UV",
        ["PLC Connection"] = "การเชื่อมต่อ PLC",
        ["PLC IP:"] = "IP ของ PLC:",
        ["Register Map"] = "ตารางรีจิสเตอร์",
        ["Start  (83)"] = "เริ่ม  (83)",
        ["Stop  (84)"] = "หยุด  (84)",
        ["+ Add Row"] = "+ เพิ่มแถว",

        // ── ตารางหน้า PLC ──
        ["Addr Start"] = "แอดเดรสเริ่ม",
        ["Addr Stop"] = "แอดเดรสจบ",
        ["PLC Start"] = "PLC เริ่ม",
        ["PLC Stop"] = "PLC จบ",
        ["List Name"] = "ชื่อรายการ",
        ["Data Type"] = "ชนิดข้อมูล",
        ["Write"] = "เขียน",

        // ── กล่องข้อความ ──
        ["Title"] = "หัวข้อ",
        ["Prompt"] = "คำถาม",
    };

    /// <summary>ไทย → อังกฤษ สร้างจากตารางบน เพื่อสลับกลับได้</summary>
    private static readonly Dictionary<string, string> Reverse = BuildReverse();

    private static Dictionary<string, string> BuildReverse()
    {
        var reverse = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Words)
        {
            // คำไทยซ้ำกันแปลว่าย้อนกลับไม่ได้แน่ ๆ ข้ามไปดีกว่าแปลผิดฝั่ง
            if (!reverse.ContainsKey(pair.Value)) reverse[pair.Value] = pair.Key;
        }
        return reverse;
    }
}
