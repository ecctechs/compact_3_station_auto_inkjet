namespace InkjetOperator;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form
        {
            Text = "Scan Barcode Preview",
            ClientSize = new Size(1100, 920),
            StartPosition = FormStartPosition.CenterScreen,
            Controls = { new InkjetOperator.Views.ScanBarcodeUserControl { Dock = DockStyle.Fill } }
        });
    }
}
