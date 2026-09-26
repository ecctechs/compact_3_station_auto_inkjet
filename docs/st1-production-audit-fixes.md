# ST1: แก้ผลตรวจข้อ 1–4

## สิ่งที่แก้

1. `TcpManager`: จำกัดเวลาเชื่อมต่อและส่ง/รอคำตอบ MK ที่ 3 วินาทีต่อคำสั่ง ใช้ cancellation กับ socket จริง เมื่อหมดเวลาจะปิด connection และไม่ส่งคำสั่งที่รออยู่ต่อบน connection เก่าหรือใหม่ ผู้เรียกแต่ละรายได้รับคำตอบของคำสั่งตัวเอง คำสั่งและช่วงเว้น 50 ms เหมือนเดิม
2. `UvTcpService.LoadAndStartAsync`: ต้องผ่านทั้ง Load และ Start จึงสำเร็จ ถ้า Start ถูกปฏิเสธ ไม่ตอบ หรือปิด connection จะคืน false พร้อมสาเหตุ ทาง Order List เดิมจะบันทึก unknown และถือคิวให้ตรวจ ไม่ส่งซ้ำเอง
3. `PushButtonSettings.Save` แจ้งตัวเฝ้าปุ่มเมื่อบันทึกครบแล้ว `PushButtonWatcher` โหลดค่าทันที รวมเปิด/ปิดใช้งาน ยกเลิกการใช้ผลอ่านที่ค้างจากค่าเก่า และใช้ค่าแรกหลังเปลี่ยนเป็นจุดตั้งต้น ไม่ถือเป็นการกด ถอด event เมื่อหยุดหรือ Dispose
4. `OrderListUserControl.MachineStatus`: เมื่อได้ snapshot คิวสำเร็จ ให้ใช้ผล Backend แทนข้อความชั่วคราวที่จบแล้ว เก็บข้อความระหว่างกำลังส่งไว้ ไม่ใช้ประวัติส่งเก่ายืนยันรอบหลัง Restore และแสดง not_sent เป็นส่งไม่สำเร็จ

ไฟล์ที่แก้ในรอบนี้: `TcpManager.cs`, `UvTcpService.cs`, `PushButtonSettings.cs`, `PushButtonWatcher.cs`, `OrderListUserControl.cs`, `OrderListUserControl.MachineStatus.cs` และชุดทดสอบ ไม่แก้ Designer, schema, protocol, ลำดับงาน หรือกฎกรณีเขียน PLC ไม่ผ่าน

## ผลตรวจ 2026-09-26

- Build solution ไป output แยกผ่าน ไม่มี compile error; MSB4078 แจ้งว่า dotnet build ไม่รองรับโครงการ Installer `.vdproj`
- HardwareFaultRegression ผ่าน: MK เงียบ/หมดเวลา/ไม่ส่งคำสั่งค้าง/ต่อใหม่/ส่งสองคำสั่ง; UV สำเร็จ/ปฏิเสธ/เงียบ/ตัดสาย/Load ไม่ผ่าน; เปลี่ยน Address ระหว่างอ่าน/เปิด/ปิด/บิตค้าง/Dispose
- OrderListRefreshRegression ผ่าน: ส่งและปล่อยแยกเครื่อง กดซ้ำ การคืนสิทธิ์ รีเฟรช คอลัมน์เดิม และสถานะหลังล้างคิว/Restore/คิว ID ใหม่/Backend ไม่ทราบผล
- Backend กับ PostgreSQL ชุดทดสอบบน loopback ผ่าน 19/19 รวมปล่อย MK และ UV2 พร้อมกันแล้วได้งาน B ทั้งสองเครื่อง โดยกดซ้ำไม่ข้ามงาน B
- ทดสอบด้วย TCP/PLC จำลองบน loopback และ config ในหน่วยความจำ ไม่แตะเครื่องพิมพ์ PLC หรือฐานข้อมูลหน้างาน
- ยังไม่ได้เปิด Visual Studio Designer หรือทดสอบอุปกรณ์จริง

## รันทดสอบซ้ำ

```powershell
dotnet build CompactInkjet.sln
dotnet run --project tests/HardwareFaultRegression/HardwareFaultRegression.csproj
dotnet run --project tests/OrderListRefreshRegression/OrderListRefreshRegression.csproj
```

ชุด Backend ใช้ขั้นตอนและฐานทดสอบแยกตาม `st1-queue-reliability-phase1.md` ห้ามชี้ฐานจริง ถ้าโปรแกรมเปิดอยู่ ให้ build ไป artifacts path แยกแทนการปิดโปรแกรมหน้างาน

## ต้องเทสหน้างานต่อ

1. ส่ง MK ปกติ ตรวจว่าแต่ละคำตอบมาทัน 3 วินาที จากนั้นจำลองสายหลุดในช่วงทดสอบ ตรวจว่าไม่ค้างและไม่ส่งซ้ำเอง
2. ส่ง UV ปกติและกรณี Start ไม่สำเร็จ ตรวจว่าหน้าจอไม่ขึ้นส่งแล้วเมื่อไม่สำเร็จ
3. บันทึก M address ใหม่ กดจริง ตรวจว่าอ่านตัวใหม่โดยไม่ต้องปิดโปรแกรม บิตที่ค้าง 1 ตอนบันทึกต้องไม่ปล่อยคิว ต้องกลับ 0 แล้วกดใหม่
4. ปล่อย MK/UV2 พร้อมกัน ทดสอบงานสองเครื่องและกดรัว ต้องได้งานถัดไปถูกใบ ไม่ข้ามคิว
5. ล้างคิว/Restore งานเดิม ตรวจว่าไม่ติดสถานะส่งแล้วจากรอบก่อน

ยังไม่รับรอง Production ทั้งระบบ: ST1 อ่าน MK, ST3 อ่าน UV2; ยังไม่มีตัวอ่านปุ่ม ST2/UV1 ขั้นตอนกู้คิวไม่ทราบผลและการหยุดเมื่อเขียน PLC ไม่ผ่านยังต้องตกลงต่างหาก ไม่อยู่ในรอบนี้
