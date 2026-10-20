using System.Threading;

namespace InkjetOperator.Services;

/// <summary>
/// ประสานการอ่าน PrintData.db3 กับการสลับไฟล์หลังนำเข้า Excel ภายในโปรเซสนี้
/// </summary>
internal static class PrintDataDbLock
{
    private static readonly ReaderWriterLockSlim Gate =
        new(LockRecursionPolicy.SupportsRecursion);

    public static IDisposable Read()
    {
        Gate.EnterReadLock();
        return new Releaser(Gate, write: false);
    }

    public static IDisposable Write()
    {
        Gate.EnterWriteLock();
        return new Releaser(Gate, write: true);
    }

    private sealed class Releaser(ReaderWriterLockSlim gate, bool write) : IDisposable
    {
        private ReaderWriterLockSlim? _gate = gate;

        public void Dispose()
        {
            var current = Interlocked.Exchange(ref _gate, null);
            if (current == null) return;
            if (write) current.ExitWriteLock();
            else current.ExitReadLock();
        }
    }
}
