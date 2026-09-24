import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {Workbook,SpreadsheetFile,FileBlob} from '@oai/artifact-tool';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../..');
const file=path.join(dir,'Station_Barcode_Code_Guide.xlsx');
const old=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
const prior=old.worksheets.getItemAt(2).name==='3 ไล่ Flow ตามโค้ด' ? old.worksheets.getItemAt(2) : old.worksheets.getItemAt(0);
const barcode=prior.getRange('A1:D24').values;
const w=Workbook.create();
const OL='InkjetOperator/Views/OrderListUserControl.cs', JS='InkjetOperator/Services/JobSendService.cs', MM='InkjetOperator/Services/MarkingMethodService.cs', PB='InkjetOperator/Services/PushButtonWatcher.cs', OD='InkjetOperator/Views/OrderDetailUserControl.cs';
async function ref(file,method){
 const lines=(await fs.readFile(path.join(root,file),'utf8')).split(/\r?\n/);
 const i=lines.findIndex(x=>x.includes(method+'(')&&/\b(private|public|internal|protected)\b/.test(x));
 if(i<0)throw new Error(`Missing ${file}: ${method}`);
 return `${path.basename(file)}:${i+1}`;
}
async function refs(...pairs){return (await Promise.all(pairs.map(([f,m])=>ref(f,m)))).join('\n');}
async function row(action,description,pairs,outcome){return [action,description,await refs(...pairs),outcome];}
const common=[
 await row('เปิดหน้า Order List','OnLoad(): ต่อ Backend ตาม PC_IP\nRefreshDataAsync(): อ่านรายการงาน\nStartPolling(): อ่านซ้ำทุก 5 วินาที\nPushButtonWatcher.Start(): เริ่มอ่านปุ่ม PLC',[[OL,'OnLoad'],[OL,'RefreshDataAsync'],[PB,'Start']],'ใช้หน้า Order List ร่วมกัน\nแต่กรองงานตาม Mode'),
 await row('เลือกงาน / ดูรายละเอียด','TblOrders_CellButtonClick(): รับปุ่มในแถว\nHandleRowButtonAsync(): แยกตามปุ่มที่กด\nLoadJobAsync(): อ่านข้อมูล Job ล่าสุด\nShowDetailDialogAsync(): เปิด Order Detail',[[OL,'TblOrders_CellButtonClick'],[OL,'HandleRowButtonAsync'],[OL,'ShowDetailDialogAsync']],'เปิดหน้าต่าง Order Detail\nปิดแล้วกลับ Order List'),
];
const st1=[
 await row('เปิดโปรแกรม\nMode 1','ApplyMenuLevel(): เลือกเมนูตาม MENU_LEVEL\nStationService.Current: ใช้กฎของ ST1\nRebindTable(): กรองสถานะและวิธีพิมพ์',[[ 'InkjetOperator/Views/MainShellForm.cs','ApplyMenuLevel'],[OL,'RebindTable'],[MM,'VisibleAt']],'เข้า Order List\nST1 ไม่แสดงงาน marking 10\nรหัส 3 ถูกแปลงเป็น 1 ก่อนใช้กฎ'),
 ...common,
 await row('กดเริ่มงาน','StartJobAsync(): อ่าน Job สดและตรวจ Station\nCanStartAt(): 10 เริ่ม ST3 ที่เหลือ ST1\nResolve(): แตกวิธีพิมพ์เป็นลำดับเครื่อง\n00 เข้า StartWithoutSendingAsync()',[[OL,'StartJobAsync'],[MM,'CanStartAt'],[MM,'Resolve']],'ผิด Station / ไม่มีแผน: หยุด\n00: เปลี่ยนเป็น Process\nโดยไม่ส่งเข้าเครื่อง'),
 await row('ยืนยันและจองคิว','ConfirmStartAsync(): แสดงแผนก่อนให้ยืนยัน\nQueueItemsFor(): ใส่เครื่องและเลขรอบ\nEnqueueMachinesAsync(): บันทึกคิวใน Backend\n22 จอง MK สองรอบ',[[OL,'ConfirmStartAsync'],[OL,'QueueItemsFor']],'ยกเลิกยืนยัน: ไม่จองคิว\nจองไม่ได้: แจ้งเหตุและหยุด'),
 await row('ขอสิทธิ์ใช้เครื่อง','SendQueuedForJobAsync(): เลือกคิว Job นี้\nClaimMachineAsync(): ขอเครื่องละหนึ่งครั้ง\nได้สิทธิ์แล้วเข้า SendStepAsync()\nเครื่องไม่ว่าง: ข้ามไปเครื่องอื่นในแผน',[[OL,'SendQueuedForJobAsync'],[OL,'SendStepAsync']],'ส่งได้เฉพาะคิวที่ขอสิทธิ์สำเร็จ\nรอบที่ 2 ของ MK ต้องรอปุ่มหน้างาน'),
 await row('ส่งข้อมูล MK','SendStepAsync(): ขอเปลี่ยนเป็น Process\nSendMkAsync(): เลือกหัวจาก Pattern\nSendToOneMkAsync(): ต่อ TCP → SQ → FW\n→ ส่งข้อความ FS/F1 ทุกช่อง → FM\nหัวที่งานนี้ไม่ใช้: สั่งหยุดถ้ามี IP',[[OL,'SendStepAsync'],[JS,'SendMkAsync'],[JS,'SendToOneMkAsync']],'ช่องว่างถูกส่งทับข้อความเก่า\nส่งผ่าน: บันทึกประวัติ MK\nเป็นผลส่งข้อมูล ไม่ใช่ยืนยันจำนวนพิมพ์'),
 await row('ส่งข้อมูล UV1 / UV2','SendUvAsync(): ตรวจข้อมูล, CPI.db3 และ IP\nUvProgramResolver.Resolve(): หา/เลือกโปรแกรม\nStopAsync(): ขอหยุด UV\nCpiWriteService.WriteAsync(): เขียนข้อมูลพิมพ์\nLoadAndStartAsync(): โหลดโปรแกรมแล้วสั่งเริ่ม',[[JS,'SendUvAsync']],'UV1 ใช้ตาราง MK063\nUV2 ใช้ตาราง MK067\nเขียน CPI ไม่ได้: ไม่สั่งโหลดและเริ่ม'),
 await row('เก็บผลส่งและคิว','SaveSendStepAsync(): เก็บประวัติขั้นที่ส่งผ่าน\nUpdateMachineQueueAsync(sent: true): กันส่งซ้ำ\nส่งไม่ผ่าน: คืนคิวเป็น pending\nไม่มีเครื่องส่งได้และไม่เคยส่ง: ล้างคิวทั้ง Job',[[OL,'SendStepAsync'],[OL,'SendQueuedForJobAsync']],'ยังอยู่ Order List\nแจ้งผลแยกแต่ละเครื่อง\nส่งไม่ผ่านในขั้นแรกอาจคืน Waiting'),
 await row('กดปุ่มหน้างาน','TickAsync(): อ่านบิต PLC และจับ 0 → 1\nOnPushButtonPressedAsync(): ปล่อยคิว MK\nReleaseMachineAsync(): ให้คิวถัดไปเป็น active\nไม่มีคิวต่อ: ResetHeadPositionAsync()',[[PB,'TickAsync'],[OL,'OnPushButtonPressedAsync'],[OL,'ResetHeadPositionAsync']],'ปล่อยเครื่องให้รอบ/งานถัดไป\nไม่ได้เปลี่ยน Job เป็น Success\nถือปุ่มค้างไม่นับเป็นกดซ้ำ'),
 await row('ส่งคิวที่ได้รับสิทธิ์','RefreshDataAsync(): อ่านสถานะใหม่\nProcessMachineQueueAsync(): เลือก active\nที่ยังไม่มี SentAt\nSendClaimedAsync(): อ่าน Job แล้วส่งผ่าน SendStep',[[OL,'ProcessMachineQueueAsync'],[OL,'SendClaimedAsync']],'ST1 เป็นผู้ส่งคิวให้ทุกเครื่อง\nไม่หยิบ pending มาส่งเอง\nส่งไม่ผ่านคืนคิวไปรอ'),
 await row('รับคำขอสำรองจาก ST3','ProcessRemoteStartsAsync(): หา remote_start=1\nRunRemoteStartAsync(): ตรวจแผนและประวัติ\nClaimRemoteStartAsync(): เปลี่ยนธงเป็น 2\nSendStepAsync(): ส่งขั้น/โปรแกรมที่ ST3 ฝาก\nSetRemoteStartAsync(): ล้างธงและฝากเหตุผิดพลาด',[[OL,'ProcessRemoteStartsAsync'],[OL,'RunRemoteStartAsync']],'ใช้กับปุ่มฝากส่งใน Detail\nเป็นคนละทางกับการจองคิวปกติ\nขั้นที่มีประวัติสำเร็จแล้วไม่ส่งซ้ำ'),
 await row('กดจบงาน','CompleteJobAsync(): อ่านประวัติล่าสุด\nCanCompleteAt(): ตรวจ Station ที่จบได้\nCheckSteps(): ตรวจขั้นและจำนวนรอบ\nไม่ครบ: ต้องยืนยัน MANUAL_COMPLETE\nล้างคิว → UpdateJobStatusAsync("Success")',[[OL,'CompleteJobAsync'],[MM,'CanCompleteAt'],[OL,'CheckSteps']],'10 / 11 / 12 จบที่ ST3\nรหัสอื่นจบที่ ST1 ได้\nSuccess แล้วออกจาก List'),
 await row('ยกเลิก / นำกลับ','CancelJobAsync(): ยืนยัน → ล้างคิว → Cancel\nแล้วล้างคำขอจาก ST3\nRestoreJobAsync(): ยืนยันใน History\n→ Waiting → ล้างคำขอส่งเดิม',[[OL,'CancelJobAsync'],[OL,'RestoreJobAsync']],'ยกเลิกไม่ได้ย้อนสิ่งที่พิมพ์ไปแล้ว\nนำกลับใช้ Job เดิมและยังมีประวัติส่งเดิม'),
];
const st3=[
 await row('เปิดโปรแกรม\nMode 3','ApplyMenuLevel(): เลือกเมนูตาม MENU_LEVEL\nStationService.Current: ใช้กฎของ ST3\nRebindTable(): กรองด้วย VisibleAt()\nSetupEvents(): ซ่อนแท็บ History ที่ ST3',[[ 'InkjetOperator/Views/MainShellForm.cs','ApplyMenuLevel'],[OL,'SetupEvents'],[MM,'VisibleAt']],'เข้า Order List\nแสดง marking 10 / 11 / 12\nรหัส 3 ถูกแปลงเป็น 1 ก่อนใช้กฎ'),
 ...common,
 await row('งานจาก Barcode / ST1','GetAllJobsAsync(): อ่าน Backend เดียวกัน\nRebindTable(): เลือกงานที่ ST3 มองเห็น\nResolve(): 10 ใช้ UV2, 11 ใช้ UV1 → UV2\n12 ใช้ MK → UV2',[[OL,'RefreshDataAsync'],[OL,'RebindTable'],[MM,'Resolve']],'ไม่มีการเปิดจอ ST3 จากเครื่องอื่น\nแต่ละเครื่องอ่านข้อมูลกลางเอง\n11 / 12 เริ่มจาก ST1'),
 await row('กดเริ่มงาน marking 10','StartJobAsync(): อ่าน Job สด\nCanStartAt(): อนุญาตเริ่ม 10 ที่ ST3\nConfirmStartAsync(): ให้ยืนยันแผน\nEnqueueMachinesAsync(): จองคิว UV2',[[OL,'StartJobAsync'],[MM,'CanStartAt'],[OL,'ConfirmStartAsync']],'ST3 แจ้งว่าเข้าคิวแล้ว\nรีเฟรชและจบฟังก์ชัน\nไม่ได้เรียก SendQueuedForJobAsync'),
 await row('ทำให้คิวไปต่อ','OnPushButtonPressedAsync(): ปล่อยเครื่อง UV2\nReleaseMachineAsync(): เลือกคิวถัดไปเป็น active\nST1 อ่านคิวผ่าน ProcessMachineQueueAsync()\n→ SendClaimedAsync() → SendStepAsync()',[[OL,'OnPushButtonPressedAsync'],[OL,'ProcessMachineQueueAsync'],[OL,'SendClaimedAsync']],'ต้องมีคิว active จึงส่งได้\npending อย่างเดียว ST1 ยังไม่ส่ง\nตรวจจุดเสี่ยงเรื่องคิวว่างท้ายชีต'),
 await row('ST1 ส่ง UV2 ให้','SendUvAsync(uvNumber: 2): ตรวจข้อมูล UV2\nหาโปรแกรมและเชื่อมต่อปลายทาง\nStopAsync(): ขอหยุดเครื่อง\nWriteAsync(): เขียน CPI.db3 ตาราง MK067\nLoadAndStartAsync(): โหลดโปรแกรมแล้วเริ่ม',[[OL,'SendStepAsync'],[JS,'SendUvAsync']],'ค่าการเชื่อมต่อและ CPI ต้องพร้อมที่ ST1\nST3 อ่านผลจาก Backend\nผลส่งสำเร็จยังไม่ใช่จบ Job'),
 await row('กดปุ่มหน้างาน UV2','PushButtonWatcher.TickAsync(): อ่านบิต PLC\nจับเฉพาะเปลี่ยน 0 → 1\nOnPushButtonPressedAsync(): ปล่อยคิว UV2\nถ้าไม่มีคิวต่อ: ResetHeadPositionAsync()',[[PB,'TickAsync'],[OL,'OnPushButtonPressedAsync']],'คิวถัดไปได้สิทธิ์ใช้ UV2\nไม่มีคิว: ไม่ขยับหัว UV2\nฟังก์ชันรีเซ็ตทำงานเฉพาะ MK\nJob ไม่ถูกจบด้วยปุ่มนี้'),
 await row('ทางสำรอง: ปุ่มใน Detail','ApplyStepButtons(): แสดงเมื่อเปิดตัวเลือก\nManualRemoteSendEnabled และมีขั้นถัดไป\nRequestRemoteStart(): ส่ง event แล้วปิด Detail\nShowDetailDialogAsync(): รับคำขอกลับหน้า List',[[OD,'ApplyStepButtons'],[OD,'RequestRemoteStart'],[OL,'ShowDetailDialogAsync']],'ค่าเริ่มต้นปุ่มนี้ปิดไว้\nไม่ใช่ทางกดเริ่มจาก List\nใช้ฝากขั้นถัดไปของงาน Process'),
 await row('ตรวจขั้นและ\nเลือกโปรแกรม','RequestRemoteStartFromDetailAsync(): อ่านสด\nหาขั้นที่ยังไม่ส่ง และต้องไม่ใช่ขั้นแรก\nRequestRemoteStartAsync(): ตรวจ Station ว่าง\nUvProgramResolver.Resolve(): เลือกไฟล์โปรแกรม\nยืนยันก่อนฝากคำขอ',[[OL,'RequestRemoteStartFromDetailAsync'],[OL,'RequestRemoteStartAsync']],'ผู้ใช้ยกเลิกเลือก/ยืนยัน: หยุด\nเลือกโปรแกรมที่ ST3 ก่อนฝาก\nST1 ใช้ชื่อนี้โดยไม่ถามเลือกซ้ำ'),
 await row('ฝาก ST1 และรอผล','SetRemoteStartAsync(): ฝากธง 1 + ขั้น + โปรแกรม\nST1: RunRemoteStartAsync() รับเป็นธง 2 แล้วส่ง\nShowRemoteOutcomeAsync(): อ่านผลทุก 700 ms\nรอได้ประมาณ 40 วินาที',[[OL,'RequestRemoteStartAsync'],[OL,'RunRemoteStartAsync'],[OL,'ShowRemoteOutcomeAsync']],'สำเร็จดูจากประวัติของขั้นที่ขอ\nล้มเหลวอ่าน remote_error\nST1 กำลังส่งอยู่: คงคำขอและให้รอ'),
 await row('คำขอส่งมีปัญหา','ShowRemoteOutcomeAsync(): ครบเวลารอ\nถ้ายังไม่อยู่สถานะกำลังส่ง: ล้างธงและคืน Waiting\nShowRemoteErrorsAsync(): ST3 แสดงเหตุจาก ST1\nST1 ต้องเปิดหน้า Order List และต่อ Backend ได้',[[OL,'ShowRemoteOutcomeAsync'],[OL,'ShowRemoteErrorsAsync']],'ตรวจข้อมูลและประวัติก่อนกดซ้ำ\nการรอหมดเวลาไม่ได้ตรวจชิ้นงานจริง'),
 await row('กดจบงานที่ ST3','CompleteJobAsync(): อ่าน Job และประวัติสด\nCanCompleteAt(): อนุญาตจบ 10 / 11 / 12 ที่ ST3\nCheckSteps(): ดูว่าส่งครบตามแผนหรือยัง\nไม่ครบต้องยืนยันจบด้วยมือ\nล้างคิว → บันทึก Success',[[OL,'CompleteJobAsync'],[MM,'CanCompleteAt'],[OL,'CheckSteps']],'งานออกจาก List หลังจบ\nการจบด้วยมือมี MANUAL_COMPLETE\nST3 ไม่มีแท็บ History ให้ย้อนดู'),
 await row('ยกเลิกงาน','CancelJobAsync(): ยืนยันยกเลิก\nClearMachineQueueAsync(): ล้างคิวของ Job\nUpdateJobStatusAsync(): บันทึก Cancel\nSetRemoteStartAsync(): ล้างคำขอค้าง',[[OL,'CancelJobAsync']],'งานออกจาก List\nหากนำกลับ ใช้ History ที่ ST1\nไม่ได้สั่งย้อนงานที่พิมพ์ไปแล้ว'),
];
function setup(name,rows,notes){
 const s=w.worksheets.add(name); const last=rows.length+notes.length+7;
 s.showGridLines=false;s.tabColor='#24445E';
 s.getRange(`A1:D${last}`).format={font:{name:'Tahoma',size:12,color:'#202A33'},wrapText:true,verticalAlignment:'center',rowHeight:18};
 [140,360,280,230].forEach((n,i)=>s.getRange(`${'ABCD'[i]}1:${'ABCD'[i]}${last}`).format.columnWidthPx=n*96/72);
 function line(r,t,h=30,bold=false){s.getRange(`A${r}:D${r}`).merge();s.getRange(`A${r}`).values=[[t]];s.getRange(`A${r}:D${r}`).format={rowHeight:h,font:{name:'Tahoma',size:r===2?17:12,bold,color:'#202A33'}};}
 line(2,name,32,true);line(3,'อ่านจากบนลงล่าง: ทำอะไร → เข้าฟังก์ชันไหน → เปิดไฟล์ไหน → ได้ผลอะไร',28);
 s.getRange('A5:D5').values=[['จังหวะใช้งาน','ฟังก์ชันและหน้าที่ (เรียงตาม Flow)','เปิดไฟล์:บรรทัด','หน้าจอ / ผลลัพธ์']];
 s.getRange('A5:D5').format={rowHeight:34,fill:'#24445E',font:{name:'Tahoma',size:12,bold:true,color:'#FFFFFF'},horizontalAlignment:'center'};
 s.getRange(`A6:D${5+rows.length}`).values=rows.map((r,i)=>[`${i+1}. ${r[0]}`,...r.slice(1)]);
 rows.forEach((r,i)=>{
  const counts=r.map((v,c)=>v.split('\n').reduce((sum,l)=>sum+Math.max(1,Math.ceil(l.length/[22,53,39,32][c])),0));
  s.getRange(`A${i+6}:D${i+6}`).format={rowHeight:Math.max(70,Math.max(...counts)*19+14),fill:i%2===0?'#EFF4F8':'#FFFFFF'};
  s.getRange(`A${i+6}`).format.font.bold=true;
 });
 notes.forEach((t,i)=>line(rows.length+7+i,t,46,i===0));
 s.freezePanes.freezeRows(5);return s;
}
const b=setup('อ่านโค้ด Flow Station barcode',barcode.slice(5,19).map(r=>[r[0].replace(/^\d+\.\s*/,''),...r.slice(1)]),barcode.slice(20,24).map(r=>r[0]??''));
// Retain the existing Barcode text exactly; only rename its sheet/title.
b.getRange('A3').values=[[barcode[2][0]]];b.getRange('A5:D19').values=barcode.slice(4,19);
const s1=setup('อ่านโค้ด Flow Station 1',st1,[
 'จุดที่ควรจำ: เริ่มงาน = จองและส่งข้อมูล • ปุ่มหน้างาน = ปล่อยเครื่อง • จบงาน = บันทึก Success',
 'จุดเสี่ยงที่ควรตรวจ: เครื่องไม่ว่างทั้งหมดและยังไม่เคยส่งสำเร็จ จะเข้าเงื่อนไขล้างคิว แม้ข้อความก่อนหน้าบอกว่าเข้าคิวรอแล้ว',
 'จุดเสี่ยงที่ควรตรวจ: เครื่องรับข้อมูลแล้วแต่บันทึก SentAt ไม่ได้ คิว active เดิมอาจถูกส่งซ้ำในรอบอ่านงานถัดไป',
 'อ้างอิงโค้ดในโปรเจกต์: Views = หน้าจอ • Services = แผนงาน/เครื่อง/PLC • พฤติกรรมหน้างานต้องตรวจด้วยเครื่องจริง',
]);
const s3=setup('อ่านโค้ด Flow Station 3',st3,[
 'จุดที่ควรจำ: ST3 ใช้ Backend ร่วมกับ ST1 ไม่ได้เปิดหน้าต่างบนเครื่อง ST1 และไม่ส่ง UV จากปุ่มเริ่มใน List เอง',
 'จุดเสี่ยงที่ควรตรวจ: กดเริ่ม marking 10 ตอนเครื่องว่างจะได้ pending; โค้ดอ่านคิวของ ST1 ส่งเฉพาะ active จึงอาจรอจนมีการปล่อยเครื่อง',
 'จุดเสี่ยงที่ควรตรวจ: ทางฝากส่งใน Detail ใช้ธงคำขอและประวัติ ส่วนทางปกติใช้คิวเครื่อง ต้องตรวจการทำพร้อมกันและการกดซ้ำหน้างาน',
 'อ้างอิงโค้ดปัจจุบัน: ST3 ไม่มี History • ปุ่มฝากส่งใน Detail ต้องเปิดตัวเลือกก่อน • ส่งครบในประวัติไม่ได้ยืนยันจำนวนชิ้นจริง',
]);
w.recalculate();
for(const [s,key] of [[b,'barcode'],[s1,'st1'],[s3,'st3']]){
 for(const [range,suffix] of [['A1:D10','top'],['A11:D23','bottom']]){
  const image=await w.render({sheetName:s.name,range,scale:1});
  await fs.writeFile(path.join(dir,`${key}-${suffix}.png`),new Uint8Array(await image.arrayBuffer()));
 }
}
await (await SpreadsheetFile.exportXlsx(w)).save(file);
const check=await SpreadsheetFile.importXlsx(await FileBlob.load(file));
if(JSON.stringify(check.worksheets.getItemAt(0).getRange('A5:D19').values)!==JSON.stringify(barcode.slice(4,19)))throw new Error('Barcode content changed');
console.log((await check.inspect({kind:'sheet',include:'id,name'})).ndjson);
console.log(`Verified existing Barcode table preserved; added ${st1.length} ST1 steps and ${st3.length} ST3 steps.`);
