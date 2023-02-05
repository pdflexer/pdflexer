@{
	
# Script module or binary module file associated with this manifest.
RootModule = 'PdfLexer.psm1'

# Version number of this module.
ModuleVersion = '0.0.0'

Author = "PdfLexer Authors"

# Supported PSEditions
CompatiblePSEditions = 'Core'

# ID used to uniquely identify this module
GUID = '8861488f-09f2-4e35-b590-142d41a7f0b9'

# Copyright statement for this module
Copyright = '(c) 2023 PdfLexer Authors. All rights reserved.'

# Description of the functionality provided by this module
Description = 'PdfLexer is a powershell pdf manipulation library for Powershell 7+.
 https://github.com/pdflexer/pdflexer'

# Minimum version of the PowerShell engine required by this module
PowerShellVersion = '7.0'

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @()

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @("New-PdfDocument", "Get-PdfPages", "Get-PdfText", "New-PdfWriter", "Open-PdfDocument", "Out-Pdf")

# Variables to export from this module
# VariablesToExport = @()

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = 'PDF','pdf-files','pdf-generation','pdf-document-processor','pdf-redaction','PSEdition_Core','Windows','Linux','MacOS'

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/pdflexer/pdflexer/blob/main/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/pdflexer/pdflexer'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = 'https://github.com/pdflexer/pdflexer/releases'

    } # End of PSData hashtable

 } # End of PrivateData hashtable
}