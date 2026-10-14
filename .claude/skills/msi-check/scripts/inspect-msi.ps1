# ตรวจไฟล์ติดตั้ง CompactDemo-*.msi ว่าพร้อมส่งลูกค้าหรือยัง — อ่านอย่างเดียว ไม่แก้ไฟล์
#
# วิธีใช้:  powershell -NoProfile -ExecutionPolicy Bypass -File inspect-msi.ps1 <path ของ .msi> [path ของ CompactDemo.vdproj]
# ผลลัพธ์: ทีละบรรทัด  PASS / FAIL / WARN  <รหัสข้อ>  <คำอธิบาย>  แล้วสรุปท้าย
# exit code: 0 = ไม่มี FAIL · 1 = มี FAIL · 2 = เปิดไฟล์ไม่ได้

param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [string]$Vdproj = ''
)

$ErrorActionPreference = 'Stop'
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }

if (-not (Test-Path $Msi)) { Write-Output "FAIL  OPEN  ไม่พบไฟล์ $Msi"; exit 2 }
$Msi = (Resolve-Path $Msi).Path

# หา .vdproj เองถ้าไม่ได้ส่งมา — ไฟล์ MSI ปกติอยู่ใน dist\ ข้าง ๆ CompactDemo\
if (-not $Vdproj) {
    $guess = Join-Path (Split-Path (Split-Path $Msi -Parent) -Parent) 'CompactDemo\CompactDemo.vdproj'
    if (Test-Path $guess) { $Vdproj = $guess }
}

$wi = New-Object -ComObject WindowsInstaller.Installer
try {
    # 0 = อ่านอย่างเดียว
    $db = $wi.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $wi, [object[]]@([string]$Msi, [int]0))
} catch {
    Write-Output "FAIL  OPEN  เปิดไฟล์ MSI ไม่ได้: $($_.Exception.Message)"; exit 2
}

function Rows($sql, $cols) {
    $out = @()
    try {
        $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, [object[]]@([string]$sql))
    } catch { return , @() }   # ไม่มีตารางนี้ในไฟล์
    [void]$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null)
    while ($r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)) {
        $out += , @(1..$cols | ForEach-Object { $r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, [object[]]@([int]$_)) })
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($r)
    }
    [void]$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null)
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($v)
    return , $out
}

$props = @{}
foreach ($p in (Rows 'SELECT `Property`, `Value` FROM `Property`' 2)) { $props[$p[0]] = $p[1] }

$fails = 0; $warns = 0
function Report($status, $id, $text) {
    if ($status -eq 'FAIL') { $script:fails++ }
    if ($status -eq 'WARN') { $script:warns++ }
    Write-Output ("{0,-4}  {1,-9} {2}" -f $status, $id, $text)
}

$ver = $props['ProductVersion']
Write-Output "ไฟล์: $Msi"
Write-Output "ProductVersion: $ver · ProductCode: $($props['ProductCode'])"
Write-Output ''

# ── VERSION: เลขบน .exe ต้องตรงกับเลขของตัวติดตั้ง ──
$files = Rows 'SELECT `File`, `FileName`, `Version`, `Component_` FROM `File`' 4
$exe = $files | Where-Object { ($_[1] -split '\|')[-1] -ieq 'InkjetOperator.exe' } | Select-Object -First 1
if (-not $exe) {
    Report FAIL VERSION 'ไม่มี InkjetOperator.exe อยู่ในตัวติดตั้ง'
} elseif ($exe[2] -like "$ver*") {
    Report PASS VERSION "InkjetOperator.exe เวอร์ชัน $($exe[2]) ตรงกับตัวติดตั้ง $ver"
} else {
    Report FAIL VERSION "InkjetOperator.exe เป็น $($exe[2]) แต่ตัวติดตั้งบอก $ver — ห่อ exe ผิดรุ่น หรือแก้เลขในตัวติดตั้งทีหลัง"
}

