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
        bool answer;
        using (owner == null ? null : DimOverlay.ShowOver(owner))
        {
            var dialog = new ConfirmDialog(spec);
            try
            {
                if (owner == null) dialog.StartPosition = FormStartPosition.CenterScreen;
                answer = (owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner)) == DialogResult.OK;
            }
            finally
            {
                DisposeSafely(dialog);
            }
        }

        // คำถามลง log พร้อมคำตอบ — ย้อนดูได้ว่าใครกดยืนยันอะไร (กล่องแจ้งปุ่มเดียว Notify บันทึกเองแล้ว)
        if (spec.ShowCancel)
            Services.AppLog.Info("Confirm", $"[{spec.Title}] {Describe(spec)} → {(answer ? spec.OkText : spec.CancelText)}");
        return answer;
    }

    /// <summary>
    /// ปิดกล่องทิ้งหลังได้คำตอบแล้ว — ปิดไม่ได้ต้องไม่ทำให้คำตอบหาย
    ///
    /// <para>
    /// พบหน้างาน 30/10/2026 (DEX-1625 กล่อง "ข้ามแล้วเริ่มงาน" เปิดค้างราว 2 นาที): ปุ่มของ AntdUI
    /// ในกล่องค้างสถานะกำลังสร้าง handle ตอนปิด WinForms โยน "Value Dispose() cannot be called while
    /// doing CreateHandle()" error หลุดออกไปทั้งที่คนกดตอบแล้ว การเริ่มงานหยุดกลางทาง
    /// ขึ้นกล่อง "เกิดข้อผิดพลาดที่ไม่ได้คาดไว้"
    /// </para>
    /// <para>
    /// คำตอบได้มาก่อนปิดแล้ว จึงใช้คำตอบต่อตามปกติ ปิดไม่ได้ลง log แล้วลองปิดอีกครั้งรอบถัดไป
    /// ของเธรดหน้าจอ (ตอนนั้นปุ่มสร้างเสร็จแล้ว) — ยังไม่ได้อีกก็ปล่อยไว้ หน้าต่างปิดจากจอไปแล้ว
    /// </para>
    /// </summary>
    private static void DisposeSafely(Form dialog)
    {
        // อ่านชื่อไว้ก่อน — หลังปิดไม่ครบ ไม่แตะคุณสมบัติของหน้าต่างอีก
        var title = dialog.Text;
        try
        {
            dialog.Dispose();
        }
        catch (InvalidOperationException ex)
        {
            Services.AppLog.Warn("Confirm", $"ปิดกล่อง [{title}] ทันทีไม่ได้ ({ex.Message}) · ลองปิดอีกครั้งภายหลัง");
            SynchronizationContext.Current?.Post(_ =>
            {
                try { dialog.Dispose(); }
                catch (InvalidOperationException retry)
                {
                    Services.AppLog.Warn("Confirm", $"ปิดกล่อง [{title}] รอบสองไม่ได้ ({retry.Message}) · ปล่อยไว้");
                }
            }, null);
        }
    }

    /// <summary>เนื้อหากล่องยืนยันเป็นบรรทัดเดียวสำหรับ log</summary>
    private static string Describe(ConfirmSpec spec)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(spec.Headline)) parts.Add(spec.Headline!);
        parts.AddRange(spec.Facts.Select(f => $"{f.Caption}: {f.Value}"));
        if (!string.IsNullOrWhiteSpace(spec.Warning)) parts.Add("⚠ " + spec.Warning);
        return string.Join(" / ", parts.Select(p => p.Replace("\r\n", " · ").Replace("\n", " · ")));
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
