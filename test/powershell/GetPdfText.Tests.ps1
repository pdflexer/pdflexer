
Describe 'Get-PdfTest' {
    It 'From path result' {
        $text = "$PSScriptRoot/../pdfs/pdfjs/issue7406.pdf" | Get-PdfText
        $text | Should -Match "7406"
    }

}
