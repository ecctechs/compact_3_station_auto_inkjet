using InkjetOperator.Views;

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

    private const string UvdxExtension = ".uvdx";

    /// <summary>
    /// เจอ 1 ไฟล์ → ใช้เลย · เจอหลายไฟล์ → ให้เลือก · ไม่เจอ → default
    /// ไม่รู้จักโฟลเดอร์ document → ส่งชื่อที่ได้มาตรงๆ ให้เครื่องตัดสินเอง
    /// </summary>
    /// <param name="uvNumber">เครื่องที่จะส่ง (1 / 2) — ใช้บอกชื่อเครื่องบนกล่องเลือกรุ่นย่อย</param>
    public static UvProgramPick Resolve(string? programName, string? docFolder, IWin32Window? owner = null, int? uvNumber = null)
        => Choose(FindVariants(programName, docFolder), owner, uvNumber);

    /// <summary>
    /// เหมือน <see cref="Resolve"/> แต่ส่วนที่แตะแชร์ของเครื่อง UV (หาโฟลเดอร์ document,
    /// ไล่รายชื่อไฟล์ .uvdx, อ่านรายชื่อรูป) ทำบนเธรดพื้นหลังทั้งหมด
    /// เหลือแค่หน้าจอเลือกรุ่นย่อยที่กลับมาเปิดบนเธรดหน้าจอ — ผลที่ได้เหมือนเดิมทุกประการ
    /// </summary>
    public static async Task<UvProgramPick> ResolveAsync(string? programName, int uvNumber, IWin32Window? owner = null)
    {
        var docFolder = await UvSettingsManager.GetDocumentFolderAsync(uvNumber);
        return await ResolveAsync(programName, docFolder, owner, uvNumber);
    }

    /// <inheritdoc cref="ResolveAsync(string?, int, IWin32Window?)"/>
    public static async Task<UvProgramPick> ResolveAsync(string? programName, string? docFolder, IWin32Window? owner = null, int? uvNumber = null)
    {
        var found = await Task.Run(() => FindVariants(programName, docFolder));

        // หน้าจอเลือกรุ่นย่อยค้นรูปรุ่นละครั้ง อ่านรายชื่อรูปเตรียมไว้ก่อน หน้าจอจะเปิดทันที
        if (found.Candidates is { Count: > 1 }) await MarkingRefImageService.WarmAsync();

        return Choose(found, owner, uvNumber);
    }

    /// <summary>ผลการไล่ไฟล์ — Candidates = null คือไม่รู้จักโฟลเดอร์ ส่งชื่อที่ได้มาตรง ๆ</summary>
    /// <summary>
    /// โปรแกรมนี้มีให้เลือกกี่ไฟล์ในเครื่อง — ไม่เปิดกล่องอะไร
    /// · null = ไม่รู้ (เครื่องนี้ไม่ได้ตั้งโฟลเดอร์ / ชื่อโปรแกรมว่าง / อ่านโฟลเดอร์ไม่ได้)
    /// · 0 = ไม่พบ (ตอนส่งจะถามว่าใช้ default ไหม)
    /// </summary>
    public static async Task<int?> VariantCountAsync(string? programName, int uvNumber)
    {
        var docFolder = await UvSettingsManager.GetDocumentFolderAsync(uvNumber);
        var found = await Task.Run(() => FindVariants(programName, docFolder));
        return found.BaseName.Length == 0 ? null : found.Candidates?.Count;
    }

    /// <summary>
    /// รายชื่อรุ่นย่อยทั้งหมดของโปรแกรมนี้ในโฟลเดอร์ที่ให้มา — ว่าง = ไม่มีหรืออ่านไม่ได้
    ///
    /// <para>แตะดิสก์ / แชร์ จึงทำบนเธรดพื้นหลัง เหมือน <see cref="VariantCountAsync"/></para>
    /// </summary>
    public static async Task<List<string>> VariantsAsync(string? programName, string? docFolder)
    {
        var found = await Task.Run(() => FindVariants(programName, docFolder));
        return found.Candidates ?? [];
    }

    private sealed record Variants(string BaseName, List<string>? Candidates);

    /// <summary>ส่วนที่แตะดิสก์ / แชร์ — ไม่มีหน้าจอ เรียกจากเธรดไหนก็ได้</summary>
    private static Variants FindVariants(string? programName, string? docFolder)
    {
        var baseName = (programName ?? "").Trim();
        if (baseName.Length == 0) return new Variants("", null);

        // ชื่อโปรแกรมมีจุดอยู่ในตัวได้ เช่น "AKBONO.Lot664" — ห้ามใช้
        // GetFileNameWithoutExtension เพราะมันมองว่า ".Lot664" เป็นนามสกุลแล้วตัดทิ้ง
        // เหลือ "AKBONO" ซึ่งไปหาไฟล์ไม่เจอแล้วตกไปใช้ default.uvdx
        //
        // ตัดเฉพาะ ".uvdx" ท้ายชื่อ เผื่อค่าที่เก็บมาติดนามสกุลมาด้วย
        if (baseName.EndsWith(UvdxExtension, StringComparison.OrdinalIgnoreCase))
            baseName = baseName[..^UvdxExtension.Length];
        if (docFolder == null) return new Variants(baseName, null);

        try
        {
            var candidates = Directory.GetFiles(docFolder, "*.uvdx")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(f => !string.IsNullOrEmpty(f))
                .Where(f => f! == baseName ||
                            f!.StartsWith(baseName + "-", StringComparison.Ordinal))
                .OrderBy(f => f)
                .ToList()!;
            return new Variants(baseName, candidates!);
        }
        catch
        {
            return new Variants(baseName, null);
        }
    }

    /// <summary>
    /// เจอ 1 ไฟล์ → ใช้เลย · เจอหลายไฟล์ → ให้เลือก · ไม่เจอ → default
    /// ไม่รู้จักโฟลเดอร์ document → ส่งชื่อที่ได้มาตรงๆ ให้เครื่องตัดสินเอง
    /// </summary>
    private static UvProgramPick Choose(Variants found, IWin32Window? owner, int? uvNumber)
    {
        if (found.BaseName.Length == 0) return new UvProgramPick(null, false);
        if (found.Candidates == null) return new UvProgramPick(found.BaseName, false);

        var candidates = found.Candidates;
        if (candidates.Count == 1) return new UvProgramPick(candidates[0], false);
        if (candidates.Count > 1) return new UvProgramPick(PromptVariant(candidates, owner, uvNumber), false);

        return new UvProgramPick(DefaultProgram, true);
    }

    /// <summary>
    /// ยืนยันก่อนใช้ default — ไม่ให้พิมพ์ผิดแบบโดยไม่รู้ตัว
    ///
    /// ใช้กล่องของ AntdUI ชุดเดียวกับที่ยืนยันเรื่องอื่นทั้งระบบ ไม่ใช่กล่องของ
    /// Windows ซึ่งตัวหนังสือเล็กและหน้าตาคนละแบบกับทุกหน้าในโปรแกรม
    /// </summary>
    public static bool ConfirmDefault(string requestedProgram, string machineName, Control? owner = null)
    {
        var requested = string.IsNullOrWhiteSpace(requestedProgram) ? "-" : requestedProgram.Trim();

        return Views.Confirm.Ask(owner, new Views.ConfirmSpec
        {
            Title = "ไม่พบโปรแกรม UV",
            Headline = "ใช้โปรแกรม default แทนไปก่อนไหม?",
            Facts = [("เครื่อง", machineName), ("ไม่พบ", requested), ("จะใช้", "default.uvdx")],
            Detail = "แจ้งผู้ดูแลให้เพิ่มโปรแกรมนี้เข้าเครื่อง",
            OkText = "ใช้ default",
            Icon = "FileExclamationOutlined",
        });
    }

    /// <summary>
    /// ให้ผู้ใช้เลือกรุ่นย่อยพร้อมเห็นรูปของแต่ละรุ่น
    ///
    /// ฝั่งนี้เลือกแล้ว "เปลี่ยนสิ่งที่พิมพ์จริง" ข้อความจึงต้องสื่อแบบนั้น
    /// ต่างจากฝั่ง MK ที่เลือกแล้วเปลี่ยนแค่รูปที่ดู
    /// </summary>
    /// <remarks>
    /// บอกชื่อเครื่องทั้งบนหัวกล่องและในบรรทัดคำอธิบาย — งาน 11 ต้องเลือกทั้ง UV1 และ UV2
    /// กล่องจึงเด้งสองครั้งหน้าตาเหมือนกัน ถ้าไม่บอกเครื่อง พนักงานแยกไม่ออกว่ากำลังเลือกของด้านไหน
    /// </remarks>
    /// <summary>
    /// เปิดกล่องเลือกรุ่นจากรายการที่ผู้เรียกกรองมาแล้ว — null = ไม่มีให้เลือกหรือคนกดยกเลิก
    ///
    /// <para>
    /// ใช้ตอนพิมพ์รุ่นย่อยเพิ่มให้งานใบเดิม ซึ่งต้องตัดรุ่นที่พิมพ์ไปแล้วออกก่อน
    /// เหลือรุ่นเดียวไม่ต้องถามให้เสียเวลา กล่องยืนยันก่อนส่งยังถามอยู่ดี
    /// </para>
    /// </summary>
    public static string? PickFrom(List<string> variants, IWin32Window? owner, int? uvNumber) =>
        variants.Count switch
        {
            0 => null,
            1 => variants[0],
            _ => PromptVariant(variants, owner, uvNumber),
        };

    /// <summary>
    /// เลือกรุ่นถัดไปตอนกด "เปลี่ยนรุ่น" — กล่องเดียวกับตอนเลือกรุ่นแรก แต่คำบอกว่ากำลังเปลี่ยนรุ่น
    /// · null = ยกเลิก
    ///
    /// <para>
    /// แสดงทุกรุ่นให้เห็นภาพรวม รุ่นที่อยู่ในเครื่องตอนนี้กับรุ่นที่พิมพ์ไปแล้วมีป้ายกำกับ
    /// และเลือกไม่ได้ (ปุ่มกดไม่ได้) · ขึ้นกล่องเสมอแม้เหลือรุ่นเดียว คนกดต้องเห็นว่าจะได้รุ่นไหน
    /// กล่องนี้คือการยืนยันในตัว ปุ่มบอกการกระทำตรง ๆ
    /// </para>
    /// </summary>
    public static string? PickNextVariant(
        List<string> variants, string? current, IReadOnlySet<string> printed,
        IWin32Window? owner, int? uvNumber)
    {
        var options = variants.Select(v =>
        {
            bool isCurrent = string.Equals(v, current, StringComparison.OrdinalIgnoreCase);
            bool isPrinted = printed.Contains(v);
            var label = v + ".uvdx" + (isCurrent ? "   · อยู่ในเครื่องตอนนี้" : isPrinted ? "   · พิมพ์แล้ว" : "");
            return new MarkingRefOption(v, label, MarkingRefImageService.FindImagesExact(v), isCurrent || isPrinted);
        }).ToList();

        var machine = MachineLabel(uvNumber);
        return MarkingRefPickerDialog.Pick(
            owner,
            machine == null ? "เปลี่ยนรุ่น" : $"เปลี่ยนรุ่น — {machine}",
            "รุ่นเดิมพิมพ์เสร็จแล้ว? เลือกรุ่นถัดไป — กดแล้วเครื่องโหลดรุ่นใหม่ทันที · รุ่นที่พิมพ์แล้วเลือกซ้ำไม่ได้",
            options,
            okText: "พิมพ์รุ่นนี้");
    }

    private static string? PromptVariant(List<string> variants, IWin32Window? owner, int? uvNumber)
    {
        var options = variants
            .Select(v => new MarkingRefOption(v, v + ".uvdx", MarkingRefImageService.FindImagesExact(v)))
            .ToList();

        var machine = MachineLabel(uvNumber);
        return MarkingRefPickerDialog.Pick(
            owner,
            machine == null ? "เลือกโปรแกรมที่จะพิมพ์" : $"เลือกโปรแกรมที่จะพิมพ์ — {machine}",
            machine == null
                ? "โปรแกรมนี้มีหลายรุ่นย่อยในเครื่อง — เลือกรุ่นที่จะโหลดเข้าเครื่องเพื่อพิมพ์จริง"
                : $"{machine}: โปรแกรมนี้มีหลายรุ่นย่อยในเครื่อง — เลือกรุ่นที่จะโหลดเข้าเครื่องเพื่อพิมพ์จริง",
            options);
    }

    /// <summary>ชื่อเครื่องตามที่ตั้งไว้ พร้อมด้านที่พิมพ์ เช่น "UV-001 (Plate)" — ไม่รู้เครื่องคืน null</summary>
    private static string? MachineLabel(int? uvNumber) => uvNumber switch
    {
        1 => $"{UvSettingsManager.Read("UV1_NAME", "UV-001")} (Plate)",
        2 => $"{UvSettingsManager.Read("UV2_NAME", "UV-002")} (Shim)",
        _ => null,
    };
}
