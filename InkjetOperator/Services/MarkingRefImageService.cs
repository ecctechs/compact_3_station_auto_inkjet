using System.Drawing;

namespace InkjetOperator.Services;

/// <summary>
/// Looks up marking reference images stored as loose files in a configurable
/// folder (Setting.config key MARKING_REF_FOLDER, typically a network share).
/// Files are named by program_name; one program may have several images named
/// "{program_name}", "{program_name}-1", "{program_name}-2", ...
/// </summary>
public static class MarkingRefImageService
{
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".bmp" };

    /// <summary>Configured folder (network share). Empty if not set.</summary>
    public static string FolderPath => CustomSettingsManager.Read("MARKING_REF_FOLDER", "");

    /// <summary>
    /// Files whose name (without extension) equals <paramref name="programName"/>
    /// or starts with "{programName}-". Extension-agnostic, case-insensitive,
    /// sorted by name. Returns empty on missing folder / unreachable share.
    /// </summary>
    public static List<string> FindImages(string? programName)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(programName)) return result;

        string folder = FolderPath;
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return result;

        string name = programName.Trim();
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                if (Array.IndexOf(Extensions, Path.GetExtension(file).ToLowerInvariant()) < 0)
                    continue;

                string stem = Path.GetFileNameWithoutExtension(file);
                if (stem.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    stem.StartsWith(name + "-", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(file);
                }
            }
        }
        catch
        {
            // share unavailable / permission — treat as no images
            return new List<string>();
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Load an image as an independent copy so the source file is NOT locked
    /// (customer can overwrite the file anytime). Returns null on error.
    /// </summary>
    public static Image? LoadImageNoLock(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            using var ms = new MemoryStream(bytes);
            using var tmp = Image.FromStream(ms);
            return new Bitmap(tmp); // copy — no dependency on stream/file
        }
        catch
        {
            return null;
        }
    }
}