# ── NAME: ชื่อไฟล์ต้องบอกเลขเวอร์ชันเดียวกับข้างใน ──
$name = Split-Path $Msi -Leaf
if ($name -match '^CompactDemo-(\d+\.\d+\.\d+)-([0-9a-f]{7,})\.msi$') {
    if ($Matches[1] -eq $ver) { Report PASS NAME "ชื่อไฟล์บอกเวอร์ชัน $($Matches[1]) และ commit $($Matches[2]) ตรงกับข้างใน" }
    else { Report FAIL NAME "ชื่อไฟล์บอกเวอร์ชัน $($Matches[1]) แต่ข้างในเป็น $ver" }
} else {
    Report WARN NAME "ชื่อไฟล์ไม่ใช่แบบ CompactDemo-<เวอร์ชัน>-<commit>.msi ที่ build-installer.bat สร้าง — ตามกลับไม่ได้ว่ามาจาก commit ไหน"
}

# ── DOTNET: ต้องหยุดติดตั้งถ้าเครื่องไม่มี .NET 8 Desktop Runtime ──
$lc  = Rows 'SELECT `Condition` FROM `LaunchCondition`' 1
$as  = Rows 'SELECT `Property`, `Signature_` FROM `AppSearch`' 2
$sig = Rows 'SELECT `Signature`, `FileName`, `MinVersion` FROM `Signature`' 3
$drl = Rows 'SELECT `Signature_`, `Path` FROM `DrLocator`' 2
$hasLc  = [bool]($lc | Where-Object { $_[0] -eq 'Installed OR NETDESKTOP8' })
$hasAs  = [bool]($as | Where-Object { $_[0] -eq 'NETDESKTOP8' -and $_[1] -eq 'NetDesktop8' })
$hasSig = [bool]($sig | Where-Object { $_[0] -eq 'NetDesktop8' -and $_[1] -ieq 'System.Windows.Forms.dll' -and $_[2] -like '8.*' })
$hasDrl = [bool]($drl | Where-Object { $_[0] -eq 'NetDesktop8' -and $_[1] -like '*dotnet\shared\Microsoft.WindowsDesktop.App*' })
if ($hasLc -and $hasAs -and $hasSig -and $hasDrl) {
    Report PASS DOTNET 'มีการเช็ค .NET 8 Desktop Runtime ครบ เครื่องที่ไม่มีจะหยุดตั้งแต่หน้าแรก'
} else {
    $miss = @()
    if (-not $hasLc)  { $miss += 'LaunchCondition' }
    if (-not $hasAs)  { $miss += 'AppSearch' }
    if (-not $hasSig) { $miss += 'Signature' }
    if (-not $hasDrl) { $miss += 'DrLocator' }
    Report FAIL DOTNET ("ไม่มีการเช็ค .NET 8 (ขาด $($miss -join ', ')) — ลงเครื่องที่ไม่มี runtime ได้ แต่เปิดโปรแกรมไม่ขึ้น " +
                        'สาเหตุที่พบบ่อย: build จาก Visual Studio ตรง ๆ ไม่ผ่าน build-installer.bat')
}
$fw = $lc | Where-Object { $_[0] -match 'NETFRAMEWORK|MsiNetAssemblySupport' }
if ($fw) { Report WARN DOTNET "ยังมีการเช็ค .NET Framework ตัวเก่าอยู่ ($($fw[0][0])) ซึ่งผ่านเสมอ ไม่มีประโยชน์" }

# ── SHORTCUT: ต้องชี้ไปที่ exe ตรง ๆ ไม่ใช่แบบ advertised ──
$shortcuts = Rows 'SELECT `Shortcut`, `Name`, `Component_`, `Icon_` FROM `Shortcut`' 4
$components = @{}
foreach ($c in (Rows 'SELECT `Component`, `KeyPath` FROM `Component`' 2)) { $components[$c[0]] = $c[1] }
$fileById = @{}
foreach ($f in $files) { $fileById[$f[0]] = ($f[1] -split '\|')[-1] }
if ($props['DISABLEADVTSHORTCUTS'] -ne '1') {
    Report FAIL SHORTCUT 'ไม่มี DISABLEADVTSHORTCUTS=1 — shortcut จะเป็นแบบ advertised เปิดแล้ว Windows อาจซ่อมแซมทับไฟล์ตั้งค่า'
} elseif ($shortcuts.Count -eq 0) {
    Report FAIL SHORTCUT 'ไม่มี shortcut เลย'
} else {
    $bad = $shortcuts | Where-Object { $fileById[$components[$_[2]]] -ine 'InkjetOperator.exe' }
    if ($bad) { Report FAIL SHORTCUT "shortcut '$(($bad[0][1] -split '\|')[-1])' ไม่ได้ชี้ไปที่ InkjetOperator.exe" }
    else { Report PASS SHORTCUT "shortcut $($shortcuts.Count) อัน ชี้ไปที่ InkjetOperator.exe ตรง ๆ" }
}

