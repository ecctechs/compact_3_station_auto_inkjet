@echo off
rem ดับเบิลคลิกไฟล์นี้เพื่อสร้างตัวติดตั้งจากโค้ดล่าสุด
rem รายละเอียดอยู่ใน tools\build-installer.ps1
rem
rem สคริปต์ PowerShell ค้างหน้าต่างรอ Enter ให้เองแล้ว ไฟล์นี้จึงไม่ต้อง pause ซ้ำ

rem 65001 = UTF-8 ถ้าไม่ตั้ง ข้อความภาษาไทยจะกลายเป็นขยะหรือไม่แสดงเลย
chcp 65001 >nul

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\build-installer.ps1"
