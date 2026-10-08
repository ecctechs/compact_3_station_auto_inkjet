using InkjetOperator.Services;
using Microsoft.Data.Sqlite;

internal static class ParallelSendRegression
{
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }

    public static async Task RunAsync()
    {
        await CheckPreflightAsync();
        foreach (var marking in new[] { "11", "12", "32" })
        {
            var steps = MarkingMethodService.Resolve(marking).Steps;
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            int started = 0;
            var batch = MachineSendBatch.RunAsync(steps, x => x, async machine =>
            {
                if (Interlocked.Increment(ref started) == 2) entered.SetResult();
                await release.Task;
                return machine;
            }, (_, ex) => throw ex);
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Check(!batch.IsCompleted, "batch completed before both devices finished");
            }
            finally { release.TrySetResult(); }
            Check((await batch).SequenceEqual(steps), $"{marking}: independent devices did not overlap");
        }

        int calls = 0;
        await MachineSendBatch.RunAsync(MarkingMethodService.Resolve("22").Steps, x => x,
            x => { calls++; return Task.FromResult(x); }, (_, ex) => throw ex);
        Check(calls == 1, "22 sent MK round two without release");

        foreach (bool syncFailure in new[] { true, false })
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            bool siblingStarted = false;
            var batch = MachineSendBatch.RunAsync(new[] { "MK", "UV2" }, x => x, machine =>
            {
                if (machine == "MK")
                {
                    if (syncFailure) throw new IOException("injected");
                    return Task.FromException<string>(new IOException("injected"));
                }
                siblingStarted = true;
                return FinishAsync();
                async Task<string> FinishAsync() { await release.Task; return "sent"; }
            }, (_, _) => "failed");
            Check(siblingStarted && !batch.IsCompleted, "one failure prevented or abandoned sibling send");
            release.SetResult();
            Check((await batch).SequenceEqual(new[] { "failed", "sent" }), "partial results lost");
        }

        using (var mk = MachineBusy.TryHoldExclusive("MK"))
        {
            Check(mk != null && MachineBusy.TryHoldExclusive("mk") == null, "same machine overlaps");
            using (MachineBusy.Hold("MK")) { }
            Check(MachineBusy.IsBusy("MK"), "nested release made busy machine free");
            using var uv = MachineBusy.TryHoldExclusive("UV2");
            Check(uv != null, "MK blocked independent UV2");
        }
        Check(!MachineBusy.Active, "busy lease leaked");
        using (var again = MachineBusy.TryHoldExclusive("MK")) Check(again != null, "machine cannot be reused");

        string path = Path.Combine(Path.GetTempPath(), $"compact-cpi-test-{Guid.NewGuid():N}.db3");
        try
        {
            await using (var db = new SqliteConnection(SqlitePath.ReadWriteCreate(path)))
            {
                await db.OpenAsync();
                foreach (var table in new[] { "MK063", "MK067" })
                {
                    await using var cmd = db.CreateCommand();
                    cmd.CommandText = $"CREATE TABLE {table}(id INTEGER PRIMARY KEY,lot TEXT,name TEXT,text1 TEXT,text2 TEXT,text3 TEXT,text4 TEXT,text5 TEXT); INSERT INTO {table}(id) VALUES(1);";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            var writes = await Task.WhenAll(
                CpiWriteService.WriteAsync(path, "MK063", "LOT-1", "UV1", null, null, null, null, null),
                CpiWriteService.WriteAsync(path.Replace('\\', '/'), "MK067", "LOT-2", "UV2", null, null, null, null, null));
            Check(writes.All(r => r.ok), "concurrent shared CPI write failed");
            Check(!(await CpiWriteService.WriteAsync(path, "missing", "bad", null, null, null, null, null, null)).ok,
                "invalid CPI table reported success");
            Check((await CpiWriteService.WriteAsync(path, "MK063", "LOT-1", "UV1", null, null, null, null, null)).ok,
                "CPI lock leaked after failure");
            await using var checkDb = new SqliteConnection(SqlitePath.ReadOnly(path));
            await checkDb.OpenAsync();
            await using var read = checkDb.CreateCommand();
            read.CommandText = "SELECT lot FROM MK063 UNION ALL SELECT lot FROM MK067";
            await using var reader = await read.ExecuteReaderAsync();
            var lots = new List<string>();
            while (await reader.ReadAsync()) lots.Add(reader.GetString(0));
            Check(lots.SequenceEqual(new[] { "LOT-1", "LOT-2" }), "shared CPI crossed or lost data");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
        Console.WriteLine("PASS: parallel 11/12/32, serial 22, partial failure, device gate and shared CPI");
    }

    private static async Task CheckPreflightAsync()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        var check = ConnectionPreflight.CheckAsync(
            new[] { ("MK", 9000), ("UV", 10086), ("mk", 9000) }, async (host, _) =>
            {
                if (Interlocked.Increment(ref calls) == 2) entered.SetResult();
                await release.Task;
                return host == "MK";
            });
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Check(calls == 2 && !check.IsCompleted, "preflight must overlap independent endpoints and deduplicate identical endpoint");
        }
        finally { release.TrySetResult(); }
        Check((await check).SequenceEqual(new[] { true, false, true }), "preflight lost target order or failed endpoint");
        var failures = await ConnectionPreflight.CheckAsync(new[] { ("MK", 9000), ("UV", 10086) },
            (host, _) => host == "MK" ? throw new IOException("injected") : Task.FromResult(true));
        Check(failures.SequenceEqual(new[] { false, true }), "preflight exception hid another endpoint result");
        Check((await ConnectionPreflight.CheckAsync(Array.Empty<(string, int)>(), (_, _) => throw new Exception())).Length == 0,
            "empty preflight contacted a device");
        Console.WriteLine("PASS: concurrent connection preflight, endpoint deduplication and failure isolation");
    }
}
