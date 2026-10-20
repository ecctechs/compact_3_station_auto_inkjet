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
        btnBrowseExcel.Click += (_, _) => BrowseExcelFile();
        btnImportNow.Click += async (_, _) => await ImportNowAsync();
        btnExportTemplate.Click += async (_, _) => await ExportTemplateAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();

        // ไฟสถานะต้องตรงกับของจริง ไม่ใช่ภาพนิ่งตั้งแต่ตอนเปิดโปรแกรม
        // กติกาทั้งหมดอยู่ที่ StatusRecheck
        Services.StatusRecheck.Wire(this, tmrAutoCheck, () => CheckStatusAsync());
    }

    private void LoadData()
    {
        txtDbPath.Text = CustomSettingsManager.Read("DB_PATH");
        UpdateStatus(txtDbPath.Text);
        txtDbPath.BackColor = Color.White;

        txtClampPath.Text = CustomSettingsManager.Read("CLAMP_DB_PATH");
        UpdateClampStatus(txtClampPath.Text);
        txtClampPath.BackColor = Color.White;

        txtExcelPath.Text = CustomSettingsManager.Read(PrintDataExcelImport.ExcelPathKey);
        txtExcelPath.BackColor = Color.White;
        chkImportAtStart.Checked = CustomSettingsManager.Read(PrintDataExcelImport.ImportAtStartKey, "0") == "1";
        UpdateExcelStatus(txtExcelPath.Text, txtDbPath.Text);
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
        var excelPath = CustomSettingsManager.Read(PrintDataExcelImport.ExcelPathKey);
        await Task.Run(() =>
        {
            UpdateStatus(path);
            UpdateClampStatus(clampPath);
            UpdateExcelStatus(excelPath, path);
        });
    }

    /// <summary>
    /// สถานะของไฟล์ Excel — แปลงล่าสุดเมื่อไร จากไฟล์ไหน
    /// ไม่ตั้งค่าก็ไม่ถือว่าผิด เครื่องที่ไม่ได้แปลงไม่ต้องใช้
    /// </summary>
    private void UpdateExcelStatus(string excelPath, string dbPath)
    {
        bool set = !string.IsNullOrWhiteSpace(excelPath);
        bool exists = set && File.Exists(excelPath);
        var last = PrintDataExcelImport.LastImport(dbPath);

        void Apply()
        {
            if (set && !exists)
            {
                lblExcelStatus.Text = "✗  ไม่พบไฟล์ Excel";
                lblExcelStatus.ForeColor = DesignTokens.Danger;
            }
            else
            {
                lblExcelStatus.Text = last ?? (set ? "ยังไม่เคยแปลงจาก Excel" : "");
                lblExcelStatus.ForeColor = last != null ? DesignTokens.SuccessText : Color.Gray;
            }
        }

        if (lblExcelStatus.InvokeRequired) lblExcelStatus.Invoke(Apply); else Apply();
    }

    private void BrowseExcelFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Excel Workbook (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|Excel Workbook (*.xlsx)|*.xlsx|Macro-Enabled Excel Workbook (*.xlsm)|*.xlsm",
            Title = "Select the PrintData Excel file"
        };

        var current = txtExcelPath.Text.Trim();
        if (!string.IsNullOrEmpty(current) && File.Exists(current))
            dlg.InitialDirectory = Path.GetDirectoryName(current);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtExcelPath.Text = dlg.FileName;
        txtExcelPath.BackColor = Color.LightYellow;
        UpdateExcelStatus(dlg.FileName, txtDbPath.Text);
    }

    /// <summary>
    /// แปลง Excel ที่เลือกไว้ในช่องเป็น PrintData.db3 เดี๋ยวนี้ ไม่ต้องรอเปิดโปรแกรมใหม่
    ///
    /// <para>
    /// แปลงแม้ Excel ไม่เปลี่ยนจากครั้งก่อน — คนกดเองแปลว่าต้องการแปลง
    /// แถวหายเกินครึ่งถามยืนยันอีกชั้น ตอนเปิดโปรแกรมไม่มีใครตอบ จึงไม่แปลงให้เลย
    /// คนต้องมากดที่นี่เอง
    /// </para>
    /// </summary>
    private async Task ImportNowAsync()
    {
        var excelPath = txtExcelPath.Text.Trim();
        var dbPath = txtDbPath.Text.Trim();
        if (excelPath.Length == 0 || dbPath.Length == 0)
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณาเลือกไฟล์ Excel และไฟล์ PrintData.db3 ก่อน");
            return;
        }

        if (!Confirm.Ask(this, "แปลงไฟล์ Excel",
                "แทนที่ PrintData.db3 ทั้งไฟล์ด้วยข้อมูลจาก Excel\n\n"
                + $"Excel: {excelPath}\nPrintData.db3: {dbPath}\n\n"
                + "ไฟล์เดิมเก็บสำรองไว้เป็น PrintData.db3.bak"))
            return;

        btnImportNow.Loading = true;
        btnExportTemplate.Enabled = false;
        try
        {
            var result = await Task.Run(() => PrintDataExcelImport.Import(excelPath, dbPath, force: true, allowShrink: false));

            if (result.Outcome == PrintDataExcelImport.Outcome.NeedsConfirm
                && Confirm.Ask(this, "ข้อมูลน้อยลงมาก",
                    result.Message + "\n\nตรวจแล้วว่าใช้ไฟล์ถูกต้อง และต้องการแปลงต่อหรือไม่?"))
                result = await Task.Run(() => PrintDataExcelImport.Import(excelPath, dbPath, force: true, allowShrink: true));

            ShowImportResult(result);
        }
        finally
        {
            btnImportNow.Loading = false;
            btnExportTemplate.Enabled = true;
            UpdateStatus(dbPath);
            UpdateExcelStatus(excelPath, dbPath);
        }
    }

    private void ShowImportResult(PrintDataExcelImport.ImportResult result)
    {
        switch (result.Outcome)
        {
            case PrintDataExcelImport.Outcome.Imported:
                Notify.SuccessModal(this, "แปลงไฟล์สำเร็จ", result.Message);
                break;
            case PrintDataExcelImport.Outcome.NeedsConfirm:
                // คนกดไม่ยืนยัน — ไม่ต้องบอกซ้ำ
                break;
            default:
                Notify.WarnModal(this, "แปลงไฟล์ไม่สำเร็จ", result.Message);
                break;
        }
    }

    /// <summary>
    /// สร้าง Excel ต้นแบบจาก PrintData.db3 ที่ใช้อยู่ — ให้ลูกค้าใช้เป็นแม่แบบ
    /// ชื่อชีตและหัวคอลัมน์ตรงกับที่ตัวแปลงต้องการทุกตัว
    /// </summary>
    private async Task ExportTemplateAsync()
    {
        var dbPath = txtDbPath.Text.Trim();
        if (dbPath.Length == 0 || !File.Exists(dbPath))
        {
            Notify.WarnModal(this, "แจ้งเตือน", "กรุณาเลือกไฟล์ PrintData.db3 ที่ใช้อยู่ก่อน");
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            Title = "Save Excel template",
            FileName = "PrintData.xlsx",
            InitialDirectory = Path.GetDirectoryName(dbPath),
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var target = dlg.FileName;

        btnExportTemplate.Loading = true;
        btnImportNow.Enabled = false;
        try
        {
            await Task.Run(() => PrintDataExcelImport.ExportTemplate(dbPath, target));
            AppLog.Info("ExcelImport", $"สร้าง Excel ต้นแบบ {target} จาก {dbPath}");
            Notify.SuccessModal(this, "สร้างไฟล์ต้นแบบแล้ว", target);
        }
        catch (IOException ex)
        {
            AppLog.Warn("ExcelImport", $"สร้าง Excel ต้นแบบไม่ได้: {ex.Message}");
            Notify.WarnModal(this, "สร้างไฟล์ไม่สำเร็จ", $"ไฟล์นี้อาจเปิดอยู่ใน Excel ปิดก่อนแล้วลองใหม่\n\n{ex.Message}");
        }
        catch (Exception ex)
        {
            AppLog.Error("ExcelImport", ex);
            Notify.WarnModal(this, "สร้างไฟล์ไม่สำเร็จ", ex.Message);
        }
        finally
        {
            btnExportTemplate.Loading = false;
            btnImportNow.Enabled = true;
        }
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

        // Excel ไม่บังคับ — เครื่องที่ไม่ได้แปลงไม่ต้องตั้ง แต่ติ๊กให้แปลงแล้วต้องมีไฟล์
        var excelPath = txtExcelPath.Text.Trim();
        if (excelPath.Length > 0 && !File.Exists(excelPath))
        {
            Notify.WarnModal(this, "แจ้งเตือน", $"ไม่พบไฟล์ Excel:\n{excelPath}");
            return;
        }

        if (chkImportAtStart.Checked && excelPath.Length == 0)
        {
            Notify.WarnModal(this, "แจ้งเตือน", "ติ๊กให้แปลงตอนเปิดโปรแกรมแล้ว กรุณาเลือกไฟล์ Excel");
            return;
        }

        CustomSettingsManager.Write("DB_PATH", path);
        CustomSettingsManager.Write("CLAMP_DB_PATH", clampPath);
        CustomSettingsManager.Write(PrintDataExcelImport.ExcelPathKey, excelPath);
        CustomSettingsManager.Write(PrintDataExcelImport.ImportAtStartKey, chkImportAtStart.Checked ? "1" : "0");

        txtDbPath.BackColor = Color.White;
        txtClampPath.BackColor = Color.White;
        txtExcelPath.BackColor = Color.White;
        UpdateStatus(path);
        UpdateClampStatus(clampPath);
        UpdateExcelStatus(excelPath, path);

        Notify.Success(this, "บันทึกเรียบร้อย");
    }

    private static bool HasInkjetDataTable(string dbPath) => HasTable(dbPath, "inkjet_data");

    private static bool HasTable(string dbPath, string tableName)
    {
        try
        {
            using var readLock = PrintDataDbLock.Read();
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
