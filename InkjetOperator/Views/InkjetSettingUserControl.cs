using System.Net.Sockets;
using Microsoft.Data.Sqlite;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class InkjetSettingUserControl : UserControl
{
    private const int MkPrinterPort = 9004;
    private const string REL_CPI = @"database\sys\CPI.db3";
    private const string REL_DEFAULT_UVDX = @"document\default.uvdx";

    public InkjetSettingUserControl()
    {
        InitializeComponent();
        LoadAllSettings();

        txtMk058Ip.TextChanged += (_, _) => MarkDirty(txtMk058Ip);
        txtMk059Ip.TextChanged += (_, _) => MarkDirty(txtMk059Ip);
        txtUv1Ip.TextChanged += (_, _) => MarkDirty(txtUv1Ip);
        txtUv1Port.TextChanged += (_, _) => MarkDirty(txtUv1Port);
        txtUv2Ip.TextChanged += (_, _) => MarkDirty(txtUv2Ip);
        txtUv2Port.TextChanged += (_, _) => MarkDirty(txtUv2Port);

        btnMk058Name.Click += (_, _) => EditMkName("MK058_NAME", lblMk058Badge);
        btnMk059Name.Click += (_, _) => EditMkName("MK059_NAME", lblMk059Badge);
        btnUv1Edit.Click += (_, _) => EditUvName("UV1_NAME", lblUv1Badge);
        btnUv2Edit.Click += (_, _) => EditUvName("UV2_NAME", lblUv2Badge);

        btnUv1Browse.Click += (_, _) => BrowseFolder(txtUv1Folder, lblUv1Status, "MK063");
        btnUv2Browse.Click += (_, _) => BrowseFolder(txtUv2Folder, lblUv2Status, "MK067");

        btnMarkingRefBrowse.Click += (_, _) => BrowseMarkingRefFolder();
        txtMarkingRefFolder.TextChanged += (_, _) => MarkDirty(txtMarkingRefFolder);

        btnCheckStatus.Click += async (_, _) => await CheckAllStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadAllSettings();

        Load += async (_, _) => await CheckAllStatusAsync();
    }

    private void LoadAllSettings()
    {
        txtMk058Ip.Text = CustomSettingsManager.Read("MK058_COM");
        txtMk059Ip.Text = CustomSettingsManager.Read("MK059_COM");
        lblMk058Badge.Text = CustomSettingsManager.Read("MK058_NAME", "MK-058");
        lblMk059Badge.Text = CustomSettingsManager.Read("MK059_NAME", "MK-059");

        txtUv1Ip.Text = CustomSettingsManager.Read("UV001_IP");
        txtUv1Port.Text = CustomSettingsManager.Read("UV001_PORT");
        txtUv2Ip.Text = CustomSettingsManager.Read("UV002_IP");
        txtUv2Port.Text = CustomSettingsManager.Read("UV002_PORT");
        txtUv1Folder.Text = UvSettingsManager.Read("UV1_FOLDER");
        txtUv2Folder.Text = UvSettingsManager.Read("UV2_FOLDER");
        lblUv1Badge.Text = UvSettingsManager.Read("UV1_NAME", "UV-001");
        lblUv2Badge.Text = UvSettingsManager.Read("UV2_NAME", "UV-002");

        txtMarkingRefFolder.Text = CustomSettingsManager.Read("MARKING_REF_FOLDER", "");

        ShowFolderStatus(txtUv1Folder.Text, lblUv1Status, "MK063");
        ShowFolderStatus(txtUv2Folder.Text, lblUv2Status, "MK067");
        ResetColors();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var ip058 = txtMk058Ip.Text.Trim();
        var ip059 = txtMk059Ip.Text.Trim();
        if (!string.IsNullOrEmpty(ip058) && !string.IsNullOrEmpty(ip059) && ip058 == ip059)
        {
            Notify.WarnModal(this, "แจ้งเตือน", "MK058 and MK059 IP addresses must not be the same.");
            return;
        }

        var f1 = txtUv1Folder.Text.Trim();
        var f2 = txtUv2Folder.Text.Trim();
        if (!string.IsNullOrEmpty(f1) && !Directory.Exists(f1))
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"UV1 folder not found:\n{f1}");
            return;
        }
        if (!string.IsNullOrEmpty(f2) && !Directory.Exists(f2))
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"UV2 folder not found:\n{f2}");
            return;
        }

        CustomSettingsManager.Write("MK058_COM", ip058);
        CustomSettingsManager.Write("MK059_COM", ip059);
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

        CustomSettingsManager.Write("MARKING_REF_FOLDER", txtMarkingRefFolder.Text.Trim());

        ResetColors();
        Notify.Success(this, "บันทึกเรียบร้อย");
    }

    // ── Check Status ────────────────────────────────────────────────

    public async Task CheckAllStatusAsync()
    {
        btnCheckStatus.Loading = true;
        btnCheckStatus.Enabled = false;
        try
        {
            await Task.WhenAll(
                CheckMkPrinterAsync(txtMk058Ip.Text.Trim(), lblMk058Status),
                CheckMkPrinterAsync(txtMk059Ip.Text.Trim(), lblMk059Status),
                CheckUvPrinterAsync(txtUv1Ip.Text.Trim(), txtUv1Port.Text.Trim(),
                    txtUv1Folder.Text.Trim(), "MK063", lblUv1Dot, lblUv1Status),
                CheckUvPrinterAsync(txtUv2Ip.Text.Trim(), txtUv2Port.Text.Trim(),
                    txtUv2Folder.Text.Trim(), "MK067", lblUv2Dot, lblUv2Status));
        }
        finally
        {
            btnCheckStatus.Loading = false;
            btnCheckStatus.Enabled = true;
        }
    }

    private static async Task CheckMkPrinterAsync(string ip, Label dot)
    {
        if (string.IsNullOrWhiteSpace(ip)) { SetDotColor(dot, Color.Gray); return; }
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(ip, MkPrinterPort).WaitAsync(TimeSpan.FromSeconds(3));
            SetDotColor(dot, DesignTokens.Success);
        }
        catch { SetDotColor(dot, DesignTokens.Danger); }
    }

    private static async Task CheckUvPrinterAsync(
        string ip, string portText, string folder, string table,
        Label dot, Label statusLabel)
    {
        var folderOk = ValidateFolder(folder, table).Count == 0
                       && !string.IsNullOrWhiteSpace(folder);

        var ipOk = false;
        if (!string.IsNullOrWhiteSpace(ip) && int.TryParse(portText, out var port) && port > 0)
        {
            try
            {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromSeconds(3));
                ipOk = true;
            }
            catch { /* ipOk stays false */ }
        }

        void Apply()
        {
            if (folderOk && ipOk)
            {
                dot.ForeColor = DesignTokens.Success;
                statusLabel.Text = "✓ เชื่อมต่อสำเร็จ + ไฟล์พร้อมใช้งาน";
                statusLabel.ForeColor = DesignTokens.SuccessText;
            }
            else
            {
                dot.ForeColor = DesignTokens.Danger;
                var parts = new List<string>();
                if (!ipOk) parts.Add("เชื่อมต่อ IP:Port ไม่ได้");
                if (!folderOk && !string.IsNullOrWhiteSpace(folder))
                    parts.Add("ไฟล์ไม่ครบ");
                else if (string.IsNullOrWhiteSpace(folder))
                    parts.Add("ยังไม่ได้เลือกโฟลเดอร์");
                statusLabel.Text = "⚠ " + string.Join(" / ", parts);
                statusLabel.ForeColor = DesignTokens.Danger;
            }
        }

        if (dot.InvokeRequired) dot.Invoke(Apply); else Apply();
    }

    // ── Folder browsing & validation ────────────────────────────────

    private void BrowseFolder(AntdUI.Input target, Label statusLabel, string requiredTable)
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };
        if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
            dlg.SelectedPath = target.Text;
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        target.Text = dlg.SelectedPath;
        MarkDirty(target);

        var problems = ValidateFolder(dlg.SelectedPath, requiredTable);
        ShowFolderStatus(dlg.SelectedPath, statusLabel, requiredTable);
        if (problems.Count > 0)
            Notify.WarnModal(this, "แจ้งเตือน", string.Join("\n\n", problems));
    }

    private static List<string> ValidateFolder(string folder, string requiredTable)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(folder)) return problems;

        var cpiPath = Path.Combine(folder, REL_CPI);
        if (!File.Exists(cpiPath))
            problems.Add($"ไม่พบ {REL_CPI}");
        else if (!HasTable(cpiPath, requiredTable))
            problems.Add($"พบ CPI.db3 แต่ไม่มีตาราง {requiredTable}");

        if (!File.Exists(Path.Combine(folder, REL_DEFAULT_UVDX)))
            problems.Add($"ไม่พบ {REL_DEFAULT_UVDX}");

        return problems;
    }

    private static void ShowFolderStatus(string folder, Label lbl, string table)
    {
        if (string.IsNullOrWhiteSpace(folder)) { lbl.Text = ""; lbl.ForeColor = Color.Gray; return; }
        var problems = ValidateFolder(folder, table);
        if (problems.Count == 0)
        {
            lbl.Text = "✓ CPI.db3 + default.uvdx พร้อมใช้งาน";
            lbl.ForeColor = DesignTokens.SuccessText;
        }
        else
        {
            lbl.Text = $"⚠ พบ {problems.Count} ปัญหา";
            lbl.ForeColor = DesignTokens.Danger;
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

    // ── Edit name helpers ───────────────────────────────────────────

    private void EditMkName(string key, Label badge)
    {
        var current = CustomSettingsManager.Read(key, key.Replace("_NAME", ""));
        using var dlg = new InputDialog("Rename", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        CustomSettingsManager.Write(key, dlg.Value);
        badge.Text = dlg.Value;
    }

    private void EditUvName(string key, Label badge)
    {
        var current = UvSettingsManager.Read(key, badge.Text);
        using var dlg = new InputDialog("Rename", "Display name:", current);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        UvSettingsManager.Write(key, dlg.Value);
        badge.Text = dlg.Value;
    }

    // ── Dirty / reset ───────────────────────────────────────────────

    private static void SetDotColor(Label dot, Color color)
    {
        if (dot.InvokeRequired) dot.Invoke(() => dot.ForeColor = color);
        else dot.ForeColor = color;
    }

    private void BrowseMarkingRefFolder()
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };
        if (!string.IsNullOrEmpty(txtMarkingRefFolder.Text) && Directory.Exists(txtMarkingRefFolder.Text))
            dlg.SelectedPath = txtMarkingRefFolder.Text;
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtMarkingRefFolder.Text = dlg.SelectedPath;
        MarkDirty(txtMarkingRefFolder);
    }

    private void MarkDirty(AntdUI.Input input) => input.BackColor = Color.LightYellow;

    private void ResetColors()
    {
        txtMk058Ip.BackColor = Color.White;
        txtMk059Ip.BackColor = Color.White;
        txtUv1Ip.BackColor = Color.White;
        txtUv1Port.BackColor = Color.White;
        txtUv2Ip.BackColor = Color.White;
        txtUv2Port.BackColor = Color.White;
        txtUv1Folder.BackColor = Color.White;
        txtUv2Folder.BackColor = Color.White;
        txtMarkingRefFolder.BackColor = Color.White;
    }
}
