param(
    [ValidateSet("full", "selected")]
    [string]$Mode,

    [ValidateSet("backend", "web", "docker")]
    [string]$Job
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent $scriptDirectory
Set-Location $repoRoot

$workflowPath = ".github/workflows/ci.yml"
if (-not (Test-Path $workflowPath)) {
    throw "Workflow file '$workflowPath' was not found."
}

$actCommand = Get-Command act -ErrorAction SilentlyContinue
if ($null -eq $actCommand) {
    throw "act is not installed. Install it from https://github.com/nektos/act and rerun this script."
}

if ([string]::IsNullOrWhiteSpace($Mode)) {
    $selection = Read-Host "Run CI workflow in full or selected mode? (full/selected)"
    $Mode = $selection.Trim().ToLowerInvariant()
}

if ($Mode -notin @("full", "selected")) {
    throw "Invalid mode '$Mode'. Use 'full' or 'selected'."
}

if ($Mode -eq "selected") {
    if ([string]::IsNullOrWhiteSpace($Job)) {
        Write-Host "Available jobs: backend, web, docker"
        $selectedJob = Read-Host "Enter job name"
        $Job = $selectedJob.Trim().ToLowerInvariant()
    }

    if ($Job -notin @("backend", "web", "docker")) {
        throw "Invalid job '$Job'."
    }
}

$arguments = @("pull_request", "--workflows", $workflowPath)
if ($Mode -eq "selected") {
    $arguments += @("-j", $Job)
}

# The backend job runs the integration tests, which self-provision a LocalStack
# container via Testcontainers. Under act the job executes inside a container, so
# Testcontainers starts LocalStack as a sibling on the host and its mapped port is
# not reachable as "localhost" from inside the act container. Expose the host
# gateway and tell Testcontainers to resolve mapped ports via it so the in-process
# API can reach LocalStack. These flags are act-only (inert for the web/docker jobs)
# and do not affect real CI.
$arguments += @(
    "--container-options", "--add-host=host.docker.internal:host-gateway",
    "--env", "TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal"
)

Write-Host "Running act $($arguments -join ' ')"
& $actCommand.Source @arguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "act completed successfully."