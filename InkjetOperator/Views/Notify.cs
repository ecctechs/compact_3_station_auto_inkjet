namespace InkjetOperator.Views;

/// <summary>
/// In-window feedback for the operator, following the pattern the NastoKeyence
/// project uses: <c>AntdUI.Message</c> for a one-line toast tied to the action the
/// operator just took, and <c>AntdUI.Notification</c> for an outcome that carries
/// its own heading and several lines of detail.
/// <para>
/// Both render inside the application window rather than as desktop-level popups,
/// because <c>Program.ConfigureAntdUi()</c> sets <c>Config.ShowInWindowByMessage</c>
/// and <c>Config.ShowInWindowByNotification</c>.
/// </para>
/// <para>
/// <b>UI layer only.</b> Nothing under <c>Services/</c> or <c>Managers/</c> should
/// call this - those layers report through their return values and let the page
/// that invoked them decide what the operator sees.
/// </para>
/// <para>
/// <b>Why the owner is optional:</b> AntdUI anchors both widgets to a
/// <see cref="Form"/>, and several of the callers are static helpers with no
/// instance to ask. Pass the control when there is one - it resolves to the form
/// that actually hosts it, which stays correct even while a modal dialog is open -
/// and leave it null otherwise.
/// </para>
/// </summary>
internal static class Notify
{
    // How long each severity stays on screen, in seconds. Failures linger longer
    // than confirmations because the operator has to read and act on them.
    private const int SuccessSeconds = 3;
    private const int InfoSeconds = 4;
    private const int WarnSeconds = 5;
    private const int ErrorSeconds = 6;
    private const int DetailSeconds = 10;

    // ---- One-line toast ----

    /// <summary>An action completed - saved, sent, created.</summary>
    public static void Success(Control? owner, string text)
    {
        System.Media.SystemSounds.Asterisk.Play();

        if (Resolve(owner) is { } form)
            AntdUI.Message.success(form, text,
                font: new Font(form.Font.FontFamily, 16f, FontStyle.Bold),
                autoClose: SuccessSeconds);
        else
            Fallback(text, MessageBoxIcon.Information);
    }

    /// <summary>Neutral progress or state information.</summary>
    public static void Info(Control? owner, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Message.info(form, text, autoClose: InfoSeconds);
        else
            Fallback(text, MessageBoxIcon.Information);
    }

    /// <summary>Validation problem or a step that could not run as asked.</summary>
    public static void Warn(Control? owner, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Message.warn(form, text, autoClose: WarnSeconds);
        else
            Fallback(text, MessageBoxIcon.Warning);
    }

    /// <summary>Something failed.</summary>
    public static void Error(Control? owner, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Message.error(form, text, autoClose: ErrorSeconds);
        else
            Fallback(text, MessageBoxIcon.Error);
    }

    // ---- Centered modal dialog (blocks until OK) ----

    public static void WarnModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Warn,
                OkText = "OK",
                CancelText = null,
                Font = new Font(form.Font.FontFamily, 14f),
                OkFont = new Font(form.Font.FontFamily, 14f, FontStyle.Bold),
            });
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Warning);
    }

    public static void ErrorModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Error,
                OkText = "OK",
                CancelText = null,
                Font = new Font(form.Font.FontFamily, 14f),
                OkFont = new Font(form.Font.FontFamily, 14f, FontStyle.Bold),
            });
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Error);
    }

    public static void SuccessModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Success,
                OkText = "OK",
                CancelText = null,
                Font = new Font(form.Font.FontFamily, 14f),
                OkFont = new Font(form.Font.FontFamily, 14f, FontStyle.Bold),
            });
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Information);
    }

    // ---- Titled notification, for multi-line outcomes ----

    // Without an explicit font AntdUI falls back to the owning form's, which is the
    // 9pt application default - far below the 12-15pt the operator pages use, so the
    // notification came out tiny on the shop-floor display. AntdUI derives the whole
    // box from these: the panel grows with the text, and the status icon is sized
    // from the title's line height. Created once and kept, because AntdUI holds the
    // reference for the life of the notification.
    private static readonly Font DetailTitleFont = InkjetOperator.Theme.DesignTokens.SectionLabel(20.5f);
    private static readonly Font DetailBodyFont = InkjetOperator.Theme.DesignTokens.Body(16f);

    /// <summary>Breathing room inside the panel; AntdUI scales it by the display DPI.</summary>
    private static readonly Size DetailPadding = new(32, 26);

    /// <summary>A finished job or transfer, with a per-machine heading.</summary>
    public static void SuccessDetail(Control? owner, string title, string text) =>
        Detail(owner, title, text, AntdUI.TType.Success, MessageBoxIcon.Information);

    /// <summary>A partial or blocked outcome that lists what did and did not run.</summary>
    public static void WarnDetail(Control? owner, string title, string text) =>
        Detail(owner, title, text, AntdUI.TType.Warn, MessageBoxIcon.Warning);

    /// <summary>A failure with troubleshooting steps attached.</summary>
    public static void ErrorDetail(Control? owner, string title, string text) =>
        Detail(owner, title, text, AntdUI.TType.Error, MessageBoxIcon.Error);

    private static void Detail(
        Control? owner, string title, string text, AntdUI.TType type, MessageBoxIcon fallbackIcon)
    {
        if (Resolve(owner) is not { } form)
        {
            Fallback($"{title}\n\n{text}", fallbackIcon);
            return;
        }

        AntdUI.Notification.open(new AntdUI.Notification.Config(
            form, title, text, type, AntdUI.TAlignFrom.TR, DetailBodyFont, DetailSeconds)
        {
            FontTitle = DetailTitleFont,
            Padding = DetailPadding,
            Radius = InkjetOperator.Theme.DesignTokens.Radius,
        });
    }

    // ---- Plumbing ----

    /// <summary>
    /// Finds the form to draw on: the one hosting <paramref name="owner"/> when a
    /// control was supplied, otherwise whichever window is currently in front.
    /// <para>Shared with <see cref="Confirm"/>, which anchors its modal the same way.</para>
    /// </summary>
    internal static Form? Resolve(Control? owner)
    {
        var form = owner?.FindForm();
        if (form is { IsDisposed: false })
            return form;

        if (Form.ActiveForm is { IsDisposed: false } active)
            return active;

        foreach (Form open in Application.OpenForms)
        {
            if (!open.IsDisposed)
                return open;
        }

        return null;
    }

    /// <summary>
    /// Last resort when no window is available - during shutdown, or before the
    /// shell has been shown. Losing a message the operator needed is worse than an
    /// out-of-style dialog, so this stays.
    /// </summary>
    private static void Fallback(string text, MessageBoxIcon icon) =>
        MessageBox.Show(text, "InkjetOperator", MessageBoxButtons.OK, icon);
}
