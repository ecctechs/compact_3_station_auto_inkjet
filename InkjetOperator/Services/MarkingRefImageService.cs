using System.Drawing;

namespace InkjetOperator.Services;

public static class MarkingRefImageService
{
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".bmp" };

    public static string FolderPath => CustomSettingsManager.Read("MARKING_REF_FOLDER", "");

    public enum FolderState
    {
        NotConfigured,

        Unreachable,

        Ok,
    }

    public static FolderState CheckFolder()
    {
        var folder = FolderPath;
        if (string.IsNullOrWhiteSpace(folder)) return FolderState.NotConfigured;

        try
        {
            return Directory.Exists(folder) ? FolderState.Ok : FolderState.Unreachable;
        }
        catch
        {
            return FolderState.Unreachable;
        }
    }

    public static string DescribeEmpty(FolderState state) => state switch
    {
        FolderState.NotConfigured => "ยังไม่ได้ตั้งโฟลเดอร์รูปอ้างอิง (Setting → Inkjet Setting)",
        FolderState.Unreachable => "เข้าโฟลเดอร์รูปอ้างอิงไม่ได้ — ตรวจการเชื่อมต่อ share",
        _ => "ไม่มีรูปอ้างอิง",
    };

    public static List<string> FindImages(string? programName) => Search(programName, true);

    public static List<string> FindImagesExact(string? programName) => Search(programName, false);

    private static List<string> Search(string? programName, bool includeSuffixed)
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
                bool match = stem.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    (includeSuffixed && stem.StartsWith(name + "-", StringComparison.OrdinalIgnoreCase));

                if (match) result.Add(file);
            }
        }
        catch
        {
            return new List<string>();
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    public static Image Placeholder() => new Bitmap(Properties.Resources.NoImageAvailable);

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
