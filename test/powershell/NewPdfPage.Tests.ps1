using namespace PdfLexer.Powershell;

Describe 'Create blank pages' {
    It 'Page size by name' {
        $doc = New-PdfPage -Size EXECUTIVE | Out-PdfBytes | Open-PdfDocument
        $doc.Pages[0].MediaBox.Height | Should -Be 756.0
    }

    It 'Page size by name A0' {
        $doc = New-PdfPage -Size A0 | Out-PdfBytes | Open-PdfDocument
        $doc.Pages[0].MediaBox.Height | Should -Be 3370.39
    }

    It 'Page size default' {
        $doc = New-PdfPage | Out-PdfBytes | Open-PdfDocument
        $doc.Pages[0].MediaBox.Height | Should -Be 792.0
    }

    It 'Page size by size' {
        $w = 101.;
        $h = 201.;
        $doc = New-PdfPage -Width $w -Height $h | Out-PdfBytes | Open-PdfDocument
        $doc.Pages[0].MediaBox.Height | Should -Be $h
        $doc.Pages[0].MediaBox.Width | Should -Be $w
    }

    AfterEach {
        Close-PdfDocuments
    }

}

Describe 'Create multiple pages' {
    It 'Page size by name' {
        $doc = 1..10 | New-PdfPage | Out-PdfBytes | Open-PdfDocument
        $doc.Pages.Count | Should -Be 10
    }

    AfterEach {
        Close-PdfDocuments
    }

}


Describe 'Create pages with text' {
    It 'writes simple text' {
        New-PdfPage -Action @([PdfTextAction] @{ text = "writes simple text"}) | Get-PdfText | Should -BeLike "*writes simple text*"
    }

    AfterEach {
        Close-PdfDocuments
    }

}
