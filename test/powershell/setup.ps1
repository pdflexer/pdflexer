$pdfs = "$PSScriptRoot/../pdfs/pdfjs/"
Remove-Module PdfLexer
& $PSScriptRoot/../../create-module.ps1
Import-Module (Resolve-Path $PSScriptRoot/../../.psm/PdfLexer/) -Force