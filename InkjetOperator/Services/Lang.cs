namespace InkjetOperator.Services
{
    /// <summary>
    /// ระบบแปลภาษาแบบง่าย — toggle EN/TH
    /// ใช้ key-based dictionary, ไม่ใช้ .resx
    /// </summary>
    public static class Lang
    {
        private static string _current = "EN";
        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
        {
            // ══════════════════════════════════════════
            //  Form1 — Menu & Title
            // ══════════════════════════════════════════
            ["menu.input_order"]    = new() { ["EN"] = "Input Order",    ["TH"] = "สร้างงาน" },
            ["menu.order_list"]     = new() { ["EN"] = "Order List",     ["TH"] = "รายการงาน" },
            ["menu.edit_pattern"]   = new() { ["EN"] = "Edit Pattern",   ["TH"] = "แก้ไขแพทเทิร์น" },
            ["menu.setting"]        = new() { ["EN"] = "Setting",        ["TH"] = "ตั้งค่า" },
            ["menu.bot_uv"]         = new() { ["EN"] = "Bot UV",         ["TH"] = "บอท UV" },
            ["menu.job_st3"]        = new() { ["EN"] = "Job Station 3",  ["TH"] = "งานสถานี 3" },
            ["btn.refresh"]         = new() { ["EN"] = "Refresh",        ["TH"] = "รีเฟรช" },

            // ══════════════════════════════════════════
            //  ucInputOrder
            // ══════════════════════════════════════════
            ["input.barcode"]       = new() { ["EN"] = "Barcode:",       ["TH"] = "บาร์โค้ด:" },
            ["input.order_no"]      = new() { ["EN"] = "Order No:",      ["TH"] = "เลขที่ออเดอร์:" },
            ["input.customer"]      = new() { ["EN"] = "Customer:",      ["TH"] = "ลูกค้า:" },
            ["input.type"]          = new() { ["EN"] = "Type:",          ["TH"] = "ประเภท:" },
            ["input.qty"]           = new() { ["EN"] = "Qty:",           ["TH"] = "จำนวน:" },
            ["input.ok"]            = new() { ["EN"] = "OK",             ["TH"] = "ตกลง" },
            ["input.cancel"]        = new() { ["EN"] = "Cancel",         ["TH"] = "ยกเลิก" },
            ["input.scan_wait"]     = new() { ["EN"] = "📷 Waiting for barcode scan...", ["TH"] = "📷 รอสแกนบาร์โค้ด..." },
            ["input.title"]         = new() { ["EN"] = "Scan Barcode",   ["TH"] = "สแกนบาร์โค้ด" },

            // ══════════════════════════════════════════
            //  ucOrder — Job Detail
            // ══════════════════════════════════════════
            ["order.pending_jobs"]  = new() { ["EN"] = "Pending Jobs",   ["TH"] = "งานรอดำเนินการ" },
            ["order.job_detail"]    = new() { ["EN"] = "Job Detail",     ["TH"] = "รายละเอียดงาน" },
            ["order.barcode"]       = new() { ["EN"] = "Barcode:",       ["TH"] = "บาร์โค้ด:" },
            ["order.lot"]           = new() { ["EN"] = "Lot:",           ["TH"] = "ล็อต:" },
            ["order.status"]        = new() { ["EN"] = "Status:",        ["TH"] = "สถานะ:" },
            ["order.pattern"]       = new() { ["EN"] = "Pattern:",       ["TH"] = "แพทเทิร์น:" },
            ["order.inkjet_configs"]= new() { ["EN"] = "Inkjet Configs", ["TH"] = "ตั้งค่าหัวพิมพ์" },
            ["order.text_blocks"]   = new() { ["EN"] = "Text Blocks",    ["TH"] = "บล็อกข้อความ" },
            ["order.inkjet_uv"]     = new() { ["EN"] = "Inkjet UV",      ["TH"] = "อิงค์เจ็ท UV" },
            ["order.pending_label"] = new() { ["EN"] = "📋 Pending Jobs",    ["TH"] = "📋 งานรอดำเนินการ" },
            ["order.completed_label"]= new() { ["EN"] = "✅ Completed Jobs", ["TH"] = "✅ งานเสร็จแล้ว" },
            ["order.send_mk12"]     = new() { ["EN"] = "Send MK1,MK2",  ["TH"] = "ส่งหา MK1,MK2" },
            ["order.send_mk3"]      = new() { ["EN"] = "Send MK3",      ["TH"] = "ส่งหา MK3" },
            ["order.send_uv1"]      = new() { ["EN"] = "Send UV1",      ["TH"] = "ส่งหา UV1" },
            ["order.send_uv2"]      = new() { ["EN"] = "Send UV2",      ["TH"] = "ส่งหา UV2" },
            ["order.tab_list"]      = new() { ["EN"] = "List",           ["TH"] = "รายการ" },
            ["order.tab_history"]   = new() { ["EN"] = "History",        ["TH"] = "ประวัติ" },

            // ══════════════════════════════════════════
            //  Validation / MessageBox
            // ══════════════════════════════════════════
            ["msg.scan_barcode"]    = new() { ["EN"] = "Please scan barcode",           ["TH"] = "กรุณาสแกนบาร์โค้ด" },
            ["msg.enter_order_no"]  = new() { ["EN"] = "Please enter Order No",         ["TH"] = "กรุณาระบุเลขที่ออเดอร์" },
            ["msg.qty_number"]      = new() { ["EN"] = "Qty must be a number greater than 0", ["TH"] = "จำนวนต้องเป็นตัวเลขที่มากกว่า 0" },
            ["msg.barcode_format"]  = new() { ["EN"] = "Invalid barcode format!\n\nRequired format:\n1. [Pattern]-[Sub]-[Lot]\n2. [Pattern]-[Lot]\n\n* Last '-' separates Lot from Pattern",
                                              ["TH"] = "รูปแบบบาร์โค้ดไม่ถูกต้อง!\n\nต้องมีรูปแบบดังนี้:\n1. [Pattern]-[Sub]-[Lot] (เช่น xxxx-xxx-yyyyyy)\n2. [Pattern]-[Lot] (เช่น xxxxxx-yyyyy)\n\n* เครื่องหมาย '-' ตัวสุดท้ายจะถูกใช้เพื่อแยก Lot ออกจาก Pattern" },
            ["msg.pattern_not_found"]= new() { ["EN"] = "Pattern '{0}' not registered",  ["TH"] = "ไม่พบ Pattern '{0}' ในระบบ" },
            ["msg.sync_failed"]     = new() { ["EN"] = "Cannot prepare Pattern data. Please check connection.", ["TH"] = "ไม่สามารถจัดเตรียมข้อมูล Pattern ได้ กรุณาตรวจสอบการเชื่อมต่อ" },
            ["msg.job_success"]     = new() { ["EN"] = "Create job success",             ["TH"] = "สร้างงานสำเร็จ" },
            ["msg.job_failed"]      = new() { ["EN"] = "Create job failed",              ["TH"] = "สร้างงานล้มเหลว" },
            ["msg.select_job"]      = new() { ["EN"] = "Please select a Job first",      ["TH"] = "กรุณาเลือกรายการ Job ก่อน" },
            ["msg.no_config"]       = new() { ["EN"] = "No printer config found",        ["TH"] = "ไม่พบข้อมูล Config ของเครื่องพิมพ์" },
            ["msg.send_success"]    = new() { ["EN"] = "Data sent successfully",         ["TH"] = "ส่งข้อมูลเรียบร้อยแล้ว" },
            ["msg.send_error"]      = new() { ["EN"] = "Error: {0}",                     ["TH"] = "เกิดข้อผิดพลาด: {0}" },
            ["msg.connect_error"]   = new() { ["EN"] = "Cannot connect to printer ({0}:{1})", ["TH"] = "ไม่สามารถเชื่อมต่อเครื่องพิมพ์ ({0}:{1})" },
            ["msg.db_error"]        = new() { ["EN"] = "❌ Cannot connect to SQLite database\n\nDB Path: {0}\n\nPlease check:\n• File exists\n• DB_PATH in Setting is correct",
                                              ["TH"] = "❌ ไม่สามารถเชื่อมต่อฐานข้อมูล SQLite ได้\n\nDB Path: {0}\n\nกรุณาตรวจสอบ:\n• ไฟล์ .db3 มีอยู่จริงหรือไม่\n• ตั้งค่า DB_PATH ในหน้า Setting ถูกต้องหรือไม่" },
            ["msg.backend_error"]   = new() { ["EN"] = "❌ Cannot connect to Backend Server\n\nPC IP: {0}\nURL: {1}\n\nPlease check:\n• Backend Server is running\n• PC_IP in Setting is correct\n• Network is connected",
                                              ["TH"] = "❌ ไม่สามารถเชื่อมต่อ Backend Server ได้\n\nPC IP: {0}\nURL: {1}\n\nกรุณาตรวจสอบ:\n• Backend Server เปิดอยู่หรือไม่\n• ตั้งค่า PC_IP ในหน้า Setting ถูกต้องหรือไม่\n• เครือข่ายเชื่อมต่อได้หรือไม่" },
            ["msg.validation_error"]= new() { ["EN"] = "Validation Error",               ["TH"] = "ข้อมูลไม่ถูกต้อง" },
            ["msg.warning"]         = new() { ["EN"] = "Warning",                        ["TH"] = "คำเตือน" },
            ["msg.error"]           = new() { ["EN"] = "Error",                          ["TH"] = "ข้อผิดพลาด" },
            ["msg.success"]         = new() { ["EN"] = "Success",                        ["TH"] = "สำเร็จ" },
            ["msg.not_found"]       = new() { ["EN"] = "Not Found",                      ["TH"] = "ไม่พบข้อมูล" },
            ["msg.connection_error"]= new() { ["EN"] = "Connection Error",               ["TH"] = "ข้อผิดพลาดการเชื่อมต่อ" },
            ["msg.database_error"]  = new() { ["EN"] = "Database Error",                 ["TH"] = "ข้อผิดพลาดฐานข้อมูล" },
            ["msg.barcode_format_title"] = new() { ["EN"] = "Barcode Format Error",      ["TH"] = "รูปแบบบาร์โค้ดไม่ถูกต้อง" },
            ["msg.no_barcode"]      = new() { ["EN"] = "No barcode data",                ["TH"] = "ไม่พบข้อมูล Barcode" },
            ["msg.no_config_for"]   = new() { ["EN"] = "No config found for: {0}",       ["TH"] = "ไม่พบ Config สำหรับ: {0}" },
            ["msg.save_error"]      = new() { ["EN"] = "Cannot save print status (Server Error)", ["TH"] = "ไม่สามารถบันทึกสถานะการพิมพ์ได้ (Server Error)" },
            ["msg.uv_sending"]      = new() { ["EN"] = "Sending UV print for Job ID: {0}", ["TH"] = "กำลังส่งข้อมูลการพิมพ์ UV สำหรับ Job ID: {0}" },
            ["msg.backend_not_found"]= new() { ["EN"] = "Job/Pattern not found in Backend", ["TH"] = "ไม่พบข้อมูล Job หรือ Pattern นี้ในระบบ Backend" },
            ["msg.backend_connect_fail"]= new() { ["EN"] = "Failed to connect to Backend", ["TH"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับ Backend" },
        };

        /// <summary>ภาษาปัจจุบัน ("EN" หรือ "TH")</summary>
        public static string Current => _current;

        /// <summary>Event สำหรับแจ้ง Form/UserControl ให้อัปเดตภาษา</summary>
        public static event Action? LanguageChanged;

        /// <summary>Toggle ระหว่าง EN ↔ TH</summary>
        public static void Toggle()
        {
            _current = _current == "EN" ? "TH" : "EN";
            LanguageChanged?.Invoke();
        }

        /// <summary>ดึงข้อความตาม key + ภาษาปัจจุบัน</summary>
        public static string Get(string key)
        {
            if (_dict.TryGetValue(key, out var translations) &&
                translations.TryGetValue(_current, out var text))
                return text;

            return key; // fallback: แสดง key เดิม (ช่วย debug)
        }

        /// <summary>ดึงข้อความแบบมี format parameter — เช่น Lang.Format("msg.error", ex.Message)</summary>
        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
