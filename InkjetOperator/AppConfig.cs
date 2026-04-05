using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace InkjetOperator
{
    public class AppConfig
    {
        public int MenuMode { get; set; } = 1;  // 1 = แสดงบางเมนู, 2 = แสดงทั้งหมด
        public string AppName { get; set; } = "Inkjet Operator";
        public string Company { get; set; } = "ECC Solutions";

        public static string PcIp =>
            CustomSettingsManager.GetValue("PC_IP") ?? "localhost";

        public static string ApiUrl
        {
            get
            {
                var ip = PcIp;
                Debug.WriteLine("PC IP: " + ip);    
                if (ip.StartsWith("http"))
                    return ip;

                return $"http://{ip}:3000";
            }
        }

        // โหลด config จาก App.config
        public static AppConfig Load()
        {
            var config = new AppConfig();
            try
            {
                var menuModeStr = ConfigurationManager.AppSettings["MenuMode"];
                if (int.TryParse(menuModeStr, out int menuMode))
                    config.MenuMode = menuMode;

                var appName = ConfigurationManager.AppSettings["AppName"];
                if (!string.IsNullOrEmpty(appName))
                    config.AppName = appName;

                var company = ConfigurationManager.AppSettings["Company"];
                if (!string.IsNullOrEmpty(company))
                    config.Company = company;
            }
            catch { /* ignore, use default */ }
            return config;
        }

        // ตรวจสอบว่าควรแสดงเมนูหรือไม่
        public bool ShouldShowMenu(string menuName)
        {
            string menu = menuName.ToLower();

            return MenuMode switch
            {
                // โหมด 0: หน้าหลักเบื้องต้น
                0 => menu switch { "input" or "setting" => true, _ => false },

                // โหมด 1: ทั้งหมด ยกเว้น ucBot และ ucST3 (โหมดหน้างานปกติ)
                1 => menu switch { "bot" or "st3" or "input" => false, _ => true },

                // โหมด 2: โหมด Bot (เน้นใช้ ucBot)
                2 => menu switch {  "bot" or "setting" => true, _ => false },

                // โหมด 3: โหมด Station 3 (เน้นใช้ ucST3)
                3 => menu switch { "st3" or "setting" => true, _ => false },

                // โหมด 4: โหมด Station 4 
                4 => menu switch { "bot" or "setting" => true, _ => false },

                // โหมด 5: Developer / Admin (เห็นทุกเมนู)
                5 => true,

                _ => false
            };
        }
    }
}