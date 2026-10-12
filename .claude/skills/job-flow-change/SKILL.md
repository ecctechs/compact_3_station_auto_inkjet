---
name: job-flow-change
description: ใช้ก่อนและระหว่างแก้ตรรกะงานพิมพ์ของ Compact Inkjet — marking method, เริ่ม / จบ / ยกเลิกงาน, คิวเครื่อง MK / UV1 / UV2, ปุ่มกดหน้างาน, งาน 22 รอบสอง, ST3 กดเริ่มแทน ST1, การส่งเข้า MK / UV / PLC, OrderListUserControl, JobSendService, PlcOrderService, MachineQueueController, queueGuard หรือ schema ของคิวใน backend
argument-hint: "[สิ่งที่จะแก้ หรืออาการ bug]"
---

# Job Flow Change

ส่วนนี้คือหัวใจของโปรแกรม แก้ผิดแล้วเครื่องพ่นงานซ้ำ ข้ามคิว หรืองานค้าง ที่หน้างานจริง
skill นี้ไม่ได้ห้ามแก้ — แต่บังคับให้รู้ว่ากระทบอะไร และไม่ทำผิดกฎที่ตกลงกันไว้แล้ว

งานที่ได้รับ: $ARGUMENTS

## ขั้นที่ 1 — อ่านกฎก่อนแตะโค้ด

อ่าน [reference.md](reference.md) ทั้งไฟล์ แล้วระบุว่างานนี้แตะกฎข้อไหน
ถ้างานที่สั่ง **ขัดกับกฎในนั้น** ให้หยุดแล้วถามผู้ใช้ก่อน บอกว่าขัดข้อไหน
(กฎพวกนี้ตกลงกับหน้างานแล้ว ไม่ใช่ความชอบของโค้ด — เปลี่ยนได้ แต่ต้องเป็นการตัดสินใจของผู้ใช้)

## ขั้นที่ 2 — ระบุขอบเขต

ตอบให้ได้ก่อนแก้ แล้วบอกผู้ใช้สั้น ๆ:

| คำถาม | ตัวอย่างคำตอบ |
|---|---|
| marking code ไหนได้รับผล | 22 เท่านั้น / 11, 12, 32 / ทุกรหัส |
| สถานีไหน | ST1 / ST3 / Scan Barcode |
| เครื่องไหน | MK / UV1 / UV2 / PLC MK / PLC แคลมป์ |
| แตะ client, backend หรือทั้งคู่ | ถ้าทั้งคู่ ต้องอัปเดตทุกเครื่องพร้อมกัน (ดู reference.md หัวข้อ "ติดตั้ง") |
| เปลี่ยน schema ไหม | ถ้าใช่ ต้องมี migration (ขั้นที่ 4) |

## ขั้นที่ 3 — หาทุกจุดที่เกี่ยว

ใช้ Grep หาทุกที่ที่กฎนี้ถูกใช้ ไม่แก้แค่จุดแรกที่เจอ

- กฎ marking: ต้องอยู่ใน `InkjetOperator/Services/MarkingMethodService.cs` ที่เดียว
  Grep `marking_method`, `MarkingMethod`, `"22"`, `"10"`, `St3Codes` — ถ้าเจอการตีความรหัสนอกไฟล์นี้ ให้ย้ายมาเรียก service แทน ห้ามเขียนกฎซ้ำ
- การส่งเข้าเครื่องมี **2 ทาง**: `JobSendService` (จาก Order List ใช้ในการผลิต) กับ `OrderDetailUserControl` (`new TcpManager()` / `new UvTcpService()` — ปุ่มทดสอบ) แก้ protocol ต้องดูทั้งสองทาง
- คิว: client `ApiClient.cs` (`/machine-queue/*`) ↔ backend `routes/MachineQueue.js` → `MachineQueueController.js` → `services/queueGuard.js` ต้องแก้ให้ตรงกันทั้งสองฝั่ง
- PLC: แถวใน `plc_register_maps`, `PlcSettingUserControl.DefaultRows()` และ `PlcOrderService.BuildPlanAsync` ต้องตรงกันเสมอ

## ขั้นที่ 4 — แก้

- แก้ให้น้อยที่สุดที่ทำให้งานสำเร็จ ไม่ refactor ส่วนที่ไม่เกี่ยว (`OrderListUserControl.cs` ยาว 3,000+ บรรทัด แต่ไม่ใช่งานของรอบนี้)
- **ไม่แตะ** ถ้าไม่ได้สั่ง: protocol ของเครื่อง, address ใน register map, ไฟล์ `.Designer.cs`, schema, ลำดับการพ่นจริงของเครื่อง
- เพิ่มคอลัมน์ / ตารางใน backend: `sequelize.sync()` ไม่เพิ่มคอลัมน์ให้ตารางเดิม ต้องเขียน `InkjetBackend/migrations/ensure<ชื่อ>.js` แบบเดียวกับไฟล์ที่มีอยู่ (ตรวจก่อนเพิ่ม รันซ้ำได้ไม่พัง) แล้วเรียกใน `migrations/index.js`
- comment อธิบาย **เหตุผล** ของกฎเป็นภาษาไทย แบบเดียวกับโค้ดรอบข้าง — โดยเฉพาะเมื่อกฎมาจากหน้างาน
- ข้อความที่พนักงานเห็นเป็นภาษาไทย สั้น บอกว่าต้องทำอะไรต่อ

## ขั้นที่ 5 — ตรวจ

เรียก skill `safe-verify` (build + เทสที่เกี่ยว + เทียบ baseline)
แก้กฎ marking หรือคิว → บอกผู้ใช้ด้วยว่า `QueueClientRegression` / backend `queue-reliability` ครอบกรณีนี้ไหม และต้องมี API / DB ทดสอบถึงจะรันได้

## ขั้นที่ 6 — สรุป

เขียนสรุปแบบเดียวกับ `docs/st1-*.md`:

```
## ก่อน / หลัง
- เดิม: ...
- ใหม่: ...

## กฎที่คงเดิม
- (ข้อที่แตะใกล้ ๆ แต่ยืนยันว่ายังเหมือนเดิม)

## ไฟล์ที่แก้
...

## ต้องลองที่หน้างาน
- (กรณีที่ทดสอบในเครื่องไม่ได้ เพราะต้องใช้เครื่องจริง)
```

ถ้าเป็นการเปลี่ยนใหญ่ ถามผู้ใช้ว่าจะเก็บสรุปนี้เป็นไฟล์ใน `docs/` ไหม — ไม่สร้างเอง
ไม่ commit จนกว่าผู้ใช้สั่ง
