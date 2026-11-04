# Kiem tra nhanh ung dung DANG CHAY (sau: run-lab.ps1 -Action up). Dung cho demo ban v3.
# Chay: pwsh -NoProfile -File .\scripts\runtime-checks.ps1
# Luu y: buoc rate limit (cuoi cung) se khoa dang nhap tu may nay trong ~60 giay.
param([string]$Base = 'http://localhost:3000', [switch]$SkipRateLimit)
$ErrorActionPreference = 'Continue'
$results = New-Object System.Collections.Generic.List[object]

function Check([string]$id, [string]$name, [bool]$ok, [string]$detail) {
  $results.Add([pscustomobject]@{ Ma = $id; KiemTra = $name; KetQua = $(if ($ok) { 'PASS' } else { 'FAIL' }); ChiTiet = $detail })
  $color = if ($ok) { 'Green' } else { 'Red' }
  Write-Host ("[{0}] {1,-6} {2} :: {3}" -f $(if ($ok) { 'PASS' } else { 'FAIL' }), $id, $name, $detail) -ForegroundColor $color
}
function Code([string[]]$curlArgs) { (& curl.exe -s -o NUL -w '%{http_code}' @curlArgs) -join '' }

# LAB-02: frontend chay, chi bind loopback
$c = Code @("$Base/")
Check 'LAB-02' 'Frontend tra 200' ($c -eq '200') "HTTP $c"
# SEC-10: kiem tra CONTAINER db co publish cong ra host khong (docker port rong = dat).
# Khong dung Test-NetConnection 5432 vi may co the dang chay PostgreSQL cai san tren Windows (duong tinh gia).
$dbPorts = ((docker port chatapp-devsecops-db-1 2>$null) -join ' ').Trim()
$detail = if ($dbPorts) { "docker port db: $dbPorts" } else { 'docker port db: (trong)' }
try {
  $l = Get-NetTCPConnection -LocalPort 5432 -State Listen -ErrorAction Stop | Select-Object -First 1
  if ($l) { $detail += "; cong 5432 tren host do tien trinh '$((Get-Process -Id $l.OwningProcess -ErrorAction SilentlyContinue).ProcessName)' mo (khong phai container lab)" }
} catch { }
Check 'SEC-10' 'Container db khong publish cong ra host' (-not $dbPorts) $detail

# SEC-14: healthcheck backend
$h = (docker inspect chatapp-devsecops-backend-1 --format '{{.State.Health.Status}}' 2>$null) -join ''
Check 'SEC-14' 'Backend HEALTHCHECK = healthy' ($h -eq 'healthy') "status=$h"

# SEC-08/09: container khong privileged, khong chay root
foreach ($svc in 'db','backend','frontend') {
  $info = (docker inspect "chatapp-devsecops-$svc-1" --format '{{.HostConfig.Privileged}}|{{.Config.User}}|{{.HostConfig.CapDrop}}' 2>$null) -join ''
  $parts = $info -split '\|'
  $ok = ($parts.Count -eq 3) -and ($parts[0] -eq 'false') -and ($parts[2] -match 'ALL')
  Check 'SEC-09' "$svc khong privileged, cap_drop ALL" $ok $info
}

# SEC-13: security headers
$hdr = (& curl.exe -s -D - -o NUL "$Base/") -join "`n"
foreach ($name in 'Content-Security-Policy','X-Content-Type-Options','X-Frame-Options','Referrer-Policy') {
  Check 'SEC-13' "Header $name" ($hdr -match "(?im)^$name\s*:") ''
}

# SEC-07: CORS khong cho origin la
$cors = (& curl.exe -s -D - -o NUL -X OPTIONS "$Base/api/auth/login" -H 'Origin: http://evil.example' -H 'Access-Control-Request-Method: POST') -join "`n"
Check 'SEC-07' 'CORS tu choi http://evil.example' (-not ($cors -match '(?im)^Access-Control-Allow-Origin')) ''

# SEC-05: /uploads khong con duoc proxy toi backend; API media bat buoc token
$up = (& curl.exe -s -o NUL -w '%{http_code} %{content_type}' "$Base/uploads/test.png") -join ''
Check 'SEC-05' '/uploads/* khong tra file tu backend' ($up -notmatch 'image/') $up
$m = Code @("$Base/api/messages/00000000-0000-0000-0000-000000000001/media")
Check 'SEC-05' 'API media khong token -> 401' ($m -eq '401') "HTTP $m"

# SEC-06: token sai/het han bi tu choi
$t = Code @("$Base/api/rooms", '-H', 'Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.e30.invalid')
Check 'SEC-06' 'Token khong hop le -> 401' ($t -eq '401') "HTTP $t"

# SEC-12: rate limit dang nhap (10 lan/phut/IP)
if (-not $SkipRateLimit) {
  $codes = 1..12 | ForEach-Object {
    Code @('-X', 'POST', "$Base/api/auth/login", '-H', 'Content-Type: application/json', '-d', '{"emailOrUserName":"demo_khong_ton_tai","password":"Sai-mat-khau-1"}')
  }
  Check 'SEC-12' '12 lan dang nhap sai -> co 429' ($codes -contains '429') ($codes -join ' ')
}

Write-Host ''
$results | Format-Table -AutoSize | Out-Host
$fail = @($results | Where-Object { $_.KetQua -eq 'FAIL' }).Count
Write-Host "Tong: $($results.Count) kiem tra, $fail FAIL" -ForegroundColor $(if ($fail) { 'Red' } else { 'Green' })
if ($fail -gt 0) { exit 1 } else { exit 0 }