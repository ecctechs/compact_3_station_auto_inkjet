import fs from 'node:fs/promises';
import {Workbook, SpreadsheetFile} from '@oai/artifact-tool';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../..');
const wb=Workbook.create();
const blue='#24445E', pale='#EFF4F8', ink='#202A33';
const sources=[];
async function source(file,needle,label){
 const lines=(await fs.readFile(path.join(root,file),'utf8')).split(/\r?\n/);
 const index=lines.findIndex(x=>x.includes(needle));
 if(index<0)throw new Error(`Missing source: ${file} ${needle}`);
 sources.push({file,line:index+1,needle});
 return `${label??file}\nบรรทัด ${index+1}`;
}
function setup(name,widths,last){
 const s=wb.worksheets.add(name); s.showGridLines=false;
 s.getRange(`A1:C${last}`).format={font:{name:'Tahoma',size:12,color:ink},verticalAlignment:'center',wrapText:true,rowHeight:18};
 widths.forEach((n,i)=>s.getRange(`${'ABC'[i]}1:${'ABC'[i]}${last}`).format.columnWidthPx=n*96/72);
 s.tabColor=blue;
 return s;
}
function text(s,row,value,height=24,size=12,bold=false){
 s.getRange(`A${row}:C${row}`).merge();
 s.getRange(`A${row}`).values=[[value]];
 s.getRange(`A${row}:C${row}`).format={rowHeight:height,font:{name:'Tahoma',size,bold,color:ink}};
}
function header(s,row,values){
 s.getRange(`A${row}:C${row}`).values=[values];
 s.getRange(`A${row}:C${row}`).format={fill:blue,font:{name:'Tahoma',size:12,bold:true,color:'#FFFFFF'},rowHeight:25,horizontalAlignment:'center'};
}
function rows(s,start,values,height){
 s.getRange(`A${start}:C${start+values.length-1}`).values=values;
 for(let i=0;i<values.length;i++){
  const r=start+i;
  s.getRange(`A${r}:C${r}`).format={rowHeight:Array.isArray(height)?height[i]:height,fill:i%2===0?pale:'#FFFFFF'};
  s.getRange(`A${r}`).format.font={name:'Tahoma',size:12,bold:true,color:ink};
 }
}
const a=setup('1 อ่านภาพรวม',[130,315,335],23);
text(a,2,'Station Barcode (Mode 0)',28,17,true);
text(a,3,'หน้ารับ Lot และสร้างงานให้ Station 1 / 3 ใช้ต่อ',23);
header(a,5,['ขั้นหลัก','โปรแกรมทำอะไร','จำแค่นี้']);
rows(a,6,[
 ['1. เปิดหน้า','MENU_LEVEL = 0 แสดง Scan Barcode และ Setting','MainShellForm เป็นคนเลือกเมนู'],
 ['2. รับ Barcode','Enter แล้วค้นทันที หรือหยุดพิมพ์ 600 ms แล้วค้นเอง','เอา Barcode ไปเทียบ lot_no ใน PrintData.db3'],
 ['3. ตรวจข้อมูล','แสดง Order No, Marking Method, Qty\nแก้ Qty ได้ด้วยปุ่มดินสอ','ถ้ายังไม่เคยโหลด Lot กด OK ครั้งแรกจะโหลดให้\nต้องตรวจข้อมูลแล้วกด OK อีกครั้ง'],
 ['4. สร้างงาน','เช็กฐานข้อมูลและ Backend แล้วบันทึก\nJob, Pattern, UV, Routing และค่า IAI','ProcessBarcodeAsync() คือฟังก์ชันหลัก\nลำดับบันทึกละเอียดอยู่ในชีต 2'],
 ['5. รับงานต่อ','Order List ของ Station 1 / 3 อ่านงานจาก Backend\nแล้วกรองตามกฎของ Station','ใช้ Job ID เดียวกันเชื่อมข้อมูล\nการลงทะเบียนจบแล้วล้างหน้ารอสแกนถัดไป']
], [34,38,44,44,44]);
text(a,12,'ข้อมูลที่ควรรู้',23,12,true);
header(a,13,['ข้อมูล','อ่านมาจากไหน','ใช้ทำอะไร']);
rows(a,14,[
 ['Order No / Qty','PrintData.db3: print_data.erp_mfg / qty','เป็นข้อมูลหัวงานที่แสดงบนหน้า Barcode'],
 ['Marking Method','PrintData.db3: plan_routing.marking_method','บอกประเภทงาน ใช้กำหนดงานของแต่ละ Station'],
 ['Pattern / UV','inkjet_data เก็บ MK และค่าประกอบ\nprint_data.m1_* / m2_* เก็บ UV1 / UV2','Pattern = ข้อมูลตั้งค่าพิมพ์\nUV = ชื่อโปรแกรมและข้อความที่จะใช้'],
 ['ค่า IAI','mydatabase.db3 ผ่าน ClampService.Lookup()','ค่าระยะแคลมป์ของงาน\nขั้นตอนลงทะเบียนอ่านค่าไปเก็บ ไม่สั่งแคลมป์วิ่ง']
], [30,30,38,38]);
text(a,19,'สองเรื่องที่ห้ามสับสน',23,12,true);
text(a,20,'แก้ Qty บนจอ: เปลี่ยนเฉพาะ print_jobs ของงานใหม่ ส่วน PrintData.db3 และ Qty ในข้อมูล UV ยังเป็นค่าเดิม',31);
text(a,21,'แจ้งสร้างงานสำเร็จ: ไม่ได้แปลว่าพิมพ์เสร็จ และข้อมูล UV / Routing / IAI อาจบันทึกไม่ครบ ต้องดูคำเตือนด้วย',31);
text(a,23,'อ่านโค้ดปัจจุบัน 24/09/2026 (40324a6)   จุดอ้างอิงและลำดับอ่านอยู่ในชีต 2',21,11);

