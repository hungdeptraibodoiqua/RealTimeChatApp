# Ham dung chung cho cac script kiem thu runtime (dot-source: . .\scripts\lab-auth.ps1).
# Chi dung voi TAI KHOAN TEST tren lab cuc bo. Khong in token, khong luu token ra file.
# Luu y: /api/auth/login bi rate limit 10 lan/phut/IP o ban v3 (SEC-12).

function Read-LabSecret([string]$Prompt) {
  $sec = Read-Host $Prompt -AsSecureString
  $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
  try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
  finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

# Dang nhap bang username/email + mat khau (nhap an), tra ve access token.
function Get-LabToken([string]$Base, [string]$Label) {
  $user = Read-Host "[$Label] Username hoac email"
  $pass = Read-LabSecret "[$Label] Mat khau (khong hien tren man hinh)"
  $body = @{ emailOrUserName = $user; password = $pass } | ConvertTo-Json
  try {
    $res = Invoke-RestMethod -Method Post -Uri "$Base/api/auth/login" -ContentType 'application/json' -Body $body
  } catch {
    throw "[$Label] Dang nhap that bai: $($_.Exception.Message) (sai mat khau, hoac bi rate limit 429 - doi 60 giay)"
  } finally { $pass = $null; $body = $null }
  $token = $res.tokens.accessToken
  if (-not $token) { throw "[$Label] Phan hoi dang nhap khong co tokens.accessToken" }
  Write-Host "[$Label] Dang nhap OK: $user (token dai $($token.Length) ky tu, khong in ra)" -ForegroundColor DarkGray
  return $token
}

function Get-LabRooms([string]$Base, [string]$Token) {
  @(Invoke-RestMethod -Uri "$Base/api/rooms" -Headers @{ Authorization = "Bearer $Token" })
}

# Cho nguoi dung chon 1 phong trong danh sach phong cua token.
function Select-LabRoom([string]$Base, [string]$Token, [string]$Label) {
  $rooms = Get-LabRooms $Base $Token
  if ($rooms.Count -eq 0) { throw "[$Label] Tai khoan chua co phong nao - tao phong tren web truoc" }
  for ($i = 0; $i -lt $rooms.Count; $i++) { Write-Host ("  [{0}] {1}  ({2})" -f $i, $rooms[$i].name, $rooms[$i].id) }
  $pick = Read-Host "[$Label] Chon so thu tu phong"
  $room = $rooms[[int]$pick]
  if (-not $room) { throw 'Lua chon khong hop le' }
  return [string]$room.id
}
