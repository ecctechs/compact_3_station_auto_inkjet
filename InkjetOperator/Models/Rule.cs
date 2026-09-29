namespace InkjetOperator.Models;

public class Rule
{
    public int SourceStart { get; set; }

    public int SourceEnd { get; set; }

    public TransformRuleType TransformRule { get; set; } = TransformRuleType.COPY;

    public string Parameter { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public string Extract(string input)
    {
        input ??= "";
        int s = Math.Max(1, SourceStart);
        int e = Math.Max(0, SourceEnd);
        if (s > e || s > input.Length) return "";

        int idx = s - 1;
        int len = Math.Min(e, input.Length) - idx;
        return len > 0 ? input.Substring(idx, len) : "";
    }

    public string Apply(string input)
    {
        if (!IsActive) return "";

        string extracted = Extract(input);

        return TransformRule switch
        {
            TransformRuleType.DELETE => "",
            TransformRuleType.FIX_TEXT => Parameter ?? "",
            TransformRuleType.COPY => extracted,
            TransformRuleType.PAD_LEFT => (Parameter ?? "") + extracted,
            TransformRuleType.PAD_RIGHT => extracted + (Parameter ?? ""),
            TransformRuleType.AZ_LOWER => SwapAz(extracted, upper: false),
            TransformRuleType.AZ_UPPER => SwapAz(extracted, upper: true),
            TransformRuleType.TAKE_RIGHT => Take(extracted, fromRight: true),
            TransformRuleType.TAKE_LEFT => Take(extracted, fromRight: false),
            _ => "",
        };
    }

    private string SwapAz(string extracted, bool upper)
    {
        if (!int.TryParse(extracted, out int num)) return "";

        int baseVal = 0;
        if (!string.IsNullOrEmpty(Parameter)) int.TryParse(Parameter, out baseVal);

        int offset = num - baseVal;
        if (offset < 0) return "";

        string s = OffsetToAZ(offset);
        return upper ? s.ToUpperInvariant() : s.ToLowerInvariant();
    }

    private string Take(string extracted, bool fromRight)
    {
        if (!int.TryParse(Parameter, out int n) || n <= 0) return extracted;
        if (n >= extracted.Length) return extracted;
        return fromRight ? extracted.Substring(extracted.Length - n) : extracted.Substring(0, n);
    }

    public static string OffsetToAZ(int offset)
    {
        int n = offset + 1;
        string s = "";
        while (n > 0)
        {
            n--;
            s = (char)('A' + (n % 26)) + s;
            n /= 26;
        }
        return s;
    }
}
