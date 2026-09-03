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
/// It opens maximised. The page is laid out 1750x1250 at 100% scaling, which is
/// already wider and taller than a 1920x1080 panel at 125% - anything smaller than
/// the full working area only adds scrolling without showing more.
/// </para>
/// <para>
/// The title bar carries no window buttons at all: minimise and maximise were
/// already off, and the close cross is off too because the page has its own ปิด
/// button. One way out, in the place the operator is already looking.
/// </para>
/// </summary>
internal sealed partial class OrderDetailDialog : AntdUI.BorderlessForm
{
    public OrderDetailDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        // The page can ask to be dismissed - this is the only way out now that the
        // title bar has no close cross.
        detailPage.CloseRequested += (_, _) => Close();

        // Neither MaximizeRequested (double-click on the bar) nor DragRequested is
        // handled. Both restore a maximised BorderlessForm to its Normal size, which
        // for this page is 1750x1250 - larger than the panel, and with no maximise
        // button left to undo it. The bar is a heading here, not a window control.
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
    /// ขยายเต็มจอต้องได้ขนาดจอเต็ม ไม่ใช่แค่พื้นที่ทำงาน
    /// เหตุผลอยู่ที่ <see cref="FullScreenMaximize"/>
    /// </summary>
    protected override void WndProc(ref Message m)
    {
        FullScreenMaximize.Handle(this, ref m);
        base.WndProc(ref m);
    }

    /// <summary>
    /// Fills the working area.
    /// <para>
    /// Maximises through AntdUI's own <c>MaxRestore</c> rather than assigning
    /// <see cref="Form.WindowState"/>. <c>BorderlessForm</c> keeps a private
    /// <c>isMax</c> flag beside the window state and guards its own sizing with it;
    /// setting WindowState by hand leaves that flag out of step and the rounded
    /// frame draws wrong.
    /// </para>
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MaxRestore();
    }

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
