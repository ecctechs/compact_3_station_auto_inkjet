namespace InkjetOperator.Services;

public static class MachineBusy
{
    private static int _holders;

    private static readonly Dictionary<string, int> _byMachine = new(StringComparer.OrdinalIgnoreCase);

    public static bool Active => Volatile.Read(ref _holders) > 0;

    public static bool IsBusy(string? machine)
    {
        if (string.IsNullOrWhiteSpace(machine)) return false;

        lock (_byMachine)
            return _byMachine.TryGetValue(machine, out int n) && n > 0;
    }

    public static IDisposable Hold(string? machine = null) => new Holder(machine);

    public static IDisposable? TryHoldExclusive(string machine)
    {
        lock (_byMachine)
        {
            if (_byMachine.TryGetValue(machine, out int count) && count > 0) return null;
            return new Holder(machine);
        }
    }

    private sealed class Holder : IDisposable
    {
        private readonly string? _machine;
        private int _released;

        public Holder(string? machine)
        {
            _machine = string.IsNullOrWhiteSpace(machine) ? null : machine;
            Interlocked.Increment(ref _holders);

            if (_machine == null) return;
            lock (_byMachine)
                _byMachine[_machine] = _byMachine.TryGetValue(_machine, out int n) ? n + 1 : 1;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) != 0) return;

            Interlocked.Decrement(ref _holders);

            if (_machine == null) return;
            lock (_byMachine)
                if (_byMachine.TryGetValue(_machine, out int n))
                    _byMachine[_machine] = n - 1;
        }
    }
}
