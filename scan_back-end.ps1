dotnet --list-sdks          # phai co dong 9.0.x; neu khong: winget install Microsoft.DotNet.SDK.9 roi mo lai VS Code
$scan = "$env:USERPROFILE\.sonar-dotnet"
if (-not (Test-Path "$scan\dotnet-sonarscanner.exe")) { dotnet tool install dotnet-sonarscanner --tool-path $scan }
& "$scan\dotnet-sonarscanner" begin /k:"chatapp-backend" /n:"ChatApp Backend (.NET)" `
  /d:sonar.host.url="$env:SONAR_HOST_URL" /d:sonar.token="$env:SONAR_TOKEN" /d:sonar.exclusions="**/bin/**,**/obj/**"
dotnet build backend\backend.sln -c Release
& "$scan\dotnet-sonarscanner" end /d:sonar.token="$env:SONAR_TOKEN"
