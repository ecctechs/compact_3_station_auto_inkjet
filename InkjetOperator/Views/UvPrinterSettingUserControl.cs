using Microsoft.Data.Sqlite;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class UvPrinterSettingUserControl : UserControl
{
    private const string REL_CPI = @"database\sys\CPI.db3";
    private const string REL_DEFAULT_UVDX = @"document\default.uvdx";
    private const string REL_DOCUMENT = "document";

    public UvPrinterSettingUserControl()
    {
        InitializeComponent();
        LoadData();

        txtUv1Ip.TextChanged += (_, _) => MarkDirty(txtUv1Ip);
        txtUv1Port.TextChanged += (_, _) => MarkDirty(txtUv1Port);
        txtUv2Ip.TextChanged += (_, _) => MarkDirty(txtUv2Ip);
        txtUv2Port.TextChanged += (_, _) => MarkDirty(txtUv2Port);

        btnUv1Edit.Click += (_, _) => EditName("UV1_NAME", lblUv1Badge);
        btnUv2Edit.Click += (_, _) => EditName("UV2_NAME", lblUv2Badge);
        btnUv1Browse.Click += (_, _) => BrowseFolder(txtUv1Folder, lblUv1Status, "MK063");
        btnUv2Browse.Click += (_, _) => BrowseFolder(txtUv2Folder, lblUv2Status, "MK067");
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();
    }

    private void LoadData()
    {
        txtUv1Ip.Text = CustomSettingsManager.Read("UV001_IP");
        txtUv1Port.Text = CustomSettingsManager.Read("UV001_PORT");
        txtUv2Ip.Text = CustomSettingsManager.Read("UV002_IP");
        txtUv2Port.Text = CustomSettingsManager.Read("UV002_PORT");

        txtUv1Folder.Text = UvSettingsManager.Read("UV1_FOLDER");
        txtUv2Folder.Text = UvSettingsManager.Read("UV2_FOLDER");

        lblUv1Badge.Text = UvSettingsManager.Read("UV1_NAME", "UV-001");
        lblUv2Badge.Text = UvSettingsManager.Read("UV2_NAME", "UV-002");

        ValidateAndShowStatus(txtUv1Folder.Text, lblUv1Status, "MK063");
        ValidateAndShowStatus(txtUv2Folder.Text, lblUv2Status, "MK067");
        ResetColors();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var f1 = txtUv1Folder.Text.Trim();
        var f2 = txtUv2Folder.Text.Trim();

        if (!string.IsNullOrEmpty(f1) && !Directory.Exists(f1))
        {
            MessageBox.Show($"UV1 folder not found:\n{f1}", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!string.IsNullOrEmpty(f2) && !Directory.Exists(f2))
        {
            MessageBox.Show($"UV2 folder not found:\n{f2}", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CustomSettingsManager.Write("UV001_IP", txtUv1Ip.Text.Trim());
        CustomSettingsManager.Write("UV001_PORT", txtUv1Port.Text.Trim());
        CustomSettingsManager.Write("UV002_IP", txtUv2Ip.Text.Trim());
        CustomSettingsManager.Write("UV002_PORT", txtUv2Port.Text.Trim());

        UvSettingsManager.Write("UV1_FOLDER", f1);
        UvSettingsManager.Write("UV2_FOLDER", f2);

        if (!string.IsNullOrEmpty(f1))
            UvSettingsManager.Write("UV1DB3_PATH", Path.Combine(f1, REL_CPI));
        if (!string.IsNullOrEmpty(f2))
            UvSettingsManager.Write("UV1DB3_PATH_2", Path.Combine(f2, REL_CPI));

        ResetColors();
        MessageBox.Show("บันทึกเรียบร้อย", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BrowseFolder(AntdUI.Input target, System.Windows.Forms.Label statusLabel, string requiredTable)
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };
        if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
            dlg.SelectedPath = target.Text;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        target.Text = dlg.SelectedPath;
        MarkDirty(target);

        var problems = ValidateFolder(dlg.SelectedPath, requiredTable);
        ValidateAndShowStatus(dlg.SelectedPath, statusLabel, requiredTable);

        if (problems.Count > 0)
            MessageBox.Show(string.Join("\n\n", problems), "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static List<string> ValidateFolder(string folder, string requiredTable)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(folder)) return problems;

        var cpiPath = Path.Combine(folder, REL_CPI);
        if (!File.Exists(cpiPath))
        {
            problems.Add($"ไม่พบ {REL_CPI}");
        }
        else if (!HasTable(cpiPath, requiredTable))
        {
            problems.Add($"พบ CPI.db3 แต่ไม่มีตาราง {requiredTable}");
        }

        var uvdxPath = Path.Combine(folder, REL_DEFAULT_UVDX);
        if (!File.Exists(uvdxPath))
            problems.Add($"ไม่พบ {REL_DEFAULT_UVDX}");

        return problems;
    }

    private static void ValidateAndShowStatus(string folder, System.Windows.Forms.Label lbl, string requiredTable)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            lbl.Text = "";
            lbl.ForeColor = Color.Gray;
            return;
        }

        var problems = ValidateFolder(folder, requiredTable);
        if (problems.Count == 0)
        {
            lbl.Text = "✓ CPI.db3 + default.uvdx พร้อมใช้งาน";
            lbl.ForeColor = Color.FromArgb(21, 128, 61);
        }
        else
        {
            lbl.Text = $"⚠ พบ {problems.Count} ปัญหา";
            lbl.ForeColor = Color.FromArgb(220, 38, 38);
        }
    }

    private static bool HasTable(string dbPath, string tableName)
    {
        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@t";
            cmd.Parameters.AddWithValue("@t", tableName);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }
        catch { return false; }
    }

    private void EditName(string key, Label badge)
    {
        var current = UvSettingsManager.Read(key, badge.Text);
        using var dlg = new InputDialog("Edit Name", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        UvSettingsManager.Write(key, dlg.Value);
        badge.Text = dlg.Value;
    }

    private void MarkDirty(AntdUI.Input input) => input.BackColor = Color.LightYellow;

    private void ResetColors()
    {
        txtUv1Ip.BackColor = Color.White;
        txtUv1Port.BackColor = Color.White;
        txtUv2Ip.BackColor = Color.White;
        txtUv2Port.BackColor = Color.White;
        txtUv1Folder.BackColor = Color.White;
        txtUv2Folder.BackColor = Color.White;
    }

    // --- Static helpers for other pages ---

    public static string? GetCpiPath(int uvNumber)
    {
        var folderKey = uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER";
        var folder = UvSettingsManager.Read(folderKey);
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_CPI);
        return File.Exists(path) ? path : null;
    }

    public static string? GetDocumentFolder(int uvNumber)
    {
        var folderKey = uvNumber == 1 ? "UV1_FOLDER" : "UV2_FOLDER";
        var folder = UvSettingsManager.Read(folderKey);
        if (string.IsNullOrWhiteSpace(folder)) return null;
        var path = Path.Combine(folder, REL_DOCUMENT);
        return Directory.Exists(path) ? path : null;
    }
}
