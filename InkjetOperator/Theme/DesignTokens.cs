using System.Drawing;

namespace InkjetOperator.Theme;

/// <summary>
/// Single source of truth for the visual language of InkjetOperator.
/// <para>
/// Every value here was extracted from what the project already draws - nothing is
/// new. The counts in the comments are how many times that literal appears across
/// the designer files under <c>Views/</c>, so each token is the value the UI already
/// uses in the majority of places rather than a redesign of it.
/// </para>
/// <para>
/// The named colours agree with the palette in <c>.claude/rules/winforms-ui.md</c>;
/// the tokens that rule file does not name are ones the screens use anyway.
/// </para>
/// <para>
/// <b>Scope:</b> colours, typography and control metrics only. Spacing and padding
/// are deliberately absent - the pages do not agree on them (root padding is 16 on
/// six pages, 40 on two, 32 on two) and picking a winner would move layout.
/// </para>
/// <para>
/// <b>Designer note:</b> the WinForms designer serialises visual properties as
/// literals into <c>.Designer.cs</c>. Referencing these tokens from a designer file
/// is therefore not safe - the designer rewrites them back to literals the first
/// time the property grid touches that control. Use them from code-behind.
/// </para>
/// </summary>
public static class DesignTokens
{
    // ขนาดฟอนต์และความสูงของ control ทั้งไฟล์ถูกขยาย 15% พร้อมกับ .Designer.cs
    // ทุกหน้า เพื่อให้อ่านง่ายขึ้นหน้างาน — ตัวเลข "จำนวนการใช้งาน" ในคอมเมนต์
    // ด้านล่างนับไว้ตั้งแต่ก่อนขยาย ใช้ดูสัดส่วนได้ แต่ค่าที่เขียนไว้คือค่าปัจจุบัน

    // ---- Brand ----

    /// <summary>#5B9BD5 - page background, input borders, active tab (83 uses).</summary>
    public static readonly Color PrimaryBlue = Color.FromArgb(0x5B, 0x9B, 0xD5);

    /// <summary>#244765 - strong borders, headers, emphasised text (95 uses).</summary>
    public static readonly Color DarkNavy = Color.FromArgb(0x24, 0x47, 0x65);

    // ---- Surfaces ----

    /// <summary>Card and panel background.</summary>
    public static readonly Color Surface = Color.White;

    /// <summary>#EDF3F9 - read-only fields, alternating rows, log panes (35 uses).</summary>
    public static readonly Color SurfaceMuted = Color.FromArgb(0xED, 0xF3, 0xF9);

    /// <summary>#F5F9FD - the faintest fill, used inside cards (10 uses).</summary>
    public static readonly Color SurfaceSubtle = Color.FromArgb(0xF5, 0xF9, 0xFD);

    /// <summary>#DCE9F5 - tinted container behind imagery.</summary>
    public static readonly Color SurfaceAccent = Color.FromArgb(0xDC, 0xE9, 0xF5);

    // ---- Borders ----

    /// <summary>
    /// #AFC8E0 - the ordinary border (33 uses).
    /// <para>
    /// Two heavier borders exist and stay separate on purpose: use
    /// <see cref="PrimaryBlue"/> for input borders and <see cref="DarkNavy"/> for
    /// the strong outline around cards.
    /// </para>
    /// </summary>
    public static readonly Color Border = Color.FromArgb(0xAF, 0xC8, 0xE0);

    // ---- Text ----

    /// <summary>#111111 - headings and field values (27 uses).</summary>
    public static readonly Color TextPrimary = Color.FromArgb(0x11, 0x11, 0x11);

    /// <summary>#333333 - labels and body copy (77 uses).</summary>
    public static readonly Color TextSecondary = Color.FromArgb(0x33, 0x33, 0x33);

    /// <summary>#787878 - hints and disabled captions.</summary>
    public static readonly Color TextMuted = Color.FromArgb(0x78, 0x78, 0x78);

    /// <summary>Navy text on a light surface - section titles (49 uses).</summary>
    public static readonly Color TextEmphasis = DarkNavy;

    /// <summary>Text drawn on a filled primary or status background.</summary>
    public static readonly Color TextOnPrimary = Color.White;

    // ---- Status ----
    // Green appears in two shades on purpose: the saturated one reads as a filled
    // indicator, the darker one is legible as text on white. Do not merge them.

    /// <summary>#4CAF50 - connected / OK indicator fill.</summary>
    public static readonly Color Success = Color.FromArgb(0x4C, 0xAF, 0x50);

