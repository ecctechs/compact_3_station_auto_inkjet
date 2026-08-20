namespace InkjetOperator.Views;

/// <summary>
/// Title bar for the application shell.
/// <para>
/// <c>AntdUI.Window</c> removes the whole non-client area - system title bar,
/// border and the minimise / maximise / close buttons with it - so a window based
/// on it has to supply its own. This control is that replacement, following the
/// same split NastoKeyence uses: the bar raises intent as events and the window
/// decides what to do with them.
/// </para>
/// </summary>
public partial class AppTitleBarUserControl : UserControl
{
    public AppTitleBarUserControl()
    {
        InitializeComponent();

        btnMinimize.Click += (_, _) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
        btnMaximize.Click += (_, _) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        // Dragging the bar has to move the window, the way the system title bar did.
        foreach (Control control in new Control[] { this, tlpTitleBarRoot, lblAppTitle })
        {
            control.MouseDown += TitleBar_MouseDown;
        }

        // Double-clicking the bar toggles maximise, matching the system behaviour.
        foreach (Control control in new Control[] { this, tlpTitleBarRoot, lblAppTitle })
        {
            control.DoubleClick += (_, _) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? MinimizeRequested;

    public event EventHandler? MaximizeRequested;

    public event EventHandler? CloseRequested;

    public event EventHandler? DragRequested;

    /// <summary>Text shown on the left of the bar.</summary>
    public string TitleText
    {
        get => lblAppTitle.Text;
        set => lblAppTitle.Text = value;
    }

    /// <summary>
    /// Hides the minimise button. A popup has nothing sensible to minimise to,
    /// which is how the previous Order Detail window behaved as well.
    /// </summary>
    public bool ShowMinimizeButton
    {
        get => btnMinimize.Visible;
        set
        {
            btnMinimize.Visible = value;
            tlpTitleBarRoot.ColumnStyles[1].Width = value ? 46F : 0F;
        }
    }

    /// <summary>Swaps the maximise glyph to match the window state.</summary>
    public void SetMaximized(bool maximized) => btnMaximize.Text = maximized ? "❐" : "□";

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            DragRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
