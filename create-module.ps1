Push-Location $PSScriptRoot
try {
    dotnet publish -c release -f net6.0 src/PdfLexer.Powershell
    dotnet publish -c release -f net7.0 src/PdfLexer.Powershell
    dotnet publish -c release -f netstandard2.1 src/PdfLexer.Powershell
    [IO.Directory]::CreateDirectory("$pwd/.psm/PdfLexer/net7.0")
    [IO.Directory]::CreateDirectory("$pwd/.psm/PdfLexer/net6.0")
    [IO.Directory]::CreateDirectory("$pwd/.psm/PdfLexer/netstandard2.1")
    Copy-Item ./src/PdfLexer.Powershell/bin/release/net7.0/publish/* ./.psm/PdfLexer/net7.0/
    Copy-Item ./src/PdfLexer.Powershell/bin/release/net6.0/publish/* ./.psm/PdfLexer/net6.0/
    Copy-Item ./src/PdfLexer.Powershell/bin/release/netstandard2.1/publish/* ./.psm/PdfLexer/netstandard2.1/
    Copy-Item ./src/PdfLexer.Powershell/PdfLexer.psd1 ./.psm/PdfLexer/
    Copy-Item ./src/PdfLexer.Powershell/PdfLexer.psm1 ./.psm/PdfLexer/
}
finally {
    Pop-Location
}