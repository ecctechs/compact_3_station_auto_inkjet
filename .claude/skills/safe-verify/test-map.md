# เทสที่มีอยู่ และใช้เมื่อแก้อะไร

baseline ล่าสุด: ตรวจเมื่อ 8/10/2026 บน main (`f37de07`)
ถ้ามีคนซ่อมเทส ให้อัปเดตแถว baseline ในไฟล์นี้

## ตารางเลือกเทส

| แก้ไฟล์ | รันเทส |
|---|---|
| `Managers/TcpManager.cs`, `Adapters/MkCompactAdapter.cs` | HardwareFaultRegression |
| `Services/UvTcpService.cs` | HardwareFaultRegression, QueueClientRegression* |
| `Services/McProtocolService.cs`, `PushButtonWatcher.cs`, `PushButtonSettings.cs` | HardwareFaultRegression |
| `Views/OrderListUserControl*.cs` | OrderListRefreshRegression |
| `Services/MarkingMethodService.cs`, `JobStageService.cs`, `MachineSendBatch.cs`, `MachineBusy.cs`, `CpiWriteService.cs`, `ApiClient.cs` | QueueClientRegression* |
| `InkjetBackend/controllers/MachineQueueController.js`, `JobController.js`, `services/queueGuard.js`, `model/machineQueueModel.js` | backend queue-reliability* |
| ไฟล์อื่นใน InkjetOperator | build อย่างเดียว + ตรวจด้วยการอ่านโค้ด |

\* ต้องมี API / DB ทดสอบที่ผู้ใช้เตรียมไว้ ไม่มี = ข้าม แล้วบอกในรายงาน

## รายละเอียดแต่ละเทส

### HardwareFaultRegression — ปลอดภัย

- ไฟล์: `tests/HardwareFaultRegression/`
- ทำอะไร: ตั้ง TCP listener ปลอมบน `127.0.0.1` แล้วทดสอบ MK (timeout, คำสั่งไม่ค้าง, ต่อใหม่), UV (สำเร็จ / ปฏิเสธ / เงียบ / ตัดสาย), ปุ่มกดหน้างาน
- ค่าตั้ง: ใช้ `CustomSettingsManager` ปลอมของตัวเอง (`Program.cs:176`) ไม่แตะไฟล์จริง
- รัน: `dotnet run --project tests/HardwareFaultRegression`
- **baseline: คอมไพล์ไม่ผ่าน** — `Program.cs(151,42)` และ `(155,39)` เรียก `PushButtonWatcher.Address` ที่ไม่มีแล้ว (error CS1061 สองจุด)

### OrderListRefreshRegression — ปลอดภัย (อ่านค่าตั้งจริงอย่างเดียว)

- ไฟล์: `tests/OrderListRefreshRegression/` (อ้าง `InkjetOperator.csproj` ทั้งโปรเจกต์)
- ทำอะไร: สร้าง `OrderListUserControl` กับ HTTP handler ปลอม (`http://test.invalid`) ตรวจการรีเฟรช / การส่งไม่ซ้อนกัน / คอลัมน์สถานะเครื่อง ไม่เปิด poll timer จริง (`Program.cs:45`)
- ค่าตั้ง: `StationService` อ่าน `MENU_LEVEL` จาก Setting.config ตัวจริง ต้องเป็น ST1 (`Program.cs:80`) — อ่านอย่างเดียว
- รัน: `dotnet run --project tests/OrderListRefreshRegression`
- **baseline: PASS 4 ข้อ แล้วล้มที่ `System.Exception: existing column widths changed`** (ความกว้างคอลัมน์หน้า Order List ถูกปรับหลังเขียนเทส)

### QueueClientRegression — ต้องมี API ทดสอบ

- ไฟล์: `tests/QueueClientRegression/`
- ต้องตั้ง env: `QUEUE_TEST_API_URL` (ต้องเป็น loopback และไม่ใช่ port 3000 — เทสเช็คเองที่ `Program.cs:7`), `QUEUE_TEST_JOB_ID`
- ทำอะไร: ตรวจกฎ marking 11/12/32/22, คิว enqueue / release / begin-send ซ้ำ, ส่งคนละเครื่องพร้อมกัน, CPI write พร้อมกัน
- **baseline: ยังไม่เคยรันใน session ที่บันทึกไว้** — ไม่มี API ทดสอบ

### backend queue-reliability — ต้องมี DB ทดสอบ

- ไฟล์: `InkjetBackend/test/queue-reliability.test.js` (node:test)
- ต้องตั้ง env: `QUEUE_TEST_DATABASE_URL` ชี้ฐาน `queue_reliability_test` บน loopback (เทสปฏิเสธเองถ้าไม่ใช่ — บรรทัด 6–9)
- รัน: `cd InkjetBackend && node --test test/`
- **baseline: ยังไม่เคยรันใน session ที่บันทึกไว้**
