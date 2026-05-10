#Requires -Version 7
<#
.SYNOPSIS
    Runs the LocalStack Web container, publishing the console on the host port.
.DESCRIPTION
    The container talks to a LocalStack endpoint supplied via the AWS_ENDPOINT_URL
    environment variable. By default it targets the host machine's LocalStack at
    http://host.docker.internal:4566.
#>
[CmdletBinding()]
param(
    [string]$Tag = 'localstackweb:latest',
    [int]$Port = 8080,
    [string]$AwsEndpointUrl = 'http://host.docker.internal:4566'
)

$ErrorActionPreference = 'Stop'

Write-Host "Running '$Tag' on http://localhost:$Port (AWS endpoint: $AwsEndpointUrl)..." -ForegroundColor Cyan
docker run --rm `
    --publish "${Port}:8080" `
    --env "AWS_ENDPOINT_URL=$AwsEndpointUrl" `
    --add-host host.docker.internal:host-gateway `
    $Tag
