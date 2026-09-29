namespace InkjetOperator.Theme;

internal static class JobStatusDisplay
{
    public static (string Text, Color Fore) Resolve(string? status)
    {
        if (string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase))
            return ("Working", DesignTokens.Warning);
        if (string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase))
            return ("Finished", DesignTokens.SuccessText);
        if (string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return ("Waiting", DesignTokens.Danger);
        if (string.Equals(status, "Cancel", StringComparison.OrdinalIgnoreCase))
            return ("Cancelled", DesignTokens.TextMuted);

        return (status ?? "", DesignTokens.Danger);
    }

    public static (string Text, Color Fore) Resolve(string? status, bool finishedIncomplete) =>
        finishedIncomplete ? ("Incomplete", DesignTokens.Warning) : Resolve(status);

    public static string Text(string? status) => Resolve(status).Text;

    public static Color Fore(string? status) => Resolve(status).Fore;
}
