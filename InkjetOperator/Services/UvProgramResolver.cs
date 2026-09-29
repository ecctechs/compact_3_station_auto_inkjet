using InkjetOperator.Views;

namespace InkjetOperator.Services;

public sealed record UvProgramPick(string? Program, bool IsDefault);

public static class UvProgramResolver
{
    public const string DefaultProgram = "default";

    private const string UvdxExtension = ".uvdx";

    public static UvProgramPick Resolve(string? programName, string? docFolder, IWin32Window? owner = null)
    {
        var baseName = (programName ?? "").Trim();
        if (baseName.Length == 0) return new UvProgramPick(null, false);

        if (baseName.EndsWith(UvdxExtension, StringComparison.OrdinalIgnoreCase))
            baseName = baseName[..^UvdxExtension.Length];
        if (docFolder == null) return new UvProgramPick(baseName, false);

        List<string> candidates;
        try
        {
            candidates = Directory.GetFiles(docFolder, "*.uvdx")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(f => !string.IsNullOrEmpty(f))
                .Where(f => f! == baseName ||
                            f!.StartsWith(baseName + "-", StringComparison.Ordinal))
                .OrderBy(f => f)
                .ToList()!;
        }
        catch
        {
            return new UvProgramPick(baseName, false);
        }

        if (candidates.Count == 1) return new UvProgramPick(candidates[0], false);
        if (candidates.Count > 1) return new UvProgramPick(PromptVariant(candidates, owner), false);

        return new UvProgramPick(DefaultProgram, true);
    }

    public static bool ConfirmDefault(string requestedProgram, string machineName, Control? owner = null)
    {
        var text =
            $"ไม่พบโปรแกรม \"{requestedProgram}\" ในเครื่อง {machineName}\n\n"
            + "ระบบจะใช้โปรแกรม default.uvdx แทนไปก่อน\n"
            + "กรุณาแจ้งผู้ดูแลให้เพิ่มโปรแกรมนี้เข้าเครื่อง\n\n"
            + "ต้องการทำต่อหรือไม่?";

        return Views.Confirm.Ask(owner, "ไม่พบโปรแกรม", text);
    }

    private static string? PromptVariant(List<string> variants, IWin32Window? owner)
    {
        var options = variants
            .Select(v => new MarkingRefOption(v, v + ".uvdx", MarkingRefImageService.FindImagesExact(v)))
            .ToList();

        return MarkingRefPickerDialog.Pick(
            owner,
            "เลือกโปรแกรมที่จะพิมพ์",
            "โปรแกรมนี้มีหลายรุ่นย่อยในเครื่อง — เลือกรุ่นที่จะโหลดเข้าเครื่องเพื่อพิมพ์จริง",
            options);
    }
}
