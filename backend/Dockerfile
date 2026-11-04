FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore backend.sln
RUN dotnet publish src/ChatApp.API/ChatApp.API.csproj -c Release -o /out --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_HTTP_PORTS=8080
# SEC-05: thu muc media nam ngoai wwwroot, thuoc user app (volume 'uploads' mount vao day)
RUN mkdir -p /app/wwwroot /app/data/uploads && chown -R app:app /app/data
# SEC-14 (CKV_DOCKER_2): HEALTHCHECK kiem tra cong 8080 dang mo (image aspnet khong co curl/wget)
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD ["bash", "-c", "exec 3<>/dev/tcp/127.0.0.1/8080"]
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatApp.API.dll"]
