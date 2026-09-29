$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
$Output = Join-Path $Root 'artifacts\publish\win-x64'

New-Item -ItemType Directory -Path $Output -Force | Out-Null

dotnet publish "$Root\src\DomainPcInfo\DomainPcInfo.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $Output

Write-Host "Published to $Output"
