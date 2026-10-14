---
name: msi-check
description: ตรวจไฟล์ติดตั้ง CompactDemo-*.msi ของ Compact Inkjet ว่าพร้อมส่งลูกค้าหรือยัง — เลขเวอร์ชันตรงกับ exe ข้างใน, มีการเช็ค .NET 8 Desktop Runtime, shortcut ชี้ไปที่ exe, ติดตั้งให้ทุกบัญชี, UpgradeCode ไม่เปลี่ยน, ไฟล์ครบ ใช้ทุกครั้งที่ผู้ใช้พูดถึงไฟล์ .msi หรือตัวติดตั้ง เช่น "ตรวจ MSI" "ตัวติดตั้งนี้ส่งได้ไหม" "ลงเครื่องลูกค้าแล้วเปิดไม่ขึ้น" "ติดตั้งแล้วขึ้นสองตัว" "เวอร์ชันไม่ตรง" "check installer" แม้ผู้ใช้ไม่ได้พูดว่าตรวจ อ่านอย่างเดียว ไม่แก้ไฟล์ MSI
argument-hint: "[path ของไฟล์ .msi — เว้นว่าง = ตัวล่าสุดใน dist\\]"
allowed-tools: Bash, PowerShell, Read, Glob
---

# MSI Check

ตัวติดตั้งที่ผิดจะไปพังที่เครื่องลูกค้า ซึ่งแก้ยากกว่าในเครื่องเรามาก
อาการที่เคยเจอจริงในโปรเจกต์นี้: ลงได้แต่เปิดโปรแกรมไม่ขึ้นเพราะไม่มี .NET 8,
ห่อ exe ตัวเก่าไว้ในตัวติดตั้งใหม่, ลงซ้อนกันเป็นสองโปรแกรม, shortcut แบบ advertised ที่ซ่อมแซมทับไฟล์ตั้งค่า
skill นี้ตรวจของพวกนี้จากตัวไฟล์ .msi จริง ไม่ใช่จาก log ว่า build ผ่าน

ไฟล์ที่ผู้ใช้ระบุ: $ARGUMENTS

## ขั้นตอน

### 1. หาไฟล์

