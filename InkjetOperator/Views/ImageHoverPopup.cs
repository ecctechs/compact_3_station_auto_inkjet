using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

internal sealed class ImageHoverPopup : Form
{
    private const int ThumbHeight = 280;
    private const int ThumbMaxWidth = 380;
    private const int MaxTotalWidth = 1300;

    private readonly FlowLayoutPanel _flow;

    public ImageHoverPopup()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = DesignTokens.DarkNavy; // navy frame
        Padding = new Padding(2);

        _flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
            BackColor = Color.White,
            Padding = new Padding(6),
        };
        Controls.Add(_flow);
    }

    protected override bool ShowWithoutActivation => true;

    public void ShowImages(List<string> paths, Point screenLocation)
    {
        ClearImages();

        int totalWidth = _flow.Padding.Horizontal;
        foreach (var path in paths)
        {
            var img = MarkingRefImageService.LoadImageNoLock(path);
            if (img == null) continue;

            int w = (int)(img.Width * (ThumbHeight / (double)img.Height));
            w = Math.Clamp(w, 60, ThumbMaxWidth);

            var pb = new PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = w,
                Height = ThumbHeight,
                Margin = new Padding(4),
                BackColor = Color.White,
            };
            _flow.Controls.Add(pb);
            totalWidth += w + pb.Margin.Horizontal;
        }

        if (_flow.Controls.Count == 0) return;

        Width = Math.Min(totalWidth + Padding.Horizontal, MaxTotalWidth);
        Height = ThumbHeight + _flow.Padding.Vertical + Padding.Vertical + 8;

        var area = Screen.FromPoint(screenLocation).WorkingArea;
        int x = Math.Min(screenLocation.X, area.Right - Width);
        int y = Math.Min(screenLocation.Y, area.Bottom - Height);
        Location = new Point(Math.Max(area.Left, x), Math.Max(area.Top, y));

        if (!Visible) Show();
        else BringToFront();
    }

    public void HidePopup()
    {
        if (Visible) Hide();
        ClearImages();
    }

    private void ClearImages()
    {
        foreach (Control c in _flow.Controls)
        {
            if (c is PictureBox pb) pb.Image?.Dispose();
            c.Dispose();
        }
        _flow.Controls.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) ClearImages();
        base.Dispose(disposing);
    }
}
