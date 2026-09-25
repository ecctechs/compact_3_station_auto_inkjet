# สร้างไฟล์ติดตั้งจากโค้ดล่าสุด แล้วเก็บไว้ที่ dist\
#
# ทำ 5 อย่างให้อัตโนมัติ
#   1. ตรวจว่าโปรแกรมไม่ได้เปิดค้างอยู่  (ถ้าเปิดอยู่ไฟล์จะถูกล็อกจน build ไม่ผ่าน)
#   2. หา Visual Studio
#   3. เพิ่มเลขเวอร์ชัน และสร้าง ProductCode / PackageCode ใหม่
#      (ถ้าไม่เปลี่ยน Windows จะไม่ยอมติดตั้งทับ)
#   4. build ทั้ง solution แบบ Release ด้วย devenv  (MSBuild สร้าง .vdproj ไม่ได้)
#   5. copy .msi ออกมาพร้อมเลขเวอร์ชันในชื่อไฟล์
#      (ก่อน copy แก้ .msi ให้ shortcut ชี้ไปที่ InkjetOperator.exe ตรง ๆ ไม่ใช่แบบ advertised)
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
$csproj = Join-Path $root 'InkjetOperator\InkjetOperator.csproj'
$sln    = Join-Path $root 'CompactInkjet.sln'
$dist   = Join-Path $root 'dist'
$log    = Join-Path $dist 'build.log'

$ok = $false

function Step($n, $msg) { Write-Host "[$n/6] $msg" -ForegroundColor Cyan }
function Note($msg)     { Write-Host "      $msg" -ForegroundColor DarkGray }
function Good($msg)     { Write-Host "      $msg" -ForegroundColor Green }

# throw ไม่ใช่ exit — exit จะข้าม finally ทำให้หน้าต่างปิดก่อนที่ผู้ใช้จะทันอ่าน
# ตั้งธงไว้ให้ catch รู้ว่าเป็นข้อความที่เราเขียนเอง ไม่ต้องโชว์บรรทัดในสคริปต์
$script:known = $false
function Fail($msg) { $script:known = $true; throw $msg }

