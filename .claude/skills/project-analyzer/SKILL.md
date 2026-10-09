---
name: project-analyzer
description: วิเคราะห์โครงสร้างโปรเจกต์ C# (.NET / WinForms / ASP.NET), Node.js (Express ฯลฯ) และเว็บไซต์ฝั่งหน้าบ้าน (HTML / React / Vue / Vite) แล้วอธิบาย Flow การทำงานและความสัมพันธ์ของโค้ด ใช้เมื่อผู้ใช้ขอให้ "วิเคราะห์โปรเจกต์" "อธิบายโครงสร้าง" "flow ทำงานยังไง" "ไฟล์นี้เกี่ยวกับอะไรบ้าง" "เริ่มอ่านโค้ดตรงไหน" หรือก่อนลงมือแก้ส่วนที่ยังไม่รู้จัก อ่านอย่างเดียว ไม่แก้ไฟล์
argument-hint: "[โฟลเดอร์ / ฟีเจอร์ / ไฟล์ที่อยากให้ดู — เว้นว่าง = ทั้งโปรเจกต์]"
allowed-tools: Read, Grep, Glob, Bash
---

# Project Analyzer

อธิบายโปรเจกต์จาก **ไฟล์จริง** ให้คนที่เพิ่งเข้ามาอ่านแล้วรู้ว่า
มีอะไรบ้าง เริ่มทำงานที่ไหน ข้อมูลไหลไปทางไหน และไฟล์ไหนเกี่ยวกับไฟล์ไหน

ขอบเขตที่ผู้ใช้ระบุ: $ARGUMENTS
(ถ้าว่าง = ทั้งโปรเจกต์ ถ้าระบุฟีเจอร์หรือไฟล์ = เจาะเฉพาะ flow นั้น แต่ยังบอกว่ามันต่อกับส่วนอื่นยังไง)

## กติกา

- **อ่านอย่างเดียว** ห้ามสร้าง แก้ หรือลบไฟล์ ห้าม `npm install` / `dotnet restore` / รันโปรแกรม / เรียก API
  คำสั่ง shell ใช้ได้แค่แบบอ่าน เช่น `git log`, `git ls-files`, `ls`, `wc`
- **ทุกข้อสรุปต้องมีหลักฐาน** อ้าง `path/ไฟล์:บรรทัด` ถ้าเดาจากชื่อไฟล์หรือรูปแบบทั่วไป ให้เขียนกำกับว่า **(สันนิษฐาน)**
- **ห้ามเปิดเผยความลับ** ไฟล์ `.env`, connection string, password, token — รายงานแค่ชื่อ key ว่ามีอะไรบ้าง ไม่ใส่ค่า
- **ข้ามโฟลเดอร์ที่ไม่ใช่โค้ดของโปรเจกต์** `node_modules/`, `bin/`, `obj/`, `dist/`, `build/`, `.git/`, `.vs/`, `packages/`, `coverage/`
  และโค้ด third-party ที่ก๊อปมาวาง (ดูจาก `.gitignore`, `DefaultItemExcludes` ใน csproj, หรือโฟลเดอร์ที่มี license/README ของคนอื่น)
- โปรเจกต์ใหญ่: อ่านไฟล์ entry point และไฟล์ที่ถูกอ้างถึงมากที่สุดก่อน ไม่ต้องเปิดทุกไฟล์ บอกตรง ๆ ว่าส่วนไหนยังไม่ได้อ่าน
- อ่าน `CLAUDE.md`, `AGENTS.md`, `README*`, `docs/` ก่อนเสมอ แต่ **เชื่อโค้ดมากกว่าเอกสาร** ถ้าไม่ตรงกันให้รายงานว่าไม่ตรง

## ขั้นตอน

### 1. ระบุ stack จากไฟล์จริง

หาไฟล์บอก stack ด้วย Glob แล้วเปิดอ่าน (รายละเอียด pattern อยู่ใน [patterns.md](patterns.md))

| เจอไฟล์ | แปลว่า | อ่านอะไรต่อ |
|---|---|---|
| `*.sln`, `*.csproj` | C# / .NET | `TargetFramework`, `OutputType`, `UseWindowsForms`/`UseWPF`, `Sdk="Microsoft.NET.Sdk.Web"`, `PackageReference`, `ProjectReference` |
| `package.json` | Node.js / JS | `main`, `scripts`, `dependencies` (express, nest, next, react, vue, vite ฯลฯ), `type: module` |
| `index.html`, `vite.config.*`, `next.config.*`, `angular.json`, `nuxt.config.*` | เว็บหน้าบ้าน | framework, router, จุด mount |
| `Dockerfile`, `docker-compose.yml`, `.github/workflows/`, `*.vdproj`, `*.wxs` | build / deploy / installer | ขั้นตอน build และสิ่งที่ถูกแพ็ก |

ถ้ามีหลายโปรเจกต์ใน repo เดียว ให้ระบุแต่ละตัวแยกกันก่อน แล้วค่อยหาว่ามันคุยกันทางไหนในข้อ 5

### 2. วาดโครงสร้างโฟลเดอร์

- ใช้ `git ls-files` (ถ้าเป็น git) จะได้เฉพาะไฟล์ที่ track จริง แล้วนับไฟล์ต่อโฟลเดอร์
- อธิบายแต่ละโฟลเดอร์หลักหนึ่งบรรทัด ว่าเป็นชั้นไหน (UI / service / data / config / test / tool)
- ชี้ไฟล์ใหญ่ที่สุด 5–10 ไฟล์ (`wc -l`) — มักเป็นที่รวมตรรกะหลัก

### 3. หา Entry point

