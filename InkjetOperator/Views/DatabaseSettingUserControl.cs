using Microsoft.Data.Sqlite;
using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

public partial class DatabaseSettingUserControl : UserControl
{
    public DatabaseSettingUserControl()
    {
        InitializeComponent();
        LoadData();

        btnBrowse.Click += (_, _) => BrowseFile();
        btnBrowseClamp.Click += (_, _) => BrowseClampFile();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();
    }

    private void LoadData()
    {
        txtDbPath.Text = CustomSettingsManager.Read("DB_PATH");
        UpdateStatus(txtDbPath.Text);
        txtDbPath.BackColor = Color.White;

        txtClampPath.Text = CustomSettingsManager.Read("CLAMP_DB_PATH");
        UpdateClampStatus(txtClampPath.Text);
        txtClampPath.BackColor = Color.White;
    }

    /// <summary>
    /// Re-reads both database paths and refreshes the status labels.
    /// <para>
    /// The page no longer has a Check Status button; this stays public because
    /// <see cref="SettingUserControl"/> calls it when the Setting page opens.
    /// </para>
    /// </summary>
    public async Task CheckStatusAsync()
    {
        var path = CustomSettingsManager.Read("DB_PATH");
        var clampPath = CustomSettingsManager.Read("CLAMP_DB_PATH");
        await Task.Run(() =>
        {
            UpdateStatus(path);
            UpdateClampStatus(clampPath);
        });
    }

    private void UpdateStatus(string path)
    {
        bool ok = !string.IsNullOrWhiteSpace(path)
                  && File.Exists(path)
                  && HasInkjetDataTable(path);

        void Apply()
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                lblStatus.Text = "";
                lblStatus.ForeColor = Color.Gray;
            }
            else if (ok)
            {
                lblStatus.Text = "✓  พร้อมใช้งาน";
                lblStatus.ForeColor = DesignTokens.SuccessText;
            }
            else
            {
                if (!File.Exists(path))
                    lblStatus.Text = "✗  ไม่พบไฟล์";
                else
                    lblStatus.Text = "✗  ไม่มีตาราง 'inkjet_data'";
                lblStatus.ForeColor = DesignTokens.Danger;
            }
        }

        if (lblStatus.InvokeRequired) lblStatus.Invoke(Apply); else Apply();
    }

    /// <summary>
    /// สถานะของ mydatabase.db3 — ต้องมีตาราง MainTable ถึงจะอ่านระยะแคลมป์ได้
    /// ไม่ตั้งค่าก็ไม่ถือว่าผิด (งานที่ไม่ผ่าน UV ไม่ต้องใช้) จึงขึ้นเป็นข้อความเทาเฉยๆ
    /// </summary>
    private void UpdateClampStatus(string path)
    {
        bool exists = !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        bool ok = exists && HasTable(path, "MainTable");

        void Apply()
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                lblClampStatus.Text = "ยังไม่ได้ตั้งค่า — ระยะแคลมป์ (IAI) จะถูกบันทึกเป็นค่าว่าง";
                lblClampStatus.ForeColor = Color.Gray;
            }
            else if (ok)
            {
                lblClampStatus.Text = "✓  พร้อมใช้งาน";
                lblClampStatus.ForeColor = DesignTokens.SuccessText;
            }
            else
            {
                lblClampStatus.Text = exists ? "✗  ไม่มีตาราง 'MainTable'" : "✗  ไม่พบไฟล์";
                lblClampStatus.ForeColor = DesignTokens.Danger;
            }
        }

        if (lblClampStatus.InvokeRequired) lblClampStatus.Invoke(Apply); else Apply();
    }

    private void BrowseClampFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*",
            Title = "Select mydatabase.db3"
        };

        var current = txtClampPath.Text.Trim();
        if (!string.IsNullOrEmpty(current) && File.Exists(current))
            dlg.InitialDirectory = Path.GetDirectoryName(current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtClampPath.Text = dlg.FileName;
        txtClampPath.BackColor = Color.LightYellow;
        UpdateClampStatus(dlg.FileName);
    }

    private void BrowseFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*",
            Title = "Select PrintData.db3"
        };

        var current = txtDbPath.Text.Trim();
        if (!string.IsNullOrEmpty(current) && File.Exists(current))
            dlg.InitialDirectory = Path.GetDirectoryName(current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtDbPath.Text = dlg.FileName;
        txtDbPath.BackColor = Color.LightYellow;
        UpdateStatus(dlg.FileName);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var path = txtDbPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณาเลือกไฟล์ Database");
            return;
        }

        if (!File.Exists(path))
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบไฟล์:\n{path}");
            return;
        }

        if (!HasInkjetDataTable(path))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ไฟล์ Database ไม่มีตาราง 'inkjet_data'");
            return;
        }

        // Clamp DB ไม่บังคับ — ว่างไว้ได้ แต่ถ้าใส่มาต้องใช้ได้จริง
        var clampPath = txtClampPath.Text.Trim();
        if (clampPath.Length > 0)
        {
            if (!File.Exists(clampPath))
            {
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบไฟล์ Clamp Database:\n{clampPath}");
                return;
            }

            if (!HasTable(clampPath, "MainTable"))
            {
                Notify.WarnModal(this, "แจ้งเตือน", "ไฟล์ Clamp Database ไม่มีตาราง 'MainTable'");
                return;
            }
        }

        CustomSettingsManager.Write("DB_PATH", path);
        CustomSettingsManager.Write("CLAMP_DB_PATH", clampPath);

        txtDbPath.BackColor = Color.White;
        txtClampPath.BackColor = Color.White;
        UpdateStatus(path);
        UpdateClampStatus(clampPath);

        Notify.Success(this, "บันทึกเรียบร้อย");
    }

    private static bool HasInkjetDataTable(string dbPath) => HasTable(dbPath, "inkjet_data");

    private static bool HasTable(string dbPath, string tableName)
    {
        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@t";
            cmd.Parameters.AddWithValue("@t", tableName);
            return cmd.ExecuteScalar() != null;
        }
        catch { return false; }
    }
}
