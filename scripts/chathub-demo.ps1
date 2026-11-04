# TV2 - Hinh 5.7: tam thoi dua ChatHub.cs ve ban CO LOI (SEC-03/SEC-04) de chup FAIL, roi tra lai ban DA VA.
# Chay:  pwsh -NoProfile -File .\scripts\chathub-demo.ps1 -Mode vulnerable   -> build lai backend ban co loi
#        pwsh -NoProfile -File .\scripts\test-signalr-auth.ps1                 -> mong doi FAIL SEC-03/SEC-04
#        pwsh -NoProfile -File .\scripts\chathub-demo.ps1 -Mode fixed         -> tra lai ban da va, build lai
#        pwsh -NoProfile -File .\scripts\test-signalr-auth.ps1                 -> mong doi PASS SEC-03/SEC-04
#        pwsh -NoProfile -File .\scripts\chathub-demo.ps1 -Mode status        -> xem dang o ban nao
# KHONG commit ChatHub.cs khi dang o ban vulnerable.
param([Parameter(Mandatory)][ValidateSet('vulnerable', 'fixed', 'status')][string]$Mode)
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '..'))
$hub     = 'backend\src\ChatApp.API\Hubs\ChatHub.cs'
$vuln    = 'training\runtime\SEC-03-04\ChatHub.vulnerable.cs'
$backup  = 'training\runtime\SEC-03-04\ChatHub.fixed.backup'
$envfile = Join-Path $env:USERPROFILE 'chatapp-lab.env'
function Test-Vuln { (Get-Content $hub -Raw) -match 'SEC-03 DEMO' }

switch ($Mode) {
  'status' {
    if (Test-Vuln) { Write-Host 'ChatHub.cs dang la ban CO LOI (vulnerable)' -ForegroundColor Red }
    else { Write-Host 'ChatHub.cs dang la ban DA VA (fixed)' -ForegroundColor Green }
    exit 0
  }
  'vulnerable' {
    if (Test-Vuln) { Write-Host 'ChatHub.cs da o ban vulnerable' }
    else {
      if ((Get-Content $hub -Raw) -notmatch 'Not authorized to join this room') { throw 'ChatHub.cs khac ban da va mong doi - dung lai, khong ghi de' }
      Copy-Item $hub $backup -Force
      Copy-Item $vuln $hub -Force
      Write-Host "Da sao luu ban da va vao $backup va chuyen sang ban CO LOI" -ForegroundColor Yellow
    }
  }
  'fixed' {
    if (-not (Test-Vuln)) { Write-Host 'ChatHub.cs da o ban fixed' }
    else {
      if (-not (Test-Path $backup)) { throw "Khong tim thay $backup de khoi phuc" }
      Copy-Item $backup $hub -Force
      Remove-Item $backup
      Write-Host 'Da khoi phuc ChatHub.cs ban DA VA' -ForegroundColor Green
    }
  }
}

if (-not (Test-Path $envfile)) { throw "Thieu $envfile" }
Write-Host "Build lai va khoi dong lai backend ($Mode)..." -ForegroundColor Cyan
docker compose --env-file $envfile up -d --build backend
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
for ($i = 0; $i -lt 40; $i++) {
  $s = (docker inspect chatapp-devsecops-backend-1 --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' 2>$null) -join ''
  if ($s -eq 'healthy' -or $s -eq 'running') { break }
  Start-Sleep -Seconds 3
}
Start-Sleep -Seconds 5
Write-Host "Backend: $s. Gio chay: pwsh -NoProfile -File .\scripts\test-signalr-auth.ps1" -ForegroundColor Cyan
