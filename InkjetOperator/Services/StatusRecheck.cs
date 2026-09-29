namespace InkjetOperator.Services;

public static class StatusRecheck
{
    public const int IntervalMs = 15000;

    public static void Wire(Control page, System.Windows.Forms.Timer timer, Func<Task> check)
    {
        bool busy = false;

        timer.Interval = IntervalMs;

        timer.Tick += async (_, _) =>
        {
            if (busy || page.IsDisposed || !page.Visible) return;

            if (MachineBusy.Active) return;

            busy = true;
            try
            {
                await check();
            }
            catch
            {
            }
            finally
            {
                busy = false;
            }
        };

        page.VisibleChanged += (_, _) =>
        {
            if (page.Visible) timer.Start();
            else timer.Stop();
        };

        if (page.Visible) timer.Start();
    }
}
