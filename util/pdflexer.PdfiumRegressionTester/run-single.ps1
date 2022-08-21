
$outputPath = "$PSScriptRoot/../../test/results/txt"
$pdf = "$PSScriptRoot/../../test/pdfs/pdfjs/issue3207r.pdf"
.\bin\Release\net6.0\publish\pdflexer.PdfiumRegressionTester.exe --strict --type text --output $outputPath --pdf $pdf