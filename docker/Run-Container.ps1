#Requires -Version 7
<#
.SYNOPSIS
    Runs the LocalStack Web container, publishing the console on the host port.
.DESCRIPTION
    Starts the LocalStack Web container detached with a restart policy so it
    survives Docker/host restarts until the operator explicitly stops it. Any
    existing container with the same name is removed first.

    The container reaches LocalStack via the AWS_ENDPOINT_URL environment
    variable. In network mode (default) it joins an existing Docker network and
    targets the LocalStack service hostname; with -NetworkName '' it runs on the
    default bridge and reaches the host's LocalStack via host.docker.internal.
.PARAMETER ContainerName
    Name for the container. Any existing container with this name is removed first.
.PARAMETER NetworkName
    Existing Docker network to join. The script verifies the network exists and
    never creates it. Pass '' to skip --network and use host.docker.internal.
.PARAMETER Tag
    Image tag to run.
.PARAMETER HostPort
    Host port published to the container's port 8080.
.PARAMETER AwsEndpointUrl
    Optional override for the LocalStack/AWS endpoint. When omitted it defaults
    to the LocalStack service hostname in network mode, or host.docker.internal
    in host-gateway mode.
#>
[CmdletBinding()]
param(
    [string]$ContainerName = 'LocalStackWeb',
    [string]$NetworkName = 'localstack-net',
    [string]$Tag = 'localstackweb:latest',
    [int]$HostPort = 5080,
    [string]$AwsEndpointUrl
)

$ErrorActionPreference = 'Stop'

$useNetwork = -not [string]::IsNullOrWhiteSpace($NetworkName)

if (-not $PSBoundParameters.ContainsKey('AwsEndpointUrl')) {
    $AwsEndpointUrl = if ($useNetwork) { 'http://localstack:4566' } else { 'http://host.docker.internal:4566' }
}

# Verify the requested network exists (never create it).
if ($useNetwork) {
    docker network inspect $NetworkName *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker network '$NetworkName' does not exist. Create it first (e.g. 'docker network create $NetworkName') or pass -NetworkName '' to run on the default bridge via host.docker.internal."
    }
}

# Remove any existing container with the same name.
docker rm --force $ContainerName *> $null

$dockerArgs = @(
    'run', '-d',
    '--name', $ContainerName,
    '--restart', 'unless-stopped',
    '--publish', "${HostPort}:8080",
    '--env', "AWS_ENDPOINT_URL=$AwsEndpointUrl"
)

if ($useNetwork) {
    $dockerArgs += @('--network', $NetworkName)
}
else {
    $dockerArgs += @('--add-host', 'host.docker.internal:host-gateway')
}

$dockerArgs += $Tag

$networkLabel = if ($useNetwork) { $NetworkName } else { 'host-gateway' }
Write-Host "Running '$Tag' as '$ContainerName' on http://localhost:$HostPort (network: $networkLabel, AWS endpoint: $AwsEndpointUrl)..." -ForegroundColor Cyan
docker @dockerArgs

if ($LASTEXITCODE -ne 0) {
    throw "docker run failed with exit code $LASTEXITCODE"
}

Write-Host "Container '$ContainerName' started." -ForegroundColor Green
