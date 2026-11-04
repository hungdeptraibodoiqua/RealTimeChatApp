param([switch]$ImageScan)
$ErrorActionPreference = "Stop"
Set-Location (Resolve-Path (Join-Path $PSScriptRoot ".."))
$root = (Get-Location).Path
. (Join-Path $PSScriptRoot 'tool-images.ps1')
New-Item -ItemType Directory -Force -Path security-reports | Out-Null
$results = @{}
function Scan([string]$name,[scriptblock]$command) {
  Write-Host "==== $name ====" -ForegroundColor Cyan
  & $command
  $results[$name] = $LASTEXITCODE
  Write-Host "$name => exit $LASTEXITCODE"
}
Scan 'Gitleaks-working-tree' {
  docker run --rm --mount "type=bind,source=$root,target=/work" $GITLEAKS_IMAGE dir /work --config /work/.gitleaks.toml --redact --report-format json --report-path /work/security-reports/gitleaks.json --exit-code 1
}
Scan 'Checkov' {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $CHECKOV_IMAGE --directory /work --skip-path training --framework dockerfile --check CKV_DOCKER_3,CKV_DOCKER_7,CKV_DOCKER_8 --compact
}
$config = docker compose --env-file lab.env.example config --format json
if ($LASTEXITCODE -ne 0) { throw 'Compose config parsing failed' }
$enc = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $root 'security-reports/compose.rendered.json'), ($config -join [Environment]::NewLine), $enc)
Scan 'Policy-as-Code-real-Compose' {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $OPA_IMAGE eval --fail-defined -d /work/policies/compose.rego -i /work/security-reports/compose.rendered.json 'data.chatapp.compose.deny[_]'
}
Scan 'Policy-as-Code-FAIL-fixture' {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $OPA_IMAGE eval --fail-defined -d /work/policies/compose.rego -i /work/fixtures/compose.bad.json 'data.chatapp.compose.deny[_]'
}
Scan 'Policy-as-Code-PASS-fixture' {
  docker run --rm --mount "type=bind,source=$root,target=/work,readonly" $OPA_IMAGE eval --fail-defined -d /work/policies/compose.rego -i /work/fixtures/compose.good.json 'data.chatapp.compose.deny[_]'
}
if ($ImageScan) {
  docker save -o security-reports/backend.tar chatapp-backend:lab
  Scan 'Trivy-backend' {
    docker run --rm -v trivy-cache:/root/.cache --mount "type=bind,source=$root,target=/work" $TRIVY_IMAGE image --input /work/security-reports/backend.tar --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed --ignorefile /work/.trivyignore --exit-code 1 --format json --output /work/security-reports/trivy-backend.json
  }
  Remove-Item security-reports/backend.tar -ErrorAction SilentlyContinue
  docker save -o security-reports/frontend.tar chatapp-frontend:lab
  Scan 'Trivy-frontend' {
    docker run --rm -v trivy-cache:/root/.cache --mount "type=bind,source=$root,target=/work" $TRIVY_IMAGE image --input /work/security-reports/frontend.tar --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed --ignorefile /work/.trivyignore --exit-code 1 --format json --output /work/security-reports/trivy-frontend.json
  }
  Remove-Item security-reports/frontend.tar -ErrorAction SilentlyContinue
}
Write-Host '===== Summary ====='
$results.GetEnumerator() | Format-Table Name,Value
Write-Host 'A nonzero exit on the BAD fixture is EXPECTED (negative test).'
$badExpected = $results['Policy-as-Code-FAIL-fixture'] -ne 0
$goodExpected = $results['Policy-as-Code-PASS-fixture'] -eq 0
$gateNames = @('Gitleaks-working-tree','Checkov','Policy-as-Code-real-Compose')
if ($ImageScan) { $gateNames += @('Trivy-backend','Trivy-frontend') }
$gateFail = ($gateNames | Where-Object { $results[$_] -ne 0 }).Count -gt 0
if (-not $badExpected -or -not $goodExpected -or $gateFail) { exit 1 }
exit 0
