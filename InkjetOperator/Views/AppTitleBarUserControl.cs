namespace InkjetOperator.Views;

public partial class AppTitleBarUserControl : UserControl
{
    public AppTitleBarUserControl()
    {
        InitializeComponent();

        btnMinimize.Click += (_, _) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
        btnMaximize.Click += (_, _) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        foreach (Control control in new Control[] { this, tlpTitleBarRoot, lblAppTitle })
        {
            control.MouseDown += TitleBar_MouseDown;
        }

        foreach (Control control in new Control[] { this, tlpTitleBarRoot, lblAppTitle })
        {
            control.DoubleClick += (_, _) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? MinimizeRequested;

    public event EventHandler? MaximizeRequested;

    public event EventHandler? CloseRequested;

    public event EventHandler? DragRequested;

    public string TitleText
    {
        get => lblAppTitle.Text;
        set => lblAppTitle.Text = value;
    }

    public bool ShowMinimizeButton
    {
        get => btnMinimize.Visible;
        set
        {
            btnMinimize.Visible = value;
            tlpTitleBarRoot.ColumnStyles[1].Width = value ? 46F : 0F;
        }
    }

    public bool ShowCloseButton
    {
        get => btnClose.Visible;
        set
        {
            btnClose.Visible = value;
            tlpTitleBarRoot.ColumnStyles[3].Width = value ? 58F : 0F;
        }
    }

    public void SetMaximized(bool maximized) => btnMaximize.Text = maximized ? "❐" : "□";

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            DragRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
