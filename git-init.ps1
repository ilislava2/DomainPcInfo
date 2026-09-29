$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'Git is not installed or is not available in PATH.'
}

if (-not (Test-Path '.git')) {
    git init
}

git branch -M main

Write-Host ''
Write-Host '=== Git status ==='
git status

Write-Host ''
Write-Host 'Next commands:'
Write-Host '  git add .'
Write-Host '  git commit -m "Initial DomainPcInfo version"'
Write-Host ''
Write-Host 'After creating an empty GitHub repository:'
Write-Host '  git remote add origin https://github.com/<USER>/DomainPcInfo.git'
Write-Host '  git push -u origin main'
