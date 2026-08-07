using Microsoft.Data.Sqlite;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

public partial class DatabaseSettingUserControl : UserControl
{
    public DatabaseSettingUserControl()
    {
        InitializeComponent();
        LoadData();

        btnBrowse.Click += (_, _) => BrowseFile();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();
    }

    private void LoadData()
    {
        txtDbPath.Text = CustomSettingsManager.Read("DB_PATH");
        ShowStatus(txtDbPath.Text);
        txtDbPath.BackColor = Color.White;
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
        ShowStatus(dlg.FileName);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var path = txtDbPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show("กรุณาเลือกไฟล์ Database",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show($"ไม่พบไฟล์:\n{path}",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!HasInkjetDataTable(path))
        {
            MessageBox.Show("ไฟล์ Database ไม่มีตาราง 'inkjet_data'",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CustomSettingsManager.Write("DB_PATH", path);
        txtDbPath.BackColor = Color.White;
        MessageBox.Show("บันทึกเรียบร้อย", "Settings",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowStatus(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            lblStatus.Text = "";
            lblStatus.ForeColor = Color.Gray;
            return;
        }

        if (!File.Exists(path))
        {
            lblStatus.Text = "⚠ ไม่พบไฟล์";
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            return;
        }

        if (!HasInkjetDataTable(path))
        {
            lblStatus.Text = "⚠ ไม่มีตาราง 'inkjet_data'";
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            return;
        }

        lblStatus.Text = "✓ พร้อมใช้งาน";
        lblStatus.ForeColor = Color.FromArgb(21, 128, 61);
    }

    private static bool HasInkjetDataTable(string dbPath)
    {
        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='inkjet_data'";
            return cmd.ExecuteScalar() != null;
        }
        catch { return false; }
    }
}
