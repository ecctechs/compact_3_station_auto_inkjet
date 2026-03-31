using InkjetOperator.Models;

namespace InkjetOperator.Services
{
    public static class PatternEngine
    {
        public static string Process(string barcode, string blockText)
        {
            if (string.IsNullOrEmpty(barcode)) return blockText ?? string.Empty;
            if (string.IsNullOrEmpty(blockText)) return string.Empty;
            foreach (var p in PatternStore.Patterns)
            {
                if (!string.IsNullOrEmpty(p.Name) && blockText.Contains(p.Name))
                    return blockText.Replace(p.Name, p.Apply(barcode));
            }
            return blockText;
        }

        public static string[] ProcessBlocks(string barcode, string[] blockTexts)
        {
            if (blockTexts == null) return new string[0];
            var results = new string[blockTexts.Length];
            for (int i = 0; i < blockTexts.Length; i++)
                results[i] = Process(barcode, blockTexts[i]);
            return results;
        }
    }
}