- ผู้ใช้ให้ path มา → ใช้ไฟล์นั้น
- ไม่ได้ให้ → ใช้ไฟล์ `CompactDemo-*.msi` ที่ใหม่ที่สุดใน `dist\` ของ repo แล้วบอกผู้ใช้ว่าตรวจไฟล์ไหน
- ไม่เจอเลย → บอกว่ายังไม่มีไฟล์ แนะนำให้รัน `build-installer.bat` ก่อน แล้วจบ

### 2. รัน script ตรวจ

```bash
powershell -NoProfile -ExecutionPolicy Bypass -File .claude/skills/msi-check/scripts/inspect-msi.ps1 "<path ของ .msi>" "<repo>\CompactDemo\CompactDemo.vdproj"
```

ส่ง path ของ `.vdproj` ไปด้วยเสมอ เพราะข้อ UPGRADE ต้องเทียบกับโปรเจค ถ้าไฟล์ MSI อยู่นอก repo script จะหาเองไม่เจอ

script อ่านอย่างเดียว (เปิดฐานข้อมูล MSI แบบ read-only) ใช้ผลจาก script เป็นหลัก
ไม่ต้องเขียน query เองซ้ำ — ถ้าอยากรู้รายละเอียดเพิ่มค่อย query ตารางเฉพาะที่สงสัย

ผลแต่ละข้อ:

| รหัส | ตรวจอะไร | ถ้า FAIL แปลว่า |
|---|---|---|
| VERSION | เลข exe ข้างในตรงกับ ProductVersion | ห่อ exe ผิดรุ่น — ลูกค้าได้โค้ดเก่า |
| NAME | ชื่อไฟล์ `CompactDemo-<เวอร์ชัน>-<commit>.msi` ตรงกับข้างใน | ไฟล์ถูกเปลี่ยนชื่อหรือแก้เลขทีหลัง |
| DOTNET | มี LaunchCondition + AppSearch + Signature + DrLocator ของ .NET 8 | ลงได้ แต่เปิดไม่ขึ้นในเครื่องที่ไม่มี runtime (มักเพราะ build จาก VS ตรง ๆ) |
| SHORTCUT | `DISABLEADVTSHORTCUTS=1` และ shortcut ชี้ไปที่ `InkjetOperator.exe` | shortcut แบบ advertised อาจเด้งซ่อมแซมกลางกะ |
| ICON | ไอคอน `app.ico` ใน shortcut และ Programs and Features | ใช้งานได้ แค่รูปไม่สวย (WARN) |
| SCOPE | `ALLUSERS` เป็น 1 หรือ 2 | ลงให้แค่คนที่กดติดตั้ง |
| UPGRADE | UpgradeCode ตรงกับ `.vdproj` | ลงซ้อนกับตัวเก่าเป็นสองโปรแกรม |
| FILES | มี exe, dll, runtimeconfig, AntdUI, ไฟล์ตั้งค่าตั้งต้น 3 ตัว | โปรแกรมเปิดไม่ขึ้นหรือไม่มีค่าเริ่มต้น |

### 3. สิ่งที่ไม่ใช่ปัญหา (อย่ารายงานเป็นข้อผิดพลาด)

- `FolderForm_AllUsers = ME` — ตัวเลือก Everyone / Just me ถูกซ่อนไว้ (`InstallAllUsersVisible = 0`) ค่านี้จึงไม่ถูกใช้ ค่าที่ใช้จริงคือ `ALLUSERS`
- ข้อความเช็ค .NET เป็นภาษาอังกฤษ — ตั้งใจ เพราะ MSI ของ VS ใช้ code page 1252 เก็บภาษาไทยไม่ได้
- `AntdUI.dll` อยู่ในตาราง File สองแถว — VS ใส่ทั้ง dependency ที่ตรวจเจอเองและไฟล์จาก bin ตามปกติ

### 4. ถ้าผู้ใช้เล่าอาการจากหน้างาน

จับคู่อาการกับข้อที่ FAIL แล้วบอกให้ชัดว่าอันนี้คือสาเหตุหรือไม่ใช่
ถ้าทุกข้อ PASS แต่ผู้ใช้ยังเจอปัญหา ให้บอกตรง ๆ ว่าตัวไฟล์ MSI ไม่ใช่สาเหตุ และชี้ทางที่ควรดูต่อ
(เช่น เครื่องลูกค้าลง runtime ผิดแบบ x86, ไฟล์ตั้งค่าใน `C:\ProgramData\CompactInkjet\`, สิทธิ์โฟลเดอร์)

### 5. วิธีแก้ที่แนะนำได้

- DOTNET / SHORTCUT / ICON FAIL → build ใหม่ด้วย `build-installer.bat` อย่า build จาก Visual Studio ตรง ๆ
- VERSION / NAME FAIL → ปิด Visual Studio แล้ว build ใหม่ด้วย `build-installer.bat` (เลขจะขยับเองและ exe ถูกคอมไพล์ใหม่)
- UPGRADE FAIL → ห้ามส่ง ต้องคืน UpgradeCode ใน `.vdproj` ให้เป็นค่าเดิมก่อน

ไม่แก้ไฟล์ MSI เอง — ไฟล์ที่ถูกแก้มือตามกลับไม่ได้ว่าตรงกับ commit ไหน

## รูปแบบรายงาน

ภาษาไทย สั้น ใช้แบบนี้

```
## ผลตรวจ <ชื่อไฟล์>
เวอร์ชัน <x.y.z> · สรุป: ✅ พร้อมส่ง / ⚠️ ส่งได้แต่ควรดู / ❌ ยังไม่พร้อมส่ง

| ข้อ | ผล | รายละเอียด |
(ทุกข้อจาก script)

## ต้องทำอะไรต่อ
(เฉพาะเมื่อมี FAIL / WARN — วิธีแก้จากขั้นที่ 5 หรือคำตอบต่ออาการที่ผู้ใช้เล่า)
```
