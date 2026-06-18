#Requires -Version 7
<#
.SYNOPSIS
    Builds the LocalStack Web container image from the repository root context.
#>
[CmdletBinding()]
param(
    [string]$Tag = 'lsweb:latest'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')
$dockerfile = Resolve-Path (Join-Path -Path $repoRoot -ChildPath 'docker' -AdditionalChildPath 'Dockerfile')

Write-Host "Building image '$Tag' from context '$repoRoot'..." -ForegroundColor Cyan
docker build --file $dockerfile --tag $Tag $repoRoot

if ($LASTEXITCODE -ne 0) {
    throw "docker build failed with exit code $LASTEXITCODE"
}

Write-Host "Built image '$Tag'." -ForegroundColor Green
