Push-Location $PSScriptRoot
try {
    dotnet publish -c release -f net7.0 src/PdfLexer.Powershell
    [IO.Directory]::CreateDirectory("$pwd/.psm/PdfLexer/net7.0")
    Copy-Item ./src/PdfLexer.Powershell/bin/release/net7.0/publish/* ./.psm/PdfLexer/net7.0/
    Copy-Item ./src/PdfLexer.Powershell/PdfLexer.psd1 ./.psm/PdfLexer/
    Copy-Item ./src/PdfLexer.Powershell/PdfLexer.psm1 ./.psm/PdfLexer/
}
finally {
    Pop-Location
}