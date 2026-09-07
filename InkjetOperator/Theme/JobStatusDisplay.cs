namespace InkjetOperator.Theme;

/// <summary>
/// สถานะงานที่ผู้ใช้เห็นมี 3 แบบ: Waiting แดง · Working ส้ม · Finished เขียว
/// ส่วน backend ยังเก็บเป็น Waiting / Process / Success เหมือนเดิม แปลงตอนแสดงผล
/// <para>
/// อยู่ที่เดียวเพราะ Order List กับ Order Detail ต้องใช้สีชุดเดียวกัน — ถ้าแยกกัน
/// ตีความ พอวันหนึ่งเพิ่มสถานะใหม่แล้วแก้ไม่ครบ สองหน้าจะบอกคนละเรื่องกัน
/// </para>
/// </summary>
internal static class JobStatusDisplay
{
    /// <summary>ชื่อกับสีที่หน้าจอใช้ สำหรับค่าสถานะดิบจาก backend</summary>
    public static (string Text, Color Fore) Resolve(string? status)
    {
        if (string.Equals(status, "Process", StringComparison.OrdinalIgnoreCase))
            return ("Working", DesignTokens.Warning);
        if (string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase))
            return ("Finished", DesignTokens.SuccessText);
        if (string.Equals(status, "Waiting", StringComparison.OrdinalIgnoreCase))
            return ("Waiting", DesignTokens.Danger);
        if (string.Equals(status, "Cancel", StringComparison.OrdinalIgnoreCase))
            return ("Cancelled", DesignTokens.TextSecondary);

        // สถานะนอกเหนือจาก 3 แบบถูกกรองออกไปแล้ว โชว์ค่าดิบไว้กันงงถ้าหลุดมา
        return (status ?? "", DesignTokens.Danger);
    }

    public static string Text(string? status) => Resolve(status).Text;

    public static Color Fore(string? status) => Resolve(status).Fore;
}
