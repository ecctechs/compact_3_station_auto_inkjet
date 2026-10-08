namespace InkjetOperator.Models;

/// <summary>
/// A barcode-transform pattern. <see cref="Name"/> is the placeholder that is
/// searched for inside a printer block text (e.g. "CCCC", "DDDD"); the rules run
/// in order and their results are concatenated into a single string.
///
/// NOTE: this is the local XML-stored transform pattern — not the backend
/// "patterns" table that holds printer settings.
/// </summary>
public class Pattern
{
    /// <summary>The placeholder searched for in a block text (e.g. "CCCC").</summary>
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    /// <summary>Transform rules in order; results are concatenated.</summary>
    public List<Rule> Rules { get; set; } = new();

    /// <summary>Sample barcode for the on-screen test (persisted).</summary>
    public string TestBarcode { get; set; } = "";

    /// <summary>Sample block text for the on-screen test (persisted).</summary>
    public string TestBlockText { get; set; } = "";

    /// <summary>Last-computed preview result (persisted).</summary>
    public string TestPreview { get; set; } = "";

    /// <summary>Run every active rule in order over <paramref name="input"/>. A single
    /// failing rule must not bring down the whole pattern — it contributes "".</summary>
    public string Apply(string input)
    {
        var parts = new List<string>();
        foreach (var r in Rules)
        {
            try { parts.Add(r.Apply(input)); }
            catch { parts.Add(string.Empty); }
        }
        return string.Concat(parts);
    }
}
