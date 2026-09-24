from pathlib import Path
import json, subprocess
ROOT=Path(__file__).resolve().parents[2]
manifest=Path(__file__).with_name('comment-changes.json')
changes=json.loads(manifest.read_text(encoding='utf-8'))
# ปรับขอบเขตที่ท้ายเมธอด โดยรักษาเลขบรรทัดและข้อความโค้ดเดิม
extra=next(x for x in changes if x['file']=='InkjetBackend/controllers/JobController.js' and x['line']==130)
p=ROOT/extra['file']
raw=p.read_bytes()
old=(extra['before']+' // '+extra['comment']).encode('utf-8')
assert raw.count(old)>=1
lines=raw.splitlines(keepends=True)
lines[129]=lines[129].replace(old,extra['before'].encode('utf-8'))
p.write_bytes(b''.join(lines))
changes.remove(extra)
p=ROOT/'InkjetOperator/Views/ScanBarcodeUserControl.cs'
lines=p.read_bytes().splitlines(keepends=True)
old=lines[458].decode('utf-8').rstrip('\r\n')
assert old.strip()=='Notify.ErrorModal(null, "Error", msg);'
comment='เปิดกล่องข้อผิดพลาด'
lines[458]=lines[458].replace(old.encode('utf-8'),(old+' // '+comment).encode('utf-8'))
p.write_bytes(b''.join(lines))
changes.append({'file':'InkjetOperator/Views/ScanBarcodeUserControl.cs','line':459,'before':old,'comment':comment})
manifest.write_text(json.dumps(changes,ensure_ascii=False,indent=2),encoding='utf-8')

files=sorted(set(c['file'] for c in changes))
for file in files:
    actual=(ROOT/file).read_text(encoding='utf-8-sig').splitlines()
    original=subprocess.check_output(['git','show','HEAD:'+file],cwd=ROOT).decode('utf-8-sig').splitlines()
    assert len(actual)==len(original),f'Line count changed: {file}'
    for c in [x for x in changes if x['file']==file]:
        i=c['line']-1
        assert actual[i]==c['before']+' // '+c['comment'],f'Unexpected edit: {file}:{i+1}'
        actual[i]=c['before']
    assert actual==original, f'Non-comment change: {file}'
print(f'PASS: {len(changes)} inline comments in {len(files)} files. Original code and all line numbers unchanged.')
