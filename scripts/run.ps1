$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

dotnet run --project "$Root\src\DomainPcInfo\DomainPcInfo.csproj"
