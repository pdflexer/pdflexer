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


Push-Location $PSScriptRoot;

dotnet publish -c release -f net6.0

$pdfs = "$PSScriptRoot/../../test/pdfs/pdfjs/*.pdf"
# $pdfs = "C:\source\Github\pdfium\testing\resources\*.pdf"

$outputPath = "$PSScriptRoot/../../test/results/pdfjs"
$outputPath = [IO.Path]::GetFullPath($outputPath);
$threads = 5;
$all = ls $pdfs
$size = $all.Count / ($threads * 10);
# '--strict', 
$all | % { $_ | % { '--pdf', $_ } } | Slice-Array -Size ($size * 2) | ForEach-Object -Throttle $threads  -Parallel { $outputPath = $using:outputPath; $a = @('--strict', '--type', 'rebuild', '--output', $outputPath, $_ ); .\bin\Release\net6.0\publish\pdflexer.PdfiumRegressionTester.exe @a; }

Pop-Location;