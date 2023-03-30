## from https://stackoverflow.com/questions/45948580/slice-a-powershell-array-into-groups-of-smaller-arrays
function Slice-Array {

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $True)]
        [String[]]$Item,
        [int]$Size = 10
    )
    BEGIN { $Items = @() }
    PROCESS {
        foreach ($i in $Item ) { $Items += $i }
    }
    END {
        0..[math]::Floor($Items.count / $Size) | ForEach-Object { 
            $x, $Items = $Items[0..($Size - 1)], $Items[$Size..$Items.Length]; , $x
        } 
    }
}

$testType = "model-rebuild"

Push-Location $PSScriptRoot;

dotnet publish -c release -f net7.0

$pdfs = "$PSScriptRoot/../../test/pdfs/pdfjs/*.pdf"
# $pdfs = "C:\source\Github\pdfium\testing\resources\*.pdf"

$outputPath = "$PSScriptRoot/../../test/results/$testType"
$outputPath = [IO.Path]::GetFullPath($outputPath);
rm $outputPath/*.jsonl
$threads = 24;
[System.Collections.ArrayList]$all = @()
gci $pdfs | % { $all.Add($_) | Out-Null; }
gci "$PSScriptRoot/../../test/pdfs/pdfjs/need_repair/*.pdf" | % { $all.Add($_) | Out-Null; }
$all = $all | % { $_ | % { '--pdf', $_.FullName } }
$size = 2 * $all.Count / ($threads * 10)
$size = $size - ($size % 2) + 2;
$failures = $false;
# '--strict', 

$counter = @{ val = 0 }
$lock = [System.Threading.ReaderWriterLockSlim]::new()


$all | Slice-Array -Size $size | ForEach-Object -Throttle $threads  -Parallel { 
    $locker = $using:lock
    $t = $using:testType;
    $cnt = $using:counter;
    $iv = "";
    try{
        $locker.EnterWriteLock()
        $iv = $cnt.val.ToString();
        $cnt.val = $cnt.val + 1;
    }
    finally{
        if($locker.IsWriteLockHeld){
            $locker.ExitWriteLock()
        }
    }
    Write-Host $iv;
    $outputPath = $using:outputPath; $a = @('--strict', '--data', $PSScriptRoot,'--index', $iv, '--type', $t, '--output', $outputPath, $_ ); .\bin\Release\net7.0\publish\pdflexer.PdfiumRegressionTester.exe @a; if (!$?) { Write-Host "HAD FAILURES"; $failures = $true }; }
if ($failures) {
    Write-Error "HAD A FAILURE";
}
cat $outputPath/*res.jsonl | Out-File "$PSScriptRoot/bin/$testType.results.jsonl"
$results = @{}
foreach ($line in [io.file]::ReadAllLines("$PSScriptRoot/bin/$testType.results.jsonl")) {
    $data = $line | ConvertFrom-Json -AsHashtable;
    $nm = $data.Result;
    if ($null -eq $results[$nm]) {
        $results[$nm] = 1;
    } else {
        $results[$nm] = $results[$nm] + 1;
    }
}
"Current run results:"
$results;

cat $outputPath/*err.jsonl | Out-File "$PSScriptRoot/bin/$testType.pdf-info.jsonl"
$status = @{
    "0" = 'PdfLexerError'
    "1" = 'PdfLexerSkip'
    "2" = 'Differences'
    "3" = 'PdfiumError'
    "4" = 'Match'
}
$results = @{}
$errs = [System.Collections.ArrayList]@()
foreach ($line in [io.file]::ReadAllLines("$PSScriptRoot/bin/$testType.pdf-info.jsonl")) {
    $data = $line | ConvertFrom-Json -AsHashtable;
    if ($null -ne $data.FailureMsg -and $data.Status -ne 1) {
        $errs.Add($data.PdfName + " - " + $data.FailureMsg) | Out-Null
    }
    $nm = $status[$data.Status.ToString()]
    if ($null -eq $results[$nm]) {
        $results[$nm] = 1;
    } else {
        $results[$nm] = $results[$nm] + 1;
    }
}
"Current run stats:"
$results;
"Current run errors:"
$errs;
$errs.Count;
Pop-Location;