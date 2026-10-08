using System.ComponentModel;

namespace InkjetOperator.Models;

/// <summary>
/// The nine barcode-transform operations. The [Description] is the on-screen
/// label shown in the Rule dropdown; the enum value is what gets persisted.
/// </summary>
public enum TransformRuleType
{
    [Description("Delete")]
    DELETE,

    [Description("FIX_TEXT")]
    FIX_TEXT,

    [Description("COPY")]
    COPY,

    [Description("Keep + Pad Left")]
    PAD_LEFT,

    [Description("Keep + Pad Right")]
    PAD_RIGHT,

    [Description("Swap a-z")]
    AZ_LOWER,

    [Description("Swap A-Z")]
    AZ_UPPER,

    [Description("TAKE_RIGHT")]
    TAKE_RIGHT,

    [Description("TAKE_LEFT")]
    TAKE_LEFT,
}
