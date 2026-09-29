using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class JobStationService
{
    public static int? StationOf(string? command)
    {
        if (string.Equals(command, "MK", StringComparison.OrdinalIgnoreCase)) return 1;
        if (string.Equals(command, "UV1", StringComparison.OrdinalIgnoreCase)) return 2;
        if (string.Equals(command, "UV2", StringComparison.OrdinalIgnoreCase)) return 3;

        return null;
    }

    public static int? Current(IEnumerable<CommandResult>? commands)
    {
        if (commands == null) return null;

        int? station = null;
        var latest = DateTime.MinValue;
        int index = 0, latestIndex = -1;

        foreach (var command in commands)
        {
            int position = index++;
            if (!command.Success) continue;
            if (StationOf(command.Command) is not int candidate) continue;

            var when = ParseSentAt(command.SentAt);
            if (when < latest) continue;
            if (when == latest && position < latestIndex) continue;

            latest = when;
            latestIndex = position;
            station = candidate;
        }

        return station;
    }

    public static string Label(int? station) => station == null ? "" : $"ST{station}";

    private static DateTime ParseSentAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DateTime.MinValue;

        return DateTime.TryParse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : DateTime.MinValue;
    }
}
