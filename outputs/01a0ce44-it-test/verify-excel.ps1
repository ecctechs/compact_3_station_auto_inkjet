$ErrorActionPreference = 'Stop'
$taskPath = Join-Path $PSScriptRoot 'Compact_Inkjet_IT_Test_fixed.xlsx'
$excel = $null
$book = $null
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $excel.AskToUpdateLinks = $false
    $m = [Type]::Missing
    # CorruptLoad = 0 means normal open, not repair or data extraction.
    $book = $excel.Workbooks.Open($taskPath, 0, $true)
    if ($book.Worksheets.Count -ne 3) { throw 'Wrong worksheet count' }
    $counts = @(15, 28, 18)
    $pass = ([char]0x2611).ToString() + ' ' + ([char]0x0E1C).ToString() + ([char]0x0E48).ToString() + ([char]0x0E32).ToString() + ([char]0x0E19).ToString()
    for ($i=1; $i -le 3; $i++) {
        $sheet = $book.Worksheets.Item($i)
        if ($sheet.ListObjects.Count -ne 1) { throw "Missing table in sheet $i" }
        if ($sheet.ListObjects.Item(1).DataBodyRange.Rows.Count -ne $counts[$i-1]) { throw "Wrong case count in sheet $i" }
        if ($sheet.Range('F11').Validation.Type -ne 3) { throw "Missing dropdown in sheet $i" }
        $before = $sheet.Range('F11').Value2
        $sheet.Range('F11').Value2 = $pass
        $excel.CalculateFull()
        if (-not $sheet.Range('B8').Value2.EndsWith(' 1')) { throw "Pass count formula failed in sheet $i" }
        if (-not $sheet.Range('D8').Value2.EndsWith(" $($counts[$i-1]-1)")) { throw "Pending count failed in sheet $i" }
        $sheet.Range('F11').Value2 = $before
        $excel.CalculateFull()
        if (-not $sheet.Range('B8').Value2.EndsWith(' 0')) { throw "Restore failed in sheet $i" }
        if ([string]::IsNullOrEmpty($sheet.PageSetup.PrintArea)) { throw "Missing print area in sheet $i" }
        Write-Output "Sheet ${i}: normal Excel open, $($counts[$i-1]) cases, dropdown, live formulas and print area OK"
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($sheet)
    }
    Write-Output "Excel RepairMode: $($book.RepairMode)"
    Write-Output 'Native Excel verification passed. File opened with normal load, no repair requested.'
}
finally {
    if ($book) { $book.Close($false); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($book) }
    if ($excel) { $excel.Quit(); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($excel) }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
