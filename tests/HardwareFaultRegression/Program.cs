using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using InkjetOperator.Managers;
using InkjetOperator.Services;

internal static class Program
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }

    [STAThread]
    private static int Main()
    {
        int exit = 0;
        using var context = new ApplicationContext();
        using var control = new Control();
        _ = control.Handle;
        control.BeginInvoke(new Action(async () =>
        {
            try
            {
                await CheckMkAsync();
                await CheckUvAsync();
                await CheckPushButtonAsync();
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); exit = 1; }
            finally { context.ExitThread(); }
        }));
        Application.Run(context);
        return exit;
    }

    private static async Task CheckMkAsync()
    {
        using var server = new TcpListener(IPAddress.Loopback, 0);
        server.Start();
        int port = ((IPEndPoint)server.LocalEndpoint).Port;
        var mk = new TcpManager();
        try
        {
            await mk.ConnectAsync("127.0.0.1", port);
            using (var peer = await server.AcceptTcpClientAsync())
            {
                var first = mk.SendCommandAsync("SQ\r");
                var queued = mk.SendCommandAsync("FW,1\r");
                var bytes = new byte[3];
                await peer.GetStream().ReadExactlyAsync(bytes);
                Check(Encoding.ASCII.GetString(bytes) == "SQ\r", "MK command changed");
                var watch = Stopwatch.StartNew();
                try { await first.WaitAsync(TimeSpan.FromSeconds(6)); throw new Exception("silent MK returned success"); }
                catch (TimeoutException ex) { Check(ex.Message.Contains("MK"), "MK did not enforce its own timeout"); }
                Check(watch.Elapsed < TimeSpan.FromSeconds(5), "MK timeout exceeded bound");
                Check(await queued == "" && !mk.IsConnected(), "queued command reused timed-out socket");
                Check(await peer.GetStream().ReadAsync(new byte[32]) == 0, "queued command reached hardware after timeout");
            }

            // A fresh connection works; concurrent calls receive their own response.
            await mk.ConnectAsync("127.0.0.1", port);
            using (var peer = await server.AcceptTcpClientAsync())
            {
                var first = mk.SendCommandAsync("SR\r");
                var second = mk.SendCommandAsync("SQ\r");
                foreach (var command in new[] { "SR\r", "SQ\r" })
                {
                    var bytes = new byte[3];
                    await peer.GetStream().ReadExactlyAsync(bytes);
                    Check(Encoding.ASCII.GetString(bytes) == command, "MK command order changed");
                    await peer.GetStream().WriteAsync(Encoding.ASCII.GetBytes(command));
                }
                Check(await first == "SR" && await second == "SQ", "MK responses crossed callers");
            }
        }
        finally { mk.Disconnect(); }
        Console.WriteLine("PASS: MK silence times out, closes socket, drops queued IO, reconnects and keeps responses separate");
    }

    private static async Task CheckUvAsync()
    {
        foreach (var scenario in new[] { "ok", "reject", "silent", "disconnect", "malformed", "load-reject", "load-disconnect", "load-malformed" })
        {
            using var server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            int port = ((IPEndPoint)server.LocalEndpoint).Port;
            var operation = new UvTcpService().LoadAndStartAsync("127.0.0.1", port, "TEST");
            using (var peer = await server.AcceptTcpClientAsync())
            {
                var bytes = new byte[1024];
                int read = await peer.GetStream().ReadAsync(bytes);
                Check(Encoding.UTF8.GetString(bytes, 0, read).Contains("\"KEY\":85"), "UV load protocol changed");
                if (scenario == "load-disconnect") peer.Close();
                else await peer.GetStream().WriteAsync(Encoding.UTF8.GetBytes(scenario == "load-reject" ? "{\"RS\":1}" :
                    scenario == "load-malformed" ? "invalid" : "{\"RS\":0}"));
            }
            if (!scenario.StartsWith("load-"))
            {
                using var peer = await server.AcceptTcpClientAsync();
                var bytes = new byte[1024];
                int read = await peer.GetStream().ReadAsync(bytes);
                Check(Encoding.UTF8.GetString(bytes, 0, read).Contains("\"KEY\":83"), "UV start protocol changed");
                if (scenario == "disconnect") peer.Close();
                else if (scenario != "silent")
                    await peer.GetStream().WriteAsync(Encoding.UTF8.GetBytes(scenario == "reject" ? "{\"RS\":1}" :
                        scenario == "malformed" ? "invalid" : "{\"RS\":0}"));
                var result = await operation.WaitAsync(TimeSpan.FromSeconds(6));
                Check(result.ok, $"confirmed Load must count as delivered for UV {scenario}");
                Check((result.startWarning == null) == (scenario == "ok"), "Start result was hidden or marked confirmed incorrectly");
                if (scenario == "reject") Check(result.startWarning!.Contains("RS=1"), "UV rejection detail lost");
            }
            else Check(!(await operation).ok, "UV failed load reported success");
            Check(!server.Pending(), "UV retried hardware command automatically");
        }
        Console.WriteLine("PASS: UV confirmed Load counts as delivered; Start rejection/silence/disconnect/malformed stay warnings; failed Load blocks; no retry");
    }

    private static async Task CheckPushButtonAsync()
    {
        using var server = new TcpListener(IPAddress.Loopback, 0);
        server.Start();
        CustomSettingsManager.Values["CLAMP_PLC_IP"] = "127.0.0.1";
        CustomSettingsManager.Values["CLAMP_PLC_PORT"] = ((IPEndPoint)server.LocalEndpoint).Port.ToString();
        var settings = new PushButtonSettings { AddressSt1 = "M800", Enabled = false, PollMs = 900 };
        settings.Save();
        using var watcher = new PushButtonWatcher();
        watcher.Start();
        Check(!watcher.Running, "disabled watcher started");
        int presses = 0;
        watcher.Pressed += (_, _) => presses++;
        void Save()
        {
            settings.Save();
            // Test drives ticks explicitly while keeping Running true, on the real UI message loop.
            if (watcher.Running)
                ((System.Windows.Forms.Timer)typeof(PushButtonWatcher).GetField("_timer", Private)!.GetValue(watcher)!).Interval = 60000;
        }
        async Task Read(bool on, int address, Action? duringRead = null)
        {
            var tick = (Task)typeof(PushButtonWatcher).GetMethod("TickAsync", Private)!.Invoke(watcher, null)!;
            using var peer = await server.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(3));
            var request = new byte[21];
            await peer.GetStream().ReadExactlyAsync(request);
            Check((request[15] | request[16] << 8 | request[17] << 16) == address, "watcher read old address");
            duringRead?.Invoke();
            await peer.GetStream().WriteAsync(new byte[] { 0xD0, 0, 0, 0xFF, 0xFF, 3, 0, 3, 0, 0, 0, on ? (byte)0x10 : (byte)0 });
            await tick;
        }
        settings.Enabled = true;
        Save();
        Check(watcher.Running && watcher.Address == "M800", "enable after Save did not start watcher");
        await Read(false, 800); await Read(true, 800); await Read(true, 800); await Read(false, 800);
        Check(presses == 1, "held PLC bit released more than once");
        await Read(true, 800, () => { settings.AddressSt1 = "M801"; Save(); });
        Check(presses == 1 && watcher.Address == "M801", "old in-flight reply fired after address change");
        await Read(true, 801); // First sample after reload must only resync.
        Check(presses == 1, "reload counted already-high bit as a press");
        await Read(false, 801); await Read(true, 801);
        Check(presses == 2, "new address did not release on a new edge");
        await Read(false, 801);
        await Read(true, 801, () => { settings.Enabled = false; Save(); });
        Check(presses == 2 && !watcher.Running, "disable allowed stale press");
        settings.Enabled = true;
        Save();
        await Read(false, 801);
        await Read(true, 801, watcher.Dispose);
        Check(presses == 2, "dispose allowed stale press");
        settings.Save(); // Disposed watcher must be unsubscribed.
        Console.WriteLine("PASS: Pushbutton save enables/reloads/disables; stale reads discarded; high bit resync; dispose unsubscribes");
    }
}

namespace InkjetOperator.Services
{
    // Only configuration is stubbed. All network traffic is loopback; no production settings are read or written.
    public static class CustomSettingsManager
    {
        public static readonly Dictionary<string, string> Values = new();
        public static string Read(string key, string fallback = "") => Values.GetValueOrDefault(key, fallback);
        public static void Write(string key, string value) => Values[key] = value;
    }
    public static class StationService { public static int Current => 1; }
}
