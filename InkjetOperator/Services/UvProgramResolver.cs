using InkjetOperator.Theme;

namespace InkjetOperator.Services;

/// <summary>ผลการหาไฟล์โปรแกรม UV</summary>
/// <param name="Program">ชื่อไฟล์ที่จะส่งให้เครื่อง (ไม่มีนามสกุล) — null = ยกเลิก</param>
/// <param name="IsDefault">true = ไม่พบโปรแกรมที่ขอ ตกไปใช้ default.uvdx</param>
public sealed record UvProgramPick(string? Program, bool IsDefault);

/// <summary>
/// หาไฟล์ .uvdx ที่จะส่งให้เครื่อง จากชื่อโปรแกรมที่ได้มา
///
/// รุ่นย่อย = ชื่อเดิมต่อด้วย "-" เท่านั้น (S-DEX-1624 → S-DEX-1624-1)
/// ตรงกับเงื่อนไข SQL ของระบบเดิม: WHERE program = base OR program LIKE 'base-%'
/// ใช้ StartsWith เปล่าๆ จะกินชื่อที่แค่ขึ้นต้นเหมือนกัน (S-DEX-16240, KJZ-684)
///
/// อยู่ที่นี่เพื่อให้หน้าส่งงานจริงกับหน้าทดสอบใช้ตรรกะเดียวกัน ไม่หลุดกันภายหลัง
/// </summary>
public static class UvProgramResolver
{
    public const string DefaultProgram = "default";

    /// <summary>
    /// เจอ 1 ไฟล์ → ใช้เลย · เจอหลายไฟล์ → ให้เลือก · ไม่เจอ → default
    /// ไม่รู้จักโฟลเดอร์ document → ส่งชื่อที่ได้มาตรงๆ ให้เครื่องตัดสินเอง
    /// </summary>
    public static UvProgramPick Resolve(string? programName, string? docFolder, IWin32Window? owner = null)
    {
        var baseName = (programName ?? "").Trim();
        if (baseName.Length == 0) return new UvProgramPick(null, false);

        baseName = Path.GetFileNameWithoutExtension(baseName);
        if (docFolder == null) return new UvProgramPick(baseName, false);

        List<string> candidates;
        try
        {
            candidates = Directory.GetFiles(docFolder, "*.uvdx")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(f => !string.IsNullOrEmpty(f))
                .Where(f => f! == baseName ||
                            f!.StartsWith(baseName + "-", StringComparison.Ordinal))
                .OrderBy(f => f)
                .ToList()!;
        }
        catch
        {
            return new UvProgramPick(baseName, false);
        }

        if (candidates.Count == 1) return new UvProgramPick(candidates[0], false);
        if (candidates.Count > 1) return new UvProgramPick(PromptVariant(candidates, owner), false);

        return new UvProgramPick(DefaultProgram, true);
    }

    /// <summary>ยืนยันก่อนใช้ default — ไม่ให้พิมพ์ผิดแบบโดยไม่รู้ตัว</summary>
    public static bool ConfirmDefault(string requestedProgram, string machineName, IWin32Window? owner = null)
    {
        var text =
            $"ไม่พบโปรแกรม \"{requestedProgram}\" ในเครื่อง {machineName}\n\n"
            + "ระบบจะใช้โปรแกรม default.uvdx แทนไปก่อน\n"
            + "กรุณาแจ้งผู้ดูแลให้เพิ่มโปรแกรมนี้เข้าเครื่อง\n\n"
            + "ต้องการทำต่อหรือไม่?";

        var result = owner == null
            ? MessageBox.Show(text, "ไม่พบโปรแกรม",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
            : MessageBox.Show(owner, text, "ไม่พบโปรแกรม",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }

    private static string? PromptVariant(List<string> variants, IWin32Window? owner)
    {
        using var dlg = new Form
        {
            Text = "เลือกรุ่นย่อย",
            Size = new Size(420, 340),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowIcon = false,
        };

        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.Body(14f),
            IntegralHeight = false,
        };
        foreach (var v in variants) list.Items.Add(v + ".uvdx");
        list.SelectedIndex = 0;

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 56,
            Padding = new Padding(10),
        };
        var ok = new Button { Text = "ตกลง", DialogResult = DialogResult.OK, Size = new Size(100, 36) };
        var cancel = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Size = new Size(100, 36) };
        bar.Controls.Add(ok);
        bar.Controls.Add(cancel);

        list.DoubleClick += (_, _) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

        dlg.Controls.Add(list);
        dlg.Controls.Add(bar);
        dlg.AcceptButton = ok;
        dlg.CancelButton = cancel;

        var answer = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
        if (answer != DialogResult.OK) return null;

        int i = list.SelectedIndex;
        return i >= 0 && i < variants.Count ? variants[i] : null;
    }
}
