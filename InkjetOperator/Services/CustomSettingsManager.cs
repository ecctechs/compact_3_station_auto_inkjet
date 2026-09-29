namespace InkjetOperator.Services;

public static class CustomSettingsManager
{
    private static readonly SettingsStore _store = new(AppSettingsFile.Resolve("Setting.config"));

    public static string? LastError { get; private set; }

    internal static void ReportWriteError(string message) => LastError = message;

    public static string Read(string key, string defaultValue = "") => _store.Read(key, defaultValue);

    public static bool Write(string key, string value)
    {
        LastError = _store.Write(key, value);
        return LastError == null;
    }
}
