# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all source files
COPY . .

# Restore and publish supporting all possible Render root contexts
RUN dotnet restore ./server/src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj || dotnet restore ./src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj || dotnet restore ScreenWorking.Server.API.csproj
RUN dotnet publish ./server/src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj -c Release -o /app/out || dotnet publish ./src/ScreenWorking.Server.API/ScreenWorking.Server.API.csproj -c Release -o /app/out || dotnet publish ScreenWorking.Server.API.csproj -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ScreenWorking.Server.API.dll"]
