

Describe 'Get-PdfPages' {
    It 'From relative string, it opens doc' {
        try {
        
            Push-Location $PSScriptRoot
            $pages = "../pdfs/pdfjs/asciihexdecode.pdf" | Get-PdfPages
            $pages.Count | Should -Be 1
        } finally {
            Pop-Location
        }
    }

    It 'From absolute string, it opens doc' {
        $path = Resolve-Path "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf";
        $pages = $path | Get-PdfPages
        $pages.Count | Should -Be 1
    }

    It 'From path result' {
        $paths = Get-ChildItem "$PSScriptRoot/../pdfs/pdfjs/asciihexdecode.pdf";
        $pages = $paths[0] | Get-PdfPages
        $pages.Count | Should -Be 1
    }

    AfterEach {
        Close-PdfDocuments
    }
}

