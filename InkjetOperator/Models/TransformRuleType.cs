using System.ComponentModel;

namespace InkjetOperator.Models
{
    public enum TransformRuleType
    {
        [Description("Delete")] // เพิ่มบรรทัดนี้
        DELETE,
        FIX_TEXT,
        COPY,
        [Description("Keep + Pad Left")] // เพิ่มบรรทัดนี้
        PAD_LEFT,
        [Description("Keep + Pad Right")] // เพิ่มบรรทัดนี้
        PAD_RIGHT,
        [Description("Swap a-z")] // เพิ่มบรรทัดนี้
        AZ_LOWER,
        [Description("Swap A-Z")] // เพิ่มบรรทัดนี้
        AZ_UPPER,
        TAKE_RIGHT,
        TAKE_LEFT
    }
}