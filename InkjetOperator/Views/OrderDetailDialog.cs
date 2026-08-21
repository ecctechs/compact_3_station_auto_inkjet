using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// Window that presents <see cref="OrderDetailUserControl"/>.
/// <para>
/// It replaces the bare <see cref="Form"/> the Order List used to build at runtime,
/// which carried the system chrome and so was the one popup left that did not look
/// like the rest of the application. Same shape as the shell and
/// <see cref="InputDialog"/>: an <c>AntdUI.BorderlessForm</c> with a rounded,
/// shadowed frame and the shared <see cref="AppTitleBarUserControl"/> in place of a
/// system title bar.
/// </para>
/// <para>
/// It opens wide enough to show a detail section across its full width, but stays a
/// centred popup rather than filling the screen; the body scrolls for the rest.
/// </para>
/// </summary>
internal sealed partial class OrderDetailDialog : AntdUI.BorderlessForm
{
    /// <summary>Fraction of the working area the popup may occupy.</summary>
    private const double MaxWidthRatio = 0.92;
    private const double MaxHeightRatio = 0.86;

    public OrderDetailDialog()
    {
        InitializeComponent();

        // A borderless form has no system chrome, so the header drives the window.
        titleBar.MaximizeRequested += (_, _) => ToggleMaximize();
        titleBar.CloseRequested += (_, _) => Close();
        titleBar.DragRequested += (_, _) => DraggableMouseDown();

        // Same contract as before: the page can ask to be dismissed.
        detailPage.CloseRequested += (_, _) => Close();

        Resize += (_, _) => titleBar.SetMaximized(WindowState == FormWindowState.Maximized);
    }

    /// <summary>Heading shown in the title bar.</summary>
    public string TitleText
    {
        get => titleBar.TitleText;
        set => titleBar.TitleText = value;
    }

    /// <summary>
    /// Fills the page, then hands it to the caller.
    /// <para>
    /// The fill runs with the whole control tree's layout suspended. The page holds
    /// roughly 270 controls across 24 nested <see cref="TableLayoutPanel"/>s, and
    /// without this every single assignment reflows that tree - which is what made
    /// opening the popup feel slow.
    /// </para>
    /// </summary>
    public void SetTransferMode() => detailPage.SetTransferMode();

    public void LoadDetail(ResolvedJobResponse resolved, ApiClient? api = null)
    {
        SuspendTree(detailPage);
        try
        {
            detailPage.LoadDetail(resolved, api);
        }
        finally
        {
            ResumeTree(detailPage);
            detailPage.PerformLayout();
        }
    }

    /// <summary>
    /// Sizes the popup to the content width and centres it, clamped so it still fits
    /// on a smaller shop-floor panel. Deliberately not maximised.
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var area = Screen.FromControl(this).WorkingArea;
        var width = Math.Min(Width, (int)(area.Width * MaxWidthRatio));
        var height = Math.Min(Height, (int)(area.Height * MaxHeightRatio));

        Bounds = new Rectangle(
            area.X + (area.Width - width) / 2,
            area.Y + (area.Height - height) / 2,
            width,
            height);
    }

    /// <summary>
    /// Toggles through AntdUI's own <c>MaxRestore</c> rather than assigning
    /// <see cref="Form.WindowState"/> by hand.
    /// <para>
    /// <c>BorderlessForm</c> tracks a private <c>isMax</c> flag alongside the window
    /// state and guards <c>Max()</c> with it. Restoring by setting WindowState
    /// directly left that flag set, so the next <c>Max()</c> returned immediately and
    /// the window could not be maximised a second time. <c>MaxRestore</c> keeps the
    /// flag in step and refreshes the frame.
    /// </para>
    /// </summary>
    private void ToggleMaximize() => titleBar.SetMaximized(MaxRestore());

    private static void SuspendTree(Control control)
    {
        control.SuspendLayout();
        foreach (Control child in control.Controls)
        {
            SuspendTree(child);
        }
    }

    private static void ResumeTree(Control control)
    {
        foreach (Control child in control.Controls)
        {
            ResumeTree(child);
        }

        // false: hold the reflow back until the whole tree is resumed.
        control.ResumeLayout(false);
    }
}
