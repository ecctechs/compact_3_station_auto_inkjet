using Microsoft.Data.Sqlite;

namespace InkjetOperator.Services;

public static class SqlitePath
{
    public static string ReadOnly(string path) => Build(path, SqliteOpenMode.ReadOnly);

    public static string ReadWrite(string path) => Build(path, SqliteOpenMode.ReadWrite);

    public static string ReadWriteCreate(string path) => Build(path, SqliteOpenMode.ReadWriteCreate);

    private static string Build(string path, SqliteOpenMode mode) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = Normalize(path),
            Mode = mode,
        }.ToString();

    private static string Normalize(string path)
    {
        var trimmed = (path ?? "").Trim();
        return trimmed.StartsWith(@"\\", StringComparison.Ordinal)
            ? trimmed.Replace('\\', '/')
            : trimmed;
    }
}
