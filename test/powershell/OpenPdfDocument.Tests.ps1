

Describe 'Open-PdfDocument' {
    It 'From relative string, it opens doc' {
        try {
        
            Push-Location $PSScriptRoot
            $doc = "../pdfs/pdfjs/asciihexdecode.pdf" | Open-PdfDocument
            $doc.Pages.Count | Should -Be 1
        } finally {
            Pop-Location
        }
    }

    It 'From absolute string, it opens doc' {
        $path = Resolve-Path "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf";
        $doc = $path | Open-PdfDocument
        $doc.Pages.Count | Should -Be 1
    }

    It 'From path result' {
        $paths = Get-ChildItem "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf";
        $doc = $paths[0] | Open-PdfDocument
        $doc.Pages.Count | Should -Be 1
    }

    AfterEach {
        Close-PdfDocuments
    }
}

