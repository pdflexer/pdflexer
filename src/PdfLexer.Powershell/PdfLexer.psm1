Set-StrictMode -Version Latest

$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase
$binaryModuleRoot = $PSModuleRoot

$mv = $PSVersionTable.PSVersion;
if ($mv -lt [version]'7.3') {
    throw "PdfLexer is not supported on your powershell version. PS 7.3+ is required."

}
$binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net7.0'

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'PdfLexer.Powershell.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}

foreach ($script in 
  (Get-ChildItem -File -Recurse -LiteralPath $PSScriptRoot -Filter *.ps1)
) { 
  . $script 
}