    /// <summary>#15803D - "OK" wording on a light surface.</summary>
    public static readonly Color SuccessText = Color.FromArgb(0x15, 0x80, 0x3D);

    /// <summary>#C8DCC8 - fill of a button whose action already completed.</summary>
    public static readonly Color SuccessSoft = Color.FromArgb(0xC8, 0xDC, 0xC8);

    /// <summary>#D4EDBC - table row highlight for a finished job.</summary>
    public static readonly Color RowSuccess = Color.FromArgb(0xD4, 0xED, 0xBC);

    /// <summary>#DC2626 - errors and disconnected state (14 uses).</summary>
    public static readonly Color Danger = Color.FromArgb(0xDC, 0x26, 0x26);

    /// <summary>#D48806 - pending / needs attention.</summary>
    public static readonly Color Warning = Color.FromArgb(0xD4, 0x88, 0x06);

    /// <summary>#B0B0B0 - inactive tab, disabled control.</summary>
    public static readonly Color Inactive = Color.FromArgb(0xB0, 0xB0, 0xB0);

    // ---- Typography ----

    /// <summary>The face every screen already uses.</summary>
    public const string FontFamily = "Segoe UI";

    /// <summary>Used by the log and preview panes so columns line up.</summary>
    public const string MonospaceFontFamily = "Consolas";

    /// <summary>Only reached if the machine is missing the family above.</summary>
    public const string FallbackFontFamily = "Microsoft Sans Serif";

    /// <summary>Page title - 28pt bold, as on the ScanBarcode reference page.</summary>
    public static Font Heading(float size = 32f) => Create(size, FontStyle.Bold);

    /// <summary>Card / section title.</summary>
    public static Font Subheading(float size = 23f) => Create(size, FontStyle.Bold);

    /// <summary>Group label above a set of fields.</summary>
    public static Font SectionLabel(float size = 16f) => Create(size, FontStyle.Bold);

    /// <summary>Field labels and values on the main workflow pages.</summary>
    public static Font Body(float size = 14f) => Create(size, FontStyle.Regular);

    /// <summary>
    /// Body text on the dense settings pages - the second scale of the type ramp,
    /// not a mistake. Was 10pt before the screens were scaled up by 15%.
    /// </summary>
    public static Font BodySmall(float size = 11.5f) => Create(size, FontStyle.Regular);

    /// <summary>Text inside an input control.</summary>
    public static Font Input(float size = 14f) => Create(size, FontStyle.Regular);

    /// <summary>Primary action buttons.</summary>
    public static Font ButtonFont(float size = 17f) => Create(size, FontStyle.Regular);

    /// <summary>Secondary notes and units.</summary>
    public static Font Caption(float size = 10.5f) => Create(size, FontStyle.Regular);

    /// <summary>Log output and payload previews.</summary>
    public static Font Monospace(float size = 12f) =>
        Create(size, FontStyle.Regular, MonospaceFontFamily);

    /// <summary>
    /// Builds a font, falling back if the requested family is not installed.
    /// <para>
    /// GDI+ substitutes silently when a family is missing - it returns a usable
    /// <see cref="Font"/> whose <see cref="Font.Name"/> is some other face - so the
    /// name has to be checked rather than trusting the constructor to throw.
    /// </para>
    /// <para>The caller owns the returned font, exactly like <c>new Font(...)</c>.</para>
    /// </summary>
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
            // Fall through to the fallback family.
        }

        return new Font(FallbackFontFamily, size, style, GraphicsUnit.Point);
    }

    // ---- Control metrics ----

    /// <summary>Corner radius for inputs, buttons and tables (77 uses).</summary>
    public const int Radius = 8;

    /// <summary>Tighter radius used by the compact settings rows (36 uses).</summary>
    public const int RadiusSmall = 4;

    /// <summary>Radius of a card / container panel.</summary>
    public const int RadiusPanel = 12;

    /// <summary>Height of an input on the settings pages (28 uses).</summary>
    public const int InputHeight = 39;

    /// <summary>Height of an input on the operator-facing pages.</summary>
    public const int InputHeightLarge = 53;

    /// <summary>Standard action button height (16 uses).</summary>
    public const int ButtonHeight = 51;

    /// <summary>Inline / row-level button height (16 uses).</summary>
    public const int ButtonHeightSmall = 39;

    /// <summary>Full-width confirm button, as on the ScanBarcode reference page.</summary>
    public const int ButtonHeightLarge = 71;

    /// <summary>Border thickness of an outlined button (34 of 35 uses).</summary>
    public const float ButtonBorderWidth = 2F;
}
