# In dong tieu de bang chung truoc khi chup man hinh (Phu luc B: moi anh can thay thoi gian, commit SHA).
# Chay: . .\scripts\evidence-header.ps1 'Hinh 5.8 - TV3'
param([string]$Label = '')
$sha = 'khong co git'
if (Get-Command git -ErrorAction SilentlyContinue) {
  $s = (git rev-parse --short HEAD 2>$null) -join ''
  if ($LASTEXITCODE -eq 0 -and $s) { $sha = $s }
}
$hub = Join-Path $PSScriptRoot '..\backend\src\ChatApp.API\Hubs\ChatHub.cs'
$hubMode = if ((Test-Path $hub) -and ((Get-Content $hub -Raw) -match 'SEC-03 DEMO')) { 'vulnerable (demo)' } else { 'fixed' }
$ver = if (Test-Path (Join-Path $PSScriptRoot '..\backend\src\ChatApp.API\Services\MediaFileInspector.cs')) { 'v3.x (da va SEC-05/11/12/13/14)' } else { 'v2 (chua va SEC-05/11/12/13/14)' }
Write-Host ('=' * 78) -ForegroundColor Cyan
Write-Host ("BANG CHUNG {0}" -f $Label) -ForegroundColor Cyan
Write-Host ("Thoi gian : {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'))
Write-Host ("Commit    : {0}" -f $sha)
Write-Host ("Ma nguon  : {0}; ChatHub: {1}" -f $ver, $hubMode)
Write-Host ('=' * 78) -ForegroundColor Cyan
