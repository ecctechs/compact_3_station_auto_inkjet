@echo off
rem Capture the Compact Inkjet window without the "Activate Windows" watermark - see capture-window.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0capture-window.ps1" %*
pause
