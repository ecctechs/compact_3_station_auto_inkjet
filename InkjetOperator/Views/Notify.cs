namespace InkjetOperator.Views;

internal static class Notify
{
    private const int SuccessSeconds = 3;
    private const int InfoSeconds = 4;
    private const int WarnSeconds = 5;
    private const int ErrorSeconds = 6;
    private const int DetailSeconds = 10;

    private const int ResultSeconds = 5;

    private static readonly Font ToastFont = InkjetOperator.Theme.DesignTokens.SectionLabel(19f);

    private static readonly Size ToastPadding = new(20, 14);

    private const int ToastMaxWidth = 1000;

    private static void Toast(
        Control? owner, AntdUI.TType type, string text, int seconds, MessageBoxIcon fallbackIcon)
    {
        if (Resolve(owner) is not { } form)
        {
            Fallback(text, fallbackIcon);
            return;
        }

        AntdUI.Message.open(new AntdUI.Message.Config(form, text, type, ToastFont, seconds)
        {
            Padding = ToastPadding,
            MaxWidth = ToastMaxWidth,
        });
    }

    public static void Success(Control? owner, string text)
    {
        System.Media.SystemSounds.Asterisk.Play();
        Toast(owner, AntdUI.TType.Success, text, SuccessSeconds, MessageBoxIcon.Information);
    }

    public static void Info(Control? owner, string text) =>
        Toast(owner, AntdUI.TType.Info, text, InfoSeconds, MessageBoxIcon.Information);

    public static void Warn(Control? owner, string text) =>
        Toast(owner, AntdUI.TType.Warn, text, WarnSeconds, MessageBoxIcon.Warning);

    public static void Error(Control? owner, string text) =>
        Toast(owner, AntdUI.TType.Error, text, ErrorSeconds, MessageBoxIcon.Error);

    public static void WarnModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(Sized(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Warn,
                OkText = "OK",
                CancelText = null,
            }));
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Warning);
    }

    public static void ErrorModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(Sized(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Error,
                OkText = "OK",
                CancelText = null,
            }));
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Error);
    }

    public static void SuccessModal(Control? owner, string title, string text)
    {
        if (Resolve(owner) is { } form)
            AntdUI.Modal.open(Sized(new AntdUI.Modal.Config(form, title, text)
            {
                Icon = AntdUI.TType.Success,
                OkText = "OK",
                CancelText = null,
            }));
        else
            Fallback($"{title}\n\n{text}", MessageBoxIcon.Information);
    }

    private const int ModalWidth = 560;
    private const int ModalButtonHeight = 54;
    private const int ModalButtonWidth = 120;
    private static readonly Size ModalPadding = new(32, 28);
    private static readonly Font ModalBodyFont = InkjetOperator.Theme.DesignTokens.Body(16.5f);
    private static readonly Font ModalButtonFont = InkjetOperator.Theme.DesignTokens.SectionLabel(16.5f);

    internal static AntdUI.Modal.Config Sized(AntdUI.Modal.Config config)
    {
        config.Width = ModalWidth;
        config.BtnHeight = ModalButtonHeight;
        config.Padding = ModalPadding;
        config.Font = ModalBodyFont;
        config.OkFont = ModalButtonFont;
        config.CancelFont = ModalButtonFont;

        config.OnButtonStyle = (_, button) =>
        {
            button.AutoSizeMode = AntdUI.TAutoSize.None;
            button.Width = (int)Math.Round(ModalButtonWidth * AntdUI.Config.Dpi);
            button.Margin = new Padding((int)Math.Round(6 * AntdUI.Config.Dpi), 0, 0, 0);
        };

        return config;
    }

    public static void SuccessDetail(Control? owner, string title, string text) =>
        Detail(owner, title, text, AntdUI.TType.Success, MessageBoxIcon.Information);

    public static void WarnDetail(Control? owner, string title, string text) =>
        Detail(owner, title, text, AntdUI.TType.Warn, MessageBoxIcon.Warning);

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

        AntdUI.Modal.open(Sized(new AntdUI.Modal.Config(form, title, text)
        {
            Icon = type,
            OkText = "OK",
            CancelText = null,
        }));
    }

    public enum ResultKind { Success, Warn, Error, Info }

    public readonly record struct ResultLine(ResultKind Kind, string Text);

    public static ResultLine Ok(string text) => new(ResultKind.Success, text);
    public static ResultLine Bad(string text) => new(ResultKind.Error, text);
    public static ResultLine Careful(string text) => new(ResultKind.Warn, text);
    public static ResultLine Note(string text) => new(ResultKind.Info, text);

    public static List<ResultLine> MkLines(IEnumerable<Services.MkMachineResult> machines) =>
        machines.Select(m => m switch
        {
            { Ok: true, Suspended: true } => Ok($"{m.Name} — ไม่มีงาน สั่งหยุดพิมพ์แล้ว"),

            { Ok: true, Note: not null } => Careful($"{m.Name} — ส่งสำเร็จ · {m.Note}"),

            { Ok: true } => Ok($"{m.Name} — ส่งสำเร็จ"),
            { Suspended: true } => Note(
                $"{m.Name} — ไม่มีงานอยู่แล้ว แต่สั่งหยุดพิมพ์ไม่ได้ · ไปดูว่าเครื่องหยุดจริงไหม"),
            _ => Bad($"{m.Name} — {m.Error}"),
        }).ToList();

    public static void Result(Control? owner, string title, IEnumerable<ResultLine> lines)
    {
        var list = lines.ToList();
        if (list.Count == 0) return;

        var body = string.Join(Environment.NewLine,
            list.Select(l => $"{Mark(l.Kind)}  {l.Text}"));

        var worst = list.Any(l => l.Kind == ResultKind.Error) ? AntdUI.TType.Error
            : list.Any(l => l.Kind == ResultKind.Warn) ? AntdUI.TType.Warn
            : list.Any(l => l.Kind == ResultKind.Success) ? AntdUI.TType.Success
            : AntdUI.TType.Info;

        if (worst == AntdUI.TType.Success || worst == AntdUI.TType.Info)
        {
            System.Media.SystemSounds.Asterisk.Play();

            var plain = string.Join(Environment.NewLine, list.Select(l => l.Text));
            Toast(owner, AntdUI.TType.Success, $"{title}{Environment.NewLine}{plain}",
                ResultSeconds, MessageBoxIcon.Information);
            return;
        }

        if (Resolve(owner) is not { } form)
        {
            Fallback($"{title}\n\n{body}", worst == AntdUI.TType.Error
                ? MessageBoxIcon.Error
                : worst == AntdUI.TType.Warn ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            return;
        }

        AntdUI.Modal.open(Sized(new AntdUI.Modal.Config(form, title, body)
        {
            Icon = worst,
            OkText = "OK",
            CancelText = null,
        }));
    }

    private static string Mark(ResultKind kind) => kind switch
    {
        ResultKind.Success => "✔",
        ResultKind.Warn => "⚠",
        ResultKind.Error => "✖",
        _ => "•",
    };

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

    private static void Fallback(string text, MessageBoxIcon icon) =>
        MessageBox.Show(text, "InkjetOperator", MessageBoxButtons.OK, icon);
}
