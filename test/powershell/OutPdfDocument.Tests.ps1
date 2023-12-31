$tmp = $null;
BeforeAll {
    $tmp = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("$PSScriptRoot/.tmp") 
    [IO.Directory]::CreateDirectory($tmp)
}


Describe "Writing test" {
    It 'appends to file' {
        New-PdfPage | Out-Pdf "$tmp/append.pdf" -Append
        New-PdfPage | Out-Pdf "$tmp/append.pdf" -Append
        New-PdfPage | Out-Pdf "$tmp/append.pdf" -Append
        New-PdfPage | Out-Pdf "$tmp/append.pdf" -Append
        ("$tmp/append.pdf" | Open-PdfDocument).Pages.Count | Should -Be 4
    }

    It 'overwrites file with force' {
        New-PdfPage | Out-Pdf "$tmp/overwrite.pdf"
        New-PdfPage | Out-Pdf "$tmp/overwrite.pdf" -Force
        ("$tmp/overwrite.pdf" | Open-PdfDocument).Pages.Count | Should -Be 1
    }

    It 'errors if file exists without force' {
        New-PdfPage | Out-Pdf "$tmp/noforce.pdf"
        { New-PdfPage | Out-Pdf "$tmp/noforce.pdf" } | Should -Throw "*-Force option not used*"
    }

    It "writes to literal path" {
        try {
            Push-Location $PSScriptRoot
            $path = 1..2 | New-PdfPage | Out-Pdf ./.tmp/literal.pdf
            ($path | Open-PdfDocument).Pages.Count | Should -Be 2
        } finally {
            Pop-Location
        }
    }

    It "writes to string path" {
        try {
            Push-Location $PSScriptRoot
            $path = 1..3 | New-PdfPage | Out-Pdf "./.tmp/string.pdf"
            ($path | Open-PdfDocument).Pages.Count | Should -Be 3
        } finally {
            Pop-Location
        }
    }

    AfterEach {
        Close-PdfDocuments
    }
}

AfterAll {
    [IO.Directory]::Delete($tmp, $true)
}