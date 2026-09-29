namespace InkjetOperator.Views;

internal static class Confirm
{
    private const string ConfirmText = "Yes";
    private const string CancelText = "No";

    public static bool Ask(Control? owner, string title, string content)
    {
        var form = Notify.Resolve(owner);

        var config = form is null
            ? new AntdUI.Modal.Config(title, content, AntdUI.TType.Warn)
            : new AntdUI.Modal.Config(form, title, content, AntdUI.TType.Warn);

        config.OkText = ConfirmText;
        config.CancelText = CancelText;

        return AntdUI.Modal.open(Notify.Sized(config)) == DialogResult.OK;
    }
}
