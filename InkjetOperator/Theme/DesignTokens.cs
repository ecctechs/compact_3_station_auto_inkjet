using System.Drawing;

namespace InkjetOperator.Theme;

public static class DesignTokens
{

    public static readonly Color PrimaryBlue = Color.FromArgb(0x5B, 0x9B, 0xD5);

    public static readonly Color DarkNavy = Color.FromArgb(0x24, 0x47, 0x65);

    public static readonly Color Surface = Color.White;

    public static readonly Color SurfaceMuted = Color.FromArgb(0xED, 0xF3, 0xF9);

    public static readonly Color SurfaceSubtle = Color.FromArgb(0xF5, 0xF9, 0xFD);

    public static readonly Color SurfaceAccent = Color.FromArgb(0xDC, 0xE9, 0xF5);

    public static readonly Color Border = Color.FromArgb(0xAF, 0xC8, 0xE0);

    public static readonly Color TextPrimary = Color.FromArgb(0x11, 0x11, 0x11);

    public static readonly Color TextSecondary = Color.FromArgb(0x33, 0x33, 0x33);

    public static readonly Color TextMuted = Color.FromArgb(0x78, 0x78, 0x78);

    public static readonly Color TextEmphasis = DarkNavy;

    public static readonly Color TextOnPrimary = Color.White;

    public static readonly Color Success = Color.FromArgb(0x4C, 0xAF, 0x50);

    public static readonly Color SuccessText = Color.FromArgb(0x15, 0x80, 0x3D);

    public static readonly Color SuccessSoft = Color.FromArgb(0xC8, 0xDC, 0xC8);

    public static readonly Color RowSuccess = Color.FromArgb(0xD4, 0xED, 0xBC);

    public static readonly Color Danger = Color.FromArgb(0xDC, 0x26, 0x26);

    public static readonly Color Warning = Color.FromArgb(0xD4, 0x88, 0x06);

    public static readonly Color Inactive = Color.FromArgb(0xB0, 0xB0, 0xB0);

    public const string FontFamily = "Segoe UI";

    public const string MonospaceFontFamily = "Consolas";

    public const string FallbackFontFamily = "Microsoft Sans Serif";

    public static Font Heading(float size = 35f) => Create(size, FontStyle.Bold);

    public static Font Subheading(float size = 25f) => Create(size, FontStyle.Bold);

    public static Font SectionLabel(float size = 17.5f) => Create(size, FontStyle.Bold);

    public static Font Body(float size = 15f) => Create(size, FontStyle.Regular);

    public static Font BodySmall(float size = 12.5f) => Create(size, FontStyle.Regular);

    public static Font Input(float size = 15f) => Create(size, FontStyle.Regular);

    public static Font ButtonFont(float size = 19f) => Create(size, FontStyle.Regular);

    public static Font Caption(float size = 11f) => Create(size, FontStyle.Regular);

    public static Font Monospace(float size = 13f) =>
        Create(size, FontStyle.Regular, MonospaceFontFamily);

    public static Font Create(float size, FontStyle style, string family = FontFamily)
    {
        try
        {
            var font = new Font(family, size, style, GraphicsUnit.Point);
            if (font.Name.Equals(family, StringComparison.OrdinalIgnoreCase))
                return font;

            font.Dispose();
        }
        catch (ArgumentException)
        {
        }

        return new Font(FallbackFontFamily, size, style, GraphicsUnit.Point);
    }

    public const int Radius = 8;

    public const int RadiusSmall = 4;

    public const int RadiusPanel = 12;

    public const int InputHeight = 42;

    public const int InputHeightLarge = 58;

    public const int ButtonHeight = 55;

    public const int ButtonHeightSmall = 42;

    public const int ButtonHeightLarge = 78;

    public const float ButtonBorderWidth = 2F;
}
