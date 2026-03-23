using System;

namespace InkjetOperator.Models
{
    public class Rule
    {
        public int SourceStart { get; set; }
        public int SourceEnd { get; set; }
        public TransformRuleType TransformRule { get; set; }
        public string Parameter { get; set; }
        public bool IsActive { get; set; }

        public Rule() { Parameter = string.Empty; IsActive = true; }

        public string Apply(string input)
        {
            if (!IsActive) return string.Empty;
            input = input ?? string.Empty;
            string ext = Extract(input);
            switch (TransformRule)
            {
                case TransformRuleType.DELETE: return string.Empty;
                case TransformRuleType.FIX_TEXT: return Parameter ?? string.Empty;
                case TransformRuleType.COPY: return ext;
                case TransformRuleType.PAD_LEFT: return (Parameter ?? "") + ext;
                case TransformRuleType.PAD_RIGHT: return ext + (Parameter ?? "");
                case TransformRuleType.AZ_LOWER: return SwapAZ(ext, true);
                case TransformRuleType.AZ_UPPER: return SwapAZ(ext, false);
                case TransformRuleType.TAKE_RIGHT: return TakeRight(ext);
                case TransformRuleType.TAKE_LEFT: return TakeLeft(ext);
                default: return ext;
            }
        }

        private string Extract(string input)
        {
            int s = Math.Max(1, SourceStart);
            int e = Math.Max(0, SourceEnd);
            if (s > e || s > input.Length) return string.Empty;
            int idx = s - 1;
            int len = Math.Min(e, input.Length) - idx;
            return (len > 0) ? input.Substring(idx, len) : string.Empty;
        }

        private string SwapAZ(string ext, bool lower)
        {
            int num;
            if (!int.TryParse(ext, out num)) return string.Empty;
            int baseVal = 0;
            if (!string.IsNullOrEmpty(Parameter)) int.TryParse(Parameter, out baseVal);
            int offset = num - baseVal;
            if (offset < 0) return string.Empty;
            string az = OffsetToAZ(offset);
            return lower ? az.ToLower() : az.ToUpper();
        }

        private string TakeRight(string s)
        {
            int n;
            if (!int.TryParse(Parameter, out n) || n <= 0) return s;
            return n >= s.Length ? s : s.Substring(s.Length - n);
        }

        private string TakeLeft(string s)
        {
            int n;
            if (!int.TryParse(Parameter, out n) || n <= 0) return s;
            return n >= s.Length ? s : s.Substring(0, n);
        }

        private static string OffsetToAZ(int offset)
        {
            int n = offset + 1; string s = string.Empty;
            while (n > 0) { n--; s = (char)('A' + (n % 26)) + s; n /= 26; }
            return s;
        }
    }
}