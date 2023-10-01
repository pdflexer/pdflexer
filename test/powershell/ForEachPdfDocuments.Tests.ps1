

Describe 'ForEach-PdfDocuments' {

    It 'Accesses local variable' {
        $pages = 0;
        Get-ChildItem "$PSScriptRoot/../pdfs/pdfjs/issue8*.pdf" | ForEach-PdfDocument { $pages += $_.Pages.Count; }
        $pages | Should -Be 27
    }

    It 'Runs multiple times without locking' {
        "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf" | ForEach-PdfDocument { }
        "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf" | ForEach-PdfDocument { }
    }

}

