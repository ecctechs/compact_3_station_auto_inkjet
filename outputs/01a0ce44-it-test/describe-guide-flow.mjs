import fs from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
import {SpreadsheetFile,FileBlob} from '@oai/artifact-tool';
const dir=path.dirname(fileURLToPath(import.meta.url));
const file=path.join(dir,'Station_Barcode_Code_Guide.xlsx');
const w=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
const s=w.worksheets.getItem('3 ไล่ Flow ตามโค้ด');
const before=[0,1].map(i=>JSON.stringify(w.worksheets.getItemAt(i).getRange('A1:C24').values));
const oldRows=s.getRange('A6:D19').values;
const descriptions=[
 'Program.Main(): เริ่มโปรแกรมและโหลดค่าพื้นฐาน\nMainShellForm(): เตรียมหน้าหลัก\nApplyMenuLevel(): เลือกเมนูและหน้าแรกตาม Mode',
 'TxtBarcode_KeyDown(): รับ Enter จากช่อง Barcode\nLoadLot(quiet: false): ค้น Lot และแจ้งเตือนถ้าไม่พบ',
 'LoadLot(): คุมการค้นและเติมข้อมูลบนจอ\nOpenSourceDb(): เตรียมตัวอ่านไฟล์ตาม DB_PATH\nCanConnect(): ตรวจว่าเปิดไฟล์ SQLite ได้\nGetLotSummary(): อ่าน Order No, Qty, วิธีพิมพ์และลูกค้า',
 'BtnEditQty_Click(): รับการกดปุ่มแก้ Qty\nInputDialog.ShowDialog(): เปิดกล่องรับจำนวนใหม่\nint.TryParse(): ตรวจจำนวนเต็ม แล้วเช็กว่ามากกว่า 0\nค่าที่ผ่านจะใส่ txtQty ไว้ใช้ตอนสร้าง Job',
 'BtnConfirm_Click(): รับ OK และกันกดซ้ำระหว่างบันทึก\nValidateForm(): ตรวจ Barcode, Lot ที่โหลด และ Qty\nProcessBarcodeAsync(): เริ่มลำดับสร้างงานเมื่อผ่าน',
 'ProcessBarcodeAsync(): คุมขั้นตอนสร้างงานทั้งหมด\nGetServices(): เตรียม ApiClient และตัวอ่าน SQLite\nCanConnect(): ตรวจไฟล์ฐานข้อมูลต้นทาง\nPingAsync(): ตรวจว่า Backend ตอบกลับ\nConfirmClampDatabase(): ถามทำต่อถ้าไฟล์แคลมป์ไม่พร้อม',
 'GetPatternDetail(): อ่านค่า MK, สายพาน และ Servo\nGetUvDetail(): อ่านโปรแกรมและข้อความ UV1 / UV2\nGetPlanRouting(): อ่านวิธีพิมพ์และลำดับกระบวนการ',
 'CreateJobAsync(): ส่งหัวงานไป POST /job/create\nJobController.create(): สร้างแถว print_jobs\nBackend คืน Job ID มาใช้ผูกข้อมูลขั้นถัดไป',
 'CreatePatternAsync(): ส่งค่าไป POST /pattern/create\nPatternController.create(): บันทึก Pattern และตารางลูก\nDeleteJobAsync(): ขอลบ Job ถ้าสร้าง Pattern ไม่สำเร็จ',
 'CreateUvJobDataAsync(): ส่งข้อมูลไป POST /uv-job/create\nUvJobController.create(): ล้าง UV เดิมของ Job\nแล้วบันทึกรายการ UV ชุดใหม่',
 'CreatePlanRoutingAsync(): ส่งไป POST /plan-routing/create\nPlanRoutingController.create(): ล้าง Routing เดิม\nแล้วบันทึกแผนงานชุดใหม่ของ Job',
 'SyncIaiAsync(): รวมค่าแคลมป์ของโปรแกรม UV ในงาน\nClampService.Lookup(): ค้นระยะจาก mydatabase.db3\nCreateIaiAsync(): ส่งค่าไป POST /iai/create\nIaiController.create(): สร้างหรืออัปเดต IAI ของ Job',
 'Notify.Success(): แจ้งว่าจบขั้นตอนสร้างงาน\nClearForm(): ล้าง Barcode และข้อมูลประกอบ\nClearLotInfo(): ล้าง Lot เดิม, Order No, วิธีพิมพ์, Qty\ntxtBarcode.Focus(): วางเคอร์เซอร์รอสแกนถัดไป',
 'StartPolling(): ตั้งรอบอ่านงานทุก 5 วินาที\nRefreshDataAsync(): อ่านงานแล้วอัปเดตหน้า Order List\nGetAllJobsAsync(): เรียก GET /job/getAll\nJobController.getAll(): ส่งรายการงานจาก Backend\nRebindTable(): กรองงานของ Station แล้วแสดงในตาราง',
];
if(descriptions.length!==oldRows.length)throw new Error('Flow row count mismatch');
s.getRange('B5').values=[['ฟังก์ชันและหน้าที่ (อ่านจากบนลงล่าง)']];
s.getRange('A5:D5').format.rowHeight=32;
s.getRange('A3').values=[['แต่ละข้อเรียงฟังก์ชันตามที่เรียกใช้งาน พร้อมคำอธิบายสั้น ๆ ส่วนหน้าจอและผลลัพธ์ดูคอลัมน์สุดท้าย']];
s.getRange('B1:B24').format.columnWidthPx=360*96/72;
s.getRange('B6:B19').values=descriptions.map(x=>[x]);
s.getRange('B6:B19').format.verticalAlignment='center';
for(let i=0;i<descriptions.length;i++){
 const estimate=descriptions[i].split('\n').reduce((n,x)=>n+Math.ceil(x.length/56),0);
 s.getRange(`A${i+6}:D${i+6}`).format.rowHeight=Math.max(66,estimate*18+12);
}
s.getRange('A21').values=[['ทางเข้าอัตโนมัติ: TxtBarcode_TextChanged() เริ่มจับเวลาใหม่เมื่อข้อความเปลี่ยน พอหยุดพิมพ์ 600 ms จะเข้า AutoLookupTimer_Tick()\nจากนั้น LoadLot(quiet: true) ค้น Lot โดยไม่เด้งเตือน แล้วกลับเข้า Flow ข้อ 3']];
s.getRange('A21:D21').format.rowHeight=44;
w.recalculate();
console.log((await w.inspect({kind:'table',range:"'3 ไล่ Flow ตามโค้ด'!B6:B9",include:'values',tableMaxRows:4,tableMaxCols:1,maxChars:1600})).ndjson);
console.log((await w.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?',options:{useRegex:true,maxResults:20},summary:'Final error scan'})).ndjson);
for(const [range,name] of [['A1:D10','described-flow-1.png'],['A11:D15','described-flow-2.png'],['A16:D24','described-flow-3.png']]){
 const p=await w.render({sheetName:s.name,range,scale:1.2});
 await fs.writeFile(path.join(dir,name),new Uint8Array(await p.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(w)).save(file);
const check=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
for(let i=0;i<2;i++)if(JSON.stringify(check.worksheets.getItemAt(i).getRange('A1:C24').values)!==before[i])throw new Error('Other sheet changed');
const after=check.worksheets.getItem(s.name).getRange('A6:D19').values;
for(let i=0;i<14;i++){
 for(const col of [0,2,3])if(after[i][col]!==oldRows[i][col])throw new Error('Existing stage or reference changed');
 if(after[i][1]!==descriptions[i])throw new Error('Description lost');
}
console.log('Verified: 14 expanded function descriptions; original steps, references, outcomes and other two sheets preserved.');