| Stack | ดูที่ |
|---|---|
| C# WinForms / WPF | `Program.cs` → `Main`, `Application.Run(...)`, ฟอร์มแรก, สิ่งที่ทำก่อนเปิดฟอร์ม (โหลด config, init library) |
| ASP.NET | `Program.cs` → `builder.Services.Add*` (DI), `app.Map*` / `MapControllers`, middleware |
| Node.js backend | `package.json` → `main` / `scripts.start` → ไฟล์ที่มี `app.listen` / `createServer`, ลำดับ `app.use(...)` |
| เว็บหน้าบ้าน | `index.html` → script หลัก → `createApp` / `createRoot` / router |

### 4. ไล่ Flow หลัก

เลือก flow ที่สำคัญที่สุด 2–5 อัน (หรือ flow ที่ผู้ใช้ถาม) แล้วไล่จากจุดเริ่มถึงปลายทางจริง:

```
ผู้ใช้กด / request เข้า → handler → service / business logic → ข้อมูล (DB, ไฟล์, API อื่น, อุปกรณ์) → ผลกลับไปที่ไหน
```

- **C#**: event handler (`Click +=`, `_Click`), ตามการเรียก method ข้ามคลาส, `async`/`await`, timer, background thread
- **Node.js**: `routes/` → `controllers/` → `services/` → `models/` (ORM) → DB, รวม middleware ที่ผ่าน (validate, auth)
- **เว็บ**: route → component → state / store → API call (`fetch`, `axios`) → URL ที่เรียก
- จดจุดที่ **เปลี่ยนสถานะ** (DB write, ไฟล์, ส่งคำสั่งไปเครื่อง) และจุดจัดการ error / retry / lock

### 5. หาความสัมพันธ์ข้ามส่วน

- **Client ↔ Backend**: เอา URL / path ที่ฝั่ง client เรียก (Grep `"/api`, `HttpClient`, `fetch(`, `axios.`) ไปจับคู่กับ route ที่ backend ประกาศ
  รายงานเป็นตาราง: endpoint | ใครเรียก (ไฟล์:บรรทัด) | ใครรับ (ไฟล์:บรรทัด) | ทำอะไร
  ถ้ามีฝั่งเดียว (เรียกแต่ไม่มีคนรับ หรือกลับกัน) ให้แจ้ง
- **ข้อมูลร่วม**: DB / ตารางที่หลายส่วนใช้, ไฟล์ config ที่อ่านร่วมกัน, ชื่อ key ใน config
- **ระบบภายนอก**: IP / port / protocol (TCP, Modbus, Serial, HTTP), SDK ที่ใช้ — รายงานชื่อ ไม่รายงานรหัสผ่าน
- **Dependency ภายใน**: คลาส / โมดูลไหนถูกเรียกจากหลายที่ (Grep ชื่อคลาส) = แก้แล้วกระทบกว้าง

### 6. ชั้นข้อมูล

- Model / entity / schema อยู่ที่ไหน ตารางหลักและความสัมพันธ์ (FK, association)
- schema ถูกสร้างหรือแก้ยังไง (migration, `sync()`, EF migration, SQL script) — มีจุดที่ schema ไม่อัปเดตเองไหม

### 7. Build / test / deploy

- คำสั่ง build, test, run ที่มีจริง (`scripts` ใน package.json, sln, script ใน `tools/`)
- มีเทสอะไรบ้าง ครอบคลุมส่วนไหน ไม่ต้องรันเทสเอง — แค่บอกว่ามีและรันยังไง

## รูปแบบรายงาน

เขียนเป็นภาษาเดียวกับที่ผู้ใช้ถาม (ปกติภาษาไทย) ใช้หัวข้อนี้ ตัดหัวข้อที่ไม่เกี่ยวทิ้งได้

1. **สรุปสั้น** — 3–5 บรรทัด: โปรเจกต์นี้คืออะไร มีกี่ส่วน stack อะไร
2. **โครงสร้าง** — ตารางโฟลเดอร์ / โปรเจกต์ย่อย หน้าที่ และไฟล์หลัก
3. **ภาพรวมระบบ** — Mermaid `flowchart` แสดงส่วนต่าง ๆ และทางที่มันคุยกัน (ใส่ protocol / port บนเส้น)
4. **Flow หลัก** — แต่ละ flow เป็นขั้นเลขข้อ อ้างไฟล์:บรรทัดทุกขั้น ถ้าซับซ้อนใช้ Mermaid `sequenceDiagram`
5. **ความสัมพันธ์ของโค้ด** — ตาราง endpoint client↔backend, คลาส/โมดูลที่ถูกใช้หลายที่, ข้อมูลที่ใช้ร่วม
6. **ข้อมูลและการตั้งค่า** — DB / ตาราง, ไฟล์ config และชื่อ key (ไม่มีค่าลับ)
7. **จุดที่ควรระวัง** — ไฟล์ใหญ่ / ตรรกะที่ซ้ำกันหลายที่ / เอกสารไม่ตรงโค้ด / ส่วนที่แก้แล้วกระทบกว้าง
8. **ยังไม่ได้ตรวจ / สันนิษฐาน** — บอกให้ชัดว่าข้อไหนไม่ได้เปิดดูจริง

Mermaid: ข้อความภาษาไทยใส่ในเครื่องหมายคำพูด เช่น `A["หน้าสแกนบาร์โค้ด"]` และใช้ `<br/>` แทนการขึ้นบรรทัด

ถ้าผู้ใช้ต้องการเป็นไฟล์หรือเอกสาร ให้ถามก่อนว่าจะเก็บที่ไหน — skill นี้ไม่สร้างไฟล์เอง
