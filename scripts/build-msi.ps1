$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot

$Project = Join-Path $Root 'src\DomainPcInfo\DomainPcInfo.csproj'
$SetupProject = Join-Path $Root 'installer\DomainPcInfo.Setup\DomainPcInfo.Setup.wixproj'
$PublishDir = Join-Path $Root 'artifacts\publish\win-x64'
$SetupBinDir = Join-Path $Root 'installer\DomainPcInfo.Setup\bin'

Write-Host ''
Write-Host '=== Cleaning previous publish ==='

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

Write-Host ''
Write-Host '=== Publishing DomainPcInfo ==='

$publishArgs = @(
    'publish'
    $Project
    '-c'
    'Release'
    '-r'
    'win-x64'
    '--self-contained'
    'true'
    '-p:PublishSingleFile=true'
    '-p:IncludeNativeLibrariesForSelfExtract=true'
    '-p:DebugType=None'
    '-o'
    $PublishDir
)

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw 'DomainPcInfo publish failed.'
}

Write-Host ''
Write-Host '=== Checking EXE ==='

$ExePath = Join-Path $PublishDir 'DomainPcInfo.exe'

if (-not (Test-Path $ExePath)) {
    throw ('EXE not found: ' + $ExePath)
}

Write-Host ('EXE found: ' + $ExePath)

Write-Host ''
Write-Host '=== Building MSI ==='

$msiArgs = @(
    'build'
    $SetupProject
    '-c'
    'Release'
    '-p:InstallerPlatform=x64'
)

& dotnet @msiArgs

if ($LASTEXITCODE -ne 0) {
    throw 'MSI build failed.'
}

Write-Host ''
Write-Host '=== DONE ==='
Write-Host ''

$MsiFiles = Get-ChildItem -Path $SetupBinDir -Filter '*.msi' -Recurse -ErrorAction SilentlyContinue

if (-not $MsiFiles) {
    throw ('MSI not found in: ' + $SetupBinDir)
}

$MsiFiles | Select-Object FullName, Length, LastWriteTime