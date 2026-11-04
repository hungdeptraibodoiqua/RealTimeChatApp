# ===== CAU HINH =====
$SonarUrl = 'http://localhost:32768'      # doi thanh http://localhost:32768 neu SonarQube dang o cong do
$GhRepo   = ''                           # vd 'hungdeptrai/chatapp-devsecops'; de trong neu chua co repo GitHub
# ====================
$cred = Get-Credential -UserName admin -Message 'Mat khau admin SonarQube'
$basic = { param($u,$p) @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${u}:${p}")) } }
$adm = & $basic $cred.UserName $cred.GetNetworkCredential().Password

# 1. Tao Global Analysis Token moi, het han sau 30 ngay
$exp  = (Get-Date).AddDays(30).ToString('yyyy-MM-dd')
$name = "chatapp-lab-$(Get-Date -Format yyyyMMdd-HHmm)"
$t = Invoke-RestMethod -Method Post -Uri "$SonarUrl/api/user_tokens/generate" -Headers $adm `
       -Body @{ name = $name; type = 'GLOBAL_ANALYSIS_TOKEN'; expirationDate = $exp }

# 2. Kiem tra token moi
$ok = Invoke-RestMethod -Uri "$SonarUrl/api/authentication/validate" -Headers (& $basic $t.token '')
if (-not $ok.valid) { throw 'Token moi khong hop le - dung lai, KHONG thu hoi token cu' }

# 3. Thu hoi 2 token cu
foreach ($old in 'SonarSecurityToken','report') {
  try { Invoke-RestMethod -Method Post -Uri "$SonarUrl/api/user_tokens/revoke" -Headers $adm -Body @{ name = $old } | Out-Null; "Da thu hoi: $old" }
  catch { "Bo qua $old (khong ton tai hoac da thu hoi)" }
}

# 4. Gan cho terminal hien tai
$env:SONAR_TOKEN = $t.token; $env:SONAR_HOST_URL = $SonarUrl

# 5. Cap nhat GitHub secret (neu co gh va da dien $GhRepo)
if ($GhRepo -and (Get-Command gh -ErrorAction SilentlyContinue)) {
  $t.token | gh secret set SONAR_TOKEN -R $GhRepo
  gh secret set SONAR_HOST_URL -b $SonarUrl -R $GhRepo
  "Da cap nhat GitHub secrets cho $GhRepo"
}
"XONG: token '$name' (Global), het han $exp, valid=$($ok.valid)"
Remove-Variable cred, adm -ErrorAction SilentlyContinue
