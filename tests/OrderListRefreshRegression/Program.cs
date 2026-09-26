using System.Net;
using System.Reflection;
using InkjetOperator.Services;
using InkjetOperator.Views;
using InkjetOperator.Models;
using System.Text.Json;
using System.Collections;

internal static class Program
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static FieldInfo Field(string name) => typeof(OrderListUserControl).GetField(name, Private)!;
    private static Task Refresh(OrderListUserControl view, bool force = false) =>
        (Task)typeof(OrderListUserControl).GetMethod("RefreshDataAsync", Private)!.Invoke(view, [force])!;
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    [STAThread]
    private static int Main()
    {
        var app = typeof(OrderListUserControl).Assembly.GetType("InkjetOperator.Program")!;
        app.GetMethod("InitAntdUiIconDb", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null);
        app.GetMethod("ConfigureAntdUi", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null);
        int exit = 0;
        using var context = new ApplicationContext();
        using var view = new TestOrderList();
        _ = view.Handle;
        view.BeginInvoke(new Action(async () =>
        {
            try { await RunAsync(view); }
            catch (Exception ex) { Console.Error.WriteLine(ex); exit = 1; }
            finally { context.ExitThread(); }
        }));
        Application.Run(context);
        return exit;
    }

    private static async Task RunAsync(OrderListUserControl view)
    {
        // Replace HTTP entirely: no real server, database, PLC or printer can be reached.
        using var handler = new TestHttp();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://test.invalid") };
        var api = new ApiClient("http://test.invalid");
        typeof(ApiClient).GetField("_http", Private)!.SetValue(api, http);
        Field("_api").SetValue(view, api);
        Check(Field("_pollTimer").GetValue(view) == null, "test must never start real polling");

        await Refresh(view);
        Check(handler.JobReads == 1 && handler.QueueReads == 1, "idle refresh must read queue only once");

        handler.HoldNextJobs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var running = Refresh(view);
        int before = handler.JobReads;
        for (int i = 0; i < 10; i++) await Refresh(view, true);
        Check(handler.JobReads == before, "refresh requests overlapped active HTTP read");
        handler.HoldNextJobs.SetResult();
        await running;
        await PumpUntilAsync(() => handler.JobReads == before + 1 && !(bool)Field("_refreshing").GetValue(view)!);
        Check(handler.QueueReads == 3, "forced refreshes should merge into one extra round");

        Field("_sendOperations").SetValue(view, 1);
        before = handler.JobReads;
        await Refresh(view, true);
        Check(handler.JobReads == before && (bool)Field("_refreshRequested").GetValue(view)!, "request lost during send");
        Field("_sendOperations").SetValue(view, 0);
        typeof(OrderListUserControl).GetMethod("SchedulePendingRefresh", Private)!.Invoke(view, null);
        await PumpUntilAsync(() => handler.JobReads == before + 1 && !(bool)Field("_refreshing").GetValue(view)!);

        handler.HoldNextJobs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        running = Refresh(view);
        int queues = handler.QueueReads;
        Field("_sendOperations").SetValue(view, 1);
        handler.HoldNextJobs.SetResult();
        await running;
        Check(handler.QueueReads == queues && (bool)Field("_refreshRequested").GetValue(view)!,
            "refresh must defer if a send starts while loading jobs");
        Field("_sendOperations").SetValue(view, 0);
        before = handler.JobReads;
        typeof(OrderListUserControl).GetMethod("SchedulePendingRefresh", Private)!.Invoke(view, null);
        await PumpUntilAsync(() => handler.JobReads == before + 1 && !(bool)Field("_refreshing").GetValue(view)!);
        Check(!StationService.IsSt3, "this regression requires the isolated test output to use ST1 defaults");
        handler.Paths.Clear();
        await (Task)typeof(OrderListUserControl).GetMethod("OnPushButtonPressedAsync", Private)!.Invoke(view, ["UV2"])!;
        Check(handler.Paths.SequenceEqual(new[] {
            "/machine-queue/getAll", "/machine-queue/release", "/machine-queue/getAll",
            "/job/getAll", "/machine-queue/getAll"
        }), "after release, queue dispatch must run before the full job refresh");
        Check(handler.Unexpected == 0, "unexpected API/hardware path in test");
        await CheckConcurrentReleaseAsync(view, handler);
        await CheckIndependentDispatchAsync(view, handler);
        CheckMachineStatusColumn(view);
        Console.WriteLine("PASS: real OrderList refresh, one queue read, coalesced requests, deferred refresh during sending, immediate queue check after release");
    }

    private static void CheckMachineStatusColumn(OrderListUserControl view)
    {
        var table = (AntdUI.Table)Field("tblOrders").GetValue(view)!;
        Check(table.Columns!.Last().Key == "MachineStatus" && table.Columns.Last().Width == "460", "new column missing");
        Check(table.Columns.Single(c => c.Key == "Status").Width == "8%" &&
            table.Columns.Single(c => c.Key == "Op").Width == "12%", "existing column widths changed");
        Check(typeof(OrderListUserControl).GetField("spinSending", Private) == null, "spinner remains in Designer");
        var job = new PrintJob { Id = 990, Status = "Waiting", PlanRouting = new PlanRoutingDto { MarkingMethod = "12" } };
        Field("_allJobs").SetValue(view, new List<PrintJob> { job });
        var mk = new MachineQueueRow { Id = 991, PrintJobsId = 990, Machine = "MK", State = "pending" };
        var uv = new MachineQueueRow { Id = 992, PrintJobsId = 990, Machine = "UV2", State = "pending" };
        Field("_queueRows").SetValue(view, new List<MachineQueueRow> { mk, uv });
        typeof(OrderListUserControl).GetMethod("RebindTable", Private)!.Invoke(view, null);
        var rows = (IList)table.DataSource!;
        var row = rows[0]!;
        var originalStatus = row.GetType().GetProperty("Status")!.GetValue(row);
        AntdUI.CellTag[] Tags() => (AntdUI.CellTag[])row.GetType().GetProperty("MachineStatus")!.GetValue(row)!;
        Check(Tags().Length == 2 && Tags().All(t => t.Text!.Contains("รอคิว")), "waiting machines not shown");
        var update = typeof(OrderListUserControl).GetMethod("SetMachineStatus", Private)!;
        update.Invoke(view, [mk, "กำลังส่ง", AntdUI.TTypeMini.Primary]);
        update.Invoke(view, [uv, "ส่งแล้ว", AntdUI.TTypeMini.Success]);
        Check(Tags()[0].Type == AntdUI.TTypeMini.Primary && Tags()[1].Type == AntdUI.TTypeMini.Success,
            "independent machine colors not updated");
        update.Invoke(view, [mk, "ต้องตรวจสอบก่อนส่งซ้ำ", AntdUI.TTypeMini.Warn]);
        Check(Tags()[0].Text!.Contains("ตรวจสอบ") && Tags()[1].Text!.Contains("ส่งแล้ว"), "uncertain MK overwrote UV2 success");
        Check(ReferenceEquals(originalStatus, row.GetType().GetProperty("Status")!.GetValue(row)), "original Status column mutated");
        Check(((Control)Field("tlpTableInner").GetValue(view)!).Enabled, "table still disabled by sending overlay");
        // Render only the added column to inspect long Thai labels; production columns are unchanged.
        var visible = table.Columns.Select(c => c.Visible).ToArray();
        try
        {
            foreach (var col in table.Columns) col.Visible = col.Key == "MachineStatus";
            view.Size = new Size(1400, 1000);
            view.PerformLayout();
            table.Refresh();
            using var bitmap = new Bitmap(table.Width, table.Height);
            table.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            string path = Path.Combine(Path.GetTempPath(), "compact-machine-status-preview.png");
            bitmap.Save(path);
            Console.WriteLine("PREVIEW: " + path);
        }
        finally { for (int i = 0; i < table.Columns.Count; i++) table.Columns[i].Visible = visible[i]; }
        table.ScrollColumn("MachineStatus", true, false);
        table.Refresh();
        using (var bitmap = new Bitmap(table.Width, table.Height))
        {
            table.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(Path.GetTempPath(), "compact-machine-status-full-table.png"));
        }
        Console.WriteLine("PASS: added machine status column, independent live colors, old Status unchanged, spinner removed");
        // Restoring a job preserves history but must not reuse an old queue's labels.
        job.Commands = [new CommandResult { Command = "UV2", Success = true }];
        void Snapshot(params MachineQueueRow[] queues)
        {
            Field("_queueRows").SetValue(view, queues.ToList());
            typeof(OrderListUserControl).GetMethod("ReconcileMachineStatus", Private)!.Invoke(view, null);
            typeof(OrderListUserControl).GetMethod("UpdateMachineStatusCells", Private)!.Invoke(view, null);
        }
        Snapshot();
        Check(Tags().Length == 2 && Tags().All(t => t.Text!.Contains("ยังไม่เข้าคิว")),
            "cleared/restored job retained old sent/error labels or successful command history");
        var nextMk = new MachineQueueRow { Id = 993, PrintJobsId = 990, Machine = "MK", State = "active" };
        Snapshot(nextMk);
        Check(Tags().Length == 2 && Tags()[0].Text!.Contains("รอส่ง"), "new queue inherited old labels or duplicate tags");
        update.Invoke(view, [nextMk, "ส่งแล้ว", AntdUI.TTypeMini.Success]);
        nextMk.DispatchState = "unknown";
        Snapshot(nextMk);
        Check(Tags()[0].Type == AntdUI.TTypeMini.Warn, "local success hid backend unknown result");
        var active = (HashSet<int>)Field("_activeStatusQueues").GetValue(view)!;
        active.Add(nextMk.Id);
        update.Invoke(view, [nextMk, "กำลังส่ง", AntdUI.TTypeMini.Primary]);
        Snapshot();
        Check(Tags()[0].Text!.Contains("กำลังส่ง"), "snapshot removed an in-flight status");
        active.Clear();
        Snapshot();
        Check(Tags()[0].Text!.Contains("ยังไม่เข้าคิว"), "finished send cache never cleared");
        nextMk.DispatchState = "not_sent";
        Snapshot(nextMk);
        Check(Tags()[0].Type == AntdUI.TTypeMini.Error, "definite no-send lost its failure status");
        Console.WriteLine("PASS: cleared/restored queues discard old status/history, new queue IDs stay separate, backend uncertainty wins, live progress retained");
    }

    private static Task Press(OrderListUserControl view, string machine) =>
        (Task)typeof(OrderListUserControl).GetMethod("OnPushButtonPressedAsync", Private)!.Invoke(view, [machine])!;

    private static bool CanRelease(OrderListUserControl view, string machine) =>
        (bool)typeof(OrderListUserControl).GetMethod("CanReleaseNow", Private)!.Invoke(view, [machine])!;

    private static async Task CheckConcurrentReleaseAsync(OrderListUserControl view, TestHttp handler)
    {
        handler.Releases.Clear();
        handler.ReleaseGates["MK"] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.ReleaseGates["UV2"] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Field("_sendOperations").SetValue(view, 1); // Another send does not block release of an idle machine.
        var mk = Press(view, "MK");
        await Press(view, "MK");
        var uv = Press(view, "UV2");
        Check(handler.Releases.SequenceEqual(new[] { "MK", "UV2" }), "independent releases blocked, or duplicate MK accepted");
        handler.ReleaseGates["UV2"].SetResult();
        await uv;
        Check(!mk.IsCompleted && CanRelease(view, "UV2") && !CanRelease(view, "MK"), "UV2 release remained blocked by MK");
        handler.ReleaseGates["MK"].SetResult();
        await mk;
        Check((int)Field("_sendOperations").GetValue(view)! == 1, "one dispatcher cleared another sender's busy state");
        handler.ReleaseGates.Clear();
        Field("_sendOperations").SetValue(view, 0);
        await Refresh(view, true);
        Console.WriteLine("PASS: simultaneous MK/UV2 release, duplicate MK suppressed, independent completion");
    }

    private static async Task CheckIndependentDispatchAsync(OrderListUserControl view, TestHttp handler)
    {
        var itemType = typeof(OrderListUserControl).GetNestedType("PreparedQueueSend", BindingFlags.NonPublic)!;
        var listType = typeof(List<>).MakeGenericType(itemType);
        Task SendMany(params (string Machine, int Id)[] rows)
        {
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var (machine, id) in rows)
                list.Add(Activator.CreateInstance(itemType, new MachineQueueRow { Id = id, Machine = machine, PrintJobsId = 2, ProgramName = "test" }, new ResolvedJobResponse()));
            return (Task)typeof(OrderListUserControl).GetMethod("SendPreparedBatchAsync", Private)!.Invoke(view, [list, false, false])!;
        }
        handler.BeginGates[101] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.BeginGates[102] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var mk = SendMany(("MK", 101));
        var uv = SendMany(("UV2", 102));
        await SendMany(("MK", 101));
        Check(handler.Begins.SequenceEqual(new[] { 101, 102 }), "UV2 waited for MK or MK dispatched twice");
        // Reject permission before hardware IO: exercises real orchestration without touching devices.
        handler.BeginGates[102].SetResult();
        await uv;
        Check(!mk.IsCompleted && !MachineBusy.IsBusy("UV2") && MachineBusy.IsBusy("MK"), "completed UV2 kept waiting for MK");
        Check(CanRelease(view, "UV2") && !CanRelease(view, "MK"), "dispatch ownership was not released per machine");
        handler.BeginGates[101].SetResult();
        await mk;
        Check(!MachineBusy.Active, "dispatch leaked busy lease");
        handler.BeginGates[201] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.BeginGates[202] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        handler.BeginGates[203] = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var batch = SendMany(("MK", 201), ("UV2", 202));
        handler.BeginGates[202].SetResult();
        await PumpUntilAsync(() => CanRelease(view, "UV2"));
        Check(!batch.IsCompleted, "test must leave MK pending when UV2 finishes");
        var nextUv = SendMany(("UV2", 203));
        handler.BeginGates[201].SetResult();
        await batch;
        Check(!nextUv.IsCompleted && !CanRelease(view, "UV2"), "old batch cleared ownership of a new UV2 dispatch");
        handler.BeginGates[203].SetResult();
        await nextUv;
        Check(!MachineBusy.Active && CanRelease(view, "UV2"), "new UV2 lease leaked");
        Console.WriteLine("PASS: independent real dispatch paths, no duplicate begin, per-machine lease cleanup without hardware IO");
    }

    private static async Task PumpUntilAsync(Func<bool> done)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!done()) await Task.Delay(10, timeout.Token);
    }

    private sealed class TestOrderList : OrderListUserControl
    {
        // Do not raise the production Load event: it starts hardware watchers.
        protected override void OnLoad(EventArgs e) { }
    }

    private sealed class TestHttp : HttpMessageHandler
    {
        public int JobReads, QueueReads, Unexpected;
        public List<string> Paths = [];
        public List<string> Releases = [];
        public List<int> Begins = [];
        public Dictionary<string, TaskCompletionSource> ReleaseGates = [];
        public Dictionary<int, TaskCompletionSource> BeginGates = [];
        public TaskCompletionSource? HoldNextJobs;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body;
            Paths.Add(request.RequestUri!.AbsolutePath);
            if (request.RequestUri.AbsolutePath.EndsWith("/begin-send"))
            {
                int id = int.Parse(request.RequestUri.AbsolutePath.Split('/')[2]);
                Begins.Add(id);
                await BeginGates[id].Task.WaitAsync(cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("test: permission denied before device IO") };
            }
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/job/getAll":
                    JobReads++;
                    var hold = HoldNextJobs;
                    if (hold != null)
                    {
                        await hold.Task.WaitAsync(cancellationToken);
                        HoldNextJobs = null;
                    }
                    body = "{\"data\":{\"data\":[]}}";
                    break;
                case "/machine-queue/getAll": QueueReads++; body = "{\"data\":[]}"; break;
                case "/machine-queue/release":
                    var payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                    string machine = payload.RootElement.GetProperty("machine").GetString()!;
                    Releases.Add(machine);
                    if (ReleaseGates.TryGetValue(machine, out var gate)) await gate.Task.WaitAsync(cancellationToken);
                    // Another client may finish dispatch before the next read; return no ready rows above.
                    body = "{\"data\":{\"next\":{\"id\":42,\"machine\":\"UV2\",\"state\":\"active\"}}}";
                    break;
                default: Unexpected++; throw new Exception("Unexpected request: " + request.RequestUri);
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
        }
    }
}
