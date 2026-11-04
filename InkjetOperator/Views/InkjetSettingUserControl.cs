using System.Net.Sockets;
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

        btnUv1Browse.Click += async (_, _) => await BrowseFolderAsync(txtUv1Folder, lblUv1Status);
        btnUv2Browse.Click += async (_, _) => await BrowseFolderAsync(txtUv2Folder, lblUv2Status);

        btnMarkingRefBrowse.Click += async (_, _) => await BrowseMarkingRefFolderAsync();
        txtMarkingRefFolder.TextChanged += (_, _) => MarkDirty(txtMarkingRefFolder);

        btnCheckStatus.Click += async (_, _) => await CheckAllStatusAsync();
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => LoadAllSettings();

        Load += async (_, _) => await CheckAllStatusAsync();

        // ไฟสถานะต้องตรงกับของจริง ไม่ใช่ภาพนิ่งตั้งแต่ตอนเปิดโปรแกรม
        // กติกาทั้งหมดอยู่ที่ StatusRecheck
        Services.StatusRecheck.Wire(this, tmrAutoCheck, () => CheckAllStatusAsync(quiet: true));
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

        // ไม่รอผล — เครื่อง UV ดับตอนเปิดหน้า หน้าต้องขึ้นมาก่อน ไฟสถานะค่อยตามมา
        _ = ShowFolderStatusAsync(txtUv1Folder.Text, lblUv1Status);
        _ = ShowFolderStatusAsync(txtUv2Folder.Text, lblUv2Status);
        ResetColors();
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        // async void ของ event — กันกดซ้ำระหว่างรอตรวจโฟลเดอร์ และห้าม exception หลุด
        btnSave.Enabled = false;
        try
        {
            await SaveAsync();
        }
        catch (Exception ex)
        {
            AppLog.Error("InkjetSetting.Save", ex);
            Notify.ErrorModal(this, "บันทึกไม่สำเร็จ", ex.Message);
        }
        finally
        {
            if (!IsDisposed) btnSave.Enabled = true;
        }
    }

    private async Task SaveAsync()
    {
        // หัวพ่นสองตัวใช้ IP เดียวกันได้
        //
        // เดิมกันไว้เพราะคิดว่าเป็นคนละเครื่องต้องคนละที่อยู่ แต่หน้างานมีกรณีที่ทั้งสอง
        // หัวอยู่หลังที่อยู่เดียวกัน และการกันไว้ทำให้ตั้งค่าไม่ได้เลย
        var ip058 = txtMk058Ip.Text.Trim();
        var ip059 = txtMk059Ip.Text.Trim();

        var f1 = txtUv1Folder.Text.Trim();
        var f2 = txtUv2Folder.Text.Trim();
        if (!await ConfirmFolderAsync(f1, "UV1")) return;
        if (!await ConfirmFolderAsync(f2, "UV2")) return;
        if (IsDisposed) return;

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

    /// <param name="quiet">
    /// true = รอบตรวจซ้ำอัตโนมัติ ไม่ต้องหมุนปุ่มให้รำคาญตาทุก 15 วินาที
    /// เปลี่ยนแค่สีไฟกับข้อความ ซึ่งเป็นสิ่งที่คนเปิดหน้านี้มาดูอยู่แล้ว
    /// </param>
    public async Task CheckAllStatusAsync(bool quiet = false)
    {
        if (!quiet)
        {
            btnCheckStatus.Loading = true;
            btnCheckStatus.Enabled = false;
        }
        try
        {
            await Task.WhenAll(
                CheckMkPrinterAsync(txtMk058Ip.Text.Trim(), lblMk058Status),
                CheckMkPrinterAsync(txtMk059Ip.Text.Trim(), lblMk059Status),
                CheckUvPrinterAsync(txtUv1Ip.Text.Trim(), txtUv1Port.Text.Trim(),
                    txtUv1Folder.Text.Trim(), lblUv1Dot, lblUv1Status),
                CheckUvPrinterAsync(txtUv2Ip.Text.Trim(), txtUv2Port.Text.Trim(),
                    txtUv2Folder.Text.Trim(), lblUv2Dot, lblUv2Status));
        }
        finally
        {
            if (!quiet)
            {
                btnCheckStatus.Loading = false;
                btnCheckStatus.Enabled = true;
            }
        }
    }

    private static async Task CheckMkPrinterAsync(string ip, Label dot)
    {
        if (string.IsNullOrWhiteSpace(ip)) { SetDotColor(dot, Color.Gray); return; }

        // โหมด Mockup สถานะ (ตัวเลือกหน้างาน) — ขึ้นว่าต่อได้โดยไม่ต่อจริง ใช้ถ่ายรูปคู่มือ
        if (StatusMockup.Enabled) { SetDotColor(dot, DesignTokens.Success); return; }

        try
        {
            using var tcp = new TcpClient();
            await Services.TcpConnect.ConnectAsync(tcp, ip, MkPrinterPort, TimeSpan.FromSeconds(3));
            SetDotColor(dot, DesignTokens.Success);
        }
        catch { SetDotColor(dot, DesignTokens.Danger); }
    }

    private static async Task CheckUvPrinterAsync(
        string ip, string portText, string folder,
        Label dot, Label statusLabel)
    {
        // โหมด Mockup สถานะ (ตัวเลือกหน้างาน) — ขึ้นว่าต่อได้โดยไม่ต่อจริง ใช้ถ่ายรูปคู่มือ
        bool mockup = StatusMockup.Enabled && !string.IsNullOrWhiteSpace(ip);

        var check = mockup ? FolderCheck.Fine : await ValidateFolderAsync(folder);
        var folderOk = check.Problems.Count == 0 && !string.IsNullOrWhiteSpace(folder);

        var ipOk = false;

        if (mockup)
        {
            folderOk = true;
            ipOk = true;
        }
        else if (!string.IsNullOrWhiteSpace(ip) && int.TryParse(portText, out var port) && port > 0)
        {
            try
            {
                using var tcp = new TcpClient();
                await Services.TcpConnect.ConnectAsync(tcp, ip, port, TimeSpan.FromSeconds(3));
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
                    parts.Add(check.Unreachable ?? "ไฟล์ไม่ครบ");
                else if (string.IsNullOrWhiteSpace(folder))
                    parts.Add("ยังไม่ได้เลือกโฟลเดอร์");
                statusLabel.Text = "⚠ " + string.Join(" / ", parts);
                statusLabel.ForeColor = DesignTokens.Danger;
            }
        }

        if (dot.IsDisposed) return;
        if (dot.InvokeRequired) dot.Invoke(Apply); else Apply();
    }

    // ── Folder browsing & validation ────────────────────────────────

    private async Task BrowseFolderAsync(AntdUI.Input target, Label statusLabel)
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };

        // กล่องเลือกโฟลเดอร์ของ Windows เปิดไปที่แชร์ที่หลุดก็ค้างเหมือนกัน — เปิดที่เดิมเฉพาะตอนเข้าถึงได้
        if (await ReachableFolderAsync(target.Text)) dlg.SelectedPath = target.Text;
        if (IsDisposed || dlg.ShowDialog(this) != DialogResult.OK) return;

        target.Text = dlg.SelectedPath;
        MarkDirty(target);

        var check = await ValidateFolderAsync(dlg.SelectedPath);
        if (IsDisposed) return;

        ApplyFolderStatus(dlg.SelectedPath, statusLabel, check);
        if (check.Problems.Count > 0)
            Notify.WarnModal(this, "แจ้งเตือน", string.Join("\n\n", check.Problems));
    }

    /// <summary>
    /// ผลตรวจโฟลเดอร์ UV — Problems ว่าง = พร้อมใช้ ·
    /// Unreachable มีค่า = เครื่องที่เก็บโฟลเดอร์ไม่ตอบ บอกไม่ได้ว่าไฟล์อยู่ครบไหม
    /// </summary>
    private sealed record FolderCheck(List<string> Problems, string? Unreachable)
    {
        public static FolderCheck Fine => new([], null);
    }

    /// <summary>
    /// ตรวจว่าโฟลเดอร์โปรแกรม UV มีไฟล์ที่ต้องใช้ครบไหม
    ///
    /// <para>
    /// โฟลเดอร์นี้อยู่บนเครื่อง UV ผ่านเครือข่าย เดิมเรียก <c>File.Exists</c> บนหน้าจอตรง ๆ
    /// ถอดสายเครื่อง UV แล้วรอ 20–60 วินาทีต่อครั้ง หน้านี้ตรวจซ้ำทุก 15 วินาที
    /// โปรแกรมจึงค้างเกือบตลอด (เจอที่หน้างาน 4/11/2026 — call stack ค้างที่ตรงนี้)
    /// ใช้ <see cref="PathProbe"/> ถามเครื่องก่อนไม่เกิน 1 วินาที แล้วอ่านไฟล์นอกหน้าจอ
    /// </para>
    /// </summary>
    private static async Task<FolderCheck> ValidateFolderAsync(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return FolderCheck.Fine;

        // ตรวจแค่ว่ามีไฟล์ไหม ไม่เปิดเข้าไปดูชื่อตารางข้างใน
        //
        // เดิมบังคับว่าต้องมีตารางชื่อ MK063 (UV1) หรือ MK067 (UV2) แต่ชื่อตารางขึ้นกับ
        // ซอฟต์แวร์ที่ลงในเครื่อง UV แต่ละตัว ไม่ได้ตายตัวตามหมายเลขเครื่อง โฟลเดอร์ที่
        // ใช้งานได้จริงจึงถูกตีว่าใช้ไม่ได้ ทั้งที่ส่งงานเข้าเครื่องได้ปกติ
        //
        // ชื่อตารางที่ใช้ตอนเขียนจริงอยู่ที่ JobSendService ไม่ได้อ่านจากหน้านี้
        var problems = new List<string>();
        foreach (var rel in new[] { REL_CPI, REL_DEFAULT_UVDX })
        {
            var result = await PathProbe.FileAsync(Path.Combine(folder, rel));

            // เครื่องไม่ตอบ / อ่านไม่ทัน — ไม่ใช่ไฟล์หาย ไม่ต้องลองไฟล์ถัดไปให้รอซ้ำ
            if (result.State is PathState.HostDown or PathState.Slow)
                return new FolderCheck([result.Detail], result.Detail);

            if (result.State != PathState.Ok)
                problems.Add($"ไม่พบ {rel}");
        }

        return new FolderCheck(problems, null);
    }

    /// <summary>โฟลเดอร์นี้เปิดได้จริงไหม โดยไม่ค้าง — ใช้ก่อนส่งให้กล่องของ Windows</summary>
    private static async Task<bool> ReachableFolderAsync(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) && (await PathProbe.FolderAsync(folder)).State == PathState.Ok;

    /// <summary>
    /// ตรวจโฟลเดอร์ก่อนบันทึก — ไม่มีจริง = ไม่ให้บันทึก ·
    /// เครื่องไม่ตอบ = ถามก่อน เพราะไฟล์อาจอยู่ครบ แค่ตอนนี้เครื่อง UV ปิดหรือสายหลุด
    /// </summary>
    private async Task<bool> ConfirmFolderAsync(string folder, string uv)
    {
        if (string.IsNullOrEmpty(folder)) return true;

        var result = await PathProbe.FolderAsync(folder);
        if (IsDisposed) return false;

        switch (result.State)
        {
            case PathState.Ok:
                return true;
            case PathState.HostDown or PathState.Slow:
                return Confirm.Ask(this, new ConfirmSpec
                {
                    Title = "ตรวจโฟลเดอร์ไม่ได้",
                    Headline = $"บันทึกโฟลเดอร์ {uv} ทั้งที่ยังตรวจไม่ได้?",
                    Message = $"{result.Detail}\n{folder}",
                    Detail = "เครื่อง UV อาจปิดอยู่หรือสายหลุด — เปิดเครื่องแล้วกลับมาดูไฟสถานะอีกครั้ง",
                    OkText = "บันทึก",
                    CancelText = "ยกเลิก",
                    Icon = "ExclamationCircleFilled",
                });
            default:
                Notify.WarnModal(this, "ไม่พบโฟลเดอร์", $"ไม่พบโฟลเดอร์โปรแกรม {uv}\n\n{folder}");
                return false;
        }
    }

    private async Task ShowFolderStatusAsync(string folder, Label lbl)
    {
        var check = await ValidateFolderAsync(folder);
        if (lbl.IsDisposed) return;
        ApplyFolderStatus(folder, lbl, check);
    }

    private static void ApplyFolderStatus(string folder, Label lbl, FolderCheck check)
    {
        if (string.IsNullOrWhiteSpace(folder)) { lbl.Text = ""; lbl.ForeColor = Color.Gray; return; }
        if (check.Problems.Count == 0)
        {
            lbl.Text = "✓ CPI.db3 + default.uvdx พร้อมใช้งาน";
            lbl.ForeColor = DesignTokens.SuccessText;
        }
        else
        {
            lbl.Text = check.Unreachable != null ? $"⚠ {check.Unreachable}" : $"⚠ พบ {check.Problems.Count} ปัญหา";
            lbl.ForeColor = DesignTokens.Danger;
        }
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

    private async Task BrowseMarkingRefFolderAsync()
    {
        using var dlg = new FolderBrowserDialog { ShowNewFolderButton = false };
        if (await ReachableFolderAsync(txtMarkingRefFolder.Text))
            dlg.SelectedPath = txtMarkingRefFolder.Text;
        if (IsDisposed || dlg.ShowDialog(this) != DialogResult.OK) return;

        txtMarkingRefFolder.Text = dlg.SelectedPath;
        MarkDirty(txtMarkingRefFolder);
    }

    // รับเป็น Control เพราะช่อง IP เปลี่ยนไปใช้ IpAddressInput ที่ไม่ใช่ AntdUI.Input
    private void MarkDirty(Control input) => input.BackColor = Color.LightYellow;

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
