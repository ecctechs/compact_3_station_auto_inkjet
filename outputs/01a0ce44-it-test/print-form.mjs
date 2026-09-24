import fs from 'node:fs/promises';
import {Workbook, SpreadsheetFile, FileBlob} from '@oai/artifact-tool';
const base=new URL('.',import.meta.url).pathname.replace(/^\/([A-Za-z]:)/,'$1');
const source=await SpreadsheetFile.importXlsx(await FileBlob.load(`${base}/Compact_Inkjet_IT_Test_fixed.xlsx`));
const manifest=JSON.parse(await fs.readFile(`${base}/manifest.json`,'utf8'));
const out=Workbook.create();
const pageSizes=[[6,6,3],[6,6,6,6,4],[5,5,5,3]];
const contexts=[
 'สแกน → ตรวจข้อมูล → ลงทะเบียน → ตรวจงานที่ Station 1 / 3',
 'รับงาน → เริ่มงาน → ส่ง MK / UV → ปล่อยคิว → จบงานหรือส่งต่อ ST3',
 'รับรายการ → เริ่ม / รับงานต่อ → ให้ ST1 ส่ง UV2 → ปล่อยคิว → จบงาน',
];
const resultManifest=[];
for(let k=0;k<manifest.length;k++){
 const old=source.worksheets.getItem(manifest[k].sheet);
 const records=old.getRange(`A11:G${manifest[k].end}`).values;
 const all=old.getRange(`A1:A${manifest[k].last}`).values.flat();
 const riskAt=all.indexOf('จุดเสี่ยงที่ควรตรวจ');
 const risks=all.slice(riskAt+1).filter(x=>typeof x==='string' && /^(B-|S1-|S3-)/.test(x));
 const sheet=out.worksheets.add(manifest[k].sheet);
 sheet.showGridLines=false;
 let row=1, offset=0;
 const starts=[], caseRows=[];
 for(let p=0;p<pageSizes[k].length;p++){
  starts.push(row);
  sheet.getRange(`A${row}:F${row}`).merge();
  const title=['Station Barcode (Mode 0)','Station 1 (Mode 1)','Station 3 (Mode 3)'][k];
  sheet.getRange(`A${row}`).values=[[`IT TEST  |  ${title}  |  ชุดที่ ${p+1}/${pageSizes[k].length}`]];
  sheet.getRange(`A${row}:F${row}`).format={font:{name:'Tahoma',size:15,bold:true},rowHeight:24,verticalAlignment:'center'};
  row++;
  sheet.getRange(`A${row}:C${row}`).merge();
  sheet.getRange(`D${row}:F${row}`).merge();
  sheet.getRange(`A${row}`).values=[['ผู้ทดสอบ ___________________ วันที่ ___________ รอบ ______']];
  sheet.getRange(`D${row}`).values=[['เครื่อง / เวอร์ชัน __________________________']];
  sheet.getRange(`A${row}:F${row}`).format.rowHeight=23;
  row++;
  sheet.getRange(`A${row}:F${row}`).merge();
  sheet.getRange(`A${row}`).values=[[contexts[k]+'  |  ติ๊กผล 1 ช่อง หากเทสไม่ได้ให้เขียน “รอเทส” พร้อมเหตุผล']];
  sheet.getRange(`A${row}:F${row}`).format.rowHeight=28;
  sheet.getRange(`A${row}:F${row}`).format.wrapText=true;
  row++;
  sheet.getRange(`A${row}:F${row}`).values=[['รหัส / รายการ','เตรียมและวิธีทดสอบ','ผลที่คาดหวัง','ผ่าน','ไม่ผ่าน','ผลจริง / Lot / หมายเหตุ']];
  sheet.getRange(`A${row}:F${row}`).format={fill:'#E6E6E6',font:{name:'Tahoma',size:12,bold:true},rowHeight:30,wrapText:true,horizontalAlignment:'center',verticalAlignment:'center'};
  const header=row;
  row++;
  for(let j=0;j<pageSizes[k][p];j++){
   const t=records[offset++];
   caseRows.push({row,id:t[0]});
   sheet.getRange(`A${row}:F${row}`).values=[[
    `${t[0]}\n${t[1]}`,
    `${t[2]}\n${t[3]}`,
    t[4], '□','□',
    '________________________\n\n________________________'
   ]];
   sheet.getRange(`A${row}:F${row}`).format={font:{name:'Tahoma',size:12},rowHeight:72,wrapText:true,verticalAlignment:'top'};
   sheet.getRange(`A${row}`).format.font={name:'Tahoma',size:12,bold:true};
   sheet.getRange(`D${row}:E${row}`).format={font:{name:'Tahoma',size:19},horizontalAlignment:'center',verticalAlignment:'center'};
   sheet.getRange(`F${row}`).format.font={name:'Tahoma',size:11,color:'#555555'};
   row++;
  }
  sheet.getRange(`A${header}:F${row-1}`).format.borders={preset:'all',style:'thin',color:'#929292'};
  if(p===pageSizes[k].length-1){
   sheet.getRange(`A${row}:F${row}`).merge();
   sheet.getRange(`A${row}`).values=[['จุดเสี่ยงที่ควรตรวจ (ยังไม่ใช่ผล Fail จนกว่าจะทดสอบจริง)']];
   sheet.getRange(`A${row}:F${row}`).format={font:{name:'Tahoma',size:11,bold:true},rowHeight:23};
   row++;
   for(const risk of risks){
    sheet.getRange(`A${row}:F${row}`).merge();
    sheet.getRange(`A${row}`).values=[[risk]];
    sheet.getRange(`A${row}:F${row}`).format={font:{name:'Tahoma',size:11},rowHeight:21,wrapText:true};
    row++;
   }
  }
 }
 const last=row-1;
 // Print dimensions are set in points by native Excel after the export.
 const widthsPt=[98,225,205,34,40,178];
 widthsPt.forEach((width,i)=>sheet.getRange(`${String.fromCharCode(65+i)}1:${String.fromCharCode(65+i)}${last}`).format.columnWidthPx=width*96/72);
 // Populate metadata font without overriding the already formatted test rows.
 for(const start of starts) sheet.getRange(`A${start+1}:F${start+2}`).format.font={name:'Tahoma',size:11};
 if(offset!==records.length) throw new Error('Lost test case');
 resultManifest.push({name:manifest[k].sheet,starts,last,caseRows,count:offset});
}
out.recalculate();
for(let i=0;i<3;i++){
 const p=await out.render({sheetName:resultManifest[i].name,range:'A1:F10',scale:1});
 await fs.writeFile(`${base}/print-preview-${i}.png`,new Uint8Array(await p.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(out)).save(`${base}/Compact_Inkjet_IT_Test_Print.xlsx`);
await fs.writeFile(`${base}/print-manifest.json`,JSON.stringify(resultManifest,null,2));
console.log(JSON.stringify(resultManifest.map(x=>({sheet:x.name,cases:x.count,pages:x.starts.length}))));
