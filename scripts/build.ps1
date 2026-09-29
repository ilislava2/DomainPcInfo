$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

Push-Location $Root
try {
    dotnet restore .\DomainPcInfo.sln
    dotnet build .\DomainPcInfo.sln -c Release --no-restore
}
finally {
    Pop-Location
}
