Set-StrictMode -Version Latest

$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase
$binaryModuleRoot = $PSModuleRoot

$mv = $PSVersionTable.PSVersion;
if ($mv -lt [version]'7.0') {
    throw "PdfLexer is not supported on your powershell version. PS 7.0+ is required."

}
elseif ($mv -lt [version]'7.2') {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'netstandard2.1'
}
elseif ($mv.Major -eq 7 -and $mv.Minor -eq 2) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net6'
} else {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net7'
}

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'PdfLexer.Powershell.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}