# ── ICON: ไอคอนโปรแกรมใน shortcut และหน้า Programs and Features ──
$icons = Rows 'SELECT `Name` FROM `Icon`' 1
$iconOk = $props['ARPPRODUCTICON'] -eq 'app.ico' -and ($icons | Where-Object { $_[0] -eq 'app.ico' }) -and
          -not ($shortcuts | Where-Object { $_[3] -ne 'app.ico' })
if ($iconOk) { Report PASS ICON 'ไอคอนโปรแกรมครบ ทั้ง shortcut และหน้า Programs and Features' }
else { Report WARN ICON 'ไอคอนไม่ครบ — ติดตั้งได้ แต่จะเห็นรูปสำเร็จรูปของ Visual Studio' }

# ── SCOPE: ติดตั้งให้ทุกบัญชีในเครื่อง ──
# FolderForm_AllUsers จะเป็น ME ก็ได้ ไม่มีผล เพราะตัวเลือกนี้ถูกซ่อน (InstallAllUsersVisible = 0)
# ALLUSERS ใน Property คือค่าที่ใช้จริง
if ($props['ALLUSERS'] -eq '2' -or $props['ALLUSERS'] -eq '1') { Report PASS SCOPE "ALLUSERS=$($props['ALLUSERS']) ติดตั้งให้ทุกบัญชีในเครื่อง" }
else { Report FAIL SCOPE 'ไม่ได้ตั้ง ALLUSERS — จะติดตั้งให้เฉพาะคนที่กดติดตั้ง บัญชีอื่นเปิดโปรแกรมไม่เจอ' }

# ── UPGRADE: UpgradeCode ต้องคงเดิมตลอดอายุโปรแกรม ──
if ($Vdproj -and (Test-Path $Vdproj)) {
    $text = Get-Content $Vdproj -Raw -Encoding UTF8
    if ($text -match '"UpgradeCode" = "8:(\{[^}]+\})"') {
        if ($Matches[1] -ieq $props['UpgradeCode']) { Report PASS UPGRADE 'UpgradeCode ตรงกับโปรเจค ติดตั้งทับตัวเก่าได้' }
        else { Report FAIL UPGRADE "UpgradeCode $($props['UpgradeCode']) ไม่ตรงกับโปรเจค $($Matches[1]) — จะลงซ้อนกับตัวเก่าเป็นสองโปรแกรม" }
    }
} else {
    Report WARN UPGRADE 'หา CompactDemo.vdproj ไม่เจอ เลยเทียบ UpgradeCode ไม่ได้'
}

# ── FILES: ไฟล์ที่โปรแกรมต้องใช้ ──
$need = 'InkjetOperator.exe', 'InkjetOperator.dll', 'InkjetOperator.runtimeconfig.json', 'AntdUI.dll',
        'Setting.default.config', 'uv.config', 'patterns.xml'
$have = $files | ForEach-Object { ($_[1] -split '\|')[-1] }
$missing = $need | Where-Object { $have -notcontains $_ }
if ($missing) { Report FAIL FILES "ขาดไฟล์: $($missing -join ', ')" }
else { Report PASS FILES "มีไฟล์ที่โปรแกรมต้องใช้ครบ $($need.Count) ไฟล์ (รวมทั้งหมด $($files.Count) ไฟล์)" }

[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($db)
[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($wi)

Write-Output ''
if ($fails -gt 0) { Write-Output "สรุป: ยังไม่พร้อมส่ง — FAIL $fails ข้อ, WARN $warns ข้อ"; exit 1 }
elseif ($warns -gt 0) { Write-Output "สรุป: ส่งได้ แต่มี WARN $warns ข้อ ควรดูก่อน"; exit 0 }
else { Write-Output 'สรุป: พร้อมส่ง ผ่านทุกข้อ'; exit 0 }
