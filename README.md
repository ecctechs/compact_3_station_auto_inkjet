# Compact Inkjet (3 Station Auto Inkjet)

โปรแกรมคุมสายพิมพ์ Inkjet ของงาน Compact ใช้พิมพ์บนแผ่นเหล็ก (Plate) และชิม (Shim) ของผ้าเบรก
พนักงานยิงบาร์โค้ดใบงานที่จุดสแกน งานจะเข้าคิวให้ Station 1 แล้วโปรแกรมส่งข้อมูลเข้าเครื่องพิมพ์ MK, เครื่อง UV และ PLC ให้เอง
ไม่ต้องไปพิมพ์ข้อมูลที่หน้าเครื่องทีละตัว

ทุกเครื่องใช้โปรแกรมตัวเดียวกัน ต่างกันแค่ค่า `MENU_LEVEL` ในไฟล์ตั้งค่า ว่าเครื่องนั้นเป็นจุดสแกน, Station 1 หรือ Station 3

---

## ทำอะไรได้บ้าง

- ยิงบาร์โค้ดลงทะเบียนงาน ดึงข้อมูลงานจาก PrintData.db3 มาให้เอง
- ส่งข้อความและโปรแกรมพิมพ์เข้า MK-058 / MK-059 และ UV1 / UV2 ตาม Marking Method ของงาน
- ส่งค่า PostAct, Delay และความเร็วสายพานเข้า PLC ทุกครั้งที่เริ่มงาน MK
- สั่งระยะแคลมป์ 6 แกนของงาน UV ผ่าน PLC แคลมป์
- ใช้ปุ่มกดหน้างานแทนการกดเริ่มงานบนจอได้
- ดูสถานะเครื่องทุกตัวได้จากหน้า Order List ว่าเครื่องไหนกำลังพิมพ์งานอะไร มีงานรอคิวไหม

## งานเดินยังไง

1. จุดสแกนยิงบาร์โค้ด กด OK งานเข้า Backend ที่เครื่อง Station 1
2. Station 1 เห็นงานในหน้า Order List กด **เริ่มงาน** โปรแกรมส่งข้อมูลเข้าเครื่องตาม Marking Method
3. งานที่ต้องผ่าน UV2 (Marking 10 / 11 / 12) ไปขึ้นที่ Station 3 ด้วย กดเริ่มหรือจบงานที่ Station 3 ได้
   แต่คนส่งข้อมูลเข้า UV2 จริง ๆ ยังเป็นเครื่อง Station 1 เครื่อง Station 1 จึงต้องเปิดค้างไว้
4. พิมพ์เสร็จกด **จบงาน** งานย้ายไปอยู่ในแท็บ History

---

## หน้าจอ

### 1. จุดสแกนบาร์โค้ด (Input Order)

![Scan Barcode](docs/images/scan-barcode.png)

ยิงบาร์โค้ดที่ช่อง Barcode ได้เลย ไม่ต้องคลิกช่องก่อน โปรแกรมวางเคอร์เซอร์ไว้ให้ตั้งแต่เปิด
ERP MFG, Marking Method และ Qty จะขึ้นมาเอง เช็คให้ตรงกับใบงานแล้วกด OK
ถ้า Qty ว่างหรือไม่ตรงให้กดรูปดินสอแก้ก่อน

ตอนติดตั้งครั้งแรกต้องตั้ง 2 อย่างในหน้า Setting

- **Database Setting** เลือกไฟล์ `PrintData.db3` (ข้อมูลงานพิมพ์) กับ `mydatabase.db3` (ระยะแคลมป์ของงาน UV)
- **Backend DB Setting** ใส่ IP ของเครื่อง Station 1 กด Check Status ให้ไฟเขียวแล้วค่อย Save

![Database Setting](docs/images/scan-barcode-database.png)

### 2. Order List (Station 1)

![Station 1 Order List](docs/images/st1-order-list.png)

หน้าหลักของ Station 1

