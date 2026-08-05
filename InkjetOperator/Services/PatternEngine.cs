using InkjetOperator.Models;

namespace InkjetOperator.Services;

/// <summary>
/// Applies transform patterns to printer block texts. Finds the first pattern
/// whose Name appears in the block text and replaces it with the transformed
/// barcode result.
/// </summary>
public static class PatternEngine
{
    /// <summary>
    /// barcode empty  → return blockText unchanged.
    /// blockText empty → return "".
    /// Otherwise use the first pattern whose Name appears in blockText;
    /// none found → return blockText unchanged.
    /// </summary>
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

    /// <summary>Run <see cref="Process"/> over each block text.</summary>
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
