# ============================================================
#  build.ps1 - Build the Inkjet Operator MSI in one shot.
#  Steps: publish the app -> build the .msi
#
#  Usage:
#     .\build.ps1              (uses default version 1.0.0)
#     .\build.ps1 1.0.1        (set a new version when updating)
#  Or just double-click build.bat
# ============================================================

param(
    [string]$Version = ""   # e.g. 1.0.1  (empty = use default in Product.wxs)
)

$ErrorActionPreference = "Stop"

# --- Resolve paths from the script location ---
$InstallerDir = $PSScriptRoot
$RepoRoot     = Split-Path $InstallerDir -Parent
$Csproj       = Join-Path $RepoRoot "InkjetOperator\InkjetOperator.csproj"
$PublishDir   = Join-Path $RepoRoot "publish"
$Wxs          = Join-Path $InstallerDir "Product.wxs"
$MsiOut       = Join-Path $InstallerDir "InkjetOperator.msi"

# --- Make sure the wix global tool is on PATH ---
$env:PATH += ";$HOME\.dotnet\tools"

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Building Inkjet Operator installer" -ForegroundColor Cyan
if ($Version -ne "") { Write-Host "  Version: $Version" -ForegroundColor Cyan }
Write-Host "==================================================" -ForegroundColor Cyan

# --- Step 1: Publish (self-contained, .NET bundled in) ---
Write-Host ""
Write-Host "[1/2] Publishing app..." -ForegroundColor Yellow
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }

dotnet publish $Csproj -c Release -r win-x64 --self-contained true -o $PublishDir
if ($LASTEXITCODE -ne 0) { Write-Host "Publish FAILED. Stopping." -ForegroundColor Red; exit 1 }

$fileCount = (Get-ChildItem $PublishDir -File).Count
Write-Host "      Publish OK ($fileCount files)" -ForegroundColor Green

# --- Step 2: Build the MSI with WiX ---
Write-Host ""
Write-Host "[2/2] Building MSI..." -ForegroundColor Yellow

# Run from the Installer folder so license.rtf and ..\publish resolve correctly
Push-Location $InstallerDir
try {
    if ($Version -ne "") {
        # Pass the version to override the default in Product.wxs
        wix build "Product.wxs" -ext WixToolset.UI.wixext -d ProductVersion=$Version -o $MsiOut
    } else {
        wix build "Product.wxs" -ext WixToolset.UI.wixext -o $MsiOut
    }
} finally {
    Pop-Location
}
if ($LASTEXITCODE -ne 0) { Write-Host "MSI build FAILED." -ForegroundColor Red; exit 1 }

# --- Done ---
$sizeMB = [math]::Round((Get-Item $MsiOut).Length / 1MB, 1)
Write-Host ""
Write-Host "==================================================" -ForegroundColor Green
Write-Host "  SUCCESS - installer created" -ForegroundColor Green
Write-Host "  $MsiOut" -ForegroundColor White
Write-Host "  Size: $sizeMB MB" -ForegroundColor White
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""