const b=setup('2 เปิดโค้ดตามนี้',[185,330,265],24);
text(b,2,'เปิดอ่านเฉพาะจุดหลัก',28,17,true);
text(b,3,'เริ่มที่ LoadLot(), ValidateForm() และ ProcessBarcodeAsync() ก่อน แล้วค่อยตาม Service ที่ถูกเรียก',25);
header(b,5,['อ่านตรงนี้','หน้าที่ / สิ่งที่ต้องดู','ไฟล์และบรรทัด']);
const scan='InkjetOperator/Views/ScanBarcodeUserControl.cs';
const sql='InkjetOperator/Services/SqliteDataService.cs';
const api='InkjetOperator/Services/ApiClient.cs';
rows(b,6,[
 ['1. ApplyMenuLevel()','เลือกเมนูตาม MENU_LEVEL\nMode 0 เปิดหน้า Scan Barcode กับ Setting',await source('InkjetOperator/Views/MainShellForm.cs','private void ApplyMenuLevel()', 'Views/MainShellForm.cs')],
 ['2. LoadLot()','ค้น Lot แล้วเติมข้อมูลบนจอ\n_loadedBarcode จำ Lot ที่โหลดแล้ว\nTextChanged ล้างข้อมูลเมื่อเปลี่ยน Barcode',await source(scan,'private bool LoadLot(', 'Views/ScanBarcodeUserControl.cs')],
 ['3. ValidateForm()','ตรวจ Barcode และ Qty ซึ่งต้องเป็นจำนวนเต็ม > 0\nถ้า Barcode นี้ยังไม่โหลด ให้โหลดก่อนและรอกด OK ใหม่\nBtnEditQty_Click() เป็นจุดรับ Qty ที่ผู้ใช้แก้',await source(scan,'private bool ValidateForm()', 'Views/ScanBarcodeUserControl.cs')],
 ['4. ProcessBarcodeAsync()','ตรวจ SQLite / Backend แล้วสร้าง Job เพื่อรับ Job ID\nบันทึก Pattern แล้วตามด้วย UV, Routing และ IAI\nจบแล้วแจ้งผลและ ClearForm()',await source(scan,'private async Task ProcessBarcodeAsync(', 'Views/ScanBarcodeUserControl.cs')],
 ['5. SqliteDataService','GetLotSummary() อ่านหัวงาน\nGetPatternDetail() / GetUvDetail() / GetPlanRouting()\nอ่านข้อมูลต้นทางสำหรับส่งเข้า Backend',await source(sql,'public CreatePatternRequest? GetPatternDetail(', 'Services/SqliteDataService.cs')],
 ['6. ApiClient / Backend','ApiClient ส่ง HTTP ไปที่ http://PC_IP:3000\n/job/create สร้าง print_jobs สถานะ Waiting\nBackend บันทึกใน PostgreSQL',`${await source(api,'public async Task<(PrintJob? job,', 'Services/ApiClient.cs')}\nBackend: JobController.create() บรรทัด 35`],
 ['7. SyncIaiAsync()','อ่านค่าแคลมป์แยก Plate / Shim จากชื่อโปรแกรม\nชื่อขึ้นต้น P- เป็น Plate ที่เหลือเป็น Shim\nหาไม่เจอส่งค่าว่าง ไม่มีชื่อโปรแกรมเลยก็ข้าม',await source(scan,'private static async Task SyncIaiAsync(', 'Views/ScanBarcodeUserControl.cs')]
], [39,51,51,51,51,58,51]);
text(b,14,'ถ้าเจออาการนี้ ให้ย้อนดูตรงนี้',23,12,true);
rows(b,15,[
 ['ไม่เจอ Lot / ต่อ DB ไม่ได้','LoadLot() และ DB_PATH\nค้นอัตโนมัติจะเงียบ กด Enter / OK จึงแจ้งปัญหา','ScanBarcodeUserControl.cs:140\nSqliteDataService.cs:15, 122'],
 ['Job มี แต่ข้อมูลไม่ครบ','Pattern ล้มเหลวพยายามลบ Job แต่ไม่เช็กผลลบ\nUV / Routing ล้มเหลวแจ้งเตือนแต่ยังทำต่อ','ScanBarcodeUserControl.cs:304–344'],
 ['ขึ้นสำเร็จ แต่ค่า IAI ไม่มี','SyncIaiAsync() ไม่ตรวจผลที่ CreateIaiAsync() คืนมา\nจึงยังแจ้งสร้างงานสำเร็จได้แม้บันทึก IAI ไม่สำเร็จ','ScanBarcodeUserControl.cs:429']
], [40,40,40]);
text(b,19,'ไฟล์ Views/ และ Services/ ด้านบนอยู่ใต้ InkjetOperator/ ส่วน Backend อยู่ใน InkjetBackend/controllers/',26,11);
text(b,20,'ค่าตั้งหลัก: MENU_LEVEL เลือกโหมด, DB_PATH ชี้ PrintData.db3, PC_IP ชี้ Backend, CLAMP_DB_PATH ชี้ mydatabase.db3',28,11);
text(b,21,'ยังไม่ต้องอ่าน Designer.cs (จัดหน้าจอ) หรือคำสั่งเครื่องพิมพ์ทั้งหมด เริ่มจาก 3 ฟังก์ชันที่แนะนำด้านบนก่อน',26,11);
text(b,23,'ส่งต่องาน: OrderListUserControl.RefreshDataAsync() บรรทัด 454 อ่าน Backend ตามรอบ 5 วินาทีเมื่อรอบก่อนเสร็จ',26,11);
// Supporting source anchors are verified against the current files, without changing project code.
await source('InkjetBackend/controllers/JobController.js','static async create(req, res)');
await source('InkjetBackend/model/jobModel.js','defaultValue: "Waiting"');
await source('InkjetOperator/Services/ClampService.cs','public static ClampLookup Lookup(');
await source('InkjetOperator/Views/OrderListUserControl.cs','private async Task RefreshDataAsync(');
b.freezePanes.freezeRows(5);
wb.recalculate();
console.log((await wb.inspect({kind:'table',range:"'2 เปิดโค้ดตามนี้'!A6:C9",include:'values',tableMaxRows:4,tableMaxCols:3,maxChars:1500})).ndjson);
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?',options:{useRegex:true,maxResults:20},summary:'Final error scan'})).ndjson);
for(const [sheet,range,name] of [[a,'A1:C23','guide-overview'],[b,'A1:C13','guide-code'],[b,'A14:C23','guide-notes']]){
 const blob=await wb.render({sheetName:sheet.name,range,scale:1.4});
 await fs.writeFile(path.join(dir,`${name}.png`),new Uint8Array(await blob.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(wb)).save(path.join(dir,'Station_Barcode_Code_Guide.xlsx'));
await fs.writeFile(path.join(dir,'guide-sources.json'),JSON.stringify(sources,null,2));
console.log('Created Station_Barcode_Code_Guide.xlsx, 2 sheets; source anchors verified.');
