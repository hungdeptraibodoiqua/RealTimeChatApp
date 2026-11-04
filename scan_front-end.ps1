npx --yes @sonar/scan "-Dsonar.host.url=$env:SONAR_HOST_URL" "-Dsonar.token=$env:SONAR_TOKEN" `
  "-Dsonar.projectKey=chatapp-frontend" "-Dsonar.projectName=ChatApp Frontend (React)" `
  "-Dsonar.sources=frontend" "-Dsonar.exclusions=**/node_modules/**,**/dist/**,**/package-lock.json"
