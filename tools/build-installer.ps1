# สร้างไฟล์ติดตั้งจากโค้ดล่าสุด แล้วเก็บไว้ที่ dist\
#
# ทำ 5 อย่างให้อัตโนมัติ
#   1. ตรวจว่าโปรแกรมไม่ได้เปิดค้างอยู่  (ถ้าเปิดอยู่ไฟล์จะถูกล็อกจน build ไม่ผ่าน)
#   2. หา Visual Studio
#   3. เพิ่มเลขเวอร์ชัน และสร้าง ProductCode / PackageCode ใหม่
#      (ถ้าไม่เปลี่ยน Windows จะไม่ยอมติดตั้งทับ)
#   4. build ทั้ง solution แบบ Release ด้วย devenv  (MSBuild สร้าง .vdproj ไม่ได้)
#   5. copy .msi ออกมาพร้อมเลขเวอร์ชันในชื่อไฟล์
#
# UpgradeCode ไม่ถูกแตะ — ตัวนี้ต้องคงเดิมตลอดอายุโปรแกรม เป็นตัวที่บอก Windows
# ว่านี่คือโปรแกรมเดียวกัน ถ้าเปลี่ยนจะกลายเป็นคนละตัวแล้วลงซ้อนกัน
#
# หน้าต่างจะค้างรอให้กด Enter เสมอ ไม่ว่าจะสำเร็จหรือพัง เพราะเวลาสั่ง
# "Run with PowerShell" หน้าต่างจะปิดทันทีที่จบ จนดูเหมือนไม่มีอะไรเกิดขึ้นเลย
# ใส่ -NoPause เมื่อเรียกจากสคริปต์อื่นที่ไม่มีคนนั่งดู

param([switch]$NoPause)

$ErrorActionPreference = 'Stop'

# หน้าต่าง console ปกติใช้ code page ที่อ่านภาษาไทยไม่ออก ถ้าไม่ตั้งตรงนี้ข้อความจะ
# กลายเป็นขยะหรือหายไปเลย จนดูเหมือนสคริปต์ไม่ทำงาน
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }

$root   = Split-Path $PSScriptRoot -Parent
$vdproj = Join-Path $root 'CompactDemo\CompactDemo.vdproj'
$sln    = Join-Path $root 'CompactInkjet.sln'
$dist   = Join-Path $root 'dist'
$log    = Join-Path $dist 'build.log'

$ok = $false

function Step($n, $msg) { Write-Host "[$n/5] $msg" -ForegroundColor Cyan }
function Note($msg)     { Write-Host "      $msg" -ForegroundColor DarkGray }
function Good($msg)     { Write-Host "      $msg" -ForegroundColor Green }

# throw ไม่ใช่ exit — exit จะข้าม finally ทำให้หน้าต่างปิดก่อนที่ผู้ใช้จะทันอ่าน
# ตั้งธงไว้ให้ catch รู้ว่าเป็นข้อความที่เราเขียนเอง ไม่ต้องโชว์บรรทัดในสคริปต์
$script:known = $false
function Fail($msg) { $script:known = $true; throw $msg }

