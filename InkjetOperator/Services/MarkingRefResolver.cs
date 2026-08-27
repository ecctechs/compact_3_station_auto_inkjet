namespace InkjetOperator.Services;

/// <summary>รูปอ้างอิงของด้านหนึ่ง (Plate หรือ Shim) ของงาน</summary>
/// <param name="Side">ชื่อด้าน — "Plate" หรือ "Shim"</param>
/// <param name="Machine">ชื่อเครื่องที่จะแสดงบนป้าย</param>
/// <param name="LookupName">ชื่อที่ใช้ค้นรูป — null = ประกอบชื่อไม่ได้</param>
/// <param name="Images">path ของรูปที่พบ เรียงตามชื่อไฟล์</param>
public sealed record MarkingRefSide(
    string Side,
    string Machine,
    string? LookupName,
    List<string> Images);

/// <summary>
/// รวมกฎว่างานหนึ่งต้องโชว์รูปของด้านไหน และรูปนั้นมาจากชื่ออะไร
///
/// ฝั่ง MK ประกอบชื่อจาก ERP: Plate = "P-{ERP}" · Shim = "S-{ERP}"
/// ฝั่ง UV ใช้ชื่อโปรแกรมในเครื่อง ซึ่งรู้ชื่อจริงหลังผู้ใช้เลือกรุ่นย่อยแล้ว
///
/// ป้ายเครื่องฝั่ง UV อ่านชื่อจริงจาก setting ได้ แต่ฝั่ง MK เขียนได้แค่ "MK"
/// เพราะ marking_method ไม่ได้บอกว่าเป็น MK-058 หรือ MK-059
/// </summary>
public static class MarkingRefResolver
{
    public const string MkMachineLabel = "MK";

    /// <summary>
    /// คืนเฉพาะด้านที่งานนี้ทำจริง — ด้านที่เป็น 0 หรือรหัสที่ไม่มีจริง (21) จะไม่มีในผลลัพธ์
    /// </summary>
    /// <param name="markingMethod">plan_routing.marking_method</param>
    /// <param name="erpMfg">plan_routing.erp_mfg — ใช้ประกอบชื่อรูปฝั่ง MK</param>
    /// <param name="uv1Program">ชื่อโปรแกรมของ UV1 (ด้าน Plate) เท่าที่รู้ ณ ตอนนั้น</param>
    /// <param name="uv2Program">ชื่อโปรแกรมของ UV2 (ด้าน Shim) เท่าที่รู้ ณ ตอนนั้น</param>
    public static List<MarkingRefSide> Resolve(
        string? markingMethod, string? erpMfg, string? uv1Program, string? uv2Program)
    {
        var plan = MarkingMethodService.Resolve(markingMethod);
        var sides = new List<MarkingRefSide>();
        if (plan.NoCase) return sides;

        Add(sides, "Plate", plan.Plate, erpMfg, "P-", uv1Program, uv2Program);
        Add(sides, "Shim", plan.Shim, erpMfg, "S-", uv1Program, uv2Program);
        return sides;
    }

    private static void Add(
        List<MarkingRefSide> sides, string side, MarkingMachine machine,
        string? erpMfg, string erpPrefix, string? uv1Program, string? uv2Program)
    {
        if (machine == MarkingMachine.None) return;

        string label;
        string? lookup;

        switch (machine)
        {
            case MarkingMachine.Mk:
                label = MkMachineLabel;
                // ไม่มี ERP ก็ประกอบชื่อไม่ได้ — ยังต้องโชว์ช่องไว้ให้เห็นว่าด้านนี้มีงาน
                var erp = (erpMfg ?? "").Trim();
                lookup = erp.Length == 0 ? null : erpPrefix + erp;
                break;

            case MarkingMachine.Uv1:
                label = UvSettingsManager.Read("UV1_NAME", "UV-001");
                lookup = Clean(uv1Program);
                break;

            default:
                label = UvSettingsManager.Read("UV2_NAME", "UV-002");
                lookup = Clean(uv2Program);
                break;
        }

        var images = lookup == null
            ? new List<string>()
            : MarkingRefImageService.FindImages(lookup);

        sides.Add(new MarkingRefSide(side, label, lookup, images));
    }

    private static string? Clean(string? name)
    {
        var value = (name ?? "").Trim();
        return value.Length == 0 ? null : value;
    }
}
