@echo off
REM ============================================================
REM  Double-click this file to build the MSI installer.
REM  To set a version, run from cmd:   build.bat 1.0.1
REM ============================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
echo.
echo Press any key to close...
pause >nul
