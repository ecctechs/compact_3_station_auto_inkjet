using System.Drawing;

namespace InkjetOperator.Theme;

public static class ButtonStyles
{
    public static void Primary(AntdUI.Button button, Color? back = null, float fontSize = 19f)
    {
        ArgumentNullException.ThrowIfNull(button);

        button.Type = AntdUI.TTypeMini.Primary;
        button.Font = DesignTokens.ButtonFont(fontSize);
        button.Radius = DesignTokens.Radius;
        button.DefaultBack = back ?? DesignTokens.PrimaryBlue;
        button.ForeColor = DesignTokens.TextOnPrimary;
        button.BorderWidth = 0F;
    }

    public const string CloseIcon = "CloseOutlined";

    public const string CloseText = "ปิด";

    public static void Close(AntdUI.Button button, float fontSize = 13f)
    {
        ArgumentNullException.ThrowIfNull(button);

        button.Text = CloseText;
        button.IconSvg = CloseIcon;
        button.Type = AntdUI.TTypeMini.Error;
        button.Font = DesignTokens.ButtonFont(fontSize);
        button.Radius = DesignTokens.Radius;
        button.ForeColor = DesignTokens.TextOnPrimary;
        button.BorderWidth = 0F;
    }

    public static void Outline(
        AntdUI.Button button, Color? border = null, Color? fore = null, float fontSize = 15f)
    {
        ArgumentNullException.ThrowIfNull(button);

        var borderColor = border ?? DesignTokens.Border;

        button.Type = AntdUI.TTypeMini.Default;
        button.Font = DesignTokens.ButtonFont(fontSize);
        button.Radius = DesignTokens.Radius;
        button.DefaultBack = DesignTokens.Surface;
        button.DefaultBorderColor = borderColor;
        button.BorderWidth = DesignTokens.ButtonBorderWidth;

        button.ForeColor = fore ?? (borderColor == DesignTokens.Border
            ? DesignTokens.TextPrimary
            : borderColor);
    }

    public static void SetSelected(AntdUI.Button button, bool selected)
    {
        ArgumentNullException.ThrowIfNull(button);

        button.Type = selected ? AntdUI.TTypeMini.Primary : AntdUI.TTypeMini.Default;
        button.ForeColor = selected ? DesignTokens.TextOnPrimary : DesignTokens.TextSecondary;
    }
}
