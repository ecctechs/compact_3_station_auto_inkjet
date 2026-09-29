using InkjetOperator.Models;

namespace InkjetOperator.Services;

public static class PatternEngine
{
    public static string Process(string barcode, string blockText)
    {
        if (string.IsNullOrEmpty(barcode)) return blockText;
        if (string.IsNullOrEmpty(blockText)) return "";

        foreach (var pattern in PatternStore.Patterns)
        {
            if (!string.IsNullOrEmpty(pattern.Name) && blockText.Contains(pattern.Name))
            {
                return blockText.Replace(pattern.Name, pattern.Apply(barcode));
            }
        }

        return blockText;
    }

    public static string[] ProcessBlocks(string barcode, string[] blockTexts)
    {
        if (blockTexts == null) return Array.Empty<string>();

        var result = new string[blockTexts.Length];
        for (int i = 0; i < blockTexts.Length; i++)
        {
            result[i] = Process(barcode, blockTexts[i]);
        }
        return result;
    }
}
