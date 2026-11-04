# SEC-03 / SEC-04 (TV2): kiem thu tich hop SignalR - tai khoan B KHONG thuoc phong cua A
# goi JoinRoom va SendTyping vao phong do. Ban v3 (da va): PASS. Ban goc/v2 neu chua va: FAIL.
# Chay: pwsh -NoProfile -File .\scripts\test-signalr-auth.ps1        (hoac: lab signalr)
# Can: ung dung dang chay (lab up), Node.js 18+ tren may (node -v), 2 tai khoan test A va B.
param([string]$Base = 'http://localhost:3000', [string]$RoomId)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lab-auth.ps1')
$frontend = Resolve-Path (Join-Path $PSScriptRoot '..\frontend')

if (-not (Get-Command node -ErrorAction SilentlyContinue)) { throw 'Chua co Node.js. Cai: winget install OpenJS.NodeJS.LTS, mo lai terminal.' }
if (-not (Test-Path (Join-Path $frontend 'node_modules\@microsoft\signalr'))) {
  Write-Host 'Cai thu vien frontend (npm ci) lan dau...' -ForegroundColor Cyan
  Push-Location $frontend; try { npm ci --no-audit --no-fund; if ($LASTEXITCODE -ne 0) { throw 'npm ci that bai' } } finally { Pop-Location }
}

# 1. Phong muc tieu: lay tu tai khoan A (chu phong) neu chua truyen -RoomId
if (-not $RoomId) {
  Write-Host '== Tai khoan A (chu phong rieng, KHONG co B trong phong) ==' -ForegroundColor Cyan
  $tokenA = Get-LabToken $Base 'A'
  $RoomId = Select-LabRoom $Base $tokenA 'A'
  $tokenA = $null
}

# 2. Token cua B (ke tan cong co tai khoan hop le)
Write-Host '== Tai khoan B (KHONG phai thanh vien phong tren) ==' -ForegroundColor Cyan
$tokenB = Get-LabToken $Base 'B'
$roomsB = Get-LabRooms $Base $tokenB
if ($roomsB.id -contains $RoomId) {
  Write-Warning 'B dang LA thanh vien phong nay -> day khong phai kich ban tan cong. Xoa B khoi phong hoac chon phong khac.'
}

# 3. Goi hub bang script Node (frontend/scripts/test-room-auth.mjs)
$env:DEMO_TOKEN_B = $tokenB; $env:DEMO_ROOM_A = $RoomId
Write-Host "== Goi JoinRoom/SendTyping vao phong $RoomId bang token cua B ==" -ForegroundColor Cyan
Push-Location $frontend
try { node ./scripts/test-room-auth.mjs; $code = $LASTEXITCODE }
finally {
  Pop-Location
  Remove-Item Env:DEMO_TOKEN_B, Env:DEMO_ROOM_A -ErrorAction SilentlyContinue
  $tokenB = $null
}
$color = if ($code -eq 0) { 'Green' } else { 'Red' }
Write-Host "Ket qua SignalR test exit=$code (0 = B bi tu choi o ca JoinRoom va SendTyping)" -ForegroundColor $color
exit $code
