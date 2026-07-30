namespace InkjetOperator.Views;

/// <summary>
/// Application main window (shell): top menu bar + content host. The pages are
/// placed in pnlContent (Dock=Fill) at design time; menu buttons switch between
/// them by z-order (BringToFront) and update the active-tab colour. The Input
/// Order tab shows the Scan Barcode page. Only navigation/tab-state code lives
/// here — no other business logic, no runtime control creation, no custom paint.
/// </summary>
public partial class MainShellForm : Form
{
    private static readonly System.Drawing.Color ActiveTab = System.Drawing.Color.FromArgb(91, 155, 213);
    private static readonly System.Drawing.Color InactiveTab = System.Drawing.Color.FromArgb(176, 176, 176);

    public MainShellForm()
    {
        InitializeComponent();
    }

    private void SetActiveTab(AntdUI.Button active)
    {
        foreach (var b in new[] { btnInputOrder, btnOrderList, btnEditPattern, btnSetting })
        {
            var color = b == active ? ActiveTab : InactiveTab;
            b.DefaultBack = color;
        }
    }

    private void btnInputOrder_Click(object sender, EventArgs e)
    {
        scanBarcodePage.BringToFront();
        SetActiveTab(btnInputOrder);
    }

    private void btnOrderList_Click(object sender, EventArgs e)
    {
        orderListPage.BringToFront();
        SetActiveTab(btnOrderList);
    }

    private void btnEditPattern_Click(object sender, EventArgs e)
    {
        editPatternPage.BringToFront();
        SetActiveTab(btnEditPattern);
    }

    private void btnSetting_Click(object sender, EventArgs e)
    {
        settingPage.BringToFront();
        SetActiveTab(btnSetting);
    }
}
