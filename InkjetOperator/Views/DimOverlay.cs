using InkjetOperator.Theme;

namespace InkjetOperator.Views;

/// <summary>
/// ชั้นมืดครอบหน้าต่างหลักระหว่างกล่องยืนยันเปิดอยู่ — แบบเดียวกับ NastoKeyence (DimOverlayForm)
/// <para>
/// เป็นหน้าต่างเปล่าสีทึบโปร่งแสง ไม่มี control ข้างใน ไม่ใช่หน้าจอของโปรแกรม
/// การกันไม่ให้กดหน้าต่างหลักมาจากกล่องที่เปิดแบบ modal ทับอยู่ ไม่ใช่จากชั้นนี้
/// </para>
/// </summary>
internal sealed class DimOverlay : Form
{
    private DimOverlay(Form owner)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = DesignTokens.DarkNavy;
        Opacity = 0.45;
        ControlBox = false;
        TabStop = false;
        Bounds = owner.Bounds;
    }

    /// <summary>ไม่แย่งโฟกัสจากกล่องที่กำลังจะเปิด</summary>
    protected override bool ShowWithoutActivation => true;

    /// <summary>แสดงชั้นมืดทับ owner แล้วคืนตัวที่ Dispose แล้วปิดให้ — เปิดไม่ได้ก็ไม่เป็นไร (null)</summary>
    public static IDisposable? ShowOver(Form owner)
    {
        if (owner.IsDisposed || !owner.Visible || owner.WindowState == FormWindowState.Minimized) return null;
        try
        {
            var overlay = new DimOverlay(owner);
            overlay.Show(owner);
            return overlay;
        }
        catch
        {
            // พื้นหลังมืดเป็นของแต่ง เปิดไม่ได้ก็ยังถามได้ตามปกติ
            return null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed && Visible) Hide();
        base.Dispose(disposing);
    }
}
