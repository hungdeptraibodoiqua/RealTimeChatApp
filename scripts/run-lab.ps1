param([ValidateSet('prerequisites','build','up','status','scan','demo','runtime','signalr','upload','down','reset')][string]$Action='status')
$ErrorActionPreference='Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '..'))
$envfile = Join-Path $env:USERPROFILE 'chatapp-lab.env'
function NeedEnv { if (-not (Test-Path $envfile)) { throw "Missing $envfile - create it from instructions (do not commit)" } }
switch ($Action) {
 'prerequisites' { docker version; docker compose version; docker info --format '{{.OSType}}' }
 'build' { NeedEnv; docker compose --env-file $envfile build; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE } }   # v3.1: can --env-file, neu khong Compose dung vi thieu DB_PASSWORD
 'up' { NeedEnv; docker compose --env-file $envfile up -d --build; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE } }
 'status' { NeedEnv; docker compose --env-file $envfile ps; docker compose --env-file $envfile logs --tail 40 backend }
 'scan' { & (Join-Path $PSScriptRoot 'scan-local.ps1') -ImageScan; exit $LASTEXITCODE }
 'demo' { & (Join-Path $PSScriptRoot 'demo-15-cases.ps1'); exit $LASTEXITCODE }
 'runtime' { & (Join-Path $PSScriptRoot 'runtime-checks.ps1'); exit $LASTEXITCODE }
 'signalr' { & (Join-Path $PSScriptRoot 'test-signalr-auth.ps1'); exit $LASTEXITCODE }
 'upload' { & (Join-Path $PSScriptRoot 'test-upload-svg.ps1'); exit $LASTEXITCODE }
 'down' { NeedEnv; docker compose --env-file $envfile down }
 'reset' { throw 'Intentional guard: never automate down -v; use explicit docker compose down -v yourself after backup' }
}
