namespace InkjetOperator.Services;

public static class StationService
{
    public const int St1 = 1;
    public const int St3 = 3;

    public const string ProductName = "Compact Inkjet";

    public static int Current => Level == St3 ? St3 : St1;

    public static bool IsSt3 => Current == St3;

    public static string ProgramTitle => Level switch
    {
        0 => $"{ProductName} - Scan barcode",
        1 => $"{ProductName} - Station 1",
        3 => $"{ProductName} - Station 3",
        _ => ProductName,
    };

    public const string ManualRemoteSendKey = "ST3_MANUAL_SEND";

    public static bool ManualRemoteSendEnabled =>
        CustomSettingsManager.Read(ManualRemoteSendKey, "0") == "1";

    public const string HoldForNextRoundKey = "MK_HOLD_FOR_ROUND2";

    public static bool HoldForNextRound =>
        CustomSettingsManager.Read(HoldForNextRoundKey, "1") == "1";

    public const string ProcessTabsKey = "ORDER_PROCESS_TABS";

    public enum ProcessTabsMode
    {
        Off,

        DevOnly,

        Stations,
    }

    public static ProcessTabsMode ProcessTabs =>
        CustomSettingsManager.Read(ProcessTabsKey, "").Trim().ToLowerInvariant() switch
        {
            "off" => ProcessTabsMode.Off,
            "stations" => ProcessTabsMode.Stations,
            _ => ProcessTabsMode.DevOnly,
        };

    public static string ProcessTabsValue(ProcessTabsMode mode) => mode switch
    {
        ProcessTabsMode.Off => "off",
        ProcessTabsMode.Stations => "stations",
        _ => "dev",
    };

    public static bool ShowProcessTabs => ProcessTabs switch
    {
        ProcessTabsMode.Stations => Level is St1 or St3 || IsDevMode,
        ProcessTabsMode.DevOnly => IsDevMode,
        _ => false,
    };

    public static bool IsDevMode => Level == 99;

    private static int Level
    {
        get
        {
            var raw = CustomSettingsManager.Read("MENU_LEVEL", "1");
            return int.TryParse(raw, out var level) ? level : St1;
        }
    }
}