# ใส่ไอคอนของโปรแกรมลงในไฟล์ติดตั้ง ทั้ง shortcut และหน้า Programs and Features
#
# ทำไมมาแก้ที่ตัว .msi แทนที่จะตั้งใน .vdproj — ช่อง AddRemoveProgramsIcon ของ VS
# เก็บค่าเป็นรหัสของ "ไฟล์ในโปรเจค" ซึ่งต้องประกาศไฟล์เพิ่มสองที่พร้อมกัน
# ลองเขียนมือแล้ว VS ตีความ .ico เป็น assembly จน build ไม่ผ่าน
# ("Unable to build assembly named 'app.ico'") ตัว .vdproj เองก็ไม่มีเอกสารกำกับ
# เดาต่อไปเสี่ยงพังทั้งไฟล์ เลยเลือกใช้ API ของ Windows Installer ตรง ๆ แทน
# ซึ่งมีเอกสารและตรวจผลย้อนได้ คือฝังไอคอนเข้าตาราง Icon แล้วชี้ ARPPRODUCTICON มาที่มัน
#
# ถ้าพลาดจะไม่ล้มทั้ง build เพราะ shortcut บน Desktop กับ Start Menu ได้ไอคอน
# จากตัว .exe อยู่แล้ว ขาดแค่รูปในหน้า Programs and Features
function Set-MsiIcons($msiPath, $icoPath) {
    $wi = New-Object -ComObject WindowsInstaller.Installer
    $db = $null
    try {
        # 1 = เปิดแบบแก้ไขได้ ต้องสั่ง Commit เองถึงจะเขียนลงไฟล์จริง
        $db = $wi.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $wi,
            [object[]]@([string]$msiPath, [int]1))

        $run = {
            param($sql, $rec)
            $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, [object[]]@([string]$sql))
            [void]$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, @($rec))
            [void]$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null)
        }

        # ลบของเดิมก่อน เผื่อมีการรันซ้ำบนไฟล์เดียวกัน
        try { & $run 'DELETE FROM `Icon` WHERE `Name` = ''app.ico''' $null } catch { }
        try { & $run 'DELETE FROM `Property` WHERE `Property` = ''ARPPRODUCTICON''' $null } catch { }

        # ตัวไฟล์ .ico ฝังลงไปเป็น stream ในตาราง Icon
        $rec = $wi.GetType().InvokeMember('CreateRecord', 'InvokeMethod', $null, $wi, [object[]]@([int]1))
        [void]$rec.GetType().InvokeMember('SetStream', 'InvokeMethod', $null, $rec,
            [object[]]@([int]1, [string]$icoPath))
        & $run 'INSERT INTO `Icon` (`Name`, `Data`) VALUES (''app.ico'', ?)' $rec

        & $run 'INSERT INTO `Property` (`Property`, `Value`) VALUES (''ARPPRODUCTICON'', ''app.ico'')' $null

        # ชี้ shortcut ทุกอันมาที่ไอคอนตัวนี้
        #
        # ตอนแรกเข้าใจว่าช่อง Icon ที่เว้นว่างใน vdproj แปลว่า "ใช้ไอคอนของไฟล์
        # ปลายทาง" แต่พอแกะไฟล์ที่ VS สร้างออกมาดู มันฝังไอคอนเอกสารสำเร็จรูป
        # ของตัวเองมาแทน ปลายทางเลยเห็นเป็นรูปกระดาษ ไม่ใช่รูปโปรแกรม
        & $run 'UPDATE `Shortcut` SET `Icon_` = ''app.ico'', `IconIndex` = 0' $null

        # ไอคอนสำเร็จรูปที่ VS ใส่มาไม่มีใครอ้างถึงแล้ว เอาออกไม่ให้ค้างในไฟล์
        try { & $run 'DELETE FROM `Icon` WHERE `Name` <> ''app.ico''' $null } catch { }

        [void]$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null)
        return $null
    }
    catch { return $_.Exception.Message }
    finally {
        # ต้องปล่อย COM ให้หมด ไม่งั้น handle ค้างจนแตะไฟล์ต่อไม่ได้
        if ($db) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($db) }
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($wi)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    }
}

# หา git.exe — ไม่พึ่ง PATH อย่างเดียว
#
# เครื่อง build บางเครื่องไม่ได้ลง Git for Windows แยก มีแต่ git ที่ติดมากับ
# Visual Studio หรือ GitHub Desktop ซึ่งไม่ได้อยู่ใน PATH เรียก git ตรง ๆ จึงพัง
# ด้วย "The term 'git' is not recognized" ทั้งที่ในเครื่องมี git อยู่แล้ว
function Find-Git {
    $cmd = Get-Command git.exe -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($cmd) { return $cmd.Source }

    $candidates = @(
        "$env:ProgramFiles\Git\cmd\git.exe",
        "${env:ProgramFiles(x86)}\Git\cmd\git.exe",
        "$env:LOCALAPPDATA\Programs\Git\cmd\git.exe"
    )

    # GitHub Desktop เก็บไว้ในโฟลเดอร์ตามเวอร์ชัน เอาตัวใหม่สุด
    $candidates += @(Get-ChildItem "$env:LOCALAPPDATA\GitHubDesktop\app-*\resources\app\git\cmd\git.exe" -ErrorAction SilentlyContinue |
        Sort-Object { try { [version]($_.FullName -replace '^.*\\app-([\d.]+)\\.*$', '$1') } catch { [version]'0.0' } } -Descending |
        ForEach-Object FullName)

    # git ที่ติดมากับ Visual Studio
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        foreach ($vs in @(& $vswhere -all -prerelease -property installationPath)) {
            if ($vs) {
                $candidates += Join-Path $vs 'Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe'
            }
        }
    }

    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c)) { return $c }
    }
    return $null
}

