namespace InkjetOperator.Views;

public class ReadOnlyInput : AntdUI.Input
{
    public ReadOnlyInput() => ApplyLock(base.ReadOnly);

    public override bool ReadOnly
    {
        get => base.ReadOnly;
        set
        {
            base.ReadOnly = value;
            ApplyLock(value);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (ReadOnly) return;
        base.OnMouseDown(e);
    }

    private void ApplyLock(bool locked)
    {
        SetStyle(ControlStyles.Selectable, !locked);

        TabStop = !locked;
        CaretVisible = !locked;
    }
}
