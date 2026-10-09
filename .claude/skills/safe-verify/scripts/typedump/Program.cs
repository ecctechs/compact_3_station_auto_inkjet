using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

// วิธีใช้: dotnet run --project .claude/skills/safe-verify/scripts/typedump -- <path ของ .dll>
//
// อ่าน metadata ของ .dll ตรง ๆ — ไม่ต้องโหลด WinForms / AntdUI ที่ dll นั้นอ้างถึง
// ผลเรียงตามตัวอักษรแล้ว จึง diff สองไฟล์ได้ทันที
if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("usage: typedump <path to .dll>");
    return 1;
}

using var stream = File.OpenRead(args[0]);
using var pe = new PEReader(stream);
var md = pe.GetMetadataReader();

var lines = new List<string>();

foreach (var handle in md.TypeDefinitions)
{
    var type = md.GetTypeDefinition(handle);
    var name = $"{md.GetString(type.Namespace)}.{md.GetString(type.Name)}";
    var methods = type.GetMethods()
        .Select(m => md.GetString(md.GetMethodDefinition(m).Name))
        .OrderBy(m => m, StringComparer.Ordinal);
    lines.Add($"{name} | {string.Join(",", methods)}");
}

// resource ของฟอร์ม (.resx) หายไปพร้อมคลาส — นับไว้ด้วยจะได้เห็นครบ
foreach (var handle in md.ManifestResources)
    lines.Add("RES " + md.GetString(md.GetManifestResource(handle).Name));

foreach (var line in lines.OrderBy(l => l, StringComparer.Ordinal))
    Console.WriteLine(line);

return 0;