try {
    Write-Host ''
    Write-Host '=== Compact Inkjet - build installer ===' -ForegroundColor White
    Write-Host ''

    if (-not (Test-Path $vdproj)) { Fail "ไม่พบไฟล์โปรเจคตัวติดตั้ง: $vdproj" }
    New-Item -ItemType Directory -Force -Path $dist | Out-Null

    # ── โปรแกรมเปิดค้างอยู่ไหม ───────────────────────────────
    # ถ้าเปิดอยู่ Windows จะล็อกไฟล์ใน bin\Release ทำให้ build ล้มด้วยข้อความยาว ๆ
    # ที่อ่านไม่รู้เรื่อง เช็คก่อนแล้วบอกตรง ๆ ดีกว่า
    Step 1 'ตรวจสภาพก่อนเริ่ม'

    if (Get-Process -Name 'InkjetOperator' -ErrorAction SilentlyContinue) {
        Fail 'โปรแกรม InkjetOperator เปิดค้างอยู่ — ปิดโปรแกรมก่อนแล้วรันใหม่'
    }
    Note 'ไม่มีโปรแกรมเปิดค้าง'

    # ── หา devenv ────────────────────────────────────────────
    Step 2 'หา Visual Studio'

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) { Fail 'ไม่พบ vswhere.exe — ต้องติดตั้ง Visual Studio ก่อน' }

    $devenv = & $vswhere -latest -property productPath
    if (-not $devenv -or -not (Test-Path $devenv)) { Fail 'ไม่พบ devenv.exe' }
    Note $devenv

    # ── เลขเวอร์ชันและ GUID ─────────────────────────────────
    Step 3 'เพิ่มเลขเวอร์ชัน'

    $text = Get-Content $vdproj -Raw -Encoding UTF8
    if ($text -notmatch '"ProductVersion" = "8:(\d+)\.(\d+)\.(\d+)"') {
        Fail 'อ่านเลขเวอร์ชันปัจจุบันจาก .vdproj ไม่ได้'
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
    Good "$old  ->  $new"

    # ── build ───────────────────────────────────────────────
    Step 4 'build แบบ Release (ใช้เวลาสักครู่ รอสักหน่อย)'
    Note "รายละเอียดถูกบันทึกไว้ที่ $log"

    $start = Get-Date

    # ใช้ Start-Process -Wait เพราะเรียก devenv ตรง ๆ มันคืนค่ากลับมาก่อนที่
    # ตัวติดตั้งจะเขียนไฟล์เสร็จ
    $p = Start-Process -FilePath $devenv `
        -ArgumentList @($sln, '/build', 'Release', '/out', $log) `
        -Wait -PassThru -NoNewWindow

    if ($p.ExitCode -ne 0) {
        Write-Host ''
        Write-Host '      บรรทัดท้าย ๆ ของ log:' -ForegroundColor DarkYellow
        if (Test-Path $log) {
            Get-Content $log -Tail 25 | ForEach-Object {
                Write-Host "      $_" -ForegroundColor DarkYellow
            }
        }
        Fail "devenv จบด้วยรหัส $($p.ExitCode)  —  log เต็มอยู่ที่ $log"
    }

    Good "build ผ่าน ใช้เวลา $([math]::Round(((Get-Date) - $start).TotalMinutes, 1)) นาที"

    # ── เก็บผลลัพธ์ ──────────────────────────────────────────
    Step 5 'เก็บไฟล์ติดตั้ง'

    $msi = Join-Path $root 'CompactDemo\Release\CompactDemo.msi'

    # เผื่อไฟล์ยังเขียนไม่เสร็จ รอสูงสุด 30 วินาที
    $waited = 0
    while (-not (Test-Path $msi) -and $waited -lt 30) {
        Start-Sleep -Seconds 1
        $waited++
    }
    if (-not (Test-Path $msi)) { Fail "build ผ่านแล้วแต่ไม่พบไฟล์ $msi" }

    # ตัวติดตั้งเก็บไว้ที่เดิม ถ้า build ไม่ได้สร้างใหม่ ไฟล์เก่าจะยังอยู่ แล้วจะถูก
    # copy ออกไปเหมือนสำเร็จ เอาไปติดตั้งแล้วจะได้โค้ดเก่าโดยไม่รู้ตัว
    if ((Get-Item $msi).LastWriteTime -lt $start) {
        Fail "$msi เป็นไฟล์เก่า — build ไม่ได้สร้างตัวติดตั้งใหม่"
    }

    $out = Join-Path $dist "CompactDemo-$new.msi"
    Copy-Item $msi $out -Force

    $mb = [math]::Round((Get-Item $out).Length / 1MB, 1)

    Write-Host ''
    Write-Host '  =============== สำเร็จ ===============' -ForegroundColor Green
    Write-Host "  ไฟล์ติดตั้ง : $out" -ForegroundColor Green
    Write-Host "  เวอร์ชัน    : $new   ($mb MB)" -ForegroundColor Green
    Write-Host ''
    Write-Host '  เอาไฟล์นี้ไปติดตั้งที่เครื่องปลายทางได้เลย' -ForegroundColor Gray
    Write-Host '  ติดตั้งทับตัวเก่าได้ ไม่ต้องถอนก่อน' -ForegroundColor Gray
    Write-Host ''

    $ok = $true
}
catch {
    Write-Host ''
    Write-Host '  =============== ไม่สำเร็จ ===============' -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ''

    # บอกตำแหน่งในสคริปต์เฉพาะตอนพังแบบไม่ได้ตั้งใจ ข้อความจาก Fail อ่านรู้เรื่องอยู่แล้ว
    if (-not $script:known) {
        Write-Host '  ข้อผิดพลาดที่ไม่ได้คาดไว้ รายละเอียดสำหรับผู้พัฒนา:' -ForegroundColor DarkGray
        Write-Host "  $($_.ScriptStackTrace)" -ForegroundColor DarkGray
        Write-Host ''
    }
}
finally {
    # ต้องค้างหน้าต่างไว้เสมอ ไม่งั้นคนที่สั่ง "Run with PowerShell" จะเห็นแค่
    # จอดำแวบเดียวแล้วหาย ไม่มีทางรู้ว่าพังตรงไหน
    if (-not $NoPause) {
        Write-Host 'กด Enter เพื่อปิดหน้าต่าง ...' -ForegroundColor White
        try { Read-Host | Out-Null } catch { Start-Sleep -Seconds 30 }
    }
}

if (-not $ok) { exit 1 }
