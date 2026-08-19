namespace InkjetOperator.Views;

/// <summary>
/// Blocking yes/no question, following the <c>AntdUI.Modal.open</c> pattern the
/// NastoKeyence project uses for every confirmation.
/// <para>
/// <c>Modal.open</c> calls <c>ShowDialog()</c> internally and returns a
/// <see cref="DialogResult"/>, so it blocks and resolves exactly like the
/// <c>MessageBox.Show</c> calls it replaces - the caller keeps its original
/// control flow. It also marshals itself onto the UI thread when needed.
/// </para>
/// <para>
/// Companion to <see cref="Notify"/>: that one is for feedback the operator does
/// not answer, this one is for the question they do.
/// </para>
/// </summary>
internal static class Confirm
{
    // AntdUI takes its default button captions from AntdUI.Localization, which
    // falls back to Chinese ("确定" / "取消") when no provider is registered - and
    // this project registers none. Spelling the captions out keeps the two buttons
    // reading exactly as they did on the MessageBox these calls replaced.
    private const string ConfirmText = "Yes";
    private const string CancelText = "No";

    /// <summary>
    /// Asks the operator to confirm. Returns <see langword="true"/> only when they
    /// accept - dismissing with Escape, the close icon or the mask all return
    /// <see langword="false"/>, matching how "No" behaved before.
    /// </summary>
    /// <param name="owner">
    /// The control raising the question, so the modal anchors to the window hosting
    /// it. Null is fine for a static caller with no instance to hand.
    /// </param>
    public static bool Ask(Control? owner, string title, string content)
    {
        var form = Notify.Resolve(owner);

        // A confirmation is a caution by nature, and AntdUI.TType has no question
        // icon, so Warn is what NastoKeyence uses for all of these.
        var config = form is null
            ? new AntdUI.Modal.Config(title, content, AntdUI.TType.Warn)
            : new AntdUI.Modal.Config(form, title, content, AntdUI.TType.Warn);

        config.OkText = ConfirmText;
        config.CancelText = CancelText;

        return AntdUI.Modal.open(config) == DialogResult.OK;
    }
}
