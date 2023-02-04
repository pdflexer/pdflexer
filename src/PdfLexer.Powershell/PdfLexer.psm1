Set-StrictMode -Version Latest

$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase
$binaryModuleRoot = $PSModuleRoot

$mv = $PSVersionTable.PSVersion.Major;
if ($mv -ge 7) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net7.0'
}
elseif ($mv -eq 6) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net6.0'
}
else {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'netstandard2.1'
}

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'PdfLexer.Powershell.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}