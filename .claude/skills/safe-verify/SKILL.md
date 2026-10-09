---
name: safe-verify
description: ตรวจว่าการแก้โค้ด InkjetOperator / InkjetBackend ไม่ทำให้อะไรพัง โดยไม่แตะไฟล์ตั้งค่าตัวจริงและไม่ต่อเครื่องจริง (MK, UV, PLC, backend หน้างาน) ใช้หลังแก้ Services / Views / Backend และก่อน commit หรือเมื่อผู้ใช้ถามว่า "พังไหม" "ตรวจให้หน่อย" "ยังทำงานเหมือนเดิมไหม" ตรวจและรายงานอย่างเดียว ไม่แก้โค้ด
argument-hint: "[ไฟล์หรือเรื่องที่เพิ่งแก้ — เว้นว่าง = ทุกอย่างที่ยังไม่ commit]"
allowed-tools: Bash, PowerShell, Read, Grep, Glob
---

# Safe Verify

ยืนยันผลการแก้ด้วย build และเทสที่ปลอดภัย แล้วรายงานตามจริง

ขอบเขต: $ARGUMENTS
(ถ้าว่าง = ดูจาก `git status` / `git diff` ว่าแก้อะไรไปบ้าง)

## ข้อห้ามเด็ดขาด

โปรแกรมนี้คุยกับเครื่องจักรจริงและไฟล์ตั้งค่าตัวจริงของทั้งเครื่อง ผิดครั้งเดียวกระทบหน้างาน

- **ห้ามเขียน `C:\ProgramData\CompactInkjet\*`** (`Setting.config`, `uv.config`, `patterns.xml`)
  - ห้ามเรียก `CustomSettingsManager.Write`, `UvSettingsManager`, `PatternStore.Save`, `St1DefaultSettings.ApplyIfSt1` จากเทสหรือ probe
  - ถ้าจำเป็นต้องใช้ค่าตั้ง ให้เปลี่ยน `_store._path` ไปที่ไฟล์ปลอมใน scratchpad ก่อน และตั้ง `AppSettingsFile._accessChecked = true` กันไม่ให้ไปแก้สิทธิ์โฟลเดอร์จริง
- **ห้ามต่อเครื่องจริงหรือ backend จริง**
  - ห้าม 10.10.100.x, 192.168.1.1, IP ที่อ่านได้จาก Setting.config
  - ห้าม backend port 3000 และฐาน PostgreSQL หน้างาน
  - ใช้ได้แค่ `127.0.0.1` (listener ปลอม), `192.0.2.x` (TEST-NET ไม่มีเครื่องจริง), `http://test.invalid`
- **ห้ามแก้โค้ดหรือแก้เทสเพื่อให้ผ่าน** — เจอปัญหาให้รายงาน ผู้ใช้ตัดสินใจเอง
- **ห้าม commit / push**
- **ห้ามรัน devenv** ถ้า Visual Studio เปิดอยู่ (`dotnet build` ใช้ได้)

## ขั้นตอน

### 1. ดูว่าแก้อะไร

```bash
git status --short
git diff --stat
```

จัดกลุ่มไฟล์ที่แก้: `InkjetOperator/Services`, `InkjetOperator/Views`, `InkjetBackend`, `tools/`, เอกสารล้วน
ถ้าแก้แต่เอกสาร / รูป / README → รายงานว่าไม่ต้อง build แล้วจบ

### 2. Build

- ถ้า process `InkjetOperator` เปิดอยู่ ไฟล์ exe ใน `bin` จะถูกล็อก → build ลงโฟลเดอร์แยกใน scratchpad

```bash
dotnet build CompactInkjet.sln
# หรือถ้าโปรแกรมเปิดอยู่
dotnet build InkjetOperator/InkjetOperator.csproj -o <scratchpad>/verify-build
```

- `warning MSB4078` ของ `CompactDemo.vdproj` เป็นเรื่องปกติ ไม่ต้องรายงานเป็นปัญหา
- แก้ backend: ตรวจ syntax ด้วย `node --check <ไฟล์ที่แก้>` (ไม่ต้องเปิด server)

### 3. เลือกเทสจาก [test-map.md](test-map.md)

ดูว่าไฟล์ที่แก้ตรงกับเทสไหน รันเฉพาะเทสที่ test-map บอกว่า **ปลอดภัย**
เทสที่ต้องใช้ API / DB ทดสอบ: รันก็ต่อเมื่อผู้ใช้ตั้ง env ไว้แล้วและตรวจว่าเป็น loopback ไม่ใช่ port 3000 — ไม่งั้นข้ามและบอกว่าข้าม

### 4. เทียบกับ baseline

เทสบางตัวพังอยู่ก่อนแล้ว ดูแค่ผ่าน / ไม่ผ่านไม่พอ ต้องเทียบกับ baseline ใน test-map.md

- ผลเหมือน baseline = ไม่ได้ทำให้แย่ลง
- ผ่านเพิ่ม = ดีขึ้น
- ล้มที่ข้อใหม่ = การแก้นี้น่าจะเป็นสาเหตุ

ถ้าไม่แน่ใจว่า baseline ยังจริงไหม ให้ `git stash` ไม่ได้ (ห้ามแตะงานของผู้ใช้) — ใช้ `git worktree add <scratchpad>/base HEAD` แล้วรันเทสเดียวกันที่นั่นแทน เสร็จแล้ว `git worktree remove`

### 5. ถ้าเป็นการลบ / ย้าย / refactor โค้ด

เทียบคลาสและเมธอดใน `InkjetOperator.dll` ก่อนกับหลัง (ใช้ worktree ของ HEAD เป็น "ก่อน")

```bash
dotnet run --project .claude/skills/safe-verify/scripts/typedump -- <before>/InkjetOperator.dll > <scratchpad>/types_before.txt
dotnet run --project .claude/skills/safe-verify/scripts/typedump -- <after>/InkjetOperator.dll  > <scratchpad>/types_after.txt
diff <scratchpad>/types_before.txt <scratchpad>/types_after.txt
```

anonymous type (`<>f__AnonymousType`) เปลี่ยนเลขได้เป็นเรื่องปกติ ให้ดูชื่อคลาส / เมธอดจริงที่หายหรือเพิ่ม

### 6. เก็บกวาด

ลบ worktree และโฟลเดอร์ build ชั่วคราวใน scratchpad ที่สร้างขึ้น
`bin/` `obj/` ที่เทสสร้างใน `tests/*/` อยู่ใน .gitignore แล้ว ไม่ต้องลบ

## รูปแบบรายงาน

ตอบเป็นภาษาไทย สั้น ตรง

| รายการ | ผล |
|---|---|
| ไฟล์ที่แก้ | ... |
| Build | ผ่าน / ไม่ผ่าน (error, warning ที่ไม่ใช่ MSB4078) |
| เทสที่รัน | ชื่อ — ผล — เทียบ baseline |
| ล้มใหม่ | ไม่มี / รายการพร้อมข้อความ error |
| ไม่ได้ตรวจ | อะไร และเพราะอะไร (เช่น ต้องลองกับเครื่องจริงที่หน้างาน, ไม่มี DB ทดสอบ) |

ปิดท้ายด้วยสิ่งที่ผู้ใช้ควรลองเองที่หน้างาน ถ้าการแก้เกี่ยวกับเครื่องจริง
ถ้า baseline ใน test-map.md เปลี่ยนไปแล้ว (เช่น มีคนซ่อมเทส) ให้บอกผู้ใช้ว่าควรอัปเดต test-map — ไม่แก้เอง