- แท็บ List คืองานที่ยังไม่จบ ส่วน History คืองานที่จบหรือยกเลิกแล้ว กรองได้ว่าจะดูทั้งหมด, In-line หรือ Off-line
- Status สีแดงคือ Waiting (ยังไม่เริ่ม) สีส้มคือ Working (กำลังทำ)
- ปุ่มท้ายแถว: เริ่มงาน / จบงาน, X ยกเลิกงาน, แว่นขยายเปิด Order Detail
- แถบใต้ตารางบอกว่า MK, UV1, UV2 กำลังถืองานไหนอยู่ และมีงานรอคิวหรือเปล่า
- Preview โชว์รูปตัวอย่างของงานที่เลือกในตาราง ส่วน Processing โชว์งานที่กำลังพิมพ์

### 3. Order Detail

กดแว่นขยายที่แถวงานเพื่อดูว่าจะส่งอะไรเข้าเครื่องบ้าง แก้ข้อมูลแล้วกด **บันทึกค่า**

![Order Detail MK](docs/images/st1-order-detail-mk.png)

ส่วนบนเป็นข้อมูลงาน, Marking Method และเครื่องที่ทำแต่ละด้าน
ถัดลงมาเป็น MK Section มีโปรแกรม ข้อความ และตำแหน่งของ MK แต่ละหัว ถ้าใส่สลับหัวกัน กด SWAP ทีเดียวจบ

![Order Detail UV](docs/images/st1-order-detail-uv.png)

เลื่อนลงมาเป็น UV Section (ตัวอย่างในรูปเป็น Marking 11 คือ UV1 พิมพ์ Plate, UV2 พิมพ์ Shim)
มี Program Name, ข้อความ Text1–Text5 และระยะแคลมป์ ปรับ -1 / +1 แล้วกด Send ให้ PLC ขยับ
ถ้าต้องการเก็บค่าใหม่ไว้ใช้รอบหน้าให้กด Upload ลงฐานข้อมูล

### 4. Printer Setting

![Printer Setting](docs/images/st1-printer-setting.png)

ตั้ง IP ของ MK-058 / MK-059 และ IP:Port ของ UV1 / UV2
เครื่อง UV ต้องเลือกโฟลเดอร์โปรแกรม UV ของแต่ละเครื่องด้วย ถ้าขึ้น "เชื่อมต่อสำเร็จ" กับ "ไฟล์พร้อมใช้งาน" ครบแปลว่าใช้ได้
ถ้าขึ้นข้อความสีแดงให้เช็คสายแลน เปิดเครื่อง และ IP ก่อน

### 5. PLC MK Setting

![PLC MK Setting](docs/images/st1-plc-mk-setting.png)

PLC ที่คุมตำแหน่งหัวพิมพ์และสายพาน (Modbus TCP)
Register Map คือค่าที่ส่งให้ PLC ทุกครั้งที่เริ่มงาน MK ต้องกด Unlock ก่อนถึงจะแก้ได้
Read All ใช้อ่านค่าปัจจุบันจาก PLC มาดูทั้งหมด แก้เสร็จกด Save ด้านล่าง

### 6. PLC UV Setting และปุ่มหน้างาน

![PLC UV Setting](docs/images/st1-plc-uv-setting.png)

PLC แคลมป์ 6 แกนของงาน UV ใส่ IP กับ Port แล้วกดเช็คการเชื่อมต่อ
ไฟล์ `mydatabase.db3` ต้องขึ้นว่ามีคอลัมน์ครบทั้ง 6 แกน กด Unlock แล้วจะอ่าน เขียน หรือรีเซ็ตทีละแกนได้

![Push Button](docs/images/st1-push-button.png)

เลื่อนลงมาเป็นการตั้งค่าปุ่มกดหน้างานของ ST1 / ST2 / ST3
ใส่ Address ของแต่ละปุ่ม ติ๊กเปิดใช้งาน แล้วกด **ทดสอบอ่านค่า** ต้องได้ 3/3 ก่อนกด Save

### 7. Station 3

![Station 3 Order List](docs/images/st3-order-list.png)

