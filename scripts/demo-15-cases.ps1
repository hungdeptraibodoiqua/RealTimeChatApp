# Run from PowerShell (Windows); Docker Desktop Linux engine is required.
# This script validates deliberately BAD->FAIL / FIXED->PASS without modifying live app.
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '..'))
$root = (Get-Location).Path
. (Join-Path $PSScriptRoot 'tool-images.ps1')
New-Item -ItemType Directory -Force security-reports | Out-Null
$global:failed = 0
function Expect([string]$name, [int]$exitExpected, [scriptblock]$command) {
  Write-Host "===== $name (expected exit $exitExpected) =====" -ForegroundColor Cyan
  & $command
  $exitObserved = [int]$LASTEXITCODE
  if ($exitExpected -eq 0 -and $exitObserved -ne 0) { $global:failed++ ; Write-Host "FAIL $name (exit $exitObserved)" -ForegroundColor Red }
  elseif ($exitExpected -ne 0 -and $exitObserved -eq 0) { $global:failed++ ; Write-Host "FAIL $name (unexpected pass)" -ForegroundColor Red }
  else { Write-Host "PASS $name (exit $exitObserved)" -ForegroundColor Green }
}
# Python is inside a local container; nothing to install on Windows other than Docker.
Expect '10 security cases: regression expectation' 0 {
  docker run --rm --mount "type=bind,source=$root,target=/work" -w /work $PYTHON_IMAGE python scripts/verify-training-cases.py
}
Expect '5 code-quality cases: regression expectation' 0 {
  docker run --rm --mount "type=bind,source=$root,target=/work" -w /work $PYTHON_IMAGE python scripts/verify-quality-cases.py
}
foreach ($case in @('SEC-01', 'SEC-02')) {
  foreach ($mode in @('vulnerable','fixed')) {
    $expected = if ($mode -eq 'vulnerable') { 1 } else { 0 }
    Expect "$case $mode Gitleaks" $expected {
      docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $GITLEAKS_IMAGE dir "/work/training/security/$case/$mode" --config /work/training/gitleaks-demo.toml --redact --exit-code 1
    }
  }
}
foreach ($mode in @('vulnerable','fixed')) {
  $expected = if ($mode -eq 'vulnerable') { 1 } else { 0 }
  Expect "SEC-08 $mode Checkov" $expected {
    docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $CHECKOV_IMAGE --directory "/work/training/security/SEC-08/$mode" --framework dockerfile --check CKV_DOCKER_3,CKV_DOCKER_8 --compact --quiet
  }
}
foreach ($case in @('SEC-09','SEC-10')) {
  foreach ($mode in @('vulnerable','fixed')) {
    $expected = if ($mode -eq 'vulnerable') { 1 } else { 0 }
    $inputFile = "security-reports/$case-$mode.json"
    $raw = docker compose -f "training/security/$case/$mode/compose.yaml" config --format json
    if ($LASTEXITCODE -ne 0) { throw "docker compose render failed for $case $mode" }
    [System.IO.File]::WriteAllText((Join-Path $root $inputFile),($raw -join [Environment]::NewLine),(New-Object System.Text.UTF8Encoding($false)))
    Expect "$case $mode OPA" $expected {
      docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $OPA_IMAGE eval --fail-defined -d /work/policies/compose.rego -i "/work/$inputFile" 'data.chatapp.compose.deny[_]'
    }
  }
}
Write-Host "===== Overall: $global:failed unexpected results ====="
if ($global:failed -gt 0) { exit 1 }
exit 0
