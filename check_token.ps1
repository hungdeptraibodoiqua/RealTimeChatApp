$env:SONAR_TOKEN.Length                      # Global token thuong dai ~44 ky tu, khong phai 0
$env:SONAR_TOKEN.Substring(0,4)              # phai ra: sqa_
# v3.1: dung header Bearer thay cho "-u token:" (rule curl-auth-user cua Gitleaks bao duong tinh gia)
curl.exe -s -H "Authorization: Bearer $env:SONAR_TOKEN" "$env:SONAR_HOST_URL/api/authentication/validate"