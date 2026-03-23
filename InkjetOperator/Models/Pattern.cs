using System.Collections.Generic;

namespace InkjetOperator.Models
{
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

        public string Apply(string input)
        {
            input = input ?? string.Empty;
            var parts = new List<string>();
            foreach (var r in Rules)
            {
                try { parts.Add(r.Apply(input)); }
                catch { parts.Add(string.Empty); }
            }
            return string.Concat(parts);
        }
    }
}