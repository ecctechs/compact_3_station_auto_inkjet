namespace InkjetOperator.Views;

/// <summary>
/// กล่องถาม ยืนยัน / ยกเลิก ของทั้งโปรแกรม — หน้าตาแบบหน้ายืนยันของ NastoKeyence
/// (<see cref="ConfirmDialog"/>) พื้นหลังมืดระหว่างเปิด
/// <para>
/// บล็อกจนคนตอบเหมือน <c>MessageBox.Show</c> ที่เคยใช้ — ผู้เรียกเขียนต่อได้ตามลำดับเดิม
/// คืน true เฉพาะกดปุ่มยืนยัน · ปุ่มยกเลิก, ✕ และ Esc คืน false
/// </para>
/// <para>
/// คู่กับ <see cref="Notify"/>: ที่นั่นคือเรื่องที่บอกให้รู้ ที่นี่คือเรื่องที่ต้องตอบ
/// กล่องแจ้งของ Notify ก็ใช้หน้าต่างเดียวกันนี้ (ปุ่มเดียว) ทั้งโปรแกรมจึงมีกล่องแบบเดียว
/// </para>
/// </summary>
internal static class Confirm
{
    /// <summary>
    /// ข้อความธรรมดาก้อนเดียว — ย่อหน้าแรกขึ้นเป็นหัวตัวใหญ่ ที่เหลือเป็นเนื้อความ
    /// กล่องที่ต้องการแยกหัวข้อ/ค่า ใช้ <see cref="Ask(Control?, ConfirmSpec)"/>
    /// </summary>
    public static bool Ask(Control? owner, string title, string content)
    {
        var (headline, rest) = SplitHeadline(content);
        return Ask(owner, new ConfirmSpec { Title = title, Headline = headline, Message = rest });
    }

    /// <summary>
    /// กล่องยืนยันแบบมีโครง — หัวข้อ/ค่า ปุ่มบอกการกระทำ คำเตือนตัวแดง
    /// </summary>
    /// <param name="owner">
    /// control ที่ถาม กล่องจะลอยอยู่กลางหน้าต่างที่มันอยู่ · null ได้ (ใช้หน้าต่างที่อยู่หน้าสุด)
    /// </param>
    public static bool Ask(Control? owner, ConfirmSpec spec)
    {
        var form = Notify.Resolve(owner);

        // ถูกเรียกจากเธรดอื่นได้ — พากลับเธรดหน้าจอก่อนเปิดหน้าต่าง
        if (form is { IsDisposed: false, InvokeRequired: true })
            return (bool)form.Invoke(() => ShowDialog(form, spec));

        return ShowDialog(form, spec);
    }

    private static bool ShowDialog(Form? owner, ConfirmSpec spec)
    {
        using var dim = owner == null ? null : DimOverlay.ShowOver(owner);
        using var dialog = new ConfirmDialog(spec);
        if (owner == null)
        {
            dialog.StartPosition = FormStartPosition.CenterScreen;
            return dialog.ShowDialog() == DialogResult.OK;
        }
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }

    /// <summary>
    /// แยกย่อหน้าแรกออกมาเป็นหัว ถ้าสั้นพอจะอ่านเป็นประโยคเดียว — ข้อความเดิมส่วนใหญ่เขียนแบบ
    /// "สรุปหนึ่งบรรทัด ว่างหนึ่งบรรทัด แล้วรายละเอียด" อยู่แล้ว
    /// · ตัด "ยืนยันหรือไม่?" ท้ายข้อความทิ้ง ปุ่มบอกอยู่แล้วว่ากดแล้วจะเกิดอะไร
    /// </summary>
    internal static (string? Headline, string? Body) SplitHeadline(string? text)
    {
        var body = (text ?? "").Replace("\r\n", "\n").Trim();
        foreach (var tail in new[] { "ยืนยันหรือไม่?", "ยืนยันหรือไม่", "ต้องการทำต่อหรือไม่?" })
            if (body.EndsWith(tail, StringComparison.Ordinal)) body = body[..^tail.Length].TrimEnd();

        if (body.Length == 0) return (null, null);

        int gap = body.IndexOf("\n\n", StringComparison.Ordinal);
        var first = gap < 0 ? body : body[..gap];
        var rest = gap < 0 ? null : body[(gap + 2)..].Trim();

        // หัวต้องเป็นประโยคสั้น ๆ บรรทัดเดียว — ยาวกว่านั้นเป็นเนื้อความทั้งก้อน
        if (first.Contains('\n') || first.Length > 70)
            return (null, Normalize(body));

        return (first, string.IsNullOrEmpty(rest) ? null : Normalize(rest));
    }

    private static string Normalize(string text) => text.Replace("\n", Environment.NewLine);
}
