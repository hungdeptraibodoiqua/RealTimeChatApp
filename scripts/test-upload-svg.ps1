# SEC-11 / SEC-05 (TV3): kiem thu upload va truy cap media tren ung dung dang chay.
#  - SEC-11: file SVG chua <script> (ca dang .svg va dang doi duoi .png) phai bi tu choi 400.
#  - Doi chung: anh PNG that phai upload duoc.
#  - SEC-05: URL /uploads/<file> khong duoc tra anh; API media khong token -> 401; co token thanh vien -> 200.
# Ban v3 (da va): tat ca PASS. Ban goc/v2: SEC-11 va SEC-05 FAIL (dung lam anh "truoc khi va").
# Chay: pwsh -NoProfile -File .\scripts\test-upload-svg.ps1        (hoac: lab upload)
# Script se tao tin nhan anh/SVG test trong phong da chon -> chi dung phong test.
param([string]$Base = 'http://localhost:3000', [string]$RoomId)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'lab-auth.ps1')
$results = New-Object System.Collections.Generic.List[object]
function Check([string]$id, [string]$name, [bool]$ok, [string]$detail) {
  $results.Add([pscustomobject]@{ Ma = $id; KiemTra = $name; KetQua = $(if ($ok) { 'PASS' } else { 'FAIL' }); ChiTiet = $detail })
  Write-Host ("[{0}] {1,-6} {2} :: {3}" -f $(if ($ok) { 'PASS' } else { 'FAIL' }), $id, $name, $detail) -ForegroundColor $(if ($ok) { 'Green' } else { 'Red' })
}

Write-Host '== Tai khoan test (thanh vien phong) ==' -ForegroundColor Cyan
$token = Get-LabToken $Base 'User'
if (-not $RoomId) { $RoomId = Select-LabRoom $Base $token 'User' }

# Tep thu nghiem (payload vo hai: chi console.log)
$work = Join-Path ([IO.Path]::GetTempPath()) "chatapp-upload-test-$PID"
New-Item -ItemType Directory -Force $work | Out-Null
$svg = '<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10"><script>console.log("SEC-11 test")</script></svg>'
$enc = New-Object System.Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $work 'xss-test.svg'), $svg, $enc)
[IO.File]::WriteAllText((Join-Path $work 'xss-test-svg.png'), $svg, $enc)
[IO.File]::WriteAllBytes((Join-Path $work 'real.png'), [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=='))

function Upload([string]$file, [string]$type) {
  $out = Join-Path $work 'resp.json'
  $code = (& curl.exe -s -o $out -w '%{http_code}' -X POST "$Base/api/messages/upload" `
            -H "Authorization: Bearer $token" -F "file=@$(Join-Path $work $file);type=$type" -F "roomId=$RoomId") -join ''
  $body = if (Test-Path $out) { Get-Content $out -Raw -Encoding UTF8 } else { '' }
  Remove-Item $out -ErrorAction SilentlyContinue
  [pscustomobject]@{ Code = $code; Body = $body }
}

try {
  # SEC-11
  $r1 = Upload 'xss-test.svg' 'image/svg+xml'
  Check 'SEC-11' 'Upload SVG (image/svg+xml) bi tu choi' ($r1.Code -eq '400') "HTTP $($r1.Code)"
  $r2 = Upload 'xss-test-svg.png' 'image/png'
  Check 'SEC-11' 'SVG doi duoi .png, khai bao image/png bi tu choi' ($r2.Code -eq '400') "HTTP $($r2.Code)"

  # Doi chung + SEC-05
  $r3 = Upload 'real.png' 'image/png'
  Check 'CTRL' 'Anh PNG that upload duoc' ($r3.Code -eq '200') "HTTP $($r3.Code)"
  if ($r3.Code -eq '200') {
    $msg = $r3.Body | ConvertFrom-Json
    $pub = (& curl.exe -s -o NUL -w '%{http_code} %{content_type}' "$Base$($msg.content)") -join ''
    Check 'SEC-05' "URL truc tiep $($msg.content) khong tra anh khi khong dang nhap" ($pub -notmatch 'image/') $pub
    $noTok = (& curl.exe -s -o NUL -w '%{http_code}' "$Base/api/messages/$($msg.id)/media") -join ''
    Check 'SEC-05' 'API media khong token -> 401' ($noTok -eq '401') "HTTP $noTok"
    $withTok = (& curl.exe -s -o NUL -w '%{http_code} %{content_type}' -H "Authorization: Bearer $token" "$Base/api/messages/$($msg.id)/media") -join ''
    Check 'SEC-05' 'API media co token thanh vien -> 200 image/png' ($withTok -match '^200 image/png') $withTok
  }
} finally {
  Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
  $token = $null
}

Write-Host ''
$results | Format-Table -AutoSize | Out-Host
$fail = @($results | Where-Object { $_.KetQua -eq 'FAIL' }).Count
Write-Host "Tong: $($results.Count) kiem tra, $fail FAIL" -ForegroundColor $(if ($fail) { 'Red' } else { 'Green' })
if ($fail -gt 0) { exit 1 } else { exit 0 }
