import fs from 'node:fs/promises';
import { Workbook, SpreadsheetFile } from '@oai/artifact-tool';

const dir = new URL('.', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');
const wb = Workbook.create();
const pending = '☐ ยังไม่ทดสอบ';
const options = [pending, '☑ ผ่าน', '☒ ไม่ผ่าน', 'รอทดสอบ', 'ไม่เกี่ยวข้อง'];
// Each test has a concrete fixture, short action and observable acceptance criterion.
const groups = [
{
 name:'0 Station Barcode', title:'Station Barcode · Menu Mode 0', prefix:'B', color:'#24566F',
 flow:'Flow: สแกน → ตรวจ Lot / Qty / Marking → ลงทะเบียน → ตรวจงานที่ Station 1 และ 3',
 prep:'เตรียม: PrintData.db3, mydatabase.db3 และ Backend ชุดทดสอบ ใช้ Lot จริงที่ทราบ Marking และข้อความอ้างอิง',
 tests:[
 ['เมนูและค่าเชื่อมต่อ','ตั้ง MENU_LEVEL = 0','เปิดโปรแกรม ตรวจชื่อหน้าและเมนู แล้วปิดเปิดใหม่','เห็น Scan Barcode, Database Setting, Backend DB Setting; ค่าตั้งยังอยู่'],
 ['เปิดโปรแกรมซ้ำ','โปรแกรมเปิดอยู่แล้ว','เปิดไอคอนซ้ำ 2–3 ครั้ง','มีโปรแกรมทำงานตัวเดียว ไม่ลงทะเบียนหรือส่งคำสั่งซ้ำ'],
 ['สแกนและตรวจข้อมูล','Lot ที่มี print_data, inkjet_data และ Routing ครบ','สแกนแล้ว Enter; เทียบ Order No, Qty, Marking กับข้อมูลต้นทาง','แสดงข้อมูลตรง Lot ครบ ไม่ใช้ข้อมูลจากงานก่อน'],
 ['สแกนโดยไม่กด Enter','Lot ที่มีข้อมูลครบ','สแกนแล้วรอแสดงข้อมูล; ถ้ายังไม่โหลดให้กด OK','โหลดข้อมูลอัตโนมัติ; ถ้า OK เพิ่งดึงข้อมูล ต้องกด OK อีกครั้งจึงลงทะเบียน'],
 ['Barcode ว่าง / ไม่พบ','Barcode ว่าง และ Lot ที่ไม่มีในฐานข้อมูล','ลองค้นหาและกด OK ทีละกรณี','แจ้งให้กรอกหรือไม่พบข้อมูล; ไม่สร้าง Job'],
 ['เปลี่ยน Lot ระหว่างกรอก','Lot A มีข้อมูล; Lot B ไม่พบ','โหลด A แล้วเปลี่ยนเป็น B จากนั้นกด OK','ไม่สร้าง B ด้วย Order / Qty / Marking ของ A'],
 ['แก้ Qty','Lot ที่อ่านข้อมูลได้','ใส่ Qty = 5 แล้วลงทะเบียน; เทียบต้นทางและงานใหม่','Job ใหม่ใช้ Qty 5; Qty ใน PrintData.db3 และ UV ต้นทางไม่ถูกแก้'],
 ['Qty ไม่ถูกต้อง','Lot ที่โหลดแล้ว','ลอง 0, -1, 1.5 และตัวอักษร; แก้เป็นจำนวนเต็มบวก','ไม่รับค่าผิด; หลังแก้ถูกต้องลงทะเบียนได้'],
 ['ลงทะเบียนครบและส่งต่อ','Lot 02, 01, 10, 11, 12, 22 อย่างละงาน','ลงทะเบียนทีละ Lot; จด Job ID แล้วตรวจที่ Station 1 / 3','งานเป็น Waiting; Pattern / UV / Routing ผูกถูก Job; แสดงตรง Station ตาม Marking'],
 ['ไฟล์ต้นทาง / Share หลุด','สำเนา SQLite ผ่าน Network Share','ทำให้ Path ใช้ไม่ได้ แล้วสแกน; คืนการเชื่อมต่อและลองใหม่','แจ้งอ่านฐานข้อมูลไม่ได้; ไม่สร้างงานจากข้อมูลค้าง; กลับมาอ่านได้'],
 ['Backend หรือ PostgreSQL ล่ม','ผู้ดูแลควบคุมระบบทดสอบ','หยุด Backend หรือ DB แล้วลงทะเบียน; เปิดกลับและลองใหม่','แจ้งสร้างงานไม่ได้; ไม่รายงานสำเร็จผิด และไม่มีงานซ้ำโดยไม่ทราบสาเหตุ'],
 ['ข้อมูลหรือ API ไม่ครบ','Lot ขาด inkjet_data / Routing; ผู้ดูแลจำลอง API ล้มเหลว','ทดสอบแต่ละกรณี; ตรวจ Job, Pattern, UV และ IAI ที่เหลือ','ขาด inkjet_data ไม่สร้างงาน; Pattern ล้มเหลวลบ Job คืน; ข้อมูลส่วนอื่นขาดต้องตรวจพบ'],
 ['ฐานข้อมูลแคลมป์ไม่พร้อม','ยังไม่ตั้ง mydatabase หรือไม่มีโปรแกรมที่ค้นหา','ลงทะเบียนงาน UV; ลองยกเลิกและยืนยันทำต่อเมื่อมีคำเตือน','ยกเลิกแล้วไม่สร้าง Job; ทำต่อเก็บ IAI ที่หาไม่พบเป็นค่าว่าง ไม่ใช้ค่าของงานอื่น'],
 ['รับงานพร้อมกัน / ข้ามวัน','สองเครื่อง Barcode ในระบบทดสอบ; เวลาวันไทยที่เตรียมไว้','ลงทะเบียนพร้อมกัน และทดสอบก่อน/หลังเที่ยงคืนตามเวลาไทย','Job ID ไม่ชน; เลขงานประจำวันไม่ซ้ำ และเริ่มวันใหม่ถูกต้อง'],
 ['ไม่ส่งเครื่องจากหน้า Barcode','มีคิว active ที่ยังไม่ส่งใน Backend ทดสอบ','เปิดเฉพาะ Mode 0; เฝ้าประวัติคำสั่งและปลายทางจำลอง','หน้า Barcode ต้องไม่หยิบคิวไปสั่งเครื่องเอง; หากมีคำสั่งออกให้บันทึกไม่ผ่าน'],
 ],
 risks:[
 'B-12: ลงทะเบียนผ่านหลาย API จึงมีโอกาสเหลือ Job ที่ข้อมูลไม่ครบ; ผลบันทึก IAI ไม่ถูกแจ้งชัด',
 'B-15: StationService.Current มองทุกโหมดที่ไม่ใช่ 3 เป็น ST1 ต้องตรวจว่าหน้า Barcode ไม่ส่งงานเบื้องหลัง',
 'B-10: ต้องทดลอง Share หลังเปิดเครื่องใหม่ด้วย เพื่อยืนยันสิทธิ์เข้าถึงไฟล์จริง',
 ],
 source:'อ้างอิงโค้ด 2e12b36: MainShellForm, ScanBarcodeUserControl, SqliteDataService, StationService, JobController',
},
{
 name:'1 Station 1', title:'Station 1 · Menu Mode 1', prefix:'S1', color:'#354C83',
 flow:'Flow: รับงานจาก Barcode → ตรวจ/แก้ข้อมูล → เริ่มและจองคิว → ส่ง MK / UV → ปล่อยเครื่อง → จบหรือส่งต่อให้ ST3',
 prep:'เตรียม: งาน Waiting ครบทุก Marking, MK ทั้งสองหัว, UV1/UV2, CPI/โปรแกรม UV, PLC และฐานข้อมูลชุดทดสอบ',
 tests:[
 ['เมนูและรายการงาน','ตั้ง MENU_LEVEL = 1; มีงานจาก Barcode','เปิดโปรแกรมและตรวจเมนู / Order List / History','เห็นเมนูตาม Mode 1; ไม่เห็น Scan; List ไม่แสดง 10/30; History ดูงานทุก Station ได้'],
 ['ข้อมูลจาก Barcode','งานใหม่และ Job ID จาก B-09','เปิดรายละเอียด เทียบ Lot, Qty, Pattern, UV และรูปอ้างอิง','ข้อมูลตรงงานเดียวกัน; ไม่มีรูปหรือข้อความของงานก่อนค้าง'],
 ['เริ่ม MK ตาม Marking','งาน 02 และ 20; MK ว่าง','ตรวจข้อมูลแล้วเริ่มงานทีละรหัส','จอง/ส่ง MK ตาม Pattern; ไม่ส่ง UV ที่ไม่อยู่ในแผน; บันทึก MK สำเร็จเมื่อรับครบ'],
 ['เริ่มงานที่ใช้ UV','งาน 01, 11 และ 12; เครื่องว่าง','เริ่มงานทีละรหัสและตรวจคิว/คำสั่งปลายทาง','01 ใช้ UV1; 11 ใช้ UV1+UV2; 12 ใช้ MK+UV2; ทุกเครื่องที่ว่างอาจรับข้อมูลตั้งแต่เริ่ม'],
 ['รหัสเทียบเท่าและงานมือ','งาน 03, 13, 31, 32, 33, 00 และ 21','เทียบแผนกับ 01, 11, 12; ลองเริ่ม 00 และ 21','เลข 3 ทำเหมือน 1; 00 เริ่มโดยไม่ส่งเครื่อง; 21 เริ่มไม่ได้'],
 ['ยกเลิกก่อนเริ่ม','งาน Waiting ที่ยังไม่เคยส่ง','กดเริ่ม แล้วกดยกเลิกในกล่องยืนยัน','ไม่จองคิว ไม่เปลี่ยนโปรแกรมเครื่อง และงานยังรอเริ่ม'],
 ['แก้ข้อความ / SWAP / ABC','งาน MK ที่ยังไม่เริ่ม','แก้ข้อความและบันทึก; SWAP; เปลี่ยน ABC; เปิดรายละเอียดใหม่แล้วส่ง','ค่าที่บันทึกยังอยู่; ส่งถูกหัวและทิศทางตรงที่เลือก'],
 ['ข้อความ MK ครบทุกช่อง','งาน A ใช้ 5 ช่อง; งาน B ใช้ 1 ช่อง','ส่ง A แล้วส่ง B ตามคิว; ตรวจข้อความในเครื่องและชิ้นงาน','Block 1–5 ลงช่องเครื่อง 6–10; ช่องที่ B ไม่ใช้ไม่เหลือข้อความของ A'],
 ['MK หัวเดียว / สองหัว','Pattern ใช้หัวเดียว และใช้สองหัว','ส่งแต่ละแบบ; ปิดหัวที่ไม่ได้ใช้งานอีกครั้ง','หัวที่ใช้รับข้อมูลครบ; หัวไม่ใช้ถูกสั่งหยุด; หยุดหัวไม่ใช้ไม่ได้ต้องมีคำเตือน'],
 ['ค่าพิมพ์ MK เกินช่วง','สำเนางานสำหรับลองค่า เช่น Scale = 0','ลองบันทึก/ส่งค่าผิด แล้วแก้กลับค่าที่เครื่องรับได้','แจ้งค่าที่ผิดก่อนส่งหัวนั้น; หลังแก้ส่งได้และข้อมูลบนเครื่องตรงงาน'],
 ['MK ขัดข้องบางหัว','งานใช้สองหัว; ปิดหรือตัดสายหนึ่งหัว','ส่งงาน ตรวจผลแยกหัว; ต่อกลับแล้วลองใหม่','ไม่บันทึก MK ครบเมื่อหัวจำเป็นล้มเหลว; ตรวจว่าหัวที่ผ่านแล้วไม่พิมพ์ซ้ำ'],
 ['เลือกโปรแกรม UV','มีไฟล์ตรงชื่อ / หลายรุ่น / ไม่พบ / ชื่อมีจุด','ทดสอบทีละแบบ; เลือกรุ่นหรือยกเลิก; ลองยืนยัน default','โหลดรุ่นที่เลือก; ชื่อไม่ถูกตัดผิด; default ต้องยืนยัน; ยกเลิกไม่ส่ง'],
 ['ข้อมูล UV1 / UV2','งานมี Lot และข้อความ 1–5 ต่างกันชัดเจน','ส่ง UV1 และ UV2; ตรวจ CPI และโปรแกรมบนเครื่อง','UV1 เขียน MK063; UV2 เขียน MK067 ที่ id=1; ข้อความและโปรแกรมไม่สลับฝั่ง'],
 ['CPI เขียนไม่ได้','สำเนา CPI: อ่านอย่างเดียว / ล็อก / ขาดคอลัมน์ / ไม่มี id=1','ทดสอบแต่ละกรณีแล้วคืนไฟล์ปกติ','แจ้งจุดผิด; ไม่โหลดและเริ่ม UV ต่อเมื่อเขียนข้อมูลล้มเหลว'],
 ['UV ไม่ตอบแต่ละขั้น','ปลายทางทดสอบจำลอง Stop / Load / Start ไม่ตอบ','จำลองทีละคำสั่ง แล้วตรวจหน้าจอและประวัติ','Load ล้มเหลวไม่สั่ง Start; ต้องไม่สรุปว่าพิมพ์จริงสำเร็จเมื่อ Start ไม่ตอบ'],
 ['คิวรอไม่ทับงานเดิม','A ถือ MK; B รอ MK และยังไม่เคยส่ง','เริ่ม B แล้วตรวจคิว; กดปล่อย A หนึ่งครั้ง','B อยู่รอและไม่เปลี่ยนโปรแกรมทับ A; ปล่อยแล้วส่ง B ครั้งเดียว'],
 ['คิวงานหลายเครื่อง','A ถือ UV2; B เป็น 12 และ MK ว่าง','เริ่ม B; ตรวจ MK และ UV2; ปล่อย UV2','MK รับ B ได้; UV2 ของ B ยังรอ; หลังปล่อยจึงส่ง UV2 โดยไม่ส่ง MK ซ้ำ'],
 ['งาน 22 ถือเครื่องรอบสอง','ตั้งถือเครื่องรอบสอง; A = 22; B รอ MK','ส่ง A รอบแรก; กดปล่อย แล้วตรวจรอบสองของ A','A ได้ MK ต่อสำหรับรอบสอง; B ยังรอ; ครบต้องมี MK สำเร็จสองครั้ง'],
 ['งาน 22 ปล่อยให้คิวอื่น','ตั้งไม่ถือเครื่องรอบสอง; A = 22; B รอ MK','จบ A รอบแรกและกดปล่อย; จากนั้นปล่อย B','B ได้เครื่องก่อน; A รอบสองไปท้ายคิวและกลับมาส่งได้'],
 ['ปุ่มกดค้าง / PLC หลุด','ตั้งปุ่ม MK พร้อม; มีคิวรอ','กดค้าง; เปิดโปรแกรมขณะบิต 1; ตัดต่อ PLC แล้วกดใหม่','หนึ่งขอบ 0→1 ปล่อยครั้งเดียว; ไม่ถือบิตค้างเป็นการกดใหม่; แจ้งและฟื้นการเชื่อมต่อ'],
 ['ปล่อยคิว UV1','มี A ถือ UV1 และ B รอ','ทดสอบช่องทางปุ่มจริงที่หน้างานใช้ปล่อย UV1','ต้องปล่อย A และส่ง B ได้จริง; ถ้ายังไม่มีช่องทางให้เลือก รอทดสอบ และระบุเหตุผล'],
 ['PLC MK และคืนตำแหน่ง','Register Map ถูกต้อง; ใช้หน้าตั้งค่า PLC MK','Write/Read ค่าในชุดทดสอบ; ปล่อย MK ขณะไม่มีคิว','ค่าที่อ่านตรงค่าที่เขียน; ตอนคิวว่างตำแหน่งหัวกลับ 0 ตาม Map'],
 ['IAI ทุกแกนที่ใช้งาน','งาน Process; ตั้ง Target/Run ของ Plate และ Shim','Send ทีละแกน X/Z1/Z2; ลอง Address ขาดและ PLC หลุด','เขียน/Pulse ถูกแกน; ตำแหน่งจริงตรง; ค่าตั้งไม่ครบไม่ส่ง; แจ้งเชื่อมต่อผิดพลาด'],
 ['Upload IAI ได้บางส่วน','สำเนา mydatabase และ Backend','Upload แล้วเปิดใหม่; ทำให้ฐานข้อมูลข้างหนึ่งเขียนไม่ได้','ค่าที่บันทึกตรงทั้งสองฝั่ง; ถ้าได้ข้างเดียวต้องแจ้งว่าไม่ครบ'],
 ['เครื่องรับแล้ว Backend หลุด','ผู้ดูแลจำลองการขาดช่วงหลังส่งสำเร็จ','ตัด API ก่อนบันทึกประวัติ/เวลาส่งคิว; ต่อกลับและเปิดโปรแกรมใหม่','เห็นความไม่ตรงกัน; ไม่ส่งงานเดิมซ้ำเอง; จดคำสั่งและเวลาที่เกิดจริง'],
 ['จบ / ยกเลิก / พิมพ์ใหม่','งาน MK สำเร็จหนึ่งงาน และงานรออีกหนึ่งงาน','จบงานแรก ยกเลิกงานที่สอง; นำกลับจาก History และเริ่มใหม่','Success/Cancel ถูกต้อง; ไม่มีคิวเก่ากันเครื่อง; พิมพ์ใหม่ไม่ถูกนับครบจากประวัติเดิม'],
 ['ประวัติและสิทธิ์จบงาน','งาน 11/12 และงานที่ ST3 จบแล้ว','ลองจบ 11/12 ที่ ST1; ตรวจ History และกรองวันไทย','ST1 จบงานกลุ่มนี้ไม่ได้; ประวัติจบที่ ST3 แสดงถูกวันและระบุงานที่ส่งไม่ครบ'],
 ['รับคำขอ / การส่งชนกัน','ST1 และ ST3 ใช้ Backend เดียวกัน','ให้ ST3 ฝากคำขอ; เริ่ม/ปล่อยคิวใกล้กันจากสองจอ','งานและโปรแกรมตรงคำขอ; เครื่องเดียวไม่ถูกรับงานทับหรือส่งซ้ำ'],
 ],
 risks:[
 'S1-16: ถ้าทุกเครื่องไม่ว่างและงานยังไม่เคยส่ง โค้ดอาจล้างคิวที่เพิ่งจองไว้',
 'S1-11 / 25: ส่งได้บางหัว หรือเครื่องรับแล้วบันทึกคิวไม่ลง มีโอกาสส่งซ้ำ',
 'S1-15: UV Stop ไม่ตอบยังทำต่อ และ Start ไม่ตอบยังคืนผลสำเร็จ ต้องเทียบกับเครื่องจริง',
 'S1-21: ยังไม่พบตัวเฝ้าปุ่มจริงของ UV1 ในโปรแกรม ต้องยืนยันช่องทางปล่อยคิวหน้างาน',
 'S1-22 / 23: เริ่มงานไม่ได้ส่ง Servo/Conveyor หรือ IAI อัตโนมัติ; IAI อ่าน Status ไม่ได้ยังอาจแจ้งสำเร็จ',
 'S1-26: Restore ไม่พบการล้างประวัติคำสั่งเดิม; สูตรแคลมป์ Z ยังต้องยืนยันกับอุปกรณ์จริง',
 ],
 source:'อ้างอิงโค้ด 2e12b36: OrderList/OrderDetail, JobSendService, UvTcpService, ClampService, MachineQueueController',
},
{
 name:'3 Station 3', title:'Station 3 · Menu Mode 3', prefix:'S3', color:'#655080',
 flow:'Flow: รับรายการจาก Backend → เริ่มงานที่มีสิทธิ์ / รับงานต่อ → ให้ ST1 ส่ง UV2 → ปล่อยคิว → ยืนยันจบงาน',
 prep:'เตรียม: ST1 และ ST3 ใช้ Backend เดียวกัน, งาน 10/11/12 และรหัสเทียบเท่า, ปุ่ม UV2 และโปรแกรมอ้างอิงพร้อม',
 tests:[
 ['เมนูและ Backend','ตั้ง MENU_LEVEL = 3','เปิดโปรแกรม ตรวจเมนูและ Backend; ปิดเปิดใหม่','เห็น Order List และ Backend DB Setting; ไม่มี Scan, History หรือ Printer Setting'],
 ['กรองรายการ','มีงาน 10,11,12,30,31,32,33 และ 02','เปิด Order List แล้วเทียบ Job ID','เห็นกลุ่มที่ใช้ UV2; ไม่แสดงงาน 02; ข้อมูล Lot/Qty ไม่สลับ'],
 ['สิทธิ์เริ่มงาน','งาน 10/30 และ 11/12 ยัง Waiting','ลองเริ่มทีละงานโดยยังไม่ให้ ST1 เริ่ม 11/12','10/30 เริ่มที่ ST3 ได้; 11/12 ต้องรอ ST1 และไม่ให้จบข้ามตั้งแต่ยังไม่เริ่ม'],
 ['เริ่มงาน 10 เครื่องว่าง','UV2 ว่าง; ST1 เปิดอยู่','เริ่ม 10 ที่ ST3; ดูคิวและเครื่องหลังรอบรีเฟรช','คิวเดินจน ST1 ส่ง UV2 ได้; ถ้าค้าง pending ให้บันทึกไม่ผ่าน'],
 ['รับงาน 11 จาก ST1','งาน 11 ลงทะเบียนจาก Barcode','เริ่มที่ ST1 แล้วตรวจข้อมูล/คำสั่งที่ ST3','Job ID เดียวกัน; ใช้ UV1 และ UV2 ถูกฝั่ง; ไม่สร้างงานหรือส่งขั้นเดิมซ้ำ'],
 ['รับงาน 12 / 32 จาก ST1','งาน 12 และ 32 อย่างละงาน','เริ่มที่ ST1; ตรวจ MK และ UV2 ก่อนกดจบที่ ST3','ทั้งสองรหัสใช้ MK+UV2 เหมือนกัน; ไม่ถือว่าจบครบเมื่อยังขาดขั้นใด'],
 ['ปุ่มปล่อย UV2','A ถือ UV2; B รอ UV2','กดปุ่มหนึ่งครั้ง แล้วลองกดค้างในรอบถัดไป','ปล่อย A แล้วเปิดคิว B ครั้งเดียว; ST1 ส่ง B; ไม่ข้ามไป C จากการกดค้าง'],
 ['เริ่มขณะ UV2 ไม่ว่าง','A ถือ UV2; B เป็น 10','เริ่ม B ที่ ST3 แล้วตรวจคิว; ปล่อย A','ไม่เปลี่ยนโปรแกรมทับ A; B รอและรับเครื่องได้หลังปล่อย'],
 ['ST1 ปิดแล้วเปิดกลับ','งาน UV2 มีคิวรอ/active','ปิด ST1 ก่อนส่ง; ทำรายการที่ ST3 แล้วเปิด ST1 กลับ','ไม่แจ้งพิมพ์สำเร็จก่อนส่งจริง; กลับมาส่งได้ตามคิวโดยไม่ซ้ำ'],
 ['Backend หลุดระหว่างใช้งาน','มีงานและคิวเดิม','ตัดเครือข่าย ST3; ลองเริ่ม/ปล่อย; เชื่อมต่อกลับ','แจ้งการติดต่อผิดพลาด; ไม่รายงานเปลี่ยนคิวสำเร็จผิด; โหลดสถานะจริงกลับได้'],
 ['UV2 ล้มเหลวผ่านคิวปกติ','ให้ UV2 Offline หรือ CPI เขียนไม่ได้','ให้ ST3 เปิดคิวแล้วรอ ST1 ส่ง','ผู้ใช้ ST3 ต้องทราบว่างานไม่ผ่านและสาเหตุ; ไม่เห็นแค่ค้างหรือสำเร็จผิด'],
 ['ปุ่มส่งสำรองปิด / เปิด','ผู้ดูแลตั้ง ST3_MANUAL_SEND ตามกรณี','เปิด Order Detail; ทดสอบค่าปิดและเปิดกับงานที่มีขั้นเหลือ','ปิดแล้วไม่เห็นปุ่ม; เปิดแล้วขอให้ ST1 ส่งขั้นที่ยังขาดได้'],
 ['เลือกรุ่นจาก ST3','เปิดส่งสำรอง; โฟลเดอร์ UV2 มีหลายรุ่น','เลือกรุ่นแล้วส่งคำขอ; ทดลองยกเลิกและไม่พบไฟล์','ST1 ใช้รุ่นที่เลือก; ยกเลิกไม่ส่ง; default ต้องยืนยัน; ตรวจไฟล์ที่เครื่องโหลดจริง'],
 ['คำขอสำรองล้มเหลว','เปิดส่งสำรอง; ST1/UV2 ไม่พร้อม','ส่งคำขอแล้วรอผล; คืนการเชื่อมต่อและลองตามข้อความแจ้ง','ST3 ได้ผลผิดพลาดหรือหมดเวลารอ; ไม่มีคำขอเก่าทำให้พิมพ์เพิ่มเอง'],
 ['IAI จากรายละเอียดงาน','งาน Waiting และ Process; แกน Shim ตั้งค่าครบ','ลอง Send/Upload ก่อนเริ่ม; หลังเริ่มทดสอบ X/Z1/Z2','Waiting สั่งไม่ได้; Process ส่งถูกแกน; บันทึกไม่ครบสองฐานข้อมูลต้องแจ้ง'],
 ['จบครบทุกขั้น','งาน 10,11,12 ส่งครบตามแผน','ตรวจชิ้นงานจริงและ Qty แล้วกดจบ; เปิด History ที่ ST1','สถานะ Success; คิวถูกล้าง; ประวัติทั้งสองจอตรงกัน; ไม่ส่งเพิ่มหลังจบ'],
 ['จบทั้งที่ยังไม่ครบ','งานที่เริ่มแล้วแต่ขาด UV2','กดจบ; ลองยกเลิกคำยืนยัน แล้วลองยืนยันจบ','แจ้งขั้นที่ขาด; ยกเลิกไม่จบ; ยืนยันบันทึก MANUAL_COMPLETE และแสดงว่าไม่ครบ'],
 ['ยกเลิก / คำขอชนกัน','งานมีคิวหรือคำขอค้าง; สอง Station พร้อม','ยกเลิกพร้อมกับอีกจอกำลังดำเนินงานในระบบทดสอบ','งาน Cancel ไม่ถูกส่งภายหลัง; ไม่มีผู้ถือเครื่องซ้ำ; จดลำดับเหตุการณ์ถ้าผิด'],
 ],
 risks:[
 'S3-04: เริ่มที่ ST3 สร้าง pending แต่ ST1 ส่งเฉพาะ active ที่ยังไม่มีเวลาส่ง มีโอกาสคิวค้าง',
 'S3-11: ผลล้มเหลวของคิวปกติแสดงที่ ST1 แต่ไม่พบการฝาก remote_error กลับเหมือนปุ่มสำรอง',
 'S3-13: โฟลเดอร์ UV2 ที่ ST3 ต้องตั้งใน uv.config; Mode 3 ไม่มีหน้าจอแก้โฟลเดอร์นี้',
 'S3-18: คิวปกติกับ Remote ใช้ตัวตรวจเครื่องว่างต่างกัน ต้องทดสอบเมื่อทำพร้อมกัน',
 'S3-16 / 17: Success เกิดจากคำสั่งและผู้ใช้ยืนยัน ไม่ได้ยืนยันจำนวนพิมพ์จริงครบ Qty',
 ],
 source:'อ้างอิงโค้ด 2e12b36: MainShellForm, SettingUserControl, OrderList/OrderDetail, MarkingMethodService, StationService',
}
];

for (const g of groups) {
 const s = wb.worksheets.add(g.name);
 s.showGridLines = false; s.tabColor = g.color;
 const start=11, end=start+g.tests.length-1, riskStart=end+3, last=riskStart+g.risks.length+3;
 s.getRange(`A1:G${last}`).format.font={name:'Tahoma',size:11,color:'#243449'};
 s.getRange(`A1:G${last}`).format.verticalAlignment='center';
 const widths=[12,25,30,46,51,21,34];
 widths.forEach((w,i)=>s.getRange(`${String.fromCharCode(65+i)}1:${String.fromCharCode(65+i)}${last}`).format.columnWidth=w);
 s.getRange(`A1:G${last}`).format.rowHeight=23;
 s.getRange('A1:G1').format.rowHeight=9;
 s.getRange('A2').values=[[g.title]]; s.getRange('A2:G2').format.font={name:'Tahoma',size:16,bold:true,color:g.color};
 s.getRange('A2:G2').format.rowHeight=31;
 s.getRange('A3').values=[['Compact Inkjet · Integration Test หน้างาน · อ้างอิงโค้ด 2e12b36 · จัดทำ 24/09/2026']];
 s.getRange('A3:G3').format.borders={bottom:{style:'thin',color:g.color}};
 s.getRange('A4:G4').values=[['ผู้ทดสอบ',null,'วันที่ทดสอบ',null,'เวอร์ชัน / เครื่อง / รอบทดสอบ',null,null]];
 for(const r of ['B4','D4','F4:G4']) s.getRange(r).format.fill='#FFF2CC';
 s.getRange('D4').setNumberFormat('dd/mm/yyyy');
 s.getRange('A5').values=[[g.flow]];
 s.getRange('A6').values=[[g.prep]];
 s.getRange('A7').values=[['วิธีใช้: เลือกผลช่อง F; ช่อง G จด Lot/Job, ผลจริง, เวลา/รูป/เลขปัญหา • ผลคาดหวังคือเกณฑ์ผ่าน ยังไม่ใช่ผลทดสอบแล้ว']];
 s.getRange('A7:G7').format.font={name:'Tahoma',size:11,color:'#64748B'};
 s.getRange('A8').formulas=[[`="รวม "&COUNTA(A${start}:A${end})`]];
 for (const [cell,status,label] of [['B8','☑ ผ่าน','ผ่าน '],['C8','☒ ไม่ผ่าน','ไม่ผ่าน '],['D8',pending,'ยังไม่ทดสอบ '],['E8','รอทดสอบ','รอทดสอบ '],['F8','ไม่เกี่ยวข้อง','ไม่เกี่ยวข้อง ']]) {
   s.getRange(cell).formulas=[[`="${label}"&COUNTIFS(F${start}:F${end},"${status}")`]];
 }
 s.getRange('A8:F8').format.font={name:'Tahoma',size:11,bold:true,color:g.color};
 s.getRange('A9').values=[['ทดสอบ Error/ตัดการเชื่อมต่อกับชุดทดสอบที่เตรียมไว้; ไม่มีอุปกรณ์หรือเงื่อนไขไม่พร้อมให้เลือก “รอทดสอบ” พร้อมเหตุผล']];
 s.getRange('A10:G10').values=[['Test ID','รายการทดสอบ','เตรียมก่อนทดสอบ','วิธีทดสอบ','ผลที่คาดหวัง','ผลทดสอบ','ผลจริง / Lot / หลักฐาน']];
 const rows=g.tests.map((t,i)=>[`${g.prefix}-${String(i+1).padStart(2,'0')}`,...t,pending,null]);
 s.getRange(`A${start}:G${end}`).values=rows;
 const table=s.tables.add(`A10:G${end}`,true,`Tests_${g.prefix}`); table.showFilterButton=true; table.style='TableStyleLight1';
 s.getRange(`A${start}:G${end}`).format.fill='#FFFFFF';
 s.getRange(`A10:G${end}`).format.wrapText=true;
 s.getRange(`A${start}:G${end}`).format.verticalAlignment='top';
 s.getRange('A10:G10').format={fill:g.color,font:{name:'Tahoma',size:11,bold:true,color:'#FFFFFF'},horizontalAlignment:'center',verticalAlignment:'center',rowHeight:30,wrapText:true};
 rows.forEach((row,i)=>{
   const r=start+i;
   s.getRange(`A${r}:G${r}`).format.rowHeight=64;
   if(i%2===1) s.getRange(`A${r}:E${r}`).format.fill='#F1F5F9';
 });
 s.getRange(`F${start}:G${end}`).format.fill='#FFF9E8';
 s.getRange(`F${start}:F${end}`).format.horizontalAlignment='center';
 s.getRange(`F${start}:F${end}`).dataValidation={rule:{type:'list',values:options}};
 for(const [value,fill,color] of [['☑ ผ่าน','#E7F3EC','#286340'],['☒ ไม่ผ่าน','#FDE9E7','#A32A28'],['รอทดสอบ','#FFF0C4','#865B13'],['ไม่เกี่ยวข้อง','#E8EDF2','#596777']]){
   s.getRange(`F${start}:F${end}`).conditionalFormats.add('containsText',{text:value,format:{fill,font:{color,bold:true}}});
 }
 s.freezePanes.freezeRows(10); s.freezePanes.freezeColumns(2);
 s.getRange(`A${riskStart}`).values=[['จุดเสี่ยงที่ควรตรวจ']];
 s.getRange(`A${riskStart}:G${riskStart}`).format.font={name:'Tahoma',size:11,bold:true,color:'#9A5C10'};
 for(let i=0;i<g.risks.length;i++) s.getRange(`A${riskStart+1+i}`).values=[[g.risks[i]]];
 s.getRange(`A${last-1}`).values=[[g.source]];
 s.getRange(`A${last}`).values=[['ข้อสังเกตจากโค้ดยังไม่ใช่ผล Fail จริง ต้องทดสอบและบันทึกหลักฐานก่อนสรุป']];
 s.getRange(`A${last-1}:G${last}`).format.font={name:'Tahoma',size:10,color:'#64748B'};
 g.start=start;g.end=end;g.last=last;g.riskStart=riskStart;
}

// Exercise editable results and verify summary formulas, then restore pristine checklist.
for (const g of groups) {
 const s=wb.worksheets.getItem(g.name);
 for(const status of options){
  s.getRange('F11').values=[[status]];
  const got=s.getRange('A8:F8').values[0];
  const expected=[`รวม ${g.tests.length}`,`ผ่าน ${status==='☑ ผ่าน'?1:0}`,`ไม่ผ่าน ${status==='☒ ไม่ผ่าน'?1:0}`,`ยังไม่ทดสอบ ${g.tests.length-(status===pending?0:1)}`,`รอทดสอบ ${status==='รอทดสอบ'?1:0}`,`ไม่เกี่ยวข้อง ${status==='ไม่เกี่ยวข้อง'?1:0}`];
  if(JSON.stringify(got)!==JSON.stringify(expected)) throw new Error(`Summary mismatch ${g.name} ${status}: ${JSON.stringify(got)}`);
 }
 s.getRange('F11').values=[[pending]];
}
wb.recalculate();
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#NUM!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:30},summary:'Formula error check'})).ndjson);
for (const [i,g] of groups.entries()) {
 console.log(JSON.stringify({sheet:g.name,tests:g.tests.length,summary:wb.worksheets.getItem(g.name).getRange('A8:F8').values}));
 for(const [label,range] of [['top','A1:G14'],['bottom',`A${g.end-1}:G${g.last}`]]) {
  const preview=await wb.render({sheetName:g.name,range,scale:1,format:'png'});
  await fs.writeFile(`${dir}/preview-${i}-${label}.png`,new Uint8Array(await preview.arrayBuffer()));
 }
}
const file=await SpreadsheetFile.exportXlsx(wb);
await file.save(`${dir}/Compact_Inkjet_IT_Test.xlsx`);
await fs.writeFile(`${dir}/manifest.json`,JSON.stringify(groups.map(g=>({sheet:g.name,count:g.tests.length,start:g.start,end:g.end,last:g.last})),null,2));
console.log('Export complete');
