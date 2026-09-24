$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'print-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$xlsx = Join-Path $PSScriptRoot 'Compact_Inkjet_IT_Test_Print.xlsx'
$pdf = Join-Path $PSScriptRoot 'Compact_Inkjet_IT_Test_Print.pdf'
$excel = $null
$book = $null
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $originalMapPaper = $excel.MapPaperSize
    $excel.MapPaperSize = $false
    $book = $excel.Workbooks.Open($xlsx, 0, $false)
    $allIds = @()
    for ($i=1; $i -le 3; $i++) {
        $s = $book.Worksheets.Item($i)
        $info = $manifest[$i-1]
        foreach ($item in $info.caseRows) {
            $value = [string]$s.Range("A$($item.row)").Value2
            if (-not $value.StartsWith($item.id + "`n")) { throw "Wrong ID $($item.id)" }
            $allIds += $item.id
        }
        $setup = $s.PageSetup
        $setup.PaperSize = 9
        $setup.Orientation = 2
        $setup.Zoom = 100
        $setup.LeftMargin = 18
        $setup.RightMargin = 18
        $setup.TopMargin = 18
        $setup.BottomMargin = 18
        $setup.HeaderMargin = 6
        $setup.FooterMargin = 6
        $setup.CenterHorizontally = $true
        $setup.PrintGridlines = $false
        $setup.PrintHeadings = $false
        $setup.PrintArea = "`$A`$1:`$F`$$($info.last)"
        $setup.LeftFooter = 'Compact Inkjet - IT Test'
        $setup.RightFooter = '&P / &N'
        $s.ResetAllPageBreaks()
        foreach($start in $info.starts | Select-Object -Skip 1) {
            [void]$s.HPageBreaks.Add($s.Rows.Item([int]$start))
        }
        $s.Activate()
        $excel.ActiveWindow.DisplayGridlines = $false
        $excel.ActiveWindow.Zoom = 90
        $s.Range('A1').Select()
        Write-Output "Sheet ${i}: $($info.count) cases, width $($s.Range('A1:F1').Width) pt, planned pages $($info.starts.Count)"
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($setup)
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($s)
    }
    if (($allIds | Select-Object -Unique).Count -ne 61) { throw 'Expected 61 unique cases' }
    $book.Worksheets.Item(1).Activate()
    $excel.CalculateFull()
    $book.Save()
    $book.ExportAsFixedFormat(0, $pdf)
    $book.Close($false)
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($book)
    $book = $excel.Workbooks.Open($xlsx,0,$true)
    if ($book.Worksheets.Count -ne 3) { throw 'Native reopen failed' }
    Write-Output 'Excel saved, PDF exported, normal reopen verified'
}
finally {
    if ($book) { $book.Close($false); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($book) }
    if ($excel) { $excel.MapPaperSize = $originalMapPaper; $excel.Quit(); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($excel) }
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}
