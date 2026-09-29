namespace InkjetOperator.Services;

public static class UvSettingsManager
{
    private static readonly SettingsStore _store = new(AppSettingsFile.Resolve("uv.config"));

    public static string Read(string key, string defaultValue = "") => _store.Read(key, defaultValue);

    public static bool Write(string key, string value)
    {
        var error = _store.Write(key, value);
        if (error == null) return true;

        CustomSettingsManager.ReportWriteError(error);
        return false;
    }

    private const string REL_CPI = @"database\sys\CPI.db3";
    private const string REL_DOCUMENT = "document";

    public static string? GetCpiPath(int uvNumber)
    {
        var folder = Read(uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER");
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_CPI);
        return File.Exists(path) ? path : null;
    }

    public static string? GetDocumentFolder(int uvNumber)
    {
        var folder = Read(uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER");
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_DOCUMENT);
        return Directory.Exists(path) ? path : null;
    }
}
