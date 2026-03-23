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

Push-Location $PSScriptRoot; # 453

dotnet publish -c release -f net10.0

$pdfs = "$PSScriptRoot/../../test/pdfs/pdfjs/*.pdf"
# $pdfs = "C:\source\Github\pdfium\testing\resources\*.pdf"

$outputPath = "$PSScriptRoot/../../test/results/$testType"
$outputPath = [IO.Path]::GetFullPath($outputPath);
rm $outputPath/*.jsonl
$threads = 12;
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
    $outputPath = $using:outputPath; $a = @('--strict', '--data', $PSScriptRoot,'--index', $iv, '--type', $t, '--output', $outputPath, $_ ); .\bin\Release\net10.0\publish\pdflexer.PdfiumRegressionTester.exe @a; if (!$?) { Write-Host "HAD FAILURES"; $failures = $true }; }
if ($failures) {
    Write-Error "HAD A FAILURE";
}


function Add-Count {
    param(
        [hashtable]$Table,
        [string]$Key
    )

    if ([string]::IsNullOrWhiteSpace($Key)) {
        $Key = "<unknown>"
    }

    if ($null -eq $Table[$Key]) {
        $Table[$Key] = 1
    }
    else {
        $Table[$Key] = $Table[$Key] + 1
    }
}

function Add-GroupedValue {
    param(
        [hashtable]$Table,
        [string]$Key,
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Key)) {
        $Key = "<unknown>"
    }

    if ($null -eq $Table[$Key]) {
        $Table[$Key] = [System.Collections.ArrayList]@()
    }

    if (-not [string]::IsNullOrWhiteSpace($Value)) {
        $Table[$Key].Add($Value) | Out-Null
    }
}

function Get-OrderedCountTable {
    param(
        [hashtable]$Table,
        [string[]]$PreferredOrder = @()
    )

    $ordered = [ordered]@{}
    foreach ($key in $PreferredOrder) {
        if ($Table.ContainsKey($key)) {
            $ordered[$key] = $Table[$key]
        }
    }

    foreach ($entry in $Table.GetEnumerator() | Sort-Object Key) {
        if (-not $ordered.Contains($entry.Key)) {
            $ordered[$entry.Key] = $entry.Value
        }
    }

    return $ordered
}

function Write-Section {
    param(
        [string]$Title,
        $Value
    )

    $Title
    $Value
}

function Get-JsonLines {
    param(
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        return @()
    }

    $lines = [io.file]::ReadAllLines($Path)
    if ($null -eq $lines -or $lines.Length -eq 0) {
        return @()
    }

    $items = [System.Collections.ArrayList]@()
    foreach ($line in $lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $items.Add(($line | ConvertFrom-Json -AsHashtable)) | Out-Null
    }

    return $items
}

function Get-TransitionDescription {
    param(
        [hashtable]$ResultRecord,
        [hashtable]$InfoRecord
    )

    $result = [string]$ResultRecord["Result"]
    $message = [string]$ResultRecord["Message"]
    $currentStatus = if ($null -ne $InfoRecord) { [string]$InfoRecord["StatusName"] } else { "" }

    if ([string]::IsNullOrWhiteSpace($result)) {
        $result = "<unknown>"
    }

    if ($result -eq "Improvement" -or $result -eq "Regression") {
        $match = [regex]::Match($message, '^Status to (?<to>\w+) from (?<from>\w+)$')
        if ($match.Success) {
            return "{0}: {1} -> {2}" -f $result, $match.Groups["from"].Value, $match.Groups["to"].Value
        }
    }

    if ($result -eq "MatchErrorIncrease" -and -not [string]::IsNullOrWhiteSpace($currentStatus)) {
        return "{0}: {1}" -f $result, $currentStatus
    }

    if (-not [string]::IsNullOrWhiteSpace($message)) {
        return "{0}: {1}" -f $result, $message
    }

    return $result
}

function Get-DisplayEntries {
    param(
        [System.Collections.ArrayList]$Values,
        [int]$MaxItems = 12
    )

    if ($null -eq $Values -or $Values.Count -eq 0) {
        return @()
    }

    $sorted = $Values | Sort-Object
    if ($sorted.Count -le $MaxItems) {
        return $sorted
    }

    $items = [System.Collections.ArrayList]@()
    foreach ($item in $sorted | Select-Object -First $MaxItems) {
        $items.Add($item) | Out-Null
    }
    $items.Add(("... +{0} more" -f ($sorted.Count - $MaxItems))) | Out-Null
    return $items
}

cat $outputPath/*res.jsonl | Out-File "$PSScriptRoot/bin/$testType.results.jsonl"

$resultRecords = Get-JsonLines "$PSScriptRoot/bin/$testType.results.jsonl"
$resultCounts = @{}
$resultByPdf = @{}

foreach ($data in $resultRecords) {
    $resultName = [string]$data["Result"]
    Add-Count -Table $resultCounts -Key $resultName

    $pdfName = [string]$data["PdfName"]
    if (-not [string]::IsNullOrWhiteSpace($pdfName)) {
        $resultByPdf[$pdfName] = $data
    }
}

Write-Section "Current run results:" (Get-OrderedCountTable -Table $resultCounts -PreferredOrder @("Regression", "MatchErrorIncrease", "Improvement", "NewTest", "Match"))

cat $outputPath/*err.jsonl | Out-File "$PSScriptRoot/bin/$testType.pdf-info.jsonl"

$status = @{
    "0" = 'PdfLexerError'
    "1" = 'PdfLexerSkip'
    "2" = 'Differences'
    "3" = 'PdfiumError'
    "4" = 'Match'
}

$stats = @{}
$infoRecords = Get-JsonLines "$PSScriptRoot/bin/$testType.pdf-info.jsonl"
$infoByPdf = @{}
$errs = [System.Collections.ArrayList]@()

foreach ($data in $infoRecords) {
    if ($null -ne $data.FailureMsg -and $data.Status -ne 1) {
        $errs.Add($data.PdfName + " - " + $data.FailureMsg) | Out-Null
    }

    $statusName = $status[$data.Status.ToString()]
    $data["StatusName"] = $statusName
    Add-Count -Table $stats -Key $statusName

    $pdfName = [string]$data["PdfName"]
    if (-not [string]::IsNullOrWhiteSpace($pdfName)) {
        $infoByPdf[$pdfName] = $data
    }
}

Write-Section "Current run stats:" (Get-OrderedCountTable -Table $stats -PreferredOrder @("PdfLexerError", "PdfLexerSkip", "PdfiumError", "Differences", "Match"))

$transitionCounts = @{}
$transitionFiles = @{}

foreach ($entry in $resultByPdf.GetEnumerator()) {
    $pdfName = [string]$entry.Key
    $resultRecord = $entry.Value
    $infoRecord = $infoByPdf[$pdfName]

    $transition = Get-TransitionDescription -ResultRecord $resultRecord -InfoRecord $infoRecord
    Add-Count -Table $transitionCounts -Key $transition
    Add-GroupedValue -Table $transitionFiles -Key $transition -Value $pdfName
}

$interestingTransitions = $transitionCounts.GetEnumerator() |
    Where-Object { $_.Key -ne "Match" } |
    Sort-Object @{ Expression = "Value"; Descending = $true }, @{ Expression = "Key"; Descending = $false }

$transitionSummary = [System.Collections.ArrayList]@()
foreach ($transition in $interestingTransitions) {
    $transitionSummary.Add(("{0} = {1}" -f $transition.Key, $transition.Value)) | Out-Null
    foreach ($pdfName in Get-DisplayEntries -Values $transitionFiles[$transition.Key] -MaxItems 12) {
        $transitionSummary.Add("  $pdfName") | Out-Null
    }
}

if ($transitionSummary.Count -eq 0) {
    $transitionSummary.Add("No non-match transitions.") | Out-Null
}

Write-Section "Current run transitions:" $transitionSummary

"Current run errors:"
$errs
$errs.Count
Pop-Location
