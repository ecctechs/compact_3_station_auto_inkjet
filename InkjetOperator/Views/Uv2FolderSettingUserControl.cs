using InkjetOperator.Services;

using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// หน้าตั้งค่าเล็ก ๆ สำหรับ ST3 โดยเฉพาะ — มีช่องเดียวคือโฟลเดอร์ UV2
///
/// ST3 ต้องรู้จักโฟลเดอร์นี้เพื่อไล่ดูว่าโปรแกรมที่จะพิมพ์มีรุ่นย่อย .uvdx กี่ไฟล์
/// ตอนกดเริ่มงาน (ต้องเลือกที่ ST3 เพราะจอ ST1 ไม่มีคนเฝ้า) โดยปกติจะชี้ไปที่ share
/// บนเครื่อง ST1 เช่น <c>\\ST1-PC\UV2</c>
/// <para>
/// แยกออกมาจาก <see cref="InkjetSettingUserControl"/> ทั้งที่เก็บคีย์เดียวกัน
/// เพราะหน้านั้นมีทั้ง COM port ของ MK และ IP ของ UV ปนอยู่ ซึ่ง ST3 ไม่ควรแตะ
/// </para>
/// <para>
/// ตั้งใจไม่เขียน <c>UV1DB3_PATH_2</c> ตามอย่างหน้า Inkjet Setting — ค่านั้นคือ path
/// ของ CPI.db3 ที่ใช้ตอนส่งงานจริง ซึ่งเป็นงานของ ST1 ที่เดียว ST3 ไม่ได้เขียนอะไรลง UV
/// </para>
/// </summary>
public partial class Uv2FolderSettingUserControl : UserControl
{
    /// <summary>โฟลเดอร์ย่อยที่เก็บไฟล์โปรแกรม — ต้องตรงกับ UvSettingsManager</summary>
    private const string DocumentFolder = "document";

    public Uv2FolderSettingUserControl()
    {
        InitializeComponent();
        LoadData();

        btnBrowse.Click += (_, _) => BrowseFolder();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadData();
        txtUv2Folder.TextChanged += (_, _) => MarkDirty();
    }

    private void LoadData()
    {
        txtUv2Folder.Text = UvSettingsManager.Read("UV2_FOLDER");
        txtUv2Folder.BackColor = Color.White;
        UpdateStatus(txtUv2Folder.Text);
    }

    private void MarkDirty() => txtUv2Folder.BackColor = Color.LightYellow;

    /// <summary>
    /// บอกให้ครบว่าใช้ได้จริงไหม ไม่ใช่แค่ว่าโฟลเดอร์มีอยู่
    ///
    /// สิ่งที่ต้องรู้คือ "เห็นไฟล์ .uvdx กี่ไฟล์" เพราะ share ที่ต่อได้แต่ไม่มีสิทธิ์อ่าน
    /// หรือชี้ผิดชั้น จะดูเหมือนตั้งค่าสำเร็จทั้งที่เลือกรุ่นย่อยไม่ได้
    /// </summary>
    private void UpdateStatus(string folder)
    {
        folder = folder.Trim();

        if (folder.Length == 0)
        {
            lblStatus.Text = "ยังไม่ได้ตั้งค่า — จะเลือกรุ่นย่อยของโปรแกรมไม่ได้";
            lblStatus.ForeColor = Color.Gray;
            return;
        }

        if (!DirectoryReachable(folder))
        {
            lblStatus.Text = "✗  เข้าโฟลเดอร์ไม่ได้ — ตรวจ path และสิทธิ์เข้าถึง share";
            lblStatus.ForeColor = DesignTokens.Danger;
            return;
        }

        var documents = Path.Combine(folder, DocumentFolder);
        if (!DirectoryReachable(documents))
        {
            lblStatus.Text = $"✗  ไม่พบโฟลเดอร์ '{DocumentFolder}' ข้างใน — น่าจะชี้ผิดชั้น";
            lblStatus.ForeColor = DesignTokens.Danger;
            return;
        }

        int count = CountPrograms(documents);
        if (count == 0)
        {
            lblStatus.Text = "!  เข้าถึงได้ แต่ไม่พบไฟล์ .uvdx เลย";
            lblStatus.ForeColor = DesignTokens.Warning;
            return;
        }

        lblStatus.Text = $"✓  พร้อมใช้งาน — พบโปรแกรม {count} ไฟล์";
        lblStatus.ForeColor = DesignTokens.SuccessText;
    }

    private static bool DirectoryReachable(string path)
    {
        try { return Directory.Exists(path); }
        catch { return false; }
    }

    private static int CountPrograms(string documents)
    {
        try { return Directory.GetFiles(documents, "*.uvdx").Length; }
        catch { return 0; }
    }

    private void BrowseFolder()
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };

        var current = txtUv2Folder.Text.Trim();
        if (current.Length > 0 && DirectoryReachable(current))
            dlg.SelectedPath = current;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtUv2Folder.Text = dlg.SelectedPath;
        UpdateStatus(dlg.SelectedPath);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var folder = txtUv2Folder.Text.Trim();

        // ล้างค่าทิ้งได้ ไม่ถือว่าผิด — โปรแกรมยังทำงานต่อได้ แค่เลือกรุ่นย่อยไม่ได้
        if (folder.Length > 0 && !DirectoryReachable(folder))
        {
            Notify.WarnModal(this, "แจ้งเตือน",
                $"เข้าโฟลเดอร์นี้ไม่ได้:\n{folder}\n\n"
                + "ตรวจว่าเครื่อง ST1 แชร์โฟลเดอร์ไว้แล้ว และเครื่องนี้มีสิทธิ์อ่าน");
            return;
        }

        UvSettingsManager.Write("UV2_FOLDER", folder);

        txtUv2Folder.BackColor = Color.White;
        UpdateStatus(folder);

        Notify.Success(this, "บันทึกเรียบร้อย");
    }
}
