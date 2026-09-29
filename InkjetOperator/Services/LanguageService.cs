namespace InkjetOperator.Services;

public static class LanguageService
{
    private const string SettingKey = "UI_LANG";

    public static bool IsThai { get; private set; }

    public static void Init() =>
        IsThai = CustomSettingsManager.Read(SettingKey, "en")
            .Equals("th", StringComparison.OrdinalIgnoreCase);

    public static void Toggle()
    {
        IsThai = !IsThai;
        CustomSettingsManager.Write(SettingKey, IsThai ? "th" : "en");
    }

    public static string T(string english) =>
        IsThai && Words.TryGetValue(english, out var thai) ? thai : english;

    public static void Apply(Control? root)
    {
        if (root == null) return;

        var map = IsThai ? Words : Reverse;
        Walk(root, map);
    }

    private static void Walk(Control control, Dictionary<string, string> map)
    {
        bool isDataField = control is AntdUI.Input or TextBox or ComboBox or AntdUI.Select;

        if (!isDataField && !string.IsNullOrEmpty(control.Text)
            && map.TryGetValue(control.Text, out var translated))
        {
            control.Text = translated;
        }

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

    private static readonly Dictionary<string, string> Words = new(StringComparer.Ordinal)
    {
        ["Input Order"] = "รับออเดอร์",
        ["Order List"] = "รายการออเดอร์",
        ["Edit Pattern"] = "แก้ไขแพทเทิร์น",
        ["Setting"] = "ตั้งค่า",

        ["Save"] = "บันทึก",
        ["Cancel"] = "ยกเลิก",
        ["Clear"] = "ล้างข้อมูล",
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

        ["Scan Barcode"] = "สแกนบาร์โค้ด",
        ["Barcode / Lot No."] = "บาร์โค้ด / เลขล็อต",
        ["Barcode:"] = "บาร์โค้ด:",
        ["Marking Method:"] = "รูปแบบการมาร์ก:",
        ["Qty:"] = "จำนวน:",
        ["Lot:"] = "ล็อต:",

        ["List"] = "รายการ",
        ["History"] = "ประวัติ",
        ["Lot Number"] = "เลขล็อต",
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

        ["Job Information"] = "ข้อมูลงาน",
        ["Marking Method"] = "รูปแบบการมาร์ก",
        ["Plate"] = "เพลต",
        ["Shim"] = "ชิม",
        ["Station"] = "สถานี",

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
        ["Clamp (mm)"] = "แคลมป์ (มม.)",
        ["SWAP"] = "สลับ",
        ["Block"] = "บล็อก",
        ["Text"] = "ข้อความ",
        ["Field"] = "ฟิลด์",
        ["Value"] = "ค่า",

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
        ["Backend Folder"] = "โฟลเดอร์ Backend",
        ["Backend Folder:"] = "โฟลเดอร์ Backend:",
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

        ["Addr Start"] = "แอดเดรสเริ่ม",
        ["Addr Stop"] = "แอดเดรสจบ",
        ["PLC Start"] = "PLC เริ่ม",
        ["PLC Stop"] = "PLC จบ",
        ["List Name"] = "ชื่อรายการ",
        ["Data Type"] = "ชนิดข้อมูล",
        ["Write"] = "เขียน",

        ["Title"] = "หัวข้อ",
        ["Prompt"] = "คำถาม",
    };

    private static readonly Dictionary<string, string> Reverse = BuildReverse();

    private static Dictionary<string, string> BuildReverse()
    {
        var reverse = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Words)
        {
            if (!reverse.ContainsKey(pair.Value)) reverse[pair.Value] = pair.Key;
        }
        return reverse;
    }
}
