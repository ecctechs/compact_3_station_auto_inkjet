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
            return MenuMode switch
            {
                1 => menuName.ToLower() switch { "input" or "setting" => true, _ => false },
                2 => true,  // ทั้งหมด
                3 => menuName.ToLower() switch { "input" or "order" => true, _ => false }, // โหมดใหม่
                4 => true,  // ทั้งหมด
            };
        }
    }
}