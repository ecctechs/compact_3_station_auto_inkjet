from pathlib import Path
import zipfile, re, xml.etree.ElementTree as E

folder = Path(__file__).parent
source = folder / 'Compact_Inkjet_IT_Test.xlsx'
target = folder / 'Compact_Inkjet_IT_Test_fixed.xlsx'
ns = {'s':'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
with zipfile.ZipFile(source) as z:
    original = {name:z.read(name) for name in z.namelist()}
fixed = dict(original)
for i in range(1,4):
    name = f'xl/worksheets/sheet{i}.xml'
    text = original[name].decode('utf-8')
    matches = list(re.finditer(r'<pageMargins\b[^>]*/>', text))
    assert len(matches) == 2, f'Expected duplicate print margins in sheet {i}'
    # Remove only the older margin element preceding printOptions.
    old = matches[0]
    text = text[:old.start()]+text[old.end():]
    fixed[name] = text.encode('utf-8')
    before = E.fromstring(original[name])
    after = E.fromstring(fixed[name])
    assert len(after.findall('s:pageMargins',ns)) == 1
    children = [e.tag.rsplit('}',1)[-1] for e in after]
    assert children.index('printOptions') < children.index('pageMargins') < children.index('pageSetup')
    # All cell contents and sheet functionality remain byte-for-byte equivalent as parsed.
    for tag in ['sheetData','sheetViews','conditionalFormatting','dataValidations','tableParts','pageSetup']:
        assert [E.tostring(e) for e in before.findall('s:'+tag,ns)] == [E.tostring(e) for e in after.findall('s:'+tag,ns)]
    print(f'Sheet {i}: removed duplicate pageMargins only; cells and controls unchanged')
for name in fixed:
    if name not in [f'xl/worksheets/sheet{i}.xml' for i in range(1,4)]:
        assert fixed[name] == original[name]
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED) as z:
    for name,data in fixed.items(): z.writestr(name,data)
print('Fixed workbook created')
