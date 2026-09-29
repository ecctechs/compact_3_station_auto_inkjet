using InkjetOperator.Services;

namespace InkjetOperator.Views;

public sealed record MarkingRefOption(string Key, string Display, List<string> ImagePaths);

internal sealed partial class MarkingRefPickerDialog : Form
{
    private const int ThumbHeight = 300;
    private const int ThumbMaxWidth = 420;

    private List<MarkingRefOption> _options = new();

    public MarkingRefPickerDialog()
    {
        InitializeComponent();
        Services.LanguageService.Apply(this);

        lstOptions.SelectedIndexChanged += (_, _) => ShowImagesForSelection();

        pnlImages.SizeChanged += (_, _) => CenterImages();
        lstOptions.DoubleClick += (_, _) => AcceptIfSelected();
        btnOk.Click += (_, _) => AcceptIfSelected();
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        AcceptButton = null;
        CancelButton = null;
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            else if (e.KeyCode == Keys.Enter) AcceptIfSelected();
        };

        Shown += (_, _) => CenterImages();

        FormClosed += (_, _) => ClearImages();
    }

    public string? SelectedKey { get; private set; }

    public static string? Pick(
        IWin32Window? owner, string title, string prompt, List<MarkingRefOption> options)
    {
        if (options.Count == 0) return null;

        using var dlg = new MarkingRefPickerDialog();
        dlg.Text = title;
        dlg.lblPrompt.Text = prompt;
        dlg.SetOptions(options);

        var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
        return result == DialogResult.OK ? dlg.SelectedKey : null;
    }

    public static void View(
        IWin32Window? owner, string title, string prompt, List<string> images)
    {
        using var dlg = new MarkingRefPickerDialog();
        dlg.Text = title;
        dlg.lblPrompt.Text = prompt;
        dlg.SetViewOnly();
        dlg.SetOptions([new MarkingRefOption("", "", images)]);

        if (owner == null) dlg.ShowDialog();
        else dlg.ShowDialog(owner);
    }

    private void SetViewOnly()
    {
        lstOptions.Visible = false;
        tlpContent.ColumnStyles[0].Width = 0F;
        btnCancel.Visible = false;

        Theme.ButtonStyles.Close(btnOk, fontSize: 15f);
    }

    private void SetOptions(List<MarkingRefOption> options)
    {
        _options = options;

        lstOptions.BeginUpdate();
        lstOptions.Items.Clear();
        foreach (var option in options) lstOptions.Items.Add(option.Display);
        lstOptions.EndUpdate();

        lstOptions.SelectedIndex = 0;
    }

    private void AcceptIfSelected()
    {
        int index = lstOptions.SelectedIndex;
        if (index < 0 || index >= _options.Count) return;

        SelectedKey = _options[index].Key;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowImagesForSelection()
    {
        ClearImages();

        int index = lstOptions.SelectedIndex;
        if (index < 0 || index >= _options.Count) return;

        var paths = _options[index].ImagePaths;
        if (paths.Count == 0)
        {
            lblEmpty.Text = MarkingRefImageService.DescribeEmpty(MarkingRefImageService.CheckFolder());
            lblEmpty.Visible = true;
            flpImages.Visible = false;
            return;
        }

        lblEmpty.Visible = false;
        flpImages.Visible = true;

        foreach (var path in paths)
        {
            var image = MarkingRefImageService.LoadImageNoLock(path);
            if (image == null) continue;

            int width = image.Height == 0
                ? ThumbMaxWidth
                : (int)(image.Width * (ThumbHeight / (double)image.Height));
            width = Math.Clamp(width, 80, ThumbMaxWidth);

            var box = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = width,
                Height = ThumbHeight,
                Margin = new Padding(4),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            flpImages.Controls.Add(box);
        }

        if (flpImages.Controls.Count == 0)
        {
            lblEmpty.Text = "เปิดไฟล์รูปไม่ได้";
            lblEmpty.Visible = true;
            flpImages.Visible = false;
            return;
        }

        flpImages.PerformLayout();
        pnlImages.AutoScrollPosition = Point.Empty;
        CenterImages();
    }

    private bool _centering;

    private void CenterImages()
    {
        if (_centering || !flpImages.Visible) return;

        _centering = true;
        try
        {
            var frame = pnlImages.ClientSize;
            var content = flpImages.PreferredSize;

            flpImages.Location = new Point(
                Math.Max(0, (frame.Width - content.Width) / 2),
                Math.Max(0, (frame.Height - content.Height) / 2));
        }
        finally
        {
            _centering = false;
        }
    }

    private void ClearImages()
    {
        var current = flpImages.Controls.Cast<Control>().ToArray();
        flpImages.Controls.Clear();

        foreach (var control in current)
        {
            if (control is PictureBox box) box.Image?.Dispose();
            control.Dispose();
        }
    }
}
