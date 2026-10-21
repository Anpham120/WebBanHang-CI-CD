# 5 build targets correspond to Jenkins pipeline stages.
# .NET SDK is only needed during CI; the final image has the lightweight runtime.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src
COPY WebBanHang/WebBanHang.csproj WebBanHang/
COPY WebBanHang.Tests/WebBanHang.Tests.csproj WebBanHang.Tests/
RUN dotnet restore WebBanHang.Tests/WebBanHang.Tests.csproj

FROM restore AS build
COPY . .
RUN dotnet build WebBanHang.Tests/WebBanHang.Tests.csproj -c Release --no-restore

FROM build AS test
RUN dotnet test WebBanHang.Tests/WebBanHang.Tests.csproj -c Release --no-build --no-restore --logger "console;verbosity=normal"

FROM test AS publish
RUN dotnet publish WebBanHang/WebBanHang.csproj -c Release --no-build --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=publish /app/publish/ ./
ENTRYPOINT ["dotnet", "WebBanHang.dll"]
