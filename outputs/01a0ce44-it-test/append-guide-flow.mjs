import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {SpreadsheetFile,FileBlob} from '@oai/artifact-tool';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../..');
const file=path.join(dir,'Station_Barcode_Code_Guide.xlsx');
const w=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
const before=[0,1].map(i=>JSON.stringify(w.worksheets.getItemAt(i).getRange('A1:C24').values));
const name='3 ไล่ Flow ตามโค้ด';
const s=w.worksheets.add(name);
const scan='InkjetOperator/Views/ScanBarcodeUserControl.cs';
const sql='InkjetOperator/Services/SqliteDataService.cs';
const api='InkjetOperator/Services/ApiClient.cs';
async function ref(p,needle){
 const lines=(await fs.readFile(path.join(root,p),'utf8')).split(/\r?\n/);
 const i=lines.findIndex(x=>x.includes(needle));
 if(i<0)throw new Error('Source not found '+p+' '+needle);
 return `${path.basename(p)}:${i+1}`;
}
const data=[
 ['1. เปิดโปรแกรม\nMode 0','Program.Main() → MainShellForm()\n→ ApplyMenuLevel() เลือกเมนูตาม MENU_LEVEL',`${await ref('InkjetOperator/Program.cs','Application.Run(new Views.MainShellForm());')}\n${await ref('InkjetOperator/Views/MainShellForm.cs','private void ApplyMenuLevel()')}`,'แสดง Scan Barcode\nและเมนู Setting'],
 ['2. กด Enter\nหลังสแกน','TxtBarcode_KeyDown()\n→ LoadLot(quiet: false)',await ref(scan,'private void TxtBarcode_KeyDown('),'อยู่หน้า Scan Barcode\nเริ่มค้น Lot ยังไม่สร้าง Job'],
 ['3. ค้นและแสดง Lot','LoadLot() → OpenSourceDb() → CanConnect()\n→ GetLotSummary(barcode)\n→ เติม Order No / Marking Method / Qty',`${await ref(scan,'private bool LoadLot(')}\n${await ref(sql,'public LotSummary? GetLotSummary(')}`,'อยู่หน้าเดิม\nจำ Lot ใน _loadedBarcode\nรอผู้ใช้ตรวจข้อมูล'],
 ['4. แก้ Qty\nถ้าจำเป็น','BtnEditQty_Click() → เปิด InputDialog\n→ ตรวจจำนวนเต็ม > 0 → เปลี่ยน txtQty',await ref(scan,'private void BtnEditQty_Click('),'ปิดกล่องแล้วกลับหน้าเดิม\nยังไม่บันทึกฐานข้อมูล'],
 ['5. กด OK','BtnConfirm_Click() → ValidateForm()\n→ ผ่านแล้วปิดปุ่ม OK ชั่วคราว\n→ ProcessBarcodeAsync()',`${await ref(scan,'private async void BtnConfirm_Click(')}\n${await ref(scan,'private bool ValidateForm()')}`,'อยู่หน้าเดิม\nถ้าเพิ่งโหลด Lot ในขั้นนี้\nต้องตรวจแล้วกด OK อีกครั้ง'],
 ['6. ตรวจความพร้อม','ProcessBarcodeAsync() → GetServices()\n→ CanConnect() → PingAsync()\n→ ConfirmClampDatabase()',await ref(scan,'private async Task ProcessBarcodeAsync('),'ต่อ SQLite / Backend ไม่ได้ให้หยุด\nไฟล์ Clamp ไม่พร้อมให้เลือก\nว่าจะลงทะเบียนต่อหรือไม่'],
 ['7. อ่านข้อมูลเต็ม','GetPatternDetail() → GetUvDetail()\n→ GetPlanRouting()\nอ่านจาก PrintData.db3',`${await ref(sql,'public CreatePatternRequest? GetPatternDetail(')}\n${await ref(sql,'public List<UvJobItem> GetUvDetail(')}\n${await ref(sql,'public CreatePlanRoutingRequest? GetPlanRouting(')}`,'ได้ชุดข้อมูลเตรียมส่ง\nถ้าไม่พบ Pattern จะหยุด'],
 ['8. สร้าง Job','CreateJobAsync() → POST /job/create\n→ JobController.create() → print_jobs\n→ ส่ง Job ID กลับมา',`${await ref(api,'public async Task<(PrintJob? job,')}\n${await ref('InkjetBackend/controllers/JobController.js','static async create(req, res)')}`,'งานใหม่สถานะ Waiting\nใช้ Job ID ผูกข้อมูลขั้นถัดไป'],
 ['9. บันทึก Pattern','CreatePatternAsync() → POST /pattern/create\n→ PatternController.create()',`${await ref(api,'public async Task<(PatternDetail? pattern,')}\n${await ref('InkjetBackend/controllers/PatternController.js','static async create(req, res)')}`,'ผูกข้อมูล MK / สายพาน / Servo\nถ้าล้มเหลว พยายามลบ Job\nแล้วหยุด'],
 ['10. บันทึก UV','ถ้ามีข้อมูล: CreateUvJobDataAsync()\n→ POST /uv-job/create\n→ UvJobController.create()',`${await ref(api,'public async Task<(bool ok, string? error)> CreateUvJobDataAsync(')}\n${await ref('InkjetBackend/controllers/UvJobController.js','static async create(req, res)')}`,'เก็บข้อมูล UV ของ Job\nไม่มีข้อมูลให้ข้าม\nล้มเหลวเตือนแล้วยังทำต่อ'],
 ['11. บันทึก Routing','ถ้ามีข้อมูล: CreatePlanRoutingAsync()\n→ POST /plan-routing/create\n→ PlanRoutingController.create()',`${await ref(api,'public async Task<(bool ok, string? error)> CreatePlanRoutingAsync(')}\n${await ref('InkjetBackend/controllers/PlanRoutingController.js','static async create(req, res)')}`,'เก็บแผนงานของ Job\nไม่มีข้อมูล / บันทึกไม่ได้\nเตือนแล้วยังทำต่อ'],
 ['12. บันทึกค่า IAI','SyncIaiAsync() → ClampService.Lookup()\n→ CreateIaiAsync() → POST /iai/create\n→ IaiController.create()',`${await ref(scan,'private static async Task SyncIaiAsync(')}\n${await ref('InkjetOperator/Services/ClampService.cs','public static ClampLookup Lookup(')}\n${await ref('InkjetBackend/controllers/IaiController.js','static async create(req, res)')}`,'อ่านค่าจาก mydatabase.db3\nไม่มีชื่อโปรแกรม UV ให้ข้าม\nขั้นนี้ไม่ได้สั่งแคลมป์วิ่ง'],
 ['13. ลงทะเบียนจบ','Notify.Success() → ClearForm()\n→ ClearLotInfo() → txtBarcode.Focus()',await ref(scan,'private void ClearForm()'),'อยู่หน้า Scan Barcode เดิม\nล้างข้อมูล รอสแกนถัดไป\nปุ่ม OK กลับมาใช้ได้'],
 ['14. Station อื่นรับงาน','OrderListUserControl.StartPolling()\n→ RefreshDataAsync() → GetAllJobsAsync()\n→ GET /job/getAll แล้วกรองตาม Station',`${await ref('InkjetOperator/Views/OrderListUserControl.cs','private void StartPolling()')}\n${await ref('InkjetOperator/Views/OrderListUserControl.cs','private async Task RefreshDataAsync(')}\n${await ref(api,'public async Task<(List<PrintJob> jobs,')}`,'แสดงงานใน Order List\nของเครื่อง Station 1 / 3\nเป็นการอ่าน Backend ร่วมกัน']
];
s.showGridLines=false; s.tabColor='#24445E';
s.getRange('A1:D24').format={font:{name:'Tahoma',size:12,color:'#202A33'},rowHeight:18,wrapText:true,verticalAlignment:'center'};
[140,300,260,220].forEach((n,i)=>s.getRange(`${'ABCD'[i]}1:${'ABCD'[i]}24`).format.columnWidthPx=n*96/72);
function line(r,t,h=26,size=12,bold=false){
 s.getRange(`A${r}:D${r}`).merge();s.getRange(`A${r}`).values=[[t]];
 s.getRange(`A${r}:D${r}`).format={rowHeight:h,font:{name:'Tahoma',size,bold,color:'#202A33'}};
}
line(2,'Station Barcode: ไล่ Flow พร้อมเปิดโค้ด',29,17,true);
line(3,'อ่านจากบนลงล่างตามการใช้งานจริง คอลัมน์สุดท้ายบอกว่าหน้าจออยู่ที่ไหนและได้ผลอะไร',26);
s.getRange('A5:D5').values=[['จังหวะใช้งาน','ฟังก์ชันที่เรียกต่อ','เปิดไฟล์:บรรทัด','หน้าจอ / ผลลัพธ์']];
s.getRange('A5:D5').format={fill:'#24445E',font:{name:'Tahoma',size:12,bold:true,color:'#FFFFFF'},horizontalAlignment:'center',rowHeight:26};
s.getRange(`A6:D${5+data.length}`).values=data;
data.forEach((_,i)=>{
 const r=6+i;s.getRange(`A${r}:D${r}`).format={rowHeight:66,fill:i%2===0?'#EFF4F8':'#FFFFFF'};
 s.getRange(`A${r}`).format.font={name:'Tahoma',size:12,bold:true,color:'#202A33'};
});
line(21,'ทางเข้าอีกแบบ: TextChanged → ตั้งเวลา 600 ms → AutoLookupTimer_Tick() → LoadLot(quiet: true) แล้วกลับเข้า Flow ข้อ 3',28,11);
line(22,'จุดสำคัญ: หน้าสแกนไม่ได้เปิดหน้า Station ถัดไปหลังสร้างงาน ทุก Station อ่านงานจาก Backend เดียวกัน',26,12,true);
line(23,'ไฟล์หน้าจออยู่ InkjetOperator/Views/ ส่วน Service อยู่ InkjetOperator/Services/ และ Controller อยู่ InkjetBackend/controllers/',27,11);
line(24,'ข้อมูลเพิ่มเติม: Pattern / UV / Routing บันทึกแยกคำขอ การแจ้งสำเร็จจึงไม่ได้ยืนยันว่าข้อมูลเสริมครบหรือพิมพ์แล้ว',26,11);
s.freezePanes.freezeRows(5);
w.recalculate();
console.log((await w.inspect({kind:'table',range:`'${name}'!A6:D8`,include:'values',tableMaxRows:3,tableMaxCols:4,maxChars:1400})).ndjson);
for(const [range,out] of [['A1:D12','flow-top.png'],['A13:D24','flow-bottom.png']]){
 const pic=await w.render({sheetName:name,range,scale:1.2});
 await fs.writeFile(path.join(dir,out),new Uint8Array(await pic.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(w)).save(file);
const check=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
for(let i=0;i<2;i++)if(JSON.stringify(check.worksheets.getItemAt(i).getRange('A1:C24').values)!==before[i])throw new Error('Existing sheet changed');
if(check.worksheets.getItem(name).getRange('A6:D19').values.length!==14)throw new Error('Missing flow rows');
console.log('Saved 3-sheet workbook. Original 2 sheets values preserved. Added 14 flow steps.');
