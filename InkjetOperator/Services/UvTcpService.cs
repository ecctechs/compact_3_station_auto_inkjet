using System;
using System.Text;
using System.Threading.Tasks;
using InkjetOperator.Managers;

namespace InkjetOperator.Services
{
    /// <summary>
    /// M3: ส่ง KEY command ไปเครื่อง UV ผ่าน TCP (พอร์ต 10086)
    /// รูปแบบตรงตาม Node-RED flows.json:
    ///   KEY:85 + DATA=&lt;program&gt;.uvdx = โหลด/เปลี่ยนโปรแกรม
    ///   KEY:84 = start,  KEY:83 = stop
    /// </summary>
    public class UvTcpService
    {
        private const string StartCmd = "{\"KEY\":84}";
        private const string StopCmd = "{\"KEY\":83}";

        private static string LoadProgramCmd(string program)
        {
            string data = program.EndsWith(".uvdx", StringComparison.OrdinalIgnoreCase)
                ? program
                : program + ".uvdx";
            return "{\"KEY\":85,\"DATA\": \"" + data + "\"}";
        }

        private static string Resp(string r) => string.IsNullOrEmpty(r) ? "(no resp)" : r;

        /// <summary>โหลดโปรแกรม (.uvdx) แล้ว start — คืน (สำเร็จไหม, log ราย step)</summary>
        public async Task<(bool ok, string log)> LoadAndStartAsync(string ip, int port, string programName)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return (false, "ยังไม่ได้ตั้ง IP (UV Printer Setting)");

            var log = new StringBuilder();
            var tcp = new TcpManager();
            try
            {
                await tcp.ConnectAsync(ip, port);
                if (!tcp.IsConnected())
                    return (false, $"❌ เชื่อมต่อไม่ได้ {ip}:{port}");

                log.AppendLine($"เชื่อมต่อ {ip}:{port} ✓");

                // KEY:85 — โหลดโปรแกรม
                if (!string.IsNullOrWhiteSpace(programName))
                {
                    string r85 = await tcp.SendCommandAsync(LoadProgramCmd(programName));
                    log.AppendLine($"KEY:85 โหลด {programName}.uvdx → {Resp(r85)}");
                    await Task.Delay(500); // หน่วงให้เครื่องโหลดโปรแกรม
                }

                // KEY:84 — start
                string r84 = await tcp.SendCommandAsync(StartCmd);
                log.AppendLine($"KEY:84 start → {Resp(r84)}");

                return (true, log.ToString());
            }
            catch (Exception ex)
            {
                return (false, log + "\n❌ " + ex.Message);
            }
            finally
            {
                tcp.Disconnect();
            }
        }

        // ── ปุ่มแยก (สำหรับหน้าเทสหน้างาน) ──
        public Task<(bool ok, string log)> SendLoadAsync(string ip, int port, string program)
            => SendOneAsync(ip, port, LoadProgramCmd(program), $"KEY:85 โหลด {program}.uvdx");

        public Task<(bool ok, string log)> SendStartAsync(string ip, int port)
            => SendOneAsync(ip, port, StartCmd, "KEY:84 start");

        public Task<(bool ok, string log)> SendStopAsync(string ip, int port)
            => SendOneAsync(ip, port, StopCmd, "KEY:83 stop");

        /// <summary>ส่ง 1 command แล้วคืน response</summary>
        private async Task<(bool ok, string log)> SendOneAsync(string ip, int port, string command, string label)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return (false, "ยังไม่ได้ตั้ง IP");

            var tcp = new TcpManager();
            try
            {
                await tcp.ConnectAsync(ip, port);
                if (!tcp.IsConnected())
                    return (false, $"❌ เชื่อมต่อไม่ได้ {ip}:{port}");

                string r = await tcp.SendCommandAsync(command);
                return (true, $"{label} → {Resp(r)}");
            }
            catch (Exception ex)
            {
                return (false, $"{label} → ❌ {ex.Message}");
            }
            finally
            {
                tcp.Disconnect();
            }
        }

        /// <summary>ส่ง stop (KEY:83)</summary>
        public async Task<(bool ok, string log)> StopAsync(string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return (false, "ยังไม่ได้ตั้ง IP");

            var tcp = new TcpManager();
            try
            {
                await tcp.ConnectAsync(ip, port);
                if (!tcp.IsConnected())
                    return (false, $"❌ เชื่อมต่อไม่ได้ {ip}:{port}");

                string r = await tcp.SendCommandAsync(StopCmd);
                return (true, $"KEY:83 stop → {Resp(r)}");
            }
            catch (Exception ex)
            {
                return (false, "❌ " + ex.Message);
            }
            finally
            {
                tcp.Disconnect();
            }
        }
    }
}
