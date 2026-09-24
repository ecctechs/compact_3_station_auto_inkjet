"""Set print options not exposed by Artifact Tool, then verify the exported XLSX."""
from pathlib import Path
import json, re, zipfile, xml.etree.ElementTree as ET

base = Path(__file__).parent
path = base / 'Compact_Inkjet_IT_Test.xlsx'
manifest = json.loads((base/'manifest.json').read_text(encoding='utf-8'))
with zipfile.ZipFile(path) as archive:
    files = {n: archive.read(n) for n in archive.namelist()}
ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
names = []
for i, item in enumerate(manifest, 1):
    key = f'xl/worksheets/sheet{i}.xml'
    xml = files[key].decode('utf-8').replace('<x:', '<').replace('</x:', '</').replace('xmlns:x=', 'xmlns=')
    # Maintain the existing spreadsheet namespaces and element ordering.
    if '<sheetPr' in xml:
        xml = xml.replace('</sheetPr>', '<pageSetUpPr fitToPage="1"/></sheetPr>', 1)
    else:
        xml = re.sub(r'(<worksheet\b[^>]*>)', r'\1<sheetPr><pageSetUpPr fitToPage="1"/></sheetPr>', xml, count=1)
    xml = re.sub(r'(<sheetView\b[^>]*)(>)', r'\1 zoomScale="85"\2', xml, count=1) if 'zoomScale=' not in xml else xml
    # Export already includes pageMargins. Replace existing print elements, never duplicate.
    xml = re.sub(r'<(?:printOptions|pageMargins|pageSetup)\b[^>]*/>', '', xml)
    page = '<printOptions horizontalCentered="1"/><pageMargins left="0.25" right="0.25" top="0.35" bottom="0.35" header="0.15" footer="0.15"/><pageSetup paperSize="8" orientation="landscape" fitToWidth="1" fitToHeight="0"/>'
    pos = re.search(r'<(?:drawing|legacyDrawing|tableParts|extLst)\b', xml)
    anchor = pos.start() if pos else xml.index('</worksheet>')
    xml = xml[:anchor]+page+xml[anchor:]
    files[key] = xml.encode('utf-8')
    quoted = "'"+item['sheet'].replace("'", "''")+"'"
    names.append(f'<definedName name="_xlnm.Print_Area" localSheetId="{i-1}">{quoted}!$A$1:$G${item["last"]}</definedName>')
    names.append(f'<definedName name="_xlnm.Print_Titles" localSheetId="{i-1}">{quoted}!$10:$10</definedName>')

w = files['xl/workbook.xml'].decode('utf-8').replace('<x:', '<').replace('</x:', '</').replace('xmlns:x=', 'xmlns=')
if '</definedNames>' in w:
    w = w.replace('</definedNames>', ''.join(names)+'</definedNames>')
else:
    at = w.find('<calcPr')
    if at < 0: at = w.index('</workbook>')
    w = w[:at]+'<definedNames>'+''.join(names)+'</definedNames>'+w[at:]
files['xl/workbook.xml'] = w.encode('utf-8')
with zipfile.ZipFile(path, 'w', zipfile.ZIP_DEFLATED) as archive:
    for name, data in files.items(): archive.writestr(name, data)

shared = ET.fromstring(files['xl/sharedStrings.xml']) if 'xl/sharedStrings.xml' in files else None
strings = [''.join(si.itertext()) for si in shared] if shared is not None else []
def cell_text(c):
    if c is None: return ''
    value = c.find('s:v', ns)
    if c.get('t') == 's': return strings[int(value.text)]
    if c.get('t') == 'inlineStr': return ''.join(c.find('s:is',ns).itertext())
    return value.text if value is not None else ''

ids = set()
for i, item in enumerate(manifest, 1):
    root = ET.fromstring(files[f'xl/worksheets/sheet{i}.xml'])
    cells = {c.get('r'): c for c in root.findall('.//s:sheetData/s:row/s:c', ns)}
    for row in range(item['start'],item['end']+1):
        test_id = cell_text(cells.get(f'A{row}'))
        assert test_id and test_id not in ids
        ids.add(test_id)
        assert all(cell_text(cells.get(f'{col}{row}')) for col in 'BCDE')
        assert cell_text(cells.get(f'F{row}')) == '☐ ยังไม่ทดสอบ'
        assert not cell_text(cells.get(f'G{row}'))
    assert len(root.findall('.//s:f',ns)) == 6
    assert not root.findall('.//s:c[@t="e"]',ns)
    pane = root.find('.//s:pane',ns)
    assert pane.get('xSplit') == '2' and pane.get('ySplit') == '10'
    validation = root.find('.//s:dataValidation',ns)
    assert validation.get('sqref') == f'F{item["start"]}:F{item["end"]}'
    assert '☑ ผ่าน' in validation.find('s:formula1',ns).text
    assert '☒ ไม่ผ่าน' in validation.find('s:formula1',ns).text
    assert len(root.findall('.//s:cfRule',ns)) == 4
    print(f'Sheet {i}: {item["count"]} cases, blank results, validation, formulas, filters, freeze panes and print setup verified')
assert len(ids)==61
ET.fromstring(files['xl/workbook.xml'])
print(f'Final workbook: {len(ids)} test cases; {path.stat().st_size} bytes')
