namespace InkjetOperator.Services;

public static class ThaiTime
{
    public const string Format = "dd/MM/yy HH:mm";

    public const string DateFormat = "dd/MM/yy";

    private static readonly TimeZoneInfo Zone = ResolveZone();

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Bangkok" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "ICT", "ICT");
    }

    public static DateTime ToUtc(DateTime thai) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(thai, DateTimeKind.Unspecified), Zone);

    public static DateTime? ToThai(DateTime? utc)
    {
        if (utc == null) return null;

        var value = utc.Value;
        if (value.Kind == DateTimeKind.Unspecified)
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), Zone);
    }

    public static string Text(DateTime? utc, string format = Format, string empty = "-") =>
        ToThai(utc) is { } t
            ? t.ToString(format, System.Globalization.CultureInfo.InvariantCulture)
            : empty;
}
