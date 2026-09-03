# สร้างไฟล์ติดตั้งจากโค้ดล่าสุด แล้วเก็บไว้ที่ dist\
#
# ทำ 4 อย่างให้อัตโนมัติ
#   1. เพิ่มเลขเวอร์ชันขึ้น 1
#   2. สร้าง ProductCode / PackageCode ใหม่  (ถ้าไม่เปลี่ยน Windows จะไม่ยอมติดตั้งทับ)
#   3. build ทั้ง solution แบบ Release ด้วย devenv  (MSBuild สร้าง .vdproj ไม่ได้)
#   4. copy .msi ออกมาพร้อมเลขเวอร์ชันในชื่อไฟล์
#
# UpgradeCode ไม่ถูกแตะ — ตัวนี้ต้องคงเดิมตลอดอายุโปรแกรม เป็นตัวที่บอก Windows
# ว่านี่คือโปรแกรมเดียวกัน ถ้าเปลี่ยนจะกลายเป็นคนละตัวแล้วลงซ้อนกัน

$ErrorActionPreference = 'Stop'

# หน้าต่าง cmd ปกติใช้ code page ที่อ่านภาษาไทยไม่ออก ถ้าไม่ตั้งตรงนี้ข้อความจะ
# กลายเป็นขยะหรือหายไปเลย จนดูเหมือนสคริปต์ไม่ทำงาน
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }

$root   = Split-Path $PSScriptRoot -Parent
$vdproj = Join-Path $root 'CompactDemo\CompactDemo.vdproj'
$sln    = Join-Path $root 'CompactInkjet.sln'
$dist   = Join-Path $root 'dist'
$log    = Join-Path $root 'dist\build.log'

function Step($n, $msg) { Write-Host "[$n/4] $msg" -ForegroundColor Cyan }
function Fail($msg) {
    Write-Host ''
    Write-Host "  FAILED: $msg" -ForegroundColor Red
    exit 1
}

Write-Host ''
Write-Host '=== Compact Inkjet - build installer ===' -ForegroundColor White
Write-Host ''

if (-not (Test-Path $vdproj)) { Fail "not found: $vdproj" }
New-Item -ItemType Directory -Force -Path $dist | Out-Null

# ── หา devenv ────────────────────────────────────────────
Step 1 'looking for Visual Studio'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) { Fail 'vswhere.exe not found - install Visual Studio first' }

$devenv = & $vswhere -latest -property productPath
if (-not $devenv -or -not (Test-Path $devenv)) { Fail 'devenv.exe not found' }
Write-Host "      $devenv" -ForegroundColor DarkGray

# ── เลขเวอร์ชันและ GUID ─────────────────────────────────
Step 2 'bumping version'

$text = Get-Content $vdproj -Raw -Encoding UTF8
if ($text -notmatch '"ProductVersion" = "8:(\d+)\.(\d+)\.(\d+)"') {
    Fail 'cannot read current ProductVersion'
}
$old = "$($Matches[1]).$($Matches[2]).$($Matches[3])"
$new = "$($Matches[1]).$($Matches[2]).$([int]$Matches[3] + 1)"

$product = [guid]::NewGuid().ToString().ToUpper()
$package = [guid]::NewGuid().ToString().ToUpper()

$text = $text -replace '"ProductVersion" = "8:[^"]*"', "`"ProductVersion`" = `"8:$new`""
$text = $text -replace '"ProductCode" = "8:\{[^}]*\}"',  "`"ProductCode`" = `"8:{$product}`""
$text = $text -replace '"PackageCode" = "8:\{[^}]*\}"',  "`"PackageCode`" = `"8:{$package}`""

# ต้องเป็น TRUE ไม่งั้นติดตั้งทับแล้วตัวเก่าไม่ถูกถอด จะเหลือสองตัวใน Add/Remove
$text = $text -replace '"RemovePreviousVersions" = "11:FALSE"', '"RemovePreviousVersions" = "11:TRUE"'

Set-Content $vdproj -Value $text -Encoding UTF8 -NoNewline
Write-Host "      $old  ->  $new" -ForegroundColor Green

$start = Get-Date

# ── build ───────────────────────────────────────────────
Step 3 'building Release (this takes a few minutes, please wait)'

# ใช้ Start-Process -Wait เพราะเรียก devenv ตรง ๆ มันคืนค่ากลับมาก่อนที่
# ตัวติดตั้งจะเขียนไฟล์เสร็จ
$p = Start-Process -FilePath $devenv `
    -ArgumentList @($sln, '/build', 'Release', '/out', $log) `
    -Wait -PassThru -NoNewWindow
$code = $p.ExitCode

if ($code -ne 0) {
    Write-Host ''
    if (Test-Path $log) { Get-Content $log -Tail 25 | ForEach-Object { Write-Host "      $_" -ForegroundColor DarkYellow } }
    Fail "devenv exit code $code  -  full log: $log"
}
Write-Host "      log: $log" -ForegroundColor DarkGray

# ── เก็บผลลัพธ์ ──────────────────────────────────────────
Step 4 'collecting output'

$msi = Join-Path $root 'CompactDemo\Release\CompactDemo.msi'

# เผื่อไฟล์ยังเขียนไม่เสร็จ รอสูงสุด 30 วินาที
$waited = 0
while (-not (Test-Path $msi) -and $waited -lt 30) {
    Start-Sleep -Seconds 1
    $waited++
}
if (-not (Test-Path $msi)) { Fail "build succeeded but $msi is missing" }

# ตัวติดตั้งเก็บไว้ที่เดิม ถ้า build ไม่ได้สร้างใหม่ ไฟล์เก่าจะยังอยู่ แล้วจะถูก
# copy ออกไปเหมือนสำเร็จ เอาไปติดตั้งแล้วจะได้โค้ดเก่าโดยไม่รู้ตัว
if ((Get-Item $msi).LastWriteTime -lt $start) {
    Fail "$msi is stale - the build did not produce a new installer"
}

$out = Join-Path $dist "CompactDemo-$new.msi"
Copy-Item $msi $out -Force

Write-Host ''
Write-Host "  DONE  ->  $out" -ForegroundColor Green
Write-Host ''