Station 3 เห็นเฉพาะงานที่ผ่าน UV2 และไม่มีแท็บ History
งาน Marking 10 กดเริ่มที่ Station 3 ได้เลย ส่วนงาน 11 / 12 ต้องเริ่มที่ Station 1 ก่อน แล้ว Station 3 ค่อยมีปุ่มจบงาน
งานที่ผ่าน UV2 ทุกงานจบที่ Station 3

![Station 3 Order Detail](docs/images/st3-order-detail.png)

---

## ตั้งค่าแต่ละเครื่อง

ค่าทั้งหมดอยู่ที่ `C:\ProgramData\CompactInkjet\Setting.config` โปรแกรมสร้างไฟล์ให้เองตอนเปิดครั้งแรก
IP กับ path ต่าง ๆ แก้ผ่านหน้า Setting ในโปรแกรม ส่วน `MENU_LEVEL` แก้ในไฟล์

| `MENU_LEVEL` | เครื่อง | เห็นเมนู |
| :---: | :--- | :--- |
| 0 | จุดสแกนบาร์โค้ด | Input Order, Setting |
| 1 | Station 1 | Order List, Setting |
| 3 | Station 3 | Order List (เฉพาะงาน UV2), Setting |
| 9 | ทดสอบหน้างาน | Setting อย่างเดียว |
| 99 | สำหรับพัฒนา | ทุกหน้า |

ค่า IP ตั้งต้นของ Station 1 (เครื่องที่ตั้งเป็น Station 1 ได้ค่าพวกนี้ให้เองตอนเปิดครั้งแรก)

| เครื่อง | IP | Port |
| :--- | :--- | :--- |
| MK-058 / MK-059 | 10.10.100.103 / 10.10.100.104 | |
| UV1 / UV2 | 10.10.100.4 / 10.10.100.3 | 10086 |
| PLC MK | 192.168.1.1 | 502 |
| PLC แคลมป์ | 10.10.100.100 | 5012 |
| ปุ่มหน้างาน ST1 / ST2 / ST3 | M4000 / M4001 / M4003 | |

รายละเอียดเรื่องไฟล์ตั้งค่าอยู่ใน [docs/settings-file-locations.md](docs/settings-file-locations.md)

---

## Tech Stack

| ส่วน | ใช้อะไร |
| :--- | :--- |
| โปรแกรมหน้างาน | C# WinForms, .NET 8 |
| UI | AntdUI 2.4.3 |
| ฐานข้อมูลงานพิมพ์ / แคลมป์ | SQLite (`PrintData.db3`, `mydatabase.db3`) |
| Backend | Node.js, Express, Sequelize |
| ฐานข้อมูลคิวงาน | PostgreSQL |
| ตัวติดตั้ง | Visual Studio Installer Project (MSI) |

## โครงสร้างโฟลเดอร์

```
InkjetOperator/   โปรแกรมหลัก
InkjetBackend/    Backend API
CompactDemo/      โปรเจคตัวติดตั้ง
tools/            สคริปต์สร้างตัวติดตั้ง และสคริปต์แคปหน้าจอ
tests/            โปรแกรมทดสอบ
docs/             เอกสารและรูปหน้าจอ
```

## Build และรัน

โปรแกรม: เปิด `CompactInkjet.sln` ด้วย Visual Studio 2022 แล้วกด Run หรือสั่ง

```bash
dotnet build CompactInkjet.sln
```

Backend: สร้างไฟล์ `InkjetBackend/.env` ใส่ `POSTGRESQL_HOST` เป็น connection string ของฐานข้อมูล (ถ้าไม่ใส่ `PORT` จะใช้ 3000) แล้วสั่ง

```bash
cd InkjetBackend
npm install
npm start
```

ตัวติดตั้ง: ปิด Visual Studio ก่อน แล้วดับเบิลคลิก `build-installer.bat`
สคริปต์จะเพิ่มเลขเวอร์ชันให้และวางไฟล์ MSI ไว้ในโฟลเดอร์ `dist\`
ตัวติดตั้งเช็คให้ว่าเครื่องมี .NET 8 Desktop Runtime หรือยัง และติดตั้งให้ทุกบัญชีผู้ใช้ในเครื่อง
