using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BotClickApp
{
    public enum TransformRuleType
    {
        DELETE,
        FIX_TEXT,
        COPY,
        PAD_LEFT,
        PAD_RIGHT,
        AZ_LOWER,
        AZ_UPPER,
        YEAR_AZ,
        TAKE_RIGHT,
        TAKE_LEFT
    }

    public class Rule
    {
        public int SourceStart { get; set; }
        public int SourceEnd { get; set; }
        public TransformRuleType TransformRule { get; set; }
        public string Parameter { get; set; }
        public bool IsActive { get; set; }

        public Rule()
        {
            Parameter = string.Empty;
            IsActive = true;
        }

        public string Apply(string input)
        {
            if (!IsActive)
                return string.Empty;

            if (input == null)
                input = string.Empty;

            // convert 1-based indexes to 0-based
            int s = Math.Max(1, SourceStart);
            int e = Math.Max(0, SourceEnd);

            string extracted = string.Empty;

            if (s <= e && s <= input.Length)
            {
                int startIdx = s - 1;
                int len = Math.Min(e, input.Length) - startIdx;
                if (len > 0 && startIdx >= 0 && startIdx < input.Length)
                {
                    extracted = input.Substring(startIdx, len);
                }
            }

            switch (TransformRule)
            {
                case TransformRuleType.DELETE:
                    return string.Empty;
                case TransformRuleType.FIX_TEXT:
                    return Parameter ?? string.Empty;
                case TransformRuleType.COPY:
                    return extracted;
                case TransformRuleType.PAD_LEFT:
                    {
                        int total = 0;
                        if (!int.TryParse(Parameter, out total) || total <= 0)
                            return extracted;
                        return extracted.PadLeft(total, '0');
                    }
                case TransformRuleType.PAD_RIGHT:
                    {
                        int total = 0;
                        if (!int.TryParse(Parameter, out total) || total <= 0)
                            return extracted;
                        return extracted.PadRight(total, '0');
                    }
                case TransformRuleType.AZ_LOWER:
                    {
                        if (int.TryParse(extracted, out int num))
                        {
                            return NumberToAZSeq(num).ToLower();
                        }
                        return string.Empty;
                    }
                case TransformRuleType.AZ_UPPER:
                    {
                        if (int.TryParse(extracted, out int num))
                        {
                            return NumberToAZSeq(num).ToUpper();
                        }
                        return string.Empty;
                    }
                case TransformRuleType.YEAR_AZ:
                    {
                        if (int.TryParse(Parameter, out int baseYear) && int.TryParse(extracted, out int year))
                        {
                            int offset = year - baseYear;
                            if (offset < 0)
                                return string.Empty;
                            return OffsetToAZ(offset);
                        }
                        return string.Empty;
                    }
                case TransformRuleType.TAKE_RIGHT:
                    {
                        int count = 0;
                        if (int.TryParse(Parameter, out count) && count > 0 && extracted.Length > 0)
                        {
                            if (count >= extracted.Length)
                                return extracted;
                            return extracted.Substring(extracted.Length - count);
                        }
                        return extracted;
                    }
                case TransformRuleType.TAKE_LEFT:
                    {
                        int count = 0;
                        if (int.TryParse(Parameter, out count) && count > 0 && extracted.Length > 0)
                        {
                            if (count >= extracted.Length)
                                return extracted;
                            return extracted.Substring(0, count);
                        }
                        return extracted;
                    }
                default:
                    return extracted;
            }
        }

        private static string NumberToAZSeq(int number)
        {
            if (number <= 0)
                return string.Empty;

            int n = number;
            string s = string.Empty;
            while (n > 0)
            {
                n--; // 1 -> a
                char ch = (char)('a' + (n % 26));
                s = ch + s;
                n /= 26;
            }
            return s;
        }

        private static string OffsetToAZ(int offset)
        {
            // offset 0 -> A, offset 25 -> Z, offset 26 -> AA
            int n = offset + 1; // map 0->1
            string s = string.Empty;
            while (n > 0)
            {
                n--;
                char ch = (char)('A' + (n % 26));
                s = ch + s;
                n /= 26;
            }
            return s;
        }
    }

    public class Pattern
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Rule> Rules { get; set; }
        public string TestBarcode { get; set; }
        public string TestBlockText { get; set; }
        public string TestPreview { get; set; }

        public Pattern()
        {
            Rules = new List<Rule>();
            TestBarcode = string.Empty;
            TestBlockText = string.Empty;
            TestPreview = string.Empty;
        }

        /// <summary>
        /// Apply rules กับ input — นับตำแหน่งรวมขีดด้วย
        /// </summary>
        public string Apply(string input)
        {
            if (input == null) input = string.Empty;

            var parts = new List<string>();
            foreach (var r in Rules)
            {
                try
                {
                    parts.Add(r.Apply(input));
                }
                catch
                {
                    parts.Add(string.Empty);
                }
            }
            return string.Concat(parts);
        }
    }

    /// <summary>
    /// Static engine — หน้าไหนก็เรียกได้
    /// หา pattern name ใน blockText → replace ตรงตำแหน่งด้วยผล rule
    /// ไม่เจอ → คืน blockText ตรงๆ
    /// </summary>
    public static class PatternEngine
    {
        /// <summary>
        /// หา pattern name ใน blockText → replace ตรงตำแหน่งด้วยผลการแปลง barcode
        /// ไม่เจอ pattern name → คืน blockText ตรงๆ
        /// </summary>
        public static string Process(string barcode, string blockText)
        {
            if (string.IsNullOrEmpty(barcode))
                return blockText ?? string.Empty;
            if (string.IsNullOrEmpty(blockText))
                return string.Empty;

            // หา pattern ที่ชื่อตรงกับ placeholder ใน blockText → replace ตรงตำแหน่ง
            foreach (var p in PatternStore.Patterns)
            {
                if (!string.IsNullOrEmpty(p.Name) && blockText.Contains(p.Name))
                {
                    string transformed = p.Apply(barcode);
                    return blockText.Replace(p.Name, transformed);
                }
            }

            // ไม่เจอ pattern name → คืน blockText ตรงๆ
            return blockText;
        }

        /// <summary>
        /// Process หลาย block พร้อมกัน (block1-block5 จาก DB)
        /// </summary>
        public static string[] ProcessBlocks(string barcode, string[] blockTexts)
        {
            if (blockTexts == null)
                return new string[0];

            var results = new string[blockTexts.Length];
            for (int i = 0; i < blockTexts.Length; i++)
            {
                results[i] = Process(barcode, blockTexts[i]);
            }
            return results;
        }
    }

    public static class PatternStore
    {
        public static List<Pattern> Patterns { get; } = new List<Pattern>();

        public static void SaveToFile(string filePath)
        {
            try
            {
                var ser = new XmlSerializer(typeof(List<Pattern>));
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    ser.Serialize(fs, Patterns);
                }
            }
            catch
            {
                // ignore
            }
        }

        public static void LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;
                var ser = new XmlSerializer(typeof(List<Pattern>));
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var list = (List<Pattern>)ser.Deserialize(fs);
                    Patterns.Clear();
                    Patterns.AddRange(list);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
