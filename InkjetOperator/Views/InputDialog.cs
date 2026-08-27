namespace InkjetOperator.Views;

/// <summary>
/// Single-field prompt ("Edit Name", "Unlock"). Presentation follows the modal
/// pattern of the NastoKeyence reference project - an <c>AntdUI.BorderlessForm</c>
/// with a rounded, shadowed frame and its own navy header standing in for the
/// system title bar.
/// <para>
/// The contract callers rely on is unchanged: construct with title/prompt/default,
/// call <see cref="Form.ShowDialog(IWin32Window)"/>, check for
/// <see cref="DialogResult.OK"/>, then read <see cref="Value"/>. Enter accepts and
/// Escape cancels, because <c>AntdUI.Button</c> implements
/// <see cref="IButtonControl"/> and so works as the form's accept/cancel button.
/// </para>
/// </summary>
internal sealed partial class InputDialog : AntdUI.BorderlessForm
{
    /// <summary>
    /// Design-time constructor. The WinForms designer instantiates the form to
    /// render the surface and can only do that through a parameterless constructor.
    /// </summary>
    public InputDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        // A borderless form has no system title bar, so the header has to move the
        // window itself.
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

    /// <summary>The text the operator entered, trimmed.</summary>
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