# เรียก git แล้วคืนผลแยก stdout / stderr / exit code
#
# ใส่ safe.directory เฉพาะคำสั่งนี้ผ่าน -c ไม่ได้แก้ config ของเครื่อง — ตอนรันแบบ
# Administrator เจ้าของโฟลเดอร์ไม่ตรงกับคนรัน git จะปฏิเสธด้วย "dubious ownership"
#
# ปิด Stop ชั่วคราว เพราะใน PowerShell 5.1 ข้อความใน stderr ของโปรแกรมภายนอก
# (เช่นคำเตือนเรื่อง CRLF) จะถูกแปลงเป็น error แล้วล้มทั้งสคริปต์
function Invoke-Git {
    $ErrorActionPreference = 'Continue'
    $safe = $root -replace '\\', '/'
    $all = & $script:git -c "safe.directory=$safe" -C $root @args 2>&1
    $code = $LASTEXITCODE

    $out = @($all | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] } | ForEach-Object { "$_" })
    $err = @($all | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] } | ForEach-Object { "$_" })

    [pscustomobject]@{ Code = $code; Out = $out; Err = ($err -join ' ').Trim() }
}

# ให้ shortcut ชี้ไปที่ตัว .exe ตรง ๆ ไม่ใช่ shortcut แบบ advertised
#
# Setup Project ของ Visual Studio สร้าง shortcut แบบ advertised เสมอและไม่มีช่อง
# ให้ปิดใน .vdproj — shortcut แบบนั้นชี้ไปที่ตัวติดตั้ง ไม่ใช่ไฟล์โปรแกรม
# ช่อง Target เป็นสีเทาแก้ไม่ได้ "Open file location" ใช้ไม่ได้ และทุกครั้งที่เปิด
# Windows Installer จะตรวจไฟล์ก่อน ถ้าเจอไฟล์ตั้งค่าที่ถูกแก้อาจเด้งหน้าต่างซ่อมแซม
# กลางกะแล้วเขียนทับไฟล์เดิม
#
# DISABLEADVTSHORTCUTS=1 เป็นวิธีที่ Windows Installer กำหนดไว้เอง: สร้าง shortcut
# ธรรมดาที่ชี้ไปยังไฟล์หลัก (KeyPath) ของ component ซึ่งคือ InkjetOperator.exe
#
# ต่างจากไอคอน — ถ้าทำไม่สำเร็จ build ต้องล้ม เพราะตัวติดตั้งจะได้ shortcut ผิดแบบ
function Set-MsiShortcutsToExe($msiPath) {
    $wi = New-Object -ComObject WindowsInstaller.Installer
    $db = $null
    try {
        $db = $wi.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $wi,
            [object[]]@([string]$msiPath, [int]1))

        $run = {
            param($sql)
            $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, [object[]]@([string]$sql))
            [void]$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null)
            [void]$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null)
        }

        $query = {
            param($sql, $cols)
            $rows = @()
            $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, [object[]]@([string]$sql))
            [void]$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null)
            while ($true) {
                $rec = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
                if (-not $rec) { break }
                $row = @()
                for ($i = 1; $i -le $cols; $i++) {
                    $row += $rec.GetType().InvokeMember('StringData', 'GetProperty', $null, $rec, [object[]]@([int]$i))
                }
                $rows += , $row
            }
            [void]$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null)
            return , $rows
        }

        try { & $run 'DELETE FROM `Property` WHERE `Property` = ''DISABLEADVTSHORTCUTS''' } catch { }
        & $run 'INSERT INTO `Property` (`Property`, `Value`) VALUES (''DISABLEADVTSHORTCUTS'', ''1'')'

        [void]$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null)

        # ตรวจย้อนจากตัวไฟล์จริง ไม่เชื่อแค่ว่าคำสั่งข้างบนไม่ error
        $prop = & $query 'SELECT `Value` FROM `Property` WHERE `Property` = ''DISABLEADVTSHORTCUTS''' 1
        if ($prop.Count -ne 1 -or $prop[0][0] -ne '1') { return 'เขียน DISABLEADVTSHORTCUTS ไม่ลง' }

        $shortcuts = & $query 'SELECT `Shortcut`, `Name`, `Component_` FROM `Shortcut`' 3
        if ($shortcuts.Count -eq 0) { return 'ไม่พบ shortcut ในตัวติดตั้งเลย' }

        $targets = @()
        foreach ($s in $shortcuts) {
            $comp = & $query "SELECT ``KeyPath`` FROM ``Component`` WHERE ``Component`` = '$($s[2])'" 1
            $file = if ($comp.Count -eq 1) {
                & $query "SELECT ``FileName`` FROM ``File`` WHERE ``File`` = '$($comp[0][0])'" 1
            } else { @() }

            # FileName เก็บเป็น "ชื่อสั้น|ชื่อยาว" เอาชื่อยาว
            $name = if ($file.Count -eq 1) { ($file[0][0] -split '\|')[-1] } else { '' }
            if ($name -ine 'InkjetOperator.exe') {
                $label = ($s[1] -split '\|')[-1]
                return "shortcut '$label' ไม่ได้ชี้ไปที่ InkjetOperator.exe (ชี้ไปที่ '$name')"
            }
            $targets += ($s[1] -split '\|')[-1]
        }

        return [pscustomobject]@{ Count = $shortcuts.Count; Names = $targets }
    }
    catch { return $_.Exception.Message }
    finally {
        if ($db) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($db) }
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($wi)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    }
}

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

    # ไม่ใช่ปัญหา แต่ควรรู้ไว้ ถ้า VS เปิดอยู่ devenv ที่สคริปต์เรียกจะโยนงาน
    # ไปให้ IDE ตัวที่เปิดอยู่ทำแทน แล้วจบตัวเองทันที รหัสที่คืนมาจึงเชื่อไม่ได้
    # (สคริปต์เลยไปดูบรรทัดสรุปใน log เป็นหลักแทน)
    if (Get-Process -Name 'devenv' -ErrorAction SilentlyContinue) {
        Note 'พบ Visual Studio เปิดอยู่ — build จะถูกส่งไปให้ IDE ตัวนั้นทำ'
    }

    # ── โค้ดที่จะถูก build ────────────────────────────────────
    #
    # ตัวติดตั้งต้องตรงกับ commit เสมอ ไม่ใช่ตรงกับสิ่งที่บังเอิญค้างอยู่ในโฟลเดอร์
    # เคยเกิดจริง — มีคน reset commit ทิ้ง 84 วินาทีก่อน build ตัวติดตั้งจึงได้โค้ด
    # ของ commit ก่อนหน้าโดยไม่มีใครรู้ตัว กว่าจะรู้ก็ตอนเอาไปลงเครื่องแล้ว
    #
    # กันสองชั้น บอกให้เห็นว่ากำลัง build จาก commit ไหน และหยุดถ้ามีของที่ยัง
    # ไม่ได้ commit เพราะไฟล์ที่ได้จะไม่ตรงกับ commit ไหนเลย ตามกลับไม่ได้
    $commit = ''
    if (-not (Test-Path (Join-Path $root '.git'))) {
        Note 'ไม่ใช่โฟลเดอร์ git — ข้ามการตรวจว่าตรงกับ commit ไหน'
    }
    else {
        # เป็นโฟลเดอร์ git แต่หา git ไม่เจอหรือ git ตอบไม่ได้ ต้องหยุด ห้ามข้ามเงียบ ๆ
        # เพราะการข้ามคือการปิดด่านกันโค้ดไม่ตรง commit ทิ้งไปทั้งด่าน
        $script:git = Find-Git
        if (-not $script:git) {
            Fail ('หา git ไม่เจอ — ติดตั้ง Git for Windows (https://git-scm.com) ' +
                  'หรือเปิดสคริปต์จากเครื่องที่มี Visual Studio / GitHub Desktop')
        }
        Note "git: $script:git"

        $r = Invoke-Git rev-parse --short HEAD
        if ($r.Code -ne 0 -or -not $r.Out) {
            Fail "git อ่าน commit ไม่ได้ — $($r.Err)"
        }

        $commit = "$($r.Out[0])".Trim()
        $branch = "$((Invoke-Git rev-parse --abbrev-ref HEAD).Out)".Trim()
        $subject = "$((Invoke-Git log -1 --pretty=format:%s).Out)".Trim()
        $when = "$((Invoke-Git log -1 --pretty=format:%ad --date=format:'%d/%m %H:%M').Out)".Trim()

        Note "branch $branch"
        Note "commit $commit  $when"
        Note "        $subject"

        $st = Invoke-Git status --porcelain
        if ($st.Code -ne 0) { Fail "git ตรวจไฟล์ที่ยังไม่ได้ commit ไม่ได้ — $($st.Err)" }

        # ไม่นับสองไฟล์นี้ เพราะสคริปต์เองเป็นคนแก้เลขเวอร์ชันในนั้น
        $dirty = @($st.Out |
            Where-Object { $_ -and $_ -notmatch 'CompactDemo\.vdproj' -and $_ -notmatch 'InkjetOperator\.csproj' })

        if ($dirty.Count -gt 0) {
            Write-Host ''
            Write-Host '      ไฟล์ที่ยังไม่ได้ commit:' -ForegroundColor DarkYellow
            $dirty | Select-Object -First 12 | ForEach-Object {
                Write-Host "      $_" -ForegroundColor DarkYellow
            }
            if ($dirty.Count -gt 12) { Note "... และอีก $($dirty.Count - 12) ไฟล์" }
            Write-Host ''
            Fail 'มีของที่ยังไม่ได้ commit — commit หรือ stash ก่อน ไม่งั้นตัวติดตั้งจะไม่ตรงกับ commit ไหนเลย'
        }

        Good 'โฟลเดอร์ตรงกับ commit ล่าสุดแล้ว'
    }

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

    # เลขบน .exe ต้องเดินตามเลขของตัวติดตั้งเสมอ ไม่งั้นเปิดดูคุณสมบัติไฟล์แล้ว
    # เห็นเลขค้างอยู่ที่เดิม ทั้งที่เพิ่งลงตัวใหม่ไป
    $cs = Get-Content $csproj -Raw -Encoding UTF8
    if ($cs -notmatch '<Version>\d+\.\d+\.\d+</Version>') {
        Fail "ไม่พบ <Version> ใน $csproj — ต้องมีไว้ให้สคริปต์บวกเลขให้"
    }
    $cs = $cs -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$new</Version>"
    Set-Content $csproj -Value $cs -Encoding UTF8 -NoNewline

    Good "$old  ->  $new   (ทั้งตัวติดตั้งและ .exe)"


    # ── คอมไพล์ตัวโปรแกรมเองก่อน ──────────────────────────────
    #
    # ไม่ปล่อยให้ devenv เป็นคนคอมไพล์ .exe เพราะถ้ามี Visual Studio เปิดอยู่
    # devenv จะโยนงานไปให้ IDE ตัวนั้นทำ แล้ว IDE ใช้สถานะโปรเจคที่จำไว้ในหน่วยความจำ
    # มองไม่เห็นว่า .csproj เพิ่งถูกแก้ จึงตอบว่า up-to-date แล้วข้ามการคอมไพล์
    # ผลคือได้ .msi ใหม่ที่ห่อ .exe ตัวเก่าไว้ข้างใน โดยไม่มีอะไรฟ้อง
    #
    # เจอจริง — csproj เป็น 1.0.22 แต่ exe ที่ถูกห่อยังเป็นของ build เมื่อ 16 นาทีก่อน
    #
    # dotnet build ไม่ผ่าน IDE จึงเห็นไฟล์บนดิสก์ตามจริงเสมอ
    Step 4 'คอมไพล์โปรแกรม'

    $csprojDir = Split-Path $csproj -Parent
    $buildOut = & dotnet build $csproj -c Release --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host ''
        $buildOut | Select-Object -Last 15 | ForEach-Object {
            Write-Host "      $_" -ForegroundColor DarkYellow
        }
        Fail 'คอมไพล์โปรแกรมไม่ผ่าน'
    }

    # ยืนยันว่า .exe ที่จะถูกห่อเป็นของรอบนี้จริง ไม่ใช่ของเก่าที่ค้างอยู่
    $exe = Join-Path $csprojDir 'bin\Release\net8.0-windows\InkjetOperator.exe'
    if (-not (Test-Path $exe)) { Fail "คอมไพล์ผ่านแล้วแต่ไม่พบ $exe" }

    $exeVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe).FileVersion
    if ($exeVersion -notlike "$new*") {
        Fail "เลขเวอร์ชันบน .exe เป็น $exeVersion ไม่ใช่ $new — แปลว่ายังเป็นไฟล์เก่า ปิด Visual Studio แล้วลองใหม่"
    }

    Good "คอมไพล์แล้ว  .exe เวอร์ชัน $exeVersion"

    # ── สร้างตัวติดตั้ง ──────────────────────────────────────
    Step 5 'สร้างตัวติดตั้ง (ใช้เวลาสักครู่ รอสักหน่อย)'
    Note "รายละเอียดถูกบันทึกไว้ที่ $log"

    # ลบ log รอบก่อนทิ้ง จะได้แน่ใจว่าบรรทัดที่อ่านทีหลังเป็นของรอบนี้จริง
    Remove-Item $log -Force -ErrorAction SilentlyContinue

    $start = Get-Date

    # เคยใช้ Start-Process -Wait แล้วค้างเป็นสิบนาทีทั้งที่ build เสร็จไปแล้ว
    # เพราะ -Wait ของ PowerShell รอ "ลูกหลาน" ของ process ด้วย ไม่ใช่แค่ตัวมันเอง
    # devenv ทิ้ง VBCSCompiler กับ MSBuild node ไว้ให้ค้างเผื่อ build รอบหน้า
    # พวกนี้หมดอายุเองราว 10-15 นาที สคริปต์เลยนั่งรอมันเปล่า ๆ
    # WaitForExit() รอเฉพาะ devenv ตัวเดียว ไม่สนลูกหลาน
    $p = Start-Process -FilePath $devenv `
        -ArgumentList @($sln, '/build', 'Release', '/out', $log) `
        -PassThru -NoNewWindow
    $p.WaitForExit()

    # devenv จบแล้วไม่ได้แปลว่า build จบ — ถ้ามี IDE เปิดอยู่ งานจะถูกโยนไปให้
    # ตัวนั้นทำต่อ ยึดบรรทัดสรุปท้าย log เป็นตัวชี้ขาดแทน
    $deadline = (Get-Date).AddMinutes(20)
    $summary  = $null
    while (-not $summary -and (Get-Date) -lt $deadline) {
        # devenv พิมพ์บรรทัดสรุปได้สองแบบ ขึ้นกับว่ามีโปรเจคไหนถูก build ใหม่จริงบ้าง
        #   Build: 2 succeeded, 0 failed, 0 up-to-date, 0 skipped
        #   Build: 1 succeeded or up-to-date, 0 failed, 0 skipped
        # เดิมรับแบบแรกอย่างเดียว พอเจอแบบที่สองจะหาบรรทัดสรุปไม่เจอแล้วรอจนครบ
        # 20 นาทีทั้งที่ build เสร็จไปแล้ว
        #
        # devenv อาจถือ handle ของ log ค้างไว้อยู่ อ่านไม่ได้ก็แค่วนมาใหม่
        try {
            $summary = Select-String -Path $log -Pattern '^=+ Build: (\d+) succeeded(?: or up-to-date)?, (\d+) failed' -ErrorAction Stop |
                       Select-Object -Last 1
        } catch { }
        if (-not $summary) { Start-Sleep -Milliseconds 500 }
    }

    if (-not $summary) {
        Fail "build ไม่จบภายใน 20 นาที  —  log อยู่ที่ $log"
    }

    $failed = [int]$summary.Matches[0].Groups[2].Value
    if ($failed -gt 0) {
        Write-Host ''
        Write-Host '      บรรทัดท้าย ๆ ของ log:' -ForegroundColor DarkYellow
        Get-Content $log -Tail 25 | ForEach-Object {
            Write-Host "      $_" -ForegroundColor DarkYellow
        }
        Fail "build ไม่ผ่าน $failed โปรเจค  —  log เต็มอยู่ที่ $log"
    }

    # devenv ข้ามโปรเจคตัวติดตั้งไปเงียบ ๆ ถ้าเครื่องไม่มี extension ที่อ่าน .vdproj ได้
    # บรรทัดสรุปยังขึ้น "succeeded" ตามปกติเพราะนับแค่ InkjetOperator ตัวเดียว
    # ถ้าไม่ดักตรงนี้จะไปพังตอนหา .msi ด้วยข้อความที่ไม่บอกสาเหตุ
    if (-not (Select-String -Path $log -Pattern 'Project: CompactDemo' -Quiet)) {
        Fail ('devenv ไม่ได้ build ตัวติดตั้ง (CompactDemo.vdproj) เลย — ' +
              'Visual Studio ตัวนี้ยังไม่มี extension "Microsoft Visual Studio Installer Projects" ' +
              'ติดตั้งจาก Extensions > Manage Extensions แล้วรันใหม่')
    }

    Good "build ผ่าน ใช้เวลา $([math]::Round(((Get-Date) - $start).TotalMinutes, 1)) นาที"

    # ── เก็บผลลัพธ์ ──────────────────────────────────────────
    Step 6 'เก็บไฟล์ติดตั้ง'

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

    $ico = Join-Path $root 'InkjetOperator\Resources\app.ico'
    if (Test-Path $ico) {
        $iconProblem = Set-MsiIcons $msi $ico
        if ($iconProblem) {
            Note "ใส่ไอคอนในไฟล์ติดตั้งไม่สำเร็จ: $iconProblem"
            Note 'ยังติดตั้งได้ตามปกติ แต่ไอคอนจะเป็นรูปสำเร็จรูปของ Visual Studio'
        } else {
            Good 'ใส่ไอคอนให้ shortcut และหน้า Programs and Features แล้ว'
        }
    }

    $sc = Set-MsiShortcutsToExe $msi
    if ($sc -is [string]) { Fail "ตั้ง shortcut ให้ชี้ไปที่ .exe ไม่สำเร็จ: $sc" }
    Good "shortcut $($sc.Count) อัน ($($sc.Names -join ', ')) ชี้ไปที่ InkjetOperator.exe โดยตรง ไม่ใช่แบบ advertised"

    # ใส่เลข commit ในชื่อไฟล์ด้วย — เลขเวอร์ชันอย่างเดียวไม่พอ เพราะมันย้อนกลับได้
    # เวลามีใคร reset แล้ว build ใหม่ จะได้เลขเดิมซ้ำแล้วทับไฟล์เก่าจนแยกไม่ออก
    $stamp = if ($commit) { "-$commit" } else { "" }
    $out = Join-Path $dist "CompactDemo-$new$stamp.msi"

    if (Test-Path $out) {
        Fail "มีไฟล์ $out อยู่แล้ว — สร้างจาก commit เดียวกันและเวอร์ชันเดียวกัน ลบทิ้งก่อนถ้าต้องการสร้างใหม่"
    }

    Copy-Item $msi $out

    $mb = [math]::Round((Get-Item $out).Length / 1MB, 1)

    Write-Host ''
    Write-Host '  =============== สำเร็จ ===============' -ForegroundColor Green
    Write-Host "  ไฟล์ติดตั้ง : $out" -ForegroundColor Green
    Write-Host "  เวอร์ชัน    : $new   ($mb MB)" -ForegroundColor Green
    if ($commit) { Write-Host "  จาก commit  : $commit" -ForegroundColor Green }
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
