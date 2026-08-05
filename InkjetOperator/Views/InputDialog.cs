namespace InkjetOperator.Views;

internal sealed class InputDialog : Form
{
    private readonly TextBox _textBox;

    public string Value => _textBox.Text.Trim();

    public InputDialog(string title, string prompt, string defaultValue)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        Size = new Size(360, 170);

        var lbl = new Label
        {
            Text = prompt,
            Location = new Point(12, 16),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F)
        };
        Controls.Add(lbl);

        _textBox = new TextBox
        {
            Text = defaultValue,
            Location = new Point(12, 44),
            Size = new Size(320, 28),
            Font = new Font("Segoe UI", 11F)
        };
        Controls.Add(_textBox);

        var btnOk = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(168, 88),
            Size = new Size(80, 32)
        };
        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(252, 88),
            Size = new Size(80, 32)
        };
        Controls.Add(btnOk);
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }
}
