namespace InkjetOperator.Services;

public sealed record UvProgramInfo(string? Program, bool Confirmed);

public sealed record MarkingRefSide(
    string Side,
    string Machine,
    string Step,
    string? LookupName,
    List<string> Images,
    bool Pending,
    int NearMiss);

public static class MarkingRefResolver
{
    public const string MkMachineLabel = "MK";

    public static List<MarkingRefSide> Resolve(
        string? markingMethod, string? erpMfg, UvProgramInfo? uv1, UvProgramInfo? uv2)
    {
        var plan = MarkingMethodService.Resolve(markingMethod);
        var sides = new List<MarkingRefSide>();
        if (plan.NoCase) return sides;

        Add(sides, "Plate", plan.Plate, erpMfg, "P-", uv1, uv2);
        Add(sides, "Shim", plan.Shim, erpMfg, "S-", uv1, uv2);
        return sides;
    }

    private static void Add(
        List<MarkingRefSide> sides, string side, MarkingMachine machine,
        string? erpMfg, string erpPrefix, UvProgramInfo? uv1, UvProgramInfo? uv2)
    {
        if (machine == MarkingMachine.None) return;

        string label;
        string step;
        string? lookup;
        bool confirmed = true;
        bool exactOnly = true;

        switch (machine)
        {
            case MarkingMachine.Mk:
                label = MkMachineLabel;
                step = "MK";
                var erp = (erpMfg ?? "").Trim();
                lookup = erp.Length == 0 ? null : erpPrefix + erp;
                break;

            case MarkingMachine.Uv1:
                label = UvSettingsManager.Read("UV1_NAME", "UV-001");
                step = "UV1";
                lookup = Clean(uv1?.Program);
                confirmed = uv1?.Confirmed ?? false;
                exactOnly = confirmed;
                break;

            default:
                label = UvSettingsManager.Read("UV2_NAME", "UV-002");
                step = "UV2";
                lookup = Clean(uv2?.Program);
                confirmed = uv2?.Confirmed ?? false;
                exactOnly = confirmed;
                break;
        }

        var images = new List<string>();
        int nearMiss = 0;

        if (lookup != null)
        {
            images = exactOnly
                ? MarkingRefImageService.FindImagesExact(lookup)
                : MarkingRefImageService.FindImages(lookup);

            if (exactOnly && images.Count == 0)
                nearMiss = MarkingRefImageService.FindImages(lookup).Count;
        }

        bool pending = !confirmed && images.Count > 1;

        sides.Add(new MarkingRefSide(side, label, step, lookup, images, pending, nearMiss));
    }

    private static string? Clean(string? name)
    {
        var value = (name ?? "").Trim();
        return value.Length == 0 ? null : value;
    }
}
