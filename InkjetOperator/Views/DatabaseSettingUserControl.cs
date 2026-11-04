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

        btnBrowse.Click += async (_, _) => await BrowseFileAsync();
        btnBrowseClamp.Click += async (_, _) => await BrowseClampFileAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();

        // ไฟสถานะต้องตรงกับของจริง ไม่ใช่ภาพนิ่งตั้งแต่ตอนเปิดโปรแกรม
        // กติกาทั้งหมดอยู่ที่ StatusRecheck
        Services.StatusRecheck.Wire(this, tmrAutoCheck, () => CheckStatusAsync());
    }

    private void LoadData()
    {
        // ไม่รอผล — เครื่องที่เก็บไฟล์ดับตอนเปิดหน้า หน้าต้องขึ้นมาก่อน ข้อความสถานะค่อยตามมา
        txtDbPath.Text = CustomSettingsManager.Read("DB_PATH");
        _ = UpdateStatusAsync(txtDbPath.Text);
        txtDbPath.BackColor = Color.White;

        txtClampPath.Text = CustomSettingsManager.Read("CLAMP_DB_PATH");
        _ = UpdateClampStatusAsync(txtClampPath.Text);
        txtClampPath.BackColor = Color.White;
    }

    private enum DbState { NotSet, Ok, Missing, NoTable, Unreachable }

    /// <summary>เพดานเวลาเปิดไฟล์ดูชื่อตาราง หลังเครื่องปลายทางตอบแล้ว</summary>
    private static readonly TimeSpan TableTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// ตรวจไฟล์ฐานข้อมูลโดยไม่แตะหน้าจอ — Detail คือเหตุผลตอน Unreachable
    ///
    /// <para>
    /// ไฟล์ทั้งสองอยู่บนไดรฟ์ที่ map ไว้ เดิมเรียก <c>File.Exists</c> บนหน้าจอ (ตอนเปิดหน้า กด Save
    /// และตอนขึ้นข้อความ "ไม่พบไฟล์" ของรอบตรวจซ้ำ) เครื่องปลายทางหลุดแล้วค้าง 20–60 วินาที
    /// ถามเครื่องก่อนด้วย <see cref="PathProbe"/> ไม่ตอบก็ไม่แตะไฟล์ ตอบแล้วค่อยเปิดดูนอกหน้าจอ
    /// </para>
    /// </summary>
    private static async Task<(DbState State, string Detail)> ProbeDbAsync(string? path, string table)
    {
        if (string.IsNullOrWhiteSpace(path)) return (DbState.NotSet, "");

        var file = await PathProbe.FileAsync(path);
        if (file.State is PathState.HostDown or PathState.Slow) return (DbState.Unreachable, file.Detail);
        if (file.State != PathState.Ok) return (DbState.Missing, "");

        try
        {
            bool has = await Task.Run(() => HasTable(path, table)).WaitAsync(TableTimeout);
            return has ? (DbState.Ok, "") : (DbState.NoTable, "");
        }
        catch (TimeoutException)
        {
            return (DbState.Unreachable, $"เปิดไฟล์ไม่ทันใน {TableTimeout.TotalSeconds:0} วินาที");
        }
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
        await Task.WhenAll(UpdateStatusAsync(path), UpdateClampStatusAsync(clampPath));
    }

    private async Task UpdateStatusAsync(string path)
    {
        var (state, detail) = await ProbeDbAsync(path, "inkjet_data");

        void Apply()
        {
            switch (state)
            {
                case DbState.NotSet:
                    lblStatus.Text = "";
                    lblStatus.ForeColor = Color.Gray;
                    return;
                case DbState.Ok:
                    lblStatus.Text = "✓  พร้อมใช้งาน";
                    lblStatus.ForeColor = DesignTokens.SuccessText;
                    return;
                default:
                    lblStatus.Text = StateText(state, detail, "inkjet_data");
                    lblStatus.ForeColor = DesignTokens.Danger;
                    return;
            }
        }

        if (lblStatus.IsDisposed) return;
        if (lblStatus.InvokeRequired) lblStatus.Invoke(Apply); else Apply();
    }

    /// <summary>
    /// สถานะของ mydatabase.db3 — ต้องมีตาราง MainTable ถึงจะอ่านระยะแคลมป์ได้
    /// ไม่ตั้งค่าก็ไม่ถือว่าผิด (งานที่ไม่ผ่าน UV ไม่ต้องใช้) จึงขึ้นเป็นข้อความเทาเฉยๆ
    /// </summary>
    private async Task UpdateClampStatusAsync(string path)
    {
        var (state, detail) = await ProbeDbAsync(path, "MainTable");

        void Apply()
        {
            switch (state)
            {
                case DbState.NotSet:
                    lblClampStatus.Text = "ยังไม่ได้ตั้งค่า — ระยะแคลมป์ (IAI) จะถูกบันทึกเป็นค่าว่าง";
                    lblClampStatus.ForeColor = Color.Gray;
                    return;
                case DbState.Ok:
                    lblClampStatus.Text = "✓  พร้อมใช้งาน";
                    lblClampStatus.ForeColor = DesignTokens.SuccessText;
                    return;
                default:
                    lblClampStatus.Text = StateText(state, detail, "MainTable");
                    lblClampStatus.ForeColor = DesignTokens.Danger;
                    return;
            }
        }

        if (lblClampStatus.IsDisposed) return;
        if (lblClampStatus.InvokeRequired) lblClampStatus.Invoke(Apply); else Apply();
    }

    private static string StateText(DbState state, string detail, string table) => state switch
    {
        DbState.NoTable => $"✗  ไม่มีตาราง '{table}'",
        DbState.Unreachable => $"✗  {detail}",
        _ => "✗  ไม่พบไฟล์",
    };

    /// <summary>
    /// โฟลเดอร์ของไฟล์เดิม ถ้าเปิดได้จริง — กล่องเลือกไฟล์ของ Windows เปิดไปที่แชร์ที่หลุดก็ค้างเหมือนกัน
    /// </summary>
    private static async Task<string?> ReachableFolderOfAsync(string current)
    {
        if (string.IsNullOrEmpty(current)) return null;
        return (await PathProbe.FileAsync(current)).State == PathState.Ok
            ? Path.GetDirectoryName(current)
            : null;
    }

    private async Task BrowseClampFileAsync()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*",
            Title = "Select mydatabase.db3"
        };

        if (await ReachableFolderOfAsync(txtClampPath.Text.Trim()) is string folder)
            dlg.InitialDirectory = folder;

        if (IsDisposed || dlg.ShowDialog(this) != DialogResult.OK) return;

        txtClampPath.Text = dlg.FileName;
        txtClampPath.BackColor = Color.LightYellow;
        await UpdateClampStatusAsync(dlg.FileName);
    }

    private async Task BrowseFileAsync()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "SQLite Database (*.db3)|*.db3|All Files (*.*)|*.*",
            Title = "Select PrintData.db3"
        };

        if (await ReachableFolderOfAsync(txtDbPath.Text.Trim()) is string folder)
            dlg.InitialDirectory = folder;

        if (IsDisposed || dlg.ShowDialog(this) != DialogResult.OK) return;

        txtDbPath.Text = dlg.FileName;
        txtDbPath.BackColor = Color.LightYellow;
        await UpdateStatusAsync(dlg.FileName);
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        // async void ของ event — กันกดซ้ำระหว่างรอตรวจไฟล์ และห้าม exception หลุด
        btnSave.Enabled = false;
        try
        {
            await SaveAsync();
        }
        catch (Exception ex)
        {
            AppLog.Error("DatabaseSetting.Save", ex);
            Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", ex.Message);
        }
        finally
        {
            if (!IsDisposed) btnSave.Enabled = true;
        }
    }

    private async Task SaveAsync()
    {
        var path = txtDbPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณาเลือกไฟล์ Database");
            return;
        }

        if (!await AcceptDbAsync(path, "inkjet_data", "Database")) return;

        // Clamp DB ไม่บังคับ — ว่างไว้ได้ แต่ถ้าใส่มาต้องใช้ได้จริง
        var clampPath = txtClampPath.Text.Trim();
        if (clampPath.Length > 0 && !await AcceptDbAsync(clampPath, "MainTable", "Clamp Database")) return;

        if (IsDisposed) return;

        CustomSettingsManager.Write("DB_PATH", path);
        CustomSettingsManager.Write("CLAMP_DB_PATH", clampPath);

        txtDbPath.BackColor = Color.White;
        txtClampPath.BackColor = Color.White;
        _ = UpdateStatusAsync(path);
        _ = UpdateClampStatusAsync(clampPath);

        Notify.Success(this, "บันทึกเรียบร้อย");
    }

    /// <summary>
    /// ตรวจไฟล์ก่อนบันทึก — ไม่มีไฟล์ / ไม่มีตาราง = ไม่ให้บันทึก ·
    /// เครื่องไม่ตอบ = ถามก่อน เพราะไฟล์อาจอยู่ครบ แค่ตอนนี้เครื่องที่เก็บไฟล์ปิดหรือสายหลุด
    /// </summary>
    private async Task<bool> AcceptDbAsync(string path, string table, string name)
    {
        var (state, detail) = await ProbeDbAsync(path, table);
        if (IsDisposed) return false;

        switch (state)
        {
            case DbState.Ok:
                return true;
            case DbState.Unreachable:
                return Confirm.Ask(this, new ConfirmSpec
                {
                    Title = "ตรวจไฟล์ไม่ได้",
                    Headline = $"บันทึก {name} ทั้งที่ยังตรวจไม่ได้?",
                    Message = $"{detail}\n{path}",
                    Detail = "เครื่องที่เก็บไฟล์อาจปิดอยู่หรือสายหลุด — เปิดเครื่องแล้วกลับมาดูสถานะอีกครั้ง",
                    OkText = "บันทึก",
                    CancelText = "ยกเลิก",
                    Icon = "ExclamationCircleFilled",
                });
            case DbState.NoTable:
                Notify.WarnModal(this, "แจ้งเตือน", $"ไฟล์ {name} ไม่มีตาราง '{table}'");
                return false;
            default:
                Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบไฟล์ {name}:\n{path}");
                return false;
        }
    }

    private static bool HasTable(string dbPath, string tableName)
    {
        try
        {
            using var conn = new SqliteConnection(SqlitePath.ReadOnly(dbPath));
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@t";
            cmd.Parameters.AddWithValue("@t", tableName);
            return cmd.ExecuteScalar() != null;
        }
        catch { return false; }
    }
}
