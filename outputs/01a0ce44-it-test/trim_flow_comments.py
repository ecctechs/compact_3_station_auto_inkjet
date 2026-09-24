from pathlib import Path
import json, subprocess

root = Path(__file__).resolve().parents[2]
manifest = Path(__file__).with_name('comment-changes.json')
changes = json.loads(manifest.read_text(encoding='utf-8'))

# เก็บเฉพาะทางเดินของงาน เหตุผลของเงื่อนไข และข้อจำกัดที่อ่านจากคำสั่งอย่างเดียวไม่ชัด
keep = {
    'เก็บชื่อลูกค้าไว้ส่งตอนสร้าง Job',
    'หยุดพิมพ์ 600 ms แล้วค้นให้',
    'กด OK ไปตรวจและสร้างงาน',
    'กด Enter ไปค้น Lot',
    'ครบ 600 ms ค้นให้โดยไม่เด้งเตือน',
    'รอบค้นอัตโนมัติไม่เด้งกล่องเตือน',
    'อ่าน Order No, Qty, วิธีพิมพ์และลูกค้า',
    'เปลี่ยนค่าบนจอ ยังไม่บันทึก DB',
    'กันกด OK ซ้ำระหว่างบันทึก',
    'ข้อมูลบนจอยังไม่ใช่ของ Barcode นี้',
    'ให้ตรวจค่าที่เพิ่งโหลดแล้วกด OK ใหม่',
    'ผู้ใช้ไม่ทำต่อเมื่อไฟล์แคลมป์ไม่พร้อม ให้หยุด',
    'ใช้ Qty ล่าสุด รวมค่าที่ผู้ใช้แก้',
    'พยายามลบ Job โดยตรงนี้ไม่ได้ตรวจผลลบ',
    'แสดงข้อความเดิม แม้ยังไม่ได้ยืนยันผลลบ Job',
    'เตือนแต่ยังทำ Routing ต่อ',
    'เตือนแต่ยังทำ IAI ต่อ',
    'อ่านค่า IAI ไปเก็บ ยังไม่สั่ง PLC',
    'ชื่อขึ้นต้น P- ให้ใช้ฝั่ง Plate',
    'ชื่ออื่นใช้ฝั่ง Shim',
    'ค้นไม่พบให้เก็บค่าว่าง',
    'ไม่มีไฟล์ให้เก็บค่าว่าง',
    'ไม่มีชื่อโปรแกรมทั้งสองฝั่ง ไม่สร้างแถว IAI',
    'บันทึก IAI โดยไม่ได้ตรวจผลที่คืนมา',
    'จบที่หน้าสแกนเดิม ไม่เปิดหน้า Station อื่น',
    'เก็บข้อมูล m1 เป็นงาน UV1',
    'เก็บข้อมูล m2 เป็นงาน UV2',
    'ใช้ Qty ต้นทาง ไม่ใช่ค่าที่แก้บนจอ',
    'มีชื่อโปรแกรมหรือข้อความอย่างใดอย่างหนึ่งก็เก็บ',
    'ตั้ง SQLite ให้อ่านอย่างเดียว',
    'ไม่มีคอลัมน์หรือแปลงไม่ได้ให้คืนค่าว่าง',
    'ส่ง JSON ไป JobController.create()',
    'ส่ง JSON ไป PatternController.create()',
    'ส่ง JSON ไป UvJobController.create()',
    'ส่ง JSON ไป IaiController.create()',
    'ส่ง JSON ไป PlanRoutingController.create()',
    'อ่านรายการจาก JobController.getAll()',
    'เปิดหน้าหลัก ให้ ApplyMenuLevel() เลือกหน้าตามโหมด',
    'ไปเลือกเมนูและหน้าเริ่มต้นตามโหมด',
    'แปลงเป็นเลข ถ้าแปลงไม่ได้จะได้ 0',
    'Mode 0 แสดง Scan Barcode และ Setting',
    'Mode 0 จะดึงหน้าสแกนมาไว้ด้านหน้า',
    'ตั้งรอบอ่านทุก 5 วินาที',
    'กำลังส่งเครื่องอยู่ ยังไม่เปลี่ยนรายการบนจอ',
    'รอบก่อนยังไม่จบ ไม่เริ่มซ้อน',
    'อ่านงานจาก Backend รวมงานที่ Barcode เพิ่งสร้าง',
    'จัดการคำขอเริ่มงานจากอีก Station',
    'กรองงานของ Station แล้วใส่ตาราง',
    'History ของ ST1 ดูงานได้ทุก Station',
    'เก็บงานที่กฎอนุญาตให้ Station นี้เห็น',
    'คืนค่าที่ปรับให้อยู่ในช่วงระยะที่รองรับ',
    'สร้าง Job และเลขงานในธุรกรรมเดียวกัน',
    'ล็อกการออกเลขงาน ไม่ให้สองคำขอได้เลขซ้ำ',
    'อ่านวันที่ไทยและเลขงานถัดไปของวัน',
    'ใช้ Barcode เป็นเลข Lot',
    'ใช้ Barcode อ้างอิง Pattern ERP',
    'ส่ง Job รวม ID ใหม่กลับไปบันทึก Pattern ต่อ',
    'แนบแผนงานเพื่อกรองตาม Station',
    'เตรียมบันทึก Pattern และตารางลูกพร้อมกัน',
    'ยกเลิกข้อมูลในธุรกรรมนี้เมื่อพลาด',
    'ล้าง UV เดิมของ Job นี้ก่อนใส่ชุดใหม่',
    'ล้างแผนงานเดิมของ Job นี้',
    'ค้นว่า Job นี้มีค่า IAI แล้วหรือยัง',
}
remaining = []
removed = 0
for file in sorted({c['file'] for c in changes}):
    p = root / file
    lines = p.read_bytes().splitlines(keepends=True)
    for c in [c for c in changes if c['file'] == file]:
        i = c['line'] - 1
        old = (c['before'] + ' // ' + c['comment']).encode('utf-8')
        assert lines[i].rstrip(b'\r\n') == old, f'Changed since annotation: {file}:{i+1}'
        if c['comment'].startswith('Flow ') or c['comment'] in keep:
            remaining.append(c)
        else:
            ending = lines[i][len(old):]
            lines[i] = c['before'].encode('utf-8') + ending
            removed += 1
    p.write_bytes(b''.join(lines))

# ลบคอมเมนต์ที่เราเพิ่มออกจากสำเนา แล้วเทียบกับโค้ดก่อนแก้
for file in sorted({c['file'] for c in changes}):
    current = (root/file).read_text(encoding='utf-8-sig').splitlines()
    before = subprocess.check_output(['git','show','HEAD:'+file],cwd=root).decode('utf-8-sig').splitlines()
    assert len(current) == len(before), f'Line numbers changed: {file}'
    for c in [c for c in remaining if c['file'] == file]:
        current[c['line']-1] = c['before']
    assert current == before, f'Non-comment edit: {file}'
manifest.write_text(json.dumps(remaining,ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Removed {removed} obvious comments; retained {len(remaining)} flow and business-rule comments.')
print('Verified: original code and line numbers unchanged.')
