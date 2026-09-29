using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

internal sealed partial class OrderDetailDialog : AntdUI.BorderlessForm
{
    public OrderDetailDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        detailPage.CloseRequested += (_, _) => Close();

        detailPage.RemoteStartRequested += (_, step) => RemoteStartStep = step;

    }

    public string? RemoteStartStep { get; private set; }

    public string TitleText
    {
        get => titleBar.TitleText;
        set => titleBar.TitleText = value;
    }

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

    protected override void WndProc(ref Message m)
    {
        FullScreenMaximize.Handle(this, ref m);
        base.WndProc(ref m);
    }

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

        control.ResumeLayout(false);
    }
}
