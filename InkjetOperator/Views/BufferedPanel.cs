namespace InkjetOperator.Views;

internal sealed class BufferedPanel : Panel
{
    private const int WsExComposited = 0x02000000;

    public BufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WsExComposited;
            return cp;
        }
    }
}
