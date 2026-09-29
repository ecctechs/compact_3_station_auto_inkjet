namespace InkjetOperator.Views;

internal sealed partial class InputDialog : AntdUI.BorderlessForm
{
    public InputDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        tlpTitleBar.MouseDown += TitleBar_MouseDown;
        lblTitle.MouseDown += TitleBar_MouseDown;

        Shown += InputDialog_Shown;
    }

    public InputDialog(string title, string prompt, string defaultValue)
        : this()
    {
        Text = title;
        lblTitle.Text = title;
        lblPrompt.Text = prompt;
        txtValue.Text = defaultValue;
    }

    public string Value => txtValue.Text.Trim();

    private void InputDialog_Shown(object? sender, EventArgs e) => txtValue.Focus();

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            DraggableMouseDown();
        }
    }
}
