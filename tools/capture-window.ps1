# แคปหน้าต่างโปรแกรม Compact Inkjet เป็นไฟล์ PNG — ไม่ติดลายน้ำ "Activate Windows"
#
# ลายน้ำเป็นชั้นที่ Windows วาดทับบนหน้าจอ ไม่ได้อยู่ในหน้าต่างโปรแกรม เครื่องมือแคป
# ทั่วไปถ่ายภาพจากหน้าจอจึงติดมาด้วย สคริปต์นี้ขอให้หน้าต่างวาดตัวเองลงรูป (PrintWindow)
# แทนการถ่ายจากจอ ภาพที่ได้จึงมีแต่โปรแกรม
#
# วิธีใช้: ดับเบิลคลิก capture-window.bat → ภายใน 3 วินาทีคลิกไปที่หน้าต่างโปรแกรม
# (หรือหน้า Order Detail ที่เปิดอยู่) แล้วรอเสียงติ๊ด ได้ไฟล์ในโฟลเดอร์ปลายทาง
#
#   -Delay 3              รอกี่วินาทีก่อนแคป ให้เวลาสลับไปหน้าต่างโปรแกรม
#   -OutDir <โฟลเดอร์>     ที่เก็บรูป (ค่าเริ่มต้น Desktop\CompactCapture)
#   -ProcessName <ชื่อ>    ชื่อโปรเซสของโปรแกรม (ค่าเริ่มต้น InkjetOperator)

param(
    [int]$Delay = 3,
    [string]$OutDir = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'CompactCapture'),
    [string]$ProcessName = 'InkjetOperator'
)

$ErrorActionPreference = 'Stop'
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class CaptureNative {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr ctx);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);
    [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr hWnd, int attr, out RECT rect, int size);
}
'@

# ทำงานแบบรู้ DPI ของแต่ละจอ ไม่งั้นขนาดหน้าต่างที่อ่านได้กับขนาดที่วาดจริงไม่ตรงกัน
# บนจอที่ตั้งสเกลไว้ (เช่น 150%) แล้วรูปจะโดนตัดหรือมีขอบดำ
[void][CaptureNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

$procs = @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)
if ($procs.Count -eq 0) {
    Write-Host "ไม่พบโปรแกรม $ProcessName ที่เปิดอยู่ — เปิดโปรแกรมก่อนแล้วลองใหม่" -ForegroundColor Red
    exit 1
}

if ($Delay -gt 0) {
    Write-Host "คลิกไปที่หน้าต่างที่ต้องการแคปภายใน $Delay วินาที ..." -ForegroundColor Cyan
    Start-Sleep -Seconds $Delay
}

# หน้าต่างที่อยู่ข้างหน้าสุดถ้าเป็นของโปรแกรม (เช่น Order Detail ที่เปิดซ้อนอยู่)
# ไม่ใช่ก็ใช้หน้าต่างหลักของโปรแกรม
$hwnd = [CaptureNative]::GetForegroundWindow()
[uint32]$fgPid = 0
[void][CaptureNative]::GetWindowThreadProcessId($hwnd, [ref]$fgPid)
if (-not ($procs.Id -contains [int]$fgPid)) {
    $hwnd = ($procs | Where-Object { $_.MainWindowHandle -ne [IntPtr]::Zero } | Select-Object -First 1).MainWindowHandle
}
if (-not $hwnd -or $hwnd -eq [IntPtr]::Zero) {
    Write-Host 'หาหน้าต่างของโปรแกรมไม่เจอ' -ForegroundColor Red
    exit 1
}
if ([CaptureNative]::IsIconic($hwnd)) {
    Write-Host 'หน้าต่างถูกพับอยู่ — เปิดขึ้นมาก่อนแล้วลองใหม่' -ForegroundColor Red
    exit 1
}

$win = New-Object CaptureNative+RECT
[void][CaptureNative]::GetWindowRect($hwnd, [ref]$win)
$w = $win.Right - $win.Left
$h = $win.Bottom - $win.Top

# ให้หน้าต่างวาดตัวเองลงรูป — 2 = PW_RENDERFULLCONTENT (วาดเนื้อหาแบบที่เห็นบนจอจริง)
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
$ok = [CaptureNative]::PrintWindow($hwnd, $hdc, 2)
$g.ReleaseHdc($hdc)
$g.Dispose()
if (-not $ok) {
    $bmp.Dispose()
    Write-Host 'แคปหน้าต่างไม่สำเร็จ' -ForegroundColor Red
    exit 1
}

# ตัดขอบที่มองไม่เห็นออก — หน้าต่างที่ขยายเต็มจอจะเลยขอบจอออกไปเล็กน้อย
# ใช้ขอบเขตที่มองเห็นจริงจาก DWM แล้วตัดให้อยู่ในจอที่หน้าต่างอยู่
$vis = New-Object CaptureNative+RECT
$hr = [CaptureNative]::DwmGetWindowAttribute($hwnd, 9, [ref]$vis, 16)   # 9 = DWMWA_EXTENDED_FRAME_BOUNDS
if ($hr -ne 0) { $vis = $win }
$screen = [System.Windows.Forms.Screen]::FromHandle($hwnd).Bounds
$left = [Math]::Max($vis.Left, $screen.Left);  $top = [Math]::Max($vis.Top, $screen.Top)
$right = [Math]::Min($vis.Right, $screen.Right); $bottom = [Math]::Min($vis.Bottom, $screen.Bottom)
$crop = New-Object System.Drawing.Rectangle ($left - $win.Left), ($top - $win.Top), ($right - $left), ($bottom - $top)
if ($crop.Width -gt 0 -and $crop.Height -gt 0 -and ($crop.Width -lt $w -or $crop.Height -lt $h)) {
    $cut = $bmp.Clone($crop, $bmp.PixelFormat)
    $bmp.Dispose()
    $bmp = $cut
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$file = Join-Path $OutDir ("Compact_{0:yyyy-MM-dd_HHmmss}.png" -f (Get-Date))
$bmp.Save($file, [System.Drawing.Imaging.ImageFormat]::Png)
$size = "{0}x{1}" -f $bmp.Width, $bmp.Height
$bmp.Dispose()

[Console]::Beep(1200, 120)
Write-Host "บันทึกแล้ว: $file ($size)" -ForegroundColor Green
