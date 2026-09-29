using System.ComponentModel;

namespace InkjetOperator.Models;

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
