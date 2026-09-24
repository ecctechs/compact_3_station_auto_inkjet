import json, subprocess
from pathlib import Path
root=Path(__file__).resolve().parents[2]
changes=json.loads(Path(__file__).with_name('station-comment-changes.json').read_text(encoding='utf-8'))
for file in sorted({c['file'] for c in changes}):
    data=(root/file).read_bytes().decode('utf-8-sig').splitlines(keepends=True)
    for c in [c for c in changes if c['file']==file]:
        assert data[c['line']-1]==c['new'], (file,c['line'])
        data[c['line']-1]=c['old']
    original=subprocess.check_output(['git','show','HEAD:'+file],cwd=root).decode('utf-8-sig')
    assert ''.join(data).replace('\r\n','\n')==original.replace('\r\n','\n'), file
    print(file+': only the recorded Thai comments changed')
print(f'Verified {len(changes)} comments across {len({c["file"] for c in changes})} files; no code changes.')
