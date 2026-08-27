namespace InkjetOperator.Services;

/// <summary>ชื่อโปรแกรมของเครื่อง UV หนึ่งเครื่อง เท่าที่รู้ ณ ตอนนั้น</summary>
/// <param name="Program">ชื่อที่จะเอาไปหารูป — null = ยังไม่รู้</param>
/// <param name="Confirmed">
/// true = เป็นรุ่นย่อยที่ผู้ใช้เลือกและส่งเข้าเครื่องไปแล้ว
/// false = เป็นชื่อฐานจากข้อมูลงาน ยังไม่ผ่านการเลือก
/// </param>
public sealed record UvProgramInfo(string? Program, bool Confirmed);

/// <summary>รูปอ้างอิงของด้านหนึ่ง (Plate หรือ Shim) ของงาน</summary>
/// <param name="Side">ชื่อด้าน — "Plate" หรือ "Shim"</param>
/// <param name="Machine">ชื่อเครื่องที่จะแสดงบนป้าย</param>
/// <param name="LookupName">ชื่อที่ใช้ค้นรูป — null = ประกอบชื่อไม่ได้</param>
/// <param name="Images">path ของรูปที่พบ เรียงตามชื่อไฟล์</param>
/// <param name="Pending">
/// true = ฝั่ง UV ที่ยังไม่ได้เลือกรุ่นย่อย และมีให้เลือกมากกว่า 1 แบบ
/// กรณีนี้ห้ามหยิบรูปใบใดใบหนึ่งมาโชว์ เพราะจะดูเหมือนระบบเลือกไว้แล้ว
/// </param>
public sealed record MarkingRefSide(
    string Side,
    string Machine,
    string? LookupName,
    List<string> Images,
    bool Pending);

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
    /// <param name="uv1">โปรแกรมของ UV1 (ด้าน Plate)</param>
    /// <param name="uv2">โปรแกรมของ UV2 (ด้าน Shim)</param>
    public static List<MarkingRefSide> Resolve(
        string? markingMethod, string? erpMfg, UvProgramInfo? uv1, UvProgramInfo? uv2)
    {
        var plan = MarkingMethodService.Resolve(markingMethod);
        var sides = new List<MarkingRefSide>();
        if (plan.NoCase) return sides;

        Add(sides, "Plate", plan.Plate, erpMfg, "P-", uv1, uv2);
        Add(sides, "Shim", plan.Shim, erpMfg, "S-", uv1, uv2);
        return sides;
    }

    private static void Add(
        List<MarkingRefSide> sides, string side, MarkingMachine machine,
        string? erpMfg, string erpPrefix, UvProgramInfo? uv1, UvProgramInfo? uv2)
    {
        if (machine == MarkingMachine.None) return;

        string label;
        string? lookup;
        bool confirmed = true;

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
                lookup = Clean(uv1?.Program);
                confirmed = uv1?.Confirmed ?? false;
                break;

            default:
                label = UvSettingsManager.Read("UV2_NAME", "UV-002");
                lookup = Clean(uv2?.Program);
                confirmed = uv2?.Confirmed ?? false;
                break;
        }

        var images = lookup == null
            ? new List<string>()
            : MarkingRefImageService.FindImages(lookup);

        // ยังไม่ได้เลือกรุ่นและมีให้เลือกหลายแบบ = ยังตอบไม่ได้ว่าจะพิมพ์ใบไหน
        // มีใบเดียวก็ไม่มีอะไรให้เลือก โชว์ได้เลย
        bool pending = !confirmed && images.Count > 1;

        sides.Add(new MarkingRefSide(side, label, lookup, images, pending));
    }

    private static string? Clean(string? name)
    {
        var value = (name ?? "").Trim();
        return value.Length == 0 ? null : value;
    }
}
