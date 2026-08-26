using System.Drawing;

namespace InkjetOperator.Theme;

/// <summary>
/// Standard button appearances, so every screen styles a button the same way.
/// <para>
/// <b>Why this exists:</b> <c>AntdUI.Button</c> draws no border at all unless
/// <c>BorderWidth</c> is set. Setting only <c>DefaultBorderColor</c> leaves the
/// control looking like floating text rather than a button, which is why
/// <see cref="Outline"/> always sets both.
/// </para>
/// <para>
/// <b>Where to call these:</b> from code-behind, not from <c>.Designer.cs</c> - the
/// WinForms designer only round-trips property assignments, so a helper call placed
/// in a designer file would be dropped the next time the designer regenerates it.
/// Buttons that are styled in the designer today keep their designer values; this
/// class is what new code and the Phase 3 migration should use.
/// </para>
/// </summary>
public static class ButtonStyles
{
    /// <summary>Primary action - filled background, white text (Save, Send, Search).</summary>
    public static void Primary(AntdUI.Button button, Color? back = null, float fontSize = 15f)
    {
        ArgumentNullException.ThrowIfNull(button);

        button.Type = AntdUI.TTypeMini.Primary;
        button.Font = DesignTokens.ButtonFont(fontSize);
        button.Radius = DesignTokens.Radius;
        button.DefaultBack = back ?? DesignTokens.PrimaryBlue;
        button.ForeColor = DesignTokens.TextOnPrimary;
        button.BorderWidth = 0F;
    }

    /// <summary>
    /// Secondary action - white fill with a visible border (Browse, Close, Add Row).
    /// </summary>
    /// <param name="border">Border colour; defaults to the ordinary border.</param>
    /// <param name="fore">Text colour; defaults to the border colour when that
    /// border is a deliberate accent, otherwise to the primary text colour.</param>
    public static void Outline(
        AntdUI.Button button, Color? border = null, Color? fore = null, float fontSize = 12f)
    {
        ArgumentNullException.ThrowIfNull(button);

        var borderColor = border ?? DesignTokens.Border;

        button.Type = AntdUI.TTypeMini.Default;
        button.Font = DesignTokens.ButtonFont(fontSize);
        button.Radius = DesignTokens.Radius;
        button.DefaultBack = DesignTokens.Surface;
        button.DefaultBorderColor = borderColor;
        button.BorderWidth = DesignTokens.ButtonBorderWidth;

        // The neutral border is too light to read as text, so fall back to the
        // normal text colour rather than tinting the label the same grey-blue.
        button.ForeColor = fore ?? (borderColor == DesignTokens.Border
            ? DesignTokens.TextPrimary
            : borderColor);
    }

    /// <summary>
    /// Selected state of a tab or menu button.
    /// <para>
    /// This is the toggle that <c>OrderListUserControl.SwitchTab</c> and
    /// <c>SettingUserControl.SelectMenu</c> were each spelling out by hand with the
    /// same two colours.
    /// </para>
    /// </summary>
    public static void SetSelected(AntdUI.Button button, bool selected)
    {
        ArgumentNullException.ThrowIfNull(button);

        button.Type = selected ? AntdUI.TTypeMini.Primary : AntdUI.TTypeMini.Default;
        button.ForeColor = selected ? DesignTokens.TextOnPrimary : DesignTokens.TextSecondary;
    }
}
