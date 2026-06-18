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

    Pass -UserDataPath to bind-mount a host directory into the container for
    persisted user data (saved Lambda test payloads, favourites, recently-viewed).
    The container stays stateless for AWS data; only this explicit user data is
    written to the mounted directory.
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
.PARAMETER UserDataPath
    Optional host directory to bind-mount for persisted user data (for example
    saved Lambda test payloads). When supplied, the directory is created if it
    does not exist and the container's LSW_USER_DATA_DIR points at the mount, so
    user data survives container restarts. When omitted the container keeps user
    data in memory only (cleared on restart).
.PARAMETER AwsAccessKeyId
    Optional AWS access key id (AWS_ACCESS_KEY_ID). When omitted the container
    falls back to its built-in default suitable for LocalStack.
.PARAMETER AwsSecretAccessKey
    Optional AWS secret access key (AWS_SECRET_ACCESS_KEY). When omitted the
    container falls back to its built-in default suitable for LocalStack. The
    value is never echoed to the console.
.PARAMETER AwsRegion
    Optional AWS region (AWS_REGION). When omitted the container falls back to
    its built-in default region.
.PARAMETER ContainerPort
    Port the application listens on inside the container (PORT). The host port is
    published to this port. Defaults to 8080.
.PARAMETER SeqUrl
    Optional Seq ingestion URL (Seq__Url) for structured-log shipping. When
    omitted the container does not ship logs to Seq.
.PARAMETER AllowDiagnosticReveal
    When set, allows the diagnostics endpoint to reveal otherwise-redacted
    configuration values (LSW_ALLOW_DIAGNOSTIC_REVEAL=true). Off by default.
#>
[CmdletBinding()]
param(
    [string]$ContainerName = 'LocalStackWeb',
    [string]$NetworkName = 'localstack-net',
    [string]$Tag = 'lsweb:latest',
    [int]$HostPort = 5080,
    [string]$AwsEndpointUrl,
    [string]$UserDataPath,
    [string]$AwsAccessKeyId,
    [string]$AwsSecretAccessKey,
    [string]$AwsRegion,
    [int]$ContainerPort = 8080,
    [string]$SeqUrl,
    [switch]$AllowDiagnosticReveal
)

$ErrorActionPreference = 'Stop'

$useNetwork = -not [string]::IsNullOrWhiteSpace($NetworkName)

if (-not $PSBoundParameters.ContainsKey('AwsEndpointUrl')) {
    $AwsEndpointUrl = if ($useNetwork) { 'http://localstack:4566' } else { 'http://host.docker.internal:4566' }
}

# Resolve the optional user-data bind mount. The application persists user data (saved Lambda test
# payloads, favourites, recently-viewed) to LSW_USER_DATA_DIR when set; mapping it to a host
# directory makes that data survive container restarts while AWS data stays stateless.
$mountUserData = -not [string]::IsNullOrWhiteSpace($UserDataPath)
$containerUserDataDir = '/data'

if ($mountUserData) {
    if (-not (Test-Path -LiteralPath $UserDataPath)) {
        New-Item -ItemType Directory -Path $UserDataPath -Force | Out-Null
    }
    $UserDataPath = (Resolve-Path -LiteralPath $UserDataPath).Path
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
    '--publish', "${HostPort}:${ContainerPort}",
    '--env', "AWS_ENDPOINT_URL=$AwsEndpointUrl",
    '--env', "PORT=$ContainerPort"
)

# Optional API configuration. Each value is only forwarded when supplied so the
# container keeps its built-in defaults (suitable for LocalStack) otherwise.
if ($PSBoundParameters.ContainsKey('AwsAccessKeyId')) {
    $dockerArgs += @('--env', "AWS_ACCESS_KEY_ID=$AwsAccessKeyId")
}
if ($PSBoundParameters.ContainsKey('AwsSecretAccessKey')) {
    $dockerArgs += @('--env', "AWS_SECRET_ACCESS_KEY=$AwsSecretAccessKey")
}
if ($PSBoundParameters.ContainsKey('AwsRegion')) {
    $dockerArgs += @('--env', "AWS_REGION=$AwsRegion")
}
if ($PSBoundParameters.ContainsKey('SeqUrl')) {
    $dockerArgs += @('--env', "Seq__Url=$SeqUrl")
}
if ($AllowDiagnosticReveal) {
    $dockerArgs += @('--env', 'LSW_ALLOW_DIAGNOSTIC_REVEAL=true')
}

if ($useNetwork) {
    $dockerArgs += @('--network', $NetworkName)
}
else {
    $dockerArgs += @('--add-host', 'host.docker.internal:host-gateway')
}

if ($mountUserData) {
    $dockerArgs += @(
        '--volume', "${UserDataPath}:$containerUserDataDir",
        '--env', "LSW_USER_DATA_DIR=$containerUserDataDir"
    )
}

$dockerArgs += $Tag

$networkLabel = if ($useNetwork) { $NetworkName } else { 'host-gateway' }
Write-Host "Running '$Tag' as '$ContainerName' on http://localhost:$HostPort (network: $networkLabel, AWS endpoint: $AwsEndpointUrl)..." -ForegroundColor Cyan
if ($PSBoundParameters.ContainsKey('AwsRegion')) {
    Write-Host "AWS region: $AwsRegion." -ForegroundColor Cyan
}
if ($PSBoundParameters.ContainsKey('SeqUrl')) {
    Write-Host "Shipping logs to Seq at $SeqUrl." -ForegroundColor Cyan
}
if ($mountUserData) {
    Write-Host "Persisting user data to '$UserDataPath' (mounted at $containerUserDataDir)." -ForegroundColor Cyan
}
docker @dockerArgs

if ($LASTEXITCODE -ne 0) {
    throw "docker run failed with exit code $LASTEXITCODE"
}

Write-Host "Container '$ContainerName' started." -ForegroundColor Green
